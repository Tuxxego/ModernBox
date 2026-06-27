//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using NCMS;
using NCMS.Utils;

namespace ModernBox
{
    class Resourcez
    {
        public List<string> addResources = new List<string>();

        internal void init()
        {




			
	    ResourceAsset Parts = new ResourceAsset();
            Parts.id = "Parts";
            Parts.path_icon = "resources/iconResParts";
            AssetManager.resources.add(Parts);
            Parts.path_unit_item = "bag_food";
            Parts.type = ResType.Strategic;
            Parts.storage_max = 999;
            Parts.tradeBound = 200;
            Parts.tradeGive = 50;
            NCMS.Utils.Localization.addLocalization("Parts", "Parts");
            addResources.Add(Parts.id);
			

			
			ResourceAsset CyberWareParts = new ResourceAsset();
            CyberWareParts.id = "CyberWareParts";
            CyberWareParts.path_icon = "resources/iconResCyberWareParts";
            AssetManager.resources.add(CyberWareParts);
            CyberWareParts.path_unit_item = "bag_food";
            CyberWareParts.type = ResType.Strategic;
            CyberWareParts.storage_max = 999;
            CyberWareParts.tradeBound = 200;
            CyberWareParts.tradeGive = 50;
            NCMS.Utils.Localization.addLocalization("CyberWareParts", "CyberWareParts");
            addResources.Add(CyberWareParts.id);
			
			ResourceAsset Xenium = new ResourceAsset();
            Xenium.id = "Xenium";
            Xenium.path_icon = "resources/iconResXenium";
            AssetManager.resources.add(Xenium);
            Xenium.path_unit_item = "bag_food";
            Xenium.type = ResType.Strategic;
            Xenium.storage_max = 9999;
            Xenium.tradeBound = 200;
            Xenium.tradeGive = 50;
            NCMS.Utils.Localization.addLocalization("Xenium", "Xenium");
            addResources.Add(Xenium.id);
			
			ProjectileAsset bullet = new ProjectileAsset();
			bullet.id = "bullet";
			bullet.speed = 18f;
			bullet.texture = "w_bullet";
			bullet.parabolic = true;
			bullet.texture_shadow = "shadow_arrow";
			bullet.sound_launch = "event:/SFX/WEAPONS/WeaponStartArrow";
			bullet.sound_impact = "event:/SFX/HIT/HitGeneric";
			AssetManager.projectiles.add(bullet);


            ProjectileAsset NUKER = new ProjectileAsset();
            NUKER.id = "NUKER";
            NUKER.speed = 3.5f;
            NUKER.parabolic = true;
            NUKER.texture = "NUKER";
            NUKER.hitShake = true;
            NUKER.endEffect = "fx_explosion_nuke_atomic";
            NUKER.terraformOption = "NUKERExplode";
            NUKER.terraformRange = 43;
            NUKER.startScale = 0.01f;
            NUKER.targetScale = 0.2f;
            AssetManager.projectiles.add(NUKER);

            TerraformOptions NUKERExplode = new TerraformOptions();
            NUKERExplode.id = "NUKERExplode";
            NUKERExplode.flash = true;
            NUKERExplode.damageBuildings = true;
            NUKERExplode.damage = 10000;
            NUKERExplode.applyForce = true;
            NUKERExplode.explode_and_set_random_fire = true;
            NUKERExplode.explode_tile = true;
            NUKERExplode.explosion_pixel_effect = true;
            NUKERExplode.explode_strength = 1;
            NUKERExplode.transformToWasteland = true;
            NUKERExplode.shake = true;
            NUKERExplode.removeRuins = false;
            NUKERExplode.removeTornado = false;
            AssetManager.terraform.add(NUKERExplode);
			
			
            ProjectileAsset Blast = new ProjectileAsset();
            Blast.id = "Blast";
            Blast.speed = 15f;
            Blast.parabolic = true;
            Blast.texture = "firebomb";
            Blast.hitShake = true;
            Blast.endEffect = "fx_explosion_huge"; //fireballExplosion, tower (for dp search to make cannon later)
            Blast.terraformOption = "czarBomba";//make a new terraform remember :)
            Blast.terraformRange = 300;
            Blast.speed = 200f;
            Blast.startScale = 0.1f;
            Blast.targetScale = 0.1f;
            AssetManager.projectiles.add(Blast);


			  AssetManager.projectiles.add(new ProjectileAsset {
				id = "bullet",
				speed = 200f,
				texture = "w_bullet",
				parabolic = true,
				terraformOption = "gunshot",
				texture_shadow = "shadow_arrow",
				sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow",
				sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
				hitShake = false,
				terraformRange = 1,
			  });
			  
			  AssetManager.projectiles.add(new ProjectileAsset {
				id = "GunshipBullet",
				speed = 800f,
				texture = "w_bullet",
				parabolic = true,
				terraformOption = "gunshot",
				texture_shadow = "shadow_arrow",
				sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow",
				sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
				hitShake = false,
				terraformRange = 1,
			  });
  
  
			  AssetManager.terraform.add(new TerraformOptions {
				id = "gunshot",
				damage = 30,
				damageBuildings = false,
				explode_tile = true,
				removeBorders = false,
			  });
			  
			  ItemAsset PipeGun = new ItemAsset {
			  id = "PipeGun",
			  equipment_value = 100
			  };
			  PipeGun.setCost(1, "Parts", 1);
			  PipeGun.tech_needed = "LowFirearms";
			  NCMS.Utils.Localization.AddOrSet("item_mat_PipeGun", "Pipe Gun");
			  AssetManager.items_material_weapon.add(PipeGun);

			  ItemAsset Gun = new ItemAsset {
			  id = "Gun",
			  equipment_value = 100
			  };
			  Gun.setCost(1, "Parts", 1);
			  Gun.tech_needed = "Firearms";
			  NCMS.Utils.Localization.AddOrSet("item_mat_Gun", "Gun");
			  AssetManager.items_material_weapon.add(Gun);
			  
            TerraformOptions CanExplode = new TerraformOptions();
            CanExplode.id = "BlastExplode";
			CanExplode.shake = false;
			CanExplode.damageBuildings = true;
			CanExplode.damage = 30;
			CanExplode.setFire = true;
            AssetManager.terraform.add(CanExplode);

        }
		public static void toggleShake()
		{
			Main.modifyBoolOption("ShakeOption", PowerButtons.GetToggleValue("Shake_toggle"));
			if (PowerButtons.GetToggleValue("Shake_toggle"))
			{
				turnOnShake();
			}
			else
			{
				turnOffShake();
			}
		}
		public static void turnOnShake()
		{
			SetShake(true);
		}

		public static void turnOffShake()
		{
			SetShake(false);
		}
		
		private static void SetShake(bool f)
		{

				TerraformOptions e = AssetManager.terraform.get("CanExplode");
				if (e != null)
				{
					e.shake = f;
				}
			}
		}

    }
