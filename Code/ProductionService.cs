using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ModernBoxM1Rewrite
{
    // The old game's UnitSpawner, which M1's factories and barracks used. A building got
    // one only if its factory toggle was on when the building was made or loaded. It then
    // made a unit after 1 second and every 10 seconds after that, for free, while it had
    // fewer than 5 of its units alive. Each unit joined the building's city.
    internal static class ProductionService
    {
        private const int UnitLimit = 5;
        private const float SpawnInterval = 10f;
        private const float FirstSpawn = 1f;

        private sealed class Spawner
        {
            internal bool Active;
            internal float Timer = FirstSpawn;
            internal readonly List<KeyValuePair<long, Actor>> Units = new List<KeyValuePair<long, Actor>>();
        }

        private static readonly Dictionary<long, Spawner> Spawners = new Dictionary<long, Spawner>();
        private static readonly List<long> Gone = new List<long>();
        private static float _tick;
        private static float _repairTick;

        // Building ids repeat in the next world, and a reloaded building gets a new spawner.
        internal static void ResetWorldState()
        {
            Spawners.Clear();
            _tick = 0f;
        }

        internal static void Update(float elapsed)
        {
            _tick += elapsed;
            if (_tick < 0.5f) return;
            float step = _tick;
            _tick = 0f;
            if (World.world == null || World.world.cities == null) return;
            // A new world has no ModernKingdom yet, and old saves can hold units without a kingdom.
            _repairTick -= step;
            if (_repairTick <= 0f)
            {
                _repairTick = 5f;
                ActorsAndBuildingsRegistry.RepairUnits();
            }
            Gone.Clear();
            foreach (long id in Spawners.Keys) Gone.Add(id);
            foreach (City city in World.world.cities.list.ToArray())
            {
                if (city == null || city.isRekt() || city.buildings == null) continue;
                foreach (Building building in city.buildings.ToArray())
                {
                    if (building == null || building.asset == null) continue;
                    BuildingSpec spec = ContentRegistry.FindBuilding(building.asset.id);
                    if (spec == null || string.IsNullOrEmpty(spec.SpawnUnit)) continue;
                    long key = building.id;
                    Gone.Remove(key);
                    Spawner spawner;
                    if (!Spawners.TryGetValue(key, out spawner))
                    {
                        spawner = new Spawner { Active = M1Toggles.SpawnsUnits(spec.Id) };
                        Spawners[key] = spawner;
                    }
                    if (!spawner.Active || !building.isAlive() || building.isRuin() || building.isAbandoned()) continue;
                    spawner.Timer -= step;
                    if (spawner.Timer > 0f) continue;
                    spawner.Timer = SpawnInterval;
                    try { Spawn(city, building, spec, spawner); }
                    catch (Exception exception) { ModernBoxDiagnostics.Error("Factory " + spec.Id + " failed: " + exception); }
                }
            }
            foreach (long id in Gone) Spawners.Remove(id);
        }

        private static void Spawn(City city, Building building, BuildingSpec spec, Spawner spawner)
        {
            spawner.Units.RemoveAll(pair => pair.Value == null || !pair.Value.isAlive() || pair.Value.getID() != pair.Key);
            if (spawner.Units.Count >= UnitLimit) return;
            WorldTile tile = building.current_tile ?? city.getTile(false);
            if (tile == null) return;
            ModernUnitSpec unit = ContentRegistry.FindUnit(spec.SpawnUnit);
            Actor actor = World.world.units.createNewUnit(spec.SpawnUnit, tile, false, unit != null && unit.Flying ? 1.5f : 0f, null);
            if (actor == null) return;
            spawner.Units.Add(new KeyValuePair<long, Actor>(actor.getID(), actor));
            actor.setCity(city);
            actor.setKingdom(city.kingdom);
            ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(actor);
            actor.setHomeBuilding(building);
            if (unit != null && unit.Humanoid) city.makeWarrior(actor);
            else
            {
                actor.setProfession(UnitProfession.Warrior, true);
                if (city.status != null) city.status.warriors_current++;
            }
        }
    }

    [HarmonyPatch(typeof(MapBox), nameof(MapBox.clearWorld))]
    internal static class M1WorldReset
    {
        private static void Postfix()
        {
            ProductionService.ResetWorldState();
        }
    }
}
