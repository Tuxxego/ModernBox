//=============================BY TUXXEGO====================================
// 
// NOTE FROM TUX: this file is a complete mess i'd avoid changing stuff in here if i were you.
//
//===========================================================================
using System;
using UnityEngine;
using System.Collections.Generic;
using ModernBox;
using System.IO;
using Newtonsoft.Json; 
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

public class StarManager : MonoBehaviour
{
    private List<Star> stars = new List<Star>(); 
    private List<string> favoritesList = new List<string>();
    private string favoritesFilePath;
    private Star hoveredStar;
    private Star selectedStar;
    private Camera mainCamera;
    public float moveSpeed = 15f; 
    public float zoomSpeed = 20f; 
    public float minZoom = 1f; 
    public float maxZoom = 50f; 
    private Vector3 lastMousePosition;
    private bool isDragging = false;
    public float starDensity = 0.7f; 
    public float nebulaDensity = 0.05f; 
    public float nebulaSize = 6f; 
    private bool showDarkVoidWindow = false;
    private bool showDarkVoidRelationshipWindow = false;
    private bool showPlanetWindow = false;
	private bool showParametersWindow = false; 
    private bool showGalaxySelectionWindow = false;
    private bool showFavoritesWindow = false; 
	private bool showCompendiumWindow = false;
    private List<CompendiumEntry> compendiumEntries = new List<CompendiumEntry>
    {
        new CompendiumEntry("Star Types", "There are several types of stars such as Red Dwarfs, Yellow Dwarfs, and Blue Giants."),
        new CompendiumEntry("What will be here", "Stuff on creatures, planets, etc will be here."),
    };
    private Vector2 compendiumScrollPosition;
	private Vector2 sidebarScrollPosition;
    private Vector2 contentScrollPosition;
    private string currentTitle = "Welcome";
    private string currentContent = "Welcome to the Compendium! Here you will find information on various aspects of the universe.";
	private List<string> journeyTracker = new List<string>();
	private bool showJourneyTrackerWindow = false;
	private LocalizationManager localizationManager;
    private string _lastLoadedPlanetSave = "";
    private bool _cachedSaveExists = false;
    private int _cachedKingdomCount = 0;
    private int _cachedCityCount = 0;
    private int _cachedWorldYear = 0;
    private string regionName = "Unknown Region";
    private string galaxyName = "Crabby Way";
    private string[] predefinedGalaxies = { "Crabby Way", "Tuxxus", "Krummple", "BlueNight", "Glass", "Centuga", "Dank" };
	private Dictionary<string, int> galaxyRequirements = new Dictionary<string, int>
	{
		{ "Crabby Way", 0 }, 
		{ "Tuxxus", 10 }, 
		{ "Krummple", 20 }, 
		{ "BlueNight", 30 }, 
		{ "Glass", 45 }, 
		{ "Centuga", 55 }, 
		{ "Dank", 85 } 
	};
        private Dictionary<string, int> galaxyStarCounts = new Dictionary<string, int>
        {
            { "Crabby Way", 600 },
            { "Tuxxus", 1200 },
            { "Krummple", 800 },
            { "BlueNight", 2300 },
            { "Glass", 1800 },
			{ "Centuga", 1700 }, 
            { "Dank", 300 }
        };
        private Dictionary<string, int> galaxyDangerRatings = new Dictionary<string, int>
        {
            { "Crabby Way", 1 },
            { "Tuxxus", 3 },
            { "Krummple", 2 },
            { "BlueNight", 4 },
            { "Glass", 4 },
			{ "Centuga", 4 }, 
            { "Dank", 5 }
        };
		public string activeGalaxy
		{
			get
			{
				if (string.IsNullOrEmpty(galaxyName))
				{
					LoadActiveGalaxy(); 
				}
				return galaxyName;
			}
			set
			{
				galaxyName = value;
				SaveActiveGalaxy(galaxyName);
			}
		}
    private string activeGalaxyFilePath;
    private string galaxyDescription = "";
    private AudioSource audioSource;
    private AudioClip backgroundMusic;
    private bool showTutorialPrompt = false;
    private bool showTutorialStep1 = false;
    private bool showTutorialStep2 = false;
    private bool tutorialShown = false;
    private string appDataLocation = Path.Combine(Application.persistentDataPath, "ModernBox");
    private bool isGeneratingStars = false;
    private bool starsGenerationComplete = false;
	private bool showSearchWindow = false;
    private static Texture2D _panelBg;
    private static Texture2D _accentLine;
    private static Texture2D _btnNormal;
    private static Texture2D _btnVisit;
    private static Texture2D _btnClose;
    private static Texture2D _statCard;
    private static Texture2D _visitedBadge;
    private static Texture2D _barBg;
    private static Texture2D _btnTex;
    private static Texture2D _btnHoverTex;
    private static Texture2D _btnStarTex;
    private static Texture2D _btnStarHoverTex;
    private static Texture2D _btnSecretTex;
    private static Texture2D _btnSecretHoverTex;
    private static Texture2D _badgeTex;
    private static Texture2D _dividerTex;
    private float _windowOpenTime;
	private string searchQuery = "";
	private Vector2 scrollPosition;
	private bool showFilterWindow = false;
	private bool applyFilters = false;
	private string selectedStarType = "Any";
	private int minPlanets = 0;
	private int maxPlanets = int.MaxValue;
	private bool useFiltering = false; 
    private static System.Random rand = new System.Random();
    private static string pattern = "psx";
    private static string completedStarName = "";
    private static string[] prefixes = { "Al", "Betel", "Proxi", "Vega", "Anta", "Capel", "Siri", "Tauri", "Poll", "Cen", "Alpha", "Beta", "Delta", "Epsilon", "Gamma", "Cenu" };
    private static string[] syllables = { "al", "bel", "den", "mar", "nus", "zar", "pho", "cos", "tera", "van", "ly", "pe", "xor", "sta", "pro", "lux", "tor", "el", "ris", "zir", "quar", "vix" };
    private static string[] suffixes = { "on", "or", "a", "us", "ae", "um", "is", "us", "i", "an", "es", "ia", "ar", "il", "or", "an", "is" };
    private string galaxiesFolder = Path.Combine(Application.dataPath, "../galaxies/");
	private Dictionary<string, string> CustomGalaxyDescriptions = new Dictionary<string, string>();
	private int loadedGalaxyCount = 0; 
	private bool isCustomGalaxiesMode = false; 
	private Dictionary<string, (Color nebulaColor1, Color nebulaColor2)> CustomGalaxyNebulas = new Dictionary<string, (Color, Color)>();
	private Dictionary<string, (Color nebulaColor1, Color nebulaColor2)> predefinedNebulas = new Dictionary<string, (Color, Color)>
	{
		{ "Crabby Way", (new Color(1f, 1f, 1f, 0.4f), new Color(1f, 1f, 1f, 0.2f)) },
		{ "Tuxxus", (new Color(1f, 0.843f, 0f, 0.5f), new Color(1f, 0.843f, 0f, 0.3f)) },
		{ "Krummple", (new Color(1f, 0f, 0f, 0.5f), new Color(0f, 1f, 0f, 0.3f)) },
		{ "BlueNight", (new Color(0f, 0f, 1f, 0.5f), new Color(0f, 0f, 1f, 0.3f)) },
		{ "Glass", (new Color(0.5f, 0f, 0.5f, 0.5f), new Color(0.7f, 0f, 0.7f, 0.3f)) },
		{ "Centuga", (new Color(0f, 1f, 1f, 0.5f), new Color(0f, 1f, 1f, 0.3f)) },
		{ "Dank", (new Color(0.5f, 0f, 0f, 0.5f), new Color(0.5f, 0f, 0f, 0.3f)) }
	};
	private Dictionary<string, bool> GlassGalaxies = new Dictionary<string, bool>
	{
		{ "Glass", true }
	};
    private List<string> randomFacts = new List<string>
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
		"I WANNA STAR WARS MOD!!!!!!",
        "Sim if you are reading this that means I have brought you back to life at least TWICE."
    };

    private void Start()
    {
		localizationManager = FindObjectOfType<LocalizationManager>();
        activeGalaxyFilePath = Path.Combine(Application.persistentDataPath, "activeGalaxy.txt");
        LoadActiveGalaxy();
        LoadGalaxies();
        StartCoroutine(InitializeAndGenerateStars());
		LoadJourneyTracker();
    }

	public void LoadGalaxies()
	{
		string galaxiesFolder = Path.Combine(Application.dataPath, "../galaxies/");
		
		if (!Directory.Exists(galaxiesFolder))
		{
			Directory.CreateDirectory(galaxiesFolder);
			Debug.Log($"Created galaxies folder at: {galaxiesFolder}");

			string readmePath = Path.Combine(galaxiesFolder, "CustomGalaxiesReadme.txt");
			string readmeContent = "Join the discord for help and custom galaxy downloads.";
			File.WriteAllText(readmePath, readmeContent);
			Debug.Log($"Created readme file at: {readmePath}");

			return;
		}

		string[] galaxyFiles = Directory.GetFiles(galaxiesFolder, "*.gal");

		if (galaxyFiles.Length == 0)
		{
			Debug.Log("No .gal files found. Skipping galaxy loading.");
			return;
		}

		Dictionary<string, bool> galaxyStates = LoadGalaxiesState();

		foreach (string filePath in galaxyFiles)
		{
			try
			{
				string json = File.ReadAllText(filePath);
				GalaxyData galaxy = JsonUtility.FromJson<GalaxyData>(json);

				if (galaxyStates.ContainsKey(galaxy.name) && galaxyStates[galaxy.name])
				{
					predefinedGalaxies = AddToArray(predefinedGalaxies, galaxy.name);
					CustomGalaxyDescriptions[galaxy.name] = galaxy.description;
					galaxyRequirements[galaxy.name] = galaxy.requirement;
					galaxyStarCounts[galaxy.name] = galaxy.starCount;
					galaxyDangerRatings[galaxy.name] = galaxy.dangerRating;
					galaxyStarWeights[galaxy.name] = galaxy.starWeights;

					if (galaxy.nebulaColor1 != null && galaxy.nebulaColor2 != null)
					{
						Color nebula1 = new Color(galaxy.nebulaColor1[0], galaxy.nebulaColor1[1], galaxy.nebulaColor1[2], galaxy.nebulaColor1[3]);
						Color nebula2 = new Color(galaxy.nebulaColor2[0], galaxy.nebulaColor2[1], galaxy.nebulaColor2[2], galaxy.nebulaColor2[3]);
						CustomGalaxyNebulas[galaxy.name] = (nebula1, nebula2);
					}

					if (galaxy.GlassStructure)
					{
						GlassGalaxies[galaxy.name] = true;
					}

					loadedGalaxyCount++;
					Debug.Log($"Galaxy '{galaxy.name}' loaded successfully.");
				}
				else
				{
					Debug.Log($"Galaxy '{galaxy.name}' is disabled and will not be loaded.");
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error loading galaxy file '{filePath}': {ex.Message}");
			}
		}
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
				List<GalaxyData> galaxies = LoadGalaxiesFromThatFuckingFile();
				Dictionary<string, bool> galaxyStates = new Dictionary<string, bool>();
				foreach (var galaxy in galaxies)
				{
					galaxyStates[galaxy.name] = true;
				}
				SaveGalaxiesState(galaxyStates);
				return galaxyStates;
			}
		}

        private static List<GalaxyData> LoadGalaxiesFromThatFuckingFile()
        {
            string galaxiesDirectory = Path.Combine(Application.dataPath, "../galaxies/");
            List<GalaxyData> galaxies = new List<GalaxyData>();

            if (Directory.Exists(galaxiesDirectory))
            {
                string[] galaxyFiles = Directory.GetFiles(galaxiesDirectory, "*.gal");
                foreach (var file in galaxyFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        GalaxyData galaxy = JsonConvert.DeserializeObject<GalaxyData>(json);
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


    private string[] AddToArray(string[] array, string newItem)
    {
        var list = new List<string>(array) { newItem };
        return list.ToArray();
    }
	
    private void LoadActiveGalaxy()
    {
        if (File.Exists(activeGalaxyFilePath))
        {
            galaxyName = File.ReadAllText(activeGalaxyFilePath).Trim();
            Debug.Log($"Loaded active galaxy: {galaxyName}");
        }
        else
        {
            galaxyName = "Crabby Way";
            activeGalaxy = "Crabby Way";
            Debug.LogWarning($"activeGalaxy.txt not found. Defaulting to {galaxyName}");
        }
    }

    private void SaveActiveGalaxy(string galaxy)
    {
        File.WriteAllText(activeGalaxyFilePath, galaxy);
        Debug.Log($"Saved active galaxy: {galaxy}");
    }

    private IEnumerator InitializeAndGenerateStars()
    {
        CreateMainCamera();

        if (mainCamera == null)
        {
            Debug.LogError("Failed to create the main camera.");
            yield break;
        }

        if (File.Exists(activeGalaxyFilePath))
        {
            galaxyName = File.ReadAllText(activeGalaxyFilePath).Trim();
            Debug.Log($"Loaded active galaxy: {galaxyName}");
        }
        else
        {
            galaxyName = "Crabby Way";
            activeGalaxy = "Crabby Way";
            Debug.LogWarning($"activeGalaxy.txt not found. Defaulting to {galaxyName}");
        }

        yield return null;

        string galaxyPath = Path.Combine(appDataLocation, activeGalaxy, "Galaxies", galaxyName);

        if (Directory.Exists(galaxyPath))
        {
            isGeneratingStars = true;
        yield return null;
            GenerateLoadedStars(galaxyPath);
        }
        else
        {
            isGeneratingStars = true;
        yield return null;
            GenerateStars();
        }

        favoritesFilePath = Path.Combine(Application.persistentDataPath, "favorites.txt");
        Debug.Log($"Favorites file path: {favoritesFilePath}");
        LoadFavorites();

            showTutorialPrompt = true;

        SetRandomPlanetIfNeeded();

    }

    private void CreateMainCamera()
    {

        Camera existingCamera = Camera.main;
        if (existingCamera != null)
        {
            mainCamera = existingCamera;
            Debug.Log("Using existing main camera.");
            return;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        mainCamera = cameraObject.AddComponent<Camera>();
        mainCamera.tag = "MainCamera"; 
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20; 
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;
        mainCamera.transform.position = new Vector3(0, 0, -10); 
        Debug.Log("Created a new main camera.");
    }

private void GenerateStars()
{
    ClearStars();
    ClearAllActiveSprites();
    GenerateNebulas();

    if (mainCamera == null)
    {
        Debug.LogError("Main camera is not available for generating stars.");
        return;
    }

    if (!galaxyStarCounts.ContainsKey(activeGalaxy))
    {
        Debug.LogError($"Galaxy '{activeGalaxy}' is not found in the star counts dictionary.");
        return;
    }

    float extraWidth = 100;
    float extraHeight = 100;

    int starCount = galaxyStarCounts[activeGalaxy];
    float cameraWidth = (mainCamera.orthographicSize * mainCamera.aspect * 2) + extraWidth;
    float cameraHeight = (mainCamera.orthographicSize * 2) + extraHeight;

    for (int i = 0; i < starCount; i++)
    {
        GameObject starObject = new GameObject("Star");
        Star star = starObject.AddComponent<Star>();

        SpriteRenderer spriteRenderer = starObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateStarSprite(star);

        if (spriteRenderer.sprite == null)
        {
            Debug.LogError("Failed to create star sprite.");
            continue;
        }

        star.transform.localScale = new Vector3(0.3f, 0.3f, 1);

        Vector3 starPosition;

            if (activeGalaxy == "Glass")
            {

                starPosition = GeneratePositionInSpiralArms(cameraWidth, cameraHeight);
				IsPositionInShatteredArea(starPosition);
            }
            else
            {

                float posX = UnityEngine.Random.Range(-cameraWidth / 2, cameraWidth / 2);
                float posY = UnityEngine.Random.Range(-cameraHeight / 2, cameraHeight / 2);
                starPosition = new Vector3(posX, posY, 0);
            }

        star.transform.position = starPosition;

        star.name = GenerateStarName();
        star.starType = GenerateStarType();
        star.planetCount = UnityEngine.Random.Range(1, 10);

      //  spriteRenderer.color = GetStarColor(star.starType);

        GeneratePlanetsForStar(star);

        stars.Add(star);
    }

    SaveStarsAndPlanets();
    starsGenerationComplete = true;
    isGeneratingStars = false;
}

private Vector3 GeneratePositionInSpiralArms(float cameraWidth, float cameraHeight)
{
    float radius = UnityEngine.Random.Range(0.5f, cameraWidth / 2);
    float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
    float spiralOffset = radius * 0.2f * Mathf.Sin(7 * angle); 

    float posX = radius * Mathf.Cos(angle) + spiralOffset;
    float posY = radius * Mathf.Sin(angle) + spiralOffset;

    return new Vector3(posX, posY, 0);
}

private bool IsPositionInShatteredArea(Vector3 position)
{

    float waveFrequency = 0.1f;
    float waveAmplitude = 50f;

    float wave = waveAmplitude * Mathf.Sin(position.x * waveFrequency);

    bool inExclusionZone = Mathf.Abs(position.y - wave) < 10f; 

    if (inExclusionZone)
    {

    }

    return inExclusionZone;
}

private void GenerateNebulas()
{
    float extraWidth = 100;
    float extraHeight = 100;

    int nebulaCount = Mathf.RoundToInt(nebulaDensity * Mathf.Pow((mainCamera.orthographicSize * 2) + extraWidth, 2));
    float cameraWidth = (mainCamera.orthographicSize * mainCamera.aspect * 2) + extraWidth;
    float cameraHeight = (mainCamera.orthographicSize * 2) + extraHeight;

    Sprite sharedNebulaSprite = CreateNebulaSprite(activeGalaxy);

    if (sharedNebulaSprite == null)
    {
        Debug.LogError("Failed to create nebula sprite.");
        return;
    }

    bool useSpiralArms = activeGalaxy == "Glass" || 
                         (GlassGalaxies.ContainsKey(activeGalaxy) && GlassGalaxies[activeGalaxy]);

    for (int i = 0; i < nebulaCount; i++)
    {
        GameObject nebulaObject = new GameObject("Nebula");
        SpriteRenderer spriteRenderer = nebulaObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = sharedNebulaSprite;

        Vector3 nebulaPosition;

        do
        {
            if (useSpiralArms)
            {
                nebulaPosition = GeneratePositionInSpiralArms(cameraWidth, cameraHeight);
            }
            else
            {
                float posX = UnityEngine.Random.Range(-cameraWidth / 2, cameraWidth / 2);
                float posY = UnityEngine.Random.Range(-cameraHeight / 2, cameraHeight / 2);
                nebulaPosition = new Vector3(posX, posY, -1);
            }
        }
        while (IsPositionInShatteredArea(nebulaPosition)); 

        nebulaObject.transform.position = nebulaPosition;
        nebulaObject.transform.localScale = new Vector3(nebulaSize, nebulaSize, 1);
    }
}

private Sprite CreateNebulaSprite(string galaxyName)
{
    int width = 256;
    int height = 256;
    Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

    float centerX = width / 2f;
    float centerY = height / 2f;
    float maxDistance = width / 2f; 

    Color nebulaColor1;
    Color nebulaColor2;

    if (predefinedNebulas.TryGetValue(galaxyName, out var colors))
    {
        (nebulaColor1, nebulaColor2) = colors;
    }
    else if (CustomGalaxyNebulas.TryGetValue(galaxyName, out var customColors))
    {
        (nebulaColor1, nebulaColor2) = customColors;
    }
    else
    {
        nebulaColor1 = new Color(1f, 1f, 1f, 0.4f);
        nebulaColor2 = new Color(1f, 1f, 1f, 0.2f);
    }

    float scale = 0.1f;
    float intensity = 0.6f;

    Color[] pixels = new Color[width * height];

    for (int y = 0; y < height; y++)
    {
        float perlinY = y * scale;
        float distY = y - centerY;
        float distYSq = distY * distY; 

        for (int x = 0; x < width; x++)
        {
            float perlinX = x * scale;
            float distX = x - centerX;

            float distanceFromCenter = Mathf.Sqrt((distX * distX) + distYSq);
            float normalizedDistance = distanceFromCenter / maxDistance;

            float noise = Mathf.PerlinNoise(perlinX, perlinY);

            float edgeAlpha = Mathf.Clamp01(1f - normalizedDistance);
            edgeAlpha *= Mathf.Lerp(1f, 0f, normalizedDistance * 2f); 

            float radialEffect = Mathf.Clamp01(1f - normalizedDistance);
            Color nebulaColor = Color.Lerp(nebulaColor1, nebulaColor2, noise * intensity * radialEffect);
            nebulaColor.a *= edgeAlpha;

            pixels[y * width + x] = nebulaColor;
        }
    }

    texture.SetPixels(pixels);
    texture.Apply();

    return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
}

private Color RandomColor(Color[] colors)
{
    return colors[UnityEngine.Random.Range(0, colors.Length)];
}

private void GeneratePlanetsForStar(Star star)
{
    star.planetInfo = new PlanetInfo[star.planetCount];
    string starSystemName = star.name;
    string starType = star.starType; 

    for (int i = 0; i < star.planetCount; i++)
    {
        string planetType = GetPlanetTypeBasedOnStar(starType); 

        
        int sizeX = UnityEngine.Random.Range(1, 25);
        int sizeY = UnityEngine.Random.Range(1, 25);
        string size = $"{sizeX} x {sizeY}";

        
        bool hasFauna = UnityEngine.Random.value > 0.5f; 

        star.planetInfo[i] = new PlanetInfo
        {
            name = $"{starSystemName} {ToRomanNumeral(i + 1)}",
            description = GenerateNormalDescription(starType, planetType),
            resources = GenerateResources(),
            dangerRating = UnityEngine.Random.Range(1, 10),
            dangerDescription = GenerateDangerDescription(starType, planetType),
            planetType = planetType,
            size = size, 
            hasFauna = hasFauna 
        };
    }
}

/*
private void GenerateMoonsForStar(PlanetInfo planet)
{
    planet.moonInfo = new MoonInfo[planet.moonCount];
    string planetName = planet.name;
    string starType = star.starType; 

    for (int i = 0; i < star.planetCount; i++)
    {
        string planetType = GetPlanetTypeBasedOnStar(moonType); 

        
        int sizeX = UnityEngine.Random.Range(1, 25);
        int sizeY = UnityEngine.Random.Range(1, 25);
        string size = $"{sizeX} x {sizeY}";

        
        bool hasFauna = UnityEngine.Random.value > 0.5f; 

        star.planetInfo[i] = new PlanetInfo
        {
            name = $"{starSystemName} {ToRomanNumeral(i + 1)}",
            description = GenerateNormalDescription(starType, planetType),
            resources = GenerateResources(),
            dangerRating = UnityEngine.Random.Range(1, 10),
            dangerDescription = GenerateDangerDescription(starType, planetType),
            planetType = planetType,
            size = size, 
            hasFauna = hasFauna 
        };
    }
}
*/

private string GetPlanetTypeBasedOnStar(string starType)
{
    switch (starType)
    {
        case "Red Dwarf":
            return GetRandomPlanetType(new[] { "Desert World", "Icy", "Swamp World", "Mushroom World" });

        case "Yellow Dwarf":
            return GetRandomPlanetType(new[] { "Desert World", "Oceanic", "Chess World", "Lemon World" });

        case "Blue Giant":
            return GetRandomPlanetType(new[] { "Gas Giant", "Crystal World", "Lava World", "Wasteland World" });

        case "White Dwarf":
            return GetRandomPlanetType(new[] { "Desert World", "Icy", "Mechanical World", "Corrupted World" });

        case "Corrupted Star":
            return GetRandomPlanetType(new[] { "Corrupted World" });
			
        case "Neutron Star":
            return GetRandomPlanetType(new[] { "Desert World", "Gas Giant", "Jungle World", "Corrupted World" });

        case "Brown Dwarf":
            return GetRandomPlanetType(new[] { "Icy", "Gas Giant", "Swamp World", "Mushroom World" });

        case "Supergiant":
            return GetRandomPlanetType(new[] { "Gas Giant", "Crystal World", "Lava World", "Wasteland World" });

        case "Pulsar":
            return GetRandomPlanetType(new[] { "Desert World", "Swamp World", "Jungle World", "Corrupted World" });

        case "White Supergiant":
            return GetRandomPlanetType(new[] { "Gas Giant", "Crystal World", "Mechanical World", "Wasteland World" });

        case "Red Supergiant":
            return GetRandomPlanetType(new[] { "Desert World", "Oceanic", "Lava World", "Mushroom World" });

        case "Black Hole":
            return GetRandomPlanetType(new[] { "Desert World", "Icy", "Jungle World", "Corrupted World" });

        case "Rainbow Star":
            return GetRandomPlanetType(new[] { "Crystal World", "Oceanic", "Jungle World", "Lemon World" });

        case "Void Star":
            return GetRandomPlanetType(new[] { "Icy", "Gas Giant", "Lava World", "Corrupted World" });

        case "Crystal Star":
            return GetRandomPlanetType(new[] { "Crystal World", "Mechanical World", "Oceanic", "Lemon World" });

        case "Quantum Star":
            return GetRandomPlanetType(new[] { "Desert World", "Gas Giant", "Swamp World", "Wasteland World" });

        case "Echo Star":
            return GetRandomPlanetType(new[] { "Jungle World", "Oceanic", "Mechanical World", "Lemon World" });

        case "Chrono Star":
            return GetRandomPlanetType(new[] { "Icy", "Desert World", "Crystal World", "Corrupted World" });

        case "Phantom Star":
            return GetRandomPlanetType(new[] { "Swamp World", "Jungle World", "Lava World", "Mushroom World" });

        case "Prism Star":
            return GetRandomPlanetType(new[] { "Oceanic", "Crystal World", "Gas Giant", "Lemon World" });

        case "Nebula Star":
            return GetRandomPlanetType(new[] { "Chess World", "Swamp World", "Icy", "Mushroom World" });

        case "Graviton Star":
            return GetRandomPlanetType(new[] { "Lava World", "Crystal World", "Mechanical World", "Wasteland World" });

        case "Aurora Star":
            return GetRandomPlanetType(new[] { "Oceanic", "Jungle World", "Icy", "Lemon World" });

        default:
            return GetRandomPlanetType(); 
    }
}

private string GetRandomPlanetType(string[] possibleTypes = null)
{
    if (possibleTypes == null)
    {
        possibleTypes = new[] { "Desert World", "Gas Giant", "Oceanic", "Icy", "Crystal World", "Swamp World", "Lava World", "Chess World", "Mechanical World", "Jungle World", "Corrupted World", "Lemon World", "Mushroom World", "Wasteland World" };
    }
    int index = UnityEngine.Random.Range(0, possibleTypes.Length);
    return possibleTypes[index];
}

private Sprite CreateStarSprite(Star star)
{
    string spritePath = GetStarSpritePath(star.starType); 

    Sprite sprite = Resources.Load<Sprite>(spritePath);

    if (sprite == null)
    {
        Debug.LogError($"Sprite not found for star type: {star.starType} at path: {spritePath}");
        return null;
    }

    Texture2D resizedTexture = ResizeTexture(sprite.texture, 64, 64);
    Sprite resizedSprite = Sprite.Create(resizedTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));

    return resizedSprite;
}

private Texture2D ResizeTexture(Texture2D originalTexture, int width, int height)
{
    RenderTexture rt = new RenderTexture(width, height, 24);
    Graphics.Blit(originalTexture, rt);

    Texture2D resizedTexture = new Texture2D(width, height);
    RenderTexture.active = rt;
    resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    resizedTexture.Apply();

    RenderTexture.active = null;
    rt.Release();

    return resizedTexture;
}



private string GetStarSpritePath(string starType)
{
    switch (starType)
    {
        case "Red Dwarf":
            return "Stars/RedDwarf";

        case "Yellow Dwarf":
            return "Stars/YellowDwarf";

        case "Blue Giant":
            return "Stars/BlueGiant";

        case "White Dwarf":
            return "Stars/WhiteDwarf";

        case "Neutron Star":
            return "Stars/NeutronStar";

        case "Brown Dwarf":
            return "Stars/BrownDwarf";

        case "Supergiant":
            return "Stars/Supergiant";

        case "Pulsar":
            return "Stars/Pulsar";

        case "White Supergiant":
            return "Stars/WhiteSupergiant";

        case "Red Supergiant":
            return "Stars/RedSupergiant";

        case "Black Hole":
            return "Stars/BlackHole";

        case "Rainbow Star":
            return "Stars/RainbowStar";

        case "Void Star":
            return "Stars/VoidStar";

        case "Crystal Star":
            return "Stars/CrystalStar";

        case "Quantum Star":
            return "Stars/QuantumStar";

        case "Echo Star":
            return "Stars/EchoStar";

        case "Chrono Star":
            return "Stars/ChronoStar";

        case "Phantom Star":
            return "Stars/PhantomStar";

        case "Prism Star":
            return "Stars/PrismStar";

        case "Nebula Star":
            return "Stars/NebulaStar";

        case "Graviton Star":
            return "Stars/GravitonStar";

        case "Aurora Star":
            return "Stars/AuroraStar";

        case "Corrupted Star":
            return "Stars/CorruptedStar";

        default:
            return "Stars/YellowDwarf";
    }
}

private void SaveStarsAndPlanets()
{
    string savePath = Path.Combine(appDataLocation, activeGalaxy, "Galaxies", galaxyName);

    Directory.CreateDirectory(savePath);

    JsonSerializerSettings settings = new JsonSerializerSettings();
    settings.Converters.Add(new Vector3Converter());
    settings.Converters.Add(new GameObjectConverter());

    foreach (Star star in stars)
    {
        string starPath = Path.Combine(savePath, star.name);
        Directory.CreateDirectory(starPath);

        string starData = JsonConvert.SerializeObject(star, settings);
        File.WriteAllText(Path.Combine(starPath, "star.json"), starData);

    }
}

private void LoadStarsAndPlanets(string path)
{
    if (!Directory.Exists(path))
    {
        Debug.LogError($"Directory does not exist: {path}");
        return;
    }

    string[] starFolders = Directory.GetDirectories(path);
    foreach (string starFolder in starFolders)
    {
        string starFilePath = Path.Combine(starFolder, "star.json");
        if (File.Exists(starFilePath))
        {
            string starData = File.ReadAllText(starFilePath);

            Star star = JsonConvert.DeserializeObject<Star>(starData, new StarConverter());
            stars.Add(star);

        }
    }
}

private void GenerateLoadedStars(string path)
{
	            ClearStars();
		            ClearAllActiveSprites();
        GenerateNebulas();

    Debug.Log("Starting to load stars from path: " + path);

        LoadStarsAndPlanets(path);
        Debug.Log("Successfully loaded stars and planets from path: " + path);

    foreach (Star starData in stars)
    {
        try
        {

            GameObject starObject = new GameObject("Star");
            Star starComponent = starObject.AddComponent<Star>();

            starComponent.name = starData.name;
            starComponent.starType = starData.starType;
            starComponent.planetCount = starData.planetCount;
            starComponent.selectedPlanet = starData.selectedPlanet;
            starComponent.planetInfo = starData.planetInfo ?? new PlanetInfo[0]; 

            SpriteRenderer spriteRenderer = starObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = CreateStarSprite(starData); 

            if (spriteRenderer.sprite != null)
            {

            }
            else
            {
                Debug.LogWarning("Failed to create or assign sprite.");
            }
           // spriteRenderer.color = GetStarColor(starData.starType);

            starObject.transform.position = starData.transform.position;

        }
        catch (InvalidCastException castEx)
        {
            Debug.LogError("Invalid cast exception while generating star: " + starData.name + ". Exception: " + castEx.Message);
            Debug.LogError($"Star data type: {starData.GetType().FullName}");
            Debug.LogError($"Star position: {starData.transform.position}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to generate star: " + starData.name + ". Exception: " + ex.Message);
        }
    }

        starsGenerationComplete = true;
        isGeneratingStars = false;

    Debug.Log("Finished generating all stars.");
}

    private void SetRandomPlanetIfNeeded()
    {
        string currentPlanet = PlanetManager.instance.GetCurrentPlanet();
        if (string.IsNullOrEmpty(currentPlanet))
        {

            if (stars.Count > 0)
            {
                Star randomStar = stars[UnityEngine.Random.Range(0, stars.Count)];
                if (randomStar.planetInfo.Length > 0)
                {
                    PlanetInfo randomPlanet = randomStar.planetInfo[UnityEngine.Random.Range(0, randomStar.planetInfo.Length)];
                    PlanetManager.instance.SetCurrentPlanet(randomPlanet.name);
                }
            }
        }
    }

private void OnGUI()
{
    BottomBar();

    if (hoveredStar != null)
    {
        Vector2 mousePos = Event.current.mousePosition;

        string starInfo = $"{localizationManager.Localize("star_info_name")}: {hoveredStar.name}\n" +
                          $"{localizationManager.Localize("star_info_type")}: {hoveredStar.starType}\n" +
                          $"{localizationManager.Localize("star_info_planets")}: {hoveredStar.planetCount}";

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

    if (showDarkVoidWindow)
    {
        Rect darkVoidRect = new Rect((Screen.width - 500) / 2, (Screen.height - 420) / 2, 500, 420);
        GUI.Window(20, darkVoidRect, DarkVoidWindow, "");
    }

    if (showDarkVoidRelationshipWindow)
    {
        Rect relRect = new Rect((Screen.width - 420) / 2, (Screen.height - 300) / 2, 420, 300);
        GUI.Window(21, relRect, DarkVoidRelationshipWindow, "");
    }

    if (showJourneyTrackerWindow)
    {
        Rect journeyTrackerWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400);
        GUI.Window(12, journeyTrackerWindowRect, JourneyTrackerWindow, localizationManager.Localize("journey_tracker"));
    }

	if (selectedStar != null)
	{
		Rect starM3InfoWindowRect = new Rect(10, 10, 350, 450); 
		GUI.Window(0, starM3InfoWindowRect, StarM3InfoWindow, 
			new GUIContent(localizationManager.Localize("star_information"), "Details about the selected star"));
	}
	
    if (showPlanetWindow && selectedStar != null)
    {
        Rect planetM3InfoWindowRect = new Rect(360, 10, 300, 400);
        GUI.Window(1, planetM3InfoWindowRect, PlanetM3InfoWindow, localizationManager.Localize("planet_information"));
    }

    if (showParametersWindow)
    {
        Rect parametersWindowRect = new Rect(450, 100, 300, 200);
        GUILayout.Window(1, parametersWindowRect, PlanetParametersWindow, localizationManager.Localize("parameters"));
    }
    
    if (showTutorialPrompt)
    {
        GUI.Window(2, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), TutorialPromptWindow, localizationManager.Localize("tutorial"));
    }

    if (showTutorialStep1)
    {
        GUI.Window(3, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), TutorialStep1Window, localizationManager.Localize("tutorial_step1"));
    }

    if (showTutorialStep2)
    {
        GUI.Window(4, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), TutorialStep2Window, localizationManager.Localize("tutorial_step2"));
    }

    Rect smallWindowRect = new Rect(Screen.width - 160, 10, 150, 150);
    GUI.Window(5, smallWindowRect, SmallWindow, localizationManager.Localize("options"));


if (showGalaxySelectionWindow)
{
    Rect galaxyWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 300) / 2, 400, 400);
    Rect descriptionWindowRect = new Rect(galaxyWindowRect.x + galaxyWindowRect.width + 20, galaxyWindowRect.y, 300, 400);
    Rect infoWindowRect = new Rect(galaxyWindowRect.x - 320, galaxyWindowRect.y, 300, 400);

    GUI.Window(6, galaxyWindowRect, GalaxySelectionWindow, new GUIContent(localizationManager.Localize("select_galaxy"), "Browse galaxies"));
    GUI.Window(7, descriptionWindowRect, GalaxyDescriptionWindow, new GUIContent(localizationManager.Localize("galaxy_description"), "Galaxy details"));
    GUI.Window(8, infoWindowRect, GalaxyM3InfoWindow, new GUIContent(localizationManager.Localize("galaxy_info"), "Galaxy stats"));

    Rect closeButtonRect = new Rect(galaxyWindowRect.x + (galaxyWindowRect.width / 2) - 50, galaxyWindowRect.y + galaxyWindowRect.height + 15, 100, 35);

    Rect customButtonRect = new Rect(closeButtonRect.x + closeButtonRect.width + 10, closeButtonRect.y, 150, 35); 

    if (GUI.Button(closeButtonRect, localizationManager.Localize("close")))
    {
        showGalaxySelectionWindow = false;
    }

    string buttonText = isCustomGalaxiesMode ? "Normal Galaxies" : "Custom Galaxies";
    if (GUI.Button(customButtonRect, buttonText))
    {
        isCustomGalaxiesMode = !isCustomGalaxiesMode; 
    }
}



    if (showFavoritesWindow)
    {
        Rect favoritesWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400);
        GUI.Window(8, favoritesWindowRect, FavoritesWindow, localizationManager.Localize("favorite_stars"));
    }

	
	if (isGeneratingStars)
	{
		GUI.Window(
			2, 
			new Rect(Screen.width / 2 - 350, Screen.height / 2 - 250, 700, 500), 
			StarLoadingWindow, 
			localizationManager.Localize("loading")
		);
	}

    if (showSearchWindow)
    {
        Rect searchWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 500) / 2, 400, 500);
        GUI.Window(9, searchWindowRect, SearchWindow, localizationManager.Localize("search_stars"));
    }

    if (showFilterWindow)
    {
        Rect filterWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400);
        GUI.Window(10, filterWindowRect, FilterWindow, localizationManager.Localize("filter_options"));
    }

    if (showCompendiumWindow)
    {
        Rect compendiumWindowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400);
        GUI.Window(11, compendiumWindowRect, CompendiumWindow, localizationManager.Localize("compendium"));
    }


GUIStyle galaxyInfoStyle = new GUIStyle(GUI.skin.label);
galaxyInfoStyle.fontSize = 16; 
galaxyInfoStyle.fontStyle = FontStyle.Italic;
galaxyInfoStyle.normal.textColor = new Color(0.8f, 0.8f, 0.9f); 
galaxyInfoStyle.alignment = TextAnchor.MiddleLeft;

GUIStyle galaxyNameStyle = new GUIStyle(GUI.skin.label);
galaxyNameStyle.fontSize = 30; 
galaxyNameStyle.fontStyle = FontStyle.Bold;
galaxyNameStyle.normal.textColor = new Color(1f, 0.84f, 0f); 
galaxyNameStyle.alignment = TextAnchor.MiddleLeft;


float blockWidth = 600f;
float blockHeight = 50f;
float blockX = 10f; 
float blockY = Screen.height - 10f; 


float textSpacing = 20f; 
float infoTextHeight = 20f; 
float galaxyNameHeight = 30f; 


GUI.Label(new Rect(blockX, blockY - galaxyNameHeight - textSpacing, blockWidth, infoTextHeight), "You are currently in", galaxyInfoStyle);
GUI.Label(new Rect(blockX, blockY - galaxyNameHeight, blockWidth, galaxyNameHeight), galaxyName, galaxyNameStyle);

}


private void SearchWindow(int windowID)
{
    GUILayout.BeginVertical();

    if (GUILayout.Button(localizationManager.Localize("filter")))
    {
        showFilterWindow = true;
    }

    GUILayout.Label(localizationManager.Localize("search_stars"));
    searchQuery = GUILayout.TextField(searchQuery);

    applyFilters = GUILayout.Toggle(applyFilters, localizationManager.Localize("apply_filters_label"));

    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(380), GUILayout.Height(300));

    bool selectionMade = false;

    foreach (Star star in stars)
    {
        bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || star.name.StartsWith(searchQuery, StringComparison.OrdinalIgnoreCase);
        bool matchesFilters = !applyFilters || 
            (star.starType == selectedStarType || selectedStarType == "Any") &&
            star.planetCount >= minPlanets && star.planetCount <= maxPlanets;

        if (matchesSearchQuery && matchesFilters)
        {
            if (GUILayout.Button(star.name))
            {
                selectedStar = star;
                MoveCameraToStar(star);
                showSearchWindow = false; 
                selectionMade = true;
                break; 
            }
        }
    }

    if (!selectionMade && !string.IsNullOrEmpty(searchQuery))
    {
        GUILayout.Label(localizationManager.Localize("no_results_found"));
    }

    GUILayout.EndScrollView();

    if (GUILayout.Button(localizationManager.Localize("close")))
    {
        showSearchWindow = false;
    }

    GUILayout.EndVertical();
}

private void FilterWindow(int windowID)
{
    GUILayout.BeginVertical("box");

    GUILayout.Label("Filter Options:");

    string[] starTypes = { 
        "Any", 
        "Red Dwarf", 
        "Yellow Dwarf", 
        "Blue Giant", 
        "White Dwarf", 
        "Neutron Star", 
        "Brown Dwarf", 
        "Supergiant", 
        "Pulsar", 
        "White Supergiant", 
        "Red Supergiant", 
        "Black Hole" 
    };
    int selectedIndex = Array.IndexOf(starTypes, selectedStarType);
    selectedIndex = GUILayout.SelectionGrid(selectedIndex, starTypes, starTypes.Length / 2);

    selectedStarType = starTypes[selectedIndex];

    GUILayout.Label("Minimum Number of Planets:");
    int.TryParse(GUILayout.TextField(minPlanets.ToString()), out minPlanets);

    GUILayout.Label("Maximum Number of Planets:");
    int.TryParse(GUILayout.TextField(maxPlanets.ToString()), out maxPlanets);

    if (GUILayout.Button(localizationManager.Localize("apply")))
    {
        showFilterWindow = false;
    }
    if (GUILayout.Button(localizationManager.Localize("cancel")))
    {
        showFilterWindow = false;
    }

    GUILayout.EndVertical();
}

private void StarLoadingWindow(int windowID)
{
    if (localizationManager == null)
    {
        Debug.LogError("LocalizationManager is not initialized!");
        GUILayout.Label("LocalizationManager is not available.");
        return;
    }

    if (randomFacts == null || randomFacts.Count == 0)
    {
        Debug.LogWarning(randomFacts == null ? "randomFacts list is not initialized!" : "randomFacts list is empty!");
        GUILayout.Label("No random facts available.");
        return;
    }

    string loadingMessage = localizationManager.Localize("stars_loading_message");
    string randomFact = randomFacts[UnityEngine.Random.Range(0, randomFacts.Count)];
    float t = Time.realtimeSinceStartup;

    GUIStyle sysBadgeStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 10,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = false,
        normal = { textColor = new Color(0.1f, 1f, 0.67f) }
    };

    GUIStyle mainTitleStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 48,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true,

        normal = { textColor = new Color(0.1f, 1f, 0.67f, 0.9f + Mathf.Sin(t * 3.1f) * 0.08f) }
    };

    GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 13,
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = false,
        normal = { textColor = new Color(1f, 0.63f, 0.24f) }
    };

    GUIStyle loadingMsgStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 16,
        fontStyle = FontStyle.Italic,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true,
        normal = { textColor = new Color(0.78f, 0.84f, 1f, 0.75f) }
    };

    GUIStyle dykLabelStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 9,
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = false,
        normal = { textColor = new Color(0.1f, 1f, 0.67f, 0.5f) }
    };

    GUIStyle factStyle = new GUIStyle(GUI.skin.box)
    {
        fontSize = 14,
        fontStyle = FontStyle.Italic,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true,
        normal = { textColor = new Color(0.82f, 0.69f, 1f) }
    };

    GUIStyle engineNameStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 11,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleLeft,
        wordWrap = false,
        normal = { textColor = new Color(0.59f, 0.75f, 1f, 0.7f) }
    };

    GUIStyle galaxyCountStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 26,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleLeft,
        wordWrap = false,
        normal = { textColor = new Color(1f, 0.63f, 0.24f) }
    };

    GUIStyle galaxySubStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 12,
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        wordWrap = false,
        normal = { textColor = new Color(1f, 0.63f, 0.24f, 0.55f) }
    };

    GUIStyle pctStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 10,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.UpperRight,
        wordWrap = false,
        normal = { textColor = new Color(0.1f, 1f, 0.67f, 0.5f) }
    };

    Texture2D grad = CreateGradientTexture(700, 520, new Color(0.05f, 0.07f, 0.24f), new Color(0.01f, 0.03f, 0.1f));

    GUI.skin.window.normal.background = grad;
    GUI.skin.window.active.background = grad;
    GUI.skin.window.focused.background = grad;

    GUI.skin.window.onNormal.background = grad;
    GUI.skin.window.onActive.background = grad;
    GUI.skin.window.onFocused.background = grad;

    GUILayout.BeginVertical();
    GUILayout.Space(16);

    GUILayout.Label("Starmap", sysBadgeStyle);
    GUILayout.Space(8);

    GUILayout.Label("Loading stars...", mainTitleStyle);
    GUILayout.Space(6);

    GUILayout.Label("You are gay", subtitleStyle);
    GUILayout.Space(20);

    DrawHorizontalRule(new Color(0.47f, 0.63f, 1f, 0.18f));
    GUILayout.Space(16);

    GUILayout.Label(loadingMessage, loadingMsgStyle);
    GUILayout.Space(16);

    GUILayout.Label("Did you know?", dykLabelStyle);
    GUILayout.Space(6);
    GUILayout.Box($"{randomFact}", factStyle, GUILayout.MinHeight(80));

    GUILayout.Space(20);
    DrawHorizontalRule(new Color(0.47f, 0.63f, 1f, 0.15f));
    GUILayout.Space(16);

    GUILayout.BeginHorizontal();

    GUILayout.BeginVertical(GUILayout.Width(200));
    GUILayout.Label("Galaxies", engineNameStyle);
    GUILayout.Label($"{loadedGalaxyCount}", galaxyCountStyle);
    GUILayout.Label("custom galaxies created", galaxySubStyle);
    GUILayout.EndVertical();

    GUILayout.FlexibleSpace();

    GUILayout.BeginVertical(GUILayout.Width(280));
    GUILayout.Space(18);
    float progress = Mathf.Clamp01((Mathf.Sin(t * 0.4f) * 0.5f + 0.5f) * 0.96f); 

    DrawProgressBar(progress, new Color(0.1f, 1f, 0.67f), new Color(0.3f, 0.78f, 1f));
    GUILayout.Label($"{Mathf.RoundToInt(progress * 100)}%", pctStyle);
    GUILayout.EndVertical();

    GUILayout.EndHorizontal();

    GUILayout.Space(16);
    GUILayout.EndVertical();
}

private void DrawHorizontalRule(Color color)
{
    Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1), GUILayout.ExpandWidth(true));
    Texture2D line = new Texture2D(1, 1);
    line.SetPixel(0, 0, color);
    line.Apply();
    GUI.DrawTexture(r, line);
}

private void DrawProgressBar(float t, Color colorA, Color colorB)
{
    Rect track = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(4), GUILayout.ExpandWidth(true));

    Texture2D trackTex = new Texture2D(1, 1);
    trackTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.06f));
    trackTex.Apply();
    GUI.DrawTexture(track, trackTex);

    Rect fill = new Rect(track.x, track.y, track.width * t, track.height);
    Texture2D fillTex = new Texture2D(1, 1);
    fillTex.SetPixel(0, 0, Color.Lerp(colorA, colorB, t));
    fillTex.Apply();
    GUI.DrawTexture(fill, fillTex);
}

private Texture2D CreateGradientTexture(int width, int height, Color startColor, Color endColor)
{
    Texture2D texture = new Texture2D(width, height);
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Color lerpedColor = Color.Lerp(startColor, endColor, (float)y / height);
            texture.SetPixel(x, y, lerpedColor);
        }
    }
    texture.Apply();
    return texture;
}




    private void TutorialPromptWindow(int windowID)
    {
        GUILayout.Label(localizationManager.Localize("tutorial"));

        if (GUILayout.Button(localizationManager.Localize("yes")))
        {
            showTutorialPrompt = false;
            showTutorialStep1 = true;
        }

        if (GUILayout.Button(localizationManager.Localize("no")))
        {
            showTutorialPrompt = false;
            tutorialShown = true;
        }
    }

    private void TutorialStep1Window(int windowID)
    {
        GUILayout.Label(localizationManager.Localize("tutorial1"));

        if (GUILayout.Button(localizationManager.Localize("next")))
        {
            showTutorialStep1 = false;
            showTutorialStep2 = true;
        }

        if (GUILayout.Button(localizationManager.Localize("skip")))
        {
            showTutorialStep1 = false;
            tutorialShown = true;
        }
    }

    private void TutorialStep2Window(int windowID)
    {
        GUILayout.Label(localizationManager.Localize("tutorial2"));

        if (GUILayout.Button(localizationManager.Localize("finish")))
        {
            showTutorialStep2 = false;
            tutorialShown = true;
        }

        if (GUILayout.Button(localizationManager.Localize("skip")))
        {
            showTutorialStep2 = false;
            tutorialShown = true;
        }
    }

	private void SmallWindow(int windowID)
	{
		
		GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 20,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter,
			wordWrap = true,
			normal = { textColor = new Color(0.4f, 0.9f, 1.0f) } 
		};

		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
		{
			fontSize = 14,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter,
			normal = { background = MakeTex(2, 2, new Color(0f, 0.2f, 0.2f, 0.7f)) }, 
			hover = { background = MakeTex(2, 2, new Color(0.1f, 0.5f, 0.5f, 1f)) } 
		};

		


		
		if (GUILayout.Button(localizationManager.Localize("galaxy_select"), buttonStyle))
		{
			showGalaxySelectionWindow = true;
		}

		
		if (GUILayout.Button(localizationManager.Localize("favorite_systems"), buttonStyle))
		{
			showFavoritesWindow = true;
		}

		
		if (GUILayout.Button(localizationManager.Localize("search_stars"), buttonStyle))
		{
			showSearchWindow = true;
		}

		
		if (GUILayout.Button(localizationManager.Localize("exit"), buttonStyle))
		{
			Debug.Log("Exiting Starmap");
			SpaceManager.DisableSpace(); 
		}
	}

    private void FavoritesWindow(int windowID)
    {
        List<string> favoriteStarNames = LoadFavorites();
        List<Star> allStars = GetAllStarGameObjects();

        Debug.Log("Loading favorites window.");

        foreach (string favoriteName in favoriteStarNames)
        {
            Star star = allStars.Find(s => s.name.Equals(favoriteName, StringComparison.OrdinalIgnoreCase));

            if (star != null)
            {
                if (GUILayout.Button(star.name))
                {
                    Debug.Log($"Favorite star selected: {star.name}");
                    selectedStar = star;
                    showPlanetWindow = false;
                    showFavoritesWindow = false;
                    MoveCameraToStar(star);
                    break;
                }
            }
            else
            {
                Debug.Log($"Star not found: {favoriteName}");
            }
        }

        if (GUILayout.Button(localizationManager.Localize("close")))
        {
            Debug.Log("Closing favorites window.");
            showFavoritesWindow = false;
        }
    }

    private void MoveCameraToStar(Star star)
    {
        Vector3 starPosition = star.transform.position;
        mainCamera.transform.position = new Vector3(starPosition.x, starPosition.y, mainCamera.transform.position.z);
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize * 0.5f, minZoom, maxZoom); 

        Debug.Log($"Moved camera to star: {star.name}, Position: {starPosition}");
    }

    private void SaveFavorites()
    {
        File.WriteAllLines(favoritesFilePath, favoritesList);
        Debug.Log($"Saved favorites: {string.Join(", ", favoritesList)}");
    }

    public void AddFavorite(string starName)
    {
        if (!favoritesList.Contains(starName))
        {
            favoritesList.Add(starName);
            SaveFavorites();
            Debug.Log($"Added to favorites: {starName}");
        }
        else
        {
            Debug.Log($"{starName} is already in favorites.");
        }
    }

    public void RemoveFavorite(string starName)
    {
        if (favoritesList.Contains(starName))
        {
            favoritesList.Remove(starName);
            SaveFavorites();
            Debug.Log($"Removed from favorites: {starName}");
        }
        else
        {
            Debug.Log($"{starName} is not in favorites.");
        }
    }

private List<Star> GetAllStarGameObjects()
{
    List<Star> stars = GameObject.FindObjectsOfType<Star>().ToList();
  //  Debug.Log($"Found {stars.Count} stars in the scene.");
    return stars;
}

private Star FindStarByName(string starName)
{

    List<Star> stars = GetAllStarGameObjects();

    Star foundStar = stars.FirstOrDefault(star => star.name == starName);

    if (foundStar == null)
    {
        Debug.LogWarning($"Star with name {starName} not found.");
    }

    return foundStar;
}

private List<string> LoadFavorites()
{
    List<string> favorites = new List<string>();
    string path = Path.Combine(Application.persistentDataPath, "favorites.txt");

    if (File.Exists(path))
    {
        string[] lines = File.ReadAllLines(path);
        favorites.AddRange(lines.Select(line => line.Trim()));
        Debug.Log($"Loaded favorites from file: {string.Join(", ", favorites)}");
    }
    else
    {
        Debug.Log("Favorites file does not exist.");
    }

    return favorites;
}

    private void CompendiumWindow(int windowID)
    {

        Rect sidebarRect = new Rect(0, 0, 150, 500);
        Rect contentRect = new Rect(150, 0, 450, 500);

        GUILayout.BeginArea(sidebarRect, GUI.skin.box);
        sidebarScrollPosition = GUILayout.BeginScrollView(sidebarScrollPosition);

        foreach (CompendiumEntry entry in compendiumEntries)
        {
            if (GUILayout.Button(entry.Title))
            {
                currentTitle = entry.Title;
                currentContent = entry.Content;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(contentRect, GUI.skin.box);
        GUILayout.Label(currentTitle, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 16 });
        contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition);
        GUILayout.Label(currentContent);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

private int planetsVisited = 0; 

private void InitBarTextures()
{
    if (_barBg != null) return;
    _barBg            = MakeTex(1, 1, new Color(0.016f, 0.039f, 0.094f, 0.96f));
    _btnTex           = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f,    0.06f));
    _btnHoverTex      = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f,    0.14f));
    _btnStarTex       = MakeTex(1, 1, new Color(1f,    0.78f, 0.31f, 0.05f));
    _btnStarHoverTex  = MakeTex(1, 1, new Color(1f,    0.78f, 0.31f, 0.14f));
    _btnSecretTex     = MakeTex(1, 1, new Color(0.63f, 0.24f, 1f,    0.07f));
    _btnSecretHoverTex= MakeTex(1, 1, new Color(0.63f, 0.24f, 1f,    0.17f));
    _badgeTex         = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f,    0.04f));
    _dividerTex       = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f,    0.14f));
}

private void DarkVoidWindow(int windowID)
{
    GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
    headerStyle.fontSize = 22;
    headerStyle.fontStyle = FontStyle.Bold;
    headerStyle.alignment = TextAnchor.MiddleCenter;
    headerStyle.normal.textColor = new Color(0.55f, 0.2f, 1f);

    GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
    closeStyle.fontSize = 11;
    closeStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);

    GUIStyle loreBoxStyle = new GUIStyle(GUI.skin.box);
    loreBoxStyle.fontSize = 12;
    loreBoxStyle.wordWrap = true;
    loreBoxStyle.alignment = TextAnchor.UpperLeft;
    loreBoxStyle.normal.textColor = new Color(0.85f, 0.82f, 0.95f);
    loreBoxStyle.padding = new RectOffset(12, 12, 10, 10);

    GUIStyle actionBtnStyle = new GUIStyle(GUI.skin.button);
    actionBtnStyle.fontSize = 11;
    actionBtnStyle.fontStyle = FontStyle.Bold;
    actionBtnStyle.normal.textColor = new Color(0.7f, 0.5f, 1f);
    actionBtnStyle.hover.textColor = Color.white;

    GUI.Label(new Rect(0, 10, 500, 35), "✦  THE DARK VOID  ✦", headerStyle);

    GUI.Box(new Rect(40, 48, 420, 2), "");

    string loreText =
        "The Dark Void is a galactic empire that controls Dank, they consist of multiple " +
        "different races that evolved separate from each other but have a common goal, to defeat you. " +
        "Why are they doing this? Well let's find that out.";
    GUI.Box(new Rect(25, 58, 450, 110), loreText, loreBoxStyle);

    float btnY = 185;
    float btnW = 105;
    float btnH = 34;
    float btnSpacing = 8;
    float totalW = (btnW * 4) + (btnSpacing * 3);
    float startX = (500 - totalW) / 2;

    string[] labels = { "RELATIONSHIP", "PAST INVASIONS", "ORGANISMS", "KNOWLEDGE" };
    for (int i = 0; i < labels.Length; i++)
    {
        Rect btnRect = new Rect(startX + i * (btnW + btnSpacing), btnY, btnW, btnH);
        if (GUI.Button(btnRect, labels[i], actionBtnStyle))
        {
            if (labels[i] == "RELATIONSHIP")
            {
                showDarkVoidWindow = false;
                showDarkVoidRelationshipWindow = true;
            }
        }
    }

    GUI.Box(new Rect(40, 232, 420, 2), "");

    GUIStyle flavorStyle = new GUIStyle(GUI.skin.label);
    flavorStyle.fontSize = 10;
    flavorStyle.fontStyle = FontStyle.Italic;
    flavorStyle.alignment = TextAnchor.MiddleCenter;
    flavorStyle.normal.textColor = new Color(0.45f, 0.35f, 0.6f);
    GUI.Label(new Rect(0, 238, 500, 20), "\"They do not negotiate. They do not forgive.\"", flavorStyle);

    if (GUI.Button(new Rect(450, 8, 40, 22), "✕", closeStyle))
        showDarkVoidWindow = false;
}

private void DarkVoidRelationshipWindow(int windowID)
{
    GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
    headerStyle.fontSize = 20;
    headerStyle.fontStyle = FontStyle.Bold;
    headerStyle.alignment = TextAnchor.MiddleCenter;
    headerStyle.normal.textColor = new Color(0.55f, 0.2f, 1f);

    GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
    closeStyle.fontSize = 11;
    closeStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);

    GUIStyle endLabelStyle = new GUIStyle(GUI.skin.label);
    endLabelStyle.fontSize = 10;
    endLabelStyle.fontStyle = FontStyle.Bold;
    endLabelStyle.alignment = TextAnchor.UpperCenter;

    GUIStyle statusHeaderStyle = new GUIStyle(GUI.skin.label);
    statusHeaderStyle.fontSize = 16;
    statusHeaderStyle.fontStyle = FontStyle.Bold;
    statusHeaderStyle.alignment = TextAnchor.MiddleCenter;
    statusHeaderStyle.normal.textColor = new Color(0.85f, 0.82f, 0.95f);

    GUIStyle descStyle = new GUIStyle(GUI.skin.label);
    descStyle.fontSize = 11;
    descStyle.fontStyle = FontStyle.Italic;
    descStyle.alignment = TextAnchor.MiddleCenter;
    descStyle.normal.textColor = new Color(0.5f, 0.48f, 0.6f);

    GUI.Label(new Rect(0, 10, 420, 32), "✦  THE DARK VOID  ✦", headerStyle);
    GUI.Box(new Rect(30, 45, 360, 2), "");

    float barX = 50;
    float barY = 65;
    float barW = 320;
    float barH = 18;
    float fillFraction = 0.5f; 

    GUI.Box(new Rect(barX, barY, barW, barH), "");

    GUIStyle fillStyle = new GUIStyle(GUI.skin.box);
    fillStyle.normal.background = Texture2D.whiteTexture;
    Color prevColor = GUI.color;
    GUI.color = Color.Lerp(new Color(0.8f, 0.15f, 0.15f), new Color(0.25f, 0.7f, 0.4f), fillFraction);
    GUI.Box(new Rect(barX, barY, barW * fillFraction, barH), "", fillStyle);
    GUI.color = prevColor;

    endLabelStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
    GUI.Label(new Rect(barX, barY + barH + 4, 60, 18), "HATRED", endLabelStyle);
    endLabelStyle.normal.textColor = new Color(0.3f, 0.85f, 0.5f);
    GUI.Label(new Rect(barX + barW - 60, barY + barH + 4, 60, 18), "LOVE", endLabelStyle);

    GUI.Label(new Rect(0, barY + barH + 30, 420, 28), "Neutral", statusHeaderStyle);
    GUI.Label(new Rect(0, barY + barH + 56, 420, 22), "I don't have a description yet", descStyle);

    GUIStyle backStyle = new GUIStyle(GUI.skin.button);
    backStyle.fontSize = 11;
    backStyle.fontStyle = FontStyle.Bold;
    backStyle.normal.textColor = new Color(0.7f, 0.5f, 1f);
    if (GUI.Button(new Rect(160, 240, 100, 30), "◀  BACK", backStyle))
    {
        showDarkVoidRelationshipWindow = false;
        showDarkVoidWindow = true;
    }

    if (GUI.Button(new Rect(370, 8, 40, 22), "✕", closeStyle))
        showDarkVoidRelationshipWindow = false;
}

private void BottomBar()
{
    InitBarTextures();

    string planetCountFilePath = Path.Combine(Application.persistentDataPath, "ModernBox", "PlanetCount.txt");
    if (File.Exists(planetCountFilePath))
    {
        string raw = File.ReadAllText(planetCountFilePath);
        if (!int.TryParse(raw, out planetsVisited))
        {
            Debug.LogError("Failed to parse planetsVisited from PlanetCount.txt");
            planetsVisited = 0;
        }
    }
    else
    {
        Debug.LogWarning("PlanetCount.txt not found, initializing to 0.");
        planetsVisited = 0;
        File.WriteAllText(planetCountFilePath, "0");
    }

    float t     = Time.realtimeSinceStartup;
    float barH  = 72f;
    float barY  = Screen.height - barH;

    GUI.DrawTexture(new Rect(0, barY, Screen.width, barH), _barBg);

    float lineAlpha = 0.3f + Mathf.Sin(t * 1.6f) * 0.2f;
    var lineTex = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f, lineAlpha));
    GUI.DrawTexture(new Rect(0, barY, Screen.width, 1f), lineTex);

    GUIStyle MakeBtn(Texture2D normal, Texture2D hover, Color textColor, Color hoverColor) =>
        new GUIStyle(GUI.skin.button)
        {
            fontSize    = 9,
            fontStyle   = FontStyle.Bold,
            alignment   = TextAnchor.MiddleCenter,
            normal      = { background = normal, textColor = textColor },
            hover       = { background = hover,  textColor = hoverColor },
            active      = { background = hover,  textColor = Color.white },
        };

    var btnStyle    = MakeBtn(_btnTex,       _btnHoverTex,       new Color(0.49f, 0.85f, 0.97f), new Color(0f, 0.9f, 1f));
    var starStyle   = MakeBtn(_btnStarTex,   _btnStarHoverTex,   new Color(1f, 0.82f, 0.39f),    new Color(1f, 0.84f, 0f));
    var secretStyle = MakeBtn(_btnSecretTex, _btnSecretHoverTex, new Color(0.78f, 0.51f, 1f),    new Color(0.88f, 0.5f, 1f));

    GUIStyle brandStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize   = 18,
        fontStyle  = FontStyle.Bold,
        alignment  = TextAnchor.MiddleCenter,

        normal     = { textColor = new Color(1f, 0.84f, 0f, 0.88f + Mathf.Sin(t * 2.3f) * 0.1f) }
    };

    GUIStyle subStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize   = 10,
        fontStyle  = FontStyle.Italic,
        alignment  = TextAnchor.MiddleCenter,
        normal     = { textColor = new Color(0.86f, 0.92f, 1f, 0.4f) }
    };

    GUIStyle countStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize   = 17,
        fontStyle  = FontStyle.Bold,
        alignment  = TextAnchor.MiddleCenter,
        normal     = { textColor = new Color(0.1f, 1f, 0.67f) }
    };

    GUIStyle countLabelStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize   = 8,
        fontStyle  = FontStyle.Bold,
        alignment  = TextAnchor.MiddleCenter,
        normal     = { textColor = new Color(0.1f, 1f, 0.67f, 0.45f) }
    };

    float btnW   = 120f;
    float btnH   = 42f;
    float btnY   = barY + (barH - btnH) / 2f;
    float spacing = 10f;
    float marginR = 18f;

    float compendiumX      = Screen.width - marginR - btnW;
    float starX            = compendiumX - spacing - btnW;
    float journeyX         = starX - spacing - btnW;
    float secretX          = journeyX - spacing - btnW;

    if (GUI.Button(new Rect(compendiumX, btnY, btnW, btnH), localizationManager.Localize("compendium").ToUpper(), btnStyle))
        showCompendiumWindow = !showCompendiumWindow;

    if (GUI.Button(new Rect(starX, btnY, btnW, btnH), localizationManager.Localize("current_star").ToUpper(), starStyle))
    {
        string name = PlanetManager.instance.FindParentStar();
        MoveCameraToStar(FindStarByName(name));
    }

    if (GUI.Button(new Rect(journeyX, btnY, btnW, btnH), localizationManager.Localize("journey_tracker").ToUpper(), btnStyle))
        showJourneyTrackerWindow = !showJourneyTrackerWindow;

        float pulse = 0.35f + Mathf.Abs(Mathf.Sin(t * 2.5f)) * 0.35f;
        secretStyle.normal.background = MakeTex(1, 1, new Color(0.63f, 0.24f, 1f, pulse * 0.12f));

    /*    if (GUI.Button(new Rect(secretX, btnY, btnW, btnH), localizationManager.Localize("arg_redacted").ToUpper(), secretStyle))
        {
            showDarkVoidWindow = true;
        }    
    */
    float badgeW = 80f;
    float badgeH = 42f;
    float badgeX = 258f;
    float badgeY = barY + (barH - badgeH) / 2f;

    GUI.DrawTexture(new Rect(badgeX, badgeY, badgeW, badgeH), _badgeTex);
    GUI.Label(new Rect(badgeX, badgeY + 4f,  badgeW, 20f), $"{planetsVisited}", countStyle);
    GUI.Label(new Rect(badgeX, badgeY + 24f, badgeW, 14f), "PLANETS",          countLabelStyle);

    GUI.DrawTexture(new Rect(badgeX + badgeW + 10f, barY + 18f, 1f, 36f), _dividerTex);

    GUI.DrawTexture(new Rect(secretX > 0 ? secretX - 14f : journeyX - 14f, barY + 18f, 1f, 36f), _dividerTex);

    float textW  = 320f;
    float centerX = Screen.width / 2f - textW / 2f;
    GUI.Label(new Rect(centerX, barY + 10f, textW, 26f), "ModernBox 5.01", brandStyle);
    GUI.Label(new Rect(centerX, barY + 36f, textW, 18f), "BY TUXXEGO",     subStyle);

    DrawBarCorner(0,          barY,    true,  true);
    DrawBarCorner(Screen.width, barY,  true,  false);
}

private void DrawBarCorner(float x, float y, bool top, bool left)
{
    float sz = 12f;
    var tex  = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f, 0.38f));
    float lx = left ? x : x - sz;
    GUI.DrawTexture(new Rect(lx,        y,        sz, 1.5f), tex);
    GUI.DrawTexture(new Rect(left?x:x-1.5f, y,   1.5f, sz), tex);
}

private Texture2D MakeTex(int width, int height, Color col)
{
    Color[] pix = new Color[width * height];
    for (int i = 0; i < pix.Length; i++)
    {
        pix[i] = col;
    }
    Texture2D result = new Texture2D(width, height);
    result.SetPixels(pix);
    result.Apply();
    return result;
}


private Texture2D MakeGradientTexture(int width, int height, Color topColor, Color bottomColor)
{
    Texture2D texture = new Texture2D(width, height);
    for (int y = 0; y < height; y++)
    {
        Color color = Color.Lerp(topColor, bottomColor, (float)y / height);
        for (int x = 0; x < width; x++)
        {
            texture.SetPixel(x, y, color);
        }
    }

    texture.Apply();
    return texture;
}

private string hoveredGalaxy = null; 

private void GalaxySelectionWindow(int windowID)
{
    string planetCountFilePath = Path.Combine(Application.persistentDataPath, "ModernBox", "PlanetCount.txt");

    if (File.Exists(planetCountFilePath))
    {
        string fileContents = File.ReadAllText(planetCountFilePath);
        if (int.TryParse(fileContents, out int visitedCount))
        {
            planetsVisited = visitedCount;
        }
        else
        {
            Debug.LogError("Failed to parse planetsVisited from PlanetCount.txt");
            planetsVisited = 0;
        }
    }
    else
    {
        Debug.LogWarning("PlanetCount.txt not found, initializing planetsVisited to 0.");
        planetsVisited = 0;
    }

    GUIStyle galaxyButtonStyle = new GUIStyle(GUI.skin.button)
    {
        fontSize = 15,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        fixedHeight = 30,
        wordWrap = true,
        normal = { textColor = new Color(0.5f, 1f, 1f) },  
        hover = { textColor = new Color(1f, 0.5f, 0.8f) }  
    };

    GUIStyle unavailableLabelStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 18,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true,
        normal = { textColor = new Color(1f, 0.3f, 0.3f) }  
    };

    GUILayout.Label("Explore the Galaxies", new GUIStyle(GUI.skin.label)
    {
        fontSize = 30,
        fontStyle = FontStyle.BoldAndItalic,
        alignment = TextAnchor.UpperCenter,
        normal = { textColor = new Color(1f, 0.9f, 0.3f) }  
    });

    GUILayout.Space(20); 


    HashSet<string> excludedGalaxies = new HashSet<string>
    {
        "Crabby Way", "Tuxxus", "Krummple", "BlueNight", "Centuga", "Glass", "Dank"
    };

    GUILayout.Space(20); 

    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(350), GUILayout.Height(400));

    foreach (string galaxy in predefinedGalaxies)
    {
        if (isCustomGalaxiesMode && excludedGalaxies.Contains(galaxy))
        {
            continue;
        }

        int requiredPlanets = galaxyRequirements.ContainsKey(galaxy) ? galaxyRequirements[galaxy] : 0;

        if (planetsVisited >= requiredPlanets)
        {
            if (GUILayout.Button(galaxy, galaxyButtonStyle))
            {
                isGeneratingStars = true;
                ClearAllActiveSprites();
                Debug.Log($"Loading galaxy: {galaxy}");

                string galaxyPath = Path.Combine(appDataLocation, galaxy, "Galaxies", galaxy);

                if (Directory.Exists(galaxyPath))
                {
                    showGalaxySelectionWindow = false;
                    ClearStars();
                    SaveActiveGalaxy(galaxy);
                    activeGalaxy = galaxy;
                    GenerateLoadedStars(galaxyPath);
                }
                else
                {
                    ClearStars();
                    SaveActiveGalaxy(galaxy);
                    activeGalaxy = galaxy;
                    GenerateStars();
                }
                isGeneratingStars = false;
            }
        }
        else
        {
            string labelText = $"{galaxy} (Requires {requiredPlanets} planets, you have {planetsVisited})";
            GUILayout.Label(labelText, unavailableLabelStyle);
        }

        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
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
    GUIStyle descriptionStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 20,
        fontStyle = FontStyle.Italic,
        wordWrap = true,
        alignment = TextAnchor.MiddleCenter,
        normal = { textColor = new Color(0.8f, 0.9f, 1f) }  
    };

    GUILayout.Label(galaxyDescription, descriptionStyle);
}


private void GalaxyM3InfoWindow(int windowID)
{
    GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 18,
        fontStyle = FontStyle.Normal,
        wordWrap = true,
        alignment = TextAnchor.MiddleLeft,
        normal = { textColor = new Color(0.5f, 1f, 0.5f) }  
    };

    if (hoveredGalaxy != null)
    {
        if (hoveredGalaxy == "Dank")
        {
            GUILayout.Label(localizationManager.Localize("stars_unknown"), infoStyle);
            GUILayout.Label(localizationManager.Localize("danger_unknown"), infoStyle);
        }
        else if (galaxyStarCounts.ContainsKey(hoveredGalaxy) && galaxyDangerRatings.ContainsKey(hoveredGalaxy))
        {
            GUILayout.Label(
                string.Format(localizationManager.Localize("stars_count"), galaxyStarCounts[hoveredGalaxy]), 
                infoStyle
            );
            GUILayout.Label(
                string.Format(localizationManager.Localize("danger_rating"), galaxyDangerRatings[hoveredGalaxy]), 
                infoStyle
            );
        }
        else
        {
            GUILayout.Label(localizationManager.Localize("no_information"), infoStyle);
        }
    }
    else
    {
        GUILayout.Label(localizationManager.Localize("no_galaxy_selected"), infoStyle);
    }
}



private void StarM3InfoWindow(int windowID)
{
    if (selectedStar == null) return;

    
    GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 28,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true,
        normal = { textColor = new Color(0.4f, 0.9f, 1.0f) } 
    };

    GUIStyle contentStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 18,
        fontStyle = FontStyle.Italic,
        wordWrap = true,
        normal = { textColor = Color.white }
    };

    GUIStyle favoriteButtonStyle = new GUIStyle(GUI.skin.button)
    {
        fontSize = 18,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        normal = { textColor = Color.yellow },
        hover = { textColor = new Color(0.9f, 0.2f, 0.2f) } 
    };

    GUIStyle planetButtonStyle = new GUIStyle(GUI.skin.button)
    {
        fontSize = 16,
        alignment = TextAnchor.MiddleLeft,
        normal = { textColor = new Color(0.6f, 1.0f, 0.6f) }, 
        hover = { textColor = Color.cyan }
    };

    
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
    for (int i = 0; i < selectedStar.planetCount; i++)
    {
        PlanetInfo planetInfo = selectedStar.planetInfo[i];
        if (planetInfo != null)
        {
            if (GUILayout.Button(planetInfo.name, planetButtonStyle))
            {
                selectedStar.selectedPlanet = i;
                showPlanetWindow = true;
                Event.current.Use(); 
            }
        }
    }

    
    GUILayout.FlexibleSpace(); 
    if (GUILayout.Button(localizationManager.Localize("close"), favoriteButtonStyle))
    {
        selectedStar = null; 
    }
}

	private void JourneyTrackerWindow(int windowID)
{
    GUILayout.BeginVertical();

    foreach (string starName in journeyTracker)
    {
        Star star = FindStarByName(starName);
        if (star != null)
        {
            if (GUILayout.Button(starName))
            {
                selectedStar = star;
                MoveCameraToStar(star);
            }
        }
    }

    if (GUILayout.Button(localizationManager.Localize("close")))
    {
        showJourneyTrackerWindow = false;
    }

    GUILayout.EndVertical();
}

private void AddToJourneyTracker(Star star)
{
    if (star != null && !journeyTracker.Contains(star.name))
    {
        journeyTracker.Add(star.name);
        SaveJourneyTracker();
    }
}

private void SaveJourneyTracker()
{
    string path = Path.Combine(Application.persistentDataPath, "JourneyTracker.txt");
    File.WriteAllLines(path, journeyTracker);
    Debug.Log($"Saved journey tracker: {string.Join(", ", journeyTracker)}");
}

private void LoadJourneyTracker()
{
    string path = Path.Combine(Application.persistentDataPath, "JourneyTracker.txt");
    if (File.Exists(path))
    {
        string[] lines = File.ReadAllLines(path);
        journeyTracker.Clear();
        journeyTracker.AddRange(lines.Select(line => line.Trim()));
        Debug.Log($"Loaded journey tracker: {string.Join(", ", journeyTracker)}");
    }
    else
    {
        Debug.Log("JourneyTracker.txt does not exist.");
    }
}



private void InitWindowTextures()
{
    if (_panelBg != null) return;
    _panelBg      = MakeTex(1, 1, new Color(0.03f, 0.06f, 0.14f, 0.92f));
    _accentLine   = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f, 0.22f));
    _btnNormal    = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f, 0.07f));
    _btnVisit     = MakeTex(1, 1, new Color(0.39f, 1f, 0.63f, 0.07f));
    _btnClose     = MakeTex(1, 1, new Color(1f, 0.31f, 0.31f, 0.06f));
    _statCard     = MakeTex(1, 1, new Color(0.25f, 0.78f, 1f, 0.05f));
    _visitedBadge = MakeTex(1, 1, new Color(0.1f, 1f, 0.4f, 0.08f));
    _windowOpenTime = Time.realtimeSinceStartup;
}

private void PlanetM3InfoWindow(int windowID)
{
    InitWindowTextures();
    PlanetInfo planetInfo = selectedStar.planetInfo[selectedStar.selectedPlanet];
    float t = Time.realtimeSinceStartup - _windowOpenTime;

    TryLoadPlanetSaveStats(planetInfo.name);

    GUIStyle orbitronTitle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 13,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = false,
        normal = { textColor = new Color(0.25f, 0.78f, 1f) }
    };

    GUIStyle dataLabel = new GUIStyle(GUI.skin.label)
    {
        fontSize = 9,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleLeft,
        normal = { textColor = new Color(0.25f, 0.78f, 1f, 0.5f) }
    };

    GUIStyle dataValue = new GUIStyle(GUI.skin.label)
    {
        fontSize = 14,
        fontStyle = FontStyle.Italic,
        alignment = TextAnchor.MiddleLeft,
        wordWrap = true,
        normal = { textColor = new Color(0.86f, 0.92f, 1f, 0.9f) }
    };

    GUIStyle visitedStyle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 9,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        normal = { textColor = new Color(0.1f, 1f, 0.4f) }
    };

    GUIStyle btnStyleBase = new GUIStyle(GUI.skin.button)
    {
        fontSize = 10,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
    };

    GUIStyle btnNormal = new GUIStyle(btnStyleBase)
    {
        normal  = { background = _btnNormal,  textColor = new Color(0.63f, 0.91f, 1f) },
        hover   = { background = _btnNormal,  textColor = new Color(0f, 0.9f, 1f) },
        active  = { background = _btnNormal,  textColor = Color.white },
    };
    GUIStyle btnVisit = new GUIStyle(btnStyleBase)
    {
        normal  = { background = _btnVisit,  textColor = new Color(0.5f, 1f, 0.72f) },
        hover   = { background = _btnVisit,  textColor = new Color(0f, 1f, 0.67f) },
        active  = { background = _btnVisit,  textColor = Color.white },
    };
    GUIStyle btnClose = new GUIStyle(btnStyleBase)
    {
        normal  = { background = _btnClose,  textColor = new Color(1f, 0.63f, 0.63f, 0.8f) },
        hover   = { background = _btnClose,  textColor = new Color(1f, 0.5f, 0.5f) },
        active  = { background = _btnClose,  textColor = Color.white },
    };

    GUI.skin.window.normal.background = _panelBg;

GUILayout.BeginVertical();
    GUILayout.Space(10);
    DrawHLine(new Color(0.25f, 0.78f, 1f, 0.3f + Mathf.Sin(t * 2f) * 0.15f));
    GUILayout.Space(12);
    DrawPlanetOrb(t);
    GUILayout.Space(10);
    GUILayout.Label("PLANET INFORMATION", orbitronTitle);
    GUILayout.Space(14);

    DrawDataRow("NAME", planetInfo.name, dataLabel, dataValue);
    GUILayout.Space(4);
    string displayType = planetInfo.planetType.Contains("(Habitable)") ? "Gas Giant" : planetInfo.planetType;
    DrawDataRow("TYPE", displayType, dataLabel, dataValue);

    GUILayout.Space(10);
    DrawHLine(new Color(0.25f, 0.78f, 1f, 0.14f));
    GUILayout.Space(10);

    if (_cachedSaveExists)
    {
        GUILayout.Label("COLONY STATS", orbitronTitle);
        DrawDataRow("KINGDOMS", _cachedKingdomCount.ToString(), dataLabel, dataValue);
        DrawDataRow("YEAR", _cachedWorldYear.ToString(), dataLabel, dataValue);
        GUILayout.Space(10);
        DrawHLine(new Color(0.25f, 0.78f, 1f, 0.14f));
        GUILayout.Space(10);
    }

    if (!planetInfo.planetType.ToLower().Contains("gas giant"))
    {
        if (_cachedSaveExists)
        {

            if (GUILayout.Button("RESUME", btnVisit, GUILayout.Height(34)))
            {
                SpaceManager.GeneratePlanet(planetInfo.name, planetInfo.planetType, planetInfo.size, planetInfo.hasFauna);
            }
        }
        else
        {

            if (GUILayout.Button(localizationManager.Localize("visit").ToUpper(), btnVisit, GUILayout.Height(34)))
            {
                ClearAllActiveSprites();
                SaveVisitedPlanet(planetInfo.name);
                SpaceManager.GeneratePlanet(planetInfo.name, planetInfo.planetType, planetInfo.size, planetInfo.hasFauna);
            }
        }
        GUILayout.Space(6);
    }

    if (GUILayout.Button(localizationManager.Localize("parameters").ToUpper(), btnNormal, GUILayout.Height(34)))
        showParametersWindow = true;

    GUILayout.Space(6);
    if (GUILayout.Button(localizationManager.Localize("close").ToUpper(), btnClose, GUILayout.Height(34)))
        showPlanetWindow = false;

    GUILayout.Space(12);
    DrawHLine(new Color(0.25f, 0.78f, 1f, 0.12f));
    GUILayout.EndVertical();
}

private void PlanetParametersWindow(int windowID)
{
    InitWindowTextures();
    PlanetInfo planetInfo = selectedStar.planetInfo[selectedStar.selectedPlanet];
    float t = Time.realtimeSinceStartup - _windowOpenTime;

    GUIStyle orbitronTitle = new GUIStyle(GUI.skin.label)
    {
        fontSize = 13,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        normal = { textColor = new Color(0.25f, 0.78f, 1f) }
    };
    GUIStyle statKey = new GUIStyle(GUI.skin.label)
    {
        fontSize = 8,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleLeft,
        normal = { textColor = new Color(0.25f, 0.78f, 1f, 0.45f) }
    };
    GUIStyle statVal = new GUIStyle(GUI.skin.label)
    {
        fontSize = 15,
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        normal = { textColor = new Color(0.86f, 0.92f, 1f, 0.92f) }
    };
    GUIStyle betaNote = new GUIStyle(GUI.skin.label)
    {
        fontSize = 11,
        fontStyle = FontStyle.Italic,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true,
        normal = { textColor = new Color(1f, 0.78f, 0.31f, 0.6f) }
    };
    GUIStyle btnClose = new GUIStyle(GUI.skin.button)
    {
        fontSize = 10,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        normal  = { background = _btnClose, textColor = new Color(1f, 0.63f, 0.63f, 0.8f) },
        hover   = { background = _btnClose, textColor = new Color(1f, 0.5f, 0.5f) },
        active  = { background = _btnClose, textColor = Color.white },
    };

    GUI.skin.window.normal.background = _panelBg;

    GUILayout.BeginVertical();
    GUILayout.Space(10);
    DrawHLine(new Color(0.25f, 0.78f, 1f, 0.3f + Mathf.Sin(t * 2f) * 0.15f));
    GUILayout.Space(12);

    GUILayout.Label("PLANET PARAMETERS", orbitronTitle);
    GUILayout.Space(16);

    GUILayout.BeginHorizontal();
    DrawStatCard("SIZE",      planetInfo.size.ToString(), statKey, statVal);
    GUILayout.Space(8);
    DrawStatCard("HAS FAUNA", planetInfo.hasFauna ? "Yes" : "No", statKey, statVal);
    GUILayout.EndHorizontal();

    GUILayout.Space(14);
    DrawHLine(new Color(0.25f, 0.78f, 1f, 0.12f));
    GUILayout.Space(10);

    if (GUILayout.Button("CLOSE", btnClose, GUILayout.Height(34)))
        showParametersWindow = false;

    GUILayout.FlexibleSpace();
    GUILayout.Space(8);

    DrawBetaNotice(betaNote);
    GUILayout.Space(12);
    DrawHLine(new Color(0.25f, 0.78f, 1f, 0.12f));
    GUILayout.EndVertical();
}

private void DrawHLine(Color color)
{
    Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1), GUILayout.ExpandWidth(true));
    var tex = MakeTex(1, 1, color);
    GUI.DrawTexture(r, tex);
}

private void DrawDataRow(string label, string value, GUIStyle labelStyle, GUIStyle valueStyle)
{
    GUILayout.BeginHorizontal();
    GUILayout.Label(label, labelStyle, GUILayout.Width(52));
    GUILayout.Label(value, valueStyle);
    GUILayout.EndHorizontal();
}

private void DrawStatCard(string key, string value, GUIStyle keyStyle, GUIStyle valStyle)
{
    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
    var cardRect = GUILayoutUtility.GetRect(0, 58, GUILayout.ExpandWidth(true));
    GUI.DrawTexture(cardRect, _statCard);
    GUI.Label(new Rect(cardRect.x + 10, cardRect.y + 8,  cardRect.width - 20, 18), key,   keyStyle);
    GUI.Label(new Rect(cardRect.x + 10, cardRect.y + 28, cardRect.width - 20, 24), value, valStyle);
    GUILayout.EndVertical();
}

private void DrawVisitedBadge(GUIStyle textStyle, float t)
{
    GUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    var badgeRect = GUILayoutUtility.GetRect(110, 22);
    GUI.DrawTexture(badgeRect, _visitedBadge);

    float alpha = 0.5f + Mathf.Sin(t * 4f) * 0.5f;
    var dotTex = MakeTex(1, 1, new Color(0.1f, 1f, 0.4f, alpha));
    GUI.DrawTexture(new Rect(badgeRect.x + 10, badgeRect.y + 7, 8, 8), dotTex);
    GUI.Label(new Rect(badgeRect.x + 24, badgeRect.y, badgeRect.width - 24, 22), "VISITED", textStyle);
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
}

private void DrawPlanetOrb(float t)
{
    GUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    float hue = (t * 0.04f) % 1f;
    var orb = MakeTex(1, 1, Color.HSVToRGB(hue, 0.7f, 0.9f));
    var orbRect = GUILayoutUtility.GetRect(44, 44);
    GUI.DrawTexture(orbRect, orb);
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
}

private void DrawBetaNotice(GUIStyle style)
{
    var noticeRect = GUILayoutUtility.GetRect(GUIContent.none, style, GUILayout.MinHeight(44), GUILayout.ExpandWidth(true));
    GUI.DrawTexture(noticeRect, MakeTex(1, 1, new Color(1f, 0.78f, 0.31f, 0.04f)));
    GUI.Label(noticeRect, "Planet Parameters are in beta — more parameters will be added in future updates.", style);
}

private void SaveVisitedPlanet(string planetName)
{
    string filePath = Path.Combine(Application.persistentDataPath, "visited_planets.txt");

    if (!File.Exists(filePath))
    {
        File.WriteAllText(filePath, planetName + Environment.NewLine);
    }
    else
    {
        var visitedPlanets = File.ReadAllLines(filePath).ToList();
        if (!visitedPlanets.Contains(planetName))
        {
            File.AppendAllText(filePath, planetName + Environment.NewLine);
        }
    }
}

private bool IsPlanetVisited(string planetName)
{
    string filePath = Path.Combine(Application.persistentDataPath, "visited_planets.txt");

    if (File.Exists(filePath))
    {
        var visitedPlanets = File.ReadAllLines(filePath);
        return visitedPlanets.Contains(planetName);
    }

    return false;
}

private void Update()
{
    if (mainCamera == null)
    {
        Debug.LogError("Main camera is not available for update.");
        return;
    }

    hoveredStar = GetHoveredStar();

    float moveX = Input.GetAxis("Horizontal");
    float moveY = Input.GetAxis("Vertical");
    Vector3 moveDirection = new Vector3(moveX, moveY, 0);
    mainCamera.transform.position += moveDirection * moveSpeed * Time.deltaTime;

    float scroll = Input.GetAxis("Mouse ScrollWheel");
    mainCamera.orthographicSize = Mathf.Clamp(
        mainCamera.orthographicSize - scroll * zoomSpeed,
        minZoom,
        maxZoom
    );

    if (Input.GetMouseButtonDown(1)) 

    {
        lastMousePosition = Input.mousePosition;
        isDragging = true;
    }

    if (Input.GetMouseButtonUp(1)) 

    {
        isDragging = false;
    }

    if (isDragging)
    {
        Vector3 delta = Input.mousePosition - lastMousePosition;

        float camSize = mainCamera.orthographicSize;
        float camHeight = camSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        Vector3 move = new Vector3(
            -delta.x * camWidth / Screen.width,
            -delta.y * camHeight / Screen.height,
            0
        );

        mainCamera.transform.position += move;

        lastMousePosition = Input.mousePosition;
    }
}

private Star GetHoveredStar()
{

    if (stars == null || stars.Count == 0)
    {
        return null;
    }

    Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

    foreach (Star star in stars)
    {
        if (Vector2.Distance(mousePos, star.transform.position) < 0.5f)
        {
            return star;
        }
    }

    return null;
}

private static string GenerateStarName()
{
    string completedStarName = "";

    for (int i = 0; i < pattern.Length; i++)
    {
        if (pattern[i] == 'p')
        {
            completedStarName += prefixes[rand.Next(prefixes.Length)];
        }
        else if (pattern[i] == 's')
        {
            completedStarName += syllables[rand.Next(syllables.Length)];
        }
        else if (pattern[i] == 'x')
        {
            completedStarName += suffixes[rand.Next(suffixes.Length)];
        }
    }

    int randomNumber = UnityEngine.Random.Range(1, 1000);
    completedStarName += "-" + randomNumber.ToString(); 

    return completedStarName;
}

private Dictionary<string, float[]> galaxyStarWeights = new Dictionary<string, float[]>
{
    { "Crabby Way", new float[] { 0.1f, 0.2f, 0.1f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.05f, 0.1f, 0.1f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.00f } },
    { "Tuxxus", new float[] { 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.05f, 0.04f, 0.04f, 0.04f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.00f } },
    { "Krummple", new float[] { 0.1f, 0.2f, 0.1f, 0.1f, 0.05f, 0.05f, 0.1f, 0.05f, 0.05f, 0.1f, 0.1f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.00f } },
    { "BlueNight", new float[] { 0.1f, 0.1f, 0.3f, 0.05f, 0.05f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.04f, 0.04f, 0.04f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.00f } },
    { "Glass", new float[] { 0.1f, 0.1f, 0.3f, 0.05f, 0.05f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.04f, 0.04f, 0.04f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.00f } },
    { "Centuga", new float[] { 0.05f, 0.05f, 0.15f, 0.05f, 0.1f, 0.1f, 0.15f, 0.05f, 0.05f, 0.1f, 0.05f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.00f } }, 
    { "Dank", new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.2f, 0.1f, 0.05f, 0.05f, 0.1f, 0.05f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 1f } },
};

private string[] starTypes = { 
    "Red Dwarf", 
    "Yellow Dwarf", 
    "Blue Giant", 
    "White Dwarf", 
    "Neutron Star", 
    "Brown Dwarf", 
    "Supergiant", 
    "Pulsar", 
    "White Supergiant", 
    "Red Supergiant", 
    "Black Hole",
    "Rainbow Star",         
    "Void Star",            
    "Crystal Star",         
    "Quantum Star",         
    "Echo Star",            
    "Chrono Star",          
    "Phantom Star",         
    "Prism Star",           
    "Nebula Star",          
    "Graviton Star",        
    "Aurora Star",
	"Corrupted Star"
};

private string GenerateStarType()
{

    if (!galaxyStarWeights.TryGetValue(activeGalaxy, out float[] weights))
    {

	Debug.Log("Gay Balls");
        weights = new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
    }

    float totalWeight = weights.Sum();
    float[] normalizedWeights = weights.Select(weight => weight / totalWeight).ToArray();

    float randomValue = UnityEngine.Random.value;
    float cumulativeWeight = 0f;
    for (int i = 0; i < normalizedWeights.Length; i++)
    {
        cumulativeWeight += normalizedWeights[i];
        if (randomValue <= cumulativeWeight)
        {
            return starTypes[i];
        }
    }

    return starTypes[starTypes.Length - 1]; 
}


private string GetGalaxyDescription(string galaxyName)
{
    switch (galaxyName)
    {
        case "Crabby Way":
            return localizationManager.Localize("crabby_way");
        case "Tuxxus":
            return localizationManager.Localize("tuxxus");
        case "Krummple":
            return localizationManager.Localize("krummple");
        case "BlueNight":
            return localizationManager.Localize("bluenight");
        case "Glass":
            return localizationManager.Localize("glass");
        case "Centuga":
            return localizationManager.Localize("centuga");
        case "Dank":
            return localizationManager.Localize("dank");
        default:
            if (CustomGalaxyDescriptions.ContainsKey(galaxyName))
            {
                return CustomGalaxyDescriptions[galaxyName];
            }
            else
            {
                return "No description available.";
            }
    }
}


	private string ToRomanNumeral(int number)
{

    string[] romanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

    if (number < 1 || number > romanNumerals.Length)
    {
        return number.ToString(); 
    }

    return romanNumerals[number - 1];
}

private void TryLoadPlanetSaveStats(string planetName)
{

    if (_lastLoadedPlanetSave == planetName) return;

    _lastLoadedPlanetSave = planetName;
    _cachedSaveExists = false;
    _cachedKingdomCount = 0;
    _cachedCityCount = 0;
    _cachedWorldYear = 0;

    string spaceBoxPath = Path.Combine(Application.persistentDataPath, "modernbox");
    if (!Directory.Exists(spaceBoxPath)) return;

    string[] allDirectories = Directory.GetDirectories(spaceBoxPath, "*", SearchOption.AllDirectories);
    string targetDirectory = null;

    foreach (string directory in allDirectories)
    {
        if (Path.GetFileName(directory).Equals(planetName, StringComparison.OrdinalIgnoreCase))
        {
            targetDirectory = directory;
            break;
        }
    }

    if (!string.IsNullOrEmpty(targetDirectory))
    {

        SaveReader reader = new SaveReader(targetDirectory);

        if (reader.ReadFile())
        {
            _cachedSaveExists = true;

            var kingdoms = reader.ReadKingdoms();
            var cities = reader.ReadCities();
            var stats = reader.ReadMapStats();

            if (kingdoms != null) _cachedKingdomCount = kingdoms.Count;
            if (cities != null) _cachedCityCount = cities.Count;
            if (stats != null) _cachedWorldYear = stats.year;

            reader.Dispose();
        }
    }
}

private void ClearStars()
{
    foreach (Star star in stars)
    {
        if (star != null)
        {
            Destroy(star.gameObject); 
        }
    }
    stars.Clear(); 
}

private void ClearAllActiveSprites()
{

    SpriteRenderer[] allSprites = GameObject.FindObjectsOfType<SpriteRenderer>();

    foreach (SpriteRenderer sprite in allSprites)
    {

        Destroy(sprite.gameObject);
    }
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

    string description = "";

    switch (starType)
    {
        case "Red Dwarf":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        case "Yellow Dwarf":
            description += RandomPhrase(climatePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            break;
        case "Blue Giant":
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            description += RandomPhrase(climatePhrases) + ". ";
            break;
        case "White Dwarf":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(atmospherePhrases) + ". ";
            break;
        case "Neutron Star":
            description += RandomPhrase(geologicalPhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            break;
        case "Brown Dwarf":
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(geologicalPhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            break;
        case "Supergiant":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(atmospherePhrases) + ". ";
            break;
        case "Pulsar":
            description += RandomPhrase(geologicalPhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(climatePhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        case "White Supergiant":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(atmospherePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            break;
        case "Red Supergiant":
            description += RandomPhrase(climatePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(atmospherePhrases) + ". ";
            break;
        case "Black Hole":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(atmospherePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            break;
        case "Rainbow Star":
            description += RandomPhrase(climatePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            break;
        case "Void Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(atmospherePhrases) + ". ";
            break;
        case "Crystal Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        case "Quantum Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(geologicalPhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        case "Echo Star":
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            break;
        case "Chrono Star":
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(surfacePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            break;
        case "Phantom Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(geologicalPhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        case "Prism Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            break;
        case "Nebula Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        case "Graviton Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(floraFaunaPhrases) + ", ";
            description += RandomPhrase(geologicalPhrases) + ". ";
            break;
        case "Aurora Star":
            description += RandomPhrase(surfacePhrases) + ", ";
            description += RandomPhrase(climatePhrases) + ". ";
            description += RandomPhrase(atmospherePhrases) + ", ";
            description += RandomPhrase(floraFaunaPhrases) + ". ";
            break;
        default:
            description += "The planet features a diverse and dynamic environment.";
            break;
    }

    switch (planetType)
    {
        case "Desert World":
            description += "The desert surface is a notable feature of this planet.";
            break;
        case "Gas Giant":
            description += "The gaseous composition creates a unique and visually stunning environment.";
            break;
        case "Oceanic":
            description += "The extensive oceans provide a rich and dynamic ecosystem.";
            break;
        case "Icy":
            description += "The icy surface presents a stark, yet beautiful landscape.";
            break;
        case "Crystal World":
            description += "The planet's surface is covered in luminous crystals that create a surreal visual effect.";
            break;
        case "Swamp World":
            description += "The planet is shrouded in murky swamps with glowing, bioluminescent vegetation.";
            break;
        case "Lava World":
            description += "The planet features an intense volcanic landscape with rivers of molten lava.";
            break;
        case "Gas Giant (Habitable)":
            description += "Floating islands and habitats within the upper atmosphere provide a unique living environment.";
            break;
        case "Mechanical World":
            description += "An artificial surface dominated by advanced technology and remnants of a bygone civilization.";
            break;
        case "Jungle World":
            description += "Filled with dense flora and diverse fauna.";
            break;
        case "Corrupted World":
            description += "The planet is a desolate and twisted wasteland, corrupted by unknown forces.";
            break;
        case "Lemon World":
            description += "The surface is covered in yellow, citrus-like vegetation, creating a surreal and fragrant landscape.";
            break;
        case "Mushroom World":
            description += "Giant mushrooms dominate the landscape, providing shelter and resources to the unique ecosystem.";
            break;
        case "Wasteland World":
            description += "The planet is a barren and scorched wasteland, with ruins of ancient civilizations scattered across its surface.";
            break;
        default:
            description += "The planet features a diverse and dynamic environment.";
            break;
    }

    return description;
}

private string RandomPhrase(List<string> phrases)
{
    int index = UnityEngine.Random.Range(0, phrases.Count);
    return phrases[index];
}

    private Dictionary<string, int> GenerateResources()
    {

        string[] resourceTypes = { "Steel", "Soil", "Water", "Gold", "Uranium" };
        Dictionary<string, int> resources = new Dictionary<string, int>();
        foreach (string resource in resourceTypes)
        {
            resources[resource] = UnityEngine.Random.Range(0, 1000);
        }
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

    var hazardPhrases = new List<string>
    {
        "Unstable surface with frequent eruptions",
        "Toxic atmosphere with corrosive elements",
        "High radiation levels affecting electronics and life forms",
        "Severe gravitational effects causing tidal waves and geological disruptions",
        "Dangerous weather patterns including violent storms and temperature extremes",
        "High risk of meteor impacts and space debris",
        "Hazardous chemical spills and environmental contamination",
        "Extreme tidal forces leading to significant geological instability",
        "High volcanic activity with continuous lava flows",
        "Unpredictable climate variations making the environment hostile"
    };

    string description = "";

    switch (starType)
    {
        case "Red Dwarf":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "due to the star's minimal luminosity and unstable radiation output. ";
            description += "This creates a challenging environment for survival and exploration.";
            break;
        case "Yellow Dwarf":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "resulting from the planet's position in the habitable zone with fluctuating conditions. ";
            description += "The diverse but hazardous environment demands careful navigation.";
            break;
        case "Blue Giant":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "because of the intense radiation and high energy output of the star. ";
            description += "These factors contribute to a hazardous environment for both equipment and life.";
            break;
        case "White Dwarf":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "due to the star's extreme density and high gravitational pull. ";
            description += "This causes severe environmental hazards and challenges for exploration.";
            break;
        case "Neutron Star":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "caused by the intense gravitational forces and high radiation levels from the star. ";
            description += "These conditions make the planet's environment extremely dangerous.";
            break;
        case "Brown Dwarf":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "as the star's low energy output leads to unstable conditions on nearby planets. ";
            description += "Explorers must be prepared for unpredictable hazards.";
            break;
        case "Supergiant":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "due to the massive energy output and unstable stellar conditions. ";
            description += "The extreme environment presents significant challenges for survival.";
            break;
        case "Pulsar":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "due to the pulsar's intense radiation bursts and high-energy emissions. ";
            description += "These factors create hazardous conditions for any exploration or habitation.";
            break;
        case "White Supergiant":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "resulting from the star's enormous size and high energy levels. ";
            description += "The environment is fraught with danger due to these extreme conditions.";
            break;
        case "Red Supergiant":
            description += RandomPhrase(dangerPhrases) + ", ";
            description += "because of the star's immense size and unstable energy output. ";
            description += "These factors contribute to a highly dangerous environment.";
            break;
        case "Black Hole":
            description += "The planet orbits a black hole, creating extreme gravitational effects and intense tidal forces. ";
            description += "The environment is highly hazardous with severe space-time distortions and gravitational disruptions.";
            break;
        default:
            description += "Planet faces danger due to " + RandomPhrase(dangerPhrases) + ". ";
            description += "These conditions result from its proximity to " + starType + ". ";
            description += "The hazardous environment requires careful consideration for exploration.";
            break;
    }

    switch (planetType)
    {
        case "Desert World":
            description += " The desert surface is prone to seismic instability and geological hazards.";
            break;
        case "Gas Giant":
            description += " The gaseous composition creates unpredictable atmospheric conditions and hazardous weather patterns.";
            break;
        case "Oceanic":
            description += " The extensive oceans contribute to dangerous weather conditions and extreme climate variations.";
            break;
        case "Icy":
            description += " The icy surface creates challenges for exploration due to extreme cold and unstable terrain.";
            break;
        case "Crystal World":
            description += " The crystalline surface is not only visually stunning but also presents unique environmental hazards.";
            break;
        case "Swamp World":
            description += " The swampy environment is filled with hazardous gases and dangerous wildlife.";
            break;
        case "Lava World":
            description += " The volcanic activity creates severe hazards, including constant lava flows and unstable terrain.";
            break;
        case "Gas Giant (Habitable)":
            description += " The floating habitats are subject to unpredictable atmospheric conditions and hazards.";
            break;
        case "Mechanical World":
            description += " The artificial surface and technological remnants create environmental hazards due to malfunctioning systems.";
            break;
        case "Jungle World":
            description += " Extreme tidal forces create severe geological instability and environmental challenges.";
            break;
        case "Corrupted World":
            description += "The planet is highly unstable, with areas of corruption that could pose unknown dangers.";
            break;
        case "Lemon World":
            description += "While the surface appears harmless, the acidic environment poses a hidden threat.";
            break;
        case "Mushroom World":
            description += "The giant mushrooms release spores that can be toxic to unprepared explorers.";
            break;
        case "Wasteland World":
            description += "The harsh environment and radiation pockets make this planet extremely dangerous.";
            break;
    }

    return description;
}

    private void OnDisable()
    {

        if (mainCamera != null)
        {
            Destroy(mainCamera.gameObject);
            mainCamera = null;
        }
    }
}

[System.Serializable]
public class Star : MonoBehaviour
{
    public string name;
    public string starType;
    public int planetCount;
    public PlanetInfo[] planetInfo;
    public int selectedPlanet;

    [JsonIgnore] 
    public GameObject gameObject; 
}

[System.Serializable]
public class PlanetInfo
{
    public string name;
    public string description;
    public Dictionary<string, int> resources;
    public int dangerRating;
    public string dangerDescription;
    public string planetType; 
    public string size; 
    public bool hasFauna; 
    public MoonInfo[] moonInfo;
}

[System.Serializable]
public class MoonInfo
{
    public string name;
    public string description;
    public string moonType; 
    public string size; 
    public bool hasFauna; 
}

    public class CompendiumEntry
    {
        public string Title { get; }
        public string Content { get; }

        public CompendiumEntry(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }

    [System.Serializable]
    public class GalaxyData
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
	
public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector3 vector = (Vector3)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vector.z);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        float x = obj["x"].Value<float>();
        float y = obj["y"].Value<float>();
        float z = obj["z"].Value<float>();
        return new Vector3(x, y, z);
    }
}