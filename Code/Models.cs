using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal enum ProgressionTier
    {
        None = 0,
        Urban = 1,
        Industrial = 2,
        Modern = 3,
        Advanced = 4,
        Strategic = 5,
        Nuclear = 6
    }

    internal sealed class ModernUnitSpec
    {
        internal string Id;
        internal string Attack;
        internal string NameTemplate;
        internal string IconPath;
        internal float Health;
        internal float Speed;
        internal float Armor;
        internal float Damage;
        internal float AttackSpeed;
        internal float Range;
        internal float Scale;
        internal bool Flying;
        internal string[] Traits;
    }

    internal sealed class BuildingSpec
    {
        internal string Id;
        internal ProgressionTier Tier;
        internal ConstructionCost Cost;
        internal int Limit;
        internal int Housing;
        internal bool Civilian;
        internal bool Tower;
    }

    internal sealed class FactorySpec
    {
        internal string BuildingId;
        internal string[] UnitIds;
        internal ProgressionTier Tier;
        internal float Interval;
        internal ConstructionCost UnitCost;
        internal string SettingKey;
    }

    internal sealed class EquipmentSpec
    {
        internal string Id;
        internal string DisplayName;
        internal string Projectile;
        internal ProgressionTier Tier;
        internal EquipmentType Type;
        internal float Damage;
        internal float Range;
        internal float AttackSpeed;
        internal float Accuracy;
        internal int Value;
        internal string Resource1;
        internal int Resource1Cost;
        internal string Resource2;
        internal int Resource2Cost;
    }

    internal sealed class BombSpec
    {
        internal string Id;
        internal string DisplayName;
        internal string IconPath;
        internal string DropTexture;
        internal int Radius;
        internal string TerraformId;
        internal string EffectId;
        internal float EffectScaleMin;
        internal float EffectScaleMax;
    }

    internal static class ModernBoxCatalog
    {
        internal const string Guid = "TUXXEGO_MODERNBOX_M1_REWRITE";
        internal const string HarmonyId = "tuxxego.modernbox.m1.rewrite.0_51_2";
        internal const string Human = "human";

        internal static readonly HashSet<string> UnitIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "Soldier", "Tank", "MissileSystem", "Railgun", "Humvee", "Heli", "Gunship",
            "Drone", "Zeppelin", "MIRVBomber", "CargoPlane", "F22", "F55"
        };

        internal static readonly HashSet<string> CivilianBuildingIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "casino", "restaurant", "mall", "school", "modernbuilding"
        };

        internal static readonly HashSet<string> FactoryBuildingIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "ModernBarracks", "AirFactory", "TankFactory", "RailgunFactory", "HumveeFactory",
            "HelicopterFactory", "DroneFactory", "AirshipFactory", "F22Factory", "F55Factory",
            "BoiFactory", "GunshipFactory"
        };

        internal static readonly string[] GunIds =
        {
            "Mp5", "NorincoCQ", "Sniper", "mm9", "RocketLauncher", "Uzi", "Minigun",
            "GunshipCannon", "AK47", "AK103", "XM8", "SGT44", "ThompsonM1A1", "M4A1",
            "FAMAS", "malorian", "SCAR", "PipeRifle", "PipePistol", "PipeShotgun", "Musket",
            "MP7", "HK416", "M16", "DesertEagle", "Glock17"
        };

        internal static readonly string[] MirvIds =
        {
            "BudgetMIRV", "DecentMIRV", "MIRV", "MIRVBomb", "STRONGMIRV"
        };

        internal static readonly HashSet<string> EquipmentIds = CreateEquipmentSet();
        internal static readonly Dictionary<string, ProgressionTier> EquipmentTiers = new Dictionary<string, ProgressionTier>(StringComparer.Ordinal);

        private static HashSet<string> CreateEquipmentSet()
        {
            HashSet<string> result = new HashSet<string>(GunIds, StringComparer.Ordinal);
            foreach (string id in MirvIds) result.Add(id);
            result.Add("Sandevistan");
            result.Add("TurboBooster");
            result.Add("Meth");
            result.Add("Crack");
            return result;
        }
    }

    internal static class ModernProgression
    {
        internal static ProgressionTier GetTier(City city)
        {
            if (!IsHumanCity(city)) return ProgressionTier.None;
            int population = city.getPopulationPeople();
            int buildings = city.countBuildings();
            if (population >= 220 && buildings >= 30) return ProgressionTier.Nuclear;
            if (population >= 170 && buildings >= 26) return ProgressionTier.Strategic;
            if (population >= 130 && buildings >= 23) return ProgressionTier.Advanced;
            if (population >= 100 && buildings >= 20) return ProgressionTier.Modern;
            if (population >= 75 && buildings >= 18) return ProgressionTier.Industrial;
            if (population >= 50 && buildings >= 16) return ProgressionTier.Urban;
            return ProgressionTier.None;
        }

        internal static bool IsHumanCity(City city)
        {
            ActorAsset asset = city == null ? null : city.getActorAsset();
            return asset != null && string.Equals(asset.id, ModernBoxCatalog.Human, StringComparison.Ordinal);
        }

        internal static void GetRequirements(ProgressionTier tier, out int population, out int buildings)
        {
            switch (tier)
            {
                case ProgressionTier.Urban: population = 50; buildings = 16; break;
                case ProgressionTier.Industrial: population = 75; buildings = 18; break;
                case ProgressionTier.Modern: population = 100; buildings = 20; break;
                case ProgressionTier.Advanced: population = 130; buildings = 23; break;
                case ProgressionTier.Strategic: population = 170; buildings = 26; break;
                case ProgressionTier.Nuclear: population = 220; buildings = 30; break;
                default: population = int.MaxValue; buildings = int.MaxValue; break;
            }
        }
    }

    internal static class ModernBoxSettings
    {
        private const string Prefix = ModernBoxCatalog.Guid + ".";
        private const string MigrationKey = Prefix + "migration_10_1";
        private static readonly Dictionary<string, bool> Values = new Dictionary<string, bool>(StringComparer.Ordinal);

        internal static readonly string[] FactoryKeys =
        {
            "SoldierOption", "HumveeOption", "TankOption", "AirshipOption", "HeliOption", "DronesOption",
            "RailgunOption", "F22Option", "F55Option", "GunshipOption", "BoiOption", "MIRVBomberOption"
        };

        internal static readonly string[] FeatureKeys =
        {
            "NukeOption", "MIRVOption", "GunOption", "PipeGunOption", "CyberwareOption", "DrugsOption",
            "IdeologiesOption", "namesOption", "othernamesOption", "StartupAudio", "DeveloperDiagnostics"
        };

        internal static void LoadAndMigrate()
        {
            foreach (string key in FactoryKeys) Values[key] = Read(key, true);
            foreach (string key in FeatureKeys) Values[key] = Read(key, key != "DeveloperDiagnostics" && key != "MIRVOption");
            if (PlayerPrefs.GetInt(MigrationKey, 0) == 0)
            {
                foreach (string key in FactoryKeys) MigrateLegacy(key);
                foreach (string key in FeatureKeys) MigrateLegacy(key);
                PlayerPrefs.SetInt(MigrationKey, 1);
                PlayerPrefs.Save();
            }
        }

        private static bool Read(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(Prefix + key, defaultValue ? 1 : 0) == 1;
        }

        private static void MigrateLegacy(string key)
        {
            if (PlayerPrefs.HasKey(Prefix + key) || !PlayerPrefs.HasKey(key)) return;
            bool value = PlayerPrefs.GetInt(key, 0) == 1;
            Values[key] = value;
            PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0);
        }

        internal static bool Get(string key)
        {
            bool value;
            return Values.TryGetValue(key, out value) && value;
        }

        internal static void Set(string key, bool value)
        {
            Values[key] = value;
            PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0);
            PlayerPrefs.Save();
            ProductionService.ApplyDynamicSettings();
            ModernBoxUi.SyncNativeToggle(key, value);
        }

        internal static void Reset()
        {
            foreach (string key in FactoryKeys) Set(key, true);
            foreach (string key in FeatureKeys) Set(key, key != "DeveloperDiagnostics" && key != "MIRVOption");
        }
    }
}
