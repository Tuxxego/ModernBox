using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;
using HarmonyLib;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections;
using System.IO.Compression;
using ai;
using UnityEngine.Tilemaps;

namespace ModernBox
{
    class PlanetGenerator : MonoBehaviour
    {

		public static int mapSizeX = 13;
		public static int mapSizeY = 13;
        private MapGenTemplateLibrary mapGenTemplateLibrary; 

		public void Awake()
		{

		}

		public static bool setMapSize_Prefix(ref int pWidth, ref int pHeight)
		{

				pWidth = mapSizeX;
				pHeight = mapSizeY;

			return true;
		}

        private static void ClearBiomes()
        {
            BiomeLibrary.pool_biomes.Clear();
        }

        private static void LogAndClearBiomes()
        {

            Debug.Log("Current Biome Pool:");
            foreach (var biome in BiomeLibrary.pool_biomes)
            {
                Debug.Log(biome.ToString()); 
            }

            ClearBiomes();

            Debug.Log("Biome Pool After Clearing:");
            if (BiomeLibrary.pool_biomes.Count == 0)
            {
                Debug.Log("Biome pool is now empty.");
            }
            else
            {
                foreach (var biome in BiomeLibrary.pool_biomes)
                {
                    Debug.Log(biome.ToString()); 
                }
            }
        }

        public static void ChoosePlanetBiomes(string type, bool hasFauna)
        {




			Harmony harmony = new Harmony("tuxxego.worldbox.spaceindabox");

			MethodInfo original = AccessTools.Method(typeof(MapBox), "setMapSize");
			MethodInfo patch = AccessTools.Method(typeof(PlanetGenerator), "setMapSize_Prefix");
			harmony.Patch(original, new HarmonyMethod(patch));
			Debug.Log("Pre patch: MapBox.setMapSize");

				Config.current_map_template = "ballsass";

            LogAndClearBiomes();

            switch (type.ToLower())
            {
                case "desert world":
				Config.current_map_template = "boring_plains";
                        AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_desert"));
			break;

    case "icy":
            Config.current_map_template = "anthill";


BiomeAsset MoonSurface = new BiomeAsset();
MoonSurface.id = "MoonSurface";
MoonSurface.tile_low = "MoonSurface_low";
MoonSurface.tile_high = "MoonSurface_high";
MoonSurface.localized_key = "Moon";
MoonSurface.spread_biome = true;
MoonSurface.spread_by_drops_water = true;
MoonSurface.generator_pot_amount = 20;
MoonSurface.grow_strength = 20;
MoonSurface.grow_vegetation_auto = true;
MoonSurface.grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral;
MoonSurface.grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees;
MoonSurface.grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants;
MoonSurface.grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes;
MoonSurface.subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("moonarian", "spacer", "tuxxian");
MoonSurface.addActorTrait("genius");
MoonSurface.addCultureTrait("fast_learners");
MoonSurface.addLanguageTrait("melodic");
MoonSurface.addLanguageTrait("elegant_words");
MoonSurface.addUnit("angle");
MoonSurface.addSapientUnit("alien");
MoonSurface.addMineral("mineral_stone", 10);
MoonSurface.addMineral("mineral_metals", 5);
MoonSurface.addTree("lemon_tree");
MoonSurface.addPlant("mushroom_red");
MoonSurface.addPlant("mushroom_green");
MoonSurface.addPlant("mushroom_teal");
MoonSurface.addPlant("mushroom_white");
MoonSurface.addPlant("mushroom_yellow");
MoonSurface.addPlant("green_herb", 5);
MoonSurface.addPlant("flower", 5);
MoonSurface.addBush("fruit_bush");
AssetManager.biome_library.add(MoonSurface);
AssetManager.biome_library.addBiomeToPool(MoonSurface);
LocalizedTextManager.add("Moon", "Moon Biome");

TopTileType MoonSurface_low = AssetManager.top_tiles.clone("MoonSurface_low", "grass_low");
MoonSurface_low.id = "MoonSurface_low";
		MoonSurface_low.color_hex = "#868686";
        MoonSurface_low.color = Toolbox.makeColor("#969696", -1f);
		MoonSurface_low.setBiome("MoonSurface");
        MoonSurface_low.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Rocklands);
		MoonSurface_low.setDrawLayer(TileZIndexes.enchanted_low);
		MoonSurface_low.step_action = ActionLibrary.restoreMana;
        MoonSurface_low.biome_asset = MoonSurface;
        MoonSurface_low.rank_type = TileRank.Low;
            MoonSurface_low.setDrawLayer(TileZIndexes.infernal_low, null);
            MoonSurface_low.liquid = false;
            MoonSurface_low.ground = true;
            MoonSurface_low.layer_type = TileLayerType.Ground;
        MoonSurface_low.walk_multiplier = 0.5f;
        MoonSurface_low.can_be_set_on_fire_by_burning_feet = false;
		MoonSurface_low.burnable = false;
		MoonSurface_low.hold_lava = true;
		MoonSurface_low.can_be_frozen = false;
		MoonSurface_low.food_resource = "crystal_salt";
AssetManager.top_tiles.add(MoonSurface_low);
AssetManager.top_tiles.loadSpritesForTile(MoonSurface_low);


TopTileType MoonSurface_high = AssetManager.top_tiles.clone("MoonSurface_high", "grass_high");
MoonSurface_high.id = "MoonSurface_high";
		MoonSurface_high.color_hex = "#bdbdbd";
        MoonSurface_high.color = Toolbox.makeColor("#bdbdbd", -1f);
		MoonSurface_high.setBiome("MoonSurface");
        MoonSurface_high.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Rocklands);
		MoonSurface_high.setDrawLayer(TileZIndexes.enchanted_low);
        MoonSurface_high.biome_asset = MoonSurface;
		MoonSurface_high.step_action = ActionLibrary.restoreMana;
                MoonSurface_high.rank_type = TileRank.High;
            MoonSurface_high.setDrawLayer(TileZIndexes.infernal_low, null);
            MoonSurface_high.liquid = false;
            MoonSurface_high.ground = true;
            MoonSurface_high.layer_type = TileLayerType.Ground;
        MoonSurface_high.walk_multiplier = 0.5f;
        MoonSurface_high.can_be_set_on_fire_by_burning_feet = false;
		MoonSurface_high.burnable = false;
		MoonSurface_high.hold_lava = true;
		MoonSurface_high.can_be_frozen = false;
		MoonSurface_high.food_resource = "crystal_salt";
AssetManager.top_tiles.add(MoonSurface_high);
AssetManager.top_tiles.loadSpritesForTile(MoonSurface_high);





break;			

    case "oceanic":

				Config.current_map_template = "empty";
break;
    case "mechanical world":
				Config.current_map_template = "boring_plains";
                AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_cybertile"));
			break;			

    case "lava world":
				Config.current_map_template = "boring_plains";
                AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_infernal"));

			break;
    case "corrupted world":
				Config.current_map_template = "boring_plains";
                
			BiomeAsset Glitch = new BiomeAsset();
            Glitch.id = "biome_Glitch";
			Glitch.tile_low = "Glitch_low";
			Glitch.tile_high = "Glitch_high";
		//	Glitch.force_unit_skin_set = "infernal";
			Glitch.grow_strength = 20;
			Glitch.spread_biome = true;
			Glitch.generator_pot_amount = 80;
            Glitch.grow_vegetation_auto = true;
			Glitch.grow_type_selector_minerals = new GrowTypeSelector(TileActionLibrary.getGrowTypeRandomMineral);
			Glitch.grow_type_selector_trees = new GrowTypeSelector(TileActionLibrary.getGrowTypeRandomTrees);
			Glitch.grow_type_selector_plants = new GrowTypeSelector(TileActionLibrary.getGrowTypeRandomPlants);
	//		Glitch.addTree("Glitch_tree", 2);
	//		Glitch.addPlant("Glitch_plant", 4);
	//		Glitch.addTree("Glitch_tree_big", 1);
     //       Glitch.addTree("Glitch_candle", 2);
	//		Glitch.addPlant("Glitch_tomb", 4);
     //       Glitch.addUnit("glitchspectre", 2);
     //       Glitch.addUnit("glitchdrake", 1);
     //       Glitch.addUnit("glitchtarantula", 2);
		//	Glitch.addMineral(SB.mineral_bones, 20);
		//	Glitch.addMineral(SB.mineral_adamantine, 20);
            AssetManager.biome_library.add(Glitch);
            AssetManager.biome_library.addBiomeToPool(Glitch);

            TopTileType Glitch_low = AssetManager.top_tiles.clone("Glitch_low", "infernal_low");
            Glitch_low.color = Toolbox.makeColor("#898672", -1f);
            Glitch_low.setBiome("biome_Glitch");
			Glitch_low.rank_type = TileRank.Low;
            Glitch_low.setDrawLayer(TileZIndexes.infernal_low, null);
       //     Glitch_low.food_resource = SR.evil_beets;
            Glitch_low.liquid = false;
            Glitch_low.ground = true;
           // Glitch_low.unitDeathAction = new WorldAction(spawnGlitchCreature);
		 //   Glitch_low.stepActionChance = 1f;
            Glitch_low.hold_lava = false;
            Glitch_low.can_be_frozen = true;
            Glitch_low.burnable = false;
         //   Glitch_low.walkMod = 1f;
            Glitch_low.layer_type = TileLayerType.Ground;
			Glitch_low.biome_asset = Glitch;
            AssetManager.top_tiles.add(Glitch_low);
            AssetManager.top_tiles.loadSpritesForTile(Glitch_low);
            AssetManager.top_tiles.add(AssetManager.top_tiles.get("Glitch_low"));
          //  TilemapUtils.SetRendererMode("Glitch_low", "render_z", TilemapRenderer.Mode.Chunk); // etc.

            TopTileType Glitch_high = AssetManager.top_tiles.clone("Glitch_high", "infernal_high");
            Glitch_high.color = Toolbox.makeColor("#343434", -1f);
            Glitch_high.setBiome("biome_Glitch");
			Glitch_high.rank_type = TileRank.High;
            Glitch_high.setDrawLayer(TileZIndexes.infernal_high);
          //  Glitch_high.unitDeathAction = new WorldAction(spawnGlitchCreature);
         //   Glitch_high.stepActionChance = 1f;
        //    Glitch_high.food_resource = SR.evil_beets;
            Glitch_high.liquid = false;
            Glitch_high.hold_lava = false;
            Glitch_high.can_be_frozen = true;
            Glitch_high.burnable = false;
            Glitch_high.layer_type = TileLayerType.Ground;
			Glitch_high.biome_asset = Glitch;
            AssetManager.top_tiles.add(Glitch_high);
            AssetManager.top_tiles.loadSpritesForTile(Glitch_high);
            AssetManager.top_tiles.add(AssetManager.top_tiles.get("Glitch_high"));
       //     TilemapUtils.SetRendererMode("Glitch_high", "render_z", TilemapRenderer.Mode.Individual);

			break;
    case "chess world":
				Config.current_map_template = "boring_plains";


			break;
    case "lemon world":

            SetRandomMapTemplate();
            AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_lemon"));

			break;
    case "mushroom world":

            SetRandomMapTemplate();
            AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_mushroom"));
			break;
    case "wasteland world":
				Config.current_map_template = "boring_plains";


            BiomeAsset WastelandPlanet = new BiomeAsset();
            WastelandPlanet.id = "biome_WastelandPlanet";
			      WastelandPlanet.tile_low = "wasteland_low";
			      WastelandPlanet.tile_high = "wasteland_high";
			      WastelandPlanet.grow_strength = 10;
			      WastelandPlanet.spread_biome = true;
			      WastelandPlanet.generator_pot_amount = 80;
            WastelandPlanet.grow_vegetation_auto = true;
			      WastelandPlanet.grow_type_selector_minerals = new GrowTypeSelector(TileActionLibrary.getGrowTypeRandomMineral);
			      WastelandPlanet.grow_type_selector_trees = new GrowTypeSelector(TileActionLibrary.getGrowTypeRandomTrees);
			      WastelandPlanet.grow_type_selector_plants = new GrowTypeSelector(TileActionLibrary.getGrowTypeRandomPlants);
		    //    WastelandPlanet.addUnit(Creatures.Terlanius, 1);
		      //  WastelandPlanet.addMineral(SB.mineral_stone, 5);
		     //   WastelandPlanet.addMineral(SB.mineral_metals, 3);
            AssetManager.biome_library.add(WastelandPlanet);
            AssetManager.biome_library.addBiomeToPool(WastelandPlanet);

			break;
    case "crystal world":

            SetRandomMapTemplate();
            AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_crystal"));
break;			

    case "swamp world":

            SetRandomMapTemplate();
            AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_swamp"));
break;			

    case "jungle world":
      Config.current_map_template = "boring_plains";
      AssetManager.biome_library.addBiomeToPool(AssetManager.biome_library.get("biome_jungle"));

break;			

    case "gas giant":

              SetRandomMapTemplate();
break;			

                default:
                    Debug.Log("Space biome selected.");


                    break;
            }
        }

     public static void SetRandomMapTemplate()
        {

            string[] options = { "boring_plains", "ballsass" };

            int randomIndex = UnityEngine.Random.Range(0, options.Length);

            Config.current_map_template = options[randomIndex];

            Debug.Log($"Current map template set to: {Config.current_map_template}");
        }
    }
}
