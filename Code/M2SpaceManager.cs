using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModernBoxM2Rewrite
{
    /// Port of M2's SpaceManager: hides the world, shows the star map, and moves the
    /// player between planet worlds that are saved under persistentDataPath/ModernBox.
    internal static class M2SpaceManager
    {
        private static M2SpaceHost host;
        private static GameObject spaceGameObject;
        private static bool isSpaceEnabled;
        private static Action pendingAction;

        internal static bool IsSpaceEnabled => isSpaceEnabled;

        internal static void EnsureHost()
        {
            if (host != null) return;
            GameObject hostObject = new GameObject("ModernBoxM2Space");
            UnityEngine.Object.DontDestroyOnLoad(hostObject);
            host = hostObject.AddComponent<M2SpaceHost>();
            M2PlanetManager.Load();
        }

        private static int spaceLayer = -1;

        // The game world stays loaded underneath, so the space camera only renders its own layer.
        internal static int SpaceLayer
        {
            get
            {
                if (spaceLayer >= 0) return spaceLayer;
                spaceLayer = 31;
                for (int i = 31; i >= 8; i--)
                {
                    if (string.IsNullOrEmpty(LayerMask.LayerToName(i))) { spaceLayer = i; break; }
                }
                return spaceLayer;
            }
        }

        internal static void EnableSpace()
        {
            EnsureHost();
            if (isSpaceEnabled) return;
            try
            {
                M2SpaceAudio.MuteGame(true);
                M2SpaceAudio.PlayRoar();
                HideWorld();

                spaceGameObject = new GameObject("SpaceGameObject");
                UnityEngine.Object.DontDestroyOnLoad(spaceGameObject);
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.transform.SetParent(spaceGameObject.transform, false);
                cameraObject.transform.position = new Vector3(0, 0, -10);
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 20;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.depth = 100;
                camera.cullingMask = 1 << SpaceLayer;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 100f;
                M2StarMap starMap = spaceGameObject.AddComponent<M2StarMap>();
                starMap.mainCamera = camera;
                isSpaceEnabled = true;
                Debug.Log("[ModernBox] star map opened");
            }
            catch (Exception ex)
            {
                ModernBoxDiagnostics.Error("Star map failed to open: " + ex);
                isSpaceEnabled = true;
                DisableSpace();
            }
        }

        internal static void DisableSpace()
        {
            if (!isSpaceEnabled) return;
            try
            {
                // Untag and drop the space camera now; Destroy only runs at the end of the
                // frame, and the game's own Camera.main lookups run before that.
                if (spaceGameObject != null)
                {
                    foreach (Camera camera in spaceGameObject.GetComponentsInChildren<Camera>(true))
                    {
                        camera.tag = "Untagged";
                        camera.enabled = false;
                    }
                    UnityEngine.Object.Destroy(spaceGameObject);
                }
                spaceGameObject = null;
                Debug.Log("[ModernBox] closing star map");
                ShowWorld();
                RestoreMainCamera();
                M2SpaceAudio.StopRoar();
                M2SpaceAudio.MuteGame(false);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModernBox Space] Exception in DisableSpace: " + ex.Message);
            }
            isSpaceEnabled = false;
            Debug.Log("[ModernBox] star map closed");
        }

        // IMGUI buttons fire inside OnGUI; world loading is started on the next Update instead.
        internal static void RequestDisableSpace()
        {
            pendingAction = DisableSpace;
        }

        internal static void RequestPlanetVisit(string planetName, string planetType, string planetSize, bool hasFauna)
        {
            pendingAction = () => GeneratePlanet(planetName, planetType, planetSize, hasFauna);
        }

        internal static void RunPending()
        {
            Action action = pendingAction;
            pendingAction = null;
            if (action == null) return;
            try { action(); }
            catch (Exception ex) { ModernBoxDiagnostics.Error("Space action failed: " + ex); }
        }

        // M2 switched off every root object here. Re-enabling the game's roots crashes
        // 0.51.2, so only the cameras and UI canvases are turned off and the game is paused.
        private static readonly List<Behaviour> hiddenBehaviours = new List<Behaviour>();
        private static bool wasPaused;
        private static bool wasLocked;

        private static bool IsOurs(Component component)
        {
            if (component == null) return true;
            Transform root = component.transform.root;
            return (spaceGameObject != null && root == spaceGameObject.transform) || (host != null && root == host.transform);
        }

        private static void HideWorld()
        {
            hiddenBehaviours.Clear();
            wasPaused = Config.paused;
            wasLocked = Config.lockGameControls;
            Config.paused = true;
            Config.lockGameControls = true;
            foreach (Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>())
            {
                if (!camera.enabled || IsOurs(camera)) continue;
                camera.enabled = false;
                hiddenBehaviours.Add(camera);
            }
            foreach (Canvas canvas in UnityEngine.Object.FindObjectsOfType<Canvas>())
            {
                if (!canvas.enabled || !canvas.isRootCanvas || IsOurs(canvas)) continue;
                canvas.enabled = false;
                hiddenBehaviours.Add(canvas);
            }
        }

        private static void ShowWorld()
        {
            foreach (Behaviour behaviour in hiddenBehaviours)
            {
                if (behaviour != null) behaviour.enabled = true;
            }
            hiddenBehaviours.Clear();
            Config.paused = wasPaused;
            Config.lockGameControls = wasLocked;
        }

        // If the world camera did not come back (for example after an exception while
        // hiding), switch the game's tagged camera back on so Camera.main is never null.
        private static void RestoreMainCamera()
        {
            if (Camera.main != null) return;
            foreach (Camera camera in Resources.FindObjectsOfTypeAll<Camera>())
            {
                if (camera == null || !camera.gameObject.scene.IsValid() || !camera.CompareTag("MainCamera")) continue;
                camera.gameObject.SetActive(true);
                camera.enabled = true;
                Debug.Log("[ModernBox] restored the world camera after the star map");
                return;
            }
        }

        internal static void GeneratePlanet(string planetName, string planetType, string planetSize, bool hasFauna)
        {
            if (World.world == null || SmoothLoader.isLoading())
            {
                Debug.LogWarning("[ModernBox Space] The world is busy, landing aborted.");
                return;
            }

            M2SpacePaths.EnsureRoot();
            int planetCount = M2SpacePaths.ReadPlanetCount() + 1;
            File.WriteAllText(M2SpacePaths.PlanetCountFile, planetCount.ToString());

            string currentPlanetName = M2PlanetManager.GetCurrentPlanet();
            string saveDirectory = FindStarFolderOf(currentPlanetName);
            if (saveDirectory == null)
            {
                Debug.LogError($"[ModernBox Space] Planet {currentPlanetName} not found in any star file.");
                return;
            }

            DisableSpace();
            SaveManager.saveWorldToDirectory(Path.Combine(saveDirectory, M2SpacePaths.SafeName(currentPlanetName)));

            M2PlanetManager.SetCurrentPlanet(planetName);
            M2PlanetManager.SetCurrentPlanetType(planetType);
            string nextPlanetLoadDirectory = FindPlanetSave(planetName);
            if (nextPlanetLoadDirectory != null)
            {
                ScrollWindow.hideAllEvent();
                World.world.save_manager.loadWorld(nextPlanetLoadDirectory);
                M2PlanetManager.ShowTouchdownGUI(planetType);
                return;
            }

            if (!M2PlanetGenerator.PreparePlanet(planetName, planetType, planetSize, hasFauna)) return;
            ScrollWindow.hideAllEvent();
            MapBox.instance.generateNewMap();
            M2PlanetManager.ShowTouchdownGUI(planetType);
        }

        internal static string FindStarFolderOf(string planetName)
        {
            if (string.IsNullOrEmpty(planetName) || !Directory.Exists(M2SpacePaths.Root)) return null;
            foreach (string starFile in Directory.GetFiles(M2SpacePaths.Root, "star.json", SearchOption.AllDirectories))
            {
                string text;
                try { text = File.ReadAllText(starFile); }
                catch (Exception) { continue; }
                if (text.IndexOf(planetName, StringComparison.OrdinalIgnoreCase) < 0) continue;
                M2StarData star = M2StarData.TryLoad(starFile);
                if (star != null && star.planetInfo.Any(p => p != null && string.Equals(p.name, planetName, StringComparison.OrdinalIgnoreCase)))
                    return Path.GetDirectoryName(starFile);
            }
            return null;
        }

        private static string FindPlanetSave(string planetName)
        {
            string folderName = M2SpacePaths.SafeName(planetName);
            foreach (string directory in Directory.GetDirectories(M2SpacePaths.Root, "*", SearchOption.AllDirectories))
            {
                if (!Path.GetFileName(directory).Equals(folderName, StringComparison.OrdinalIgnoreCase)) continue;
                if (SaveManager.doesSaveExist(SaveManager.folderPath(directory))) return directory;
            }
            return null;
        }

        /// TUDDS: wipes every planet, galaxy and journey file M2 keeps.
        internal static void DeleteBomb()
        {
            string spaceBoxPath = M2SpacePaths.Root;
            if (Directory.Exists(spaceBoxPath))
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(spaceBoxPath);
                    foreach (FileInfo file in dir.GetFiles()) file.Delete();
                    foreach (DirectoryInfo subDir in dir.GetDirectories()) subDir.Delete(true);
                }
                catch (IOException e)
                {
                    Debug.LogError($"[ModernBox Space] Error while deleting contents of 'ModernBox': {e.Message}");
                }
            }
            foreach (string filePath in new[] { M2SpacePaths.ActiveGalaxyFile, M2SpacePaths.JourneyTrackerFile, M2SpacePaths.VisitedPlanetsFile })
            {
                try { if (File.Exists(filePath)) File.Delete(filePath); }
                catch (IOException e) { Debug.LogError($"[ModernBox Space] Error while deleting {Path.GetFileName(filePath)}: {e.Message}"); }
            }
            M2PlanetManager.Load();
        }
    }

    internal sealed class M2SpaceHost : MonoBehaviour
    {
        private void Update()
        {
            M2SpaceManager.RunPending();
        }

        private void OnGUI()
        {
            M2PlanetManager.DrawGui();
        }
    }

    /// Port of M2's PlanetManager: remembers the planet you stand on and shows the landing log.
    internal static class M2PlanetManager
    {
        private static string currentPlanetName;
        private static string currentPlanetType;
        private static bool showTouchdownWindow;
        private static bool showNextWindow;
        private static bool showFinalWindow;
        private static string currentWindowTitle;
        private static string currentWindowDescription;

        private static string TypeFile => Path.Combine(M2SpacePaths.Root, "currentPlanetType.txt");

        internal static void Load()
        {
            currentPlanetName = ReadFile(M2SpacePaths.CurrentPlanetFile);
            currentPlanetType = ReadFile(TypeFile);
        }

        private static string ReadFile(string path)
        {
            try { return File.Exists(path) ? File.ReadAllText(path) : null; }
            catch (Exception) { return null; }
        }

        internal static string GetCurrentPlanet() => currentPlanetName;
        internal static string GetCurrentPlanetType() => currentPlanetType;

        internal static void SetCurrentPlanet(string planetName)
        {
            currentPlanetName = planetName;
            M2SpacePaths.EnsureRoot();
            File.WriteAllText(M2SpacePaths.CurrentPlanetFile, planetName ?? string.Empty);
        }

        internal static void SetCurrentPlanetType(string planetType)
        {
            currentPlanetType = planetType;
            M2SpacePaths.EnsureRoot();
            File.WriteAllText(TypeFile, planetType ?? string.Empty);
        }

        internal static string FindParentStar()
        {
            string folder = M2SpaceManager.FindStarFolderOf(currentPlanetName);
            if (folder == null) return null;
            M2StarData star = M2StarData.TryLoad(Path.Combine(folder, "star.json"));
            return star != null ? star.name : Path.GetFileName(folder);
        }

        internal static void ShowTouchdownGUI(string planetType)
        {
            currentWindowTitle = "Landing Sequence Complete";
            currentWindowDescription = $"Mission Success: Your vessel has safely touched down on a {planetType}.";
            showTouchdownWindow = true;
            showNextWindow = false;
            showFinalWindow = false;
        }

        internal static void DrawGui()
        {
            if (!showTouchdownWindow && !showNextWindow && !showFinalWindow) return;
            if (SmoothLoader.isLoading()) return;
            Color color = GUI.color, background = GUI.backgroundColor, content = GUI.contentColor;
            GUI.color = Color.cyan;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f);
            GUI.contentColor = Color.green;
            Rect windowRect = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 350, 200);
            if (showTouchdownWindow) GUI.Window(0x4D3280, windowRect, TouchdownWindow, currentWindowTitle);
            else if (showNextWindow) GUI.Window(0x4D3281, windowRect, NextWindow, "Directive Alpha");
            else if (showFinalWindow) GUI.Window(0x4D3282, windowRect, FinalWindow, "Log Update");
            GUI.color = color;
            GUI.backgroundColor = background;
            GUI.contentColor = content;
        }

        private static void TouchdownWindow(int windowID)
        {
            GUILayout.Label(currentWindowDescription, LabelStyle());
            if (GUILayout.Button("Acknowledge", ButtonStyle()))
            {
                showTouchdownWindow = false;
                showNextWindow = true;
            }
        }

        private static void NextWindow(int windowID)
        {
            GUILayout.Label("Alert: Ensure planetary safety protocols. Scan for hostile alien organisms and hazardous conditions.", LabelStyle());
            if (GUILayout.Button("Proceed", ButtonStyle()))
            {
                showNextWindow = false;
                showFinalWindow = true;
            }
        }

        private static void FinalWindow(int windowID)
        {
            GUILayout.Label("System Update: The coordinates and data for your previous location have been archived securely.", LabelStyle());
            if (GUILayout.Button("Close", ButtonStyle())) showFinalWindow = false;
        }

        private static GUIStyle LabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14, wordWrap = true };
            style.normal.textColor = Color.green;
            return style;
        }

        private static GUIStyle ButtonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = 12 };
            style.normal.textColor = Color.cyan;
            style.hover.textColor = Color.white;
            style.hover.background = Texture2D.grayTexture;
            return style;
        }
    }

    /// Roar on entering space, game audio muted meanwhile (M2 played roar.wav through FMOD core).
    internal static class M2SpaceAudio
    {
        private static FMOD.Sound roarSound;
        private static FMOD.Channel roarChannel;
        private static bool hasRoar;
        private static bool muted;

        internal static void PlayRoar()
        {
            try
            {
                string path = Path.Combine(ModernBoxMod.ModFolder ?? string.Empty, "Sounds", "roar.ogg");
                if (!File.Exists(path)) return;
                FMOD.System core;
                if (RuntimeManager.StudioSystem.getCoreSystem(out core) != FMOD.RESULT.OK) return;
                FMOD.ChannelGroup master;
                core.getMasterChannelGroup(out master);
                if (core.createSound(path, FMOD.MODE.DEFAULT | FMOD.MODE.CREATESTREAM, out roarSound) != FMOD.RESULT.OK) return;
                if (core.playSound(roarSound, master, false, out roarChannel) != FMOD.RESULT.OK)
                {
                    roarSound.release();
                    return;
                }
                hasRoar = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ModernBox Space] Roar failed: " + ex.Message);
            }
        }

        internal static void StopRoar()
        {
            if (!hasRoar) return;
            hasRoar = false;
            try
            {
                roarChannel.stop();
                roarSound.release();
            }
            catch (Exception) { }
        }

        internal static void MuteGame(bool mute)
        {
            if (muted == mute) return;
            muted = mute;
            SetVca("vca:/Music", "volume_music", mute);
            SetVca("vca:/Sound Effects", "volume_sound_effects", mute);
            SetVca("vca:/UI", "volume_ui", mute);
        }

        private static void SetVca(string path, string option, bool mute)
        {
            try
            {
                FMOD.Studio.VCA vca = RuntimeManager.GetVCA(path);
                if (!vca.isValid()) return;
                vca.setVolume(mute ? 0f : PlayerConfig.getIntValue(option) / 100f);
            }
            catch (Exception) { }
        }
    }
}

namespace ModernBoxM2Rewrite
{
    // PixelDetector reads Camera.main without a null check; for a frame around the star
    // map closing there may be no main camera, which threw every frame in MapBox.Update.
    [HarmonyLib.HarmonyPatch(typeof(PixelDetector), nameof(PixelDetector.GetSpritePixelColorUnderMousePointer))]
    internal static class M2PixelDetectorCameraGuard
    {
        [HarmonyLib.HarmonyPrefix]
        private static bool Prefix(ref UnityEngine.Vector2Int pVector, ref bool __result)
        {
            if (UnityEngine.Camera.main != null) return true;
            pVector = new UnityEngine.Vector2Int(-1, -1);
            __result = false;
            return false;
        }
    }
}
