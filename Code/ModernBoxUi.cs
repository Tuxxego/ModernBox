using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class ModernBoxUi
    {
        private const string TabId = "modernbox_rewrite_tab";
        private static readonly Dictionary<string, List<GameObject>> Pages = new Dictionary<string, List<GameObject>>(StringComparer.Ordinal);
        private static readonly List<GameObject> HubControls = new List<GameObject>();
        private static readonly Dictionary<string, string> ToggleSettings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> SettingOptions = new Dictionary<string, string>(StringComparer.Ordinal);
        private static PowersTab _tab;
        private static GameObject _backButton;
        private static string _activePage;
        private static bool _showingHub;

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
            ShowHub(false);
            ModernLocalization.Apply();
            ModernBoxDiagnostics.Info("ModernBox animated subtab power tab created through NML TabManager.");
            Debug.Log("[ModernBox Rewrite] Animated subtab power tab created.");
        }

        private static void CreatePages()
        {
            string[] pageNames = { "Progression", "Armies", "Units", "Equipment", "Bombs", "Invasions", "Settings" };
            for (int i = 0; i < pageNames.Length; i++)
            {
                string page = pageNames[i];
                Pages[page] = new List<GameObject>();
                ModernLocalization.Add("modernbox_page_" + page, page);
                ModernLocalization.Add("modernbox_page_" + page + "_description", "Opens the " + page + " tab.");
                PowerButton pageButton = PowerButtonCreator.CreateSimpleButton(
                    "modernbox_page_" + page,
                    () => ShowPage(page),
                    Resources.Load<Sprite>(PageIcon(page)),
                    _tab.transform,
                    GridPosition(i));
                PowerButtonCreator.AddButtonToTab(pageButton, _tab, null);
                HubControls.Add(pageButton.gameObject);
            }

            ModernLocalization.Add("modernbox_page_back", "Back");
            ModernLocalization.Add("modernbox_page_back_description", "Return to the M2 category hub.");
            PowerButton backButton = PowerButtonCreator.CreateSimpleButton(
                "modernbox_page_back",
                () => ShowHub(true),
                Resources.Load<Sprite>("ui/Icons/Reset"),
                _tab.transform,
                new Vector2(36f, 36f));
            PowerButtonCreator.AddButtonToTab(backButton, _tab, null);
            _backButton = backButton.gameObject;

            CreateProgressionPage();
            CreateArmyPage();
            CreateUnitPage();
            CreateEquipmentPage();
            CreateBombPage();
            CreateInvasionPage();
            CreateSettingsPage();
        }

        private static void CreateProgressionPage()
        {
            AddToggle("Progression", "modernbox_toggle_ProgressionOption", Resources.Load<Sprite>("ui/Icons/Renaissance"), "Standard Era Progression", "The whole world advances together after 50-200 world years per era. Buildings, armies, factories, equipment, and civilization visuals are restricted to the current world era.", 0, "ProgressionOption");
            AddToggle("Progression", "modernbox_toggle_ConstructionOption", Resources.Load<Sprite>("ui/Icons/Skyscraper"), "M2 Construction", "Allow all four civilizations to build M2 civilian structures, factories, and era upgrades.", 1, "ConstructionOption");
        }

        private static void CreateArmyPage()
        {
            int index = 0;
            AddToggle("Armies", "modernbox_toggle_FactoriesOption", Resources.Load<Sprite>("ui/Icons/Factories"), "Army and Factory Production", "Master toggle for M2 military production.", index++, "FactoriesOption");
            foreach (FactorySpec factory in ContentRegistry.Factories)
            {
                string key = factory.SettingKey;
                AddToggle("Armies", "modernbox_toggle_" + key, Resources.Load<Sprite>(FactoryIcon(factory.BuildingId)), FactoryToggleTitle(factory), FactoryToggleDescription(factory), index++, key);
            }
            AddToggle("Armies", "modernbox_toggle_NukeOption", Resources.Load<Sprite>("ui/Icons/Nuke"), "Wartime Nuclear Silos", "Allow 32-second MissileSilo launches only against kingdoms currently at war. Off by default.", index, "NukeOption");
        }

        private static void CreateUnitPage()
        {
            int index = 0;
            for (int i = 0; i < ContentRegistry.Units.Count; i++)
            {
                ModernUnitSpec unit = ContentRegistry.Units[i];
                if (unit.Role == M2UnitRole.Creature) continue;
                AddGodPower("Units", "modernbox_spawn_" + unit.Id, Resources.Load<Sprite>(unit.IconPath), index++);
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
            AddToggle("Equipment", "modernbox_toggle_EquipmentOption", Resources.Load<Sprite>("ui/Icons/Suit"), "M2 Equipment", "Master toggle for M2 equipment crafting.", 0, "EquipmentOption");
            AddToggle("Equipment", "modernbox_toggle_GunOption", Resources.Load<Sprite>("ui/Icons/firearm"), "Era Weapons and Armor", "Allow Renaissance, industrial, modern, and future equipment to be crafted at the matching culture era.", 1, "GunOption");
            AddToggle("Equipment", "modernbox_toggle_PipeGunOption", Resources.Load<Sprite>("ui/Icons/lowfirearm"), "Pipe Weapons", "Allow Renaissance pipe weapons to be crafted.", 2, "PipeGunOption");
            AddToggle("Equipment", "modernbox_toggle_CyberwareOption", Resources.Load<Sprite>("ui/Icons/SolarPoweredCyberBody"), "Cyberware", "Allow Sandevistan and TurboBooster to be crafted.", 3, "CyberwareOption");
            AddToggle("Equipment", "modernbox_toggle_DrugsOption", Resources.Load<Sprite>("ui/Icons/Drugs"), "Drugs", "Allow Meth and Crack to be crafted.", 4, "DrugsOption");
            AddToggle("Equipment", "modernbox_toggle_MIRVOption", Resources.Load<Sprite>("ui/Icons/MIRV"), "MIRV Crafting", "Allow MIRV and MIRVBomb crafting. MissileSystem combat remains independent.", 5, "MIRVOption");
            AddToggle("Equipment", "modernbox_toggle_IdeologiesOption", Resources.Load<Sprite>("ui/Icons/Ideologies"), "M2 Ideologies", "Automatically assign Dynastic, Mercantile, Peoplewoven, Martial, or Chaosvolt and preserve inheritance.", 6, "IdeologiesOption");
            AddToggle("Equipment", "modernbox_toggle_namesOption", Resources.Load<Sprite>("ui/Icons/name_1"), "M2 Names", "Use M2 race and military name generators.", 7, "namesOption");
        }

        private static void CreateInvasionPage()
        {
            AddToggle("Invasions", "modernbox_toggle_AutomaticInvasionsOption", Resources.Load<Sprite>("ui/Icons/Vatican"), "Automatic Invasions", "Allow bounded Hashbrown and Vatican automatic invasion checks. Off by default.", 0, "AutomaticInvasionsOption");
            int index = 1;
            foreach (ModernUnitSpec unit in ContentRegistry.Units)
            {
                if (unit.Role != M2UnitRole.Creature) continue;
                AddGodPower("Invasions", "modernbox_spawn_" + unit.Id, Resources.Load<Sprite>(unit.IconPath), index++);
            }
        }

        private static void CreateSettingsPage()
        {
            AddClick("Settings", "modernbox_open_diagnostics", Resources.Load<Sprite>("ui/Icons/FactoryJob"), "Diagnostics", "Open bounded developer diagnostics.", 0, () => ModernBoxRuntime.Instance.OpenDiagnostics());
            AddToggle("Settings", "modernbox_toggle_StartupAudio", Resources.Load<Sprite>("ui/Icons/TabText"), "Startup Audio", "Play the bundled ModernBox startup audio on the next launch.", 1, "StartupAudio");
            AddToggle("Settings", "modernbox_toggle_DeveloperDiagnostics", Resources.Load<Sprite>("ui/Icons/FactoryJob"), "Verbose Diagnostics", "Enable informational console diagnostics.", 2, "DeveloperDiagnostics");
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
            power.sprite_icon = icon;
            power.ignore_cursor_icon = true;
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
            const int columns = 16;
            return new Vector2(72f + 36f * (index % columns), 36f - 36f * (index / columns));
        }

        private static void ShowHub(bool animate)
        {
            if (_showingHub) return;

            foreach (GameObject control in HubControls)
                if (control != null) control.SetActive(true);

            foreach (KeyValuePair<string, List<GameObject>> pair in Pages)
                foreach (GameObject item in pair.Value)
                    if (item != null) item.SetActive(false);

            if (_backButton != null) _backButton.SetActive(false);
            _activePage = null;
            _showingHub = true;
            AnimateSubtab(animate);
        }

        private static void ShowPage(string page)
        {
            if (!Pages.ContainsKey(page) || (!_showingHub && string.Equals(_activePage, page, StringComparison.Ordinal))) return;

            foreach (GameObject control in HubControls)
                if (control != null) control.SetActive(false);

            foreach (KeyValuePair<string, List<GameObject>> pair in Pages)
                foreach (GameObject item in pair.Value)
                    if (item != null) item.SetActive(pair.Key == page);

            if (_backButton != null) _backButton.SetActive(true);
            _activePage = page;
            _showingHub = false;
            AnimateSubtab(true);
        }

        private static void AnimateSubtab(bool animate)
        {
            if (!animate || _tab == null) return;

            _tab.transform.DOKill(true);
            _tab.transform.localScale = new Vector3(0.2f, 0.9f, 0.9f);
            _tab.transform.DOScale(Vector3.one, PowersTab.scale_time).SetEase(Ease.OutBack);
            MusicBox.playSoundUI("event:/SFX/UI/ThumbnailsSlide");
        }

        private static string FactoryIcon(string buildingId)
        {
            if (buildingId == "$era_barracks$") return "ui/Icons/Soldier";
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
                case "$era_barracks$": return "Era Barracks Armies";
                case "AirFactory": return "MIRV Bomber Factories";
                case "BoiFactory": return "Missile System Factories";
                default: return ActorsAndBuildingsRegistry.FriendlyName(factory.BuildingId) + " Production";
            }
        }

        private static string FactoryToggleDescription(FactorySpec factory)
        {
            string units = factory.UnitIds.Length == 0 ? "race and era role-table units" : string.Join(" / ", factory.UnitIds);
            return "Toggle whether civilized kingdoms produce " + units + " at " + ActorsAndBuildingsRegistry.FriendlyName(factory.BuildingId) + ".";
        }

        private static string PageIcon(string page)
        {
            switch (page)
            {
                case "Progression": return "ui/Icons/Renaissance";
                case "Armies": return "ui/Icons/Factories";
                case "Units": return "ui/Icons/Tank";
                case "Bombs": return "ui/Icons/MOAB";
                case "Equipment": return "ui/Icons/firearm";
                case "Invasions": return "ui/Icons/Vatican";
                default: return "ui/Icons/Reset";
            }
        }
    }
}
