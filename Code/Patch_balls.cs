using HarmonyLib;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using NCMS.Utils;
using NCMS;

namespace ModernBox
{
    [HarmonyPatch(typeof(MapBox), "addLastStep")]
    public class BitchBalls_Patch
    {
        public static bool gameBegun = false;
        public static bool allowed;

        public static void turnOnPersistence() => allowed = true;
        public static void turnOffPersistence() => allowed = false;

        public static void togglePersistence() {
        Main.modifyBoolOption("PersistenceOption",
                                PowerButtons.GetToggleValue("beginningtoggle"));
        if (PowerButtons.GetToggleValue("beginningtoggle")) {
            turnOnPersistence();
            return;
        }
        turnOffPersistence();
        }

        public static void Postfix()
        {
            if (!allowed) return;

            if (PlayerPrefs.GetInt("DoneBefore", 0) == 0)
            {
                PlayerPrefs.SetInt("DoneBefore", 1);
                PlayerPrefs.Save();
                SpaceManager.startup = true;
                SpaceManager.EnableSpace();
            }

            WindowPreloaderUtils.ForceClearPreloader();

            if (!gameBegun)
            {
                string planetName = PlanetManager.instance.GetCurrentPlanet();
                if (string.IsNullOrEmpty(planetName)) return;

                string savePath = GetPlanetFolderPath(planetName);

                if (!string.IsNullOrEmpty(savePath) && File.Exists(Path.Combine(savePath, "map.wbox")))
                {
                    SaveManager manager = UnityEngine.Object.FindObjectOfType<SaveManager>();
                    if (manager != null)
                    {
                        Debug.Log($"ModernBox: Loading planet {planetName} from {savePath}");
                        gameBegun = true;
                        manager.loadWorld(savePath); 
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning($"ModernBox: No existing save for {planetName}. Proceeding with new generation.");
                    if (string.IsNullOrEmpty(Config.current_map_template) || Config.current_map_template == "none")
                    {
                        Config.current_map_template = "boring_plains"; 
                    }
                    
                    gameBegun = true; 
                }
            }
        }
        private static string GetPlanetFolderPath(string planetName)
        {
            string spaceBoxPath = Path.Combine(Application.persistentDataPath, "ModernBox");
            
            if (!Directory.Exists(spaceBoxPath)) return null;

            string[] allDirectories = Directory.GetDirectories(spaceBoxPath, "*", SearchOption.AllDirectories);

            foreach (string directory in allDirectories)
            {
                if (Path.GetFileName(directory).Equals(planetName, StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
            }

            return null;
        }
    }
}