using System;
using UnityEngine;
using UnityEngine.UI;
using NCMS;
using NCMS.Utils;
// A portion of this file is derived from NCMS (made by Nikon)
namespace ModernBox
{
    public class TabBuilder
    {
        private string buttonID;
        private string tabID;
        private string name;
        private string description;
        private bool black;
        private int xPos;
        private Sprite icon;
        private bool toolbarButtonVisible = true;
        private string repeatPressReturnTabID;

        public TabBuilder SetTabID(string id)
        {
            tabID = id;
            buttonID = id + "FuckYou";
            return this;
        }
        public TabBuilder SetName(string tabName)
        {
            name = tabName;
            return this;
        }
        public TabBuilder isAfrican(bool isAfrican)
        {
            black = true;
            return this;
        }
        public TabBuilder SetDescription(string tabDescription)
        {
            description = tabDescription;
            return this;
        }
        public TabBuilder SetPosition(int positionX)
        {
            xPos = positionX;
            return this;
        }
        public TabBuilder SetIcon(string resourcePath)
        {
        
            icon = Resources.Load<Sprite>(resourcePath);
            return this;
        }

        public TabBuilder SetToolbarButtonVisible(bool isVisible)
        {
            toolbarButtonVisible = isVisible;
            return this;
        }

        public TabBuilder SetRepeatPressReturnTab(string tabID)
        {
            repeatPressReturnTabID = tabID;
            return this;
        }

        public void Build()
        {
            GameObject otherTabButton = FindAllGameObjectsCreditToNikonForThisFunctionBTW("button_other");
            if (otherTabButton == null)
            {
                ModernBoxLogger.Error("Error: Could not find 'Button_Other' to clone for tab creation.");
                return;
            }
                Localization.AddOrSet(buttonID, name);
                Localization.AddOrSet($"{buttonID} Description", description);
                Localization.AddOrSet("Tuxxego_mod_creator",  "Mhm. Yes, this mod was made by Tuxxego.");
                Localization.AddOrSet(tabID, name);
            GameObject newTabButton = GameObject.Instantiate(otherTabButton);
            newTabButton.transform.SetParent(otherTabButton.transform.parent);
            var drag = newTabButton.GetComponent<DragOrderElement>();
            if (drag != null)
            {
                GameObject.Destroy(drag);
            }
            Image img = newTabButton.GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>("ui/cool_button");
            newTabButton.name = buttonID;
            Button buttonComponent = newTabButton.GetComponent<Button>();
            buttonComponent.image = img;
            buttonComponent.spriteState = new SpriteState
            {
                highlightedSprite = Resources.Load<Sprite>("ui/cool_button"),
                pressedSprite = Resources.Load<Sprite>("ui/cool_button"),
                disabledSprite = Resources.Load<Sprite>("ui/cool_button")
            };
            newTabButton.AddComponent<GayAndStupid>().forcedSprite = Resources.Load<Sprite>("ui/cool_button");
            TipButton tipButton = newTabButton.GetComponent<TipButton>();
            tipButton.textOnClick = buttonID;
            tipButton.textOnClickDescription = $"{buttonID} Description";
            tipButton.text_description_2 = "Tuxxego_mod_creator";
            float toolbarX = xPos;
            if (toolbarButtonVisible)
            {
                toolbarX = otherTabButton.transform.localPosition.x + 36f;
                newTabButton.transform.SetSiblingIndex(otherTabButton.transform.GetSiblingIndex() + 1);
            }

            newTabButton.transform.localPosition = new Vector3(toolbarX, otherTabButton.transform.localPosition.y, 0f);
            newTabButton.transform.localScale = Vector3.one;
            if (icon != null)
            {
                newTabButton.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            }
            GameObject otherTab = FindAllGameObjectsCreditToNikonForThisFunctionBTW("other");
            if (otherTab == null)
            {
                ModernBoxLogger.Error("Error: Could not find 'Tab_Other' to clone for tab creation.");
                return;
            }
            foreach (Transform child in otherTab.transform)
            {
                child.gameObject.SetActive(false);
            }
            GameObject newTab = GameObject.Instantiate(otherTab);
            foreach (Transform child in newTab.transform)
            {
                if (child.gameObject.name == "tabBackButton" || child.gameObject.name == "-space")
                {
                    child.gameObject.SetActive(true);
                    continue;
                }
                GameObject.Destroy(child.gameObject);
            }

            if (black)
             {

                SpriteRenderer mainSR = newTab.GetComponent<SpriteRenderer>();
                if (mainSR != null) 
                {
                    mainSR.color = Color.black;
                }

               
                UnityEngine.UI.Image mainImg = newTab.GetComponent<UnityEngine.UI.Image>();
                if (mainImg != null) 
                {
                    mainImg.color = Color.black;
                }
             }

            foreach (Transform child in otherTab.transform)
            {
                child.gameObject.SetActive(true);
            }
            newTab.transform.SetParent(otherTab.transform.parent);
            newTab.name = tabID;
            PowersTab powersTabComponent = newTab.GetComponent<PowersTab>();
            powersTabComponent.powerButton = buttonComponent;
            powersTabComponent._power_buttons.Clear();
            powersTabComponent.powerButton.onClick = new Button.ButtonClickedEvent();
            powersTabComponent.powerButton.onClick.AddListener(() => SwitchTab(tabID, repeatPressReturnTabID));
            newTab.SetActive(true);
            powersTabComponent.powerButton.gameObject.SetActive(toolbarButtonVisible);

            var asset = new PowerTabAsset
            {
                id = tabID,
                locale_key = "tab_modernbox",
                tab_type_main = toolbarButtonVisible,
                get_power_tab = () => powersTabComponent
            };
            AssetManager.power_tab_library.add(asset);
            powersTabComponent._asset = asset;

        }

        public static void SwitchTab(string tabID)
        {
            SwitchTab(tabID, null);
        }

        public static void SwitchTab(string tabID, string returnToTabID)
        {
            PowersTab activeTab = PowersTab.getActiveTab();
            if (TryGetPowersTab(tabID, out PowersTab targetTab))
            {
                if (activeTab == targetTab && !string.IsNullOrEmpty(returnToTabID))
                {
                    ShowTab(returnToTabID);
                    return;
                }

                targetTab.showTab(targetTab.powerButton);
            }
        }

        private static void ShowTab(string tabID)
        {
            if (TryGetPowersTab(tabID, out PowersTab powersTabComponent))
            {
                powersTabComponent.showTab(powersTabComponent.powerButton);
            }
        }

        private static bool TryGetPowersTab(string tabID, out PowersTab powersTab)
        {
            powersTab = null;
            GameObject additionalTab = FindAllGameObjectsCreditToNikonForThisFunctionBTW(tabID);
            if (additionalTab == null)
            {
                return false;
            }

            powersTab = additionalTab.GetComponent<PowersTab>();
            return powersTab != null;
        }

        // FindAllGameObjectsCreditToNikonForThisFunctionBTW is from NCMS (made by Nikon)
        public static GameObject FindAllGameObjectsCreditToNikonForThisFunctionBTW(string Name)
        {
            GameObject[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int index = 0; index < objectsOfTypeAll.Length; ++index)
            {
                if (objectsOfTypeAll[index].gameObject.gameObject.name == Name)
                    return objectsOfTypeAll[index];
            }
            return (GameObject)null;
        }
    }
}
