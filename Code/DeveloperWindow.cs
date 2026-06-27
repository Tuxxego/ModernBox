//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReflectionUtility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NCMS.Utils;
using System.Text.RegularExpressions;
using System.Reflection;


namespace ModernBox
{
    class DeveloperWindow
    {
        public static void init()
        {
          var window = Windows.CreateNewWindow("DeveloperWindow", "ModernBox");
          var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
          scrollView.gameObject.SetActive(true);
          var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
          var viewportRect = viewport.GetComponent<RectTransform>();
          viewportRect.sizeDelta = new Vector2(0, 17);
          var content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
          string gold = "#FFD700";
          string Dgold = "#ffae00";
          var description =
@"<color='" + gold + @"'>[Developer Mode activated ]</color>

NOTE: when developer mode is active, all non duplicate errors are sent to a private discord channel, all that is sent is the error and stack trace, no ip, screenshots, etc, this is mainly meant for the devs so we can patch bugs during updates.";
          var name = window.transform.Find("Background").Find("Name").gameObject;
          var nameText = name.GetComponent<Text>();
          nameText.text = description;
          nameText.color = new Color(0.9f, 0.6f, 0, 1);
          nameText.fontSize = 10;
          nameText.alignment = TextAnchor.UpperCenter;
          nameText.supportRichText = true;
          name.transform.SetParent(window.transform.Find("Background").Find("Scroll View").Find("Viewport").Find("Content"));
          name.SetActive(true);
          var nameRect = name.GetComponent<RectTransform>();
          nameRect.anchorMin = new Vector2(0.5f, 1);
          nameRect.anchorMax = new Vector2(0.5f, 1);
          nameRect.offsetMin = new Vector2(-90f, nameText.preferredHeight * -1);
          nameRect.offsetMax = new Vector2(90f, -17);
          nameRect.sizeDelta = new Vector2(180, nameText.preferredHeight + 50);
          window.GetComponent<RectTransform>().sizeDelta = new Vector2(0, nameText.preferredHeight + 50);
          name.transform.localPosition = new Vector2(name.transform.localPosition.x, ((nameText.preferredHeight / 2) + 30) * -1);
			Sprite imageSprite = Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare");

			
			GameObject imageGO = new GameObject("ImageGO");
			imageGO.transform.SetParent(content.transform); 


			Image imageComponent = imageGO.AddComponent<Image>();
			imageComponent.sprite = imageSprite;


			RectTransform imageRect = imageGO.GetComponent<RectTransform>();
			imageRect.anchoredPosition = new Vector2(0, -100); 
			imageRect.sizeDelta = new Vector2(200, 200); 
			
			
			
			
        }
  }
}
