using System;
using System.Collections.Generic;

namespace ModernBoxM1Rewrite
{
    // Every unit, building, item and bomb of the original M1 10.0, with its values.
    internal static class ContentRegistry
    {
        internal static readonly List<ModernUnitSpec> Units = new List<ModernUnitSpec>();
        internal static readonly List<BuildingSpec> Buildings = new List<BuildingSpec>();
        internal static readonly List<EquipmentSpec> Equipment = new List<EquipmentSpec>();
        internal static readonly List<BombSpec> Bombs = new List<BombSpec>();
        private static readonly Dictionary<string, ModernUnitSpec> UnitsById = new Dictionary<string, ModernUnitSpec>(StringComparer.Ordinal);
        private static readonly Dictionary<string, BuildingSpec> BuildingsById = new Dictionary<string, BuildingSpec>(StringComparer.Ordinal);
        private static readonly Dictionary<string, EquipmentSpec> EquipmentById = new Dictionary<string, EquipmentSpec>(StringComparer.Ordinal);
        internal static string Summary { get; private set; }

        internal static ModernUnitSpec FindUnit(string id)
        {
            ModernUnitSpec spec;
            return id != null && UnitsById.TryGetValue(id, out spec) ? spec : null;
        }

        internal static BuildingSpec FindBuilding(string id)
        {
            BuildingSpec spec;
            return id != null && BuildingsById.TryGetValue(id, out spec) ? spec : null;
        }

        internal static EquipmentSpec FindEquipment(string id)
        {
            EquipmentSpec spec;
            return id != null && EquipmentById.TryGetValue(id, out spec) ? spec : null;
        }

        internal static void RegisterAll()
        {
            BuildSpecifications();
            M1OldVanillaStats.SnapshotGameTraits();
            M1AttackSpeed.AllowOriginalSlowest();
            // Same order as Main.Awake: names and guns first, the kingdoms and units after.
            EquipmentAndTraitsRegistry.RegisterNames();
            EquipmentAndTraitsRegistry.RegisterResourcesAndProjectiles();
            EquipmentAndTraitsRegistry.RegisterEquipment();
            EquipmentAndTraitsRegistry.RegisterTraits();
            ActorsAndBuildingsRegistry.RegisterUnits();
            ActorsAndBuildingsRegistry.RegisterBuildingsAndOrders();
            Safe("BombRegistry.RegisterBombs", BombRegistry.RegisterBombs);
            M1Toggles.ApplyStartup();
            ModernLocalization.Apply();
            Summary = Units.Count + " units, " + Buildings.Count + " buildings, " + Equipment.Count + " items, " + Bombs.Count + " bombs";
            Safe("ModernBoxDiagnostics.ValidateAssets", ModernBoxDiagnostics.ValidateAssets);
            Safe("M1HeldSpriteGuard.Apply", M1HeldSpriteGuard.Apply);
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
            Equipment.Clear();
            Bombs.Clear();
            UnitsById.Clear();
            BuildingsById.Clear();
            EquipmentById.Clear();
            ModernBoxCatalog.UnitIds.Clear();
            ModernBoxCatalog.EquipmentIds.Clear();
            BuildUnits();
            BuildBuildings();
            BuildGuns();
            BuildMirvs();
            BuildAccessories();
            BuildBombs();
        }

        // Aircraft.cs: walk_0 then walk_1,walk_2 47 times, for walking and idling.
        private static string[] AircraftFrames()
        {
            string[] frames = new string[95];
            frames[0] = "walk_0";
            for (int i = 1; i < frames.Length; i++) frames[i] = i % 2 == 1 ? "walk_1" : "walk_2";
            return frames;
        }

        private static void BuildUnits()
        {
            string[] aircraft = AircraftFrames();
            string[] aircraftSwim = { "walk_1", "walk_2" };
            // The Gunship's frame string is missing commas after the 8th frame. The old
            // loader stopped at the first frame it couldn't find, so only these 7 played.
            string[] gunship = { "walk_0", "walk_1", "walk_2", "walk_3", "walk_0", "walk_1", "walk_2" };
            string[] four = { "walk_0", "walk_1", "walk_2", "walk_3" };
            string[] first = { "walk_0" };

            // Aircraft.cs
            AddUnit(new ModernUnitSpec { Id = "Heli", Health = 200f, Speed = 200f, Damage = 93f, AttackSpeed = 100f, Accuracy = 100f, Scale = 0.2f, Attack = "Mp5", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "Helicopter", "immortal", "Vehicle" } });
            AddUnit(new ModernUnitSpec { Id = "Gunship", Health = 200f, Speed = 200f, Damage = 93f, AttackSpeed = 100f, Accuracy = 100f, Scale = 0.2f, Attack = "GunshipCannon", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, Walk = gunship, Idle = gunship, Swim = aircraftSwim, Traits = new[] { "immortal", "Vehicle" } });
            // The Drone never set its scale or block flags, so it kept the _mob defaults.
            AddUnit(new ModernUnitSpec { Id = "Drone", Health = 15f, Speed = 30f, Damage = 93f, AttackSpeed = 100f, Accuracy = 100f, Attack = "Mp5", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, DieOnBlocks = true, MoveFromBlock = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "immortal", "Vehicle" } });
            AddUnit(new ModernUnitSpec { Id = "Zeppelin", Health = 1000f, Speed = 50f, Damage = 93f, AttackSpeed = 100f, Accuracy = 100f, Scale = 0.5f, Attack = "Mp5", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "Zeppelin", "immortal", "Vehicle" } });
            AddUnit(new ModernUnitSpec { Id = "MIRVBomber", Health = 1000f, Speed = 500f, Damage = 93f, AttackSpeed = 100f, Accuracy = 100f, Range = 10f, Scale = 0.4f, Attack = "MIRVBomb", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, JetJob = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "Jet", "immortal", "Vehicle" } });
            AddUnit(new ModernUnitSpec { Id = "CargoPlane", Health = 5000f, Speed = 500f, Damage = 93f, AttackSpeed = 100f, Accuracy = 100f, Scale = 0.5f, Attack = "MIRV", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, JetJob = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "Jet", "immortal", "Vehicle" } });
            AddUnit(new ModernUnitSpec { Id = "F22", Health = 600f, Speed = 800f, Damage = 93f, AttackSpeed = 0f, Accuracy = 0f, Scale = 0.3f, Attack = "RocketLauncher", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, JetJob = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "immortal", "Vehicle" } });
            AddUnit(new ModernUnitSpec { Id = "F55", Health = 600f, Speed = 800f, Damage = 93f, AttackSpeed = 0f, Accuracy = 0f, Scale = 0.3f, Attack = "RocketLauncher", NameTemplate = "Jet_Names", KnockbackReduction = 300f, Flying = true, IgnoreBlocks = true, JetJob = true, Walk = aircraft, Idle = aircraft, Swim = aircraftSwim, Traits = new[] { "immortal", "Vehicle" } });

            // LandVehicles.cs. Tank and MissileSystem never set an idle animation (walk_0 by default).
            AddUnit(new ModernUnitSpec { Id = "Tank", Health = 200f, Speed = 100f, Damage = 40f, AttackSpeed = 0f, Range = 150f, Armor = 100f, Scale = 0.25f, Attack = "RocketLauncher", NameTemplate = "Jet_Names", KnockbackReduction = 300f, DieOnBlocks = true, MoveFromBlock = true, Walk = four, Idle = first, Swim = four, Traits = new[] { "Vehicle", "Tank", "immortal" } });
            AddUnit(new ModernUnitSpec { Id = "MissileSystem", Health = 200f, Speed = 100f, Damage = 40f, AttackSpeed = 0f, Range = 150f, Armor = 100f, Scale = 0.3f, Attack = "MIRV", NameTemplate = "Jet_Names", KnockbackReduction = 300f, IgnoreBlocks = true, MoveFromBlock = true, Walk = four, Idle = first, Swim = four, Traits = new[] { "Vehicle", "immortal" } });
            AddUnit(new ModernUnitSpec { Id = "Railgun", Health = 600f, Speed = 80f, Damage = 60f, AttackSpeed = 0f, Range = 150f, Armor = 100f, Scale = 0.25f, Attack = "RocketLauncher", NameTemplate = "Jet_Names", KnockbackReduction = 320f, DieOnBlocks = true, MoveFromBlock = true, Walk = four, Idle = first, Swim = four, Traits = new[] { "Vehicle", "Railgun", "immortal" } });
            AddUnit(new ModernUnitSpec { Id = "Humvee", Health = 200f, Speed = 100f, Damage = 40f, AttackSpeed = 0f, Range = 150f, Armor = 100f, Scale = 0.2f, Attack = "Minigun", NameTemplate = "Humvee_Names", KnockbackReduction = 300f, MoveFromBlock = true, Walk = four, Idle = first, Swim = four, Traits = new[] { "Humvee", "Vehicle", "immortal" } });

            // ModernMilitary.cs: a soldier that uses items and starts with an XM8 or Mp5.
            AddUnit(new ModernUnitSpec { Id = "Soldier", Health = 100f, Speed = 40f, Damage = 40f, AttackSpeed = 80f, Range = 25f, Armor = 100f, Scale = 0.1f, Warfare = 20f, Attack = "base", NameTemplate = "Modern_Names", Humanoid = true, Equipment = true, MoveFromBlock = true, DefaultWeapons = new[] { "XM8", "Mp5" }, Walk = four, Idle = first, Swim = four });
        }

        private static void AddUnit(ModernUnitSpec spec)
        {
            spec.IconPath = "actors/" + spec.Id + "/walk_0";
            Units.Add(spec);
            UnitsById[spec.Id] = spec;
            ModernBoxCatalog.UnitIds.Add(spec.Id);
        }

        private static ConstructionCost Cost(int wood, int stone, int metals, int gold)
        {
            return new ConstructionCost(wood, stone, metals, gold);
        }

        // Commerce.cs
        private static void BuildBuildings()
        {
            foreach (string id in new[] { "casino", "restaurant", "mall", "school" })
                AddBuilding(new BuildingSpec { Id = id, Tech = "Casino", Cost = Cost(15, 25, 0, 250), Burnable = false, PlaceSingle = true, LivingHouse = false });
            AddBuilding(new BuildingSpec { Id = "modernbuilding", Tech = "Casino", Cost = Cost(0, 2, 1, 1), Housing = 60 });

            AddFactory("AirFactory", "MIRVBomber", "MIRVBomber", "MIRVBomberOption");
            // Commerce.cs:168 adds order_RailgunFactory where the tank factory's order should be.
            AddFactory("TankFactory", "Tanks", "Tank", "TankOption").HasOrder = false;
            // "Railguns" and "Boi" were never made as techs, so cities can never build these two.
            AddFactory("RailgunFactory", "Railguns", "Railgun", "RailgunOption");
            AddFactory("HumveeFactory", "Tanks", "Humvee", "HumveeOption");
            AddFactory("HelicopterFactory", "Helicopter", "Heli", "HeliOption");
            // The drone toggle always set spawnUnits to false, so drones are never made.
            AddFactory("DroneFactory", "Drone", "Drone", null);
            AddFactory("AirshipFactory", "Casino", "Zeppelin", "AirshipOption").HasOrder = false;
            AddFactory("F22Factory", "FighterJet", "F22", "F22Option");
            // No button ever turned the F55 factory on.
            AddFactory("F55Factory", "F55", "F55", null);
            AddFactory("BoiFactory", "Boi", "MissileSystem", "BoiOption");
            AddFactory("GunshipFactory", "Gunship", "Gunship", "GunshipOption");
            BuildingSpec barracks = AddFactory("ModernBarracks", "MilitaryModern", "Soldier", "SoldierOption");
            barracks.Cost = Cost(0, 0, 1, 1);
            barracks.Priority = 89893289;
            barracks.SpawnsByDefault = true;

            // Only buildable after the nuke toggle has been switched on.
            AddBuilding(new BuildingSpec
            {
                Id = M1Silo.SiloId, Tech = "Nukes", Cost = Cost(0, 0, 32, 72), Health = 2500f, Priority = 3750,
                Burnable = false, PlaceSingle = true, Tower = true, HasOrder = false
            });
        }

        private static BuildingSpec AddFactory(string id, string tech, string unit, string setting)
        {
            return AddBuilding(new BuildingSpec { Id = id, Tech = tech, Cost = Cost(0, 2, 1, 1), Housing = 60, SpawnUnit = unit, SpawnSetting = setting });
        }

        private static BuildingSpec AddBuilding(BuildingSpec spec)
        {
            Buildings.Add(spec);
            BuildingsById[spec.Id] = spec;
            return spec;
        }

        // guns.cs: every gun is iron and fires the old shotgun bullet, except the RPG (fireball)
        // and the gunship cannon. XM8, FAMAS, SCAR, the pipe guns and the musket never set a
        // quality and use bow names.
        private static void BuildGuns()
        {
            Gun("Mp5", "Mp5", 60f, 14f, 12f, 5f, 10f, 500, true, "Firearms");
            Gun("NorincoCQ", "Norinco CQ", 60f, 14f, -16f, 5f, 10f, 500, true, "Firearms");
            Gun("Sniper", "Sniper", 193f, 20f, 12f, 5f, 10f, 500, true, "Firearms");
            Gun("mm9", "9mm", 3f, 6f, 70f, 400f, 10f, 500, true, "Firearms");
            Gun("RocketLauncher", "Rocket Propelled Grenade (RPG)", 993f, 14f, -101f, 400f, 10f, 1000, true, "Firearms").Projectile = M1Projectiles.LegacyFireballId;
            Gun("Uzi", "Uzi", 3f, 6f, 70f, 400f, 10f, 500, true, "Firearms");
            Gun("Minigun", "Minigun", 13f, 14f, 200f, 3f, 10f, 500, true, "Firearms");
            Gun("GunshipCannon", "GunshipCannon", 13f, 0f, 200f, 400f, 10f, 500, true, "Firearms").Projectile = "GunshipBullet";
            Gun("AK47", "AK47", 13f, 14f, 170f, 3f, 10f, 500, true, "Firearms");
            Gun("AK103", "AK-103", 13f, 14f, 170f, 400f, 10f, 500, true, "Firearms");
            Gun("XM8", "XM8", 13f, 14f, 180f, 400f, 0f, 500, false, "Firearms", "bow_name");
            Gun("SGT44", "SGT44", 11f, 14f, 70f, 400f, 10f, 500, true, "Firearms");
            Gun("ThompsonM1A1", "ThompsonM1A1", 13f, 14f, 170f, 400f, 10f, 500, true, "Firearms");
            Gun("M4A1", "M4A1", 15f, 14f, 170f, 400f, 10f, 500, true, "Firearms");
            Gun("FAMAS", "FAMAS", 13f, 14f, 180f, 400f, 0f, 500, false, "Firearms", "bow_name");
            Gun("malorian", "Malorian Arms 3516", 3f, 6f, 70f, 400f, 10f, 500, true, "Firearms");
            Gun("SCAR", "SCAR", 13f, 6f, 180f, 400f, 0f, 500, false, "Firearms", "bow_name");
            Gun("PipeRifle", "Pipe Rifle", 13f, 6f, 180f, 70f, 0f, 100, false, "LowFirearms", "bow_name");
            Gun("PipePistol", "Pipe Pistol", 13f, 6f, 20f, 70f, 0f, 100, false, "LowFirearms", "bow_name");
            Gun("PipeShotgun", "Pipe Shotgun", 130f, 6f, 20f, 70f, 0f, 100, false, "LowFirearms", "bow_name");
            Gun("Musket", "Musket", 130f, 0f, 20f, 70f, 0f, 100, false, "LowFirearms", "bow_name");
            Gun("MP7", "MP7", 60f, 0f, 12f, 5f, 10f, 500, true, "Firearms");
            Gun("HK416", "HK416", 60f, 0f, 12f, 5f, 10f, 500, true, "Firearms");
            Gun("M16", "M16", 60f, 0f, 12f, 5f, 10f, 500, true, "Firearms");
            Gun("DesertEagle", "Desert Eagle", 40f, 20f, 1f, 90f, 10f, 500, true, "Firearms");
            Gun("Glock17", "Glock 17", 35f, 15f, 1f, 95f, 10f, 500, true, "Firearms");
        }

        private static EquipmentSpec Gun(string id, string name, float damage, float range, float attackSpeed, float accuracy,
            float health, int value, bool legendary, string tech, string names = "sword_name")
        {
            EquipmentSpec spec = new EquipmentSpec
            {
                Id = id, DisplayName = name, Tech = tech, Value = value, Legendary = legendary, NameTemplate = names,
                Projectile = M1Projectiles.LegacyBulletId
            };
            // _range gave every gun 1 projectile and 0.6 damage range.
            spec.BaseStats["projectiles"] = 1f;
            spec.BaseStats["damage_range"] = 0.6f;
            spec.BaseStats["damage"] = damage;
            spec.BaseStats["range"] = range;
            spec.BaseStats["attack_speed"] = attackSpeed;
            spec.BaseStats["accuracy"] = accuracy;
            spec.BaseStats["health"] = health;
            return AddEquipment(spec);
        }

        // MIRV.cs: bow copies that shoot the vanilla fireball. MIRVBomb and STRONGMIRV asked
        // for name generators that were never made, so they use MIRV_Names here. Their Xenium cost was set on the
        // item, which the old game never charged (it charged the iron material instead).
        private static void BuildMirvs()
        {
            Mirv("BudgetMIRV", "Short Range MIRV", 200f, 40f, "BudgetMIRV");
            Mirv("DecentMIRV", "Decent Range MIRV", 500f, 70f, "DecentMIRV");
            Mirv("MIRV", "Long Range Ballistic MIRV", 50000f, 101f, "MIRV");
            Mirv("MIRVBomb", "MIRV Bomber Dropper", 0f, 101f, "MIRV");
            Mirv("STRONGMIRV", "Nuclear Powered MIRV", 50000f, 1001f, "STRONGMIRV");
        }

        private static void Mirv(string id, string name, float range, float attackSpeed, string tech)
        {
            EquipmentSpec spec = new EquipmentSpec
            {
                Id = id, DisplayName = name, Tech = tech, Value = 1000, Legendary = true,
                NameTemplate = "MIRV_Names", Projectile = M1Projectiles.LegacyFireballId
            };
            // From the bow and _range they were cloned from.
            spec.BaseStats["projectiles"] = 1f;
            spec.BaseStats["damage_range"] = 0.6f;
            spec.BaseStats["critical_chance"] = 0.1f;
            spec.BaseStats["critical_damage_multiplier"] = 0.5f;
            spec.BaseStats["range"] = range;
            spec.BaseStats["accuracy"] = 0f;
            spec.BaseStats["attack_speed"] = attackSpeed;
            spec.BaseStats["damage"] = 993f;
            spec.BaseStats["health"] = 10f;
            AddEquipment(spec);
        }

        // cyberware.cs and Drugs.cs: steel rings, tech Cyberware, value 1500.
        private static void BuildAccessories()
        {
            EquipmentSpec sandevistan = Accessory("Sandevistan", "Military Grade Sandevistan");
            CyberwareStats(sandevistan);
            sandevistan.BaseStats["speed"] = 500f;

            EquipmentSpec booster = Accessory("TurboBooster", "Military Grade TurboBooster");
            CyberwareStats(booster);
            booster.BaseStats["health"] = 1000f;
            booster.BaseStats["armor"] = 100f;
            booster.BaseStats["knockback_reduction"] = 200f;

            EquipmentSpec meth = Accessory("Meth", "Meth");
            DrugStats(meth, 20f, 50f);
            EquipmentSpec crack = Accessory("Crack", "Crack");
            DrugStats(crack, -20f, -50f);
            crack.BaseStats["cities"] = -5f;
        }

        private static EquipmentSpec Accessory(string id, string name)
        {
            return AddEquipment(new EquipmentSpec
            {
                Id = id, DisplayName = name, Tech = "Cyberware", Value = 1500, Material = "steel",
                Type = EquipmentType.Ring, NameTemplate = "ring_name"
            });
        }

        private static void CyberwareStats(EquipmentSpec spec)
        {
            spec.BaseStats["fertility"] = -0.1f;
            spec.BaseStats["max_age"] = 10f;
            spec.BaseStats["attack_speed"] = 20f;
            spec.BaseStats["damage"] = 20f;
            spec.BaseStats["range"] = 1f;
            spec.BaseStats["targets"] = 2f;
            spec.BaseStats["critical_chance"] = 0.15f;
            spec.BaseStats["intelligence"] = 7f;
            spec.BaseStats["warfare"] = 4f;
        }

        private static void DrugStats(EquipmentSpec spec, float attackSpeed, float speed)
        {
            spec.BaseStats["fertility"] = -0.1f;
            spec.BaseStats["max_children"] = -5f;
            spec.BaseStats["max_age"] = -10f;
            spec.BaseStats["attack_speed"] = attackSpeed;
            spec.BaseStats["damage"] = 90f;
            spec.BaseStats["speed"] = speed;
            spec.BaseStats["health"] = -30f;
            spec.BaseStats["range"] = 1f;
            spec.BaseStats["armor"] = 100f;
            spec.BaseStats["targets"] = 2f;
            spec.BaseStats["critical_chance"] = 0.15f;
            spec.BaseStats["knockback_reduction"] = 200f;
            spec.BaseStats["intelligence"] = -7f;
            spec.BaseStats["warfare"] = -4f;
        }

        private static EquipmentSpec AddEquipment(EquipmentSpec spec)
        {
            M1Materials.Apply(spec);
            Equipment.Add(spec);
            EquipmentById[spec.Id] = spec;
            ModernBoxCatalog.EquipmentIds.Add(spec.Id);
            return spec;
        }

        // Buttonz.cs: radius, terraform, effect and effect size of each bomb.
        private static void BuildBombs()
        {
            AddBomb("MOAB", "Super-Nuke", "Also known as the 'Lag Bomb'.", "ui/Icons/MOAB", 50, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f);
            AddBomb("Cobalt", "Cobalt Bomb", "Small Mushroom but huge radius, watch out with this one.", "ui/Icons/Cobalt", 120, "czar_bomba", "fx_explosion_huge", 0.2f, 0.3f);
            AddBomb("Ultron", "Ultron Bomb", "WOOOAH", "ui/Icons/Ultron", 100, "czar_bomba", "fx_explosion_huge", 0.8f, 0.9f);
            AddBomb("Death", "Death Bomb", "Such an original name.", "ui/Icons/Death", 100, "czar_bomba", "fx_explosion_huge", 1.2f, 1.6f);
            AddBomb("Xenium", "Xenium Bomb", "You thought the ultron bomb was big? This thing is HUGE.", "ui/Icons/Xeno", 400, "czar_bomba", "fx_explosion_huge", 4.3f, 7.9f);
            AddBomb("Mini", "Mini Nuke", "Small nukes, great for minor scuffles.", "ui/Icons/Mini", 5, "czar_bomba", "fx_explosion_huge", 0.4f, 0.6f);
            AddBomb("Proton", "Proton Bomb", "wtf is this?", "ui/Icons/Proton", 786, "czar_bomba", "fx_explosion_huge", 16.3f, 28.9f);
            AddBomb("Jupiter", "Jupiter Bomb", "The new monster.", "ui/Icons/Jupiter", 1486, "czar_bomba", "fx_explosion_huge", 32.3f, 56.9f);
            AddBomb("Eraser", "Eraser Bomb", "also known as the overcompensating bomb.", "ui/Icons/Eraser", 1000, "destroy_no_flash", "fx_antimatter_effect", 5.3f, 9.9f);
            AddBomb("Random", "Random Bomb", "You could be dropping a proton bomb, or a mini nuke, it's random!", "ui/Icons/wat", 0, "czar_bomba", "fx_explosion_huge", 0f, 0f, BombPattern.RandomLegacy);
        }

        private static void AddBomb(string id, string name, string description, string icon, int radius, string terraform,
            string effect, float scaleMin, float scaleMax, BombPattern pattern = BombPattern.Radial)
        {
            Bombs.Add(new BombSpec
            {
                Id = id,
                DisplayName = name,
                Description = description,
                IconPath = icon,
                // M1 used drops/drop_antimatter for the Eraser; 0.51.2 only ships drop_antimatterbomb.
                DropTexture = id == "Eraser" ? "drops/drop_antimatterbomb" : "drops/drop_czarbomba",
                Radius = radius,
                TerraformId = terraform,
                EffectId = effect,
                EffectScaleMin = scaleMin,
                EffectScaleMax = scaleMax,
                Pattern = pattern
            });
        }
    }
}
