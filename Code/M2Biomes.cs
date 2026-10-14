using System.Collections.Generic;
using ai;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Tartarus and Glitch biomes from M2's Creatures.cs, plus the Alien Jungle
    /// fog waves from "biome wave effect". Like the original, both biomes have a
    /// generator amount of 0: they exist and spread, but normal map generation
    /// only places them when something (the planet generator) adds them to the pool.
    /// </summary>
    internal static class M2Biomes
    {
        internal const string TartarusBiomeId = "biome_tartarus";
        internal const string GlitchBiomeId = "biome_Glitch";
        internal const int TartarusGeneratorAmount = 0;
        internal const int GlitchGeneratorAmount = 0;
        private const string FogEffectId = "fogjungle";

        internal static void Register()
        {
            RegisterTartarus();
            RegisterGlitch();
            RegisterFog();
        }

        private static void RegisterTartarus()
        {
            if (AssetManager.biome_library.get(TartarusBiomeId) != null) return;
            Vegetation("tartarus_desert_bones_big", "jungle_tree", false, 1, 0.1f, false, Res("bones", 10, "wood", 5, "stone", 5));
            Vegetation("tartarus_tar_bones_big", "jungle_tree", false, 1, 0.1f, false, Res("bones", 10, "wood", 5, "stone", 5));
            Vegetation("tartarus_vent", "jungle_tree", false, 1, 0.1f, false, Res("mythril", 1, "wood", 5, "stone", 20));
            Vegetation("tartarus_desert_bones", "jungle_plant", false, 3, 0.1f, false, Res("bones", 1, "wood", 1, "stone", 1));
            Vegetation("tartarus_tar_bones", "jungle_plant", false, 3, 0.1f, false, Res("bones", 1, "wood", 1, "stone", 1));
            Vegetation("tartarus_ruins", "jungle_plant", false, 3, 0.1f, false, Res("common_metals", 2, "wood", 1, "stone", 1));

            BiomeAsset biome = Biome(TartarusBiomeId, "tartarus_low", "tartarus_high", TartarusGeneratorAmount);
            biome.addTree("tartarus_desert_bones_big", 1);
            biome.addTree("tartarus_tar_bones_big", 1);
            biome.addTree("tartarus_vent", 1);
            biome.addPlant("tartarus_desert_bones", 2);
            biome.addPlant("tartarus_tar_bones", 1);
            biome.addPlant("tartarus_ruins", 1);
            biome.addUnit("scandid", 4);
            biome.addUnit("Duneworm", 1);
            biome.addMineral("mineral_bones", 20);
            biome.addMineral("mineral_stone", 20);
            biome.addMineral("mineral_metals", 5);
            AssetManager.biome_library.add(biome);

            // tartarus_low was a liquid "tar" tile in M2. A liquid top tile on dry
            // land breaks 0.51.2 island/region pathing, so it stays walkable ground.
            Tile("tartarus_low", "infernal_low", biome, TileRank.Low, "#272727", "desert_berries", null);
            Tile("tartarus_high", "infernal_high", biome, TileRank.High, "#d57d4f", "desert_berries", null);
            ModernLocalization.Add(TartarusBiomeId, "Tartarus");
        }

        private static void RegisterGlitch()
        {
            if (AssetManager.biome_library.get(GlitchBiomeId) != null) return;
            Vegetation("Glitch_tree", "jungle_tree", false, 1, 0.1f, false, Res("wood", 6, "evil_beets", 5));
            Vegetation("Glitch_tree_big", "jungle_tree", true, 1, 0.2f, false, Res("wood", 30, "evil_beets", 15));
            Vegetation("Glitch_plant", "jungle_plant", false, 1, 0.1f, false, Res("evil_beets", 2, "bones", 2));
            Vegetation("Glitch_tomb", "jungle_plant", false, 1, 0.1f, false, Res("evil_beets", 2, "stone", 2));
            Vegetation("Glitch_candle", "jungle_plant", false, 1, 0.1f, true, Res("evil_beets", 1));

            BiomeAsset biome = Biome(GlitchBiomeId, "Glitch_low", "Glitch_high", GlitchGeneratorAmount);
            biome.addTree("Glitch_tree", 2);
            biome.addPlant("Glitch_plant", 4);
            biome.addTree("Glitch_tree_big", 1);
            biome.addTree("Glitch_candle", 2);
            biome.addPlant("Glitch_tomb", 4);
            biome.addUnit("glitchspectre", 2);
            biome.addUnit("glitchdrake", 1);
            biome.addUnit("glitchtarantula", 2);
            biome.addMineral("mineral_bones", 20);
            biome.addMineral("mineral_adamantine", 20);
            AssetManager.biome_library.add(biome);

            Tile("Glitch_low", "infernal_low", biome, TileRank.Low, "#898672", "evil_beets", SpawnGlitchCreature);
            Tile("Glitch_high", "infernal_high", biome, TileRank.High, "#343434", "evil_beets", SpawnGlitchCreature);
            ModernLocalization.Add(GlitchBiomeId, "Glitch");
        }

        private static BiomeAsset Biome(string id, string low, string high, int generatorAmount)
        {
            BiomeAsset biome = new BiomeAsset
            {
                id = id,
                localized_key = id,
                tile_low = low,
                tile_high = high,
                grow_strength = 20,
                spread_biome = true,
                generator_pot_amount = generatorAmount,
                grow_vegetation_auto = true,
                grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
                grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
                grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants
            };
            return biome;
        }

        private static string[] Res(params object[] pairs)
        {
            string[] result = new string[pairs.Length];
            for (int i = 0; i < pairs.Length; i++) result[i] = pairs[i].ToString();
            return result;
        }

        private static void Vegetation(string id, string template, bool burnable, int limitPerZone, float lightSize, bool light, string[] resources)
        {
            if (AssetManager.buildings.get(id) != null) return;
            BuildingAsset building = AssetManager.buildings.clone(id, template);
            building.id = id;
            building.sprite_path = "buildings/" + id;
            building.main_path = building.sprite_path;
            building.setAtlasID("buildings", "buildings");
            building.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
            building.affected_by_drought = false;
            building.burnable = burnable;
            building.draw_light_area = light;
            building.draw_light_size = lightSize;
            building.limit_per_zone = limitPerZone;
            building.spread_ids = new[] { id };
            // Grows only where its biome spawns it (Infernal-tagged tiles).
            building.biome_tags_growth = new HashSet<BiomeTag> { BiomeTag.Infernal };
            building.has_biome_tags = true;
            building.setShadow(0.5f, 0.03f, 0.12f);
            building.resources_given = null;
            for (int i = 0; i + 1 < resources.Length; i += 2)
                building.addResource(resources[i], int.Parse(resources[i + 1]));
            building.loadBuildingSprites();
            ActorsAndBuildingsRegistry.EnsureBuildingRenderSprites(building);
        }

        private static void Tile(string id, string template, BiomeAsset biome, TileRank rank, string color, string food, WorldAction deathAction)
        {
            if (AssetManager.top_tiles.get(id) != null) return;
            TopTileType tile = AssetManager.top_tiles.clone(id, template);
            tile.id = id;
            tile.color_hex = color;
            tile.color = Toolbox.makeColor(color, -1f);
            tile.setBiome(biome.id);
            tile.rank_type = rank;
            tile.setDrawLayer(rank == TileRank.Low ? TileZIndexes.infernal_low : TileZIndexes.infernal_high, null);
            tile.food_resource = food;
            tile.liquid = false;
            tile.ground = true;
            tile.is_biome = true;
            tile.can_be_biome = true;
            tile.biome_build_check = true;
            tile.only_allowed_to_build_with_tag = string.Empty;
            tile.hold_lava = false;
            tile.can_be_frozen = true;
            tile.burnable = false;
            tile.walk_multiplier = 1f;
            tile.layer_type = TileLayerType.Ground;
            tile.step_action_chance = 1f;
            tile.unit_death_action = deathAction;
            tile.biome_asset = biome;
            AssetManager.top_tiles.loadSpritesForTile(tile);
        }

        // Glitch tiles turn units that die on them into glitch creatures.
        private static bool SpawnGlitchCreature(BaseSimObject target, WorldTile tile = null)
        {
            if (target == null || !target.isActor()) return false;
            Actor dead = target.a;
            if (dead == null || dead.asset == null || !dead.asset.can_turn_into_tumor) return false;
            string id = dead.asset.has_soul ? "glitchspectre" : (dead.asset.default_animal ? "glitchtarantula" : "glitchspectre");
            WorldTile spawnTile = tile ?? dead.current_tile;
            if (spawnTile == null) return false;
            Actor creature = World.world.units.createNewUnit(id, spawnTile);
            if (creature == null) return false;
            creature.removeTrait("blessed");
            ActorTool.copyUnitToOtherUnit(dead, creature);
            EffectsLibrary.spawn("fx_spawn", spawnTile);
            return true;
        }

        #region fog

        private static void RegisterFog()
        {
            if (AssetManager.effects_library.get(FogEffectId) == null)
            {
                AssetManager.effects_library.add(new EffectAsset
                {
                    id = FogEffectId,
                    use_basic_prefab = true,
                    sprite_path = "effects/fogjungle",
                    draw_light_area = false,
                    draw_light_size = 0.5f,
                    show_on_mini_map = false,
                    limit = 30,
                    sorting_layer_id = "EffectsTop",
                    spawn_action = SpawnFogOnTile
                });
            }
            if (AssetManager.world_behaviours.get("fogjungle1") == null)
            {
                WorldBehaviourAsset waves = new WorldBehaviourAsset
                {
                    id = "fogjungle1",
                    enabled = true,
                    enabled_on_minimap = false,
                    interval = 0.7f,
                    interval_random = 0.7f,
                    action = StartFogWaves
                };
                waves.manager = new WorldBehaviour(waves);
                AssetManager.world_behaviours.add(waves);
            }
        }

        private static BaseEffect SpawnFogOnTile(BaseEffect effect, WorldTile tile, string param1, string param2, float floatParam, Actor actor)
        {
            effect.spawnOnTile(tile);
            effect.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            return effect;
        }

        private static void StartFogWaves()
        {
            for (int i = 0; i < 5; i++) SpawnFogWave();
        }

        private static void SpawnFogWave()
        {
            if (World.world == null || World.world.zone_camera == null || World.world.stack_effects == null) return;
            if (!World.world.stack_effects.dictionary.ContainsKey(FogEffectId)) World.world.stack_effects.checkInit();
            List<TileZone> zones = World.world.zone_camera.getVisibleZones();
            if (zones == null || zones.Count == 0) return;
            TileZone zone = zones[Random.Range(0, zones.Count)];
            if (zone == null || zone.tiles == null || zone.tiles.Length == 0) return;
            WorldTile tile = zone.tiles[Random.Range(0, zone.tiles.Length)];
            if (tile != null && tile.Type != null && tile.Type.biome_id == AlienJungleRegistry.BiomeId)
                EffectsLibrary.spawn(FogEffectId, tile);
        }

        #endregion
    }
}
