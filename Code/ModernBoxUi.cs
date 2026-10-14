using System;
using System.Collections;
using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModernBoxM2Rewrite
{
    // The M2 tab. Same buttons and spots as the original Buttonz.cs.
    internal static class ModernBoxUi
    {
        private const string TabId = "Tab_ModernBox";
        private static readonly Dictionary<string, string> ToggleSettings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> SettingOptions = new Dictionary<string, string>(StringComparer.Ordinal);
        private static PowersTab _tab;
        private static Transform _tabTextLeft;
        private static Transform _tabTextRight;

        internal static void BeginCreate(MonoBehaviour host)
        {
            host.StartCoroutine(CreateWhenReady());
        }

        private static IEnumerator CreateWhenReady()
        {
            while (PowerButtonSelector.instance == null || CanvasMain.instance == null) yield return null;
            ModernLocalization.Add("modernbox_tab", "M2");
            ModernLocalization.Add("modernbox_tab_description", "Guns, Vehicles, Drugs, Casinos, MIRVs, and SPACE. Welcome to the Modern Age.");
            ModernLocalization.Add("Tuxxego_mod_creator", "Made By Tuxxego");
            _tab = TabManager.CreateTab(TabId, "modernbox_tab", "modernbox_tab_description", Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare"), "Tuxxego_mod_creator");
            if (_tab == null)
            {
                ModernBoxDiagnostics.Error("Could not make the M2 tab.");
                yield break;
            }
            M2Windows.CreateAll();
            CreateButtons();
            ModernLocalization.Apply();
            while (_tab.parentObj == null) yield return null;
            _tab.recalc();
            Debug.Log("[ModernBox] tab ready");

            if (ModernBoxSettings.IsNewVersion)
            {
                while (!Config.game_loaded) yield return null;
                yield return new WaitForSeconds(1f);
                M2Windows.Show("SaveSystemWindow");
            }
        }

        // Buttons go in column by column (top then bottom), the game lays them out in that order.
        private static void CreateButtons()
        {
            // x 72
            AddClick("galaxy", "ui/Icons/Galaxy", "Star Map", "View a map of everything.", M2Hooks.OpenStarMap);
            AddClick("what", "ui/Icons/wat", "Coming soon", "COMING SOON", null);
            // x 108
            AddGodPowerIfThere("arrowleft2", "ui/Icons/Arrowleft", "Choose Units", "Select the units you want to send to space.");
            AddGodPowerIfThere("arrowleft3", "ui/Icons/Arrowright", "Land Units", "Land Units on your planet.");
            // x 144
            AddSpace(2);
            // x 180
            AddClick("about", "ui/Icons/Guide", "Guide", "Read the guide on how ModernBox works.", () => M2Windows.Show("GuideWindow"));
            AddClick("discord_server", "ui/Icons/DiscordServer", "Discord Server", "Click this to join the ModernBox Discord server!", M2Windows.OpenDiscord);
            // x 216
            AddClick("ResetSettings", "ui/Icons/Reset", "Reset to defaults", "Resets ALL saved settings to their default values.", ResetToDefaults);
            AddClick("credits", "ui/icons/iconabout", "Credits", "All the people behind ModernBox and more!", () => M2Windows.Show("CreditsWindow"));
            // x 252
            AddToggle("other_names_toggle", "ui/Icons/tabIconModernWarfare", "Modern Names for Other Races.", "Enable or Disable First and Last names for other races..", "othernamesOption");
            AddToggle("names_toggle", "ui/Icons/name_1", "Modern Names", "Enable or Disable Modern Names.", "namesOption");
            // x 288
            _tabTextLeft = AddClick("MedievalWindow", "ui/Icons/Bomber", "Medieval Units Menu", "Tis but a scratch", () => M2Windows.Show("Medieval Unit Spawner"));
            AddClick("TechWindow", "ui/Icons/Renaissance", "Technologies", "See the era and techs of each culture.", M2TechWindow.Open);
            // x 324 - 468, the TabText picture sits here
            AddSpace(10);
            // x 504
            _tabTextRight = AddBomb("Ultron", "ui/Icons/Ultron", "Ultron Bomb", "WOOOAH");
            AddToggle("Nuke_toggle", "effects/projectiles/NUKER/0", "Toggle Nuke Silos", "(GREEN MEANS ON, GREY IS OFF) Toggles if kingdoms can nuke each other.", "NukeOption");
            // x 540
            AddBomb("Mini", "ui/Icons/Mini", "Mini Nuke", "Small nukes, great for minor scuffles.");
            AddBomb("Cobalt", "ui/Icons/Cobalt", "Cobalt Bomb", "Small Mushroom but huge radius, watch out with this one.");
            // x 576
            AddBomb("MOAB", "ui/Icons/MOAB", "Super-Nuke", "Also known as the 'Lag Bomb'.");
            AddBomb("Xenium", "ui/Icons/Xeno", "Xenium Bomb", "You thought the ultron bomb was big? This thing is HUGE.");
            // x 612
            AddBomb("Death", "ui/Icons/Death", "Death Bomb", "Such an original name.");
            AddBomb("Jupiter", "ui/Icons/Jupiter", "Jupiter Bomb", "The new monster.");
            // x 648
            AddBomb("Random", "ui/Icons/wat", "Random Bomb", "You could be dropping a proton bomb, or a mini nuke, it's random!");
            AddBomb("Eraser", "ui/Icons/Eraser", "Eraser Bomb", "also known as the overcompensating bomb.");
            // x 684, the alien only shows up sometimes
            // WorldBox reseeds UnityEngine.Random from the date at startup, so use a
            // fresh clock-seeded System.Random like the original did.
            if (new System.Random().NextDouble() <= 0.2) AddGodPower("modernbox_spawn_Xiexel", "ui/Icons/alien", "ALIEN EMOJI", "OH NO YOU GOT AN EASTER EGG!");
            else AddSpace(1);
            AddClick("BombMenu", "ui/Icons/Bomber", "Bomb Menu", "Tux and Dank got bored and added a lot of extra bombs....", () => M2Windows.Show("EXTRA BOMBS"));
            // x 720 - 1224 were the factory buttons, those are commented out in M2
            AddSpace(30);
            // x 1260
            AddToggle("Devmode", "ui/Icons/tabIconModernWarfare", "Toggle Developer Mode", "Secret stuff", "Developer_Mode");
            AddSpace(1);
            // x 1296
            AddGodPower("modernbox_spawn_Assimilatus", "ui/Icons/AssimilatusIcon", "Cyber Boss", "Do not spawn in a 69km radius from the closest city, DO NOT, THIS IS NOT REVERSE PSYCHOLOGY, I SWEAR, DO NOT :3");
            AddGodPower("modernbox_spawn_Cocytuswalker", "ui/Icons/Walker_TitanIcon", "Ice Walker Boss", "He came to worldbox to get away from Shinji, he do not trust what Shinji would do to him if he falls into a coma");

            AddTabText();
        }

        private static void ResetToDefaults()
        {
            M2Windows.Show("DefaultSettingsWindow");
            ModernBoxSettings.Reset();
            if (PowerButtonSelector.instance != null) PowerButtonSelector.instance.checkToggleIcons();
        }

        private static Transform AddBomb(string bombId, string icon, string title, string description)
        {
            return AddGodPower(bombId + "button", icon, title, description);
        }

        private static void AddGodPowerIfThere(string id, string icon, string title, string description)
        {
            if (AssetManager.powers.get(id) != null) AddGodPower(id, icon, title, description);
            else AddSpace(1);
        }

        private static Transform AddGodPower(string id, string icon, string title, string description)
        {
            GodPower power = AssetManager.powers.get(id);
            if (power == null)
            {
                AddSpace(1);
                return null;
            }
            ModernLocalization.Add(id, title);
            ModernLocalization.Add(id + "_description", description);
            PowerButton button = PowerButtonCreator.CreateGodPowerButton(id, LoadIcon(icon, power), _tab.transform, Vector2.zero);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            // The game greys out buttons with "Button" in their name.
            if (button.icon != null) button.icon.color = Color.white;
            Image background = button.GetComponent<Image>();
            if (background != null) background.color = Color.white;
            return button.transform;
        }

        private static Transform AddClick(string id, string icon, string title, string description, UnityAction action)
        {
            string buttonId = "modernbox_" + id;
            ModernLocalization.Add(buttonId, title);
            ModernLocalization.Add(buttonId + "_description", description);
            PowerButton button = PowerButtonCreator.CreateSimpleButton(buttonId, action, Resources.Load<Sprite>(icon), _tab.transform, Vector2.zero);
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            return button.transform;
        }

        private static void AddToggle(string id, string icon, string title, string description, string setting)
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
            power.toggle_action = ToggleSetting;

            PowerButton button = PowerButtonCreator.CreateToggleButton(powerId, sprite, _tab.transform, Vector2.zero, true);
            if (button == null)
            {
                AddSpace(1);
                return;
            }
            PowerButtonCreator.AddButtonToTab(button, _tab, null);
            SyncNativeToggle(setting, ModernBoxSettings.Get(setting), false);
        }

        private static void AddSpace(int slots)
        {
            for (int i = 0; i < slots; i++)
            {
                GameObject space = new GameObject("_space", typeof(RectTransform));
                space.transform.SetParent(_tab.transform, false);
                ((RectTransform)space.transform).sizeDelta = new Vector2(32f, 32f);
            }
        }

        private static void AddTabText()
        {
            GameObject picture = new GameObject("LargeImage", typeof(RectTransform));
            picture.transform.SetParent(_tab.transform, false);
            Image image = picture.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/Icons/TabText");
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200f, 100f);
            TabTextSpot spot = picture.AddComponent<TabTextSpot>();
            spot.Left = _tabTextLeft;
            spot.Right = _tabTextRight;
        }

        private static Sprite LoadIcon(string icon, GodPower power)
        {
            Sprite sprite = Resources.Load<Sprite>(icon);
            if (sprite == null && power != null && !string.IsNullOrEmpty(power.path_icon)) sprite = Resources.Load<Sprite>(power.path_icon);
            return sprite;
        }

        private static void EnsureOption(string optionId, bool value)
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

        private static void ToggleSetting(string powerId)
        {
            string setting;
            if (!ToggleSettings.TryGetValue(powerId, out setting)) return;
            bool value = !ModernBoxSettings.Get(setting);
            ModernBoxSettings.Set(setting, value);
            if (!value) return;
            if (setting == "NukeOption") M2Windows.Show("NukeWindow");
            else if (setting == "Developer_Mode") M2Windows.Show("DeveloperWindow");
        }

        internal static void SyncNativeToggle(string setting, bool value, bool savePlayerConfig = true)
        {
            string optionId;
            if (!SettingOptions.TryGetValue(setting, out optionId)) return;
            EnsureOption(optionId, value);
            PlayerConfig.setOptionBool(optionId, value);
            if (savePlayerConfig) PlayerConfig.saveData();
            if (PowerButtonSelector.instance != null) PowerButtonSelector.instance.checkToggleIcons();
        }
    }

    // Keeps the TabText picture between the Medieval button and the bombs, like in M2.
    internal sealed class TabTextSpot : MonoBehaviour
    {
        internal Transform Left;
        internal Transform Right;

        private void LateUpdate()
        {
            if (Left == null || Right == null) return;
            Vector3 left = Left.localPosition;
            Vector3 right = Right.localPosition;
            transform.localPosition = new Vector3((left.x + right.x) / 2f, right.y, 0f);
        }
    }
}
