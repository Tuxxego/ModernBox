//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using NCMS.Utils;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine.UI;
using System.IO;
using pathfinding;
using HarmonyLib;


namespace ModernBox
{
    class Factory
    {


		
        internal void init()
        {
            factory_init();


        }

        private void factory_init()
        {
            // BuildingAsset factory = AssetManager.buildings.clone("factory", "!city_building");
            // AssetManager.buildings.add(factory);
            // factory.id = "factory";
            // factory.type = "factory";
            // factory.priority = 69999;
            // factory.fundament = new BuildingFundament(2, 2, 2, 0);
            // factory.cost = new ConstructionCost(pWood: 15, pStone: 25, pGold: 500);
            // factory.burnable = false;
            // factory.build_place_single = true;
            // factory.build_place_batch = false;
            // factory.base_stats[S.health] = 3000f;
            // factory.base_stats[S.size] = 1.3f;
            // factory.canBeLivingHouse = false;
            // factory.resources_given.Add(new ResourceContainer
            // {
                // id = "Parts",
                // amount = 2
            // });
            // loadSprites(factory);

            // Race humanRace = AssetManager.raceLibrary.get("human");
            // humanRace.building_order_keys.Add("order_factory", "factory");

            // RaceBuildOrderAsset human = AssetManager.race_build_orders.get("kingdom_base");
           // // human.addBuilding("order_factory", 1, pPop: 50, pBuildings: 16);


			
		// //	human.addBuilding("order_xeniumpowerplant", 1, pPop: 100, pBuildings: 20);
			
		    // BuildingAsset xeniumplant = AssetManager.buildings.clone("xeniumpowerplant", "!city_building");
            // AssetManager.buildings.add(xeniumplant);
            // xeniumplant.id = "xeniumpowerplant";
            // xeniumplant.type = "xenplant";
            // xeniumplant.priority = 69999;
            // xeniumplant.fundament = new BuildingFundament(2, 2, 2, 0);
            // xeniumplant.cost = new ConstructionCost(pWood: 20, pStone: 35, pGold: 350);
            // xeniumplant.burnable = false;
            // xeniumplant.build_place_single = true;
            // xeniumplant.build_place_batch = false;
            // xeniumplant.base_stats[S.health] = 6000f;
            // xeniumplant.base_stats[S.size] = 1.3f;
            // xeniumplant.canBeLivingHouse = false;
            // xeniumplant.resources_given.Add(new ResourceContainer
            // {
                // id = "Xenium",
                // amount = 20
            // });
            // loadSprites(xeniumplant);

            // humanRace.building_order_keys.Add("order_xeniumpowerplant", "xeniumpowerplant");

		}
			
		// private static Dictionary<string, Sprite[]> cached_sprite_list;
        // internal static void loadSprites(BuildingAsset pTemplate){
            // if(cached_sprite_list is null){
                // cached_sprite_list = Reflection.GetField(typeof(SpriteTextureLoader), null, "cached_sprite_list") as Dictionary<string, Sprite[]>;
            // }

            // string pPath = pTemplate.sprite_path;
            // if(String.IsNullOrEmpty(pPath)){
                // pPath = $"buildings/{pTemplate.id}"; 
            // }

            // if(cached_sprite_list.ContainsKey(pPath)){
                // cached_sprite_list.Remove(pPath);
            // }
            
            // AssetManager.buildings.loadSprites(pTemplate);
        // }



			
	}
}