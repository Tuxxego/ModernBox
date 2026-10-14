using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Restores custom civilization actors to the native undead enemy pool.
    /// Build 719's chunk enemy cache can omit M2 actors whose wild fallback
    /// kingdom was replaced with their producing city's kingdom after spawn.
    /// The normal attack validator still decides whether a particular zombie can
    /// reach the target (for example, melee zombies cannot hit high aircraft).
    /// </summary>
    internal static class ZombieVehicleTargetingService
    {
        private static readonly HashSet<EnemyFinderData> AugmentedThisFrame = new HashSet<EnemyFinderData>();
        private static readonly HashSet<string> CivilizationUnitIds = new HashSet<string>(StringComparer.Ordinal);
        private static int _lastFrame = -1;

        internal static void IncludeNearbyCivilizationUnits(
            WorldTile origin,
            Kingdom searchingKingdom,
            int requestedChunkRange,
            EnemyFinderData result)
        {
            if (origin == null || origin.chunk == null || searchingKingdom == null ||
                searchingKingdom.asset == null || result == null || result.list == null ||
                World.world == null || World.world.map_chunk_manager == null) return;

            string kingdomId = searchingKingdom.asset.id;
            if (kingdomId != ActorsAndBuildingsRegistry.NativeUndeadKingdomId &&
                kingdomId != ActorsAndBuildingsRegistry.UndeadKingdomId) return;

            // EnemyFinderData is cached by kingdom, origin chunk, and range. Many
            // zombies can request the same object in one frame, so augment that
            // cache once rather than repeating the surrounding-chunk scan for
            // every actor. The set is discarded on the next rendered frame.
            if (_lastFrame != Time.frameCount)
            {
                _lastFrame = Time.frameCount;
                AugmentedThisFrame.Clear();
            }
            if (!AugmentedThisFrame.Add(result)) return;
            EnsureCivilizationUnitIds();

            int range = requestedChunkRange;
            if (range < 0)
                range = SimGlobals.m == null ? 1 : SimGlobals.m.unit_chunk_sight_range;
            range = Math.Max(0, Math.Min(range, 4));

            MapChunk center = origin.chunk;
            for (int offsetX = -range; offsetX <= range; offsetX++)
            {
                for (int offsetY = -range; offsetY <= range; offsetY++)
                {
                    MapChunk chunk = World.world.map_chunk_manager.get(center.x + offsetX, center.y + offsetY);
                    if (chunk == null || chunk.objects == null || chunk.objects.units_all == null) continue;
                    AddEligibleUnits(chunk.objects.units_all, searchingKingdom, result.list);
                }
            }
        }

        private static void EnsureCivilizationUnitIds()
        {
            if (CivilizationUnitIds.Count > 0) return;
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
                if (spec != null && spec.Role != M2UnitRole.Creature)
                    CivilizationUnitIds.Add(spec.Id);
        }

        private static void AddEligibleUnits(
            IEnumerable<Actor> candidates,
            Kingdom searchingKingdom,
            List<BaseSimObject> targets)
        {
            foreach (Actor candidate in candidates)
            {
                if (candidate == null || !candidate.isAlive() || candidate.asset == null ||
                    candidate.kingdom == null || candidate.kingdom == searchingKingdom ||
                    !candidate.kingdom.isCiv() || !candidate.asset.can_be_killed_by_stuff) continue;

                if (!CivilizationUnitIds.Contains(candidate.asset.id)) continue;
                if (!searchingKingdom.isEnemy(candidate.kingdom)) continue;
                if (!targets.Contains(candidate)) targets.Add(candidate);
            }
        }
    }
}
