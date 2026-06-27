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
using System.Reflection;

namespace ModernBox {
class Commerce {

  internal void init() { commerce_init(); }
  private void commerce_init() {

    Harmony harmony = new Harmony("com.tux.modernbox.modernbox");
            MethodInfo balls = typeof(BaseSimObject).GetMethod(nameof(BaseSimObject.findEnemyObjectTarget), BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo dalls = typeof(Commerce).GetMethod(nameof(Commerce.findEnemyObjectTarget_prefix));
            harmony.Patch(balls, new HarmonyMethod(dalls)); 
    RaceBuildOrderAsset human = AssetManager.race_build_orders.get("kingdom_base");

    BuildingAsset casino = AssetManager.buildings.clone("casino", "!city_building");
    AssetManager.buildings.add(casino);
    casino.id = "casino";

    casino.priority = 69999;
    casino.fundament = new BuildingFundament(2, 2, 2, 0);
    casino.cost = new ConstructionCost(pWood : 15, pStone : 25, pGold : 250);
    casino.burnable = false;
    casino.build_place_single = true;
    casino.tech = "Casino";
    casino.build_place_batch = false;
    casino.base_stats[S.health] = 3000f;
    casino.base_stats[S.size] = 1.0f;
    casino.canBeLivingHouse = false;
    casino.resources_given.Add(new ResourceContainer{id = "Gold", amount = 2});
    loadSprites(casino);

    AddBuildingOrderKeysToCivRaces("order_casino", "casino");

    human.addBuilding("order_casino", 1, pPop : 50, pBuildings : 16);

    BuildingAsset restaurant = AssetManager.buildings.clone("restaurant", "!city_building");
    AssetManager.buildings.add(restaurant);
    restaurant.id = "restaurant";

    restaurant.priority = 69999;
    restaurant.fundament = new BuildingFundament(2, 2, 2, 0);
    restaurant.cost = new ConstructionCost(pWood : 15, pStone : 25, pGold : 250);
    restaurant.burnable = false;
    restaurant.build_place_single = true;
    restaurant.tech = "Casino";
    restaurant.build_place_batch = false;
    restaurant.base_stats[S.health] = 3000f;
    restaurant.base_stats[S.size] = 1.0f;
    restaurant.canBeLivingHouse = false;
    restaurant.resources_given.Add(new ResourceContainer{id = "Gold", amount = 2});
    loadSprites(restaurant);

    AddBuildingOrderKeysToCivRaces("order_restaurant", "restaurant");

    human.addBuilding("order_restaurant", 1, pPop : 50, pBuildings : 16);

    BuildingAsset mall = AssetManager.buildings.clone("mall", "!city_building");
    AssetManager.buildings.add(mall);
    mall.id = "mall";

    mall.priority = 69999;
    mall.fundament = new BuildingFundament(2, 2, 2, 0);
    mall.cost = new ConstructionCost(pWood : 15, pStone : 25, pGold : 250);
    mall.burnable = false;
    mall.build_place_single = true;
    mall.tech = "Casino";
    mall.build_place_batch = false;
    mall.base_stats[S.health] = 3000f;
    mall.base_stats[S.size] = 1.0f;
    mall.canBeLivingHouse = false;
    mall.resources_given.Add(new ResourceContainer{id = "Gold", amount = 2});
    loadSprites(mall);

    AddBuildingOrderKeysToCivRaces("order_mall", "mall");

    human.addBuilding("order_mall", 1, pPop : 50, pBuildings : 16);



    BuildingAsset school = AssetManager.buildings.clone("school", "!city_building");
    AssetManager.buildings.add(school);
    school.id = "school";

    school.priority = 69999;
    school.fundament = new BuildingFundament(2, 2, 2, 0);
    school.cost = new ConstructionCost(pWood : 15, pStone : 25, pGold : 250);
    school.burnable = false;
    school.build_place_single = true;
    school.tech = "Casino";
    school.build_place_batch = false;
    school.base_stats[S.health] = 3000f;
    school.base_stats[S.size] = 1.0f;
    school.canBeLivingHouse = false;
    school.resources_given.Add(new ResourceContainer{id = "Gold", amount = 2});
    loadSprites(school);

    AddBuildingOrderKeysToCivRaces("order_school", "school");

    human.addBuilding("order_school", 1, pPop : 50, pBuildings : 16);

    BuildingAsset modernbuilding = AssetManager.buildings.clone("modernbuilding", "!city_building");
    AssetManager.buildings.add(modernbuilding);
    modernbuilding.id = "modernbuilding";

    modernbuilding.priority = 69999;
    modernbuilding.fundament = new BuildingFundament(2, 2, 2, 0);
    modernbuilding.cost = new ConstructionCost(0, 2, 1, 1);
    modernbuilding.tech = "Casino";
    modernbuilding.housing = 60;
    modernbuilding.base_stats[S.health] = 3000f;
    loadSprites(modernbuilding);

    AddBuildingOrderKeysToCivRaces("order_modernbuilding", "modernbuilding");

    human.addBuilding("order_modernbuilding", 1, pPop : 50, pBuildings : 16);

    BuildingAsset AirFactory = AssetManager.buildings.clone("AirFactory", "!city_building");
    AssetManager.buildings.add(AirFactory);
    AirFactory.id = "AirFactory";

    AirFactory.priority = 69999;
    AirFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    AirFactory.cost = new ConstructionCost(0, 2, 1, 1);
    AirFactory.tech = "MIRVBomber";
    AirFactory.housing = 60;
    AirFactory.spawnUnits_asset = "MIRVBomber";
    AirFactory.base_stats[S.health] = 3000f;
    loadSprites(AirFactory);

    AddBuildingOrderKeysToCivRaces("order_AirFactory", "AirFactory");

    human.addBuilding("order_AirFactory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset TankFactory = AssetManager.buildings.clone("TankFactory", "!city_building");
    AssetManager.buildings.add(TankFactory);
    TankFactory.id = "TankFactory";

    TankFactory.priority = 69999;
    TankFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    TankFactory.cost = new ConstructionCost(0, 2, 1, 1);
    TankFactory.tech = "Tanks";
    TankFactory.housing = 60;
    TankFactory.spawnUnits_asset = "Tank";
    TankFactory.base_stats[S.health] = 3000f;
    loadSprites(TankFactory);

    AddBuildingOrderKeysToCivRaces("order_TankFactory", "TankFactory");

    human.addBuilding("order_RailgunFactory", 1, pPop : 50, pBuildings : 16);

            BuildingAsset RailgunFactory = AssetManager.buildings.clone("RailgunFactory", "!city_building");
            AssetManager.buildings.add(RailgunFactory);
            RailgunFactory.id = "RailgunFactory";

            RailgunFactory.priority = 69999;
            RailgunFactory.fundament = new BuildingFundament(2, 2, 2, 0);
            RailgunFactory.cost = new ConstructionCost(0, 2, 1, 1);
            RailgunFactory.tech = "Railguns";
            RailgunFactory.housing = 60;
            RailgunFactory.spawnUnits_asset = "Railgun";
            RailgunFactory.base_stats[S.health] = 3000f;
            loadSprites(RailgunFactory);

            AddBuildingOrderKeysToCivRaces("order_RailgunFactory", "RailgunFactory");

            BuildingAsset HumveeFactory = AssetManager.buildings.clone("HumveeFactory", "!city_building");
    AssetManager.buildings.add(HumveeFactory);
    HumveeFactory.id = "HumveeFactory";

    HumveeFactory.priority = 69999;
    HumveeFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    HumveeFactory.cost = new ConstructionCost(0, 2, 1, 1);
    HumveeFactory.tech = "Tanks";
    HumveeFactory.housing = 60;
    HumveeFactory.spawnUnits_asset = "Humvee";
    HumveeFactory.base_stats[S.health] = 3000f;
    loadSprites(HumveeFactory);

    AddBuildingOrderKeysToCivRaces("order_HumveeFactory", "HumveeFactory");

    human.addBuilding("order_HumveeFactory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset HelicopterFactory = AssetManager.buildings.clone("HelicopterFactory", "!city_building");
    AssetManager.buildings.add(HelicopterFactory);
    HelicopterFactory.id = "HelicopterFactory";

    HelicopterFactory.priority = 69999;
    HelicopterFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    HelicopterFactory.cost = new ConstructionCost(0, 2, 1, 1);
    HelicopterFactory.tech = "Helicopter";
    HelicopterFactory.housing = 60;
    HelicopterFactory.spawnUnits_asset = "Heli";
    HelicopterFactory.base_stats[S.health] = 3000f;
    loadSprites(HelicopterFactory);

    AddBuildingOrderKeysToCivRaces("order_HelicopterFactory", "HelicopterFactory");

    human.addBuilding("order_HelicopterFactory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset DroneFactory = AssetManager.buildings.clone("DroneFactory", "!city_building");
    AssetManager.buildings.add(DroneFactory);
    DroneFactory.id = "DroneFactory";

    DroneFactory.priority = 69999;
    DroneFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    DroneFactory.cost = new ConstructionCost(0, 2, 1, 1);
    DroneFactory.tech = "Drone";
    DroneFactory.housing = 60;
    DroneFactory.spawnUnits_asset = "Drone";
    DroneFactory.base_stats[S.health] = 3000f;
    loadSprites(DroneFactory);

    AddBuildingOrderKeysToCivRaces("order_DroneFactory", "DroneFactory");

    human.addBuilding("order_DroneFactory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset AirshipFactory = AssetManager.buildings.clone("AirshipFactory", "!city_building");
    AssetManager.buildings.add(AirshipFactory);
    AirshipFactory.id = "AirshipFactory";

    AirshipFactory.priority = 69999;
    AirshipFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    AirshipFactory.cost = new ConstructionCost(0, 2, 1, 1);
    AirshipFactory.tech = "Casino";
    AirshipFactory.housing = 60;
    AirshipFactory.spawnUnits_asset = "Zeppelin";
    AirshipFactory.base_stats[S.health] = 3000f;
    loadSprites(AirshipFactory);

    AddBuildingOrderKeysToCivRaces("order_AirshipFactory", "AirshipFactory");

    BuildingAsset F22Factory = AssetManager.buildings.clone("F22Factory", "!city_building");
    AssetManager.buildings.add(F22Factory);
    F22Factory.id = "F22Factory";

    F22Factory.priority = 69999;
    F22Factory.fundament = new BuildingFundament(2, 2, 2, 0);
    F22Factory.cost = new ConstructionCost(0, 2, 1, 1);
    F22Factory.tech = "FighterJet";
    F22Factory.housing = 60;
    F22Factory.spawnUnits_asset = "F22";
    F22Factory.base_stats[S.health] = 3000f;
    loadSprites(F22Factory);

    AddBuildingOrderKeysToCivRaces("order_F22Factory", "F22Factory");
    human.addBuilding("order_F22Factory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset F55Factory = AssetManager.buildings.clone("F55Factory", "!city_building");
    AssetManager.buildings.add(F55Factory);
    F55Factory.id = "F55Factory";

    F55Factory.priority = 69999;
    F55Factory.fundament = new BuildingFundament(2, 2, 2, 0);
    F55Factory.cost = new ConstructionCost(0, 2, 1, 1);
    F55Factory.tech = "F55";
    F55Factory.housing = 60;
    F55Factory.spawnUnits_asset = "F55";
    F55Factory.base_stats[S.health] = 3000f;
    loadSprites(F55Factory);

    AddBuildingOrderKeysToCivRaces("order_F55Factory", "F55Factory");

    human.addBuilding("order_F55Factory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset BoiFactory = AssetManager.buildings.clone("BoiFactory", "!city_building");
    AssetManager.buildings.add(BoiFactory);
    BoiFactory.id = "BoiFactory";

    BoiFactory.priority = 69999;
    BoiFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    BoiFactory.cost = new ConstructionCost(0, 2, 1, 1);
    BoiFactory.tech = "Boi";
    BoiFactory.housing = 60;
    BoiFactory.spawnUnits_asset = "MissileSystem";
    BoiFactory.base_stats[S.health] = 3000f;
    loadSprites(BoiFactory);

    AddBuildingOrderKeysToCivRaces("order_BoiFactory", "BoiFactory");

    human.addBuilding("order_BoiFactory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset GunshipFactory = AssetManager.buildings.clone("GunshipFactory", "!city_building");
    AssetManager.buildings.add(GunshipFactory);
    GunshipFactory.id = "GunshipFactory";

    GunshipFactory.priority = 69999;
    GunshipFactory.fundament = new BuildingFundament(2, 2, 2, 0);
    GunshipFactory.cost = new ConstructionCost(0, 2, 1, 1);
    GunshipFactory.tech = "Gunship";
    GunshipFactory.housing = 60;
    GunshipFactory.spawnUnits_asset = "Gunship";
    GunshipFactory.base_stats[S.health] = 3000f;
    loadSprites(GunshipFactory);

    AddBuildingOrderKeysToCivRaces("order_GunshipFactory", "GunshipFactory");

    human.addBuilding("order_GunshipFactory", 1, pPop : 50, pBuildings : 16);

    BuildingAsset ModernBarracks = AssetManager.buildings.clone("ModernBarracks", "!city_building");
    AssetManager.buildings.add(ModernBarracks);
    ModernBarracks.id = "ModernBarracks";

    ModernBarracks.priority = 89893289;
    ModernBarracks.fundament = new BuildingFundament(2, 2, 2, 0);
    ModernBarracks.cost = new ConstructionCost(pCommonMetals: 1, pGold: 1);
    ModernBarracks.tech = "MilitaryModern";
    ModernBarracks.housing = 60;
    ModernBarracks.spawnUnits = true;
    ModernBarracks.spawnUnits_asset = "Soldier";
    ModernBarracks.base_stats[S.health] = 3000f;
    loadSprites(ModernBarracks);

    AddBuildingOrderKeysToCivRaces("order_ModernBarracks", "ModernBarracks");

    human.addBuilding("order_ModernBarracks", 1, pPop : 50, pBuildings : 16);

            BuildingAsset MissileSilo = AssetManager.buildings.clone("MissileSilo", "!city_building");
            AssetManager.buildings.add(MissileSilo);
            MissileSilo.id = "MissileSilo";
            MissileSilo.type = "MissileSilo";
            MissileSilo.priority = 3750;
            MissileSilo.fundament = new BuildingFundament(4, 2, 2, 0);
            MissileSilo.cost = new ConstructionCost(pCommonMetals: 32, pGold: 72);
            MissileSilo.burnable = false;
            MissileSilo.canBeUpgraded = false;
            MissileSilo.tower_projectile_amount = 1;
            MissileSilo.build_place_single = true;
            MissileSilo.build_place_batch = false;
            MissileSilo.base_stats[S.health] = 2500;
            MissileSilo.canBeLivingHouse = true;
            MissileSilo.tech = "Nukes";
            MissileSilo.buildRoadTo = false;
            MissileSilo.tower = true;
            MissileSilo.tower_projectile = "NUKER";
            MissileSilo.tower_projectile_offset = 4f;
            MissileSilo.tower_projectile_reload = 64f;
            AssetManager.buildings.loadSprites(MissileSilo);


	
    MethodInfo original = typeof(BuildOrder).GetMethod(nameof(BuildOrder.getBuildingAsset), BindingFlags.Instance | BindingFlags.Public);
    MethodInfo prefix = typeof(Commerce).GetMethod(nameof(getBuildingAsset_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
    harmony.Patch(original, new HarmonyMethod(prefix));
    original = typeof(Building).GetMethod(nameof(Building.checkSpriteToRender), BindingFlags.Instance | BindingFlags.Public);
    prefix = typeof(Commerce).GetMethod(nameof(Building_checkSpriteToRender_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
    harmony.Patch(original, new HarmonyMethod(prefix));
  }

  public static void toggleDrones() {
    Main.modifyBoolOption("DronesOption", PowerButtons.GetToggleValue("Drones_Toggle"));
    if (PowerButtons.GetToggleValue("Drones_Toggle")) {
      turnOnDrones();
    } else {
      turnOffDrones();
    }
  }

  public static void turnOnDrones() { SetFactoriesSpawnDrone(true); }

  public static void turnOffDrones() { SetFactoriesSpawnDrone(false); }

  public static void toggleAirFactory() {
    Main.modifyBoolOption("MIRVBomberOption", PowerButtons.GetToggleValue("AirFactory_toggle"));
    if (PowerButtons.GetToggleValue("AirFactory_toggle")) {
      turnOnAirFactory();
    } else {
      turnOffAirFactory();
    }
  }

  public static void turnOnAirFactory() { SetFactorySpawnUnits("AirFactory", true); }

  public static void turnOffAirFactory() { SetFactorySpawnUnits("AirFactory", false); }

  public static void toggleTankFactory() {
    Main.modifyBoolOption("TankOption", PowerButtons.GetToggleValue("TankFactory_toggle"));
    if (PowerButtons.GetToggleValue("TankFactory_toggle")) {
      turnOnTankFactory();
    } else {
      turnOffTankFactory();
    }
  }

  public static void turnOnTankFactory() { SetFactorySpawnUnits("TankFactory", true); }

  public static void turnOffTankFactory() { SetFactorySpawnUnits("TankFactory", false); }
  
  public static void toggleBarracks() {
    Main.modifyBoolOption("SoldierOption", PowerButtons.GetToggleValue("Soldier_toggle"));
    if (PowerButtons.GetToggleValue("Soldier_toggle")) {
      turnOnSoldiers();
    } else {
      turnOffSoldiers();
    }
  }

  public static void turnOnSoldiers() { SetFactorySpawnUnits("ModernBarracks", true); }

  public static void turnOffSoldiers() { SetFactorySpawnUnits("ModernBarracks", false); }


        public static void toggleRailgunFactory()
        {
            Main.modifyBoolOption("RailgunOption", PowerButtons.GetToggleValue("RailgunFactory_toggle"));
            if (PowerButtons.GetToggleValue("RailgunFactory_toggle"))
            {
                turnOnRailgunFactory();
            }
            else
            {
                turnOffRailgunFactory();
            }
        }

        public static void turnOnRailgunFactory() { SetFactorySpawnUnits("RailgunFactory", true); }

        public static void turnOffRailgunFactory() { SetFactorySpawnUnits("RailgunFactory", false); }

        public static void toggleHumveeFactory() {
    Main.modifyBoolOption("HumveeOption", PowerButtons.GetToggleValue("HumveeFactory_toggle"));
    if (PowerButtons.GetToggleValue("HumveeFactory_toggle")) {
      turnOnHumveeFactory();
    } else {
      turnOffHumveeFactory();
    }
  }

  public static void turnOnHumveeFactory() { SetFactorySpawnUnits("HumveeFactory", true); }

  public static void turnOffHumveeFactory() { SetFactorySpawnUnits("HumveeFactory", false); }

  public static void toggleHelicopterFactory() {
    Main.modifyBoolOption("HeliOption", PowerButtons.GetToggleValue("HelicopterFactory_toggle"));
    if (PowerButtons.GetToggleValue("HelicopterFactory_toggle")) {
      turnOnHelicopterFactory();
    } else {
      turnOffHelicopterFactory();
    }
  }

  public static void turnOnHelicopterFactory() { SetFactorySpawnUnits("HelicopterFactory", true); }

  public static void turnOffHelicopterFactory() { SetFactorySpawnUnits("HelicopterFactory", false); }

  public static void toggleF22Factory() {
    Main.modifyBoolOption("F22Option", PowerButtons.GetToggleValue("F22Factory_toggle"));
    if (PowerButtons.GetToggleValue("F22Factory_toggle")) {
      turnOnF22Factory();
    } else {
      turnOffF22Factory();
    }
  }

  public static void turnOnF22Factory() { SetFactorySpawnUnits("F22Factory", true); }

  public static void turnOffF22Factory() { SetFactorySpawnUnits("F22Factory", false); }

  public static void toggleAirshipFactory() {
    Main.modifyBoolOption("AirshipOption", PowerButtons.GetToggleValue("AirshipFactory_toggle"));
    if (PowerButtons.GetToggleValue("AirshipFactory_toggle")) {
      turnOnAirshipFactory();
    } else {
      turnOffAirshipFactory();
    }
  }

  public static void turnOnAirshipFactory() { SetFactorySpawnUnits("AirshipFactory", true); }

  public static void turnOffAirshipFactory() { SetFactorySpawnUnits("AirshipFactory", false); }

  public static void toggleBoiFactory() {
    Main.modifyBoolOption("BoiOption", PowerButtons.GetToggleValue("BoiFactory_toggle"));
    if (PowerButtons.GetToggleValue("BoiFactory_toggle")) {
      turnOnBoiFactory();
    } else {
      turnOffBoiFactory();
    }
  }

  public static void turnOnBoiFactory() { SetFactorySpawnUnits("BoiFactory", true); }

  public static void turnOffBoiFactory() { SetFactorySpawnUnits("BoiFactory", false); }

  public static void toggleGunshipFactory() {
    Main.modifyBoolOption("GunshipOption", PowerButtons.GetToggleValue("GunshipFactory_toggle"));
    if (PowerButtons.GetToggleValue("GunshipFactory_toggle")) {
      turnOnGunshipFactory();
    } else {
      turnOffGunshipFactory();
    }
  }

  public static void turnOnGunshipFactory() { SetFactorySpawnUnits("GunshipFactory", true); }

  public static void turnOffGunshipFactory() { SetFactorySpawnUnits("GunshipFactory", false); }

  private static void SetFactorySpawnUnits(string factoryID, bool spawn) {
    BuildingAsset factory = AssetManager.buildings.get(factoryID);
    if (factory != null) {
      factory.spawnUnits = spawn;
    }
  }

  private static void SetFactoriesSpawnDrone(bool spawn) {
    List<string> droneIDs = new List<string>{"DroneFactory"};
    foreach (string droneID in droneIDs) {
      BuildingAsset dronefac = AssetManager.buildings.get(droneID);
      if (dronefac != null) {
        dronefac.spawnUnits = false;
      }
    }
  }
  
  public static void toggleNukes() {
    Main.modifyBoolOption("NukeOption", PowerButtons.GetToggleValue("Nuke_toggle"));
    if (PowerButtons.GetToggleValue("Nuke_toggle")) {
      turnOnNukes();
    } else {
      turnOffNukes();
    }
  }

  public static void turnOnNukes() 
  { 
  OpenNukeWindow();
      RaceBuildOrderAsset human = AssetManager.race_build_orders.get("kingdom_base");

      human.addBuilding("order_MissileSilo", 1, pPop : 50, pBuildings : 16);

  AddBuildingOrderKeysToCivRaces("order_MissileSilo", "MissileSilo"); 
  
  }

  public static void turnOffNukes() { }


  private static void OpenNukeWindow() {


    Windows.ShowWindow("NukeWindow");

  }
  
  private static bool getBuildingAsset_Prefix(Asset __instance, ref BuildingAsset __result, City pCity, string pBuildingID = null) {
    if (string.IsNullOrEmpty(pBuildingID))
      pBuildingID = __instance.id;
    try {
      string buildingOrderKey = pCity.race.building_order_keys[pBuildingID];
      __result = AssetManager.buildings.get(buildingOrderKey);
    } catch (Exception e) {
      Debug.Log("Failed to get building order key for " + pBuildingID);
      Debug.Log(e);
    }

    return false;
  }
  private static bool Building_checkSpriteToRender_Prefix(Building __instance) {
    bool isNotRuin = true;
    bool isRuin = __instance.isRuin();
    if (isRuin)
      isNotRuin = false;
    if (__instance.isUnderConstruction()) {
      __instance.last_main_sprite = __instance.asset.sprites.construction;
      __instance.spriteRenderer.sprite = __instance.last_main_sprite;
    } else {
      Sprite[] pSpriteAnimations;
      if (__instance.asset.has_special_animation_state)
        pSpriteAnimations = !__instance.hasResources ? __instance.animData.special : __instance.animData.main;
      else if (isRuin && __instance.asset.has_ruins_graphics) {
        pSpriteAnimations = __instance.animData.ruins;
      } else if (__instance.asset.spawn_drops && __instance.data.hasFlag(S.stop_spawn_drops))
        pSpriteAnimations = __instance.animData.main_disabled;
      else if (__instance.data.state == BuildingState.CivAbandoned) {
        pSpriteAnimations = __instance.animData.main_disabled.Length == 0 ? __instance.animData.main : __instance.animData.main_disabled;
        isNotRuin = false;
      } else {
        pSpriteAnimations = __instance.animData.main;
        Sprite[] spriteArray = __instance.asset.get_override_sprite_main?.Invoke(__instance);
        if (spriteArray != null)
          pSpriteAnimations = spriteArray;
      }
      Sprite pBuildingSprite;
      try {
        pBuildingSprite = !__instance.check_spawn_animation ?(!isNotRuin || pSpriteAnimations.Length == 1 ? pSpriteAnimations[0] : AnimationHelper.getSpriteFromList(__instance.GetHashCode(), pSpriteAnimations, __instance.asset.animation_speed)) : __instance.getSpawnFrameSprite();
      } catch (Exception) {
        __instance.kill();
        __instance.startRemove();
        return false;
      }
      if (!(__instance.last_main_sprite != pBuildingSprite) && __instance._last_color_asset == __instance.kingdom.kingdomColor)
        return false;
      __instance._last_colored_sprite = UnitSpriteConstructor.getRecoloredSpriteBuilding(pBuildingSprite, __instance.kingdom.kingdomColor);
      __instance.spriteRenderer.sprite = __instance._last_colored_sprite;
      __instance.last_main_sprite = pBuildingSprite;
      __instance._last_color_asset = __instance.kingdom.kingdomColor;
    }

    return false;
  }

  private static void AddBuildingOrderKeysToCivRaces(string key, string value) {
    foreach (Race race in AssetManager.raceLibrary.list.Where(race => race.civilization)) {
      race.building_order_keys.Add(key, value);
    }
  }
  private static void RemoveBuildingOrderKeysToCivRaces(string key, string value) {
    foreach (Race race in AssetManager.raceLibrary.list.Where(race => race.civilization)) {
        race.building_order_keys.Remove(key, out value);
    }
  }
        [HarmonyPatch(typeof(BaseSimObject), "findEnemyObjectTarget")]
        [HarmonyPrefix]
        public static bool findEnemyObjectTarget_prefix(BaseSimObject __instance, ref BaseSimObject __result)
        {
            bool pFindClosest = Toolbox.randomChance(0.6f);
            int range = -1; 

            if (__instance is Building d && d.asset.id == "MissileSilo")
            { 
                range = 25; 
            }
            EnemyFinderData enemyFinderData = EnemiesFinder.findEnemiesFrom(__instance.currentTile, __instance.kingdom, range);
            if (enemyFinderData.list == null)
            {
                __result = null;
                return false;
            }

            BaseSimObject result = null;
            __instance.checkObjectList(enemyFinderData.list, pFindClosest, out result);
            __result = result;
            return false;
        }
    
	
  private static Dictionary<string, Sprite[]> cached_sprite_list;
  internal static void loadSprites(BuildingAsset pTemplate) {
    if (cached_sprite_list is null) {
      cached_sprite_list = Reflection.GetField(typeof(SpriteTextureLoader), null, "cached_sprite_list") as Dictionary<string, Sprite[]>;
    }

    string pPath = pTemplate.sprite_path;
    if (String.IsNullOrEmpty(pPath)) {
      pPath = $"buildings/{pTemplate.id}";
    }

    if (cached_sprite_list.ContainsKey(pPath)) {
      cached_sprite_list.Remove(pPath);
    }

    AssetManager.buildings.loadSprites(pTemplate);
  }
}
}