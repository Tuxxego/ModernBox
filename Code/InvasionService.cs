using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class InvasionService
    {
        private sealed class SpawnBatch
        {
            internal string[] ActorIds;
            internal string FactionId;
            internal int Remaining;
            internal int WorldCap;
            internal WorldTile SpawnTile;
        }

        private static readonly Queue<SpawnBatch> Batches = new Queue<SpawnBatch>();
        private static float _checkTimer;
        private static int _worldKey;
        private static bool _hashbrownTriggered;
        private static bool _vaticanTriggered;

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.isPaused()) return;
            int key = World.world.GetHashCode();
            if (key != _worldKey)
            {
                _worldKey = key;
                _hashbrownTriggered = false;
                _vaticanTriggered = false;
                Batches.Clear();
            }
            ProcessBatch();
            if (!ModernBoxSettings.Get("AutomaticInvasionsOption")) return;
            _checkTimer += elapsed;
            if (_checkTimer < 30f) return;
            _checkTimer = 0f;
            CheckHashbrown();
            CheckVatican();
        }

        private static void CheckHashbrown()
        {
            if (_hashbrownTriggered || World.world.cities.list.Count < 4 || TotalPopulation() < 500) return;
            _hashbrownTriggered = true;
            Batches.Enqueue(new SpawnBatch
            {
                ActorIds = new[] { "hashbrowncat" },
                FactionId = "ModernBoxM2Invasion",
                Remaining = UnityEngine.Random.Range(1, 6),
                WorldCap = 50
            });
            ModernBoxDiagnostics.Info("Queued bounded Hashbrown invasion.");
        }

        private static void CheckVatican()
        {
            if (_vaticanTriggered || CountActors(candidate => candidate.asset != null && candidate.asset.id.IndexOf("zombie", System.StringComparison.OrdinalIgnoreCase) >= 0) < 500) return;
            _vaticanTriggered = true;
            Batches.Enqueue(new SpawnBatch
            {
                // Include M2's full Vatican force in the initial counter-invasion.
                // Base crusaders remain weighted three times more heavily and may
                // still evolve into any of the specialist units after six kills.
                ActorIds = new[] { "basecrusader", "basecrusader", "basecrusader", "crusaderdreadnaught", "crusaderHeli", "crusadermaus" },
                FactionId = ActorsAndBuildingsRegistry.CrusaderKingdomId,
                Remaining = 300,
                WorldCap = 1000,
                // Original M2 chose one unrestricted random world tile and
                // created the entire Vatican force at that location.
                SpawnTile = RandomSpawnTile()
            });
            ModernBoxDiagnostics.Info("Queued a mixed 300-unit Vatican counter-invasion across multiple frames.");
        }

        private static void ProcessBatch()
        {
            if (Batches.Count == 0) return;
            SpawnBatch batch = Batches.Peek();
            int current = CountActors(candidate => candidate.kingdom != null && candidate.kingdom.asset != null && candidate.kingdom.asset.id == batch.FactionId);
            int allowance = Mathf.Min(10, Mathf.Min(batch.Remaining, batch.WorldCap - current));
            for (int i = 0; i < allowance; i++)
            {
                WorldTile tile = batch.SpawnTile ?? RandomSpawnTile();
                if (tile == null) break;
                string actorId = batch.ActorIds[UnityEngine.Random.Range(0, batch.ActorIds.Length)];
                ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == actorId);
                Actor actor = World.world.units.spawnNewUnit(actorId, tile, true, true, spec != null && spec.Flying ? 2f : 0f, null, false, true);
                if (actor != null)
                {
                    ActorsAndBuildingsRegistry.MakeInvasionActor(actor, batch.FactionId);
                    batch.Remaining--;
                }
            }
            if (batch.Remaining <= 0 || current >= batch.WorldCap) Batches.Dequeue();
        }

        private static int TotalPopulation()
        {
            int total = 0;
            foreach (City city in World.world.cities.list) if (city != null && !city.isRekt()) total += city.getPopulationPeople();
            return total;
        }

        private static int CountActors(System.Predicate<Actor> predicate)
        {
            int count = 0;
            foreach (Actor actor in World.world.units) if (actor != null && actor.isAlive() && predicate(actor)) count++;
            return count;
        }

        private static WorldTile RandomSpawnTile()
        {
            // Original M2 used World.world.tilesList.GetRandom(), so the Vatican
            // counter-invasion remains possible even after zombies erase every
            // civilization and city from the map.
            WorldTile[] tiles = World.world.tiles_list;
            return tiles == null || tiles.Length == 0
                ? null
                : tiles[UnityEngine.Random.Range(0, tiles.Length)];
        }
    }
}
