using System.Collections.Generic;
using System.IO;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace ModernBoxM1Rewrite
{
    // The original windows (InfoWindow.cs, CreditsWindow.cs, ...), all titled "ModernBox":
    // orange text, a gold heading and the tab icon underneath.
    internal static class M1Windows
    {
        internal const string DiscordLink = "https://discord.gg/HEBNQpbCJf";
        private const string Gold = "#FFD700";
        private static readonly Color TextColor = new Color(0.9f, 0.6f, 0f, 1f);
        private static readonly Dictionary<string, float> TextBottom = new Dictionary<string, float>();
        private static readonly List<string> Pending = new List<string>();
        private static bool _created;
        private static bool _ready;
        private static Text _techText;

        internal static void CreateAll()
        {
            if (_created) return;
            _created = true;

            CreateText("SaveSystemWindow", "ModernBox ",
                "Welcome to ModernBox! If this is your first time installing, you can ignore this. If you are updating to this version (10.0.0), all your saved settings in the ModernBox tab are reset to defaults. Also if you are new, you should consider joining the discord for news about upcoming versions and other things.");
            CreateText("InfoWindow", "ModernBox ",
                "Welcome to ModernBox! This mod adds Guns, MIRVs, Vehicles, and more. Press the arrow in the right corner of the mods tab to see more buttons.");
            CreateText("DiscordWindow", "ModernBox ",
                "The Discord Invite has been opened in your default browser, we currently have over 800 members and are still growing! it would mean a lot to have more active members too.");
            CreateText("NukeWindow", "Nuclear Warfare Activated ",
                "You have activated Missile Silos, kingdoms will be able to build them now. to turn them off, simpily click the toggle again and restart your game. ",
                "ui/Icons/Nuke");
            // The original sent every error to a private Discord channel. This version doesn't,
            // so the note says what it really does.
            CreateText("DeveloperWindow", "Developer Mode activated ",
                "NOTE: when developer mode is active, ModernBox writes extra information to the game log so bugs are easier to find. Nothing is sent anywhere.");
            CreateText("NCMSWindow", "WARNING ",
                "It seems you are using NCMS, consider upgrading from NCMS to NML as NCMS is obsolete and no longer being updated by its creator.");
            CreateText("DefaultSettingsWindow", "Reset! ",
                "Your settings have been reset! Restart the game for them to take affect!");
            CreateCredits();
            _techText = AddWindow("TechWindow", string.Empty, 8, null);
            if (_techText != null) _techText.gameObject.AddComponent<TechWindowRefresher>();
        }

        // Not in the original: 0.51.2 has no culture tech window, so this shows the
        // research the mod runs instead.
        internal static void ShowTech()
        {
            RefreshTech();
            Show("TechWindow");
        }

        internal static void RefreshTech()
        {
            if (_techText == null) return;
            _techText.text = M1Tech.Report(Gold);
            RectTransform rect = _techText.rectTransform;
            float height = _techText.preferredHeight;
            rect.sizeDelta = new Vector2(180f, height);
            RectTransform content = rect.parent as RectTransform;
            if (content != null) content.sizeDelta = new Vector2(content.sizeDelta.x, 17f + height + 50f);
        }

        // Called once a world has loaded; windows asked for before that open now.
        internal static void Ready()
        {
            _ready = true;
            foreach (string id in Pending.ToArray()) Show(id);
            Pending.Clear();
        }

        // Windows asked for during startup (saved toggles) open once the game has loaded.
        internal static void Show(string id)
        {
            if (!_created || !_ready)
            {
                if (!Pending.Contains(id)) Pending.Add(id);
                return;
            }
            if (ScrollWindow._all_windows.ContainsKey(id)) ScrollWindow.showWindow(id);
        }

        internal static void OpenDiscord()
        {
            Show("DiscordWindow");
            Application.OpenURL(DiscordLink);
        }

        // NCMSDetector.cs
        internal static bool NcmsInstalled()
        {
            string mods = Path.Combine(Application.streamingAssetsPath, "mods");
            return File.Exists(Path.Combine(mods, "NCMS_memload.dll"));
        }

        private static void CreateCredits()
        {
            const string text =
                "<color='" + Gold + "'>CREDITS</color>\n\n" +
                "Programmers\nTuxxego\ndankmrgreen6444\nthebluenightyes\n\n" +
                "<color='" + Gold + "'>Artists:</color>\narielp2\nimmortalglitch5500\nthedesertroad\nluck (some of the guns)\nAgriche (owns the tank sprite)\nalexnitaly (a lot of the nuclear missile code is from gunsmith book)\n\n" +
                "<color='" + Gold + "'>DISCORD SERVER:</color>\nAdmins:\nslowatplaye\ngm_blahaj\nkeymasterer\n\n" +
                "Moderators:\nmatthewn0008\nmichealllllllllllllll\nthe_state_of_florida\ncakewashere\n.xiexel\nh_d.v0rtex\nmynameisfakher\ngeckofficial\ngeckomobile\n_harrier\nhoneybee1661\n" +
                "<color='" + Gold + "'>SERVER BOOSTERS:</color>\nkeymasterer\nsog1029\nicee9924\nwolfenmicky\n";
            AddWindow("CreditsWindow", text, 4, null);
        }

        private static void CreateText(string id, string heading, string body, string image = "ui/Icons/tabIconModernWarfare")
        {
            AddWindow(id, "<color='" + Gold + "'>[" + heading + "]</color>\n\n" + body, 10, image);
        }

        private static Text AddWindow(string id, string text, int fontSize, string image)
        {
            const string titleKey = "modernbox_m1_window_title";
            ModernLocalization.Add(titleKey, "ModernBox");
            ScrollWindow window = WindowCreator.CreateEmptyWindow(id, titleKey, "tabIconModernWarfare");
            if (window == null) return null;
            Transform titleTransform = window.transform.Find("Background/Title");
            LocalizedText titleText = titleTransform == null ? null : titleTransform.GetComponent<LocalizedText>();
            if (titleText != null)
            {
                titleText.setKeyAndUpdate(titleKey);
                titleText.autoField = false;
            }

            RectTransform content = Content(window);
            if (content == null) return null;
            foreach (LayoutGroup group in content.GetComponents<LayoutGroup>()) group.enabled = false;
            foreach (ContentSizeFitter fitter in content.GetComponents<ContentSizeFitter>()) fitter.enabled = false;

            GameObject textObject = new GameObject("M1Text", typeof(RectTransform));
            textObject.transform.SetParent(content, false);
            Text label = textObject.AddComponent<Text>();
            if (window.titleText != null) label.font = window.titleText.font;
            label.fontSize = fontSize;
            label.color = TextColor;
            label.alignment = TextAnchor.UpperCenter;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.text = text;
            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(180f, 10f);
            float height = label.preferredHeight;
            rect.sizeDelta = new Vector2(180f, height);
            rect.anchoredPosition = new Vector2(0f, -17f);
            TextBottom[id] = 17f + height;
            content.sizeDelta = new Vector2(content.sizeDelta.x, TextBottom[id] + 50f);

            Sprite sprite = string.IsNullOrEmpty(image) ? null : Resources.Load<Sprite>(image);
            if (sprite == null) return label;
            GameObject imageObject = new GameObject("ImageGO", typeof(RectTransform));
            imageObject.transform.SetParent(content, false);
            Image picture = imageObject.AddComponent<Image>();
            picture.sprite = sprite;
            picture.raycastTarget = false;
            RectTransform imageRect = picture.rectTransform;
            imageRect.anchorMin = new Vector2(0.5f, 1f);
            imageRect.anchorMax = new Vector2(0.5f, 1f);
            imageRect.pivot = new Vector2(0.5f, 1f);
            imageRect.sizeDelta = new Vector2(200f, 200f);
            imageRect.anchoredPosition = new Vector2(0f, -(TextBottom[id] + 5f));
            content.sizeDelta = new Vector2(content.sizeDelta.x, TextBottom[id] + 215f);
            return label;
        }

        // Keeps the Tech window up to date while it is open.
        private sealed class TechWindowRefresher : MonoBehaviour
        {
            private float _timer;

            private void Update()
            {
                _timer -= Time.unscaledDeltaTime;
                if (_timer > 0f) return;
                _timer = 1f;
                RefreshTech();
            }
        }

        private static RectTransform Content(ScrollWindow window)
        {
            if (window == null) return null;
            if (window.transform_content != null) return window.transform_content;
            Transform content = window.transform.Find("Background/Scroll View/Viewport/Content");
            return content as RectTransform;
        }
    }
}
