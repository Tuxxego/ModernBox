using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// Port of M2's StarManager: galaxies, stars, nebulas, favorites, search, compendium,
    /// journey tracker and the IMGUI star map. Everything it creates lives under the space root.
    internal sealed class M2StarMap : MonoBehaviour
    {
        private const int WindowBase = 0x4D3200;

        private readonly List<M2StarData> stars = new List<M2StarData>();
        private readonly Dictionary<M2StarData, GameObject> starObjects = new Dictionary<M2StarData, GameObject>();
        private List<string> favoritesList = new List<string>();
        private M2StarData hoveredStar;
        private M2StarData selectedStar;
        internal Camera mainCamera;
        private Transform worldRoot;

        public float moveSpeed = 20f;
        public float zoomSpeed = 20f;
        public float minZoom = 1f;
        public float maxZoom = 50f;
        public float nebulaDensity = 0.05f;
        public float nebulaSize = 6f;

        private bool showPlanetWindow;
        private bool showParametersWindow;
        private bool showGalaxySelectionWindow;
        private bool showFavoritesWindow;
        private bool showCompendiumWindow;
        private readonly List<M2CompendiumEntry> compendiumEntries = new List<M2CompendiumEntry>
        {
            new M2CompendiumEntry("Star Types", "There are several types of stars such as Red Dwarfs, Yellow Dwarfs, and Blue Giants."),
            new M2CompendiumEntry("What will be here", "Stuff on creatures, planets, etc will be here."),
        };
        private Vector2 sidebarScrollPosition;
        private Vector2 contentScrollPosition;
        private string currentTitle = "Welcome";
        private string currentContent = "Welcome to the Compendium! Here you will find information on various aspects of the universe.";
        private readonly List<string> journeyTracker = new List<string>();
        private bool showJourneyTrackerWindow;
        private string galaxyName = "Crabby Way";
        private string[] predefinedGalaxies = { "Crabby Way", "Tuxxus", "Krummple", "BlueNight", "Glass", "Centuga", "Dank" };
        private readonly Dictionary<string, int> galaxyRequirements = new Dictionary<string, int>
        {
            { "Crabby Way", 0 }, { "Tuxxus", 10 }, { "Krummple", 20 }, { "BlueNight", 30 }, { "Glass", 45 }, { "Centuga", 55 }, { "Dank", 85 }
        };
        private readonly Dictionary<string, int> galaxyStarCounts = new Dictionary<string, int>
        {
            { "Crabby Way", 600 }, { "Tuxxus", 1200 }, { "Krummple", 800 }, { "BlueNight", 2300 }, { "Glass", 1800 }, { "Centuga", 1700 }, { "Dank", 300 }
        };
        private readonly Dictionary<string, int> galaxyDangerRatings = new Dictionary<string, int>
        {
            { "Crabby Way", 1 }, { "Tuxxus", 3 }, { "Krummple", 2 }, { "BlueNight", 4 }, { "Glass", 4 }, { "Centuga", 4 }, { "Dank", 5 }
        };
        private string galaxyDescription = "";
        private bool showTutorialPrompt;
        private bool showTutorialStep1;
        private bool showTutorialStep2;
        private bool isGeneratingStars;
        private bool showSearchWindow;
        private string searchQuery = "";
        private Vector2 scrollPosition;
        private bool showFilterWindow;
        private bool applyFilters;
        private string selectedStarType = "Any";
        private int minPlanets;
        private int maxPlanets = int.MaxValue;
        private static readonly System.Random rand = new System.Random();
        private const string pattern = "psx";
        private static readonly string[] prefixes = { "Al", "Betel", "Proxi", "Vega", "Anta", "Capel", "Siri", "Tauri", "Poll", "Cen", "Alpha", "Beta", "Delta", "Epsilon", "Gamma", "Cenu" };
        private static readonly string[] syllables = { "al", "bel", "den", "mar", "nus", "zar", "pho", "cos", "tera", "van", "ly", "pe", "xor", "sta", "pro", "lux", "tor", "el", "ris", "zir", "quar", "vix" };
        private static readonly string[] suffixes = { "on", "or", "a", "us", "ae", "um", "is", "us", "i", "an", "es", "ia", "ar", "il", "or", "an", "is" };
        private readonly Dictionary<string, string> CustomGalaxyDescriptions = new Dictionary<string, string>();
        private int loadedGalaxyCount;
        private bool isCustomGalaxiesMode;
        private readonly Dictionary<string, (Color, Color)> CustomGalaxyNebulas = new Dictionary<string, (Color, Color)>();
        private readonly Dictionary<string, (Color, Color)> predefinedNebulas = new Dictionary<string, (Color, Color)>
        {
            { "Crabby Way", (new Color(1f, 1f, 1f, 0.4f), new Color(1f, 1f, 1f, 0.2f)) },
            { "Tuxxus", (new Color(1f, 0.843f, 0f, 0.5f), new Color(1f, 0.843f, 0f, 0.3f)) },
            { "Krummple", (new Color(1f, 0f, 0f, 0.5f), new Color(0f, 1f, 0f, 0.3f)) },
            { "BlueNight", (new Color(0f, 0f, 1f, 0.5f), new Color(0f, 0f, 1f, 0.3f)) },
            { "Glass", (new Color(0.5f, 0f, 0.5f, 0.5f), new Color(0.7f, 0f, 0.7f, 0.3f)) },
            { "Centuga", (new Color(0f, 1f, 1f, 0.5f), new Color(0f, 1f, 1f, 0.3f)) },
            { "Dank", (new Color(0.5f, 0f, 0f, 0.5f), new Color(0.5f, 0f, 0f, 0.3f)) }
        };
        private readonly Dictionary<string, bool> GlassGalaxies = new Dictionary<string, bool> { { "Glass", true } };
        private readonly List<string> randomFacts = new List<string>
        {
            "Krummple and Tuxxus will collide in 6 Billion years.",
            "BlueNight was one of the first Galaxies in the universe?",
            "Space Serpants are real, one was observed in HD-843 in Crabby Way.",
            "I hate the ModernBox save system.",
            "Dank is a relativly new galaxy, odd considering it's apparent abundence of alien intellegence, somethings going on here...",
            "Tuxxus is not gay.",
            "You can add custom galaxies, check the discord server for more info.",
            "What if Tuxxus is gay????? (he's not)",
            "Filama is goated.",
            "I WANNA STAR WARS MOD!!!!!!"
        };
        private string currentFact;
        private int planetsVisited;
        private float planetCountReadTime = -10f;
        private string hoveredGalaxy;

        private static readonly Dictionary<string, Sprite> StarSprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private Texture2D gradientTexture;
        private Texture2D smallNormalTex, smallHoverTex, barNormalTex, barHoverTex;

        private string activeGalaxy
        {
            get { return galaxyName; }
            set { galaxyName = value; SaveActiveGalaxy(value); }
        }

        private static string L(string key) => M2SpaceLocalization.Localize(key);

        private void Start()
        {
            worldRoot = new GameObject("StarMapObjects").transform;
            worldRoot.SetParent(transform, false);
            LoadActiveGalaxy();
            LoadGalaxies();
            LoadJourneyTracker();
            StartCoroutine(InitializeAndGenerateStars());
        }

        public void LoadGalaxies()
        {
            string galaxiesFolder = M2SpacePaths.CustomGalaxiesFolder;
            try
            {
                if (!Directory.Exists(galaxiesFolder))
                {
                    Directory.CreateDirectory(galaxiesFolder);
                    File.WriteAllText(Path.Combine(galaxiesFolder, "CustomGalaxiesReadme.txt"), "Join the discord for help and custom galaxy downloads.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ModernBox Space] Custom galaxies folder unavailable: " + ex.Message);
                return;
            }

            foreach (string filePath in Directory.GetFiles(galaxiesFolder, "*.gal"))
            {
                try
                {
                    M2GalaxyData galaxy = JsonUtility.FromJson<M2GalaxyData>(File.ReadAllText(filePath));
                    if (galaxy == null || string.IsNullOrEmpty(galaxy.name)) continue;
                    if (!predefinedGalaxies.Contains(galaxy.name)) predefinedGalaxies = predefinedGalaxies.Concat(new[] { galaxy.name }).ToArray();
                    CustomGalaxyDescriptions[galaxy.name] = galaxy.description;
                    galaxyRequirements[galaxy.name] = galaxy.requirement;
                    galaxyStarCounts[galaxy.name] = galaxy.starCount;
                    galaxyDangerRatings[galaxy.name] = galaxy.dangerRating;
                    if (galaxy.starWeights != null && galaxy.starWeights.Length > 0) galaxyStarWeights[galaxy.name] = galaxy.starWeights;
                    if (galaxy.nebulaColor1 != null && galaxy.nebulaColor2 != null && galaxy.nebulaColor1.Length >= 4 && galaxy.nebulaColor2.Length >= 4)
                    {
                        CustomGalaxyNebulas[galaxy.name] = (
                            new Color(galaxy.nebulaColor1[0], galaxy.nebulaColor1[1], galaxy.nebulaColor1[2], galaxy.nebulaColor1[3]),
                            new Color(galaxy.nebulaColor2[0], galaxy.nebulaColor2[1], galaxy.nebulaColor2[2], galaxy.nebulaColor2[3]));
                    }
                    if (galaxy.GlassStructure) GlassGalaxies[galaxy.name] = true;
                    loadedGalaxyCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModernBox Space] Error loading galaxy file '{filePath}': {ex.Message}");
                }
            }
        }

        private void LoadActiveGalaxy()
        {
            string path = M2SpacePaths.ActiveGalaxyFile;
            if (File.Exists(path))
            {
                galaxyName = File.ReadAllText(path).Trim();
                if (string.IsNullOrEmpty(galaxyName)) galaxyName = "Crabby Way";
            }
            else
            {
                activeGalaxy = "Crabby Way";
            }
        }

        private void SaveActiveGalaxy(string galaxy)
        {
            try { File.WriteAllText(M2SpacePaths.ActiveGalaxyFile, galaxy); }
            catch (Exception ex) { Debug.LogWarning("[ModernBox Space] " + ex.Message); }
        }

        private IEnumerator InitializeAndGenerateStars()
        {
            isGeneratingStars = true;
            yield return null;
            yield return null;
            string galaxyPath = M2SpacePaths.GalaxyFolder(activeGalaxy);
            if (Directory.Exists(galaxyPath)) GenerateLoadedStars(galaxyPath);
            else GenerateStars();
            favoritesList = LoadFavorites();
            showTutorialPrompt = true;
            SetRandomPlanetIfNeeded();
        }

        private void GenerateStars()
        {
            ClearStars();
            GenerateNebulas();
            if (!galaxyStarCounts.ContainsKey(activeGalaxy))
            {
                Debug.LogError($"[ModernBox Space] Galaxy '{activeGalaxy}' is not found in the star counts dictionary.");
                isGeneratingStars = false;
                return;
            }

            int starCount = galaxyStarCounts[activeGalaxy];
            float cameraWidth = (mainCamera.orthographicSize * mainCamera.aspect * 2) + 100;
            float cameraHeight = (mainCamera.orthographicSize * 2) + 100;

            for (int i = 0; i < starCount; i++)
            {
                M2StarData star = new M2StarData();
                star.starType = GenerateStarType();
                if (GetStarSprite(star.starType) == null) continue;
                if (activeGalaxy == "Glass")
                {
                    star.position = GeneratePositionInSpiralArms(cameraWidth, cameraHeight);
                }
                else
                {
                    star.position = new Vector3(UnityEngine.Random.Range(-cameraWidth / 2, cameraWidth / 2), UnityEngine.Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
                }
                star.name = GenerateStarName();
                star.planetCount = UnityEngine.Random.Range(1, 10);
                GeneratePlanetsForStar(star);
                stars.Add(star);
                CreateStarObject(star);
            }

            SaveStarsAndPlanets();
            isGeneratingStars = false;
        }

        private void CreateStarObject(M2StarData star)
        {
            GameObject starObject = new GameObject("Star");
            starObject.layer = M2SpaceManager.SpaceLayer;
            starObject.transform.SetParent(worldRoot, false);
            starObject.transform.position = star.position;
            starObject.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            SpriteRenderer renderer = starObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetStarSprite(star.starType);
            starObjects[star] = starObject;
        }

        private Vector3 GeneratePositionInSpiralArms(float cameraWidth, float cameraHeight)
        {
            float radius = UnityEngine.Random.Range(0.5f, cameraWidth / 2);
            float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            float spiralOffset = radius * 0.2f * Mathf.Sin(7 * angle);
            return new Vector3(radius * Mathf.Cos(angle) + spiralOffset, radius * Mathf.Sin(angle) + spiralOffset, 0);
        }

        private bool IsPositionInShatteredArea(Vector3 position)
        {
            float wave = 50f * Mathf.Sin(position.x * 0.1f);
            return Mathf.Abs(position.y - wave) < 10f;
        }

        private void GenerateNebulas()
        {
            int nebulaCount = Mathf.RoundToInt(nebulaDensity * Mathf.Pow((mainCamera.orthographicSize * 2) + 100, 2));
            float cameraWidth = (mainCamera.orthographicSize * mainCamera.aspect * 2) + 100;
            float cameraHeight = (mainCamera.orthographicSize * 2) + 100;
            Sprite nebulaSprite = CreateNebulaSprite(activeGalaxy);
            bool useSpiralArms = activeGalaxy == "Glass" || (GlassGalaxies.ContainsKey(activeGalaxy) && GlassGalaxies[activeGalaxy]);

            for (int i = 0; i < nebulaCount; i++)
            {
                Vector3 nebulaPosition;
                int guard = 0;
                do
                {
                    if (useSpiralArms) nebulaPosition = GeneratePositionInSpiralArms(cameraWidth, cameraHeight);
                    else nebulaPosition = new Vector3(UnityEngine.Random.Range(-cameraWidth / 2, cameraWidth / 2), UnityEngine.Random.Range(-cameraHeight / 2, cameraHeight / 2), -1);
                }
                while (IsPositionInShatteredArea(nebulaPosition) && ++guard < 50);

                GameObject nebulaObject = new GameObject("Nebula");
                nebulaObject.layer = M2SpaceManager.SpaceLayer;
                nebulaObject.transform.SetParent(worldRoot, false);
                nebulaObject.transform.position = nebulaPosition;
                nebulaObject.transform.localScale = new Vector3(nebulaSize, nebulaSize, 1);
                nebulaObject.AddComponent<SpriteRenderer>().sprite = nebulaSprite;
            }
        }

        private Sprite CreateNebulaSprite(string galaxy)
        {
            const int width = 256;
            const int height = 256;
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Vector2 center = new Vector2(width / 2, height / 2);
            float maxDistance = Vector2.Distance(Vector2.zero, center);

            Color nebulaColor1;
            Color nebulaColor2;
            if (predefinedNebulas.TryGetValue(galaxy, out var colors)) (nebulaColor1, nebulaColor2) = colors;
            else if (CustomGalaxyNebulas.TryGetValue(galaxy, out var customColors)) (nebulaColor1, nebulaColor2) = customColors;
            else
            {
                nebulaColor1 = new Color(1f, 1f, 1f, 0.4f);
                nebulaColor2 = new Color(1f, 1f, 1f, 0.2f);
            }

            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float normalizedDistance = Vector2.Distance(new Vector2(x, y), center) / maxDistance;
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float edgeAlpha = Mathf.Clamp01(1f - normalizedDistance) * Mathf.Lerp(1f, 0f, normalizedDistance * 2f);
                    float radialEffect = Mathf.Clamp01(1f - normalizedDistance);
                    Color nebulaColor = Color.Lerp(nebulaColor1, nebulaColor2, noise * 0.6f * radialEffect);
                    nebulaColor.a *= edgeAlpha;
                    pixels[y * width + x] = nebulaColor;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        private void GeneratePlanetsForStar(M2StarData star)
        {
            star.planetInfo = new M2PlanetInfo[star.planetCount];
            for (int i = 0; i < star.planetCount; i++)
            {
                string planetType = GetPlanetTypeBasedOnStar(star.starType);
                int sizeX = UnityEngine.Random.Range(1, 25);
                int sizeY = UnityEngine.Random.Range(1, 25);
                star.planetInfo[i] = new M2PlanetInfo
                {
                    name = $"{star.name} {ToRomanNumeral(i + 1)}",
                    description = GenerateNormalDescription(star.starType, planetType),
                    resources = GenerateResources(),
                    dangerRating = UnityEngine.Random.Range(1, 10),
                    dangerDescription = GenerateDangerDescription(star.starType, planetType),
                    planetType = planetType,
                    size = $"{sizeX} x {sizeY}",
                    hasFauna = UnityEngine.Random.value > 0.5f
                };
            }
        }

        private string GetPlanetTypeBasedOnStar(string starType)
        {
            switch (starType)
            {
                case "Red Dwarf": return GetRandomPlanetType(new[] { "Desert World", "Icy", "Swamp World", "Mushroom World" });
                case "Yellow Dwarf": return GetRandomPlanetType(new[] { "Desert World", "Oceanic", "Chess World", "Lemon World" });
                case "Blue Giant": return GetRandomPlanetType(new[] { "Gas Giant", "Crystal World", "Lava World", "Wasteland World" });
                case "White Dwarf": return GetRandomPlanetType(new[] { "Desert World", "Icy", "Mechanical World", "Corrupted World" });
                case "Corrupted Star": return GetRandomPlanetType(new[] { "Corrupted World" });
                case "Neutron Star": return GetRandomPlanetType(new[] { "Desert World", "Gas Giant", "Jungle World", "Corrupted World" });
                case "Brown Dwarf": return GetRandomPlanetType(new[] { "Icy", "Gas Giant", "Swamp World", "Mushroom World" });
                case "Supergiant": return GetRandomPlanetType(new[] { "Gas Giant", "Crystal World", "Lava World", "Wasteland World" });
                case "Pulsar": return GetRandomPlanetType(new[] { "Desert World", "Swamp World", "Jungle World", "Corrupted World" });
                case "White Supergiant": return GetRandomPlanetType(new[] { "Gas Giant", "Crystal World", "Mechanical World", "Wasteland World" });
                case "Red Supergiant": return GetRandomPlanetType(new[] { "Desert World", "Oceanic", "Lava World", "Mushroom World" });
                case "Black Hole": return GetRandomPlanetType(new[] { "Desert World", "Icy", "Jungle World", "Corrupted World" });
                case "Rainbow Star": return GetRandomPlanetType(new[] { "Crystal World", "Oceanic", "Jungle World", "Lemon World" });
                case "Void Star": return GetRandomPlanetType(new[] { "Icy", "Gas Giant", "Lava World", "Corrupted World" });
                case "Crystal Star": return GetRandomPlanetType(new[] { "Crystal World", "Mechanical World", "Oceanic", "Lemon World" });
                case "Quantum Star": return GetRandomPlanetType(new[] { "Desert World", "Gas Giant", "Swamp World", "Wasteland World" });
                case "Echo Star": return GetRandomPlanetType(new[] { "Jungle World", "Oceanic", "Mechanical World", "Lemon World" });
                case "Chrono Star": return GetRandomPlanetType(new[] { "Icy", "Desert World", "Crystal World", "Corrupted World" });
                case "Phantom Star": return GetRandomPlanetType(new[] { "Swamp World", "Jungle World", "Lava World", "Mushroom World" });
                case "Prism Star": return GetRandomPlanetType(new[] { "Oceanic", "Crystal World", "Gas Giant", "Lemon World" });
                case "Nebula Star": return GetRandomPlanetType(new[] { "Chess World", "Swamp World", "Icy", "Mushroom World" });
                case "Graviton Star": return GetRandomPlanetType(new[] { "Lava World", "Crystal World", "Mechanical World", "Wasteland World" });
                case "Aurora Star": return GetRandomPlanetType(new[] { "Oceanic", "Jungle World", "Icy", "Lemon World" });
                default: return GetRandomPlanetType();
            }
        }

        private string GetRandomPlanetType(string[] possibleTypes = null)
        {
            if (possibleTypes == null)
                possibleTypes = new[] { "Desert World", "Gas Giant", "Oceanic", "Icy", "Crystal World", "Swamp World", "Lava World", "Chess World", "Mechanical World", "Jungle World", "Corrupted World", "Lemon World", "Mushroom World", "Wasteland World" };
            return possibleTypes[UnityEngine.Random.Range(0, possibleTypes.Length)];
        }

        private static Sprite GetStarSprite(string starType)
        {
            string file = GetStarSpritePath(starType).Substring("Stars/".Length);
            Sprite sprite;
            if (StarSprites.TryGetValue(file, out sprite)) return sprite;
            sprite = null;
            try
            {
                string folder = Path.Combine(ModernBoxMod.ModFolder ?? string.Empty, "GameResources", "Stars");
                string path = Directory.Exists(folder)
                    ? Directory.GetFiles(folder, "*.png").FirstOrDefault(candidate => string.Equals(Path.GetFileNameWithoutExtension(candidate), file, StringComparison.OrdinalIgnoreCase))
                    : null;
                if (path != null)
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(File.ReadAllBytes(path));
                    texture.filterMode = FilterMode.Bilinear;
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width / 0.64f);
                }
                else
                {
                    sprite = Resources.Load<Sprite>(GetStarSpritePath(starType));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModernBox Space] Star sprite failed for " + starType + ": " + ex.Message);
            }
            if (sprite == null) Debug.LogError($"[ModernBox Space] Sprite not found for star type: {starType}");
            StarSprites[file] = sprite;
            return sprite;
        }

        private static string GetStarSpritePath(string starType)
        {
            switch (starType)
            {
                case "Red Dwarf": return "Stars/RedDwarf";
                case "Yellow Dwarf": return "Stars/YellowDwarf";
                case "Blue Giant": return "Stars/BlueGiant";
                case "White Dwarf": return "Stars/WhiteDwarf";
                case "Neutron Star": return "Stars/NeutronStar";
                case "Brown Dwarf": return "Stars/BrownDwarf";
                case "Supergiant": return "Stars/Supergiant";
                case "Pulsar": return "Stars/Pulsar";
                case "White Supergiant": return "Stars/WhiteSupergiant";
                case "Red Supergiant": return "Stars/RedSupergiant";
                case "Black Hole": return "Stars/BlackHole";
                case "Rainbow Star": return "Stars/RainbowStar";
                case "Void Star": return "Stars/VoidStar";
                case "Crystal Star": return "Stars/CrystalStar";
                case "Quantum Star": return "Stars/QuantumStar";
                case "Echo Star": return "Stars/EchoStar";
                case "Chrono Star": return "Stars/ChronoStar";
                case "Phantom Star": return "Stars/PhantomStar";
                case "Prism Star": return "Stars/PrismStar";
                case "Nebula Star": return "Stars/NebulaStar";
                case "Graviton Star": return "Stars/GravitonStar";
                case "Aurora Star": return "Stars/AuroraStar";
                case "Corrupted Star": return "Stars/CorruptedStar";
                default: return "Stars/Pulsar";
            }
        }

        private void SaveStarsAndPlanets()
        {
            string savePath = M2SpacePaths.GalaxyFolder(activeGalaxy);
            try
            {
                Directory.CreateDirectory(savePath);
                foreach (M2StarData star in stars)
                {
                    string starPath = Path.Combine(savePath, M2SpacePaths.SafeName(star.name));
                    Directory.CreateDirectory(starPath);
                    File.WriteAllText(Path.Combine(starPath, "star.json"), star.ToJson());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModernBox Space] Could not save galaxy " + activeGalaxy + ": " + ex.Message);
            }
        }

        private void GenerateLoadedStars(string path)
        {
            ClearStars();
            GenerateNebulas();
            foreach (string starFolder in Directory.GetDirectories(path))
            {
                M2StarData star = M2StarData.TryLoad(Path.Combine(starFolder, "star.json"));
                if (star == null) continue;
                stars.Add(star);
                if (GetStarSprite(star.starType) == null) Debug.LogWarning("[ModernBox Space] Failed to create or assign sprite.");
                CreateStarObject(star);
            }
            isGeneratingStars = false;
        }

        private void SetRandomPlanetIfNeeded()
        {
            if (!string.IsNullOrEmpty(M2PlanetManager.GetCurrentPlanet()) || stars.Count == 0) return;
            M2StarData randomStar = stars[UnityEngine.Random.Range(0, stars.Count)];
            if (randomStar.planetInfo.Length > 0)
                M2PlanetManager.SetCurrentPlanet(randomStar.planetInfo[UnityEngine.Random.Range(0, randomStar.planetInfo.Length)].name);
        }

        private GUIStyle windowStyle;

        // M2 painted the loading gradient into the global window skin, so every star map
        // window ended up with it. Same look here, but kept to our own windows.
        private GUIStyle WindowStyle()
        {
            if (windowStyle != null) return windowStyle;
            if (gradientTexture == null) gradientTexture = CreateGradientTexture(700, 500, new Color(0.1f, 0.1f, 0.3f), new Color(0.3f, 0.3f, 0.5f));
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = gradientTexture;
            return windowStyle;
        }

        private void OnGUI()
        {
            Color oldBackground = GUI.backgroundColor;
            try
            {
                DrawGui();
            }
            finally
            {
                GUI.backgroundColor = oldBackground;
            }
        }

        private void DrawGui()
        {
            BottomBar();

            if (hoveredStar != null)
            {
                Vector2 mousePos = Event.current.mousePosition;
                string starInfo = $"{L("star_info_name")}: {hoveredStar.name}\n" +
                                  $"{L("star_info_type")}: {hoveredStar.starType}\n" +
                                  $"{L("star_info_planets")}: {hoveredStar.planetCount}";
                Vector2 textSize = GUI.skin.box.CalcSize(new GUIContent(starInfo));
                GUI.Box(new Rect(mousePos.x + 10, mousePos.y + 10, textSize.x + 10, textSize.y + 10), starInfo);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    selectedStar = hoveredStar;
                    AddToJourneyTracker(selectedStar);
                    showPlanetWindow = true;
                    Event.current.Use();
                }
            }

            if (showJourneyTrackerWindow)
                GUI.Window(WindowBase + 12, new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400), JourneyTrackerWindow, L("journey_tracker"), WindowStyle());

            if (selectedStar != null)
                GUI.Window(WindowBase + 0, new Rect(10, 10, 350, 450), StarInfoWindow, new GUIContent(L("star_information"), "Details about the selected star"), WindowStyle());

            if (showPlanetWindow && selectedStar != null && selectedStar.planetInfo.Length > 0)
                GUI.Window(WindowBase + 1, new Rect(360, 10, 300, 400), PlanetInfoWindow, L("planet_information"), WindowStyle());

            if (showParametersWindow && selectedStar != null && selectedStar.planetInfo.Length > 0)
                GUILayout.Window(WindowBase + 13, new Rect(450, 100, 300, 200), PlanetParametersWindow, L("parameters"), WindowStyle());

            if (showTutorialPrompt)
                GUI.Window(WindowBase + 2, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), TutorialPromptWindow, L("tutorial"), WindowStyle());
            if (showTutorialStep1)
                GUI.Window(WindowBase + 3, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), TutorialStep1Window, L("tutorial_step1"), WindowStyle());
            if (showTutorialStep2)
                GUI.Window(WindowBase + 4, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), TutorialStep2Window, L("tutorial_step2"), WindowStyle());

            GUI.Window(WindowBase + 5, new Rect(Screen.width - 160, 10, 150, 150), SmallWindow, L("options"), WindowStyle());

            if (showGalaxySelectionWindow)
            {
                Rect galaxyWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 300) / 2, 400, 400);
                Rect descriptionWindowRect = new Rect(galaxyWindowRect.x + galaxyWindowRect.width + 20, galaxyWindowRect.y, 300, 400);
                Rect infoWindowRect = new Rect(galaxyWindowRect.x - 320, galaxyWindowRect.y, 300, 400);
                GUI.Window(WindowBase + 6, galaxyWindowRect, GalaxySelectionWindow, new GUIContent(L("select_galaxy"), "Browse galaxies"), WindowStyle());
                GUI.Window(WindowBase + 7, descriptionWindowRect, GalaxyDescriptionWindow, new GUIContent(L("galaxy_description"), "Galaxy details"), WindowStyle());
                GUI.Window(WindowBase + 8, infoWindowRect, GalaxyInfoWindow, new GUIContent(L("galaxy_info"), "Galaxy stats"), WindowStyle());

                Rect closeButtonRect = new Rect(galaxyWindowRect.x + (galaxyWindowRect.width / 2) - 50, galaxyWindowRect.y + galaxyWindowRect.height + 15, 100, 35);
                Rect customButtonRect = new Rect(closeButtonRect.x + closeButtonRect.width + 10, closeButtonRect.y, 150, 35);
                if (GUI.Button(closeButtonRect, L("close"))) showGalaxySelectionWindow = false;
                if (GUI.Button(customButtonRect, isCustomGalaxiesMode ? "Normal Galaxies" : "Custom Galaxies")) isCustomGalaxiesMode = !isCustomGalaxiesMode;
            }

            if (showFavoritesWindow)
                GUI.Window(WindowBase + 14, new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400), FavoritesWindow, L("favorite_stars"), WindowStyle());

            if (isGeneratingStars)
                GUI.Window(WindowBase + 15, new Rect(Screen.width / 2 - 350, Screen.height / 2 - 250, 700, 500), StarLoadingWindow, L("loading"), WindowStyle());

            if (showSearchWindow)
                GUI.Window(WindowBase + 9, new Rect((Screen.width - 400) / 2, (Screen.height - 500) / 2, 400, 500), SearchWindow, L("search_stars"), WindowStyle());

            if (showFilterWindow)
                GUI.Window(WindowBase + 10, new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400), FilterWindow, L("filter_options"), WindowStyle());

            if (showCompendiumWindow)
                GUI.Window(WindowBase + 11, new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400), CompendiumWindow, L("compendium"), WindowStyle());

            GUIStyle galaxyInfoStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleLeft };
            galaxyInfoStyle.normal.textColor = new Color(0.8f, 0.8f, 0.9f);
            GUIStyle galaxyNameStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            galaxyNameStyle.normal.textColor = new Color(1f, 0.84f, 0f);
            float blockY = Screen.height - 10f;
            GUI.Label(new Rect(10f, blockY - 30f - 20f, 600f, 20f), "You are currently in", galaxyInfoStyle);
            GUI.Label(new Rect(10f, blockY - 30f, 600f, 30f), galaxyName, galaxyNameStyle);
        }

        private void SearchWindow(int windowID)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button(L("filter"))) showFilterWindow = true;
            GUILayout.Label(L("search_stars"));
            searchQuery = GUILayout.TextField(searchQuery ?? string.Empty);
            applyFilters = GUILayout.Toggle(applyFilters, L("apply_filters_label"));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(380), GUILayout.Height(300));

            bool selectionMade = false;
            foreach (M2StarData star in stars)
            {
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || star.name.StartsWith(searchQuery, StringComparison.OrdinalIgnoreCase);
                bool matchesFilters = !applyFilters ||
                    (star.starType == selectedStarType || selectedStarType == "Any") &&
                    star.planetCount >= minPlanets && star.planetCount <= maxPlanets;
                if (matchesSearchQuery && matchesFilters && GUILayout.Button(star.name))
                {
                    selectedStar = star;
                    MoveCameraToStar(star);
                    showSearchWindow = false;
                    selectionMade = true;
                    break;
                }
            }
            if (!selectionMade && !string.IsNullOrEmpty(searchQuery)) GUILayout.Label(L("no_results_found"));
            GUILayout.EndScrollView();
            if (GUILayout.Button(L("close"))) showSearchWindow = false;
            GUILayout.EndVertical();
        }

        private void FilterWindow(int windowID)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Filter Options:");
            string[] filterTypes = { "Any", "Red Dwarf", "Yellow Dwarf", "Blue Giant", "White Dwarf", "Neutron Star", "Brown Dwarf", "Supergiant", "Pulsar", "White Supergiant", "Red Supergiant", "Black Hole" };
            int selectedIndex = Array.IndexOf(filterTypes, selectedStarType);
            selectedIndex = GUILayout.SelectionGrid(selectedIndex, filterTypes, filterTypes.Length / 2);
            if (selectedIndex >= 0) selectedStarType = filterTypes[selectedIndex];
            GUILayout.Label("Minimum Number of Planets:");
            int.TryParse(GUILayout.TextField(minPlanets.ToString()), out minPlanets);
            GUILayout.Label("Maximum Number of Planets:");
            if (!int.TryParse(GUILayout.TextField(maxPlanets.ToString()), out maxPlanets)) maxPlanets = 0;
            if (GUILayout.Button(L("apply"))) showFilterWindow = false;
            if (GUILayout.Button(L("cancel"))) showFilterWindow = false;
            GUILayout.EndVertical();
        }

        private void StarLoadingWindow(int windowID)
        {
            if (currentFact == null) currentFact = randomFacts[UnityEngine.Random.Range(0, randomFacts.Count)];
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 50, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = new Color(0.1f, 1f, 0.5f);
            GUIStyle titleStyle2 = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle2.normal.textColor = new Color(0.7f, 0.8f, 1f);
            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            subtitleStyle.normal.textColor = new Color(1f, 0.6f, 0f);
            GUIStyle italicStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Italic, alignment = TextAnchor.UpperCenter, wordWrap = true };
            italicStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            GUIStyle factStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            factStyle.normal.textColor = new Color(1f, 0.4f, 0.8f);

            GUI.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUILayout.Label("⚡ SYSTEM ONLINE ⚡", titleStyle);
            GUILayout.Label("SPACE AGE", subtitleStyle);
            GUILayout.Space(40);
            GUILayout.Label(L("stars_loading_message"), italicStyle);
            GUILayout.Space(15);
            GUILayout.Label(L("did_you_know"), italicStyle);
            GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(120));
            GUILayout.Label($"*{currentFact}*", factStyle);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("GALACTIC ENGINE", titleStyle2);
            GUILayout.Label($"Custom Galaxies Created: {loadedGalaxyCount}", subtitleStyle);
            GUILayout.EndVertical();
        }

        private static Texture2D CreateGradientTexture(int width, int height, Color startColor, Color endColor)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                Color lerpedColor = Color.Lerp(startColor, endColor, (float)y / height);
                for (int x = 0; x < width; x++) pixels[y * width + x] = lerpedColor;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void TutorialPromptWindow(int windowID)
        {
            GUILayout.Label(L("tutorial"));
            if (GUILayout.Button(L("yes")))
            {
                showTutorialPrompt = false;
                showTutorialStep1 = true;
            }
            if (GUILayout.Button(L("no"))) showTutorialPrompt = false;
        }

        private void TutorialStep1Window(int windowID)
        {
            GUILayout.Label(L("tutorial1"));
            if (GUILayout.Button(L("next")))
            {
                showTutorialStep1 = false;
                showTutorialStep2 = true;
            }
            if (GUILayout.Button(L("skip"))) showTutorialStep1 = false;
        }

        private void TutorialStep2Window(int windowID)
        {
            GUILayout.Label(L("tutorial2"));
            if (GUILayout.Button(L("finish"))) showTutorialStep2 = false;
            if (GUILayout.Button(L("skip"))) showTutorialStep2 = false;
        }

        private void SmallWindow(int windowID)
        {
            if (smallNormalTex == null)
            {
                smallNormalTex = MakeTex(2, 2, new Color(0f, 0.2f, 0.2f, 0.7f));
                smallHoverTex = MakeTex(2, 2, new Color(0.1f, 0.5f, 0.5f, 1f));
            }
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            buttonStyle.normal.background = smallNormalTex;
            buttonStyle.hover.background = smallHoverTex;

            if (GUILayout.Button(L("galaxy_select"), buttonStyle)) showGalaxySelectionWindow = true;
            if (GUILayout.Button(L("favorite_systems"), buttonStyle)) showFavoritesWindow = true;
            if (GUILayout.Button(L("search_stars"), buttonStyle)) showSearchWindow = true;
            if (GUILayout.Button(L("exit"), buttonStyle)) M2SpaceManager.RequestDisableSpace();
        }

        private void FavoritesWindow(int windowID)
        {
            foreach (string favoriteName in LoadFavorites())
            {
                M2StarData star = stars.Find(s => s.name.Equals(favoriteName, StringComparison.OrdinalIgnoreCase));
                if (star != null && GUILayout.Button(star.name))
                {
                    selectedStar = star;
                    showPlanetWindow = false;
                    showFavoritesWindow = false;
                    MoveCameraToStar(star);
                    break;
                }
            }
            if (GUILayout.Button(L("close"))) showFavoritesWindow = false;
        }

        private void MoveCameraToStar(M2StarData star)
        {
            if (star == null || mainCamera == null) return;
            mainCamera.transform.position = new Vector3(star.position.x, star.position.y, mainCamera.transform.position.z);
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize * 0.5f, minZoom, maxZoom);
        }

        private void SaveFavorites()
        {
            try { File.WriteAllLines(M2SpacePaths.FavoritesFile, favoritesList); }
            catch (Exception ex) { Debug.LogWarning("[ModernBox Space] " + ex.Message); }
        }

        private void AddFavorite(string starName)
        {
            if (favoritesList.Contains(starName)) return;
            favoritesList.Add(starName);
            SaveFavorites();
        }

        private void RemoveFavorite(string starName)
        {
            if (favoritesList.Remove(starName)) SaveFavorites();
        }

        private M2StarData FindStarByName(string starName)
        {
            return string.IsNullOrEmpty(starName) ? null : stars.FirstOrDefault(star => star.name == starName);
        }

        private static List<string> LoadFavorites()
        {
            List<string> favorites = new List<string>();
            try
            {
                if (File.Exists(M2SpacePaths.FavoritesFile)) favorites.AddRange(File.ReadAllLines(M2SpacePaths.FavoritesFile).Select(line => line.Trim()));
            }
            catch (Exception) { }
            return favorites;
        }

        private void CompendiumWindow(int windowID)
        {
            GUILayout.BeginArea(new Rect(0, 0, 150, 500), GUI.skin.box);
            sidebarScrollPosition = GUILayout.BeginScrollView(sidebarScrollPosition);
            foreach (M2CompendiumEntry entry in compendiumEntries)
            {
                if (GUILayout.Button(entry.Title))
                {
                    currentTitle = entry.Title;
                    currentContent = entry.Content;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(150, 0, 450, 500), GUI.skin.box);
            GUILayout.Label(currentTitle, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 16 });
            contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition);
            GUILayout.Label(currentContent);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void RefreshPlanetsVisited()
        {
            if (Time.unscaledTime - planetCountReadTime < 1f) return;
            planetCountReadTime = Time.unscaledTime;
            planetsVisited = M2SpacePaths.ReadPlanetCount();
        }

        private void BottomBar()
        {
            RefreshPlanetsVisited();
            if (barNormalTex == null)
            {
                barNormalTex = MakeTex(2, 2, new Color(0f, 0.5f, 0.5f, 0.6f));
                barHoverTex = MakeTex(2, 2, new Color(0.2f, 0.7f, 0.8f, 0.8f));
            }
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            buttonStyle.normal.textColor = Color.cyan;
            buttonStyle.hover.textColor = Color.white;
            buttonStyle.normal.background = barNormalTex;
            buttonStyle.hover.background = barHoverTex;

            const float buttonWidth = 120f;
            const float buttonHeight = 40f;
            const float spacing = 15f;
            float compendiumX = Screen.width - buttonWidth - 20f;
            float journeyTrackerX = compendiumX - (buttonWidth + spacing);
            float trackStarX = journeyTrackerX - (buttonWidth + spacing);
            float websiteButtonX = trackStarX - (buttonWidth + spacing);

            if (GUI.Button(new Rect(journeyTrackerX, Screen.height - 50, buttonWidth, buttonHeight), L("journey_tracker"), buttonStyle))
                showJourneyTrackerWindow = !showJourneyTrackerWindow;

            if (GUI.Button(new Rect(trackStarX, Screen.height - 50, buttonWidth, buttonHeight), L("current_star"), buttonStyle))
                MoveCameraToStar(FindStarByName(M2PlanetManager.FindParentStar()));

            if (GUI.Button(new Rect(compendiumX, Screen.height - 50, buttonWidth, buttonHeight), L("compendium"), buttonStyle))
                showCompendiumWindow = !showCompendiumWindow;

            if (planetsVisited >= 150 && GUI.Button(new Rect(websiteButtonX, Screen.height - 50, buttonWidth, buttonHeight), L("arg_redacted"), buttonStyle))
                Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

            GUIStyle fancyTextStyle = new GUIStyle { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            fancyTextStyle.normal.textColor = new Color(1f, 0.84f, 0f);
            GUIStyle smallerTextStyle = new GUIStyle { fontSize = 14, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter };
            smallerTextStyle.normal.textColor = Color.white;
            float centerX = Screen.width / 2f;
            float barY = Screen.height - 60 + 15f;
            GUI.Label(new Rect(centerX - 200f, barY, 400f, 30f), "ModernBox 2.1.0.1", fancyTextStyle);
            GUI.Label(new Rect(centerX - 200f, barY + 20f, 400f, 30f), "BY TUXXEGO", smallerTextStyle);
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void GalaxySelectionWindow(int windowID)
        {
            RefreshPlanetsVisited();
            GUIStyle galaxyButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, fixedHeight = 30, wordWrap = true };
            galaxyButtonStyle.normal.textColor = new Color(0.5f, 1f, 1f);
            galaxyButtonStyle.hover.textColor = new Color(1f, 0.5f, 0.8f);
            GUIStyle unavailableLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            unavailableLabelStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.BoldAndItalic, alignment = TextAnchor.UpperCenter };
            headerStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);

            GUILayout.Label("Explore the Galaxies", headerStyle);
            GUILayout.Space(20);
            HashSet<string> excludedGalaxies = new HashSet<string> { "Crabby Way", "Tuxxus", "Krummple", "BlueNight", "Centuga", "Glass", "Dank" };
            GUILayout.Space(20);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(350), GUILayout.Height(400));

            foreach (string galaxy in predefinedGalaxies)
            {
                if (isCustomGalaxiesMode && excludedGalaxies.Contains(galaxy)) continue;
                int requiredPlanets = galaxyRequirements.ContainsKey(galaxy) ? galaxyRequirements[galaxy] : 0;
                if (planetsVisited >= requiredPlanets)
                {
                    if (GUILayout.Button(galaxy, galaxyButtonStyle))
                    {
                        string galaxyPath = M2SpacePaths.GalaxyFolder(galaxy);
                        activeGalaxy = galaxy;
                        selectedStar = null;
                        hoveredStar = null;
                        if (Directory.Exists(galaxyPath))
                        {
                            showGalaxySelectionWindow = false;
                            GenerateLoadedStars(galaxyPath);
                        }
                        else
                        {
                            GenerateStars();
                        }
                        isGeneratingStars = false;
                    }
                }
                else
                {
                    GUILayout.Label($"{galaxy} (Requires {requiredPlanets} planets, you have {planetsVisited})", unavailableLabelStyle);
                }

                if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    galaxyDescription = GetGalaxyDescription(galaxy);
                    hoveredGalaxy = galaxy;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.Space(20);
        }

        private void GalaxyDescriptionWindow(int windowID)
        {
            GUIStyle descriptionStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Italic, wordWrap = true, alignment = TextAnchor.MiddleCenter };
            descriptionStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
            GUILayout.Label(galaxyDescription, descriptionStyle);
        }

        private void GalaxyInfoWindow(int windowID)
        {
            GUIStyle infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Normal, wordWrap = true, alignment = TextAnchor.MiddleLeft };
            infoStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            if (hoveredGalaxy == null)
            {
                GUILayout.Label(L("no_galaxy_selected"), infoStyle);
            }
            else if (hoveredGalaxy == "Dank")
            {
                GUILayout.Label(L("stars_unknown"), infoStyle);
                GUILayout.Label(L("danger_unknown"), infoStyle);
            }
            else if (galaxyStarCounts.ContainsKey(hoveredGalaxy) && galaxyDangerRatings.ContainsKey(hoveredGalaxy))
            {
                GUILayout.Label(string.Format(L("stars_count"), galaxyStarCounts[hoveredGalaxy]), infoStyle);
                GUILayout.Label(string.Format(L("danger_rating"), galaxyDangerRatings[hoveredGalaxy]), infoStyle);
            }
            else
            {
                GUILayout.Label(L("no_information"), infoStyle);
            }
        }

        private void StarInfoWindow(int windowID)
        {
            if (selectedStar == null) return;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = new Color(0.4f, 0.9f, 1.0f);
            GUIStyle contentStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Italic, wordWrap = true };
            contentStyle.normal.textColor = Color.white;
            GUIStyle favoriteButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            favoriteButtonStyle.normal.textColor = Color.yellow;
            favoriteButtonStyle.hover.textColor = new Color(0.9f, 0.2f, 0.2f);
            GUIStyle planetButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
            planetButtonStyle.normal.textColor = new Color(0.6f, 1.0f, 0.6f);
            planetButtonStyle.hover.textColor = Color.cyan;

            GUILayout.Label(selectedStar.name, titleStyle);
            GUILayout.Space(10);
            GUILayout.Label($"Type: {selectedStar.starType}", contentStyle);
            GUILayout.Label($"Planets: {selectedStar.planetCount}", contentStyle);

            bool isFavorite = favoritesList.Contains(selectedStar.name);
            if (GUILayout.Button(isFavorite ? "★ Unfavorite Star" : "☆ Favorite Star", favoriteButtonStyle))
            {
                if (isFavorite) RemoveFavorite(selectedStar.name);
                else AddFavorite(selectedStar.name);
            }

            GUILayout.Space(15);
            GUILayout.Label("Planets in System:", contentStyle);
            for (int i = 0; i < selectedStar.planetCount && i < selectedStar.planetInfo.Length; i++)
            {
                M2PlanetInfo planetInfo = selectedStar.planetInfo[i];
                if (planetInfo != null && GUILayout.Button(planetInfo.name, planetButtonStyle))
                {
                    selectedStar.selectedPlanet = i;
                    showPlanetWindow = true;
                    Event.current.Use();
                }
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(L("close"), favoriteButtonStyle)) selectedStar = null;
        }

        private void JourneyTrackerWindow(int windowID)
        {
            GUILayout.BeginVertical();
            foreach (string starName in journeyTracker.ToList())
            {
                M2StarData star = FindStarByName(starName);
                if (star != null && GUILayout.Button(starName))
                {
                    selectedStar = star;
                    MoveCameraToStar(star);
                }
            }
            if (GUILayout.Button(L("close"))) showJourneyTrackerWindow = false;
            GUILayout.EndVertical();
        }

        private void AddToJourneyTracker(M2StarData star)
        {
            if (star == null || journeyTracker.Contains(star.name)) return;
            journeyTracker.Add(star.name);
            try { File.WriteAllLines(M2SpacePaths.JourneyTrackerFile, journeyTracker); }
            catch (Exception ex) { Debug.LogWarning("[ModernBox Space] " + ex.Message); }
        }

        private void LoadJourneyTracker()
        {
            try
            {
                if (!File.Exists(M2SpacePaths.JourneyTrackerFile)) return;
                journeyTracker.Clear();
                journeyTracker.AddRange(File.ReadAllLines(M2SpacePaths.JourneyTrackerFile).Select(line => line.Trim()));
            }
            catch (Exception) { }
        }

        private M2PlanetInfo SelectedPlanet()
        {
            if (selectedStar == null || selectedStar.planetInfo.Length == 0) return null;
            int index = Mathf.Clamp(selectedStar.selectedPlanet, 0, selectedStar.planetInfo.Length - 1);
            return selectedStar.planetInfo[index];
        }

        private void PlanetInfoWindow(int windowID)
        {
            M2PlanetInfo planetInfo = SelectedPlanet();
            if (planetInfo == null) return;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = new Color(0.4f, 0.9f, 1.0f);
            GUIStyle contentStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Italic, wordWrap = true };
            contentStyle.normal.textColor = Color.white;
            GUIStyle visitedStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            visitedStyle.normal.textColor = Color.green;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            buttonStyle.normal.textColor = new Color(0.6f, 1.0f, 0.6f);
            buttonStyle.hover.textColor = Color.cyan;

            string planetType = planetInfo.planetType ?? string.Empty;
            GUILayout.Label("Planet Information", titleStyle);
            GUILayout.Space(10);
            GUILayout.Label($"Name: {planetInfo.name}", contentStyle);
            GUILayout.Label($"Type: {(planetType.Contains("(Habitable)") ? "Gas Giant" : planetType)}", contentStyle);
            if (IsPlanetVisited(planetInfo.name)) GUILayout.Label(L("visited"), visitedStyle);
            GUILayout.Space(15);

            if (!planetType.ToLower().Contains("gas giant") && GUILayout.Button(L("visit"), buttonStyle))
            {
                SaveVisitedPlanet(planetInfo.name);
                M2SpaceManager.RequestPlanetVisit(planetInfo.name, planetType, planetInfo.size, planetInfo.hasFauna);
            }
            if (GUILayout.Button(L("parameters"), buttonStyle)) showParametersWindow = true;
            GUILayout.Space(10);
            if (GUILayout.Button(L("close"), buttonStyle)) showPlanetWindow = false;
        }

        private void PlanetParametersWindow(int windowID)
        {
            M2PlanetInfo planetInfo = SelectedPlanet();
            if (planetInfo == null) return;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = new Color(0.4f, 0.9f, 1.0f);
            GUIStyle contentStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Italic, wordWrap = true };
            contentStyle.normal.textColor = Color.white;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            buttonStyle.normal.textColor = new Color(0.6f, 1.0f, 0.6f);
            buttonStyle.hover.textColor = Color.cyan;
            GUIStyle footerTextStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            footerTextStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);

            GUILayout.Label("Planet Parameters", titleStyle);
            GUILayout.Space(10);
            GUILayout.Label($"Size: {planetInfo.size}", contentStyle);
            GUILayout.Label($"Has Fauna?: {planetInfo.hasFauna}", contentStyle);
            GUILayout.Space(15);
            if (GUILayout.Button(L("close"), buttonStyle)) showParametersWindow = false;
            GUILayout.FlexibleSpace();
            GUILayout.Label("Planet Parameters are in beta and more parameters will be added in the future.", footerTextStyle);
        }

        private static void SaveVisitedPlanet(string planetName)
        {
            try
            {
                string filePath = M2SpacePaths.VisitedPlanetsFile;
                if (!File.Exists(filePath)) File.WriteAllText(filePath, planetName + Environment.NewLine);
                else if (!File.ReadAllLines(filePath).Contains(planetName)) File.AppendAllText(filePath, planetName + Environment.NewLine);
            }
            catch (Exception ex) { Debug.LogWarning("[ModernBox Space] " + ex.Message); }
        }

        private bool IsPlanetVisited(string planetName)
        {
            try
            {
                return File.Exists(M2SpacePaths.VisitedPlanetsFile) && File.ReadAllLines(M2SpacePaths.VisitedPlanetsFile).Contains(planetName);
            }
            catch (Exception) { return false; }
        }

        private void Update()
        {
            if (mainCamera == null) return;
            hoveredStar = isGeneratingStars ? null : GetHoveredStar();

            float moveX = Axis("Horizontal", KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow);
            float moveY = Axis("Vertical", KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow);
            mainCamera.transform.position += new Vector3(moveX, moveY, 0) * moveSpeed * Time.unscaledDeltaTime;

            float scroll;
            try { scroll = Input.GetAxis("Mouse ScrollWheel"); }
            catch (ArgumentException) { scroll = Input.mouseScrollDelta.y * 0.1f; }
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
        }

        private static float Axis(string axis, KeyCode positive, KeyCode positiveAlt, KeyCode negative, KeyCode negativeAlt)
        {
            try { return Input.GetAxis(axis); }
            catch (ArgumentException)
            {
                float value = 0f;
                if (Input.GetKey(positive) || Input.GetKey(positiveAlt)) value += 1f;
                if (Input.GetKey(negative) || Input.GetKey(negativeAlt)) value -= 1f;
                return value;
            }
        }

        private M2StarData GetHoveredStar()
        {
            if (stars.Count == 0) return null;
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            foreach (M2StarData star in stars)
            {
                if (Vector2.Distance(mousePos, star.position) < 0.5f) return star;
            }
            return null;
        }

        private static string GenerateStarName()
        {
            string completedStarName = "";
            foreach (char part in pattern)
            {
                if (part == 'p') completedStarName += prefixes[rand.Next(prefixes.Length)];
                else if (part == 's') completedStarName += syllables[rand.Next(syllables.Length)];
                else if (part == 'x') completedStarName += suffixes[rand.Next(suffixes.Length)];
            }
            return completedStarName + "-" + UnityEngine.Random.Range(1, 1000);
        }

        private readonly Dictionary<string, float[]> galaxyStarWeights = new Dictionary<string, float[]>
        {
            { "Crabby Way", new float[] { 0.1f, 0.2f, 0.1f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.05f, 0.1f, 0.1f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.00f } },
            { "Tuxxus", new float[] { 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.05f, 0.04f, 0.04f, 0.04f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.00f } },
            { "Krummple", new float[] { 0.1f, 0.2f, 0.1f, 0.1f, 0.05f, 0.05f, 0.1f, 0.05f, 0.05f, 0.1f, 0.1f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.00f } },
            { "BlueNight", new float[] { 0.1f, 0.1f, 0.3f, 0.05f, 0.05f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.04f, 0.04f, 0.04f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.00f } },
            { "Glass", new float[] { 0.1f, 0.1f, 0.3f, 0.05f, 0.05f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.04f, 0.04f, 0.04f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.00f } },
            { "Centuga", new float[] { 0.05f, 0.05f, 0.15f, 0.05f, 0.1f, 0.1f, 0.15f, 0.05f, 0.05f, 0.1f, 0.05f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.00f } },
            { "Dank", new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.2f, 0.1f, 0.05f, 0.05f, 0.1f, 0.05f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 1f } },
        };

        private static readonly string[] starTypes =
        {
            "Red Dwarf", "Yellow Dwarf", "Blue Giant", "White Dwarf", "Neutron Star", "Brown Dwarf", "Supergiant", "Pulsar",
            "White Supergiant", "Red Supergiant", "Black Hole", "Rainbow Star", "Void Star", "Crystal Star", "Quantum Star",
            "Echo Star", "Chrono Star", "Phantom Star", "Prism Star", "Nebula Star", "Graviton Star", "Aurora Star", "Corrupted Star"
        };

        private string GenerateStarType()
        {
            if (!galaxyStarWeights.TryGetValue(activeGalaxy, out float[] weights))
                weights = new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
            float totalWeight = weights.Sum();
            if (totalWeight <= 0f) return starTypes[0];
            float randomValue = UnityEngine.Random.value;
            float cumulativeWeight = 0f;
            for (int i = 0; i < weights.Length && i < starTypes.Length; i++)
            {
                cumulativeWeight += weights[i] / totalWeight;
                if (randomValue <= cumulativeWeight) return starTypes[i];
            }
            return starTypes[starTypes.Length - 1];
        }

        private string GetGalaxyDescription(string galaxy)
        {
            switch (galaxy)
            {
                case "Crabby Way": return L("crabby_way");
                case "Tuxxus": return L("tuxxus");
                case "Krummple": return L("krummple");
                case "BlueNight": return L("bluenight");
                case "Glass": return L("glass");
                case "Centuga": return L("centuga");
                case "Dank": return L("dank");
                default: return CustomGalaxyDescriptions.TryGetValue(galaxy, out string description) ? description : "No description available.";
            }
        }

        private static string ToRomanNumeral(int number)
        {
            string[] romanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
            return number < 1 || number > romanNumerals.Length ? number.ToString() : romanNumerals[number - 1];
        }

        private void ClearStars()
        {
            stars.Clear();
            starObjects.Clear();
            if (worldRoot == null) return;
            for (int i = worldRoot.childCount - 1; i >= 0; i--) Destroy(worldRoot.GetChild(i).gameObject);
        }

        private void OnDestroy()
        {
            if (gradientTexture != null) Destroy(gradientTexture);
        }

        private string GenerateNormalDescription(string starType, string planetType)
        {
            var surfacePhrases = new List<string>
            {
                "features a diverse landscape with mountains and valleys",
                "is covered in vast plains with scattered rocky formations",
                "has a smooth, oceanic surface with occasional island chains",
                "boasts an icy crust with hidden liquid oceans beneath",
                "is adorned with glowing crystals that refract light into beautiful patterns",
                "is enveloped in a thick, murky swamp with bioluminescent flora",
                "is dominated by rivers of molten lava and active volcanoes",
                "has floating islands within its gaseous layers, creating an ethereal landscape",
                "is covered by an artificial surface with advanced technology remnants",
                "experiences extreme tidal forces creating massive ocean swells and geological instability"
            };
            var climatePhrases = new List<string>
            {
                "experiences a temperate climate with mild seasons",
                "has a tropical climate with heavy rainfall year-round",
                "enjoys a cool, arid climate with minimal weather variation",
                "features extreme temperature fluctuations between day and night",
                "has a stable, mild climate due to the crystal structure's influence",
                "features a humid and unstable climate with frequent fog and rain",
                "is subject to intense heat and volcanic activity",
                "maintains a stable climate within floating habitats despite external gas turbulence",
                "has a climate controlled by sophisticated machinery, maintaining a constant temperature",
                "experiences extreme climate shifts due to tidal forces"
            };
            var atmospherePhrases = new List<string>
            {
                "has a breathable atmosphere with a balanced composition",
                "features a thick atmosphere rich in exotic gases",
                "boasts a thin atmosphere with limited oxygen",
                "possesses a highly variable atmosphere with frequent weather changes",
                "has an atmosphere rich in mineral vapors that contribute to its unique appearance",
                "contains dense, toxic gases making it unsuitable for most life forms",
                "features a highly corrosive atmosphere due to volcanic activity",
                "has breathable layers within the upper atmosphere but harsh conditions below",
                "has an artificial atmosphere created for specific technological purposes",
                "features an atmosphere with extreme pressure variations due to tidal effects"
            };
            var floraFaunaPhrases = new List<string>
            {
                "home to a variety of unique flora and fauna",
                "inhabited by a few hardy species adapted to harsh conditions",
                "features a rich ecosystem with vibrant and diverse life forms",
                "has a sparse and resilient biosphere with minimal life",
                "is inhabited by crystal-based life forms that have unique energy properties",
                "hosts bioluminescent plants and aggressive swamp creatures",
                "is home to resilient life forms adapted to the extreme heat",
                "features floating flora and fauna within its gaseous layers",
                "is populated by robotic life forms and advanced machinery",
                "has a unique ecosystem affected by the extreme tidal forces"
            };
            var geologicalPhrases = new List<string>
            {
                "has an active geological landscape with frequent volcanic activity",
                "features ancient geological formations with minimal tectonic activity",
                "is characterized by a dynamic crust with shifting tectonic plates",
                "displays a stable geological environment with few changes over millennia",
                "boasts crystalline geological formations that create stunning visual effects",
                "features shifting swamp terrain with unstable ground",
                "is characterized by a molten surface with ongoing volcanic eruptions",
                "has floating landmasses with stable geological features despite gaseous turbulence",
                "has a surface covered by artificial structures and technology",
                "experiences frequent geological upheavals due to tidal forces"
            };

            List<string>[] order;
            switch (starType)
            {
                case "Red Dwarf": order = new[] { surfacePhrases, climatePhrases, atmospherePhrases, floraFaunaPhrases }; break;
                case "Supergiant": case "Void Star": order = new[] { surfacePhrases, climatePhrases, floraFaunaPhrases, atmospherePhrases }; break;
                case "Yellow Dwarf": case "Rainbow Star": order = new[] { climatePhrases, surfacePhrases, floraFaunaPhrases, geologicalPhrases }; break;
                case "Blue Giant": order = new[] { atmospherePhrases, surfacePhrases, geologicalPhrases, climatePhrases }; break;
                case "White Dwarf": order = new[] { surfacePhrases, geologicalPhrases, floraFaunaPhrases, atmospherePhrases }; break;
                case "Neutron Star": order = new[] { geologicalPhrases, climatePhrases, floraFaunaPhrases, surfacePhrases }; break;
                case "Brown Dwarf": order = new[] { atmospherePhrases, surfacePhrases, geologicalPhrases, climatePhrases }; break;
                case "Pulsar": order = new[] { geologicalPhrases, surfacePhrases, climatePhrases, floraFaunaPhrases }; break;
                case "White Supergiant": case "Black Hole": order = new[] { surfacePhrases, atmospherePhrases, floraFaunaPhrases, geologicalPhrases }; break;
                case "Red Supergiant": order = new[] { climatePhrases, surfacePhrases, floraFaunaPhrases, atmospherePhrases }; break;
                case "Crystal Star": order = new[] { surfacePhrases, geologicalPhrases, atmospherePhrases, floraFaunaPhrases }; break;
                case "Quantum Star": case "Phantom Star": order = new[] { surfacePhrases, climatePhrases, geologicalPhrases, floraFaunaPhrases }; break;
                case "Echo Star": case "Chrono Star": order = new[] { atmospherePhrases, surfacePhrases, floraFaunaPhrases, climatePhrases }; break;
                case "Prism Star": case "Graviton Star": order = new[] { surfacePhrases, climatePhrases, floraFaunaPhrases, geologicalPhrases }; break;
                case "Nebula Star": case "Aurora Star": order = new[] { surfacePhrases, climatePhrases, atmospherePhrases, floraFaunaPhrases }; break;
                default: order = null; break;
            }

            string description;
            if (order == null) description = "The planet features a diverse and dynamic environment.";
            else if (starType == "Blue Giant") description = RandomPhrase(order[0]) + ", " + RandomPhrase(order[1]) + ", " + RandomPhrase(order[2]) + ". " + RandomPhrase(order[3]) + ". ";
            else description = RandomPhrase(order[0]) + ", " + RandomPhrase(order[1]) + ". " + RandomPhrase(order[2]) + ", " + RandomPhrase(order[3]) + ". ";

            switch (planetType)
            {
                case "Desert World": description += "The desert surface is a notable feature of this planet."; break;
                case "Gas Giant": description += "The gaseous composition creates a unique and visually stunning environment."; break;
                case "Oceanic": description += "The extensive oceans provide a rich and dynamic ecosystem."; break;
                case "Icy": description += "The icy surface presents a stark, yet beautiful landscape."; break;
                case "Crystal World": description += "The planet's surface is covered in luminous crystals that create a surreal visual effect."; break;
                case "Swamp World": description += "The planet is shrouded in murky swamps with glowing, bioluminescent vegetation."; break;
                case "Lava World": description += "The planet features an intense volcanic landscape with rivers of molten lava."; break;
                case "Gas Giant (Habitable)": description += "Floating islands and habitats within the upper atmosphere provide a unique living environment."; break;
                case "Mechanical World": description += "An artificial surface dominated by advanced technology and remnants of a bygone civilization."; break;
                case "Jungle World": description += "Filled with dense flora and diverse fauna."; break;
                case "Corrupted World": description += "The planet is a desolate and twisted wasteland, corrupted by unknown forces."; break;
                case "Lemon World": description += "The surface is covered in yellow, citrus-like vegetation, creating a surreal and fragrant landscape."; break;
                case "Mushroom World": description += "Giant mushrooms dominate the landscape, providing shelter and resources to the unique ecosystem."; break;
                case "Wasteland World": description += "The planet is a barren and scorched wasteland, with ruins of ancient civilizations scattered across its surface."; break;
                default: description += "The planet features a diverse and dynamic environment."; break;
            }
            return description;
        }

        private static string RandomPhrase(List<string> phrases)
        {
            return phrases[UnityEngine.Random.Range(0, phrases.Count)];
        }

        private static Dictionary<string, int> GenerateResources()
        {
            Dictionary<string, int> resources = new Dictionary<string, int>();
            foreach (string resource in new[] { "Steel", "Soil", "Water", "Gold", "Uranium" }) resources[resource] = UnityEngine.Random.Range(0, 1000);
            return resources;
        }

        private string GenerateDangerDescription(string starType, string planetType)
        {
            var dangerPhrases = new List<string>
            {
                "The planet is plagued by frequent seismic activity.",
                "The atmosphere is filled with toxic gases, making it uninhabitable.",
                "The planet experiences extreme weather conditions with frequent storms.",
                "Radiation from the star poses a serious threat to any form of life.",
                "The surface is unstable, with constant volcanic eruptions.",
                "The planet's gravity creates intense tidal forces, causing severe geological instability.",
                "The environment is filled with hazardous materials and contaminants.",
                "Frequent meteor showers bombard the surface, creating dangerous conditions.",
                "The planet's atmosphere is corrosive and hostile to most known life forms.",
                "Extreme temperature fluctuations make survival difficult and unpredictable."
            };

            string description;
            switch (starType)
            {
                case "Red Dwarf": description = RandomPhrase(dangerPhrases) + ", due to the star's minimal luminosity and unstable radiation output. This creates a challenging environment for survival and exploration."; break;
                case "Yellow Dwarf": description = RandomPhrase(dangerPhrases) + ", resulting from the planet's position in the habitable zone with fluctuating conditions. The diverse but hazardous environment demands careful navigation."; break;
                case "Blue Giant": description = RandomPhrase(dangerPhrases) + ", because of the intense radiation and high energy output of the star. These factors contribute to a hazardous environment for both equipment and life."; break;
                case "White Dwarf": description = RandomPhrase(dangerPhrases) + ", due to the star's extreme density and high gravitational pull. This causes severe environmental hazards and challenges for exploration."; break;
                case "Neutron Star": description = RandomPhrase(dangerPhrases) + ", caused by the intense gravitational forces and high radiation levels from the star. These conditions make the planet's environment extremely dangerous."; break;
                case "Brown Dwarf": description = RandomPhrase(dangerPhrases) + ", as the star's low energy output leads to unstable conditions on nearby planets. Explorers must be prepared for unpredictable hazards."; break;
                case "Supergiant": description = RandomPhrase(dangerPhrases) + ", due to the massive energy output and unstable stellar conditions. The extreme environment presents significant challenges for survival."; break;
                case "Pulsar": description = RandomPhrase(dangerPhrases) + ", due to the pulsar's intense radiation bursts and high-energy emissions. These factors create hazardous conditions for any exploration or habitation."; break;
                case "White Supergiant": description = RandomPhrase(dangerPhrases) + ", resulting from the star's enormous size and high energy levels. The environment is fraught with danger due to these extreme conditions."; break;
                case "Red Supergiant": description = RandomPhrase(dangerPhrases) + ", because of the star's immense size and unstable energy output. These factors contribute to a highly dangerous environment."; break;
                case "Black Hole": description = "The planet orbits a black hole, creating extreme gravitational effects and intense tidal forces. The environment is highly hazardous with severe space-time distortions and gravitational disruptions."; break;
                default: description = "Planet faces danger due to " + RandomPhrase(dangerPhrases) + ". These conditions result from its proximity to " + starType + ". The hazardous environment requires careful consideration for exploration."; break;
            }

            switch (planetType)
            {
                case "Desert World": description += " The desert surface is prone to seismic instability and geological hazards."; break;
                case "Gas Giant": description += " The gaseous composition creates unpredictable atmospheric conditions and hazardous weather patterns."; break;
                case "Oceanic": description += " The extensive oceans contribute to dangerous weather conditions and extreme climate variations."; break;
                case "Icy": description += " The icy surface creates challenges for exploration due to extreme cold and unstable terrain."; break;
                case "Crystal World": description += " The crystalline surface is not only visually stunning but also presents unique environmental hazards."; break;
                case "Swamp World": description += " The swampy environment is filled with hazardous gases and dangerous wildlife."; break;
                case "Lava World": description += " The volcanic activity creates severe hazards, including constant lava flows and unstable terrain."; break;
                case "Gas Giant (Habitable)": description += " The floating habitats are subject to unpredictable atmospheric conditions and hazards."; break;
                case "Mechanical World": description += " The artificial surface and technological remnants create environmental hazards due to malfunctioning systems."; break;
                case "Jungle World": description += " Extreme tidal forces create severe geological instability and environmental challenges."; break;
                case "Corrupted World": description += "The planet is highly unstable, with areas of corruption that could pose unknown dangers."; break;
                case "Lemon World": description += "While the surface appears harmless, the acidic environment poses a hidden threat."; break;
                case "Mushroom World": description += "The giant mushrooms release spores that can be toxic to unprepared explorers."; break;
                case "Wasteland World": description += "The harsh environment and radiation pockets make this planet extremely dangerous."; break;
            }
            return description;
        }
    }
}
