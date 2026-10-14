using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// M2's creature disasters (CyberDisaster, IceWalkerDisaster,
    /// hashbrowncatdisaster, Vaticandisaster) as native disasters, so the game's
    /// own disaster roll decides when they happen, exactly like the original.
    /// </summary>
    internal static class InvasionService
    {
        private const string ZombieTrait = "zombie";
        private const int VaticanZombieThreshold = 500;

        internal static void Update(float elapsed)
        {
        }

        internal static void RegisterDisasters()
        {
            RegisterLog("modernbox_disaster_cyber", "worldlog_disaster_alien_invasion", "ui/Icons/SolarPoweredCyberBody");
            RegisterLog("modernbox_disaster_ice_walker", "worldlog_disaster_ice_ones", "ui/Icons/Walker_TitanIcon");
            RegisterLog("modernbox_disaster_hashbrown", "worldlog_disaster_alien_invasion", "ui/Icons/iconCat");
            RegisterLog("modernbox_disaster_vatican", "worldlog_disaster_alien_invasion", "ui/Icons/Vatican");

            // rate 0 keeps these two out of the disaster pool, as in the original.
            Add("CyberDisaster", 0, 0f, 10000, 100, "modernbox_disaster_cyber", "Assimilatus", 1, 1, 1,
                SimpleUnitAssetSpawnUsingIslands, "age_hope", "age_sun", "age_wonders");
            Add("IceWalkerDisaster", 0, 0f, 10000, 100, "modernbox_disaster_ice_walker", "Cocytuswalker", 1, 1, 1,
                SimpleUnitAssetSpawnUsingIslands, "age_ice", "age_despair");
            Add("hashbrowncatdisaster", 1, 0.1f, 500, 4, "modernbox_disaster_hashbrown", "hashbrowncat", 5, 5, 1,
                SimpleUnitAssetSpawnUsingIslands, "age_hope", "age_sun", "age_wonders");
            Add("Vaticandisaster", 4, 0.5f, 0, 0, "modernbox_disaster_vatican", "basecrusader", 1000, 300, 300,
                SpawnVaticanDisasterWithTrait, "age_hope", "age_sun", "age_ash", "age_dark", "age_tears", "age_moon",
                "age_chaos", "age_despair", "age_ice", "age_wonders");
        }

        private static void RegisterLog(string id, string localeId, string icon)
        {
            if (AssetManager.world_log_library.get(id) != null) return;
            WorldLogAsset log = AssetManager.world_log_library.clone(id, "$basic_disaster$");
            log.locale_id = localeId;
            log.path_icon = icon;
        }

        private static void Add(string id, int rate, float chance, int population, int cities, string log, string unit,
            int maxExisting, int unitsMin, int unitsMax, DisasterAction action, params string[] ages)
        {
            if (AssetManager.disasters.get(id) != null) return;
            DisasterAsset disaster = new DisasterAsset
            {
                id = id,
                rate = rate,
                chance = chance,
                min_world_population = population,
                min_world_cities = cities,
                world_log = log,
                spawn_asset_unit = unit,
                max_existing_units = maxExisting,
                units_min = unitsMin,
                units_max = unitsMax,
                type = DisasterType.Other,
                premium_only = false,
                action = action
            };
            foreach (string age in ages) disaster.ages_allow.Add(age);
            AssetManager.disasters.add(disaster);
        }

        private static void SimpleUnitAssetSpawnUsingIslands(DisasterAsset disaster)
        {
            if (!CheckUnitSpawnLimits(disaster) || World.world.islands_calculator == null) return;
            TileIsland island = World.world.islands_calculator.getRandomIslandGround();
            if (island == null) return;
            WorldTile tile = island.getRandomTile();
            if (tile == null) return;
            SpawnDisasterUnits(disaster, tile);
            WorldLog.logDisaster(disaster, tile);
        }

        // The original counted every unit of the spawned unit's race.
        private static bool CheckUnitSpawnLimits(DisasterAsset disaster)
        {
            if (string.IsNullOrEmpty(disaster.spawn_asset_unit) || AssetManager.actor_library.get(disaster.spawn_asset_unit) == null) return false;
            string[] race;
            switch (disaster.spawn_asset_unit)
            {
                case "Assimilatus":
                    race = new[] { "assimilator", "assimilarptor", "assimilatrax", "helilator", "assizeppelin", "Assimilatus" };
                    break;
                case "Cocytuswalker":
                    race = new[] { "cold_one", "newwalker", "normalwalker", "icedracoid", "buffrost", "Cocytuswalker" };
                    break;
                case "hashbrowncat":
                    race = new[] { "cat", "hashbrowncat", "peones", "xenodogo" };
                    break;
                default:
                    race = new[] { disaster.spawn_asset_unit };
                    break;
            }
            int count = 0;
            foreach (string id in race)
            {
                ActorAsset asset = AssetManager.actor_library.get(id);
                if (asset != null && asset.units != null) count += asset.units.Count;
            }
            return count < disaster.max_existing_units;
        }

        private static void SpawnDisasterUnits(DisasterAsset disaster, WorldTile tile)
        {
            EffectsLibrary.spawn("fx_spawn", tile);
            int amount = Random.Range(disaster.units_min, disaster.units_max);
            for (int i = 0; i < amount; i++)
            {
                Actor actor = World.world.units.createNewUnit(disaster.spawn_asset_unit, tile);
                if (actor != null) ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(actor);
            }
        }

        private static void SpawnVaticanDisasterWithTrait(DisasterAsset disaster)
        {
            if (!HasSufficientTraitCount(ZombieTrait, VaticanZombieThreshold)) return;
            WorldTile[] tiles = World.world.tiles_list;
            if (tiles == null || tiles.Length == 0) return;
            WorldTile tile = tiles[Random.Range(0, tiles.Length)];
            SpawnDisasterUnits(disaster, tile);
            WorldLog.logDisaster(disaster, tile);
        }

        private static bool HasSufficientTraitCount(string traitId, int requiredCount)
        {
            int count = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || !actor.isAlive() || !actor.hasTrait(traitId)) continue;
                if (++count >= requiredCount) return true;
            }
            return false;
        }
    }
}
