using System.Collections.Generic;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace ModernBoxM2Rewrite
{
    // All the M2 windows (credits, guide, bomb menu, etc).
    internal static class M2Windows
    {
        internal const string DiscordLink = "https://discord.gg/HEBNQpbCJf";
        private const string Gold = "#FFD700";
        private static readonly Color TextColor = new Color(0.9f, 0.6f, 0f, 1f);
        private static bool _created;

        internal static void CreateAll()
        {
            if (_created) return;
            _created = true;

            CreateText("SaveSystemWindow", "ModernBox 2",
                "<color='" + Gold + "'>[ModernBox 2]</color>\n\n" +
                "Thank you for playing the mod! If you have any problems or you want news on future updates (and vote on features), join the discord server! It's also a great place to socialize. Click the button below to join! This menu showes up either when you first get the mod or whenever it updates, remember, every update will reset your settings for ModernBox 2.");
            AddDiscordImageButton("SaveSystemWindow");

            CreateText("DiscordWindow", "ModernBox",
                "<color='" + Gold + "'>[ModernBox ]</color>\n\n" +
                "The Discord Invite has been opened in your default browser, we currently have over 2,600 members and are still growing! it would mean a lot to have more active members too.");

            CreateText("NukeWindow", "ModernBox",
                "<color='" + Gold + "'>[Nuclear Warfare Activated ]</color>\n\n" +
                "You have activated Missile Silos, kingdoms will be able to build them now. to turn them off, simpily click the toggle again and restart your game. ");

            CreateText("DefaultSettingsWindow", "ModernBox",
                "<color='" + Gold + "'>[Reset! ]</color>\n\n" +
                "Your settings have been reset! Restart the game for them to take affect!");

            CreateText("DeveloperWindow", "ModernBox",
                "<color='" + Gold + "'>[Developer Mode activated ]</color>\n\n" +
                "NOTE: when developer mode is active, ModernBox prints a lot more stuff to the log so we can find bugs. Nothing gets sent anywhere.");
            AddImage("DeveloperWindow", Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare"), new Vector2(200f, 200f));

            CreateText("CreditsWindow", "ModernBox", CreditsText, 8);

            CreateGuide();
            CreateBombsWindow();
            CreateMedievalWindow();
            M2TechWindow.Create();
        }

        internal static void Show(string id)
        {
            if (!ScrollWindow._all_windows.ContainsKey(id)) return;
            ScrollWindow.showWindow(id);
        }

        internal static void OpenDiscord()
        {
            Show("DiscordWindow");
            Application.OpenURL(DiscordLink);
        }

        private const string CreditsText =
            "<color='" + Gold + "'>CREDITS</color>\n" +
            "Lead Developer:\n" +
            "Tuxxego\n\n" +
            "<color='" + Gold + "'>Developer Team:</color>\n\n" +
            "Dankmrgreen6444\nMORFOS\nFull Auto Sherman\nGoosefang\nTrike\nAriel\nMr. P\nMelvin Shwuaner\nNico_the_Nine\n\n" +
            "M2 Dev Team:\nFakher\nLonelyFear\nPlayerCro7\n\n" +
            "Other Mod Developers to Credit:\n" +
            "alexnitaly (Gunsmith Book)\nhaydar_kara (Hivemind)\n3m1rh4n (RifleActions/WestActions)\n\n\n" +
            "M1 Contributors:\narielp2\nimmortalglitch5500\nthedesertroad\nluck";

        private static void CreateGuide()
        {
            string[] pages =
            {
                "TECH: This page covers tech and when they should be unlocked. The modern era should begin around 500-1000 years into a cultures existence, with them first unlocking guns and later on MIRVs, vehicles, and buildings. MIRVs are one of the last things they will develop. The next page is on Cyberware and Drugs.",
                "DRUGS AND CYBERWARE: Drugs and Cyberware are one of the oldest things to be added into ModernBox, and one of the least changed things in it. All these do is increase or decrease unit stats and not much else, and they are rare to find. The next page is on nukes.",
                "NUKES: This covers ONLY the kingdoms nuking eachother, not the nukes you can drop on them such as the proton bomb. Nukes are an unpredictable feature and kingdoms sometimes nuke literally anything that attacks them, even bears. They sure are deadly and be mindful that if you turn them on, there's no way to turn them back off for that world aside from restarting the game. Nukes are far from a perfect future but at least they work. The next page covers vehicles and MIRVS.",
                "VEHICLES AND MIRVS: Vehicles are a staple point of ModernBox, ranging from Tanks to Fighter jets. They can be produced either by you or by kingdoms as long as that specific vehicle is enabled for factory production. You can only place vehicles in kingdoms, not in the wilderness (duh). Modern Soldiers are considered vehicles in code so they will be covered here, they spawn through the barracks building and have better stats than normal soldiers. If you enable vehicles late game, factories that were built before it was enabled will NOT produce vehicles. MIRVs are in fact not M.I.R.VS as they are in real life, instead they are really fast rocket launchers that can mow down entire countries. The next page is on misc stuff. ",
                "MISC STUFF: Stuff like Ideologies will be covered here. Ideologies are a set of traits that can influence that units actions (especially if they are a king) and yeah that's basically all they are. Modern Names are pretty self explainatory, it just adds first and last names for all units. "
            };
            ModernLocalization.Add("m2_guide_next", "Next page");
            ModernLocalization.Add("m2_guide_next_description", "");
            for (int i = 0; i < pages.Length; i++)
            {
                string id = GuideId(i);
                ScrollWindow window = CreateText(id, "Guide", pages[i]);
                if (window == null || i == pages.Length - 1) continue;
                string next = GuideId(i + 1);
                PowerButton button = PowerButtonCreator.CreateSimpleButton("m2_guide_next", () => Show(next),
                    Resources.Load<Sprite>("ui/Icons/nextpage"), window.transform, new Vector2(120f, 52f));
                button.transform.SetAsLastSibling();
            }
        }

        internal static string GuideId(int page)
        {
            return page == 0 ? "GuideWindow" : "GuideWindow" + (page + 1);
        }

        private static void CreateBombsWindow()
        {
            const string id = "EXTRA BOMBS";
            ScrollWindow window = CreateText(id, "ModernBox", "<color='" + Gold + "'>Dank and Tux got bored so here's a bunch of destructive stuff. </color>\n");
            if (window == null) return;
            // Same spots as the original menu.
            AddWindowPower(id, "AtomicGrenadebutton", "ui/Icons/AtomicGrenade", "Atomic Grenade", "A warcrime in the palm of your hand.", 0, 0);
            AddWindowPower(id, "FuryOfTuxiabutton", "ui/Icons/FuryOfTuxia", "Fury of Tuxia", "Dank told me to stop making nukes, I instead decided to create this monstrosity (if your computer survives this, you're cool!)", 1, 0);
            AddWindowPower(id, "ZeussRagebutton", "ui/Icons/ZeusRage", "Zeus's Rage", "Tremble in fear Kratos.", 2, 0);
            AddWindowPower(id, "ClusterNukebutton", "ui/Icons/ClusterNuke", "Cluster Nuke", "EXACTLY what the title says.", 3, 0);
            AddWindowPower(id, "NotSoAtomicbutton", "ui/Icons/NotSoAtomic", "the Not so atomic Bomb", "The bomb that wanted to become atomic but failed the test in 12th grade to become atomic", 4, 0);
            AddWindowPower(id, "ClusterStrikebutton", "ui/Icons/ClusterStrike", "The Cluster Strike", "Damn is it Stormy bro, or am i just trippin?", 0, 1);
            AddWindowPower(id, "Protonbutton", "ui/Icons/Proton", "The Proton Bomb", "The bomb to be forgoten no longer", 1, 1);
            AddWindowPower(id, "Spreaderbutton", "ui/Icons/MOAB", "The Spreader Bomb", "Quickly spreads to engulf your whole world in fire.", 2, 1);
            AddWindowPower(id, "ColorBombbutton", "ui/Icons/ColorGrenade", "Color Bomb", "Its a bomb with a colorfull effect", 3, 1);
            AddWindowPower(id, "DankyBombbutton", "ui/Icons/Danky", "Danky Bomb", "There's really no hard limit to how long these achievement names can be and to be quite honest I'm rather curious to see how far we can go. Adolphus W. Green (1844 to 1917) started as the Principal of the Groton School in 1864. By 1865, he became second assistant librarian at the New York Mercantile Library; from 1867 to 1869, he was promoted to full librarian. From 1869 to 1873, he worked for Evarts, Southmayd & Choate, a law firm co-founded by William M. Evarts, Charles Ferdinand Southmayd and Joseph Hodges Choate. He was admitted to the New York State Bar Association in 1873. Anyway, how's your day been?", 4, 1);
            AddWindowPower(id, "BloodLightningbutton", "ui/Icons/BloodLightning", "Blood Lightning", "Forgive me for I have gone mad -Zeus", 0, 2);
            AddWindowPower(id, "NoDamagebutton", "ui/Icons/BlueOne", "No Damage", "Its a bomb that looks cool and thats it. have Fun :)", 1, 2);
            // TUDDS comes from the space part, only show it when it's there.
            AddWindowPower(id, "DeleterButton", "ui/Icons/UniversalDestroyer", "The Unholy Universal Destruction System", "Destroys the entire universe (literally it deletes EVERYTHING, watch out with this bad boy.", 2, 2);
            FitContent(id, 3);
        }

        private static void CreateMedievalWindow()
        {
            const string id = "Medieval Unit Spawner";
            ScrollWindow window = CreateText(id, "Medieval Unit Spawner", "<color='" + Gold + "'>Tis but a scratch</color>\n");
            if (window == null) return;
            string[] units =
            {
                "catapulta", "orcatapulta", "batteringram", "humancavalry", "humanpaladin",
                "humancannon", "orccannon", "elfcannon", "dwarfcannon", "dwarfdoctor",
                "orcwarlock", "fairelf", "fairydragon", "ogreunit", "golemgem",
                "treant", "armoredwolf", "woolyrhino", "santaguin"
            };
            int slot = 0;
            foreach (string unit in units)
            {
                string powerId = "modernbox_spawn_" + unit;
                GodPower power = AssetManager.powers.get(powerId);
                if (power == null) continue;
                Sprite icon = string.IsNullOrEmpty(power.path_icon) ? null : Resources.Load<Sprite>(power.path_icon);
                if (AddWindowPower(id, powerId, icon, null, null, slot % 5, slot / 5)) slot++;
            }
            FitContent(id, (slot + 4) / 5);
        }

        private static ScrollWindow CreateText(string id, string title, string text, int fontSize = 10)
        {
            string titleKey = "m2_window_" + id.Replace(' ', '_');
            ModernLocalization.Add(titleKey, title);
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
            if (content == null) return window;
            foreach (LayoutGroup group in content.GetComponents<LayoutGroup>()) group.enabled = false;
            foreach (ContentSizeFitter fitter in content.GetComponents<ContentSizeFitter>()) fitter.enabled = false;

            GameObject textObject = new GameObject("M2Text", typeof(RectTransform));
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
            return window;
        }

        private static readonly Dictionary<string, float> TextBottom = new Dictionary<string, float>();

        private static RectTransform Content(ScrollWindow window)
        {
            if (window == null) return null;
            if (window.transform_content != null) return window.transform_content;
            Transform content = window.transform.Find("Background/Scroll View/Viewport/Content");
            return content == null ? null : content as RectTransform;
        }

        private static bool AddWindowPower(string windowId, string powerId, string iconPath, string title, string description, int column, int row)
        {
            return AddWindowPower(windowId, powerId, Resources.Load<Sprite>(iconPath), title, description, column, row);
        }

        private static bool AddWindowPower(string windowId, string powerId, Sprite icon, string title, string description, int column, int row)
        {
            ScrollWindow window = Windows(windowId);
            RectTransform content = Content(window);
            if (content == null || AssetManager.powers.get(powerId) == null) return false;
            if (title != null) ModernLocalization.Add(powerId, title);
            if (description != null) ModernLocalization.Add(powerId + "_description", description);
            PowerButton button = PowerButtonCreator.CreateGodPowerButton(powerId, icon, content, Vector2.zero);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(-72f + 36f * column, -(TextBottom[windowId] + 25f + 50f * row));
            // The game greys out buttons with "Button" in their name.
            if (button.icon != null) button.icon.color = Color.white;
            Image background = button.GetComponent<Image>();
            if (background != null) background.color = Color.white;
            Button unityButton = button.GetComponent<Button>();
            if (unityButton != null) unityButton.onClick.AddListener(() => ScrollWindow.hideAllEvent(true));
            return true;
        }

        private static void FitContent(string windowId, int rows)
        {
            RectTransform content = Content(Windows(windowId));
            if (content == null) return;
            content.sizeDelta = new Vector2(content.sizeDelta.x, TextBottom[windowId] + 50f * rows + 30f);
        }

        private static void AddDiscordImageButton(string windowId)
        {
            RectTransform content = Content(Windows(windowId));
            if (content == null) return;
            GameObject imageObject = new GameObject("M2DiscordButton", typeof(RectTransform));
            imageObject.transform.SetParent(content, false);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/Icons/buttonSprite");
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(200f, 100f);
            rect.anchoredPosition = new Vector2(0f, -(TextBottom[windowId] + 5f));
            Button button = imageObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.yellow;
            colors.pressedColor = Color.gray;
            button.colors = colors;
            button.onClick.AddListener(() => Application.OpenURL(DiscordLink));
            content.sizeDelta = new Vector2(content.sizeDelta.x, TextBottom[windowId] + 115f);
        }

        private static void AddImage(string windowId, Sprite sprite, Vector2 size)
        {
            RectTransform content = Content(Windows(windowId));
            if (content == null || sprite == null) return;
            GameObject imageObject = new GameObject("M2Image", typeof(RectTransform));
            imageObject.transform.SetParent(content, false);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, -(TextBottom[windowId] + 5f));
            content.sizeDelta = new Vector2(content.sizeDelta.x, TextBottom[windowId] + size.y + 15f);
        }

        private static ScrollWindow Windows(string id)
        {
            ScrollWindow window;
            return ScrollWindow._all_windows.TryGetValue(id, out window) ? window : null;
        }
    }
}
