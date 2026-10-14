using System;
using System.Collections.Generic;

namespace ModernBoxM1Rewrite
{
    internal static class BombService
    {
        private static readonly HashSet<int> BuiltBrushes = new HashSet<int>();
        // Buttonz.randomdrop picks one entry from these three lists. They don't line up
        // with the named bombs (radius 50 gets the Cobalt's effect size), as in the original.
        private static readonly int[] RandomRadii = { 50, 120, 100, 100, 400, 5, 786 };
        private static readonly float[] RandomScaleMin = { 0.2f, 0.4f, 0.8f, 1.2f, 4.3f, 0.4f, 16.3f };
        private static readonly float[] RandomScaleMax = { 0.3f, 0.6f, 0.9f, 1.6f, 7.9f, 0.6f, 28.9f };

        // Same sequence as the original drop actions: effect, MapAction.damageWorld, shake.
        internal static void Detonate(WorldTile tile, BombSpec spec)
        {
            if (tile == null || spec == null || World.world == null) return;
            int radius = spec.Radius;
            float scaleMin = spec.EffectScaleMin;
            float scaleMax = spec.EffectScaleMax;
            if (spec.Pattern == BombPattern.RandomLegacy)
            {
                System.Random random = new System.Random();
                int index = random.Next(RandomRadii.Length);
                radius = RandomRadii[index];
                float scale = (float)(RandomScaleMin[index] + random.NextDouble() * (RandomScaleMax[index] - RandomScaleMin[index]));
                scaleMin = scale;
                scaleMax = scale;
            }

            TerraformOptions options = AssetManager.terraform.get(spec.TerraformId);
            if (options == null)
            {
                ModernBoxDiagnostics.Error("Bomb terraform is missing: " + spec.TerraformId);
                return;
            }
            EffectsLibrary.spawnAtTileRandomScale(spec.EffectId, tile, scaleMin, scaleMax);
            // The old engine's blasts never stunned.
            M1NoBlastStun.Begin();
            try { MapAction.damageWorld(tile, EnsureCircleBrush(radius), options, null); }
            finally { M1NoBlastStun.End(); }
            World.world.startShake(0.3f, 0.01f, 2f, true, true);
        }

        // 0.51.2's Brush.get clones circ_1 for sizes the game never pre-built, so large radii
        // would only touch a few tiles. Build the real circle once per radius, like the old
        // Brush.get did. Radii past the map diagonal are capped to it, which still covers the map.
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
    }
}
