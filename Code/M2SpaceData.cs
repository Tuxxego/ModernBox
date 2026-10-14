using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    [Serializable]
    public class M2PlanetInfo
    {
        public string name;
        public string description;
        public Dictionary<string, int> resources;
        public int dangerRating;
        public string dangerDescription;
        public string planetType;
        public string size;
        public bool hasFauna;
        public M2MoonInfo[] moonInfo;
    }

    [Serializable]
    public class M2MoonInfo
    {
        public string name;
        public string description;
        public string moonType;
        public string size;
        public bool hasFauna;
    }

    /// Star system as stored in star.json. Reads the old M2 layout too (position under gameObject).
    public class M2StarData
    {
        public string name;
        public string starType;
        public int planetCount;
        public Vector3 position;
        public M2PlanetInfo[] planetInfo = new M2PlanetInfo[0];
        public int selectedPlanet;

        public string ToJson()
        {
            JObject obj = new JObject
            {
                { "name", name },
                { "starType", starType },
                { "planetCount", planetCount },
                { "selectedPlanet", selectedPlanet },
                { "x", position.x },
                { "y", position.y },
                { "z", position.z },
                { "gameObject", new JObject { { "position", new JObject { { "x", position.x }, { "y", position.y }, { "z", position.z } } } } },
                { "planetInfo", JArray.FromObject(planetInfo ?? new M2PlanetInfo[0]) }
            };
            return obj.ToString(Formatting.Indented);
        }

        public static M2StarData FromJson(string json)
        {
            JObject obj = JObject.Parse(json);
            M2StarData star = new M2StarData
            {
                name = obj["name"]?.ToString() ?? "DefaultName",
                starType = obj["starType"]?.ToString() ?? "Unknown",
                planetCount = obj["planetCount"]?.ToObject<int>() ?? 0,
                selectedPlanet = obj["selectedPlanet"]?.ToObject<int>() ?? 0
            };
            JObject positionObj = obj["gameObject"]?["position"] as JObject;
            if (positionObj != null)
                star.position = new Vector3(positionObj["x"]?.ToObject<float>() ?? 0f, positionObj["y"]?.ToObject<float>() ?? 0f, positionObj["z"]?.ToObject<float>() ?? 0f);
            else
                star.position = new Vector3(obj["x"]?.ToObject<float>() ?? 0f, obj["y"]?.ToObject<float>() ?? 0f, obj["z"]?.ToObject<float>() ?? 0f);
            JArray planets = obj["planetInfo"] as JArray;
            star.planetInfo = planets != null ? planets.ToObject<M2PlanetInfo[]>() : new M2PlanetInfo[0];
            if (star.planetInfo == null) star.planetInfo = new M2PlanetInfo[0];
            if (star.planetCount > star.planetInfo.Length) star.planetCount = star.planetInfo.Length;
            if (star.selectedPlanet < 0 || star.selectedPlanet >= star.planetInfo.Length) star.selectedPlanet = 0;
            return star;
        }

        public static M2StarData TryLoad(string starFile)
        {
            try
            {
                return File.Exists(starFile) ? FromJson(File.ReadAllText(starFile)) : null;
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModernBox Space] Failed to read " + starFile + ": " + ex.Message);
                return null;
            }
        }
    }

    [Serializable]
    public class M2GalaxyData
    {
        public string name;
        public string description;
        public int requirement;
        public int starCount;
        public int dangerRating;
        public float[] starWeights;
        public float[] nebulaColor1;
        public float[] nebulaColor2;
        public bool GlassStructure;
    }

    internal sealed class M2CompendiumEntry
    {
        internal string Title { get; }
        internal string Content { get; }

        internal M2CompendiumEntry(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }

    /// All space files live outside the game's save slots, in the same places M2 used.
    internal static class M2SpacePaths
    {
        internal static string Root => Path.Combine(Application.persistentDataPath, "ModernBox");
        internal static string PlanetCountFile => Path.Combine(Root, "PlanetCount.txt");
        internal static string CurrentPlanetFile => Path.Combine(Root, "currentPlanet.txt");
        internal static string ActiveGalaxyFile => Path.Combine(Application.persistentDataPath, "activeGalaxy.txt");
        internal static string JourneyTrackerFile => Path.Combine(Application.persistentDataPath, "JourneyTracker.txt");
        internal static string VisitedPlanetsFile => Path.Combine(Application.persistentDataPath, "visited_planets.txt");
        internal static string FavoritesFile => Path.Combine(Application.persistentDataPath, "favorites.txt");
        internal static string CustomGalaxiesFolder => Path.Combine(Application.dataPath, "../galaxies/");

        internal static string GalaxyFolder(string galaxy)
        {
            return Path.Combine(Root, SafeName(galaxy), "Galaxies", SafeName(galaxy));
        }

        internal static string SafeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unnamed";
            char[] invalid = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        }

        internal static int ReadPlanetCount()
        {
            try
            {
                if (File.Exists(PlanetCountFile) && int.TryParse(File.ReadAllText(PlanetCountFile), out int count)) return count;
            }
            catch (Exception) { }
            return 0;
        }

        internal static void EnsureRoot()
        {
            Directory.CreateDirectory(Root);
        }
    }
}
