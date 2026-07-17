using System;
using System.Collections;
using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class ModernBoxUi
    {
        private const string TabId = "modernbox_rewrite_tab";
        private static readonly Dictionary<string, List<GameObject>> Pages = new Dictionary<string, List<GameObject>>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> ToggleSettings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> SettingOptions = new Dictionary<string, string>(StringComparer.Ordinal);
        private static PowersTab _tab;

        internal static void BeginCreate(MonoBehaviour host)
        {
            host.StartCoroutine(CreateWhenReady());
        }

        private static IEnumerator CreateWhenReady()
        {
            while (PowerButtonSelector.instance == null) yield return null;
            _tab = TabManager.CreateTab(TabId, "modernbox_tab", "modernbox_tab_description", Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare"), null);
            if (_tab == null)
            {
                ModernBoxDiagnostics.Error("NML TabManager could not create the ModernBox tab.");
                yield break;
            }
            CreatePages();
            // TabManager assigns parentObj during the tab's first Unity frame.
            // Recalculate while every page is still active so the scroll area is
            // wide enough for the largest page, then hide the inactive pages.
            while (_tab.parentObj == null) yield return null;
            _tab.recalc();
            ShowPage("Industry");
            ModernLocalization.Apply();
            ModernBoxDiagnostics.Info("ModernBox paginated power tab created through NML TabManager.");
            Debug.Log("[ModernBox Rewrite] Paginated power tab created.");
        }

        private static void CreatePages()
        {
            string[] pageNames = { "Industry", "Units", "Bombs", "Equipment", "Settings" };
            for (int i = 0; i < pageNames.Length; i++)
            {
                string page = pageNames[i];
                Pages[page] = new List<GameObject>();
                ModernLocalization.Add("modernbox_page_" + page, page);
                ModernLocalization.Add("modernbox_page_" + page + "_description", "Show the " + page + " page.");
                PowerButton pageButton = PowerButtonCreator.CreateSimpleButton(
                    "modernbox_page_" + page,
                    () => ShowPage(page),
                    Resources.Load<Sprite>(PageIcon(page)),
                    _tab.transform,
                    new Vector2(72f + 36f * i, 90f));
                PowerButtonCreator.AddButtonToTab(pageButton, _tab, null);
            }
            CreateIndustryPage();
            CreateUnitPage();
            CreateBombPage();
            CreateEquipmentPage();
            CreateSettingsPage();
        }

        private static void CreateIndustryPage()
        {
            int index = 0;
            foreach (FactorySpec factory in ContentRegistry.Factories)
            {
                string key = factory.SettingKey;
                AddToggle("Industry", "modernbox_toggle_" + key, Resources.Load<Sprite>(FactoryIcon(factory.BuildingId)), FactoryToggleTitle(factory), FactoryToggleDescription(factory), index++, key);
            }
            AddToggle("Industry", "modernbox_toggle_NukeOption", Resources.Load<Sprite>("ui/Icons/Nuke"), "Nuclear Silos", "Toggle MissileSilo attacks.", index, "NukeOption");
        }

        private static void CreateUnitPage()
        {
            for (int i = 0; i < ContentRegistry.Units.Count; i++)
            {
                ModernUnitSpec unit = ContentRegistry.Units[i];
                AddGodPower("Units", "modernbox_spawn_" + unit.Id, Resources.Load<Sprite>(unit.IconPath), i);
            }
        }

        private static void CreateBombPage()
        {
            for (int i = 0; i < ContentRegistry.Bombs.Count; i++)
            {
                BombSpec bomb = ContentRegistry.Bombs[i];
                AddGodPower("Bombs", bomb.Id + "button", Resources.Load<Sprite>(bomb.IconPath), i);
            }
        }

        private static void CreateEquipmentPage()
        {
            AddToggle("Equipment", "modernbox_toggle_GunOption", Resources.Load<Sprite>("ui/Icons/firearm"), "Modern Guns", "Allow modern guns to be crafted.", 0, "GunOption");
            AddToggle("Equipment", "modernbox_toggle_PipeGunOption", Resources.Load<Sprite>("ui/Icons/lowfirearm"), "Pipe Guns", "Allow industrial pipe guns to be crafted.", 1, "PipeGunOption");
            AddToggle("Equipment", "modernbox_toggle_CyberwareOption", Resources.Load<Sprite>("ui/Icons/Cyberware"), "Cyberware", "Allow cyberware to be crafted.", 2, "CyberwareOption");
            AddToggle("Equipment", "modernbox_toggle_DrugsOption", Resources.Load<Sprite>("ui/Icons/Drugs"), "Drugs", "Allow M1 drug accessories to be crafted.", 3, "DrugsOption");
            AddToggle("Equipment", "modernbox_toggle_MIRVOption", Resources.Load<Sprite>("ui/Icons/MIRV"), "Nuclear Weapons", "Allow MIRV weapons to be crafted. MissileSystem vehicles always retain their built-in MIRV launcher.", 4, "MIRVOption");
            AddToggle("Equipment", "modernbox_toggle_IdeologiesOption", Resources.Load<Sprite>("ui/Icons/Ideologies"), "Ideologies", "Give every human, orc, elf, and dwarf one ideology by default and preserve inheritance.", 5, "IdeologiesOption");
            AddToggle("Equipment", "modernbox_toggle_namesOption", Resources.Load<Sprite>("ui/Icons/name_1"), "Human Names", "Use ModernBox human names.", 6, "namesOption");
            AddToggle("Equipment", "modernbox_toggle_othernamesOption", Resources.Load<Sprite>("ui/Icons/name_1"), "Other-Race Names", "Use ModernBox orc, elf, and dwarf names.", 7, "othernamesOption");
        }

        private static void CreateSettingsPage()
        {
            AddClick("Settings", "modernbox_open_info", Resources.Load<Sprite>("ui/icons/iconabout"), "About and Credits", "ModernBox information and credits.", 0, () => ModernBoxRuntime.Instance.OpenInfo());
            AddClick("Settings", "modernbox_open_settings", Resources.Load<Sprite>("ui/Icons/Reset"), "All Settings", "Open the complete ModernBox settings window.", 1, () => ModernBoxRuntime.Instance.OpenSettings());
            AddClick("Settings", "modernbox_open_diagnostics", Resources.Load<Sprite>("ui/Icons/FactoryJob"), "Diagnostics", "Open bounded developer diagnostics.", 2, () => ModernBoxRuntime.Instance.OpenDiagnostics());
            AddToggle("Settings", "modernbox_toggle_StartupAudio", Resources.Load<Sprite>("ui/Icons/TabText"), "Startup Audio", "Play the local M1 startup audio on the next launch.", 3, "StartupAudio");
            AddToggle("Settings", "modernbox_toggle_DeveloperDiagnostics", Resources.Load<Sprite>("ui/Icons/FactoryJob"), "Verbose Diagnostics", "Enable informational console diagnostics.", 4, "DeveloperDiagnostics");
        }

        private static void AddGodPower(string page, string id, Sprite icon, int index)
        {
            Vector2 position = GridPosition(index);
            PowerButton button = PowerButtonCreator.CreateGodPowerButton(id, icon, _tab.transform, position);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            Pages[page].Add(button.gameObject);
        }

        private static void AddToggle(string page, string id, Sprite icon, string title, string description, int index, string setting)
        {
            ModernLocalization.Add(id, title);
            ModernLocalization.Add(id + "_description", description);
            Vector2 position = GridPosition(index);
            string optionId = ModernBoxCatalog.Guid + "." + setting;
            ToggleSettings[id] = setting;
            SettingOptions[setting] = optionId;
            EnsureNativeOption(optionId, ModernBoxSettings.Get(setting));

            GodPower power = AssetManager.powers.get(id);
            if (power == null)
            {
                power = new GodPower { id = id };
                AssetManager.powers.add(power);
            }
            power.name = id;
            power.type = PowerActionType.PowerSpecial;
            power.rank = PowerRank.Rank0_free;
            power.path_icon = null;
            power.track_activity = false;
            power.toggle_name = optionId;
            power.toggle_action = ToggleNativeSetting;

            PowerButton button = PowerButtonCreator.CreateToggleButton(id, icon, _tab.transform, position, true);
            if (button == null) throw new InvalidOperationException("NML could not create native toggle button: " + id);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            Pages[page].Add(button.gameObject);
            SyncNativeToggle(setting, ModernBoxSettings.Get(setting), false);
        }

        private static void EnsureNativeOption(string optionId, bool value)
        {
            OptionAsset option = AssetManager.options_library.get(optionId);
            if (option == null)
            {
                option = new OptionAsset
                {
                    id = optionId,
                    type = OptionType.Bool,
                    default_bool = value,
                    has_locales = false
                };
                AssetManager.options_library.add(option);
            }
            PlayerOptionData data;
            if (!PlayerConfig.dict.TryGetValue(optionId, out data))
            {
                data = new PlayerOptionData(optionId) { boolVal = value };
                PlayerConfig.instance.data.add(data);
            }
            else
            {
                data.boolVal = value;
            }
        }

        private static void ToggleNativeSetting(string powerId)
        {
            string setting;
            if (!ToggleSettings.TryGetValue(powerId, out setting)) return;
            ModernBoxSettings.Set(setting, !ModernBoxSettings.Get(setting));
        }

        internal static void SyncNativeToggle(string setting, bool value, bool savePlayerConfig = true)
        {
            string optionId;
            if (!SettingOptions.TryGetValue(setting, out optionId)) return;
            EnsureNativeOption(optionId, value);
            PlayerConfig.setOptionBool(optionId, value);
            if (savePlayerConfig) PlayerConfig.saveData();
            if (PowerButtonSelector.instance != null) PowerButtonSelector.instance.checkToggleIcons();
        }

        private static void AddClick(string page, string id, Sprite icon, string title, string description, int index, UnityEngine.Events.UnityAction action)
        {
            ModernLocalization.Add(id, title);
            ModernLocalization.Add(id + "_description", description);
            Vector2 position = GridPosition(index);
            PowerButton button = PowerButtonCreator.CreateSimpleButton(id, action, icon, _tab.transform, position);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            Pages[page].Add(button.gameObject);
        }

        private static Vector2 GridPosition(int index)
        {
            // The 0.51.2 desktop toolbar is wide enough for the full M1 control row.
            // Keeping all 14 Industry controls on one row prevents the last factory,
            // silo, and labeled controls button from being clipped below the screen.
            const int columns = 16;
            return new Vector2(72f + 36f * (index % columns), 36f - 36f * (index / columns));
        }

        private static void ShowPage(string page)
        {
            foreach (KeyValuePair<string, List<GameObject>> pair in Pages)
                foreach (GameObject item in pair.Value)
                    if (item != null) item.SetActive(pair.Key == page);
        }

        private static string FactoryIcon(string buildingId)
        {
            if (buildingId == "ModernBarracks") return "actors/Soldier/walk_0";
            if (buildingId == "AirFactory") return "ui/Icons/MIRVBomber";
            if (buildingId == "AirshipFactory") return "ui/Icons/Airship";
            if (buildingId == "HelicopterFactory") return "ui/Icons/Heli";
            if (buildingId == "BoiFactory") return "ui/Icons/Nuke";
            return "ui/Icons/" + buildingId.Replace("Factory", string.Empty);
        }

        private static string FactoryToggleTitle(FactorySpec factory)
        {
            switch (factory.BuildingId)
            {
                case "ModernBarracks": return "Modern Militaries";
                case "AirFactory": return "MIRV Bomber Factories";
                case "BoiFactory": return "Missile System Factories";
                default: return ActorsAndBuildingsRegistry.FriendlyName(factory.BuildingId) + " Production";
            }
        }

        private static string FactoryToggleDescription(FactorySpec factory)
        {
            return "Toggle whether human kingdoms produce " + string.Join(" / ", factory.UnitIds) + " at " + ActorsAndBuildingsRegistry.FriendlyName(factory.BuildingId) + ".";
        }

        private static string PageIcon(string page)
        {
            switch (page)
            {
                case "Industry": return "ui/Icons/Factories";
                case "Units": return "ui/Icons/Tank";
                case "Bombs": return "ui/Icons/MOAB";
                case "Equipment": return "ui/Icons/firearm";
                default: return "ui/Icons/Reset";
            }
        }
    }
}
