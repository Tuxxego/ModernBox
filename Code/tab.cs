//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace ModernBox
{
    class tab
    {
        public static void init()
        {}
        public static void createTab(string buttonID, string tabID, string name, string desc, int xPos)
        {
            GameObject OtherTabButton = GameObjects.FindEvenInactive("Button_Other");
            if (OtherTabButton != null)
            {
                Localization.AddOrSet(buttonID, name);
                Localization.AddOrSet($"{buttonID} Description", desc);
                Localization.AddOrSet("Tuxxego_mod_creator",  "Made By Tuxxego");
                Localization.AddOrSet(tabID, name);


                GameObject newTabButton = GameObject.Instantiate(OtherTabButton);
                newTabButton.transform.SetParent(OtherTabButton.transform.parent);
                Button buttonComponent = newTabButton.GetComponent<Button>();
                TipButton tipButton = buttonComponent.gameObject.GetComponent<TipButton>();
                tipButton.textOnClick = buttonID;
                tipButton.textOnClickDescription = $"{buttonID} Description";
                tipButton.text_description_2 = "Tuxxego_mod_creator";



                newTabButton.transform.localPosition = new Vector3(150f, 49.57f);
                newTabButton.transform.localScale = new Vector3(1f, 1f);
                newTabButton.name = buttonID;

                var spriteForTab = Resources.Load<Sprite>("ui/icons/tabIconModernWarfare");
                newTabButton.transform.Find("Icon").GetComponent<Image>().sprite = spriteForTab;


                GameObject OtherTab = GameObjects.FindEvenInactive("Tab_Other");
                foreach (Transform child in OtherTab.transform)
                {
                    child.gameObject.SetActive(false);
                }

                GameObject additionalPowersTab = GameObject.Instantiate(OtherTab);

                foreach (Transform child in additionalPowersTab.transform)
                {
                    if (child.gameObject.name == "tabBackButton" || child.gameObject.name == "-space")
                    {
                        child.gameObject.SetActive(true);
                        continue;
                    }

                    GameObject.Destroy(child.gameObject);
                }

                foreach (Transform child in OtherTab.transform)
                {
                    child.gameObject.SetActive(true);
                }


                additionalPowersTab.transform.SetParent(OtherTab.transform.parent);
                PowersTab powersTabComponent = additionalPowersTab.GetComponent<PowersTab>();
                powersTabComponent.powerButton = buttonComponent;
                powersTabComponent.powerButtons.Clear();


                additionalPowersTab.name = tabID;
                powersTabComponent.powerButton.onClick = new Button.ButtonClickedEvent();
                powersTabComponent.powerButton.onClick.AddListener(() => tabOnClick(tabID));
                Reflection.SetField<GameObject>(powersTabComponent, "parentObj", OtherTab.transform.parent.parent.gameObject);

                additionalPowersTab.SetActive(true);
                powersTabComponent.powerButton.gameObject.SetActive(true);
            }
        }

        public static void tabOnClick(string tabID)
        {
            GameObject AdditionalTab = GameObjects.FindEvenInactive(tabID);
            PowersTab AdditionalPowersTab = AdditionalTab.GetComponent<PowersTab>();

            AdditionalPowersTab.showTab(AdditionalPowersTab.powerButton);
        }
    }
}