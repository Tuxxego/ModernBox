using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class BombService
    {
        private sealed class BombJob
        {
            internal MapBox Map;
            internal WorldTile CenterTile;
            internal int CenterX;
            internal int CenterY;
            internal int Radius;
            internal int MaxShell;
            internal int Shell = -1;
            internal int ShellIndex;
            internal readonly List<Vector2Int> ShellPoints = new List<Vector2Int>();
            internal TerraformOptions Options;
            internal string Name;
            internal bool ErasesToDeepOcean;
            internal long Processed;
        }

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

        private const int TileBudgetPerFrame = 1024;

        private static readonly Queue<BombJob> Jobs = new Queue<BombJob>();
        private static readonly List<PatternJob> Patterns = new List<PatternJob>();
        private static readonly int[] RandomRadii = { 50, 120, 100, 100, 400, 5, 786 };
        private static readonly float[] RandomScaleMin = { 0.2f, 0.4f, 0.8f, 1.2f, 4.3f, 0.4f, 16.3f };
        private static readonly float[] RandomScaleMax = { 0.3f, 0.6f, 0.9f, 1.6f, 7.9f, 0.6f, 28.9f };

        internal static int PendingJobs { get { return Jobs.Count + Patterns.Count; } }

        // M1 called MapAction.damageWorld with radii far beyond the game's built-in
        // brush sizes. WorldBox 0.51.2 no longer generates those missing circular
        // brushes correctly: Brush.get clones circ_1, leaving every large call with
        // a one-tile footprint. Process the same circular area explicitly so the M1
        // radii remain real, while spreading the work across frames to avoid a long
        // simulation stall.
        internal static void EnqueueBlast(WorldTile tile, BombSpec spec)
        {
            if (tile == null || spec == null || World.world == null) return;

            if (spec.Pattern == BombPattern.VisualOnly)
            {
                EffectsLibrary.spawnAtTileRandomScale(spec.EffectId, tile, spec.EffectScaleMin, spec.EffectScaleMax);
                World.world.startShake(0.3f, 0.01f, 2f, true, true);
                return;
            }
            if (spec.Pattern == BombPattern.ClusterNuke || spec.Pattern == BombPattern.ClusterLightning)
            {
                Patterns.Add(new PatternJob
                {
                    Map = World.world,
                    Center = tile,
                    Spec = spec,
                    NextEmission = Time.time,
                    Remaining = 25
                });
                return;
            }
            if (spec.Pattern == BombPattern.Spreader)
            {
                Patterns.Add(new PatternJob
                {
                    Map = World.world,
                    Center = tile,
                    Spec = spec,
                    NextEmission = Time.time,
                    Remaining = 4,
                    Frontier = new List<WorldTile> { tile }
                });
                return;
            }

            EnqueueRadial(tile, spec);
        }

        private static void EnqueueRadial(WorldTile tile, BombSpec spec)
        {
            if (tile == null || spec == null || World.world == null) return;

            int radius = spec.Radius;
            float scaleMin = spec.EffectScaleMin;
            float scaleMax = spec.EffectScaleMax;
            if (spec.Id == "Random")
            {
                int index = UnityEngine.Random.Range(0, RandomRadii.Length);
                radius = RandomRadii[index];
                scaleMin = RandomScaleMin[index];
                scaleMax = RandomScaleMax[index];
            }

            TerraformOptions options = AssetManager.terraform.get(spec.TerraformId);
            if (options == null)
            {
                ModernBoxDiagnostics.Error("Bomb terraform asset is missing: " + spec.TerraformId);
                return;
            }

            EffectsLibrary.spawnAtTileRandomScale(spec.EffectId, tile, scaleMin, scaleMax);
            World.world.startShake(0.3f, 0.01f, 2f, true, true);
            World.world.resetRedrawTimer();
            if (options.remove_tornado) MapAction.tryRemoveTornadoFromTile(tile);

            long maxDx = Math.Max(tile.x, MapBox.width - 1 - tile.x);
            long maxDy = Math.Max(tile.y, MapBox.height - 1 - tile.y);
            int lastWorldShell = (int)Math.Floor(Math.Sqrt(maxDx * maxDx + maxDy * maxDy));
            Jobs.Enqueue(new BombJob
            {
                Map = World.world,
                CenterTile = tile,
                CenterX = tile.x,
                CenterY = tile.y,
                Radius = radius,
                MaxShell = Math.Min(radius, lastWorldShell),
                Options = options,
                Name = spec.DisplayName,
                ErasesToDeepOcean = spec.TerraformId == "destroy_no_flash"
            });

            ModernBoxDiagnostics.Info("Queued M2 " + spec.DisplayName + " at radius " + radius + ".");
        }

        internal static void Update()
        {
            if ((Jobs.Count == 0 && Patterns.Count == 0) || World.world == null) return;
            UpdatePatterns();

            int budget = TileBudgetPerFrame;
            while (Jobs.Count > 0 && budget-- > 0)
            {
                // Rotate after every tile so a Jupiter or Eraser job cannot block
                // newer bomb landings until its entire radius has finished.
                BombJob job = Jobs.Dequeue();
                if (job.Map == World.world && ProcessNextTile(job))
                {
                    Jobs.Enqueue(job);
                }
                else if (job.Map == World.world)
                {
                    ModernBoxDiagnostics.Info(job.Name + " completed at radius " + job.Radius +
                        " after processing " + job.Processed + " tile(s).");
                }
            }
        }

        internal static void Clear()
        {
            Jobs.Clear();
            Patterns.Clear();
        }

        private static void UpdatePatterns()
        {
            for (int index = Patterns.Count - 1; index >= 0; index--)
            {
                PatternJob pattern = Patterns[index];
                if (pattern.Map != World.world)
                {
                    Patterns.RemoveAt(index);
                    continue;
                }
                if (Time.time < pattern.NextEmission) continue;
                if (pattern.Spec.Pattern == BombPattern.ClusterNuke || pattern.Spec.Pattern == BombPattern.ClusterLightning)
                {
                    WorldTile tile = RandomTile(pattern.Center, 35);
                    if (tile != null) EnqueueRadial(tile, pattern.Spec);
                    pattern.Remaining--;
                    pattern.NextEmission = Time.time + 0.2f;
                }
                else if (pattern.Spec.Pattern == BombPattern.Spreader)
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
                            EnqueueRadial(tile, pattern.Spec);
                            next.Add(tile);
                        }
                    }
                    pattern.Frontier = next;
                    pattern.Wave++;
                    pattern.Remaining--;
                    pattern.NextEmission = Time.time + 1f;
                }
                if (pattern.Remaining <= 0) Patterns.RemoveAt(index);
            }
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

        private static bool ProcessNextTile(BombJob job)
        {
            while (job.ShellIndex >= job.ShellPoints.Count)
            {
                job.Shell++;
                if (job.Shell > job.MaxShell) return false;
                BuildShell(job);
            }

            Vector2Int position = job.ShellPoints[job.ShellIndex++];
            int x = position.x;
            int y = position.y;
            long dx = x - job.CenterX;
            long dy = y - job.CenterY;
            long distanceSquared = dx * dx + dy * dy;
            WorldTile tile = World.world.GetTileSimple(x, y);
            if (tile == null) return true;

            if (job.ErasesToDeepOcean)
            {
                MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, job.Options, false);
            }
            else
            {
                ApplyNuclearTile(tile, Mathf.Sqrt(distanceSquared), job);
            }

            job.Processed++;
            return true;
        }

        // Enumerate integer-distance shells instead of scanning the bounding box
        // from one corner. Every tile in shell N has floor(distance) == N, so all
        // center damage finishes before the next outward ring begins.
        private static void BuildShell(BombJob job)
        {
            job.ShellPoints.Clear();
            job.ShellIndex = 0;

            int shell = job.Shell;
            long innerSquared = (long)shell * shell;
            long outerSquared = shell == job.Radius
                ? innerSquared
                : (long)(shell + 1) * (shell + 1) - 1;
            int minY = Math.Max(0, job.CenterY - shell);
            int maxY = Math.Min(MapBox.height - 1, job.CenterY + shell);

            for (int y = minY; y <= maxY; y++)
            {
                long dy = y - job.CenterY;
                long dySquared = dy * dy;
                long maximumDxSquared = outerSquared - dySquared;
                if (maximumDxSquared < 0) continue;

                int maximumAbsDx = FloorSqrt(maximumDxSquared);
                long minimumDxSquared = innerSquared - dySquared;
                int minimumAbsDx = minimumDxSquared <= 0 ? 0 : CeilSqrt(minimumDxSquared);
                for (int dx = -maximumAbsDx; dx <= maximumAbsDx; dx++)
                {
                    if (Math.Abs(dx) < minimumAbsDx) continue;
                    int x = job.CenterX + dx;
                    if (x >= 0 && x < MapBox.width) job.ShellPoints.Add(new Vector2Int(x, y));
                }
            }
        }

        private static int FloorSqrt(long value)
        {
            int result = (int)Math.Floor(Math.Sqrt(value));
            while ((long)(result + 1) * (result + 1) <= value) result++;
            while ((long)result * result > value) result--;
            return result;
        }

        private static int CeilSqrt(long value)
        {
            int floor = FloorSqrt(value);
            return (long)floor * floor == value ? floor : floor + 1;
        }

        private static void ApplyNuclearTile(WorldTile tile, float distance, BombJob job)
        {
            TerraformOptions options = job.Options;

            if (options.add_burned && !tile.Type.liquid) tile.setBurned(-1);
            if (options.lightning_effect) MapAction.applyLightningEffect(tile);
            if (options.add_heat != 0) World.world.heat.addTile(tile, options.add_heat);

            if (tile.hasBuilding() && options.damage_buildings && tile.building != null && tile.building.isAlive())
            {
                tile.building.getHit(options.damage, true, AttackType.Explosion, null, true, false, true);
            }

            if (options.apply_force && tile.hasUnits())
            {
                tile.doUnits(actor =>
                {
                    if (actor != null && actor.isAlive())
                        actor.applyRandomForce(0.4f, Math.Max(0.8f, options.force_power));
                });
            }

            if (options.set_fire) tile.startFire(true);

            bool exploded = false;
            if (options.explode_tile)
                exploded = MapAction.explodeTile(tile, distance, job.Radius, job.CenterTile, options);

            if (options.transform_to_wasteland && !exploded)
                MapAction.checkAcidTerraform(tile);
        }
    }
}

