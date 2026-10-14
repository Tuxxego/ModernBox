using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using ai.behaviours;

namespace ModernBox
{
    // Manu-Fix 018: Forschung gehoert zum gespeicherten Reich, nicht zum letzten weltweit geprueften Wagen.
    // Der gesamte Aufstieg wird von realem Aufbau und bezahlten Forschungsjahren getragen.
    public static class Development
    {
        public sealed class Technology
        {
            public string Id, Name; public int Points, Age, Population, Army, Buildings;
            public Technology(string id, string name, int points, int age, int population, int army, int buildings)
            { Id=id; Name=name; Points=points; Age=age; Population=population; Army=army; Buildings=buildings; }
        }
        public static readonly Technology[] Technologies = {
            new Technology("architecture", "Baukunst", 20, 10, 30, 0, 8),
            new Technology("metallurgy", "Metallverarbeitung", 35, 20, 60, 0, 12),
            new Technology("education", "Bildungswesen", 50, 35, 100, 0, 18),
            new Technology("renaissance", "Renaissance und Feuerwaffen", 80, 60, 200, 35, 25),
            new Technology("industry", "Industrialisierung", 130, 100, 400, 70, 35),
            // Manu-Fix 038: modern population thresholds lowered from 650, 900, 1500 and 2200. A natural 720-year world
            // had 56 of 64 kingdoms blocked only by size; humans, elves and dwarves stayed below 700 inhabitants per
            // kingdom and never left the industrial stage. Nuclear use keeps its 1,000/150 gate.
            new Technology("military", "Motorisierung und Panzer", 180, 150, 400, 120, 45),
            new Technology("aviation", "Luftfahrt", 200, 190, 600, 180, 55),
            new Technology("nuclear", "Kerntechnik", 400, 280, 1000, 300, 70),
            new Technology("future", "Zukunftstechnologien", 650, 380, 1500, 450, 90)
        };
        public const string StageKey = "manu_mb_research_stage";
        public const string PointsKey = "manu_mb_research_points";
        public const string YearKey = "manu_mb_research_year";
        public static bool Ready;
        public static string T(string de,string en) => LocalizedTextManager.instance?.language=="de"?de:en;
        private static string TechName(Technology tech) {
            string[] names={"Architecture","Metallurgy","Education","Renaissance and firearms","Industrialization","Motorization and tanks","Aviation","Nuclear technology","Future technology"};
            return T(tech.Name,names[Array.IndexOf(Technologies,tech)]);
        }
        public static int RegisteredSpecies => SpeciesTemplates.Count;
        private static readonly Dictionary<BuildingAsset, object[]> BaselineBuildings = new Dictionary<BuildingAsset, object[]>();
        private static readonly Dictionary<string,string> BaselineTemplates = new Dictionary<string,string>();
        private static readonly Dictionary<string,string> SpeciesTemplates = new Dictionary<string,string>();
        private static readonly Dictionary<string,string> SpeciesGroups = new Dictionary<string,string>();
        private static readonly Dictionary<string,ArchitectureAsset> ExpandedArchitectures = new Dictionary<string,ArchitectureAsset>();
        private static readonly Dictionary<string,int> BuildingStages = new Dictionary<string,int>();
        private static readonly Dictionary<string,ConstructionCost> ReserveCache = new Dictionary<string,ConstructionCost>();
        private static readonly Dictionary<long,int> CultureStages = new Dictionary<long,int>();
        // Manu-Fix 036: terminal buildings shared by several architectures (vanilla "bonfire" for humans, elves,
        // orcs and dwarves) need one upgrade target per faction group instead of one global upgrade_to.
        private static readonly Dictionary<string,Dictionary<string,string>> SharedUpgrades = new Dictionary<string,Dictionary<string,string>>();
        private static readonly HashSet<string> SharedTerminals = new HashSet<string>();
        private static readonly FieldInfo[] BuildingFields = typeof(BuildingAsset).GetFields(BindingFlags.Public|BindingFlags.Instance);
        private static int _lastAssets = -1;
        private static int _constructionCapacity=1000;
        [ThreadStatic] internal static int EconomyScope;
        [ThreadStatic] private static bool _calculatingReserve;
        public static void CaptureBeforeBuildings()
        {
            foreach (BuildingAsset b in AssetManager.buildings.list)
                BaselineBuildings[b] = BuildingFields.Select(f=>f.Name=="base_stats" ? ((BaseStats)f.GetValue(b))?.Clone() : f.GetValue(b)).ToArray();
            foreach (ActorAsset a in AssetManager.actor_library.list)
                BaselineTemplates[a.id] = a.build_order_template_id;
        }
        public static void Install(GameObject host)
        {
            // Restore the exact pre-ModernBox vanilla/other-mod definitions before extending their final upgrades.
            foreach (var pair in BaselineBuildings)
                for (int i=0;i<BuildingFields.Length;i++)
                    if (!BuildingFields[i].IsInitOnly) BuildingFields[i].SetValue(pair.Key,pair.Value[i]);
            foreach (ActorAsset a in AssetManager.actor_library.list)
                if (BaselineTemplates.TryGetValue(a.id,out string template)) a.build_order_template_id=template;
            foreach (var era in EraLibrary.All)
            {
                int stage=era.key=="hyperfuture"?9:era.key=="modern"?6:era.key=="renaissance"?4:0;
                // Manu-Fix 026: later eras reuse modern barracks, docks, towers and the mine.
                // Reuse must not postpone their first availability from motorization to the future.
                foreach (var ids in era.cityBuildings.Values)
                    foreach (string id in ids)
                        if (!BaselineBuildings.Keys.Any(b=>b.id==id) &&
                            (!BuildingStages.TryGetValue(id,out int earliest) || stage<earliest)) BuildingStages[id]=stage;
                foreach (var ids in era.bonfires.Values)
                    foreach (string id in ids)
                        if (!BuildingStages.TryGetValue(id,out int earliest) || stage<earliest) BuildingStages[id]=stage;
            }
            // Bonfires follow earned eras through the same paid, research-gated upgrade path as buildings.
            // Removing their upgrade path would make the supplied future artwork unreachable.
            Ready=true;
            EnsureAllSpecies();
            foreach(BuildingAsset b in AssetManager.buildings.list.Where(b=>b.city_building&&b.cost!=null))
                _constructionCapacity=Math.Max(_constructionCapacity,2*Math.Max(Math.Max(b.cost.wood,b.cost.stone),Math.Max(b.cost.gold,b.cost.common_metals))+100);
            EnsureResourceCapacity();
            // Main.Awake already installs annotated patches once for this assembly.
            BindOtherMods();
            host.AddComponent<DevelopmentClock>();
            Debug.Log("[MB-FORSCHUNG] aktiv; Voelker="+RegisteredSpecies+"; Forschungsschritte="+Technologies.Length);
        }
        public static int EarnedStage(Kingdom kingdom)
        {
            if (kingdom?.data==null) return 0;
            kingdom.data.get(StageKey,out int stage,0);
            return Mathf.Clamp(stage,0,Technologies.Length);
        }
        public static int Stage(Kingdom kingdom)
        {
            // Explicit player era controls remain god powers; automatic research never writes an override into earned progress.
            string manual=StatManager.Instance?.eraoverride;
            if(manual=="medieval")return 0;
            if(manual=="renaissance")return 4;
            if(manual=="modern")return 8;
            if(manual=="hyperfuture")return 9;
            return EarnedStage(kingdom);
        }
        public static int Stage(City city) => Stage(city?.kingdom);
        public static string CultureEra(Culture culture)
        {
            int stage=culture!=null&&CultureStages.TryGetValue(culture.id,out int value)?value:0;
            return stage>=9?"Hyperfuture":stage>=6?"Modern":stage>=4?"Renaissance":"Medieval";
        }
        public static string Era(City city)
        {
            int stage=Stage(city);
            return stage>=9?"hyperfuture":stage>=6?"modern":stage>=4?"renaissance":"medieval";
        }
        public static string Group(ActorAsset asset)
        {
            if (asset!=null && SpeciesGroups.TryGetValue(asset.id,out string saved)) return saved;
            // Preserve ModernBox's existing faction/art assignment before applying the fallback for new species.
            if(asset!=null) foreach(var group in EraLibrary.All[0].cityBuildings)
                if(group.Value.Contains("house_"+asset.id+"_0"))return group.Key;
            string architecture=asset?.architecture_id??"human";
            return architecture=="dwarf"?"harden":architecture=="elf"?"gaia":architecture=="orc"?"horde":"alliance";
        }
        public static string VehicleSpecies(ActorAsset asset)
        { string group=Group(asset); return group=="harden"?"dwarf":group=="gaia"?"elf":group=="horde"?"orc":"human"; }
        public static void EnsureAllSpecies()
        {
            if (!Ready) return;
            foreach (ActorAsset a in AssetManager.actor_library.list.ToArray()) EnsureSpecies(a);
            _lastAssets=AssetManager.actor_library.list.Count;
            Type cache=AccessTools.TypeByName("Buldins.ArchCache");
            if(cache!=null) {
                AccessTools.Method(cache,"Build").Invoke(null,null);
                var architectures=(Dictionary<string,ArchitectureAsset>)AccessTools.Field(cache,"ArchByID").GetValue(null);
                var reverse=(Dictionary<string,Dictionary<string,string>>)AccessTools.Field(cache,"ReverseOrderByArchID").GetValue(null);
                foreach(var pair in ExpandedArchitectures) {
                    architectures[pair.Key]=pair.Value;
                    // The culture-style replacement path also uses the old civ_kingdom key, not ArchitectureResolver.
                    // Teach that actual consumer every new level so a style change cannot silently downgrade it.
                    var orders=new Dictionary<string,string>();
                    foreach(var order in pair.Value.building_ids_for_construction)orders[order.Value]=order.Key;
                    reverse[pair.Key]=orders;
                }
            }
        }
        public static void EnsureSpecies(ActorAsset a)
        {
            if (!Ready || a==null || !a.civ || a.isTemplateAsset() || !string.IsNullOrEmpty(a.grow_into_id)) return;
            if (SpeciesTemplates.TryGetValue(a.id,out string assigned)) { a.build_order_template_id=assigned; return; }
            ArchitectureAsset architecture=a.architecture_asset;
            // AssetLibrary.clone deliberately skips this NonSerialized runtime link. Resolve the actual registered ID.
            if(architecture==null&&!string.IsNullOrEmpty(a.architecture_id)) architecture=AssetManager.architecture_library.get(a.architecture_id);
            if (architecture?.building_ids_for_construction==null) return;
            a.architecture_asset=architecture;
            SpeciesGroups[a.id]=Group(a);
            // Each species keeps its existing art and gains its own missing levels; never rewrite a shared architecture.
            ArchitectureAsset originalArchitecture=architecture;
            architecture=AssetManager.architecture_library.clone("manu_mb_"+a.id,originalArchitecture.id);
            architecture.building_ids_for_construction=new Dictionary<string,string>(originalArchitecture.building_ids_for_construction);
            a.architecture_id=architecture.id; a.architecture_asset=architecture;
            if(!ExpandedArchitectures.ContainsKey(originalArchitecture.id))ExpandedArchitectures[originalArchitecture.id]=architecture;
            CityBuildOrderAsset original=AssetManager.city_build_orders.get(a.build_order_template_id);
            var template=new CityBuildOrderAsset { id="manu_mb_"+a.id };
            var advanced=AssetManager.city_build_orders.get("build_order_advanced");
            var combined=(original?.list??new List<BuildOrder>()).Concat(advanced.list);
            var keys=new HashSet<string>();
            foreach (BuildOrder order in combined)
            {
                if (order.id.Contains("_epochs") || order.id.StartsWith("order_bonfire_alliance") || order.id.StartsWith("order_bonfire_harden") || order.id.StartsWith("order_bonfire_gaia") || order.id.StartsWith("order_bonfire_horde")) continue;
                if (!keys.Add(order.id+"/"+order.upgrade)) continue;
                EnsureBuildingOrder(architecture,order.id);
                if (order.requirements_orders!=null) foreach (string required in order.requirements_orders) EnsureBuildingOrder(architecture,required);
                template.list.Add(new BuildOrder { id=order.id, required_pop=order.required_pop, required_buildings=order.required_buildings,
                    limit_type=order.limit_type, check_full_village=order.check_full_village, check_house_limit=order.check_house_limit,
                    min_zones=order.min_zones, upgrade=order.upgrade,
                    requirements_orders=order.requirements_orders, requirements_types=order.requirements_types });
            }
            foreach(string prefix in new[]{"house","hall","windmill"})
            {
                int maximum=prefix=="house"?5:prefix=="hall"?2:1;
                for(int level=0;level<maximum;level++)
                {
                    string from="order_"+prefix+"_"+level,to="order_"+prefix+"_"+(level+1);
                    EnsureBuildingOrder(architecture,from);EnsureBuildingOrder(architecture,to);
                    if(!architecture.building_ids_for_construction.ContainsKey(from)||!architecture.building_ids_for_construction.ContainsKey(to))continue;
                    BuildingAsset lower=AssetManager.buildings.get(architecture.building_ids_for_construction[from]);
                    BuildingAsset upper=AssetManager.buildings.get(architecture.building_ids_for_construction[to]);
                    if(lower!=null&&upper!=null&&lower.id!=upper.id) { lower.can_be_upgraded=true;lower.upgrade_to=upper.id; }
                }
            }
            foreach (string order in new[]{"order_house_5","order_hall_2","order_temple","order_barracks","order_docks_1","order_watch_tower","order_mine","order_bonfire"})
            {
                EnsureBuildingOrder(architecture,order);
                string baseId=architecture.building_ids_for_construction[order];
                BuildingAsset terminal=AssetManager.buildings.get(baseId);
                if (terminal==null) continue;
                string group=Group(a);
                string target=order=="order_house_5"?"House_rain_"+group:order=="order_hall_2"?"Hall_rain_"+group:
                    order=="order_temple"?"Temple_rain_"+group:order=="order_barracks"?"Barracks_rain_"+group:
                    order=="order_docks_1"?"Docks_modern_"+group:order=="order_watch_tower"?"watch_tower_modern_"+group:
                    order=="order_bonfire"?"bonfire_rain_"+group:"mine_modern";
                if (AssetManager.buildings.get(target)==null) continue;
                // A species-specific terminal avoids changing another species' architecture or a shared mine's upgrade target.
                // Manu-Fix 036: the vanilla bonfire is one asset for several architectures. Overwriting upgrade_to per species
                // let the last processed species decide every faction's renaissance bonfire (elves, dwarves and orcs got
                // bonfire_rain_alliance). Keep the target per group; SharedUpgradeTarget applies it for the upgrading city and
                // SharedRequirementChain lets every group's chain count as the existing building.
                if(!SharedUpgrades.TryGetValue(terminal.id,out Dictionary<string,string> byGroup)) SharedUpgrades[terminal.id]=byGroup=new Dictionary<string,string>();
                byGroup[group]=target;
                if(byGroup.Values.Distinct().Count()>1) SharedTerminals.Add(terminal.id);
                terminal.can_be_upgraded=true;
                if(byGroup.Count==1) terminal.upgrade_to=target;
                BuildingAsset upgraded=AssetManager.buildings.get(target);
                upgraded.housing_slots=Math.Max(upgraded.housing_slots,terminal.housing_slots);
                upgraded.max_houses=Math.Max(upgraded.max_houses,terminal.max_houses);
                upgraded.book_slots=Math.Max(upgraded.book_slots,terminal.book_slots);
                var visited=new HashSet<string>();BuildingAsset previous=upgraded;
                while(previous!=null&&visited.Add(previous.id)&&!string.IsNullOrEmpty(previous.upgrade_to)) {
                    BuildingAsset next=AssetManager.buildings.get(previous.upgrade_to);if(next==null)break;
                    next.housing_slots=Math.Max(next.housing_slots,previous.housing_slots);
                    next.max_houses=Math.Max(next.max_houses,previous.max_houses);
                    next.book_slots=Math.Max(next.book_slots,previous.book_slots);previous=next;
                }
                if (keys.Add(order+"/True")) template.addUpgrade(order);
            }
            foreach (string group in new[]{"alliance","harden","gaia","horde"})
            foreach (string id in new[]{"House_rain_"+group,"House_modern_"+group,"Barracks_rain_"+group,"bonfire_rain_"+group,"bonfire_modern_"+group})
            {
                string order="manu_mb_upgrade_"+id;
                architecture.addBuildingOrderKey(order,id);
                template.addUpgrade(order);
            }
            string market="market_"+Group(a);
            if(AssetManager.buildings.get(market)!=null) {
                architecture.addBuildingOrderKey("manu_mb_market",market);
                template.list.Add(new BuildOrder { id="manu_mb_market",required_pop=60,required_buildings=10,limit_type=1 });
            }
            template.prepareForAssetGeneration();
            AssetManager.city_build_orders.add(template);
            SpeciesTemplates[a.id]=template.id; a.build_order_template_id=template.id;
        }
        private static void EnsureBuildingOrder(ArchitectureAsset architecture,string order)
        {
            if (architecture.building_ids_for_construction.TryGetValue(order,out string present) && AssetManager.buildings.get(present)!=null) return;
            ArchitectureAsset human=AssetManager.actor_library.get("human").architecture_asset;
            if (!human.building_ids_for_construction.TryGetValue(order,out string fallback)) return;
            string ownBase=order.StartsWith("order_house_")?"order_house_0":order.StartsWith("order_hall_")?"order_hall_0":order.StartsWith("order_windmill_")?"order_windmill_0":null;
            if(ownBase!=null&&architecture.building_ids_for_construction.TryGetValue(ownBase,out string style))
            {
                string id=architecture.id+"_"+order;
                BuildingAsset extended=AssetManager.buildings.get(id);
                if(extended==null)
                {
                    BuildingAsset model=AssetManager.buildings.get(fallback);
                    extended=AssetManager.buildings.clone(id,style);
                    BuildingAsset source=AssetManager.buildings.get(style);
                    // A new asset ID has no resource directory of its own. Bind the existing species art explicitly,
                    // including after saving/loading, instead of relying on a transient BuildingSprites object.
                    extended.sprite_path=string.IsNullOrEmpty(source.sprite_path)?source.main_path+source.id:source.sprite_path;
                    extended.sprites_are_initiated=false;
                    extended.cost=new ConstructionCost(model.cost.wood,model.cost.stone,model.cost.common_metals,model.cost.gold);
                    extended.housing_slots=model.housing_slots;extended.max_houses=model.max_houses;extended.book_slots=Math.Max(extended.book_slots,model.book_slots);
                    extended.upgrade_level=model.upgrade_level;extended.can_be_upgraded=false;extended.upgrade_to=null;
                    extended.base_stats["health"]=Math.Max(extended.base_stats["health"],model.base_stats["health"]);
                }
                architecture.addBuildingOrderKey(order,id);
            }
            else architecture.addBuildingOrderKey(order,fallback);
        }
        public static bool AllowsBuilding(City city,BuildingAsset asset)
        {
            if (!Ready || city==null || asset==null) return asset!=null;
            return !BuildingStages.TryGetValue(asset.id,out int stage) || Stage(city)>=stage;
        }
        public static bool AllowsItem(City city,EquipmentAsset item)
        {
            if (!Ready || item==null) return item!=null;
            int stage=Stage(city);
            if (IsNuclearItem(item)) return NuclearAllowed(city?.kingdom);
            if (CustomItemsList.WeaponEras.TryGetValue(item.id,out string era))
                return stage>=(era=="Hyperfuture"?9:era=="Modern"?6:4);
            return true;
        }
        public static bool AllowsVehicle(City city,string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            int stage=Stage(city);
            if (id=="EliteBomber") return NuclearAllowed(city?.kingdom);
            if (id.IndexOf("Heli",StringComparison.OrdinalIgnoreCase)>=0 || id.IndexOf("Fighter",StringComparison.OrdinalIgnoreCase)>=0 || id.StartsWith("Bomber_")) return stage>=7;
            return true;
        }
        public static bool NuclearAllowed(Kingdom kingdom)
        {
            if (Stage(kingdom)<8) return false;
            if(!string.IsNullOrEmpty(StatManager.Instance?.eraoverride))return true;
            var cities=Cities(kingdom);
            return cities.Sum(c=>c.getPopulationPeople())>=1000 && cities.Sum(c=>c.countWarriors())>=150;
        }
        public static bool IsNuclearItem(EquipmentAsset item)
        { return item!=null&&(CustomItemsList.Kys.Contains(item.id)||item.id=="N2Attack"); }
        [HarmonyPatch(typeof(Actor),"tryToAttack")]
        private static class NuclearUseGate
        {
            // Loot and inherited equipment must not bypass the crafting gate after a kingdom fragments.
            [HarmonyPriority(Priority.First)]
            static bool Prefix(Actor __instance,ref bool __result)
            {
                if(!Ready||__instance?.asset==null||!IsNuclearItem(__instance.getWeaponAsset())||NuclearAllowed(__instance.kingdom))return true;
                __result=false;return false;
            }
        }
        public static List<City> Cities(Kingdom kingdom)
        { return kingdom==null?new List<City>():World.world.cities.list.Where(c=>c!=null && c.isAlive() && c.kingdom==kingdom).ToList(); }
        public static string Requirement(Kingdom kingdom,Technology technology)
        {
            var cities=Cities(kingdom);
            int pop=cities.Sum(c=>c.getPopulationPeople());
            int buildings=cities.Sum(c=>c.buildings.Count(b=>b.asset.city_building&&!b.isUnderConstruction()));
            int age=Math.Max(kingdom.getAge(),cities.Count==0?0:cities.Max(c=>c.getAge()));
            if (age<technology.Age) return T("Siedlungsalter ","Settlement age ")+age+"/"+technology.Age;
            if (pop<technology.Population) return T("Einwohner ","Population ")+pop+"/"+technology.Population;
            // Manu-Fix 042: research no longer counts warriors. With large armies wanted for their own sake, the warrior
            // threshold only held back kingdoms that had just lost a war. Army > 0 still marks the military stages that need
            // a barracks; nuclear crafting and use keep their own 1,000 inhabitants / 150 warriors gate.
            if (buildings<technology.Buildings) return T("fertige Stadtgebaeude ","Completed city buildings ")+buildings+"/"+technology.Buildings;
            if (technology.Id!="architecture" && !cities.Any(c=>c.hasBuildingType("type_library"))) return T("Bibliothek fehlt","Library required");
            if (technology.Army>0 && !cities.Any(c=>c.hasBuildingType("type_barracks"))) return T("Kaserne fehlt","Barracks required");
            return null;
        }
        public static void Tick()
        {
            if (!Ready || World.world==null || !Config.game_loaded || SmoothLoader.isLoading()) return;
            EnsureResourceCapacity();
            if (_lastAssets!=AssetManager.actor_library.list.Count) EnsureAllSpecies();
            foreach(ActorAsset a in AssetManager.actor_library.list.Where(a=>a.civ&&!SpeciesTemplates.ContainsKey(a.id)).ToArray())EnsureSpecies(a);
            int year=Date.getCurrentYear();
            foreach (Kingdom kingdom in World.world.kingdoms.list.ToArray())
            {
                if (kingdom?.data==null || !kingdom.isCiv() || !kingdom.isAlive()) continue;
                var cities=Cities(kingdom);
                // A coup, secession or conquest does not erase the knowledge retained by the inhabitants of its cities.
                int inherited=EarnedStage(kingdom);
                foreach(City city in cities) { city.data.get(StageKey,out int known,0);inherited=Math.Max(inherited,known); }
                if(inherited>EarnedStage(kingdom)) { kingdom.data.set(StageKey,inherited);kingdom.data.set(PointsKey,0f); }
                kingdom.data.get(PointsKey,out float retained,0f);
                foreach(City city in cities) {
                    city.data.get(StageKey,out int known,0);city.data.get(PointsKey,out float pending,0f);
                    if(known==inherited)retained=Math.Max(retained,pending);
                }
                kingdom.data.set(PointsKey,retained);
                foreach(City city in cities) {city.data.set(StageKey,inherited);city.data.set(PointsKey,retained);}
                kingdom.data.get(YearKey,out int last,year-1);
                if (year<=last) continue;
                kingdom.data.set(YearKey,year);
                int stage=EarnedStage(kingdom);
                if (stage>=Technologies.Length) continue;
                var tech=Technologies[stage];
                if (Requirement(kingdom,tech)!=null) continue;
                int paidYears=Math.Min(5,year-last);
                City sponsor=cities.Where(c=>HasSurplus(c,"gold",2*paidYears,true) && c.hasBuildingType("type_hall")).OrderByDescending(c=>c.amount_gold).FirstOrDefault();
                if (sponsor==null) continue;
                // A paid research year cannot consume the city's construction reserve or be duplicated after loading.
                sponsor.takeResource("gold",2*paidYears);
                int libraries=cities.Sum(c=>c.countBuildingsType("type_library"));
                float earned=1+libraries*2+Mathf.Sqrt(cities.Sum(c=>c.getPopulationPeople()))/10f;
                kingdom.data.get(PointsKey,out float points,0f);
                points+=earned*paidYears;
                if (points>=tech.Points)
                {
                    kingdom.data.set(StageKey,stage+1); kingdom.data.set(PointsKey,0f);
                    Debug.Log("[MB-FORSCHUNG] Jahr="+year+" Reich="+kingdom.id+" Technik="+tech.Id+" Einwohner="+cities.Sum(c=>c.getPopulationPeople())+" Heer="+cities.Sum(c=>c.countWarriors()));
                }
                else kingdom.data.set(PointsKey,points);
                kingdom.data.get(PointsKey,out float savedPoints,0f);
                foreach(City city in cities){city.data.set(StageKey,EarnedStage(kingdom));city.data.set(PointsKey,savedPoints);}
            }
            CultureStages.Clear();
            foreach (City city in World.world.cities.list.ToArray()) {
                if(city==null||!city.isAlive())continue;
                if(city.culture!=null) { CultureStages.TryGetValue(city.culture.id,out int value); CultureStages[city.culture.id]=Math.Max(value,Stage(city)); }
                if(city.leader!=null) Traits.VehicleSummonEffect(city.leader);
            }
            ReserveCache.Clear();
        }
        private static void EnsureResourceCapacity()
        {
            // Vanilla hard-caps a storage resource at 999 and stops deliveries at storage_max (usually 50).
            // ModernBox's default future bonfire alone costs 1000 gold. Capacity must allow the real bill,
            // its reserve and the food/industry needs of growing cities; this never grants resources.
            int population=World.world?.cities?.list?.Where(c=>c!=null&&c.isAlive()).Select(c=>c.getPopulationPeople()).DefaultIfEmpty(0).Max()??0;
            int capacity=Math.Max(_constructionCapacity,Math.Min(population,int.MaxValue/4)*4);
            foreach(ResourceAsset resource in AssetManager.resources.list) {
                if(!resource.food&&resource.id!="wood"&&resource.id!="stone"&&resource.id!="common_metals"&&resource.id!="gold")continue;
                resource.maximum=Math.Max(resource.maximum,capacity);
                resource.storage_max=Math.Max(resource.storage_max,capacity);
            }
        }
        public static string Report()
        {
            var text=new StringBuilder(T("<color=#FFD78C><b>Forschung und Aufbau</b></color>\n\nAlle Zivilisationsvoelker nutzen denselben Forschungsweg. Bibliotheken, bezahlte Forschung, Stadtbau und Heeresaufbau tragen den Fortschritt.\n\n","<color=#FFD78C><b>Research and development</b></color>\n\nAll civilization species share this research path. Libraries, funded research, cities and armies drive progress.\n\n"));
            if(!string.IsNullOrEmpty(StatManager.Instance?.eraoverride))text.Append(T("<color=#FFCE70>Goettereingriff aktiv: ","<color=#FFCE70>God override active: ")).Append(StatManager.Instance.eraoverride).Append(T(". Mit Automatische Forschung zur normalen Entwicklung zurueckkehren.</color>\n\n",". Select Automatic research to restore normal progression.</color>\n\n"));
            if(World.world==null||!Config.game_loaded)return text.Append(T("Erzeuge oder lade eine Welt.","Create or load a world.")).ToString();
            foreach(Kingdom kingdom in World.world.kingdoms.list.Where(k=>k!=null&&k.isAlive()&&k.isCiv()).OrderByDescending(k=>Cities(k).Sum(c=>c.getPopulationPeople())))
            {
                var cities=Cities(kingdom);int stage=EarnedStage(kingdom);kingdom.data.get(PointsKey,out float points,0f);
                string name=(kingdom.data.name??"Reich").Replace("<","").Replace(">","");
                text.Append("<color=#9DE5E2><b>").Append(name).Append("</b></color>\n").Append(cities.Sum(c=>c.getPopulationPeople())).Append(T(" Einwohner · "," people · ")).Append(cities.Sum(c=>c.countWarriors())).Append(T(" Soldaten · "," soldiers · ")).Append(cities.Count).Append(T(" Staedte\n"," cities\n"));
                text.Append(T("Erforscht: ","Researched: ")).Append(stage==0?T("Grundlagen","Foundations"):TechName(Technologies[stage-1])).Append('\n');
                if(stage<Technologies.Length) { var next=Technologies[stage];text.Append(T("Naechstes Ziel: ","Next goal: ")).Append(TechName(next)).Append(T("\nForschung: ","\nResearch: ")).Append(points.ToString("0.0")).Append('/').Append(next.Points).Append("\n").Append(Requirement(kingdom,next)??T("Aufbau erfuellt; Forschung aus Goldueberschuss","Development requirements met; research uses surplus gold")).Append('\n'); }
                text.Append('\n');
            }
            return text.ToString();
        }
        public static int VehicleLimit(City city)
        { return Math.Max(0,Math.Min(city.getPopulationPeople()/8,city.countWarriors()/3)); }
        public static int NavalLimit(City city) => Math.Max(1,city.getPopulationPeople()/150);
        public static bool CanProduceVehicle(City city)
        {
            if (city==null || !city.hasBuildingType("type_barracks") || city.getPopulationPeople()<80) return false;
            city.data.get("manu_mb_vehicle_year",out int last,int.MinValue);
            if (Date.getCurrentYear()<=last) return false;
            var cost=VehicleCost(city);
            // A resource the vehicle does not cost must not block it because the city sits at its reserve (metals hover at 5).
            return (cost.wood<=0||HasSurplus(city,"wood",cost.wood,true))&&(cost.common_metals<=0||HasSurplus(city,"common_metals",cost.common_metals,true))
                &&(cost.gold<=0||HasSurplus(city,"gold",cost.gold,true));
        }
        // Manu-Fix 037: modern vehicles no longer require 12 common metals above the reserve. Measured over 20 years in a
        // 58-city world, all cities together received about one metal per city and year and crafting consumed it at once;
        // no city could ever hold 17 metals, so not a single modern vehicle was paid. Gold and wood flow far better.
        private static ConstructionCost VehicleCost(City city) => Stage(city)>=6?new ConstructionCost(6,0,0,12):new ConstructionCost(8,0,0,5);
        public static void PayVehicle(City city)
        {
            var cost=VehicleCost(city); city.takeResource("wood",cost.wood);city.takeResource("common_metals",cost.common_metals);city.takeResource("gold",cost.gold);
            city.data.set("manu_mb_vehicle_year",Date.getCurrentYear());
        }
        public static bool HasSurplus(City city,string resource,int cost,bool ownProgress=false)
        { return city!=null && city.getResourcesAmount(resource)-Reserve(city,resource,ownProgress)>=cost; }
        // Manu-Fix 040: ownProgress = ModernBox's own research payment and vehicle production. They skip the next era
        // bonfire (300/700/1000 gold), which neither depends on; other mods' spending keeps the full reserve so the
        // bonfire savings stay protected. Before, stage-6+ research and every vehicle waited for 700 saved gold.
        public static int Reserve(City city,string resource,bool ownProgress=false)
        {
            if (city==null || _calculatingReserve || (resource!="wood"&&resource!="stone"&&resource!="gold"&&resource!="common_metals")) return 0;
            string key=city.id+"/"+city.buildings.Count+"/"+Stage(city)+(ownProgress?"/own":"");
            if (!ReserveCache.TryGetValue(key,out ConstructionCost reserve))
            {
                reserve=new ConstructionCost(20,20,5,100);
                _calculatingReserve=true;
                try
                {
                    // Manu-Fix 043: City.getActorAsset() is the species of the current leader. When that species has no build
                    // orders or architecture (measured: an infected host from a parasite mod leading a city after 740 years),
                    // the template lookup returned null and every call threw a NullReferenceException, 3,719 times in 40 years
                    // through a wall mod's cost check. Such a city keeps the base reserve, and an order whose building is
                    // missing from the architecture is skipped before the vanilla check reads it.
                    ActorAsset species=city.getActorAsset();
                    var template=species?.architecture_asset==null||string.IsNullOrEmpty(species.build_order_template_id)?null:AssetManager.city_build_orders.get(species.build_order_template_id);
                    if (template?.list!=null) foreach (BuildOrder order in template.list)
                    {
                        BuildingAsset building=order.getBuildingAsset(city);
                        if (building==null || !CityBehBuild.canUseBuildAsset(order,city)) continue;
                        if (order.upgrade) building=AssetManager.buildings.get(building.upgrade_to);
                        if (building?.cost==null || !AllowsBuilding(city,building)) continue;
                        if (ownProgress && building.type=="type_bonfire") continue;
                        reserve.wood=Math.Max(reserve.wood,building.cost.wood);reserve.stone=Math.Max(reserve.stone,building.cost.stone);
                        reserve.common_metals=Math.Max(reserve.common_metals,building.cost.common_metals);reserve.gold=Math.Max(reserve.gold,building.cost.gold);
                    }
                }
                finally { _calculatingReserve=false; }
                ReserveCache[key]=reserve;
            }
            return resource=="wood"?reserve.wood:resource=="stone"?reserve.stone:resource=="gold"?reserve.gold:reserve.common_metals;
        }
        private static void BindOtherMods()
        {
            var harmony=new Harmony("manu.modernbox.integration");
            foreach (string target in new[]{"CoreBox.JobEquipment:Ensure","WorldAscension.MilitarySupply:TryIssuePaid",
                "WorldAscension.ModernizationSystem:ProcessFactoryYear","WorldAscension.GovernanceSystem:Spend",
                "WorldAscension.GovernanceSystem:ApplyPolicy","WorldAscension.NationalPrograms:TryPlanNation",
                "WorldAscension.NationalPrograms:Commit","WallBox.WallMaterials:CanAfford"})
            {
                string[] parts=target.Split(':'); Type type=AccessTools.TypeByName(parts[0]);
                if (type==null) continue;
                foreach(MethodInfo method in AccessTools.GetDeclaredMethods(type).Where(m=>m.Name==parts[1]))
                    harmony.Patch(method,prefix:new HarmonyMethod(typeof(Development),nameof(EnterEconomy)),finalizer:new HarmonyMethod(typeof(Development),nameof(LeaveEconomy)));
                Debug.Log("[MB-ANBINDUNG] Baureserve: "+target);
            }
            Type nuclear=AccessTools.TypeByName("WorldAscension.NuclearTechnology");
            PatchOptional(harmony,nuclear,"CanStrike",nameof(NuclearPostfix));
            Type law=AccessTools.TypeByName("EconomyMod.Core.LawAi");
            PatchOptional(harmony,law,"StadtgoldVerfuegbar",nameof(LawGoldPostfix));
            Type modernization=AccessTools.TypeByName("WorldAscension.ModernizationSystem");
            if(modernization!=null) {
                PatchOptional(harmony,modernization,"CanCityCraftWeapon",nameof(OdysseyWeaponPostfix));
                PatchOptional(harmony,modernization,"MeetsAutomaticRequirements",nameof(OdysseyEraPostfix));
            }
            Type styles=AccessTools.TypeByName("Buldins.ArchitectureResolver");
            if(styles!=null) {
                harmony.Patch(AccessTools.Method(styles,"Resolve",new[]{typeof(City),typeof(string).MakeByRefType()}),
                    postfix:new HarmonyMethod(typeof(Development),nameof(StylePostfix)));
                Debug.Log("[MB-ANBINDUNG] Building Styles verwendet vollstaendige Bauentwicklung auch bei Kulturstilen");
            }
        }
        private static void EnterEconomy() { EconomyScope++; }
        private static Exception LeaveEconomy(Exception __exception) { EconomyScope=Math.Max(0,EconomyScope-1);return __exception; }
        private static void NuclearPostfix(Kingdom source,ref string reason,ref bool __result)
        { if(__result && !NuclearAllowed(source)) { __result=false;reason="ModernBox: Kerntechnik und ein entwickeltes Grossreich erforderlich."; } }
        private static void OdysseyWeaponPostfix(City city,string itemId,ref bool __result)
        {
            // CanCityCraftWeapon is also consulted for vanilla and other-mod weapons. Their own rules must survive.
            // Only Odyssey's five registered firearm/artillery IDs receive this additional era requirement.
            if(!__result)return;
            int required=itemId=="world_ascension_pistol_steel"?4:
                itemId=="world_ascension_rifle_steel"||itemId=="world_ascension_marksman_steel"||
                itemId=="world_ascension_grenade_launcher_steel"||itemId=="world_ascension_field_artillery_steel"?6:0;
            if(required>0&&Stage(city)<required)__result=false;
        }
        private static void PatchOptional(Harmony harmony,Type type,string method,string postfix)
        {
            if(type==null)return;
            MethodInfo target=AccessTools.GetDeclaredMethods(type).FirstOrDefault(m=>m.Name==method);
            if(target!=null)harmony.Patch(target,postfix:new HarmonyMethod(typeof(Development),postfix));
            else Debug.LogWarning("[MB-ANBINDUNG] Optional integration unavailable: "+type.FullName+"."+method);
        }
        private static void OdysseyEraPostfix(City city,int nextEra,ref bool __result)
        { if(__result) __result=Stage(city)>=(nextEra>=3?6:nextEra==2?5:2); }
        private static void LawGoldPostfix(City c,ref int __result)
        { __result=c==null?0:Math.Min(__result,Math.Max(0,c.getResourcesAmount("gold")-Reserve(c,"gold")-20)); }
        private static void StylePostfix(City pCity,ref string archID,ref ArchitectureAsset __result)
        {
            if(archID!=null&&ExpandedArchitectures.TryGetValue(archID,out ArchitectureAsset extended)) {
                __result=extended;archID=extended.id;
            }
        }

        [HarmonyPatch(typeof(CityBehBuild),nameof(CityBehBuild.canUseBuildAsset))]
        private static class BuildGate
        {
            static void Postfix(BuildOrder pBuildAsset,City pCity,ref bool __result)
            {
                if(!__result||!Ready)return;
                BuildingAsset asset=pBuildAsset.getBuildingAsset(pCity);
                if(pBuildAsset.upgrade) asset=AssetManager.buildings.get(asset.upgrade_to);
                if(asset==null || !AllowsBuilding(pCity,asset)) __result=false;
            }
        }
        [HarmonyPatch(typeof(CityBehBuild),nameof(CityBehBuild.upgradeBuilding))]
        private static class SharedUpgradeTarget
        {
            // Manu-Fix 036: runs before ModernBox's own upgrade prefixes, which read pBuilding.asset.upgrade_to.
            [HarmonyPriority(Priority.First)]
            static void Prefix(Building pBuilding,City pCity)
            {
                if(!Ready||pBuilding?.asset==null||pCity==null)return;
                if(!SharedUpgrades.TryGetValue(pBuilding.asset.id,out Dictionary<string,string> byGroup)||byGroup.Count<2)return;
                if(byGroup.TryGetValue(Group(pCity.getActorAsset()),out string target)&&AssetManager.buildings.get(target)!=null)
                    pBuilding.asset.upgrade_to=target;
            }
        }
        [HarmonyPatch(typeof(CityBehBuild),"haveRequiredBuildings")]
        private static class SharedRequirementChain
        {
            // Manu-Fix 036: vanilla accepts a required building when the city has it or one of its upgrades and follows
            // upgrade_to for that. On the shared vanilla bonfire this is the chain of the city that upgraded last, so cities
            // with another faction's bonfire (and conquered cities) failed every bonfire requirement: no new houses, hall,
            // windmill, docks, mine, tower or temple. A shared terminal is satisfied by any faction's chain.
            static void Postfix(BuildOrder pOrder,City pCity,ref bool __result)
            {
                if(__result||!Ready||pCity==null||pOrder?.requirements_orders==null||SharedTerminals.Count==0)return;
                bool shared=false;
                foreach(string required in pOrder.requirements_orders)
                    if(SharedTerminals.Contains(pOrder.getBuildingAsset(pCity,required)?.id??"")){shared=true;break;}
                if(!shared)return;
                foreach(string required in pOrder.requirements_orders)
                {
                    BuildingAsset building=pOrder.getBuildingAsset(pCity,required);
                    if(building!=null&&building.id==building.upgrade_to)continue;
                    if(!HasBuildingOrUpgrade(pCity,building,0))return;
                }
                __result=true;
            }
            private static bool HasBuildingOrUpgrade(City city,BuildingAsset building,int depth)
            {
                if(building==null||depth>16)return false;
                if(city.countBuildingsOfID(building.id)>0)return true;
                if(SharedTerminals.Contains(building.id))
                {
                    foreach(string target in SharedUpgrades[building.id].Values)
                        if(HasBuildingOrUpgrade(city,AssetManager.buildings.get(target),depth+1))return true;
                    return false;
                }
                return building.can_be_upgraded&&!string.IsNullOrEmpty(building.upgrade_to)&&building.upgrade_to!=building.id
                    &&HasBuildingOrUpgrade(city,AssetManager.buildings.get(building.upgrade_to),depth+1);
            }
        }
        [HarmonyPatch(typeof(Building),"canBeUpgraded")]
        private static class UpgradeGate
        { static void Postfix(Building __instance,ref bool __result) { if(__result&&Ready) __result=AllowsBuilding(__instance.city,AssetManager.buildings.get(__instance.asset.upgrade_to)); } }
        [HarmonyPatch(typeof(Building),"setTemplate")]
        private static class UpgradeStorage
        {
            static void Postfix(Building __instance)
            {
                // Manu-Fix 021: vanilla upgradeBuilding calls setTemplate, not setBuilding's storage initialization.
                // Renaissance halls/temples gain storage. Without this real container City.countFood dereferences null.
                if(!Ready||__instance?.data==null||__instance.asset==null||
                    (!BuildingStages.ContainsKey(__instance.asset.id)&&!__instance.asset.id.StartsWith("manu_mb_")))return;
                if(__instance.asset.storage&&__instance.data.resources==null)__instance.data.resources=new CityResources();
                if(__instance.asset.book_slots>0&&__instance.data.books==null)__instance.data.books=new StorageBooks();
            }
        }
        [HarmonyPatch(typeof(ItemCrafting),"hasEnoughResourcesToCraft")]
        private static class CraftGate
        {
            static void Postfix(EquipmentAsset pAsset,City pCity,ref bool __result)
            {
                if(!__result||!Ready)return;
                __result=AllowsItem(pCity,pAsset)
                    && (pAsset.cost_resource_id_1=="none"||HasSurplus(pCity,pAsset.cost_resource_id_1,pAsset.cost_resource_1))
                    && (pAsset.cost_resource_id_2=="none"||HasSurplus(pCity,pAsset.cost_resource_id_2,pAsset.cost_resource_2));
            }
        }
        [HarmonyPatch(typeof(City),nameof(City.getResourcesAmount))]
        private static class EconomyAvailable
        { static void Postfix(City __instance,string pResourceID,ref int __result) { if(Ready&&EconomyScope>0&&!_calculatingReserve) __result=Math.Max(0,__result-Reserve(__instance,pResourceID)-(pResourceID=="gold"?20:0)); } }
        [HarmonyPatch(typeof(City),nameof(City.getZoneRange))]
        private static class Territory
        { static void Postfix(ref int __result) { if(Ready&&World.world?.zone_calculator!=null) __result=Math.Max(__result,2*(World.world.zone_calculator.zones_total_x+World.world.zone_calculator.zones_total_y)); } }
        [HarmonyPatch(typeof(City),"recalculateMaxHouses")]
        private static class Housing
        { static void Postfix(City __instance) { if(Ready) __instance.status.houses_max=Math.Max(__instance.status.houses_max,__instance.zones.Count*64); } }
        [HarmonyPatch(typeof(City),nameof(City.getArmyMaxMultiplier))]
        private static class Army
        { static void Postfix(City __instance,ref float __result) { if(Ready) __result=Math.Max(__result,0.45f+Stage(__instance)*0.015f); } }
        [HarmonyPatch(typeof(City),"updateCityStatus")]
        private static class EquipmentStorage
        { static void Postfix(City __instance) { if(Ready) __instance.status.maximum_items=Math.Max(15,__instance.getPopulationPeople()); } }
        [HarmonyPatch(typeof(City),nameof(City.getLimitOfBuildingsType))]
        private static class Infrastructure
        {
            static void Postfix(City __instance,BuildOrder pElement,ref int __result)
            {
                if(!Ready||__result==0||pElement.upgrade)return;
                string type=pElement.getBuildingAsset(__instance)?.type;
                if(type=="type_bonfire"||type=="type_hall"||type=="type_stockpile")return;
                // No fixed count cap: supporting infrastructure grows with the settlement rather than crowding out its first houses.
                __result=Math.Max(__result,__instance.getPopulationPeople()/(type=="type_watch_tower"?60:180));
            }
        }
        [HarmonyPatch(typeof(Kingdom),nameof(Kingdom.getMaxCities))]
        private static class KingdomSize
        { static void Postfix(Kingdom __instance,ref int __result) { if(Ready) __result=Math.Max(__result,__instance.cities.Count+1); } }
    }
    public sealed class DevelopmentClock : MonoBehaviour
    {
        private float _next;
        private void Start() { DevelopmentWindow.Init(); }
        private void Update() { if(Time.unscaledTime<_next)return;_next=Time.unscaledTime+0.5f;Development.Tick();DevelopmentWindow.Refresh(); }
    }
}
