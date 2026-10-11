using System;
using System.Collections;
using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModernBoxM1Rewrite
{
    // The M1 tab: the same buttons, names, descriptions and spots as Buttonz.cs.
    internal static class ModernBoxUi
    {
        private const string TabId = "Tab_ModernBox";
        private static readonly Dictionary<string, string> ToggleSettings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> SettingOptions = new Dictionary<string, string>(StringComparer.Ordinal);
        private static PowersTab _tab;

        internal static void BeginCreate(MonoBehaviour host)
        {
            host.StartCoroutine(CreateWhenReady());
        }

        private static IEnumerator CreateWhenReady()
        {
            while (PowerButtonSelector.instance == null || CanvasMain.instance == null) yield return null;
            ModernLocalization.Add("modernbox_tab", "ModernBox");
            ModernLocalization.Add("modernbox_tab_description", "Guns, Vehicles, Drugs, Casinos, MIRVs. Welcome to the Modern Age.");
            ModernLocalization.Add("Tuxxego_mod_creator", "Made By Tuxxego");
            _tab = TabManager.CreateTab(TabId, "modernbox_tab", "modernbox_tab_description", Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare"), "Tuxxego_mod_creator");
            if (_tab == null)
            {
                ModernBoxDiagnostics.Error("Could not make the ModernBox tab.");
                yield break;
            }
            M1Windows.CreateAll();
            CreateButtons();
            ModernLocalization.Apply();
            while (_tab.parentObj == null) yield return null;
            _tab.recalc();

            while (!Config.game_loaded) yield return null;
            yield return new WaitForSeconds(1f);
            // Main.Awake: these windows open at startup.
            M1Windows.Ready();
            if (M1Windows.NcmsInstalled()) M1Windows.Show("NCMSWindow");
            if (ModernBoxSettings.IsNewVersion) M1Windows.Show("SaveSystemWindow");
        }

        // Buttons go in column by column (top, then bottom); the game lays them out in that
        // order 36 pixels apart from x 72, like the original's fixed positions.
        private static void CreateButtons()
        {
            // x 72
            AddClick("about", "ui/icons/iconabout", "About", "Learn about ModernBox.", () => M1Windows.Show("InfoWindow"));
            AddClick("discord_server", "ui/Icons/DiscordServer", "Discord Server", "Click this to join the ModernBox Discord server!", M1Windows.OpenDiscord);
            // x 108
            AddClick("ResetSettings", "ui/Icons/Reset", "Reset to defaults", "Resets ALL saved settings to their default values.", ResetToDefaults);
            AddClick("credits", "ui/icons/iconabout", "Credits", "All the people behind ModernBox and more!", () => M1Windows.Show("CreditsWindow"));
            // x 144
            AddToggle("other_names_toggle", "ui/Icons/tabIconModernWarfare", "Modern Names for Other Races.", "Enable or Disable First and Last names for other races..", "othernamesOption");
            AddToggle("names_toggle", "ui/Icons/name_1", "Modern Names", "Enable or Disable Modern Names.", "namesOption");
            // x 180
            AddToggle("MIRV_toggle", "ui/Icons/MIRV", "MIRVs", "Toggle MIRVs from being developed (this won't remove existing MIRVS)", "MIRVOption");
            AddToggle("STRONGMIRV_toggle", "ui/Icons/MIRV_nuke", "Nuclear MIRVs", "Toggle Nuclear MIRVs from being developed (this won't remove existing Nuclear MIRVS)", "STRONGMIRVOption");
            // x 216
            Transform bannerHolder = AddSpace();
            Transform bannerLeft = AddToggle("Soldier_toggle", "actors/Soldier/walk_0", "Modern Militaries", "Toggles if modern soldiers can be enlisted.", "SoldierOption");
            // x 252 - 324, the TabText picture sits here
            AddSpace(6);
            // x 360
            AddSpace();
            Transform bannerRight = AddToggle("Nuke_toggle", "effects/projectiles/NUKER/0", "Toggle Nuke Silos", "Toggles if kingdoms can nuke each other.", "NukeOption");
            // x 396 - 540
            AddBomb("Mini");
            AddBomb("Cobalt");
            AddBomb("MOAB");
            AddBomb("Xenium");
            AddBomb("Ultron");
            AddBomb("Proton");
            AddBomb("Death");
            AddBomb("Jupiter");
            AddBomb("Random");
            AddBomb("Eraser");
            // x 576
            AddSpace(2);
            // x 612 - 900: a spawn button over each factory toggle
            AddGodPower("heli_spawn", "ui/Icons/Heli", "Helicopter Spawn", "Spawn a Helicopter");
            AddToggle("HelicopterFactory_toggle", "ui/Icons/Heli", "Helicopter Factories", "Toggles if factories make Helicopters", "HeliOption");
            AddGodPower("spawn_airship", "ui/Icons/Airship", "Spawn Zeppelin", "Spawn a Zeppelin.");
            AddToggle("AirshipFactory_toggle", "ui/Icons/Airship", "Airship Factories", "Toggles if factories make Airships", "AirshipOption");
            AddGodPower("spawn_tank", "ui/Icons/Tank", "Spawn Tank", "Spawn a tank.");
            AddToggle("TankFactory_toggle", "ui/Icons/Tank", "Tank Factories", "Toggles if factories make Tanks", "TankOption");
            AddGodPower("spawn_mirv_bomber", "ui/Icons/MIRVBomber", "Spawn MIRV Bomber", "Spawn a MIRV Bomber.");
            AddToggle("AirFactory_toggle", "ui/Icons/MIRVBomber", "MIRV Bomber Factories", "Toggles if factories make MIRV Bombers", "MIRVBomberOption");
            AddGodPower("spawn_humvee", "ui/Icons/Humvee", "Spawn Humvee", "Spawn a Humvee.");
            AddToggle("HumveeFactory_toggle", "ui/Icons/Humvee", "Humvee Factories", "Toggles if factories make Humvees", "HumveeOption");
            AddGodPower("spawn_F22", "ui/Icons/F22", "Spawn F22", "Spawn a F22.");
            AddToggle("F22Factory_toggle", "ui/Icons/F22", "F22 Factories", "Toggles if factories make F22s", "F22Option");
            AddGodPower("spawn_MissileSystem", "actors/MissileSystem/swim_1", "Spawn Artillery", "Spawn a Missile System.");
            AddToggle("BoiFactory_toggle", "actors/MissileSystem/swim_1", "Missile System Factories", "Toggles if factories make Missile Systems", "BoiOption");
            AddGodPower("gunship_spawn", "ui/Icons/Gunship", "Gunship Spawn", "Spawn a Gunship");
            AddToggle("GunshipFactory_toggle", "ui/Icons/Gunship", "Gunship Factories", "Toggles if factories make Gunships", "GunshipOption");
            AddGodPower("spawn_Railgun", "ui/Icons/Railgun", "Spawn Railgun", "Spawn a Railgun");
            AddToggle("RailgunFactory_toggle", "ui/Icons/Railgun", "Railgun Factories", "Toggles if factories make Railguns", "RailgunOption");
            // x 936
            AddSpace(2);
            // x 972
            AddToggle("Ideologies_toggle", "ui/Icons/Ideologies", "Toggle Ideologies", "bruh", "IdeologiesOption");
            AddToggle("Drones_Toggle", "ui/Icons/Drone", "Drones", "Toggle if kingdoms can create Drones.", "DronesOption");
            // x 1008
            AddToggle("PipeGun_toggle", "ui/Icons/lowfirearm", "Toggle Pipe Guns.", "Toggle Pipe Guns from being developed (this won't remove existing Pipe Guns already made). You're welcome LonelyFear.", "PipeGunOption");
            AddToggle("Gun_toggle", "ui/Icons/firearm", "Toggle Guns.", "Toggle Guns from being developed (this won't remove existing Guns already made).", "GunOption");
            // x 1044
            AddToggle("Cyberware_toggle", "ui/Icons/Cyberware", "Cyberware", "Toggle Cyberware from being developed (this won't remove existing Cyberware)", "CyberwareOption");
            AddToggle("Drugs_toggle", "ui/Icons/Drugs", "Drugs", "Toggle Drugs from being developed (this won't remove existing Drugs already made)", "DrugsOption");
            // x 1080
            AddToggle("Devmode", "ui/Icons/tabIconModernWarfare", "Toggle Developer Mode", "Secret stuff", "Developer_Mode");
            // Not in the original: 0.51.2 has no culture tech window.
            AddClick("tech", "ui/Icons/iconCulture", "Tech", "See how far each culture has researched and when ModernBox things unlock.", M1Windows.ShowTech);

            AddTabText(bannerHolder, bannerLeft, bannerRight);
        }

        // Main.resetToDefaults: the defaults are saved, but the buttons only change after a restart.
        private static void ResetToDefaults()
        {
            M1Windows.Show("DefaultSettingsWindow");
            ModernBoxSettings.ResetSaved();
        }

        private static void AddBomb(string bombId)
        {
            BombSpec spec = ContentRegistry.Bombs.Find(candidate => candidate.Id == bombId);
            if (spec == null) AddSpace();
            else AddGodPower(bombId + "button", spec.IconPath, spec.DisplayName, spec.Description);
        }

        private static void AddGodPower(string id, string icon, string title, string description)
        {
            GodPower power = AssetManager.powers.get(id);
            if (power == null)
            {
                AddSpace();
                return;
            }
            ModernLocalization.Add(id, title);
            ModernLocalization.Add(id + "_description", description);
            Sprite sprite = Resources.Load<Sprite>(icon);
            if (sprite == null && !string.IsNullOrEmpty(power.path_icon)) sprite = Resources.Load<Sprite>(power.path_icon);
            PowerButton button = PowerButtonCreator.CreateGodPowerButton(id, sprite, _tab.transform, Vector2.zero);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            // The game greys out buttons with "Button" in their name.
            if (button.icon != null) button.icon.color = Color.white;
            Image background = button.GetComponent<Image>();
            if (background != null) background.color = Color.white;
        }

        private static void AddClick(string id, string icon, string title, string description, UnityAction action)
        {
            string buttonId = "modernbox_" + id;
            ModernLocalization.Add(buttonId, title);
            ModernLocalization.Add(buttonId + "_description", description);
            PowerButton button = PowerButtonCreator.CreateSimpleButton(buttonId, action, Resources.Load<Sprite>(icon), _tab.transform, Vector2.zero);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
        }

        private static Transform AddToggle(string id, string icon, string title, string description, string setting)
        {
            string powerId = "modernbox_" + id;
            ModernLocalization.Add(powerId, title);
            ModernLocalization.Add(powerId + "_description", description);
            string optionId = ModernBoxCatalog.Guid + "." + setting;
            ToggleSettings[powerId] = setting;
            SettingOptions[setting] = optionId;
            EnsureOption(optionId, ModernBoxSettings.Get(setting));

            Sprite sprite = Resources.Load<Sprite>(icon);
            GodPower power = AssetManager.powers.get(powerId);
            if (power == null)
            {
                power = new GodPower { id = powerId };
                AssetManager.powers.add(power);
            }
            power.name = powerId;
            power.type = PowerActionType.PowerSpecial;
            power.rank = PowerRank.Rank0_free;
            power.path_icon = null;
            power.sprite_icon = sprite;
            power.ignore_cursor_icon = true;
            power.track_activity = false;
            power.toggle_name = optionId;
            power.toggle_action = OnToggle;

            PowerButton button = PowerButtonCreator.CreateToggleButton(powerId, sprite, _tab.transform, Vector2.zero, true);
            if (button == null)
            {
                return AddSpace();
            }
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            SyncNativeToggle(setting, ModernBoxSettings.Get(setting), false);
            return button.transform;
        }

        private static void OnToggle(string powerId)
        {
            string setting;
            if (!ToggleSettings.TryGetValue(powerId, out setting)) return;
            bool value = !ModernBoxSettings.Get(setting);
            M1Toggles.Toggle(setting, value);
            SyncNativeToggle(setting, value);
        }

        private static Transform AddSpace(int slots = 1)
        {
            Transform last = null;
            for (int i = 0; i < slots; i++)
            {
                GameObject space = new GameObject("_space", typeof(RectTransform));
                space.transform.SetParent(_tab.transform, false);
                ((RectTransform)space.transform).sizeDelta = new Vector2(32f, 32f);
                last = space.transform;
            }
            return last;
        }

        // Buttonz.cs: the TabText picture, 200x100, centred at x 288 on the top row, behind
        // the buttons to its right. It hangs off the empty spot at x 216 so it doesn't take a
        // slot in the tab, and is drawn before every button after that spot. The game never
        // moves empty spots, so it is placed between the buttons below it at x 216 and x 360.
        private static void AddTabText(Transform holder, Transform left, Transform right)
        {
            if (holder == null || left == null || right == null) return;
            GameObject picture = new GameObject("LargeImage", typeof(RectTransform));
            picture.transform.SetParent(holder, false);
            Image image = picture.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/Icons/TabText");
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200f, 100f);
            TabTextSpot spot = picture.AddComponent<TabTextSpot>();
            spot.Tab = _tab.transform;
            spot.Left = left;
            spot.Right = right;
        }

        private static void EnsureOption(string optionId, bool value)
        {
            OptionAsset option = AssetManager.options_library.get(optionId);
            if (option == null)
            {
                option = new OptionAsset { id = optionId, type = OptionType.Bool, default_bool = value, has_locales = false };
                AssetManager.options_library.add(option);
            }
            PlayerOptionData data;
            if (!PlayerConfig.dict.TryGetValue(optionId, out data))
            {
                data = new PlayerOptionData(optionId) { boolVal = value };
                PlayerConfig.instance.data.add(data);
            }
            else data.boolVal = value;
        }

        private static void SyncNativeToggle(string setting, bool value, bool savePlayerConfig = true)
        {
            string optionId;
            if (!SettingOptions.TryGetValue(setting, out optionId)) return;
            EnsureOption(optionId, value);
            PlayerConfig.setOptionBool(optionId, value);
            if (savePlayerConfig) PlayerConfig.saveData();
            if (PowerButtonSelector.instance != null) PowerButtonSelector.instance.checkToggleIcons();
        }
    }

    // Keeps the TabText picture halfway between its two buttons, on the top row.
    internal sealed class TabTextSpot : MonoBehaviour
    {
        // PowersTab.sortButtons puts the top row at y 32.
        private const float TopRowY = 32f;
        internal Transform Tab;
        internal Transform Left;
        internal Transform Right;

        private void LateUpdate()
        {
            if (Tab == null || Left == null || Right == null) return;
            float x = (Left.localPosition.x + Right.localPosition.x) / 2f;
            transform.position = Tab.TransformPoint(new Vector3(x, TopRowY, 0f));
        }
    }
}
