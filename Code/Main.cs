//========= MODERNBOX 2.1.0.1 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.IO;
using System.Linq;
using NCMS;
using tools;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;
using HarmonyLib;
using System.Reflection;
using Newtonsoft.Json;
using ModernBox;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Threading.Tasks;
using static Config;
using System.Reflection.Emit;
using UnityEngine.Tilemaps;
using UnityEngine.Purchasing.MiniJSON;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using UnityEngine.CrashReportHandler;
using System.IO.Compression;
using System.Threading;
using System.Text;
using Beebyte.Obfuscator;
using ai;
using ai.behaviours;
using life.taxi;
using SleekRender;
using tools.debug;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using WorldBoxConsole;
using UnityEngine.UI;
using static TopTileLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions.Must;
using Random=UnityEngine.Random;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
// using TuxModLoader.Builders;

namespace ModernBox{
    [ModEntry]
    class Main : MonoBehaviour{
        #region
        public static Main instance;
        #endregion
        internal const string id = "Tuxxego.mods.worldbox.MX";
        internal static Harmony harmony;
        internal static Dictionary<string, UnityEngine.Object> modsResources;
        public BuildingLibrary buildingLibrary = new BuildingLibrary();
        public const string mainPath = "Mods/MX";
        public Buttonz Buttonz = new Buttonz();
        public PlayWavDirectly PlayWavDirectly = new PlayWavDirectly();
        public SpaceManager SpaceManager = new SpaceManager();
        public PlanetGenerator PlanetGenerator = new PlanetGenerator();
        public AchievementManager AchievementManager = new AchievementManager();
        public LocalizationManager LocalizationManager = new LocalizationManager();
        public BombUtilities testBombDebugger = new BombUtilities();
        public Bombs Bombs = new Bombs();
        public StatManager StatManager = new StatManager();
        public UnitTracker UnitTracker = new UnitTracker();
        public FirstTimeSetup FirstTimeSetup = new FirstTimeSetup();
        public StupidWorldboxia StupidWorldboxia = new StupidWorldboxia();
        private static string correctSettingsVersion = "5.01"; 
        public const string settingsKey = "MBoxSettings"; 
        public static bool isNewVersion;
        public static SavedSettings savedSettings = new SavedSettings();
        public static PizzaManager PizzaManager = new PizzaManager();

        internal static string ModName
        {
            get { return Mod.Info.Name; }
        }

        void Awake()
        {
            try
            {
	            loadSettings();

                FuckWorldboxia();

                harmony = new Harmony("com.Tuxxego.MX");

                Assembly targetAssembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(targetAssembly);

                new TabBuilder()
				.SetTabID("ModernBoxTab")
				.SetName("ModernBox")
				.SetDescription("Eras, Space, Politics, and more.")
				.SetPosition(200)
                .isAfrican(false)
				.SetIcon("ui/icons/tabIconModernWarfare")
				.Build();

                new TabBuilder()
				.SetTabID("ModernBoxUnits")
				.SetName("ModernBox Units")
				.SetDescription("A BUNCH of guys to spawn!")
				.SetPosition(200)
                .isAfrican(true)
				.SetIcon("ui/icons/warhamma")
				.Build();

                new TabBuilder()
				.SetTabID("ModernBoxBombs")
				.SetName("ModernBox Bombs")
				.SetDescription("A BUNCH of weapons made by foreign terrorists to spawn!")
				.SetPosition(200)
                .isAfrican(true)
				.SetIcon("ui/icons/Overload")
				.Build();

                new TabBuilder()
				.SetTabID("ModernBoxEras")
				.SetName("ModernBox Eras")
				.SetDescription("Everything related to eras!")
				.SetPosition(200)
                .isAfrican(true)
				.SetIcon("ui/icons/Industrial")
				.Build();

                new TabBuilder()
				.SetTabID("ModernBoxSpace")
				.SetName("ModernBox Space")
				.SetDescription("Explore the vastness of space!")
				.SetPosition(200)
                .isAfrican(true)
				.SetIcon("Stars/Gravitonstar")
				.Build();

                new TabBuilder()
				.SetTabID("ModernBoxItems")
				.SetName("ModernBox Items")
				.SetDescription("Toggle specific item types.")
				.SetPosition(200)
                .isAfrican(true)
				.SetIcon("ui/icons/firearm")
				.Build();

                ModernBoxPrefs.Load();

            //    PowersTab tab = TabManager.CreateTab("ModernBox", "ModernBox", "Best Mod Ever", Resources.Load<Sprite>("ui/icon"));

                  ModernBoxLogger.Log("[M2] Space Manager starting...");
                SpaceManager = gameObject.AddComponent<SpaceManager>();
                ModernBoxLogger.Log("[M2] That big manager has started.");
        		AchievementManager = gameObject.AddComponent<AchievementManager>();
        		StatManager = gameObject.AddComponent<StatManager>();
                gameObject.AddComponent<YourMomIsFat>();
        		UnitTracker = gameObject.AddComponent<UnitTracker>();
        		testBombDebugger = gameObject.AddComponent<BombUtilities>();
                gameObject.AddComponent<PizzaManager>();
                ModernBoxLogger.Log("SpaceBoxModernBox: Pls no lag!");

                ModernBoxLogger.Log("[MX] Initializing Bombs...");
                Bombs.Init();
                ModernBoxLogger.Log("[MX] Bombs loaded!");

                ModernBoxLogger.Log("[MX] Loading AboutWindow...");
                AboutWindow.init();
                ModernBoxLogger.Log("[MX] AboutWindow loaded!");

                ModernBoxLogger.Log("[MX] Loading InfiniteBoxWindow...");
                InfiniteBoxWindow.init();
                ModernBoxLogger.Log("[MX] InfiniteBoxWindow loaded!");

                ModernBoxLogger.Log("[MX] Loading CreditsWindow...");
                CreditsWindow.init();
                ModernBoxLogger.Log("[MX] CreditsWindow loaded!");

                ModernBoxLogger.Log("[MX] Loading SpaceWindow...");
                SpaceWindow.init();
                ModernBoxLogger.Log("[MX] SpaceWindow loaded!");
                ModernBoxLogger.Log("[MX] Loading CustomGalaxiesWindow...");
                CustomGalaxiesWindow.init();
                ModernBoxLogger.Log("[MX] CustomGalaxiesWindow loaded!");

                              ModernBoxLogger.Log("[MX] Loading AchievementsWindow...");
                 AchievementsWindow.init();
                 ModernBoxLogger.Log("[MX] AchievementsWindow loaded!");
                                 Vehicles.init();
                ModernBoxLogger.Log("[MX] Initializing Buttonz...");
                Buttonz.Init();
                ModernBoxLogger.Log("[MX] Buttonz loaded!");

                ModernBoxLogger.Log("[MX] Initializing Zombies...");
                Zombies.create_Zombies();
                ModernBoxLogger.Log("[MX] Zombies loaded!");

                BombEffects.Init();
                ModernBoxLogger.Log("[MX] Bomb effects initialized!");

                ModernBoxLogger.Log("[MX] Buildings initialized");

			    Windows.ShowWindow("AboutWindow");

                GameObject setupGO = new GameObject("FirstTimeSetupGO");
                setupGO.AddComponent<FirstTimeSetup>();
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[MX] Exception in Awake: {ex.Message}");
                ModernBoxLogger.Error($"[MX] Stack Trace: {ex.StackTrace}");
            }
        }


        void Start()
        {
            InitializeComponents();

        }

        void InitializeComponents()
        {
            try
            {
                modsResources = Reflection.GetField(typeof(ResourcesPatch), null, "modsResources") as Dictionary<string, UnityEngine.Object>;
                ModernBoxLogger.Log("Meow meow meow, FEED ME YOU MOTHERF");
                ModernBoxLogger.Log("No thanks Morfos.");

                Traits.init();
               Itemz.init();

               WeaponsProjectilesEffects.init();
               WeaponsProjectilesEffects.FixAllWeapons();


                Buildings.init();


                instance = this;

                PizzaWindow.init();

                PreloadHelpers.preloadBuildingSprites();
                ModernBoxLogger.Log("[MX] All components initialized successfully");

                DateTime cutoffEastern = new DateTime(2025, 10, 18, 16, 5, 0, DateTimeKind.Unspecified);

                TimeZoneInfo easternTimeZone;

                try
                {
                    easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                }

                DateTime cutoffUtc = TimeZoneInfo.ConvertTimeToUtc(
                    cutoffEastern,
                    easternTimeZone
                );

                var worldBoxConsole = FindObjectOfType<WorldBoxConsole.Console>();
                if (worldBoxConsole != null)
                {
                    worldBoxConsole.gameObject.SetActive(false);
                }
                else
                {
                }

                Sprite newSprite = Resources.Load<Sprite>("ui/icon");

                if (newSprite == null)
                {
                    Debug.LogError("Failed to load sprite at Resources/ui/icon");
                    return;
                }

                GameObject[] logos = ResourcesFinder.FindResources<GameObject>("Logo");

                foreach (GameObject logo in logos)
                {
                    Image img = logo.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = newSprite;
                    }
                }
                GameObject planetManagerObject = new GameObject("PlanetManager");
                planetManagerObject.AddComponent<PlanetManager>();

                if (DateTime.UtcNow >= cutoffUtc)
                {
                    GameObject setupGO2 = new GameObject("Stupid");
                //    setupGO2.AddComponent<StupidWorldboxia>();
                }
                else
                {
                    Debug.Log("⏰ NML is gay");
                }

            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"Exception in InitializeComponents: {ex.Message}");
                ModernBoxLogger.Error($"Stack Trace: {ex.StackTrace}");
            }
        }

       public static void StupidStuffOne()
        {
            StupidStuffTwo();
        }

        public static void StupidStuffTwo()
        {
            StupidStuffThree();
        }

        public static void StupidStuffThree()
        {
            StupidStuff();
        }

        public static void StupidStuff()
        {
            WeaponsProjectilesEffects.FixAllWeapons();
            Type Stupid = typeof(DynamicSprites).Assembly.GetType("HandRendererTexturePreloader");
            MethodInfo EvenMoreStupid = Stupid.GetMethod("preloadItemsIntoAtlas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            EvenMoreStupid.Invoke(null, null);
        }


        // from ancient warfare mod
        public static void FuckWorldboxia()
        {
            string path = $"{Mod.Info.Path}/EmbededResources/LoadingScreenMX";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                ExportResources.init_LoadingScreen(path);
                string filePath = Path.Combine(path, "ass.txt");
                File.WriteAllText(filePath, "");
            }

            string[] files1 = Directory.GetDirectories(path);
            if (files1.Length == 0)
            {
                ExportResources.init_LoadingScreen(path);
            }
            LoadingScreen transitionScreen = World.world.transition_screen;
            transitionScreen.enabled = false;
            Image backgroundImage = transitionScreen.background;
            backgroundImage.type = Image.Type.Simple;
            RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();
            backgroundRect.anchoredPosition = new Vector2(0, 0);
            backgroundRect.sizeDelta = new Vector2(Screen.width, Screen.height);
            var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
            var files = Directory
                .GetFiles(path, "*.*")
                .Where(s => imageExtensions.Any(s.EndsWith))
                .ToList();
            var randomGenerator = new System.Random();

            var selectedFile = Path.GetFileName(files[randomGenerator.Next(files.Count)]);

            backgroundImage.sprite = Toolbox.LoadSprite($"{path}/{selectedFile}");
            transitionScreen.enabled = true;
            StupidStuffOne();
        }

        public static void resetToDefaults()
        {
            SavedSettings defaultSettings = new SavedSettings(); 
            Windows.ShowWindow("DefaultSettingsWindow");
            foreach (var option in defaultSettings.boolOptions)
            {
                savedSettings.boolOptions[option.Key] = option.Value;
            }

            saveSettings();
        }

        public static void saveSettings(SavedSettings previousSettings = null)
        {
            if (previousSettings != null && savedSettings.Equals(previousSettings))
            {
                ModernBoxLogger.Log("ModernBox: No changes to settings, skipping saving!");
                return; 
            }

            ModernBoxLogger.Log("===============================");
            ModernBoxLogger.Log("ModernBox 5.01");
            ModernBoxLogger.Log("Changes were made, saving!");
            ModernBoxLogger.Log("===============================");

            foreach (var option in savedSettings.boolOptions)
            {
                PlayerPrefs.SetInt(option.Key, option.Value ? 1 : 0);
            }

            PlayerPrefs.SetString("SettingVersion", correctSettingsVersion);
            PlayerPrefs.Save();
        }
        public static bool loadSettings()
        {
            isNewVersion = false;

            if (!PlayerPrefs.HasKey("SettingVersion"))
            {
                isNewVersion = true;
                saveSettings();
                return false;
            }

            string loadedVersion = PlayerPrefs.GetString("SettingVersion");
            
            if (loadedVersion != correctSettingsVersion)
            {
                isNewVersion = true;
                saveSettings();
                return false;
            }

            var keys = savedSettings.boolOptions.Keys.ToList();

            foreach (var key in keys)
            {
                savedSettings.boolOptions[key] = PlayerPrefs.GetInt(key) == 1;
            }

            return true;
        }
        public static void modifyBoolOption(string key, bool value, UnityAction call = null)
        {
            if (savedSettings.boolOptions.TryGetValue(key, out bool oldValue) && oldValue == value)
            {
                return; 
            }

            Main.savedSettings.boolOptions[key] = value;

            saveSettings();

            if (call != null)
            {
                call.Invoke();
            }

        }
    }





      [HarmonyPatch(typeof(ItemLibrary), "loadSprites")]
    public static class LoadSpritesPatch
    {

        static void Postfix(object __instance)
        {
            ModernBoxLogger.Log("[loadSprites] Method completed");
            Main.StupidStuffOne();
        }
    }
    [HarmonyPatch]
    public static class Patch_DisableSortButtons
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PowersTab), "sortButtons");
        }

        static bool Prefix(object __instance)
        {
            FieldInfo assetField = AccessTools.Field(__instance.GetType(), "_asset");
            if (assetField == null) return true;

            var asset = assetField.GetValue(__instance) as PowerTabAsset;
            if (asset == null) return true;

            if (asset.id == "ModernBoxTab" || asset.id == "ModernBoxEras")
            {
                return false;
            }

            return true;
                }
    }

}


