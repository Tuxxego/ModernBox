//========= MODERNBOX 2.2.0.0 ============//
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
    class SpaceWindow
    {
		public static int MoveDown = -50;
        private static ScrollWindow window;
        private static GameObject content;

	  private static PowersTab getPowersTab(string id) {
		GameObject gameObject = GameObjects.FindEvenInactive(id);
		return gameObject.GetComponent<PowersTab>();
	  }
        public static void init()
        {
			    PowersTab tab = getPowersTab("ModernBox");
          window = Windows.CreateNewWindow("SpaceWindow", "ModernBox");
          var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
          scrollView.gameObject.SetActive(true);
          var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
          var viewportRect = viewport.GetComponent<RectTransform>();
          viewportRect.sizeDelta = new Vector2(0, 17);
          content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");

			          string gold = "#FFD700";
					  string Dgold = "#ffae00";
					  var description =
			@"<color='" + gold + @"'>With M3 you can travel to other planets and even galaxies!</color>
			";
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

					  PowerButton Starmap = PowerButtons.CreateButton("OpenStarmap", Resources.Load<Sprite>("ui/Icons/Galaxy"), "Open Starmap", "Visit other planets, stars, and galaxies.", new Vector2(96, MoveDown), ButtonType.Click, content.transform, openStarMap);
					  PowerButton CustomGalaxies = PowerButtons.CreateButton("CustomGalaxies", Resources.Load<Sprite>("ui/Icons/DiesIraeIcon"), "Custom Galaxies", "Toggle the custom galaxies you have installed.", new Vector2(168, MoveDown), ButtonType.Click, content.transform, openCustomGalaxies);
					  PowerButton GalaxyHub = PowerButtons.CreateButton("GalaxyHub", Resources.Load<Sprite>("ui/Icons/DiesIraeIcon"), "GalaxyHub", "Download and install community created galaxies!", new Vector2(96, MoveDown*2), ButtonType.Click, content.transform, null);	
        }

		    private static void openStarMap() {

			SpaceManager.EnableSpace();
         	Debug.Log("SpaceBox: openStarMap has been called but the star map ain't actually fucking showing up. (ofc it isn't)");

		}

		  private static void openCustomGalaxies() {

			 Windows.ShowWindow("CustomGalaxiesWindow");
		  }

  }
}