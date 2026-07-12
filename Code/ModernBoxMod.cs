using System;
using System.Collections;
using System.IO;
using HarmonyLib;
using NCMS;
using NeoModLoader.api;
using UnityEngine;
using UnityEngine.Networking;

namespace ModernBoxRewrite
{
    [ModEntry]
    internal sealed class ModernBoxMod : BasicMod<ModernBoxMod>
    {
        private const string HostName = "ModernBoxM1RewriteRuntime";
        private const string CommunityUrl = "https://gamebanana.com/mods/462076";
        internal static string ModFolder { get; private set; }

        public override string GetUrl()
        {
            return CommunityUrl;
        }

        protected override void OnModLoad()
        {
            // The override above is required on NML 1.2.0.1 because its manifest
            // constructor does not copy RepoUrl into the live declaration.
            ModFolder = GetDeclaration().FolderPath;
            ModernBoxSettings.LoadAndMigrate();
            GameObject host = GameObject.Find(HostName);
            if (host == null)
            {
                host = new GameObject(HostName);
                UnityEngine.Object.DontDestroyOnLoad(host);
            }
            ModernBoxRuntime runtime = host.GetComponent<ModernBoxRuntime>();
            if (runtime == null) runtime = host.AddComponent<ModernBoxRuntime>();
            runtime.Begin();
            LogInfo("Persistent rewrite runtime host created.");
        }
    }

    internal sealed class ModernBoxRuntime : MonoBehaviour
    {
        internal static ModernBoxRuntime Instance { get; private set; }
        internal bool Ready { get; private set; }

        private Harmony _harmony;
        private bool _started;
        private bool _failed;
        private string _failure;
        private bool _showInfo;
        private bool _showSettings;
        private bool _showDiagnostics;
        private Rect _infoRect = new Rect(120f, 90f, 520f, 430f);
        private Rect _settingsRect = new Rect(160f, 80f, 560f, 560f);
        private Rect _diagnosticsRect = new Rect(210f, 100f, 640f, 470f);
        private Vector2 _settingsScroll;
        private Vector2 _diagnosticsScroll;
        private AudioSource _audioSource;

        internal void Begin()
        {
            if (_started) return;
            _started = true;
            Instance = this;
            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            // ResourceLibrary loads its ground sprites after its assets are created.
            // Registering custom resources before that second phase leaves the
            // vanilla sprite template empty and aborts the entire mod startup.
            while (AssetManager.actor_library == null || AssetManager.buildings == null || AssetManager.powers == null ||
                   AssetManager.resources == null || AssetManager.dynamic_sprites_library == null ||
                   DynamicSpritesLibrary.items == null || AssetManager.world_log_library == null || !ResourceSpritesReady())
                yield return null;
            yield return null;
            try
            {
                string conflict = ModernBoxDiagnostics.FindAssetConflict();
                if (!string.IsNullOrEmpty(conflict)) throw new InvalidOperationException(conflict);
                ContentRegistry.RegisterAll();
                _harmony = new Harmony(ModernBoxCatalog.HarmonyId);
                _harmony.PatchAll(typeof(ModernBoxMod).Assembly);
                ModernBoxUi.BeginCreate(this);
                Ready = true;
                ModernBoxDiagnostics.Info("Clean rewrite initialized for WorldBox 0.51.2 build 719.");
                Debug.Log("[ModernBox Rewrite] Initialization complete: " + ContentRegistry.Summary);
                if (ModernBoxSettings.Get("StartupAudio")) StartCoroutine(PlayStartupAudio());
            }
            catch (Exception exception)
            {
                _failed = true;
                _failure = exception.ToString();
                ModernBoxDiagnostics.Error("Initialization failed: " + exception);
            }
        }

        private static bool ResourceSpritesReady()
        {
            ResourceAsset template = AssetManager.resources == null ? null : AssetManager.resources.get("common_metals");
            return template != null && template.gameplay_sprites != null && template.gameplay_sprites.Length > 0 && template.gameplay_sprites[0] != null;
        }

        private IEnumerator PlayStartupAudio()
        {
            string path = Path.Combine(ModernBoxMod.ModFolder, "file.mp3");
            if (!File.Exists(path)) yield break;
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri, AudioType.MPEG))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    ModernBoxDiagnostics.Warn("Startup audio could not be loaded: " + request.error);
                    yield break;
                }
                if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
                _audioSource.volume = 0.35f;
                _audioSource.Play();
            }
        }

        private void Update()
        {
            if (!Ready || World.world == null) return;
            ProductionService.Update(Time.deltaTime);
            BombService.Update();
        }

        internal void OpenInfo() { _showInfo = true; }
        internal void OpenSettings() { _showSettings = true; }
        internal void OpenDiagnostics() { _showDiagnostics = true; }

        private void OnGUI()
        {
            if (_failed) GUI.Box(new Rect(10f, 10f, 620f, 70f), "ModernBox M1 Rewrite failed to initialize\n" + _failure);
            if (_showInfo) _infoRect = GUI.Window(51021, _infoRect, DrawInfoWindow, "ModernBox M1 Rewrite");
            if (_showSettings) _settingsRect = GUI.Window(51022, _settingsRect, DrawSettingsWindow, "ModernBox Settings");
            if (_showDiagnostics) _diagnosticsRect = GUI.Window(51023, _diagnosticsRect, DrawDiagnosticsWindow, "ModernBox Diagnostics");
        }

        private void DrawInfoWindow(int id)
        {
            GUILayout.Label("ModernBox M1 10.1.0 — clean WorldBox 0.51.2 rewrite");
            GUILayout.Space(8f);
            GUILayout.Label("Original concept and content: Tuxxego and the original ModernBox contributors.");
            GUILayout.Label("This edition uses current strongly typed NML/game APIs and no reflection compatibility layer.");
            GUILayout.Space(10f);
            GUILayout.Label("Human progression: Urban 50/16, Industrial 75/18, Modern 100/20, Advanced 130/23, Strategic 170/26, Nuclear 220/30.");
            GUILayout.Label("Civilian modern buildings are unlimited. Each factory, ModernBarracks, and MissileSilo is one per city.");
            GUILayout.Label("Huge bombs retain exact M1 radii and finish over multiple frames.");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Diagnostics and credits")) _showDiagnostics = true;
            if (GUILayout.Button("Close")) _showInfo = false;
            GUI.DragWindow();
        }

        private void DrawSettingsWindow(int id)
        {
            _settingsScroll = GUILayout.BeginScrollView(_settingsScroll);
            GUILayout.Label("Kingdom production");
            foreach (string key in ModernBoxSettings.FactoryKeys) DrawSetting(key);
            GUILayout.Space(8f);
            GUILayout.Label("Equipment, names, and extras");
            foreach (string key in ModernBoxSettings.FeatureKeys) DrawSetting(key);
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset defaults")) ModernBoxSettings.Reset();
            if (GUILayout.Button("Close")) _showSettings = false;
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private static void DrawSetting(string key)
        {
            bool oldValue = ModernBoxSettings.Get(key);
            GUILayout.BeginVertical("box");
            bool newValue = GUILayout.Toggle(oldValue, SettingTitle(key) + (oldValue ? "  [ON]" : "  [OFF]"));
            GUILayout.Label(SettingDescription(key));
            GUILayout.EndVertical();
            if (oldValue != newValue) ModernBoxSettings.Set(key, newValue);
        }

        private static string SettingTitle(string key)
        {
            switch (key)
            {
                case "SoldierOption": return "Modern Militaries";
                case "HumveeOption": return "Humvee Factories";
                case "TankOption": return "Tank Factories";
                case "AirshipOption": return "Airship Factories";
                case "HeliOption": return "Helicopter Factories";
                case "DronesOption": return "Drone Factories";
                case "RailgunOption": return "Railgun Factories";
                case "F22Option": return "F22 Factories";
                case "F55Option": return "F55 Factories";
                case "GunshipOption": return "Gunship Factories";
                case "BoiOption": return "Missile System Factories";
                case "MIRVBomberOption": return "Strategic Air Factories";
                case "NukeOption": return "Nuclear Missile Silos";
                case "MIRVOption": return "Nuclear Weapons";
                case "GunOption": return "Modern Guns";
                case "PipeGunOption": return "Pipe Guns";
                case "CyberwareOption": return "Cyberware";
                case "DrugsOption": return "Drugs";
                case "IdeologiesOption": return "Ideologies";
                case "namesOption": return "Modern Human Names";
                case "othernamesOption": return "Modern Names for Other Races";
                case "StartupAudio": return "Startup Audio";
                case "DeveloperDiagnostics": return "Verbose Developer Diagnostics";
                default: return key;
            }
        }

        private static string SettingDescription(string key)
        {
            if (key.EndsWith("Option", StringComparison.Ordinal) && Array.IndexOf(ModernBoxSettings.FactoryKeys, key) >= 0)
                return "Controls whether the matching factory produces its military unit.";
            switch (key)
            {
                case "NukeOption": return "Controls whether MissileSilos fire nuclear missiles.";
                case "MIRVOption": return "Controls craftable MIRV weapons. MissileSystem vehicles always use their built-in MIRV launcher. Off by default.";
                case "GunOption": return "Allows modern firearms to be developed in eligible human cities.";
                case "PipeGunOption": return "Allows lower-tier pipe weapons to be developed.";
                case "CyberwareOption": return "Allows Sandevistan and TurboBooster development.";
                case "DrugsOption": return "Allows Meth and Crack accessories to be developed.";
                case "IdeologiesOption": return "Gives every human, orc, elf, and dwarf one ModernBox ideology by default and preserves inheritance.";
                case "namesOption": return "Uses the original M1 modern human name generator.";
                case "othernamesOption": return "Uses M1 name generators for orcs, elves, and dwarves.";
                case "StartupAudio": return "Plays the bundled local M1 startup audio on launch.";
                case "DeveloperDiagnostics": return "Writes bounded informational diagnostics to the console.";
                default: return "ModernBox setting.";
            }
        }

        private void DrawDiagnosticsWindow(int id)
        {
            GUILayout.Label("Registered content: " + ContentRegistry.Summary);
            GUILayout.Label("Pending bomb jobs: " + BombService.PendingJobs);
            GUILayout.Label("Factory cycles: " + ProductionService.CompletedCycles);
            GUILayout.Label("Missile-silo launches: " + SiloLaunchEvents.LaunchCount);
            _diagnosticsScroll = GUILayout.BeginScrollView(_diagnosticsScroll);
            foreach (string line in ModernBoxDiagnostics.Lines) GUILayout.Label(line);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Run asset validation")) ModernBoxDiagnostics.ValidateAssets();
            if (GUILayout.Button("Close")) _showDiagnostics = false;
            GUI.DragWindow();
        }

        private void OnDestroy()
        {
            if (_harmony != null) _harmony.UnpatchSelf();
            if (Instance == this) Instance = null;
        }
    }
}
