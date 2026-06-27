using System;
using tools;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using HarmonyLib;
using System.Text.RegularExpressions;
using Beebyte.Obfuscator;
namespace ModernBox
{
    class Buildings : MonoBehaviour
    {
        public static void init()
        {
            BuildingOrderandStuff();
        }
        private static void BuildingOrderandStuff()
        {
            float[] era = ModernBoxPrefs.EraProgress;
            float[] ren = ModernBoxPrefs.RenaissanceTime;
            float[] fuckmaxim = ModernBoxPrefs.HyperfutureTime;
           BuildingAsset stockpile = AssetManager.buildings.get("stockpile");
            stockpile.base_stats["health"] = 999999f;
            stockpile.burnable = false;
           BuildingAsset stockpile_fireproof = AssetManager.buildings.get("stockpile_fireproof");
            stockpile_fireproof.base_stats["health"] = 999999f;
   BuildingAsset mine = AssetManager.buildings.get("mine");
mine.cost = new ConstructionCost(1, 0, 0, 0);
mine.upgrade_level = 0;
mine.can_be_upgraded = true;
mine.burnable = false;
mine.upgrade_to = "mine_modern";
mine.fundament = new BuildingFundament(1, 1, 1, 0);
mine.construction_progress_needed = 5;
BuildingAsset mine_modern = AssetManager.buildings.clone("mine_modern", "mine");
mine_modern.can_be_upgraded = false;
mine_modern.priority = 9999;
mine_modern.cost = new ConstructionCost();
mine_modern.smoke = true;
mine_modern.smoke_interval = 2.5f;
mine_modern.smoke_offset = new Vector2Int(2, 3);
mine_modern.produce_biome_food = true;
mine_modern.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
mine_modern.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
mine_modern.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
mine_modern.sprite_path = "buildings/mine_modern";
mine_modern.has_sprites_main_disabled = false;
mine_modern.has_sprites_main = true;
mine_modern.has_sprites_ruin = true;
mine_modern.setShadow(0.5f, 0.23f, 0.27f);
mine_modern.upgrade_level = 1;
mine_modern.has_sprite_construction = false;
  mine_modern.has_sprites_special = false;
  mine_modern.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(mine_modern);



string[] alliancecivs = new string[] { "human", "plague_doctor", "evil_mage", "civ_white_mage", "civ_cat", "civ_dog", "civ_chicken", "civ_sheep", "civ_acid_gentleman", "bandit"};
foreach (string allianceciv in alliancecivs)
{
    string barracksId = $"barracks_{allianceciv}";
    BuildingAsset barracks = AssetManager.buildings.get(barracksId);
    if (barracks != null)
    {
        barracks.priority = 100;
        barracks.fundament = new BuildingFundament(3, 3, 4, 0);
        barracks.cost = new ConstructionCost(1, 0, 0, 0);
barracks.upgrade_level = 1;
barracks.build_prefer_replace_house = true;
barracks.can_be_upgraded = true;
barracks.upgrade_to = "Barracks_rain_alliance";
    }
    string houseId = $"house_{allianceciv}_0";
    BuildingAsset house = AssetManager.buildings.get(houseId);
    if (house != null)
    {
        house.cost = new ConstructionCost(1, 0, 0, 0);
house.upgrade_level = 0;
house.can_be_upgraded = true;
house.upgrade_to = "House_rain_alliance";
    }
      string hallId = $"hall_{allianceciv}_0";
    BuildingAsset hall = AssetManager.buildings.get(hallId);
    if (hall != null)
    {
        hall.cost = new ConstructionCost(1, 0, 0, 0);
        hall.max_houses = Mathf.Max(hall.max_houses, 8);
hall.upgrade_level = 0;
hall.can_be_upgraded = true;
hall.upgrade_to = "Hall_rain_alliance";
    }
          string templeId = $"temple_{allianceciv}";
    BuildingAsset temple = AssetManager.buildings.get(templeId);
    if (temple != null)
    {
        temple.cost = new ConstructionCost(1, 0, 0, 0);
temple.upgrade_level = 0;
temple.can_be_upgraded = true;
temple.max_houses = 1;
temple.storage = true;
temple.book_slots = 10;
temple.build_prefer_replace_house = true;
temple.upgrade_to = "Temple_rain_alliance";
    }
    string libraryId = $"library_{allianceciv}";
    BuildingAsset library = AssetManager.buildings.get(libraryId);
    if (library != null)
    {
        library.cost = new ConstructionCost(1, 0, 0, 0);
library.upgrade_level = 0;
library.can_be_upgraded = false;
library.burnable = false;
    }
      string dockId = $"docks_{allianceciv}";
    BuildingAsset docks = AssetManager.buildings.get(dockId);
    if (docks != null)
    {
docks.cost = new ConstructionCost(1, 0, 0, 0);
docks.upgrade_level = 2;
docks.can_be_upgraded = true;
docks.build_prefer_replace_house = true;
docks.upgrade_to = "Docks_modern_alliance";
    }
          string watch_towerId = $"watch_tower_{allianceciv}";
    BuildingAsset watch_tower = AssetManager.buildings.get(watch_towerId);
    if (watch_tower != null)
    {
watch_tower.cost = new ConstructionCost(1, 0, 0, 0);
watch_tower.upgrade_level = 1;
watch_tower.can_be_upgraded = true;
watch_tower.upgrade_to = "watch_tower_modern_alliance";
    }
}
BuildingAsset bonfire_alliance = AssetManager.buildings.clone("bonfire_alliance", "$city_building$");
bonfire_alliance.draw_light_area = true;
bonfire_alliance.can_be_upgraded = true;
bonfire_alliance.upgrade_to = "bonfire_rain_alliance";
bonfire_alliance.draw_light_size = 0.8f;
bonfire_alliance.can_be_abandoned = false;
bonfire_alliance.priority = 120;
bonfire_alliance.type = "type_bonfire";
bonfire_alliance.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_alliance.construction_progress_needed = 30;
bonfire_alliance.cost = new ConstructionCost();
bonfire_alliance.smoke = true;
bonfire_alliance.smoke_interval = 2.5f;
bonfire_alliance.smoke_offset = new Vector2Int(2, 3);
bonfire_alliance.can_be_living_house = false;
bonfire_alliance.build_place_batch = false;
bonfire_alliance.build_prefer_replace_house = true;
bonfire_alliance.check_for_close_building = false;
bonfire_alliance.produce_biome_food = true;
bonfire_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_alliance.check_for_adaptation_tags = false;
bonfire_alliance.max_houses = 1;
bonfire_alliance.base_stats["health"] = 1500f;
bonfire_alliance.burnable = false;
bonfire_alliance.sprite_path = "buildings/bonfire_alliance";
bonfire_alliance.has_sprites_main_disabled = false;
bonfire_alliance.has_sprites_main = true;
bonfire_alliance.has_sprites_ruin = true;
bonfire_alliance.setShadow(0.5f, 0.23f, 0.27f);
bonfire_alliance.upgrade_level = 0;
bonfire_alliance.has_sprite_construction = true;
 bonfire_alliance.has_sprites_special = false;
  bonfire_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_alliance);
BuildingAsset market_alliance = AssetManager.buildings.clone("market_alliance", "market_human");
market_alliance.cost = new ConstructionCost(1, 0, 0, 0);
market_alliance.upgrade_level = 0;
market_alliance.can_be_upgraded = false;
market_alliance.burnable = false;
market_alliance.sprite_path = "buildings/market_alliance";
market_alliance.has_sprites_special = false;
  market_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(market_alliance);
BuildingAsset bonfire_rain_alliance = AssetManager.buildings.clone("bonfire_rain_alliance", "$city_building$");
bonfire_rain_alliance.draw_light_area = true;
bonfire_rain_alliance.can_be_upgraded = true;
bonfire_rain_alliance.upgraded_from = "bonfire_alliance";
bonfire_rain_alliance.upgrade_to = "bonfire_modern_alliance";
bonfire_rain_alliance.draw_light_size = 0.8f;
bonfire_rain_alliance.can_be_abandoned = false;
bonfire_rain_alliance.priority = 500;
bonfire_rain_alliance.type = "type_bonfire";
bonfire_rain_alliance.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_rain_alliance.construction_progress_needed = 30;
bonfire_rain_alliance.cost = new ConstructionCost(Mathf.RoundToInt(ren[0]), Mathf.RoundToInt(ren[1]), Mathf.RoundToInt(ren[2]), Mathf.RoundToInt(ren[3]));
bonfire_rain_alliance.smoke = true;
bonfire_rain_alliance.smoke_interval = 2.5f;
bonfire_rain_alliance.smoke_offset = new Vector2Int(2, 3);
bonfire_rain_alliance.can_be_living_house = false;
bonfire_rain_alliance.build_place_batch = false;
bonfire_rain_alliance.build_prefer_replace_house = true;
bonfire_rain_alliance.check_for_close_building = false;
bonfire_rain_alliance.produce_biome_food = true;
bonfire_rain_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_rain_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_rain_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_rain_alliance.check_for_adaptation_tags = false;
bonfire_rain_alliance.max_houses = 1;
bonfire_rain_alliance.base_stats["health"] = 20000f;
bonfire_rain_alliance.burnable = false;
bonfire_rain_alliance.sprite_path = "buildings/bonfire_rain";
bonfire_rain_alliance.has_sprites_main_disabled = false;
bonfire_rain_alliance.has_sprites_main = true;
bonfire_rain_alliance.has_sprites_ruin = true;
bonfire_rain_alliance.setShadow(0.5f, 0.23f, 0.27f);
bonfire_rain_alliance.upgrade_level = 1;
bonfire_rain_alliance.has_sprite_construction = true;
bonfire_rain_alliance.has_sprites_special = false;
  bonfire_rain_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_rain_alliance);
 BuildingAsset House_rain_alliance = AssetManager.buildings.clone("House_rain_alliance", "$building_civ_human$");
		House_rain_alliance.setHousingSlots(10);
		House_rain_alliance.loot_generation = 6;
		House_rain_alliance.housing_happiness = 7;
		House_rain_alliance.type = "type_house";
		House_rain_alliance.sound_hit = "event:/SFX/HIT/HitWood";
		House_rain_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		House_rain_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
        House_rain_alliance.draw_light_area = true;
		House_rain_alliance.draw_light_size = 0.2f;
        House_rain_alliance.priority = 100;
        House_rain_alliance.fundament = new BuildingFundament(1, 1, 2, 0);
        House_rain_alliance.cost = new ConstructionCost(0, 0, 0, 0);
        House_rain_alliance.group = "human";
        House_rain_alliance.sprite_path = "buildings/House_rain_alliance";
        House_rain_alliance.base_stats["health"] = 5000f;
        House_rain_alliance.burnable = false;
        House_rain_alliance.has_sprites_main_disabled = false;
	    House_rain_alliance.has_sprites_main = true;
	    House_rain_alliance.has_sprites_ruin = true;
        House_rain_alliance.setShadow(0.5f, 0.23f, 0.27f);
House_rain_alliance.upgrade_level = 1;
House_rain_alliance.can_be_upgraded = true;
House_rain_alliance.has_sprite_construction = false;
House_rain_alliance.upgraded_from = "house_human_0";
House_rain_alliance.upgrade_to = "House_modern_alliance";
House_rain_alliance.has_sprites_special = false;
  House_rain_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_rain_alliance);
BuildingAsset Barracks_rain_alliance = AssetManager.buildings.clone("Barracks_rain_alliance", "$building_civ_human$");
Barracks_rain_alliance.draw_light_area = true;
Barracks_rain_alliance.draw_light_size = 0.5f;
Barracks_rain_alliance.type = "type_barracks";
Barracks_rain_alliance.priority = 100;
Barracks_rain_alliance.fundament = new BuildingFundament(2, 2, 2, 0);
Barracks_rain_alliance.cost = new ConstructionCost(0, 0, 0, 0);
Barracks_rain_alliance.group = "human";
Barracks_rain_alliance.sprite_path = "buildings/Barracks_rain_alliance";
Barracks_rain_alliance.base_stats["health"] = 6000f;
Barracks_rain_alliance.burnable = false;
Barracks_rain_alliance.has_sprites_main_disabled = false;
Barracks_rain_alliance.has_sprites_main = true;
Barracks_rain_alliance.has_sprites_ruin = true;
Barracks_rain_alliance.setShadow(0.5f, 0.23f, 0.27f);
Barracks_rain_alliance.upgrade_level = 2;
Barracks_rain_alliance.can_be_upgraded = true;
Barracks_rain_alliance.has_sprite_construction = false;
Barracks_rain_alliance.upgraded_from = "barracks_human";
Barracks_rain_alliance.upgrade_to = "Barracks_modern_alliance";
Barracks_rain_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
Barracks_rain_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Barracks_rain_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Barracks_rain_alliance.has_sprites_special = false;
  Barracks_rain_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Barracks_rain_alliance);
BuildingAsset Temple_rain_alliance = AssetManager.buildings.clone("Temple_rain_alliance", "$building_civ_human$");
Temple_rain_alliance.storage = true;
Temple_rain_alliance.book_slots = 15;
Temple_rain_alliance.draw_light_area = true;
Temple_rain_alliance.draw_light_size = 0.3f;
Temple_rain_alliance.draw_light_area_offset_y = 3f;
Temple_rain_alliance.priority = 100;
Temple_rain_alliance.type = "type_temple";
Temple_rain_alliance.fundament = new BuildingFundament(2, 2, 3, 0);
Temple_rain_alliance.cost = new ConstructionCost(0, 0, 0, 0);
Temple_rain_alliance.group = "human";
Temple_rain_alliance.max_houses = 1;
Temple_rain_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
Temple_rain_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Temple_rain_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Temple_rain_alliance.base_stats["health"] = 10000f;
Temple_rain_alliance.burnable = false;
Temple_rain_alliance.can_be_upgraded = false;
Temple_rain_alliance.sprite_path = "buildings/Temple_rain_alliance";
Temple_rain_alliance.has_sprites_main_disabled = false;
Temple_rain_alliance.has_sprites_main = true;
Temple_rain_alliance.has_sprites_ruin = true;
Temple_rain_alliance.setShadow(0.5f, 0.23f, 0.27f);
Temple_rain_alliance.upgrade_level = 1;
Temple_rain_alliance.has_sprite_construction = false;
Temple_rain_alliance.upgraded_from = "temple_human";
Temple_rain_alliance.has_sprites_special = false;
  Temple_rain_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Temple_rain_alliance);
 BuildingAsset Hall_rain_alliance = AssetManager.buildings.clone("Hall_rain_alliance", "$building_civ_human$");
		Hall_rain_alliance.priority = 100;
		Hall_rain_alliance.storage = true;
		Hall_rain_alliance.type = "type_hall";
		Hall_rain_alliance.fundament = new BuildingFundament(3, 3, 4, 0);
		Hall_rain_alliance.can_be_upgraded = false;
		Hall_rain_alliance.base_stats["health"] = 10000f;
		Hall_rain_alliance.burnable = false;
		Hall_rain_alliance.setHousingSlots(12);
		Hall_rain_alliance.housing_happiness = 10;
		Hall_rain_alliance.loot_generation = 3;
		Hall_rain_alliance.ignore_other_buildings_for_upgrade = true;
		Hall_rain_alliance.build_place_batch = true;
		Hall_rain_alliance.max_houses = Mathf.Max(Hall_rain_alliance.max_houses, 12);
		Hall_rain_alliance.produce_biome_food = true;
		Hall_rain_alliance.setShadow(0.56f, 0.41f, 0.43f);
		Hall_rain_alliance.draw_light_size = 0.3f;
		Hall_rain_alliance.book_slots = 5;
        Hall_rain_alliance.draw_light_area = true;
		Hall_rain_alliance.upgraded_from = "hall_human_0";
		Hall_rain_alliance.sound_hit = "event:/SFX/HIT/HitWood";
		Hall_rain_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		Hall_rain_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
        Hall_rain_alliance.cost = new ConstructionCost(0, 0, 0, 0);
        Hall_rain_alliance.group = "human";
        Hall_rain_alliance.sprite_path = "buildings/Hall_rain_alliance";
        Hall_rain_alliance.has_sprites_main_disabled = false;
	    Hall_rain_alliance.has_sprites_main = true;
	    Hall_rain_alliance.has_sprites_ruin = true;
        Hall_rain_alliance.setShadow(0.5f, 0.23f, 0.27f);
Hall_rain_alliance.upgrade_level = 1;
Hall_rain_alliance.has_sprite_construction = false;
Hall_rain_alliance.has_sprites_special = false;
  Hall_rain_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Hall_rain_alliance);
BuildingAsset Docks_modern_alliance = AssetManager.buildings.clone("Docks_modern_alliance", "docks_human");
Docks_modern_alliance.boat_types = new string[13] { "cargo_alliance_boat", "destroyer_a_alliance_boat", "destroyer_b_alliance_boat", "carrier_alliance_boat", "submarine_alliance_boat", "fishing_alliance_boat", "abrawler_alliance_boat", "bbrawler_alliance_boat", "cbrawler_alliance_boat", "dbrawler_alliance_boat", "ebrawler_alliance_boat", "fbrawler_alliance_boat", "transporter_alliance_boat" };
Docks_modern_alliance.priority = 100;
Docks_modern_alliance.group = "human";
Docks_modern_alliance.civ_kingdom = "human";
Docks_modern_alliance.cost = new ConstructionCost(0, 0, 0, 0);
Docks_modern_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
Docks_modern_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Docks_modern_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Docks_modern_alliance.base_stats["health"] = 10000f;
Docks_modern_alliance.burnable = false;
Docks_modern_alliance.sprite_path = "buildings/Docks_modern";
Docks_modern_alliance.has_sprites_main_disabled = false;
Docks_modern_alliance.has_sprites_main = true;
Docks_modern_alliance.has_sprites_ruin = true;
Docks_modern_alliance.setShadow(0.5f, 0.23f, 0.27f);
Docks_modern_alliance.upgrade_level = 3;
Docks_modern_alliance.has_sprite_construction = false;
Docks_modern_alliance.can_be_upgraded = false;
Docks_modern_alliance.upgraded_from = "docks_human";
Docks_modern_alliance.has_sprites_special = false;
  Docks_modern_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Docks_modern_alliance);
BuildingAsset bonfire_modern_alliance = AssetManager.buildings.clone("bonfire_modern_alliance", "$city_building$");
bonfire_modern_alliance.draw_light_area = true;
bonfire_modern_alliance.can_be_upgraded = true;
bonfire_modern_alliance.upgraded_from = "bonfire_rain_alliance";
bonfire_modern_alliance.cost = new ConstructionCost(Mathf.RoundToInt(era[0]), Mathf.RoundToInt(era[1]), Mathf.RoundToInt(era[2]), Mathf.RoundToInt(era[3]));
bonfire_modern_alliance.draw_light_size = 5f;
bonfire_modern_alliance.can_be_abandoned = true;
bonfire_modern_alliance.priority = 1000;
bonfire_modern_alliance.type = "type_bonfire";
bonfire_modern_alliance.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_modern_alliance.construction_progress_needed = 30;
bonfire_modern_alliance.smoke = true;
bonfire_modern_alliance.smoke_interval = 2.5f;
bonfire_modern_alliance.smoke_offset = new Vector2Int(2, 3);
bonfire_modern_alliance.can_be_living_house = false;
bonfire_modern_alliance.build_place_batch = false;
bonfire_modern_alliance.build_prefer_replace_house = true;
bonfire_modern_alliance.check_for_close_building = false;
bonfire_modern_alliance.produce_biome_food = true;
bonfire_modern_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_modern_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_modern_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_modern_alliance.check_for_adaptation_tags = false;
bonfire_modern_alliance.max_houses = 1;
bonfire_modern_alliance.base_stats["health"] = 40000f;
bonfire_modern_alliance.burnable = false;
bonfire_modern_alliance.sprite_path = "buildings/bonfire_modern";
bonfire_modern_alliance.has_sprites_main_disabled = false;
bonfire_modern_alliance.has_sprites_main = true;
bonfire_modern_alliance.has_sprites_ruin = true;
bonfire_modern_alliance.setShadow(0.5f, 0.23f, 0.27f);
bonfire_modern_alliance.upgrade_level = 2;
bonfire_modern_alliance.upgrade_to = "bonfire_future_alliance";
bonfire_modern_alliance.has_sprite_construction = true;
bonfire_modern_alliance.has_sprites_special = false;
  bonfire_modern_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_modern_alliance);
BuildingAsset bonfire_future_alliance = AssetManager.buildings.clone("bonfire_future_alliance", "$city_building$");
bonfire_future_alliance.draw_light_area = true;
bonfire_future_alliance.can_be_upgraded = false;
bonfire_future_alliance.upgraded_from = "bonfire_modern_alliance";
bonfire_future_alliance.cost = new ConstructionCost(Mathf.RoundToInt(fuckmaxim[0]), Mathf.RoundToInt(fuckmaxim[1]), Mathf.RoundToInt(fuckmaxim[2]), Mathf.RoundToInt(fuckmaxim[3]));
bonfire_future_alliance.draw_light_size = 5f;
bonfire_future_alliance.can_be_abandoned = true;
bonfire_future_alliance.priority = 1000;
bonfire_future_alliance.type = "type_bonfire";
bonfire_future_alliance.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_future_alliance.construction_progress_needed = 30;
bonfire_future_alliance.smoke = true;
bonfire_future_alliance.smoke_interval = 2.5f;
bonfire_future_alliance.smoke_offset = new Vector2Int(2, 3);
bonfire_future_alliance.can_be_living_house = false;
bonfire_future_alliance.build_place_batch = false;
bonfire_future_alliance.build_prefer_replace_house = true;
bonfire_future_alliance.check_for_close_building = false;
bonfire_future_alliance.produce_biome_food = true;
bonfire_future_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_future_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_future_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_future_alliance.check_for_adaptation_tags = false;
bonfire_future_alliance.max_houses = 1;
bonfire_future_alliance.base_stats["health"] = 40000f;
bonfire_future_alliance.burnable = false;
bonfire_future_alliance.sprite_path = "buildings/bonfire_future";
bonfire_future_alliance.has_sprites_main_disabled = false;
bonfire_future_alliance.has_sprites_main = true;
bonfire_future_alliance.has_sprites_ruin = true;
bonfire_future_alliance.setShadow(0.5f, 0.23f, 0.27f);
bonfire_future_alliance.upgrade_level = 3;
bonfire_future_alliance.has_sprite_construction = true;
bonfire_future_alliance.has_sprites_special = false;
  bonfire_future_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_future_alliance);
 BuildingAsset House_modern_alliance = AssetManager.buildings.clone("House_modern_alliance", "$building_civ_human$");
		House_modern_alliance.setHousingSlots(20);
		House_modern_alliance.loot_generation = 6;
		House_modern_alliance.housing_happiness = 7;
		House_modern_alliance.type = "type_house";
		House_modern_alliance.sound_hit = "event:/SFX/HIT/HitWood";
		House_modern_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		House_modern_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
        House_modern_alliance.draw_light_area = true;
		House_modern_alliance.draw_light_size = 0.2f;
        House_modern_alliance.priority = 100;
        House_modern_alliance.fundament = new BuildingFundament(1, 1, 2, 0);
        House_modern_alliance.cost = new ConstructionCost(0, 0, 0, 0);
        House_modern_alliance.group = "human";
        House_modern_alliance.sprite_path = "buildings/House_modern";
        House_modern_alliance.base_stats["health"] = 6000f;
        House_modern_alliance.burnable = false;
        House_modern_alliance.has_sprites_main_disabled = false;
	    House_modern_alliance.has_sprites_main = true;
	    House_modern_alliance.has_sprites_ruin = true;
        House_modern_alliance.setShadow(0.5f, 0.23f, 0.27f);
House_modern_alliance.upgrade_level = 2;
House_modern_alliance.can_be_upgraded = true;
House_modern_alliance.has_sprite_construction = false;
House_modern_alliance.upgraded_from = "House_rain_alliance";
House_modern_alliance.upgrade_to = "House_future_alliance";
House_modern_alliance.has_sprites_special = false;
  House_modern_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_modern_alliance);
 BuildingAsset House_future_alliance = AssetManager.buildings.clone("House_future_alliance", "$building_civ_human$");
		House_future_alliance.setHousingSlots(20);
		House_future_alliance.loot_generation = 6;
		House_future_alliance.housing_happiness = 7;
		House_future_alliance.type = "type_house";
		House_future_alliance.sound_hit = "event:/SFX/HIT/HitWood";
		House_future_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		House_future_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
        House_future_alliance.draw_light_area = true;
		House_future_alliance.draw_light_size = 0.2f;
        House_future_alliance.priority = 10000;
        House_future_alliance.fundament = new BuildingFundament(1, 1, 2, 0);
        House_future_alliance.cost = new ConstructionCost(0, 0, 0, 0);
        House_future_alliance.group = "human";
        House_future_alliance.sprite_path = "buildings/House_future";
        House_future_alliance.base_stats["health"] = 6000f;
        House_future_alliance.base_stats["scale"] = 1.4f;
        House_future_alliance.burnable = false;
        House_future_alliance.has_sprites_main_disabled = false;
	    House_future_alliance.has_sprites_main = true;
	    House_future_alliance.has_sprites_ruin = true;
        House_future_alliance.setShadow(0.5f, 0.23f, 0.27f);
House_future_alliance.upgrade_level = 3;
House_future_alliance.can_be_upgraded = false;
House_future_alliance.has_sprite_construction = false;
House_future_alliance.upgraded_from = "House_modern_alliance";
House_future_alliance.has_sprites_special = false;
  House_future_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_future_alliance);
BuildingAsset Barracks_modern_alliance = AssetManager.buildings.clone("Barracks_modern_alliance", "$building_civ_human$");
Barracks_modern_alliance.draw_light_area = true;
Barracks_modern_alliance.draw_light_size = 0.5f;
Barracks_modern_alliance.type = "type_barracks";
Barracks_modern_alliance.priority = 100;
Barracks_modern_alliance.fundament = new BuildingFundament(2, 2, 2, 0);
Barracks_modern_alliance.cost = new ConstructionCost(0, 0, 0, 0);
Barracks_modern_alliance.group = "human";
Barracks_modern_alliance.sprite_path = "buildings/Barracks_modern";
Barracks_modern_alliance.base_stats["health"] = 10000f;
Barracks_modern_alliance.burnable = false;
Barracks_modern_alliance.has_sprites_main_disabled = false;
Barracks_modern_alliance.has_sprites_main = true;
Barracks_modern_alliance.has_sprites_ruin = true;
Barracks_modern_alliance.setShadow(0.5f, 0.23f, 0.27f);
Barracks_modern_alliance.upgrade_level = 3;
Barracks_modern_alliance.can_be_upgraded = false;
Barracks_modern_alliance.has_sprite_construction = false;
Barracks_modern_alliance.upgraded_from = "Barracks_rain_alliance";
Barracks_modern_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
Barracks_modern_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Barracks_modern_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Barracks_modern_alliance.has_sprites_special = false;
  Barracks_modern_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Barracks_modern_alliance);
BuildingAsset watch_tower_modern_alliance = AssetManager.buildings.clone("watch_tower_modern_alliance", "$building_civ_human$");
watch_tower_modern_alliance.upgrade_level = 2;
watch_tower_modern_alliance.can_be_upgraded = false;
watch_tower_modern_alliance.upgraded_from = "watch_tower_human";
watch_tower_modern_alliance.has_sprite_construction = false;
watch_tower_modern_alliance.burnable = false;
watch_tower_modern_alliance.has_sprites_main_disabled = false;
watch_tower_modern_alliance.has_sprites_main = true;
watch_tower_modern_alliance.has_sprites_ruin = true;
		watch_tower_modern_alliance.draw_light_area = true;
		watch_tower_modern_alliance.draw_light_size = 1f;
        watch_tower_modern_alliance.build_road_to = false;
		watch_tower_modern_alliance.base_stats["health"] = 10000f;
		watch_tower_modern_alliance.base_stats["targets"] = 1f;
		watch_tower_modern_alliance.base_stats["area_of_effect"] = 1f;
		watch_tower_modern_alliance.base_stats["damage"] = 20f;
		watch_tower_modern_alliance.base_stats["knockback"] = 1f;
        watch_tower_modern_alliance.smoke = true;
watch_tower_modern_alliance.smoke_interval = 2.5f;
watch_tower_modern_alliance.smoke_offset = new Vector2Int(2, 3);
		watch_tower_modern_alliance.priority = 100;
		watch_tower_modern_alliance.type = "type_watch_tower";
		watch_tower_modern_alliance.fundament = new BuildingFundament(1, 1, 1, 0);
		watch_tower_modern_alliance.cost = new ConstructionCost(0, 0, 0, 0);
		watch_tower_modern_alliance.tower = true;
		watch_tower_modern_alliance.sprite_path = "buildings/watch_tower_modern";
        watch_tower_modern_alliance.tower_projectile = "arrow";
		watch_tower_modern_alliance.tower_projectile_offset = 4f;
		watch_tower_modern_alliance.tower_projectile_amount = 1;
        watch_tower_modern_alliance.tower_projectile_reload = 0f;
		watch_tower_modern_alliance.build_place_borders = true;
		watch_tower_modern_alliance.build_place_batch = false;
		watch_tower_modern_alliance.build_place_single = true;
		watch_tower_modern_alliance.setShadow(0.5f, 0.23f, 0.27f);
		watch_tower_modern_alliance.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
		watch_tower_modern_alliance.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		watch_tower_modern_alliance.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
        watch_tower_modern_alliance.has_sprites_special = false;
  watch_tower_modern_alliance.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(watch_tower_modern_alliance);
CityBuildOrderAsset allianceBuild = new CityBuildOrderAsset();
allianceBuild.id = "build_order_alliance_epochs";
BuildOrder allianceorder;
allianceorder = allianceBuild.addBuilding("order_bonfire_alliance", 1);
allianceBuild.addUpgrade("order_bonfire_alliance");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
allianceBuild.addUpgrade("order_bonfire_rain_alliance");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
  allianceBuild.addUpgrade("order_bonfire_modern_alliance");
  allianceorder = allianceBuild.list.Last();
  allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
  allianceBuild.addUpgrade("order_bonfire_future_alliance");
  allianceorder = allianceBuild.list.Last();
  allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
  allianceorder = allianceBuild.addBuilding("order_stockpile", 1);
allianceorder = allianceBuild.addBuilding("order_hall_0", 1);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
allianceBuild.addUpgrade("order_hall_0");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceorder = allianceBuild.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire");
allianceBuild.addUpgrade("order_house_0");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_alliance");
allianceBuild.addUpgrade("order_house_rain_alliance");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceBuild.addUpgrade("order_house_modern_alliance");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceBuild.addUpgrade("order_house_future_alliance");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_future_alliance");
allianceorder = allianceBuild.addBuilding("order_watch_tower", 0, 0, 0);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
allianceBuild.addUpgrade("order_watch_tower");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceorder = allianceBuild.addBuilding("order_temple", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
allianceBuild.addUpgrade("order_temple");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceorder = allianceBuild.addBuilding("order_mine", 1, 10, 10);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
allianceBuild.addUpgrade("order_mine");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceorder = allianceBuild.addBuilding("order_docks_0", 1, 0, 0);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
allianceBuild.addUpgrade("order_docks_0");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
allianceBuild.addUpgrade("order_docks_1");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceorder = allianceBuild.addBuilding("order_windmill_0", 1, 0, 0);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
allianceorder = allianceBuild.addBuilding("order_barracks", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
allianceBuild.addUpgrade("order_barracks");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_alliance");
allianceBuild.addUpgrade("order_barracks_rain_alliance");
allianceorder = allianceBuild.list.Last();
allianceorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_alliance");
allianceorder = allianceBuild.addBuilding("order_library", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
allianceorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
AssetManager.city_build_orders.add(allianceBuild);



string[] hardencivs = new string[] { "dwarf", "cold_one", "snowman", "civ_armadillo", "civ_rhino", "civ_crab", "civ_penguin", "civ_turtle", "civ_crystal_golem", "civ_candy_man", "civ_goat" };
foreach (string hardenciv in hardencivs)
{
    string barracksId = $"barracks_{hardenciv}";
    BuildingAsset barracks = AssetManager.buildings.get(barracksId);
    if (barracks != null)
    {
        barracks.priority = 100;
        barracks.fundament = new BuildingFundament(3, 3, 4, 0);
        barracks.cost = new ConstructionCost(1, 0, 0, 0);
        barracks.upgrade_level = 1;
        barracks.build_prefer_replace_house = true;
        barracks.can_be_upgraded = true;
        barracks.upgrade_to = "Barracks_rain_harden";
    }
    string houseId = $"house_{hardenciv}_0";
    BuildingAsset house = AssetManager.buildings.get(houseId);
    if (house != null)
    {
        house.cost = new ConstructionCost(1, 0, 0, 0);
        house.upgrade_level = 0;
        house.can_be_upgraded = true;
        house.upgrade_to = "House_rain_harden";
    }
    string hallId = $"hall_{hardenciv}_0";
    BuildingAsset hall = AssetManager.buildings.get(hallId);
    if (hall != null)
    {
        hall.cost = new ConstructionCost(1, 0, 0, 0);
        hall.max_houses = Mathf.Max(hall.max_houses, 8);
        hall.upgrade_level = 0;
        hall.can_be_upgraded = true;
        hall.upgrade_to = "Hall_rain_harden";
    }
    string templeId = $"temple_{hardenciv}";
    BuildingAsset temple = AssetManager.buildings.get(templeId);
    if (temple != null)
    {
        temple.cost = new ConstructionCost(1, 0, 0, 0);
        temple.upgrade_level = 0;
        temple.can_be_upgraded = true;
        temple.max_houses = 1;
        temple.storage = true;
        temple.book_slots = 10;
        temple.build_prefer_replace_house = true;
        temple.upgrade_to = "Temple_rain_harden";
    }
    string libraryId = $"library_{hardenciv}";
    BuildingAsset library = AssetManager.buildings.get(libraryId);
    if (library != null)
    {
        library.cost = new ConstructionCost(1, 0, 0, 0);
        library.upgrade_level = 0;
        library.can_be_upgraded = false;
        library.burnable = false;
    }
    string dockId = $"docks_{hardenciv}";
    BuildingAsset docks = AssetManager.buildings.get(dockId);
    if (docks != null)
    {
        docks.cost = new ConstructionCost(1, 0, 0, 0);
        docks.upgrade_level = 2;
        docks.can_be_upgraded = true;
        docks.build_prefer_replace_house = true;
        docks.upgrade_to = "Docks_modern_harden";
    }
    string watch_towerId = $"watch_tower_{hardenciv}";
    BuildingAsset watch_tower = AssetManager.buildings.get(watch_towerId);
    if (watch_tower != null)
    {
        watch_tower.cost = new ConstructionCost(1, 0, 0, 0);
        watch_tower.upgrade_level = 1;
        watch_tower.can_be_upgraded = true;
        watch_tower.upgrade_to = "watch_tower_modern_harden";
    }
}
BuildingAsset bonfire_harden = AssetManager.buildings.clone("bonfire_harden", "$city_building$");
bonfire_harden.draw_light_area = true;
bonfire_harden.can_be_upgraded = true;
bonfire_harden.upgrade_to = "bonfire_rain_harden";
bonfire_harden.draw_light_size = 0.8f;
bonfire_harden.can_be_abandoned = false;
bonfire_harden.priority = 120;
bonfire_harden.type = "type_bonfire";
bonfire_harden.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_harden.construction_progress_needed = 30;
bonfire_harden.cost = new ConstructionCost();
bonfire_harden.smoke = true;
bonfire_harden.smoke_interval = 2.5f;
bonfire_harden.smoke_offset = new Vector2Int(2, 3);
bonfire_harden.can_be_living_house = false;
bonfire_harden.build_place_batch = false;
bonfire_harden.build_prefer_replace_house = true;
bonfire_harden.check_for_close_building = false;
bonfire_harden.produce_biome_food = true;
bonfire_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_harden.check_for_adaptation_tags = false;
bonfire_harden.max_houses = 1;
bonfire_harden.base_stats["health"] = 1500f;
bonfire_harden.burnable = false;
bonfire_harden.sprite_path = "buildings/bonfire_harden";
bonfire_harden.has_sprites_main_disabled = false;
bonfire_harden.has_sprites_main = true;
bonfire_harden.has_sprites_ruin = true;
bonfire_harden.setShadow(0.5f, 0.23f, 0.27f);
bonfire_harden.upgrade_level = 0;
bonfire_harden.has_sprite_construction = true;
bonfire_harden.has_sprites_special = false;
bonfire_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_harden);
BuildingAsset market_harden = AssetManager.buildings.clone("market_harden", "market_human");
market_harden.cost = new ConstructionCost(1, 0, 0, 0);
market_harden.upgrade_level = 0;
market_harden.can_be_upgraded = false;
market_harden.burnable = false;
market_harden.sprite_path = "buildings/market_harden";
market_harden.has_sprites_special = false;
market_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(market_harden);
BuildingAsset bonfire_rain_harden = AssetManager.buildings.clone("bonfire_rain_harden", "$city_building$");
bonfire_rain_harden.draw_light_area = true;
bonfire_rain_harden.can_be_upgraded = true;
bonfire_rain_harden.upgraded_from = "bonfire_harden";
bonfire_rain_harden.upgrade_to = "bonfire_modern_harden";
bonfire_rain_harden.draw_light_size = 0.8f;
bonfire_rain_harden.can_be_abandoned = false;
bonfire_rain_harden.priority = 500;
bonfire_rain_harden.type = "type_bonfire";
bonfire_rain_harden.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_rain_harden.construction_progress_needed = 30;
bonfire_rain_harden.cost = new ConstructionCost(Mathf.RoundToInt(ren[0]), Mathf.RoundToInt(ren[1]), Mathf.RoundToInt(ren[2]), Mathf.RoundToInt(ren[3]));
bonfire_rain_harden.smoke = true;
bonfire_rain_harden.smoke_interval = 2.5f;
bonfire_rain_harden.smoke_offset = new Vector2Int(2, 3);
bonfire_rain_harden.can_be_living_house = false;
bonfire_rain_harden.build_place_batch = false;
bonfire_rain_harden.build_prefer_replace_house = true;
bonfire_rain_harden.check_for_close_building = false;
bonfire_rain_harden.produce_biome_food = true;
bonfire_rain_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_rain_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_rain_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_rain_harden.check_for_adaptation_tags = false;
bonfire_rain_harden.max_houses = 1;
bonfire_rain_harden.base_stats["health"] = 20000f;
bonfire_rain_harden.burnable = false;
bonfire_rain_harden.sprite_path = "buildings/bonfire_rain";
bonfire_rain_harden.has_sprites_main_disabled = false;
bonfire_rain_harden.has_sprites_main = true;
bonfire_rain_harden.has_sprites_ruin = true;
bonfire_rain_harden.setShadow(0.5f, 0.23f, 0.27f);
bonfire_rain_harden.upgrade_level = 1;
bonfire_rain_harden.has_sprite_construction = true;
bonfire_rain_harden.has_sprites_special = false;
bonfire_rain_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_rain_harden);
BuildingAsset House_rain_harden = AssetManager.buildings.clone("House_rain_harden", "$building_civ_human$");
House_rain_harden.setHousingSlots(10);
House_rain_harden.loot_generation = 6;
House_rain_harden.housing_happiness = 7;
House_rain_harden.type = "type_house";
House_rain_harden.sound_hit = "event:/SFX/HIT/HitWood";
House_rain_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
House_rain_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
House_rain_harden.draw_light_area = true;
House_rain_harden.draw_light_size = 0.2f;
House_rain_harden.priority = 100;
House_rain_harden.fundament = new BuildingFundament(1, 1, 2, 0);
House_rain_harden.cost = new ConstructionCost(0, 0, 0, 0);
House_rain_harden.group = "human";
House_rain_harden.sprite_path = "buildings/House_rain_alliance";
House_rain_harden.base_stats["health"] = 5000f;
House_rain_harden.burnable = false;
House_rain_harden.has_sprites_main_disabled = false;
House_rain_harden.has_sprites_main = true;
House_rain_harden.has_sprites_ruin = true;
House_rain_harden.setShadow(0.5f, 0.23f, 0.27f);
House_rain_harden.upgrade_level = 1;
House_rain_harden.can_be_upgraded = true;
House_rain_harden.has_sprite_construction = false;
House_rain_harden.upgraded_from = "house_human_0";
House_rain_harden.upgrade_to = "House_modern_harden";
House_rain_harden.has_sprites_special = false;
House_rain_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_rain_harden);
BuildingAsset Barracks_rain_harden = AssetManager.buildings.clone("Barracks_rain_harden", "$building_civ_human$");
Barracks_rain_harden.draw_light_area = true;
Barracks_rain_harden.draw_light_size = 0.5f;
Barracks_rain_harden.type = "type_barracks";
Barracks_rain_harden.priority = 100;
Barracks_rain_harden.fundament = new BuildingFundament(2, 2, 2, 0);
Barracks_rain_harden.cost = new ConstructionCost(0, 0, 0, 0);
Barracks_rain_harden.group = "human";
Barracks_rain_harden.sprite_path = "buildings/Barracks_rain_alliance";
Barracks_rain_harden.base_stats["health"] = 6000f;
Barracks_rain_harden.burnable = false;
Barracks_rain_harden.has_sprites_main_disabled = false;
Barracks_rain_harden.has_sprites_main = true;
Barracks_rain_harden.has_sprites_ruin = true;
Barracks_rain_harden.setShadow(0.5f, 0.23f, 0.27f);
Barracks_rain_harden.upgrade_level = 2;
Barracks_rain_harden.can_be_upgraded = true;
Barracks_rain_harden.has_sprite_construction = false;
Barracks_rain_harden.upgraded_from = "barracks_human";
Barracks_rain_harden.upgrade_to = "Barracks_modern_harden";
Barracks_rain_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
Barracks_rain_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Barracks_rain_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Barracks_rain_harden.has_sprites_special = false;
Barracks_rain_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Barracks_rain_harden);
BuildingAsset Temple_rain_harden = AssetManager.buildings.clone("Temple_rain_harden", "$building_civ_human$");
Temple_rain_harden.storage = true;
Temple_rain_harden.book_slots = 15;
Temple_rain_harden.draw_light_area = true;
Temple_rain_harden.draw_light_size = 0.3f;
Temple_rain_harden.draw_light_area_offset_y = 3f;
Temple_rain_harden.priority = 100;
Temple_rain_harden.type = "type_temple";
Temple_rain_harden.fundament = new BuildingFundament(2, 2, 3, 0);
Temple_rain_harden.cost = new ConstructionCost(0, 0, 0, 0);
Temple_rain_harden.group = "human";
Temple_rain_harden.max_houses = 1;
Temple_rain_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
Temple_rain_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Temple_rain_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Temple_rain_harden.base_stats["health"] = 10000f;
Temple_rain_harden.burnable = false;
Temple_rain_harden.can_be_upgraded = false;
Temple_rain_harden.sprite_path = "buildings/Temple_rain_alliance";
Temple_rain_harden.has_sprites_main_disabled = false;
Temple_rain_harden.has_sprites_main = true;
Temple_rain_harden.has_sprites_ruin = true;
Temple_rain_harden.setShadow(0.5f, 0.23f, 0.27f);
Temple_rain_harden.upgrade_level = 1;
Temple_rain_harden.has_sprite_construction = false;
Temple_rain_harden.upgraded_from = "temple_human";
Temple_rain_harden.has_sprites_special = false;
Temple_rain_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Temple_rain_harden);
BuildingAsset Hall_rain_harden = AssetManager.buildings.clone("Hall_rain_harden", "$building_civ_human$");
Hall_rain_harden.priority = 100;
Hall_rain_harden.storage = true;
Hall_rain_harden.type = "type_hall";
Hall_rain_harden.fundament = new BuildingFundament(3, 3, 4, 0);
Hall_rain_harden.can_be_upgraded = false;
Hall_rain_harden.base_stats["health"] = 10000f;
Hall_rain_harden.burnable = false;
Hall_rain_harden.setHousingSlots(12);
Hall_rain_harden.housing_happiness = 10;
Hall_rain_harden.loot_generation = 3;
Hall_rain_harden.ignore_other_buildings_for_upgrade = true;
Hall_rain_harden.build_place_batch = true;
Hall_rain_harden.max_houses = Mathf.Max(Hall_rain_harden.max_houses, 12);
Hall_rain_harden.produce_biome_food = true;
Hall_rain_harden.setShadow(0.56f, 0.41f, 0.43f);
Hall_rain_harden.draw_light_size = 0.3f;
Hall_rain_harden.book_slots = 5;
Hall_rain_harden.draw_light_area = true;
Hall_rain_harden.upgraded_from = "hall_human_0";
Hall_rain_harden.sound_hit = "event:/SFX/HIT/HitWood";
Hall_rain_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
Hall_rain_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
Hall_rain_harden.cost = new ConstructionCost(0, 0, 0, 0);
Hall_rain_harden.group = "human";
Hall_rain_harden.sprite_path = "buildings/Hall_rain_alliance";
Hall_rain_harden.has_sprites_main_disabled = false;
Hall_rain_harden.has_sprites_main = true;
Hall_rain_harden.has_sprites_ruin = true;
Hall_rain_harden.setShadow(0.5f, 0.23f, 0.27f);
Hall_rain_harden.upgrade_level = 1;
Hall_rain_harden.has_sprite_construction = false;
Hall_rain_harden.has_sprites_special = false;
Hall_rain_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Hall_rain_harden);
BuildingAsset Docks_modern_harden = AssetManager.buildings.clone("Docks_modern_harden", "docks_human");
Docks_modern_harden.boat_types = new string[13] { "cargo_harden_boat", "destroyer_a_harden_boat", "destroyer_b_harden_boat", "carrier_harden_boat", "submarine_harden_boat", "fishing_harden_boat", "abrawler_harden_boat", "bbrawler_harden_boat", "cbrawler_harden_boat", "dbrawler_harden_boat", "ebrawler_harden_boat", "fbrawler_harden_boat", "transporter_harden_boat" };
Docks_modern_harden.priority = 100;
Docks_modern_harden.group = "human";
Docks_modern_harden.civ_kingdom = "human";
Docks_modern_harden.cost = new ConstructionCost(0, 0, 0, 0);
Docks_modern_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
Docks_modern_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Docks_modern_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Docks_modern_harden.base_stats["health"] = 10000f;
Docks_modern_harden.burnable = false;
Docks_modern_harden.sprite_path = "buildings/Docks_modern";
Docks_modern_harden.has_sprites_main_disabled = false;
Docks_modern_harden.has_sprites_main = true;
Docks_modern_harden.has_sprites_ruin = true;
Docks_modern_harden.setShadow(0.5f, 0.23f, 0.27f);
Docks_modern_harden.upgrade_level = 3;
Docks_modern_harden.has_sprite_construction = false;
Docks_modern_harden.can_be_upgraded = false;
Docks_modern_harden.upgraded_from = "docks_human";
Docks_modern_harden.has_sprites_special = false;
Docks_modern_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Docks_modern_harden);
BuildingAsset bonfire_modern_harden = AssetManager.buildings.clone("bonfire_modern_harden", "$city_building$");
bonfire_modern_harden.draw_light_area = true;
bonfire_modern_harden.can_be_upgraded = true;
bonfire_modern_harden.upgraded_from = "bonfire_rain_harden";
bonfire_modern_harden.cost = new ConstructionCost(Mathf.RoundToInt(era[0]), Mathf.RoundToInt(era[1]), Mathf.RoundToInt(era[2]), Mathf.RoundToInt(era[3]));
bonfire_modern_harden.draw_light_size = 5f;
bonfire_modern_harden.can_be_abandoned = true;
bonfire_modern_harden.priority = 1000;
bonfire_modern_harden.type = "type_bonfire";
bonfire_modern_harden.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_modern_harden.construction_progress_needed = 30;
bonfire_modern_harden.smoke = true;
bonfire_modern_harden.smoke_interval = 2.5f;
bonfire_modern_harden.smoke_offset = new Vector2Int(2, 3);
bonfire_modern_harden.can_be_living_house = false;
bonfire_modern_harden.build_place_batch = false;
bonfire_modern_harden.build_prefer_replace_house = true;
bonfire_modern_harden.check_for_close_building = false;
bonfire_modern_harden.produce_biome_food = true;
bonfire_modern_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_modern_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_modern_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_modern_harden.check_for_adaptation_tags = false;
bonfire_modern_harden.max_houses = 1;
bonfire_modern_harden.base_stats["health"] = 40000f;
bonfire_modern_harden.burnable = false;
bonfire_modern_harden.sprite_path = "buildings/bonfire_modern";
bonfire_modern_harden.has_sprites_main_disabled = false;
bonfire_modern_harden.has_sprites_main = true;
bonfire_modern_harden.has_sprites_ruin = true;
bonfire_modern_harden.setShadow(0.5f, 0.23f, 0.27f);
bonfire_modern_harden.upgrade_level = 2;
bonfire_modern_harden.upgrade_to = "bonfire_future_harden";
bonfire_modern_harden.has_sprite_construction = true;
bonfire_modern_harden.has_sprites_special = false;
bonfire_modern_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_modern_harden);
BuildingAsset bonfire_future_harden = AssetManager.buildings.clone("bonfire_future_harden", "$city_building$");
bonfire_future_harden.draw_light_area = true;
bonfire_future_harden.can_be_upgraded = false;
bonfire_future_harden.upgraded_from = "bonfire_modern_harden";
bonfire_future_harden.cost = new ConstructionCost(Mathf.RoundToInt(fuckmaxim[0]), Mathf.RoundToInt(fuckmaxim[1]), Mathf.RoundToInt(fuckmaxim[2]), Mathf.RoundToInt(fuckmaxim[3]));
bonfire_future_harden.draw_light_size = 5f;
bonfire_future_harden.can_be_abandoned = true;
bonfire_future_harden.priority = 1000;
bonfire_future_harden.type = "type_bonfire";
bonfire_future_harden.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_future_harden.construction_progress_needed = 30;
bonfire_future_harden.smoke = true;
bonfire_future_harden.smoke_interval = 2.5f;
bonfire_future_harden.smoke_offset = new Vector2Int(2, 3);
bonfire_future_harden.can_be_living_house = false;
bonfire_future_harden.build_place_batch = false;
bonfire_future_harden.build_prefer_replace_house = true;
bonfire_future_harden.check_for_close_building = false;
bonfire_future_harden.produce_biome_food = true;
bonfire_future_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_future_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_future_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_future_harden.check_for_adaptation_tags = false;
bonfire_future_harden.max_houses = 1;
bonfire_future_harden.base_stats["health"] = 40000f;
bonfire_future_harden.burnable = false;
bonfire_future_harden.sprite_path = "buildings/bonfire_future";
bonfire_future_harden.has_sprites_main_disabled = false;
bonfire_future_harden.has_sprites_main = true;
bonfire_future_harden.has_sprites_ruin = true;
bonfire_future_harden.setShadow(0.5f, 0.23f, 0.27f);
bonfire_future_harden.upgrade_level = 3;
bonfire_future_harden.has_sprite_construction = true;
bonfire_future_harden.has_sprites_special = false;
bonfire_future_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_future_harden);
BuildingAsset House_modern_harden = AssetManager.buildings.clone("House_modern_harden", "$building_civ_human$");
House_modern_harden.setHousingSlots(20);
House_modern_harden.loot_generation = 6;
House_modern_harden.housing_happiness = 7;
House_modern_harden.type = "type_house";
House_modern_harden.sound_hit = "event:/SFX/HIT/HitWood";
House_modern_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
House_modern_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
House_modern_harden.draw_light_area = true;
House_modern_harden.draw_light_size = 0.2f;
House_modern_harden.priority = 100;
House_modern_harden.fundament = new BuildingFundament(1, 1, 2, 0);
House_modern_harden.cost = new ConstructionCost(0, 0, 0, 0);
House_modern_harden.group = "human";
House_modern_harden.sprite_path = "buildings/House_modern";
House_modern_harden.base_stats["health"] = 6000f;
House_modern_harden.burnable = false;
House_modern_harden.has_sprites_main_disabled = false;
House_modern_harden.has_sprites_main = true;
House_modern_harden.has_sprites_ruin = true;
House_modern_harden.setShadow(0.5f, 0.23f, 0.27f);
House_modern_harden.upgrade_level = 2;
House_modern_harden.can_be_upgraded = true;
House_modern_harden.has_sprite_construction = false;
House_modern_harden.upgraded_from = "House_rain_harden";
House_modern_harden.upgrade_to = "House_future_harden";
House_modern_harden.has_sprites_special = false;
House_modern_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_modern_harden);
BuildingAsset House_future_harden = AssetManager.buildings.clone("House_future_harden", "$building_civ_human$");
House_future_harden.setHousingSlots(20);
House_future_harden.loot_generation = 6;
House_future_harden.housing_happiness = 7;
House_future_harden.type = "type_house";
House_future_harden.sound_hit = "event:/SFX/HIT/HitWood";
House_future_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
House_future_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
House_future_harden.draw_light_area = true;
House_future_harden.draw_light_size = 0.2f;
House_future_harden.priority = 10000;
House_future_harden.fundament = new BuildingFundament(1, 1, 2, 0);
House_future_harden.cost = new ConstructionCost(0, 0, 0, 0);
House_future_harden.group = "human";
House_future_harden.sprite_path = "buildings/House_future";
House_future_harden.base_stats["health"] = 6000f;
House_future_harden.base_stats["scale"] = 1.4f;
House_future_harden.burnable = false;
House_future_harden.has_sprites_main_disabled = false;
House_future_harden.has_sprites_main = true;
House_future_harden.has_sprites_ruin = true;
House_future_harden.setShadow(0.5f, 0.23f, 0.27f);
House_future_harden.upgrade_level = 3;
House_future_harden.can_be_upgraded = false;
House_future_harden.has_sprite_construction = false;
House_future_harden.upgraded_from = "House_modern_harden";
House_future_harden.has_sprites_special = false;
House_future_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_future_harden);
BuildingAsset Barracks_modern_harden = AssetManager.buildings.clone("Barracks_modern_harden", "$building_civ_human$");
Barracks_modern_harden.draw_light_area = true;
Barracks_modern_harden.draw_light_size = 0.5f;
Barracks_modern_harden.type = "type_barracks";
Barracks_modern_harden.priority = 100;
Barracks_modern_harden.fundament = new BuildingFundament(2, 2, 2, 0);
Barracks_modern_harden.cost = new ConstructionCost(0, 0, 0, 0);
Barracks_modern_harden.group = "human";
Barracks_modern_harden.sprite_path = "buildings/Barracks_modern";
Barracks_modern_harden.base_stats["health"] = 10000f;
Barracks_modern_harden.burnable = false;
Barracks_modern_harden.has_sprites_main_disabled = false;
Barracks_modern_harden.has_sprites_main = true;
Barracks_modern_harden.has_sprites_ruin = true;
Barracks_modern_harden.setShadow(0.5f, 0.23f, 0.27f);
Barracks_modern_harden.upgrade_level = 3;
Barracks_modern_harden.can_be_upgraded = false;
Barracks_modern_harden.has_sprite_construction = false;
Barracks_modern_harden.upgraded_from = "Barracks_rain_harden";
Barracks_modern_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
Barracks_modern_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Barracks_modern_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Barracks_modern_harden.has_sprites_special = false;
Barracks_modern_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Barracks_modern_harden);
BuildingAsset watch_tower_modern_harden = AssetManager.buildings.clone("watch_tower_modern_harden", "$building_civ_human$");
watch_tower_modern_harden.upgrade_level = 2;
watch_tower_modern_harden.can_be_upgraded = false;
watch_tower_modern_harden.upgraded_from = "watch_tower_human";
watch_tower_modern_harden.has_sprite_construction = false;
watch_tower_modern_harden.burnable = false;
watch_tower_modern_harden.has_sprites_main_disabled = false;
watch_tower_modern_harden.has_sprites_main = true;
watch_tower_modern_harden.has_sprites_ruin = true;
watch_tower_modern_harden.draw_light_area = true;
watch_tower_modern_harden.draw_light_size = 1f;
watch_tower_modern_harden.build_road_to = false;
watch_tower_modern_harden.base_stats["health"] = 10000f;
watch_tower_modern_harden.base_stats["targets"] = 1f;
watch_tower_modern_harden.base_stats["area_of_effect"] = 1f;
watch_tower_modern_harden.base_stats["damage"] = 20f;
watch_tower_modern_harden.base_stats["knockback"] = 1f;
watch_tower_modern_harden.smoke = true;
watch_tower_modern_harden.smoke_interval = 2.5f;
watch_tower_modern_harden.smoke_offset = new Vector2Int(2, 3);
watch_tower_modern_harden.priority = 100;
watch_tower_modern_harden.type = "type_watch_tower";
watch_tower_modern_harden.fundament = new BuildingFundament(1, 1, 1, 0);
watch_tower_modern_harden.cost = new ConstructionCost(0, 0, 0, 0);
watch_tower_modern_harden.tower = true;
watch_tower_modern_harden.sprite_path = "buildings/watch_tower_modern";
watch_tower_modern_harden.tower_projectile = "arrow";
watch_tower_modern_harden.tower_projectile_offset = 4f;
watch_tower_modern_harden.tower_projectile_amount = 1;
watch_tower_modern_harden.tower_projectile_reload = 0f;
watch_tower_modern_harden.build_place_borders = true;
watch_tower_modern_harden.build_place_batch = false;
watch_tower_modern_harden.build_place_single = true;
watch_tower_modern_harden.setShadow(0.5f, 0.23f, 0.27f);
watch_tower_modern_harden.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
watch_tower_modern_harden.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
watch_tower_modern_harden.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
watch_tower_modern_harden.has_sprites_special = false;
watch_tower_modern_harden.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(watch_tower_modern_harden);
CityBuildOrderAsset hardenBuild = new CityBuildOrderAsset();
hardenBuild.id = "build_order_harden_epochs";
BuildOrder hardenorder;
hardenorder = hardenBuild.addBuilding("order_bonfire_harden", 1);
hardenBuild.addUpgrade("order_bonfire_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenBuild.addUpgrade("order_bonfire_rain_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenBuild.addUpgrade("order_bonfire_modern_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenBuild.addUpgrade("order_bonfire_future_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenorder = hardenBuild.addBuilding("order_stockpile", 1);
hardenorder = hardenBuild.addBuilding("order_hall_0", 1);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
hardenBuild.addUpgrade("order_hall_0");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenorder = hardenBuild.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire");
hardenBuild.addUpgrade("order_house_0");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_harden");
hardenBuild.addUpgrade("order_house_rain_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenBuild.addUpgrade("order_house_modern_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenBuild.addUpgrade("order_house_future_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_future_harden");
hardenorder = hardenBuild.addBuilding("order_watch_tower", 0, 0, 0);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
hardenBuild.addUpgrade("order_watch_tower");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenorder = hardenBuild.addBuilding("order_temple", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
hardenBuild.addUpgrade("order_temple");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenorder = hardenBuild.addBuilding("order_mine", 1, 10, 10);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenBuild.addUpgrade("order_mine");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenorder = hardenBuild.addBuilding("order_docks_0", 1, 0, 0);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
hardenBuild.addUpgrade("order_docks_0");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenBuild.addUpgrade("order_docks_1");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenorder = hardenBuild.addBuilding("order_windmill_0", 1, 0, 0);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
hardenorder = hardenBuild.addBuilding("order_barracks", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
hardenBuild.addUpgrade("order_barracks");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_harden");
hardenBuild.addUpgrade("order_barracks_rain_harden");
hardenorder = hardenBuild.list.Last();
hardenorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_harden");
hardenorder = hardenBuild.addBuilding("order_library", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
hardenorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
AssetManager.city_build_orders.add(hardenBuild);




string[] gaiacivs = new string[] { "elf", "civ_rabbit", "civ_monkey", "civ_cow", "civ_buffalo", "civ_alpaca", "civ_capybara", "civ_frog", "civ_liliar", "druid", "fairy", "civ_garlic_man", "civ_lemon_man", "unicorn" };
foreach (string gaiaciv in gaiacivs)
{
    string barracksId = $"barracks_{gaiaciv}";
    BuildingAsset barracks = AssetManager.buildings.get(barracksId);
    if (barracks != null)
    {
        barracks.priority = 100;
        barracks.fundament = new BuildingFundament(3, 3, 4, 0);
        barracks.cost = new ConstructionCost(1, 0, 0, 0);
        barracks.upgrade_level = 1;
        barracks.build_prefer_replace_house = true;
        barracks.can_be_upgraded = true;
        barracks.upgrade_to = "Barracks_rain_gaia";
    }
    string houseId = $"house_{gaiaciv}_0";
    BuildingAsset house = AssetManager.buildings.get(houseId);
    if (house != null)
    {
        house.cost = new ConstructionCost(1, 0, 0, 0);
        house.upgrade_level = 0;
        house.can_be_upgraded = true;
        house.upgrade_to = "House_rain_gaia";
    }
    string hallId = $"hall_{gaiaciv}_0";
    BuildingAsset hall = AssetManager.buildings.get(hallId);
    if (hall != null)
    {
        hall.cost = new ConstructionCost(1, 0, 0, 0);
        hall.max_houses = Mathf.Max(hall.max_houses, 8);
        hall.upgrade_level = 0;
        hall.can_be_upgraded = true;
        hall.upgrade_to = "Hall_rain_gaia";
    }
    string templeId = $"temple_{gaiaciv}";
    BuildingAsset temple = AssetManager.buildings.get(templeId);
    if (temple != null)
    {
        temple.cost = new ConstructionCost(1, 0, 0, 0);
        temple.upgrade_level = 0;
        temple.can_be_upgraded = true;
        temple.max_houses = 1;
        temple.storage = true;
        temple.book_slots = 10;
        temple.build_prefer_replace_house = true;
        temple.upgrade_to = "Temple_rain_gaia";
    }
    string libraryId = $"library_{gaiaciv}";
    BuildingAsset library = AssetManager.buildings.get(libraryId);
    if (library != null)
    {
        library.cost = new ConstructionCost(1, 0, 0, 0);
        library.upgrade_level = 0;
        library.can_be_upgraded = false;
        library.burnable = false;
    }
    string dockId = $"docks_{gaiaciv}";
    BuildingAsset docks = AssetManager.buildings.get(dockId);
    if (docks != null)
    {
        docks.cost = new ConstructionCost(1, 0, 0, 0);
        docks.upgrade_level = 2;
        docks.can_be_upgraded = true;
        docks.build_prefer_replace_house = true;
        docks.upgrade_to = "Docks_modern_gaia";
    }
    string watch_towerId = $"watch_tower_{gaiaciv}";
    BuildingAsset watch_tower = AssetManager.buildings.get(watch_towerId);
    if (watch_tower != null)
    {
        watch_tower.cost = new ConstructionCost(1, 0, 0, 0);
        watch_tower.upgrade_level = 1;
        watch_tower.can_be_upgraded = true;
        watch_tower.upgrade_to = "watch_tower_modern_gaia";
    }
}
BuildingAsset bonfire_gaia = AssetManager.buildings.clone("bonfire_gaia", "$city_building$");
bonfire_gaia.draw_light_area = true;
bonfire_gaia.can_be_upgraded = true;
bonfire_gaia.upgrade_to = "bonfire_rain_gaia";
bonfire_gaia.draw_light_size = 0.8f;
bonfire_gaia.can_be_abandoned = false;
bonfire_gaia.priority = 120;
bonfire_gaia.type = "type_bonfire";
bonfire_gaia.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_gaia.construction_progress_needed = 30;
bonfire_gaia.cost = new ConstructionCost();
bonfire_gaia.smoke = true;
bonfire_gaia.smoke_interval = 2.5f;
bonfire_gaia.smoke_offset = new Vector2Int(2, 3);
bonfire_gaia.can_be_living_house = false;
bonfire_gaia.build_place_batch = false;
bonfire_gaia.build_prefer_replace_house = true;
bonfire_gaia.check_for_close_building = false;
bonfire_gaia.produce_biome_food = true;
bonfire_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_gaia.check_for_adaptation_tags = false;
bonfire_gaia.max_houses = 1;
bonfire_gaia.base_stats["health"] = 1500f;
bonfire_gaia.burnable = false;
bonfire_gaia.sprite_path = "buildings/bonfire_gaia";
bonfire_gaia.has_sprites_main_disabled = false;
bonfire_gaia.has_sprites_main = true;
bonfire_gaia.has_sprites_ruin = true;
bonfire_gaia.setShadow(0.5f, 0.23f, 0.27f);
bonfire_gaia.upgrade_level = 0;
bonfire_gaia.has_sprite_construction = true;
bonfire_gaia.has_sprites_special = false;
bonfire_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_gaia);
BuildingAsset market_gaia = AssetManager.buildings.clone("market_gaia", "market_human");
market_gaia.cost = new ConstructionCost(1, 0, 0, 0);
market_gaia.upgrade_level = 0;
market_gaia.can_be_upgraded = false;
market_gaia.burnable = false;
market_gaia.sprite_path = "buildings/market_gaia";
market_gaia.has_sprites_special = false;
market_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(market_gaia);
BuildingAsset bonfire_rain_gaia = AssetManager.buildings.clone("bonfire_rain_gaia", "$city_building$");
bonfire_rain_gaia.draw_light_area = true;
bonfire_rain_gaia.can_be_upgraded = true;
bonfire_rain_gaia.upgraded_from = "bonfire_gaia";
bonfire_rain_gaia.upgrade_to = "bonfire_modern_gaia";
bonfire_rain_gaia.draw_light_size = 0.8f;
bonfire_rain_gaia.can_be_abandoned = false;
bonfire_rain_gaia.priority = 500;
bonfire_rain_gaia.type = "type_bonfire";
bonfire_rain_gaia.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_rain_gaia.construction_progress_needed = 30;
bonfire_rain_gaia.cost = new ConstructionCost(Mathf.RoundToInt(ren[0]), Mathf.RoundToInt(ren[1]), Mathf.RoundToInt(ren[2]), Mathf.RoundToInt(ren[3]));
bonfire_rain_gaia.smoke = true;
bonfire_rain_gaia.smoke_interval = 2.5f;
bonfire_rain_gaia.smoke_offset = new Vector2Int(2, 3);
bonfire_rain_gaia.can_be_living_house = false;
bonfire_rain_gaia.build_place_batch = false;
bonfire_rain_gaia.build_prefer_replace_house = true;
bonfire_rain_gaia.check_for_close_building = false;
bonfire_rain_gaia.produce_biome_food = true;
bonfire_rain_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_rain_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_rain_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_rain_gaia.check_for_adaptation_tags = false;
bonfire_rain_gaia.max_houses = 1;
bonfire_rain_gaia.base_stats["health"] = 20000f;
bonfire_rain_gaia.burnable = false;
bonfire_rain_gaia.sprite_path = "buildings/bonfire_rain";
bonfire_rain_gaia.has_sprites_main_disabled = false;
bonfire_rain_gaia.has_sprites_main = true;
bonfire_rain_gaia.has_sprites_ruin = true;
bonfire_rain_gaia.setShadow(0.5f, 0.23f, 0.27f);
bonfire_rain_gaia.upgrade_level = 1;
bonfire_rain_gaia.has_sprite_construction = true;
bonfire_rain_gaia.has_sprites_special = false;
bonfire_rain_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_rain_gaia);
BuildingAsset House_rain_gaia = AssetManager.buildings.clone("House_rain_gaia", "$building_civ_human$");
House_rain_gaia.setHousingSlots(10);
House_rain_gaia.loot_generation = 6;
House_rain_gaia.housing_happiness = 7;
House_rain_gaia.type = "type_house";
House_rain_gaia.sound_hit = "event:/SFX/HIT/HitWood";
House_rain_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
House_rain_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
House_rain_gaia.draw_light_area = true;
House_rain_gaia.draw_light_size = 0.2f;
House_rain_gaia.priority = 100;
House_rain_gaia.fundament = new BuildingFundament(1, 1, 2, 0);
House_rain_gaia.cost = new ConstructionCost(0, 0, 0, 0);
House_rain_gaia.group = "human";
House_rain_gaia.sprite_path = "buildings/House_rain_alliance";
House_rain_gaia.base_stats["health"] = 5000f;
House_rain_gaia.burnable = false;
House_rain_gaia.has_sprites_main_disabled = false;
House_rain_gaia.has_sprites_main = true;
House_rain_gaia.has_sprites_ruin = true;
House_rain_gaia.setShadow(0.5f, 0.23f, 0.27f);
House_rain_gaia.upgrade_level = 1;
House_rain_gaia.can_be_upgraded = true;
House_rain_gaia.has_sprite_construction = false;
House_rain_gaia.upgraded_from = "house_human_0";
House_rain_gaia.upgrade_to = "House_modern_gaia";
House_rain_gaia.has_sprites_special = false;
House_rain_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_rain_gaia);
BuildingAsset Barracks_rain_gaia = AssetManager.buildings.clone("Barracks_rain_gaia", "$building_civ_human$");
Barracks_rain_gaia.draw_light_area = true;
Barracks_rain_gaia.draw_light_size = 0.5f;
Barracks_rain_gaia.type = "type_barracks";
Barracks_rain_gaia.priority = 100;
Barracks_rain_gaia.fundament = new BuildingFundament(2, 2, 2, 0);
Barracks_rain_gaia.cost = new ConstructionCost(0, 0, 0, 0);
Barracks_rain_gaia.group = "human";
Barracks_rain_gaia.sprite_path = "buildings/Barracks_rain_alliance";
Barracks_rain_gaia.base_stats["health"] = 6000f;
Barracks_rain_gaia.burnable = false;
Barracks_rain_gaia.has_sprites_main_disabled = false;
Barracks_rain_gaia.has_sprites_main = true;
Barracks_rain_gaia.has_sprites_ruin = true;
Barracks_rain_gaia.setShadow(0.5f, 0.23f, 0.27f);
Barracks_rain_gaia.upgrade_level = 2;
Barracks_rain_gaia.can_be_upgraded = true;
Barracks_rain_gaia.has_sprite_construction = false;
Barracks_rain_gaia.upgraded_from = "barracks_human";
Barracks_rain_gaia.upgrade_to = "Barracks_modern_gaia";
Barracks_rain_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
Barracks_rain_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Barracks_rain_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Barracks_rain_gaia.has_sprites_special = false;
Barracks_rain_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Barracks_rain_gaia);
BuildingAsset Temple_rain_gaia = AssetManager.buildings.clone("Temple_rain_gaia", "$building_civ_human$");
Temple_rain_gaia.storage = true;
Temple_rain_gaia.book_slots = 15;
Temple_rain_gaia.draw_light_area = true;
Temple_rain_gaia.draw_light_size = 0.3f;
Temple_rain_gaia.draw_light_area_offset_y = 3f;
Temple_rain_gaia.priority = 100;
Temple_rain_gaia.type = "type_temple";
Temple_rain_gaia.fundament = new BuildingFundament(2, 2, 3, 0);
Temple_rain_gaia.cost = new ConstructionCost(0, 0, 0, 0);
Temple_rain_gaia.group = "human";
Temple_rain_gaia.max_houses = 1;
Temple_rain_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
Temple_rain_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Temple_rain_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Temple_rain_gaia.base_stats["health"] = 10000f;
Temple_rain_gaia.burnable = false;
Temple_rain_gaia.can_be_upgraded = false;
Temple_rain_gaia.sprite_path = "buildings/Temple_rain_alliance";
Temple_rain_gaia.has_sprites_main_disabled = false;
Temple_rain_gaia.has_sprites_main = true;
Temple_rain_gaia.has_sprites_ruin = true;
Temple_rain_gaia.setShadow(0.5f, 0.23f, 0.27f);
Temple_rain_gaia.upgrade_level = 1;
Temple_rain_gaia.has_sprite_construction = false;
Temple_rain_gaia.upgraded_from = "temple_human";
Temple_rain_gaia.has_sprites_special = false;
Temple_rain_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Temple_rain_gaia);
BuildingAsset Hall_rain_gaia = AssetManager.buildings.clone("Hall_rain_gaia", "$building_civ_human$");
Hall_rain_gaia.priority = 100;
Hall_rain_gaia.storage = true;
Hall_rain_gaia.type = "type_hall";
Hall_rain_gaia.fundament = new BuildingFundament(3, 3, 4, 0);
Hall_rain_gaia.can_be_upgraded = false;
Hall_rain_gaia.base_stats["health"] = 10000f;
Hall_rain_gaia.burnable = false;
Hall_rain_gaia.setHousingSlots(12);
Hall_rain_gaia.housing_happiness = 10;
Hall_rain_gaia.loot_generation = 3;
Hall_rain_gaia.ignore_other_buildings_for_upgrade = true;
Hall_rain_gaia.build_place_batch = true;
Hall_rain_gaia.max_houses = Mathf.Max(Hall_rain_gaia.max_houses, 12);
Hall_rain_gaia.produce_biome_food = true;
Hall_rain_gaia.setShadow(0.56f, 0.41f, 0.43f);
Hall_rain_gaia.draw_light_size = 0.3f;
Hall_rain_gaia.book_slots = 5;
Hall_rain_gaia.draw_light_area = true;
Hall_rain_gaia.upgraded_from = "hall_human_0";
Hall_rain_gaia.sound_hit = "event:/SFX/HIT/HitWood";
Hall_rain_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
Hall_rain_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
Hall_rain_gaia.cost = new ConstructionCost(0, 0, 0, 0);
Hall_rain_gaia.group = "human";
Hall_rain_gaia.sprite_path = "buildings/Hall_rain_alliance";
Hall_rain_gaia.has_sprites_main_disabled = false;
Hall_rain_gaia.has_sprites_main = true;
Hall_rain_gaia.has_sprites_ruin = true;
Hall_rain_gaia.setShadow(0.5f, 0.23f, 0.27f);
Hall_rain_gaia.upgrade_level = 1;
Hall_rain_gaia.has_sprite_construction = false;
Hall_rain_gaia.has_sprites_special = false;
Hall_rain_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Hall_rain_gaia);
BuildingAsset Docks_modern_gaia = AssetManager.buildings.clone("Docks_modern_gaia", "docks_human");
Docks_modern_gaia.boat_types = new string[13] { "cargo_gaia_boat", "destroyer_a_gaia_boat", "destroyer_b_gaia_boat", "carrier_gaia_boat", "submarine_gaia_boat", "fishing_gaia_boat", "abrawler_gaia_boat", "bbrawler_gaia_boat", "cbrawler_gaia_boat", "dbrawler_gaia_boat", "ebrawler_gaia_boat", "fbrawler_gaia_boat", "transporter_gaia_boat" };
Docks_modern_gaia.priority = 100;
Docks_modern_gaia.group = "human";
Docks_modern_gaia.civ_kingdom = "human";
Docks_modern_gaia.cost = new ConstructionCost(0, 0, 0, 0);
Docks_modern_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
Docks_modern_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Docks_modern_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Docks_modern_gaia.base_stats["health"] = 10000f;
Docks_modern_gaia.burnable = false;
Docks_modern_gaia.sprite_path = "buildings/Docks_modern";
Docks_modern_gaia.has_sprites_main_disabled = false;
Docks_modern_gaia.has_sprites_main = true;
Docks_modern_gaia.has_sprites_ruin = true;
Docks_modern_gaia.setShadow(0.5f, 0.23f, 0.27f);
Docks_modern_gaia.upgrade_level = 3;
Docks_modern_gaia.has_sprite_construction = false;
Docks_modern_gaia.can_be_upgraded = false;
Docks_modern_gaia.upgraded_from = "docks_human";
Docks_modern_gaia.has_sprites_special = false;
Docks_modern_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Docks_modern_gaia);
BuildingAsset bonfire_modern_gaia = AssetManager.buildings.clone("bonfire_modern_gaia", "$city_building$");
bonfire_modern_gaia.draw_light_area = true;
bonfire_modern_gaia.can_be_upgraded = true;
bonfire_modern_gaia.upgraded_from = "bonfire_rain_gaia";
bonfire_modern_gaia.cost = new ConstructionCost(Mathf.RoundToInt(era[0]), Mathf.RoundToInt(era[1]), Mathf.RoundToInt(era[2]), Mathf.RoundToInt(era[3]));
bonfire_modern_gaia.draw_light_size = 5f;
bonfire_modern_gaia.can_be_abandoned = true;
bonfire_modern_gaia.priority = 1000;
bonfire_modern_gaia.type = "type_bonfire";
bonfire_modern_gaia.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_modern_gaia.construction_progress_needed = 30;
bonfire_modern_gaia.smoke = true;
bonfire_modern_gaia.smoke_interval = 2.5f;
bonfire_modern_gaia.smoke_offset = new Vector2Int(2, 3);
bonfire_modern_gaia.can_be_living_house = false;
bonfire_modern_gaia.build_place_batch = false;
bonfire_modern_gaia.build_prefer_replace_house = true;
bonfire_modern_gaia.check_for_close_building = false;
bonfire_modern_gaia.produce_biome_food = true;
bonfire_modern_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_modern_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_modern_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_modern_gaia.check_for_adaptation_tags = false;
bonfire_modern_gaia.max_houses = 1;
bonfire_modern_gaia.base_stats["health"] = 40000f;
bonfire_modern_gaia.burnable = false;
bonfire_modern_gaia.sprite_path = "buildings/bonfire_modern";
bonfire_modern_gaia.has_sprites_main_disabled = false;
bonfire_modern_gaia.has_sprites_main = true;
bonfire_modern_gaia.has_sprites_ruin = true;
bonfire_modern_gaia.setShadow(0.5f, 0.23f, 0.27f);
bonfire_modern_gaia.upgrade_level = 2;
bonfire_modern_gaia.upgrade_to = "bonfire_future_gaia";
bonfire_modern_gaia.has_sprite_construction = true;
bonfire_modern_gaia.has_sprites_special = false;
bonfire_modern_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_modern_gaia);
BuildingAsset bonfire_future_gaia = AssetManager.buildings.clone("bonfire_future_gaia", "$city_building$");
bonfire_future_gaia.draw_light_area = true;
bonfire_future_gaia.can_be_upgraded = false;
bonfire_future_gaia.upgraded_from = "bonfire_modern_gaia";
bonfire_future_gaia.cost = new ConstructionCost(Mathf.RoundToInt(fuckmaxim[0]), Mathf.RoundToInt(fuckmaxim[1]), Mathf.RoundToInt(fuckmaxim[2]), Mathf.RoundToInt(fuckmaxim[3]));
bonfire_future_gaia.draw_light_size = 5f;
bonfire_future_gaia.can_be_abandoned = true;
bonfire_future_gaia.priority = 1000;
bonfire_future_gaia.type = "type_bonfire";
bonfire_future_gaia.fundament = new BuildingFundament(2, 2, 2, 0);
bonfire_future_gaia.construction_progress_needed = 30;
bonfire_future_gaia.smoke = true;
bonfire_future_gaia.smoke_interval = 2.5f;
bonfire_future_gaia.smoke_offset = new Vector2Int(2, 3);
bonfire_future_gaia.can_be_living_house = false;
bonfire_future_gaia.build_place_batch = false;
bonfire_future_gaia.build_prefer_replace_house = true;
bonfire_future_gaia.check_for_close_building = false;
bonfire_future_gaia.produce_biome_food = true;
bonfire_future_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
bonfire_future_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
bonfire_future_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
bonfire_future_gaia.check_for_adaptation_tags = false;
bonfire_future_gaia.max_houses = 1;
bonfire_future_gaia.base_stats["health"] = 40000f;
bonfire_future_gaia.burnable = false;
bonfire_future_gaia.sprite_path = "buildings/bonfire_future";
bonfire_future_gaia.has_sprites_main_disabled = false;
bonfire_future_gaia.has_sprites_main = true;
bonfire_future_gaia.has_sprites_ruin = true;
bonfire_future_gaia.setShadow(0.5f, 0.23f, 0.27f);
bonfire_future_gaia.upgrade_level = 3;
bonfire_future_gaia.has_sprite_construction = true;
bonfire_future_gaia.has_sprites_special = false;
bonfire_future_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(bonfire_future_gaia);
BuildingAsset House_modern_gaia = AssetManager.buildings.clone("House_modern_gaia", "$building_civ_human$");
House_modern_gaia.setHousingSlots(20);
House_modern_gaia.loot_generation = 6;
House_modern_gaia.housing_happiness = 7;
House_modern_gaia.type = "type_house";
House_modern_gaia.sound_hit = "event:/SFX/HIT/HitWood";
House_modern_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
House_modern_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
House_modern_gaia.draw_light_area = true;
House_modern_gaia.draw_light_size = 0.2f;
House_modern_gaia.priority = 100;
House_modern_gaia.fundament = new BuildingFundament(1, 1, 2, 0);
House_modern_gaia.cost = new ConstructionCost(0, 0, 0, 0);
House_modern_gaia.group = "human";
House_modern_gaia.sprite_path = "buildings/House_modern";
House_modern_gaia.base_stats["health"] = 6000f;
House_modern_gaia.burnable = false;
House_modern_gaia.has_sprites_main_disabled = false;
House_modern_gaia.has_sprites_main = true;
House_modern_gaia.has_sprites_ruin = true;
House_modern_gaia.setShadow(0.5f, 0.23f, 0.27f);
House_modern_gaia.upgrade_level = 2;
House_modern_gaia.can_be_upgraded = true;
House_modern_gaia.has_sprite_construction = false;
House_modern_gaia.upgraded_from = "House_rain_gaia";
House_modern_gaia.upgrade_to = "House_future_gaia";
House_modern_gaia.has_sprites_special = false;
House_modern_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_modern_gaia);
BuildingAsset House_future_gaia = AssetManager.buildings.clone("House_future_gaia", "$building_civ_human$");
House_future_gaia.setHousingSlots(20);
House_future_gaia.loot_generation = 6;
House_future_gaia.housing_happiness = 7;
House_future_gaia.type = "type_house";
House_future_gaia.sound_hit = "event:/SFX/HIT/HitWood";
House_future_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
House_future_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
House_future_gaia.draw_light_area = true;
House_future_gaia.draw_light_size = 0.2f;
House_future_gaia.priority = 10000;
House_future_gaia.fundament = new BuildingFundament(1, 1, 2, 0);
House_future_gaia.cost = new ConstructionCost(0, 0, 0, 0);
House_future_gaia.group = "human";
House_future_gaia.sprite_path = "buildings/House_future";
House_future_gaia.base_stats["health"] = 6000f;
House_future_gaia.base_stats["scale"] = 1.4f;
House_future_gaia.burnable = false;
House_future_gaia.has_sprites_main_disabled = false;
House_future_gaia.has_sprites_main = true;
House_future_gaia.has_sprites_ruin = true;
House_future_gaia.setShadow(0.5f, 0.23f, 0.27f);
House_future_gaia.upgrade_level = 3;
House_future_gaia.can_be_upgraded = false;
House_future_gaia.has_sprite_construction = false;
House_future_gaia.upgraded_from = "House_modern_gaia";
House_future_gaia.has_sprites_special = false;
House_future_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(House_future_gaia);
BuildingAsset Barracks_modern_gaia = AssetManager.buildings.clone("Barracks_modern_gaia", "$building_civ_human$");
Barracks_modern_gaia.draw_light_area = true;
Barracks_modern_gaia.draw_light_size = 0.5f;
Barracks_modern_gaia.type = "type_barracks";
Barracks_modern_gaia.priority = 100;
Barracks_modern_gaia.fundament = new BuildingFundament(2, 2, 2, 0);
Barracks_modern_gaia.cost = new ConstructionCost(0, 0, 0, 0);
Barracks_modern_gaia.group = "human";
Barracks_modern_gaia.sprite_path = "buildings/Barracks_modern";
Barracks_modern_gaia.base_stats["health"] = 10000f;
Barracks_modern_gaia.burnable = false;
Barracks_modern_gaia.has_sprites_main_disabled = false;
Barracks_modern_gaia.has_sprites_main = true;
Barracks_modern_gaia.has_sprites_ruin = true;
Barracks_modern_gaia.setShadow(0.5f, 0.23f, 0.27f);
Barracks_modern_gaia.upgrade_level = 3;
Barracks_modern_gaia.can_be_upgraded = false;
Barracks_modern_gaia.has_sprite_construction = false;
Barracks_modern_gaia.upgraded_from = "Barracks_rain_gaia";
Barracks_modern_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
Barracks_modern_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
Barracks_modern_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
Barracks_modern_gaia.has_sprites_special = false;
Barracks_modern_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(Barracks_modern_gaia);
BuildingAsset watch_tower_modern_gaia = AssetManager.buildings.clone("watch_tower_modern_gaia", "$building_civ_human$");
watch_tower_modern_gaia.upgrade_level = 2;
watch_tower_modern_gaia.can_be_upgraded = false;
watch_tower_modern_gaia.upgraded_from = "watch_tower_human";
watch_tower_modern_gaia.has_sprite_construction = false;
watch_tower_modern_gaia.burnable = false;
watch_tower_modern_gaia.has_sprites_main_disabled = false;
watch_tower_modern_gaia.has_sprites_main = true;
watch_tower_modern_gaia.has_sprites_ruin = true;
watch_tower_modern_gaia.draw_light_area = true;
watch_tower_modern_gaia.draw_light_size = 1f;
watch_tower_modern_gaia.build_road_to = false;
watch_tower_modern_gaia.base_stats["health"] = 10000f;
watch_tower_modern_gaia.base_stats["targets"] = 1f;
watch_tower_modern_gaia.base_stats["area_of_effect"] = 1f;
watch_tower_modern_gaia.base_stats["damage"] = 20f;
watch_tower_modern_gaia.base_stats["knockback"] = 1f;
watch_tower_modern_gaia.smoke = true;
watch_tower_modern_gaia.smoke_interval = 2.5f;
watch_tower_modern_gaia.smoke_offset = new Vector2Int(2, 3);
watch_tower_modern_gaia.priority = 100;
watch_tower_modern_gaia.type = "type_watch_tower";
watch_tower_modern_gaia.fundament = new BuildingFundament(1, 1, 1, 0);
watch_tower_modern_gaia.cost = new ConstructionCost(0, 0, 0, 0);
watch_tower_modern_gaia.tower = true;
watch_tower_modern_gaia.sprite_path = "buildings/watch_tower_modern";
watch_tower_modern_gaia.tower_projectile = "arrow";
watch_tower_modern_gaia.tower_projectile_offset = 4f;
watch_tower_modern_gaia.tower_projectile_amount = 1;
watch_tower_modern_gaia.tower_projectile_reload = 0f;
watch_tower_modern_gaia.build_place_borders = true;
watch_tower_modern_gaia.build_place_batch = false;
watch_tower_modern_gaia.build_place_single = true;
watch_tower_modern_gaia.setShadow(0.5f, 0.23f, 0.27f);
watch_tower_modern_gaia.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
watch_tower_modern_gaia.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
watch_tower_modern_gaia.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
watch_tower_modern_gaia.has_sprites_special = false;
watch_tower_modern_gaia.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
AssetManager.buildings.add(watch_tower_modern_gaia);
CityBuildOrderAsset gaiaBuild = new CityBuildOrderAsset();
gaiaBuild.id = "build_order_gaia_epochs";
BuildOrder gaiaorder;
gaiaorder = gaiaBuild.addBuilding("order_bonfire_gaia", 1);
gaiaBuild.addUpgrade("order_bonfire_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaBuild.addUpgrade("order_bonfire_rain_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaBuild.addUpgrade("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaBuild.addUpgrade("order_bonfire_future_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaorder = gaiaBuild.addBuilding("order_stockpile", 1);
gaiaorder = gaiaBuild.addBuilding("order_hall_0", 1);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
gaiaBuild.addUpgrade("order_hall_0");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire");
gaiaBuild.addUpgrade("order_house_0");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_gaia");
gaiaBuild.addUpgrade("order_house_rain_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaBuild.addUpgrade("order_house_modern_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaBuild.addUpgrade("order_house_future_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_future_gaia");
gaiaorder = gaiaBuild.addBuilding("order_watch_tower", 0, 0, 0);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
gaiaBuild.addUpgrade("order_watch_tower");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.addBuilding("order_temple", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
gaiaBuild.addUpgrade("order_temple");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.addBuilding("order_mine", 1, 10, 10);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaBuild.addUpgrade("order_mine");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.addBuilding("order_docks_0", 1, 0, 0);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
gaiaBuild.addUpgrade("order_docks_0");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaBuild.addUpgrade("order_docks_1");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.addBuilding("order_windmill_0", 1, 0, 0);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
gaiaorder = gaiaBuild.addBuilding("order_barracks", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
gaiaBuild.addUpgrade("order_barracks");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_gaia");
gaiaBuild.addUpgrade("order_barracks_rain_gaia");
gaiaorder = gaiaBuild.list.Last();
gaiaorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_gaia");
gaiaorder = gaiaBuild.addBuilding("order_library", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
gaiaorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
AssetManager.city_build_orders.add(gaiaBuild);




string[] hordecivs = new string[] { "orc", "necromancer", "civ_fox", "civ_wolf", "civ_bear", "civ_hyena", "civ_rat", "civ_scorpion", "civ_crocodile", "civ_snake", "civ_piranha", "greg", "jumpy_skull" };
    foreach (string hordeciv in hordecivs)
    {
        string barracksId = $"barracks_{hordeciv}";
        BuildingAsset barracks = AssetManager.buildings.get(barracksId);
        if (barracks != null)
        {
            barracks.priority = 100;
            barracks.fundament = new BuildingFundament(3, 3, 4, 0);
            barracks.cost = new ConstructionCost(1, 0, 0, 0);
            barracks.upgrade_level = 1;
            barracks.build_prefer_replace_house = true;
            barracks.can_be_upgraded = true;
            barracks.upgrade_to = "Barracks_rain_horde";
        }
        string houseId = $"house_{hordeciv}_0";
        BuildingAsset house = AssetManager.buildings.get(houseId);
        if (house != null)
        {
            house.cost = new ConstructionCost(1, 0, 0, 0);
            house.upgrade_level = 0;
            house.can_be_upgraded = true;
            house.upgrade_to = "House_rain_horde";
        }
        string hallId = $"hall_{hordeciv}_0";
        BuildingAsset hall = AssetManager.buildings.get(hallId);
        if (hall != null)
        {
            hall.cost = new ConstructionCost(1, 0, 0, 0);
            hall.max_houses = Mathf.Max(hall.max_houses, 8);
            hall.upgrade_level = 0;
            hall.can_be_upgraded = true;
            hall.upgrade_to = "Hall_rain_horde";
        }
        string templeId = $"temple_{hordeciv}";
        BuildingAsset temple = AssetManager.buildings.get(templeId);
        if (temple != null)
        {
            temple.cost = new ConstructionCost(1, 0, 0, 0);
            temple.upgrade_level = 0;
            temple.can_be_upgraded = true;
            temple.max_houses = 1;
            temple.storage = true;
            temple.book_slots = 10;
            temple.build_prefer_replace_house = true;
            temple.upgrade_to = "Temple_rain_horde";
        }
        string libraryId = $"library_{hordeciv}";
        BuildingAsset library = AssetManager.buildings.get(libraryId);
        if (library != null)
        {
            library.cost = new ConstructionCost(1, 0, 0, 0);
            library.upgrade_level = 0;
            library.can_be_upgraded = false;
            library.burnable = false;
        }
        string dockId = $"docks_{hordeciv}";
        BuildingAsset docks = AssetManager.buildings.get(dockId);
        if (docks != null)
        {
            docks.cost = new ConstructionCost(1, 0, 0, 0);
            docks.upgrade_level = 2;
            docks.can_be_upgraded = true;
            docks.build_prefer_replace_house = true;
            docks.upgrade_to = "Docks_modern_horde";
        }
        string watch_towerId = $"watch_tower_{hordeciv}";
        BuildingAsset watch_tower = AssetManager.buildings.get(watch_towerId);
        if (watch_tower != null)
        {
            watch_tower.cost = new ConstructionCost(1, 0, 0, 0);
            watch_tower.upgrade_level = 1;
            watch_tower.can_be_upgraded = true;
            watch_tower.upgrade_to = "watch_tower_modern_horde";
        }
    }
    BuildingAsset bonfire_horde = AssetManager.buildings.clone("bonfire_horde", "$city_building$");
    bonfire_horde.draw_light_area = true;
    bonfire_horde.can_be_upgraded = true;
    bonfire_horde.upgrade_to = "bonfire_rain_horde";
    bonfire_horde.draw_light_size = 0.8f;
    bonfire_horde.can_be_abandoned = false;
    bonfire_horde.priority = 120;
    bonfire_horde.type = "type_bonfire";
    bonfire_horde.fundament = new BuildingFundament(2, 2, 2, 0);
    bonfire_horde.construction_progress_needed = 30;
    bonfire_horde.cost = new ConstructionCost();
    bonfire_horde.smoke = true;
    bonfire_horde.smoke_interval = 2.5f;
    bonfire_horde.smoke_offset = new Vector2Int(2, 3);
    bonfire_horde.can_be_living_house = false;
    bonfire_horde.build_place_batch = false;
    bonfire_horde.build_prefer_replace_house = true;
    bonfire_horde.check_for_close_building = false;
    bonfire_horde.produce_biome_food = true;
    bonfire_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
    bonfire_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    bonfire_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
    bonfire_horde.check_for_adaptation_tags = false;
    bonfire_horde.max_houses = 1;
    bonfire_horde.base_stats["health"] = 1500f;
    bonfire_horde.burnable = false;
    bonfire_horde.sprite_path = "buildings/bonfire_horde";
    bonfire_horde.has_sprites_main_disabled = false;
    bonfire_horde.has_sprites_main = true;
    bonfire_horde.has_sprites_ruin = true;
    bonfire_horde.setShadow(0.5f, 0.23f, 0.27f);
    bonfire_horde.upgrade_level = 0;
    bonfire_horde.has_sprite_construction = true;
    bonfire_horde.has_sprites_special = false;
    bonfire_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(bonfire_horde);
    BuildingAsset market_horde = AssetManager.buildings.clone("market_horde", "market_human");
    market_horde.cost = new ConstructionCost(1, 0, 0, 0);
    market_horde.upgrade_level = 0;
    market_horde.can_be_upgraded = false;
    market_horde.burnable = false;
    market_horde.sprite_path = "buildings/market_horde";
    market_horde.has_sprites_special = false;
    market_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(market_horde);
    BuildingAsset bonfire_rain_horde = AssetManager.buildings.clone("bonfire_rain_horde", "$city_building$");
    bonfire_rain_horde.draw_light_area = true;
    bonfire_rain_horde.can_be_upgraded = true;
    bonfire_rain_horde.upgraded_from = "bonfire_horde";
    bonfire_rain_horde.upgrade_to = "bonfire_modern_horde";
    bonfire_rain_horde.draw_light_size = 0.8f;
    bonfire_rain_horde.can_be_abandoned = false;
    bonfire_rain_horde.priority = 500;
    bonfire_rain_horde.type = "type_bonfire";
    bonfire_rain_horde.fundament = new BuildingFundament(2, 2, 2, 0);
    bonfire_rain_horde.construction_progress_needed = 30;
    bonfire_rain_horde.cost = new ConstructionCost(Mathf.RoundToInt(ren[0]), Mathf.RoundToInt(ren[1]), Mathf.RoundToInt(ren[2]), Mathf.RoundToInt(ren[3]));
    bonfire_rain_horde.smoke = true;
    bonfire_rain_horde.smoke_interval = 2.5f;
    bonfire_rain_horde.smoke_offset = new Vector2Int(2, 3);
    bonfire_rain_horde.can_be_living_house = false;
    bonfire_rain_horde.build_place_batch = false;
    bonfire_rain_horde.build_prefer_replace_house = true;
    bonfire_rain_horde.check_for_close_building = false;
    bonfire_rain_horde.produce_biome_food = true;
    bonfire_rain_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
    bonfire_rain_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    bonfire_rain_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
    bonfire_rain_horde.check_for_adaptation_tags = false;
    bonfire_rain_horde.max_houses = 1;
    bonfire_rain_horde.base_stats["health"] = 20000f;
    bonfire_rain_horde.burnable = false;
    bonfire_rain_horde.sprite_path = "buildings/bonfire_rain";
    bonfire_rain_horde.has_sprites_main_disabled = false;
    bonfire_rain_horde.has_sprites_main = true;
    bonfire_rain_horde.has_sprites_ruin = true;
    bonfire_rain_horde.setShadow(0.5f, 0.23f, 0.27f);
    bonfire_rain_horde.upgrade_level = 1;
    bonfire_rain_horde.has_sprite_construction = true;
    bonfire_rain_horde.has_sprites_special = false;
    bonfire_rain_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(bonfire_rain_horde);
    BuildingAsset House_rain_horde = AssetManager.buildings.clone("House_rain_horde", "$building_civ_human$");
    House_rain_horde.setHousingSlots(10);
    House_rain_horde.loot_generation = 6;
    House_rain_horde.housing_happiness = 7;
    House_rain_horde.type = "type_house";
    House_rain_horde.sound_hit = "event:/SFX/HIT/HitWood";
    House_rain_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    House_rain_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
    House_rain_horde.draw_light_area = true;
    House_rain_horde.draw_light_size = 0.2f;
    House_rain_horde.priority = 100;
    House_rain_horde.fundament = new BuildingFundament(1, 1, 2, 0);
    House_rain_horde.cost = new ConstructionCost(0, 0, 0, 0);
    House_rain_horde.group = "human";
    House_rain_horde.sprite_path = "buildings/House_rain_alliance";
    House_rain_horde.base_stats["health"] = 5000f;
    House_rain_horde.burnable = false;
    House_rain_horde.has_sprites_main_disabled = false;
    House_rain_horde.has_sprites_main = true;
    House_rain_horde.has_sprites_ruin = true;
    House_rain_horde.setShadow(0.5f, 0.23f, 0.27f);
    House_rain_horde.upgrade_level = 1;
    House_rain_horde.can_be_upgraded = true;
    House_rain_horde.has_sprite_construction = false;
    House_rain_horde.upgraded_from = "house_human_0";
    House_rain_horde.upgrade_to = "House_modern_horde";
    House_rain_horde.has_sprites_special = false;
    House_rain_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(House_rain_horde);
    BuildingAsset Barracks_rain_horde = AssetManager.buildings.clone("Barracks_rain_horde", "$building_civ_human$");
    Barracks_rain_horde.draw_light_area = true;
    Barracks_rain_horde.draw_light_size = 0.5f;
    Barracks_rain_horde.type = "type_barracks";
    Barracks_rain_horde.priority = 100;
    Barracks_rain_horde.fundament = new BuildingFundament(2, 2, 2, 0);
    Barracks_rain_horde.cost = new ConstructionCost(0, 0, 0, 0);
    Barracks_rain_horde.group = "human";
    Barracks_rain_horde.sprite_path = "buildings/Barracks_rain_alliance";
    Barracks_rain_horde.base_stats["health"] = 6000f;
    Barracks_rain_horde.burnable = false;
    Barracks_rain_horde.has_sprites_main_disabled = false;
    Barracks_rain_horde.has_sprites_main = true;
    Barracks_rain_horde.has_sprites_ruin = true;
    Barracks_rain_horde.setShadow(0.5f, 0.23f, 0.27f);
    Barracks_rain_horde.upgrade_level = 2;
    Barracks_rain_horde.can_be_upgraded = true;
    Barracks_rain_horde.has_sprite_construction = false;
    Barracks_rain_horde.upgraded_from = "barracks_human";
    Barracks_rain_horde.upgrade_to = "Barracks_modern_horde";
    Barracks_rain_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
    Barracks_rain_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
    Barracks_rain_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
    Barracks_rain_horde.has_sprites_special = false;
    Barracks_rain_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(Barracks_rain_horde);
    BuildingAsset Temple_rain_horde = AssetManager.buildings.clone("Temple_rain_horde", "$building_civ_human$");
    Temple_rain_horde.storage = true;
    Temple_rain_horde.book_slots = 15;
    Temple_rain_horde.draw_light_area = true;
    Temple_rain_horde.draw_light_size = 0.3f;
    Temple_rain_horde.draw_light_area_offset_y = 3f;
    Temple_rain_horde.priority = 100;
    Temple_rain_horde.type = "type_temple";
    Temple_rain_horde.fundament = new BuildingFundament(2, 2, 3, 0);
    Temple_rain_horde.cost = new ConstructionCost(0, 0, 0, 0);
    Temple_rain_horde.group = "human";
    Temple_rain_horde.max_houses = 1;
    Temple_rain_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
    Temple_rain_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
    Temple_rain_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
    Temple_rain_horde.base_stats["health"] = 10000f;
    Temple_rain_horde.burnable = false;
    Temple_rain_horde.can_be_upgraded = false;
    Temple_rain_horde.sprite_path = "buildings/Temple_rain_alliance";
    Temple_rain_horde.has_sprites_main_disabled = false;
    Temple_rain_horde.has_sprites_main = true;
    Temple_rain_horde.has_sprites_ruin = true;
    Temple_rain_horde.setShadow(0.5f, 0.23f, 0.27f);
    Temple_rain_horde.upgrade_level = 1;
    Temple_rain_horde.has_sprite_construction = false;
    Temple_rain_horde.upgraded_from = "temple_human";
    Temple_rain_horde.has_sprites_special = false;
    Temple_rain_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(Temple_rain_horde);
    BuildingAsset Hall_rain_horde = AssetManager.buildings.clone("Hall_rain_horde", "$building_civ_human$");
    Hall_rain_horde.priority = 100;
    Hall_rain_horde.storage = true;
    Hall_rain_horde.type = "type_hall";
    Hall_rain_horde.fundament = new BuildingFundament(3, 3, 4, 0);
    Hall_rain_horde.can_be_upgraded = false;
    Hall_rain_horde.base_stats["health"] = 10000f;
    Hall_rain_horde.burnable = false;
    Hall_rain_horde.setHousingSlots(12);
    Hall_rain_horde.housing_happiness = 10;
    Hall_rain_horde.loot_generation = 3;
    Hall_rain_horde.ignore_other_buildings_for_upgrade = true;
    Hall_rain_horde.build_place_batch = true;
    Hall_rain_horde.max_houses = Mathf.Max(Hall_rain_horde.max_houses, 12);
    Hall_rain_horde.produce_biome_food = true;
    Hall_rain_horde.setShadow(0.56f, 0.41f, 0.43f);
    Hall_rain_horde.draw_light_size = 0.3f;
    Hall_rain_horde.book_slots = 5;
    Hall_rain_horde.draw_light_area = true;
    Hall_rain_horde.upgraded_from = "hall_human_0";
    Hall_rain_horde.sound_hit = "event:/SFX/HIT/HitWood";
    Hall_rain_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    Hall_rain_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
    Hall_rain_horde.cost = new ConstructionCost(0, 0, 0, 0);
    Hall_rain_horde.group = "human";
    Hall_rain_horde.sprite_path = "buildings/Hall_rain_alliance";
    Hall_rain_horde.has_sprites_main_disabled = false;
    Hall_rain_horde.has_sprites_main = true;
    Hall_rain_horde.has_sprites_ruin = true;
    Hall_rain_horde.setShadow(0.5f, 0.23f, 0.27f);
    Hall_rain_horde.upgrade_level = 1;
    Hall_rain_horde.has_sprite_construction = false;
    Hall_rain_horde.has_sprites_special = false;
    Hall_rain_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(Hall_rain_horde);
    BuildingAsset Docks_modern_horde = AssetManager.buildings.clone("Docks_modern_horde", "docks_human");
    Docks_modern_horde.boat_types = new string[13] { "cargo_horde_boat", "destroyer_a_horde_boat", "destroyer_b_horde_boat", "carrier_horde_boat", "submarine_horde_boat", "fishing_horde_boat", "abrawler_horde_boat", "bbrawler_horde_boat", "cbrawler_horde_boat", "dbrawler_horde_boat", "ebrawler_horde_boat", "fbrawler_horde_boat", "transporter_horde_boat" };
    Docks_modern_horde.priority = 100;
    Docks_modern_horde.group = "human";
    Docks_modern_horde.civ_kingdom = "human";
    Docks_modern_horde.cost = new ConstructionCost(0, 0, 0, 0);
    Docks_modern_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
    Docks_modern_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
    Docks_modern_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
    Docks_modern_horde.base_stats["health"] = 10000f;
    Docks_modern_horde.burnable = false;
    Docks_modern_horde.sprite_path = "buildings/Docks_modern";
    Docks_modern_horde.has_sprites_main_disabled = false;
    Docks_modern_horde.has_sprites_main = true;
    Docks_modern_horde.has_sprites_ruin = true;
    Docks_modern_horde.setShadow(0.5f, 0.23f, 0.27f);
    Docks_modern_horde.upgrade_level = 3;
    Docks_modern_horde.has_sprite_construction = false;
    Docks_modern_horde.can_be_upgraded = false;
    Docks_modern_horde.upgraded_from = "docks_human";
    Docks_modern_horde.has_sprites_special = false;
    Docks_modern_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(Docks_modern_horde);
    BuildingAsset bonfire_modern_horde = AssetManager.buildings.clone("bonfire_modern_horde", "$city_building$");
    bonfire_modern_horde.draw_light_area = true;
    bonfire_modern_horde.can_be_upgraded = true;
    bonfire_modern_horde.upgraded_from = "bonfire_rain_horde";
    bonfire_modern_horde.cost = new ConstructionCost(Mathf.RoundToInt(era[0]), Mathf.RoundToInt(era[1]), Mathf.RoundToInt(era[2]), Mathf.RoundToInt(era[3]));
    bonfire_modern_horde.draw_light_size = 5f;
    bonfire_modern_horde.can_be_abandoned = true;
    bonfire_modern_horde.priority = 1000;
    bonfire_modern_horde.type = "type_bonfire";
    bonfire_modern_horde.fundament = new BuildingFundament(2, 2, 2, 0);
    bonfire_modern_horde.construction_progress_needed = 30;
    bonfire_modern_horde.smoke = true;
    bonfire_modern_horde.smoke_interval = 2.5f;
    bonfire_modern_horde.smoke_offset = new Vector2Int(2, 3);
    bonfire_modern_horde.can_be_living_house = false;
    bonfire_modern_horde.build_place_batch = false;
    bonfire_modern_horde.build_prefer_replace_house = true;
    bonfire_modern_horde.check_for_close_building = false;
    bonfire_modern_horde.produce_biome_food = true;
    bonfire_modern_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
    bonfire_modern_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    bonfire_modern_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
    bonfire_modern_horde.check_for_adaptation_tags = false;
    bonfire_modern_horde.max_houses = 1;
    bonfire_modern_horde.base_stats["health"] = 40000f;
    bonfire_modern_horde.burnable = false;
    bonfire_modern_horde.sprite_path = "buildings/bonfire_modern";
    bonfire_modern_horde.has_sprites_main_disabled = false;
    bonfire_modern_horde.has_sprites_main = true;
    bonfire_modern_horde.has_sprites_ruin = true;
    bonfire_modern_horde.setShadow(0.5f, 0.23f, 0.27f);
    bonfire_modern_horde.upgrade_level = 2;
    bonfire_modern_horde.upgrade_to = "bonfire_future_horde";
    bonfire_modern_horde.has_sprite_construction = true;
    bonfire_modern_horde.has_sprites_special = false;
    bonfire_modern_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(bonfire_modern_horde);
    BuildingAsset bonfire_future_horde = AssetManager.buildings.clone("bonfire_future_horde", "$city_building$");
    bonfire_future_horde.draw_light_area = true;
    bonfire_future_horde.can_be_upgraded = false;
    bonfire_future_horde.upgraded_from = "bonfire_modern_horde";
    bonfire_future_horde.cost = new ConstructionCost(Mathf.RoundToInt(fuckmaxim[0]), Mathf.RoundToInt(fuckmaxim[1]), Mathf.RoundToInt(fuckmaxim[2]), Mathf.RoundToInt(fuckmaxim[3]));
    bonfire_future_horde.draw_light_size = 5f;
    bonfire_future_horde.can_be_abandoned = true;
    bonfire_future_horde.priority = 1000;
    bonfire_future_horde.type = "type_bonfire";
    bonfire_future_horde.fundament = new BuildingFundament(2, 2, 2, 0);
    bonfire_future_horde.construction_progress_needed = 30;
    bonfire_future_horde.smoke = true;
    bonfire_future_horde.smoke_interval = 2.5f;
    bonfire_future_horde.smoke_offset = new Vector2Int(2, 3);
    bonfire_future_horde.can_be_living_house = false;
    bonfire_future_horde.build_place_batch = false;
    bonfire_future_horde.build_prefer_replace_house = true;
    bonfire_future_horde.check_for_close_building = false;
    bonfire_future_horde.produce_biome_food = true;
    bonfire_future_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
    bonfire_future_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    bonfire_future_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
    bonfire_future_horde.check_for_adaptation_tags = false;
    bonfire_future_horde.max_houses = 1;
    bonfire_future_horde.base_stats["health"] = 40000f;
    bonfire_future_horde.burnable = false;
    bonfire_future_horde.sprite_path = "buildings/bonfire_future";
    bonfire_future_horde.has_sprites_main_disabled = false;
    bonfire_future_horde.has_sprites_main = true;
    bonfire_future_horde.has_sprites_ruin = true;
    bonfire_future_horde.setShadow(0.5f, 0.23f, 0.27f);
    bonfire_future_horde.upgrade_level = 3;
    bonfire_future_horde.has_sprite_construction = true;
    bonfire_future_horde.has_sprites_special = false;
    bonfire_future_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(bonfire_future_horde);
    BuildingAsset House_modern_horde = AssetManager.buildings.clone("House_modern_horde", "$building_civ_human$");
    House_modern_horde.setHousingSlots(20);
    House_modern_horde.loot_generation = 6;
    House_modern_horde.housing_happiness = 7;
    House_modern_horde.type = "type_house";
    House_modern_horde.sound_hit = "event:/SFX/HIT/HitWood";
    House_modern_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    House_modern_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
    House_modern_horde.draw_light_area = true;
    House_modern_horde.draw_light_size = 0.2f;
    House_modern_horde.priority = 100;
    House_modern_horde.fundament = new BuildingFundament(1, 1, 2, 0);
    House_modern_horde.cost = new ConstructionCost(0, 0, 0, 0);
    House_modern_horde.group = "human";
    House_modern_horde.sprite_path = "buildings/House_modern";
    House_modern_horde.base_stats["health"] = 6000f;
    House_modern_horde.burnable = false;
    House_modern_horde.has_sprites_main_disabled = false;
    House_modern_horde.has_sprites_main = true;
    House_modern_horde.has_sprites_ruin = true;
    House_modern_horde.setShadow(0.5f, 0.23f, 0.27f);
    House_modern_horde.upgrade_level = 2;
    House_modern_horde.can_be_upgraded = true;
    House_modern_horde.has_sprite_construction = false;
    House_modern_horde.upgraded_from = "House_rain_horde";
    House_modern_horde.upgrade_to = "House_future_horde";
    House_modern_horde.has_sprites_special = false;
    House_modern_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(House_modern_horde);
    BuildingAsset House_future_horde = AssetManager.buildings.clone("House_future_horde", "$building_civ_human$");
    House_future_horde.setHousingSlots(20);
    House_future_horde.loot_generation = 6;
    House_future_horde.housing_happiness = 7;
    House_future_horde.type = "type_house";
    House_future_horde.sound_hit = "event:/SFX/HIT/HitWood";
    House_future_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
    House_future_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
    House_future_horde.draw_light_area = true;
    House_future_horde.draw_light_size = 0.2f;
    House_future_horde.priority = 10000;
    House_future_horde.fundament = new BuildingFundament(1, 1, 2, 0);
    House_future_horde.cost = new ConstructionCost(0, 0, 0, 0);
    House_future_horde.group = "human";
    House_future_horde.sprite_path = "buildings/House_future";
    House_future_horde.base_stats["health"] = 6000f;
    House_future_horde.base_stats["scale"] = 1.4f;
    House_future_horde.burnable = false;
    House_future_horde.has_sprites_main_disabled = false;
    House_future_horde.has_sprites_main = true;
    House_future_horde.has_sprites_ruin = true;
    House_future_horde.setShadow(0.5f, 0.23f, 0.27f);
    House_future_horde.upgrade_level = 3;
    House_future_horde.can_be_upgraded = false;
    House_future_horde.has_sprite_construction = false;
    House_future_horde.upgraded_from = "House_modern_horde";
    House_future_horde.has_sprites_special = false;
    House_future_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(House_future_horde);
    BuildingAsset Barracks_modern_horde = AssetManager.buildings.clone("Barracks_modern_horde", "$building_civ_human$");
    Barracks_modern_horde.draw_light_area = true;
    Barracks_modern_horde.draw_light_size = 0.5f;
    Barracks_modern_horde.type = "type_barracks";
    Barracks_modern_horde.priority = 100;
    Barracks_modern_horde.fundament = new BuildingFundament(2, 2, 2, 0);
    Barracks_modern_horde.cost = new ConstructionCost(0, 0, 0, 0);
    Barracks_modern_horde.group = "human";
    Barracks_modern_horde.sprite_path = "buildings/Barracks_modern";
    Barracks_modern_horde.base_stats["health"] = 10000f;
    Barracks_modern_horde.burnable = false;
    Barracks_modern_horde.has_sprites_main_disabled = false;
    Barracks_modern_horde.has_sprites_main = true;
    Barracks_modern_horde.has_sprites_ruin = true;
    Barracks_modern_horde.setShadow(0.5f, 0.23f, 0.27f);
    Barracks_modern_horde.upgrade_level = 3;
    Barracks_modern_horde.can_be_upgraded = false;
    Barracks_modern_horde.has_sprite_construction = false;
    Barracks_modern_horde.upgraded_from = "Barracks_rain_horde";
    Barracks_modern_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
    Barracks_modern_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
    Barracks_modern_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
    Barracks_modern_horde.has_sprites_special = false;
    Barracks_modern_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(Barracks_modern_horde);
    BuildingAsset watch_tower_modern_horde = AssetManager.buildings.clone("watch_tower_modern_horde", "$building_civ_human$");
    watch_tower_modern_horde.upgrade_level = 2;
    watch_tower_modern_horde.can_be_upgraded = false;
    watch_tower_modern_horde.upgraded_from = "watch_tower_human";
    watch_tower_modern_horde.has_sprite_construction = false;
    watch_tower_modern_horde.burnable = false;
    watch_tower_modern_horde.has_sprites_main_disabled = false;
    watch_tower_modern_horde.has_sprites_main = true;
    watch_tower_modern_horde.has_sprites_ruin = true;
    watch_tower_modern_horde.draw_light_area = true;
    watch_tower_modern_horde.draw_light_size = 1f;
    watch_tower_modern_horde.build_road_to = false;
    watch_tower_modern_horde.base_stats["health"] = 10000f;
    watch_tower_modern_horde.base_stats["targets"] = 1f;
    watch_tower_modern_horde.base_stats["area_of_effect"] = 1f;
    watch_tower_modern_horde.base_stats["damage"] = 20f;
    watch_tower_modern_horde.base_stats["knockback"] = 1f;
    watch_tower_modern_horde.smoke = true;
    watch_tower_modern_horde.smoke_interval = 2.5f;
    watch_tower_modern_horde.smoke_offset = new Vector2Int(2, 3);
    watch_tower_modern_horde.priority = 100;
    watch_tower_modern_horde.type = "type_watch_tower";
    watch_tower_modern_horde.fundament = new BuildingFundament(1, 1, 1, 0);
    watch_tower_modern_horde.cost = new ConstructionCost(0, 0, 0, 0);
    watch_tower_modern_horde.tower = true;
    watch_tower_modern_horde.sprite_path = "buildings/watch_tower_modern";
    watch_tower_modern_horde.tower_projectile = "arrow";
    watch_tower_modern_horde.tower_projectile_offset = 4f;
    watch_tower_modern_horde.tower_projectile_amount = 1;
    watch_tower_modern_horde.tower_projectile_reload = 0f;
    watch_tower_modern_horde.build_place_borders = true;
    watch_tower_modern_horde.build_place_batch = false;
    watch_tower_modern_horde.build_place_single = true;
    watch_tower_modern_horde.setShadow(0.5f, 0.23f, 0.27f);
    watch_tower_modern_horde.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
    watch_tower_modern_horde.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
    watch_tower_modern_horde.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
    watch_tower_modern_horde.has_sprites_special = false;
    watch_tower_modern_horde.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
    AssetManager.buildings.add(watch_tower_modern_horde);
    CityBuildOrderAsset hordeBuild = new CityBuildOrderAsset();
    hordeBuild.id = "build_order_horde_epochs";
    BuildOrder hordeorder;
    hordeorder = hordeBuild.addBuilding("order_bonfire_horde", 1);
    hordeBuild.addUpgrade("order_bonfire_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeBuild.addUpgrade("order_bonfire_rain_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeBuild.addUpgrade("order_bonfire_modern_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeBuild.addUpgrade("order_bonfire_future_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeorder = hordeBuild.addBuilding("order_stockpile", 1);
    hordeorder = hordeBuild.addBuilding("order_hall_0", 1);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
    hordeBuild.addUpgrade("order_hall_0");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeorder = hordeBuild.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire");
    hordeBuild.addUpgrade("order_house_0");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_horde");
    hordeBuild.addUpgrade("order_house_rain_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeBuild.addUpgrade("order_house_modern_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeBuild.addUpgrade("order_house_future_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_future_horde");
    hordeorder = hordeBuild.addBuilding("order_watch_tower", 0, 0, 0);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
    hordeBuild.addUpgrade("order_watch_tower");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeorder = hordeBuild.addBuilding("order_temple", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
    hordeBuild.addUpgrade("order_temple");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeorder = hordeBuild.addBuilding("order_mine", 1, 10, 10);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeBuild.addUpgrade("order_mine");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeorder = hordeBuild.addBuilding("order_docks_0", 1, 0, 0);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
    hordeBuild.addUpgrade("order_docks_0");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeBuild.addUpgrade("order_docks_1");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeorder = hordeBuild.addBuilding("order_windmill_0", 1, 0, 0);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_house");
    hordeorder = hordeBuild.addBuilding("order_barracks", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
    hordeBuild.addUpgrade("order_barracks");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_rain_horde");
    hordeBuild.addUpgrade("order_barracks_rain_horde");
    hordeorder = hordeBuild.list.Last();
    hordeorder.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire_modern_horde");
    hordeorder = hordeBuild.addBuilding("order_library", 1, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: false, 10);
    hordeorder.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
    AssetManager.city_build_orders.add(hordeBuild);





var human = AssetManager.actor_library.get("human");
human.build_order_template_id = "build_order_alliance_epochs";
var plague_doctor = AssetManager.actor_library.get("plague_doctor");
plague_doctor.build_order_template_id = "build_order_alliance_epochs";
var evil_mage = AssetManager.actor_library.get("evil_mage");
evil_mage.build_order_template_id = "build_order_alliance_epochs";
var white_mage = AssetManager.actor_library.get("white_mage");
white_mage.build_order_template_id = "build_order_alliance_epochs";
var bandit = AssetManager.actor_library.get("bandit");
bandit.build_order_template_id = "build_order_alliance_epochs";
var dwarf = AssetManager.actor_library.get("dwarf");
dwarf.build_order_template_id = "build_order_harden_epochs";
var cold_one = AssetManager.actor_library.get("cold_one");
cold_one.build_order_template_id = "build_order_harden_epochs";
var snowman = AssetManager.actor_library.get("snowman");
snowman.build_order_template_id = "build_order_harden_epochs";
var elf = AssetManager.actor_library.get("elf");
elf.build_order_template_id = "build_order_gaia_epochs";
var druid = AssetManager.actor_library.get("druid");
druid.build_order_template_id = "build_order_gaia_epochs";
var orc = AssetManager.actor_library.get("orc");
orc.build_order_template_id = "build_order_horde_epochs";
var necromancer = AssetManager.actor_library.get("necromancer");
necromancer.build_order_template_id = "build_order_horde_epochs";
var greg = AssetManager.actor_library.get("greg");
greg.build_order_template_id = "build_order_horde_epochs";

   RegisterCustomHousePlacementAliases();






   AddCustomOrdersToArchitectures();


        }
    private static void RegisterCustomHousePlacementAliases()
    {
        RegisterHousePlacementAlias("house_rain_alliance", "House_rain_alliance");
        RegisterHousePlacementAlias("house_modern_alliance", "House_modern_alliance");
        RegisterHousePlacementAlias("house_future_alliance", "House_future_alliance");

        RegisterHousePlacementAlias("house_rain_harden", "House_rain_harden");
        RegisterHousePlacementAlias("house_modern_harden", "House_modern_harden");
        RegisterHousePlacementAlias("house_future_harden", "House_future_harden");

        RegisterHousePlacementAlias("house_rain_gaia", "House_rain_gaia");
        RegisterHousePlacementAlias("house_modern_gaia", "House_modern_gaia");
        RegisterHousePlacementAlias("house_future_gaia", "House_future_gaia");

        RegisterHousePlacementAlias("house_rain_horde", "House_rain_horde");
        RegisterHousePlacementAlias("house_modern_horde", "House_modern_horde");
        RegisterHousePlacementAlias("house_future_horde", "House_future_horde");
    }

    private static void RegisterHousePlacementAlias(string aliasId, string sourceId)
    {
        if (AssetManager.buildings.get(aliasId) != null)
        {
            return;
        }

        BuildingAsset source = AssetManager.buildings.get(sourceId);
        if (source == null)
        {
            ModernBoxLogger.Warning($"[MOD] Could not create placement alias '{aliasId}' because source building '{sourceId}' was missing.");
            return;
        }

        BuildingAsset alias = AssetManager.buildings.clone(aliasId, sourceId);
        alias.type = source.type;
        alias.group = source.group;
        alias.sprite_path = source.sprite_path;
        alias.atlas_asset = source.atlas_asset;
        AssetManager.buildings.add(alias);
    }

    private static void AddCustomOrdersToArchitectures()
    {
        var customOrders = new Dictionary<string, string> {
            { "order_bonfire_alliance", "bonfire_alliance" },
            { "order_bonfire_modern_alliance", "bonfire_modern_alliance" },
            { "order_bonfire_future_alliance", "bonfire_future_alliance" },
            { "order_bonfire_rain_alliance", "bonfire_rain_alliance" },
            { "order_barracks_rain_alliance", "Barracks_rain_alliance" },
            { "order_house_rain_alliance", "House_rain_alliance" },
            { "order_house_modern_alliance", "House_modern_alliance" },
            { "order_house_future_alliance", "House_future_alliance" },
            { "order_market_alliance", "market_alliance" },
            { "order_bonfire_harden", "bonfire_harden" },
            { "order_bonfire_modern_harden", "bonfire_modern_harden" },
            { "order_bonfire_future_harden", "bonfire_future_harden" },
            { "order_bonfire_rain_harden", "bonfire_rain_harden" },
            { "order_barracks_rain_harden", "Barracks_rain_harden" },
            { "order_house_rain_harden", "House_rain_harden" },
            { "order_house_modern_harden", "House_modern_harden" },
            { "order_house_future_harden", "House_future_harden" },
            { "order_market_harden", "market_harden" },
            { "order_bonfire_gaia", "bonfire_gaia" },
            { "order_bonfire_modern_gaia", "bonfire_modern_gaia" },
            { "order_bonfire_future_gaia", "bonfire_future_gaia" },
            { "order_bonfire_rain_gaia", "bonfire_rain_gaia" },
            { "order_barracks_rain_gaia", "Barracks_rain_gaia" },
            { "order_house_rain_gaia", "House_rain_gaia" },
            { "order_house_modern_gaia", "House_modern_gaia" },
            { "order_house_future_gaia", "House_future_gaia" },
            { "order_market_gaia", "market_gaia" },
            { "order_bonfire_horde", "bonfire_horde" },
            { "order_bonfire_modern_horde", "bonfire_modern_horde" },
            { "order_bonfire_future_horde", "bonfire_future_horde" },
            { "order_bonfire_rain_horde", "bonfire_rain_horde" },
            { "order_barracks_rain_horde", "Barracks_rain_horde" },
            { "order_house_rain_horde", "House_rain_horde" },
            { "order_house_modern_horde", "House_modern_horde" },
            { "order_house_future_horde", "House_future_horde" },
            { "order_market_horde", "market_horde" }




        };
        foreach (var arch in AssetManager.architecture_library.list)
        {
            if (arch.building_ids_for_construction == null)
                arch.building_ids_for_construction = new Dictionary<string, string>();
            foreach (var kvp in customOrders)
            {
                if (!arch.building_ids_for_construction.ContainsKey(kvp.Key))
                {
                    arch.building_ids_for_construction.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }

    private static bool IsModernTowerOrDockUpgrade(Building building)
    {
        if (building == null || building.asset == null || string.IsNullOrEmpty(building.asset.upgrade_to))
        {
            return false;
        }

        string buildingType = building.asset.type;
        if (buildingType != "type_watch_tower" && buildingType != "type_docks")
        {
            return false;
        }

        string upgradeTarget = building.asset.upgrade_to;
        return upgradeTarget.IndexOf("watch_tower_modern_", StringComparison.OrdinalIgnoreCase) >= 0
            || upgradeTarget.IndexOf("docks_modern_", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool CityHasModernEraBonfire(City city)
    {
        if (city == null || city.buildings == null)
        {
            return false;
        }

        foreach (Building building in city.buildings)
        {
            if (building == null || building.asset == null || building.asset.type != "type_bonfire")
            {
                continue;
            }

            string buildingId = building.asset.id;
            if (!string.IsNullOrEmpty(buildingId)
                && (buildingId.IndexOf("bonfire_modern_", StringComparison.OrdinalIgnoreCase) >= 0
                || buildingId.IndexOf("bonfire_future_", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetUpgradeBuildOrder(City city, Building building, out BuildOrder upgradeOrder)
    {
        upgradeOrder = null;
        if (city == null || building == null || building.asset == null)
        {
            return false;
        }

        ActorAsset cityActorAsset = city.getActorAsset();
        if (cityActorAsset == null || string.IsNullOrEmpty(cityActorAsset.build_order_template_id))
        {
            return false;
        }

        CityBuildOrderAsset cityBuildOrder = AssetManager.city_build_orders.get(cityActorAsset.build_order_template_id);
        if (cityBuildOrder == null || cityBuildOrder.list == null)
        {
            return false;
        }

        foreach (BuildOrder candidate in cityBuildOrder.list)
        {
            if (candidate == null || !candidate.upgrade)
            {
                continue;
            }

            BuildingAsset candidateAsset = candidate.getBuildingAsset(city);
            if (candidateAsset == null)
            {
                continue;
            }

            if (string.Equals(candidateAsset.id, building.asset.id, StringComparison.OrdinalIgnoreCase))
            {
                upgradeOrder = candidate;
                return true;
            }
        }

        return false;
    }

[HarmonyPatch(typeof(ai.behaviours.CityBehBuild), nameof(ai.behaviours.CityBehBuild.upgradeBuilding))]
public static class Patch_CityBehBuild_BlockPrematureModernTowerDockUpgrades
{
    [HarmonyPrefix]
    public static bool Prefix(Building pBuilding, City pCity, ref bool __result)
    {
        if (pBuilding == null || pBuilding.asset == null || pCity == null)
        {
            return true;
        }

        if (TryGetUpgradeBuildOrder(pCity, pBuilding, out BuildOrder upgradeOrder))
        {
            if (!ai.behaviours.CityBehBuild.canUseBuildAsset(upgradeOrder, pCity))
            {
                __result = false;
                return false;
            }

            return true;
        }

        if (!IsModernTowerOrDockUpgrade(pBuilding))
        {
            return true;
        }

        if (CityHasModernEraBonfire(pCity))
        {
            return true;
        }

        __result = false;
        return false;
    }
}
[HarmonyPatch(typeof(ActorManager), "evolutionEvent")]
public static class EvolutionPatches
{
    private static void Postfix(Actor pTargetActor, bool pWithBiomeEffect, bool pAscension, ref Actor __result)
    {
        if (__result == null) return;
        AssignBuildOrderTemplate(__result);
    }
    [HarmonyPatch(typeof(ActorManager), "createNewUnit")]
    public static class CreateNewUnitPatches
    {
        private static void Postfix(Actor __result)
        {
            if (__result == null) return;
            AssignBuildOrderTemplate(__result);
        }
    }
    private static void AssignBuildOrderTemplate(Actor actor)
    {
        if (actor == null || actor.asset == null) return;
        var buildOrderMap = new Dictionary<string, string>
        {
            {"civ_dog", "build_order_alliance_epochs"},
            {"civ_cat", "build_order_alliance_epochs"},
            {"civ_chicken", "build_order_alliance_epochs"},
            {"civ_sheep", "build_order_alliance_epochs"},
            {"civ_acid_gentleman", "build_order_alliance_epochs"},
            {"unicorn", "build_order_alliance_epochs"},
            {"miniciv_unicorn", "build_order_alliance_epochs"},
            {"civ_armadillo", "build_order_harden_epochs"},
            {"civ_rhino", "build_order_harden_epochs"},
            {"civ_crab", "build_order_harden_epochs"},
            {"civ_penguin", "build_order_harden_epochs"},
            {"civ_turtle", "build_order_harden_epochs"},
            {"civ_crystal_golem", "build_order_harden_epochs"},
            {"civ_goat", "build_order_harden_epochs"},
            {"civ_candy_man", "build_order_harden_epochs"},
            {"civ_liliar", "build_order_gaia_epochs"},
            {"civ_rabbit", "build_order_gaia_epochs"},
            {"civ_monkey", "build_order_gaia_epochs"},
            {"civ_cow", "build_order_gaia_epochs"},
            {"civ_buffalo", "build_order_gaia_epochs"},
            {"civ_alpaca", "build_order_gaia_epochs"},
            {"civ_capybara", "build_order_gaia_epochs"},
            {"civ_frog", "build_order_gaia_epochs"},
            {"fairy", "build_order_gaia_epochs"},
            {"civ_lemon_man", "build_order_gaia_epochs"},
            {"civ_garlic_man", "build_order_gaia_epochs"},
            {"civ_fox", "build_order_horde_epochs"},
            {"civ_wolf", "build_order_horde_epochs"},
            {"civ_bear", "build_order_horde_epochs"},
            {"civ_hyena", "build_order_horde_epochs"},
            {"civ_rat", "build_order_horde_epochs"},
            {"civ_scorpion", "build_order_horde_epochs"},
            {"civ_crocodile", "build_order_horde_epochs"},
            {"civ_snake", "build_order_horde_epochs"},
            {"civ_piranha", "build_order_horde_epochs"}
        };
        string id = actor.asset.id;
        try {
            if (buildOrderMap.TryGetValue(id, out string buildOrderTemplate))
            {
                ActorAsset asset = AssetManager.actor_library.get(id);
                if (asset != null)
                {
                    asset.build_order_template_id = buildOrderTemplate;
                    actor.setAsset(asset);
                }
            }
        } catch (Exception ex) {
            ModernBoxLogger.Error($"AssignBuildOrderTemplate missing for {id}: {ex.Message}");
        }
    }
}
[HarmonyPatch(typeof(BuildOrder), nameof(BuildOrder.getBuildingAsset))]
public static class Patch_CustomBuildOrder
{
    private static readonly Dictionary<string, string> CustomOrderToBuilding = new Dictionary<string, string>
    {
        { "order_bonfire_alliance", "bonfire_alliance" },
        { "order_bonfire_modern_alliance", "bonfire_modern_alliance" },
        { "order_bonfire_future_alliance", "bonfire_future_alliance" },
        { "order_bonfire_rain_alliance", "bonfire_rain_alliance" },
        { "order_barracks_rain_alliance", "Barracks_rain_alliance" },
        { "order_house_rain_alliance", "House_rain_alliance" },
        { "order_house_modern_alliance", "House_modern_alliance" },
        { "order_house_future_alliance", "House_future_alliance" },
        { "order_bonfire_harden", "bonfire_harden" },
        { "order_bonfire_modern_harden", "bonfire_modern_harden" },
        { "order_bonfire_future_harden", "bonfire_future_harden" },
        { "order_bonfire_rain_harden", "bonfire_rain_harden" },
        { "order_barracks_rain_harden", "Barracks_rain_harden" },
        { "order_house_rain_harden", "House_rain_harden" },
        { "order_house_modern_harden", "House_modern_harden" },
        { "order_house_future_harden", "House_future_harden" },
        { "order_bonfire_gaia", "bonfire_gaia" },
        { "order_bonfire_modern_gaia", "bonfire_modern_gaia" },
        { "order_bonfire_future_gaia", "bonfire_future_gaia" },
        { "order_bonfire_rain_gaia", "bonfire_rain_gaia" },
        { "order_barracks_rain_gaia", "Barracks_rain_gaia" },
        { "order_house_rain_gaia", "House_rain_gaia" },
        { "order_house_modern_gaia", "House_modern_gaia" },
        { "order_house_future_gaia", "House_future_gaia" },
        { "order_bonfire_horde", "bonfire_horde" },
        { "order_bonfire_modern_horde", "bonfire_modern_horde" },
        { "order_bonfire_future_horde", "bonfire_future_horde" },
        { "order_bonfire_rain_horde", "bonfire_rain_horde" },
        { "order_barracks_rain_horde", "Barracks_rain_horde" },
        { "order_house_rain_horde", "House_rain_horde" },
        { "order_house_modern_horde", "House_modern_horde" },
        { "order_house_future_horde", "House_future_horde" }

    };
    public static bool Prefix(BuildOrder __instance, City pCity, string pOrderID, ref BuildingAsset __result)
    {
        string order = string.IsNullOrEmpty(pOrderID) ? __instance.id : pOrderID;
        if (CustomOrderToBuilding.TryGetValue(order, out string buildingId))
        {
            __result = AssetManager.buildings.get(buildingId);
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(ArchitectureAsset), nameof(ArchitectureAsset.getBuildingID))]
public static class Patch_ArchitectureAsset_GetBuildingID
{
    private static readonly Dictionary<string, string> CustomOrderToBuilding = new Dictionary<string, string>
    {
        { "order_bonfire_alliance", "bonfire_alliance" },
        { "order_bonfire_modern_alliance", "bonfire_modern_alliance" },
        { "order_bonfire_future_alliance", "bonfire_future_alliance" },
        { "order_bonfire_rain_alliance", "bonfire_rain_alliance" },
        { "order_barracks_rain_alliance", "Barracks_rain_alliance" },
        { "order_house_rain_alliance", "House_rain_alliance" },
        { "order_house_modern_alliance", "House_modern_alliance" },
        { "order_house_future_alliance", "House_future_alliance" },
        { "order_bonfire_harden", "bonfire_harden" },
        { "order_bonfire_modern_harden", "bonfire_modern_harden" },
        { "order_bonfire_future_harden", "bonfire_future_harden" },
        { "order_bonfire_rain_harden", "bonfire_rain_harden" },
        { "order_barracks_rain_harden", "Barracks_rain_harden" },
        { "order_house_rain_harden", "House_rain_harden" },
        { "order_house_modern_harden", "House_modern_harden" },
        { "order_house_future_harden", "House_future_harden" },
        { "order_bonfire_gaia", "bonfire_gaia" },
        { "order_bonfire_modern_gaia", "bonfire_modern_gaia" },
        { "order_bonfire_future_gaia", "bonfire_future_gaia" },
        { "order_bonfire_rain_gaia", "bonfire_rain_gaia" },
        { "order_barracks_rain_gaia", "Barracks_rain_gaia" },
        { "order_house_rain_gaia", "House_rain_gaia" },
        { "order_house_modern_gaia", "House_modern_gaia" },
        { "order_house_future_gaia", "House_future_gaia" },
        { "order_bonfire_horde", "bonfire_horde" },
        { "order_bonfire_modern_horde", "bonfire_modern_horde" },
        { "order_bonfire_future_horde", "bonfire_future_horde" },
        { "order_bonfire_rain_horde", "bonfire_rain_horde" },
        { "order_barracks_rain_horde", "Barracks_rain_horde" },
        { "order_house_rain_horde", "House_rain_horde" },
        { "order_house_modern_horde", "House_modern_horde" },
        { "order_house_future_horde", "House_future_horde" }
    };
    public static bool Prefix(ArchitectureAsset __instance, string pOrderID, ref string __result)
    {
        if (__instance.building_ids_for_construction != null && __instance.building_ids_for_construction.TryGetValue(pOrderID, out string value))
        {
            __result = value;
            return false;
        }
        if (CustomOrderToBuilding.TryGetValue(pOrderID, out string fallback))
        {
            ModernBoxLogger.Warning($"[MOD] Fallback for missing build order '{pOrderID}': using '{fallback}'");
            __result = fallback;
            return false;
        }
        return true;
    }
}
 [HarmonyPatch(typeof(Docks), "buildBoatFromHere")]
        public static class Patch_Docks_BuildBoatFromHere
        {
            static bool Prefix(Docks __instance, City pCity, ref Actor __result)
            {
                if (__instance.building.asset.id == "Docks_modern_alliance")
                {
                    List<string> availableBoatTypes = new List<string>();
                    if (__instance.countBoatTypes("cargo_alliance_boat") < 1)
                        availableBoatTypes.Add("CargoShip_alliance");
                    if (__instance.countBoatTypes("destroyer_a_alliance_boat") < 1)
                        availableBoatTypes.Add("aDestroyer_alliance");
                    if (__instance.countBoatTypes("destroyer_b_alliance_boat") < 1)
                        availableBoatTypes.Add("bDestroyer_alliance");
                    if (__instance.countBoatTypes("carrier_alliance_boat") < 1)
                        availableBoatTypes.Add("CarrierVessel_alliance");
                    if (__instance.countBoatTypes("submarine_alliance_boat") < 1)
                        availableBoatTypes.Add("Submarine_alliance");
                    if (__instance.countBoatTypes("fishing_alliance_boat") < 1)
                        availableBoatTypes.Add("FishingBoat_alliance");
                    if (__instance.countBoatTypes("abrawler_alliance_boat") < 1)
                        availableBoatTypes.Add("abrawler_alliance");
                    if (__instance.countBoatTypes("bbrawler_alliance_boat") < 1)
                        availableBoatTypes.Add("bbrawler_alliance");
                    if (__instance.countBoatTypes("cbrawler_alliance_boat") < 1)
                        availableBoatTypes.Add("cbrawler_alliance");
                    if (__instance.countBoatTypes("dbrawler_alliance_boat") < 1)
                        availableBoatTypes.Add("dbrawler_alliance");
                    if (__instance.countBoatTypes("ebrawler_alliance_boat") < 1)
                        availableBoatTypes.Add("ebrawler_alliance");
                    if (__instance.countBoatTypes("fbrawler_alliance_boat") < 1)
                        availableBoatTypes.Add("fbrawler_alliance");
                    if (__instance.countBoatTypes("transporter_alliance_boat") < 1)
                        availableBoatTypes.Add("Transporter_alliance");
                    if (availableBoatTypes.Count == 0)
                    {
                        __result = null;
                        return false;
                    }
                    if (!pCity.hasEnoughResourcesFor(new ConstructionCost(0, 0, 0, 1)))
                    {
                        __result = null;
                        return false;
                    }
                    if (__instance.tiles_ocean.Count == 0)
                    {
                        __instance.recalculateOceanTiles();
                        __result = null;
                        return false;
                    }
                    WorldTile tTile = __instance.tiles_ocean.GetRandom();
                    if (!tTile.region.island.goodForDocks())
                    {
                        __result = null;
                        return false;
                    }
                    string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                    Actor tNewBoat = World.world.units.createNewUnit(selectedBoatAssetId, tTile);
                    if (tNewBoat == null)
                    {
                        __result = null;
                        return false;
                    }
                    __instance.addBoatToDock(tNewBoat);
                    tNewBoat.setKingdom(pCity.kingdom);
                    tNewBoat.setCity(pCity);
                    pCity.spendResourcesForBuildingAsset(new ConstructionCost(0, 0, 0, 1));
                    __result = tNewBoat;
                    return false;
                }
                 else if (__instance.building.asset.id == "Docks_rain_gaia")
                {
                    List<string> availableBoatTypes = new List<string>();
                    if (__instance.countBoatTypes("cargo_gaia_boat") < 1)
                        availableBoatTypes.Add("CargoShip_gaia");
                    if (__instance.countBoatTypes("destroyer_a_gaia_boat") < 1)
                        availableBoatTypes.Add("aDestroyer_gaia");
                    if (__instance.countBoatTypes("destroyer_b_gaia_boat") < 1)
                        availableBoatTypes.Add("bDestroyer_gaia");
                    if (__instance.countBoatTypes("carrier_gaia_boat") < 1)
                        availableBoatTypes.Add("CarrierVessel_gaia");
                    if (__instance.countBoatTypes("submarine_gaia_boat") < 1)
                        availableBoatTypes.Add("Submarine_gaia");
                    if (__instance.countBoatTypes("fishing_gaia_boat") < 1)
                        availableBoatTypes.Add("FishingBoat_gaia");
                    if (__instance.countBoatTypes("abrawler_gaia_boat") < 1)
                        availableBoatTypes.Add("abrawler_gaia");
                    if (__instance.countBoatTypes("bbrawler_gaia_boat") < 1)
                        availableBoatTypes.Add("bbrawler_gaia");
                    if (__instance.countBoatTypes("cbrawler_gaia_boat") < 1)
                        availableBoatTypes.Add("cbrawler_gaia");
                    if (__instance.countBoatTypes("dbrawler_gaia_boat") < 1)
                        availableBoatTypes.Add("dbrawler_gaia");
                    if (__instance.countBoatTypes("ebrawler_gaia_boat") < 1)
                        availableBoatTypes.Add("ebrawler_gaia");
                    if (__instance.countBoatTypes("fbrawler_gaia_boat") < 1)
                        availableBoatTypes.Add("fbrawler_gaia");
                    if (__instance.countBoatTypes("transporter_gaia_boat") < 1)
                        availableBoatTypes.Add("Transporter_gaia");
                    if (availableBoatTypes.Count == 0)
                    {
                        __result = null;
                        return false;
                    }
                    if (!pCity.hasEnoughResourcesFor(new ConstructionCost(0, 0, 0, 1)))
                    {
                        __result = null;
                        return false;
                    }
                    if (__instance.tiles_ocean.Count == 0)
                    {
                        __instance.recalculateOceanTiles();
                        __result = null;
                        return false;
                    }
                    WorldTile tTile = __instance.tiles_ocean.GetRandom();
                    if (!tTile.region.island.goodForDocks())
                    {
                        __result = null;
                        return false;
                    }
                    string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                    Actor tNewBoat = World.world.units.createNewUnit(selectedBoatAssetId, tTile);
                    if (tNewBoat == null)
                    {
                        __result = null;
                        return false;
                    }
                    __instance.addBoatToDock(tNewBoat);
                    tNewBoat.setKingdom(pCity.kingdom);
                    tNewBoat.setCity(pCity);
                    pCity.spendResourcesForBuildingAsset(new ConstructionCost(0, 0, 0, 1));
                    __result = tNewBoat;
                    return false;
                }
                 else if (__instance.building.asset.id == "Docks_rain_horde")
                {
                    List<string> availableBoatTypes = new List<string>();
                    if (__instance.countBoatTypes("cargo_horde_boat") < 1)
                        availableBoatTypes.Add("CargoShip_horde");
                    if (__instance.countBoatTypes("destroyer_a_horde_boat") < 1)
                        availableBoatTypes.Add("aDestroyer_horde");
                    if (__instance.countBoatTypes("destroyer_b_horde_boat") < 1)
                        availableBoatTypes.Add("bDestroyer_horde");
                    if (__instance.countBoatTypes("carrier_horde_boat") < 1)
                        availableBoatTypes.Add("CarrierVessel_horde");
                    if (__instance.countBoatTypes("submarine_horde_boat") < 1)
                        availableBoatTypes.Add("Submarine_horde");
                    if (__instance.countBoatTypes("fishing_horde_boat") < 1)
                        availableBoatTypes.Add("FishingBoat_horde");
                    if (__instance.countBoatTypes("abrawler_horde_boat") < 1)
                        availableBoatTypes.Add("abrawler_horde");
                    if (__instance.countBoatTypes("bbrawler_horde_boat") < 1)
                        availableBoatTypes.Add("bbrawler_horde");
                    if (__instance.countBoatTypes("cbrawler_horde_boat") < 1)
                        availableBoatTypes.Add("cbrawler_horde");
                    if (__instance.countBoatTypes("dbrawler_horde_boat") < 1)
                        availableBoatTypes.Add("dbrawler_horde");
                    if (__instance.countBoatTypes("ebrawler_horde_boat") < 1)
                        availableBoatTypes.Add("ebrawler_horde");
                    if (__instance.countBoatTypes("fbrawler_horde_boat") < 1)
                        availableBoatTypes.Add("fbrawler_horde");
                    if (__instance.countBoatTypes("transporter_horde_boat") < 1)
                        availableBoatTypes.Add("Transporter_horde");
                    if (availableBoatTypes.Count == 0)
                    {
                        __result = null;
                        return false;
                    }
                    if (!pCity.hasEnoughResourcesFor(new ConstructionCost(0, 0, 0, 1)))
                    {
                        __result = null;
                        return false;
                    }
                    if (__instance.tiles_ocean.Count == 0)
                    {
                        __instance.recalculateOceanTiles();
                        __result = null;
                        return false;
                    }
                    WorldTile tTile = __instance.tiles_ocean.GetRandom();
                    if (!tTile.region.island.goodForDocks())
                    {
                        __result = null;
                        return false;
                    }
                    string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                    Actor tNewBoat = World.world.units.createNewUnit(selectedBoatAssetId, tTile);
                    if (tNewBoat == null)
                    {
                        __result = null;
                        return false;
                    }
                    __instance.addBoatToDock(tNewBoat);
                    tNewBoat.setKingdom(pCity.kingdom);
                    tNewBoat.setCity(pCity);
                    pCity.spendResourcesForBuildingAsset(new ConstructionCost(0, 0, 0, 1));
                    __result = tNewBoat;
                    return false;
                }
                 else if (__instance.building.asset.id == "Docks_rain_harden")
                {
                    List<string> availableBoatTypes = new List<string>();
                    if (__instance.countBoatTypes("cargo_harden_boat") < 1)
                        availableBoatTypes.Add("CargoShip_harden");
                    if (__instance.countBoatTypes("destroyer_a_harden_boat") < 1)
                        availableBoatTypes.Add("aDestroyer_harden");
                    if (__instance.countBoatTypes("destroyer_b_harden_boat") < 1)
                        availableBoatTypes.Add("bDestroyer_harden");
                    if (__instance.countBoatTypes("carrier_harden_boat") < 1)
                        availableBoatTypes.Add("CarrierVessel_harden");
                    if (__instance.countBoatTypes("submarine_harden_boat") < 1)
                        availableBoatTypes.Add("Submarine_harden");
                    if (__instance.countBoatTypes("fishing_harden_boat") < 1)
                        availableBoatTypes.Add("FishingBoat_harden");
                    if (__instance.countBoatTypes("abrawler_harden_boat") < 1)
                        availableBoatTypes.Add("abrawler_harden");
                    if (__instance.countBoatTypes("bbrawler_harden_boat") < 1)
                        availableBoatTypes.Add("bbrawler_harden");
                    if (__instance.countBoatTypes("cbrawler_harden_boat") < 1)
                        availableBoatTypes.Add("cbrawler_harden");
                    if (__instance.countBoatTypes("dbrawler_harden_boat") < 1)
                        availableBoatTypes.Add("dbrawler_harden");
                    if (__instance.countBoatTypes("ebrawler_harden_boat") < 1)
                        availableBoatTypes.Add("ebrawler_harden");
                    if (__instance.countBoatTypes("fbrawler_harden_boat") < 1)
                        availableBoatTypes.Add("fbrawler_harden");
                        if (__instance.countBoatTypes("transporter_harden_boat") < 1)
                        availableBoatTypes.Add("Transporter_harden");
                    if (availableBoatTypes.Count == 0)
                    {
                        __result = null;
                        return false;
                    }
                    if (!pCity.hasEnoughResourcesFor(new ConstructionCost(0, 0, 0, 1)))
                    {
                        __result = null;
                        return false;
                    }
                    if (__instance.tiles_ocean.Count == 0)
                    {
                        __instance.recalculateOceanTiles();
                        __result = null;
                        return false;
                    }
                    WorldTile tTile = __instance.tiles_ocean.GetRandom();
                    if (!tTile.region.island.goodForDocks())
                    {
                        __result = null;
                        return false;
                    }
                    string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                    Actor tNewBoat = World.world.units.createNewUnit(selectedBoatAssetId, tTile);
                    if (tNewBoat == null)
                    {
                        __result = null;
                        return false;
                    }
                    __instance.addBoatToDock(tNewBoat);
                    tNewBoat.setKingdom(pCity.kingdom);
                    tNewBoat.setCity(pCity);
                    pCity.spendResourcesForBuildingAsset(new ConstructionCost(0, 0, 0, 1));
                    __result = tNewBoat;
                    return false;
                }
                return true;
            }
        }
         [HarmonyPatch(typeof(BuildingAsset), "getRandomBoatAssetToBuild")]
        public static class Patch_BuildingAsset_GetRandomBoatAssetToBuild
        {
            static bool Prefix(BuildingAsset __instance, City pCity, ref ActorAsset __result)
            {
                if (__instance.id == "Docks_modern_alliance")
                {
                    Building dockBuilding = null;
                    foreach (Building building in pCity.buildings)
                    {
                        if (building.asset.id == "Docks_modern_alliance")
                        {
                            dockBuilding = building;
                            break;
                        }
                    }
                    if (dockBuilding != null)
                    {
                        List<string> availableBoatTypes = new List<string>();
                        if (dockBuilding.component_docks.countBoatTypes("cargo_alliance_boat") < 1)
                            availableBoatTypes.Add("CargoShip_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_a_alliance_boat") < 1)
                            availableBoatTypes.Add("aDestroyer_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_b_alliance_boat") < 1)
                            availableBoatTypes.Add("bDestroyer_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("carrier_alliance_boat") < 1)
                            availableBoatTypes.Add("CarrierVessel_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("submarine_alliance_boat") < 1)
                            availableBoatTypes.Add("Submarine_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("fishing_alliance_boat") < 1)
                            availableBoatTypes.Add("FishingBoat_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("abrawler_alliance_boat") < 1)
                            availableBoatTypes.Add("abrawler_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("bbrawler_alliance_boat") < 1)
                            availableBoatTypes.Add("bbrawler_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("cbrawler_alliance_boat") < 1)
                            availableBoatTypes.Add("cbrawler_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("dbrawler_alliance_boat") < 1)
                            availableBoatTypes.Add("dbrawler_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("ebrawler_alliance_boat") < 1)
                            availableBoatTypes.Add("ebrawler_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("fbrawler_alliance_boat") < 1)
                            availableBoatTypes.Add("fbrawler_alliance");
                        if (dockBuilding.component_docks.countBoatTypes("transporter_alliance_boat") < 1)
                            availableBoatTypes.Add("Transporter_alliance");
                        if (availableBoatTypes.Count == 0)
                        {
                            __result = null;
                            return false;
                        }
                        string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                    else
                    {
                        string[] boatAssetIds = new string[] {
                            "CargoShip_alliance",
                            "aDestroyer_alliance",
                            "bDestroyer_alliance",
                            "CarrierVessel_alliance",
                            "Submarine_alliance",
                            "FishingBoat_alliance",
                            "abrawler_alliance",
                            "bbrawler_alliance",
                            "cbrawler_alliance",
                            "dbrawler_alliance",
                            "ebrawler_alliance",
                            "fbrawler_alliance",
                            "Transporter_alliance"
                        };
                        string selectedBoatAssetId = boatAssetIds[Randy.randomInt(0, boatAssetIds.Length)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                }
                else if (__instance.id == "Docks_rain_harden")
                {
                    Building dockBuilding = null;
                    foreach (Building building in pCity.buildings)
                    {
                        if (building.asset.id == "Docks_rain_harden")
                        {
                            dockBuilding = building;
                            break;
                        }
                    }
                    if (dockBuilding != null)
                    {
                        List<string> availableBoatTypes = new List<string>();
                        if (dockBuilding.component_docks.countBoatTypes("cargo_harden_boat") < 1)
                            availableBoatTypes.Add("CargoShip_harden");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_a_harden_boat") < 1)
                            availableBoatTypes.Add("aDestroyer_harden");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_b_harden_boat") < 1)
                            availableBoatTypes.Add("bDestroyer_harden");
                        if (dockBuilding.component_docks.countBoatTypes("carrier_harden_boat") < 1)
                            availableBoatTypes.Add("CarrierVessel_harden");
                        if (dockBuilding.component_docks.countBoatTypes("submarine_harden_boat") < 1)
                            availableBoatTypes.Add("Submarine_harden");
                        if (dockBuilding.component_docks.countBoatTypes("fishing_harden_boat") < 1)
                            availableBoatTypes.Add("FishingBoat_harden");
                        if (dockBuilding.component_docks.countBoatTypes("abrawler_harden_boat") < 1)
                            availableBoatTypes.Add("abrawler_harden");
                        if (dockBuilding.component_docks.countBoatTypes("bbrawler_harden_boat") < 1)
                            availableBoatTypes.Add("bbrawler_harden");
                        if (dockBuilding.component_docks.countBoatTypes("cbrawler_harden_boat") < 1)
                            availableBoatTypes.Add("cbrawler_harden");
                        if (dockBuilding.component_docks.countBoatTypes("dbrawler_harden_boat") < 1)
                            availableBoatTypes.Add("dbrawler_harden");
                        if (dockBuilding.component_docks.countBoatTypes("ebrawler_harden_boat") < 1)
                            availableBoatTypes.Add("ebrawler_harden");
                        if (dockBuilding.component_docks.countBoatTypes("fbrawler_harden_boat") < 1)
                            availableBoatTypes.Add("fbrawler_harden");
                         if (dockBuilding.component_docks.countBoatTypes("transporter_harden_boat") < 1)
                            availableBoatTypes.Add("Transporter_harden");
                        if (availableBoatTypes.Count == 0)
                        {
                            __result = null;
                            return false;
                        }
                        string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                    else
                    {
                        string[] boatAssetIds = new string[] {
                            "CargoShip_harden",
                            "aDestroyer_harden",
                            "bDestroyer_harden",
                            "CarrierVessel_harden",
                            "Submarine_harden",
                            "FishingBoat_harden",
                            "abrawler_harden",
                            "bbrawler_harden",
                            "cbrawler_harden",
                            "dbrawler_harden",
                            "ebrawler_harden",
                            "fbrawler_harden",
                            "Transporter_harden"
                        };
                        string selectedBoatAssetId = boatAssetIds[Randy.randomInt(0, boatAssetIds.Length)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                }
                else if (__instance.id == "Docks_rain_horde")
                {
                    Building dockBuilding = null;
                    foreach (Building building in pCity.buildings)
                    {
                        if (building.asset.id == "Docks_rain_horde")
                        {
                            dockBuilding = building;
                            break;
                        }
                    }
                    if (dockBuilding != null)
                    {
                        List<string> availableBoatTypes = new List<string>();
                        if (dockBuilding.component_docks.countBoatTypes("cargo_horde_boat") < 1)
                            availableBoatTypes.Add("CargoShip_horde");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_a_horde_boat") < 1)
                            availableBoatTypes.Add("aDestroyer_horde");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_b_horde_boat") < 1)
                            availableBoatTypes.Add("bDestroyer_horde");
                        if (dockBuilding.component_docks.countBoatTypes("carrier_horde_boat") < 1)
                            availableBoatTypes.Add("CarrierVessel_horde");
                        if (dockBuilding.component_docks.countBoatTypes("submarine_horde_boat") < 1)
                            availableBoatTypes.Add("Submarine_horde");
                        if (dockBuilding.component_docks.countBoatTypes("fishing_horde_boat") < 1)
                            availableBoatTypes.Add("FishingBoat_horde");
                        if (dockBuilding.component_docks.countBoatTypes("abrawler_horde_boat") < 1)
                            availableBoatTypes.Add("abrawler_horde");
                        if (dockBuilding.component_docks.countBoatTypes("bbrawler_horde_boat") < 1)
                            availableBoatTypes.Add("bbrawler_horde");
                        if (dockBuilding.component_docks.countBoatTypes("cbrawler_horde_boat") < 1)
                            availableBoatTypes.Add("cbrawler_horde");
                        if (dockBuilding.component_docks.countBoatTypes("dbrawler_horde_boat") < 1)
                            availableBoatTypes.Add("dbrawler_horde");
                        if (dockBuilding.component_docks.countBoatTypes("ebrawler_horde_boat") < 1)
                            availableBoatTypes.Add("ebrawler_horde");
                        if (dockBuilding.component_docks.countBoatTypes("fbrawler_horde_boat") < 1)
                            availableBoatTypes.Add("fbrawler_horde");
                            if (dockBuilding.component_docks.countBoatTypes("transporter_horde_boat") < 1)
                            availableBoatTypes.Add("Transporter_horde");
                        if (availableBoatTypes.Count == 0)
                        {
                            __result = null;
                            return false;
                        }
                        string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                    else
                    {
                        string[] boatAssetIds = new string[] {
                            "CargoShip_horde",
                            "aDestroyer_horde",
                            "bDestroyer_horde",
                            "CarrierVessel_horde",
                            "Submarine_horde",
                            "FishingBoat_horde",
                            "abrawler_horde",
                            "bbrawler_horde",
                            "cbrawler_horde",
                            "dbrawler_horde",
                            "ebrawler_horde",
                            "fbrawler_horde",
                            "Transporter_horde"
                        };
                        string selectedBoatAssetId = boatAssetIds[Randy.randomInt(0, boatAssetIds.Length)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                }
                else if (__instance.id == "Docks_rain_gaia")
                {
                    Building dockBuilding = null;
                    foreach (Building building in pCity.buildings)
                    {
                        if (building.asset.id == "Docks_rain_gaia")
                        {
                            dockBuilding = building;
                            break;
                        }
                    }
                    if (dockBuilding != null)
                    {
                        List<string> availableBoatTypes = new List<string>();
                        if (dockBuilding.component_docks.countBoatTypes("cargo_gaia_boat") < 1)
                            availableBoatTypes.Add("CargoShip_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_a_gaia_boat") < 1)
                            availableBoatTypes.Add("aDestroyer_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("destroyer_b_gaia_boat") < 1)
                            availableBoatTypes.Add("bDestroyer_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("carrier_gaia_boat") < 1)
                            availableBoatTypes.Add("CarrierVessel_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("submarine_gaia_boat") < 1)
                            availableBoatTypes.Add("Submarine_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("fishing_gaia_boat") < 1)
                            availableBoatTypes.Add("FishingBoat_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("abrawler_gaia_boat") < 1)
                            availableBoatTypes.Add("abrawler_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("bbrawler_gaia_boat") < 1)
                            availableBoatTypes.Add("bbrawler_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("cbrawler_gaia_boat") < 1)
                            availableBoatTypes.Add("cbrawler_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("dbrawler_gaia_boat") < 1)
                            availableBoatTypes.Add("dbrawler_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("ebrawler_gaia_boat") < 1)
                            availableBoatTypes.Add("ebrawler_gaia");
                        if (dockBuilding.component_docks.countBoatTypes("fbrawler_gaia_boat") < 1)
                            availableBoatTypes.Add("fbrawler_gaia");
                         if (dockBuilding.component_docks.countBoatTypes("transporter_alliance_boat") < 1)
                            availableBoatTypes.Add("Transporter_alliance");
                        if (availableBoatTypes.Count == 0)
                        {
                            __result = null;
                            return false;
                        }
                        string selectedBoatAssetId = availableBoatTypes[Randy.randomInt(0, availableBoatTypes.Count)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                    else
                    {
                        string[] boatAssetIds = new string[] {
                            "CargoShip_gaia",
                            "aDestroyer_gaia",
                            "bDestroyer_gaia",
                            "CarrierVessel_gaia",
                            "Submarine_gaia",
                            "FishingBoat_gaia",
                            "abrawler_gaia",
                            "bbrawler_gaia",
                            "cbrawler_gaia",
                            "dbrawler_gaia",
                            "ebrawler_gaia",
                            "fbrawler_gaia",
                            "Transporter_alliance"
                        };
                        string selectedBoatAssetId = boatAssetIds[Randy.randomInt(0, boatAssetIds.Length)];
                        __result = AssetManager.actor_library.get(selectedBoatAssetId);
                        return false;
                    }
                }
                return true;
            }
        }
           [HarmonyPatch(typeof(BuildingAsset), "getBoatAssetIDFromType")]
        public static class Patch_BuildingAsset_GetBoatAssetIDFromType
        {
            static bool Prefix(string pSpeciesBoat, City pCity, ref string __result)
            {
                switch (pSpeciesBoat)
                {
                    case "cargo_alliance_boat":
                        __result = "CargoShip_alliance";
                        return false;
                    case "destroyer_a_alliance_boat":
                        __result = "aDestroyer_alliance";
                        return false;
                    case "destroyer_b_alliance_boat":
                        __result = "bDestroyer_alliance";
                        return false;
                    case "carrier_alliance_boat":
                        __result = "CarrierVessel_alliance";
                        return false;
                    case "submarine_alliance_boat":
                        __result = "Submarine_alliance";
                        return false;
                    case "fishing_alliance_boat":
                        __result = "FishingBoat_alliance";
                        return false;
                    case "abrawler_alliance_boat":
                        __result = "abrawler_alliance";
                        return false;
                       case "bbrawler_alliance_boat":
                        __result = "bbrawler_alliance";
                        return false;
                        case "cbrawler_alliance_boat":
                        __result = "cbrawler_alliance";
                        return false;
                    case "dbrawler_alliance_boat":
                        __result = "dbrawler_alliance";
                        return false;
                       case "ebrawler_alliance_boat":
                        __result = "ebrawler_alliance";
                        return false;
                        case "fbrawler_alliance_boat":
                        __result = "fbrawler_alliance";
                        return false;
                        case "transporter_alliance_boat":
                        __result = "Transporter_alliance";
                        return false;
                   case "cargo_horde_boat":
                        __result = "CargoShip_horde";
                        return false;
                    case "destroyer_a_horde_boat":
                        __result = "aDestroyer_horde";
                        return false;
                    case "destroyer_b_horde_boat":
                        __result = "bDestroyer_horde";
                        return false;
                    case "carrier_horde_boat":
                        __result = "CarrierVessel_horde";
                        return false;
                    case "submarine_horde_boat":
                        __result = "Submarine_horde";
                        return false;
                    case "fishing_horde_boat":
                        __result = "FishingBoat_horde";
                        return false;
                    case "abrawler_horde_boat":
                        __result = "abrawler_horde";
                        return false;
                       case "bbrawler_horde_boat":
                        __result = "bbrawler_horde";
                        return false;
                        case "cbrawler_horde_boat":
                        __result = "cbrawler_horde";
                        return false;
                    case "dbrawler_horde_boat":
                        __result = "dbrawler_horde";
                        return false;
                       case "ebrawler_horde_boat":
                        __result = "ebrawler_horde";
                        return false;
                        case "fbrawler_horde_boat":
                        __result = "fbrawler_horde";
                        return false;
                       case "transporter_horde_boat":
                        __result = "Transporter_horde";
                        return false;
                    case "cargo_gaia_boat":
                        __result = "CargoShip_gaia";
                        return false;
                    case "destroyer_a_gaia_boat":
                        __result = "aDestroyer_gaia";
                        return false;
                    case "destroyer_b_gaia_boat":
                        __result = "bDestroyer_gaia";
                        return false;
                    case "carrier_gaia_boat":
                        __result = "CarrierVessel_gaia";
                        return false;
                    case "submarine_gaia_boat":
                        __result = "Submarine_gaia";
                        return false;
                    case "fishing_gaia_boat":
                        __result = "FishingBoat_gaia";
                        return false;
                    case "abrawler_gaia_boat":
                        __result = "abrawler_gaia";
                        return false;
                       case "bbrawler_gaia_boat":
                        __result = "bbrawler_gaia";
                        return false;
                        case "cbrawler_gaia_boat":
                        __result = "cbrawler_gaia";
                        return false;
                    case "dbrawler_gaia_boat":
                        __result = "dbrawler_gaia";
                        return false;
                       case "ebrawler_gaia_boat":
                        __result = "ebrawler_gaia";
                        return false;
                        case "fbrawler_gaia_boat":
                        __result = "fbrawler_gaia";
                        return false;
                        case "transporter_gaia_boat":
                        __result = "Transporter_gaia";
                        return false;
                    case "cargo_harden_boat":
                        __result = "CargoShip_harden";
                        return false;
                    case "destroyer_a_harden_boat":
                        __result = "aDestroyer_harden";
                        return false;
                    case "destroyer_b_harden_boat":
                        __result = "bDestroyer_harden";
                        return false;
                    case "carrier_harden_boat":
                        __result = "CarrierVessel_harden";
                        return false;
                    case "submarine_harden_boat":
                        __result = "Submarine_harden";
                        return false;
                    case "fishing_harden_boat":
                        __result = "FishingBoat_harden";
                        return false;
                    case "abrawler_harden_boat":
                        __result = "abrawler_harden";
                        return false;
                       case "bbrawler_harden_boat":
                        __result = "bbrawler_harden";
                        return false;
                        case "cbrawler_harden_boat":
                        __result = "cbrawler_harden";
                        return false;
                    case "dbrawler_harden_boat":
                        __result = "dbrawler_harden";
                        return false;
                       case "ebrawler_harden_boat":
                        __result = "ebrawler_harden";
                        return false;
                        case "fbrawler_harden_boat":
                        __result = "fbrawler_harden";
                        return false;
                        case "transporter_harden_boat":
                        __result = "Transporter_harden";
                        return false;
                }
                return true;
            }
        }
        }
        }
