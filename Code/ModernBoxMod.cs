using System;
using System.Collections;
using System.IO;
using HarmonyLib;
using NCMS;
using NeoModLoader.api;
using UnityEngine;
using UnityEngine.Networking;

namespace ModernBoxM2Rewrite
{
    [ModEntry]
    internal sealed class ModernBoxMod : BasicMod<ModernBoxMod>
    {
        private const string HostName = "ModernBoxM2RewriteRuntime";
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
        private bool _showDiagnostics;
        private Rect _diagnosticsRect = new Rect(210f, 100f, 640f, 470f);
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
                   AssetManager.biome_library == null || AssetManager.top_tiles == null ||
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
                ModernBoxDiagnostics.Info("M2 core rewrite initialized for WorldBox 0.51.2 build 558.");
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
            M2LegacyBehaviorService.Update(Time.deltaTime);
            InvasionService.Update(Time.deltaTime);
            BombService.Update();
        }

        internal void OpenDiagnostics() { _showDiagnostics = true; }

        private void OnGUI()
        {
            if (_failed) GUI.Box(new Rect(10f, 10f, 620f, 70f), "ModernBox M2 Rewrite failed to initialize\n" + _failure);
            if (_showDiagnostics) _diagnosticsRect = GUI.Window(51023, _diagnosticsRect, DrawDiagnosticsWindow, "ModernBox Diagnostics");
        }

        private void DrawDiagnosticsWindow(int id)
        {
            GUILayout.Label("Registered content: " + ContentRegistry.Summary);
            GUILayout.Label("Bomb processing: exact-radius multi-frame jobs (pending: " + BombService.PendingJobs + ")");
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
            BombService.Clear();
            if (_harmony != null) _harmony.UnpatchSelf();
            if (Instance == this) Instance = null;
        }
    }
}

