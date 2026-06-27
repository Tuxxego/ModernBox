//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using NCMS.Utils;

namespace ModernBox
{
    class Drugs
    {
		//public List<string> addWeapons = new List<string>();

        public static void init()
        {

            //makecraftable();


			// More coming in final 2.0 version.

        
			
          // ItemAsset Meth = AssetManager.items.clone("Meth", "_equipment");
          // Meth.id = "Meth";
          // Meth.base_stats[S.fertility] = 0.0f;
          // Meth.base_stats[S.max_children] = 0f;
          // Meth.base_stats[S.max_age] = -3f;
          // Meth.base_stats[S.attack_speed] = 0;
          // Meth.base_stats[S.damage] = 0;
          // Meth.base_stats[S.speed] = 4f;
          // Meth.base_stats[S.health] = 0;
          // Meth.base_stats[S.accuracy] = 0f;
          // Meth.base_stats[S.range] = 0;
          // Meth.base_stats[S.armor] = 0;
          // Meth.base_stats[S.scale] = 0.0f;
          // Meth.base_stats[S.dodge] = 0f;
          // Meth.base_stats[S.targets] = 0f;
          // Meth.base_stats[S.critical_chance] = 0.0f;
          // Meth.base_stats[S.knockback] = 0f;
          // Meth.base_stats[S.knockback_reduction] = 0f;
          // Meth.base_stats[S.intelligence] = -2;
          // Meth.base_stats[S.warfare] = 0;
          // Meth.base_stats[S.diplomacy] = 0;
          // Meth.base_stats[S.stewardship] = 0;
          // Meth.base_stats[S.opinion] = 0f;
          // Meth.base_stats[S.loyalty_traits] = 0f;
          // Meth.base_stats[S.cities] = 0;
          // Meth.base_stats[S.zone_range] = 0;
          // Meth.equipmentType = EquipmentType.Armor;
          // Meth.name_class = "item_class_armor";
          // Meth.name_templates = List.Of<string>(new string[]{ "armor_name" });
          // Meth.materials = List.Of<string>(new string[]{ "silver" });
          // Meth.tech_needed = "way_of_live";
          // Meth.equipment_value = 300;
          // Meth.setCost(1, "Gold", 1);
          // AssetManager.items.list.AddItem(Meth);
          // Localization.addLocalization("item_Meth", "Meth");
		  
          // ItemAsset Meth = AssetManager.items.clone("Meth", "_equipment");
          // Meth.id = "Meth";
          // Meth.base_stats[S.fertility] = 0.0f;
          // Meth.base_stats[S.max_children] = 0f;
          // Meth.base_stats[S.max_age] = 0f;
          // Meth.base_stats[S.attack_speed] = 0;
          // Meth.base_stats[S.damage] = 0;
          // Meth.base_stats[S.speed] = 0f;
          // Meth.base_stats[S.health] = 60;
          // Meth.base_stats[S.accuracy] = 0f;
          // Meth.base_stats[S.range] = 0;
          // Meth.base_stats[S.armor] = 35;
          // Meth.base_stats[S.scale] = 0.0f;
          // Meth.base_stats[S.dodge] = 0f;
          // Meth.base_stats[S.targets] = 0f;
          // Meth.base_stats[S.critical_chance] = 0.0f;
          // Meth.base_stats[S.knockback] = 0f;
          // Meth.base_stats[S.knockback_reduction] = 5f;
          // Meth.base_stats[S.intelligence] = 0;
          // Meth.base_stats[S.warfare] = 0;
          // Meth.base_stats[S.diplomacy] = 0;
          // Meth.base_stats[S.stewardship] = 0;
          // Meth.base_stats[S.opinion] = 0f;
          // Meth.base_stats[S.loyalty_traits] = 0f;
          // Meth.base_stats[S.cities] = 0;
          // Meth.base_stats[S.zone_range] = 0;
          // Meth.equipmentType = EquipmentType.Armor;
          // Meth.name_class = "item_class_armor";
          // Meth.name_templates = List.Of<string>(new string[]{ "helmet_name" });
          // Meth.materials = List.Of<string>(new string[]{ "wood" });
          // Meth.tech_needed = "Cyberware";
          // Meth.equipment_value = 5;
          // AssetManager.items.list.AddItem(Meth);
          // Localization.addLocalization("item_Meth", "Meth");

          ItemAsset Meth = AssetManager.items.clone("Meth", "_accessory");
          Meth.id = "Meth";
          Meth.base_stats[S.fertility] = -0.1f;
          Meth.base_stats[S.max_children] = -5f;
          Meth.base_stats[S.max_age] = -10f;
          Meth.base_stats[S.attack_speed] = 20;
          Meth.base_stats[S.damage] = 90;
          Meth.base_stats[S.speed] = 50f;
          Meth.base_stats[S.health] = -30;
          Meth.base_stats[S.accuracy] = 0f;
          Meth.base_stats[S.range] = 1;
          Meth.base_stats[S.armor] = 100;
          Meth.base_stats[S.scale] = 0.0f;
          Meth.base_stats[S.dodge] = 0f;
          Meth.base_stats[S.targets] = 2f;
          Meth.base_stats[S.critical_chance] = 0.15f;
          Meth.base_stats[S.knockback] = 0f;
          Meth.base_stats[S.knockback_reduction] = 200f;
          Meth.base_stats[S.intelligence] = -7;
          Meth.base_stats[S.warfare] = -4;
          Meth.base_stats[S.diplomacy] = 0;
          Meth.base_stats[S.stewardship] = 0;
          Meth.base_stats[S.opinion] = 0f;
          Meth.base_stats[S.loyalty_traits] = 0f;
          Meth.base_stats[S.cities] = 0;
          Meth.base_stats[S.zone_range] = 0;
          Meth.equipmentType = EquipmentType.Ring;
          Meth.name_templates = List.Of<string>(new string[]{ "ring_name" });
          Meth.materials = List.Of<string>(new string[]{ "steel" });
          Meth.tech_needed = "Cyberware";
          Meth.equipment_value = 1500;
          AssetManager.items.list.AddItem(Meth);
          Localization.addLocalization("item_Meth", "Meth"); 
		  
		 ItemAsset Crack = AssetManager.items.clone("Crack", "_accessory");
          Crack.id = "Crack";
          Crack.base_stats[S.fertility] = -0.1f;
          Crack.base_stats[S.max_children] = -5f;
          Crack.base_stats[S.max_age] = -10f;
          Crack.base_stats[S.attack_speed] = -20;
          Crack.base_stats[S.damage] = 90;
          Crack.base_stats[S.speed] = -50f;
          Crack.base_stats[S.health] = -30;
          Crack.base_stats[S.accuracy] = 0f;
          Crack.base_stats[S.range] = 1;
          Crack.base_stats[S.armor] = 100;
          Crack.base_stats[S.scale] = 0.0f;
          Crack.base_stats[S.dodge] = 0f;
          Crack.base_stats[S.targets] = 2f;
          Crack.base_stats[S.critical_chance] = 0.15f;
          Crack.base_stats[S.knockback] = 0f;
          Crack.base_stats[S.knockback_reduction] = 200f;
          Crack.base_stats[S.intelligence] = -7;
          Crack.base_stats[S.warfare] = -4;
          Crack.base_stats[S.diplomacy] = 0;
          Crack.base_stats[S.stewardship] = 0;
          Crack.base_stats[S.opinion] = 0f;
          Crack.base_stats[S.loyalty_traits] = 0f;
          Crack.base_stats[S.cities] = -5;
          Crack.base_stats[S.zone_range] = 0;
          Crack.equipmentType = EquipmentType.Ring;
          Crack.name_templates = List.Of<string>(new string[]{ "ring_name" });
          Crack.materials = List.Of<string>(new string[]{ "steel" });
          Crack.tech_needed = "Cyberware";
          Crack.equipment_value = 1500;
          AssetManager.items.list.AddItem(Crack);
          Localization.addLocalization("item_Crack", "Crack"); 
		}
		public static void toggleDrugs()
        {
            Main.modifyBoolOption("DrugsOption", PowerButtons.GetToggleValue("Drugs_toggle"));
            if (PowerButtons.GetToggleValue("Drugs_toggle"))
            {
                turnOnDrugs();
                return;
            }
            turnOffDrugs();
        }
		
		// public static void toggleDrugs()
        // {
            // if (PowerButtons.GetToggleValue("Drugs_toggle"))
            // {
                // turnOnDrugs();
                // return;
            // }
            // turnOffDrugs();
        // }
			public static void turnOnDrugs()
			{
                Race human = AssetManager.raceLibrary.get("human");
                Race orc = AssetManager.raceLibrary.get("orc");
                Race dwarf = AssetManager.raceLibrary.get("dwarf");
                Race elf = AssetManager.raceLibrary.get("elf");

                orc.preferred_weapons.Add("Meth");
                human.preferred_weapons.Add("Meth");
                dwarf.preferred_weapons.Add("Meth");
                elf.preferred_weapons.Add("Meth");
				
				orc.preferred_weapons.Add("Crack");
                human.preferred_weapons.Add("Crack");
                dwarf.preferred_weapons.Add("Crack");
                elf.preferred_weapons.Add("Crack");

            }
			public static void turnOffDrugs()
			{
                Race human = AssetManager.raceLibrary.get("human");
                Race orc = AssetManager.raceLibrary.get("orc");
                Race dwarf = AssetManager.raceLibrary.get("dwarf");
                Race elf = AssetManager.raceLibrary.get("elf");

                orc.preferred_weapons.Remove("Meth");
                human.preferred_weapons.Remove("Meth");
                dwarf.preferred_weapons.Remove("Meth");
                elf.preferred_weapons.Remove("Meth");
				
                orc.preferred_weapons.Remove("Crack");
                human.preferred_weapons.Remove("Crack");
                dwarf.preferred_weapons.Remove("Crack");
                elf.preferred_weapons.Remove("Crack");

            }
            static void addDrugSprite(string id, string material)
            {
              var dictItems = Reflection.GetField(typeof(ActorAnimationLoader), null, "dictItems") as Dictionary<string, Sprite>;
              var sprite = Resources.Load<Sprite>("ItemTextures/w_" + id + "_" + material);
              dictItems.Add(sprite.name, sprite);
            }
             	
    }
}
