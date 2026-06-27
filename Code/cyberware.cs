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
    class cyberware
    {
        public static void init()
        {

            //makecraftable();


			
		    // ItemAsset Sandevistan = AssetManager.items.clone("Sandevistan", "_equipment");
            // Sandevistan.id = "Sandevistan";
            // Sandevistan.quality = ItemQuality.Legendary;
            // Sandevistan.name_class = "item_class_armor";
            // Sandevistan.name_templates = List.Of<string>(new string[] { "armor_name" });
            // Sandevistan.equipmentType = EquipmentType.Armor;
            // Sandevistan.base_stats[S.knockback_reduction] = 200f;
            // Sandevistan.base_stats[S.armor] = 10;
            // Sandevistan.base_stats[S.health] = 600;
		    // Sandevistan.base_stats[S.speed] = 500;
            // Sandevistan.materials = List.Of<string>(new string[] { "steel" });
            // Sandevistan.equipment_value = 900;
			// Sandevistan.tech_needed = "Cyberware";
            // Sandevistan.setCost(1, "Parts", 1);
            // AssetManager.items.list.AddItem(Sandevistan);
            // Localization.addLocalization("item_Sandevistan", "Military Grade Sandevistan");
			

		
		
          ItemAsset Sandevistan = AssetManager.items.clone("Sandevistan", "_accessory");
          Sandevistan.id = "Sandevistan";
          Sandevistan.base_stats[S.fertility] = -0.1f;
          Sandevistan.base_stats[S.max_children] = 0f;
          Sandevistan.base_stats[S.max_age] = 10f;
          Sandevistan.base_stats[S.attack_speed] = 20;
          Sandevistan.base_stats[S.damage] = 20;
          Sandevistan.base_stats[S.speed] = 500f;
          Sandevistan.base_stats[S.health] = 0;
          Sandevistan.base_stats[S.accuracy] = 0f;
          Sandevistan.base_stats[S.range] = 1;
          Sandevistan.base_stats[S.armor] = 0;
          Sandevistan.base_stats[S.scale] = 0.0f;
          Sandevistan.base_stats[S.dodge] = 0f;
          Sandevistan.base_stats[S.targets] = 2f;
          Sandevistan.base_stats[S.critical_chance] = 0.15f;
          Sandevistan.base_stats[S.knockback] = 0f;
          Sandevistan.base_stats[S.knockback_reduction] = 0f;
          Sandevistan.base_stats[S.intelligence] = 7;
          Sandevistan.base_stats[S.warfare] = 4;
          Sandevistan.base_stats[S.diplomacy] = 0;
          Sandevistan.base_stats[S.stewardship] = 0;
          Sandevistan.base_stats[S.opinion] = 0f;
          Sandevistan.base_stats[S.loyalty_traits] = 0f;
          Sandevistan.base_stats[S.cities] = 0;
          Sandevistan.base_stats[S.zone_range] = 0;
          Sandevistan.equipmentType = EquipmentType.Ring;
          Sandevistan.name_templates = List.Of<string>(new string[]{ "ring_name" });
          Sandevistan.materials = List.Of<string>(new string[]{ "steel" });
          Sandevistan.tech_needed = "Cyberware";
          Sandevistan.equipment_value = 1500;
          AssetManager.items.list.AddItem(Sandevistan);
          Localization.addLocalization("item_Sandevistan", "Military Grade Sandevistan");
		  
          ItemAsset TurboBooster = AssetManager.items.clone("TurboBooster", "_accessory");
          TurboBooster.id = "TurboBooster";
          TurboBooster.base_stats[S.fertility] = -0.1f;
          TurboBooster.base_stats[S.max_children] = 0f;
          TurboBooster.base_stats[S.max_age] = 10f;
          TurboBooster.base_stats[S.attack_speed] = 20;
          TurboBooster.base_stats[S.damage] = 20;
          TurboBooster.base_stats[S.speed] = 0f;
          TurboBooster.base_stats[S.health] = 1000;
          TurboBooster.base_stats[S.accuracy] = 0f;
          TurboBooster.base_stats[S.range] = 1;
          TurboBooster.base_stats[S.armor] = 100;
          TurboBooster.base_stats[S.scale] = 0.0f;
          TurboBooster.base_stats[S.dodge] = 0f;
          TurboBooster.base_stats[S.targets] = 2f;
          TurboBooster.base_stats[S.critical_chance] = 0.15f;
          TurboBooster.base_stats[S.knockback] = 0f;
          TurboBooster.base_stats[S.knockback_reduction] = 200f;
          TurboBooster.base_stats[S.intelligence] = 7;
          TurboBooster.base_stats[S.warfare] = 4;
          TurboBooster.base_stats[S.diplomacy] = 0;
          TurboBooster.base_stats[S.stewardship] = 0;
          TurboBooster.base_stats[S.opinion] = 0f;
          TurboBooster.base_stats[S.loyalty_traits] = 0f;
          TurboBooster.base_stats[S.cities] = 0;
          TurboBooster.base_stats[S.zone_range] = 0;
          TurboBooster.equipmentType = EquipmentType.Ring;
          TurboBooster.name_templates = List.Of<string>(new string[]{ "ring_name" });
          TurboBooster.materials = List.Of<string>(new string[]{ "steel" });
          TurboBooster.tech_needed = "Cyberware";
          TurboBooster.equipment_value = 1500;
          AssetManager.items.list.AddItem(TurboBooster);
          Localization.addLocalization("item_TurboBooster", "Military Grade TurboBooster"); 
 

		}
		public static void toggleCyberware()
        {
            Main.modifyBoolOption("CyberwareOption", PowerButtons.GetToggleValue("Cyberware_toggle"));
            if (PowerButtons.GetToggleValue("Cyberware_toggle"))
            {
                turnOnCyberware();
                return;
            }
            turnOffCyberware();
        }
		
		// public static void toggleCyberware()
        // {
            // if (PowerButtons.GetToggleValue("Cyberware_toggle"))
            // {
                // turnOnCyberware();
                // return;
            // }
            // turnOffCyberware();
        // }
			public static void turnOnCyberware()
			{
                Race human = AssetManager.raceLibrary.get("human");
                Race orc = AssetManager.raceLibrary.get("orc");
                Race dwarf = AssetManager.raceLibrary.get("dwarf");
                Race elf = AssetManager.raceLibrary.get("elf");

                orc.preferred_weapons.Add("Sandevistan");
                human.preferred_weapons.Add("Sandevistan");
                dwarf.preferred_weapons.Add("Sandevistan");
                elf.preferred_weapons.Add("Sandevistan");
				
				orc.preferred_weapons.Add("TurboBooster");
                human.preferred_weapons.Add("TurboBooster");
                dwarf.preferred_weapons.Add("TurboBooster");
                elf.preferred_weapons.Add("TurboBooster");

            }
			public static void turnOffCyberware()
			{
                Race human = AssetManager.raceLibrary.get("human");
                Race orc = AssetManager.raceLibrary.get("orc");
                Race dwarf = AssetManager.raceLibrary.get("dwarf");
                Race elf = AssetManager.raceLibrary.get("elf");

                orc.preferred_weapons.Remove("Sandevistan");
                human.preferred_weapons.Remove("Sandevistan");
                dwarf.preferred_weapons.Remove("Sandevistan");
                elf.preferred_weapons.Remove("Sandevistan");
				
				orc.preferred_weapons.Remove("TurboBooster");
                human.preferred_weapons.Remove("TurboBooster");
                dwarf.preferred_weapons.Remove("TurboBooster");
                elf.preferred_weapons.Remove("TurboBooster");

            }
            static void addcyberSprite(string id, string material)
            {
              var dictItems = Reflection.GetField(typeof(ActorAnimationLoader), null, "dictItems") as Dictionary<string, Sprite>;
              var sprite = Resources.Load<Sprite>("ItemTextures/w_" + id + "_" + material);
              dictItems.Add(sprite.name, sprite);
            }
             	
    }
}
