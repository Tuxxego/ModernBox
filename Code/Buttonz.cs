using UnityEngine;

using System.Collections.Generic;
using UnityEngine.Events;

using UnityEngine.UI;
using System.Collections;
using NCMS.Utils;
using NCMS;
using ReflectionUtility;
using TuxModLoader.Reflection;
using System.Reflection;
using System;

namespace ModernBox
{
	public class Buttonz
	{        
        public static PowerButton zomboo;

		public void Init()
		{
			PowersTab tab = getPowersTab("ModernBoxTab");
			PowersTab tab2 = getPowersTab("ModernBoxUnits");
			PowersTab tab3 = getPowersTab("ModernBoxBombs");
			PowersTab tab4 = getPowersTab("ModernBoxEras");
			PowersTab tab5 = getPowersTab("ModernBoxItems");
			PowersTab tab6 = getPowersTab("ModernBoxSpace");
            PowersTab otherTab = getPowersTab("other");

            if (tab == null || tab2 == null || tab3 == null || tab4 == null || tab5 == null || tab6 == null)
            {
                ModernBoxLogger.Error("[Buttonz] One or more ModernBox tabs are missing during Init().");
                return;
            }

			GameObject largeImageObject = new GameObject("LargeImage");
			largeImageObject.transform.SetParent(tab.transform);
			largeImageObject.transform.localPosition = new Vector3(396, 18, 0);
			largeImageObject.transform.localScale = Vector3.one;

			Image largeImage = largeImageObject.AddComponent<Image>();
			largeImage.sprite = Resources.Load<Sprite>("ui/Icons/TabText");

			RectTransform imageRect = largeImageObject.GetComponent<RectTransform>();
			imageRect.sizeDelta = new Vector2(200, 100);
			imageRect.anchorMin = new Vector2(0.5f, 0.5f);
			imageRect.anchorMax = new Vector2(0.5f, 0.5f);

            StatManager.Instance.RegisterImage(largeImage);

            GameObject statLabelObject = new GameObject("StatLabel");
            statLabelObject.transform.SetParent(tab.transform);
            statLabelObject.transform.localPosition = new Vector3(356, -18, 0); 
            statLabelObject.transform.localScale = Vector3.one;

            Text statText = statLabelObject.AddComponent<Text>();
            statText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statText.fontSize = 9;
            statText.color = new Color(1f, 0.95f, 0.8f); 
            statText.supportRichText = true;
            statText.text = "Loading stats...";

            RectTransform textRect = statLabelObject.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(200, 100);
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);

            StatManager.Instance.RegisterStatLabel(statText);

            GameObject stat2LabelObject = new GameObject("StatLabel2");
            stat2LabelObject.transform.SetParent(tab4.transform);
            stat2LabelObject.transform.localPosition = new Vector3(386, -18, 0); 
            stat2LabelObject.transform.localScale = Vector3.one;

            Text statText2 = stat2LabelObject.AddComponent<Text>();
            statText2.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statText2.fontSize = 9;
            statText2.color = new Color(1f, 0.95f, 0.8f); 
            statText2.supportRichText = true;
            statText2.text = "Loading stats...";

            RectTransform textRect2 = stat2LabelObject.GetComponent<RectTransform>();
            textRect2.sizeDelta = new Vector2(200, 100);
            textRect2.anchorMin = new Vector2(0.5f, 0.5f);
            textRect2.anchorMax = new Vector2(0.5f, 0.5f);

            StatManager.Instance.RegisterStatLabel2(statText2);

            // ── Panel background ──────────────────────────────────────────────────────────
            GameObject panelObj = new GameObject("StatPanel3");
            panelObj.transform.SetParent(tab6.transform);
            panelObj.transform.localPosition = new Vector3(386, -18, 0);
            panelObj.transform.localScale = Vector3.one;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(210, 110);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);

            // ── Outline / border via a child image ───────────────────────────────────────
            GameObject borderObj = new GameObject("PanelBorder");
            borderObj.transform.SetParent(panelObj.transform);
            borderObj.transform.localPosition = Vector3.zero;
            borderObj.transform.localScale = Vector3.one;

            Outline panelOutline = panelObj.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.4f, 0.8f, 1f, 0.6f);
            panelOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // ── Header label ─────────────────────────────────────────────────────────────
            GameObject headerObj = new GameObject("StatHeader3");
            headerObj.transform.SetParent(panelObj.transform);
            headerObj.transform.localPosition = new Vector3(0, 38, 0);
            headerObj.transform.localScale = Vector3.one;

            Text headerText = headerObj.AddComponent<Text>();
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.fontSize = 11;
            headerText.fontStyle = FontStyle.Bold;
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color = new Color(0.45f, 0.85f, 1f, 1f);   // icy blue
            headerText.supportRichText = true;
            headerText.text = "";

            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(200, 20);
            headerRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerRect.anchorMax = new Vector2(0.5f, 0.5f);

      /*      // ── Divider ───────────────────────────────────────────────────────────────────
            GameObject divObj = new GameObject("Divider3");
            divObj.transform.SetParent(panelObj.transform);
            divObj.transform.localPosition = new Vector3(0, 26, 0);
            divObj.transform.localScale = Vector3.one;

            Image divImg = divObj.AddComponent<Image>();
            divImg.color = new Color(0.4f, 0.8f, 1f, 0.3f);

            RectTransform divRect = divObj.GetComponent<RectTransform>();
            divRect.sizeDelta = new Vector2(190, 1);
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
        */
            // ── Main stat text ────────────────────────────────────────────────────────────
            GameObject stat3LabelObject = new GameObject("StatLabel3");
            stat3LabelObject.transform.SetParent(panelObj.transform);
            stat3LabelObject.transform.localPosition = new Vector3(0, -5, 0);
            stat3LabelObject.transform.localScale = Vector3.one;

            Text statText3 = stat3LabelObject.AddComponent<Text>();
            statText3.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statText3.fontSize = 12;
            statText3.alignment = TextAnchor.UpperCenter;
            statText3.color = new Color(1f, 0.95f, 0.8f, 1f);
            statText3.supportRichText = true;

            // Rich text example — your StatManager can push strings like this:
            // "<color=#7DD4FC><b>Kills</b></color>  <color=#FFFFFF>42</color>\n
            //  <color=#7DD4FC><b>Deaths</b></color>  <color=#FF6B6B>7</color>"
            statText3.text = "<color=#7DD4FC>Loading</color> <color=#AAAAAA>stats...</color>";

            // Shadow for depth
            Shadow statShadow = stat3LabelObject.AddComponent<Shadow>();
            statShadow.effectColor = new Color(0f, 0.3f, 0.5f, 0.8f);
            statShadow.effectDistance = new Vector2(1f, -1f);

            RectTransform textRect3 = stat3LabelObject.GetComponent<RectTransform>();
            textRect3.sizeDelta = new Vector2(196, 80);
            textRect3.anchorMin = new Vector2(0.5f, 0.5f);
            textRect3.anchorMax = new Vector2(0.5f, 0.5f);

            StatManager.Instance.RegisterStatLabel3(statText3);

            GameObject discordAdObject = new GameObject("DiscordAd");
            discordAdObject.transform.SetParent(tab.transform);
            discordAdObject.transform.localPosition = new Vector3(136, -20, 0); 
            discordAdObject.transform.localScale = Vector3.one;

            Image discordAdImage = discordAdObject.AddComponent<Image>();
            discordAdImage.sprite = Resources.Load<Sprite>("ui/Icons/buttonSprite"); 

            RectTransform adRect = discordAdObject.GetComponent<RectTransform>();
            adRect.sizeDelta = new Vector2(65, 25);
            adRect.anchorMin = new Vector2(0.5f, 0.5f);
            adRect.anchorMax = new Vector2(0.5f, 0.5f);

            discordAdObject.AddComponent<DiscordAdHover>();

            Button adButton = discordAdObject.AddComponent<Button>();
            adButton.onClick.AddListener(() =>
            {
                Application.OpenURL("https://discord.gg/PahzD7rtv2");

            });

            StatManager.Instance.RegisterFlashingImage(discordAdImage);

            new ButtonBuilder("credits")
            .SetSprite(Resources.Load<Sprite>("ui/icons/iconabout"))
            .SetTitle("About ModernBox")
            .SetDescription("All the people behind ModernBox and more!")
            .SetPosition(0, 0)
            .SetType(ButtonType.Click)
            .SetFunction(openAboutWindow)
            .SetTransform(tab.transform)

            .Build();

        new ButtonBuilder("resettodefaults")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Reset"))
            .SetTitle("Reset to defaults")
            .SetDescription("Resets ALL saved settings to their default values.")
            .SetPosition(1, 0)
            .SetType(ButtonType.Click)
            .SetFunction(Main.resetToDefaults)
            .SetTransform(tab.transform)
            .Build();

        new ButtonBuilder("infinitebox")
            .SetSprite(Resources.Load<Sprite>("ui/icons/insd"))
            .SetTitle("InfiniteBox")
            .SetDescription("Learn about my next project!")
            .SetPosition(2, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(openInfiniteBoxWindow)
            .Build();

        new ButtonBuilder("resetbawls")
            .SetSprite(Resources.Load<Sprite>("ui/icons/tabIconModernWarfare"))
            .SetTitle("Resetup the mod")
            .SetDescription("WARNING: SAVE YOUR GAME BEFORE PRESSING THIS BUTTON!")
            .SetPosition(4, 1)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(opendaSetup)
            .Build();

        new ButtonBuilder("m3achievements")
            .SetSprite(Resources.Load<Sprite>("ui/icons/trophy"))
            .SetTitle("Achievements")
            .SetDescription("COMING SOON")
            .SetPosition(0, 1)
            .SetType(ButtonType.Click)
            .SetFunction(openAchievmentsWindow)
            .SetTransform(tab.transform)
            .SetFunction(openAchievmentsWindow)
            .Build();

        new ButtonBuilder("modernbox_tab_units")
            .SetSprite(Resources.Load<Sprite>("ui/icons/warhamma"))
            .SetTitle("ModernBox Units")
            .SetDescription("Open the Units tab.")
            .SetPosition(16, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(() => openModernBoxSubTab("ModernBoxUnits"))
            .Build();

        new ButtonBuilder("modernbox_tab_bombs")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Overload"))
            .SetTitle("ModernBox Bombs")
            .SetDescription("Open the Bombs tab.")
            .SetPosition(16, 1)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(() => openModernBoxSubTab("ModernBoxBombs"))
            .Build();

        new ButtonBuilder("modernbox_tab_eras")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Industrial"))
            .SetTitle("ModernBox Eras")
            .SetDescription("Open the Eras tab.")
            .SetPosition(17, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(() => openModernBoxSubTab("ModernBoxEras"))
            .Build();

        new ButtonBuilder("modernbox_tab_space")
            .SetSprite(Resources.Load<Sprite>("Stars/Gravitonstar"))
            .SetTitle("ModernBox Space")
            .SetDescription("Open the Space tab.")
            .SetPosition(17, 1)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(() => openModernBoxSubTab("ModernBoxSpace"))
            .Build();

        new ButtonBuilder("modernbox_tab_items")
            .SetSprite(Resources.Load<Sprite>("ui/icons/firearm"))
            .SetTitle("ModernBox Items")
            .SetDescription("Open the Items tab.")
            .SetPosition(18, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(() => openModernBoxSubTab("ModernBoxItems"))
            .Build();

        new ButtonBuilder("modernbox_tab_trainbox")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Industrial"))
            .SetTitle("Trainbox")
            .SetDescription("Open the Trainbox tab.")
            .SetPosition(19, 1)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(openTrainboxTab)
            .Build();

        new ButtonBuilder("modernbox_tab_kaiju")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Godzilla"))
            .SetTitle("Kaiju")
            .SetDescription("Open the Kaiju tab.")
            .SetPosition(18, 1)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(() => openModernBoxSubTab("Tab_kaiju"))
            .Build();

            if (otherTab != null)
            {
                EnsureOtherTabButton("modernbox_launcher", "ui/icons/tabIconModernWarfare", "ModernBox", "Open the main ModernBox hub.", 0, 0, otherTab.transform, openModernBoxHub);
                EnsureOtherTabButton("modernbox_other_units", "ui/icons/warhamma", "MB Units", "Open the Units tab.", 1, 0, otherTab.transform, () => openModernBoxSubTab("ModernBoxUnits"));
                EnsureOtherTabButton("modernbox_other_bombs", "ui/icons/Overload", "MB Bombs", "Open the Bombs tab.", 2, 0, otherTab.transform, () => openModernBoxSubTab("ModernBoxBombs"));
                EnsureOtherTabButton("modernbox_other_trainbox", "ui/icons/Industrial", "Trainbox", "Open the Trainbox tab.", 3, 0, otherTab.transform, openTrainboxTab);
                EnsureOtherTabButton("modernbox_other_eras", "ui/icons/Industrial", "MB Eras", "Open the Eras tab.", 0, 1, otherTab.transform, () => openModernBoxSubTab("ModernBoxEras"));
                EnsureOtherTabButton("modernbox_other_space", "Stars/Gravitonstar", "MB Space", "Open the Space tab.", 1, 1, otherTab.transform, () => openModernBoxSubTab("ModernBoxSpace"));
                EnsureOtherTabButton("modernbox_other_items", "ui/icons/firearm", "MB Items", "Open the Items tab.", 2, 1, otherTab.transform, () => openModernBoxSubTab("ModernBoxItems"));
            }

            new ButtonBuilder("nukes_toggle")
                .SetSprite(Resources.Load<Sprite>("ui/Icons/MIRV_nuke"))
                .SetTitle("Toggle Nuclear Warfare")
                .SetDescription("Kingdoms can nuke each other.")
                .SetPosition(11, 0) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab3.transform)
                .SetFunction(Vehicles.toggleNukes)
                .Build();

            if (Main.savedSettings.boolOptions["NukeOption"]) {
                PowerButtons.ToggleButton("nukes_toggle");
                Vehicles.toggleNukes();
            }

            new ButtonBuilder("pizza")
                .SetSprite(Resources.Load<Sprite>("ui/Icons/Pizza"))
                .SetTitle("Pizza")
                .SetDescription("Go ahead, take a slice.")
                .SetPosition(4, 0) 
                .SetType(ButtonType.Click)
                .SetTransform(tab.transform)
                .SetFunction(PizzaManager.instance.ClickPizza)
                .Build();

            new ButtonBuilder("vehicle_toggle")
                .SetSprite(Resources.Load<Sprite>("actors/Heli_Human/new_helicopter1"))
                .SetTitle("Toggle Vehicles")
                .SetDescription("Toggles the ability for kingdoms to produce vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleVehicles)
                .Build();

            if (Main.savedSettings.boolOptions["FactoriesOption"]) {
                PowerButtons.ToggleButton("vehicle_toggle");
                Traits.toggleVehicles();
            }

            new ButtonBuilder("nuketexttoggle")
                .SetSprite(Resources.Load<Sprite>("ui/icons/Nuke"))
                .SetTitle("Toggle Nuclear Notifications")
                .SetDescription("These will alert you when a nation is nuked.")
                .SetPosition(12, 0)
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Vehicles.toggleBalls)
                .Build();

            if (Main.savedSettings.boolOptions["BallsOption"]) {
                PowerButtons.ToggleButton("nuketexttoggle");
                Vehicles.toggleBalls();
            }

            new ButtonBuilder("gun_toggle")
                .SetSprite(Resources.Load<Sprite>("weapons/AK"))
                .SetTitle("Toggle Guns")
                .SetDescription("Toggles gun production.")
                .SetPosition(19, 0) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(CustomItemsList.toggleGuns)
                .Build();

            if (Main.savedSettings.boolOptions["GunOption"]) {
                PowerButtons.ToggleButton("gun_toggle");
                CustomItemsList.toggleGuns();
            }

            new ButtonBuilder("mirv_toggle")
                .SetSprite(Resources.Load<Sprite>("ui/icons/items/icon_STRONGMIRV"))
                .SetTitle("Toggle MIRVs")
                .SetDescription("Toggles MIRV production.")
                .SetPosition(20, 1)
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(CustomItemsList.toggleMIRVs)
                .Build();

            if (Main.savedSettings.boolOptions["MIRVOption"]) {
                PowerButtons.ToggleButton("mirv_toggle");
                CustomItemsList.toggleMIRVs();
            }

            new ButtonBuilder("drugs_toggle")
                .SetSprite(Resources.Load<Sprite>("ui/icons/items/icon_morphine"))
                .SetTitle("Toggle Drugs")
                .SetDescription("Toggle the ability for people to make & do drugs.")
                .SetPosition(20, 0)
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(CustomItemsList.toggleDrugs)
                .Build();

            if (Main.savedSettings.boolOptions["DrugsOption"]) {
                PowerButtons.ToggleButton("drugs_toggle");
                CustomItemsList.toggleDrugs();
            }

            new ButtonBuilder("mgltoggle")
                .SetSprite(Resources.Load<Sprite>("ui/icons/XenoInfectionIcon"))
                .SetTitle("Toggle Chemical Weapons")
                .SetDescription("Toggles DA production.")
                .SetPosition(21, 0)
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(CustomItemsList.toggleMGL)
                .Build();

            if (Main.savedSettings.boolOptions["ChemOption"]) {
                PowerButtons.ToggleButton("mgltoggle");
                CustomItemsList.toggleMGL();
            }

            InsertLine.Space(21, tab5.transform);

            new ButtonBuilder("atat")
                .SetSprite(Resources.Load<Sprite>("actors/AT9000/main/walk_7"))
                .SetTitle("Toggle ATAT Production")
                .SetDescription("Toggles the ability for kingdoms to produce ATAT class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleATAT)
                .Build();

            if (Main.savedSettings.boolOptions["FactoriesOption"]) {
                PowerButtons.ToggleButton("atat");
                Traits.toggleATAT();
            }

            new ButtonBuilder("dread")
                .SetSprite(Resources.Load<Sprite>("ui/icons/Vatican"))
                .SetTitle("Toggle Dreadnaught Production")
                .SetDescription("Toggles the ability for kingdoms to produce Dreadnaught class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleDread)
                .Build();

            if (Main.savedSettings.boolOptions["DreadOption"]) {
                PowerButtons.ToggleButton("dread");
                Traits.toggleDread();
            }

            new ButtonBuilder("gunship")
                .SetSprite(Resources.Load<Sprite>("ui/icons/DankIsGay"))
                .SetTitle("Toggle Gunship Production")
                .SetDescription("Toggles the ability for kingdoms to produce Gunship class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleGunship)
                .Build();

            if (Main.savedSettings.boolOptions["GunshipOption"]) {
                PowerButtons.ToggleButton("gunship");
                Traits.toggleGunship();
            }

            new ButtonBuilder("tiefighter")
                .SetSprite(Resources.Load<Sprite>("ui/icons/TIEFighter"))
                .SetTitle("Toggle TIE Fighter Production")
                .SetDescription("Toggles the ability for kingdoms to produce TIE Fighter class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleTIEFighter)
                .Build();

            if (Main.savedSettings.boolOptions["TIEFighterOption"]) {
                PowerButtons.ToggleButton("tiefighter");
                Traits.toggleTIEFighter();
            }

            new ButtonBuilder("goliath")
                .SetSprite(Resources.Load<Sprite>("actors/GoliathCrawler/main/walk_1"))
                .SetTitle("Toggle Goliath Production")
                .SetDescription("Toggles the ability for kingdoms to produce Goliath class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleGoliath)
                .Build();

            if (Main.savedSettings.boolOptions["GoliathOption"]) {
                PowerButtons.ToggleButton("goliath");
                Traits.toggleGoliath();
            }

            new ButtonBuilder("mechagodzilla_goliath")
                .SetSprite(Resources.Load<Sprite>("actors/Mechagodzilla/main/walk_1"))
                .SetTitle("Toggle Mechagodzilla Production")
                .SetDescription("Toggles the ability for kingdoms to produce Mechagodzilla class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleMechagodzilla)
                .Build();

            if (Main.savedSettings.boolOptions["MechagodzillaOption"]) {
                PowerButtons.ToggleButton("mechagodzilla_goliath");
                Traits.toggleMechagodzilla();
            }

            new ButtonBuilder("earthquaker")
                .SetSprite(Resources.Load<Sprite>("ui/icons/Mecha_egg"))
                .SetTitle("Toggle Earthquaker Production")
                .SetDescription("Toggles the ability for kingdoms to produce Earthquaker class vehicles.")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleEarthquaker)
                .Build();

            if (Main.savedSettings.boolOptions["EarthquakerOption"]) {
                PowerButtons.ToggleButton("earthquaker");
                Traits.toggleEarthquaker();
            }

            new ButtonBuilder("spacemarines")
                .SetSprite(Resources.Load<Sprite>("actors/SpaceMarine/main/walk_0"))
                .SetTitle("Toggle Space Marine Conversion")
                .SetDescription("Toggles the ability for kingdoms to convert units into Space Marines. (Hyperfuture era)")
                .SetPosition(18, 1) 
                .SetType(ButtonType.Toggle)
                .SetTransform(tab5.transform)
                .SetFunction(Traits.toggleSpaceMarines)
                .Build();

            if (Main.savedSettings.boolOptions["SpaceMarineOption"]) {
                PowerButtons.ToggleButton("spacemarines");
                Traits.toggleSpaceMarines();
            }

            int index = 0;
            foreach (var unit in UnitTracker.Instance.units)
            {
                if (unit.sprite == null)
                {
                    ModernBoxLogger.Warning($"[M3] Skipping unit {unit.id} because sprite is null.");
                    continue;
                }
                int posX = index / 2; 
                int posY = index % 2;

                new ButtonBuilder($"spawn_{unit.id}")
                    .SetSprite(unit.sprite)
                    .SetTitle($"Spawn ({unit.id})")
                    .SetDescription($"Spawn the unit: {unit.id}")
                    .SetPosition(posX, posY)
                    .SetType(ButtonType.GodPower)
                    .SetTransform(tab2.transform)
                    .Build();

                index++;
            }

            SetupBombs();
            SetupEras();
            SetupLines();
            SetupSpace();
		}
        private void SetupLines()
        {
          PowersTab tab = getPowersTab("ModernBoxTab");

          InsertLine.At(10, tab.transform);
        //  InsertLine.At(21, tab.transform);
        }

        string failedprofesion;

    private void GetRandomProfession() {
        List<string> professions = new List<string>() {"rapper", "WB modder", "artist", "farmer", "fisherman", "blacksmith", "terrorist", "youtuber", "streamer", "game developer"};
        int randomIndex = UnityEngine.Random.Range(0, professions.Count);
        failedprofesion = professions[randomIndex];

    }

    private void SetupBombs()
        {
            PowersTab tab3 = getPowersTab("ModernBoxBombs");

            new ButtonBuilder("moab_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/MOAB"))
            .SetTitle("Super-Nuke")
            .SetDescription("Keeping this because it's the OG bomb, but in reality it's basically the same size as the Tsar Bomba.")
            .SetPosition(14, 0)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

             new ButtonBuilder("icebomb_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/I hate this stupid bomb it messed me up"))
            .SetTitle("Ice Bomb")
            .SetDescription("Give everyone in the vicinity a 'nuclear winter'.")
            .SetPosition(15, 0)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

             new ButtonBuilder("tuxium_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/tuxxego rocks"))
            .SetTitle("Tuxium Bomb")
            .SetDescription("In ModernBox fashion, we need at least one ungodly huge bomb that serves no use.")
            .SetPosition(15, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

             new ButtonBuilder("firebomb_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/FIRE!"))
            .SetTitle("Sun Glazing Bomb")
            .SetDescription("Drop the sun on people!")
            .SetPosition(16, 0)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("clusternuke_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/ClusterNuke"))
            .SetTitle("Cluster Nuke")
            .SetDescription("Lord have mercy, have 5 bombs!")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

     /*       new ButtonBuilder("homo_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/HOMO!"))
            .SetTitle("Homo Nuke")
            .SetDescription("OH MY GODDDD BRENNNDDDAAA")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();
    */
            new ButtonBuilder("fury_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/FuryOfTuxia"))
            .SetTitle("Fury Of Tuxia")
            .SetDescription("This will crash your Game, i guarantee it. So dont use it unless you want to restart the game")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("overload_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Overload"))
            .SetTitle("Overload Nuke")
            .SetDescription("This ones a dozey")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("zombie_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Chicken jockey"))
            .SetTitle("Zombie Nuke")
            .SetDescription("Turns the people into zombies")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("xeno_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Xeno"))
            .SetTitle("Xenium Nuke")
            .SetDescription("no description")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("jupiter_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Jupiter"))
            .SetTitle("Jupiter Nuke")
            .SetDescription("no description")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("mini_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Mini"))
            .SetTitle("Mini Nuke")
            .SetDescription("Tiny Boi nuke")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("death_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Death"))
            .SetTitle("Death Nuke")
            .SetDescription("Its not as serious as the name suggests")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("cobalt_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Cobalt"))
            .SetTitle("Cobalt Nuke")
            .SetDescription("small but deadly")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("nodmg_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/BlueOne"))
            .SetTitle("No Damage nuke")
            .SetDescription("it does nothing. But at least it has a nice effect?")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

             new ButtonBuilder("agrenade_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/AtomicGrenade"))
            .SetTitle("Atomic Grenade")
            .SetDescription("Power of a nuke in the palm of your hands!")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("colorg_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/ColorGrenade"))
            .SetTitle("Color Nuke")
            .SetDescription("Quite a colorful effect for a colorful bomb.")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("eraser_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Eraser"))
            .SetTitle("Eraser Nuke")
            .SetDescription("1 Click and the tiles dissapear. Finnaly working after 3 years (small bug with trees will be fixed in future update)")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("agony_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/MIRV"))
            .SetTitle("Agony Nuke")
            .SetDescription("Lord have mercy on those caught in its radius.")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("ultron_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Ultron"))
            .SetTitle("Ultron Nuke")
            .SetDescription("There is only one path to peace...your extinction.")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            GetRandomProfession();

            new ButtonBuilder("nsa_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/NotSoAtomic"))
            .SetTitle("Not So Atomic Nuke")
            .SetDescription("The atomic nuke that droped out of nuke school to become a " + failedprofesion + " (he failed at that too)")

            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

            new ButtonBuilder("tuuds_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/UniversalDestroyer"))
            .SetTitle("The Unholy Universal Destruction System (TUUDS)")
            .SetDescription("Destroys everything, Deletes the universe itself. This is not reversible, so use it wisely (or just use it for fun, i dont care)")
            .SetPosition(16, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();

        /*    new ButtonBuilder("test_button")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Wut"))
            .SetTitle("Test Bomb")
            .SetDescription("congrats you found the test bomb, good job i guess")
            .SetPosition(17, 1)
            .SetType(ButtonType.GodPower)
            .SetTransform(tab3.transform)
            .Build();
        */

        }
        private void SetupEras()
        {
            PowersTab tab4 = getPowersTab("ModernBoxEras");
            StatManager.Instance.EnableAllErasByDefault();

            new ButtonBuilder("era_no_set")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Primalism"))
            .SetTitle("Normal Era Passing Time")
            .SetDescription("Eras are dynamic, do NOT DM ME WHAT TYHIS MEANS IT IS SELF EXPLAINATORY.")
            .SetPosition(1, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab4.transform)
            .SetFunction(() => {
                StatManager.Instance.SetEra(null);
            })
            .Build();

            new ButtonBuilder("era_mediaval_set")
            .SetSprite(Resources.Load<Sprite>("ui/icons/landTradeDecision"))
            .SetTitle("Override era to Medieval")
            .SetDescription("The era will always stay to medieval, no matter what.")
            .SetPosition(2, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab4.transform)
            .SetFunction(() => {
                StatManager.Instance.SetEra("medieval");
            })
            .Build();

            new ButtonBuilder("era_renaissance_set")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Renaissance"))
            .SetTitle("Override era to Renaissance")
            .SetDescription("The era will always stay to renaissance, no matter what.")
            .SetPosition(3, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab4.transform)
            .SetFunction(() => {
                StatManager.Instance.SetEra("renaissance");
            })
            .Build();

            new ButtonBuilder("era_modern_set")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Tank"))
            .SetTitle("Override era to Modern")
            .SetDescription("The era will always stay to modern, no matter what.")
            .SetPosition(4, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab4.transform)
            .SetFunction(() => {
                StatManager.Instance.SetEra("modern");
            })
            .Build();

            new ButtonBuilder("era_hyperfuture_set")
            .SetSprite(Resources.Load<Sprite>("ui/icons/DankIsGay"))
            .SetTitle("Override era to Hyperfuture")
            .SetDescription("The era will always stay to hyperfuture, no matter what.")
            .SetPosition(5, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab4.transform)
            .SetFunction(() => {
                StatManager.Instance.SetEra("hyperfuture");
            })
            .Build();

            new ButtonBuilder("era_mediaval_toggle")
            .SetSprite(Resources.Load<Sprite>("ui/icons/landTradeDecision"))
            .SetTitle("Toggle Medieval Era")
            .SetDescription("Toggle if this era is allowed to happen or if it is skipped over.")
            .SetPosition(2, 1)
            .SetType(ButtonType.Toggle)
            .SetTransform(tab4.transform)
            .SetFunction(StatManager.Instance.toggleMedieval)
            .Build();

            if (StatManager.Instance.enableMedieval) {
                PowerButtons.ToggleButton("era_mediaval_toggle");
                StatManager.Instance.SetMedievalEnabled(true);
            }

            new ButtonBuilder("era_renaissance_toggle")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Renaissance"))
            .SetTitle("Toggle Renaissance Era")
            .SetDescription("Toggle if this era is allowed to happen or if it is skipped over.")
            .SetPosition(3, 1)
            .SetType(ButtonType.Toggle)
            .SetTransform(tab4.transform)
            .SetFunction(StatManager.Instance.toggleRenaissance)
            .Build();

            if (StatManager.Instance.enableRenaissance) {
                PowerButtons.ToggleButton("era_renaissance_toggle");
                StatManager.Instance.SetRenaissanceEnabled(true);
            }

            new ButtonBuilder("era_modern_toggle")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Tank"))
            .SetTitle("Toggle Modern Era")
            .SetDescription("Toggle if this era is allowed to happen or if it is skipped over.")
            .SetPosition(4, 1)
            .SetType(ButtonType.Toggle)
            .SetTransform(tab4.transform)
            .SetFunction(StatManager.Instance.toggleModern)
            .Build();

            if (StatManager.Instance.enableModern) {
                PowerButtons.ToggleButton("era_modern_toggle");
                StatManager.Instance.SetModernEnabled(true);
            }

            new ButtonBuilder("era_hyperfuture_toggle")
            .SetSprite(Resources.Load<Sprite>("ui/icons/DankIsGay"))
            .SetTitle("Toggle Hyperfuture Era")
            .SetDescription("Toggle if this era is allowed to happen or if it is skipped over.")
            .SetPosition(5, 1)
            .SetType(ButtonType.Toggle)
            .SetTransform(tab4.transform)
            .SetFunction(StatManager.Instance.toggleHyperfuture)
            .Build();

            if (StatManager.Instance.enableHyperfuture) {
                PowerButtons.ToggleButton("era_hyperfuture_toggle");
                StatManager.Instance.SetHyperfutureEnabled(true);
            }
        }

        private void SetupSpace()
        {
          PowersTab tab = getPowersTab("ModernBoxSpace");

            new ButtonBuilder("galaxy")
            .SetSprite(Resources.Load<Sprite>("Stars/Bluegiant"))
            .SetTitle("Starmap")
            .SetDescription("Visit other planets and stars!")
            .SetPosition(6, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(openStarMap)
            .Build();

            new ButtonBuilder("beginningtoggle")
            .SetSprite(Resources.Load<Sprite>("Stars/Phantomstar"))
            .SetTitle("Persistence")
            .SetDescription("Persistence makes it so you continue on the planet you were after restarting the game, instead of generating a new world.")
            .SetPosition(6, 0)
            .SetType(ButtonType.Toggle)
            .SetTransform(tab.transform)
            .SetFunction(BitchBalls_Patch.togglePersistence)
            .Build();

            if (Main.savedSettings.boolOptions["PersistenceOption"]) {
                PowerButtons.ToggleButton("beginningtoggle");
                BitchBalls_Patch.togglePersistence();
            }

            new ButtonBuilder("customgalaxies")
            .SetSprite(Resources.Load<Sprite>("Stars/Neutronstar"))
            .SetTitle("Custom Galaxies")
            .SetDescription("Manage custom galaxies!")
            .SetPosition(6, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(openCustomGalaxies)
            .Build();

            new ButtonBuilder("colonize")
            .SetSprite(Resources.Load<Sprite>("ui/icons/F55"))
            .SetTitle("Colonize")
            .SetDescription("Bring Units to other planets!")
            .SetPosition(6, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(comingSoon)
            .Build();

            new ButtonBuilder("colonizewhat")
            .SetSprite(Resources.Load<Sprite>("ui/icons/Future"))
            .SetTitle("Unknown")
            .SetDescription("To be revealed....")
            .SetPosition(6, 0)
            .SetType(ButtonType.Click)
            .SetTransform(tab.transform)
            .SetFunction(comingSoon)
            .Build();
        }
        private static void openStarMap() {
			SpaceManager.EnableSpace();
         	Debug.Log("SpaceBox: openStarMap has been called but the star map ain't actually fucking showing up. (ofc it isn't)");
		  }

        private static void comingSoon() {
            WorldTip.showNow("Coming soon. Maybe", true, "top", 3f);
        }
        private static void openAboutWindow() {

			 Windows.ShowWindow("AboutWindow");
             }
		  private static void openCustomGalaxies() {

			 Windows.ShowWindow("CustomGalaxiesWindow");
		  }
             private static void openInfiniteBoxWindow() {

                Application.OpenURL("https://tuxxego.com/infinitebox");
             }

        private static void openNothing() {

                ModernBoxLogger.Log("There's nothing here!");
             }

        private static void openModernBoxSubTab(string tabID)
        {
            TabBuilder.SwitchTab(tabID, "ModernBoxTab");
        }

        private static void openModernBoxHub()
        {
            TabBuilder.SwitchTab("ModernBoxTab");
        }

        private static void openTrainboxTab()
        {
            global::Trainbox.TrainPowers.OpenTrainboxTab();
        }

        private static void EnsureOtherTabButton(string id, string spritePath, string title, string description, int gridX, int gridY, Transform parent, UnityAction action)
        {
            if (GameObjects.FindEvenInactive(id) != null)
            {
                return;
            }

            new ButtonBuilder(id)
                .SetSprite(Resources.Load<Sprite>(spritePath))
                .SetTitle(title)
                .SetDescription(description)
                .SetPosition(gridX, gridY)
                .SetType(ButtonType.Click)
                .SetTransform(parent)
                .SetFunction(action)
                .Build();
        }

        private static void openAchievmentsWindow() {

			 Windows.ShowWindow("AchievementsWindow");
		  }

          private static void opendaSetup() {
            GameObject setupObj = new GameObject("SetupManager");
            PlayerPrefs.DeleteKey("FirstTimeSetupDone6");
            setupObj.AddComponent<FirstTimeSetup>();
        }

          public static PowersTab getPowersTab(string id) {
            GameObject gameObject = GameObjects.FindEvenInactive(id);
            return gameObject != null ? gameObject.GetComponent<PowersTab>() : null;
        }

        public static bool Stuff_Drop(WorldTile pTile, GodPower pPower)
		{
            AssetManager.powers.CallMethod("spawnDrops", pTile, pPower);
			return true;
		}
    }
    public static class InsertLine
    {

        public static void At(int gridX, Transform parent)
        {
            float x = 72 + (18 * gridX);

            GameObject line = new GameObject("NiceIfYoufoundthisDMTuxTheWord'Klopple'", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform lineRTF = line.GetComponent<RectTransform>();
            Image lineImage = line.GetComponent<Image>();

            lineImage.sprite = Resources.Load<Sprite>("ui/DAline.png");
            lineRTF.sizeDelta = new Vector2(16, 86);
            lineRTF.anchoredPosition = new Vector2(x, 0);
            lineRTF.localScale = Vector3.one;
            lineRTF.anchorMin = new Vector2(0, 0.5f);
            lineRTF.anchorMax = new Vector2(0, 0.5f);
            lineRTF.pivot = new Vector2(0.5f, 0.5f);

            UnityEngine.Object.Instantiate(line, parent);
        }

        public static void Space(int gridX, Transform parent)
        {
            float x = 72 + (18 * gridX);

            GameObject line = new GameObject("_line", typeof(RectTransform), typeof(CanvasRenderer));

            UnityEngine.Object.Instantiate(line, parent);
        }
    }
}
