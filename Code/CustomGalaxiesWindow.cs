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
using System.IO;
using Newtonsoft.Json;

namespace ModernBox
{
    class CustomGalaxiesWindow
    {
        public static int MoveDown = -50;
        private static ScrollWindow window;
        private static GameObject content;

        public class Galaxy
        {
            public string name;
            public string description;
            public int requirement;
            public int starCount;
            public int dangerRating;
            public List<float> starWeights;
            public List<float> nebulaColor1;
            public List<float> nebulaColor2;
            public bool GlassStructure;
        }

        private static List<Galaxy> LoadGalaxies()
        {
            string galaxiesDirectory = Path.Combine(Application.dataPath, "../galaxies/");
            List<Galaxy> galaxies = new List<Galaxy>();

            if (Directory.Exists(galaxiesDirectory))
            {
                string[] galaxyFiles = Directory.GetFiles(galaxiesDirectory, "*.gal");
                foreach (var file in galaxyFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        Galaxy galaxy = JsonConvert.DeserializeObject<Galaxy>(json);
                        galaxies.Add(galaxy);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error loading galaxy file {file}: {ex.Message}");
                    }
                }
            }

            return galaxies;
        }

        private static void SaveGalaxiesState(Dictionary<string, bool> galaxyStates)
        {
            string path = Path.Combine(Application.dataPath, "Galaxies.json");
            string json = JsonConvert.SerializeObject(galaxyStates, Formatting.Indented);
            File.WriteAllText(path, json);
        }

		private static Dictionary<string, bool> LoadGalaxiesState()
		{
			string path = Path.Combine(Application.dataPath, "Galaxies.json");
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				return JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
			}
			else
			{

				List<Galaxy> galaxies = LoadGalaxies();
				Dictionary<string, bool> galaxyStates = new Dictionary<string, bool>();
				foreach (var galaxy in galaxies)
				{
					galaxyStates[galaxy.name] = true; 
				}
				SaveGalaxiesState(galaxyStates); 
				return galaxyStates;
			}
		}

        public static void init()
        {
            PowersTab tab = getPowersTab("ModernBox");
            window = Windows.CreateNewWindow("CustomGalaxiesWindow", "ModernBox");
            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);
            var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.sizeDelta = new Vector2(0, 17);
            content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");

            string gold = "#FFD700";
            string Dgold = "#ffae00";
            var description =
            @"<color='" + gold + @"'>Toggle custom galaxies you have installed, to install some or create your own, join the discord server.</color>
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

            List<Galaxy> galaxies = LoadGalaxies();
            Dictionary<string, bool> galaxyStates = LoadGalaxiesState();

            int count = 0;
            int xOffset = 0;
            int yOffset = -36;

            foreach (var galaxy in galaxies)
            {
                bool isToggled = galaxyStates.ContainsKey(galaxy.name) ? galaxyStates[galaxy.name] : false;
                string galaxyName = galaxy.name;
                string galaxyDescription = galaxy.description;

                Vector2 position = new Vector2(60 + xOffset, MoveDown + yOffset);
				PowerButton toggleButton = PowerButtons.CreateButton(galaxyName, 
					Resources.Load<Sprite>("ui/Icons/Galaxy"), 
					galaxyName, 
					galaxyDescription, 
					position, 
					ButtonType.Toggle, 
					content.transform, 
					new UnityAction(() =>
					{

						isToggled = !isToggled;
						galaxyStates[galaxyName] = isToggled;
						SaveGalaxiesState(galaxyStates); 
					})
				);

                if (isToggled)
                {

                    PowerButtons.ToggleButton(toggleButton.name);
                }

                count++;
                xOffset += 36;
                if (count % 5 == 0)
                {
                    yOffset -= 36;
                    xOffset = 0;
                }
            }
        }

        private static PowersTab getPowersTab(string id)
        {
            GameObject gameObject = GameObjects.FindEvenInactive(id);
            return gameObject.GetComponent<PowersTab>();
        }
    }
}