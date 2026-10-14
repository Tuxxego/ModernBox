using System;
using System.Collections;
using HarmonyLib;
using NCMS;
using NeoModLoader.api;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    [ModEntry]
    internal sealed class ModernBoxMod : BasicMod<ModernBoxMod>
    {
        private const string HostName = "ModernBoxM2Runtime";
        private const string CommunityUrl = "https://gamebanana.com/mods/462076";
        internal static string ModFolder { get; private set; }

        public override string GetUrl()
        {
            return CommunityUrl;
        }

        protected override void OnModLoad()
        {
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

        internal void Begin()
        {
            if (_started) return;
            _started = true;
            Instance = this;
            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            // Resource sprites load a bit after the assets, wait for them.
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
                foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(ModernBoxMod).Assembly))
                {
                    if (!type.IsDefined(typeof(HarmonyPatch), true)) continue;
                    try { _harmony.CreateClassProcessor(type).Patch(); }
                    catch (Exception e) { ModernBoxDiagnostics.Error("Patch " + type.Name + " failed: " + e); }
                }
                ModernBoxUi.BeginCreate(this);
                Ready = true;
                Debug.Log("[ModernBox] loaded (" + ContentRegistry.Summary + ")");
            }
            catch (Exception exception)
            {
                _failed = true;
                _failure = exception.Message;
                ModernBoxDiagnostics.Error("Failed to load: " + exception);
            }
        }

        private static bool ResourceSpritesReady()
        {
            ResourceAsset template = AssetManager.resources == null ? null : AssetManager.resources.get("common_metals");
            return template != null && template.gameplay_sprites != null && template.gameplay_sprites.Length > 0 && template.gameplay_sprites[0] != null;
        }

        private void Update()
        {
            if (!Ready || World.world == null) return;
            InvasionService.Update(Time.deltaTime);
        }

        private void OnGUI()
        {
            if (_failed) GUI.Box(new Rect(10f, 10f, 620f, 70f), "ModernBox failed to load\n" + _failure);
        }

        private void OnDestroy()
        {
            BombService.Clear();
            if (_harmony != null) _harmony.UnpatchSelf();
            if (Instance == this) Instance = null;
        }
    }
}
