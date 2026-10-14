using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class BombService
    {
        private sealed class PatternJob
        {
            internal MapBox Map;
            internal WorldTile Center;
            internal BombSpec Spec;
            internal float NextEmission;
            internal int Remaining;
            internal int Wave;
            internal List<WorldTile> Frontier;
        }

        private static readonly List<PatternJob> Patterns = new List<PatternJob>();
        private static readonly HashSet<int> BuiltBrushes = new HashSet<int>();
        private static readonly int[] RandomRadii = { 50, 120, 100, 100, 400, 5, 786 };
        private static readonly float[] RandomScaleMin = { 0.2f, 0.4f, 0.8f, 1.2f, 4.3f, 0.4f, 16.3f };
        private static readonly float[] RandomScaleMax = { 0.3f, 0.6f, 0.9f, 1.6f, 7.9f, 0.6f, 28.9f };

        internal static int PendingJobs { get { return Patterns.Count; } }

        internal static void EnqueueBlast(WorldTile tile, BombSpec spec)
        {
            if (tile == null || spec == null || World.world == null) return;

            if (spec.Pattern == BombPattern.ClusterNuke || spec.Pattern == BombPattern.ClusterLightning)
            {
                Patterns.Add(new PatternJob { Map = World.world, Center = tile, Spec = spec, NextEmission = Now, Remaining = 25 });
                return;
            }
            if (spec.Pattern == BombPattern.Spreader)
            {
                Patterns.Add(new PatternJob
                {
                    Map = World.world,
                    Center = tile,
                    Spec = spec,
                    NextEmission = Now,
                    Remaining = 4,
                    Frontier = new List<WorldTile> { tile }
                });
                return;
            }

            Detonate(tile, spec);
        }

        // Same sequence as M2's action_*Click: effect, MapAction.damageWorld, shake.
        private static void Detonate(WorldTile tile, BombSpec spec)
        {
            if (tile == null || spec == null || World.world == null) return;

            int radius = spec.Radius;
            float scaleMin = spec.EffectScaleMin;
            float scaleMax = spec.EffectScaleMax;
            if (spec.Pattern == BombPattern.RandomLegacy)
            {
                int index = UnityEngine.Random.Range(0, RandomRadii.Length);
                radius = RandomRadii[index];
                float scale = UnityEngine.Random.Range(RandomScaleMin[index], RandomScaleMax[index]);
                scaleMin = scale;
                scaleMax = scale;
            }

            TerraformOptions options = AssetManager.terraform.get(spec.TerraformId);
            if (options == null)
            {
                ModernBoxDiagnostics.Error("Bomb terraform asset is missing: " + spec.TerraformId);
                return;
            }

            EffectsLibrary.spawnAtTileRandomScale(spec.EffectId, tile, scaleMin, scaleMax);
            // The original bombs used the old engine's blasts, which never stunned.
            M2NoBlastStun.Begin();
            try { MapAction.damageWorld(tile, EnsureCircleBrush(radius), options, null); }
            finally { M2NoBlastStun.End(); }
            World.world.startShake(spec.Pattern == BombPattern.Spreader ? 0.4f : 0.3f, 0.01f, 2f, true, true);
        }

        // 0.51.2's Brush.get clones circ_1 for sizes the game never pre-built,
        // so large radii would only touch a few tiles. Build the real circle once
        // per radius. Radii larger than the map diagonal are capped to it, which
        // still reaches every tile on the map.
        private static int EnsureCircleBrush(int radius)
        {
            int diagonal = (int)Math.Ceiling(Math.Sqrt((double)MapBox.width * MapBox.width + (double)MapBox.height * MapBox.height));
            if (diagonal > 0 && radius > diagonal) radius = diagonal;
            if (radius < 1) radius = 1;
            if (BuiltBrushes.Contains(radius)) return radius;
            BrushData brush = Brush.get(radius, "circ_");
            if (brush == null) return radius;
            if (brush.pos == null || brush.width < radius * 2 - 1)
            {
                List<BrushPixelData> pixels = new List<BrushPixelData>();
                int radiusSquared = radius * radius;
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        int distanceSquared = x * x + y * y;
                        if (distanceSquared > radiusSquared) continue;
                        pixels.Add(new BrushPixelData(x, y, (int)Math.Sqrt(distanceSquared)));
                    }
                }
                brush.pos = pixels.ToArray();
                brush.width = radius * 2 + 1;
                brush.height = radius * 2 + 1;
                brush.sqr_size = brush.width * brush.height;
            }
            BuiltBrushes.Add(radius);
            return radius;
        }

        // World time: the original coroutines waited in scaled time, so bursts follow game speed.
        private static float Now => World.world == null ? 0f : (float)World.world.getCurWorldTime();

        internal static void Update()
        {
            if (Patterns.Count == 0 || World.world == null) return;
            for (int index = Patterns.Count - 1; index >= 0; index--)
            {
                PatternJob pattern = Patterns[index];
                if (pattern.Map != World.world)
                {
                    Patterns.RemoveAt(index);
                    continue;
                }
                if (Now < pattern.NextEmission) continue;
                if (pattern.Spec.Pattern == BombPattern.Spreader)
                {
                    List<WorldTile> next = new List<WorldTile>();
                    int distance = 15 * (pattern.Wave + 1);
                    float[] angles = { -35f, 35f, -145f, 145f };
                    foreach (WorldTile origin in pattern.Frontier)
                    {
                        foreach (float angle in angles)
                        {
                            WorldTile tile = TileAtAngle(origin, angle, distance);
                            if (tile == null) continue;
                            Detonate(tile, pattern.Spec);
                            next.Add(tile);
                        }
                    }
                    pattern.Frontier = next;
                    pattern.Wave++;
                    pattern.NextEmission = Now + 1f;
                }
                else
                {
                    WorldTile tile = RandomTile(pattern.Center, 35);
                    if (tile != null) Detonate(tile, pattern.Spec);
                    pattern.NextEmission = Now + 0.2f;
                }
                pattern.Remaining--;
                if (pattern.Remaining <= 0) Patterns.RemoveAt(index);
            }
        }

        internal static void Clear()
        {
            Patterns.Clear();
        }

        private static WorldTile RandomTile(WorldTile center, int radius)
        {
            if (center == null) return null;
            int x = center.x + UnityEngine.Random.Range(-radius, radius + 1);
            int y = center.y + UnityEngine.Random.Range(-radius, radius + 1);
            return x < 0 || y < 0 || x >= MapBox.width || y >= MapBox.height ? null : World.world.GetTileSimple(x, y);
        }

        private static WorldTile TileAtAngle(WorldTile origin, float angle, float distance)
        {
            if (origin == null) return null;
            int x = origin.x + Mathf.RoundToInt(distance * Mathf.Cos(angle * Mathf.Deg2Rad));
            int y = origin.y + Mathf.RoundToInt(distance * Mathf.Sin(angle * Mathf.Deg2Rad));
            return x < 0 || y < 0 || x >= MapBox.width || y >= MapBox.height ? null : World.world.GetTileSimple(x, y);
        }
    }
}
