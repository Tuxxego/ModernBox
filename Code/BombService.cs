using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class BombService
    {
        private sealed class BombJob
        {
            internal int CenterX;
            internal int CenterY;
            internal int Radius;
            internal int X;
            internal int Y;
            internal TerraformOptions Options;
            internal string Name;
            internal long Processed;
        }

        private static readonly Queue<BombJob> Jobs = new Queue<BombJob>();
        private static readonly int[] RandomRadii = { 50, 120, 100, 100, 400, 5, 786 };
        private static readonly float[] RandomScaleMin = { 0.2f, 0.4f, 0.8f, 1.2f, 4.3f, 0.4f, 16.3f };
        private static readonly float[] RandomScaleMax = { 0.3f, 0.6f, 0.9f, 1.6f, 7.9f, 0.6f, 28.9f };

        internal static int PendingJobs { get { return Jobs.Count; } }

        internal static void Enqueue(WorldTile tile, BombSpec spec)
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
            BombJob job = new BombJob
            {
                CenterX = tile.x,
                CenterY = tile.y,
                Radius = radius,
                X = Math.Max(0, tile.x - radius),
                Y = Math.Max(0, tile.y - radius),
                Options = options,
                Name = spec.DisplayName
            };
            Jobs.Enqueue(job);
            ModernBoxDiagnostics.Info("Queued " + spec.DisplayName + " radius " + radius + ".");
        }

        internal static void Update()
        {
            if (Jobs.Count == 0 || World.world == null) return;
            float deadline = Time.realtimeSinceStartup + 0.004f;
            int budget = 4096;
            while (Jobs.Count > 0 && budget-- > 0 && Time.realtimeSinceStartup < deadline)
            {
                BombJob job = Jobs.Peek();
                if (!ProcessNextTile(job))
                {
                    Jobs.Dequeue();
                    ModernBoxDiagnostics.Info(job.Name + " completed after processing " + job.Processed + " tile(s).");
                }
            }
        }

        private static bool ProcessNextTile(BombJob job)
        {
            int maxX = Math.Min(MapBox.width - 1, job.CenterX + job.Radius);
            int maxY = Math.Min(MapBox.height - 1, job.CenterY + job.Radius);
            if (job.Y > maxY) return false;

            int x = job.X;
            int y = job.Y;
            job.X++;
            if (job.X > maxX)
            {
                job.X = Math.Max(0, job.CenterX - job.Radius);
                job.Y++;
            }

            long dx = x - job.CenterX;
            long dy = y - job.CenterY;
            long radiusSquared = (long)job.Radius * job.Radius;
            long distanceSquared = dx * dx + dy * dy;
            if (distanceSquared > radiusSquared) return true;

            WorldTile tile = World.world.GetTileSimple(x, y);
            WorldTile center = World.world.GetTileSimple(job.CenterX, job.CenterY);
            if (tile == null || center == null) return true;
            float distance = Mathf.Sqrt(distanceSquared);

            if (tile.hasBuilding() && job.Options.damage_buildings && tile.building != null && tile.building.isAlive())
                tile.building.getHit(job.Options.damage, true, AttackType.Explosion, null, true, false, true);

            if (tile.hasUnits())
            {
                tile.doUnits(actor =>
                {
                    if (actor == null || !actor.isAlive()) return;
                    actor.getHit(job.Options.damage, true, AttackType.Explosion, null, true, false, true);
                    if (actor.isAlive() && job.Options.apply_force) actor.applyRandomForce(0.4f, Math.Max(0.8f, job.Options.force_power));
                });
            }

            if (job.Options.remove_tornado) MapAction.tryRemoveTornadoFromTile(tile);
            if (job.Options.set_fire || job.Options.explode_and_set_random_fire) tile.startFire(false);
            if (job.Options.explode_tile) MapAction.explodeTile(tile, distance, job.Radius, center, job.Options);
            job.Processed++;
            return true;
        }
    }
}
