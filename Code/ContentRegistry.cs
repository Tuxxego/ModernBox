using System;
using System.Collections.Generic;

namespace ModernBoxM2Rewrite
{
    internal static partial class ContentRegistry
    {
        internal static readonly List<ModernUnitSpec> Units = new List<ModernUnitSpec>();
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
            EquipmentAndTraitsRegistry.RegisterResourcesAndProjectiles();
            EquipmentAndTraitsRegistry.RegisterEquipment();
            EquipmentAndTraitsRegistry.RegisterTraits();
            EquipmentAndTraitsRegistry.RegisterNames();
            ActorsAndBuildingsRegistry.RegisterUnits();
            AlienJungleRegistry.Register();
            ActorsAndBuildingsRegistry.RegisterBuildingsAndOrders();
            BombRegistry.RegisterBombs();
            SiloLaunchEvents.Register();
            ProductionService.ApplyDynamicSettings();
            ModernLocalization.Apply();
            Summary = Units.Count + " units, " + Buildings.Count + " buildings, " +
                Equipment.Count + " equipment, " + Bombs.Count + " bombs";
            ModernBoxDiagnostics.ValidateRegisteredContent();
            ModernBoxDiagnostics.ValidateAssets();
            ModernBoxDiagnostics.Info("Registered " + Summary + ".");
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
            ApplyM2EquipmentOverrides();
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
            AddBuilding("casino", "casino", "", M2Era.Renaissance, Cost(15, 25, 0, 250), int.MaxValue, 0, true, false, false);
            AddBuilding("restaurant", "restaurant", "", M2Era.Renaissance, Cost(15, 25, 0, 250), int.MaxValue, 0, true, false, false);
            AddBuilding("mall", "mall", "", M2Era.Renaissance, Cost(15, 25, 0, 250), int.MaxValue, 0, true, false, false);
            AddBuilding("school", "school", "", M2Era.Renaissance, Cost(15, 25, 0, 250), int.MaxValue, 0, true, false, false);
            AddBuilding("modernbuilding", "modernbuilding", "", M2Era.Renaissance, Cost(0, 2, 1, 1), int.MaxValue, 60, true, false, false);

            AddIndustry("HumveeFactory", M2Era.Industrial);
            AddIndustry("TankFactory", M2Era.Modern);
            AddIndustry("AirshipFactory", M2Era.Industrial);
            AddIndustry("RailgunFactory", M2Era.Future);
            AddIndustry("HelicopterFactory", M2Era.Modern);
            AddIndustry("DroneFactory", M2Era.Modern);
            AddIndustry("FighterJetFactory", M2Era.Modern);
            AddIndustry("BoiFactory", M2Era.Modern);
            AddIndustry("GunshipFactory", M2Era.Modern);
            AddIndustry("AirFactory", M2Era.Modern);
            AddIndustry("TerranFactory", M2Era.Future);
            AddIndustry("P9000Factory", M2Era.Future);
            AddBuilding("MissileSilo", "MissileSilo", "", M2Era.Industrial, Cost(0, 0, 32, 72), 1, 0, false, true, false);

            foreach (string race in ModernBoxCatalog.SupportedRaces)
            {
                AddEraChain(race, "barracks", "barracks_human", true, new[]
                {
                    EraUpgrade("Barracks_rain_human", M2Era.Renaissance, Cost(0, 1, 1, 1)),
                    EraUpgrade("Barracks_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 1)),
                    EraUpgrade("Barracks_modern_human", M2Era.Modern, Cost(0, 1, 1, 1)),
                    EraUpgrade("Barracks_future_human", M2Era.Future, Cost(0, 1, 1, 1))
                });
                AddEraChain(race, "watch_tower", "watch_tower_human", true, new[]
                {
                    EraUpgrade("watch_tower_rain_human", M2Era.Renaissance, Cost(1, 10, 5, 1)),
                    EraUpgrade("watch_tower_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 1)),
                    EraUpgrade("watch_tower_modern_human", M2Era.Modern, Cost(0, 1, 5, 5)),
                    EraUpgrade("watch_tower_future_human", M2Era.Future, Cost(0, 1, 10, 10))
                });
                AddEraChain(race, "mine", "mine_rain_human", false, new[]
                {
                    EraUpgrade("mine_rain_human", M2Era.Renaissance, Cost(0, 0, 0, 1)),
                    EraUpgrade("mine_industrial_human", M2Era.Industrial, Cost(0, 0, 0, 1)),
                    EraUpgrade("mine_modern_human", M2Era.Modern, Cost(0, 0, 0, 1)),
                    EraUpgrade("mine_future_human", M2Era.Future, Cost(0, 0, 0, 1))
                });
                AddEraChain(race, "temple", "temple_human", false, new[]
                {
                    EraUpgrade("temple_rain_human", M2Era.Renaissance, Cost(1, 1, 1, 1)),
                    EraUpgrade("temple_industrial_human", M2Era.Industrial, Cost(1, 1, 1, 1)),
                    EraUpgrade("temple_modern_human", M2Era.Modern, Cost(1, 1, 1, 1)),
                    EraUpgrade("temple_future_human", M2Era.Future, Cost(1, 1, 1, 1))
                });
                AddEraChain(race, "house", "house_industrial_human", false, new[]
                {
                    EraUpgrade("house_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 0)),
                    EraUpgrade("house_modern_human", M2Era.Modern, Cost(0, 1, 1, 1)),
                    EraUpgrade("house_future_human", M2Era.Future, Cost(0, 1, 1, 1))
                });
                AddEraChain(race, "hall", "hall_industrial_human", false, new[]
                {
                    EraUpgrade("hall_industrial_human", M2Era.Industrial, Cost(0, 1, 1, 0)),
                    EraUpgrade("hall_modern_human", M2Era.Modern, Cost(0, 1, 1, 1)),
                    EraUpgrade("hall_future_human", M2Era.Future, Cost(0, 1, 1, 1))
                });
                AddEraChain(race, "dock", "dock_rain_human", false, new[]
                {
                    EraUpgrade("dock_rain_human", M2Era.Renaissance, Cost(0, 0, 0, 1)),
                    EraUpgrade("dock_industrial_human", M2Era.Industrial, Cost(0, 0, 0, 1)),
                    EraUpgrade("dock_modern_human", M2Era.Modern, Cost(0, 0, 0, 1))
                });
            }
        }

        private static void AddEraChain(string race, string chain, string firstArt, bool tower, EraBuilding[] stages)
        {
            string previous = "$native_" + chain + "_" + race + "$";
            foreach (EraBuilding stage in stages)
            {
                string id = RaceAdvancedId(stage.ArtId, race);
                AddBuilding(id, stage.ArtId, race, stage.Era, stage.Cost, int.MaxValue,
                    chain == "house" ? (stage.Era == M2Era.Future ? 80 : 50) : 0,
                    false, tower || chain == "watch_tower", true);
                Buildings[Buildings.Count - 1].Type = chain;
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

        private static void AddIndustry(string id, M2Era era)
        {
            AddBuilding(id, id, "", era, Cost(0, 2, 1, 1), 1, 0, false, false, false);
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

        private static EraBuilding EraUpgrade(string art, M2Era era, ConstructionCost cost)
        {
            return new EraBuilding { ArtId = art, Era = era, Cost = cost };
        }

        private static ConstructionCost Cost(int wood, int stone, int metal, int gold)
        {
            return new ConstructionCost(wood, stone, metal, gold);
        }

        private static void BuildFactorySpecifications()
        {
            AddFactory("$era_barracks$", Array.Empty<string>(), M2Era.Renaissance, 30f, "SoldierOption");
            AddFactory("HumveeFactory", new[] { "Humvee" }, M2Era.Industrial, 45f, "HumveeOption");
            AddFactory("TankFactory", new[] { "Tank" }, M2Era.Modern, 60f, "TankOption");
            AddFactory("AirshipFactory", new[] { "Zeppelin" }, M2Era.Industrial, 75f, "AirshipOption");
            AddFactory("RailgunFactory", new[] { "Railgun" }, M2Era.Future, 60f, "RailgunOption");
            AddFactory("HelicopterFactory", new[] { "Heli" }, M2Era.Modern, 75f, "HeliOption");
            AddFactory("DroneFactory", new[] { "Drone" }, M2Era.Modern, 75f, "DronesOption");
            AddFactory("FighterJetFactory", new[] { "FighterJet" }, M2Era.Modern, 90f, "FighterJetOption");
            AddFactory("BoiFactory", new[] { "MissileSystem" }, M2Era.Modern, 60f, "BoiOption");
            AddFactory("GunshipFactory", new[] { "Gunship" }, M2Era.Modern, 90f, "GunshipOption");
            AddFactory("AirFactory", new[] { "MIRVBomber" }, M2Era.Modern, 120f, "MIRVBomberOption");
            AddFactory("TerranFactory", new[] { "Terran" }, M2Era.Future, 90f, "TerranOption");
            AddFactory("P9000Factory", new[] { "P9000" }, M2Era.Future, 90f, "P9000Option");
        }

        private static void AddFactory(string building, string[] units, M2Era era, float cooldown, string setting)
        {
            Factories.Add(new FactorySpec
            {
                BuildingId = building,
                UnitIds = units,
                Era = era,
                Tier = (ProgressionTier)(int)era,
                Interval = cooldown,
                SettingKey = setting,
                UnitCost = Cost(0, 0, 0, 0)
            });
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
            AddEquipment("MIRV", M2Era.Modern, EquipmentType.Weapon, "Parts", 3, "Xenium", 2, "MIRVartillery", 993, 36, 2, 0.7f);
            AddEquipment("MIRVBomb", M2Era.Modern, EquipmentType.Weapon, "Parts", 5, "Xenium", 4, "bigbomb", 993, 40, 2, 0.65f);

            ModernBoxCatalog.GunIds = Equipment.FindAll(candidate => candidate.Type == EquipmentType.Weapon).ConvertAll(candidate => candidate.Id).ToArray();
            ModernBoxCatalog.MirvIds = new[] { "MIRV", "MIRVBomb" };
        }

        private static void AddEquipment(string id, M2Era era, EquipmentType type = EquipmentType.Weapon,
            string resource1 = null, int resource1Cost = 0, string resource2 = null, int resource2Cost = 0,
            string projectile = null, float damage = 0, float range = 0, float speed = 0, float accuracy = 0)
        {
            bool melee = id.Contains("sword") || id.Contains("axe") || id.Contains("hammer") || id.Contains("spear") || id.Contains("saber") || id == "chainsaw";
            bool armor = id.Contains("armor") || id.Contains("boots") || id.Contains("helmet");
            if (armor) type = id.Contains("boots") ? EquipmentType.Boots : (id.Contains("helmet") ? EquipmentType.Helmet : EquipmentType.Armor);
            if (resource1 == null)
            {
                if (era == M2Era.Renaissance) { resource1 = "wood"; resource1Cost = 1; resource2 = "common_metals"; resource2Cost = 1; }
                else if (era == M2Era.Industrial) { resource1 = "common_metals"; resource1Cost = 2; resource2 = "gold"; resource2Cost = 2; }
                else if (era == M2Era.Modern) { resource1 = "Parts"; resource1Cost = 2; resource2 = "common_metals"; resource2Cost = 2; }
                else { resource1 = "Parts"; resource1Cost = 2; resource2 = "Xenium"; resource2Cost = 1; }
            }
            if (type == EquipmentType.Weapon && !melee && projectile == null) projectile = OriginalM2Projectiles.GetEquipmentProjectile(id);
            if (damage <= 0) damage = melee ? (era == M2Era.Future ? 140 : 45) : (era == M2Era.Future ? 90 : era == M2Era.Modern ? 50 : era == M2Era.Industrial ? 36 : 24);
            if (range <= 0) range = melee ? 1.5f : (era == M2Era.Future ? 22 : 16);
            if (speed <= 0) speed = melee ? 2.5f : 8f;
            if (accuracy <= 0) accuracy = melee ? 1f : 0.72f;
            EquipmentSpec spec = new EquipmentSpec
            {
                Id = id,
                DisplayName = Friendly(id),
                Projectile = projectile,
                Era = era,
                Tier = (ProgressionTier)(int)era,
                Type = type,
                Damage = damage,
                Range = range,
                AttackSpeed = speed,
                Accuracy = accuracy,
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

        private static void BuildBombSpecifications()
        {
            AddBomb("MOAB", "Super-Nuke", "ui/Icons/MOAB", 50);
            AddBomb("Cobalt", "Cobalt Bomb", "ui/Icons/Cobalt", 120);
            AddBomb("Ultron", "Ultron Bomb", "ui/Icons/Ultron", 100);
            AddBomb("Death", "Death Bomb", "ui/Icons/Death", 100);
            AddBomb("Xenium", "Xenium Bomb", "ui/Icons/Xeno", 400);
            AddBomb("Mini", "Mini Nuke", "ui/Icons/Mini", 5);
            AddBomb("Proton", "Proton Bomb", "ui/Icons/Proton", 786);
            AddBomb("Jupiter", "Jupiter Bomb", "ui/Icons/Jupiter", 1486);
            AddBomb("Eraser", "Eraser Bomb", "ui/Icons/Eraser", 1000, BombPattern.Radial, "destroy_no_flash");
            AddBomb("Random", "Random Bomb", "ui/Icons/wat", 0, BombPattern.RandomLegacy);
            AddBomb("AtomicGrenade", "Atomic Grenade", "ui/Icons/AtomicGrenade", 130);
            AddBomb("FuryOfTuxia", "Fury of Tuxia", "ui/Icons/FuryOfTuxia", 7860);
            AddBomb("ZeussRage", "Zeus's Rage", "ui/Icons/ZeusRage", 600);
            AddBomb("NotSoAtomic", "Not So Atomic", "ui/Icons/NotSoAtomic", 30);
            AddBomb("ColorBomb", "Color Bomb", "ui/Icons/ColorGrenade", 100);
            AddBomb("DankyBomb", "Danky Bomb", "ui/Icons/Danky", 50);
            AddBomb("BloodLightning", "Blood Lightning", "ui/Icons/BloodLightning", 100);
            AddBomb("NoDamage", "No Damage", "ui/Icons/BlueOne", 30, BombPattern.VisualOnly);
            AddBomb("ClusterNuke", "Cluster Nuke", "ui/Icons/ClusterNuke", 20, BombPattern.ClusterNuke);
            AddBomb("ClusterStrike", "Cluster Strike", "ui/Icons/ClusterStrike", 30, BombPattern.ClusterLightning);
            AddBomb("Spreader", "Spreader", "ui/Icons/ClusterNuke", 25, BombPattern.Spreader);
        }

        private static void AddBomb(string id, string name, string icon, int radius,
            BombPattern pattern = BombPattern.Radial, string terraform = "czar_bomba")
        {
            BombSpec spec = new BombSpec
            {
                Id = id,
                DisplayName = name,
                IconPath = icon,
                DropTexture = terraform == "destroy_no_flash" ? "drops/drop_antimatterbomb" : "drops/drop_czarbomba",
                Radius = radius,
                TerraformId = terraform,
                EffectId = terraform == "destroy_no_flash" ? "fx_antimatter_effect" : "fx_explosion_huge",
                EffectScaleMin = Math.Max(0.2f, radius / 80f),
                EffectScaleMax = Math.Max(0.4f, radius / 45f),
                Pattern = pattern
            };
            if (id == "AtomicGrenade") { spec.EffectId = "fx_explosion_small"; spec.EffectScaleMin = 4.3f; spec.EffectScaleMax = 7.9f; }
            else if (id == "FuryOfTuxia") { spec.EffectScaleMin = 160.3f; spec.EffectScaleMax = 280.9f; }
            else if (id == "ZeussRage") { spec.EffectId = "fx_lightning_big"; spec.EffectScaleMin = 4.3f; spec.EffectScaleMax = 7.9f; }
            else if (id == "Proton") { spec.EffectId = "fx_dankymatter_effect"; spec.EffectScaleMin = 0.5f; spec.EffectScaleMax = 0.5f; }
            else if (id == "NotSoAtomic") { spec.TerraformId = "bomb"; spec.EffectId = "fx_fireball_explosion"; spec.EffectScaleMin = 5.3f; spec.EffectScaleMax = 7.9f; }
            else if (id == "ColorBomb") { spec.TerraformId = "bomb"; spec.EffectId = "fx_color_grenade"; spec.EffectScaleMin = 5.3f; spec.EffectScaleMax = 7.9f; }
            else if (id == "DankyBomb") { spec.EffectId = "fx_explosion_dank"; spec.EffectScaleMin = 4.3f; spec.EffectScaleMax = 4.9f; }
            else if (id == "BloodLightning") { spec.TerraformId = "bomb"; spec.EffectId = "fx_blood_lightning"; spec.EffectScaleMin = 5.3f; spec.EffectScaleMax = 5.9f; }
            else if (id == "NoDamage") { spec.TerraformId = "nothing"; spec.EffectId = "fx_explosion_blue"; spec.EffectScaleMin = 3.3f; spec.EffectScaleMax = 3.9f; }
            else if (id == "ClusterNuke") { spec.EffectScaleMin = 0.4f; spec.EffectScaleMax = 0.6f; }
            else if (id == "ClusterStrike") { spec.EffectId = "fx_lightning_medium"; spec.EffectScaleMin = 0.4f; spec.EffectScaleMax = 0.6f; }
            else if (id == "Spreader") { spec.EffectScaleMin = 0.4f; spec.EffectScaleMax = 0.6f; }
            Bombs.Add(spec);
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
        }
    }
}
