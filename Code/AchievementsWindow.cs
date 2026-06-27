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
    class AchievementsWindow
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
          window = Windows.CreateNewWindow("AchievementsWindow", "ModernBox");
          var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
          scrollView.gameObject.SetActive(true);
          var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
          var viewportRect = viewport.GetComponent<RectTransform>();
          viewportRect.sizeDelta = new Vector2(0, 17);
          content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
			
			          string gold = "#FFD700";
					  string Dgold = "#ffae00";
					  var description =
			@"<color='" + gold + @"'>Here's a list of achievements!</color>
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
					  
					  PowerButton ResetButtonM2 = PowerButtons.CreateButton("ResetButtonM2", Resources.Load<Sprite>("ui/Icons/Reset"), "Reset Progress", "Delete all achievement Data.", new Vector2(132, MoveDown*4), ButtonType.Click, content.transform, AchievementManager.Instance.ResetAchievements);	
								
						List<M3Achievement> m3chievements = AchievementManager.Instance.GetAllAchievements();
						int count = 0;
						int xOffset = 0;
						int yOffset = 0;

						foreach (var achievement in m3chievements)
						{
							bool unlocked = AchievementManager.Instance.IsAchievementUnlocked(achievement.ID);
							string spritePath = achievement.SpritePath;
							string achievementName = unlocked ? achievement.Name : "???";
							string achievementDescription = unlocked ? achievement.Description : "???";

							Vector2 position = new Vector2(60 + xOffset, MoveDown + yOffset);
							PowerButtons.CreateButton(achievement.ID, Resources.Load<Sprite>(spritePath), achievementName, achievementDescription, position, ButtonType.Click, content.transform, null);

							count++;
							xOffset += 36;
							if (count % 5 == 0)
							{
								yOffset -= 36;
								xOffset = 0;
							}
						}

        }
		
		public static bool EditButton(string buttonID, string newLocalName, string newLocalDescription)
		{
			if (PowerButtons.CustomButtons.TryGetValue(buttonID, out PowerButton targetButton))
			{
				Localization.AddOrSet(buttonID, newLocalName);
				Localization.AddOrSet(buttonID + " Description", newLocalDescription);

				return true;
			}

			ModernBoxLogger.Error($"PowerButton with name '{buttonID}' not found.");
			return false;
		}


  }
}
