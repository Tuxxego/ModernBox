using System;
using System.Collections.Generic;

namespace ModernBoxM2Rewrite
{
    internal static partial class ContentRegistry
    {
        internal static readonly List<ModernUnitSpec> Units = new List<ModernUnitSpec>();
        private static readonly Dictionary<string, ModernUnitSpec> UnitsById = new Dictionary<string, ModernUnitSpec>(StringComparer.Ordinal);
        private static int _unitsIndexed = -1;

        // Called from per-unit hooks, so look specs up by id instead of scanning the list.
        internal static ModernUnitSpec FindUnit(string id)
        {
            if (id == null) return null;
            if (_unitsIndexed != Units.Count)
            {
                UnitsById.Clear();
                foreach (ModernUnitSpec spec in Units)
                    if (spec != null && spec.Id != null && !UnitsById.ContainsKey(spec.Id)) UnitsById[spec.Id] = spec;
                _unitsIndexed = Units.Count;
            }
            ModernUnitSpec found;
            return UnitsById.TryGetValue(id, out found) ? found : null;
        }
        internal static readonly List<BuildingSpec> Buildings = new List<BuildingSpec>();
        internal static readonly List<BuildingUpgradeSpec> Upgrades = new List<BuildingUpgradeSpec>();
        internal static readonly List<FactorySpec> Factories = new List<FactorySpec>();
        internal static readonly List<EquipmentSpec> Equipment = new List<EquipmentSpec>();
        internal static readonly List<BombSpec> Bombs = new List<BombSpec>();
        internal static readonly List<InvasionSpec> Invasions = new List<InvasionSpec>();
        internal static string Summary { get; private set; }

        internal static void RegisterAll()
        {
            BuildSpecifications();
            M2OldVanillaStats.SnapshotGameTraits();
            M2AttackSpeed.AllowOriginalSlowest();
            EquipmentAndTraitsRegistry.RegisterResourcesAndProjectiles();
            EquipmentAndTraitsRegistry.RegisterEquipment();
            EquipmentAndTraitsRegistry.RegisterTraits();
            EquipmentAndTraitsRegistry.RegisterNames();
            ActorsAndBuildingsRegistry.RegisterUnits();
            Safe("AlienJungleRegistry.Register", () => AlienJungleRegistry.Register());
            ActorsAndBuildingsRegistry.RegisterBuildingsAndOrders();
            Safe("M2Creatures.Register", () => M2Creatures.Register());
            Safe("BombRegistry.RegisterBombs", () => BombRegistry.RegisterBombs());
            ProductionService.ApplyDynamicSettings();
            ModernLocalization.Apply();
            Summary = Units.Count + " units, " + Buildings.Count + " buildings, " +
                Equipment.Count + " equipment, " + Bombs.Count + " bombs";
            Safe("ModernBoxDiagnostics.ValidateRegisteredContent", () => ModernBoxDiagnostics.ValidateRegisteredContent());
            Safe("ModernBoxDiagnostics.ValidateAssets", () => ModernBoxDiagnostics.ValidateAssets());
            ModernBoxDiagnostics.Info("Registered " + Summary + ".");
            Safe("M2Space.Register", () => M2Space.Register());
            Safe("M2HeldSpriteGuard.Apply", () => M2HeldSpriteGuard.Apply());
        }

        // One broken feature shouldn't stop the whole mod from loading.
        private static void Safe(string name, Action action)
        {
            try { action(); }
            catch (Exception e) { ModernBoxDiagnostics.Error(name + " failed: " + e); }
        }

        private static void BuildSpecifications()
        {
            Units.Clear();
            Buildings.Clear();
            Upgrades.Clear();
            Factories.Clear();
            Equipment.Clear();
            Bombs.Clear();
            Invasions.Clear();
            ModernBoxCatalog.UnitIds.Clear();
            ModernBoxCatalog.EquipmentIds.Clear();
            ModernBoxCatalog.EquipmentEras.Clear();
            ModernBoxCatalog.EquipmentTiers.Clear();

            BuildUnitSpecifications();
            // Original M2's barracks transformation tables are the authoritative
            // era assignment. Several actor files had stale or absent tech fields,
            // which made a source-only inference put them in the wrong era.
            SetUnitEra("AbramTank", M2Era.Industrial);
            SetUnitEra("biplane", M2Era.Industrial);
            SetUnitEra("EliteZeppelin", M2Era.Industrial);
            SetUnitEra("F55FighterJet", M2Era.Modern);
            SetUnitEra("fairelf", M2Era.Renaissance);
            SetUnitEra("Gunship", M2Era.Renaissance);
            SetUnitEra("Humvee", M2Era.Industrial);
            SetUnitEra("landship", M2Era.Industrial);
            SetUnitEra("Railgun", M2Era.Future);
            SetUnitEra("Tank", M2Era.Modern);
            SetUnitEra("Zeppelin", M2Era.Industrial);
            BuildBuildingSpecifications();
            BuildFactorySpecifications();
            BuildEquipmentSpecifications();
            ApplyOriginalMaterialCosts();
            ApplyM2EquipmentOverrides();
            ApplyOriginalMaterialStats();
            BuildBombSpecifications();
            BuildInvasionSpecifications();
        }

        private static void SetUnitEra(string id, M2Era era)
        {
            ModernUnitSpec spec = Units.Find(candidate => candidate.Id == id);
            if (spec != null) spec.Era = era;
        }

        private static void BuildBuildingSpecifications()
        {
            // Original M2 2.1.0.1 had these commerce buildings and factories inside a
            // commented-out block, so cities never built them. They stay registered
            // only so older rewrite saves that contain them still load.
            AddLegacyBuilding("casino", M2Era.Renaissance, Cost(15, 25, 0, 250), 0);
            AddLegacyBuilding("restaurant", M2Era.Renaissance, Cost(15, 25, 0, 250), 0);
            AddLegacyBuilding("mall", M2Era.Renaissance, Cost(15, 25, 0, 250), 0);
            AddLegacyBuilding("school", M2Era.Renaissance, Cost(15, 25, 0, 250), 0);
            AddLegacyBuilding("modernbuilding", M2Era.Renaissance, Cost(0, 2, 1, 1), 60);
            foreach (string factory in new[] { "HumveeFactory", "TankFactory", "AirshipFactory", "RailgunFactory", "HelicopterFactory", "DroneFactory", "FighterJetFactory", "BoiFactory", "GunshipFactory", "AirFactory", "TerranFactory", "P9000Factory" })
                AddLegacyBuilding(factory, M2Era.Modern, Cost(0, 2, 1, 1), 60);

            AddBuilding("MissileSilo", "MissileSilo", "", M2Era.Industrial, Cost(0, 0, 32, 72), 1, 0, false, true, false);
            Buildings[Buildings.Count - 1].Health = 2500f;

            foreach (string race in ModernBoxCatalog.SupportedRaces)
            {
                AddEraChain(race, "barracks", false, new[]
                {
                    EraUpgrade("Barracks_rain_human", M2Era.Renaissance, Cost(0, 1, 1, 1), 500f, 0),
                    EraUpgrade("Barracks_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 1), 700f, 0),
                    EraUpgrade("Barracks_modern_human", M2Era.Modern, Cost(0, 1, 1, 1), 1000f, 0),
                    EraUpgrade("Barracks_future_human", M2Era.Future, Cost(0, 1, 1, 1), 2000f, 0)
                });
                AddEraChain(race, "watch_tower", true, new[]
                {
                    EraUpgrade("watch_tower_rain_human", M2Era.Renaissance, Cost(1, 10, 5, 1), 5000f, 0),
                    EraUpgrade("watch_tower_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 1), 5000f, 0),
                    EraUpgrade("watch_tower_modern_human", M2Era.Modern, Cost(0, 1, 5, 5), 6000f, 0),
                    EraUpgrade("watch_tower_future_human", M2Era.Future, Cost(0, 1, 10, 10), 7000f, 0)
                });
                AddEraChain(race, "mine", false, new[]
                {
                    EraUpgrade("mine_rain_human", M2Era.Renaissance, Cost(0, 0, 0, 1), 2000f, 0),
                    EraUpgrade("mine_industrial_human", M2Era.Industrial, Cost(0, 0, 0, 1), 3000f, 0),
                    EraUpgrade("mine_modern_human", M2Era.Modern, Cost(0, 0, 0, 1), 4000f, 0),
                    EraUpgrade("mine_future_human", M2Era.Future, Cost(0, 0, 0, 1), 8000f, 0)
                });
                AddEraChain(race, "temple", false, new[]
                {
                    EraUpgrade("temple_rain_human", M2Era.Renaissance, Cost(1, 1, 1, 1), 4000f, 0),
                    EraUpgrade("temple_industrial_human", M2Era.Industrial, Cost(1, 1, 1, 1), 5000f, 0),
                    EraUpgrade("temple_modern_human", M2Era.Modern, Cost(1, 1, 1, 1), 6000f, 0),
                    EraUpgrade("temple_future_human", M2Era.Future, Cost(1, 1, 1, 1), 7000f, 0)
                });
                AddEraChain(race, "house", false, new[]
                {
                    EraUpgrade("house_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 0), 800f, 12),
                    EraUpgrade("house_modern_human", M2Era.Modern, Cost(0, 1, 1, 1), 1000f, 16),
                    EraUpgrade("house_future_human", M2Era.Future, Cost(0, 1, 1, 1), 2000f, 20)
                });
                AddEraChain(race, "hall", false, new[]
                {
                    EraUpgrade("hall_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 0), 800f, 20),
                    EraUpgrade("hall_modern_human", M2Era.Modern, Cost(0, 1, 1, 1), 1000f, 30),
                    EraUpgrade("hall_future_human", M2Era.Future, Cost(0, 1, 1, 1), 2000f, 40)
                });
                AddEraChain(race, "dock", false, new[]
                {
                    EraUpgrade("dock_rain_human", M2Era.Renaissance, Cost(0, 0, 0, 1), 15000f, 0),
                    EraUpgrade("dock_industrial_human", M2Era.Industrial, Cost(0, 0, 0, 1), 20000f, 0),
                    EraUpgrade("dock_modern_human", M2Era.Modern, Cost(0, 0, 0, 1), 50000f, 0)
                });
            }
        }

        private static void AddEraChain(string race, string chain, bool tower, EraBuilding[] stages)
        {
            string previous = "$native_" + chain + "_" + race + "$";
            foreach (EraBuilding stage in stages)
            {
                string id = RaceAdvancedId(stage.ArtId, race);
                AddBuilding(id, stage.ArtId, race, stage.Era, stage.Cost, int.MaxValue, stage.Housing, false, tower, true);
                Buildings[Buildings.Count - 1].Type = chain;
                Buildings[Buildings.Count - 1].Health = stage.Health;
                Buildings[Buildings.Count - 1].UpgradeFrom = previous;
                if (!previous.StartsWith("$native_", StringComparison.Ordinal))
                {
                    BuildingSpec prior = Buildings.Find(candidate => candidate.Id == previous);
                    if (prior != null) prior.UpgradeTo = id;
                }
                Upgrades.Add(new BuildingUpgradeSpec { Race = race, SourceId = previous, TargetId = id, Era = stage.Era });
                previous = id;
            }
        }

        private static string RaceAdvancedId(string humanArtId, string race)
        {
            if (race == "human") return humanArtId;
            const string suffix = "_human";
            return humanArtId.EndsWith(suffix, StringComparison.Ordinal)
                ? humanArtId.Substring(0, humanArtId.Length - suffix.Length) + "_" + race
                : humanArtId + "_" + race;
        }

        private static void AddLegacyBuilding(string id, M2Era era, ConstructionCost cost, int housing)
        {
            AddBuilding(id, id, "", era, cost, 1, housing, true, false, false);
            Buildings[Buildings.Count - 1].Legacy = true;
        }

        private static void AddBuilding(string id, string source, string race, M2Era era, ConstructionCost cost,
            int limit, int housing, bool civilian, bool tower, bool upgradeOnly)
        {
            Buildings.Add(new BuildingSpec
            {
                Id = id,
                SourceId = source,
                Race = race,
                Era = era,
                Tier = (ProgressionTier)(int)era,
                Cost = cost,
                Limit = limit,
                Housing = housing,
                Civilian = civilian,
                Tower = tower,
                UpgradeOnly = upgradeOnly
            });
        }

        private static EraBuilding EraUpgrade(string art, M2Era era, ConstructionCost cost, float health, int housing)
        {
            return new EraBuilding { ArtId = art, Era = era, Cost = cost, Health = health, Housing = housing };
        }

        private static ConstructionCost Cost(int wood, int stone, int metal, int gold)
        {
            return new ConstructionCost(wood, stone, metal, gold);
        }

        private static void BuildFactorySpecifications()
        {
            // Original M2 armies come only from barracks (see ProductionService).
        }

        // M2 items had no cost of their own; the old game charged the item's material
        // (ItemWeapon/Armor/AccessoryMaterialLibrary). Most M2 items were copper, the pipe
        // guns and pirate pistol iron, the musket and pristine set steel.
        private static readonly HashSet<string> IronItems = new HashSet<string>(StringComparer.Ordinal)
            { "PipeRifle", "PipePistol", "PipeShotgun", "piratpistol" };
        private static readonly HashSet<string> SteelItems = new HashSet<string>(StringComparer.Ordinal)
            { "Musket", "pristinearmor", "pristineboots", "pristinehelmet" };

        private static void ApplyOriginalMaterialCosts()
        {
            foreach (EquipmentSpec spec in Equipment)
            {
                int metals = SteelItems.Contains(spec.Id) ? 4 : IronItems.Contains(spec.Id) ? 3 : 1;
                bool accessory = spec.Type == EquipmentType.Amulet || spec.Type == EquipmentType.Ring;
                spec.Resource1 = "common_metals";
                if (accessory)
                {
                    // Accessory copper/iron/steel: 1/2/3 common metals plus a gem.
                    spec.Resource1Cost = metals == 1 ? 1 : metals - 1;
                    spec.Resource2 = "gems";
                    spec.Resource2Cost = 1;
                }
                else
                {
                    spec.Resource1Cost = metals;
                    spec.Resource2 = "none";
                    spec.Resource2Cost = 0;
                }
            }
        }

        // The old engine added the material's stats and value to every crafted item, and
        // only crafted it once the city held the material's minimum storage
        // (ItemWeapon/Armor/AccessoryMaterialLibrary). Runs after the overrides, which
        // set the item's own value.
        // The material tech each item also needed, on top of its own tech.
        internal static readonly Dictionary<string, string> MaterialTechs = new Dictionary<string, string>(StringComparer.Ordinal);

        private static void ApplyOriginalMaterialStats()
        {
            MaterialTechs.Clear();
            foreach (EquipmentSpec spec in Equipment)
            {
                string material = SteelItems.Contains(spec.Id) ? "steel" : IronItems.Contains(spec.Id) ? "iron" : "copper";
                MaterialTechs[spec.Id] = "material_" + material;
                spec.MaterialStats.Clear();
                if (spec.Type == EquipmentType.Amulet || spec.Type == EquipmentType.Ring)
                {
                    spec.MinimumStorage = 15;
                    spec.Value += material == "steel" ? 40 : material == "iron" ? 30 : 10;
                    spec.MaterialStats["critical_chance"] = material == "steel" ? 0.07f : material == "iron" ? 0.06f : 0.03f;
                }
                else if (spec.Type == EquipmentType.Weapon)
                {
                    spec.MinimumStorage = 10;
                    if (material == "steel")
                    {
                        spec.Value += 40;
                        spec.MaterialStats["damage"] = 7f;
                        spec.MaterialStats["speed"] = 1f;
                        spec.MaterialStats["attack_speed"] = 10f;
                        spec.MaterialStats["critical_chance"] = 0.05f;
                    }
                    else if (material == "iron")
                    {
                        spec.Value += 30;
                        spec.MaterialStats["damage"] = 6f;
                        spec.MaterialStats["attack_speed"] = 10f;
                        spec.MaterialStats["critical_chance"] = 0.05f;
                    }
                    else
                    {
                        spec.Value += 10;
                        spec.MaterialStats["damage"] = 3f;
                        spec.MaterialStats["attack_speed"] = 2f;
                    }
                }
                else
                {
                    spec.MinimumStorage = 15;
                    spec.Value += material == "steel" ? 40 : material == "iron" ? 30 : 10;
                    spec.MaterialStats["armor"] = material == "steel" ? 7f : material == "iron" ? 6f : 3f;
                }
            }
        }

        private static void BuildEquipmentSpecifications()
        {
            string[] renaissance = { "PipeRifle", "PipePistol", "PipeShotgun", "shieldedsword", "shieldedaxe", "shieldedhammer", "shieldedspear", "piratpistol", "Musket", "pristinearmor", "pristineboots", "pristinehelmet" };
            string[] industrial = { "wwarmor", "wwboots", "wwhelmet", "m1garand", "Americanshotgun" };
            string[] modern = { "modernarmor", "modernboots", "modernhelmet", "Glock17", "MP7", "HK416", "M16", "DesertEagle", "malorian", "Uzi", "Minigun", "AK47", "AK103", "XM8", "SGT44", "ThompsonM1A1", "M4A1", "FAMAS", "Sniper", "RocketLauncher" };
            string[] future = { "futurearmor", "futureboots", "futurehelmet", "blueheavyblaster", "redheavyblaster", "greenheavyblaster", "blueblastersniper", "redblastersniper", "greenblastersniper", "blueblaster", "redblaster", "greenblaster", "blueminigun", "redminigun", "greenminigun", "blueplasmagun", "redplasmagun", "greenplasmagun", "redlightsaber", "bluelightsaber", "greenlightsaber", "chainsaw" };
            foreach (string id in renaissance) AddEquipment(id, M2Era.Renaissance);
            foreach (string id in industrial) AddEquipment(id, M2Era.Industrial);
            foreach (string id in modern) AddEquipment(id, M2Era.Modern);
            foreach (string id in future) AddEquipment(id, M2Era.Future);
            AddEquipment("Sandevistan", M2Era.Industrial, EquipmentType.Amulet, "CyberWareParts", 2, "Parts", 1);
            AddEquipment("TurboBooster", M2Era.Industrial, EquipmentType.Amulet, "CyberWareParts", 2, "Parts", 1);
            AddEquipment("Meth", M2Era.Industrial, EquipmentType.Ring, "gold", 1, "gold", 0);
            AddEquipment("Crack", M2Era.Industrial, EquipmentType.Ring, "gold", 1, "gold", 0);

            // MIRV and MIRVBomb were never craftable in M2; they are registered as
            // hidden attack items in OriginalM2Projectiles.
            ModernBoxCatalog.GunIds = Equipment.FindAll(candidate => candidate.Type == EquipmentType.Weapon).ConvertAll(candidate => candidate.Id).ToArray();
            ModernBoxCatalog.MirvIds = new[] { "MIRV", "MIRVBomb" };
        }

        // Weapons M2 added to every civilized race's preferred weapons; the pipe guns
        // were added by M2's pipe gun toggle.
        internal static readonly HashSet<string> PreferredWeapons = new HashSet<string>(StringComparer.Ordinal)
        {
            "shieldedsword", "shieldedaxe", "shieldedhammer", "shieldedspear", "piratpistol", "Musket", "m1garand",
            "Americanshotgun", "Glock17", "MP7", "HK416", "M16", "DesertEagle", "malorian", "Uzi", "Minigun", "AK47",
            "AK103", "XM8", "SGT44", "ThompsonM1A1", "M4A1", "FAMAS", "Sniper", "RocketLauncher", "blueheavyblaster",
            "redheavyblaster", "greenheavyblaster", "redblastersniper", "blueblastersniper", "greenblastersniper",
            "blueblaster", "redblaster", "greenblaster", "redminigun", "blueminigun", "greenminigun", "blueplasmagun",
            "redplasmagun", "greenplasmagun", "redlightsaber", "bluelightsaber", "greenlightsaber", "chainsaw",
            "PipeRifle", "PipePistol", "PipeShotgun"
        };

        // Item names from M2's Localization.addLocalization calls. The blasters and
        // plasma guns were labelled "AK-103"/"Desert Eagle" by a copy-paste slip in M2.
        private static readonly Dictionary<string, string> EquipmentNames = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "AK103", "AK-103" }, { "AK47", "AK47" }, { "Americanshotgun", "Americanshotgun" }, { "Crack", "Crack" },
            { "DesertEagle", "Desert Eagle" }, { "FAMAS", "FAMAS" }, { "Glock17", "Glock 17" }, { "HK416", "HK416" },
            { "M16", "M16" }, { "M4A1", "M4A1" }, { "MP7", "MP7" }, { "Meth", "Meth" }, { "Minigun", "Minigun" },
            { "Musket", "Musket" }, { "PipePistol", "Pipe Pistol" }, { "PipeRifle", "Pipe Rifle" },
            { "PipeShotgun", "Pipe Shotgun" }, { "RocketLauncher", "Rocket Propelled Grenade (RPG)" }, { "SGT44", "SGT44" },
            { "Sandevistan", "Military Grade Sandevistan" }, { "Sniper", "Sniper" }, { "ThompsonM1A1", "ThompsonM1A1" },
            { "TurboBooster", "Military Grade TurboBooster" }, { "Uzi", "Uzi" }, { "XM8", "XM8" },
            { "blueblaster", "Blue Blaster" }, { "redblaster", "Red Blaster" }, { "greenblaster", "Green Blaster" },
            { "blueblastersniper", "blueblastersniper" }, { "redblastersniper", "redblastersniper" }, { "greenblastersniper", "greenblastersniper" },
            { "blueheavyblaster", "Heavy Blaster" }, { "redheavyblaster", "Heavy Blaster" }, { "greenheavyblaster", "Heavy Blaster" },
            { "bluelightsaber", "Lightsaber" }, { "redlightsaber", "Lightsaber" }, { "greenlightsaber", "Lightsaber" },
            { "blueminigun", "blueminigun" }, { "redminigun", "redminigun" }, { "greenminigun", "greenminigun" },
            { "blueplasmagun", "Blue Plasma Gun" }, { "redplasmagun", "Red Plasma Gun" }, { "greenplasmagun", "Green Plasma Gun" },
            { "chainsaw", "Chainsaw" }, { "futurearmor", "future armor" }, { "futureboots", "future boots" }, { "futurehelmet", "future helmet" },
            { "m1garand", "m1garand" }, { "malorian", "Malorian Arms 3516" }, { "modernarmor", "modern armor" },
            { "modernboots", "modern boots" }, { "modernhelmet", "modern helmet" }, { "piratpistol", "piratpistol" },
            { "pristinearmor", "pristine armor" }, { "pristineboots", "pristine boots" }, { "pristinehelmet", "pristine helmet" },
            { "shieldedaxe", "shielded axe" }, { "shieldedhammer", "shielded hammer" }, { "shieldedspear", "shielded spear" },
            { "shieldedsword", "shielded sword" }, { "wwarmor", "ww armor" }, { "wwboots", "ww boots" }, { "wwhelmet", "ww helmet" }
        };

        internal static bool IsMeleeEquipment(string id)
        {
            return id.StartsWith("shielded", StringComparison.Ordinal) || id.EndsWith("lightsaber", StringComparison.Ordinal) || id == "chainsaw";
        }

        private static void AddEquipment(string id, M2Era era, EquipmentType type = EquipmentType.Weapon,
            string resource1 = null, int resource1Cost = 0, string resource2 = null, int resource2Cost = 0)
        {
            bool armor = id.Contains("armor") || id.Contains("boots") || id.Contains("helmet");
            if (armor) type = id.Contains("boots") ? EquipmentType.Boots : (id.Contains("helmet") ? EquipmentType.Helmet : EquipmentType.Armor);
            if (resource1 == null)
            {
                if (era == M2Era.Renaissance) { resource1 = "wood"; resource1Cost = 1; resource2 = "common_metals"; resource2Cost = 1; }
                else if (era == M2Era.Industrial) { resource1 = "common_metals"; resource1Cost = 2; resource2 = "gold"; resource2Cost = 2; }
                else if (era == M2Era.Modern) { resource1 = "Parts"; resource1Cost = 2; resource2 = "common_metals"; resource2Cost = 2; }
                else { resource1 = "Parts"; resource1Cost = 2; resource2 = "Xenium"; resource2Cost = 1; }
            }
            string projectile = type == EquipmentType.Weapon && !IsMeleeEquipment(id) ? OriginalM2Projectiles.GetEquipmentProjectile(id) : null;
            string name;
            EquipmentSpec spec = new EquipmentSpec
            {
                Id = id,
                DisplayName = EquipmentNames.TryGetValue(id, out name) ? name : Friendly(id),
                Projectile = projectile,
                Era = era,
                Tier = (ProgressionTier)(int)era,
                Type = type,
                Value = 400 + ((int)era * 400),
                Resource1 = resource1,
                Resource1Cost = resource1Cost,
                Resource2 = resource2,
                Resource2Cost = resource2Cost
            };
            Equipment.Add(spec);
            ModernBoxCatalog.EquipmentIds.Add(id);
            ModernBoxCatalog.EquipmentEras[id] = era;
            ModernBoxCatalog.EquipmentTiers[id] = spec.Tier;
        }

        // Radius, terraform, effect and effect scale from M2's Buttonz action_*Click methods.
        private static void BuildBombSpecifications()
        {
            AddBomb("MOAB", "Super-Nuke", "ui/Icons/MOAB", 50, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f);
            AddBomb("Cobalt", "Cobalt Bomb", "ui/Icons/Cobalt", 120, "czar_bomba", "fx_explosion_huge", 0.2f, 0.3f);
            AddBomb("Ultron", "Ultron Bomb", "ui/Icons/Ultron", 100, "czar_bomba", "fx_explosion_huge", 0.8f, 0.9f);
            AddBomb("Death", "Death Bomb", "ui/Icons/Death", 100, "czar_bomba", "fx_explosion_huge", 1.2f, 1.6f);
            AddBomb("Xenium", "Xenium Bomb", "ui/Icons/Xeno", 400, "czar_bomba", "fx_explosion_huge", 4.3f, 7.9f);
            AddBomb("Mini", "Mini Nuke", "ui/Icons/Mini", 5, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f);
            AddBomb("Proton", "Proton Bomb", "ui/Icons/Proton", 786, "czar_bomba", "fx_dankymatter_effect", 0.5f, 0.5f);
            AddBomb("Jupiter", "Jupiter Bomb", "ui/Icons/Jupiter", 1486, "czar_bomba", "fx_explosion_huge", 32.3f, 56.9f);
            AddBomb("Eraser", "Eraser Bomb", "ui/Icons/Eraser", 1000, "destroy_no_flash", "fx_antimatter_effect", 5.3f, 9.9f);
            AddBomb("Random", "Random Bomb", "ui/Icons/wat", 0, "czar_bomba", "fx_explosion_huge", 0f, 0f, BombPattern.RandomLegacy);
            AddBomb("AtomicGrenade", "Atomic Grenade", "ui/Icons/AtomicGrenade", 130, "czar_bomba", "fx_explosion_small", 4.3f, 7.9f);
            AddBomb("FuryOfTuxia", "Fury of Tuxia", "ui/Icons/FuryOfTuxia", 7860, "czar_bomba", "fx_explosion_huge", 160.3f, 280.9f);
            AddBomb("ZeussRage", "Zeus's Rage", "ui/Icons/ZeusRage", 600, "czar_bomba", "fx_lightning_big", 4.3f, 7.9f);
            AddBomb("NotSoAtomic", "the Not so atomic Bomb", "ui/Icons/NotSoAtomic", 30, "bomb", "fx_fireball_explosion", 5.3f, 7.9f);
            AddBomb("ColorBomb", "Color Bomb", "ui/Icons/ColorGrenade", 100, "bomb", "fx_color_grenade", 5.3f, 7.9f);
            AddBomb("DankyBomb", "Danky Bomb", "ui/Icons/Danky", 50, "czar_bomba", "fx_explosion_dank", 4.3f, 4.9f);
            AddBomb("BloodLightning", "Blood Lightning", "ui/Icons/BloodLightning", 100, "bomb", "fx_blood_lightning", 5.3f, 5.9f);
            AddBomb("NoDamage", "No Damage", "ui/Icons/BlueOne", 30, "nothing", "fx_explosion_blue", 3.3f, 3.9f);
            AddBomb("ClusterNuke", "Cluster Nuke", "ui/Icons/ClusterNuke", 20, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f, BombPattern.ClusterNuke);
            AddBomb("ClusterStrike", "The Cluster Strike", "ui/Icons/ClusterStrike", 30, "czar_bomba", "fx_lightning_medium", 0.4f, 0.6f, BombPattern.ClusterLightning);
            AddBomb("Spreader", "The Spreader Bomb", "ui/Icons/MOAB", 25, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f, BombPattern.Spreader);
        }

        private static void AddBomb(string id, string name, string icon, int radius, string terraform,
            string effect, float scaleMin, float scaleMax, BombPattern pattern = BombPattern.Radial)
        {
            Bombs.Add(new BombSpec
            {
                Id = id,
                DisplayName = name,
                IconPath = icon,
                // M2 used drops/drop_antimatter for the Eraser; 0.51.2 only ships drop_antimatterbomb.
                DropTexture = id == "Eraser" ? "drops/drop_antimatterbomb" : "drops/drop_czarbomba",
                Radius = radius,
                TerraformId = terraform,
                EffectId = effect,
                EffectScaleMin = scaleMin,
                EffectScaleMax = scaleMax,
                Pattern = pattern
            });
        }

        private static void BuildInvasionSpecifications()
        {
            Invasions.Add(new InvasionSpec { Id = "Hashbrown", ActorId = "hashbrowncat", Automatic = false, MinimumPopulation = 500, MinimumCities = 4, MinimumUnits = 1, MaximumUnits = 5, WorldCap = 50 });
            Invasions.Add(new InvasionSpec { Id = "Vatican", ActorId = "basecrusader", Automatic = false, MinimumPopulation = 500, MinimumCities = 0, MinimumUnits = 300, MaximumUnits = 300, WorldCap = 1000 });
        }

        private static string Friendly(string id)
        {
            string text = id.Replace("_", " ");
            return text.Length == 0 ? id : char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        private sealed class EraBuilding
        {
            internal string ArtId;
            internal M2Era Era;
            internal ConstructionCost Cost;
            internal float Health;
            internal int Housing;
        }
    }
}
