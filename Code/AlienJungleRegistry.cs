using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Registers M2's Alien Jungle as an ordinary world biome.  None of the
    /// old planet wrappers, map-template switches, or galaxy state are used.
    /// </summary>
    internal static class AlienJungleRegistry
    {
        internal const string BiomeId = "biome_AlienJungle";
        internal const string LowTileId = "AlienJungle_low";
        internal const string HighTileId = "AlienJungle_high";
        internal const string TreeId = "AlienJungle_tree";
        internal const string PlantId = "AlienJungle_plant";

        internal static void Register()
        {
            RegisterVegetation();

            BiomeAsset biome = new BiomeAsset
            {
                id = BiomeId,
                localized_key = "biome_AlienJungle",
                tile_low = LowTileId,
                tile_high = HighTileId,
                grow_strength = 20,
                spread_biome = true,
                generator_pot_amount = 80,
                grow_vegetation_auto = true,
                grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
                grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
                grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants
            };

            biome.addTree(TreeId, 9);
            biome.addPlant(PlantId, 9);
            biome.addUnit("alienwisp", 5);
            biome.addUnit("pantherax", 2);
            biome.addUnit("pterax", 5);
            biome.addUnit("rhinokinglor", 2);
            biome.addUnit("geckoid", 5);
            biome.addUnit("peones", 5);
            biome.addUnit("xenodogo", 5);
            biome.addMineral("mineral_silver", 6);

            AssetManager.biome_library.add(biome);
            AssetManager.biome_library.addBiomeToPool(biome);
            RegisterTile(LowTileId, "infernal_low", biome, TileRank.Low, "#4affc8");
            RegisterTile(HighTileId, "infernal_high", biome, TileRank.High, "#00d695");
            ModernLocalization.Add(BiomeId, "Alien Jungle");
            ModernLocalization.Add(BiomeId + "_description", "A luminous alien ecosystem with native M2 fauna and vegetation.");
        }

        private static void RegisterVegetation()
        {
            BuildingAsset tree = AssetManager.buildings.clone(TreeId, "jungle_tree");
            tree.id = TreeId;
            tree.sprite_path = "buildings/" + TreeId;
            tree.main_path = tree.sprite_path;
            tree.setAtlasID("buildings", "buildings");
            tree.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
            tree.draw_light_area = true;
            tree.draw_light_size = 0.1f;
            tree.limit_per_zone = 20;
            tree.setShadow(0.5f, 0.03f, 0.12f);
            tree.addResource("wood", 15);
            tree.addResource("bananas", 5);
            tree.addResource("mushrooms", 5);
            tree.loadBuildingSprites();
            ActorsAndBuildingsRegistry.EnsureBuildingRenderSprites(tree);

            BuildingAsset plant = AssetManager.buildings.clone(PlantId, "jungle_plant");
            plant.id = PlantId;
            plant.sprite_path = "buildings/" + PlantId;
            plant.main_path = plant.sprite_path;
            plant.setAtlasID("buildings", "buildings");
            plant.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
            plant.draw_light_area = true;
            plant.draw_light_size = 0.1f;
            plant.limit_per_zone = 1;
            plant.setShadow(0.5f, 0.03f, 0.12f);
            plant.addResource("herbs", 1);
            plant.addResource("wheat", 1);
            plant.loadBuildingSprites();
            ActorsAndBuildingsRegistry.EnsureBuildingRenderSprites(plant);
        }

        private static void RegisterTile(string id, string template, BiomeAsset biome, TileRank rank, string color)
        {
            TopTileType tile = AssetManager.top_tiles.clone(id, template);
            tile.id = id;
            tile.color_hex = color;
            tile.color = Toolbox.makeColor(color, -1f);
            tile.setBiome(BiomeId);
            tile.rank_type = rank;
            tile.setDrawLayer(rank == TileRank.Low ? TileZIndexes.infernal_low : TileZIndexes.infernal_high, null);
            tile.food_resource = "bananas";
            tile.liquid = false;
            tile.ground = true;
            // The visual source is infernal terrain, whose build restrictions are
            // intentionally hostile to civilizations.  Alien Jungle is a normal
            // world biome in this rewrite, so explicitly restore the current
            // settlement, construction, and farming flags instead of inheriting
            // infernal's species-tag restriction.
            tile.is_biome = true;
            tile.can_be_biome = true;
            tile.biome_build_check = true;
            tile.only_allowed_to_build_with_tag = string.Empty;
            tile.soil = true;
            tile.life = true;
            tile.grass = true;
            tile.can_build_on = true;
            tile.can_be_farm = true;
            tile.considered_empty_tile = false;
            tile.hold_lava = false;
            tile.can_be_frozen = true;
            tile.burnable = true;
            tile.walk_multiplier = 1f;
            tile.layer_type = TileLayerType.Ground;
            tile.biome_asset = biome;
            AssetManager.top_tiles.loadSpritesForTile(tile);
        }
    }
}
