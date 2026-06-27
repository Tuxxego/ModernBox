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
    class guns
    {
        public static void init()
        {

            makecraftable();
            

			
			//if(Main.settings.Mp5){			            
			ItemAsset Mp5 = AssetManager.items.clone("Mp5", "_range");
            Mp5.id = "Mp5";
            Mp5.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            Mp5.materials = List.Of<string>(new string[] { "iron" });
            Mp5.projectile = "shotgun_bullet";
			Mp5.tech_needed = "Firearms";			
            Mp5.base_stats[S.range] = 14f;
            Mp5.base_stats[S.accuracy] = 5;
            Mp5.base_stats[S.attack_speed] = 12f;
            Mp5.base_stats[S.damage] = 60;
            Mp5.base_stats[S.health] = 10;
            Mp5.equipment_value = 500;
            Mp5.path_slash_animation = "effects/slashes/slash_punch";
            Mp5.quality = ItemQuality.Legendary;
            Mp5.equipmentType = EquipmentType.Weapon;
			
            AssetManager.items.list.AddItem(Mp5);
            Localization.addLocalization("item_Mp5", "Mp5"); 
            addgunSprite(Mp5.id, Mp5.materials[0]);
			//}
			//if(Main.settings.NorincoCQ){			            
			ItemAsset NorincoCQ = AssetManager.items.clone("NorincoCQ", "_range");
            NorincoCQ.id = "NorincoCQ";
            NorincoCQ.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            NorincoCQ.materials = List.Of<string>(new string[] { "iron" });
            NorincoCQ.projectile = "shotgun_bullet";
			NorincoCQ.tech_needed = "Firearms";			
            NorincoCQ.base_stats[S.range] = 14f;
            NorincoCQ.base_stats[S.accuracy] = 5;
            NorincoCQ.base_stats[S.attack_speed] = -16f;
            NorincoCQ.base_stats[S.damage] = 60;
            NorincoCQ.base_stats[S.health] = 10;
            NorincoCQ.equipment_value = 500;
            NorincoCQ.path_slash_animation = "effects/slashes/slash_punch";
            NorincoCQ.quality = ItemQuality.Legendary;
            NorincoCQ.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(NorincoCQ);
            Localization.addLocalization("item_NorincoCQ", "Norinco CQ");
            addgunSprite(NorincoCQ.id, NorincoCQ.materials[0]);
			//}
			
			//if(Main.settings.Sniper){			            
			ItemAsset Sniper = AssetManager.items.clone("Sniper", "_range");
            Sniper.id = "Sniper";
            Sniper.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            Sniper.materials = List.Of<string>(new string[] { "iron" });
            Sniper.projectile = "shotgun_bullet";
			Sniper.tech_needed = "Firearms";
            Sniper.base_stats[S.range] = 20f;
            Sniper.base_stats[S.accuracy] = 5;
            Sniper.base_stats[S.attack_speed] = 12f;
            Sniper.base_stats[S.damage] = 193;
            Sniper.base_stats[S.health] = 10;
            Sniper.equipment_value = 500;
            Sniper.path_slash_animation = "effects/slashes/slash_punch";
            Sniper.quality = ItemQuality.Legendary;
            Sniper.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(Sniper);
            Localization.addLocalization("item_Sniper", "Sniper");
            addgunSprite(Sniper.id, Sniper.materials[0]);
			//}
			
			//if(Main.settings.mm9){			            
			ItemAsset mm9 = AssetManager.items.clone("mm9", "_range");
            mm9.id = "mm9";
            mm9.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            mm9.materials = List.Of<string>(new string[] { "iron" });
            mm9.projectile = "shotgun_bullet";
			mm9.tech_needed = "Firearms";
            mm9.base_stats[S.range] = 6f;
            mm9.base_stats[S.accuracy] = 400;
            mm9.base_stats[S.attack_speed] = 70;
            mm9.base_stats[S.damage] = 3;
            mm9.base_stats[S.health] = 10;
            mm9.equipment_value = 500;
            mm9.path_slash_animation = "effects/slashes/slash_punch";
            mm9.quality = ItemQuality.Legendary;
            mm9.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(mm9);
            Localization.addLocalization("item_mm9", "9mm");
            addgunSprite(mm9.id, mm9.materials[0]);
			//}
			////if(Main.settings.RocketLauncher){			            
			ItemAsset RocketLauncher = AssetManager.items.clone("RocketLauncher", "_range");
            RocketLauncher.id = "RocketLauncher";
            RocketLauncher.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            RocketLauncher.materials = List.Of<string>(new string[] { "iron" });
            RocketLauncher.projectile = "fireball";
            RocketLauncher.base_stats[S.range] = 14f;
			RocketLauncher.tech_needed = "Firearms";
            RocketLauncher.base_stats[S.accuracy] = 400;
            RocketLauncher.base_stats[S.attack_speed] = -101f;
            RocketLauncher.base_stats[S.damage] = 993;
            RocketLauncher.base_stats[S.health] = 10;
            RocketLauncher.equipment_value = 1000;
            RocketLauncher.path_slash_animation = "effects/slashes/slash_punch";
            RocketLauncher.quality = ItemQuality.Legendary;
            RocketLauncher.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(RocketLauncher);
            Localization.addLocalization("item_RocketLauncher", "Rocket Propelled Grenade (RPG)");
            addgunSprite(RocketLauncher.id, RocketLauncher.materials[0]);
			
			//}
	

			
			ItemAsset Uzi = AssetManager.items.clone("Uzi", "_range");
            Uzi.id = "Uzi";
            Uzi.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            Uzi.materials = List.Of<string>(new string[] { "iron" });
            Uzi.projectile = "shotgun_bullet";
            Uzi.base_stats[S.range] = 6f;
            Uzi.base_stats[S.accuracy] = 400;
            Uzi.base_stats[S.attack_speed] = 70;
            Uzi.base_stats[S.damage] = 3;
            Uzi.base_stats[S.health] = 10;
			Uzi.tech_needed = "Firearms";
            Uzi.equipment_value = 500;
            Uzi.path_slash_animation = "effects/slashes/slash_punch";
            Uzi.quality = ItemQuality.Legendary;
            Uzi.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(Uzi);
            Localization.addLocalization("item_Uzi", "Uzi");
            addgunSprite(Uzi.id, Uzi.materials[0]);
			
			ItemAsset Minigun = AssetManager.items.clone("Minigun", "_range");
            Minigun.id = "Minigun";
            Minigun.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            Minigun.materials = List.Of<string>(new string[] { "iron" });
            Minigun.projectile = "shotgun_bullet";
			Minigun.tech_needed = "Firearms";			
            Minigun.base_stats[S.range] = 14f;
            Minigun.base_stats[S.accuracy] = 3;
            Minigun.base_stats[S.attack_speed] = 200f;
            Minigun.base_stats[S.damage] = 13;
            Minigun.base_stats[S.health] = 10;
            Minigun.equipment_value = 500;
            Minigun.path_slash_animation = "effects/slashes/slash_punch";
            Minigun.quality = ItemQuality.Legendary;
            Minigun.equipmentType = EquipmentType.Weapon;
			
            AssetManager.items.list.AddItem(Minigun);
            Localization.addLocalization("item_Minigun", "Minigun"); 
            addgunSprite(Minigun.id, Minigun.materials[0]);
			
			ItemAsset GunshipCannon = AssetManager.items.clone("GunshipCannon", "_range");
            GunshipCannon.id = "GunshipCannon";
            GunshipCannon.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            GunshipCannon.materials = List.Of<string>(new string[] { "iron" });
            GunshipCannon.projectile = "GunshipBullet";
			GunshipCannon.tech_needed = "Firearms";			
            GunshipCannon.base_stats[S.range] = 000f;
            GunshipCannon.base_stats[S.accuracy] = 400;
            GunshipCannon.base_stats[S.attack_speed] = 200f;
            GunshipCannon.base_stats[S.damage] = 13;
            GunshipCannon.base_stats[S.health] = 10;
            GunshipCannon.equipment_value = 500;
            GunshipCannon.path_slash_animation = "effects/slashes/slash_punch";
            GunshipCannon.quality = ItemQuality.Legendary;
            GunshipCannon.equipmentType = EquipmentType.Weapon;
			
            AssetManager.items.list.AddItem(GunshipCannon);
            Localization.addLocalization("item_GunshipCannon", "GunshipCannon"); 
            addgunSprite(GunshipCannon.id, GunshipCannon.materials[0]);
			
			ItemAsset AK47 = AssetManager.items.clone("AK47", "_range");
            AK47.id = "AK47";
            AK47.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            AK47.materials = List.Of<string>(new string[] { "iron" });
            AK47.projectile = "shotgun_bullet";
			AK47.tech_needed = "Firearms";			
            AK47.base_stats[S.range] = 14f;
            AK47.base_stats[S.accuracy] = 3;
            AK47.base_stats[S.attack_speed] = 170f;
            AK47.base_stats[S.damage] = 13;
            AK47.base_stats[S.health] = 10;
            AK47.equipment_value = 500;
            AK47.path_slash_animation = "effects/slashes/slash_punch";
            AK47.quality = ItemQuality.Legendary;
            AK47.equipmentType = EquipmentType.Weapon;
			AssetManager.items.list.AddItem(AK47);
			Localization.addLocalization("item_AK47", "AK47");
			addgunSprite(AK47.id, AK47.materials[0]);
			
			
			ItemAsset AK103 = AssetManager.items.clone("AK103", "_range");
            AK103.id = "AK103";
            AK103.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            AK103.materials = List.Of<string>(new string[] { "iron" });
            AK103.projectile = "shotgun_bullet";
			AK103.tech_needed = "Firearms";			
            AK103.base_stats[S.range] = 14f;
            AK103.base_stats[S.accuracy] = 400;
            AK103.base_stats[S.attack_speed] = 170f;
            AK103.base_stats[S.damage] = 13;
            AK103.base_stats[S.health] = 10;
            AK103.equipment_value = 500;
            AK103.path_slash_animation = "effects/slashes/slash_punch";
            AK103.quality = ItemQuality.Legendary;
            AK103.equipmentType = EquipmentType.Weapon;
			AssetManager.items.list.AddItem(AK103);
			Localization.addLocalization("item_AK103", "AK-103");
			addgunSprite(AK103.id, AK103.materials[0]);
			


			
          ItemAsset XM8 = AssetManager.items.clone("XM8", "_range");
          XM8.id = "XM8";
          XM8.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          XM8.materials = List.Of<string>(new string[]{"iron"});
          XM8.projectile = "shotgun_bullet";
          XM8.base_stats[S.fertility] = 0.0f;
          XM8.base_stats[S.max_children] = 0f;
          XM8.base_stats[S.max_age] = 0f;
          XM8.base_stats[S.attack_speed] = 180;
          XM8.base_stats[S.damage] = 13;
          XM8.base_stats[S.speed] = 0f;
          XM8.base_stats[S.health] = 0;
          XM8.base_stats[S.accuracy] = 400f;
          XM8.base_stats[S.range] = 14;
          XM8.base_stats[S.armor] = 0;
		  XM8.tech_needed = "Firearms";		
          XM8.base_stats[S.scale] = 0.0f;
          XM8.base_stats[S.dodge] = 0f;
          XM8.base_stats[S.targets] = 0f;
          XM8.base_stats[S.critical_chance] = 0.0f;
          XM8.base_stats[S.knockback] = 0f;
          XM8.base_stats[S.knockback_reduction] = 0f;
          XM8.base_stats[S.intelligence] = 0;
          XM8.base_stats[S.warfare] = 0;
          XM8.base_stats[S.diplomacy] = 0;
          XM8.base_stats[S.stewardship] = 0;
          XM8.base_stats[S.opinion] = 0f;
          XM8.base_stats[S.loyalty_traits] = 0f;
          XM8.base_stats[S.cities] = 0;
          XM8.base_stats[S.zone_range] = 0;
          XM8.equipment_value = 500;
          XM8.path_slash_animation = "effects/slashes/slash_punch";
          XM8.equipmentType = EquipmentType.Weapon;
          XM8.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(XM8);
          Localization.addLocalization("item_XM8", "XM8");
          addgunSprite(XM8.id, XM8.materials[0]);
		  
			ItemAsset SGT44 = AssetManager.items.clone("SGT44", "_range");
            SGT44.id = "SGT44";
            SGT44.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            SGT44.materials = List.Of<string>(new string[] { "iron" });
            SGT44.projectile = "shotgun_bullet";
			SGT44.path_icon = "ui/Icons/items/icon_SGT44_Gun";
			SGT44.tech_needed = "Firearms";			
            SGT44.base_stats[S.range] = 14f;
            SGT44.base_stats[S.accuracy] = 400;
            SGT44.base_stats[S.attack_speed] = 70f;
            SGT44.base_stats[S.damage] = 11;
            SGT44.base_stats[S.health] = 10;
            SGT44.equipment_value = 500;
            SGT44.path_slash_animation = "effects/slashes/slash_punch";
            SGT44.quality = ItemQuality.Legendary;
            SGT44.equipmentType = EquipmentType.Weapon;
			
					

            AssetManager.items.list.AddItem(SGT44);
            Localization.addLocalization("item_SGT44", "SGT44"); 
            addgunSprite(SGT44.id, SGT44.materials[0]);
			
			ItemAsset ThompsonM1A1 = AssetManager.items.clone("ThompsonM1A1", "_range");
            ThompsonM1A1.id = "ThompsonM1A1";
            ThompsonM1A1.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            ThompsonM1A1.materials = List.Of<string>(new string[] { "iron" });
            ThompsonM1A1.projectile = "shotgun_bullet";
			ThompsonM1A1.tech_needed = "Firearms";			
            ThompsonM1A1.base_stats[S.range] = 14f;
            ThompsonM1A1.base_stats[S.accuracy] = 400;
            ThompsonM1A1.base_stats[S.attack_speed] = 170f;
            ThompsonM1A1.base_stats[S.damage] = 13;
            ThompsonM1A1.base_stats[S.health] = 10;
            ThompsonM1A1.equipment_value = 500;
            ThompsonM1A1.path_slash_animation = "effects/slashes/slash_punch";
            ThompsonM1A1.quality = ItemQuality.Legendary;
            ThompsonM1A1.equipmentType = EquipmentType.Weapon;
			
            AssetManager.items.list.AddItem(ThompsonM1A1);
            Localization.addLocalization("item_ThompsonM1A1", "ThompsonM1A1"); 
            addgunSprite(ThompsonM1A1.id, ThompsonM1A1.materials[0]);
			
			ItemAsset M4A1 = AssetManager.items.clone("M4A1", "_range");
			M4A1.id = "M4A1";
			M4A1.name_templates = Toolbox.splitStringIntoList(new string[]
			{
				"sword_name#30",
				"sword_name_king#3",
				"weapon_name_city",
				"weapon_name_kingdom",
				"weapon_name_culture",
				"weapon_name_enemy_king",
				"weapon_name_enemy_kingdom"
			});
			M4A1.materials = List.Of<string>(new string[] { "iron" });
			M4A1.projectile = "shotgun_bullet";
			M4A1.tech_needed = "Firearms";
			M4A1.base_stats[S.range] = 14f;
			M4A1.base_stats[S.accuracy] = 400;
			M4A1.base_stats[S.attack_speed] = 170f;
			M4A1.base_stats[S.damage] = 15;
			M4A1.base_stats[S.health] = 10;
			M4A1.equipment_value = 500;
			M4A1.path_slash_animation = "effects/slashes/slash_punch";
			M4A1.quality = ItemQuality.Legendary;
			M4A1.equipmentType = EquipmentType.Weapon;

			AssetManager.items.list.AddItem(M4A1);
			Localization.addLocalization("item_M4A1", "M4A1");
			addgunSprite(M4A1.id, M4A1.materials[0]);
			
		  ItemAsset FAMAS = AssetManager.items.clone("FAMAS", "_range");
          FAMAS.id = "FAMAS";
          FAMAS.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          FAMAS.materials = List.Of<string>(new string[]{"iron"});
          FAMAS.projectile = "shotgun_bullet";
          FAMAS.base_stats[S.fertility] = 0.0f;
          FAMAS.base_stats[S.max_children] = 0f;
          FAMAS.base_stats[S.max_age] = 0f;
          FAMAS.base_stats[S.attack_speed] = 180;
          FAMAS.base_stats[S.damage] = 13;
          FAMAS.base_stats[S.speed] = 0f;
          FAMAS.base_stats[S.health] = 0;
          FAMAS.base_stats[S.accuracy] = 400f;
          FAMAS.base_stats[S.range] = 14;
          FAMAS.base_stats[S.armor] = 0;
		  FAMAS.tech_needed = "Firearms";		
          FAMAS.base_stats[S.scale] = 0.0f;
          FAMAS.base_stats[S.dodge] = 0f;
          FAMAS.base_stats[S.targets] = 0f;
          FAMAS.base_stats[S.critical_chance] = 0.0f;
          FAMAS.base_stats[S.knockback] = 0f;
          FAMAS.base_stats[S.knockback_reduction] = 0f;
          FAMAS.base_stats[S.intelligence] = 0;
          FAMAS.base_stats[S.warfare] = 0;
          FAMAS.base_stats[S.diplomacy] = 0;
          FAMAS.base_stats[S.stewardship] = 0;
          FAMAS.base_stats[S.opinion] = 0f;
          FAMAS.base_stats[S.loyalty_traits] = 0f;
          FAMAS.base_stats[S.cities] = 0;
          FAMAS.base_stats[S.zone_range] = 0;
          FAMAS.equipment_value = 500;
          FAMAS.path_slash_animation = "effects/slashes/slash_punch";
          FAMAS.equipmentType = EquipmentType.Weapon;
          FAMAS.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(FAMAS);
          Localization.addLocalization("item_FAMAS", "FAMAS");
          addgunSprite(FAMAS.id, FAMAS.materials[0]);
		  
		  	ItemAsset malorian = AssetManager.items.clone("malorian", "_range");
            malorian.id = "malorian";
            malorian.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            malorian.materials = List.Of<string>(new string[] { "iron" });
            malorian.projectile = "shotgun_bullet";
			malorian.tech_needed = "Firearms";
            malorian.base_stats[S.range] = 6f;
            malorian.base_stats[S.accuracy] = 400;
            malorian.base_stats[S.attack_speed] = 70;
            malorian.base_stats[S.damage] = 3;
            malorian.base_stats[S.health] = 10;
            malorian.equipment_value = 500;
            malorian.path_slash_animation = "effects/slashes/slash_punch";
            malorian.quality = ItemQuality.Legendary;
            malorian.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(malorian);
            Localization.addLocalization("item_malorian", "Malorian Arms 3516");
            addgunSprite(malorian.id, malorian.materials[0]);
			

		  ItemAsset SCAR = AssetManager.items.clone("SCAR", "_range");
          SCAR.id = "SCAR";
          SCAR.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          SCAR.materials = List.Of<string>(new string[]{"iron"});
          SCAR.projectile = "shotgun_bullet";
          SCAR.base_stats[S.fertility] = 0.0f;
          SCAR.base_stats[S.max_children] = 0f;
          SCAR.base_stats[S.max_age] = 0f;
          SCAR.base_stats[S.attack_speed] = 180;
          SCAR.base_stats[S.damage] = 13;
          SCAR.base_stats[S.speed] = 0f;
          SCAR.base_stats[S.health] = 0;
          SCAR.base_stats[S.accuracy] = 400f;
          SCAR.base_stats[S.range] = 6f;
          SCAR.base_stats[S.armor] = 0;
		  SCAR.tech_needed = "Firearms";		
          SCAR.base_stats[S.scale] = 0.0f;
          SCAR.base_stats[S.dodge] = 0f;
          SCAR.base_stats[S.targets] = 0f;
          SCAR.base_stats[S.critical_chance] = 0.0f;
          SCAR.base_stats[S.knockback] = 0f;
          SCAR.base_stats[S.knockback_reduction] = 0f;
          SCAR.base_stats[S.intelligence] = 0;
          SCAR.base_stats[S.warfare] = 0;
          SCAR.base_stats[S.diplomacy] = 0;
          SCAR.base_stats[S.stewardship] = 0;
          SCAR.base_stats[S.opinion] = 0f;
          SCAR.base_stats[S.loyalty_traits] = 0f;
          SCAR.base_stats[S.cities] = 0;
          SCAR.base_stats[S.zone_range] = 0;
          SCAR.equipment_value = 500;
          SCAR.path_slash_animation = "effects/slashes/slash_punch";
          SCAR.equipmentType = EquipmentType.Weapon;
          SCAR.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(SCAR);
          Localization.addLocalization("item_SCAR", "SCAR");
          addgunSprite(SCAR.id, SCAR.materials[0]);
		  
		  ItemAsset PipeRifle = AssetManager.items.clone("PipeRifle", "_range");
          PipeRifle.id = "PipeRifle";
          PipeRifle.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          PipeRifle.materials = List.Of<string>(new string[]{"iron"});
          PipeRifle.projectile = "shotgun_bullet";
          PipeRifle.base_stats[S.fertility] = 0.0f;
          PipeRifle.base_stats[S.max_children] = 0f;
          PipeRifle.base_stats[S.max_age] = 0f;
          PipeRifle.base_stats[S.attack_speed] = 180;
          PipeRifle.base_stats[S.damage] = 13;
          PipeRifle.base_stats[S.speed] = 0f;
          PipeRifle.base_stats[S.health] = 0;
          PipeRifle.base_stats[S.accuracy] = 70f;
          PipeRifle.base_stats[S.range] = 6f;
          PipeRifle.base_stats[S.armor] = 0;	
          PipeRifle.base_stats[S.scale] = 0.0f;
          PipeRifle.base_stats[S.dodge] = 0f;
          PipeRifle.base_stats[S.targets] = 0f;
          PipeRifle.base_stats[S.critical_chance] = 0.0f;
          PipeRifle.base_stats[S.knockback] = 0f;
          PipeRifle.base_stats[S.knockback_reduction] = 0f;
          PipeRifle.base_stats[S.intelligence] = 0;
          PipeRifle.base_stats[S.warfare] = 0;
          PipeRifle.base_stats[S.diplomacy] = 0;
          PipeRifle.base_stats[S.stewardship] = 0;
          PipeRifle.base_stats[S.opinion] = 0f;
          PipeRifle.base_stats[S.loyalty_traits] = 0f;
          PipeRifle.base_stats[S.cities] = 0;
          PipeRifle.base_stats[S.zone_range] = 0;
          PipeRifle.equipment_value = 100;
          PipeRifle.path_slash_animation = "effects/slashes/slash_punch";
          PipeRifle.tech_needed = "LowFirearms";
          PipeRifle.equipmentType = EquipmentType.Weapon;
          PipeRifle.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(PipeRifle);
          Localization.addLocalization("item_PipeRifle", "Pipe Rifle");
          addgunSprite(PipeRifle.id, PipeRifle.materials[0]);
		  
		  ItemAsset PipePistol = AssetManager.items.clone("PipePistol", "_range");
          PipePistol.id = "PipePistol";
          PipePistol.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          PipePistol.materials = List.Of<string>(new string[]{"iron"});
          PipePistol.projectile = "shotgun_bullet";
          PipePistol.base_stats[S.fertility] = 0.0f;
          PipePistol.base_stats[S.max_children] = 0f;
          PipePistol.base_stats[S.max_age] = 0f;
          PipePistol.base_stats[S.attack_speed] = 20;
          PipePistol.base_stats[S.damage] = 13;
          PipePistol.base_stats[S.speed] = 0f;
          PipePistol.base_stats[S.health] = 0;
          PipePistol.base_stats[S.accuracy] = 70f;
          PipePistol.base_stats[S.range] = 6f;
          PipePistol.base_stats[S.armor] = 0;	
          PipePistol.base_stats[S.scale] = 0.0f;
          PipePistol.base_stats[S.dodge] = 0f;
          PipePistol.base_stats[S.targets] = 0f;
          PipePistol.base_stats[S.critical_chance] = 0.0f;
          PipePistol.base_stats[S.knockback] = 0f;
          PipePistol.base_stats[S.knockback_reduction] = 0f;
          PipePistol.base_stats[S.intelligence] = 0;
          PipePistol.base_stats[S.warfare] = 0;
          PipePistol.base_stats[S.diplomacy] = 0;
          PipePistol.base_stats[S.stewardship] = 0;
          PipePistol.base_stats[S.opinion] = 0f;
          PipePistol.base_stats[S.loyalty_traits] = 0f;
          PipePistol.base_stats[S.cities] = 0;
          PipePistol.base_stats[S.zone_range] = 0;
          PipePistol.equipment_value = 100;
          PipePistol.path_slash_animation = "effects/slashes/slash_punch";
          PipePistol.tech_needed = "LowFirearms";
          PipePistol.equipmentType = EquipmentType.Weapon;
          PipePistol.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(PipePistol);
          Localization.addLocalization("item_PipePistol", "Pipe Pistol");
          addgunSprite(PipePistol.id, PipePistol.materials[0]);
		  
		  ItemAsset PipeShotgun = AssetManager.items.clone("PipeShotgun", "_range");
          PipeShotgun.id = "PipeShotgun";
          PipeShotgun.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          PipeShotgun.materials = List.Of<string>(new string[]{"iron"});
          PipeShotgun.projectile = "shotgun_bullet";
          PipeShotgun.base_stats[S.fertility] = 0.0f;
          PipeShotgun.base_stats[S.max_children] = 0f;
          PipeShotgun.base_stats[S.max_age] = 0f;
          PipeShotgun.base_stats[S.attack_speed] = 20;
          PipeShotgun.base_stats[S.damage] = 130;
          PipeShotgun.base_stats[S.speed] = 0f;
          PipeShotgun.base_stats[S.health] = 0;
          PipeShotgun.base_stats[S.accuracy] = 70f;
          PipeShotgun.base_stats[S.range] = 6f;
          PipeShotgun.base_stats[S.armor] = 0;
          PipeShotgun.base_stats[S.scale] = 0.0f;
          PipeShotgun.base_stats[S.dodge] = 0f;
          PipeShotgun.base_stats[S.targets] = 0f;
          PipeShotgun.base_stats[S.critical_chance] = 0.0f;
          PipeShotgun.base_stats[S.knockback] = 0f;
          PipeShotgun.base_stats[S.knockback_reduction] = 0f;
          PipeShotgun.base_stats[S.intelligence] = 0;
          PipeShotgun.base_stats[S.warfare] = 0;
          PipeShotgun.base_stats[S.diplomacy] = 0;
          PipeShotgun.base_stats[S.stewardship] = 0;
          PipeShotgun.base_stats[S.opinion] = 0f;
          PipeShotgun.base_stats[S.loyalty_traits] = 0f;
          PipeShotgun.base_stats[S.cities] = 0;
          PipeShotgun.base_stats[S.zone_range] = 0;
          PipeShotgun.equipment_value = 100;
          PipeShotgun.path_slash_animation = "effects/slashes/slash_punch";
          PipeShotgun.tech_needed = "LowFirearms";
          PipeShotgun.equipmentType = EquipmentType.Weapon;
          PipeShotgun.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(PipeShotgun);
          Localization.addLocalization("item_PipeShotgun", "Pipe Shotgun");
          addgunSprite(PipeShotgun.id, PipeShotgun.materials[0]);
		  
		  ItemAsset Musket = AssetManager.items.clone("Musket", "_range");
          Musket.id = "Musket";
          Musket.name_templates = Toolbox.splitStringIntoList(new string[]
          {
          "bow_name#30",
          "sword_name_king#3",
          "weapon_name_city",
          "weapon_name_kingdom",
          "weapon_name_culture",
          "weapon_name_enemy_king",
          "weapon_name_enemy_kingdom"
          });
          Musket.materials = List.Of<string>(new string[]{"iron"});
          Musket.projectile = "shotgun_bullet";
          Musket.base_stats[S.fertility] = 0.0f;
          Musket.base_stats[S.max_children] = 0f;
          Musket.base_stats[S.max_age] = 0f;
          Musket.base_stats[S.attack_speed] = 20;
          Musket.base_stats[S.damage] = 130;
          Musket.base_stats[S.speed] = 0f;
          Musket.base_stats[S.health] = 0;
          Musket.base_stats[S.accuracy] = 70f;
          Musket.base_stats[S.range] = 0;
          Musket.base_stats[S.armor] = 0;
          Musket.base_stats[S.scale] = 0.0f;
          Musket.base_stats[S.dodge] = 0f;
          Musket.base_stats[S.targets] = 0f;
          Musket.base_stats[S.critical_chance] = 0.0f;
          Musket.base_stats[S.knockback] = 0f;
          Musket.base_stats[S.knockback_reduction] = 0f;
          Musket.base_stats[S.intelligence] = 0;
          Musket.base_stats[S.warfare] = 0;
          Musket.base_stats[S.diplomacy] = 0;
          Musket.base_stats[S.stewardship] = 0;
          Musket.base_stats[S.opinion] = 0f;
          Musket.base_stats[S.loyalty_traits] = 0f;
          Musket.base_stats[S.cities] = 0;
          Musket.base_stats[S.zone_range] = 0;
          Musket.equipment_value = 100;
          Musket.path_slash_animation = "effects/slashes/slash_punch";
          Musket.tech_needed = "LowFirearms";
          Musket.equipmentType = EquipmentType.Weapon;
          Musket.name_class = "item_class_weapon";
          AssetManager.items.list.AddItem(Musket);
          Localization.addLocalization("item_Musket", "Musket");
          addgunSprite(Musket.id, Musket.materials[0]);
		  
		  	ItemAsset MP7 = AssetManager.items.clone("MP7", "_range");
            MP7.id = "MP7";
            MP7.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            MP7.materials = List.Of<string>(new string[] { "iron" });
            MP7.projectile = "shotgun_bullet";
			MP7.tech_needed = "Firearms";			
            MP7.base_stats[S.range] = 0f;
            MP7.base_stats[S.accuracy] = 5;
            MP7.base_stats[S.attack_speed] = 12f;
            MP7.base_stats[S.damage] = 60;
            MP7.base_stats[S.health] = 10;
            MP7.equipment_value = 500;
            MP7.path_slash_animation = "effects/slashes/slash_punch";
            MP7.quality = ItemQuality.Legendary;
            MP7.equipmentType = EquipmentType.Weapon;
			
            AssetManager.items.list.AddItem(MP7);
            Localization.addLocalization("item_MP7", "MP7"); 
            addgunSprite(MP7.id, MP7.materials[0]);
			
			ItemAsset HK416 = AssetManager.items.clone("HK416", "_range");
            HK416.id = "HK416";
            HK416.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            HK416.materials = List.Of<string>(new string[] { "iron" });
            HK416.projectile = "shotgun_bullet";
			HK416.tech_needed = "Firearms";			
            HK416.base_stats[S.range] = 0f;
            HK416.base_stats[S.accuracy] = 5;
            HK416.base_stats[S.attack_speed] = 12f;
            HK416.base_stats[S.damage] = 60;
            HK416.base_stats[S.health] = 10;
            HK416.equipment_value = 500;
            HK416.path_slash_animation = "effects/slashes/slash_punch";
            HK416.quality = ItemQuality.Legendary;
            HK416.equipmentType = EquipmentType.Weapon;
			
			AssetManager.items.list.AddItem(HK416);
            Localization.addLocalization("item_HK416", "HK416"); 
            addgunSprite(HK416.id, HK416.materials[0]);


			ItemAsset M16 = AssetManager.items.clone("M16", "_range");
            M16.id = "M16";
            M16.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "sword_name#30",
                        "sword_name_king#3",
                        "weapon_name_city",
                        "weapon_name_kingdom",
                        "weapon_name_culture",
                        "weapon_name_enemy_king",
                        "weapon_name_enemy_kingdom"
            });
            M16.materials = List.Of<string>(new string[] { "iron" });
            M16.projectile = "shotgun_bullet";
			M16.tech_needed = "Firearms";			
            M16.base_stats[S.range] = 0f;
            M16.base_stats[S.accuracy] = 5;
            M16.base_stats[S.attack_speed] = 12f;
            M16.base_stats[S.damage] = 60;
            M16.base_stats[S.health] = 10;
            M16.equipment_value = 500;
            M16.path_slash_animation = "effects/slashes/slash_punch";
            M16.quality = ItemQuality.Legendary;
            M16.equipmentType = EquipmentType.Weapon;
			
            AssetManager.items.list.AddItem(M16);
            Localization.addLocalization("item_M16", "M16"); 
            addgunSprite(M16.id, M16.materials[0]);




			// Add new gun 1
			ItemAsset DesertEagle = AssetManager.items.clone("DesertEagle", "_range");
			DesertEagle.id = "DesertEagle";
			DesertEagle.name_templates = Toolbox.splitStringIntoList(new string[]
			{
				"sword_name#30",
				"sword_name_king#3",
				"weapon_name_city",
				"weapon_name_kingdom",
				"weapon_name_culture",
				"weapon_name_enemy_king",
				"weapon_name_enemy_kingdom"
			});
			DesertEagle.materials = List.Of<string>(new string[] { "iron" });
			DesertEagle.projectile = "shotgun_bullet";
			DesertEagle.tech_needed = "Firearms";
			DesertEagle.base_stats[S.range] = 20f;
			DesertEagle.base_stats[S.accuracy] = 90;
			DesertEagle.base_stats[S.attack_speed] = 1f;
			DesertEagle.base_stats[S.damage] = 40;
			DesertEagle.base_stats[S.health] = 10;
			DesertEagle.equipment_value = 500;
			DesertEagle.path_slash_animation = "effects/slashes/slash_punch";
			DesertEagle.quality = ItemQuality.Legendary;
			DesertEagle.equipmentType = EquipmentType.Weapon;

			AssetManager.items.list.AddItem(DesertEagle);
			Localization.addLocalization("item_DesertEagle", "Desert Eagle");
			addgunSprite(DesertEagle.id, DesertEagle.materials[0]);

			// Add new gun 2
			ItemAsset Glock17 = AssetManager.items.clone("Glock17", "_range");
			Glock17.id = "Glock17";
			Glock17.name_templates = Toolbox.splitStringIntoList(new string[]
			{
				"sword_name#30",
				"sword_name_king#3",
				"weapon_name_city",
				"weapon_name_kingdom",
				"weapon_name_culture",
				"weapon_name_enemy_king",
				"weapon_name_enemy_kingdom"
			});
			Glock17.materials = List.Of<string>(new string[] { "iron" });
			Glock17.projectile = "shotgun_bullet";
			Glock17.tech_needed = "Firearms";
			Glock17.base_stats[S.range] = 15f;
			Glock17.base_stats[S.accuracy] = 95;
			Glock17.base_stats[S.attack_speed] = 1f;
			Glock17.base_stats[S.damage] = 35;
			Glock17.base_stats[S.health] = 10;
			Glock17.equipment_value = 500;
			Glock17.path_slash_animation = "effects/slashes/slash_punch";
			Glock17.quality = ItemQuality.Legendary;
			Glock17.equipmentType = EquipmentType.Weapon;

			AssetManager.items.list.AddItem(Glock17);
			Localization.addLocalization("item_Glock17", "Glock 17");
			addgunSprite(Glock17.id, Glock17.materials[0]);










			static void makecraftable()
			{


		
			}
		}
		
		public static void toggleGuns()
        {
            Main.modifyBoolOption("GunOption", PowerButtons.GetToggleValue("Gun_toggle"));
            if (PowerButtons.GetToggleValue("Gun_toggle"))
            {
                turnOnGuns();
                return;
            }
            turnOffGuns();
        }
		

			public static void turnOnGuns()
			{
				
								Race human = AssetManager.raceLibrary.get("human");
				Race orc = AssetManager.raceLibrary.get("orc");
				Race dwarf = AssetManager.raceLibrary.get("dwarf");
				Race elf = AssetManager.raceLibrary.get("elf");
				
				human.preferred_weapons.Add("Mp5");
				human.preferred_weapons.Add("Uzi");
				human.preferred_weapons.Add("Sniper");
				human.preferred_weapons.Add("RocketLauncher");
				human.preferred_weapons.Add("mm9");
				human.preferred_weapons.Add("Minigun");
				human.preferred_weapons.Add("AK47");
				human.preferred_weapons.Add("ThompsonM1A1");
				human.preferred_weapons.Add("XM8");
				human.preferred_weapons.Add("SGT44");
				human.preferred_weapons.Add("M4A1"); // Add M4A1 for human race
				human.preferred_weapons.Add("FAMAS");
				human.preferred_weapons.Add("malorian");
				human.preferred_weapons.Add("SCAR");	
				human.preferred_weapons.Add("AK103");	
				human.preferred_weapons.Add("MP7");	
				human.preferred_weapons.Add("Musket");	
				human.preferred_weapons.Add("HK416");	
				human.preferred_weapons.Add("M16");	
				human.preferred_weapons.Add("DesertEagle");	
				human.preferred_weapons.Add("Glock17");	
				
				orc.preferred_weapons.Add("Mp5");
				orc.preferred_weapons.Add("Uzi");
				orc.preferred_weapons.Add("Sniper");
				orc.preferred_weapons.Add("RocketLauncher");
				orc.preferred_weapons.Add("mm9");
				orc.preferred_weapons.Add("Minigun");
				orc.preferred_weapons.Add("AK47");
				orc.preferred_weapons.Add("ThompsonM1A1");
				orc.preferred_weapons.Add("XM8");
				orc.preferred_weapons.Add("SGT44");
				orc.preferred_weapons.Add("M4A1"); // Add M4A1 for orc race
				orc.preferred_weapons.Add("FAMAS");
				orc.preferred_weapons.Add("malorian");	
				orc.preferred_weapons.Add("SCAR");			
				orc.preferred_weapons.Add("AK103");	
				orc.preferred_weapons.Add("MP7");	
				orc.preferred_weapons.Add("Musket");	
				orc.preferred_weapons.Add("HK416");					
				orc.preferred_weapons.Add("M16");	
				orc.preferred_weapons.Add("DesertEagle");	
				orc.preferred_weapons.Add("Glock17");	
				
				dwarf.preferred_weapons.Add("Mp5");
				dwarf.preferred_weapons.Add("Uzi");
				dwarf.preferred_weapons.Add("Sniper");
				dwarf.preferred_weapons.Add("RocketLauncher");
				dwarf.preferred_weapons.Add("mm9");
				dwarf.preferred_weapons.Add("Minigun");
				dwarf.preferred_weapons.Add("AK47");
				dwarf.preferred_weapons.Add("ThompsonM1A1");
				dwarf.preferred_weapons.Add("XM8");
				dwarf.preferred_weapons.Add("SGT44");
				dwarf.preferred_weapons.Add("M4A1"); // Add M4A1 for dwarf race
				dwarf.preferred_weapons.Add("FAMAS");
				dwarf.preferred_weapons.Add("malorian");
				dwarf.preferred_weapons.Add("SCAR");	
				dwarf.preferred_weapons.Add("AK103");	
				dwarf.preferred_weapons.Add("MP7");	
				dwarf.preferred_weapons.Add("Musket");	
				dwarf.preferred_weapons.Add("HK416");	
				dwarf.preferred_weapons.Add("M16");	
				dwarf.preferred_weapons.Add("DesertEagle");	
				dwarf.preferred_weapons.Add("Glock17");			
				
				elf.preferred_weapons.Add("Mp5");
				elf.preferred_weapons.Add("Uzi");
				elf.preferred_weapons.Add("Sniper");
				elf.preferred_weapons.Add("RocketLauncher");
				elf.preferred_weapons.Add("mm9");
				elf.preferred_weapons.Add("Minigun");
				elf.preferred_weapons.Add("AK47");
				elf.preferred_weapons.Add("ThompsonM1A1");
				elf.preferred_weapons.Add("XM8");
				elf.preferred_weapons.Add("SGT44");
				elf.preferred_weapons.Add("M4A1"); // Add M4A1 for elf race
				elf.preferred_weapons.Add("FAMAS");
				elf.preferred_weapons.Add("malorian");		
				elf.preferred_weapons.Add("SCAR");			
				elf.preferred_weapons.Add("AK103");	
				elf.preferred_weapons.Add("MP7");	
				elf.preferred_weapons.Add("HK416");		
				elf.preferred_weapons.Add("M16");	
				elf.preferred_weapons.Add("DesertEagle");	
				elf.preferred_weapons.Add("Glock17");			


            }
			public static void turnOffGuns()
			{
				
				Race human = AssetManager.raceLibrary.get("human");
				Race orc = AssetManager.raceLibrary.get("orc");
				Race dwarf = AssetManager.raceLibrary.get("dwarf");
				Race elf = AssetManager.raceLibrary.get("elf");
				
				human.preferred_weapons.Remove("Mp5");
				human.preferred_weapons.Remove("Uzi");
				human.preferred_weapons.Remove("Sniper");
				human.preferred_weapons.Remove("RocketLauncher");
				human.preferred_weapons.Remove("mm9");
				human.preferred_weapons.Remove("Minigun");
				human.preferred_weapons.Remove("AK47");
				human.preferred_weapons.Remove("ThompsonM1A1");
				human.preferred_weapons.Remove("XM8");
				human.preferred_weapons.Remove("SGT44");
				human.preferred_weapons.Remove("M4A1"); // Remove M4A1 for human race
				human.preferred_weapons.Remove("FAMAS");
				human.preferred_weapons.Remove("malorian");
				human.preferred_weapons.Remove("PipePistol");				
				human.preferred_weapons.Remove("PipeRifle");	
				human.preferred_weapons.Remove("PipeShotgun");	
				human.preferred_weapons.Remove("SCAR");	
				human.preferred_weapons.Remove("AK103");	
				human.preferred_weapons.Remove("MP7");	
				human.preferred_weapons.Remove("Musket");	
				human.preferred_weapons.Remove("HK416");	
				human.preferred_weapons.Remove("M16");	
				human.preferred_weapons.Remove("DesertEagle");	
				human.preferred_weapons.Remove("Glock17");	
				
				orc.preferred_weapons.Remove("Mp5");
				orc.preferred_weapons.Remove("Uzi");
				orc.preferred_weapons.Remove("Sniper");
				orc.preferred_weapons.Remove("RocketLauncher");
				orc.preferred_weapons.Remove("mm9");
				orc.preferred_weapons.Remove("Minigun");
				orc.preferred_weapons.Remove("AK47");
				orc.preferred_weapons.Remove("ThompsonM1A1");
				orc.preferred_weapons.Remove("XM8");
				orc.preferred_weapons.Remove("SGT44");
				orc.preferred_weapons.Remove("M4A1"); // Remove M4A1 for orc race
				orc.preferred_weapons.Remove("FAMAS");
				orc.preferred_weapons.Remove("malorian");	
				orc.preferred_weapons.Remove("PipePistol");				
				orc.preferred_weapons.Remove("PipeRifle");	
				orc.preferred_weapons.Remove("PipeShotgun");	
				orc.preferred_weapons.Remove("SCAR");			
				orc.preferred_weapons.Remove("AK103");	
				orc.preferred_weapons.Remove("MP7");	
				orc.preferred_weapons.Remove("Musket");	
				orc.preferred_weapons.Remove("HK416");					
				orc.preferred_weapons.Remove("M16");	
				orc.preferred_weapons.Remove("DesertEagle");	
				orc.preferred_weapons.Remove("Glock17");	
				
				dwarf.preferred_weapons.Remove("Mp5");
				dwarf.preferred_weapons.Remove("Uzi");
				dwarf.preferred_weapons.Remove("Sniper");
				dwarf.preferred_weapons.Remove("RocketLauncher");
				dwarf.preferred_weapons.Remove("mm9");
				dwarf.preferred_weapons.Remove("Minigun");
				dwarf.preferred_weapons.Remove("AK47");
				dwarf.preferred_weapons.Remove("ThompsonM1A1");
				dwarf.preferred_weapons.Remove("XM8");
				dwarf.preferred_weapons.Remove("SGT44");
				dwarf.preferred_weapons.Remove("M4A1"); // Remove M4A1 for dwarf race
				dwarf.preferred_weapons.Remove("FAMAS");
				dwarf.preferred_weapons.Remove("malorian");
				dwarf.preferred_weapons.Remove("PipePistol");				
				dwarf.preferred_weapons.Remove("PipeRifle");	
				dwarf.preferred_weapons.Remove("PipeShotgun");	
				dwarf.preferred_weapons.Remove("SCAR");	
				dwarf.preferred_weapons.Remove("AK103");	
				dwarf.preferred_weapons.Remove("MP7");	
				dwarf.preferred_weapons.Remove("Musket");	
				dwarf.preferred_weapons.Remove("HK416");	
				dwarf.preferred_weapons.Remove("M16");	
				dwarf.preferred_weapons.Remove("DesertEagle");	
				dwarf.preferred_weapons.Remove("Glock17");			
				
				elf.preferred_weapons.Remove("Mp5");
				elf.preferred_weapons.Remove("Uzi");
				elf.preferred_weapons.Remove("Sniper");
				elf.preferred_weapons.Remove("RocketLauncher");
				elf.preferred_weapons.Remove("mm9");
				elf.preferred_weapons.Remove("Minigun");
				elf.preferred_weapons.Remove("AK47");
				elf.preferred_weapons.Remove("ThompsonM1A1");
				elf.preferred_weapons.Remove("XM8");
				elf.preferred_weapons.Remove("SGT44");
				elf.preferred_weapons.Remove("M4A1"); // Remove M4A1 for elf race
				elf.preferred_weapons.Remove("FAMAS");
				elf.preferred_weapons.Remove("malorian");		
				elf.preferred_weapons.Remove("SCAR");			
				elf.preferred_weapons.Remove("AK103");	
				elf.preferred_weapons.Remove("MP7");	
				elf.preferred_weapons.Remove("HK416");		
				elf.preferred_weapons.Remove("M16");	
				elf.preferred_weapons.Remove("DesertEagle");	
				elf.preferred_weapons.Remove("Glock17");			
				


            }
			
		public static void togglePipeGuns()
        {
            Main.modifyBoolOption("PipeGunOption", PowerButtons.GetToggleValue("PipeGun_toggle"));
            if (PowerButtons.GetToggleValue("PipeGun_toggle"))
            {
                turnOnPipeGuns();
                return;
            }
            turnOffPipeGuns();
        }
		

			public static void turnOnPipeGuns()
			{
                Race human = AssetManager.raceLibrary.get("human");
                Race orc = AssetManager.raceLibrary.get("orc");
                Race dwarf = AssetManager.raceLibrary.get("dwarf");
                Race elf = AssetManager.raceLibrary.get("elf");
				

				human.preferred_weapons.Add("PipePistol");				
				human.preferred_weapons.Add("PipeRifle");	
				human.preferred_weapons.Add("PipeShotgun");	
				human.preferred_weapons.Add("Musket");		


				orc.preferred_weapons.Add("PipePistol");				
				orc.preferred_weapons.Add("PipeRifle");	
				orc.preferred_weapons.Add("PipeShotgun");	
				orc.preferred_weapons.Add("Musket");					


				dwarf.preferred_weapons.Add("PipePistol");				
				dwarf.preferred_weapons.Add("PipeRifle");	
				dwarf.preferred_weapons.Add("PipeShotgun");	
				dwarf.preferred_weapons.Add("Musket");	

				
				elf.preferred_weapons.Add("PipePistol");				
				elf.preferred_weapons.Add("PipeRifle");	
				elf.preferred_weapons.Add("PipeShotgun");	
				elf.preferred_weapons.Add("Musket");	


            }
			public static void turnOffPipeGuns()
			{
                Race human = AssetManager.raceLibrary.get("human");
                Race orc = AssetManager.raceLibrary.get("orc");
                Race dwarf = AssetManager.raceLibrary.get("dwarf");
                Race elf = AssetManager.raceLibrary.get("elf");

 				human.preferred_weapons.Remove("PipePistol");				
				human.preferred_weapons.Remove("PipeRifle");	
				human.preferred_weapons.Remove("PipeShotgun");	
				human.preferred_weapons.Remove("Musket");		


				orc.preferred_weapons.Remove("PipePistol");				
				orc.preferred_weapons.Remove("PipeRifle");	
				orc.preferred_weapons.Remove("PipeShotgun");	
				orc.preferred_weapons.Remove("Musket");					


				dwarf.preferred_weapons.Remove("PipePistol");				
				dwarf.preferred_weapons.Remove("PipeRifle");	
				dwarf.preferred_weapons.Remove("PipeShotgun");	
				dwarf.preferred_weapons.Remove("Musket");	

				
				elf.preferred_weapons.Remove("PipePistol");				
				elf.preferred_weapons.Remove("PipeRifle");	
				elf.preferred_weapons.Remove("PipeShotgun");	
				elf.preferred_weapons.Remove("Musket");	
				


            }
		
			static void addgunSprite(string id, string material)
			{
				var dictItems = Reflection.GetField(typeof(ActorAnimationLoader), null, "dictItems") as Dictionary<string, Sprite>;
				var sprite = Resources.Load<Sprite>("ItemTextures/w_" + id + "_" + material);
				dictItems.Add(sprite.name, sprite);
			}
        }     	
    }
