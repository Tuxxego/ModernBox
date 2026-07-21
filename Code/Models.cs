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
        internal const int StandardMinimumEraYears = 50;
        internal const int StandardMaximumEraYears = 200;
        private const int StandardScheduleVersion = 1;
        private const string ScheduleVersionKey = ModernBoxCatalog.Guid + ".standard_world_era_schedule_version";
        private const string UnlockYearKeyPrefix = ModernBoxCatalog.Guid + ".standard_world_era_unlock_year.";
        private const string HighestEraKey = ModernBoxCatalog.Guid + ".highest_world_era";
        private static readonly string[] LegacyCultureEraTraits =
        {
            "m2_era_renaissance", "m2_era_industrial", "m2_era_modern", "m2_era_future"
        };
        private static M2Era? _lastWorldEra;

        internal static void UpdateCultures()
        {
            if (World.world == null || World.world.map_stats == null) return;
            EnsureStandardSchedule();
            M2Era currentEra = GetWorldEra();
            if (World.world.cultures == null) return;
            foreach (Culture culture in World.world.cultures)
            {
                if (culture == null || culture.data == null) continue;
                if (culture.data.saved_traits != null)
                {
                    foreach (string obsoleteTrait in LegacyCultureEraTraits)
                        while (culture.data.saved_traits.Remove(obsoleteTrait)) { }
                }
                if (_lastWorldEra != currentEra && ModernBoxCatalog.IsSupportedRace(culture.species_id))
                    M2LegacyBehaviorService.RefreshCultureSprites(culture);
            }
            if (_lastWorldEra == currentEra) return;
            _lastWorldEra = currentEra;
            ModernBoxDiagnostics.Info("World entered the M2 " + EraName(currentEra) + " era at world year " + GetWorldYear() + ".");
        }

        private static void EnsureStandardSchedule()
        {
            SaveCustomData data = GetScheduleData();
            if (data == null) return;
            int version;
            data.get(ScheduleVersionKey, out version, 0);
            if (version == StandardScheduleVersion) return;

            int cumulativeYear = 0;
            long seed = World.world.map_stats.life_dna;
            if (seed == 0) seed = MapBox.current_world_seed_id;
            foreach (EraSpec spec in ModernBoxCatalog.Eras)
            {
                cumulativeYear += StandardInterval(seed, spec.Era);
                data.set(UnlockYearKey(spec.Era), cumulativeYear);
            }
            data.set(ScheduleVersionKey, StandardScheduleVersion);
        }

        private static SaveCustomData GetScheduleData()
        {
            if (World.world == null || World.world.map_stats == null) return null;
            if (World.world.map_stats.custom_data == null)
                World.world.map_stats.custom_data = new SaveCustomData();
            return World.world.map_stats.custom_data;
        }

        private static int GetUnlockYear(M2Era era)
        {
            EnsureStandardSchedule();
            SaveCustomData data = GetScheduleData();
            if (data == null) return int.MaxValue;
            int year;
            data.get(UnlockYearKey(era), out year, int.MaxValue);
            return year;
        }

        private static string UnlockYearKey(M2Era era)
        {
            return UnlockYearKeyPrefix + era.ToString().ToLowerInvariant();
        }

        private static int StandardInterval(long worldSeed, M2Era era)
        {
            // The four world-era years are saved with the world. The seed only
            // supplies their first rolls, so reloading never changes the timeline.
            unchecked
            {
                ulong value = (ulong)worldSeed + 0x9E3779B97F4A7C15UL * (ulong)((int)era + 1);
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                value ^= value >> 31;
                return StandardMinimumEraYears + (int)(value %
                    (ulong)(StandardMaximumEraYears - StandardMinimumEraYears + 1));
            }
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

        internal static M2Era GetEra(Culture culture)
        {
            if (culture == null || !ModernBoxCatalog.IsSupportedRace(culture.species_id)) return M2Era.Medieval;
            return GetWorldEra();
        }

        internal static M2Era GetWorldEra()
        {
            EnsureStandardSchedule();
            SaveCustomData data = GetScheduleData();
            if (data == null) return M2Era.Medieval;

            int savedEraValue;
            data.get(HighestEraKey, out savedEraValue, -1);
            M2Era savedEra = savedEraValue < (int)M2Era.Medieval
                ? M2Era.Medieval
                : (M2Era)Math.Min(savedEraValue, (int)M2Era.Future);
            M2Era scheduledEra = GetScheduledWorldEra(GetWorldYear());

            // Existing saves from rewrite versions before this monotonic marker
            // are initialized from their present world year. This also makes a
            // newly installed year-500 world enter its proper current era at once.
            if (savedEraValue < 0)
            {
                savedEra = scheduledEra;
                data.set(HighestEraKey, (int)savedEra);
            }

            // Turning progression off pauses the world at its highest attained
            // era. It must never demote an established world back to Medieval.
            if (!ModernBoxSettings.Get("ProgressionOption"))
                return savedEra;

            if (scheduledEra > savedEra)
            {
                savedEra = scheduledEra;
                data.set(HighestEraKey, (int)savedEra);
            }
            else if (savedEraValue > (int)M2Era.Future)
            {
                data.set(HighestEraKey, (int)savedEra);
            }
            return savedEra;
        }

        private static M2Era GetScheduledWorldEra(int year)
        {
            for (int i = ModernBoxCatalog.Eras.Length - 1; i >= 0; i--)
                if (year >= GetUnlockYear(ModernBoxCatalog.Eras[i].Era)) return ModernBoxCatalog.Eras[i].Era;
            return M2Era.Medieval;
        }

        private static int GetWorldYear()
        {
            return World.world == null || World.world.map_stats == null ? 0 : Math.Max(0, World.world.map_stats.history_current_year);
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

        internal static void GetRequirements(M2Era era, out int year, out int population, out int buildings)
        {
            // Standard progression is world-year based. Build orders must not
            // impose the obsolete world-year/population/building thresholds too.
            year = 0;
            population = 0;
            buildings = 0;
        }

        internal static void GetRequirements(ProgressionTier tier, out int population, out int buildings)
        {
            int year;
            GetRequirements((M2Era)Math.Min((int)tier, (int)M2Era.Future), out year, out population, out buildings);
        }

        internal static string EraName(M2Era era)
        {
            return era == M2Era.Medieval ? "Medieval" : era.ToString();
        }
    }

    internal static class ModernBoxSettings
    {
        private const string Prefix = ModernBoxCatalog.Guid + ".";
        private const string MigrationKey = Prefix + "migration_2_2";
        private static readonly Dictionary<string, bool> Values = new Dictionary<string, bool>(StringComparer.Ordinal);

        internal static readonly string[] FactoryKeys =
        {
            "SoldierOption", "HumveeOption", "TankOption", "AirshipOption", "HeliOption", "DronesOption",
            "RailgunOption", "FighterJetOption", "GunshipOption", "BoiOption", "MIRVBomberOption",
            "TerranOption", "P9000Option"
        };

        internal static readonly string[] FeatureKeys =
        {
            "ProgressionOption", "ConstructionOption", "FactoriesOption", "EquipmentOption", "GunOption",
            "PipeGunOption", "CyberwareOption", "DrugsOption", "IdeologiesOption", "namesOption",
            "othernamesOption", "NukeOption", "MIRVOption", "AutomaticInvasionsOption", "ShakeOption",
            "StartupAudio", "DeveloperDiagnostics"
        };

        internal static void LoadAndMigrate()
        {
            foreach (string key in FactoryKeys) Values[key] = Read(key, true);
            foreach (string key in FeatureKeys) Values[key] = Read(key, DefaultFor(key));
            if (PlayerPrefs.GetInt(MigrationKey, 0) != 0) return;
            foreach (string key in FactoryKeys) MigrateLegacy(key, key);
            foreach (string key in FeatureKeys) MigrateLegacy(key, key);
            MigrateLegacy("FactoriesOption", "FactoriesOption");
            MigrateLegacy("AutomaticInvasionsOption", "InvasionsOption");
            PlayerPrefs.SetInt(MigrationKey, 1);
            PlayerPrefs.Save();
        }

        private static bool DefaultFor(string key)
        {
            return key != "NukeOption" && key != "MIRVOption" && key != "AutomaticInvasionsOption" && key != "DeveloperDiagnostics";
        }

        private static bool Read(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(Prefix + key, defaultValue ? 1 : 0) == 1;
        }

        private static void MigrateLegacy(string newKey, string oldKey)
        {
            if (PlayerPrefs.HasKey(Prefix + newKey) || !PlayerPrefs.HasKey(oldKey)) return;
            bool value = PlayerPrefs.GetInt(oldKey, 0) == 1;
            Values[newKey] = value;
            PlayerPrefs.SetInt(Prefix + newKey, value ? 1 : 0);
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
            foreach (string key in FeatureKeys) Set(key, DefaultFor(key));
        }
    }
}
