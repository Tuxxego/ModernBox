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
    class MIRV
    {
        public static void init()
        {

            //makecraftable();
            

			
			            

			
			ItemAsset BudgetMIRV = AssetManager.items.clone("BudgetMIRV", "bow");
            BudgetMIRV.id = "BudgetMIRV";
            BudgetMIRV.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "MIRV_Names",
            });
            BudgetMIRV.materials = List.Of<string>(new string[] { "iron" });
            BudgetMIRV.projectile = "fireball";
            BudgetMIRV.setCost(1, "Xenium", 50);
            BudgetMIRV.base_stats[S.range] = 200f;
            BudgetMIRV.base_stats[S.accuracy] = 0;
            BudgetMIRV.base_stats[S.attack_speed] = 40f;
            BudgetMIRV.base_stats[S.damage] = 993;
            BudgetMIRV.base_stats[S.health] = 10;
			BudgetMIRV.tech_needed = "BudgetMIRV";
            BudgetMIRV.equipment_value = 1000;
            BudgetMIRV.path_slash_animation = "effects/slashes/slash_punch";
            BudgetMIRV.quality = ItemQuality.Legendary;
            BudgetMIRV.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(BudgetMIRV);
            Localization.addLocalization("item_BudgetMIRV", "Short Range MIRV");
            addMIRVSprite(BudgetMIRV.id, BudgetMIRV.materials[0]);
			
			ItemAsset DecentMIRV = AssetManager.items.clone("DecentMIRV", "bow");
            DecentMIRV.id = "DecentMIRV";
            DecentMIRV.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "MIRV_Names",
            });
            DecentMIRV.materials = List.Of<string>(new string[] { "iron" });
            DecentMIRV.projectile = "fireball";
            DecentMIRV.setCost(1, "Xenium", 50);
            DecentMIRV.base_stats[S.range] = 500f;
            DecentMIRV.base_stats[S.accuracy] = 0;
            DecentMIRV.base_stats[S.attack_speed] = 70f;
            DecentMIRV.base_stats[S.damage] = 993;
            DecentMIRV.base_stats[S.health] = 10;
			DecentMIRV.tech_needed = "DecentMIRV";
            DecentMIRV.equipment_value = 1000;
            DecentMIRV.path_slash_animation = "effects/slashes/slash_punch";
            DecentMIRV.quality = ItemQuality.Legendary;
            DecentMIRV.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(DecentMIRV);
            Localization.addLocalization("item_DecentMIRV", "Decent Range MIRV");
            addMIRVSprite(DecentMIRV.id, DecentMIRV.materials[0]);
			
			ItemAsset MIRV = AssetManager.items.clone("MIRV", "bow");
            MIRV.id = "MIRV";
            MIRV.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "MIRV_Names",
            });
            MIRV.materials = List.Of<string>(new string[] { "iron" });
            MIRV.projectile = "fireball";
            MIRV.setCost(1, "Xenium", 50);
            MIRV.base_stats[S.range] = 50000f;
            MIRV.base_stats[S.accuracy] = 0;
            MIRV.base_stats[S.attack_speed] = 101f;
            MIRV.base_stats[S.damage] = 993;
            MIRV.base_stats[S.health] = 10;
			MIRV.tech_needed = "MIRV";
            MIRV.equipment_value = 1000;
            MIRV.path_slash_animation = "effects/slashes/slash_punch";
            MIRV.quality = ItemQuality.Legendary;
            MIRV.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(MIRV);
            Localization.addLocalization("item_MIRV", "Long Range Ballistic MIRV");
            addMIRVSprite(MIRV.id, MIRV.materials[0]);
			
			ItemAsset MIRVBomb = AssetManager.items.clone("MIRVBomb", "bow");
            MIRVBomb.id = "MIRVBomb";
            MIRVBomb.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "MIRVBomb_Names",
            });
            MIRVBomb.materials = List.Of<string>(new string[] { "iron" });
            MIRVBomb.projectile = "fireball";
            MIRVBomb.setCost(1, "Xenium", 50);
            MIRVBomb.base_stats[S.range] = 0f;
            MIRVBomb.base_stats[S.accuracy] = 0f;
            MIRVBomb.base_stats[S.attack_speed] = 101f;
            MIRVBomb.base_stats[S.damage] = 993f;
            MIRVBomb.base_stats[S.health] = 10;
			MIRVBomb.tech_needed = "MIRV";
            MIRVBomb.equipment_value = 1000;
            MIRVBomb.path_slash_animation = "effects/slashes/slash_punch";
            MIRVBomb.quality = ItemQuality.Legendary;
            MIRVBomb.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(MIRVBomb);
            Localization.addLocalization("item_MIRVBomb", "MIRV Bomber Dropper");
            addMIRVSprite(MIRVBomb.id, MIRVBomb.materials[0]);
			
			ItemAsset STRONGMIRV = AssetManager.items.clone("STRONGMIRV", "bow");
            STRONGMIRV.id = "STRONGMIRV";
            STRONGMIRV.name_templates = Toolbox.splitStringIntoList(new string[]
            {
                        "STRONGMIRV_Names",
            });
            STRONGMIRV.materials = List.Of<string>(new string[] { "iron" });
            STRONGMIRV.projectile = "fireball";
            STRONGMIRV.setCost(1, "Xenium", 50);
            STRONGMIRV.base_stats[S.range] = 50000f;
            STRONGMIRV.base_stats[S.accuracy] = 0;
            STRONGMIRV.base_stats[S.attack_speed] = 1001f;
            STRONGMIRV.base_stats[S.damage] = 993;
            STRONGMIRV.base_stats[S.health] = 10;
			STRONGMIRV.tech_needed = "STRONGMIRV";
            STRONGMIRV.equipment_value = 1000;
            STRONGMIRV.path_slash_animation = "effects/slashes/slash_punch";
            STRONGMIRV.quality = ItemQuality.Legendary;
            STRONGMIRV.equipmentType = EquipmentType.Weapon;
            AssetManager.items.list.AddItem(STRONGMIRV);
            Localization.addLocalization("item_STRONGMIRV", "Nuclear Powered MIRV");
            addMIRVSprite(STRONGMIRV.id, STRONGMIRV.materials[0]);








		}
		public static void toggleMIRVS()
        {
            Main.modifyBoolOption("MIRVOption", PowerButtons.GetToggleValue("MIRV_toggle"));
            if (PowerButtons.GetToggleValue("MIRV_toggle"))
            {
                turnOnMIRV();
                return;
            }
            turnOffMIRV();
        }
		
		public static void toggleMIRVSSTRONG()
        {
            Main.modifyBoolOption("STRONGMIRVOption", PowerButtons.GetToggleValue("STRONGMIRV_toggle"));
            if (PowerButtons.GetToggleValue("STRONGMIRV_toggle"))
            {
                turnOnMIRVSTRONG();
                return;
            }
            turnOffMIRVSTRONG();
        }
		


			public static void turnOnMIRVSTRONG()
			{
				Race human = AssetManager.raceLibrary.get("human");
				Race orc = AssetManager.raceLibrary.get("orc");
				Race dwarf = AssetManager.raceLibrary.get("dwarf");
				Race elf = AssetManager.raceLibrary.get("elf");

				human.preferred_weapons.Add("STRONGMIRV");
				orc.preferred_weapons.Add("STRONGMIRV");
				dwarf.preferred_weapons.Add("STRONGMIRV");
				elf.preferred_weapons.Add("STRONGMIRV");

			}
			
			public static void turnOffMIRVSTRONG()
			{
				Race human = AssetManager.raceLibrary.get("human");
				Race orc = AssetManager.raceLibrary.get("orc");
				Race dwarf = AssetManager.raceLibrary.get("dwarf");
				Race elf = AssetManager.raceLibrary.get("elf");

				human.preferred_weapons.Remove("STRONGMIRV");
				orc.preferred_weapons.Remove("STRONGMIRV");
				dwarf.preferred_weapons.Remove("STRONGMIRV");
				elf.preferred_weapons.Remove("STRONGMIRV");

			}

			public static void turnOnMIRV()
			{
				Race human = AssetManager.raceLibrary.get("human");
				Race orc = AssetManager.raceLibrary.get("orc");
				Race dwarf = AssetManager.raceLibrary.get("dwarf");
				Race elf = AssetManager.raceLibrary.get("elf");

				human.preferred_weapons.Add("BudgetMIRV");
				human.preferred_weapons.Add("DecentMIRV");
				human.preferred_weapons.Add("MIRV");


				orc.preferred_weapons.Add("MIRV");
				orc.preferred_weapons.Add("BudgetMIRV");
				orc.preferred_weapons.Add("DecentMIRV");



				dwarf.preferred_weapons.Add("MIRV");
				dwarf.preferred_weapons.Add("BudgetMIRV");
				dwarf.preferred_weapons.Add("DecentMIRV");


				elf.preferred_weapons.Add("MIRV");
				elf.preferred_weapons.Add("BudgetMIRV");
				elf.preferred_weapons.Add("DecentMIRV");

			}
			
			public static void turnOffMIRV()
			{
				Race human = AssetManager.raceLibrary.get("human");
				Race orc = AssetManager.raceLibrary.get("orc");
				Race dwarf = AssetManager.raceLibrary.get("dwarf");
				Race elf = AssetManager.raceLibrary.get("elf");

				human.preferred_weapons.Remove("BudgetMIRV");
				human.preferred_weapons.Remove("DecentMIRV");
				human.preferred_weapons.Remove("MIRV");


				orc.preferred_weapons.Remove("MIRV");
				orc.preferred_weapons.Remove("BudgetMIRV");
				orc.preferred_weapons.Remove("DecentMIRV");



				dwarf.preferred_weapons.Remove("MIRV");
				dwarf.preferred_weapons.Remove("BudgetMIRV");
				dwarf.preferred_weapons.Remove("DecentMIRV");


				elf.preferred_weapons.Remove("MIRV");
				elf.preferred_weapons.Remove("BudgetMIRV");
				elf.preferred_weapons.Remove("DecentMIRV");

			}

		
			static void addMIRVSprite(string id, string material)
			{
				var dictItems = Reflection.GetField(typeof(ActorAnimationLoader), null, "dictItems") as Dictionary<string, Sprite>;
				var sprite = Resources.Load<Sprite>("ItemTextures/w_" + id + "_iron");

				if (sprite != null)
				{
					dictItems.Add(sprite.name, sprite);
				}
				else
				{
					Debug.LogError("Failed to load sprite for MIRV with ID: " + id);
				}
			}
        }     	
    }




	