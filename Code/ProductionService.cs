using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class ProductionService
    {
        private static readonly Dictionary<string, float> LastProduction = new Dictionary<string, float>(StringComparer.Ordinal);
        private static readonly Dictionary<string, float> LastResourceTick = new Dictionary<string, float>(StringComparer.Ordinal);
        private static readonly Dictionary<int, float> LastConstruction = new Dictionary<int, float>();
        private static float _tick;
        private static string _humanDefaultNames;
        private static string _orcDefaultNames;
        private static string _elfDefaultNames;
        private static string _dwarfDefaultNames;
        private static bool? _lastIdeologyEnabled;
        internal static long CompletedCycles { get; private set; }

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.isPaused()) return;
            _tick += elapsed;
            if (_tick < 5f) return;
            _tick = 0f;

            ModernProgression.UpdateCultures();
            ApplyDynamicSettings();
            ActorsAndBuildingsRegistry.RepairOrphanedUnits();
            foreach (City city in World.world.cities.list.ToArray())
            {
                if (!ModernProgression.IsSupportedCity(city)) continue;
                if (ModernBoxSettings.Get("ConstructionOption")) TryConstruction(city);
                GenerateResources(city);
                if (ModernBoxSettings.Get("FactoriesOption")) TryProduceOne(city);
            }
            SiloLaunchEvents.Update();
            CompletedCycles++;
        }

        private static void TryConstruction(City city)
        {
            int cityKey = city.GetHashCode();
            float last;
            if (LastConstruction.TryGetValue(cityKey, out last) && Time.time - last < 5f) return;
            if (city.under_construction_building != null) return;
            if (TryEraUpgrade(city) || TryDirectConstruction(city)) LastConstruction[cityKey] = Time.time;
        }

        private static bool TryEraUpgrade(City city)
        {
            M2Era era = ModernProgression.GetEra(city);
            string race = ModernProgression.GetRace(city);
            foreach (BuildingUpgradeSpec upgrade in ContentRegistry.Upgrades
                .Where(candidate => candidate.Race == race && candidate.Era <= era)
                // Complete the most advanced available stage before starting
                // another building at the bottom of the chain.  With ascending
                // order, a growing city could supply a new native house every
                // cycle, so the service kept making Industrial houses and never
                // reached Modern -> Future on the houses it had already upgraded.
                .OrderByDescending(candidate => candidate.Era))
            {
                string sourceId;
                if (!ActorsAndBuildingsRegistry.UpgradeSources.TryGetValue(upgrade.TargetId, out sourceId)) continue;
                if (city.countBuildingsOfID(upgrade.TargetId) > 0 && IsSingleStructureChain(upgrade.TargetId)) continue;
                BuildingAsset target = AssetManager.buildings.get(upgrade.TargetId);
                if (target == null || !city.hasEnoughResourcesFor(target.cost)) continue;
                List<Building> sources = city.getBuildingListOfID(sourceId);
                if (sources == null) continue;
                foreach (Building source in sources.ToArray())
                {
                    if (source == null || !source.isAlive() || source.isUnderConstruction()) continue;
                    if (ai.behaviours.CityBehBuild.upgradeBuilding(source, city)) return true;
                }
            }
            return false;
        }

        private static bool IsSingleStructureChain(string targetId)
        {
            BuildingSpec spec = ContentRegistry.Buildings.Find(candidate => candidate.Id == targetId);
            return spec != null && (spec.Type == "barracks" || spec.Type == "watch_tower" || spec.Type == "mine" || spec.Type == "temple" || spec.Type == "hall" || spec.Type == "dock");
        }

        private static bool TryDirectConstruction(City city)
        {
            M2Era era = ModernProgression.GetEra(city);
            foreach (BuildingSpec spec in ContentRegistry.Buildings
                .Where(candidate => !candidate.UpgradeOnly && !candidate.Civilian && candidate.Era <= era)
                .OrderBy(candidate => city.countBuildingsOfID(candidate.Id)))
            {
                if (city.countBuildingsOfID(spec.Id) >= spec.Limit) continue;
                if (TryNativeBuild(city, spec.Id)) return true;
            }
            if (era < M2Era.Renaissance) return false;
            BuildingSpec civilian = ContentRegistry.Buildings
                .Where(candidate => candidate.Civilian && candidate.Era <= era)
                .OrderBy(candidate => city.countBuildingsOfID(candidate.Id))
                .ThenBy(candidate => candidate.Id, StringComparer.Ordinal)
                .FirstOrDefault();
            return civilian != null && TryNativeBuild(city, civilian.Id);
        }

        private static bool TryNativeBuild(City city, string id)
        {
            BuildingAsset asset = AssetManager.buildings.get(id);
            if (asset == null || !city.hasEnoughResourcesFor(asset.cost)) return false;
            return ai.behaviours.CityBehBuild.tryToBuild(city, asset) != null;
        }

        private static void TryProduceOne(City city)
        {
            M2Era era = ModernProgression.GetEra(city);
            foreach (FactorySpec factory in ContentRegistry.Factories)
            {
                if (era < factory.Era || !ModernBoxSettings.Get(factory.SettingKey)) continue;
                Building producer = factory.BuildingId == "$era_barracks$"
                    ? FindEraBarracks(city, era)
                    : FindUsableBuilding(city, factory.BuildingId);
                if (producer == null) continue;
                string timerKey = city.GetHashCode() + "|" + factory.BuildingId;
                float last;
                if (LastProduction.TryGetValue(timerKey, out last) && Time.time - last < factory.Interval) continue;

                string unitId = factory.BuildingId == "$era_barracks$"
                    ? SelectBarracksUnit(city, era)
                    : (factory.UnitIds.Length == 0 ? null : factory.UnitIds[0]);
                if (string.IsNullOrEmpty(unitId)) continue;
                ModernUnitSpec unit = ContentRegistry.Units.Find(candidate => candidate.Id == unitId);
                if (unit == null) continue;
                if (!unit.Humanoid && CountVehicles(city) >= 40) return;
                if (unit.Humanoid && CountBarracksSoldiers(city) >= Mathf.Clamp(city.getPopulationPeople() / 5, 4, 40)) continue;

                WorldTile tile = producer.door_tile ?? city.getTile(false);
                if (tile == null) return;
                Actor actor = null;
                try
                {
                    actor = World.world.units.createNewUnit(unitId, tile, true, unit.Flying ? 1.5f : 0.35f, null);
                    if (actor == null) return;
                    actor.setCity(city);
                    actor.setKingdom(city.kingdom);
                    ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(actor);
                    actor.setHomeBuilding(producer);
                    AssignCombatRole(city, actor, unit);
                    if (actor.city != city || actor.kingdom != city.kingdom || !actor.is_profession_warrior)
                        throw new InvalidOperationException("M2 unit ownership or warrior assignment failed.");
                    LastProduction[timerKey] = Time.time;
                    ModernBoxDiagnostics.Info(city + " produced " + unitId + " at " + producer.asset.id + ".");
                }
                catch (Exception exception)
                {
                    ModernBoxDiagnostics.Error("M2 production failed for " + unitId + ": " + exception);
                    if (actor != null && actor.isAlive()) actor.die(true, AttackType.Other, false, false);
                }
                return;
            }
        }

        private static Building FindEraBarracks(City city, M2Era era)
        {
            string race = ModernProgression.GetRace(city);
            foreach (BuildingSpec spec in ContentRegistry.Buildings
                .Where(candidate => candidate.UpgradeOnly && candidate.Race == race && candidate.Type == "barracks" && candidate.Era <= era)
                .OrderByDescending(candidate => candidate.Era))
            {
                Building building = FindUsableBuilding(city, spec.Id);
                if (building != null) return building;
            }
            return null;
        }

        private static string SelectBarracksUnit(City city, M2Era era)
        {
            M2UnitRole role = RollRole(era);
            string race = ModernProgression.GetRace(city);
            string[] originalIds = OriginalBarracksCandidates(race, era, role);
            List<string> candidates = originalIds.Where(id => ContentRegistry.Units.Any(candidate =>
                candidate.Id == id && candidate.Era == era)).ToList();
            if (candidates.Count == 0) return null;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private static string[] OriginalBarracksCandidates(string race, M2Era era, M2UnitRole role)
        {
            // These are the highest-tech branch tables from original M2's
            // CartTransformations. They intentionally replace, rather than append
            // to, the previous era whenever a culture advances.
            switch (role)
            {
                case M2UnitRole.Offensive:
                    if (era == M2Era.Future)
                    {
                        string infantry = race == "orc" ? "spaceork" : "SpaceMarine";
                        return new[] { infantry, "Terran", "teslatruckgun", "atst", infantry, "artilleryatst", "atstsniper" };
                    }
                    if (era == M2Era.Modern) return new[] { "modernhumvee", "wwartillery" };
                    if (era == M2Era.Industrial) return new[] { "Humvee", "wwartillery" };
                    if (era == M2Era.Renaissance)
                    {
                        if (race == "orc") return new[] { "ogreunit", "orccannon", "armoredwolf" };
                        if (race == "dwarf") return new[] { "dwarfcannon" };
                        if (race == "elf") return new[] { "treant", "elfcannon" };
                        return new[] { "humancavalry", "humancannon" };
                    }
                    break;

                case M2UnitRole.Heavy:
                    if (era == M2Era.Future) return new[] { "P9000", "dreadnaught", "Railgun", "baseMA9000" };
                    if (era == M2Era.Modern) return new[] { "Tank", "MissileSystem", "wheeledtank" };
                    if (era == M2Era.Industrial) return new[] { "AbramTank", "shermanww", "tankie", "genericwwtank", "landship", "bigtankww" };
                    if (era == M2Era.Renaissance) return new[] { "davincitank" };
                    break;

                case M2UnitRole.Support:
                    if (era == M2Era.Future) return new[] { "AT9000", "supportatst" };
                    if (era == M2Era.Modern) return new[] { "modernsupporttruck" };
                    if (era == M2Era.Industrial) return new[] { "wwsupporttruck" };
                    if (era == M2Era.Renaissance)
                    {
                        if (race == "orc") return new[] { "orcwarlock" };
                        if (race == "dwarf") return new[] { "dwarfdoctor" };
                        if (race == "elf") return new[] { "fairelf" };
                        return new[] { "humanpaladin" };
                    }
                    break;

                case M2UnitRole.Air:
                    if (era == M2Era.Future) return new[] { "HeliELite", "eliteGunship", "TIEfighter", "EliteBomber" };
                    if (era == M2Era.Modern) return new[] { "Heli", "MIRVBomber", "FighterJet", "F55FighterJet" };
                    if (era == M2Era.Industrial) return new[] { "Zeppelin", "EliteZeppelin", "americanbomberww", "biplane", "fighterww" };
                    if (era == M2Era.Renaissance)
                    {
                        if (race == "orc") return new[] { "orccannon", "armoredwolf" };
                        if (race == "dwarf") return new[] { "Gunship" };
                        if (race == "elf") return new[] { "bigfaerydragon" };
                        return new[] { "balloonunit" };
                    }
                    break;

                case M2UnitRole.Titan:
                    if (era == M2Era.Future) return new[] { "HumanTitan" };
                    break;
            }
            return Array.Empty<string>();
        }

        private static M2UnitRole RollRole(M2Era era)
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (era == M2Era.Renaissance)
                return roll < 50 ? M2UnitRole.Offensive : roll < 75 ? M2UnitRole.Heavy : roll < 95 ? M2UnitRole.Support : M2UnitRole.Air;
            if (era == M2Era.Industrial)
                return roll < 40 ? M2UnitRole.Offensive : roll < 70 ? M2UnitRole.Heavy : roll < 90 ? M2UnitRole.Support : M2UnitRole.Air;
            if (era == M2Era.Modern)
                return roll < 35 ? M2UnitRole.Offensive : roll < 65 ? M2UnitRole.Heavy : roll < 85 ? M2UnitRole.Support : M2UnitRole.Air;
            return roll < 35 ? M2UnitRole.Offensive : roll < 60 ? M2UnitRole.Heavy : roll < 80 ? M2UnitRole.Support : roll < 95 ? M2UnitRole.Air : M2UnitRole.Titan;
        }

        private static void AssignCombatRole(City city, Actor actor, ModernUnitSpec unit)
        {
            if (unit.Humanoid) city.makeWarrior(actor);
            else
            {
                actor.setProfession(UnitProfession.Warrior, true);
                city.status.warriors_current++;
            }
        }

        private static Building FindUsableBuilding(City city, string id)
        {
            List<Building> buildings = city.getBuildingListOfID(id);
            return buildings == null ? null : buildings.FirstOrDefault(building => building != null && building.isAlive() && building.isUsable() && !building.isUnderConstruction());
        }

        private static int CountVehicles(City city)
        {
            int count = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || !actor.isAlive() || actor.city != city || actor.asset == null) continue;
                ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == actor.asset.id);
                if (spec != null && !spec.Humanoid) count++;
            }
            return count;
        }

        private static int CountBarracksSoldiers(City city)
        {
            int count = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || !actor.isAlive() || actor.city != city || actor.asset == null) continue;
                ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == actor.asset.id);
                if (spec != null && spec.Humanoid) count++;
            }
            return count;
        }

        private static void GenerateResources(City city)
        {
            Generate(city, "commerce", 30f,
                city.countBuildingsOfID("casino") + city.countBuildingsOfID("restaurant") + city.countBuildingsOfID("mall"), "gold", 500);
            Generate(city, "school", 60f, city.countBuildingsOfID("school"), "CyberWareParts", 100);
            int factories = 0;
            foreach (string id in ModernBoxCatalog.FactoryBuildingIds) factories += city.countBuildingsOfID(id);
            Generate(city, "factories", 30f, factories, "Parts", 200);
            int strategic = city.countBuildingsOfID("AirFactory") + city.countBuildingsOfID("TerranFactory") +
                city.countBuildingsOfID("P9000Factory") + city.countBuildingsOfID("MissileSilo");
            Generate(city, "xenium", 120f, strategic, "Xenium", 50);
        }

        private static void Generate(City city, string timer, float interval, int buildingCount, string resource, int cap)
        {
            if (buildingCount <= 0 || city.getResourcesAmount(resource) >= cap) return;
            string key = city.GetHashCode() + "|" + timer;
            float last;
            if (LastResourceTick.TryGetValue(key, out last) && Time.time - last < interval) return;
            LastResourceTick[key] = Time.time;
            city.addResourcesToRandomStockpile(resource, Math.Min(buildingCount, cap - city.getResourcesAmount(resource)));
        }

        internal static void ApplyDynamicSettings()
        {
            BuildingAsset silo = AssetManager.buildings.get("MissileSilo");
            if (silo != null) silo.tower = false;

            bool ideologyEnabled = ModernBoxSettings.Get("IdeologiesOption");
            foreach (string id in EquipmentAndTraitsRegistry.IdeologyIds)
            {
                ActorTrait trait = AssetManager.traits.get(id);
                if (trait == null) continue;
                trait.rate_birth = ideologyEnabled ? 37 : 0;
                trait.rate_inherit = ideologyEnabled ? 100 : 0;
            }
            if (_lastIdeologyEnabled != ideologyEnabled)
            {
                _lastIdeologyEnabled = ideologyEnabled;
                if (ideologyEnabled) EquipmentAndTraitsRegistry.BackfillDefaultIdeologies();
            }

            ActorAsset human = AssetManager.actor_library.get("human");
            ActorAsset orc = AssetManager.actor_library.get("orc");
            ActorAsset elf = AssetManager.actor_library.get("elf");
            ActorAsset dwarf = AssetManager.actor_library.get("dwarf");
            if (_humanDefaultNames == null)
            {
                _humanDefaultNames = human == null ? null : human.name_template_unit;
                _orcDefaultNames = orc == null ? null : orc.name_template_unit;
                _elfDefaultNames = elf == null ? null : elf.name_template_unit;
                _dwarfDefaultNames = dwarf == null ? null : dwarf.name_template_unit;
            }
            bool names = ModernBoxSettings.Get("namesOption");
            if (human != null) human.name_template_unit = names ? "Modern_human_Names" : _humanDefaultNames;
            if (orc != null) orc.name_template_unit = names ? "Modern_orc_Names" : _orcDefaultNames;
            if (elf != null) elf.name_template_unit = names ? "Modern_elf_Names" : _elfDefaultNames;
            if (dwarf != null) dwarf.name_template_unit = names ? "Modern_dwarf_Names" : _dwarfDefaultNames;
        }

        internal static bool IsEquipmentEnabled(string id, City city)
        {
            M2Era required;
            if (!ModernBoxCatalog.EquipmentEras.TryGetValue(id, out required)) return true;
            if (!ModernProgression.IsSupportedCity(city) || ModernProgression.GetEra(city) < required) return false;
            if (!ModernBoxSettings.Get("EquipmentOption")) return false;
            if (ModernBoxCatalog.MirvIds.Contains(id)) return ModernBoxSettings.Get("MIRVOption");
            if (id.StartsWith("Pipe", StringComparison.Ordinal)) return ModernBoxSettings.Get("PipeGunOption");
            if (id == "Sandevistan" || id == "TurboBooster") return ModernBoxSettings.Get("CyberwareOption");
            if (id == "Meth" || id == "Crack") return ModernBoxSettings.Get("DrugsOption");
            return ModernBoxSettings.Get("GunOption");
        }

        internal static bool IsMirvItem(Item item)
        {
            EquipmentAsset asset = item == null ? null : item.getAsset();
            return asset != null && ModernBoxCatalog.MirvIds.Contains(asset.id);
        }

        internal static int RemoveDisabledMirvsFromCityStorage(City city)
        {
            // The M2 toggle controls crafting only. Existing items and MissileSystem
            // projectiles intentionally remain usable when nuclear crafting is off.
            return 0;
        }
    }
}
