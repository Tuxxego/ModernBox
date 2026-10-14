using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    internal sealed class ModernUnitSpec
    {
        internal string Id;
        internal string Attack;
        internal string NameTemplate;
        internal string IconPath;
        internal string Race = "human";
        internal float Health;
        internal float Speed;
        // -1 means the original never set it, so the _mob default is kept.
        internal float Armor = -1f;
        internal float Damage;
        internal float AttackSpeed;
        internal float Range = -1f;
        internal float Accuracy = -1f;
        internal float Scale = -1f;
        internal float KnockbackReduction;
        internal float Warfare;
        internal bool Flying;
        internal bool Humanoid;
        internal bool Equipment;
        internal bool DieOnBlocks;
        internal bool IgnoreBlocks;
        internal bool MoveFromBlock;
        internal bool JetJob;
        internal string[] Walk;
        internal string[] Idle;
        internal string[] Swim;
        internal string[] DefaultWeapons = Array.Empty<string>();
        internal string[] Traits = Array.Empty<string>();
    }

    internal sealed class BuildingSpec
    {
        internal string Id;
        internal string Tech;
        internal ConstructionCost Cost;
        internal int Housing;
        internal float Health = 3000f;
        internal int Priority = 69999;
        internal bool Burnable = true;
        internal bool PlaceSingle;
        internal bool LivingHouse = true;
        internal bool Tower;
        // False when Commerce.cs never added an order for it (TankFactory, AirshipFactory).
        internal bool HasOrder = true;
        internal string SpawnUnit;
        internal string SpawnSetting;
        // Only ModernBarracks started with spawnUnits = true.
        internal bool SpawnsByDefault;
    }

    internal sealed class EquipmentSpec
    {
        internal string Id;
        internal string DisplayName;
        internal string Projectile;
        internal string Tech;
        internal string Material = "iron";
        internal string NameTemplate = "sword_name";
        internal EquipmentType Type = EquipmentType.Weapon;
        internal bool Legendary;
        internal int Value;
        internal string Resource1 = "none";
        internal int Resource1Cost;
        internal string Resource2 = "none";
        internal int Resource2Cost;
        internal int MinimumStorage;
        internal readonly Dictionary<string, float> BaseStats = new Dictionary<string, float>(StringComparer.Ordinal);
        // Added by the material when the item is made; a unit's built-in attack doesn't get them.
        internal readonly Dictionary<string, float> MaterialStats = new Dictionary<string, float>(StringComparer.Ordinal);
    }

    internal enum BombPattern
    {
        Radial,
        RandomLegacy
    }

    internal sealed class BombSpec
    {
        internal string Id;
        internal string DisplayName;
        internal string Description;
        internal string IconPath;
        internal string DropTexture;
        internal int Radius;
        internal string TerraformId;
        internal string EffectId;
        internal float EffectScaleMin;
        internal float EffectScaleMax;
        internal BombPattern Pattern;
    }

    internal static class ModernBoxCatalog
    {
        internal const string Guid = "TUXXEGO_MODERNBOX_M1_REWRITE";
        internal const string HarmonyId = "tuxxego.modernbox.m1.rewrite.0_51_2";

        internal static readonly string[] SupportedRaces = { "human", "orc", "elf", "dwarf" };

        internal static readonly HashSet<string> UnitIds = new HashSet<string>(StringComparer.Ordinal);
        internal static readonly HashSet<string> EquipmentIds = new HashSet<string>(StringComparer.Ordinal);
        internal static readonly string[] MirvIds = { "BudgetMIRV", "DecentMIRV", "MIRV", "MIRVBomb", "STRONGMIRV" };

        internal static bool IsSupportedRace(string id)
        {
            return Array.IndexOf(SupportedRaces, id) >= 0;
        }

        internal static bool IsSupportedCity(City city)
        {
            if (city == null || city.isRekt() || city.kingdom == null || city.kingdom.wild || !city.kingdom.isCiv()) return false;
            ActorAsset species = city.getActorAsset();
            return species != null && IsSupportedRace(species.id);
        }

        internal static string GetRace(City city)
        {
            ActorAsset asset = city == null ? null : city.getActorAsset();
            return asset == null ? string.Empty : asset.id;
        }
    }

    // SavedSettings.cs: one PlayerPrefs bool per option and a version string.
    internal static class ModernBoxSettings
    {
        private const string Prefix = ModernBoxCatalog.Guid + ".";
        private const string VersionKey = Prefix + "SettingVersion";
        internal const string SettingsVersion = "10.0.0";
        private static readonly Dictionary<string, bool> Values = new Dictionary<string, bool>(StringComparer.Ordinal);

        internal static bool IsNewVersion { get; private set; }

        private static readonly Dictionary<string, bool> Defaults = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            { "namesOption", true },
            { "othernamesOption", true },
            { "CyberwareOption", true },
            { "DrugsOption", true },
            { "PipeGunOption", true },
            { "IdeologiesOption", true },
            { "GunOption", true },
            { "DronesOption", true },
            { "MIRVOption", false },
            { "NukeOption", false },
            { "STRONGMIRVOption", false },
            { "Developer_Mode", false },
            { "HeliOption", false },
            { "AirshipOption", false },
            { "TankOption", false },
            { "MIRVBomberOption", false },
            { "HumveeOption", false },
            { "F22Option", false },
            { "BoiOption", false },
            { "GunshipOption", false },
            { "RailgunOption", false },
            { "SoldierOption", false }
        };

        internal static void Load()
        {
            foreach (KeyValuePair<string, bool> pair in Defaults) Values[pair.Key] = pair.Value;
            // Like the original, a new version resets your settings.
            IsNewVersion = PlayerPrefs.GetString(VersionKey, string.Empty) != SettingsVersion;
            if (IsNewVersion)
            {
                SaveDefaults();
                return;
            }
            foreach (KeyValuePair<string, bool> pair in Defaults)
                Values[pair.Key] = PlayerPrefs.GetInt(Prefix + pair.Key, pair.Value ? 1 : 0) == 1;
        }

        internal static bool Get(string key)
        {
            bool value;
            return Values.TryGetValue(key, out value) && value;
        }

        // Main.modifyBoolOption: saved straight away.
        internal static void Set(string key, bool value)
        {
            Values[key] = value;
            PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        // Main.resetToDefaults only writes the defaults; the buttons keep their state
        // until the game is restarted ("Restart the game for them to take affect!").
        internal static void ResetSaved()
        {
            SaveDefaults();
        }

        private static void SaveDefaults()
        {
            foreach (KeyValuePair<string, bool> pair in Defaults) PlayerPrefs.SetInt(Prefix + pair.Key, pair.Value ? 1 : 0);
            PlayerPrefs.SetString(VersionKey, SettingsVersion);
            PlayerPrefs.Save();
        }
    }
}
