using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// Port of M2's PlanetGenerator. Planet biomes, tiles and vegetation are registered once at
    /// startup (so saved planets load after a restart); landing only swaps the generator pool,
    /// template and size for that one generation, then puts the player's settings back.
    internal static class M2PlanetGenerator
    {
        internal const string TemplateId = "ballsass";

        private static bool pending;
        private static bool pendingSize;
        private static int mapSizeX = 13;
        private static int mapSizeY = 13;
        private static string pendingPlanetName;
        private static List<BiomeAsset> savedPool;
        private static string savedTemplate;

        internal static void Register()
        {
            Safe("ballsass template", RegisterTemplate);
            Safe("tartarus", RegisterTartarus);
            Safe("glitch", RegisterGlitch);
            Safe("chess", RegisterChess);
            Safe("planet biomes", RegisterPlanetBiomes);
        }

        private static readonly string[] FrogSheep = { "frog", "sheep" };

        // Planet biomes from M2 (Creatures.cs / Space/PlanetGenerator.cs). M2 pointed them at the
        // vanilla tiles, whose own biome then decided what grew there. In 0.51.2 a tile belongs to
        // one biome, so every planet biome gets its own copy of those tiles and of the vanilla
        // biome's vegetation; units, minerals and grow strength are M2's.
        private static void RegisterPlanetBiomes()
        {
            // Creatures.cs reloaded the vanilla cybertile art from M2's own tiles folder.
            // Do it before the planet copies are cloned so they share the M2 art.
            foreach (string id in new[] { "cybertile_low", "cybertile_high" })
            {
                TopTileType cyber = AssetManager.top_tiles.get(id);
                if (cyber == null) continue;
                // The vanilla art is already cached under this path.
                SpriteTextureLoader._cached_sprite_list.Remove("tiles/" + id);
                AssetManager.top_tiles.loadSpritesForTile(cyber);
            }
            PlanetBiome("biome_IcePlanet", "IcePlanet", "biome_permafrost", "permafrost_low", "permafrost_high", 0, FrogSheep, null, null);
            PlanetBiome("biome_IcePlanet1", "IcePlanet1", "biome_permafrost", "frozen_low", "frozen_high", 0, FrogSheep, null, null);
            PlanetBiome("biome_RobotPlanet", "RobotPlanet", "biome_cybertile", "cybertile_low", "cybertile_high", 0, FrogSheep, null, null);
            PlanetBiome("biome_LavaPlanet", "LavaPlanet", "biome_infernal", "infernal_low", "infernal_high", 0, new[] { "Terlanius", "Terlanius", "Terlanius", "Terlanius", "Terlanius", "Terlanius", "Terlanius", "Terlanius" }, null, null);
            PlanetBiome("biome_CorruptedPlanet", "CorruptedPlanet", "biome_corrupted", "corrupted_low", "corrupted_high", 0, new string[0], null, null);
            PlanetBiome("biome_LemonPlanet", "LemonPlanet", "biome_lemon", "lemon_low", "lemon_high", 0, new[] { "Terlanius" }, null, null);
            PlanetBiome("biome_MushroomPlanet", "MushroomPlanet", "biome_mushroom", "mushroom_low", "mushroom_high", 0, new[] { "Terlanius" }, null, null);
            PlanetBiome("biome_WastelandPlanet", "WastelandPlanet", "biome_wasteland", "wasteland_low", "wasteland_high", 0, new[] { "Terlanius" }, null, null);
            PlanetBiome("biome_CrystalPlanet", "CrystalPlanet", "biome_crystal", "crystal_low", "crystal_high", 0, FrogSheep, null, null);
            PlanetBiome("biome_SwampPlanet", "SwampPlanet", "biome_swamp", "swamp_low", "swamp_high", 0, FrogSheep, null, null);
            PlanetBiome("biome_Jungle1Planet", "Jungle1Planet", "biome_jungle", "jungle_low", "jungle_high", 20, new[] { "frog", "sheep", "Terlanius" }, null, null);
            PlanetBiome("biome_Jungle2Planet", "Jungle2Planet", "biome_enchanted", "enchanted_low", "enchanted_high", 0, new[] { "frog", "sheep", "Terlanius" }, null, null);
            PlanetBiome("biome_Jungle3Planet", "Jungle3Planet", "biome_swamp", "swamp_low", "swamp_high", 0, new[] { "frog", "sheep", "Terlanius" }, null, null);
            PlanetBiome("biome_GasGiant", "GasGiant", "biome_mushroom", "mushroom_low", "mushroom_high", 0, FrogSheep, "#3b3aad", "#000c8d");
            PlanetBiome("biome_Space", "Space", "biome_mushroom", "mushroom_low", "mushroom_high", 0, FrogSheep, "#3b3aad", "#000c8d");
            foreach (string id in new[] { "GasGiant_low", "GasGiant_high", "Space_low", "Space_high" })
            {
                TopTileType tile = AssetManager.top_tiles.get(id);
                if (tile == null) continue;
                tile.step_action_chance = 1437f;
                tile.fire_chance = 0.02f;
                tile.food_resource = "mushrooms";
            }
        }

        private static void PlanetBiome(string id, string tilePrefix, string sourceBiome, string lowTemplate, string highTemplate, int normalMapAmount, string[] units, string lowColor, string highColor)
        {
            if (AssetManager.biome_library.get(id) != null) return;
            if (AssetManager.biome_library.get(sourceBiome) == null || AssetManager.top_tiles.get(lowTemplate) == null || AssetManager.top_tiles.get(highTemplate) == null)
            {
                ModernBoxDiagnostics.Warn("Planet biome " + id + " skipped: missing " + sourceBiome + "/" + lowTemplate);
                return;
            }
            BiomeAsset biome = AssetManager.biome_library.clone(id, sourceBiome);
            biome.id = id;
            biome.localized_key = id;
            biome.tile_low = tilePrefix + "_low";
            biome.tile_high = tilePrefix + "_high";
            biome.grow_strength = 10;
            biome.spread_biome = true;
            biome.generator_pot_amount = normalMapAmount;
            biome.grow_vegetation_auto = true;
            if (biome.grow_type_selector_minerals == null) biome.grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral;
            if (biome.grow_type_selector_trees == null) biome.grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees;
            if (biome.grow_type_selector_plants == null) biome.grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants;
            biome.pot_units_spawn = null;
            biome.pot_minerals_spawn = null;
            foreach (string unit in units) AddUnit(biome, unit, 1);
            biome.addMineral("mineral_stone", 5);
            biome.addMineral("mineral_metals", 3);

            CloneTile(biome.tile_low, lowTemplate, biome, TileRank.Low, lowColor);
            CloneTile(biome.tile_high, highTemplate, biome, TileRank.High, highColor);
            // The original pooled this biome with the plain vanilla tiles, so normal worlds only got
            // extra ordinary terrain. Pool the source biome to keep planet creatures off normal worlds.
            BiomeAsset pooled = AssetManager.biome_library.get(sourceBiome);
            for (int i = 0; i < normalMapAmount; i++) BiomeLibrary.pool_biomes.Add(pooled);
            ModernLocalization.Add(id, tilePrefix);
            ModernLocalization.Add(id + "_description", "A ModernBox planet biome.");
        }

        private static void CloneTile(string id, string template, BiomeAsset biome, TileRank rank, string color)
        {
            if (AssetManager.top_tiles.get(id) != null) return;
            TopTileType source = AssetManager.top_tiles.get(template);
            TopTileType tile = AssetManager.top_tiles.clone(id, template);
            tile.id = id;
            tile.increase_to = source.increase_to;
            tile.decrease_to = source.decrease_to;
            tile.music_assets = source.music_assets;
            tile.has_biome_tags = source.has_biome_tags;
            tile.color = source.color;
            tile.edge_color = source.edge_color;
            if (color != null)
            {
                tile.color_hex = color;
                tile.color = Toolbox.makeColor(color, -1f);
            }
            tile.setBiome(biome.id);
            tile.rank_type = rank;
            tile.is_biome = true;
            tile.can_be_biome = true;
            tile.forever_frozen = false;
            tile.can_be_unfrozen = false;
            tile.biome_asset = biome;
            if (source.sprites != null)
            {
                tile.sprites = new TileSprites();
                foreach (UnityEngine.Tilemaps.Tile variation in source.sprites._tiles)
                    tile.sprites.addVariation(variation.sprite, id);
            }
        }

        private static void Safe(string what, Action action)
        {
            try { action(); }
            catch (Exception ex) { ModernBoxDiagnostics.Warn("Planet content '" + what + "' was not registered: " + ex.Message); }
        }

        private static void RegisterTemplate()
        {
            if (AssetManager.map_gen_templates.get(TemplateId) != null) return;
            MapGenTemplate template = new MapGenTemplate { id = TemplateId };
            template.values.add_center_gradient_land = true;
            template.values.main_perlin_noise_stage = true;
            template.values.perlin_noise_stage_2 = true;
            template.values.perlin_noise_stage_3 = true;
            template.values.add_mountain_edges = true;
            template.values.remove_mountains = false;
            template.allow_edit_low_ground = false;
            template.allow_edit_high_ground = false;
            AssetManager.map_gen_templates.add(template);
            AssetManager.map_gen_templates.default_values[TemplateId] = JsonUtility.FromJson<MapGenValues>(JsonUtility.ToJson(template.values));
            ModernLocalization.Add("template_" + TemplateId, "Planet");
            ModernLocalization.Add("template_" + TemplateId + "_info", "Land with mountain edges, used by ModernBox planets.");
        }

        private static void RegisterTartarus()
        {
            EnsureVegetation("tartarus_desert_bones_big", true, 1);
            EnsureVegetation("tartarus_tar_bones_big", true, 1);
            EnsureVegetation("tartarus_vent", true, 1);
            EnsureVegetation("tartarus_desert_bones", false, 1);
            EnsureVegetation("tartarus_tar_bones", false, 1);
            EnsureVegetation("tartarus_ruins", false, 1);

            BiomeAsset tartarus = EnsureBiome("biome_tartarus", "tartarus_low", "tartarus_high", 20, "Tartarus");
            if (tartarus.pot_trees_spawn == null || tartarus.pot_trees_spawn.Count == 0)
            {
                AddTree(tartarus, "tartarus_desert_bones_big", 1);
                AddTree(tartarus, "tartarus_tar_bones_big", 1);
                AddTree(tartarus, "tartarus_vent", 1);
                AddPlant(tartarus, "tartarus_desert_bones", 2);
                AddPlant(tartarus, "tartarus_tar_bones", 1);
                AddPlant(tartarus, "tartarus_ruins", 1);
                AddUnit(tartarus, "scandid", 4);
                AddUnit(tartarus, "Duneworm", 1);
                tartarus.addMineral("mineral_bones", 20);
                tartarus.addMineral("mineral_stone", 20);
                tartarus.addMineral("mineral_metals", 5);
            }
            EnsureTile("tartarus_low", "infernal_low", tartarus, TileRank.Low, "#272727", "desert_berries", null);
            EnsureTile("tartarus_high", "infernal_high", tartarus, TileRank.High, "#d57d4f", "desert_berries", null);
        }

        private static void RegisterGlitch()
        {
            EnsureVegetation("Glitch_tree", true, 1);
            EnsureVegetation("Glitch_tree_big", true, 1);
            EnsureVegetation("Glitch_candle", true, 1);
            EnsureVegetation("Glitch_plant", false, 1);
            EnsureVegetation("Glitch_tomb", false, 1);

            BiomeAsset glitch = EnsureBiome("biome_Glitch", "Glitch_low", "Glitch_high", 20, "Glitch");
            if (glitch.pot_trees_spawn == null || glitch.pot_trees_spawn.Count == 0)
            {
                AddTree(glitch, "Glitch_tree", 2);
                AddPlant(glitch, "Glitch_plant", 4);
                AddTree(glitch, "Glitch_tree_big", 1);
                AddTree(glitch, "Glitch_candle", 2);
                AddPlant(glitch, "Glitch_tomb", 4);
                AddUnit(glitch, "glitchspectre", 2);
                AddUnit(glitch, "glitchdrake", 1);
                AddUnit(glitch, "glitchtarantula", 2);
                glitch.addMineral("mineral_bones", 20);
                glitch.addMineral("mineral_adamantine", 20);
            }
            EnsureTile("Glitch_low", "infernal_low", glitch, TileRank.Low, "#898672", "evil_beets", SpawnGlitchCreature);
            EnsureTile("Glitch_high", "infernal_high", glitch, TileRank.High, "#343434", "evil_beets", SpawnGlitchCreature);
        }

        private static void RegisterChess()
        {
            EnsureVegetation("Chess_tree", true, 1);
            EnsureVegetation("Chess_tree_2", true, 1);
            EnsureVegetation("Chess_tree_3", true, 1);

            BiomeAsset chess = EnsureBiome("biome_Chess", "Chess_low", "Chess_high", 20, "Chess");
            if (chess.pot_trees_spawn == null || chess.pot_trees_spawn.Count == 0)
            {
                AddTree(chess, "Chess_tree", 20);
                AddTree(chess, "Chess_tree_2", 20);
                AddTree(chess, "Chess_tree_3", 20);
            }
            EnsureTile("Chess_low", "infernal_low", chess, TileRank.Low, "#898672", "evil_beets", null);
            EnsureTile("Chess_high", "infernal_high", chess, TileRank.High, "#808080", "evil_beets", null);
        }

        private static BiomeAsset EnsureBiome(string id, string low, string high, int growStrength, string displayName)
        {
            BiomeAsset biome = AssetManager.biome_library.get(id);
            if (biome != null) return biome;
            biome = new BiomeAsset
            {
                id = id,
                localized_key = id,
                tile_low = low,
                tile_high = high,
                grow_strength = growStrength,
                spread_biome = true,
                generator_pot_amount = 0,
                grow_vegetation_auto = true,
                grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
                grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
                grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants
            };
            AssetManager.biome_library.add(biome);
            ModernLocalization.Add(id, displayName);
            ModernLocalization.Add(id + "_description", "A ModernBox planet biome.");
            return biome;
        }

        private static void AddTree(BiomeAsset biome, string id, int rate)
        {
            if (AssetManager.buildings.get(id) != null) biome.addTree(id, rate);
        }

        private static void AddPlant(BiomeAsset biome, string id, int rate)
        {
            if (AssetManager.buildings.get(id) != null) biome.addPlant(id, rate);
        }

        private static void AddUnit(BiomeAsset biome, string id, int rate)
        {
            if (AssetManager.actor_library.get(id) != null) biome.addUnit(id, rate);
        }

        private static void EnsureTile(string id, string template, BiomeAsset biome, TileRank rank, string color, string food, WorldAction deathAction)
        {
            TopTileType tile = AssetManager.top_tiles.get(id);
            if (tile == null)
            {
                tile = AssetManager.top_tiles.clone(id, template);
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
                tile.soil = true;
                tile.life = true;
                tile.can_build_on = true;
                tile.can_be_farm = true;
                tile.hold_lava = false;
                tile.can_be_frozen = true;
                tile.burnable = false;
                tile.walk_multiplier = 1f;
                tile.step_action_chance = 1f;
                tile.layer_type = TileLayerType.Ground;
                AssetManager.top_tiles.loadSpritesForTile(tile);
            }
            tile.biome_asset = biome;
            if (deathAction != null) tile.unit_death_action = deathAction;
        }

        private static void EnsureVegetation(string id, bool tree, int limitPerZone)
        {
            if (AssetManager.buildings.get(id) != null) return;
            try
            {
                BuildingAsset building = AssetManager.buildings.clone(id, tree ? "jungle_tree" : "jungle_plant");
                building.id = id;
                building.sprite_path = "buildings/" + id;
                building.main_path = building.sprite_path;
                building.setAtlasID("buildings", "buildings");
                building.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
                building.limit_per_zone = limitPerZone;
                building.burnable = false;
                building.spread_ids = new[] { id };
                building.setShadow(0.5f, 0.03f, 0.12f);
                building.loadBuildingSprites();
                ActorsAndBuildingsRegistry.EnsureBuildingRenderSprites(building);
                ModernLocalization.Add(id, id.Replace('_', ' '));
            }
            catch (Exception ex)
            {
                ModernBoxDiagnostics.Warn("Planet vegetation " + id + " skipped: " + ex.Message);
            }
        }

        private static bool SpawnGlitchCreature(BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || !pTarget.isActor() || pTile == null) return false;
            Actor actor = pTarget.a;
            if (actor == null || actor.asset == null || !actor.asset.can_turn_into_tumor) return false;
            string newUnitId = actor.isSapient() ? "glitchspectre" : "glitchtarantula";
            if (AssetManager.actor_library.get(newUnitId) == null) return false;
            Actor spawned = World.world.units.createNewUnit(newUnitId, pTile);
            if (spawned == null) return false;
            spawned.removeTrait("blessed");
            ai.ActorTool.copyUnitToOtherUnit(actor, spawned, false);
            EffectsLibrary.spawnAt("evilspawn", spawned.current_tile.posV3, 0.1f);
            return true;
        }

        internal static bool PreparePlanet(string planetName, string type, string planetSize, bool hasFauna)
        {
            string[] sizeParts = (planetSize ?? string.Empty).Split('x');
            if (sizeParts.Length != 2 || !int.TryParse(sizeParts[0].Trim(), out int sizeX) || !int.TryParse(sizeParts[1].Trim(), out int sizeY))
            {
                Debug.LogError($"[ModernBox Space] Invalid planet size: {planetSize}");
                return false;
            }

            if (!pending)
            {
                savedPool = new List<BiomeAsset>(BiomeLibrary.pool_biomes);
                savedTemplate = Config.current_map_template;
            }
            pending = true;
            pendingPlanetName = planetName;
            mapSizeX = ScaleSize(sizeX);
            mapSizeY = ScaleSize(sizeY);
            pendingSize = true;

            ChoosePlanetBiomes(type, hasFauna);
            if (BiomeLibrary.pool_biomes.Count == 0) BiomeLibrary.pool_biomes.AddRange(savedPool);
            if (AssetManager.map_gen_templates.get(Config.current_map_template) == null) Config.current_map_template = "boring_plains";
            return true;
        }

        private static int ScaleSize(int size)
        {
            // SpaceManager.GeneratePlanet used the star's 1-24 size as the zone count directly.
            // 1 is raised to 2, the smallest size 0.51.2 itself makes.
            return Mathf.Clamp(size, 2, 24);
        }

        private static void UseBiome(string id, string fallback = null)
        {
            BiomeAsset biome = AssetManager.biome_library.get(id) ?? (fallback != null ? AssetManager.biome_library.get(fallback) : null);
            if (biome != null) for (int i = 0; i < 80; i++) BiomeLibrary.pool_biomes.Add(biome);
        }

        private static void ChoosePlanetBiomes(string type, bool hasFauna)
        {
            Config.current_map_template = TemplateId;
            BiomeLibrary.pool_biomes.Clear();

            switch ((type ?? string.Empty).ToLower())
            {
                case "desert world":
                    Config.current_map_template = "boring_plains";
                    UseBiome("biome_tartarus", "biome_desert");
                    break;
                case "icy":
                    SetRandomMapTemplate();
                    UseBiome("biome_IcePlanet", "biome_permafrost");
                    UseBiome("biome_IcePlanet1");
                    break;
                case "oceanic":
                    Config.current_map_template = "empty";
                    break;
                case "mechanical world":
                    Config.current_map_template = "boring_plains";
                    UseBiome("biome_RobotPlanet", "biome_cybertile");
                    break;
                case "lava world":
                    Config.current_map_template = "boring_plains";
                    UseBiome("biome_LavaPlanet", "biome_infernal");
                    break;
                case "corrupted world":
                    Config.current_map_template = "boring_plains";
                    UseBiome("biome_Glitch", "biome_corrupted");
                    break;
                case "chess world":
                    Config.current_map_template = "boring_plains";
                    UseBiome("biome_Chess");
                    break;
                case "lemon world":
                    SetRandomMapTemplate();
                    UseBiome("biome_LemonPlanet", "biome_lemon");
                    break;
                case "mushroom world":
                    SetRandomMapTemplate();
                    UseBiome("biome_MushroomPlanet", "biome_mushroom");
                    break;
                case "wasteland world":
                    Config.current_map_template = "boring_plains";
                    UseBiome("biome_WastelandPlanet", "biome_wasteland");
                    break;
                case "crystal world":
                    SetRandomMapTemplate();
                    UseBiome("biome_CrystalPlanet", "biome_crystal");
                    break;
                case "swamp world":
                    SetRandomMapTemplate();
                    UseBiome("biome_SwampPlanet", "biome_swamp");
                    break;
                case "jungle world":
                    Config.current_map_template = "boring_plains";
                    UseBiome(AlienJungleRegistry.BiomeId, "biome_jungle");
                    break;
                case "gas giant":
                    SetRandomMapTemplate();
                    UseBiome("biome_GasGiant", "biome_enchanted");
                    break;
                default:
                    UseBiome("biome_Space", "biome_mushroom");
                    break;
            }
        }

        private static void SetRandomMapTemplate()
        {
            string[] options = { "boring_plains", TemplateId };
            Config.current_map_template = options[UnityEngine.Random.Range(0, options.Length)];
        }

        internal static bool TakeSizeOverride(ref int width, ref int height, bool consume)
        {
            if (!pendingSize) return false;
            width = mapSizeX;
            height = mapSizeY;
            if (consume) pendingSize = false;
            return true;
        }

        internal static void FinishGeneration()
        {
            if (!pending) return;
            pending = false;
            pendingSize = false;
            if (savedPool != null)
            {
                BiomeLibrary.pool_biomes.Clear();
                BiomeLibrary.pool_biomes.AddRange(savedPool);
            }
            if (!string.IsNullOrEmpty(savedTemplate)) Config.current_map_template = savedTemplate;
            savedPool = null;
            if (!string.IsNullOrEmpty(pendingPlanetName) && World.world != null && World.world.map_stats != null)
                World.world.map_stats.name = pendingPlanetName;
        }
    }

    [HarmonyPatch(typeof(MapBox), nameof(MapBox.addClearWorld))]
    internal static class M2PlanetClearWorldPatch
    {
        private static void Prefix(ref int pNextWidth, ref int pNextHeight)
        {
            M2PlanetGenerator.TakeSizeOverride(ref pNextWidth, ref pNextHeight, false);
        }
    }

    [HarmonyPatch(typeof(MapBox), nameof(MapBox.setMapSize))]
    internal static class M2PlanetMapSizePatch
    {
        private static void Prefix(ref int pWidth, ref int pHeight)
        {
            M2PlanetGenerator.TakeSizeOverride(ref pWidth, ref pHeight, true);
        }
    }

    [HarmonyPatch(typeof(MapBox), nameof(MapBox.finishMakingWorld))]
    internal static class M2PlanetFinishPatch
    {
        private static void Postfix()
        {
            M2PlanetGenerator.FinishGeneration();
        }
    }

    [HarmonyPatch(typeof(ModernBoxRuntime), "Update")]
    internal static class M2SpacePausesRuntimePatch
    {
        private static bool Prefix()
        {
            return !M2SpaceManager.IsSpaceEnabled;
        }
    }
}
