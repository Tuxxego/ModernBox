using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal enum M2Era
    {
        Medieval = 0,
        Renaissance = 1,
        Industrial = 2,
        Modern = 3,
        Future = 4
    }

    // Kept internal while the registries share the proven M1 service shapes.
    // Values intentionally collapse the old six-tier model into M2's four eras.
    internal enum ProgressionTier
    {
        None = 0,
        Urban = 1,
        Industrial = 2,
        Modern = 3,
        Advanced = 4,
        Strategic = 4,
        Nuclear = 4
    }

    internal enum M2UnitRole
    {
        Offensive,
        Heavy,
        Support,
        Air,
        Titan,
        Naval,
        Creature
    }

    internal sealed class EraSpec
    {
        internal M2Era Era;
    }

    internal sealed class ModernUnitSpec
    {
        internal string Id;
        internal string BaseAsset = "$basic_unit$";
        internal string TextureFolder;
        internal string Attack;
        internal string NameTemplate;
        internal string IconPath;
        internal string Race = "human";
        internal M2Era Era;
        internal M2UnitRole Role;
        internal float Health;
        internal float Speed;
        internal float Armor;
        internal float Damage;
        internal float AttackSpeed;
        internal float Range;
        internal float Projectiles;
        internal float Scale;
        internal bool Flying;
        internal bool Boat;
        internal bool Humanoid;
        internal bool Equipment;
        internal string ScrapBuilding;
        internal string[] Traits = Array.Empty<string>();
    }

    internal sealed class BuildingSpec
    {
        internal string Id;
        internal string SourceId;
        internal string Race;
        internal M2Era Era;
        internal ConstructionCost Cost;
        internal int Limit;
        internal int Housing;
        internal bool Civilian;
        internal bool Tower;
        internal ProgressionTier Tier;
        internal bool UpgradeOnly;
        internal string UpgradeFrom;
        internal string UpgradeTo;
        internal string Type;
        internal float Health = 3000f;
        // Registered for save compatibility only; never built by cities.
        internal bool Legacy;
    }

    internal sealed class BuildingUpgradeSpec
    {
        internal string Race;
        internal string SourceId;
        internal string TargetId;
        internal M2Era Era;
    }

    internal sealed class FactorySpec
    {
        internal string BuildingId;
        internal string[] UnitIds;
        internal M2Era Era;
        internal float Interval;
        internal string SettingKey;
        internal ProgressionTier Tier;
        internal ConstructionCost UnitCost;
    }

    internal sealed class EquipmentSpec
    {
        internal string Id;
        internal string DisplayName;
        internal string Projectile;
        internal M2Era Era;
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
        internal ProgressionTier Tier;
        internal readonly Dictionary<string, float> BaseStats = new Dictionary<string, float>(StringComparer.Ordinal);
        // Stats the item's material added in the old engine (crafted items only).
        internal readonly Dictionary<string, float> MaterialStats = new Dictionary<string, float>(StringComparer.Ordinal);
        internal int MinimumStorage;
    }

    internal enum BombPattern
    {
        Radial,
        RandomLegacy,
        ClusterNuke,
        ClusterLightning,
        Spreader,
        VisualOnly
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
        internal BombPattern Pattern;
    }

    internal sealed class InvasionSpec
    {
        internal string Id;
        internal string ActorId;
        internal bool Automatic;
        internal int MinimumPopulation;
        internal int MinimumCities;
        internal int MinimumUnits;
        internal int MaximumUnits;
        internal int WorldCap;
    }

    internal static class ModernBoxCatalog
    {
        internal const string Guid = "TUXXEGO_MODERNBOX_M2_REWRITE";
        internal const string HarmonyId = "tuxxego.modernbox.m2.rewrite.0_51_2";
        internal const string Human = "human";

        internal static readonly string[] SupportedRaces = { "human", "orc", "elf", "dwarf" };

        internal static readonly EraSpec[] Eras =
        {
            new EraSpec { Era = M2Era.Renaissance },
            new EraSpec { Era = M2Era.Industrial },
            new EraSpec { Era = M2Era.Modern },
            new EraSpec { Era = M2Era.Future }
        };

        internal static readonly HashSet<string> UnitIds = new HashSet<string>(StringComparer.Ordinal);
        internal static readonly HashSet<string> CivilianBuildingIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "casino", "restaurant", "mall", "school", "modernbuilding"
        };
        internal static readonly HashSet<string> FactoryBuildingIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "AirFactory", "TankFactory", "TerranFactory", "P9000Factory", "RailgunFactory",
            "HumveeFactory", "HelicopterFactory", "DroneFactory", "AirshipFactory",
            "FighterJetFactory", "BoiFactory", "GunshipFactory"
        };
        internal static readonly HashSet<string> EquipmentIds = new HashSet<string>(StringComparer.Ordinal);
        internal static readonly Dictionary<string, M2Era> EquipmentEras = new Dictionary<string, M2Era>(StringComparer.Ordinal);
        internal static readonly Dictionary<string, ProgressionTier> EquipmentTiers = new Dictionary<string, ProgressionTier>(StringComparer.Ordinal);
        internal static string[] GunIds = Array.Empty<string>();
        internal static string[] MirvIds = { "MIRV", "MIRVBomb" };

        internal static bool IsSupportedRace(string id)
        {
            return Array.IndexOf(SupportedRaces, id) >= 0;
        }

        internal static string FactionForRace(string race)
        {
            switch (race)
            {
                case "human": return "alliance";
                case "dwarf": return "harden";
                case "elf": return "gaia";
                case "orc": return "horde";
                default: return string.Empty;
            }
        }
    }

    internal static class ModernProgression
    {
        // Set by the old world-timer eras; only read now to migrate those saves.
        private const string HighestEraKey = ModernBoxCatalog.Guid + ".highest_world_era";
        private static readonly string[] LegacyCultureEraTraits =
        {
            "m2_era_renaissance", "m2_era_industrial", "m2_era_modern", "m2_era_future"
        };

        internal static void UpdateCultures()
        {
            if (World.world == null || World.world.cultures == null) return;
            foreach (Culture culture in World.world.cultures)
            {
                if (culture == null || culture.data == null || culture.data.saved_traits == null) continue;
                foreach (string obsoleteTrait in LegacyCultureEraTraits)
                    while (culture.data.saved_traits.Remove(obsoleteTrait)) { }
            }
        }

        internal static SaveCustomData WorldData()
        {
            if (World.world == null || World.world.map_stats == null) return null;
            if (World.world.map_stats.custom_data == null)
                World.world.map_stats.custom_data = new SaveCustomData();
            return World.world.map_stats.custom_data;
        }

        internal static M2Era LegacyWorldEra()
        {
            SaveCustomData data = WorldData();
            if (data == null) return M2Era.Medieval;
            int value;
            data.get(HighestEraKey, out value, -1);
            if (value < (int)M2Era.Medieval) return M2Era.Medieval;
            return (M2Era)Math.Min(value, (int)M2Era.Future);
        }

        internal static M2Era GetEra(City city)
        {
            if (!IsSupportedCity(city) || city.culture == null) return M2Era.Medieval;
            return GetEra(city.culture);
        }

        internal static ProgressionTier GetTier(City city)
        {
            return (ProgressionTier)(int)GetEra(city);
        }

        // Each culture's era comes from its researched techs, like the original.
        internal static M2Era GetEra(Culture culture)
        {
            return M2Tech.Era(culture);
        }

        internal static bool HasEra(City city, M2Era era)
        {
            return GetEra(city) >= era;
        }

        internal static bool IsSupportedCity(City city)
        {
            if (city == null || city.isRekt() || city.kingdom == null || city.kingdom.wild || !city.kingdom.isCiv()) return false;
            ActorAsset species = city.getActorAsset();
            return species != null && ModernBoxCatalog.IsSupportedRace(species.id);
        }

        internal static bool IsHumanCity(City city)
        {
            return IsSupportedCity(city);
        }

        internal static string GetRace(City city)
        {
            ActorAsset asset = city == null ? null : city.getActorAsset();
            return asset == null ? string.Empty : asset.id;
        }

        internal static string EraName(M2Era era)
        {
            return era == M2Era.Medieval ? "Medieval" : era.ToString();
        }
    }

    internal static class ModernBoxSettings
    {
        private const string Prefix = ModernBoxCatalog.Guid + ".";
        private const string VersionKey = Prefix + "SettingVersion";
        internal const string SettingsVersion = "2.2.0.0";
        private static readonly Dictionary<string, bool> Values = new Dictionary<string, bool>(StringComparer.Ordinal);

        internal static bool IsNewVersion { get; private set; }

        // Saved settings, these have buttons on the tab.
        private static readonly Dictionary<string, bool> SavedDefaults = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            { "namesOption", true },
            { "othernamesOption", true },
            { "NukeOption", false },
            { "Developer_Mode", false }
        };

        // No button for these in M2, so they always act like the original did.
        private static readonly Dictionary<string, bool> FixedDefaults = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            { "SoldierOption", true },
            { "HumveeOption", true },
            { "TankOption", true },
            { "AirshipOption", true },
            { "HeliOption", true },
            { "DronesOption", true },
            { "RailgunOption", true },
            { "FighterJetOption", true },
            { "GunshipOption", true },
            { "BoiOption", true },
            { "MIRVBomberOption", true },
            { "TerranOption", true },
            { "P9000Option", false },
            { "FactoriesOption", true },
            { "ProgressionOption", true },
            { "ConstructionOption", true },
            { "EquipmentOption", true },
            { "GunOption", true },
            { "PipeGunOption", true },
            { "CyberwareOption", true },
            { "DrugsOption", true },
            { "MIRVOption", false },
            { "IdeologiesOption", true },
            { "AutomaticInvasionsOption", true }
        };

        internal static void LoadAndMigrate()
        {
            foreach (KeyValuePair<string, bool> pair in FixedDefaults) Values[pair.Key] = pair.Value;
            foreach (KeyValuePair<string, bool> pair in SavedDefaults) Values[pair.Key] = pair.Value;

            // Like the original, a new version resets your settings.
            IsNewVersion = PlayerPrefs.GetString(VersionKey, string.Empty) != SettingsVersion;
            if (IsNewVersion)
            {
                Save();
                return;
            }
            foreach (KeyValuePair<string, bool> pair in SavedDefaults)
                Values[pair.Key] = PlayerPrefs.GetInt(Prefix + pair.Key, pair.Value ? 1 : 0) == 1;
        }

        private static void Save()
        {
            foreach (string key in SavedDefaults.Keys) PlayerPrefs.SetInt(Prefix + key, Values[key] ? 1 : 0);
            PlayerPrefs.SetString(VersionKey, SettingsVersion);
            PlayerPrefs.Save();
        }

        internal static bool Get(string key)
        {
            bool value;
            return Values.TryGetValue(key, out value) && value;
        }

        internal static void Set(string key, bool value)
        {
            Values[key] = value;
            if (SavedDefaults.ContainsKey(key)) Save();
            ProductionService.ApplyDynamicSettings();
            ModernBoxUi.SyncNativeToggle(key, value);
        }

        internal static void Reset()
        {
            foreach (KeyValuePair<string, bool> pair in FixedDefaults) Values[pair.Key] = pair.Value;
            foreach (KeyValuePair<string, bool> pair in SavedDefaults) Set(pair.Key, pair.Value);
        }
    }
}
