using HarmonyLib;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ModernBox
{
    [HarmonyPatch(typeof(MapBox), "OnApplicationQuit")]
    public class AutoSaveOnQuit_Patch
    {
        public static void Prefix()
        {
            if (PlanetManager.instance == null) return;

            string planetName = PlanetManager.instance.GetCurrentPlanet();
            if (string.IsNullOrEmpty(planetName)) return;

            string savePath = GetPlanetFolderPath(planetName);

            if (!string.IsNullOrEmpty(savePath))
            {
                    Debug.Log($"ModernBox: Auto-saving [{planetName}] to: {savePath}");
                    SaveManager.saveWorldToDirectory(savePath);
            }
        }

        private static string GetPlanetFolderPath(string planetName)
        {
            string persistentPath = Application.persistentDataPath;
            string[] possibleRoots = { "ModernBox", "modernbox" };
            
            foreach (string rootName in possibleRoots)
            {
                string rootPath = Path.Combine(persistentPath, rootName);
                if (!Directory.Exists(rootPath)) continue;

                string[] allDirs = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
                foreach (string dir in allDirs)
                {
                    if (Path.GetFileName(dir).Trim().Equals(planetName.Trim(), StringComparison.OrdinalIgnoreCase))
                        return dir;
                }
            }

            string systemName = Regex.Replace(planetName, @"\s+[IVXLCDM]+\s*$", "", RegexOptions.IgnoreCase).Trim();
            Debug.Log($"ModernBox: Planet folder not found. Searching for system folder: [{systemName}]");

            foreach (string rootName in possibleRoots)
            {
                string rootPath = Path.Combine(persistentPath, rootName);
                if (!Directory.Exists(rootPath)) continue;

                string[] allDirs = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
                foreach (string dir in allDirs)
                {
                    if (Path.GetFileName(dir).Trim().Equals(systemName, StringComparison.OrdinalIgnoreCase))
                    {
                        string newPlanetPath = Path.Combine(dir, planetName.Trim());
                        Directory.CreateDirectory(newPlanetPath);
                        Debug.Log($"ModernBox: Found system folder. Created new planet folder at: {newPlanetPath}");
                        return newPlanetPath;
                    }
                }
            }

            string fallbackPath = Path.Combine(persistentPath, "ModernBox", "Saves", systemName, planetName.Trim());
            Directory.CreateDirectory(fallbackPath);
            Debug.Log($"ModernBox: System folder not found. Created fallback path: {fallbackPath}");
            return fallbackPath;
        }
    }
}