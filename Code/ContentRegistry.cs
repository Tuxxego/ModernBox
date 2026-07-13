using System;
using System.Collections.Generic;
using NCMS.Utils;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class ContentRegistry
    {
        internal static readonly List<ModernUnitSpec> Units = new List<ModernUnitSpec>();
        internal static readonly List<BuildingSpec> Buildings = new List<BuildingSpec>();
        internal static readonly List<FactorySpec> Factories = new List<FactorySpec>();
        internal static readonly List<EquipmentSpec> Equipment = new List<EquipmentSpec>();
        internal static readonly List<BombSpec> Bombs = new List<BombSpec>();
        internal static string Summary { get; private set; }

        internal static void RegisterAll()
        {
            BuildSpecifications();
            EquipmentAndTraitsRegistry.RegisterResourcesAndProjectiles();
            EquipmentAndTraitsRegistry.RegisterEquipment();
            EquipmentAndTraitsRegistry.RegisterTraits();
            EquipmentAndTraitsRegistry.RegisterNames();
            ActorsAndBuildingsRegistry.RegisterUnits();
            ActorsAndBuildingsRegistry.RegisterBuildingsAndOrders();
            BombRegistry.RegisterBombs();
            SiloLaunchEvents.Register();
            ProductionService.ApplyDynamicSettings();
            ModernBoxDiagnostics.ValidateRegisteredContent();
            ModernLocalization.Apply();
            Summary = Units.Count + " units, " + Buildings.Count + " buildings, " + Equipment.Count + " equipment, " + Bombs.Count + " bombs";
            ModernBoxDiagnostics.ValidateAssets();
            ModernBoxDiagnostics.Info("Registered " + Summary + ".");
        }

        private static void BuildSpecifications()
        {
            Units.Clear(); Buildings.Clear(); Factories.Clear(); Equipment.Clear(); Bombs.Clear();

            AddUnit("Soldier", "Mp5", "Modern_Names", "actors/Soldier/walk_0", 100, 40, 100, 40, 80, 25, 0.1f, false);
            AddUnit("Tank", "RocketLauncher", "Jet_Names", "ui/Icons/Tank", 200, 100, 100, 40, 20, 150, 0.25f, false, "Vehicle", "Tank");
            AddUnit("MissileSystem", EquipmentAndTraitsRegistry.MissileSystemAttackId, "MIRV_Names", "actors/MissileSystem/swim_1", 200, 100, 100, 40, 15, 150, 0.3f, false, "Vehicle");
            AddUnit("Railgun", "RocketLauncher", "Jet_Names", "ui/Icons/Railgun", 600, 80, 100, 60, 12, 150, 0.25f, false, "Vehicle", "Railgun");
            AddUnit("Humvee", "Minigun", "Humvee_Names", "ui/Icons/Humvee", 200, 100, 100, 40, 25, 150, 0.2f, false, "Vehicle", "Humvee");
            AddUnit("Heli", "Mp5", "Jet_Names", "ui/Icons/Heli", 200, 200, 35, 93, 100, 25, 0.2f, true, "Vehicle", "Helicopter");
            AddUnit("Gunship", "GunshipCannon", "Jet_Names", "ui/Icons/Gunship", 350, 200, 60, 93, 100, 40, 0.2f, true, "Vehicle", "Helicopter");
            AddUnit("Drone", "Mp5", "Jet_Names", "ui/Icons/Drone", 15, 90, 10, 30, 100, 20, 0.15f, true, "Vehicle", "Helicopter");
            AddUnit("Zeppelin", "Mp5", "Jet_Names", "ui/Icons/Airship", 1000, 50, 70, 93, 70, 35, 0.5f, true, "Vehicle", "Zeppelin");
            AddUnit("MIRVBomber", "MIRVBomb", "MIRV_Names", "ui/Icons/MIRVBomber", 1000, 500, 100, 93, 50, 30, 0.4f, true, "Vehicle", "Jet");
            AddUnit("CargoPlane", "MIRV", "Jet_Names", "ui/Icons/Bomber", 5000, 500, 100, 93, 45, 30, 0.5f, true, "Vehicle", "Jet");
            AddUnit("F22", "RocketLauncher", "Jet_Names", "ui/Icons/F22", 600, 800, 80, 93, 40, 60, 0.3f, true, "Vehicle", "Jet");
            AddUnit("F55", "RocketLauncher", "Jet_Names", "ui/Icons/F55", 600, 800, 90, 93, 45, 65, 0.3f, true, "Vehicle", "Jet");

            AddBuilding("casino", ProgressionTier.Urban, new ConstructionCost(15, 25, 0, 250), int.MaxValue, 0, true);
            AddBuilding("restaurant", ProgressionTier.Urban, new ConstructionCost(15, 25, 0, 250), int.MaxValue, 0, true);
            AddBuilding("mall", ProgressionTier.Urban, new ConstructionCost(15, 25, 0, 250), int.MaxValue, 0, true);
            AddBuilding("school", ProgressionTier.Urban, new ConstructionCost(15, 25, 0, 250), int.MaxValue, 0, true);
            AddBuilding("modernbuilding", ProgressionTier.Urban, new ConstructionCost(0, 2, 1, 1), int.MaxValue, 60, true);
            AddBuilding("ModernBarracks", ProgressionTier.Industrial, new ConstructionCost(0, 0, 1, 1), 1, 0, false);
            AddBuilding("HumveeFactory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("TankFactory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("AirshipFactory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("HelicopterFactory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("DroneFactory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("F22Factory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("F55Factory", ProgressionTier.Modern, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("RailgunFactory", ProgressionTier.Advanced, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("BoiFactory", ProgressionTier.Advanced, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("GunshipFactory", ProgressionTier.Advanced, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("AirFactory", ProgressionTier.Strategic, new ConstructionCost(0, 2, 1, 1), 1, 0, false);
            AddBuilding("MissileSilo", ProgressionTier.Nuclear, new ConstructionCost(0, 0, 32, 72), 1, 0, false, true);

            Factories.Add(new FactorySpec { BuildingId = "ModernBarracks", UnitIds = new[] { "Soldier" }, Tier = ProgressionTier.Industrial, Interval = 30f, UnitCost = new ConstructionCost(0, 0, 1, 1), SettingKey = "SoldierOption" });
            Factories.Add(new FactorySpec { BuildingId = "HumveeFactory", UnitIds = new[] { "Humvee" }, Tier = ProgressionTier.Modern, Interval = 45f, UnitCost = new ConstructionCost(1, 0, 1, 1), SettingKey = "HumveeOption" });
            Factories.Add(new FactorySpec { BuildingId = "TankFactory", UnitIds = new[] { "Tank" }, Tier = ProgressionTier.Modern, Interval = 60f, UnitCost = new ConstructionCost(0, 1, 3, 2), SettingKey = "TankOption" });
            Factories.Add(new FactorySpec { BuildingId = "AirshipFactory", UnitIds = new[] { "Zeppelin" }, Tier = ProgressionTier.Modern, Interval = 75f, UnitCost = new ConstructionCost(1, 0, 2, 2), SettingKey = "AirshipOption" });
            Factories.Add(new FactorySpec { BuildingId = "HelicopterFactory", UnitIds = new[] { "Heli" }, Tier = ProgressionTier.Modern, Interval = 75f, UnitCost = new ConstructionCost(1, 0, 2, 2), SettingKey = "HeliOption" });
            Factories.Add(new FactorySpec { BuildingId = "DroneFactory", UnitIds = new[] { "Drone" }, Tier = ProgressionTier.Modern, Interval = 45f, UnitCost = new ConstructionCost(1, 0, 1, 1), SettingKey = "DronesOption" });
            Factories.Add(new FactorySpec { BuildingId = "F22Factory", UnitIds = new[] { "F22" }, Tier = ProgressionTier.Modern, Interval = 90f, UnitCost = new ConstructionCost(0, 1, 4, 4), SettingKey = "F22Option" });
            Factories.Add(new FactorySpec { BuildingId = "F55Factory", UnitIds = new[] { "F55" }, Tier = ProgressionTier.Modern, Interval = 90f, UnitCost = new ConstructionCost(0, 1, 4, 4), SettingKey = "F55Option" });
            Factories.Add(new FactorySpec { BuildingId = "RailgunFactory", UnitIds = new[] { "Railgun" }, Tier = ProgressionTier.Advanced, Interval = 60f, UnitCost = new ConstructionCost(0, 1, 3, 2), SettingKey = "RailgunOption" });
            Factories.Add(new FactorySpec { BuildingId = "GunshipFactory", UnitIds = new[] { "Gunship" }, Tier = ProgressionTier.Advanced, Interval = 90f, UnitCost = new ConstructionCost(0, 1, 4, 4), SettingKey = "GunshipOption" });
            Factories.Add(new FactorySpec { BuildingId = "BoiFactory", UnitIds = new[] { "MissileSystem" }, Tier = ProgressionTier.Advanced, Interval = 60f, UnitCost = new ConstructionCost(0, 1, 3, 2), SettingKey = "BoiOption" });
            Factories.Add(new FactorySpec { BuildingId = "AirFactory", UnitIds = new[] { "MIRVBomber", "CargoPlane" }, Tier = ProgressionTier.Strategic, Interval = 120f, UnitCost = new ConstructionCost(0, 1, 6, 8), SettingKey = "MIRVBomberOption" });

            BuildEquipmentSpecs();
            BuildBombSpecs();
        }

        private static void AddUnit(string id, string attack, string names, string icon, float health, float speed, float armor, float damage, float attackSpeed, float range, float scale, bool flying, params string[] traits)
        {
            Units.Add(new ModernUnitSpec { Id = id, Attack = attack, NameTemplate = names, IconPath = icon, Health = health, Speed = speed, Armor = armor, Damage = damage, AttackSpeed = attackSpeed, Range = range, Scale = scale, Flying = flying, Traits = traits });
        }

        private static void AddBuilding(string id, ProgressionTier tier, ConstructionCost cost, int limit, int housing, bool civilian, bool tower = false)
        {
            Buildings.Add(new BuildingSpec { Id = id, Tier = tier, Cost = cost, Limit = limit, Housing = housing, Civilian = civilian, Tower = tower });
        }

        private static void BuildEquipmentSpecs()
        {
            Dictionary<string, float[]> values = new Dictionary<string, float[]>(StringComparer.Ordinal)
            {
                { "Mp5", new[] { 60f, 14f, 12f, 0.70f } }, { "NorincoCQ", new[] { 60f, 14f, 8f, 0.68f } },
                { "Sniper", new[] { 193f, 20f, 3f, 0.92f } }, { "mm9", new[] { 12f, 6f, 18f, 0.75f } },
                { "RocketLauncher", new[] { 150f, 18f, 2f, 0.72f } }, { "Uzi", new[] { 25f, 8f, 16f, 0.55f } },
                { "Minigun", new[] { 13f, 14f, 28f, 0.62f } }, { "GunshipCannon", new[] { 20f, 35f, 30f, 0.70f } },
                { "AK47", new[] { 40f, 15f, 9f, 0.68f } }, { "AK103", new[] { 62f, 15f, 8f, 0.72f } },
                { "XM8", new[] { 45f, 14f, 10f, 0.76f } }, { "SGT44", new[] { 45f, 14f, 7f, 0.65f } },
                { "ThompsonM1A1", new[] { 32f, 10f, 10f, 0.60f } }, { "M4A1", new[] { 45f, 15f, 10f, 0.78f } },
                { "FAMAS", new[] { 42f, 14f, 12f, 0.74f } }, { "malorian", new[] { 75f, 10f, 5f, 0.82f } },
                { "SCAR", new[] { 50f, 14f, 8f, 0.78f } }, { "PipeRifle", new[] { 22f, 8f, 4f, 0.50f } },
                { "PipePistol", new[] { 13f, 6f, 5f, 0.48f } }, { "PipeShotgun", new[] { 18f, 6f, 3f, 0.30f } },
                { "Musket", new[] { 80f, 12f, 2f, 0.58f } }, { "MP7", new[] { 28f, 9f, 13f, 0.68f } },
                { "HK416", new[] { 48f, 15f, 10f, 0.82f } }, { "M16", new[] { 45f, 15f, 9f, 0.80f } },
                { "DesertEagle", new[] { 50f, 10f, 3f, 0.76f } }, { "Glock17", new[] { 35f, 9f, 5f, 0.82f } }
            };
            foreach (string id in ModernBoxCatalog.GunIds)
            {
                float[] stats = values[id];
                bool pipe = id.StartsWith("Pipe", StringComparison.Ordinal) || id == "Musket";
                string projectile = id == "RocketLauncher" ? "modernbox_blast" : (id == "GunshipCannon" ? "modernbox_gunship_bullet" : "modernbox_bullet");
                Equipment.Add(new EquipmentSpec { Id = id, DisplayName = id, Projectile = projectile, Tier = pipe ? ProgressionTier.Industrial : ProgressionTier.Modern, Type = EquipmentType.Weapon, Damage = stats[0], Range = stats[1], AttackSpeed = stats[2], Accuracy = stats[3], Value = pipe ? 350 : 900, Resource1 = pipe ? "wood" : "Parts", Resource1Cost = pipe ? 1 : 2, Resource2 = "common_metals", Resource2Cost = pipe ? 0 : 1 });
            }
            AddEquipment("BudgetMIRV", "Budget MIRV", "modernbox_mirv_budget", ProgressionTier.Modern, 993, 20, 4, 0.5f, 1225, "Parts", 2, "Xenium", 1);
            AddEquipment("DecentMIRV", "Decent MIRV", "modernbox_mirv_decent", ProgressionTier.Advanced, 993, 28, 3, 0.6f, 1600, "Parts", 3, "Xenium", 2);
            AddEquipment("MIRV", "MIRV", "modernbox_mirv", ProgressionTier.Strategic, 993, 36, 2, 0.7f, 2200, "Parts", 5, "Xenium", 4);
            AddEquipment("MIRVBomb", "MIRV Bomb", "modernbox_mirv_bomb", ProgressionTier.Strategic, 993, 30, 2, 0.65f, 2600, "Parts", 6, "Xenium", 5);
            AddEquipment("STRONGMIRV", "Strong MIRV", "modernbox_mirv_strong", ProgressionTier.Nuclear, 993, 45, 1, 0.8f, 4000, "Parts", 8, "Xenium", 8);
            AddAccessory("Sandevistan", "Sandevistan", ProgressionTier.Advanced, EquipmentType.Amulet, 1800, "CyberWareParts", 4, "Xenium", 1);
            AddAccessory("TurboBooster", "Turbo Booster", ProgressionTier.Advanced, EquipmentType.Amulet, 1500, "CyberWareParts", 3, "Parts", 2);
            AddAccessory("Meth", "Meth", ProgressionTier.Advanced, EquipmentType.Ring, 700, "CyberWareParts", 1, "gold", 1);
            AddAccessory("Crack", "Crack", ProgressionTier.Advanced, EquipmentType.Ring, 700, "CyberWareParts", 1, "gold", 1);
        }

        private static void AddEquipment(string id, string name, string projectile, ProgressionTier tier, float damage, float range, float speed, float accuracy, int value, string resource1, int cost1, string resource2, int cost2)
        {
            Equipment.Add(new EquipmentSpec { Id = id, DisplayName = name, Projectile = projectile, Tier = tier, Type = EquipmentType.Weapon, Damage = damage, Range = range, AttackSpeed = speed, Accuracy = accuracy, Value = value, Resource1 = resource1, Resource1Cost = cost1, Resource2 = resource2, Resource2Cost = cost2 });
        }

        private static void AddAccessory(string id, string name, ProgressionTier tier, EquipmentType type, int value, string resource1, int cost1, string resource2, int cost2)
        {
            Equipment.Add(new EquipmentSpec { Id = id, DisplayName = name, Projectile = null, Tier = tier, Type = type, Damage = 0, Range = 0, AttackSpeed = 0, Accuracy = 0, Value = value, Resource1 = resource1, Resource1Cost = cost1, Resource2 = resource2, Resource2Cost = cost2 });
        }

        private static void BuildBombSpecs()
        {
            Bombs.Add(Bomb("MOAB", "Super-Nuke", "ui/Icons/MOAB", 50, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f));
            Bombs.Add(Bomb("Cobalt", "Cobalt Bomb", "ui/Icons/Cobalt", 120, "czar_bomba", "fx_explosion_huge", 0.2f, 0.3f));
            Bombs.Add(Bomb("Ultron", "Ultron Bomb", "ui/Icons/Ultron", 100, "czar_bomba", "fx_explosion_huge", 0.8f, 0.9f));
            Bombs.Add(Bomb("Death", "Death Bomb", "ui/Icons/Death", 100, "czar_bomba", "fx_explosion_huge", 1.2f, 1.6f));
            Bombs.Add(Bomb("Xenium", "Xenium Bomb", "ui/Icons/Xeno", 400, "czar_bomba", "fx_explosion_huge", 4.3f, 7.9f));
            Bombs.Add(Bomb("Mini", "Mini Nuke", "ui/Icons/Mini", 5, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f));
            Bombs.Add(Bomb("Proton", "Proton Bomb", "ui/Icons/Proton", 786, "czar_bomba", "fx_explosion_huge", 16.3f, 28.9f));
            Bombs.Add(Bomb("Jupiter", "Jupiter Bomb", "ui/Icons/Jupiter", 1486, "czar_bomba", "fx_explosion_huge", 32.3f, 56.9f));
            Bombs.Add(Bomb("Eraser", "Eraser Bomb", "ui/Icons/Eraser", 1000, "destroy_no_flash", "fx_antimatter_effect", 5.3f, 9.9f));
            Bombs.Add(Bomb("Random", "Random Bomb", "ui/Icons/wat", 0, "czar_bomba", "fx_explosion_huge", 0.2f, 28.9f));
        }

        private static BombSpec Bomb(string id, string name, string icon, int radius, string terraform, string effect, float minScale, float maxScale)
        {
            string dropTexture = terraform == "destroy_no_flash" ? "drops/drop_antimatterbomb" : "drops/drop_czarbomba";
            return new BombSpec { Id = id, DisplayName = name, IconPath = icon, DropTexture = dropTexture, Radius = radius, TerraformId = terraform, EffectId = effect, EffectScaleMin = minScale, EffectScaleMax = maxScale };
        }
    }
}
