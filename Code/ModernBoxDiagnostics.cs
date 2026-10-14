using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    internal static class ModernBoxDiagnostics
    {
        private static readonly HashSet<string> Once = new HashSet<string>(StringComparer.Ordinal);

        // M1 and M2 use the same unit, building and power ids, so only one can be loaded.
        internal static string FindAssetConflict()
        {
            string[] probes = { "MIRVBomber", "AirFactory", "MOABbutton", "Parts" };
            foreach (string id in probes)
            {
                if ((AssetManager.actor_library != null && AssetManager.actor_library.has(id)) ||
                    (AssetManager.buildings != null && AssetManager.buildings.has(id)) ||
                    (AssetManager.powers != null && AssetManager.powers.has(id)) ||
                    (AssetManager.resources != null && AssetManager.resources.has(id)))
                    return "Another ModernBox is already loaded (found '" + id + "'). Turn it off and restart.";
            }
            return null;
        }

        // Only printed with Developer Mode on.
        internal static void Info(string message)
        {
            if (ModernBoxSettings.Get("Developer_Mode")) Debug.Log("[ModernBox] " + message);
        }

        internal static void Warn(string message)
        {
            string line = "[ModernBox] WARN: " + message;
            if (Once.Add(line)) Debug.LogWarning(line);
        }

        internal static void Error(string message)
        {
            string line = "[ModernBox] ERROR: " + message;
            if (Once.Add(line)) Debug.LogError(line);
        }

        // Every sprite the content points at must be in the mod folder.
        internal static void ValidateAssets()
        {
            string root = Path.Combine(ModernBoxMod.ModFolder, "GameResources");
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
                foreach (string frame in new HashSet<string>(spec.Walk))
                    if (!File.Exists(Path.Combine(root, "actors", spec.Id, frame + ".png"))) Warn("Missing frame actors/" + spec.Id + "/" + frame);
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
                if (!Directory.Exists(Path.Combine(root, "buildings", spec.Id))) Warn("Missing building sprites for " + spec.Id);
            foreach (BombSpec spec in ContentRegistry.Bombs)
                if (Resources.Load<Sprite>(spec.IconPath) == null) Warn("Missing bomb icon " + spec.IconPath);
        }
    }
}
