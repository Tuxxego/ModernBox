using System;
using System.Collections.Generic;

namespace ModernBoxM1Rewrite
{
    // What each tab toggle actually did in the original, including its quirks:
    // - At startup Buttonz only ran the toggles that were saved ON, and it ran them before
    //   the factories and ideology traits existed, so those two kinds did nothing then.
    // - The factories only make units in buildings created or loaded while their toggle is on.
    // - The weapon toggles add or remove one copy of each item from a race's preferred list.
    internal static class M1Toggles
    {
        private static readonly string[] Races = { "human", "orc", "dwarf", "elf" };

        private static readonly string[] Guns =
        {
            "Mp5", "Uzi", "Sniper", "RocketLauncher", "mm9", "Minigun", "AK47", "ThompsonM1A1", "XM8", "SGT44", "M4A1",
            "FAMAS", "malorian", "SCAR", "AK103", "MP7", "Musket", "HK416", "M16", "DesertEagle", "Glock17"
        };
        private static readonly string[] PipeGuns = { "PipePistol", "PipeRifle", "PipeShotgun", "Musket" };
        private static readonly string[] Mirvs = { "BudgetMIRV", "DecentMIRV", "MIRV" };
        private static readonly string[] StrongMirvs = { "STRONGMIRV" };
        private static readonly string[] Cyberware = { "Sandevistan", "TurboBooster" };
        private static readonly string[] Drugs = { "Meth", "Crack" };

        // race -> item -> copies in race.preferred_weapons
        private static readonly Dictionary<string, Dictionary<string, int>> Preferred = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        // BuildingAsset.spawnUnits of each factory. Commerce.cs only set it for ModernBarracks.
        private static readonly Dictionary<string, bool> SpawnUnits = new Dictionary<string, bool>(StringComparer.Ordinal);
        private static string _humanNames, _orcNames, _elfNames, _dwarfNames;

        internal static bool IdeologyBirth { get; private set; }

        // The tab buttons, their setting and what they run (Buttonz.cs order).
        internal static readonly Dictionary<string, Action<bool>> Actions = new Dictionary<string, Action<bool>>(StringComparer.Ordinal)
        {
            { "STRONGMIRVOption", on => SetPreferred(StrongMirvs, StrongMirvs, on) },
            { "MIRVOption", on => SetPreferred(Mirvs, Mirvs, on) },
            { "CyberwareOption", on => SetPreferred(Cyberware, Cyberware, on) },
            { "DrugsOption", on => SetPreferred(Drugs, Drugs, on) },
            // SetFactoriesSpawnDrone always wrote false.
            { "DronesOption", on => SpawnUnits["DroneFactory"] = false },
            { "HeliOption", on => SpawnUnits["HelicopterFactory"] = on },
            { "AirshipOption", on => SpawnUnits["AirshipFactory"] = on },
            { "TankOption", on => SpawnUnits["TankFactory"] = on },
            { "MIRVBomberOption", on => SpawnUnits["AirFactory"] = on },
            { "HumveeOption", on => SpawnUnits["HumveeFactory"] = on },
            { "F22Option", on => SpawnUnits["F22Factory"] = on },
            { "BoiOption", on => SpawnUnits["BoiFactory"] = on },
            { "GunshipOption", on => SpawnUnits["GunshipFactory"] = on },
            { "PipeGunOption", on => SetPreferred(PipeGuns, PipeGuns, on) },
            { "GunOption", on => SetGuns(on) },
            { "Developer_Mode", on => { if (on) M1Windows.Show("DeveloperWindow"); } },
            { "SoldierOption", on => SpawnUnits["ModernBarracks"] = on },
            { "NukeOption", on => { if (on) { M1Windows.Show("NukeWindow"); M1Silo.TurnOn(); } } },
            { "RailgunOption", on => SpawnUnits["RailgunFactory"] = on },
            { "IdeologiesOption", on => IdeologyBirth = on },
            { "othernamesOption", SetOtherNames },
            { "namesOption", SetHumanNames }
        };

        // Buttonz ran these at startup before Commerce and Ideologies existed, so in the
        // original the saved values did nothing until the button was clicked again. Here they
        // apply at startup, on or off, so factories, barracks and ideologies follow the tab.
        private static readonly HashSet<string> AppliedWithSavedValue = new HashSet<string>(StringComparer.Ordinal)
        {
            "DronesOption", "HeliOption", "AirshipOption", "TankOption", "MIRVBomberOption", "HumveeOption", "F22Option",
            "BoiOption", "GunshipOption", "SoldierOption", "RailgunOption", "IdeologiesOption"
        };

        internal static void ApplyStartup()
        {
            Preferred.Clear();
            foreach (string race in Races) Preferred[race] = new Dictionary<string, int>(StringComparer.Ordinal);
            SpawnUnits.Clear();
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
                if (!string.IsNullOrEmpty(spec.SpawnUnit)) SpawnUnits[spec.Id] = spec.SpawnsByDefault;
            // The ideology traits were made with a 37% birth chance.
            IdeologyBirth = true;
            CaptureDefaultNames();

            foreach (KeyValuePair<string, Action<bool>> pair in Actions)
            {
                bool saved = ModernBoxSettings.Get(pair.Key);
                if (!saved && !AppliedWithSavedValue.Contains(pair.Key)) continue;
                try { pair.Value(saved); }
                catch (Exception e) { ModernBoxDiagnostics.Error("Startup toggle " + pair.Key + " failed: " + e); }
            }
        }

        // A tab button was clicked.
        internal static void Toggle(string setting, bool value)
        {
            ModernBoxSettings.Set(setting, value);
            Action<bool> action;
            if (Actions.TryGetValue(setting, out action)) action(value);
        }

        internal static bool SpawnsUnits(string buildingId)
        {
            bool value;
            return buildingId != null && SpawnUnits.TryGetValue(buildingId, out value) && value;
        }

        internal static bool IsPreferred(string race, string itemId)
        {
            Dictionary<string, int> items;
            int copies;
            return race != null && Preferred.TryGetValue(race, out items) && items.TryGetValue(itemId, out copies) && copies > 0;
        }

        private static void SetGuns(bool on)
        {
            if (on)
            {
                foreach (string race in Races)
                    foreach (string id in Guns)
                        if (race != "elf" || id != "Musket") Change(race, id, 1);
                return;
            }
            // turnOffGuns also took one pipe gun of each kind from every race but the elves.
            foreach (string race in Races)
            {
                foreach (string id in Guns)
                    if (race != "elf" || id != "Musket") Change(race, id, -1);
                if (race == "elf") continue;
                Change(race, "PipePistol", -1);
                Change(race, "PipeRifle", -1);
                Change(race, "PipeShotgun", -1);
            }
        }

        private static void SetPreferred(string[] add, string[] remove, bool on)
        {
            foreach (string race in Races)
                foreach (string id in on ? add : remove)
                    Change(race, id, on ? 1 : -1);
        }

        private static void Change(string race, string id, int delta)
        {
            Dictionary<string, int> items = Preferred[race];
            int copies;
            items.TryGetValue(id, out copies);
            items[id] = Math.Max(0, copies + delta);
        }

        private static void CaptureDefaultNames()
        {
            if (_humanNames != null) return;
            _humanNames = NameTemplate("human");
            _orcNames = NameTemplate("orc");
            _elfNames = NameTemplate("elf");
            _dwarfNames = NameTemplate("dwarf");
        }

        private static string NameTemplate(string race)
        {
            ActorAsset asset = AssetManager.actor_library.get(race);
            return asset == null ? string.Empty : asset.name_template_unit;
        }

        private static void SetNames(string race, string template)
        {
            ActorAsset asset = AssetManager.actor_library.get(race);
            if (asset != null && !string.IsNullOrEmpty(template)) asset.name_template_unit = template;
        }

        private static void SetHumanNames(bool on)
        {
            SetNames("human", on ? "Modern_Names" : _humanNames);
        }

        // turnOffOtherNames forgot the dwarves, so they keep modern names until a restart.
        private static void SetOtherNames(bool on)
        {
            SetNames("orc", on ? "Modern_Orc_Names" : _orcNames);
            SetNames("elf", on ? "Modern_Elf_Names" : _elfNames);
            if (on) SetNames("dwarf", "Modern_Dwarf_Names");
        }
    }
}
