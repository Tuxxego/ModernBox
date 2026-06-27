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
        public SpaceManager SpaceManager;
        public PlanetGenerator PlanetGenerator;
        public AchievementManager AchievementManager;
        public LocalizationManager LocalizationManager;
        public BombUtilities testBombDebugger;
        public Bombs Bombs = new Bombs();
        public StatManager StatManager;
        public UnitTracker UnitTracker;
        public FirstTimeSetup FirstTimeSetup;
        public StupidWorldboxia StupidWorldboxia;
        private static string correctSettingsVersion = "5.01"; 
        public const string settingsKey = "MBoxSettings"; 
        public static bool isNewVersion;
        public static SavedSettings savedSettings = new SavedSettings();
        public static PizzaManager PizzaManager;
        private const bool EnableStartupAssetPreload = false;
        private const bool EnableRuntimeUiEffects = false;
        private const string PreferredModernBoxFolderName = "M5TrainsUpdateBeta";
        private bool modernBoxUiInitialized;

        private static readonly FieldInfo LocalizedTextLanguageField = AccessTools.Field(typeof(LocalizedTextManager), "language");
        private static readonly FieldInfo LocalizedTextDictionaryField = AccessTools.Field(typeof(LocalizedTextManager), "_localized_text");

        internal static string ModName
        {
            get { return Mod.Info.Name; }
        }

        void Awake()
        {
            try
            {
                if (ShouldSuppressDuplicateBootstrap())
                {
                    ModernBoxLogger.Log("[MX] Legacy ModernBox copy detected in StreamingAssets. Bootstrap suppressed in favor of Mods\\M5TrainsUpdateBeta.");
                    enabled = false;
                    return;
                }

                instance = this;
                ModernBoxLogger.Log("[MX] Main.Awake entered");
	            loadSettings();

                harmony = new Harmony("com.Tuxxego.MX");

                Assembly targetAssembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(targetAssembly);

                ModernBoxPrefs.Load();

            //    PowersTab tab = TabManager.CreateTab("ModernBox", "ModernBox", "Best Mod Ever", Resources.Load<Sprite>("ui/icon"));

                  ModernBoxLogger.Log("[M2] Space Manager set to lazy-load.");
        		AchievementManager = gameObject.AddComponent<AchievementManager>();
        		StatManager = gameObject.AddComponent<StatManager>();
                if (EnableRuntimeUiEffects)
                {
                    gameObject.AddComponent<YourMomIsFat>();
                }
        		UnitTracker = gameObject.AddComponent<UnitTracker>();
        		testBombDebugger = gameObject.AddComponent<BombUtilities>();
                PizzaManager = gameObject.AddComponent<PizzaManager>();
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

        private static bool ShouldSuppressDuplicateBootstrap()
        {
            try
            {
                string gameRoot = Directory.GetParent(Application.dataPath).FullName;
                string preferredModJson = Path.Combine(gameRoot, "Mods", PreferredModernBoxFolderName, "mod.json");
                return File.Exists(preferredModJson);
            }
            catch
            {
                return false;
            }
        }


        void Start()
        {
            InitializeEmbeddedTrainbox();
            StartCoroutine(InitializeModernBoxUiWhenReady());
            InitializeComponents();

        }

        private void InitializeEmbeddedTrainbox()
        {
            try
            {
                string modsRoot = Path.Combine(Application.streamingAssetsPath, "mods");
                string modFolder = Path.Combine(modsRoot, "M5TrainsUpdateBeta");
                if (!Directory.Exists(modFolder))
                {
                    modFolder = Path.Combine(modsRoot, "M5OceansPlusKaijuBox");
                }

                if (!Directory.Exists(modFolder) && Directory.Exists(modsRoot))
                {
                    modFolder = Directory.GetDirectories(modsRoot)
                        .FirstOrDefault(dir =>
                            File.Exists(Path.Combine(dir, "Code", "choochoo.cs")) &&
                            File.Exists(Path.Combine(dir, "Code", "Buttonz.cs")) &&
                            File.Exists(Path.Combine(dir, "mod.json")));
                }

                global::Trainbox.Main.Bootstrap(modFolder);
                ModernBoxLogger.Log("[MX] Embedded Trainbox bootstrap loaded.");
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[MX] Embedded Trainbox bootstrap failed: {ex.Message}");
                ModernBoxLogger.Error($"[MX] Embedded Trainbox bootstrap stack: {ex.StackTrace}");
            }
        }

        private IEnumerator InitializeModernBoxUiWhenReady()
        {
            if (modernBoxUiInitialized)
            {
                yield break;
            }

            const int maxFramesToWait = 600;
            int waited = 0;
            while (waited < maxFramesToWait)
            {
                GameObject buttonOther = TabBuilder.FindAllGameObjectsCreditToNikonForThisFunctionBTW("button_other");
                GameObject otherTab = TabBuilder.FindAllGameObjectsCreditToNikonForThisFunctionBTW("other");
                if (buttonOther != null && otherTab != null && otherTab.GetComponent<PowersTab>() != null)
                {
                    break;
                }

                waited++;
                yield return null;
            }

            BuildModernBoxTabsIfMissing();
            yield return null;

            try
            {
                ModernBoxLogger.Log("[MX] Initializing Buttonz...");
                Buttonz.Init();
                ModernBoxLogger.Log("[MX] Buttonz loaded!");
                modernBoxUiInitialized = true;
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[MX] Exception while initializing tabs/buttons: {ex.Message}");
                ModernBoxLogger.Error($"[MX] Stack Trace: {ex.StackTrace}");
            }
        }

        private static void BuildModernBoxTabsIfMissing()
        {
            EnsureTabBuilt("ModernBoxTab", "ModernBox", "Eras, Space, Politics, and more.", 128, false, true, null, "ui/icons/tabIconModernWarfare");
            EnsureTabBuilt("ModernBoxUnits", "ModernBox Units", "A BUNCH of guys to spawn!", 200, true, false, "ModernBoxTab", "ui/icons/warhamma");
            EnsureTabBuilt("ModernBoxBombs", "ModernBox Bombs", "A BUNCH of weapons made by foreign terrorists to spawn!", 200, true, false, "ModernBoxTab", "ui/icons/Overload");
            EnsureTabBuilt("ModernBoxEras", "ModernBox Eras", "Everything related to eras!", 200, true, false, "ModernBoxTab", "ui/icons/Industrial");
            EnsureTabBuilt("ModernBoxSpace", "ModernBox Space", "Explore the vastness of space!", 200, true, false, "ModernBoxTab", "Stars/Gravitonstar");
            EnsureTabBuilt("ModernBoxItems", "ModernBox Items", "Toggle specific item types.", 200, true, false, "ModernBoxTab", "ui/icons/firearm");
            EnsureTabBuilt("Tab_kaiju", "Kaiju", "Kaiju species and other crazy stuff", 200, true, false, "ModernBoxTab", "ui/icons/Godzilla");
        }

        private static void EnsureTabBuilt(string tabId, string tabName, string tabDescription, int positionX, bool isAfrican, bool toolbarVisible, string returnTabId, string iconPath)
        {
            if (GameObjects.FindEvenInactive(tabId) != null)
            {
                return;
            }

            TabBuilder builder = new TabBuilder()
                .SetTabID(tabId)
                .SetName(tabName)
                .SetDescription(tabDescription)
                .SetPosition(positionX)
                .SetToolbarButtonVisible(toolbarVisible)
                .SetIcon(iconPath);

            if (isAfrican)
            {
                builder.isAfrican(true);
            }

            if (!string.IsNullOrEmpty(returnTabId))
            {
                builder.SetRepeatPressReturnTab(returnTabId);
            }

            builder.Build();
        }

        void InitializeComponents()
        {
            try
            {
                modsResources = Reflection.GetField(typeof(ResourcesPatch), null, "modsResources") as Dictionary<string, UnityEngine.Object>;
                Traits.init();
               Itemz.init();

               WeaponsProjectilesEffects.init();


                Buildings.init();

                Kaiju.init();
                new KaijuUI().init();


                instance = this;

                PizzaWindow.init();

                if (EnableStartupAssetPreload)
                {
                    PreloadHelpers.preloadBuildingSprites();
                }
                ModernBoxLogger.Log("[MX] All components initialized successfully");

                DateTime cutoffEastern = new DateTime(2025, 10, 18, 16, 5, 0, DateTimeKind.Unspecified);
                DateTime cutoffUtc = TimeZoneInfo.ConvertTimeToUtc(
                    cutoffEastern,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
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
            if (!EnableStartupAssetPreload)
            {
                return;
            }

            WeaponsProjectilesEffects.FixAllWeapons();
            Type Stupid = typeof(DynamicSprites).Assembly.GetType("HandRendererTexturePreloader");
            if (Stupid == null)
            {
                return;
            }

            MethodInfo EvenMoreStupid = Stupid.GetMethod("preloadItemsIntoAtlas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (EvenMoreStupid == null)
            {
                return;
            }

            EvenMoreStupid.Invoke(null, null);
        }





    [HarmonyPatch(typeof(LocalizedTextManager), "getText", new Type[] { typeof(string), typeof(Text), typeof(bool) })]
    public static class LocalizedTextManager_GetTextSafePatch
    {
        static bool Prefix(string pKey, ref string __result)
        {
            if (string.IsNullOrWhiteSpace(pKey))
            {
                __result = string.Empty;
                return false;
            }

            LocalizedTextManager manager = LocalizedTextManager.instance;
            if (manager == null)
            {
                __result = pKey;
                return false;
            }

            string language = LocalizedTextLanguageField?.GetValue(manager) as string;
            if (language == "boat")
            {
                __result = LocalizedTextManager.transformToBoat(pKey);
                return false;
            }

            if (language == "keys")
            {
                __result = LocalizedTextManager.transformToKeys(pKey);
                return false;
            }

            Dictionary<string, string> localizedTexts = LocalizedTextDictionaryField?.GetValue(manager) as Dictionary<string, string>;
            if (localizedTexts != null && localizedTexts.TryGetValue(pKey, out string translation))
            {
                __result = translation;
            }
            else
            {
                if (pKey.IndexOf("_placeholder", StringComparison.Ordinal) >= 0)
                {
                    __result = string.Empty;
                }
                else
                {
                    __result = pKey;
                }
            }

            if (pKey.StartsWith("world_law_", StringComparison.Ordinal))
            {
                __result = __result.Replace("\n\n", "\n");
            }

            return false;
        }
    }





        // from ancient warfare mod
        public static void FuckWorldboxia()
        {
            // Disabled: do not override or load custom loading screen images.
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
                int defaultValue = savedSettings.boolOptions[key] ? 1 : 0;
                savedSettings.boolOptions[key] = PlayerPrefs.GetInt(key, defaultValue) == 1;
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


