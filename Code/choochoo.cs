using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using NCMS.Utils;
using NeoModLoader.api;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Trainbox {
    internal static class Main {
        internal const string HarmonyId = "trainbox.worldbox.mod";
        internal const string RailTileId = "trainbox_track";
        internal const string TrainAssetPrefix = "trainbox_train_";
        internal const string RoadCarAssetId = "trainbox_road_car";
        internal const string TabId = "trainbox_tab";
        internal const string ModernRoadTileId = "trainbox_modern_road";
        internal const string TrackDropId = "trainbox_track_drop";
        internal const string TrackPowerId = "trainbox_track_power";
        internal const string RoadDropId = "trainbox_road_drop";
        internal const string RoadPowerId = "trainbox_road_power";
        internal const string CarEffectDropId = "trainbox_car_effect_drop";
        internal const string CarEffectPowerId = "trainbox_car_effect_power";
        internal const string StopDropId = "trainbox_stop_drop";
        internal const string StopPowerId = "trainbox_stop_power";
        internal const string SpawnPowerId = "trainbox_spawn_power";
        internal const string RemovePowerId = "trainbox_remove_power";
        internal const string ModernRoadsPowerId = "trainbox_modern_roads_power";
        internal const string AutoRailsPowerId = "trainbox_auto_rails_power";

        internal static string ModFolderPath { get; private set; }
        private static Harmony _harmony;
        private static bool _bootstrapped;

        internal static void Bootstrap(string modFolderPath = null) {
            if (_bootstrapped) {
                return;
            }

            _bootstrapped = true;
            ModFolderPath = ResolveModFolder(modFolderPath);
            TrainVisuals.Initialize();
            RailTileRegistry.Register();
            RoadTrafficSystem.Register();
            TrainAssets.BootstrapKnownTrainAssets();
            RoadCarAssets.Bootstrap();
            TrainPowers.Register();
            TrainboxDebug.Log("Embedded Trainbox bootstrap enabled.");
            _harmony = new Harmony(HarmonyId);
            PatchTrainboxHarmony();
        }

        private static string ResolveModFolder(string modFolderPath) {
            if (!string.IsNullOrWhiteSpace(modFolderPath) && Directory.Exists(modFolderPath)) {
                return modFolderPath;
            }

            try {
                string modsRoot = Path.Combine(Application.streamingAssetsPath, "mods");
                string embeddedFolder = Path.Combine(modsRoot, "M5TrainsUpdateBeta");
                if (Directory.Exists(embeddedFolder)) {
                    return embeddedFolder;
                }

                string legacyFolder = Path.Combine(modsRoot, "M5OceansPlusKaijuBox");
                if (Directory.Exists(legacyFolder)) {
                    return legacyFolder;
                }

                if (Directory.Exists(modsRoot)) {
                    foreach (string dir in Directory.GetDirectories(modsRoot)) {
                        if (File.Exists(Path.Combine(dir, "Code", "choochoo.cs"))
                            && File.Exists(Path.Combine(dir, "Code", "Buttonz.cs"))
                            && File.Exists(Path.Combine(dir, "mod.json"))) {
                            return dir;
                        }
                    }
                }
            }
            catch {
            }

            return modFolderPath;
        }

        private static void PatchTrainboxHarmony() {
            if (_harmony == null) {
                return;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes()) {
                if (type == null || !string.Equals(type.Namespace, "Trainbox", StringComparison.Ordinal)) {
                    continue;
                }

                if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0) {
                    continue;
                }

                try {
                    _harmony.CreateClassProcessor(type).Patch();
                }
                catch {
                    // Keep the embedded bootstrap resilient if one patch target changes.
                }
            }
        }
    }

    internal static class TrainboxDebug {
        internal static void Log(string message) {
            return;
        }
    }

    internal sealed class TrainboxUiRuntime : MonoBehaviour {
        private const float TickInterval = 3f;
        private static TrainboxUiRuntime _instance;
        private float _timer;

        internal static void Install() {
            if (_instance != null) {
                return;
            }

            GameObject rootObject = new GameObject("TrainboxUiRuntime");
            UnityEngine.Object.DontDestroyOnLoad(rootObject);
            _instance = rootObject.AddComponent<TrainboxUiRuntime>();
        }

        private void Update() {
            _timer -= Time.unscaledDeltaTime;
            if (_timer > 0f) {
                return;
            }

            _timer = TickInterval;
            TrainPowers.HideStandaloneTabButton();
            if (TrainPowers.IsStandaloneTabButtonHidden()) {
                enabled = false;
            }
        }
    }

    internal static class TrainVisuals {
        private static FieldInfo _cachedSpriteField;
        private static readonly List<Sprite> TrainFrames = new List<Sprite>();

        internal static Sprite TrainIcon { get; private set; }
        internal static Sprite TrainUnitSprite { get; private set; }
        internal static Sprite TrainUpSprite { get; private set; }
        internal static Sprite TrainDownSprite { get; private set; }
        internal static Sprite RailTileSprite { get; private set; }
        internal static Sprite ModernRoadSprite { get; private set; }
        internal static Sprite TinyCarUpSprite { get; private set; }
        internal static Sprite TinyCarLeftSprite { get; private set; }
        internal static Sprite TinyCarDownSprite { get; private set; }
        internal static Sprite TinyCarRightSprite { get; private set; }
        internal static Sprite TinyCarriageSprite { get; private set; }

        internal static void Initialize() {
            LoadTrainSprites();
            RailTileSprite = BuildRailSprite();
            ModernRoadSprite = BuildModernRoadSprite();
            LoadDirectionalCarSprites();
            TinyCarriageSprite = BuildTinyCarriageSprite();
        }

        internal static Sprite GetTrainSpriteForActor(Actor actor) {
            if (actor != null && TaxiTrainLogic.TryGetTrainFacing(actor, out Vector2 forward)) {
                if (Mathf.Abs(forward.y) > Mathf.Abs(forward.x)) {
                    if (forward.y > 0.05f && TrainUpSprite != null) {
                        return TrainUpSprite;
                    }

                    if (forward.y < -0.05f && TrainDownSprite != null) {
                        return TrainDownSprite;
                    }
                }
            }

            if (TrainFrames.Count == 0) {
                return TrainUnitSprite;
            }

            int index = Mathf.FloorToInt(Time.time * 6f) % TrainFrames.Count;
            if (index < 0 || index >= TrainFrames.Count) {
                index = 0;
            }

            return TrainFrames[index];
        }

        internal static void SetCachedSprite(BaseUnlockableAsset asset, Sprite sprite) {
            if (asset == null || sprite == null) {
                return;
            }

            GetCachedSpriteField()?.SetValue(asset, sprite);
        }

        private static FieldInfo GetCachedSpriteField() {
            return _cachedSpriteField
                ?? (_cachedSpriteField = typeof(BaseUnlockableAsset).GetField(
                    "cached_sprite",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        private static void LoadTrainSprites() {
            TrainFrames.Clear();
            TrainUpSprite = null;
            TrainDownSprite = null;

                string modFolder = Main.ModFolderPath;
            if (!string.IsNullOrWhiteSpace(modFolder))
            {
                string artFolder = Path.Combine(modFolder, "Artwork");
                string[] frameNames = { "train1.png", "train2.png", "train3.png", "train4.png" };
                for (int i = 0; i < frameNames.Length; i++)
                {
                    var p = Path.Combine(artFolder, frameNames[i]);
                    Sprite s = LoadTrainFrameSprite(p);
                    if (s == null) continue;
                    TrainFrames.Add(s);
                }

                TrainUpSprite = LoadTrainFrameSprite(Path.Combine(artFolder, "train_up.png"))
                    ?? LoadTrainFrameSprite(Path.Combine(artFolder, "train_up.gif"));
                TrainDownSprite = LoadTrainFrameSprite(Path.Combine(artFolder, "train_down.png"))
                    ?? LoadTrainFrameSprite(Path.Combine(artFolder, "traindown.gif"));
            }

            if (TrainFrames.Count > 0) {
                TrainUnitSprite = TrainFrames[0];
                TrainIcon = TrainFrames[0];
                return;
            }

            TrainIcon = BuildTrainSprite(32, 32, 32f);
            TrainUnitSprite = BuildTrainSprite(16, 16, 24f);
            TrainFrames.Add(TrainUnitSprite);
        }

        internal static Sprite GetDirectionalCarSprite(Vector3 direction) {
            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x)) {
                return direction.y >= 0f ? TinyCarUpSprite : TinyCarDownSprite;
            }

            return direction.x >= 0f ? TinyCarRightSprite : TinyCarLeftSprite;
        }

        private static void LoadDirectionalCarSprites() {
            string artFolder = GetArtworkFolder();

            TinyCarUpSprite = TryLoadCarArtworkSprite(artFolder, "car_up.png", 16f)
                ?? BuildDirectionalCarSprite(CarSpriteDirection.Up);
            TinyCarLeftSprite = TryLoadCarArtworkSprite(artFolder, "car_left.png", 16f)
                ?? BuildDirectionalCarSprite(CarSpriteDirection.Left);
            TinyCarDownSprite = TryLoadCarArtworkSprite(artFolder, "car_down.png", 16f)
                ?? BuildDirectionalCarSprite(CarSpriteDirection.Down);
            TinyCarRightSprite = TryLoadCarArtworkSprite(artFolder, "car_right.png", 16f)
                ?? BuildDirectionalCarSprite(CarSpriteDirection.Right);
        }

        private static string GetArtworkFolder() {
            string modFolder = Main.ModFolderPath;
            return string.IsNullOrWhiteSpace(modFolder) ? null : Path.Combine(modFolder, "Artwork");
        }

        private static Sprite TryLoadArtworkSprite(string artFolder, string fileName, float pixelsPerUnit) {
            if (string.IsNullOrWhiteSpace(artFolder)) {
                return null;
            }

            return LoadSpriteFromFile(Path.Combine(artFolder, fileName), pixelsPerUnit);
        }

        private static Sprite TryLoadCarArtworkSprite(string artFolder, string fileName, float pixelsPerUnit) {
            if (string.IsNullOrWhiteSpace(artFolder)) {
                return null;
            }

            Texture2D texture = LoadTextureFromFile(Path.Combine(artFolder, fileName));
            if (texture == null) {
                return null;
            }

            FlipTextureVertically(texture);
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
        }

        private static Sprite LoadTrainFrameSprite(string fullPath) {
            Texture2D texture = LoadTextureFromFile(fullPath);
            if (texture == null) {
                return null;
            }

            Rect rect = new Rect(0f, 2f, texture.width, Mathf.Max(1f, texture.height - 2f));
            return Sprite.Create(
                texture,
                rect,
                new Vector2(0.5f, 0.12f),
                96f
            );
        }

        private static Sprite LoadSpriteFromFile(string fullPath, float pixelsPerUnit) {
            Texture2D texture = LoadTextureFromFile(fullPath);
            if (texture == null) {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
        }

        private static Texture2D LoadTextureFromFile(string fullPath) {
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath)) {
                return null;
            }

            byte[] pngBytes = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, pngBytes)) {
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        private static void FlipTextureVertically(Texture2D texture) {
            if (texture == null) {
                return;
            }

            int width = texture.width;
            int height = texture.height;
            for (int y = 0; y < height / 2; y++) {
                int oppositeY = height - 1 - y;
                for (int x = 0; x < width; x++) {
                    Color top = texture.GetPixel(x, y);
                    Color bottom = texture.GetPixel(x, oppositeY);
                    texture.SetPixel(x, y, bottom);
                    texture.SetPixel(x, oppositeY, top);
                }
            }

            texture.Apply(false, false);
        }

        private static Sprite BuildTrainSprite(int width, int height, float pixelsPerUnit) {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color railBlack = new Color32(28, 27, 30, 255);
            Color steel = new Color32(154, 158, 166, 255);
            Color red = new Color32(182, 47, 45, 255);
            Color redDark = new Color32(133, 28, 26, 255);
            Color gold = new Color32(225, 190, 74, 255);
            Color smoke = new Color32(220, 220, 220, 180);

            Fill(texture, clear);

            int baseY = height / 4;
            int wheelY = baseY - 1;
            DrawRect(texture, 2, baseY, width - 4, height / 3, red);
            DrawRect(texture, 4, baseY + 2, width / 3, height / 4, redDark);
            DrawRect(texture, width / 2, baseY + 4, width / 4, height / 5, gold);
            DrawRect(texture, 3, baseY + height / 3 - 2, width - 6, 2, railBlack);
            DrawRect(texture, 0, 1, width, 2, steel);

            for (int x = 4; x <= width - 5; x += Math.Max(3, width / 6)) {
                DrawRect(texture, x, wheelY, 2, 2, steel);
            }

            DrawRect(texture, width / 3, baseY + height / 4, 2, height / 3, railBlack);
            DrawRect(texture, width / 3 + 1, baseY + height / 4 + 2, 2, height / 4, steel);

            DrawPixel(texture, width - 4, baseY + height / 6, gold);
            DrawPixel(texture, width - 5, baseY + height / 6, gold);

            DrawRect(texture, 3, height - 6, 3, 2, smoke);
            DrawRect(texture, 5, height - 4, 2, 1, smoke);

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
        }

        private static Sprite BuildRailSprite() {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color ballast = new Color32(74, 66, 56, 230);
            Color ballastShade = new Color32(56, 48, 40, 230);
            Color sleeper = new Color32(122, 84, 52, 255);
            Color sleeperShade = new Color32(91, 60, 38, 255);
            Color rail = new Color32(210, 214, 220, 255);
            Color railMid = new Color32(154, 160, 168, 255);
            Color railShade = new Color32(94, 99, 106, 255);

            Fill(texture, clear);

            DrawRect(texture, 1, 0, 14, 16, ballast);
            DrawRect(texture, 0, 0, 1, 16, ballastShade);
            DrawRect(texture, 15, 0, 1, 16, ballastShade);

            for (int y = 1; y < 16; y += 4) {
                DrawRect(texture, 1, y, 14, 3, sleeperShade);
                DrawRect(texture, 2, y, 12, 2, sleeper);
            }

            DrawRect(texture, 3, 0, 3, 16, railShade);
            DrawRect(texture, 10, 0, 3, 16, railShade);
            DrawRect(texture, 4, 0, 2, 16, railMid);
            DrawRect(texture, 11, 0, 2, 16, railMid);
            DrawRect(texture, 4, 0, 1, 16, rail);
            DrawRect(texture, 11, 0, 1, 16, rail);

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 16f, 16f),
                new Vector2(0.5f, 0.5f),
                4f
            );
        }

        private static Sprite BuildModernRoadSprite() {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color asphalt = new Color32(58, 62, 70, 255);
            Color asphaltShade = new Color32(42, 46, 54, 255);
            Color lane = new Color32(232, 226, 168, 255);
            Color edge = new Color32(156, 164, 176, 220);

            Fill(texture, clear);
            DrawRect(texture, 0, 0, 16, 16, asphalt);
            DrawRect(texture, 0, 0, 16, 3, asphaltShade);
            DrawRect(texture, 0, 13, 16, 3, asphaltShade);
            DrawRect(texture, 1, 2, 1, 12, edge);
            DrawRect(texture, 14, 2, 1, 12, edge);

            for (int y = 1; y < 16; y += 4) {
                DrawRect(texture, 7, y, 2, 2, lane);
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 16f, 16f),
                new Vector2(0.5f, 0.5f),
                4f
            );
        }

        private static Sprite BuildDirectionalCarSprite(CarSpriteDirection direction) {
            Dictionary<char, Color> palette = new Dictionary<char, Color> {
                ['.'] = new Color(0f, 0f, 0f, 0f),
                ['R'] = new Color32(255, 58, 49, 255),
                ['r'] = new Color32(191, 43, 39, 255),
                ['d'] = new Color32(145, 35, 33, 255),
                ['W'] = new Color32(120, 146, 168, 255),
                ['w'] = new Color32(77, 92, 108, 255),
                ['T'] = new Color32(41, 41, 45, 255),
                ['t'] = new Color32(63, 63, 68, 255),
                ['L'] = new Color32(255, 234, 130, 255),
                ['B'] = new Color32(33, 33, 37, 255)
            };

            string[] rows;
            float pixelsPerUnit;
            switch (direction) {
                case CarSpriteDirection.Up:
                    rows = new[] {
                        "....dddd....",
                        "...dRRRRd...",
                        "..drrrrrrd..",
                        "..rRRRRRRr..",
                        "..rRWWWWRr..",
                        ".TrRwWWrRrT.",
                        ".TrRWWWWRrT.",
                        ".TrRrrrrRrT.",
                        ".TrRRRRRRrT.",
                        ".TrRRRRRRrT.",
                        ".TrRrrrrRrT.",
                        ".TrRWWWWRrT.",
                        ".TrRwWWrRrT.",
                        "..rRWWWWRr..",
                        "..drrrrrrd..",
                        "...dBBBBd..."
                    };
                    pixelsPerUnit = 16f;
                    break;
                case CarSpriteDirection.Down:
                    rows = new[] {
                        "....dddd....",
                        "...dRRRRd...",
                        "..drrrrrrd..",
                        "..rRWWWWRr..",
                        ".TrRwWWrRrT.",
                        ".TrRWWWWRrT.",
                        ".TrRrrrrRrT.",
                        ".TrRRRRRRrT.",
                        ".TrRRRRRRrT.",
                        ".TrRrrrrRrT.",
                        ".TrRWWWWRrT.",
                        ".TrRwWWrRrT.",
                        "..rRWWWWRr..",
                        "..rRRLLRRr..",
                        "..drrBBrrd..",
                        "...BBBBBB..."
                    };
                    pixelsPerUnit = 16f;
                    break;
                case CarSpriteDirection.Left:
                    rows = new[] {
                        "..............",
                        ".....dddd.....",
                        "...ddRRRRd....",
                        "..dRRWWWRRd...",
                        ".dRRWWwWWRRd..",
                        "LLRRRRRRRRRd..",
                        "BRRrRRRRrRRdT.",
                        "TrrrrrrrrrrTT.",
                        "..TT....TT....",
                        ".............."
                    };
                    pixelsPerUnit = 14f;
                    break;
                default:
                    rows = new[] {
                        "..............",
                        ".....dddd.....",
                        "....dRRRRdd...",
                        "...dRRWWWRRd..",
                        "..dRRWWwWWRRd.",
                        "..dRRRRRRRRRLL",
                        ".TrdRRrRRRRrRB",
                        ".TTrrrrrrrrrrT",
                        "....TT....TT..",
                        ".............."
                    };
                    pixelsPerUnit = 14f;
                    break;
            }

            return BuildSpriteFromPattern(rows, palette, pixelsPerUnit);
        }

        private static Sprite BuildTinyCarriageSprite() {
            Texture2D texture = new Texture2D(12, 8, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color wood = new Color32(143, 92, 46, 255);
            Color woodShade = new Color32(94, 58, 28, 255);
            Color cloth = new Color32(192, 48, 56, 255);
            Color wheel = new Color32(44, 37, 31, 255);
            Color axle = new Color32(184, 152, 92, 255);

            Fill(texture, clear);
            DrawRect(texture, 2, 2, 7, 2, wood);
            DrawRect(texture, 2, 1, 7, 1, woodShade);
            DrawRect(texture, 3, 4, 5, 2, cloth);
            DrawRect(texture, 2, 4, 1, 1, woodShade);
            DrawRect(texture, 8, 4, 1, 1, woodShade);
            DrawRect(texture, 1, 0, 2, 2, wheel);
            DrawRect(texture, 8, 0, 2, 2, wheel);
            DrawRect(texture, 3, 1, 5, 1, axle);
            DrawRect(texture, 9, 2, 2, 1, woodShade);
            DrawRect(texture, 10, 3, 2, 1, woodShade);

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 12f, 8f),
                new Vector2(0.5f, 0.5f),
                12f
            );
        }

        private static Sprite BuildSpriteFromPattern(string[] rows, Dictionary<char, Color> palette, float pixelsPerUnit) {
            if (rows == null || rows.Length == 0) {
                return null;
            }

            int height = rows.Length;
            int width = rows[0].Length;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Fill(texture, clear);

            for (int y = 0; y < height; y++) {
                string row = rows[y];
                for (int x = 0; x < width; x++) {
                    if (!palette.TryGetValue(row[x], out Color color)) {
                        color = clear;
                    }

                    texture.SetPixel(x, height - 1 - y, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
        }

        private enum CarSpriteDirection {
            Up,
            Left,
            Down,
            Right
        }

        private static void Fill(Texture2D texture, Color color) {
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
        }

        private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color) {
            for (int dx = 0; dx < width; dx++) {
                for (int dy = 0; dy < height; dy++) {
                    DrawPixel(texture, x + dx, y + dy, color);
                }
            }
        }

        private static void DrawPixel(Texture2D texture, int x, int y, Color color) {
            if (x < 0 || y < 0 || x >= texture.width || y >= texture.height) {
                return;
            }

            texture.SetPixel(x, y, color);
        }
    }

    internal static class StandaloneBuildingUtils {
        private static FieldInfo _buildingAssetField;

        internal static BuildingAsset GetBuildingAsset(Building building) {
            if (building == null) {
                return null;
            }

            if (_buildingAssetField == null) {
                _buildingAssetField = typeof(Building).GetField(
                    "asset",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            return _buildingAssetField?.GetValue(building) as BuildingAsset;
        }

        internal static string GetAssetSignature(BuildingAsset asset) {
            return string.Concat(asset?.id ?? string.Empty, "|", asset?.type ?? string.Empty, "|", asset?.group ?? string.Empty)
                .ToLowerInvariant();
        }
    }

    internal enum CityEraStage {
        None = 0,
        Medieval = 1,
        Renaissance = 2,
        Modern = 3,
        Future = 4
    }

    internal static class WorldboxEraCompat {
        internal static CityEraStage GetCityEra(City city) {
            if (city == null) {
                return CityEraStage.None;
            }

            int buildingCount = city.countBuildings();
            if (buildingCount >= 24) {
                return CityEraStage.Modern;
            }

            if (buildingCount >= 12) {
                return CityEraStage.Renaissance;
            }

            if (buildingCount > 0) {
                return CityEraStage.Medieval;
            }

            return CityEraStage.None;
        }

        internal static bool IsRailEraUnlocked(City city) {
            return GetCityEra(city) >= CityEraStage.Renaissance;
        }

        internal static bool IsModernRoadEra(City city) {
            return GetCityEra(city) >= CityEraStage.Modern;
        }
    }

    internal static class StandaloneCultureTraitSystem {
        internal static bool CultureUsesModernRoads(City city) {
            return RoadTrafficSystem.ModernRoadsEnabled
                && WorldboxEraCompat.IsModernRoadEra(city);
        }
    }

    internal static class RailTileRegistry {
        private const float AppearanceRefreshIntervalSeconds = 8f;
        private static readonly HashSet<long> TrackTileKeys = new HashSet<long>();
        private static readonly HashSet<long> StopTileKeys = new HashSet<long>();
        private static readonly Dictionary<long, float> NextAppearanceRefreshByTileKey = new Dictionary<long, float>();
        private static readonly FieldInfo TileSpritesTilesField =
            typeof(TileSprites).GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo _updateDirtyTileMethod;
        private static FieldInfo _cityKingdomField;
        private static FieldInfo _worldTilesField;
        private static object _worldToken;
        private static int _topologyVersion;

        internal static int TopologyVersion {
            get {
                EnsureWorldState();
                return _topologyVersion;
            }
        }

        internal static void Register() {
            if (TopTileLibrary.road == null || AssetManager.top_tiles.get(Main.RailTileId) != null) {
                return;
            }

            TopTileType railTile = AssetManager.top_tiles.clone(Main.RailTileId, TopTileLibrary.road.id);
            if (railTile == null) {
                return;
            }

            railTile.id = Main.RailTileId;
            railTile.road = true;
            railTile.can_build_on = true;
            railTile.considered_empty_tile = false;
            railTile.force_edge_variation = false;
            railTile.check_edge = false;

            ApplyRailSprites(railTile);
            AssetManager.top_tiles.add(railTile);
        }

        internal static TopTileType GetRailTopTile() {
            return AssetManager.top_tiles.get(Main.RailTileId) ?? TopTileLibrary.road;
        }

        internal static bool PaintTrack(WorldTile tile) {
            EnsureWorldState();
            if (!CanPaintTrack(tile, true)) {
                return false;
            }

            TileType baseTile = GetTrackBaseTile(tile);
            TopTileType railTop = GetRailTopTile();
            if (baseTile == null || railTop == null) {
                return false;
            }

            tile.setTileType(baseTile, true);
            tile.setTopTileType(railTop, true);

            long key = MakeTileKey(tile);
            bool topologyChanged = StopTileKeys.Remove(key);
            topologyChanged |= TrackTileKeys.Add(key);
            if (topologyChanged) {
                _topologyVersion++;
            }

            MarkTileChanged(tile);
            return true;
        }

        internal static bool PaintStop(WorldTile tile) {
            EnsureWorldState();
            if (!CanPaintTrack(tile, false) || TileLibrary.sand == null) {
                return false;
            }

            TopTileType railTop = GetRailTopTile();
            if (railTop == null) {
                return false;
            }

            tile.setTileType(TileLibrary.sand, true);
            tile.setTopTileType(railTop, true);

            long key = MakeTileKey(tile);
            bool topologyChanged = TrackTileKeys.Remove(key);
            topologyChanged |= StopTileKeys.Add(key);
            if (topologyChanged) {
                _topologyVersion++;
            }

            MarkTileChanged(tile);
            return true;
        }

        internal static bool IsRailTile(WorldTile tile) {
            if (!TryGetRailState(tile, out long key, out bool isStop)) {
                return false;
            }

            MaybeRefreshTrackedRailAppearance(tile, key, isStop);
            return true;
        }

        internal static bool IsRailTilePassive(WorldTile tile) {
            return TryGetRailState(tile, out _, out _);
        }

        internal static bool IsStopTile(WorldTile tile) {
            if (!TryGetRailState(tile, out long key, out bool isStop) || !isStop) {
                return false;
            }

            MaybeRefreshTrackedRailAppearance(tile, key, true);
            return true;
        }

        internal static bool IsStopTilePassive(WorldTile tile) {
            return TryGetRailState(tile, out _, out bool isStop) && isStop;
        }

        internal static List<WorldTile> GetTrackNeighbours(WorldTile tile) {
            List<WorldTile> list = new List<WorldTile>(4);
            TryAddNeighbour(list, tile?.tile_up);
            TryAddNeighbour(list, tile?.tile_down);
            TryAddNeighbour(list, tile?.tile_left);
            TryAddNeighbour(list, tile?.tile_right);
            return list;
        }

        internal static IEnumerable<long> EnumerateStopTileKeys() {
            EnsureWorldState();
            return new List<long>(StopTileKeys);
        }

        internal static bool AreRailTilesConnected(WorldTile start, WorldTile target) {
            if (start == null || target == null) {
                return false;
            }

            if (start == target) {
                return true;
            }

            Queue<WorldTile> queue = new Queue<WorldTile>();
            HashSet<long> visited = new HashSet<long>();
            queue.Enqueue(start);
            visited.Add(MakeTileKey(start));

            while (queue.Count > 0) {
                WorldTile current = queue.Dequeue();
                foreach (WorldTile neighbour in GetTrackNeighbours(current)) {
                    long key = MakeTileKey(neighbour);
                    if (!visited.Add(key)) {
                        continue;
                    }

                    if (neighbour == target) {
                        return true;
                    }

                    queue.Enqueue(neighbour);
                }
            }

            return false;
        }

        internal static WorldTile FindNearestTrack(WorldTile origin, int radius) {
            if (origin == null || World.world == null) {
                return null;
            }

            if (IsRailTilePassive(origin)) {
                return origin;
            }

            for (int r = 1; r <= radius; r++) {
                for (int x = origin.x - r; x <= origin.x + r; x++) {
                    for (int y = origin.y - r; y <= origin.y + r; y++) {
                        WorldTile candidate = World.world.GetTile(x, y);
                        if (IsRailTilePassive(candidate)) {
                            return candidate;
                        }
                    }
                }
            }

            return null;
        }

        internal static WorldTile FindNearestStop(WorldTile origin, int radius) {
            if (origin == null || World.world == null) {
                return null;
            }

            if (IsStopTilePassive(origin)) {
                return origin;
            }

            WorldTile best = null;
            float bestDistance = float.MaxValue;
            for (int x = origin.x - radius; x <= origin.x + radius; x++) {
                for (int y = origin.y - radius; y <= origin.y + radius; y++) {
                    WorldTile candidate = World.world.GetTile(x, y);
                    if (!IsStopTilePassive(candidate)) {
                        continue;
                    }

                    float dx = candidate.x - origin.x;
                    float dy = candidate.y - origin.y;
                    float distance = dx * dx + dy * dy;
                    if (distance < bestDistance) {
                        bestDistance = distance;
                        best = candidate;
                    }
                }
            }

            return best;
        }

        internal static Kingdom ResolveTrackKingdom(WorldTile tile) {
            Kingdom cityKingdom = GetCityKingdom(tile?.zone?.city);
            if (cityKingdom != null) {
                return cityKingdom;
            }

            if (tile == null) {
                return null;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(tile, 2, 10f, false)) {
                if (actor?.kingdom != null) {
                    return actor.kingdom;
                }
            }

            return null;
        }

        private static Kingdom GetCityKingdom(City city) {
            if (city == null || !city.hasKingdom()) {
                return null;
            }

            try {
                if (_cityKingdomField == null) {
                    _cityKingdomField = typeof(City).GetField(
                        "kingdom",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _cityKingdomField?.GetValue(city) as Kingdom;
            }
            catch {
                return null;
            }
        }

        internal static WorldTile FindNearestConnectedStop(WorldTile connectedToTile, WorldTile targetTile) {
            EnsureWorldState();
            WorldTile best = null;
            float bestScore = float.MaxValue;
            HashSet<long> reachableKeys = connectedToTile != null
                ? GetReachableRailKeys(connectedToTile)
                : null;

            foreach (long key in new List<long>(StopTileKeys)) {
                WorldTile stopTile = GetTileByKey(key);
                if (!IsStopTilePassive(stopTile)) {
                    continue;
                }

                if (reachableKeys != null && !reachableKeys.Contains(key)) {
                    continue;
                }

                float score = ScoreStopForTarget(stopTile, targetTile);
                if (score < bestScore) {
                    bestScore = score;
                    best = stopTile;
                }
            }

            return best;
        }

        private static HashSet<long> GetReachableRailKeys(WorldTile start) {
            HashSet<long> visited = new HashSet<long>();
            if (!IsRailTilePassive(start)) {
                return visited;
            }

            Queue<WorldTile> queue = new Queue<WorldTile>();
            queue.Enqueue(start);
            visited.Add(MakeTileKey(start));

            while (queue.Count > 0) {
                WorldTile current = queue.Dequeue();
                foreach (WorldTile neighbour in GetTrackNeighbours(current)) {
                    long key = MakeTileKey(neighbour);
                    if (visited.Add(key)) {
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return visited;
        }

        internal static bool HasAnotherConnectedStop(WorldTile sourceStop) {
            if (!IsStopTilePassive(sourceStop)) {
                return false;
            }

            EnsureWorldState();
            HashSet<long> reachableKeys = GetReachableRailKeys(sourceStop);
            foreach (long key in new List<long>(StopTileKeys)) {
                WorldTile stopTile = GetTileByKey(key);
                if (stopTile == null || stopTile == sourceStop || !IsStopTilePassive(stopTile)) {
                    continue;
                }

                if (reachableKeys.Contains(key)) {
                    return true;
                }
            }

            return false;
        }

        internal static bool CanBuildTrackOnTile(WorldTile tile, bool allowBridge) {
            return CanPaintTrack(tile, allowBridge);
        }

        internal static bool IsBridgeWaterTile(WorldTile tile) {
            return tile?.Type != null && tile.Type.ocean;
        }

        private static float ScoreStopForTarget(WorldTile stopTile, WorldTile targetTile) {
            if (stopTile == null) {
                return float.MaxValue;
            }

            if (targetTile == null) {
                return 0f;
            }

            float dx = stopTile.x - targetTile.x;
            float dy = stopTile.y - targetTile.y;
            float score = dx * dx + dy * dy;
            if (!stopTile.isSameIsland(targetTile)) {
                score += 1000000f;
            }

            return score;
        }

        private static void ApplyRailSprites(TopTileType railTile) {
            if (railTile == null || TrainVisuals.RailTileSprite == null || TileSpritesTilesField == null) {
                return;
            }

            IList sourceTiles = TopTileLibrary.road?.sprites != null
                ? TileSpritesTilesField.GetValue(TopTileLibrary.road.sprites) as IList
                : null;

            railTile.sprites = new TileSprites();
            IList tiles = TileSpritesTilesField.GetValue(railTile.sprites) as IList;
            if (tiles == null) {
                return;
            }

            tiles.Clear();

            if (sourceTiles != null && sourceTiles.Count > 0) {
                for (int i = 0; i < sourceTiles.Count; i++) {
                    Tile sourceTile = sourceTiles[i] as Tile;
                    if (sourceTile == null) {
                        continue;
                    }

                    Tile cloned = UnityEngine.Object.Instantiate(sourceTile);
                    cloned.name = $"taxi_train_rail_{i}";
                    cloned.sprite = TrainVisuals.RailTileSprite;
                    tiles.Add(cloned);
                }
            }

            if (tiles.Count == 0) {
                railTile.sprites.addVariation(TrainVisuals.RailTileSprite, "taxi_train_rail");
            }
        }

        private static bool CanPaintTrack(WorldTile tile, bool allowBridge) {
            if (tile == null || tile.Type == null) {
                return false;
            }

            if (tile.Type.ground) {
                return true;
            }

            return allowBridge && tile.Type.ocean;
        }

        private static TileType GetTrackBaseTile(WorldTile tile) {
            return TileLibrary.soil_low ?? TileLibrary.sand ?? tile?.main_type;
        }

        private static void EnsureTrackedRailAppearance(WorldTile tile, bool isStop) {
            if (tile == null) {
                return;
            }

            TopTileType railTop = GetRailTopTile();
            if (railTop == null) {
                return;
            }

            bool changed = false;
            if (tile.top_type == null || !string.Equals(tile.top_type.id, railTop.id, StringComparison.Ordinal)) {
                tile.setTopTileType(railTop, true);
                changed = true;
            }

            if (!isStop) {
                TileType trackBase = GetTrackBaseTile(tile);
                if (trackBase != null && tile.main_type != null
                    && !string.Equals(tile.main_type.id, trackBase.id, StringComparison.Ordinal)) {
                    tile.setTileType(trackBase, true);
                    changed = true;
                }
            }

            if (isStop && TileLibrary.sand != null && tile.main_type != null
                && !string.Equals(tile.main_type.id, TileLibrary.sand.id, StringComparison.Ordinal)) {
                tile.setTileType(TileLibrary.sand, true);
                changed = true;
            }

            if (changed) {
                MarkTileChanged(tile);
            }
        }

        private static void MarkTileChanged(WorldTile tile) {
            if (tile == null || World.world == null) {
                return;
            }

            MapAction.makeTileChanged(tile);
            GetUpdateDirtyTileMethod()?.Invoke(World.world, new object[] { tile });
        }

        private static MethodInfo GetUpdateDirtyTileMethod() {
            return _updateDirtyTileMethod
                ?? (_updateDirtyTileMethod = typeof(MapBox).GetMethod(
                    "updateDirtyTile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(WorldTile) },
                    null));
        }

        private static void EnsureWorldState() {
            object currentWorld = World.world;
            if (ReferenceEquals(_worldToken, currentWorld)) {
                return;
            }

            _worldToken = currentWorld;
            TrackTileKeys.Clear();
            StopTileKeys.Clear();
            NextAppearanceRefreshByTileKey.Clear();
            _topologyVersion++;

            WorldTile[] tiles = GetWorldTiles();
            if (tiles == null) {
                return;
            }

            for (int i = 0; i < tiles.Length; i++) {
                WorldTile tile = tiles[i];
                if (!HasPersistedRailTop(tile)) {
                    continue;
                }

                long key = MakeTileKey(tile);
                if (IsPersistedStop(tile)) {
                    StopTileKeys.Add(key);
                } else {
                    TrackTileKeys.Add(key);
                }
            }
        }

        private static WorldTile[] GetWorldTiles() {
            if (World.world == null) {
                return null;
            }

            try {
                if (_worldTilesField == null) {
                    _worldTilesField = typeof(MapBox).GetField(
                        "tiles_list",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _worldTilesField?.GetValue(World.world) as WorldTile[];
            }
            catch {
                return null;
            }
        }

        private static bool HasPersistedRailTop(WorldTile tile) {
            return tile?.top_type != null
                && string.Equals(tile.top_type.id, Main.RailTileId, StringComparison.Ordinal);
        }

        private static bool IsPersistedStop(WorldTile tile) {
            return HasPersistedRailTop(tile)
                && tile?.main_type != null
                && TileLibrary.sand != null
                && string.Equals(tile.main_type.id, TileLibrary.sand.id, StringComparison.Ordinal);
        }

        private static void TryAddNeighbour(List<WorldTile> list, WorldTile tile) {
            if (IsRailTilePassive(tile)) {
                list.Add(tile);
            }
        }

        private static bool TryGetRailState(WorldTile tile, out long key, out bool isStop) {
            key = long.MinValue;
            isStop = false;
            if (tile == null) {
                return false;
            }

            EnsureWorldState();
            key = MakeTileKey(tile);
            bool registeredStop = StopTileKeys.Contains(key);
            bool registeredTrack = !registeredStop && TrackTileKeys.Contains(key);
            if (registeredStop || registeredTrack) {
                if (!HasPersistedRailTop(tile)) {
                    RemoveTrackedRailKey(key);
                    return false;
                }

                bool persistedStop = IsPersistedStop(tile);
                if (registeredStop != persistedStop) {
                    StopTileKeys.Remove(key);
                    TrackTileKeys.Remove(key);
                    if (persistedStop) {
                        StopTileKeys.Add(key);
                    } else {
                        TrackTileKeys.Add(key);
                    }
                    _topologyVersion++;
                }

                isStop = persistedStop;
                return true;
            }

            if (!HasPersistedRailTop(tile)) {
                return false;
            }

            isStop = IsPersistedStop(tile);
            if (isStop) {
                if (StopTileKeys.Add(key)) {
                    _topologyVersion++;
                }
            } else {
                if (TrackTileKeys.Add(key)) {
                    _topologyVersion++;
                }
            }

            return true;
        }

        private static bool RemoveTrackedRailKey(long key) {
            bool changed = StopTileKeys.Remove(key);
            changed |= TrackTileKeys.Remove(key);
            if (changed) {
                NextAppearanceRefreshByTileKey.Remove(key);
                _topologyVersion++;
            }

            return changed;
        }

        private static void MaybeRefreshTrackedRailAppearance(WorldTile tile, long key, bool isStop) {
            if (tile == null || key == long.MinValue) {
                return;
            }

            float now = Time.time;
            if (NextAppearanceRefreshByTileKey.TryGetValue(key, out float nextRefreshAt) && now < nextRefreshAt) {
                return;
            }

            NextAppearanceRefreshByTileKey[key] = now + AppearanceRefreshIntervalSeconds;
            EnsureTrackedRailAppearance(tile, isStop);
        }

        internal static long MakeTileKey(WorldTile tile) {
            return tile == null ? long.MinValue : ((long)tile.x << 32) ^ (uint)tile.y;
        }

        internal static WorldTile GetTileByKey(long tileKey) {
            if (tileKey == long.MinValue || World.world == null) {
                return null;
            }

            int x = (int)(tileKey >> 32);
            int y = unchecked((int)tileKey);
            return World.world.GetTile(x, y);
        }
    }

    internal static class RoadTrafficSystem {
        private const float RoadRefreshInterval = 60f;
        private const float TrafficEffectInterval = 18f;
        private const int RoadRefreshRadius = 10;
        private const int MaxTrafficTilesPerPulse = 2;
        private const float TrafficSpriteLifetime = 0.9f;

        private static readonly Dictionary<long, float> NextRoadRefreshByCity = new Dictionary<long, float>();
        private static readonly Dictionary<long, float> NextTrafficPulseByCity = new Dictionary<long, float>();
        private static readonly System.Random SharedRandom = new System.Random();
        private static readonly FieldInfo TileSpritesTilesField =
            typeof(TileSprites).GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _cityKingdomField;
        private static FieldInfo _buildingDoorTileField;
        private static PropertyInfo _buildingDoorTileProperty;
        private static MethodInfo _updateDirtyTileMethod;
        private static MemberInfo _worldWidthMember;
        private static MemberInfo _worldHeightMember;

        internal static bool ModernRoadsEnabled { get; private set; } = true;

        internal static void Register() {
            EnsureModernRoadRegistered();
        }

        internal static bool ToggleModernRoads() {
            ModernRoadsEnabled = !ModernRoadsEnabled;
            RefreshAllRoadTilesNow();
            return ModernRoadsEnabled;
        }

        internal static bool PaintRoad(WorldTile tile) {
            if (!CanPaintRoad(tile)) {
                return false;
            }

            TopTileType roadTop = GetCurrentRoadTop();
            if (roadTop == null) {
                return false;
            }

            tile.setTopTileType(roadTop, true);
            MarkTileChanged(tile);
            return true;
        }

        internal static bool SpawnManualTrafficAtTile(WorldTile tile) {
            WorldTile roadTile = FindNearestRoadTile(tile, 4);
            if (roadTile == null) {
                return false;
            }

            List<WorldTile> trail = BuildManualTrafficTrail(roadTile, 6);
            if (trail == null || trail.Count < 2) {
                return false;
            }

            Sprite sprite = TrainVisuals.TinyCarRightSprite
                ?? TrainVisuals.TinyCarDownSprite
                ?? TrainVisuals.TinyCarLeftSprite
                ?? TrainVisuals.TinyCarUpSprite;
            if (sprite == null) {
                return false;
            }

            if (trail.Count >= 2 && RoadCarActorSystem.TrySpawnRoadCar(trail)) {
                return true;
            }

            if (trail.Count >= 2) {
                TrafficSpriteSystem.SpawnTrafficSprite(trail, sprite, 3.2f, true);
            } else if (RoadCarActorSystem.TrySpawnRoadCar(new List<WorldTile> { roadTile })) {
                return true;
            } else {
                TrafficSpriteSystem.SpawnStaticTrafficSprite(roadTile, sprite, 4.5f);
            }

            return true;
        }

        internal static void UpdateCity(City city) {
            if (!CanUpdateCity(city)) {
                return;
            }

            float now = Time.time;
            if (!NextRoadRefreshByCity.TryGetValue(city.id, out float nextRoadRefresh) || now >= nextRoadRefresh) {
                RefreshRoadTiles(city);
                NextRoadRefreshByCity[city.id] = now + RoadRefreshInterval + ((city.id % 5L) * 0.45f);
            }

            if (!WorldboxEraCompat.IsModernRoadEra(city)) {
                return;
            }

            if (!NextTrafficPulseByCity.TryGetValue(city.id, out float nextTrafficPulse) || now >= nextTrafficPulse) {
                SpawnTrafficPulse(city);
                NextTrafficPulseByCity[city.id] = now + TrafficEffectInterval + ((city.id % 3L) * 0.25f);
            }
        }

        private static bool CanUpdateCity(City city) {
            return city != null
                && city.hasKingdom()
                && city.countBuildings() > 0
                && World.world != null
                && GetCityKingdom(city) != null;
        }

        private static void RefreshRoadTiles(City city) {
            WorldTile origin = FindCityAnchorTile(city);
            if (origin == null) {
                return;
            }

            bool useModernRoads = StandaloneCultureTraitSystem.CultureUsesModernRoads(city);
            TopTileType targetRoad = useModernRoads
                ? AssetManager.top_tiles.get(Main.ModernRoadTileId) ?? TopTileLibrary.road
                : TopTileLibrary.road;
            if (targetRoad == null) {
                return;
            }

            for (int x = origin.x - RoadRefreshRadius; x <= origin.x + RoadRefreshRadius; x++) {
                for (int y = origin.y - RoadRefreshRadius; y <= origin.y + RoadRefreshRadius; y++) {
                    WorldTile tile = World.world.GetTile(x, y);
                    if (!ShouldRefreshRoadTile(tile, city)) {
                        continue;
                    }

                    if (tile.top_type == targetRoad) {
                        continue;
                    }

                    tile.setTopTileType(targetRoad, true);
                    MarkTileChanged(tile);
                }
            }
        }

        private static bool ShouldRefreshRoadTile(WorldTile tile, City city) {
            if (tile == null || RailTileRegistry.IsRailTilePassive(tile)) {
                return false;
            }

            TopTileType top = tile.top_type;
            if (top == null) {
                return false;
            }

            return top == TopTileLibrary.road
                || string.Equals(top.id, Main.ModernRoadTileId, StringComparison.Ordinal);
        }

        private static bool CanPaintRoad(WorldTile tile) {
            return tile != null
                && tile.Type != null
                && tile.Type.ground
                && !tile.Type.block
                && !tile.Type.ocean
                && !tile.Type.liquid
                && !RailTileRegistry.IsRailTilePassive(tile);
        }

        private static TopTileType GetCurrentRoadTop() {
            if (ModernRoadsEnabled) {
                EnsureModernRoadRegistered();
                return AssetManager.top_tiles.get(Main.ModernRoadTileId) ?? TopTileLibrary.road;
            }

            return TopTileLibrary.road;
        }

        private static void SpawnTrafficPulse(City city) {
            WorldTile cityAnchor = FindCityAnchorTile(city);
            if (cityAnchor == null) {
                return;
            }

            List<Building> buildings = GetActiveTrafficBuildings(city);
            WorldTile sourceTile = null;
            WorldTile targetTile = null;

            if (buildings.Count >= 2) {
                Building source = buildings[SharedRandom.Next(buildings.Count)];
                Building target = null;
                for (int attempt = 0; attempt < 6; attempt++) {
                    Building candidate = buildings[SharedRandom.Next(buildings.Count)];
                    if (candidate != null && !ReferenceEquals(source, candidate)) {
                        target = candidate;
                        break;
                    }
                }

                if (source != null && target != null && !ReferenceEquals(source, target)) {
                    sourceTile = GetBuildingTrafficTile(source);
                    targetTile = GetBuildingTrafficTile(target);
                }
            }

            if (sourceTile == null || targetTile == null || sourceTile == targetTile) {
                List<WorldTile> roadTiles = GetNearbyRoadTiles(cityAnchor, RoadRefreshRadius);
                if (roadTiles.Count < 2) {
                    return;
                }

                sourceTile = roadTiles[SharedRandom.Next(roadTiles.Count)];
                targetTile = null;
                for (int attempt = 0; attempt < 8; attempt++) {
                    WorldTile candidate = roadTiles[SharedRandom.Next(roadTiles.Count)];
                    if (candidate != null && candidate != sourceTile) {
                        targetTile = candidate;
                        break;
                    }
                }
            }

            if (sourceTile == null || targetTile == null || sourceTile == targetTile) {
                return;
            }

            bool useModernRoads = StandaloneCultureTraitSystem.CultureUsesModernRoads(city);
            Sprite trafficSprite = PickTrafficSprite(useModernRoads);
            if (trafficSprite == null) {
                return;
            }

            if (useModernRoads) {
                if (TryBuildRoadTrafficTrail(sourceTile, targetTile, MaxTrafficTilesPerPulse, out List<WorldTile> roadTrail)
                    && roadTrail != null
                    && roadTrail.Count >= 2
                    && RoadCarActorSystem.TrySpawnRoadCar(roadTrail)) {
                    return;
                }

                return;
            }

            List<WorldTile> trail = BuildTrafficTrail(sourceTile, targetTile, MaxTrafficTilesPerPulse);
            if (trail.Count < 2) {
                return;
            }

            TrafficSpriteSystem.SpawnTrafficSprite(trail, trafficSprite, TrafficSpriteLifetime, false);
        }

        private static List<Building> GetActiveTrafficBuildings(City city) {
            List<Building> result = new List<Building>();
            if (city?.buildings == null) {
                return result;
            }

            for (int i = 0; i < city.buildings.Count; i++) {
                Building building = city.buildings[i];
                if (!IsTrafficBuildingAlive(building)) {
                    continue;
                }

                if (GetBuildingTrafficTile(building) == null) {
                    continue;
                }

                result.Add(building);
            }

            return result;
        }

        private static bool IsTrafficBuildingAlive(Building building) {
            if (building == null) {
                return false;
            }

            try {
                if (!building.isAlive()) {
                    return false;
                }
            }
            catch {
                return false;
            }

            WorldTile tile = building.current_tile;
            return tile != null && tile.zone?.city != null;
        }

        private static WorldTile GetBuildingTrafficTile(Building building) {
            if (building == null) {
                return null;
            }

            WorldTile tile = GetBuildingDoorTile(building) ?? building.current_tile;
            if (tile == null) {
                return null;
            }

            WorldTile roadTile = FindNearestRoadTile(tile, 4);
            if (roadTile != null) {
                return roadTile;
            }

            if (tile.zone?.city == null) {
                WorldTile walkable = tile.getWalkableTileAround(tile) ?? tile.getTileAroundThisOnSameIsland(tile, true);
                if (walkable != null) {
                    tile = walkable;
                }
            }

            return tile;
        }

        private static List<WorldTile> BuildTrafficTrail(WorldTile source, WorldTile target, int maxTiles) {
            List<WorldTile> tiles = new List<WorldTile>();
            if (source == null || target == null || World.world == null) {
                return tiles;
            }

            if (TryBuildRoadTrafficTrail(source, target, maxTiles, out List<WorldTile> roadTrail) && roadTrail.Count >= 2) {
                return roadTrail;
            }

            int steps = Mathf.Clamp(Mathf.Abs(target.x - source.x) + Mathf.Abs(target.y - source.y), 2, maxTiles);
            HashSet<long> seen = new HashSet<long>();
            for (int i = 0; i <= steps; i++) {
                float t = steps <= 0 ? 0f : i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(source.x, target.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(source.y, target.y, t));
                WorldTile tile = World.world.GetTile(x, y);
                if (tile == null) {
                    continue;
                }

                long key = RailTileRegistry.MakeTileKey(tile);
                if (!seen.Add(key)) {
                    continue;
                }

                tiles.Add(tile);
            }

            return tiles;
        }

        private static List<WorldTile> GetNearbyRoadTiles(WorldTile origin, int radius) {
            List<WorldTile> result = new List<WorldTile>();
            if (origin == null || World.world == null) {
                return result;
            }

            HashSet<long> seen = new HashSet<long>();
            for (int x = origin.x - radius; x <= origin.x + radius; x++) {
                for (int y = origin.y - radius; y <= origin.y + radius; y++) {
                    WorldTile tile = World.world.GetTile(x, y);
                    if (!IsRoadTile(tile)) {
                        continue;
                    }

                    long key = RailTileRegistry.MakeTileKey(tile);
                    if (!seen.Add(key)) {
                        continue;
                    }

                    result.Add(tile);
                }
            }

            return result;
        }

        private static List<WorldTile> BuildManualTrafficTrail(WorldTile origin, int maxLength) {
            List<WorldTile> trail = new List<WorldTile>();
            if (!IsRoadTile(origin)) {
                return trail;
            }

            trail.Add(origin);
            WorldTile previous = null;
            WorldTile current = origin;

            for (int i = 0; i < maxLength; i++) {
                List<WorldTile> neighbours = new List<WorldTile>();
                foreach (WorldTile neighbour in GetRoadNeighbours(current)) {
                    if (neighbour == null || neighbour == previous) {
                        continue;
                    }

                    neighbours.Add(neighbour);
                }

                if (neighbours.Count == 0) {
                    if (previous != null && IsRoadTile(previous) && trail.Count < 2) {
                        neighbours.Add(previous);
                    } else {
                        break;
                    }
                }

                WorldTile next = neighbours[SharedRandom.Next(neighbours.Count)];
                if (next == null || next == current) {
                    break;
                }

                trail.Add(next);
                previous = current;
                current = next;
            }

            if (trail.Count == 1) {
                foreach (WorldTile neighbour in GetRoadNeighbours(origin)) {
                    if (neighbour != null) {
                        trail.Add(neighbour);
                        break;
                    }
                }
            }

            return trail;
        }

        private static bool TryBuildRoadTrafficTrail(WorldTile source, WorldTile target, int maxTiles, out List<WorldTile> trail) {
            trail = null;
            Queue<WorldTile> open = new Queue<WorldTile>();
            Dictionary<long, long> parentByKey = new Dictionary<long, long>();
            HashSet<long> visited = new HashSet<long>();

            long sourceKey = RailTileRegistry.MakeTileKey(source);
            long targetKey = RailTileRegistry.MakeTileKey(target);
            open.Enqueue(source);
            visited.Add(sourceKey);
            parentByKey[sourceKey] = long.MinValue;

            int explored = 0;
            while (open.Count > 0 && explored < 256) {
                WorldTile current = open.Dequeue();
                explored++;
                if (current == target) {
                    trail = ReconstructRoadTrail(parentByKey, targetKey);
                    return trail != null && trail.Count >= 2;
                }

                foreach (WorldTile neighbour in GetRoadNeighbours(current)) {
                    long key = RailTileRegistry.MakeTileKey(neighbour);
                    if (!visited.Add(key)) {
                        continue;
                    }

                    parentByKey[key] = RailTileRegistry.MakeTileKey(current);
                    open.Enqueue(neighbour);
                }
            }

            return false;
        }

        private static List<WorldTile> ReconstructRoadTrail(Dictionary<long, long> parentByKey, long targetKey) {
            List<WorldTile> trail = new List<WorldTile>();
            long current = targetKey;
            while (current != long.MinValue) {
                WorldTile tile = RailTileRegistry.GetTileByKey(current);
                if (tile != null) {
                    trail.Add(tile);
                }

                if (!parentByKey.TryGetValue(current, out long parentKey)) {
                    break;
                }

                current = parentKey;
            }

            trail.Reverse();
            return trail;
        }

        private static IEnumerable<WorldTile> GetRoadNeighbours(WorldTile tile) {
            if (tile == null) {
                yield break;
            }

            if (IsRoadTile(tile.tile_up)) {
                yield return tile.tile_up;
            }

            if (IsRoadTile(tile.tile_down)) {
                yield return tile.tile_down;
            }

            if (IsRoadTile(tile.tile_left)) {
                yield return tile.tile_left;
            }

            if (IsRoadTile(tile.tile_right)) {
                yield return tile.tile_right;
            }
        }

        private static WorldTile FindNearestRoadTile(WorldTile origin, int radius) {
            if (origin == null || World.world == null) {
                return null;
            }

            if (IsRoadTile(origin)) {
                return origin;
            }

            for (int r = 1; r <= radius; r++) {
                for (int x = origin.x - r; x <= origin.x + r; x++) {
                    for (int y = origin.y - r; y <= origin.y + r; y++) {
                        WorldTile candidate = World.world.GetTile(x, y);
                        if (IsRoadTile(candidate)) {
                            return candidate;
                        }
                    }
                }
            }

            return null;
        }

        internal static bool IsRoadTile(WorldTile tile) {
            if (tile == null || RailTileRegistry.IsRailTilePassive(tile)) {
                return false;
            }

            TopTileType top = tile.top_type;
            if (top == null) {
                return false;
            }

            return top == TopTileLibrary.road
                || string.Equals(top.id, Main.ModernRoadTileId, StringComparison.Ordinal);
        }

        private static WorldTile FindCityAnchorTile(City city) {
            if (city?.buildings == null) {
                return null;
            }

            for (int i = 0; i < city.buildings.Count; i++) {
                Building building = city.buildings[i];
                BuildingAsset asset = StandaloneBuildingUtils.GetBuildingAsset(building);
                if (asset == null) {
                    continue;
                }

                string combined = string.Concat(asset.id ?? string.Empty, "|", asset.type ?? string.Empty)
                    .ToLowerInvariant();
                if (combined.Contains("hall") || combined.Contains("capital") || combined.Contains("center")) {
                    WorldTile hallTile = GetBuildingDoorTile(building) ?? building.current_tile;
                    if (hallTile != null) {
                        return hallTile;
                    }
                }
            }

            return city.buildings.Count > 0 ? city.buildings[0]?.current_tile : null;
        }

        private static void ApplyModernRoadSprites(TopTileType modernRoad) {
            if (modernRoad == null || TrainVisuals.ModernRoadSprite == null || TileSpritesTilesField == null) {
                return;
            }

            IList sourceTiles = TopTileLibrary.road?.sprites != null
                ? TileSpritesTilesField.GetValue(TopTileLibrary.road.sprites) as IList
                : null;

            modernRoad.sprites = new TileSprites();
            IList tiles = TileSpritesTilesField.GetValue(modernRoad.sprites) as IList;
            if (tiles == null) {
                return;
            }

            tiles.Clear();
            if (sourceTiles != null && sourceTiles.Count > 0) {
                for (int i = 0; i < sourceTiles.Count; i++) {
                    Tile sourceTile = sourceTiles[i] as Tile;
                    if (sourceTile == null) {
                        continue;
                    }

                    Tile cloned = UnityEngine.Object.Instantiate(sourceTile);
                    cloned.name = $"trainbox_modern_road_{i}";
                    cloned.sprite = TrainVisuals.ModernRoadSprite;
                    tiles.Add(cloned);
                }
            }

            if (tiles.Count == 0) {
                modernRoad.sprites.addVariation(TrainVisuals.ModernRoadSprite, "trainbox_modern_road");
            }
        }

        private static Kingdom GetCityKingdom(City city) {
            if (city == null || !city.hasKingdom()) {
                return null;
            }

            if (_cityKingdomField == null) {
                _cityKingdomField = typeof(City).GetField(
                    "kingdom",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            return _cityKingdomField?.GetValue(city) as Kingdom;
        }

        private static WorldTile GetBuildingDoorTile(Building building) {
            if (building == null) {
                return null;
            }

            if (_buildingDoorTileProperty == null) {
                _buildingDoorTileProperty = typeof(Building).GetProperty(
                    "door_tile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            if (_buildingDoorTileProperty != null) {
                return _buildingDoorTileProperty.GetValue(building, null) as WorldTile;
            }

            if (_buildingDoorTileField == null) {
                _buildingDoorTileField = typeof(Building).GetField(
                    "door_tile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            return _buildingDoorTileField?.GetValue(building) as WorldTile;
        }

        private static Sprite PickTrafficSprite(bool useModernRoads) {
            if (!useModernRoads) {
                return TrainVisuals.TinyCarriageSprite;
            }

            return TrainVisuals.TinyCarRightSprite ?? TrainVisuals.TinyCarDownSprite;
        }

        private static void MarkTileChanged(WorldTile tile) {
            if (tile == null || World.world == null) {
                return;
            }

            MapAction.makeTileChanged(tile);
            if (_updateDirtyTileMethod == null) {
                _updateDirtyTileMethod = typeof(MapBox).GetMethod(
                    "updateDirtyTile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(WorldTile) },
                    null
                );
            }

            _updateDirtyTileMethod?.Invoke(World.world, new object[] { tile });
        }

        private static void EnsureModernRoadRegistered() {
            if (TopTileLibrary.road == null || AssetManager.top_tiles.get(Main.ModernRoadTileId) != null) {
                return;
            }

            TopTileType modernRoad = AssetManager.top_tiles.clone(Main.ModernRoadTileId, TopTileLibrary.road.id);
            if (modernRoad == null) {
                return;
            }

            modernRoad.id = Main.ModernRoadTileId;
            modernRoad.road = true;
            modernRoad.can_build_on = true;
            modernRoad.considered_empty_tile = false;
            modernRoad.force_edge_variation = false;
            modernRoad.check_edge = false;
            ApplyModernRoadSprites(modernRoad);
            AssetManager.top_tiles.add(modernRoad);
        }

        private static void RefreshAllRoadTilesNow() {
            if (World.world == null) {
                return;
            }

            EnsureModernRoadRegistered();
            if (TryRefreshRoadTilesByWorldBounds()) {
                return;
            }

            if (World.world.cities?.list == null) {
                return;
            }

            foreach (City city in World.world.cities.list) {
                if (city != null) {
                    RefreshRoadTiles(city);
                }
            }
        }

        private static bool TryRefreshRoadTilesByWorldBounds() {
            if (!TryGetWorldBounds(out int width, out int height)) {
                return false;
            }

            TopTileType targetRoad = GetCurrentRoadTop();
            if (targetRoad == null) {
                return false;
            }

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    WorldTile tile = World.world.GetTile(x, y);
                    if (!IsRoadTile(tile) || tile.top_type == targetRoad) {
                        continue;
                    }

                    tile.setTopTileType(targetRoad, true);
                    MarkTileChanged(tile);
                }
            }

            return true;
        }

        private static bool TryGetWorldBounds(out int width, out int height) {
            width = 0;
            height = 0;
            if (World.world == null) {
                return false;
            }

            width = TryReadWorldDimension(ref _worldWidthMember, "width", "mapWidth", "w", "sizeX");
            height = TryReadWorldDimension(ref _worldHeightMember, "height", "mapHeight", "h", "sizeY");
            return width > 0 && height > 0;
        }

        private static int TryReadWorldDimension(ref MemberInfo cachedMember, params string[] names) {
            if (cachedMember == null) {
                Type type = typeof(World);
                for (int i = 0; i < names.Length && cachedMember == null; i++) {
                    cachedMember = type.GetField(names[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? (MemberInfo)type.GetProperty(names[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            try {
                switch (cachedMember) {
                    case FieldInfo field:
                        return Convert.ToInt32(field.GetValue(World.world));
                    case PropertyInfo property:
                        return Convert.ToInt32(property.GetValue(World.world, null));
                }
            }
            catch {
            }

            return 0;
        }
    }

    internal static class TrafficSpriteSystem {
        private static Transform _root;

        internal static void SpawnTrafficSprite(List<WorldTile> trail, Sprite sprite, float lifetime, bool useDirectionalCarSprite) {
            if (trail == null || trail.Count < 2 || sprite == null) {
                return;
            }

            EnsureRoot();
            if (_root == null) {
                return;
            }

            GameObject go = new GameObject("TrainboxTrafficSprite");
            go.transform.SetParent(_root, false);
            TrafficSpritePulse pulse = go.AddComponent<TrafficSpritePulse>();
            pulse.Initialize(trail, sprite, lifetime, useDirectionalCarSprite);
        }

        internal static void SpawnStaticTrafficSprite(WorldTile tile, Sprite sprite, float lifetime) {
            if (tile == null || sprite == null) {
                return;
            }

            EnsureRoot();
            if (_root == null) {
                return;
            }

            GameObject go = new GameObject("TrainboxTrafficStaticSprite");
            go.transform.SetParent(_root, false);
            TrafficStaticSpritePulse pulse = go.AddComponent<TrafficStaticSpritePulse>();
            pulse.Initialize(tile, sprite, lifetime);
        }

        private static void EnsureRoot() {
            if (_root != null) {
                return;
            }

            GameObject rootObject = GameObject.Find("TrainboxTrafficRoot");
            if (rootObject == null) {
                rootObject = new GameObject("TrainboxTrafficRoot");
                UnityEngine.Object.DontDestroyOnLoad(rootObject);
            }

            _root = rootObject.transform;
        }
    }

    internal sealed class TrafficSpritePulse : MonoBehaviour {
        private readonly List<Vector3> _points = new List<Vector3>();
        private SpriteRenderer _renderer;
        private float _duration;
        private float _elapsed;
        private bool _useDirectionalCarSprite;

        internal void Initialize(List<WorldTile> trail, Sprite sprite, float duration, bool useDirectionalCarSprite) {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.sortingOrder = 5000;
            _useDirectionalCarSprite = useDirectionalCarSprite;
            transform.localScale = useDirectionalCarSprite
                ? new Vector3(1.35f, 1.35f, 1f)
                : new Vector3(1.2f, 1.2f, 1f);

            for (int i = 0; i < trail.Count; i++) {
                WorldTile tile = trail[i];
                if (tile == null) {
                    continue;
                }

                _points.Add(tile.posV3 + new Vector3(0f, 0.28f, -0.2f));
            }

            if (_points.Count < 2) {
                UnityEngine.Object.Destroy(gameObject);
                return;
            }

            _duration = Mathf.Max(0.45f, duration);
            transform.position = _points[0];
            UpdateVisualDirection(_points[1] - _points[0]);
        }

        private void Update() {
            if (_renderer == null || _points.Count < 2) {
                UnityEngine.Object.Destroy(gameObject);
                return;
            }

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            float scaled = progress * (_points.Count - 1);
            int segment = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, _points.Count - 2);
            float segmentT = scaled - segment;

            Vector3 start = _points[segment];
            Vector3 end = _points[segment + 1];
            transform.position = Vector3.Lerp(start, end, segmentT);
            UpdateVisualDirection(end - start);

            Color color = _renderer.color;
            color.a = progress < 0.8f ? 1f : Mathf.InverseLerp(1f, 0.8f, progress);
            _renderer.color = color;

            if (progress >= 1f) {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        private void UpdateVisualDirection(Vector3 direction) {
            if (direction.sqrMagnitude <= 0.0001f) {
                return;
            }

            if (_useDirectionalCarSprite) {
                Sprite directional = TrainVisuals.GetDirectionalCarSprite(direction);
                if (directional != null) {
                    _renderer.sprite = directional;
                }

                transform.rotation = Quaternion.identity;
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    internal sealed class TrafficStaticSpritePulse : MonoBehaviour {
        private SpriteRenderer _renderer;
        private float _duration;
        private float _elapsed;

        internal void Initialize(WorldTile tile, Sprite sprite, float duration) {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.sortingOrder = 5000;
            transform.localScale = new Vector3(1.45f, 1.45f, 1f);
            transform.position = tile.posV3 + new Vector3(0f, 0.28f, -0.2f);
            _duration = Mathf.Max(0.8f, duration);
        }

        private void Update() {
            if (_renderer == null) {
                UnityEngine.Object.Destroy(gameObject);
                return;
            }

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            Color color = _renderer.color;
            color.a = progress < 0.7f ? 1f : Mathf.InverseLerp(1f, 0.7f, progress);
            _renderer.color = color;

            Vector3 scale = transform.localScale;
            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.04f;
            transform.localScale = new Vector3(scale.x > 0f ? 1.45f * pulse : 1.45f, 1.45f * pulse, 1f);

            if (progress >= 1f) {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }

    internal static class RoadCarAssets {
        private static string _assetId;
        private static MethodInfo _addTraitMethod;
        private static FieldInfo _decisionIdsField;

        internal static void Bootstrap() {
            EnsureRoadCarAsset();
        }

        internal static string EnsureRoadCarAsset() {
            if (!string.IsNullOrWhiteSpace(_assetId) && AssetManager.actor_library.get(_assetId) != null) {
                return _assetId;
            }

            string[] baseIds = { "human", "elf", "dwarf", "orc" };
            ActorAsset baseAsset = null;
            for (int i = 0; i < baseIds.Length && baseAsset == null; i++) {
                baseAsset = AssetManager.actor_library.get(baseIds[i]);
            }

            if (baseAsset == null) {
                return null;
            }

            ActorAsset asset = AssetManager.actor_library.get(Main.RoadCarAssetId);
            if (asset == null) {
                asset = AssetManager.actor_library.clone(Main.RoadCarAssetId, baseAsset.id);
                if (asset == null) {
                    return null;
                }

                asset.id = Main.RoadCarAssetId;
                AssetManager.actor_library.add(asset);
            }

            ConfigureRoadCarAsset(asset);
            _assetId = asset.id;
            return _assetId;
        }

        private static void ConfigureRoadCarAsset(ActorAsset asset) {
            if (asset == null) {
                return;
            }

            asset.name_locale = "Road Car";
            asset.special = false;
            asset.inspect_stats = false;
            asset.inspect_children = false;
            asset.can_be_moved_by_powers = true;
            asset.can_attack_buildings = false;
            asset.can_attack_brains = false;
            asset.only_melee_attack = true;
            asset.effect_damage = false;
            asset.has_baby_form = false;
            asset.use_items = false;
            asset.disable_jump_animation = true;
            asset.animation_speed_based_on_walk_speed = false;
            asset.can_flip = false;
            asset.die_on_blocks = false;
            asset.ignore_blocks = true;
            asset.inspect_avatar_scale = 0.28f;
            asset.age_spawn = Math.Max(asset.age_spawn, 18);
            asset.sound_death = null;

            if (asset.base_stats == null) {
                asset.base_stats = new BaseStats();
            }

            asset.base_stats["scale"] = 0.18f;
            asset.base_stats["size"] = 1f;
            asset.base_stats["health"] = 260f;
            asset.base_stats["armor"] = 24f;
            asset.base_stats["speed"] = 120f;
            asset.base_stats["attack_speed"] = -99f;
            asset.base_stats["damage"] = 0f;
            asset.base_stats["range"] = 0f;
            asset.base_stats["stamina"] = 0f;

            TrySetBoolField(asset, false, "special");
            TrySetBoolField(asset, false, "need_food", "needs_food", "can_turn_into_zombie", "can_turn_into_skeleton");
            TryClearDecisionIds(asset);
            TryAddTrait(asset, "immune");
            TryAddTrait(asset, "fire_proof");
            TryAddTrait(asset, "freeze_proof");
            TryAddTrait(asset, "light_lamp");

            asset.has_override_sprite = true;
            asset.get_override_sprite = GetCarSprite;
            TrainVisuals.SetCachedSprite(asset, TrainVisuals.TinyCarRightSprite ?? TrainVisuals.TinyCarDownSprite);
        }

        private static Sprite GetCarSprite(Actor actor) {
            return RoadCarActorSystem.GetSpriteForActor(actor);
        }

        private static void TryAddTrait(ActorAsset asset, string traitId) {
            if (asset == null || string.IsNullOrWhiteSpace(traitId)) {
                return;
            }

            try {
                if (_addTraitMethod == null) {
                    _addTraitMethod = asset.GetType().GetMethod(
                        "addTrait",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(string) },
                        null);
                }

                _addTraitMethod?.Invoke(asset, new object[] { traitId });
            }
            catch {
            }
        }

        private static void TryClearDecisionIds(ActorAsset asset) {
            if (asset == null) {
                return;
            }

            try {
                if (_decisionIdsField == null) {
                    _decisionIdsField = asset.GetType().GetField(
                        "decision_ids",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                _decisionIdsField?.SetValue(asset, new List<string>());
            }
            catch {
            }
        }

        private static void TrySetBoolField(ActorAsset asset, bool value, params string[] fieldNames) {
            if (asset == null || fieldNames == null) {
                return;
            }

            Type type = asset.GetType();
            for (int i = 0; i < fieldNames.Length; i++) {
                try {
                    FieldInfo field = type.GetField(fieldNames[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null && field.FieldType == typeof(bool)) {
                        field.SetValue(asset, value);
                    }
                }
                catch {
                }
            }
        }
    }

    internal static class RoadCarActorSystem {
        private const float StepInterval = 0.24f;
        private const float StaticLifetime = 16f;
        private const float MovingLifetime = 16f;
        private const float EndOfPathLingerSeconds = 6f;
        private const int MaxActiveCars = 8;
        private static readonly Dictionary<long, List<long>> PathByActorId = new Dictionary<long, List<long>>();
        private static readonly Dictionary<long, int> PathIndexByActorId = new Dictionary<long, int>();
        private static readonly Dictionary<long, float> NextStepAtByActorId = new Dictionary<long, float>();
        private static readonly Dictionary<long, float> ExpireAtByActorId = new Dictionary<long, float>();
        private static readonly Dictionary<long, Vector2> ForwardByActorId = new Dictionary<long, Vector2>();
        private static FieldInfo _actorCurrentTileField;
        private static FieldInfo _actorVelocityField;
        private static MethodInfo _setProfessionMethod;
        private static MethodInfo _clearTasksMethod;
        private static MethodInfo _cancelAllBehMethod;
        private static MethodInfo _setCurrentTilePositionMethod;

        internal static bool TrySpawnRoadCar(List<WorldTile> trail) {
            if (trail == null || trail.Count == 0 || World.world?.units == null) {
                return false;
            }

            CleanupInactiveCars();
            if (PathByActorId.Count >= MaxActiveCars) {
                return false;
            }

            string assetId = RoadCarAssets.EnsureRoadCarAsset();
            if (string.IsNullOrWhiteSpace(assetId)) {
                return false;
            }

            WorldTile spawnTile = trail[0];
            Actor actor = World.world.units.createNewUnit(assetId, spawnTile, true, 0.35f, null);
            if (actor == null) {
                return false;
            }

            Kingdom kingdom = ResolveRoadKingdom(spawnTile);
            if (kingdom != null) {
                actor.joinKingdom(kingdom);
                Subspecies subspecies = kingdom.getMainSubspecies();
                if (subspecies != null) {
                    actor.setSubspecies(subspecies);
                }
            }

            List<long> path = new List<long>(trail.Count);
            for (int i = 0; i < trail.Count; i++) {
                if (trail[i] != null) {
                    path.Add(RailTileRegistry.MakeTileKey(trail[i]));
                }
            }

            if (path.Count == 0) {
                actor.dieSimpleNone();
                return false;
            }

            PathByActorId[actor.id] = path;
            PathIndexByActorId[actor.id] = 0;
            NextStepAtByActorId[actor.id] = Time.time + StepInterval;
            ExpireAtByActorId[actor.id] = Time.time + (path.Count > 1 ? MovingLifetime : StaticLifetime);

            SafeCancelAllBeh(actor);
            SafeClearTasks(actor);
            TrySetIdleProfession(actor);
            actor.restoreHealthPercent(1f);
            SafeSnapActorToTile(actor, spawnTile);

            if (path.Count > 1) {
                WorldTile nextTile = RailTileRegistry.GetTileByKey(path[1]);
                if (nextTile != null) {
                    Vector2 direction = new Vector2(nextTile.x - spawnTile.x, nextTile.y - spawnTile.y);
                    if (direction.sqrMagnitude > 0.001f) {
                        ForwardByActorId[actor.id] = direction.normalized;
                    }
                }
            }

            return true;
        }

        internal static void UpdateActor(Actor actor) {
            if (!IsRoadCar(actor)) {
                return;
            }

            if (actor == null || !actor.isAlive()) {
                CleanupActor(actor?.id ?? -1L);
                return;
            }

            if (!PathByActorId.TryGetValue(actor.id, out List<long> path) || path.Count == 0) {
                CleanupActor(actor.id);
                return;
            }

            if (ExpireAtByActorId.TryGetValue(actor.id, out float expireAt) && Time.time >= expireAt) {
                CleanupAndDespawn(actor);
                return;
            }

            int index = PathIndexByActorId.TryGetValue(actor.id, out int currentIndex) ? currentIndex : 0;
            WorldTile currentTile = index >= 0 && index < path.Count
                ? RailTileRegistry.GetTileByKey(path[index])
                : actor.current_tile;

            if (path.Count <= 1) {
                if (currentTile != null && actor.current_tile != currentTile) {
                    SafeSnapActorToTile(actor, currentTile);
                }

                actor.makeWait(0.1f);
                return;
            }

            if (NextStepAtByActorId.TryGetValue(actor.id, out float nextStepAt) && Time.time < nextStepAt) {
                actor.makeWait(0.08f);
                return;
            }

            SafeCancelAllBeh(actor);
            SafeClearTasks(actor);
            TrySetIdleProfession(actor);
            actor.restoreHealthPercent(1f);
            actor.restoreHealth(8);

            if (currentTile != null) {
                SafeSnapActorToTile(actor, currentTile);
            }

            int nextIndex = index + 1;
            if (nextIndex >= path.Count) {
                if (TryExtendPathAtRoadEnd(actor.id, path, currentTile, index)) {
                    nextIndex = index + 1;
                } else {
                    if (ExpireAtByActorId.TryGetValue(actor.id, out float currentExpireAt)) {
                        ExpireAtByActorId[actor.id] = Mathf.Max(currentExpireAt, Time.time + EndOfPathLingerSeconds);
                    } else {
                        ExpireAtByActorId[actor.id] = Time.time + EndOfPathLingerSeconds;
                    }

                    actor.makeWait(0.05f);
                    return;
                }
            }

            WorldTile nextTile = RailTileRegistry.GetTileByKey(path[nextIndex]);
            if (nextTile == null || !RoadTrafficSystem.IsRoadTile(nextTile)) {
                if (!TryExtendPathAtRoadEnd(actor.id, path, currentTile, index)) {
                    ExpireAtByActorId[actor.id] = Time.time + EndOfPathLingerSeconds;
                    actor.makeWait(0.05f);
                    return;
                }

                nextIndex = index + 1;
                nextTile = RailTileRegistry.GetTileByKey(path[nextIndex]);
                if (nextTile == null || !RoadTrafficSystem.IsRoadTile(nextTile)) {
                    ExpireAtByActorId[actor.id] = Time.time + EndOfPathLingerSeconds;
                    actor.makeWait(0.05f);
                    return;
                }
            }

            if (currentTile != null) {
                Vector2 direction = new Vector2(nextTile.x - currentTile.x, nextTile.y - currentTile.y);
                if (direction.sqrMagnitude > 0.001f) {
                    ForwardByActorId[actor.id] = direction.normalized;
                }
            }

            PathIndexByActorId[actor.id] = nextIndex;
            NextStepAtByActorId[actor.id] = Time.time + StepInterval;
            SafeSnapActorToTile(actor, nextTile);
            actor.makeWait(0.04f);
        }

        internal static bool IsManagedRoadCarActor(Actor actor) {
            return IsRoadCar(actor);
        }

        internal static Sprite GetSpriteForActor(Actor actor) {
            if (actor == null) {
                return TrainVisuals.TinyCarRightSprite ?? TrainVisuals.TinyCarDownSprite;
            }

            if (ForwardByActorId.TryGetValue(actor.id, out Vector2 forward) && forward.sqrMagnitude > 0.001f) {
                return TrainVisuals.GetDirectionalCarSprite(new Vector3(forward.x, forward.y, 0f))
                    ?? TrainVisuals.TinyCarRightSprite
                    ?? TrainVisuals.TinyCarDownSprite;
            }

            return TrainVisuals.TinyCarRightSprite ?? TrainVisuals.TinyCarDownSprite;
        }

        private static bool IsRoadCar(Actor actor) {
            return actor?.asset != null && string.Equals(actor.asset.id, Main.RoadCarAssetId, StringComparison.Ordinal);
        }

        private static void CleanupInactiveCars() {
            List<long> staleIds = null;
            foreach (KeyValuePair<long, List<long>> entry in PathByActorId) {
                Actor actor = World.world?.units?.get(entry.Key);
                if (actor != null && actor.isAlive()) {
                    continue;
                }

                if (staleIds == null) {
                    staleIds = new List<long>();
                }

                staleIds.Add(entry.Key);
            }

            if (staleIds == null) {
                return;
            }

            for (int i = 0; i < staleIds.Count; i++) {
                CleanupActor(staleIds[i]);
            }
        }

        private static void CleanupAndDespawn(Actor actor) {
            if (actor == null) {
                return;
            }

            long actorId = actor.id;
            CleanupActor(actorId);
            if (actor.isAlive()) {
                actor.dieSimpleNone();
            }
        }

        private static void CleanupActor(long actorId) {
            if (actorId < 0L) {
                return;
            }

            PathByActorId.Remove(actorId);
            PathIndexByActorId.Remove(actorId);
            NextStepAtByActorId.Remove(actorId);
            ExpireAtByActorId.Remove(actorId);
            ForwardByActorId.Remove(actorId);
        }

        private static bool TryExtendPathAtRoadEnd(long actorId, List<long> path, WorldTile currentTile, int currentIndex) {
            if (path == null || currentTile == null) {
                return false;
            }

            WorldTile previousTile = null;
            if (currentIndex > 0 && currentIndex - 1 < path.Count) {
                previousTile = RailTileRegistry.GetTileByKey(path[currentIndex - 1]);
            }

            WorldTile nextTile = PickNextRoadTile(currentTile, previousTile, actorId);
            if (nextTile == null || nextTile == currentTile) {
                return false;
            }

            while (path.Count > currentIndex + 1) {
                path.RemoveAt(path.Count - 1);
            }

            path.Add(RailTileRegistry.MakeTileKey(nextTile));
            return true;
        }

        private static WorldTile PickNextRoadTile(WorldTile currentTile, WorldTile previousTile, long actorId) {
            if (currentTile == null) {
                return null;
            }

            List<WorldTile> candidates = new List<WorldTile>();
            if (currentTile.tile_up != null && RoadTrafficSystem.IsRoadTile(currentTile.tile_up)) {
                candidates.Add(currentTile.tile_up);
            }

            if (currentTile.tile_down != null && RoadTrafficSystem.IsRoadTile(currentTile.tile_down)) {
                candidates.Add(currentTile.tile_down);
            }

            if (currentTile.tile_left != null && RoadTrafficSystem.IsRoadTile(currentTile.tile_left)) {
                candidates.Add(currentTile.tile_left);
            }

            if (currentTile.tile_right != null && RoadTrafficSystem.IsRoadTile(currentTile.tile_right)) {
                candidates.Add(currentTile.tile_right);
            }

            if (candidates.Count == 0) {
                return null;
            }

            WorldTile best = null;
            float bestScore = float.MinValue;
            Vector2 forward = ForwardByActorId.TryGetValue(actorId, out Vector2 cachedForward)
                ? cachedForward
                : Vector2.zero;

            for (int i = 0; i < candidates.Count; i++) {
                WorldTile candidate = candidates[i];
                if (candidate == null) {
                    continue;
                }

                float score = 0f;
                if (candidate == previousTile) {
                    score -= 100f;
                } else {
                    score += 10f;
                }

                if (forward.sqrMagnitude > 0.001f) {
                    Vector2 candidateDirection = new Vector2(candidate.x - currentTile.x, candidate.y - currentTile.y).normalized;
                    score += Vector2.Dot(forward.normalized, candidateDirection) * 25f;
                }

                if (score > bestScore) {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static Kingdom ResolveRoadKingdom(WorldTile tile) {
            City city = tile?.zone?.city;
            if (city != null && city.hasKingdom()) {
                return GetCityKingdom(city);
            }

            if (World.world?.cities?.list == null || tile == null) {
                return null;
            }

            City nearestCity = null;
            float bestDistance = float.MaxValue;
            foreach (City candidate in World.world.cities.list) {
                if (candidate == null || !candidate.hasKingdom()) {
                    continue;
                }

                WorldTile cityTile = candidate.getTile();
                if (cityTile == null && candidate.buildings != null && candidate.buildings.Count > 0) {
                    cityTile = candidate.buildings[0]?.current_tile;
                }
                if (cityTile == null) {
                    continue;
                }

                float distance = Toolbox.DistTile(tile, cityTile);
                if (distance < bestDistance) {
                    bestDistance = distance;
                    nearestCity = candidate;
                }
            }

            return nearestCity != null ? GetCityKingdom(nearestCity) : null;
        }

        private static Kingdom GetCityKingdom(City city) {
            if (city == null || !city.hasKingdom()) {
                return null;
            }

            FieldInfo field = typeof(City).GetField(
                "kingdom",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            return field?.GetValue(city) as Kingdom;
        }

        private static void SafeCancelAllBeh(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                if (_cancelAllBehMethod == null) {
                    _cancelAllBehMethod = typeof(Actor).GetMethod(
                        "cancelAllBeh",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                _cancelAllBehMethod?.Invoke(actor, null);
            }
            catch {
            }
        }

        private static void TrySetIdleProfession(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                if (_setProfessionMethod == null) {
                    _setProfessionMethod = typeof(Actor).GetMethod(
                        "setProfession",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(UnitProfession), typeof(bool) },
                        null);
                }

                _setProfessionMethod?.Invoke(actor, new object[] { UnitProfession.Nothing, true });
            }
            catch {
            }
        }

        private static void SafeClearTasks(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                if (_clearTasksMethod == null) {
                    _clearTasksMethod = typeof(Actor).GetMethod(
                        "clearTasks",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                _clearTasksMethod?.Invoke(actor, null);
            }
            catch {
            }
        }

        private static void SafeSnapActorToTile(Actor actor, WorldTile tile) {
            if (actor == null || tile == null) {
                return;
            }

            SafeZeroVelocity(actor);
            actor.current_position = tile.pos;
            actor.next_step_position = tile.pos;
            actor.next_step_position_possession = tile.pos;
            SafeSetCurrentTilePosition(actor, tile);
        }

        private static void SafeZeroVelocity(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                if (_actorVelocityField == null) {
                    _actorVelocityField = typeof(Actor).GetField(
                        "velocity",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _actorVelocityField?.SetValue(actor, Vector3.zero);
            }
            catch {
            }
        }

        private static void SafeSetCurrentTilePosition(Actor actor, WorldTile tile) {
            if (actor == null || tile == null) {
                return;
            }

            try {
                if (_setCurrentTilePositionMethod == null) {
                    _setCurrentTilePositionMethod = typeof(Actor).GetMethod(
                        "setCurrentTilePosition",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(WorldTile) },
                        null
                    );
                }

                if (_setCurrentTilePositionMethod != null) {
                    _setCurrentTilePositionMethod.Invoke(actor, new object[] { tile });
                    return;
                }
            }
            catch {
            }

            try {
                if (_actorCurrentTileField == null) {
                    _actorCurrentTileField = typeof(Actor).GetField(
                        "current_tile",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _actorCurrentTileField?.SetValue(actor, tile);
            }
            catch {
            }
        }
    }

    internal static class TrainAssets {
        private static readonly Dictionary<string, string> AssetIdBySpecies = new Dictionary<string, string>();
        private static readonly string[] KnownVanillaSpecies = { "human", "elf", "orc", "dwarf" };
        private static FieldInfo _speciesAssetField;

        internal static void BootstrapKnownTrainAssets() {
            for (int i = 0; i < KnownVanillaSpecies.Length; i++) {
                string speciesId = KnownVanillaSpecies[i];
                ActorAsset baseAsset = AssetManager.actor_library.get(speciesId);
                if (baseAsset == null) {
                    continue;
                }

                EnsureTrainAssetForSpecies(speciesId, baseAsset);
            }
        }

        internal static string EnsureTrainAsset(Kingdom kingdom) {
            if (kingdom == null) {
                return null;
            }

            string speciesId = kingdom.getSpecies();
            if (string.IsNullOrWhiteSpace(speciesId)) {
                return null;
            }

            ActorAsset baseAsset = FindBaseAsset(kingdom, speciesId);
            if (baseAsset == null) {
                return null;
            }

            return EnsureTrainAssetForSpecies(speciesId, baseAsset);
        }

        private static string EnsureTrainAssetForSpecies(string speciesId, ActorAsset baseAsset) {
            if (string.IsNullOrWhiteSpace(speciesId) || baseAsset == null) {
                return null;
            }

            if (AssetIdBySpecies.TryGetValue(speciesId, out string cachedId)) {
                ActorAsset cachedAsset = AssetManager.actor_library.get(cachedId);
                if (cachedAsset != null) {
                    ConfigureTrainAsset(cachedAsset);
                    return cachedAsset.id;
                }
            }

            string assetId = $"{Main.TrainAssetPrefix}{Sanitize(speciesId)}";
            ActorAsset existing = AssetManager.actor_library.get(assetId);
            if (existing != null) {
                ConfigureTrainAsset(existing);
                AssetIdBySpecies[speciesId] = existing.id;
                return existing.id;
            }

            ActorAsset trainAsset = AssetManager.actor_library.clone(assetId, baseAsset.id);
            if (trainAsset == null) {
                return null;
            }

            trainAsset.id = assetId;
            ConfigureTrainAsset(trainAsset);
            AssetManager.actor_library.add(trainAsset);

            AssetIdBySpecies[speciesId] = trainAsset.id;
            return trainAsset.id;
        }

        private static void ConfigureTrainAsset(ActorAsset trainAsset) {
            if (trainAsset == null) {
                return;
            }

            trainAsset.name_locale = "Taxi Train";
            trainAsset.special = false;
            trainAsset.inspect_stats = true;
            trainAsset.inspect_children = true;
            trainAsset.can_be_moved_by_powers = true;
            trainAsset.can_attack_buildings = false;
            trainAsset.can_attack_brains = false;
            trainAsset.only_melee_attack = true;
            trainAsset.effect_damage = false;
            trainAsset.has_baby_form = false;
            trainAsset.age_spawn = Math.Max(trainAsset.age_spawn, 30);

            if (trainAsset.base_stats == null) {
                trainAsset.base_stats = new BaseStats();
            }

            trainAsset.base_stats["health"] = 260f;
            trainAsset.base_stats["armor"] = 24f;
            trainAsset.base_stats["speed"] = 90f;
            trainAsset.base_stats["attack_speed"] = -90f;
            trainAsset.base_stats["damage"] = 1f;
            trainAsset.base_stats["range"] = 0f;
            trainAsset.base_stats["stamina"] = 0f;

            if (TrainVisuals.TrainUnitSprite != null) {
                trainAsset.has_override_sprite = true;
                trainAsset.get_override_sprite = GetTrainSprite;
            }

            TrainVisuals.SetCachedSprite(trainAsset, TrainVisuals.TrainIcon);
        }

        private static Sprite GetTrainSprite(Actor actor) {
            return TrainVisuals.GetTrainSpriteForActor(actor);
        }

        private static ActorAsset FindBaseAsset(Kingdom kingdom, string speciesId) {
            ActorAsset direct = AssetManager.actor_library.get(speciesId);
            if (direct != null) {
                return direct;
            }

            ActorAsset kingdomAsset = kingdom.getActorAsset();
            if (kingdomAsset != null) {
                return kingdomAsset;
            }

            return GetSpeciesAsset(kingdom);
        }

        private static ActorAsset GetSpeciesAsset(Kingdom kingdom) {
            if (kingdom == null) {
                return null;
            }

            if (_speciesAssetField == null) {
                _speciesAssetField = typeof(Kingdom).GetField(
                    "species_asset",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return _speciesAssetField?.GetValue(kingdom) as ActorAsset;
        }

        private static string Sanitize(string raw) {
            char[] buffer = raw.ToCharArray();
            for (int i = 0; i < buffer.Length; i++) {
                char c = buffer[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) {
                    continue;
                }

                if (c >= 'A' && c <= 'Z') {
                    buffer[i] = char.ToLowerInvariant(c);
                    continue;
                }

                buffer[i] = '_';
            }

            return new string(buffer);
        }
    }

    internal static class RailAutoBuilder {
        private const float PlanIntervalSeconds = 90f;
        private const float MaxPartnerDistance = 140f;
        private const int CityStopAdoptionRadius = 8;
        private const int InfrastructureStopAdoptionRadius = 6;
        private const int MaxPathTiles = 240;
        private const int MaxConsecutiveBridgeTiles = 36;
        private const int MaxTotalBridgeTiles = 108;
        private const int MaxSearchStates = 2000;
        private const int MaxVillageLinksPerPass = 1;
        private const int MaxInfrastructureLinksPerPass = 1;
        private const int MaxPartnerCandidatesToInspect = 8;
        private const float MaxInfrastructureDistance = 60f;
        private const float ExistingRailCost = 0.45f;
        private const float LandBuildCost = 1f;
        private const float BridgeBuildCost = 1.85f;
        private const float ForeignFriendlyTilePenalty = 0.45f;
        private const float RoadCrossingPenalty = 5.5f;
        private const float AdjacentRoadPenalty = 0.9f;
        private const float AdjacentRailPenalty = 0.55f;
        private const float TurnPenaltyScore = 1.8f;
        private const float RoadTileScorePenalty = 2.4f;
        private const float MaxTurnRatio = 0.42f;

        private sealed class RailRoutePlan {
            internal City PartnerCity;
            internal WorldTile TargetStop;
            internal List<WorldTile> Path;
        }

        private sealed class RailPathState {
            internal string Key;
            internal string ParentKey;
            internal WorldTile Tile;
            internal float CostSoFar;
            internal float EstimatedTotal;
            internal int ConsecutiveBridgeTiles;
            internal int TotalBridgeTiles;
        }

        private static readonly Dictionary<long, float> NextPlanByCity = new Dictionary<long, float>();
        private static FieldInfo _cityKingdomField;
        private static FieldInfo _buildingAssetField;
        private static PropertyInfo _buildingDoorTileProperty;
        private static FieldInfo _buildingDoorTileField;
        private static FieldInfo _civUnitsField;

        internal static bool AutoCityRailsEnabled { get; private set; } = true;

        internal static bool ToggleAutoCityRails() {
            AutoCityRailsEnabled = !AutoCityRailsEnabled;
            return AutoCityRailsEnabled;
        }

        internal static void UpdateCity(City city) {
            if (!AutoCityRailsEnabled) {
                return;
            }

            if (!WorldboxEraCompat.IsRailEraUnlocked(city)) {
                return;
            }

            if (!CanPlanForCity(city)) {
                return;
            }

            float now = Time.time;
            if (NextPlanByCity.TryGetValue(city.id, out float nextPlanAt) && now < nextPlanAt) {
                return;
            }

            NextPlanByCity[city.id] = now + PlanIntervalSeconds + ((city.id % 7L) * 0.9f);

            WorldTile sourceStop = FindCityHallStop(city);
            if (sourceStop == null) {
                return;
            }

            BuildInfrastructureLinks(city, sourceStop);

            Kingdom sourceKingdom = GetCityKingdom(city);
            foreach (RailRoutePlan plan in FindTopPartnerPlans(city, sourceStop, MaxVillageLinksPerPass)) {
                if (plan == null
                    || plan.PartnerCity == null
                    || city.id > plan.PartnerCity.id
                    || plan.TargetStop == null
                    || plan.Path == null
                    || plan.Path.Count == 0
                    || plan.Path.Count > MaxPathTiles
                    || RailTileRegistry.AreRailTilesConnected(sourceStop, plan.TargetStop)
                    || WouldCreateDuplicateParallelRail(plan.Path, sourceStop, plan.TargetStop)
                    || !IsPathCleanEnough(plan.Path, sourceStop, plan.TargetStop)) {
                    continue;
                }

                PaintRailConnection(plan.Path, sourceStop, plan.TargetStop);
            }

            TryEnsureKingdomTrain(city, sourceStop);
        }

        private static bool CanPlanForCity(City city) {
            Kingdom kingdom = GetCityKingdom(city);
            return city != null
                && city.hasKingdom()
                && kingdom != null
                && kingdom.isCiv()
                && city.countBuildings() > 0
                && World.world != null;
        }

        private static List<RailRoutePlan> FindTopPartnerPlans(City sourceCity, WorldTile sourceStop, int maxPlans) {
            Kingdom sourceKingdom = GetCityKingdom(sourceCity);
            if (sourceCity == null || sourceKingdom == null || sourceStop == null || World.world?.kingdoms == null) {
                return new List<RailRoutePlan>();
            }

            List<(City Candidate, WorldTile Stop, float Distance, bool Foreign)> shortlist =
                new List<(City, WorldTile, float, bool)>();
            List<(RailRoutePlan Plan, float Score)> candidates = new List<(RailRoutePlan, float)>();

            foreach (Kingdom kingdom in World.world.kingdoms) {
                if (!IsFriendlyRailKingdom(sourceKingdom, kingdom)) {
                    continue;
                }

                bool foreignFriendlyKingdom = kingdom != sourceKingdom;
                if (foreignFriendlyKingdom && !IsKingdomAtPeace(sourceKingdom)) {
                    continue;
                }

                foreach (City candidate in kingdom.getCities()) {
                    if (candidate == null || candidate == sourceCity || !candidate.hasKingdom()) {
                        continue;
                    }

                    WorldTile candidateStop = FindCityHallStop(candidate);
                    if (candidateStop == null) {
                        continue;
                    }

                    float distance = Vector2.Distance(sourceStop.pos, candidateStop.pos);
                    if (distance < 8f || distance > MaxPartnerDistance) {
                        continue;
                    }

                    if (RailTileRegistry.AreRailTilesConnected(sourceStop, candidateStop)) {
                        continue;
                    }

                    shortlist.Add((candidate, candidateStop, distance, foreignFriendlyKingdom));
                }
            }

            shortlist.Sort((a, b) => {
                int foreignOrder = a.Foreign.CompareTo(b.Foreign);
                if (foreignOrder != 0) {
                    return foreignOrder;
                }

                return a.Distance.CompareTo(b.Distance);
            });

            int candidatesToInspect = Math.Min(shortlist.Count, MaxPartnerCandidatesToInspect);
            for (int i = 0; i < candidatesToInspect; i++) {
                var shortlisted = shortlist[i];
                Kingdom candidateKingdom = GetCityKingdom(shortlisted.Candidate);
                if (candidateKingdom == null) {
                    continue;
                }

                if (!TryBuildRailPath(sourceStop, shortlisted.Stop, sourceKingdom, candidateKingdom, out List<WorldTile> path)) {
                    continue;
                }

                if (path == null || path.Count == 0 || path.Count > MaxPathTiles) {
                    continue;
                }

                float score = shortlisted.Distance;
                if (candidateKingdom == sourceKingdom) {
                    score -= 24f;
                } else {
                    score += 18f;
                }

                score += ScorePathMessiness(path, sourceStop, shortlisted.Stop);

                candidates.Add((new RailRoutePlan {
                    PartnerCity = shortlisted.Candidate,
                    TargetStop = shortlisted.Stop,
                    Path = path
                }, score));
            }

            candidates.Sort((a, b) => a.Score.CompareTo(b.Score));
            List<RailRoutePlan> plans = new List<RailRoutePlan>();
            HashSet<long> usedStops = new HashSet<long>();

            for (int i = 0; i < candidates.Count && plans.Count < maxPlans; i++) {
                RailRoutePlan plan = candidates[i].Plan;
                if (plan?.TargetStop == null) {
                    continue;
                }

                long targetKey = RailTileRegistry.MakeTileKey(plan.TargetStop);
                if (!usedStops.Add(targetKey)) {
                    continue;
                }

                plans.Add(plan);
            }

            return plans;
        }

        private static bool TryBuildRailPath(WorldTile sourceStop, WorldTile targetStop, Kingdom sourceKingdom, Kingdom targetKingdom, out List<WorldTile> path) {
            path = null;
            if (sourceStop == null || targetStop == null || sourceKingdom == null || targetKingdom == null) {
                return false;
            }

            List<RailPathState> open = new List<RailPathState>();
            Dictionary<string, RailPathState> stateByKey = new Dictionary<string, RailPathState>();
            Dictionary<string, float> bestCostByKey = new Dictionary<string, float>();

            RailPathState start = new RailPathState {
                Key = MakeSearchStateKey(sourceStop, 0, 0),
                ParentKey = null,
                Tile = sourceStop,
                CostSoFar = 0f,
                EstimatedTotal = EstimateDistance(sourceStop, targetStop),
                ConsecutiveBridgeTiles = 0,
                TotalBridgeTiles = 0
            };

            open.Add(start);
            stateByKey[start.Key] = start;
            bestCostByKey[start.Key] = 0f;

            int statesVisited = 0;
            while (open.Count > 0 && statesVisited < MaxSearchStates) {
                int bestIndex = GetBestStateIndex(open);
                RailPathState current = open[bestIndex];
                open.RemoveAt(bestIndex);
                statesVisited++;

                if (current.Tile == targetStop) {
                    path = ReconstructPath(stateByKey, current.Key);
                    return path != null && path.Count > 0;
                }

                foreach (WorldTile neighbour in GetCardinalNeighbours(current.Tile)) {
                    if (!CanTraverseTile(neighbour, sourceKingdom, targetKingdom)) {
                        continue;
                    }

                    bool bridgeTile = RailTileRegistry.IsBridgeWaterTile(neighbour) && !RailTileRegistry.IsRailTilePassive(neighbour);
                    int consecutiveBridgeTiles = bridgeTile ? current.ConsecutiveBridgeTiles + 1 : 0;
                    int totalBridgeTiles = bridgeTile ? current.TotalBridgeTiles + 1 : current.TotalBridgeTiles;
                    if (consecutiveBridgeTiles > MaxConsecutiveBridgeTiles || totalBridgeTiles > MaxTotalBridgeTiles) {
                        continue;
                    }

                    float nextCost = current.CostSoFar + GetTraversalCost(neighbour, sourceKingdom, targetKingdom);
                    string stateKey = MakeSearchStateKey(neighbour, consecutiveBridgeTiles, totalBridgeTiles);
                    if (bestCostByKey.TryGetValue(stateKey, out float bestKnownCost) && bestKnownCost <= nextCost) {
                        continue;
                    }

                    RailPathState next = new RailPathState {
                        Key = stateKey,
                        ParentKey = current.Key,
                        Tile = neighbour,
                        CostSoFar = nextCost,
                        EstimatedTotal = nextCost + EstimateDistance(neighbour, targetStop),
                        ConsecutiveBridgeTiles = consecutiveBridgeTiles,
                        TotalBridgeTiles = totalBridgeTiles
                    };

                    open.Add(next);
                    stateByKey[stateKey] = next;
                    bestCostByKey[stateKey] = nextCost;
                }
            }

            return false;
        }

        private static int GetBestStateIndex(List<RailPathState> open) {
            int bestIndex = 0;
            float bestScore = open[0].EstimatedTotal;
            for (int i = 1; i < open.Count; i++) {
                float score = open[i].EstimatedTotal;
                if (score < bestScore) {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static List<WorldTile> ReconstructPath(Dictionary<string, RailPathState> stateByKey, string finalStateKey) {
            List<WorldTile> path = new List<WorldTile>();
            string currentKey = finalStateKey;
            while (!string.IsNullOrWhiteSpace(currentKey) && stateByKey.TryGetValue(currentKey, out RailPathState state)) {
                if (state.Tile != null) {
                    path.Add(state.Tile);
                }

                currentKey = state.ParentKey;
            }

            path.Reverse();
            return path;
        }

        private static IEnumerable<WorldTile> GetCardinalNeighbours(WorldTile tile) {
            if (tile == null) {
                yield break;
            }

            if (tile.tile_up != null) {
                yield return tile.tile_up;
            }

            if (tile.tile_down != null) {
                yield return tile.tile_down;
            }

            if (tile.tile_left != null) {
                yield return tile.tile_left;
            }

            if (tile.tile_right != null) {
                yield return tile.tile_right;
            }
        }

        private static bool CanTraverseTile(WorldTile tile, Kingdom sourceKingdom, Kingdom targetKingdom) {
            if (tile == null || !RailTileRegistry.CanBuildTrackOnTile(tile, true)) {
                return false;
            }

            Kingdom tileKingdom = GetCityKingdom(tile.zone?.city);
            if (tileKingdom == null || tileKingdom == sourceKingdom || tileKingdom == targetKingdom) {
                return true;
            }

            return IsFriendlyRailKingdom(sourceKingdom, tileKingdom) && IsFriendlyRailKingdom(targetKingdom, tileKingdom);
        }

        private static float GetTraversalCost(WorldTile tile, Kingdom sourceKingdom, Kingdom targetKingdom) {
            float cost;
            if (RailTileRegistry.IsRailTilePassive(tile)) {
                cost = ExistingRailCost;
            } else if (RailTileRegistry.IsBridgeWaterTile(tile)) {
                cost = BridgeBuildCost;
            } else {
                cost = LandBuildCost;
            }

            Kingdom tileKingdom = GetCityKingdom(tile.zone?.city);
            if (tileKingdom != null && tileKingdom != sourceKingdom && tileKingdom != targetKingdom) {
                cost += ForeignFriendlyTilePenalty;
            }

            if (RoadTrafficSystem.IsRoadTile(tile) && !RailTileRegistry.IsRailTilePassive(tile)) {
                cost += RoadCrossingPenalty;
            }

            cost += CountAdjacentRoadTiles(tile) * AdjacentRoadPenalty;
            cost += CountAdjacentForeignRailTiles(tile) * AdjacentRailPenalty;

            return cost;
        }

        private static float EstimateDistance(WorldTile from, WorldTile to) {
            return from == null || to == null
                ? 0f
                : Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        private static string MakeSearchStateKey(WorldTile tile, int consecutiveBridgeTiles, int totalBridgeTiles) {
            return tile == null
                ? "invalid"
                : string.Concat(tile.x, ":", tile.y, ":", consecutiveBridgeTiles, ":", totalBridgeTiles);
        }

        private static bool IsFriendlyRailKingdom(Kingdom source, Kingdom candidate) {
            if (source == null || candidate == null) {
                return false;
            }

            try {
                if (source.asset == null
                    || candidate.asset == null
                    || !source.isCiv()
                    || !candidate.isCiv()) {
                    return false;
                }

                if (source == candidate) {
                    return true;
                }

                if (source.isEnemy(candidate) || candidate.isEnemy(source)) {
                    return false;
                }

                Alliance sourceAlliance = source.getAlliance();
                Alliance candidateAlliance = candidate.getAlliance();
                if (sourceAlliance != null && sourceAlliance == candidateAlliance) {
                    return true;
                }

                if (World.world?.diplomacy == null) {
                    return false;
                }

                return source.isOpinionTowardsKingdomGood(candidate)
                    && candidate.isOpinionTowardsKingdomGood(source);
            }
            catch {
                return false;
            }
        }

        private static bool IsKingdomAtPeace(Kingdom kingdom) {
            if (kingdom == null || World.world?.kingdoms == null) {
                return false;
            }

            foreach (Kingdom other in World.world.kingdoms) {
                if (other == null || other == kingdom) {
                    continue;
                }

                if (kingdom.isEnemy(other) || other.isEnemy(kingdom)) {
                    return false;
                }
            }

            return true;
        }

        private static WorldTile FindCityHallStop(City city) {
            Building hall = FindHallBuilding(city);
            if (hall == null) {
                return null;
            }

            WorldTile tile = GetBuildingDoorTile(hall) ?? hall.current_tile;
            if (tile == null) {
                return null;
            }

            WorldTile stopTile = RailTileRegistry.FindNearestStop(tile, CityStopAdoptionRadius);
            if (stopTile != null) {
                return stopTile;
            }

            WorldTile nearbyRail = RailTileRegistry.FindNearestTrack(tile, CityStopAdoptionRadius);
            if (nearbyRail != null && CanUseAsRailStop(nearbyRail)) {
                RailTileRegistry.PaintStop(nearbyRail);
                return nearbyRail;
            }

            WorldTile preferred = FindPreferredNewStopTile(tile, CityStopAdoptionRadius);
            if (preferred == null) {
                return null;
            }

            RailTileRegistry.PaintStop(preferred);
            return preferred;
        }

        private static void BuildInfrastructureLinks(City city, WorldTile sourceStop) {
            Kingdom sourceKingdom = GetCityKingdom(city);
            if (city?.buildings == null || sourceStop == null || sourceKingdom == null) {
                return;
            }

            List<(WorldTile Stop, float Score)> candidates = new List<(WorldTile, float)>();
            foreach (Building building in city.buildings) {
                BuildingAsset asset = GetBuildingAsset(building);
                if (!IsKeyInfrastructureBuilding(asset)) {
                    continue;
                }

                WorldTile stopTile = FindBuildingServiceStop(building, InfrastructureStopAdoptionRadius);
                if (stopTile == null || stopTile == sourceStop) {
                    continue;
                }

                if (RailTileRegistry.AreRailTilesConnected(sourceStop, stopTile)) {
                    continue;
                }

                float distance = Vector2.Distance(sourceStop.pos, stopTile.pos);
                if (distance <= 2f || distance > MaxInfrastructureDistance) {
                    continue;
                }

                float score = distance - ScoreInfrastructureBuilding(asset) * 0.1f;
                candidates.Add((stopTile, score));
            }

            candidates.Sort((a, b) => a.Score.CompareTo(b.Score));
            HashSet<long> usedStops = new HashSet<long>();
            int built = 0;
            for (int i = 0; i < candidates.Count && built < MaxInfrastructureLinksPerPass; i++) {
                WorldTile targetStop = candidates[i].Stop;
                if (targetStop == null) {
                    continue;
                }

                long key = RailTileRegistry.MakeTileKey(targetStop);
                if (!usedStops.Add(key) || RailTileRegistry.AreRailTilesConnected(sourceStop, targetStop)) {
                    continue;
                }

                if (!TryBuildRailPath(sourceStop, targetStop, sourceKingdom, sourceKingdom, out List<WorldTile> path)
                    || path == null
                    || path.Count == 0
                    || path.Count > MaxPathTiles
                    || WouldCreateDuplicateParallelRail(path, sourceStop, targetStop)
                    || !IsPathCleanEnough(path, sourceStop, targetStop)) {
                    continue;
                }

                PaintRailConnection(path, sourceStop, targetStop);
                built++;
            }
        }

        private static WorldTile FindBuildingServiceStop(Building building, int adoptionRadius) {
            if (building == null) {
                return null;
            }

            WorldTile tile = GetBuildingDoorTile(building) ?? building.current_tile;
            if (tile == null) {
                return null;
            }

            WorldTile stopTile = RailTileRegistry.FindNearestStop(tile, adoptionRadius);
            if (stopTile != null) {
                return stopTile;
            }

            WorldTile nearbyRail = RailTileRegistry.FindNearestTrack(tile, adoptionRadius);
            if (nearbyRail != null && CanUseAsRailStop(nearbyRail)) {
                RailTileRegistry.PaintStop(nearbyRail);
                return nearbyRail;
            }

            WorldTile preferred = FindPreferredNewStopTile(tile, adoptionRadius);
            if (preferred == null) {
                return null;
            }

            RailTileRegistry.PaintStop(preferred);
            return preferred;
        }

        private static bool IsKeyInfrastructureBuilding(BuildingAsset asset) {
            return ScoreInfrastructureBuilding(asset) > 0;
        }

        private static int ScoreInfrastructureBuilding(BuildingAsset asset) {
            if (asset == null) {
                return 0;
            }

            string combined = StandaloneBuildingUtils.GetAssetSignature(asset);
            int score = 0;

            if (combined.Contains("windmill") || combined.Contains("mill")) {
                score += 120;
            }

            if (combined.Contains("dock") || combined.Contains("harbor") || combined.Contains("harbour") || combined.Contains("port")) {
                score += 110;
            }

            if (combined.Contains("mine") || combined.Contains("quarry")) {
                score += 95;
            }

            if (combined.Contains("workshop") || combined.Contains("smith") || combined.Contains("forge")) {
                score += 80;
            }

            if (combined.Contains("market") || combined.Contains("bonfire")) {
                score += 60;
            }

            if (combined.Contains("farm") || combined.Contains("granary")) {
                score += 45;
            }

            return score;
        }

        private static bool CanUseAsRailStop(WorldTile tile) {
            return tile != null
                && (RailTileRegistry.IsRailTilePassive(tile)
                    || (tile.Type != null
                        && tile.Type.ground
                        && !tile.Type.block
                        && !tile.Type.ocean
                        && !tile.Type.liquid
                        && !RoadTrafficSystem.IsRoadTile(tile)));
        }

        private static WorldTile FindPreferredNewStopTile(WorldTile origin, int radius) {
            if (origin == null || World.world == null) {
                return null;
            }

            if (CanUseAsRailStop(origin) && CountAdjacentRoadTiles(origin) <= 1) {
                return origin;
            }

            WorldTile best = null;
            float bestScore = float.MaxValue;

            for (int r = 1; r <= radius; r++) {
                for (int x = origin.x - r; x <= origin.x + r; x++) {
                    for (int y = origin.y - r; y <= origin.y + r; y++) {
                        WorldTile candidate = World.world.GetTile(x, y);
                        if (!CanUseAsRailStop(candidate)) {
                            continue;
                        }

                        float score = Mathf.Abs(candidate.x - origin.x) + Mathf.Abs(candidate.y - origin.y);
                        score += CountAdjacentRoadTiles(candidate) * 1.2f;
                        score += CountAdjacentForeignRailTiles(candidate) * 0.4f;
                        if (score < bestScore) {
                            bestScore = score;
                            best = candidate;
                        }
                    }
                }

                if (best != null) {
                    return best;
                }
            }

            return null;
        }

        private static Building FindHallBuilding(City city) {
            if (city?.buildings == null) {
                return null;
            }

            Building best = null;
            int bestScore = int.MinValue;
            foreach (Building building in city.buildings) {
                BuildingAsset asset = GetBuildingAsset(building);
                if (asset == null) {
                    continue;
                }

                int score = ScoreHallBuilding(asset);
                if (score > bestScore) {
                    bestScore = score;
                    best = building;
                }
            }

            return bestScore > int.MinValue ? best : null;
        }

        private static BuildingAsset GetBuildingAsset(Building building) {
            if (building == null) {
                return null;
            }

            if (_buildingAssetField == null) {
                _buildingAssetField = typeof(Building).GetField(
                    "asset",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return _buildingAssetField?.GetValue(building) as BuildingAsset;
        }

        private static WorldTile GetBuildingDoorTile(Building building) {
            if (building == null) {
                return null;
            }

            if (_buildingDoorTileProperty == null) {
                _buildingDoorTileProperty = typeof(Building).GetProperty(
                    "door_tile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (_buildingDoorTileProperty != null) {
                return _buildingDoorTileProperty.GetValue(building, null) as WorldTile;
            }

            if (_buildingDoorTileField == null) {
                _buildingDoorTileField = typeof(Building).GetField(
                    "door_tile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return _buildingDoorTileField?.GetValue(building) as WorldTile;
        }

        private static bool LooksLikeHallAsset(BuildingAsset asset) {
            if (asset == null) {
                return false;
            }

            string id = asset.id ?? string.Empty;
            string type = asset.type ?? string.Empty;
            string group = asset.group ?? string.Empty;
            return id.IndexOf("hall", StringComparison.OrdinalIgnoreCase) >= 0
                || id.IndexOf("capital", StringComparison.OrdinalIgnoreCase) >= 0
                || id.IndexOf("town", StringComparison.OrdinalIgnoreCase) >= 0
                || id.IndexOf("center", StringComparison.OrdinalIgnoreCase) >= 0
                || type.IndexOf("hall", StringComparison.OrdinalIgnoreCase) >= 0
                || type.IndexOf("center", StringComparison.OrdinalIgnoreCase) >= 0
                || group.IndexOf("center", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int ScoreHallBuilding(BuildingAsset asset) {
            if (asset == null) {
                return int.MinValue;
            }

            string combined = StandaloneBuildingUtils.GetAssetSignature(asset);

            int score = 0;
            if (asset.build_place_center) {
                score += 120;
            }

            if (LooksLikeHallAsset(asset)) {
                score += 1200;
            }

            if (combined.Contains("townhall")
                || combined.Contains("town_hall")
                || combined.Contains("cityhall")
                || combined.Contains("city_hall")
                || combined.Contains("hall")
                || combined.Contains("capital")
                || combined.Contains("center")) {
                score += 300;
            }

            if (combined.Contains("bonfire")) {
                score += 120;
            }

            if (combined.Contains("windmill")
                || combined.Contains("mill")
                || combined.Contains("farm")
                || combined.Contains("mine")
                || combined.Contains("barracks")
                || combined.Contains("house")
                || combined.Contains("watchtower")
                || combined.Contains("dock")
                || combined.Contains("temple")
                || combined.Contains("hut")) {
                score -= 900;
            }

            return score;
        }

        private static void PaintRailConnection(List<WorldTile> path, WorldTile sourceStop, WorldTile targetStop) {
            if (path == null || path.Count == 0) {
                return;
            }

            for (int i = 0; i < path.Count; i++) {
                WorldTile tile = path[i];
                if (tile == null) {
                    continue;
                }

                bool isEndpoint = tile == sourceStop || tile == targetStop || i == 0 || i == path.Count - 1;
                if (isEndpoint) {
                    RailTileRegistry.PaintStop(tile);
                } else if (!RailTileRegistry.IsStopTilePassive(tile)) {
                    RailTileRegistry.PaintTrack(tile);
                }
            }

            RailTileRegistry.PaintStop(sourceStop);
            RailTileRegistry.PaintStop(targetStop);
        }

        private static bool WouldCreateDuplicateParallelRail(List<WorldTile> path, WorldTile sourceStop, WorldTile targetStop) {
            if (path == null || path.Count < 3) {
                return false;
            }

            HashSet<long> pathKeys = new HashSet<long>();
            for (int i = 0; i < path.Count; i++) {
                if (path[i] != null) {
                    pathKeys.Add(RailTileRegistry.MakeTileKey(path[i]));
                }
            }

            int parallelCount = 0;
            int freshTrackCount = 0;
            for (int i = 1; i < path.Count - 1; i++) {
                WorldTile tile = path[i];
                if (tile == null || tile == sourceStop || tile == targetStop) {
                    continue;
                }

                if (RailTileRegistry.IsRailTilePassive(tile)) {
                    continue;
                }

                freshTrackCount++;
                if (HasAdjacentForeignRail(tile, pathKeys)) {
                    parallelCount++;
                }
            }

            if (freshTrackCount == 0) {
                return true;
            }

            return parallelCount >= Math.Max(3, freshTrackCount / 2);
        }

        private static bool HasAdjacentForeignRail(WorldTile tile, HashSet<long> pathKeys) {
            if (tile == null) {
                return false;
            }

            WorldTile[] neighbours = {
                tile.tile_up,
                tile.tile_down,
                tile.tile_left,
                tile.tile_right
            };

            for (int i = 0; i < neighbours.Length; i++) {
                WorldTile neighbour = neighbours[i];
                if (neighbour == null) {
                    continue;
                }

                long key = RailTileRegistry.MakeTileKey(neighbour);
                if (pathKeys.Contains(key)) {
                    continue;
                }

                if (RailTileRegistry.IsRailTilePassive(neighbour)) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPathCleanEnough(List<WorldTile> path, WorldTile sourceStop, WorldTile targetStop) {
            if (path == null || path.Count < 2) {
                return false;
            }

            int turnCount = 0;
            int interiorTiles = 0;
            int roadTiles = 0;
            Vector2Int previousDirection = Vector2Int.zero;

            for (int i = 1; i < path.Count; i++) {
                WorldTile previous = path[i - 1];
                WorldTile current = path[i];
                if (previous == null || current == null) {
                    continue;
                }

                Vector2Int direction = new Vector2Int(
                    Math.Sign(current.x - previous.x),
                    Math.Sign(current.y - previous.y));

                if (previousDirection != Vector2Int.zero && direction != previousDirection) {
                    turnCount++;
                }

                previousDirection = direction;

                if (current == sourceStop || current == targetStop) {
                    continue;
                }

                interiorTiles++;
                if (RoadTrafficSystem.IsRoadTile(current) && !RailTileRegistry.IsRailTilePassive(current)) {
                    roadTiles++;
                }
            }

            if (interiorTiles <= 0) {
                return true;
            }

            if (roadTiles > Math.Max(1, interiorTiles / 8)) {
                return false;
            }

            return turnCount <= Mathf.CeilToInt(interiorTiles * MaxTurnRatio);
        }

        private static float ScorePathMessiness(List<WorldTile> path, WorldTile sourceStop, WorldTile targetStop) {
            if (path == null || path.Count < 2) {
                return 999f;
            }

            int turnCount = 0;
            int roadTiles = 0;
            Vector2Int previousDirection = Vector2Int.zero;

            for (int i = 1; i < path.Count; i++) {
                WorldTile previous = path[i - 1];
                WorldTile current = path[i];
                if (previous == null || current == null) {
                    continue;
                }

                Vector2Int direction = new Vector2Int(
                    Math.Sign(current.x - previous.x),
                    Math.Sign(current.y - previous.y));

                if (previousDirection != Vector2Int.zero && direction != previousDirection) {
                    turnCount++;
                }

                previousDirection = direction;

                if (current != sourceStop && current != targetStop && RoadTrafficSystem.IsRoadTile(current) && !RailTileRegistry.IsRailTilePassive(current)) {
                    roadTiles++;
                }
            }

            return turnCount * TurnPenaltyScore + roadTiles * RoadTileScorePenalty;
        }

        private static int CountAdjacentRoadTiles(WorldTile tile) {
            if (tile == null) {
                return 0;
            }

            int count = 0;
            if (RoadTrafficSystem.IsRoadTile(tile.tile_up)) count++;
            if (RoadTrafficSystem.IsRoadTile(tile.tile_down)) count++;
            if (RoadTrafficSystem.IsRoadTile(tile.tile_left)) count++;
            if (RoadTrafficSystem.IsRoadTile(tile.tile_right)) count++;
            return count;
        }

        private static int CountAdjacentForeignRailTiles(WorldTile tile) {
            if (tile == null) {
                return 0;
            }

            int count = 0;
            if (IsAdjacentForeignRail(tile.tile_up)) count++;
            if (IsAdjacentForeignRail(tile.tile_down)) count++;
            if (IsAdjacentForeignRail(tile.tile_left)) count++;
            if (IsAdjacentForeignRail(tile.tile_right)) count++;
            return count;
        }

        private static bool IsAdjacentForeignRail(WorldTile tile) {
            return tile != null && RailTileRegistry.IsRailTilePassive(tile);
        }

        private static void TryEnsureKingdomTrain(City city, WorldTile sourceStop) {
            Kingdom kingdom = GetCityKingdom(city);
            if (kingdom == null || sourceStop == null || !RailTileRegistry.HasAnotherConnectedStop(sourceStop)) {
                return;
            }

            if (!IsPrimaryCity(city, kingdom) || HasTrainForKingdom(kingdom) || TaxiTrainLogic.FindTrainOnTile(sourceStop) != null) {
                return;
            }

            string assetId = TrainAssets.EnsureTrainAsset(kingdom);
            if (string.IsNullOrWhiteSpace(assetId)) {
                return;
            }

            Subspecies subspecies = kingdom.getMainSubspecies();
            Actor actor = World.world.units.createNewUnit(assetId, sourceStop, true, 0.5f, subspecies);
            if (actor == null) {
                return;
            }

            actor.joinKingdom(kingdom);
            if (subspecies != null) {
                actor.setSubspecies(subspecies);
            }

            TaxiTrainLogic.SafeClearTasks(actor);
            TaxiTrainLogic.SafeSetStatsDirty(actor);
            actor.restoreHealthPercent(1f);
            TaxiTrainLogic.InitializeTrain(actor);
        }

        private static bool IsPrimaryCity(City city, Kingdom kingdom) {
            if (city == null || kingdom == null) {
                return false;
            }

            long lowestId = long.MaxValue;
            foreach (City candidate in kingdom.getCities()) {
                if (candidate != null && candidate.id < lowestId) {
                    lowestId = candidate.id;
                }
            }

            return city.id == lowestId;
        }

        private static bool HasTrainForKingdom(Kingdom kingdom) {
            if (kingdom == null || World.world?.units == null) {
                return false;
            }

            if (_civUnitsField == null) {
                _civUnitsField = typeof(ActorManager).GetField(
                    "units_only_civ",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            IEnumerable units = _civUnitsField?.GetValue(World.world.units) as IEnumerable;
            if (units == null) {
                return false;
            }

            foreach (object obj in units) {
                Actor actor = obj as Actor;
                if (actor != null && actor.isAlive() && actor.kingdom == kingdom && TaxiTrainLogic.IsTaxiTrainActor(actor)) {
                    return true;
                }
            }

            return false;
        }

        private static Kingdom GetCityKingdom(City city) {
            if (city == null || !city.hasKingdom()) {
                return null;
            }

            if (_cityKingdomField == null) {
                _cityKingdomField = typeof(City).GetField(
                    "kingdom",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return _cityKingdomField?.GetValue(city) as Kingdom;
        }
    }

    internal static class TrainPowers {
        private static MethodInfo _loopBrushMethod;
        private static MethodInfo _spawnDropsMethod;
        private static MethodInfo _flashPixelMethod;
        private static MethodInfo _drawingSoundMethod;
        private static FieldInfo _cachedDropField;
        private static FieldInfo _powersTabButtonField;
        private static MethodInfo _powersTabShowTabMethod;
        private static PowersTab _tab;
        private static GameObject _standaloneTabEntryObject;
        private static readonly List<PendingTabButton> _pendingTabButtons = new List<PendingTabButton>();

        private sealed class PendingTabButton {
            internal string Id;
            internal Sprite Icon;
            internal string Title;
            internal string Description;
        }

        internal static void Register() {
            RegisterTrackBrush();
            RegisterRoadBrush();
            RegisterCarEffectBrush();
            RegisterStopBrush();
            RegisterSpawnTrainPower();
            RegisterRemoveTrainPower();
            RegisterModernRoadsPower();
            RegisterAutoRailsPower();
            EnsureTab();
        }

        private static PowersTab EnsureTab() {
            if (_tab != null) return _tab;

            GameObject existingTabObject = GameObjects.FindEvenInactive(Main.TabId);
            if (existingTabObject != null) {
                _tab = existingTabObject.GetComponent<PowersTab>();
            }

            if (_tab == null) {
                Localization.AddOrSet(Main.TabId, "Trainbox");
                Localization.AddOrSet($"{Main.TabId}_info", "Paint rail lines and spawn trains.");

                try {
                    new global::ModernBox.TabBuilder()
                        .SetTabID(Main.TabId)
                        .SetName("Trainbox")
                        .SetDescription("Paint rail lines and spawn trains.")
                        .SetPosition(200)
                        .isAfrican(true)
                        .SetToolbarButtonVisible(false)
                        .SetRepeatPressReturnTab("ModernBoxTab")
                        .SetIcon("ui/icons/Industrial")
                        .Build();

                    existingTabObject = GameObjects.FindEvenInactive(Main.TabId);
                    if (existingTabObject != null) {
                        _tab = existingTabObject.GetComponent<PowersTab>();
                    }
                }
                catch {
                }
            }

            if (_tab == null) {
            Localization.AddOrSet(Main.TabId, "Trainbox");
            Localization.AddOrSet($"{Main.TabId}_info", "Paint rail lines and spawn trains.");
            _tab = TabManager.CreateTab(
                Main.TabId,
                "Trainbox",
                "Paint rail lines and spawn trains.",
                TrainVisuals.TrainIcon
            );
            }

            if (_tab != null && _tab._asset != null) {
                _tab._asset.tab_type_main = false;
            }

            if (_tab != null && _tab.powerButton != null) {
                _tab.powerButton.gameObject.SetActive(false);
                RectTransform powerButtonRect = _tab.powerButton.transform as RectTransform;
                if (powerButtonRect != null) {
                    powerButtonRect.anchoredPosition = new Vector2(-5000f, -5000f);
                    powerButtonRect.localScale = Vector3.zero;
                }
            }

            EnsureTabButtons();
            HideStandaloneTabButton();
            return _tab;
        }

        internal static void EnsureStandaloneTrainboxTab() {
            EnsureTab();
        }

        internal static bool HasStandaloneLauncher() {
            return false;
        }

        internal static bool IsStandaloneTabButtonHidden() {
            if (_tab != null && _tab.powerButton != null && !_tab.powerButton.gameObject.activeSelf) {
                return true;
            }

            return _standaloneTabEntryObject != null;
        }

        internal static void OpenTrainboxTab() {
            PowersTab trainboxTab = EnsureTab();
            if (trainboxTab == null || trainboxTab.powerButton == null) {
                return;
            }

            trainboxTab.showTab(trainboxTab.powerButton);
        }

        private static void QueueTabButton(string powerId, Sprite icon, string title, string description) {
            if (string.IsNullOrWhiteSpace(powerId)) {
                return;
            }

            if (_pendingTabButtons.Exists(button => string.Equals(button.Id, powerId, StringComparison.Ordinal))) {
                return;
            }

            _pendingTabButtons.Add(new PendingTabButton {
                Id = powerId,
                Icon = icon,
                Title = title,
                Description = description
            });

            if (_tab != null) {
                EnsureTabButtons();
            }
        }

        private static void EnsureTabButtons() {
            if (_tab == null || PowerButtons.CustomButtons == null) {
                return;
            }

            foreach (PendingTabButton pending in _pendingTabButtons) {
                if (pending == null || string.IsNullOrWhiteSpace(pending.Id)) {
                    continue;
                }

                if (PowerButtons.CustomButtons.TryGetValue(pending.Id, out PowerButton existingButton) && existingButton != null) {
                    continue;
                }

                PowerButtons.CreateButton(
                    pending.Id,
                    pending.Icon,
                    pending.Title,
                    pending.Description,
                    new Vector2(72f, 18f),
                    ButtonType.GodPower,
                    _tab.transform,
                    null
                );
            }
        }

        private static void RegisterTrackBrush() {
            if (AssetManager.drops.get(Main.TrackDropId) == null) {
                DropAsset drop = new DropAsset {
                    id = Main.TrackDropId,
                    path_texture = "ui/drops/peachpunch",
                    random_frame = true,
                    default_scale = 0.2f,
                    falling_height = new Vector2(30f, 45f),
                    sound_drop = "event:/SFX/DROPS/DropRain",
                    type = DropType.DropSeed,
                    surprises_units = false,
                    action_landed = PaintTrackAtTile
                };

                AssetManager.drops.add(drop);
            }

            if (AssetManager.powers.get(Main.TrackPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.TrackPowerId,
                name = "Track Brush",
                type = PowerActionType.PowerSpawnDrops,
                drop_id = Main.TrackDropId,
                click_power_action = SpawnDrops,
                show_close_actor = true,
                unselect_when_window = true,
                can_drag_map = true,
                hold_action = true,
                mouse_hold_animation = MouseHoldAnimation.Draw,
                falling_chance = 0.02f,
                show_tool_sizes = true
            };

            EnsureCachedDrop(power, Main.TrackDropId);
            power.click_power_action = (PowerAction)Delegate.Combine(power.click_power_action, new PowerAction(FlashPixel));
            power.click_power_action = (PowerAction)Delegate.Combine(power.click_power_action, new PowerAction(PlayDrawingSound));
            power.click_power_brush_action = new PowerAction((tile, godPower) => {
                object result = GetLoopBrushMethod()?.Invoke(AssetManager.powers, new object[] { tile, godPower });
                return result is bool value && value;
            });

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.TrackPowerId, "Track Brush");
            Localization.AddOrSet($"{Main.TrackPowerId}_info", "Paint rail track tiles on land or across water.");

            QueueTabButton(Main.TrackPowerId, TrainVisuals.TrainIcon, "Track Brush", "Paint train tracks and bridges.");
        }

        private static void RegisterRoadBrush() {
            if (AssetManager.drops.get(Main.RoadDropId) == null) {
                DropAsset drop = new DropAsset {
                    id = Main.RoadDropId,
                    path_texture = "ui/drops/peachpunch",
                    random_frame = true,
                    default_scale = 0.2f,
                    falling_height = new Vector2(30f, 45f),
                    sound_drop = "event:/SFX/DROPS/DropRain",
                    type = DropType.DropSeed,
                    surprises_units = false,
                    action_landed = PaintRoadAtTile
                };

                AssetManager.drops.add(drop);
            }

            if (AssetManager.powers.get(Main.RoadPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.RoadPowerId,
                name = "Road Brush",
                type = PowerActionType.PowerSpawnDrops,
                drop_id = Main.RoadDropId,
                click_power_action = SpawnDrops,
                show_close_actor = true,
                unselect_when_window = true,
                can_drag_map = true,
                hold_action = true,
                mouse_hold_animation = MouseHoldAnimation.Draw,
                falling_chance = 0.02f,
                show_tool_sizes = true
            };

            EnsureCachedDrop(power, Main.RoadDropId);
            power.click_power_action = (PowerAction)Delegate.Combine(power.click_power_action, new PowerAction(FlashPixel));
            power.click_power_action = (PowerAction)Delegate.Combine(power.click_power_action, new PowerAction(PlayDrawingSound));
            power.click_power_brush_action = new PowerAction((tile, godPower) => {
                object result = GetLoopBrushMethod()?.Invoke(AssetManager.powers, new object[] { tile, godPower });
                return result is bool value && value;
            });

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.RoadPowerId, "Road Brush");
            Localization.AddOrSet($"{Main.RoadPowerId}_info", "Paint roads.");

            QueueTabButton(Main.RoadPowerId, TrainVisuals.ModernRoadSprite ?? TrainVisuals.TrainIcon, "Road Brush", "Paint roads.");
        }

        private static void RegisterCarEffectBrush() {
            if (AssetManager.powers.get(Main.CarEffectPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.CarEffectPowerId,
                name = "Car Effect",
                click_action = SpawnCarEffectAtTile,
                allow_unit_selection = false,
                show_tool_sizes = false,
                unselect_when_window = true,
                can_drag_map = true
            };

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.CarEffectPowerId, "Car Effect");
            Localization.AddOrSet($"{Main.CarEffectPowerId}_info", "Spawn a car.");

            QueueTabButton(Main.CarEffectPowerId, TrainVisuals.TinyCarRightSprite ?? TrainVisuals.ModernRoadSprite ?? TrainVisuals.TrainIcon, "Car.", "Spawn a car.");
        }

        private static void RegisterStopBrush() {
            if (AssetManager.drops.get(Main.StopDropId) == null) {
                DropAsset drop = new DropAsset {
                    id = Main.StopDropId,
                    path_texture = "ui/drops/peachpunch",
                    random_frame = true,
                    default_scale = 0.2f,
                    falling_height = new Vector2(30f, 45f),
                    sound_drop = "event:/SFX/DROPS/DropRain",
                    type = DropType.DropSeed,
                    surprises_units = false,
                    action_landed = PaintStopAtTile
                };

                AssetManager.drops.add(drop);
            }

            if (AssetManager.powers.get(Main.StopPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.StopPowerId,
                name = "Stop Brush",
                type = PowerActionType.PowerSpawnDrops,
                drop_id = Main.StopDropId,
                click_power_action = SpawnDrops,
                show_close_actor = true,
                unselect_when_window = true,
                can_drag_map = true,
                hold_action = true,
                mouse_hold_animation = MouseHoldAnimation.Draw,
                falling_chance = 0.02f,
                show_tool_sizes = true
            };

            EnsureCachedDrop(power, Main.StopDropId);
            power.click_power_action = (PowerAction)Delegate.Combine(power.click_power_action, new PowerAction(FlashPixel));
            power.click_power_action = (PowerAction)Delegate.Combine(power.click_power_action, new PowerAction(PlayDrawingSound));
            power.click_power_brush_action = new PowerAction((tile, godPower) =>
            {
                object result = GetLoopBrushMethod()?.Invoke(AssetManager.powers, new object[] { tile, godPower });
                return result is bool value && value;
            });

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.StopPowerId, "Stop Brush");
            Localization.AddOrSet($"{Main.StopPowerId}_info", "Kinda useless since trains already stop.");

            QueueTabButton(Main.StopPowerId, TrainVisuals.TrainIcon, "Stop Brush", "Paint station stops.");
        }

        private static void RegisterSpawnTrainPower() {
            if (AssetManager.powers.get(Main.SpawnPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.SpawnPowerId,
                name = "Spawn Train",
                click_action = SpawnTrainAtTile,
                allow_unit_selection = false,
                show_tool_sizes = false,
                unselect_when_window = true,
                can_drag_map = true
            };

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.SpawnPowerId, "Spawn Train");
            Localization.AddOrSet($"{Main.SpawnPowerId}_info", "Spawn a train.");
            Localization.AddOrSet("spawn_train", "Spawn Train");
            Localization.AddOrSet("spawn_train_description", "Spawn a train.");

            QueueTabButton(Main.SpawnPowerId, TrainVisuals.TrainIcon, "Spawn Train", "Spawn a train.");
        }

        private static void RegisterRemoveTrainPower() {
            if (AssetManager.powers.get(Main.RemovePowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.RemovePowerId,
                name = "Remove Train",
                click_action = RemoveTrainAtTile,
                allow_unit_selection = false,
                show_tool_sizes = false,
                unselect_when_window = true,
                can_drag_map = true
            };

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.RemovePowerId, "Remove Train");
            Localization.AddOrSet($"{Main.RemovePowerId}_info", "Remove the train.");
            Localization.AddOrSet("remove_train", "Remove Train");
            Localization.AddOrSet("remove_train_description", "Remove the train.");

            QueueTabButton(Main.RemovePowerId, TrainVisuals.TrainIcon, "Remove Train", "Remove a train from a rail tile.");
        }

        private static void RegisterAutoRailsPower() {
            if (AssetManager.powers.get(Main.AutoRailsPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.AutoRailsPowerId,
                name = "Toggle Rails",
                click_action = ToggleAutoRails,
                allow_unit_selection = false,
                show_tool_sizes = false,
                unselect_when_window = false,
                can_drag_map = true
            };

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.AutoRailsPowerId, "Auto Rails");
            Localization.AddOrSet($"{Main.AutoRailsPowerId}_info", "Toggle whether cities build transport.");

            QueueTabButton(Main.AutoRailsPowerId, TrainVisuals.TrainIcon, "Auto Rails", "Toggle transport.");
        }

        private static void RegisterModernRoadsPower() {
            if (AssetManager.powers.get(Main.ModernRoadsPowerId) != null) {
                return;
            }

            GodPower power = new GodPower {
                id = Main.ModernRoadsPowerId,
                name = "Modern Roads",
                click_action = ToggleModernRoads,
                allow_unit_selection = false,
                show_tool_sizes = false,
                unselect_when_window = false,
                can_drag_map = true
            };

            AssetManager.powers.add(power);
            Localization.AddOrSet(Main.ModernRoadsPowerId, "Modern Roads");
            Localization.AddOrSet($"{Main.ModernRoadsPowerId}_info", "Toggle roads and cars.");

            QueueTabButton(Main.ModernRoadsPowerId, TrainVisuals.ModernRoadSprite ?? TrainVisuals.TrainIcon, "Modern Roads", "Toggle Trainbox modern roads on or off.");
        }

        private static void PaintTrackAtTile(WorldTile tile = null, string dropId = null) {
            if (!RailTileRegistry.PaintTrack(tile)) {
                ShowTip("Tracks can only be painted on land or water tiles!");
            }
        }

        private static void PaintStopAtTile(WorldTile tile = null, string dropId = null) {
            if (!RailTileRegistry.PaintStop(tile)) {
                ShowTip("Stops must be painted on walkable land tiles!");
            }
        }

        private static void PaintRoadAtTile(WorldTile tile = null, string dropId = null) {
            if (!RoadTrafficSystem.PaintRoad(tile)) {
                ShowTip("Roads can only be painted on walkable ground that is not already rail!");
            }
        }

        private static bool SpawnCarEffectAtTile(WorldTile tile, string powerId) {
            if (!RoadTrafficSystem.SpawnManualTrafficAtTile(tile)) {
                ShowTip("Paint or find nearby roads first, then place the road car on them!");
                return false;
            }

            return true;
        }

        private static bool SpawnTrainAtTile(WorldTile tile, string powerId) {
            if (!RailTileRegistry.IsRailTile(tile)) {
                ShowTip("Paint tracks first, then spawn the train on that rail!");
                return false;
            }

            if (TaxiTrainLogic.FindTrainOnTile(tile) != null) {
                ShowTip("There is already a train on that tile!");
                return false;
            }

            Kingdom kingdom = RailTileRegistry.ResolveTrackKingdom(tile);
            if (kingdom == null) {
                ShowTip("Spawn the train near a kingdom so it has an owner!");
                return false;
            }

            string assetId = TrainAssets.EnsureTrainAsset(kingdom);
            if (string.IsNullOrWhiteSpace(assetId)) {
                ShowTip("The train asset could not be created for that kingdom!");
                return false;
            }

            Subspecies subspecies = kingdom.getMainSubspecies();
            Actor actor = World.world.units.createNewUnit(assetId, tile, true, 0.5f, subspecies);
            if (actor == null) {
                ShowTip("WorldBox refused to spawn the train there!");
                return false;
            }

            actor.joinKingdom(kingdom);
            if (subspecies != null) {
                actor.setSubspecies(subspecies);
            }

            TaxiTrainLogic.SafeClearTasks(actor);
            TaxiTrainLogic.SafeSetStatsDirty(actor);
            actor.restoreHealthPercent(1f);
            TaxiTrainLogic.InitializeTrain(actor);
            ShowTip($"A train now serves {kingdom.name}.");
            return true;
        }

        private static bool RemoveTrainAtTile(WorldTile tile, string powerId) {
            Actor train = TaxiTrainLogic.FindTrainOnTile(tile);
            if (train == null) {
                ShowTip("No train was found on that tile.");
                return false;
            }

            TaxiTrainLogic.RemoveTrain(train);
            train.dieSimpleNone();
            ShowTip("The train has been removed.");
            return true;
        }

        private static bool ToggleAutoRails(WorldTile tile, string powerId) {
            bool enabled = RailAutoBuilder.ToggleAutoCityRails();
            ShowTip(enabled
                ? "rails enabled. Cities can build and use railways again."
                : "rails disabled. Manual rail painting still works.");
            return true;
        }

        private static bool ToggleModernRoads(WorldTile tile, string powerId) {
            bool enabled = RoadTrafficSystem.ToggleModernRoads();
            ShowTip(enabled
                ? "modern roads enabled. Existing roads were repainted as paved roads."
                : "modern roads disabled. Existing roads were repainted as vanilla roads.");
            return true;
        }

        private static bool SpawnDrops(WorldTile tile, GodPower power) {
            EnsureCachedDrop(power, power?.drop_id);
            return InvokePowerAction(GetSpawnDropsMethod(), tile, power);
        }

        private static bool FlashPixel(WorldTile tile, GodPower power) {
            return InvokePowerAction(GetFlashPixelMethod(), tile, power);
        }

        private static bool PlayDrawingSound(WorldTile tile, GodPower power) {
            return InvokePowerAction(GetDrawingSoundMethod(), tile, power);
        }

        private static bool InvokePowerAction(MethodInfo method, WorldTile tile, GodPower power) {
            object result = method?.Invoke(AssetManager.powers, new object[] { tile, power });
            return result is bool value && value;
        }

        private static void EnsureCachedDrop(GodPower power, string dropId) {
            if (power == null || string.IsNullOrWhiteSpace(dropId)) {
                return;
            }

            DropAsset drop = AssetManager.drops.get(dropId);
            if (drop == null) {
                return;
            }

            if (_cachedDropField == null) {
                _cachedDropField = typeof(GodPower).GetField(
                    "cached_drop_asset",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            _cachedDropField?.SetValue(power, drop);
        }

        private static MethodInfo GetLoopBrushMethod() {
            return _loopBrushMethod ?? (_loopBrushMethod = FindPowerLibraryMethod("loopWithCurrentBrushPowerForDropsFull"));
        }

        private static MethodInfo GetSpawnDropsMethod() {
            return _spawnDropsMethod ?? (_spawnDropsMethod = FindPowerLibraryMethod("spawnDrops"));
        }

        private static MethodInfo GetFlashPixelMethod() {
            return _flashPixelMethod ?? (_flashPixelMethod = FindPowerLibraryMethod("flashPixel"));
        }

        private static MethodInfo GetDrawingSoundMethod() {
            return _drawingSoundMethod ?? (_drawingSoundMethod = FindPowerLibraryMethod("fmodDrawingSound"));
        }

        private static MethodInfo FindPowerLibraryMethod(string name) {
            foreach (MethodInfo method in typeof(PowerLibrary).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (method.Name != name) {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 2
                    && parameters[0].ParameterType == typeof(WorldTile)
                    && parameters[1].ParameterType == typeof(GodPower)) {
                    return method;
                }
            }

            return null;
        }

        private static void ShowTip(string text) {
            WorldTip.showNow(text, false, "top", 3f);
        }

        private static GameObject FindGameObjectEvenIfInactive(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return null;
            }

            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++) {
                GameObject obj = objects[i];
                if (obj != null && string.Equals(obj.name, name, StringComparison.Ordinal)) {
                    return obj;
                }
            }

            return null;
        }

        internal static void HideStandaloneTabButton() {
            if (_tab != null && _tab.powerButton != null) {
                _tab.powerButton.gameObject.SetActive(false);
                RectTransform powerButtonRect = _tab.powerButton.transform as RectTransform;
                if (powerButtonRect != null) {
                    powerButtonRect.anchoredPosition = new Vector2(-5000f, -5000f);
                    powerButtonRect.localScale = Vector3.zero;
                }
            }

            CacheStandaloneTabEntryObject();
            if (_standaloneTabEntryObject == null) {
                return;
            }

            RectTransform rect = _standaloneTabEntryObject.transform as RectTransform;
            if (rect != null) {
                rect.anchoredPosition = new Vector2(-5000f, -5000f);
                rect.localScale = Vector3.zero;
            }

            CanvasGroup canvasGroup = _standaloneTabEntryObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = _standaloneTabEntryObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private static void CacheStandaloneTabEntryObject() {
            if (_standaloneTabEntryObject != null) {
                return;
            }

            _standaloneTabEntryObject = _tab != null && _tab.powerButton != null
                ? _tab.powerButton.gameObject
                : null;

            if (_standaloneTabEntryObject != null) {
                return;
            }

            _standaloneTabEntryObject = GameObject.Find("Button_" + Main.TabId)
                ?? FindGameObjectEvenIfInactive("Button_" + Main.TabId);
        }

        private static MethodInfo GetShowTabMethod() {
            if (_powersTabShowTabMethod != null) {
                return _powersTabShowTabMethod;
            }

            try {
                foreach (MethodInfo method in typeof(PowersTab).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                    if (method.Name == "showTab" && method.GetParameters().Length == 1) {
                        _powersTabShowTabMethod = method;
                        break;
                    }
                }
            }
            catch {
            }

            return _powersTabShowTabMethod;
        }


        internal static bool TryGetPowerDescription(string powerId, out string description) {
            switch (powerId) {
                case Main.TrackPowerId:
                    description = "Paint rail track tiles on land or across water.";
                    return true;
                case Main.RoadPowerId:
                    description = "Paint modern roads.";
                    return true;
                case Main.CarEffectPowerId:
                    description = "Spawn a car.";
                    return true;
                case Main.StopPowerId:
                    description = "Paint walkable train stations where passengers board and leave.";
                    return true;
                case Main.SpawnPowerId:
                    description = "Spawn a train for the nearest kingdom on a painted rail tile.";
                    return true;
                case Main.RemovePowerId:
                    description = "Remove the train on the clicked rail tile.";
                    return true;
                case Main.AutoRailsPowerId:
                    description = RailAutoBuilder.AutoCityRailsEnabled
                        ? "Cities will automatically build and use railways.."
                        : "Cities will not auto-build railways.";
                    return true;
                default:
                    description = null;
                    return false;
            }
        }
    }

    internal static class TaxiTrainLogic {
        private const int PassengerCapacity = 10;
        private const float UpdateInterval = 0.08f;
        private const int MaxLoadingTicks = 18;
        private const float AmbientStopIntervalSeconds = 5f;
        private const float AmbientStopDurationSeconds = 1.1f;
        private const float AmbientBoardRadius = 6f;
        private const float DestinationSearchRetryInterval = 1f;

        private sealed class CachedTrainRoute {
            internal long DestinationKey;
            internal int TopologyVersion;
            internal List<long> TileKeys;
            internal int NextIndex;
        }

        private sealed class RailReachabilitySnapshot {
            internal readonly Dictionary<long, long> FirstStepByTile = new Dictionary<long, long>();
        }

        private static readonly Dictionary<long, float> NextUpdateByTrain = new Dictionary<long, float>();
        private static readonly Dictionary<long, long> PreviousTileByTrain = new Dictionary<long, long>();
        private static readonly Dictionary<long, Vector2> LastForwardByTrain = new Dictionary<long, Vector2>();
        private static readonly Dictionary<long, long> TargetStopByTrain = new Dictionary<long, long>();
        private static readonly Dictionary<long, CachedTrainRoute> CachedRouteByTrain = new Dictionary<long, CachedTrainRoute>();
        private static readonly Dictionary<long, float> NextDestinationSearchAtByTrain = new Dictionary<long, float>();
        private static readonly Dictionary<long, int> LoadingTicksByTrain = new Dictionary<long, int>();
        private static readonly Dictionary<long, Boat> BoatByTrainId = new Dictionary<long, Boat>();
        private static readonly Dictionary<long, HashSet<Actor>> PassengerActorsByTrainId = new Dictionary<long, HashSet<Actor>>();
        private static readonly Dictionary<long, float> NextAmbientStopAtByTrain = new Dictionary<long, float>();
        private static readonly Dictionary<long, float> AmbientStopResumeAtByTrain = new Dictionary<long, float>();
        private static readonly Dictionary<long, int> AmbientStopSequenceByTrain = new Dictionary<long, int>();
        private static readonly Dictionary<long, float> AmbientBoardingUnlockAtByTrain = new Dictionary<long, float>();
        private static readonly Dictionary<long, int> AmbientBoardedStopSequenceByTrain = new Dictionary<long, int>();
        private static readonly Dictionary<long, long> ReboardBlockedTrainByActor = new Dictionary<long, long>();
        private static readonly Dictionary<long, int> ReboardBlockedStopSequenceByActor = new Dictionary<long, int>();

        private static FieldInfo _boatTaxiRequestField;
        private static FieldInfo _boatTaxiTargetField;
        private static FieldInfo _boatActorField;
        private static FieldInfo _boatPassengersField;
        private static FieldInfo _boatLastStepField;
        private static FieldInfo _boatMovementAngleField;
        private static FieldInfo _boatPassengerWaitCounterField;
        private static FieldInfo _boatPickupNearDockField;
        private static FieldInfo _actorVelocityField;
        private static FieldInfo _actorCurrentTileField;
        private static FieldInfo _actorInsideBoatField;
        private static FieldInfo _actorIsInsideBoatField;
        private static FieldInfo _civUnitsField;
        private static MethodInfo _clearTasksMethod;
        private static MethodInfo _cancelAllBehMethod;
        private static MethodInfo _clearWaitMethod;
        private static MethodInfo _setStatsDirtyMethod;
        private static MethodInfo _setProfessionMethod;
        private static MethodInfo _setCurrentTilePositionMethod;
        private static MethodInfo _initComponentsMethod;
        private static MethodInfo _getSimpleListMethod;
        private static MethodInfo _getSimpleComponentMethod;
        private static MethodInfo _boatCreateMethod;
        private static MethodInfo _boatUnloadPassengersMethod;
        private static MethodInfo _boatCancelWorkMethod;
        private static MethodInfo _boatAddPassengerMethod;
        private static MethodInfo _boatRemovePassengerMethod;
        private static MethodInfo _actorEmbarkIntoMethod;
        private static MethodInfo _requestAssignMethod;
        private static MethodInfo _requestEveryoneEmbarkedMethod;
        private static MethodInfo _requestCancelForLatePassengersMethod;
        private static MethodInfo _requestFinishMethod;
        private static FieldInfo _cityKingdomField;
        private static FieldInfo _actorProfessionAssetField;
        private static FieldInfo _professionCanCaptureField;
        private static FieldInfo _professionIdField;
        private static bool _loadRecoveryHandledForCurrentLoad;

        internal static void InitializeTrain(Actor actor) {
            if (actor == null) {
                return;
            }

            NextUpdateByTrain.Remove(actor.id);
            PreviousTileByTrain.Remove(actor.id);
            LastForwardByTrain.Remove(actor.id);
            TargetStopByTrain.Remove(actor.id);
            ClearCachedRoute(actor.id);
            NextDestinationSearchAtByTrain.Remove(actor.id);
            LoadingTicksByTrain.Remove(actor.id);
            NextAmbientStopAtByTrain[actor.id] = Time.time + AmbientStopIntervalSeconds;
            AmbientStopResumeAtByTrain.Remove(actor.id);
            AmbientStopSequenceByTrain.Remove(actor.id);
            AmbientBoardingUnlockAtByTrain.Remove(actor.id);
            AmbientBoardedStopSequenceByTrain.Remove(actor.id);
            PassengerActorsByTrainId.Remove(actor.id);

            EnsureTrainBoat(actor);
            TrySetIdleProfession(actor);
            SafeClearTasks(actor);
            actor.makeWait(0.05f);
            SnapTrainToTile(actor, actor.current_tile);
        }

        internal static void ResetLoadRecovery() {
            _loadRecoveryHandledForCurrentLoad = false;
            CachedRouteByTrain.Clear();
            NextDestinationSearchAtByTrain.Clear();
        }

        internal static void RecoverLoadedTrainBoats() {
            if (_loadRecoveryHandledForCurrentLoad) {
                return;
            }

            _loadRecoveryHandledForCurrentLoad = true;

            int repairedCount = 0;
            foreach (Actor actor in EnumerateCivilianUnits()) {
                if (!IsTrain(actor)) {
                    continue;
                }

                if (TryRecoverLoadedTrainBoat(actor)) {
                    repairedCount++;
                }
            }

            if (repairedCount > 0) {
                TrainboxDebug.Log($"Recovered {repairedCount} train boat component(s) before boat-state load.");
            }
        }

        internal static void RemoveTrain(Actor actor) {
            if (actor == null) {
                return;
            }

            Boat boat = GetTrainBoat(actor);
            if (boat != null) {
                if (GetTaxiRequest(boat) != null) {
                    SafeCancelBoatWork(boat, actor);
                }

                if (SafeBoatHasPassengers(boat)) {
                    WorldTile unloadTile = FindUnloadTile(actor.current_tile, actor);
                    SafeUnloadPassengers(boat, unloadTile, false);
                }
            }

            NextUpdateByTrain.Remove(actor.id);
            PreviousTileByTrain.Remove(actor.id);
            LastForwardByTrain.Remove(actor.id);
            TargetStopByTrain.Remove(actor.id);
            ClearCachedRoute(actor.id);
            NextDestinationSearchAtByTrain.Remove(actor.id);
            LoadingTicksByTrain.Remove(actor.id);
            NextAmbientStopAtByTrain.Remove(actor.id);
            AmbientStopResumeAtByTrain.Remove(actor.id);
            AmbientStopSequenceByTrain.Remove(actor.id);
            AmbientBoardingUnlockAtByTrain.Remove(actor.id);
            AmbientBoardedStopSequenceByTrain.Remove(actor.id);
            PassengerActorsByTrainId.Remove(actor.id);
            BoatByTrainId.Remove(actor.id);
        }

        internal static Actor FindTrainOnTile(WorldTile tile) {
            if (tile == null) {
                return null;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(tile, 0, 1.3f, false)) {
                if (actor?.current_tile == tile && IsTrain(actor)) {
                    return actor;
                }
            }

            return null;
        }

        private static IEnumerable<Actor> EnumerateCivilianUnits() {
            if (World.world?.units == null) {
                yield break;
            }

            if (_civUnitsField == null) {
                _civUnitsField = typeof(ActorManager).GetField(
                    "units_only_civ",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            IEnumerable units = _civUnitsField?.GetValue(World.world.units) as IEnumerable;
            if (units == null) {
                yield break;
            }

            foreach (object entry in units) {
                if (entry is Actor actor) {
                    yield return actor;
                }
            }
        }

        private static bool TryRecoverLoadedTrainBoat(Actor actor) {
            if (actor == null) {
                return false;
            }

            Boat boat = GetExistingBoatComponent(actor);
            bool repairedMissingComponent = false;

            if (boat == null) {
                boat = new Boat();
                SafeCreateBoat(boat, actor);
                repairedMissingComponent = GetBoatActor(boat) == actor;
            }

            if (boat == null) {
                return false;
            }

            BoatByTrainId[actor.id] = boat;
            if (!NextAmbientStopAtByTrain.ContainsKey(actor.id)) {
                NextAmbientStopAtByTrain[actor.id] = Time.time + AmbientStopIntervalSeconds;
            }

            return repairedMissingComponent;
        }

        internal static void UpdateTrain(Actor actor) {
            if (!IsTrain(actor)) {
                return;
            }

            if (!actor.isAlive() || actor.current_tile == null) {
                RemoveTrain(actor);
                return;
            }

            float now = Time.time;
            if (NextUpdateByTrain.TryGetValue(actor.id, out float nextUpdate) && now < nextUpdate) {
                return;
            }

            NextUpdateByTrain[actor.id] = now + UpdateInterval;

            actor.restoreHealth(3);
            TrySetIdleProfession(actor);
            SafeCancelAllBeh(actor);
            SafeClearTasks(actor);

            Boat boat = EnsureTrainBoat(actor);
            if (boat == null) {
                return;
            }

            if (!TryKeepTrainOnRail(actor, 6)) {
                return;
            }

            SyncPassengers(actor);

            life.taxi.TaxiRequest request = GetTaxiRequest(boat);
            if (request != null && !request.isStillLegit() && !SafeBoatHasPassengers(boat)) {
                SafeCancelBoatWork(boat, actor);
                SetTaxiRequest(boat, null);
                request = null;
            }

            if (request == null && (SafeBoatHasPassengers(boat) || IsAmbientServiceStopActive(actor.id, now))) {
                if (HandleAmbientServiceCycle(actor, boat, now)) {
                    return;
                }

                SafeClearTasks(actor);
                actor.makeWait(0.15f);
                return;
            }

            if (request == null) {
                AcquireTaxiRequest(actor, boat);
                request = GetTaxiRequest(boat);
                if (request == null) {
                    if (HandleAmbientServiceCycle(actor, boat, now)) {
                        return;
                    }

                    SafeClearTasks(actor);
                    actor.makeWait(0.15f);
                    return;
                }
            }

            if (request.isState(life.taxi.TaxiRequestState.Assigned)
                || request.isState(life.taxi.TaxiRequestState.Loading)) {
                HandlePickupPhase(actor, boat, request);
                return;
            }

            if (request.isState(life.taxi.TaxiRequestState.Transporting) || SafeBoatHasPassengers(boat)) {
                HandleDropoffPhase(actor, boat, request);
                return;
            }

            if (!request.isState(life.taxi.TaxiRequestState.Finished)) {
                AcquireTaxiRequest(actor, boat);
            }
        }

        internal static bool CanMoveToTile(Actor actor, WorldTile tile) {
            return !IsTrain(actor) || RailTileRegistry.IsRailTilePassive(tile);
        }

        internal static bool CanEmbark(Boat boat) {
            if (!IsRailBoat(boat)) {
                return true;
            }

            return SafeBoatCountPassengers(boat) < PassengerCapacity;
        }

        internal static void OnPassengerEmbarked(Actor passenger, Boat boat) {
            if (!IsRailBoat(boat) || passenger == null) {
                return;
            }

            SnapPassengerToTrain(GetBoatActor(boat), passenger);
        }

        internal static void OnPassengerDisembarking(Actor passenger, Boat boat, ref WorldTile tile) {
            if (!IsRailBoat(boat)) {
                return;
            }

            tile = FindUnloadTile(tile, passenger ?? GetBoatActor(boat));
        }

        internal static void OnPassengerDisembarked(Actor passenger, Boat boat) {
            if (!IsRailBoat(boat) || passenger == null) {
                return;
            }

            MarkPassengerAsJustDropped(passenger, boat);
            SafeClearWait(passenger);
            passenger.makeWait(0.15f);
        }

        private static void AcquireTaxiRequest(Actor train, Boat boat) {
            if (boat == null || GetTaxiRequest(boat) != null) {
                return;
            }

            life.taxi.TaxiRequest request = life.taxi.TaxiManager.getNewRequestForBoat(train);
            if (request == null) {
                return;
            }

            WorldTile pickupStop = RailTileRegistry.FindNearestConnectedStop(train.current_tile, request.getTileStart());
            if (pickupStop == null) {
                return;
            }

            SetTaxiRequest(boat, request);
            SafeAssignTaxiRequest(request, boat);
            SuppressPassengerBoardingAI(request);
            TargetStopByTrain[train.id] = RailTileRegistry.MakeTileKey(pickupStop);
            LoadingTicksByTrain.Remove(train.id);
            TrainboxDebug.Log($"Assigned request to train {train.id} for pickup at {pickupStop.x},{pickupStop.y}.");
        }

        private static void HandlePickupPhase(Actor train, Boat boat, life.taxi.TaxiRequest request) {
            SuppressPassengerBoardingAI(request);
            WorldTile pickupStop = ResolveStopTarget(train, request.getTileStart());
            if (pickupStop == null) {
                CancelCurrentRequest(train, boat, false);
                return;
            }

            if (train.current_tile != pickupStop) {
                MoveTrainToward(train, pickupStop);
                return;
            }

            request.setState(life.taxi.TaxiRequestState.Loading);
            ForcePassengersToBoard(train, boat, request);
            SyncPassengers(train);

            int loadingTicks = LoadingTicksByTrain.TryGetValue(train.id, out int currentTicks)
                ? currentTicks + 1
                : 1;
            LoadingTicksByTrain[train.id] = loadingTicks;

            bool full = SafeBoatCountPassengers(boat) >= PassengerCapacity;
            bool everyone = SafeRequestEveryoneEmbarked(request, boat);
            if (!full && !everyone && loadingTicks < MaxLoadingTicks) {
                train.makeWait(0.15f);
                return;
            }

            if (!SafeBoatHasPassengers(boat)) {
                TrainboxDebug.Log($"Train {train.id} reached pickup but has no passengers after {loadingTicks} loading ticks.");
                CancelCurrentRequest(train, boat, false);
                return;
            }

            request.setState(life.taxi.TaxiRequestState.Transporting);
            SafeCancelLatePassengers(request);
            LoadingTicksByTrain.Remove(train.id);

            WorldTile unloadStop = ResolveStopTarget(train, request.getTileTarget());
            if (unloadStop == null) {
                CancelCurrentRequest(train, boat, true);
                return;
            }

            TargetStopByTrain[train.id] = RailTileRegistry.MakeTileKey(unloadStop);
            TrainboxDebug.Log($"Train {train.id} departed with {SafeBoatCountPassengers(boat)} passengers toward {unloadStop.x},{unloadStop.y}.");
            if (train.current_tile != unloadStop) {
                MoveTrainToward(train, unloadStop);
            }
        }

        private static void HandleDropoffPhase(Actor train, Boat boat, life.taxi.TaxiRequest request) {
            WorldTile unloadStop = ResolveStopTarget(train, request.getTileTarget());
            if (unloadStop == null) {
                CancelCurrentRequest(train, boat, true);
                return;
            }

            WorldTile actualStop = ResolveActiveDropoffStop(train, unloadStop, request.getTileTarget());
            if (actualStop == null) {
                actualStop = unloadStop;
            }

            if (train.current_tile != actualStop) {
                MoveTrainToward(train, unloadStop);
                return;
            }

            WorldTile unloadTile = ResolveDropoffAnchor(actualStop, request.getTileTarget(), train);
            int passengerCountBeforeUnload = SafeBoatCountPassengers(boat);
            ForceDropoffTrainPassengers(train, boat, unloadTile);
            TrainboxDebug.Log($"Train {train.id} unloaded near {unloadTile?.x},{unloadTile?.y}. Before unload: {passengerCountBeforeUnload}, after unload: {SafeBoatCountPassengers(boat)}.");

            if (request != null) {
                SafeFinishTaxiRequest(request);
            }

            SetTaxiRequest(boat, null);
            ClearTaxiTarget(boat);
            TargetStopByTrain.Remove(train.id);
            ClearCachedRoute(train.id);
            LoadingTicksByTrain.Remove(train.id);

            SafeClearTasks(train);
            train.makeWait(0.2f);
        }

        private static void CancelCurrentRequest(Actor train, Boat boat, bool unloadPassengers) {
            if (boat == null) {
                return;
            }

            if (unloadPassengers && SafeBoatHasPassengers(boat)) {
                WorldTile unloadTile = FindUnloadTile(train.current_tile, train);
                ForceDropoffTrainPassengers(train, boat, unloadTile);
            }

            if (GetTaxiRequest(boat) != null) {
                SafeCancelBoatWork(boat, train);
            }

            TargetStopByTrain.Remove(train.id);
            ClearCachedRoute(train.id);
            LoadingTicksByTrain.Remove(train.id);
        }

        private static bool HandleAmbientServiceCycle(Actor train, Boat boat, float now) {
            if (train == null || boat == null) {
                return false;
            }

            EnsureAmbientStopTimer(train.id, now);
            if (IsAmbientServiceStopActive(train.id, now)) {
                TryBoardAmbientPassengersDuringStop(train, boat, now);
                SafeClearTasks(train);
                train.makeWait(0.15f);
                return true;
            }

            if (ShouldStartAmbientServiceStop(train.id, now)) {
                StartAmbientServiceStop(train, boat, now);
                TryBoardAmbientPassengersDuringStop(train, boat, now);
                SafeClearTasks(train);
                train.makeWait(0.15f);
                return true;
            }

            WorldTile serviceStop = ResolveServiceDestinationStop(train, boat);
            if (serviceStop != null && serviceStop != train.current_tile) {
                MoveTrainToward(train, serviceStop);
                return true;
            }

            if (TryMoveAlongPatrol(train)) {
                return true;
            }

            return false;
        }

        private static void EnsureAmbientStopTimer(long trainId, float now) {
            if (!NextAmbientStopAtByTrain.ContainsKey(trainId)) {
                NextAmbientStopAtByTrain[trainId] = now + AmbientStopIntervalSeconds;
            }
        }

        private static bool IsAmbientServiceStopActive(long trainId, float now) {
            if (!AmbientStopResumeAtByTrain.TryGetValue(trainId, out float resumeAt)) {
                return false;
            }

            if (now <= resumeAt) {
                return true;
            }

            AmbientStopResumeAtByTrain.Remove(trainId);
            return false;
        }

        private static bool ShouldStartAmbientServiceStop(long trainId, float now) {
            return !AmbientStopResumeAtByTrain.ContainsKey(trainId)
                && NextAmbientStopAtByTrain.TryGetValue(trainId, out float nextStopAt)
                && now >= nextStopAt;
        }

        private static void StartAmbientServiceStop(Actor train, Boat boat, float now) {
            long trainId = train.id;
            int nextSequence = AmbientStopSequenceByTrain.TryGetValue(trainId, out int currentSequence)
                ? currentSequence + 1
                : 1;

            AmbientStopSequenceByTrain[trainId] = nextSequence;
            AmbientStopResumeAtByTrain[trainId] = now + AmbientStopDurationSeconds;
            NextAmbientStopAtByTrain[trainId] = now + AmbientStopDurationSeconds + AmbientStopIntervalSeconds;
            AmbientBoardedStopSequenceByTrain.Remove(trainId);
            TargetStopByTrain.Remove(trainId);
            ClearCachedRoute(trainId);
            LoadingTicksByTrain.Remove(trainId);

            int beforeUnload = SafeBoatCountPassengers(boat);
            if (beforeUnload > 0) {
                WorldTile unloadTile = FindUnloadTile(train.current_tile, train);
                ForceDropoffTrainPassengers(train, boat, unloadTile);
                int afterUnload = SafeBoatCountPassengers(boat);
                TrainboxDebug.Log($"Ambient stop unload at train {train.id}: before={beforeUnload}, after={afterUnload}.");
                AmbientBoardingUnlockAtByTrain[trainId] = now + 0.45f;
            } else {
                AmbientBoardingUnlockAtByTrain[trainId] = now;
            }
        }

        private static void TryBoardAmbientPassengersDuringStop(Actor train, Boat boat, float now) {
            if (train == null || boat == null) {
                return;
            }

            long trainId = train.id;
            if (!AmbientStopSequenceByTrain.TryGetValue(trainId, out int currentSequence)) {
                return;
            }

            if (AmbientBoardedStopSequenceByTrain.TryGetValue(trainId, out int boardedSequence)
                && boardedSequence == currentSequence) {
                return;
            }

            if (AmbientBoardingUnlockAtByTrain.TryGetValue(trainId, out float unlockAt) && now < unlockAt) {
                return;
            }

            BoardAmbientPassengersAtCurrentStop(train, boat);
            AmbientBoardedStopSequenceByTrain[trainId] = currentSequence;
        }

        private static void BoardAmbientPassengersAtCurrentStop(Actor train, Boat boat) {
            int cap = PassengerCapacity;
            if (train == null || boat == null || SafeBoatCountPassengers(boat) >= cap) {
                return;
            }

            List<Actor> candidates = GetAmbientPassengerCandidates(train, boat);
            if (candidates.Count == 0) {
                return;
            }

            int attempts = Math.Min(candidates.Count, cap - SafeBoatCountPassengers(boat));
            for (int i = 0; i < attempts && SafeBoatCountPassengers(boat) < cap; i++) {
                Actor rider = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                TryEmbarkAmbientPassenger(train, boat, rider);
            }
            TrainboxDebug.Log($"Ambient boarding at train {train.id}: candidates={candidates.Count}, passengers={SafeBoatCountPassengers(boat)}.");
        }

        private static List<Actor> GetRequestActorsSnapshot(life.taxi.TaxiRequest request) {
            bool complete;
            return GetRequestActorsSnapshot(request, out complete);
        }

        private static List<Actor> GetRequestActorsSnapshot(life.taxi.TaxiRequest request, out bool complete) {
            complete = false;
            List<Actor> actors = new List<Actor>();
            if (request == null) {
                return actors;
            }

            try {
                foreach (Actor actor in request.getActors()) {
                    actors.Add(actor);
                }
                complete = true;
            }
            catch {
                complete = actors.Count > 0;
            }

            return actors;
        }

        private static void SuppressPassengerBoardingAI(life.taxi.TaxiRequest request) {
            if (request == null) {
                return;
            }

            foreach (Actor actor in GetRequestActorsSnapshot(request)) {
                if (actor == null || !actor.isAlive() || SafeActorIsInsideBoat(actor) || actor.isFighting()) {
                    continue;
                }

                actor.stopSleeping();
                SafeCancelAllBeh(actor);
                SafeClearTasks(actor);
                actor.makeWait(0.2f);
            }
        }

        private static void ForcePassengersToBoard(
            Actor train,
            Boat boat,
            life.taxi.TaxiRequest request
        ) {
            if (train?.current_tile == null || boat == null || request == null) {
                return;
            }

            foreach (Actor actor in GetRequestActorsSnapshot(request)) {
                if (SafeBoatCountPassengers(boat) >= PassengerCapacity) {
                    break;
                }

                if (actor == null || !actor.isAlive() || SafeActorIsInsideBoat(actor) || actor.isFighting()) {
                    continue;
                }

                actor.stopSleeping();
                SafeCancelAllBeh(actor);
                SafeClearTasks(actor);
                SnapPassengerToTrain(train, actor);
                SafeEmbarkIntoBoat(actor, boat);
                if (SafeActorIsInsideBoat(actor) && SafeActorInsideBoat(actor) == boat) {
                    SnapPassengerToTrain(train, actor);
                } else {
                    SafeClearTasks(actor);
                    actor.makeWait(0.2f);
                }
            }
        }

        private static List<Actor> GetAmbientPassengerCandidates(Actor train, Boat boat) {
            List<Actor> candidates = new List<Actor>();
            if (train?.current_tile == null || boat == null) {
                return candidates;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(train.current_tile, 1, AmbientBoardRadius, false)) {
                if (IsAmbientPassengerCandidate(train, boat, actor)) {
                    candidates.Add(actor);
                }
            }

            return candidates;
        }

        private static bool IsAmbientPassengerCandidate(Actor train, Boat boat, Actor actor) {
            if (train == null || boat == null || actor == null || actor == train) {
                return false;
            }

            if (!actor.isAlive() || SafeActorIsInsideBoat(actor) || actor.isFighting()) {
                return false;
            }

            if (IsTrain(actor) || actor.current_tile == null) {
                return false;
            }

            if (SafeBoatCountPassengers(boat) >= PassengerCapacity) {
                return false;
            }

            if (!IsCivilizationKingdom(train.kingdom) || !IsCivilizationKingdom(actor.kingdom)) {
                return false;
            }

            if (!IsSameOrFriendlyKingdom(train.kingdom, actor.kingdom)) {
                return false;
            }

            if (IsBlockedFromCurrentAmbientStop(train.id, actor.id)) {
                return false;
            }

            float distance = Vector2.Distance(
                new Vector2(train.current_position.x, train.current_position.y),
                new Vector2(actor.current_position.x, actor.current_position.y));
            return distance <= AmbientBoardRadius;
        }

        private static bool TryEmbarkAmbientPassenger(Actor train, Boat boat, Actor rider) {
            if (!IsAmbientPassengerCandidate(train, boat, rider)) {
                return false;
            }

            rider.stopSleeping();
            SafeCancelAllBeh(rider);
            SafeClearTasks(rider);
            SnapPassengerToTrain(train, rider);
            SafeEmbarkIntoBoat(rider, boat);
            if (!SafeActorIsInsideBoat(rider) || SafeActorInsideBoat(rider) != boat) {
                SafeClearTasks(rider);
                rider.makeWait(0.2f);
                TrainboxDebug.Log($"Ambient rider {rider.id} failed to embark train {train.id}.");
                return false;
            }

            SnapPassengerToTrain(train, rider);
            TrainboxDebug.Log($"Ambient rider {rider.id} embarked train {train.id}. Passenger count now {SafeBoatCountPassengers(boat)}.");
            return true;
        }

        private static WorldTile ResolveAmbientDestinationStop(Actor train) {
            if (train == null || train.current_tile == null) {
                return null;
            }

            long previousKey = PreviousTileByTrain.TryGetValue(train.id, out long cachedPrevious)
                ? cachedPrevious
                : long.MinValue;

            if (TargetStopByTrain.TryGetValue(train.id, out long cachedKey)) {
                WorldTile cachedStop = RailTileRegistry.GetTileByKey(cachedKey);
                if (cachedStop != null
                    && cachedStop != train.current_tile
                    && RailTileRegistry.IsStopTilePassive(cachedStop)
                    && TryPeekCachedRouteStep(train, cachedStop, previousKey, out WorldTile cachedFirstStep)
                    && !IsImmediateReverseStep(cachedFirstStep, previousKey)) {
                    return cachedStop;
                }
            }

            WorldTile bestForwardStop = null;
            float bestForwardScore = float.MinValue;
            WorldTile bestReverseStop = null;
            float bestReverseScore = float.MinValue;
            RailReachabilitySnapshot reachability = BuildRailReachability(train.current_tile);

            foreach (long key in GetStopTileKeys()) {
                WorldTile stopTile = RailTileRegistry.GetTileByKey(key);
                if (stopTile == null || stopTile == train.current_tile || !RailTileRegistry.IsStopTilePassive(stopTile)) {
                    continue;
                }

                WorldTile firstStep = GetReachableFirstStep(reachability, stopTile);
                if (firstStep == null) {
                    continue;
                }

                bool reversesImmediately = IsImmediateReverseStep(firstStep, previousKey);
                float score = ScoreAmbientStopChoice(train.current_tile, firstStep, stopTile, previousKey);
                if (reversesImmediately) {
                    if (score > bestReverseScore) {
                        bestReverseScore = score;
                        bestReverseStop = stopTile;
                    }
                } else if (score > bestForwardScore) {
                    bestForwardScore = score;
                    bestForwardStop = stopTile;
                }
            }

            WorldTile chosenStop = bestForwardStop ?? bestReverseStop;
            if (chosenStop == null) {
                return null;
            }

            TargetStopByTrain[train.id] = RailTileRegistry.MakeTileKey(chosenStop);
            return chosenStop;
        }

        private static WorldTile ResolveServiceDestinationStop(Actor train, Boat boat) {
            if (train == null || train.current_tile == null) {
                return null;
            }

            bool hasCachedTarget = TargetStopByTrain.ContainsKey(train.id);
            if (!hasCachedTarget
                && NextDestinationSearchAtByTrain.TryGetValue(train.id, out float nextSearchAt)
                && Time.time < nextSearchAt) {
                return null;
            }

            if (HasMilitaryPassengers(boat)) {
                WorldTile warStop = FindBestConnectedStop(train, IsWarDestinationStop, true);
                if (warStop != null) {
                    TargetStopByTrain[train.id] = RailTileRegistry.MakeTileKey(warStop);
                    NextDestinationSearchAtByTrain.Remove(train.id);
                    return warStop;
                }
            }

            WorldTile friendlyStop = FindBestConnectedStop(train, IsFriendlyCityDestinationStop, true);
            if (friendlyStop != null) {
                TargetStopByTrain[train.id] = RailTileRegistry.MakeTileKey(friendlyStop);
                NextDestinationSearchAtByTrain.Remove(train.id);
                return friendlyStop;
            }

            WorldTile ambientStop = ResolveAmbientDestinationStop(train);
            if (ambientStop != null) {
                NextDestinationSearchAtByTrain.Remove(train.id);
                return ambientStop;
            }

            TargetStopByTrain.Remove(train.id);
            ClearCachedRoute(train.id);
            NextDestinationSearchAtByTrain[train.id] = Time.time + DestinationSearchRetryInterval;
            return null;
        }

        private static WorldTile FindBestConnectedStop(Actor train, Func<Actor, WorldTile, bool> filter, bool preferDifferentCity) {
            if (train?.current_tile == null || filter == null) {
                return null;
            }

            long previousKey = PreviousTileByTrain.TryGetValue(train.id, out long cachedPrevious)
                ? cachedPrevious
                : long.MinValue;
            City currentCity = train.current_tile.zone?.city;

            if (TargetStopByTrain.TryGetValue(train.id, out long cachedKey)) {
                WorldTile cachedStop = RailTileRegistry.GetTileByKey(cachedKey);
                if (cachedStop != null
                    && cachedStop != train.current_tile
                    && RailTileRegistry.IsStopTilePassive(cachedStop)
                    && filter(train, cachedStop)
                    && TryPeekCachedRouteStep(train, cachedStop, previousKey, out _)) {
                    return cachedStop;
                }
            }

            WorldTile bestPreferredStop = null;
            float bestPreferredScore = float.MinValue;
            WorldTile bestFallbackStop = null;
            float bestFallbackScore = float.MinValue;
            RailReachabilitySnapshot reachability = BuildRailReachability(train.current_tile);

            foreach (long key in GetStopTileKeys()) {
                WorldTile stopTile = RailTileRegistry.GetTileByKey(key);
                if (stopTile == null || stopTile == train.current_tile || !RailTileRegistry.IsStopTilePassive(stopTile)) {
                    continue;
                }

                if (!filter(train, stopTile)) {
                    continue;
                }

                WorldTile firstStep = GetReachableFirstStep(reachability, stopTile);
                if (firstStep == null) {
                    continue;
                }

                float score = ScoreAmbientStopChoice(train.current_tile, firstStep, stopTile, previousKey);
                City stopCity = stopTile.zone?.city;
                bool isDifferentCity = stopCity != null && stopCity != currentCity;
                if (stopCity != null) {
                    score += 160f;
                }

                if (IsImmediateReverseStep(firstStep, previousKey)) {
                    score -= 380f;
                }

                if (preferDifferentCity && isDifferentCity) {
                    score += 260f;
                    if (score > bestPreferredScore) {
                        bestPreferredScore = score;
                        bestPreferredStop = stopTile;
                    }
                } else if (score > bestFallbackScore) {
                    bestFallbackScore = score;
                    bestFallbackStop = stopTile;
                }
            }

            return bestPreferredStop ?? bestFallbackStop;
        }

        private static RailReachabilitySnapshot BuildRailReachability(WorldTile startTile) {
            RailReachabilitySnapshot snapshot = new RailReachabilitySnapshot();
            if (!RailTileRegistry.IsRailTilePassive(startTile)) {
                return snapshot;
            }

            Queue<WorldTile> queue = new Queue<WorldTile>();
            HashSet<long> visited = new HashSet<long>();
            long startKey = RailTileRegistry.MakeTileKey(startTile);

            queue.Enqueue(startTile);
            visited.Add(startKey);

            while (queue.Count > 0) {
                WorldTile current = queue.Dequeue();
                long currentKey = RailTileRegistry.MakeTileKey(current);

                foreach (WorldTile neighbour in RailTileRegistry.GetTrackNeighbours(current)) {
                    long neighbourKey = RailTileRegistry.MakeTileKey(neighbour);
                    if (!visited.Add(neighbourKey)) {
                        continue;
                    }

                    snapshot.FirstStepByTile[neighbourKey] = currentKey == startKey
                        ? neighbourKey
                        : snapshot.FirstStepByTile[currentKey];
                    queue.Enqueue(neighbour);
                }
            }

            return snapshot;
        }

        private static WorldTile GetReachableFirstStep(RailReachabilitySnapshot snapshot, WorldTile destination) {
            if (snapshot == null || destination == null) {
                return null;
            }

            long destinationKey = RailTileRegistry.MakeTileKey(destination);
            return snapshot.FirstStepByTile.TryGetValue(destinationKey, out long firstStepKey)
                ? RailTileRegistry.GetTileByKey(firstStepKey)
                : null;
        }

        private static bool IsFriendlyCityDestinationStop(Actor train, WorldTile stopTile) {
            if (train?.kingdom == null || stopTile == null) {
                return false;
            }

            City stopCity = stopTile.zone?.city;
            if (stopCity == null) {
                return false;
            }

            Kingdom stopKingdom = GetCityKingdom(stopCity);
            return stopKingdom != null && stopKingdom == train.kingdom;
        }

        private static bool IsWarDestinationStop(Actor train, WorldTile stopTile) {
            if (train?.kingdom == null || stopTile == null) {
                return false;
            }

            City stopCity = stopTile.zone?.city;
            if (stopCity == null) {
                return false;
            }

            Kingdom stopKingdom = GetCityKingdom(stopCity);
            return stopKingdom != null
                && stopKingdom != train.kingdom
                && (train.kingdom.isEnemy(stopKingdom) || stopKingdom.isEnemy(train.kingdom));
        }

        private static bool HasMilitaryPassengers(Boat boat) {
            if (boat == null) {
                return false;
            }

            foreach (Actor passenger in SafeGetBoatPassengers(boat)) {
                if (IsMilitaryPassenger(passenger)) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMilitaryPassenger(Actor actor) {
            if (actor == null) {
                return false;
            }

            object professionAsset = GetProfessionAsset(actor);
            if (professionAsset == null) {
                return false;
            }

            try {
                if (_professionCanCaptureField == null) {
                    _professionCanCaptureField = professionAsset.GetType().GetField(
                        "can_capture",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                object canCapture = _professionCanCaptureField?.GetValue(professionAsset);
                if (canCapture is bool canCaptureBool && canCaptureBool) {
                    return true;
                }
            }
            catch {
            }

            try {
                if (_professionIdField == null) {
                    _professionIdField = professionAsset.GetType().GetField(
                        "profession_id",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                string professionName = _professionIdField?.GetValue(professionAsset)?.ToString();
                if (string.IsNullOrWhiteSpace(professionName)) {
                    return false;
                }

                professionName = professionName.ToLowerInvariant();
                return professionName.Contains("warrior")
                    || professionName.Contains("leader")
                    || professionName.Contains("king")
                    || professionName.Contains("soldier")
                    || professionName.Contains("attacker")
                    || professionName.Contains("general");
            }
            catch {
                return false;
            }
        }

        private static object GetProfessionAsset(Actor actor) {
            if (actor == null) {
                return null;
            }

            try {
                if (_actorProfessionAssetField == null) {
                    _actorProfessionAssetField = typeof(Actor).GetField(
                        "profession_asset",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _actorProfessionAssetField?.GetValue(actor);
            }
            catch {
                return null;
            }
        }

        private static float ScoreAmbientStopChoice(
            WorldTile currentTile,
            WorldTile firstStep,
            WorldTile stopTile,
            long previousTileKey
        ) {
            if (currentTile == null || firstStep == null || stopTile == null) {
                return float.MinValue;
            }

            float score = Mathf.Abs(stopTile.x - currentTile.x) + Mathf.Abs(stopTile.y - currentTile.y);
            if (TryGetForwardDirection(currentTile, previousTileKey, out Vector2 forward)) {
                Vector2 firstDirection = new Vector2(firstStep.x - currentTile.x, firstStep.y - currentTile.y).normalized;
                score += Vector2.Dot(forward, firstDirection) * 250f;
            }

            if (RailTileRegistry.MakeTileKey(stopTile) == previousTileKey) {
                score -= 2000f;
            }

            return score;
        }

        private static bool IsImmediateReverseStep(WorldTile nextTile, long previousTileKey) {
            return nextTile != null
                && previousTileKey != long.MinValue
                && RailTileRegistry.MakeTileKey(nextTile) == previousTileKey;
        }

        private static bool IsBlockedFromCurrentAmbientStop(long trainId, long actorId) {
            if (!ReboardBlockedTrainByActor.TryGetValue(actorId, out long blockedTrainId) || blockedTrainId != trainId) {
                return false;
            }

            if (!ReboardBlockedStopSequenceByActor.TryGetValue(actorId, out int blockedSequence)) {
                return false;
            }

            return AmbientStopSequenceByTrain.TryGetValue(trainId, out int currentSequence)
                && blockedSequence == currentSequence;
        }

        private static void MarkPassengerAsJustDropped(Actor passenger, Boat boat) {
            Actor train = GetBoatActor(boat);
            if (passenger == null || train == null) {
                return;
            }

            long trainId = train.id;
            if (!AmbientStopSequenceByTrain.TryGetValue(trainId, out int currentSequence)) {
                return;
            }

            ReboardBlockedTrainByActor[passenger.id] = trainId;
            ReboardBlockedStopSequenceByActor[passenger.id] = currentSequence;
        }

        private static bool TryMoveAlongPatrol(Actor train) {
            if (train?.current_tile == null) {
                return false;
            }

            long currentKey = RailTileRegistry.MakeTileKey(train.current_tile);
            long previousKey = PreviousTileByTrain.TryGetValue(train.id, out long cachedPrevious)
                ? cachedPrevious
                : long.MinValue;

            var nextTile = PickPatrolNeighbour(train.current_tile, previousKey);
            if (nextTile == null || nextTile == train.current_tile) {
                return false;
            }

            TargetStopByTrain.Remove(train.id);
            ClearCachedRoute(train.id);
            PreviousTileByTrain[train.id] = currentKey;
            SnapTrainToTile(train, nextTile);
            SyncPassengers(train); // keep riders glued on for now
            return true;
        }

        private static bool TryKeepTrainOnRail(Actor train, int radius) {
            if (train?.current_tile == null) {
                return false;
            }

            if (RailTileRegistry.IsRailTilePassive(train.current_tile)) {
                SnapTrainToTile(train, train.current_tile);
                return true;
            }

            WorldTile nearestRail = RailTileRegistry.FindNearestTrack(train.current_tile, radius);
            if (nearestRail == null || !RailTileRegistry.IsRailTilePassive(nearestRail)) {
                return false;
            }

            TargetStopByTrain.Remove(train.id);
            ClearCachedRoute(train.id);
            SnapTrainToTile(train, nearestRail);
            return true;
        }

        private static IEnumerable<long> GetStopTileKeys() {
            return RailTileRegistry.EnumerateStopTileKeys();
        }

        private static bool IsCivilizationKingdom(Kingdom kingdom) {
            if (kingdom == null || kingdom.asset == null) {
                return false;
            }

            try {
                return kingdom.isCiv();
            }
            catch {
                return false;
            }
        }

        private static bool IsSameOrFriendlyKingdom(Kingdom a, Kingdom b) {
            if (!IsCivilizationKingdom(a) || !IsCivilizationKingdom(b)) {
                return false;
            }

            if (a == b) {
                return true;
            }

            try {
                if (a.isEnemy(b) || b.isEnemy(a)) {
                    return false;
                }

                Alliance aAlliance = a.getAlliance();
                Alliance bAlliance = b.getAlliance();
                if (aAlliance != null && aAlliance == bAlliance) {
                    return true;
                }

                if (World.world?.diplomacy == null) {
                    return false;
                }

                return a.isOpinionTowardsKingdomGood(b) && b.isOpinionTowardsKingdomGood(a);
            }
            catch {
                // Diplomacy relations are not defined for every transient kingdom.
                return false;
            }
        }

        private static Kingdom GetCityKingdom(City city) {
            if (city == null || !city.hasKingdom()) {
                return null;
            }

            try {
                if (_cityKingdomField == null) {
                    _cityKingdomField = typeof(City).GetField(
                        "kingdom",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _cityKingdomField?.GetValue(city) as Kingdom;
            }
            catch {
                return null;
            }
        }

        private static void actorIdleWait(Actor train) {
            SafeClearTasks(train);
            train.makeWait(0.2f);
        }

        private static void MoveTrainToward(Actor train, WorldTile targetStop) {
            if (train?.current_tile == null || targetStop == null) {
                return;
            }

            long currentKey = RailTileRegistry.MakeTileKey(train.current_tile);
            long previousKey = PreviousTileByTrain.TryGetValue(train.id, out long cachedPrevious)
                ? cachedPrevious
                : long.MinValue;

            bool usingCachedRoute = TryPeekCachedRouteStep(train, targetStop, previousKey, out WorldTile nextTile);
            if (usingCachedRoute && IsImmediateReverseStep(nextTile, previousKey)) {
                WorldTile forwardAlternative = PickPatrolNeighbour(train.current_tile, previousKey);
                if (forwardAlternative != null && forwardAlternative != nextTile) {
                    ClearCachedRoute(train.id);
                    nextTile = forwardAlternative;
                    usingCachedRoute = false;
                }
            }

            if (!usingCachedRoute) {
                nextTile = PickPatrolNeighbour(train.current_tile, previousKey);
            }

            if (nextTile == null || !RailTileRegistry.IsRailTilePassive(nextTile)) {
                return;
            }

            PreviousTileByTrain[train.id] = currentKey;
            SnapTrainToTile(train, nextTile);
            if (usingCachedRoute
                && CachedRouteByTrain.TryGetValue(train.id, out CachedTrainRoute route)
                && route.NextIndex < route.TileKeys.Count
                && route.TileKeys[route.NextIndex] == RailTileRegistry.MakeTileKey(nextTile)) {
                route.NextIndex++;
            }
            SyncPassengers(train);
        }

        private static WorldTile ResolveStopTarget(Actor train, WorldTile targetTile) {
            if (train == null) {
                return null;
            }

            if (TargetStopByTrain.TryGetValue(train.id, out long key)) {
                WorldTile cachedStop = RailTileRegistry.GetTileByKey(key);
                if (RailTileRegistry.IsStopTilePassive(cachedStop)
                    && (targetTile == null || cachedStop.isSameIsland(targetTile))) {
                    if (cachedStop == train.current_tile) {
                        return cachedStop;
                    }

                    long previousKey = PreviousTileByTrain.TryGetValue(train.id, out long cachedPrevious)
                        ? cachedPrevious
                        : long.MinValue;
                    if (TryPeekCachedRouteStep(train, cachedStop, previousKey, out _)) {
                        return cachedStop;
                    }
                }

                TargetStopByTrain.Remove(train.id);
                ClearCachedRoute(train.id);
            }

            WorldTile resolved = RailTileRegistry.FindNearestConnectedStop(train.current_tile, targetTile);
            if (resolved != null) {
                TargetStopByTrain[train.id] = RailTileRegistry.MakeTileKey(resolved);
            }

            return resolved;
        }

        private static bool TryPeekCachedRouteStep(
            Actor train,
            WorldTile destinationTile,
            long previousTileKey,
            out WorldTile nextTile
        ) {
            nextTile = null;
            if (train?.current_tile == null || destinationTile == null || train.current_tile == destinationTile) {
                return false;
            }

            long currentKey = RailTileRegistry.MakeTileKey(train.current_tile);
            long destinationKey = RailTileRegistry.MakeTileKey(destinationTile);
            int topologyVersion = RailTileRegistry.TopologyVersion;

            bool routeIsValid = CachedRouteByTrain.TryGetValue(train.id, out CachedTrainRoute route)
                && route != null
                && route.DestinationKey == destinationKey
                && route.TopologyVersion == topologyVersion
                && route.TileKeys != null
                && route.NextIndex > 0
                && route.NextIndex < route.TileKeys.Count
                && route.TileKeys[route.NextIndex - 1] == currentKey;

            if (!routeIsValid) {
                List<long> path = BuildRouteToward(train.current_tile, destinationTile, previousTileKey);
                if (path == null || path.Count < 2) {
                    ClearCachedRoute(train.id);
                    return false;
                }

                route = new CachedTrainRoute {
                    DestinationKey = destinationKey,
                    TopologyVersion = RailTileRegistry.TopologyVersion,
                    TileKeys = path,
                    NextIndex = 1
                };
                CachedRouteByTrain[train.id] = route;
            }

            nextTile = RailTileRegistry.GetTileByKey(route.TileKeys[route.NextIndex]);
            if (!RailTileRegistry.IsRailTilePassive(nextTile)) {
                ClearCachedRoute(train.id);
                nextTile = null;
                return false;
            }

            return true;
        }

        private static List<long> BuildRouteToward(WorldTile currentTile, WorldTile destinationTile, long previousTileKey) {
            if (currentTile == null || destinationTile == null) {
                return null;
            }

            if (currentTile == destinationTile) {
                return new List<long> { RailTileRegistry.MakeTileKey(currentTile) };
            }

            Queue<WorldTile> queue = new Queue<WorldTile>();
            HashSet<long> visited = new HashSet<long>();
            Dictionary<long, long> previousByTile = new Dictionary<long, long>();

            long currentKey = RailTileRegistry.MakeTileKey(currentTile);
            long destinationKey = RailTileRegistry.MakeTileKey(destinationTile);

            queue.Enqueue(currentTile);
            visited.Add(currentKey);

            while (queue.Count > 0) {
                WorldTile tile = queue.Dequeue();
                long tileKey = RailTileRegistry.MakeTileKey(tile);
                long currentPathPreviousKey = tileKey == currentKey
                    ? previousTileKey
                    : (previousByTile.TryGetValue(tileKey, out long parentKey)
                        ? parentKey
                        : long.MinValue);

                foreach (WorldTile neighbour in GetOrderedTrackNeighbours(tile, currentPathPreviousKey, destinationTile)) {
                    long neighbourKey = RailTileRegistry.MakeTileKey(neighbour);
                    if (!visited.Add(neighbourKey)) {
                        continue;
                    }

                    previousByTile[neighbourKey] = tileKey;

                    if (neighbourKey == destinationKey) {
                        return ReconstructRoute(currentKey, destinationKey, previousByTile);
                    }

                    queue.Enqueue(neighbour);
                }
            }

            return null;
        }

        private static List<long> ReconstructRoute(
            long currentKey,
            long destinationKey,
            Dictionary<long, long> previousByTile
        ) {
            List<long> reversedPath = new List<long> { destinationKey };
            long cursor = destinationKey;

            while (cursor != currentKey) {
                if (!previousByTile.TryGetValue(cursor, out cursor)) {
                    return null;
                }

                reversedPath.Add(cursor);
            }

            reversedPath.Reverse();
            return reversedPath;
        }

        private static void ClearCachedRoute(long trainId) {
            CachedRouteByTrain.Remove(trainId);
        }

        private static WorldTile PickPatrolNeighbour(WorldTile currentTile, long previousTileKey) {
            List<WorldTile> neighbours = RailTileRegistry.GetTrackNeighbours(currentTile);
            if (neighbours.Count == 0) {
                return null;
            }

            if (neighbours.Count == 1) {
                return neighbours[0];
            }

            return PickBestDirectionalNeighbour(currentTile, neighbours, previousTileKey, null);
        }

        private static List<WorldTile> GetOrderedTrackNeighbours(WorldTile currentTile, long previousTileKey, WorldTile destinationTile) {
            List<WorldTile> ordered = RailTileRegistry.GetTrackNeighbours(currentTile);
            ordered.Sort((a, b) => CompareNeighbourPreference(currentTile, a, b, previousTileKey, destinationTile));
            return ordered;
        }

        private static int CompareNeighbourPreference(
            WorldTile currentTile,
            WorldTile a,
            WorldTile b,
            long previousTileKey,
            WorldTile destinationTile
        ) {
            float aScore = ScoreNeighbourPreference(currentTile, a, previousTileKey, destinationTile);
            float bScore = ScoreNeighbourPreference(currentTile, b, previousTileKey, destinationTile);
            return bScore.CompareTo(aScore);
        }

        private static float ScoreNeighbourPreference(
            WorldTile currentTile,
            WorldTile candidate,
            long previousTileKey,
            WorldTile destinationTile
        ) {
            if (candidate == null) {
                return float.MinValue;
            }

            long candidateKey = RailTileRegistry.MakeTileKey(candidate);
            float score = 0f;

            if (candidateKey == previousTileKey) {
                score -= 10000f;
            } else {
                score += 1000f;
            }

            if (TryGetForwardDirection(currentTile, previousTileKey, out Vector2 forward)) {
                Vector2 candidateDirection = new Vector2(candidate.x - currentTile.x, candidate.y - currentTile.y).normalized;
                score += Vector2.Dot(forward, candidateDirection) * 100f;
            }

            if (destinationTile != null) {
                float distance = Mathf.Abs(candidate.x - destinationTile.x) + Mathf.Abs(candidate.y - destinationTile.y);
                score -= distance;
            }

            return score;
        }

        private static WorldTile PickBestDirectionalNeighbour(
            WorldTile currentTile,
            List<WorldTile> neighbours,
            long previousTileKey,
            WorldTile destinationTile
        ) {
            if (neighbours == null || neighbours.Count == 0) {
                return null;
            }

            WorldTile best = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < neighbours.Count; i++) {
                WorldTile neighbour = neighbours[i];
                float score = ScoreNeighbourPreference(currentTile, neighbour, previousTileKey, destinationTile);
                if (score > bestScore) {
                    bestScore = score;
                    best = neighbour;
                }
            }

            return best;
        }

        private static bool TryGetForwardDirection(WorldTile currentTile, long previousTileKey, out Vector2 forward) {
            forward = Vector2.zero;
            if (currentTile != null && previousTileKey != long.MinValue) {
                int previousX = (int)(previousTileKey >> 32);
                int previousY = unchecked((int)previousTileKey);
                Vector2 direction = new Vector2(currentTile.x - previousX, currentTile.y - previousY);
                if (direction.sqrMagnitude > 0.001f) {
                    forward = direction.normalized;
                    return true;
                }
            }

            Actor train = FindTrainOnTile(currentTile);
            if (train != null
                && LastForwardByTrain.TryGetValue(train.id, out Vector2 cachedForward)
                && cachedForward.sqrMagnitude > 0.001f) {
                forward = cachedForward.normalized;
                return true;
            }

            return false;
        }

        internal static bool TryGetTrainFacing(Actor actor, out Vector2 forward) {
            forward = Vector2.zero;
            if (actor == null) {
                return false;
            }

            if (LastForwardByTrain.TryGetValue(actor.id, out Vector2 cachedForward) && cachedForward.sqrMagnitude > 0.001f) {
                forward = cachedForward.normalized;
                return true;
            }

            if (actor.current_tile != null
                && PreviousTileByTrain.TryGetValue(actor.id, out long previousTileKey)
                && previousTileKey != long.MinValue) {
                int previousX = (int)(previousTileKey >> 32);
                int previousY = unchecked((int)previousTileKey);
                Vector2 direction = new Vector2(actor.current_tile.x - previousX, actor.current_tile.y - previousY);
                if (direction.sqrMagnitude > 0.001f) {
                    forward = direction.normalized;
                    return true;
                }
            }

            return false;
        }

        private static void SnapTrainToTile(Actor train, WorldTile tile) {
            if (train == null || tile == null) {
                return;
            }

            if (!RailTileRegistry.IsRailTilePassive(tile)) {
                WorldTile fallback = RailTileRegistry.IsRailTilePassive(train.current_tile)
                    ? train.current_tile
                    : RailTileRegistry.FindNearestTrack(tile, 2) ?? RailTileRegistry.FindNearestTrack(train.current_tile, 6);
                if (fallback == null || !RailTileRegistry.IsRailTilePassive(fallback)) {
                    return;
                }

                tile = fallback;
            }

            if (train.current_tile != null && train.current_tile != tile) {
                Vector2 moved = new Vector2(tile.x - train.current_tile.x, tile.y - train.current_tile.y);
                if (moved.sqrMagnitude > 0.001f) {
                    LastForwardByTrain[train.id] = moved.normalized;
                }
            }

            SafeZeroVelocity(train);
            train.current_position = tile.pos;
            train.next_step_position = tile.pos;
            train.next_step_position_possession = tile.pos;
            SafeSetCurrentTilePosition(train, tile);
        }

        private static void SyncPassengers(Actor train) {
            Boat boat = GetTrainBoat(train);
            if (boat == null || !SafeBoatHasPassengers(boat)) {
                return;
            }

            foreach (Actor passenger in SafeGetBoatPassengers(boat)) {
                if (passenger == null || !passenger.isAlive()) {
                    continue;
                }

                SnapPassengerToTrain(train, passenger);
            }
        }

        private static void SnapPassengerToTrain(Actor train, Actor passenger) {
            if (train == null || passenger == null || train.current_tile == null) {
                return;
            }

            SafeZeroVelocity(passenger);
            passenger.current_position = train.current_position;
            passenger.next_step_position = train.current_position;
            passenger.next_step_position_possession = train.current_position;
            SafeSetCurrentTilePosition(passenger, train.current_tile);
        }

        private static void SafeZeroVelocity(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                if (_actorVelocityField == null) {
                    _actorVelocityField = typeof(Actor).GetField(
                        "velocity",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _actorVelocityField?.SetValue(actor, Vector3.zero);
            }
            catch {
            }
        }

        private static void SafeSetCurrentTilePosition(Actor actor, WorldTile tile) {
            if (actor == null || tile == null) {
                return;
            }

            try {
                if (_setCurrentTilePositionMethod == null) {
                    _setCurrentTilePositionMethod = typeof(Actor).GetMethod(
                        "setCurrentTilePosition",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(WorldTile) },
                        null
                    );
                }

                if (_setCurrentTilePositionMethod != null) {
                    _setCurrentTilePositionMethod.Invoke(actor, new object[] { tile });
                    return;
                }
            }
            catch {
            }

            try {
                if (_actorCurrentTileField == null) {
                    _actorCurrentTileField = typeof(Actor).GetField(
                        "current_tile",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _actorCurrentTileField?.SetValue(actor, tile);
            }
            catch {
            }
        }

        private static WorldTile FindUnloadTile(WorldTile destinationTile, Actor passenger) {
            if (destinationTile == null) {
                return passenger?.current_tile;
            }

            WorldTile around = destinationTile.getWalkableTileAround(passenger?.current_tile ?? destinationTile);
            return around ?? destinationTile.getTileAroundThisOnSameIsland(passenger?.current_tile ?? destinationTile, true) ?? destinationTile;
        }

        private static WorldTile ResolveActiveDropoffStop(Actor train, WorldTile plannedStop, WorldTile targetTile) {
            if (train?.current_tile == null) {
                return plannedStop;
            }

            if (train.current_tile == plannedStop) {
                return plannedStop;
            }

            if (!RailTileRegistry.IsStopTilePassive(train.current_tile) || targetTile == null) {
                return plannedStop;
            }

            if (!train.current_tile.isSameIsland(targetTile)) {
                return plannedStop;
            }

            return train.current_tile;
        }

        private static WorldTile ResolveDropoffAnchor(WorldTile stopTile, WorldTile targetTile, Actor fallbackPassenger) {
            if (targetTile != null && stopTile != null && stopTile.isSameIsland(targetTile)) {
                WorldTile candidate = FindUnloadTile(targetTile, fallbackPassenger);
                if (candidate != null) {
                    return candidate;
                }
            }

            return FindUnloadTile(stopTile ?? targetTile, fallbackPassenger);
        }

        private static void ForceDropoffTrainPassengers(Actor train, Boat boat, WorldTile anchorTile) {
            if (boat == null) {
                return;
            }

            HashSet<Actor> passengersToDrop = new HashSet<Actor>();
            foreach (Actor passenger in SafeGetBoatPassengers(boat)) {
                if (passenger != null) {
                    passengersToDrop.Add(passenger);
                }
            }

            if (train != null && PassengerActorsByTrainId.TryGetValue(train.id, out HashSet<Actor> trackedPassengers)) {
                foreach (Actor passenger in trackedPassengers) {
                    if (passenger != null) {
                        passengersToDrop.Add(passenger);
                    }
                }
            }

            List<WorldTile> dropTiles = BuildDropoffTiles(anchorTile, passengersToDrop.Count, train);
            int dropIndex = 0;
            foreach (Actor passenger in passengersToDrop) {
                if (passenger == null) {
                    continue;
                }

                WorldTile passengerTile = dropIndex < dropTiles.Count
                    ? dropTiles[dropIndex]
                    : FindUnloadTile(anchorTile, passenger);
                dropIndex++;
                ForcePassengerOffBoat(passenger, boat, passengerTile);
            }

            if (train != null && PassengerActorsByTrainId.TryGetValue(train.id, out HashSet<Actor> remainingPassengers)) {
                remainingPassengers.Clear();
            }
        }

        private static List<WorldTile> BuildDropoffTiles(WorldTile anchorTile, int requestedCount, Actor fallbackPassenger) {
            List<WorldTile> dropTiles = new List<WorldTile>();
            if (requestedCount <= 0) {
                return dropTiles;
            }

            HashSet<long> seenKeys = new HashSet<long>();
            foreach (WorldTile baseTile in EnumerateDropoffBases(anchorTile)) {
                WorldTile resolved = FindUnloadTile(baseTile, fallbackPassenger);
                TryAddDropoffTile(dropTiles, seenKeys, resolved);
            }

            if (dropTiles.Count == 0) {
                TryAddDropoffTile(dropTiles, seenKeys, anchorTile);
            }

            while (dropTiles.Count < requestedCount && dropTiles.Count > 0) {
                dropTiles.Add(dropTiles[dropTiles.Count % Math.Max(1, seenKeys.Count)]);
            }

            return dropTiles;
        }

        private static IEnumerable<WorldTile> EnumerateDropoffBases(WorldTile anchorTile) {
            if (anchorTile == null) {
                yield break;
            }

            yield return anchorTile;

            if (anchorTile.tile_up != null) {
                yield return anchorTile.tile_up;
            }

            if (anchorTile.tile_down != null) {
                yield return anchorTile.tile_down;
            }

            if (anchorTile.tile_left != null) {
                yield return anchorTile.tile_left;
            }

            if (anchorTile.tile_right != null) {
                yield return anchorTile.tile_right;
            }
        }

        private static void TryAddDropoffTile(List<WorldTile> tiles, HashSet<long> seenKeys, WorldTile tile) {
            if (tile == null) {
                return;
            }

            long key = RailTileRegistry.MakeTileKey(tile);
            if (!seenKeys.Add(key)) {
                return;
            }

            tiles.Add(tile);
        }

        private static void ForcePassengerOffBoat(Actor passenger, Boat boat, WorldTile tile) {
            if (passenger == null || boat == null) {
                return;
            }

            try {
                if (_actorInsideBoatField == null) {
                    _actorInsideBoatField = typeof(Actor).GetField(
                        "inside_boat",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_actorIsInsideBoatField == null) {
                    _actorIsInsideBoatField = typeof(Actor).GetField(
                        "is_inside_boat",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _actorInsideBoatField?.SetValue(passenger, null);
                _actorIsInsideBoatField?.SetValue(passenger, false);
            }
            catch {
            }

            if (tile != null) {
                SafeZeroVelocity(passenger);
                passenger.current_position = tile.pos;
                passenger.next_step_position = tile.pos;
                passenger.next_step_position_possession = tile.pos;
                SafeSetCurrentTilePosition(passenger, tile);
            }

            SafeBoatRemovePassenger(boat, passenger);
            MarkPassengerAsJustDropped(passenger, boat);
            SafeCancelAllBeh(passenger);
            SafeClearTasks(passenger);
            SafeClearWait(passenger);
        }

        private static Boat EnsureTrainBoat(Actor train) {
            if (!IsTrain(train)) {
                return null;
            }

            if (!BoatByTrainId.TryGetValue(train.id, out Boat boat) || boat == null || GetBoatActor(boat) != train) {
                boat = new Boat();
                SafeCreateBoat(boat, train);
                BoatByTrainId[train.id] = boat;
            }

            return boat;
        }

        private static Boat GetTrainBoat(Actor train) {
            if (train == null) {
                return null;
            }

            return BoatByTrainId.TryGetValue(train.id, out Boat boat) ? boat : null;
        }

        private static Boat GetExistingBoatComponent(Actor actor) {
            if (actor == null) {
                return null;
            }

            try {
                if (_getSimpleComponentMethod == null) {
                    foreach (MethodInfo method in typeof(Actor).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                        if (method.Name == "getSimpleComponent" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0) {
                            _getSimpleComponentMethod = method;
                            break;
                        }
                    }
                }

                return _getSimpleComponentMethod?.MakeGenericMethod(typeof(Boat)).Invoke(actor, null) as Boat;
            }
            catch {
                return null;
            }
        }

        internal static bool IsRailBoat(Boat boat) {
            return boat != null && IsTrain(GetBoatActor(boat));
        }

        internal static Boat GetInspectableTrainBoat(Actor actor) {
            return GetTrainBoat(actor);
        }

        internal static bool SafeInspectableBoatHasPassengers(Boat boat) {
            return SafeBoatHasPassengers(boat);
        }

        internal static int SafeInspectableBoatPassengerCount(Boat boat) {
            return SafeBoatCountPassengers(boat);
        }

        private static Actor GetBoatActor(Boat boat) {
            if (boat == null) {
                return null;
            }

            try {
                if (_boatActorField == null) {
                    _boatActorField = typeof(ActorSimpleComponent).GetField(
                        "actor",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _boatActorField?.GetValue(boat) as Actor;
            }
            catch {
                return null;
            }
        }

        private static void SafeCreateBoat(Boat boat, Actor train) {
            if (boat == null || train == null) {
                return;
            }

            try {
                if (_boatCreateMethod == null) {
                    _boatCreateMethod = typeof(Boat).GetMethod(
                        "create",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Actor) },
                        null
                    );
                }

                if (_boatCreateMethod != null) {
                    _boatCreateMethod.Invoke(boat, new object[] { train });
                }

                if (_boatActorField == null) {
                    _boatActorField = typeof(ActorSimpleComponent).GetField(
                        "actor",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_boatPassengersField == null) {
                    _boatPassengersField = typeof(Boat).GetField(
                        "_passengers",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_boatLastStepField == null) {
                    _boatLastStepField = typeof(Boat).GetField(
                        "_last_step",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_boatMovementAngleField == null) {
                    _boatMovementAngleField = typeof(Boat).GetField(
                        "last_movement_angle",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_boatPassengerWaitCounterField == null) {
                    _boatPassengerWaitCounterField = typeof(Boat).GetField(
                        "passengerWaitCounter",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_boatPickupNearDockField == null) {
                    _boatPickupNearDockField = typeof(Boat).GetField(
                        "pickup_near_dock",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _boatActorField?.SetValue(boat, train);

                if (_boatPassengersField != null) {
                    object passengers = _boatPassengersField.GetValue(boat);
                    if (passengers == null) {
                        _boatPassengersField.SetValue(boat, new HashSet<Actor>());
                    }
                }

                _boatLastStepField?.SetValue(boat, Vector2.zero);
                _boatMovementAngleField?.SetValue(boat, 0);
                _boatPassengerWaitCounterField?.SetValue(boat, 0);
                _boatPickupNearDockField?.SetValue(boat, false);
                GetTaxiRequestField()?.SetValue(boat, null);
                GetTaxiTargetField()?.SetValue(boat, null);
                TryRegisterBoatWithActor(train, boat);
            }
            catch {
            }
        }

        private static void TryRegisterBoatWithActor(Actor actor, Boat boat) {
            if (actor == null || boat == null) {
                return;
            }

            try {
                if (_initComponentsMethod == null) {
                    _initComponentsMethod = typeof(Actor).GetMethod(
                        "initComponents",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null
                    );
                }

                _initComponentsMethod?.Invoke(actor, null);

                if (_getSimpleListMethod == null) {
                    _getSimpleListMethod = typeof(Actor).GetMethod(
                        "getSimpleList",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null
                    );
                }

                IList simpleList = _getSimpleListMethod?.Invoke(actor, null) as IList;
                if (simpleList == null) {
                    return;
                }

                foreach (object entry in simpleList) {
                    if (ReferenceEquals(entry, boat)) {
                        return;
                    }
                }

                simpleList.Add(boat);
            }
            catch {
            }
        }

        private static void SafeEmbarkIntoBoat(Actor actor, Boat boat) {
            if (actor == null || boat == null) {
                return;
            }

            try {
                if (_actorEmbarkIntoMethod == null) {
                    _actorEmbarkIntoMethod = typeof(Actor).GetMethod(
                        "embarkInto",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Boat) },
                        null
                    );
                }

                _actorEmbarkIntoMethod?.Invoke(actor, new object[] { boat });
            }
            catch {
            }

            bool isInsideBoat = SafeActorIsInsideBoat(actor);
            Boat insideBoat = SafeActorInsideBoat(actor);

            if (isInsideBoat && insideBoat == boat) {
                SafeBoatAddPassenger(boat, actor);
                return;
            }

            try {
                if (_actorInsideBoatField == null) {
                    _actorInsideBoatField = typeof(Actor).GetField(
                        "inside_boat",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                if (_actorIsInsideBoatField == null) {
                    _actorIsInsideBoatField = typeof(Actor).GetField(
                        "is_inside_boat",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                _actorInsideBoatField?.SetValue(actor, boat);
                _actorIsInsideBoatField?.SetValue(actor, true);
            }
            catch {
            }

            if (SafeActorIsInsideBoat(actor) && SafeActorInsideBoat(actor) == boat) {
                SafeBoatAddPassenger(boat, actor);
            }
        }

        private static bool SafeBoatHasPassengers(Boat boat) {
            return SafeBoatCountPassengers(boat) > 0;
        }

        private static bool TryGetTrackedPassengers(Boat boat, out HashSet<Actor> trackedPassengers) {
            trackedPassengers = null;
            if (boat == null) {
                return false;
            }

            Actor train = GetBoatActor(boat);
            if (train != null && PassengerActorsByTrainId.TryGetValue(train.id, out trackedPassengers)) {
                return true;
            }

            foreach (KeyValuePair<long, Boat> entry in BoatByTrainId) {
                if (!ReferenceEquals(entry.Value, boat)) {
                    continue;
                }

                if (!PassengerActorsByTrainId.TryGetValue(entry.Key, out trackedPassengers)) {
                    trackedPassengers = new HashSet<Actor>();
                    PassengerActorsByTrainId[entry.Key] = trackedPassengers;
                }

                return true;
            }

            return false;
        }

        private static bool SafeActorIsInsideBoat(Actor actor) {
            if (actor == null) {
                return false;
            }

            try {
                if (_actorIsInsideBoatField == null) {
                    _actorIsInsideBoatField = typeof(Actor).GetField(
                        "is_inside_boat",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                object result = _actorIsInsideBoatField?.GetValue(actor);
                return result is bool boolResult && boolResult;
            }
            catch {
                return false;
            }
        }

        private static Boat SafeActorInsideBoat(Actor actor) {
            if (actor == null) {
                return null;
            }

            try {
                if (_actorInsideBoatField == null) {
                    _actorInsideBoatField = typeof(Actor).GetField(
                        "inside_boat",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _actorInsideBoatField?.GetValue(actor) as Boat;
            }
            catch {
                return null;
            }
        }

        private static int SafeBoatCountPassengers(Boat boat) {
            if (TryGetTrackedPassengers(boat, out HashSet<Actor> trackedPassengers)) {
                trackedPassengers.RemoveWhere(passenger => passenger == null || !passenger.isAlive() || !SafeActorIsInsideBoat(passenger));
                return trackedPassengers.Count;
            }

            if (boat == null) {
                return 0;
            }

            try {
                if (_boatPassengersField == null) {
                    _boatPassengersField = typeof(Boat).GetField(
                        "_passengers",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                object passengers = _boatPassengersField?.GetValue(boat);
                if (passengers is ICollection collection) {
                    return collection.Count;
                }
            }
            catch {
            }

            return 0;
        }

        private static IEnumerable<Actor> SafeGetBoatPassengers(Boat boat) {
            if (TryGetTrackedPassengers(boat, out HashSet<Actor> trackedPassengers)) {
                trackedPassengers.RemoveWhere(passenger => passenger == null || !passenger.isAlive() || !SafeActorIsInsideBoat(passenger));
                foreach (Actor passenger in trackedPassengers) {
                    if (passenger != null) {
                        yield return passenger;
                    }
                }

                yield break;
            }

            if (boat == null) {
                yield break;
            }

            object passengers = null;
            try {
                if (_boatPassengersField == null) {
                    _boatPassengersField = typeof(Boat).GetField(
                        "_passengers",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                passengers = _boatPassengersField?.GetValue(boat);
            }
            catch {
            }

            if (passengers is IEnumerable enumerable) {
                foreach (object obj in enumerable) {
                    if (obj is Actor actor) {
                        yield return actor;
                    }
                }
            }
        }

        private static void SafeUnloadPassengers(Boat boat, WorldTile tile, bool destroyBoat) {
            if (boat == null) {
                return;
            }

            int beforeCount = SafeBoatCountPassengers(boat);
            try {
                if (_boatUnloadPassengersMethod == null) {
                    _boatUnloadPassengersMethod = typeof(Boat).GetMethod(
                        "unloadPassengers",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(WorldTile), typeof(bool) },
                        null
                    );
                }

                _boatUnloadPassengersMethod?.Invoke(boat, new object[] { tile, destroyBoat });
            }
            catch {
            }

            if (beforeCount > 0 && SafeBoatCountPassengers(boat) > 0) {
                foreach (Actor passenger in new List<Actor>(SafeGetBoatPassengers(boat))) {
                    if (passenger == null) {
                        continue;
                    }

                    try {
                        if (_actorInsideBoatField == null) {
                            _actorInsideBoatField = typeof(Actor).GetField(
                                "inside_boat",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                            );
                        }

                        if (_actorIsInsideBoatField == null) {
                            _actorIsInsideBoatField = typeof(Actor).GetField(
                                "is_inside_boat",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                            );
                        }

                        _actorInsideBoatField?.SetValue(passenger, null);
                        _actorIsInsideBoatField?.SetValue(passenger, false);
                    }
                    catch {
                    }

                    if (tile != null) {
                        passenger.current_position = tile.pos;
                        passenger.next_step_position = tile.pos;
                        passenger.next_step_position_possession = tile.pos;
                        SafeSetCurrentTilePosition(passenger, tile);
                    }

                    SafeBoatRemovePassenger(boat, passenger);
                }
            }
        }

        private static void SafeCancelBoatWork(Boat boat, Actor actor) {
            if (boat == null) {
                return;
            }

            try {
                if (_boatCancelWorkMethod == null) {
                    _boatCancelWorkMethod = typeof(Boat).GetMethod(
                        "cancelWork",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Actor) },
                        null
                    );
                }

                _boatCancelWorkMethod?.Invoke(boat, new object[] { actor });
            }
            catch {
            }
        }

        private static void SafeBoatAddPassenger(Boat boat, Actor actor) {
            if (boat == null || actor == null) {
                return;
            }

            if (TryGetTrackedPassengers(boat, out HashSet<Actor> passengers)) {
                passengers.Add(actor);
            }

            try {
                if (_boatAddPassengerMethod == null) {
                    _boatAddPassengerMethod = typeof(Boat).GetMethod(
                        "addPassenger",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Actor) },
                        null
                    );
                }

                _boatAddPassengerMethod?.Invoke(boat, new object[] { actor });
            }
            catch {
            }
        }

        private static void SafeBoatRemovePassenger(Boat boat, Actor actor) {
            if (boat == null || actor == null) {
                return;
            }

            if (TryGetTrackedPassengers(boat, out HashSet<Actor> passengers)) {
                passengers.Remove(actor);
            }

            try {
                if (_boatRemovePassengerMethod == null) {
                    _boatRemovePassengerMethod = typeof(Boat).GetMethod(
                        "removePassenger",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Actor) },
                        null
                    );
                }

                _boatRemovePassengerMethod?.Invoke(boat, new object[] { actor });
            }
            catch {
            }
        }

        private static void SafeAssignTaxiRequest(life.taxi.TaxiRequest request, Boat boat) {
            if (request == null || boat == null) {
                return;
            }

            try {
                if (_requestAssignMethod == null) {
                    _requestAssignMethod = typeof(life.taxi.TaxiRequest).GetMethod(
                        "assign",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Boat) },
                        null
                    );
                }

                _requestAssignMethod?.Invoke(request, new object[] { boat });
            }
            catch {
            }
        }

        private static bool SafeRequestEveryoneEmbarked(life.taxi.TaxiRequest request, Boat boat) {
            if (request == null) {
                return false;
            }

            try {
                if (_requestEveryoneEmbarkedMethod == null) {
                    _requestEveryoneEmbarkedMethod = typeof(life.taxi.TaxiRequest).GetMethod(
                        "everyoneEmbarked",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null
                    );
                }

                object result = _requestEveryoneEmbarkedMethod?.Invoke(request, null);
                if (result is bool boolResult) {
                    return boolResult;
                }
            }
            catch {
            }

            if (boat == null) {
                return false;
            }

            try {
                bool snapshotComplete;
                List<Actor> actors = GetRequestActorsSnapshot(request, out snapshotComplete);
                if (!snapshotComplete && actors.Count == 0) {
                    return false;
                }

                foreach (Actor actor in actors) {
                    if (actor == null || !actor.isAlive()) {
                        continue;
                    }

                    if (!SafeActorIsInsideBoat(actor) || SafeActorInsideBoat(actor) != boat) {
                        return false;
                    }
                }

                return true;
            }
            catch {
                return false;
            }
        }

        private static void SafeCancelLatePassengers(life.taxi.TaxiRequest request) {
            if (request == null) {
                return;
            }

            try {
                if (_requestCancelForLatePassengersMethod == null) {
                    _requestCancelForLatePassengersMethod = typeof(life.taxi.TaxiRequest).GetMethod(
                        "cancelForLatePassengers",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null
                    );
                }

                _requestCancelForLatePassengersMethod?.Invoke(request, null);
            }
            catch {
            }
        }

        private static void SafeFinishTaxiRequest(life.taxi.TaxiRequest request) {
            if (request == null) {
                return;
            }

            try {
                if (_requestFinishMethod == null) {
                    _requestFinishMethod = typeof(life.taxi.TaxiRequest).GetMethod(
                        "finish",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null
                    );
                }

                _requestFinishMethod?.Invoke(request, null);
            }
            catch {
                try {
                    life.taxi.TaxiManager.finish(request);
                }
                catch {
                }
            }
        }

        internal static bool IsTrainActor(Actor actor) {
            return IsTrain(actor);
        }

        internal static void SafeClearTasks(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                GetActorMethod(ref _clearTasksMethod, "clearTasks")?.Invoke(actor, null);
            }
            catch {
            }
        }

        internal static void SafeCancelAllBeh(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                GetActorMethod(ref _cancelAllBehMethod, "cancelAllBeh")?.Invoke(actor, null);
            }
            catch {
            }
        }

        internal static void SafeClearWait(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                GetActorMethod(ref _clearWaitMethod, "clearWait")?.Invoke(actor, null);
            }
            catch {
            }
        }

        internal static void SafeSetStatsDirty(Actor actor) {
            if (actor == null) {
                return;
            }

            try {
                GetActorMethod(ref _setStatsDirtyMethod, "setStatsDirty")?.Invoke(actor, null);
            }
            catch {
            }
        }

        private static MethodInfo GetActorMethod(ref MethodInfo cache, string methodName) {
            if (cache == null) {
                cache = typeof(Actor).GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null
                );
            }

            return cache;
        }

        internal static bool IsTaxiTrainActor(Actor actor) {
            return actor != null
                && actor.asset != null
                && !string.IsNullOrWhiteSpace(actor.asset.id)
                && actor.asset.id.StartsWith(Main.TrainAssetPrefix, StringComparison.Ordinal);
        }

        private static bool IsTrain(Actor actor) {
            return actor != null
                && actor.asset != null
                && !string.IsNullOrWhiteSpace(actor.asset.id)
                && actor.asset.id.StartsWith(Main.TrainAssetPrefix, StringComparison.Ordinal);
        }

        private static life.taxi.TaxiRequest GetTaxiRequest(Boat boat) {
            if (boat == null) {
                return null;
            }

            try {
                return GetTaxiRequestField()?.GetValue(boat) as life.taxi.TaxiRequest;
            }
            catch {
                return null;
            }
        }

        private static void SetTaxiRequest(Boat boat, life.taxi.TaxiRequest request) {
            if (boat == null) {
                return;
            }

            try {
                GetTaxiRequestField()?.SetValue(boat, request);
            }
            catch {
                // keep the live update loop running if the runtime rejects this field write
            }
        }

        private static void ClearTaxiTarget(Boat boat) {
            if (boat == null) {
                return;
            }

            try {
                GetTaxiTargetField()?.SetValue(boat, null);
            }
            catch {
            }
        }

        private static FieldInfo GetTaxiRequestField() {
            if (_boatTaxiRequestField == null) {
                _boatTaxiRequestField = typeof(Boat).GetField(
                    "taxi_request",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            return _boatTaxiRequestField;
        }

        private static FieldInfo GetTaxiTargetField() {
            if (_boatTaxiTargetField == null) {
                _boatTaxiTargetField = typeof(Boat).GetField(
                    "taxi_target",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            return _boatTaxiTargetField;
        }

        private static void TrySetIdleProfession(Actor actor) {
            if (actor == null) {
                return;
            }

            if (_setProfessionMethod == null) {
                _setProfessionMethod = typeof(Actor).GetMethod(
                    "setProfession",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(UnitProfession), typeof(bool) },
                    null);
            }

            _setProfessionMethod?.Invoke(actor, new object[] { UnitProfession.Nothing, true });
        }
    }

    [HarmonyPatch(typeof(Actor), "u8_checkUpdateTimers")]
    internal static class TaxiTrainActorUpdatePatch {
        private static void Postfix(Actor __instance) {
            if (!RoadCarActorSystem.IsManagedRoadCarActor(__instance) && !TaxiTrainLogic.IsTaxiTrainActor(__instance)) {
                return;
            }

            RoadCarActorSystem.UpdateActor(__instance);
            TaxiTrainLogic.UpdateTrain(__instance);
        }
    }

    [HarmonyPatch(typeof(SaveManager), "prepareLoading")]
    internal static class TrainboxPrepareLoadingPatch {
        private static void Prefix() {
            TaxiTrainLogic.ResetLoadRecovery();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "loadActors")]
    internal static class TrainboxLoadActorsPatch {
        private static void Postfix() {
            TaxiTrainLogic.RecoverLoadedTrainBoats();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "loadActorsOld")]
    internal static class TrainboxLoadActorsOldPatch {
        private static void Postfix() {
            TaxiTrainLogic.RecoverLoadedTrainBoats();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "loadBoatStates")]
    internal static class TrainboxLoadBoatStatesPatch {
        private static void Prefix() {
            TaxiTrainLogic.RecoverLoadedTrainBoats();
        }

        private static Exception Finalizer(Exception __exception) {
            if (__exception is NullReferenceException) {
                return null;
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(City), "update")]
    internal static class TaxiTrainCityUpdatePatch {
        private const float CityUpdateIntervalSeconds = 5f;
        private static readonly Dictionary<long, float> NextTickByCityId = new Dictionary<long, float>();

        private static void Postfix(City __instance) {
            if (__instance == null) {
                return;
            }

            float now = Time.time;
            if (NextTickByCityId.TryGetValue(__instance.id, out float nextTickAt) && now < nextTickAt) {
                return;
            }

            NextTickByCityId[__instance.id] = now + CityUpdateIntervalSeconds + ((__instance.id % 7L) * 0.17f);
            RailAutoBuilder.UpdateCity(__instance);
            RoadTrafficSystem.UpdateCity(__instance);
        }
    }

    [HarmonyPatch(typeof(Actor), "moveTo")]
    internal static class TaxiTrainMovePatch {
        private static bool Prefix(Actor __instance, WorldTile pTileTarget) {
            return TaxiTrainLogic.CanMoveToTile(__instance, pTileTarget);
        }
    }

    [HarmonyPatch(typeof(PowersTab), "showTab")]
    internal static class TrainboxTabPatch {
        private static void Postfix() {
            return;
        }
    }

    [HarmonyPatch(typeof(Actor), "embarkInto")]
    internal static class TaxiTrainEmbarkPatch {
        private static bool Prefix(Actor __instance, Boat pBoat) {
            return TaxiTrainLogic.CanEmbark(pBoat);
        }

        private static void Postfix(Actor __instance, Boat pBoat) {
            TaxiTrainLogic.OnPassengerEmbarked(__instance, pBoat);
        }
    }

    [HarmonyPatch(typeof(Actor), "disembarkTo")]
    internal static class TaxiTrainDisembarkPatch {
        private static void Prefix(Actor __instance, Boat pBoat, ref WorldTile pTile) {
            TaxiTrainLogic.OnPassengerDisembarking(__instance, pBoat, ref pTile);
        }

        private static void Postfix(Actor __instance, Boat pBoat, WorldTile pTile) {
            TaxiTrainLogic.OnPassengerDisembarked(__instance, pBoat);
        }
    }

    [HarmonyPatch(typeof(UnitWindow), "showStatsRows")]
    internal static class TaxiTrainUnitWindowPatch {
        private static FieldInfo _unitWindowActorField;

        private static void Postfix(UnitWindow __instance) {
            if (__instance == null) {
                return;
            }

            Actor actor = GetWindowActor(__instance);
            if (!TaxiTrainLogic.IsTrainActor(actor) || actor?.asset?.is_boat == true) {
                return;
            }

            Boat boat = TaxiTrainLogic.GetInspectableTrainBoat(actor);
            if (boat == null) {
                return;
            }

            bool hasPassengers = TaxiTrainLogic.SafeInspectableBoatHasPassengers(boat);
            string tooltipId = hasPassengers ? "passengers" : null;
            TooltipDataGetter tooltipGetter = hasPassengers
                ? new TooltipDataGetter(__instance.getTooltipPassengers)
                : null;

            __instance.showStatRow(
                "passengers",
                TaxiTrainLogic.SafeInspectableBoatPassengerCount(boat),
                (MetaType)0,
                -1L,
                null,
                tooltipId,
                tooltipGetter
            );
        }

        private static Actor GetWindowActor(UnitWindow window) {
            if (window == null) {
                return null;
            }

            try {
                if (_unitWindowActorField == null) {
                    _unitWindowActorField = typeof(UnitWindow).GetField(
                        "actor",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }

                return _unitWindowActorField?.GetValue(window) as Actor;
            }
            catch {
                return null;
            }
        }
    }

    [HarmonyPatch(typeof(PowerButton), "getDescription")]
    internal static class TrainboxPowerDescriptionPatch {
        private static FieldInfo _godPowerField;
        private static PropertyInfo _godPowerProperty;

        private static void Postfix(PowerButton __instance, ref string __result) {
            string powerId = GetGodPower(__instance)?.id;
            if (string.IsNullOrWhiteSpace(powerId)) {
                return;
            }

            if (!string.IsNullOrWhiteSpace(__result)) {
                return;
            }

            if (TrainPowers.TryGetPowerDescription(powerId, out string description)) {
                __result = description;
            }
        }

        private static GodPower GetGodPower(PowerButton button) {
            if (button == null) {
                return null;
            }

            try {
                PropertyInfo prop = _godPowerProperty
                    ?? (_godPowerProperty = typeof(PowerButton).GetProperty(
                        "godPower",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    ));
                if (prop != null) {
                    return prop.GetValue(button, null) as GodPower;
                }

                FieldInfo field = _godPowerField
                    ?? (_godPowerField = typeof(PowerButton).GetField(
                        "godPower",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    ));
                return field?.GetValue(button) as GodPower;
            }
            catch {
                return null;
            }
        }
    }
}
