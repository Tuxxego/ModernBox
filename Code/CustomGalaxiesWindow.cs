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
        private static bool galaxyStateWritable = true;

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

        // Manu-Fix 030: optional files must not abort Main.Awake before the mod's
        // units and buttons load. Validate each entry and preserve unreadable state.
        internal static List<Galaxy> LoadGalaxies()
        {
            string galaxiesDirectory = Path.Combine(Application.dataPath, "../galaxies/");
            List<Galaxy> galaxies = new List<Galaxy>();
            var names = new HashSet<string>(StarManager.BuiltinGalaxyNames, StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(galaxiesDirectory))
            {
                string[] galaxyFiles;
                try { galaxyFiles = Directory.GetFiles(galaxiesDirectory, "*.gal"); }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ModernBox Fix030] Cannot read optional galaxy directory: {ex.Message}");
                    return galaxies;
                }
                foreach (var file in galaxyFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        Galaxy galaxy = JsonConvert.DeserializeObject<Galaxy>(json);
                        string reason;
                        if (!ValidGalaxy(galaxy, out reason))
                        {
                            Debug.LogWarning($"[ModernBox Fix030] Skipping galaxy file '{Path.GetFileName(file)}': {reason}");
                            continue;
                        }
                        if (!names.Add(galaxy.name))
                        {
                            Debug.LogWarning($"[ModernBox Fix030] Skipping duplicate galaxy name in '{Path.GetFileName(file)}'.");
                            continue;
                        }
                        galaxies.Add(galaxy);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ModernBox Fix030] Skipping unreadable galaxy file '{Path.GetFileName(file)}': {ex.Message}");
                    }
                }
            }

            return galaxies;
        }

        private static bool ValidGalaxy(Galaxy galaxy, out string reason)
        {
            reason = null;
            if (galaxy == null || string.IsNullOrWhiteSpace(galaxy.name))
                reason = "missing galaxy object or name";
            else if (galaxy.name == "." || galaxy.name == ".." || galaxy.name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                reason = "galaxy name is not a valid folder name";
            else if (galaxy.starCount <= 0)
                reason = "starCount must be positive";
            else if (galaxy.starWeights == null || galaxy.starWeights.Count == 0 || galaxy.starWeights.Count > StarManager.StarTypeCount ||
                galaxy.starWeights.Any(value => float.IsNaN(value) || float.IsInfinity(value) || value < 0) ||
                !galaxy.starWeights.Any(value => value > 0) || float.IsInfinity(galaxy.starWeights.Sum()))
                reason = $"starWeights must contain 1 to {StarManager.StarTypeCount} finite, non-negative entries with a finite positive total weight";
            else if (!ValidColor(galaxy.nebulaColor1) || !ValidColor(galaxy.nebulaColor2))
                reason = "nebula colors must contain four finite components when supplied";
            return reason == null;
        }

        private static bool ValidColor(List<float> color)
        {
            return color == null || (color.Count == 4 && color.All(value => !float.IsNaN(value) && !float.IsInfinity(value)));
        }

        private static void SaveGalaxiesState(Dictionary<string, bool> galaxyStates)
        {
            if (!galaxyStateWritable)
            {
                Debug.LogWarning("[ModernBox Fix030] Galaxy state was unreadable; preserving the existing Galaxies.json file.");
                return;
            }
            string path = Path.Combine(Application.dataPath, "Galaxies.json");
            string json = JsonConvert.SerializeObject(galaxyStates, Formatting.Indented);
            try { File.WriteAllText(path, json); }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ModernBox Fix030] Cannot save optional galaxy state: {ex.Message}");
            }
        }

		internal static Dictionary<string, bool> LoadGalaxiesState()
		{
			string path = Path.Combine(Application.dataPath, "Galaxies.json");
			galaxyStateWritable = true;
			if (File.Exists(path))
			{
				try
				{
					string json = File.ReadAllText(path);
					var states = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
					if (states == null) throw new JsonSerializationException("expected a galaxy-state object, got null");
					return states;
				}
				catch (Exception ex)
				{
					galaxyStateWritable = false;
					Debug.LogWarning($"[ModernBox Fix030] Cannot read Galaxies.json; keeping the file unchanged and custom galaxies disabled: {ex.Message}");
					return new Dictionary<string, bool>();
				}
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
            window = ModernBox.ModernBoxLocale.Window("CustomGalaxiesWindow", "ModernBox");
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
                // Display names are not globally unique NCMS button IDs.
                string buttonId = "modernbox_custom_galaxy_" + count;
                while (AssetManager.powers.has(buttonId) || PowerButtons.CustomButtons.ContainsKey(buttonId) || PowerButtons.ToggleValues.ContainsKey(buttonId))
                    buttonId += "_custom";
                string galaxyDescription = galaxy.description ?? galaxyName;

                Vector2 position = new Vector2(60 + xOffset, MoveDown + yOffset);
				PowerButton toggleButton = ModernBox.ModernBoxLocale.Button(buttonId,
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
