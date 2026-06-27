using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq; 
using System; 
using TuxModLoader.Reflection;

namespace ModernBox
{
    public class SpaceManager : MonoBehaviour
    {
        private static GameObject spaceGameObject;
        private static List<GameObject> existingGameObjects = new List<GameObject>();
        private static bool isSpaceEnabled = false;
        private static bool supportInitialized = false;
        public static bool startup = false;
        public static SpaceManager instance;
		public static string otherfilePath;
		private static MusicBox musicBox;
		public PlayWavDirectly PlayWavDirectly = new PlayWavDirectly();

        public static SpaceManager EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject managerObject = new GameObject("ModernBoxSpaceManager");
            return managerObject.AddComponent<SpaceManager>();
        }

        private void Awake()
        {
                if (instance != null && instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                instance = this;
                DontDestroyOnLoad(gameObject);
				InitializeMusicBox();
                InitializeSpaceSupportIfNeeded();
        }

		private void InitializeMusicBox()
		{
			musicBox = FindObjectOfType<MusicBox>();
			
			if (musicBox != null)
			{
				Debug.Log($"MusicBox found: {musicBox.gameObject.name}");
			}
			else
			{
				Debug.LogWarning("No MusicBox found in the scene.");
			}
		}

        private void InitializeSpaceSupportIfNeeded()
        {
            if (supportInitialized)
            {
                return;
            }

            supportInitialized = true;
			ListObjectsWithDiscordTracker();
			ModifyDiscordSetting(true);
            CleanUpModernBoxData(); 
	    }

        public static void CleanUpModernBoxData()
        {
            string persistentDataPath = Application.persistentDataPath;
            string spaceBoxPath = Path.Combine(persistentDataPath, "ModernBox");
			otherfilePath = Path.Combine(Application.persistentDataPath, "vals2.txt");

        
        if (!File.Exists(otherfilePath))
        {


            
            File.WriteAllText(otherfilePath, "The user had old data from an old modernbox version and it was cleaned. (if your a user reading this, i'm sorry i just didn't want to add a popup saying 'your old modernbox data was deleted because it was invalid now'");
        }
        else
        {
            Debug.Log("Function has already run; skipping.");
			return;
        }

        PlayerPrefs.SetInt("DoneBefore", 0);
            
            if (Directory.Exists(spaceBoxPath))
            {
                try
                {
                    
                    DirectoryInfo dir = new DirectoryInfo(spaceBoxPath);
                    foreach (FileInfo file in dir.GetFiles()) file.Delete();
                    foreach (DirectoryInfo subDir in dir.GetDirectories()) subDir.Delete(true);

                    Debug.Log("Successfully deleted all contents in the 'ModernBox' folder.");
                }
                catch (IOException e)
                {
                    Debug.LogError($"Error while deleting contents of 'ModernBox': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("'ModernBox' folder does not exist.");
            
            File.WriteAllText(otherfilePath, "The user had no old data from an old modernbox version. (if your a user reading this, i'm sorry i just didn't want to add a popup saying 'your old modernbox data was deleted because it was invalid now'");
            }

            
            string[] filesToDelete = new string[]
            {
                Path.Combine(persistentDataPath, "activeGalaxy.txt"),
                Path.Combine(persistentDataPath, "JourneyTracker.txt"),
                Path.Combine(persistentDataPath, "visited_planets.txt")
            };

            
            foreach (string filePath in filesToDelete)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        Debug.Log($"Successfully deleted {Path.GetFileName(filePath)}.");
                    }
                    catch (IOException e)
                    {
                        Debug.LogError($"Error while deleting {Path.GetFileName(filePath)}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"{Path.GetFileName(filePath)} does not exist.");
                }
            }
        }
		
        public static void DeleteBomb()
        {
            string persistentDataPath = Application.persistentDataPath;
            string spaceBoxPath = Path.Combine(persistentDataPath, "ModernBox");

		
            
            if (Directory.Exists(spaceBoxPath))
            {
                try
                {
                    
                    DirectoryInfo dir = new DirectoryInfo(spaceBoxPath);
                    foreach (FileInfo file in dir.GetFiles()) file.Delete();
                    foreach (DirectoryInfo subDir in dir.GetDirectories()) subDir.Delete(true);

                    Debug.Log("Successfully deleted all contents in the 'ModernBox' folder.");
                }
                catch (IOException e)
                {
                    Debug.LogError($"Error while deleting contents of 'ModernBox': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("'ModernBox' folder does not exist.");
                Directory.CreateDirectory(spaceBoxPath);
            }
        PlayerPrefs.SetInt("DoneBefore", 0);

            
            string[] filesToDelete = new string[]
            {
                Path.Combine(persistentDataPath, "activeGalaxy.txt"),
                Path.Combine(persistentDataPath, "JourneyTracker.txt"),
                Path.Combine(persistentDataPath, "visited_planets.txt")
            };

            
            foreach (string filePath in filesToDelete)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        Debug.Log($"Successfully deleted {Path.GetFileName(filePath)}.");
                    }
                    catch (IOException e)
                    {
                        Debug.LogError($"Error while deleting {Path.GetFileName(filePath)}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"{Path.GetFileName(filePath)} does not exist.");
                }
            }
        }
		
    public void ModifyDiscordSetting(bool disable)
    {

        if (typeof(Config).GetField("disable_discord") != null)
        {

            Config.disable_discord = disable;
            Config.disable_steam = disable;
            Debug.Log($"Config.disable_discord has been set to {disable}");
        }
        else
        {
            Debug.LogError("Config.disable_discord property not found.");
        }
    }

        public static void EnableSpace()
        {
            SpaceManager manager = EnsureInstance();
            if (manager == null || isSpaceEnabled)
            {
                return;
            }

            manager.InitializeSpaceSupportIfNeeded();
            manager.StartCoroutine(manager.EnableSpaceCoroutine());
        }

        private IEnumerator EnableSpaceCoroutine()
        {

        var busMaster = ReflectionHelper.GetFieldValue<object>(musicBox, "_bus_master");

        ReflectionHelper.InvokeMethod(busMaster, "setVolume", 0.0f);

				 string filePath = System.IO.Path.Combine($"{Mod.Info.Path}/EmbededResources/Sounds/roar.wav");

				
	Debug.Log(filePath);
	Vector3 posV3 = new Vector3(0, 0, 0); 
	PlayWavDirectly.Instance.PlaySoundAtPosition(filePath,posV3);
	
            if (instance == null)
            {
                Debug.LogError("SpaceManager instance is not initialized.");
                yield break;
            }

            ScanExistingGameObjects();

        //    yield return new WaitForSeconds(2);

            foreach (var obj in existingGameObjects)
            {
                if (obj != instance.gameObject && obj != null) 
                    obj.SetActive(false);
            }

            if (spaceGameObject == null)
            {
                spaceGameObject = new GameObject("SpaceGameObject");
                LocalizationManager LocalizationManager = spaceGameObject.AddComponent<LocalizationManager>();

                Camera spaceCamera = spaceGameObject.AddComponent<Camera>();
                spaceCamera.orthographic = true;
                spaceCamera.orthographicSize = 5;
                spaceCamera.clearFlags = CameraClearFlags.SolidColor;
                spaceCamera.backgroundColor = Color.black;

            spaceGameObject.SetActive(true);

                StarManager starManager = spaceGameObject.AddComponent<StarManager>();
            }

            isSpaceEnabled = true;
        }

public static void DisableSpace()
{
    if (instance == null || !isSpaceEnabled)
    {
        Debug.LogWarning("SpaceManager is either not initialized or space is already disabled.");
        return;
    }
    try
    {
        if (spaceGameObject != null)
        {
            Debug.Log($"Destroying spaceGameObject: {spaceGameObject.name}");
            Destroy(spaceGameObject);
            spaceGameObject = null;
        }

        if (existingGameObjects.Count > 0)
        {
            foreach (var obj in existingGameObjects)
            {
                if (obj != instance.gameObject && obj != null) 
                    obj.SetActive(true);
            }
		}
		PlayWavDirectly.Instance.StopAllSounds();

        var busMaster = ReflectionHelper.GetFieldValue<object>(musicBox, "_bus_master");

        ReflectionHelper.InvokeMethod(busMaster, "setVolume", 1.0f);

		GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
		foreach (GameObject obj in allGameObjects)
		{
			if (obj.name == "Star" || obj.name == "Nebula")
			{
				Destroy(obj);
			}
		}

        isSpaceEnabled = false;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Exception in DisableSpace: {ex.Message}");
    }
}

public static void GeneratePlanet(string planetName, string planetType, string planetSize, bool hasFauna)
{
    Debug.Log($"Generating planet: {planetName}");

    SaveManager SaveManager = FindObjectOfType<SaveManager>();
    if (SaveManager == null)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeInHierarchy)
            {
                SaveManager = obj.GetComponent<SaveManager>();
                if (SaveManager != null)
                {
                    obj.SetActive(true);
                    break;
                }
            }
        }
    }
    WindowPreloaderUtils.ForceClearPreloader();
    if (SaveManager == null)
    {
        Debug.LogError("SaveManager instance not found.");
        return;
    }

	          string planetCountFilePath = Path.Combine(Application.persistentDataPath, "ModernBox", "PlanetCount.txt");

            int planetCount = 1;
            if (File.Exists(planetCountFilePath))
            {
                string countText = File.ReadAllText(planetCountFilePath);
                if (int.TryParse(countText, out int count))
                {
                    planetCount = count + 1;
                }
            }

            File.WriteAllText(planetCountFilePath, planetCount.ToString());

            Debug.Log($"Planet Count: {planetCount}");

    string currentGalaxyName = "Crabby Way"; 
    string currentPlanetName = PlanetManager.instance.GetCurrentPlanet();

    string spaceBoxPath = Path.Combine(Application.persistentDataPath, "modernbox");
    Debug.Log($"ModernBox Path: {spaceBoxPath}");
    string[] starJsonFiles = Directory.GetFiles(spaceBoxPath, "star.json", SearchOption.AllDirectories);
    Debug.Log($"Found {starJsonFiles.Length} star.json files");

if (!startup)
{
    string saveDirectory = null;
    foreach (string starFile in starJsonFiles)
    {

        if (IsPlanetInStarFile(starFile, currentPlanetName))
        {
            saveDirectory = Path.GetDirectoryName(starFile);
            Debug.Log($"Save directory found: {saveDirectory}");
            break;
        }
    }

    if (saveDirectory == null)
    {
        Debug.LogError($"Planet {currentPlanetName} not found in any star file.");
        return;
    }

    string newSaveDirectory = Path.Combine(saveDirectory, currentPlanetName); 
    SaveManager.saveWorldToDirectory(newSaveDirectory);
}
string nextPlanetLoadDirectory = null;
string[] allDirectories = Directory.GetDirectories(spaceBoxPath, "*", SearchOption.AllDirectories);

foreach (string directory in allDirectories)
{
    if (Path.GetFileName(directory).Equals(planetName, StringComparison.OrdinalIgnoreCase))
    {
        nextPlanetLoadDirectory = directory;
        Debug.Log($"Found directory for planet {planetName}: {nextPlanetLoadDirectory}");
        PlanetManager.instance.SetCurrentPlanet(planetName);
        PlanetManager.instance.SetCurrentPlanetType(planetType);
        DisableSpace();
        SaveManager.loadWorld(nextPlanetLoadDirectory, false);
        StatManager.Instance.VisitPlanet();
        return;
    }
}

if (nextPlanetLoadDirectory == null)
{
    Debug.LogError($"No subfolder named {planetName} found in any directory.");
}

            PlanetManager.instance.SetCurrentPlanet(planetName);
            PlanetManager.instance.SetCurrentPlanetType(planetType);
            
            DisableSpace();
			PlanetGenerator.ChoosePlanetBiomes(planetType, hasFauna);
        startup = false;

    string[] sizeParts = planetSize.Split('x');
    if (sizeParts.Length == 2 && int.TryParse(sizeParts[0].Trim(), out int sizeX) && int.TryParse(sizeParts[1].Trim(), out int sizeY))
    {
        PlanetGenerator.mapSizeX = sizeX;
        PlanetGenerator.mapSizeY = sizeY;
    }
    else
    {
        Debug.LogError($"Invalid planet size: {planetSize}");
        return;
    }

            MapBox.instance.generateNewMap();
			MapBox.instance.map_stats.name = "test";
            StatManager.Instance.VisitPlanet();
}

        public static bool IsPlanetInStarFile(string starFile, string planetName)
        {
            if (!File.Exists(starFile))
            {
                Debug.LogError($"Star file does not exist: {starFile}");
                return false;
            }

            string starData = File.ReadAllText(starFile);

            try
            {

                Star star = JsonConvert.DeserializeObject<Star>(starData, new StarConverter());

                return star.planetInfo.Any(p => p.name.Equals(planetName, StringComparison.OrdinalIgnoreCase));
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Failed to deserialize star file: {starFile}. Exception: {ex.Message}");
                return false;
            }
        }

private static void ScanExistingGameObjects()
{
    existingGameObjects.Clear();
    UnityEngine.GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.GameObject>();

    foreach (var obj in allObjects)
    {
        if (obj != spaceGameObject && obj.activeInHierarchy && obj.GetComponent<DiscordTracker>() == null)
        {
            existingGameObjects.Add(obj);
        }
    }
}

public static void ListObjectsWithDiscordTracker()
{

    DiscordTracker[] discordTrackers = UnityEngine.Object.FindObjectsOfType<DiscordTracker>();

    Debug.Log($"Found {discordTrackers.Length} GameObjects with DiscordTracker:");

    foreach (var tracker in discordTrackers)
    {
        Debug.Log(tracker.gameObject.name);
    }
}

        private void Update()
        {
            if (isSpaceEnabled)
            {
                MoveCamera();
            }
        }

        private void MoveCamera()
        {
            float moveSpeed = 5f;
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (Camera.main != null)
            {
                Camera.main.transform.Translate(new Vector3(h, v, 0) * moveSpeed * Time.deltaTime);
            }
        }
    }
}
