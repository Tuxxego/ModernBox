//========= MODERNBOX 2.1.0.1 ============//
//
// Made by Tuxxego
//
//=============================================================================//
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
using ai;
using ai.behaviours;


namespace ModernBox
{
    class Vehicles : MonoBehaviour
    {

		public static bool nukesEnabled;
		public static bool balls;

		private sealed class AirVehicleProfile
		{
			public readonly string actorId;
			public readonly int ammoMax;
			public readonly int takeoffAmmoThreshold;
			public readonly float landingDistance;
			public readonly int reloadTickInterval;
			public readonly float reloadDurationSeconds;
			public readonly int fireTickInterval;
			public readonly int navTickInterval;
			public readonly string landedSpriteName;

			public AirVehicleProfile(string pActorId, int pAmmoMax, int pTakeoffAmmoThreshold, float pLandingDistance, int pReloadTickInterval, int pFireTickInterval, int pNavTickInterval, string pLandedSpriteName = "landed")
			{
				actorId = pActorId;
				ammoMax = Mathf.Max(1, pAmmoMax);
				takeoffAmmoThreshold = ammoMax;
				landingDistance = Mathf.Max(1f, pLandingDistance);
				reloadTickInterval = Mathf.Max(1, pReloadTickInterval);
				reloadDurationSeconds = Mathf.Max(1f, pReloadTickInterval);
				fireTickInterval = Mathf.Max(1, pFireTickInterval);
				navTickInterval = Mathf.Max(1, pNavTickInterval);
				landedSpriteName = string.IsNullOrEmpty(pLandedSpriteName) ? "landed" : pLandedSpriteName;
			}
		}

		private sealed class LandVehicleAmmoProfile
		{
			public readonly string actorId;
			public readonly int ammoMax;
			public readonly int reloadTickInterval;
			public readonly float reloadDurationSeconds;
			public readonly int navTickInterval;
			public readonly float reloadDistance;

			public LandVehicleAmmoProfile(string pActorId, int pAmmoMax, int pReloadTickInterval, int pNavTickInterval, float pReloadDistance)
			{
				actorId = pActorId;
				ammoMax = Mathf.Max(1, pAmmoMax);
				reloadTickInterval = Mathf.Max(1, pReloadTickInterval);
				reloadDurationSeconds = Mathf.Max(1f, pReloadTickInterval);
				navTickInterval = Mathf.Max(1, pNavTickInterval);
				reloadDistance = Mathf.Max(2f, pReloadDistance);
			}
		}

		private static readonly AirVehicleProfile DefaultAirVehicleProfile = new AirVehicleProfile("Bomber_Human", 4, 1, 7f, 35, 3, 4, "landed");
		private static readonly Dictionary<string, AirVehicleProfile> AirVehicleProfiles = new Dictionary<string, AirVehicleProfile>(StringComparer.Ordinal)
		{
			{ "Bomber_Human", new AirVehicleProfile("Bomber_Human", 4, 1, 7f, 5, 3, 4, "landed") },
			{ "Bomber_Ork", new AirVehicleProfile("Bomber_Ork", 4, 1, 7f, 5, 3, 4, "landed") },
			{ "Bomber_Dwarf", new AirVehicleProfile("Bomber_Dwarf", 4, 1, 7f, 5, 3, 4, "landed") },
			{ "Bomber_Gaia", new AirVehicleProfile("Bomber_Gaia", 4, 1, 7f, 5, 3, 4, "landed") },
			{ "EliteBomber", new AirVehicleProfile("EliteBomber", 2, 1, 7f, 5, 3, 4, "landed") },

			{ "Heli_Human", new AirVehicleProfile("Heli_Human", 30, 4, 6f, 5, 2, 3, "landed") },
			{ "Heli_Ork", new AirVehicleProfile("Heli_Ork", 30, 4, 6f, 5, 2, 3, "landed") },
			{ "Heli_Dwarf", new AirVehicleProfile("Heli_Dwarf", 30, 4, 6f, 5, 2, 3, "landed") },
			{ "Heli_Gaia", new AirVehicleProfile("Heli_Gaia", 30, 4, 6f, 5, 2, 3, "landed") },
			{ "Gunship", new AirVehicleProfile("Gunship", 20, 3, 6f, 5, 2, 3, "landed") },
			{ "FutureGunship", new AirVehicleProfile("Gunship", 20, 3, 6f, 5, 2, 3, "landed") },
			{ "HeliELite", new AirVehicleProfile("HeliELite", 20, 4, 6f, 5, 2, 3, "landed") },

			{ "FighterJet_Human", new AirVehicleProfile("FighterJet_Human", 8, 2, 6f, 5, 2, 3, "landed") },
			{ "FighterJet_Ork", new AirVehicleProfile("FighterJet_Ork", 8, 2, 6f, 5, 2, 3, "landed") },
			{ "FighterJet_Dwarf", new AirVehicleProfile("FighterJet_Dwarf", 8, 2, 6f, 5, 2, 3, "landed") },
			{ "FighterJet_Gaia", new AirVehicleProfile("FighterJet_Gaia", 8, 2, 6f, 5, 2, 3, "landed") },
			{ "TIEfighter", new AirVehicleProfile("TIEfighter", 10, 2, 6f, 5, 2, 3, "landed") },
			{ "F55FighterJet", new AirVehicleProfile("F55FighterJet", 8, 2, 6f, 5, 2, 3, "landed") }

			  
		};

		private static readonly string[] AirVehicleDecisionIds = new string[]
		{
			"check_swearing",
			"bomber_force_reload_rtb",
			"bomber_land_and_reload",
			"bomber_takeoff_for_war",
			"bomber_engage_enemy_targets",
			"bomber_peace_station"
		};

		private const string BomberAmmoCurrentKey = "bj_bomber_ammo_current";
		private const string BomberForceRtbKey = "bj_bomber_force_rtb";
		private const string BomberLandedKey = "bj_bomber_landed";
		private const string BomberReloadTickKey = "bj_bomber_reload_tick";
		private const string BomberReloadTimerKey = "bj_bomber_reload_timer";
		private const string BomberRepairPoolKey = "bj_bomber_repair_pool";
		private const string BomberFireTickKey = "bj_bomber_fire_tick";
		private const string BomberNavTickKey = "bj_bomber_nav_tick";
		private const string BomberTargetRefreshTickKey = "bj_bomber_target_refresh_tick";
		private const int AirTargetRefreshInterval = 8;
		private const int AirBuildingTargetRefreshInterval = 4;
		private const string LandVehicleAmmoCurrentKey = "bj_land_vehicle_ammo_current";
		private const string LandVehicleForceReloadKey = "bj_land_vehicle_force_reload";
		private const string LandVehicleReloadTickKey = "bj_land_vehicle_reload_tick";
		private const string LandVehicleReloadTimerKey = "bj_land_vehicle_reload_timer";
		private const string LandVehicleRepairPoolKey = "bj_land_vehicle_repair_pool";
		private const string LandVehicleNavTickKey = "bj_land_vehicle_nav_tick";
		private const float AirVehicleRepairPercentPerSecond = 2.5f;
		private const float LandVehicleRepairPercentPerSecond = 1.5f;
		private static readonly Dictionary<string, Sprite> _airVehicleLandedSpriteCache = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		private static readonly Dictionary<string, LandVehicleAmmoProfile> LandVehicleAmmoProfiles = new Dictionary<string, LandVehicleAmmoProfile>(StringComparer.Ordinal)
		{
			{ "modernhumvee_Human", new LandVehicleAmmoProfile("modernhumvee_Human", 24, 2, 4, 6f) },
			{ "modernhumvee_Ork", new LandVehicleAmmoProfile("modernhumvee_Ork", 24, 2, 4, 6f) },
			{ "modernhumvee_Dwarf", new LandVehicleAmmoProfile("modernhumvee_Dwarf", 24, 2, 4, 6f) },
			{ "modernhumvee_Gaia", new LandVehicleAmmoProfile("modernhumvee_Gaia", 24, 2, 4, 6f) },
			{ "wheeledtank_Human", new LandVehicleAmmoProfile("wheeledtank_Human", 16, 2, 4, 6f) },
			{ "wheeledtank_Ork", new LandVehicleAmmoProfile("wheeledtank_Ork", 16, 2, 4, 6f) },
			{ "wheeledtank_Dwarf", new LandVehicleAmmoProfile("wheeledtank_Dwarf", 16, 2, 4, 6f) },
			{ "wheeledtank_Gaia", new LandVehicleAmmoProfile("wheeledtank_Gaia", 16, 2, 4, 6f) },
			{ "howitzer_Human", new LandVehicleAmmoProfile("howitzer_Human", 8, 2, 5, 7f) },
			{ "howitzer_Ork", new LandVehicleAmmoProfile("howitzer_Ork", 8, 2, 5, 7f) },
			{ "howitzer_Dwarf", new LandVehicleAmmoProfile("howitzer_Dwarf", 8, 2, 5, 7f) },
			{ "howitzer_Gaia", new LandVehicleAmmoProfile("howitzer_Gaia", 8, 2, 5, 7f) },
			{ "Tank_Human", new LandVehicleAmmoProfile("Tank_Human", 12, 2, 4, 6f) },
			{ "Tank_Ork", new LandVehicleAmmoProfile("Tank_Ork", 12, 2, 4, 6f) },
			{ "Tank_Dwarf", new LandVehicleAmmoProfile("Tank_Dwarf", 12, 2, 4, 6f) },
			{ "teslatruckgun", new LandVehicleAmmoProfile("Tank_Human", 30, 2, 4, 6f) },
			{ "Terran", new LandVehicleAmmoProfile("Tank_Ork", 20, 2, 4, 6f) },
			{ "atstsniper", new LandVehicleAmmoProfile("Tank_Dwarf", 8, 2, 4, 6f) },
			{ "atst", new LandVehicleAmmoProfile("Tank_Human", 20, 2, 4, 6f) },
			{ "artilleryatst", new LandVehicleAmmoProfile("Tank_Ork", 5, 2, 4, 6f) },
			{ "P9000", new LandVehicleAmmoProfile("Tank_Dwarf", 7, 2, 4, 6f) },
			{ "Railgun", new LandVehicleAmmoProfile("Tank_Human", 10, 2, 4, 6f) },
			{ "AT9000", new LandVehicleAmmoProfile("Tank_Ork", 14, 2, 4, 6f) },
			{ "MA9000", new LandVehicleAmmoProfile("Tank_Dwarf", 7, 2, 4, 6f) },
			{ "dreadnaught", new LandVehicleAmmoProfile("Tank_Human", 40, 2, 4, 6f) },
			{ "dreadnaught_brrt", new LandVehicleAmmoProfile("Tank_Ork", 40, 2, 4, 6f) },
			{ "HumanTitan", new LandVehicleAmmoProfile("Tank_Dwarf", 40, 2, 4, 6f) },
			{ "Tank_Gaia", new LandVehicleAmmoProfile("Tank_Gaia", 12, 2, 4, 6f) }

		};

        public static void init()
        {
            baseVehicle();

        }

        private static void baseVehicle()
        {

            // Attacks
            //
            //=============================================================================//

			////////atttack with high recoil for artillery and low attack speed, tanks should have a pause too between attacks, and attacks for vehicles should consume different levels of stamina and if stamina depleted then give status to actor that recovers stamina while stunning vehicle/makewait, trait for heli for rockets similar to bomberman, attacks for land vehicles made so they cannot hurt flying vehicles
			////////each attack based on vehicle will consume different amounts of mana  (like bullets consuming 1 per shot, artillery shells consuming 20 per shot or more) , if cannot draw more mana vehicle will get stunned and recharge for a while and fuel "system" were movement = stamina depleted , to not expend it or to recharge it, vehicle will need to be in border of their own kingdom, if out of stamina vehicle will get stun until stamina is recovered by being on kingdom border
			////////
			////////spawn of vehicles based on what upgrade level the hall has and unitpotential should erase vehicles if they are below the corresponding tier of hall by ID

			WorldLogAsset balls = AssetManager.world_log_library.clone("bigballs", "$basic_disaster$");
			balls.locale_id = "NUCLEAR WEAPONS ARE IN THE AIR!";
			balls.path_icon = "ui/Icons/Nuke";

			AssetManager.world_log_library.add(balls);

			
	var cannonball = AssetManager.terraform.get("cannonball");
		cannonball.applies_to_high_flyers = false;

            EquipmentAsset mountedmachinegun = AssetManager.items.clone("mountedmachinegun", "$range");
            mountedmachinegun.has_locales = false;
            mountedmachinegun.projectile = "shotgun_bullet";
            mountedmachinegun.base_stats["projectiles"] = 1f;
            mountedmachinegun.path_slash_animation = "effects/slashes/slash_cannonball";
            mountedmachinegun.show_in_meta_editor = false;
            mountedmachinegun.show_in_knowledge_window = false;

			EquipmentAsset hordemachinegun = AssetManager.items.clone("hordemachinegun", "$range");
            hordemachinegun.has_locales = false;
            hordemachinegun.projectile = "bone";
            hordemachinegun.base_stats["projectiles"] = 1f;
            hordemachinegun.path_slash_animation = "effects/slashes/slash_cannonball";
            hordemachinegun.show_in_meta_editor = false;
            hordemachinegun.show_in_knowledge_window = false;

			EquipmentAsset icemachinegun = AssetManager.items.clone("icemachinegun", "$range");
            icemachinegun.has_locales = false;
            icemachinegun.projectile = "freeze_orb";
            icemachinegun.base_stats["projectiles"] = 1f;
            icemachinegun.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice");
            icemachinegun.path_slash_animation = "effects/slashes/slash_cannonball";
            icemachinegun.show_in_meta_editor = false;
            icemachinegun.show_in_knowledge_window = false;

			EquipmentAsset gaiamachinegun = AssetManager.items.clone("gaiamachinegun", "$range");
            gaiamachinegun.has_locales = false;
            gaiamachinegun.projectile = "green_orb";
            gaiamachinegun.base_stats["projectiles"] = 1f;
            gaiamachinegun.path_slash_animation = "effects/slashes/slash_cannonball";
            gaiamachinegun.show_in_meta_editor = false;
            gaiamachinegun.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("slowness");
            gaiamachinegun.show_in_knowledge_window = false;

			ProjectileAsset artilleryshell = new ProjectileAsset();
            artilleryshell.id = "artilleryshell";
            artilleryshell.look_at_target = true;
            artilleryshell.speed = ModernBoxPrefs.ScaleBalance(20f);
			artilleryshell.texture = "artilleryshell";
			artilleryshell.texture_shadow = "shadows/projectiles/shadow_ball";
			artilleryshell.terraform_option = "cannonball";
			artilleryshell.terraform_range = 2;
			artilleryshell.sound_launch = "event:/SFX/WEAPONS/WeaponShotgunStart";
			artilleryshell.sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand";
			artilleryshell.end_effect = "fx_firebomb_explosion";
			artilleryshell.scale_start = 0.3f;
			artilleryshell.scale_target = 0.3f;
          artilleryshell.can_be_left_on_ground = false;
          artilleryshell.can_be_blocked = false;
          AssetManager.projectiles.add(artilleryshell);

            EquipmentAsset artilleryattack = AssetManager.items.clone("artilleryattack", "$range");
            artilleryattack.has_locales = false;
            artilleryattack.projectile = "artilleryshell";
            artilleryattack.base_stats["recoil"] = 2f;
            artilleryattack.base_stats["projectiles"] = 1f;
            artilleryattack.path_slash_animation = "effects/slashes/slash_cannonball";
            artilleryattack.show_in_meta_editor = false;
            artilleryattack.show_in_knowledge_window = false;

			EquipmentAsset gaiaartilleryshell = AssetManager.items.clone("gaiaartilleryshell", "$range");
            gaiaartilleryshell.has_locales = false;
            gaiaartilleryshell.projectile = "green_orb";
            gaiaartilleryshell.base_stats["recoil"] = 2f;
            gaiaartilleryshell.base_stats["projectiles"] = 1f;
            gaiaartilleryshell.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("slowness");
            gaiaartilleryshell.path_slash_animation = "effects/slashes/slash_cannonball";
            gaiaartilleryshell.show_in_meta_editor = false;
            gaiaartilleryshell.show_in_knowledge_window = false;

			EquipmentAsset iceartilleryshell = AssetManager.items.clone("iceartilleryshell", "$range");
            iceartilleryshell.has_locales = false;
            iceartilleryshell.projectile = "snowball";
            iceartilleryshell.base_stats["recoil"] = 2f;
            iceartilleryshell.base_stats["projectiles"] = 1f;
            iceartilleryshell.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice");
            iceartilleryshell.path_slash_animation = "effects/slashes/slash_cannonball";
            iceartilleryshell.show_in_meta_editor = false;
            iceartilleryshell.show_in_knowledge_window = false;

			EquipmentAsset hordeartilleryshell = AssetManager.items.clone("hordeartilleryshell", "$range");
            hordeartilleryshell.has_locales = false;
            hordeartilleryshell.projectile = "skull";
            hordeartilleryshell.base_stats["recoil"] = 2f;
            hordeartilleryshell.base_stats["projectiles"] = 1f;
            hordeartilleryshell.path_slash_animation = "effects/slashes/slash_cannonball";
            hordeartilleryshell.show_in_meta_editor = false;
            hordeartilleryshell.show_in_knowledge_window = false;


		    ProjectileAsset tankshell = new ProjectileAsset();
            tankshell.id = "tankshell";
            tankshell.speed = ModernBoxPrefs.ScaleBalance(20f);
            tankshell.look_at_target = true;
			tankshell.texture = "artilleryshell";
			tankshell.texture_shadow = "shadows/projectiles/shadow_ball";
			tankshell.terraform_option = "cannonball";
			tankshell.terraform_range = 2;
			tankshell.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			tankshell.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			tankshell.end_effect = "fx_firebomb_explosion";
			tankshell.scale_start = 0.3f;
			tankshell.scale_target = 0.3f;
          tankshell.can_be_left_on_ground = true;
          tankshell.can_be_blocked = true;
          AssetManager.projectiles.add(tankshell);

            EquipmentAsset tankpew = AssetManager.items.clone("tankpew", "$range");
            tankpew.has_locales = false;
            tankpew.projectile = "tankshell";
            tankpew.base_stats["projectiles"] = 1f;
            tankpew.path_slash_animation = "effects/slashes/slash_cannonball";
            tankpew.show_in_meta_editor = false;
            tankpew.show_in_knowledge_window = false;

			EquipmentAsset hordetankpew = AssetManager.items.clone("hordetankpew", "$range");
            hordetankpew.has_locales = false;
            hordetankpew.projectile = "fireball";
            hordetankpew.base_stats["projectiles"] = 1f;
            hordetankpew.path_slash_animation = "effects/slashes/slash_cannonball";
            hordetankpew.show_in_meta_editor = false;
            hordetankpew.show_in_knowledge_window = false;

			ProjectileAsset grassshell = new ProjectileAsset();
            grassshell.id = "grassshell";
            grassshell.speed = ModernBoxPrefs.ScaleBalance(20f);
            grassshell.look_at_target = true;
			grassshell.texture = "pr_green_orb";
			grassshell.texture_shadow = "shadows/projectiles/shadow_ball";
			grassshell.sound_launch = "event:/SFX/WEAPONS/WeaponGreenOrbStart";
			grassshell.sound_impact = "event:/SFX/WEAPONS/WeaponGreenOrbLand";
			grassshell.end_effect = "fx_cast_top_green";
			grassshell.scale_start = 0.3f;
			grassshell.scale_target = 0.3f;
          grassshell.can_be_left_on_ground = true;
          grassshell.can_be_blocked = true;
          AssetManager.projectiles.add(grassshell);

		    EquipmentAsset gaiatankpew = AssetManager.items.clone("gaiatankpew", "$range");
            gaiatankpew.has_locales = false;
            gaiatankpew.projectile = "grassshell";
            gaiatankpew.base_stats["projectiles"] = 1f;
            gaiatankpew.path_slash_animation = "effects/slashes/slash_cannonball";
            gaiatankpew.show_in_meta_editor = false;
            gaiatankpew.show_in_knowledge_window = false;

			ProjectileAsset iceshell = new ProjectileAsset();
            iceshell.id = "iceshell";
            iceshell.speed = ModernBoxPrefs.ScaleBalance(20f);
            iceshell.look_at_target = true;
			iceshell.texture = "dark_orb";
			iceshell.texture_shadow = "shadows/projectiles/shadow_ball";
			iceshell.sound_launch = "event:/SFX/WEAPONS/WeaponFreezeOrbStart";
			iceshell.sound_impact = "event:/SFX/WEAPONS/WeaponFreezeOrbLand";
			iceshell.scale_start = 0.3f;
			iceshell.scale_target = 0.3f;
			iceshell.hit_freeze = true;
          iceshell.can_be_left_on_ground = true;
          iceshell.can_be_blocked = true;
          AssetManager.projectiles.add(iceshell);

			EquipmentAsset crystaltankpew = AssetManager.items.clone("crystaltankpew", "$range");
            crystaltankpew.has_locales = false;
            crystaltankpew.projectile = "iceshell";
            crystaltankpew.base_stats["projectiles"] = 1f;
            crystaltankpew.path_slash_animation = "effects/slashes/slash_cannonball";
            crystaltankpew.show_in_meta_editor = false;
            crystaltankpew.show_in_knowledge_window = false;

			ProjectileAsset missileartillery = new ProjectileAsset();
            missileartillery.id = "missileartillery";
            missileartillery.speed = ModernBoxPrefs.ScaleBalance(100f);
            missileartillery.look_at_target = true;
			missileartillery.texture = "missileartillery";
			missileartillery.texture_shadow = "shadows/projectiles/shadow_ball";
			missileartillery.terraform_option = "cannonball";
			missileartillery.terraform_range = 4;
			missileartillery.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			missileartillery.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			missileartillery.end_effect = "fx_firebomb_explosion";
			missileartillery.scale_start = 0.3f;
			missileartillery.scale_target = 0.3f;
          missileartillery.can_be_left_on_ground = false;
          missileartillery.can_be_blocked = false;
          AssetManager.projectiles.add(missileartillery);

            EquipmentAsset MissileSystemmissile = AssetManager.items.clone("MissileSystemmissile", "$range");
            MissileSystemmissile.has_locales = false;
            MissileSystemmissile.projectile = "missileartillery";
            MissileSystemmissile.base_stats["projectiles"] = 1f;
            MissileSystemmissile.path_slash_animation = "effects/slashes/slash_cannonball";
            MissileSystemmissile.show_in_meta_editor = false;
            MissileSystemmissile.show_in_knowledge_window = false;

			ProjectileAsset fireboneartillery = new ProjectileAsset();
            fireboneartillery.id = "fireboneartillery";
            fireboneartillery.speed = ModernBoxPrefs.ScaleBalance(100f);
            fireboneartillery.look_at_target = true;
			fireboneartillery.texture = "fireboneartillery";
			fireboneartillery.texture_shadow = "shadows/projectiles/shadow_ball";
			fireboneartillery.terraform_option = "cannonball";
			fireboneartillery.terraform_range = 4;
			fireboneartillery.sound_launch = "event:/SFX/WEAPONS/WeaponBoneProjectileStart";
			fireboneartillery.sound_impact = "event:/SFX/WEAPONS/WeaponBoneProjectileLand";
			fireboneartillery.end_effect = "fx_firebomb_explosion";
			fireboneartillery.scale_start = 0.3f;
			fireboneartillery.scale_target = 0.3f;
          fireboneartillery.can_be_left_on_ground = false;
          fireboneartillery.can_be_blocked = false;
          AssetManager.projectiles.add(fireboneartillery);

            EquipmentAsset MissileSystemHorde = AssetManager.items.clone("MissileSystemHorde", "$range");
            MissileSystemHorde.has_locales = false;
            MissileSystemHorde.projectile = "fireboneartillery";
            MissileSystemHorde.base_stats["projectiles"] = 1f;
            MissileSystemHorde.path_slash_animation = "effects/slashes/slash_cannonball";
            MissileSystemHorde.show_in_meta_editor = false;
            MissileSystemHorde.show_in_knowledge_window = false;

			ProjectileAsset frostmissileartillery = new ProjectileAsset();
            frostmissileartillery.id = "frostmissileartillery";
            frostmissileartillery.speed = ModernBoxPrefs.ScaleBalance(100f);
            frostmissileartillery.look_at_target = true;
			frostmissileartillery.texture = "frostmissileartillery";
			frostmissileartillery.texture_shadow = "shadows/projectiles/shadow_ball";
			frostmissileartillery.terraform_option = "cannonball";
			frostmissileartillery.terraform_range = 4;
			frostmissileartillery.sound_launch = "event:/SFX/WEAPONS/WeaponFreezeOrbStart";
			frostmissileartillery.sound_impact = "event:/SFX/WEAPONS/WeaponFreezeOrbLand";
			frostmissileartillery.end_effect = "fx_firebomb_explosion";
			frostmissileartillery.scale_start = 0.3f;
			frostmissileartillery.scale_target = 0.3f;
			frostmissileartillery.hit_freeze = true;
          frostmissileartillery.can_be_left_on_ground = false;
          frostmissileartillery.can_be_blocked = false;
          AssetManager.projectiles.add(frostmissileartillery);

            EquipmentAsset MissileSystemHarden = AssetManager.items.clone("MissileSystemHarden", "$range");
            MissileSystemHarden.has_locales = false;
            MissileSystemHarden.projectile = "frostmissileartillery";
            MissileSystemHarden.base_stats["projectiles"] = 1f;
            MissileSystemHarden.path_slash_animation = "effects/slashes/slash_cannonball";
            MissileSystemHarden.show_in_meta_editor = false;
            MissileSystemHarden.show_in_knowledge_window = false;

			ProjectileAsset plantmissileartillery = new ProjectileAsset();
            plantmissileartillery.id = "plantmissileartillery";
            plantmissileartillery.speed = ModernBoxPrefs.ScaleBalance(100f);
            plantmissileartillery.look_at_target = true;
			plantmissileartillery.texture = "plantmissileartillery";
			plantmissileartillery.texture_shadow = "shadows/projectiles/shadow_ball";
			plantmissileartillery.terraform_range = 4;
			plantmissileartillery.terraform_option = "cannonball";
			plantmissileartillery.sound_launch = "event:/SFX/WEAPONS/WeaponGreenOrbStart";
			plantmissileartillery.sound_impact = "event:/SFX/WEAPONS/WeaponGreenOrbLand";
			plantmissileartillery.end_effect = "fx_cast_top_green";
			plantmissileartillery.scale_start = 0.3f;
			plantmissileartillery.scale_target = 0.3f;
          plantmissileartillery.can_be_left_on_ground = false;
          plantmissileartillery.can_be_blocked = false;
          AssetManager.projectiles.add(plantmissileartillery);

            EquipmentAsset MissileSystemGaia = AssetManager.items.clone("MissileSystemGaia", "$range");
            MissileSystemGaia.has_locales = false;
            MissileSystemGaia.projectile = "plantmissileartillery";
            MissileSystemGaia.base_stats["projectiles"] = 1f;
            MissileSystemGaia.path_slash_animation = "effects/slashes/slash_cannonball";
            MissileSystemGaia.show_in_meta_editor = false;
            MissileSystemGaia.show_in_knowledge_window = false;


var AntiAirbomb = AssetManager.terraform.clone("AntiAirbomb", "grenade");
		AntiAirbomb.shake = false;
		AntiAirbomb.applies_to_high_flyers = true;
		AntiAirbomb.damage_buildings = true;
		AntiAirbomb.damage = 0;
		AntiAirbomb.explode_strength = 2;
		AntiAirbomb.apply_force = true;
		AntiAirbomb.force_power = 2f;
        AssetManager.terraform.add(AntiAirbomb);

			ProjectileAsset jetrocketprojectile = new ProjectileAsset();
            jetrocketprojectile.id = "jetrocketprojectile";
            jetrocketprojectile.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetrocketprojectile.look_at_target = true;
			jetrocketprojectile.texture = "jetrocketprojectile";
			jetrocketprojectile.texture_shadow = "shadows/projectiles/shadow_ball";
			jetrocketprojectile.terraform_option = "AntiAirbomb";
			jetrocketprojectile.terraform_range = 1;
			jetrocketprojectile.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetrocketprojectile.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetrocketprojectile.end_effect = "fx_firebomb_explosion";
			jetrocketprojectile.scale_start = 0.3f;
			jetrocketprojectile.scale_target = 0.3f;
          jetrocketprojectile.can_be_left_on_ground = true;
          jetrocketprojectile.can_be_blocked = true;
          AssetManager.projectiles.add(jetrocketprojectile);

            EquipmentAsset fighterattack = AssetManager.items.clone("fighterattack", "$range");
            fighterattack.has_locales = false;
            fighterattack.projectile = "jetrocketprojectile";
            fighterattack.base_stats["projectiles"] = 2f;
            fighterattack.path_slash_animation = "effects/slashes/slash_cannonball";
            fighterattack.show_in_meta_editor = false;
            fighterattack.show_in_knowledge_window = false;

			ProjectileAsset jetrocketprojectileHorde = new ProjectileAsset();
            jetrocketprojectileHorde.id = "jetrocketprojectileHorde";
            jetrocketprojectileHorde.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetrocketprojectileHorde.look_at_target = true;
			jetrocketprojectileHorde.texture = "jetrocketprojectileHorde";
			jetrocketprojectileHorde.texture_shadow = "shadows/projectiles/shadow_ball";
			jetrocketprojectileHorde.terraform_option = "AntiAirbomb";
			jetrocketprojectileHorde.terraform_range = 1;
			jetrocketprojectileHorde.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetrocketprojectileHorde.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetrocketprojectileHorde.end_effect = "fx_firebomb_explosion";
			jetrocketprojectileHorde.scale_start = 0.3f;
			jetrocketprojectileHorde.scale_target = 0.3f;
          jetrocketprojectileHorde.can_be_left_on_ground = true;
          jetrocketprojectileHorde.can_be_blocked = true;
          AssetManager.projectiles.add(jetrocketprojectileHorde);

            EquipmentAsset fighterattackHorde = AssetManager.items.clone("fighterattackHorde", "$range");
            fighterattackHorde.has_locales = false;
            fighterattackHorde.projectile = "jetrocketprojectileHorde";
            fighterattackHorde.base_stats["projectiles"] = 2f;
            fighterattackHorde.path_slash_animation = "effects/slashes/slash_cannonball";
            fighterattackHorde.show_in_meta_editor = false;
            fighterattackHorde.show_in_knowledge_window = false;


			ProjectileAsset jetrocketprojectileHarden = new ProjectileAsset();
            jetrocketprojectileHarden.id = "jetrocketprojectileHarden";
            jetrocketprojectileHarden.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetrocketprojectileHarden.look_at_target = true;
			jetrocketprojectileHarden.texture = "jetrocketprojectileHarden";
			jetrocketprojectileHarden.texture_shadow = "shadows/projectiles/shadow_ball";
			jetrocketprojectileHarden.terraform_option = "AntiAirbomb";
			jetrocketprojectileHarden.terraform_range = 1;
			jetrocketprojectileHarden.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetrocketprojectileHarden.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetrocketprojectileHarden.end_effect = "fx_firebomb_explosion";
			jetrocketprojectileHarden.scale_start = 0.3f;
			jetrocketprojectileHarden.scale_target = 0.3f;
          jetrocketprojectileHarden.can_be_left_on_ground = true;
          jetrocketprojectileHarden.can_be_blocked = true;
          AssetManager.projectiles.add(jetrocketprojectileHarden);

            EquipmentAsset fighterattackHarden = AssetManager.items.clone("fighterattackHarden", "$range");
            fighterattackHarden.has_locales = false;
            fighterattackHarden.projectile = "jetrocketprojectileHarden";
            fighterattackHarden.base_stats["projectiles"] = 2f;
            fighterattackHarden.path_slash_animation = "effects/slashes/slash_cannonball";
            fighterattackHarden.show_in_meta_editor = false;
            fighterattackHarden.show_in_knowledge_window = false;

            ProjectileAsset jetrocketprojectileGaia = new ProjectileAsset();
            jetrocketprojectileGaia.id = "jetrocketprojectileGaia";
            jetrocketprojectileGaia.speed = ModernBoxPrefs.ScaleBalance(20f);
			jetrocketprojectileGaia.look_at_target = true;
			jetrocketprojectileGaia.texture = "jetrocketprojectileGaia";
			jetrocketprojectileGaia.texture_shadow = "shadows/projectiles/shadow_ball";
			jetrocketprojectileGaia.terraform_option = "AntiAirbomb";
			jetrocketprojectileGaia.terraform_range = 1;
			jetrocketprojectileGaia.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetrocketprojectileGaia.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetrocketprojectileGaia.end_effect = "fx_firebomb_explosion";
			jetrocketprojectileGaia.scale_start = 0.3f;
			jetrocketprojectileGaia.scale_target = 0.3f;
          jetrocketprojectileGaia.can_be_left_on_ground = true;
          jetrocketprojectileGaia.can_be_blocked = true;
          AssetManager.projectiles.add(jetrocketprojectileGaia);

            EquipmentAsset fighterattackGaia = AssetManager.items.clone("fighterattackGaia", "$range");
            fighterattackGaia.has_locales = false;
            fighterattackGaia.projectile = "jetrocketprojectileGaia";
            fighterattackGaia.base_stats["projectiles"] = 2f;
            fighterattackGaia.path_slash_animation = "effects/slashes/slash_cannonball";
            fighterattackGaia.show_in_meta_editor = false;
            fighterattackGaia.show_in_knowledge_window = false;



			ProjectileAsset bigbomb = new ProjectileAsset();
            bigbomb.id = "bigbomb";
			bigbomb.speed = ModernBoxPrefs.ScaleBalance(18f);
			bigbomb.texture = "bigbomb";
			bigbomb.look_at_target = true;
			bigbomb.texture_shadow = "shadows/projectiles/shadow_ball";
			bigbomb.terraform_option = "cannonball";
			bigbomb.terraform_range = 6;
			bigbomb.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			bigbomb.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			bigbomb.end_effect = "fx_firebomb_explosion";
			bigbomb.scale_start = 0.3f;
			bigbomb.scale_target = 0.3f;
          bigbomb.can_be_left_on_ground = true;
          bigbomb.can_be_blocked = true;
          AssetManager.projectiles.add(bigbomb);

            EquipmentAsset BomberAttack = AssetManager.items.clone("BomberAttack", "$range");
            BomberAttack.has_locales = false;
            BomberAttack.projectile = "bigbomb";
			BomberAttack.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(220f);
            BomberAttack.base_stats["projectiles"] = 4f;
            BomberAttack.path_slash_animation = "effects/slashes/slash_cannonball";
            BomberAttack.show_in_meta_editor = false;
            BomberAttack.show_in_knowledge_window = false;

			ProjectileAsset bigbombGaia = new ProjectileAsset();
            bigbombGaia.id = "bigbombGaia";
			bigbombGaia.speed = ModernBoxPrefs.ScaleBalance(18f);
            bigbombGaia.look_at_target = true;
			bigbombGaia.texture = "bigbombGaia";
			bigbombGaia.texture_shadow = "shadows/projectiles/shadow_ball";
			bigbombGaia.terraform_option = "cannonball";
			bigbombGaia.terraform_range = 6;
			bigbombGaia.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			bigbombGaia.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			bigbombGaia.end_effect = "fx_firebomb_explosion";
			bigbombGaia.scale_start = 0.3f;
			bigbombGaia.scale_target = 0.3f;
          bigbombGaia.can_be_left_on_ground = true;
          bigbombGaia.can_be_blocked = true;
          AssetManager.projectiles.add(bigbombGaia);

            EquipmentAsset BomberAttackGaia = AssetManager.items.clone("BomberAttackGaia", "$range");
            BomberAttackGaia.has_locales = false;
            BomberAttackGaia.projectile = "bigbombGaia";
			BomberAttackGaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(220f);
            BomberAttackGaia.base_stats["projectiles"] = 4f;
            BomberAttackGaia.path_slash_animation = "effects/slashes/slash_cannonball";
            BomberAttackGaia.show_in_meta_editor = false;
            BomberAttackGaia.show_in_knowledge_window = false;

			ProjectileAsset bigbombHarden = new ProjectileAsset();
            bigbombHarden.id = "bigbombHarden";
			bigbombHarden.speed = ModernBoxPrefs.ScaleBalance(18f);
            bigbombHarden.look_at_target = true;
			bigbombHarden.texture = "bigbombHarden";
			bigbombHarden.texture_shadow = "shadows/projectiles/shadow_ball";
			bigbombHarden.terraform_option = "cannonball";
			bigbombHarden.terraform_range = 6;
			bigbombHarden.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			bigbombHarden.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			bigbombHarden.end_effect = "fx_firebomb_explosion";
			bigbombHarden.scale_start = 0.3f;
			bigbombHarden.scale_target = 0.3f;
          bigbombHarden.can_be_left_on_ground = true;
          bigbombHarden.can_be_blocked = true;
          AssetManager.projectiles.add(bigbombHarden);

            EquipmentAsset BomberAttackHarden = AssetManager.items.clone("BomberAttackHarden", "$range");
            BomberAttackHarden.has_locales = false;
            BomberAttackHarden.projectile = "bigbombHarden";
			BomberAttackHarden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(220f);
            BomberAttackHarden.base_stats["projectiles"] = 4f;
            BomberAttackHarden.path_slash_animation = "effects/slashes/slash_cannonball";
            BomberAttackHarden.show_in_meta_editor = false;
            BomberAttackHarden.show_in_knowledge_window = false;

			ProjectileAsset bigbombHorde = new ProjectileAsset();
            bigbombHorde.id = "bigbombHorde";
			bigbombHorde.speed = ModernBoxPrefs.ScaleBalance(18f);
            bigbombHorde.look_at_target = true;
			bigbombHorde.texture = "bigbombHorde";
			bigbombHorde.texture_shadow = "shadows/projectiles/shadow_ball";
			bigbombHorde.terraform_option = "cannonball";
			bigbombHorde.terraform_range = 6;
			bigbombHorde.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			bigbombHorde.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			bigbombHorde.end_effect = "fx_firebomb_explosion";
			bigbombHorde.scale_start = 0.3f;
			bigbombHorde.scale_target = 0.3f;
          bigbombHorde.can_be_left_on_ground = true;
          bigbombHorde.can_be_blocked = true;
          AssetManager.projectiles.add(bigbombHorde);

            EquipmentAsset BomberAttackHorde = AssetManager.items.clone("BomberAttackHorde", "$range");
            BomberAttackHorde.has_locales = false;
            BomberAttackHorde.projectile = "bigbombHorde";
			BomberAttackHorde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(220f);
            BomberAttackHorde.base_stats["projectiles"] = 4f;
            BomberAttackHorde.path_slash_animation = "effects/slashes/slash_cannonball";
            BomberAttackHorde.show_in_meta_editor = false;
            BomberAttackHorde.show_in_knowledge_window = false;


EffectAsset jetdropbomb_alliance = new EffectAsset();
jetdropbomb_alliance.id = "jetdropbomb_alliance";
jetdropbomb_alliance.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall";
jetdropbomb_alliance.use_basic_prefab = true;
jetdropbomb_alliance.sorting_layer_id = "EffectsTop";
jetdropbomb_alliance.sprite_path = "effects/jetdropbomb_alliance";
jetdropbomb_alliance.draw_light_area = true;
AssetManager.effects_library.add(jetdropbomb_alliance);


  		ProjectileAsset jetprojectile_alliance = new ProjectileAsset();
            jetprojectile_alliance.id = "jetprojectile_alliance";
            jetprojectile_alliance.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetprojectile_alliance.look_at_target = true;
			jetprojectile_alliance.texture = "jetprojectile_alliance";
			jetprojectile_alliance.texture_shadow = "shadows/projectiles/shadow_ball";
			jetprojectile_alliance.terraform_option = "cannonball";
			jetprojectile_alliance.terraform_range = 4;
			jetprojectile_alliance.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetprojectile_alliance.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetprojectile_alliance.end_effect = "jetdropbomb_alliance";
			jetprojectile_alliance.scale_start = 0.3f;
			jetprojectile_alliance.scale_target = 0.3f;
          jetprojectile_alliance.can_be_left_on_ground = false;
          jetprojectile_alliance.can_be_blocked = false;
          AssetManager.projectiles.add(jetprojectile_alliance);

            EquipmentAsset AirstrikejetAttack_alliance = AssetManager.items.clone("AirstrikejetAttack_alliance", "$range");
            AirstrikejetAttack_alliance.has_locales = false;
            AirstrikejetAttack_alliance.projectile = "jetprojectile_alliance";
            AirstrikejetAttack_alliance.base_stats["projectiles"] = 1f;
            AirstrikejetAttack_alliance.path_slash_animation = "effects/slashes/slash_cannonball";
            AirstrikejetAttack_alliance.show_in_meta_editor = false;
            AirstrikejetAttack_alliance.show_in_knowledge_window = false;


EffectAsset jetdropbomb_horde = new EffectAsset();
jetdropbomb_horde.id = "jetdropbomb_horde";
jetdropbomb_horde.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall";
jetdropbomb_horde.use_basic_prefab = true;
jetdropbomb_horde.sorting_layer_id = "EffectsTop";
jetdropbomb_horde.sprite_path = "effects/jetdropbomb_horde";
jetdropbomb_horde.draw_light_area = true;
AssetManager.effects_library.add(jetdropbomb_horde);

  		ProjectileAsset jetprojectile_horde = new ProjectileAsset();
            jetprojectile_horde.id = "jetprojectile_horde";
            jetprojectile_horde.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetprojectile_horde.look_at_target = true;
			jetprojectile_horde.texture = "jetprojectile_horde";
			jetprojectile_horde.texture_shadow = "shadows/projectiles/shadow_ball";
			jetprojectile_horde.terraform_option = "cannonball";
			jetprojectile_horde.terraform_range = 4;
			jetprojectile_horde.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetprojectile_horde.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetprojectile_horde.end_effect = "jetdropbomb_horde";
			jetprojectile_horde.scale_start = 0.3f;
			jetprojectile_horde.scale_target = 0.3f;
          jetprojectile_horde.can_be_left_on_ground = false;
          jetprojectile_horde.can_be_blocked = false;
          AssetManager.projectiles.add(jetprojectile_horde);

            EquipmentAsset AirstrikejetAttack_horde = AssetManager.items.clone("AirstrikejetAttack_horde", "$range");
            AirstrikejetAttack_horde.has_locales = false;
            AirstrikejetAttack_horde.projectile = "jetprojectile_horde";
            AirstrikejetAttack_horde.base_stats["projectiles"] = 1f;
            AirstrikejetAttack_horde.path_slash_animation = "effects/slashes/slash_cannonball";
            AirstrikejetAttack_horde.show_in_meta_editor = false;
            AirstrikejetAttack_horde.show_in_knowledge_window = false;


EffectAsset jetdropbomb_gaia = new EffectAsset();
jetdropbomb_gaia.id = "jetdropbomb_gaia";
jetdropbomb_gaia.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall";
jetdropbomb_gaia.use_basic_prefab = true;
jetdropbomb_gaia.sorting_layer_id = "EffectsTop";
jetdropbomb_gaia.sprite_path = "effects/jetdropbomb_gaia";
jetdropbomb_gaia.draw_light_area = true;
AssetManager.effects_library.add(jetdropbomb_gaia);

  		ProjectileAsset jetprojectile_gaia = new ProjectileAsset();
            jetprojectile_gaia.id = "jetprojectile_gaia";
            jetprojectile_gaia.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetprojectile_gaia.look_at_target = true;
			jetprojectile_gaia.texture = "jetprojectile_gaia";
			jetprojectile_gaia.texture_shadow = "shadows/projectiles/shadow_ball";
			jetprojectile_gaia.terraform_option = "cannonball";
			jetprojectile_gaia.terraform_range = 4;
			jetprojectile_gaia.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetprojectile_gaia.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetprojectile_gaia.end_effect = "jetdropbomb_gaia";
			jetprojectile_gaia.scale_start = 0.3f;
			jetprojectile_gaia.scale_target = 0.3f;
          jetprojectile_gaia.can_be_left_on_ground = false;
          jetprojectile_gaia.can_be_blocked = false;
          AssetManager.projectiles.add(jetprojectile_gaia);

            EquipmentAsset AirstrikejetAttack_gaia = AssetManager.items.clone("AirstrikejetAttack_gaia", "$range");
            AirstrikejetAttack_gaia.has_locales = false;
            AirstrikejetAttack_gaia.projectile = "jetprojectile_gaia";
            AirstrikejetAttack_gaia.base_stats["projectiles"] = 1f;
            AirstrikejetAttack_gaia.path_slash_animation = "effects/slashes/slash_cannonball";
            AirstrikejetAttack_gaia.show_in_meta_editor = false;
            AirstrikejetAttack_gaia.show_in_knowledge_window = false;


EffectAsset jetdropbomb_harden = new EffectAsset();
jetdropbomb_harden.id = "jetdropbomb_harden";
jetdropbomb_harden.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall";
jetdropbomb_harden.use_basic_prefab = true;
jetdropbomb_harden.sorting_layer_id = "EffectsTop";
jetdropbomb_harden.sprite_path = "effects/jetdropbomb_harden";
jetdropbomb_harden.draw_light_area = true;
AssetManager.effects_library.add(jetdropbomb_harden);

  		ProjectileAsset jetprojectile_harden = new ProjectileAsset();
            jetprojectile_harden.id = "jetprojectile_harden";
            jetprojectile_harden.speed = ModernBoxPrefs.ScaleBalance(20f);
            jetprojectile_harden.look_at_target = true;
			jetprojectile_harden.texture = "jetprojectile_harden";
			jetprojectile_harden.texture_shadow = "shadows/projectiles/shadow_ball";
			jetprojectile_harden.terraform_option = "cannonball";
			jetprojectile_harden.terraform_range = 4;
			jetprojectile_harden.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			jetprojectile_harden.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			jetprojectile_harden.end_effect = "jetdropbomb_harden";
			jetprojectile_harden.scale_start = 0.3f;
			jetprojectile_harden.scale_target = 0.3f;
          jetprojectile_harden.can_be_left_on_ground = false;
          jetprojectile_harden.can_be_blocked = false;
          AssetManager.projectiles.add(jetprojectile_harden);

            EquipmentAsset AirstrikejetAttack_harden = AssetManager.items.clone("AirstrikejetAttack_harden", "$range");
            AirstrikejetAttack_harden.has_locales = false;
            AirstrikejetAttack_harden.projectile = "jetprojectile_harden";
            AirstrikejetAttack_harden.base_stats["projectiles"] = 1f;
            AirstrikejetAttack_harden.path_slash_animation = "effects/slashes/slash_cannonball";
            AirstrikejetAttack_harden.show_in_meta_editor = false;
            AirstrikejetAttack_harden.show_in_knowledge_window = false;


            EffectAsset hyperboom = new EffectAsset();
hyperboom.id = "hyperboom";
hyperboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
hyperboom.use_basic_prefab = true;
hyperboom.sorting_layer_id = "EffectsTop";
hyperboom.sprite_path = "effects/hyperboom";
hyperboom.draw_light_area = true;
AssetManager.effects_library.add(hyperboom);

  		ProjectileAsset hyperkame = new ProjectileAsset();
            hyperkame.id = "hyperkame";
            hyperkame.speed = ModernBoxPrefs.ScaleBalance(20f);
            hyperkame.look_at_target = true;
			hyperkame.texture = "hyperkame";
			hyperkame.texture_shadow = "shadows/projectiles/shadow_ball";
			hyperkame.terraform_option = "AntiAirbomb";
			hyperkame.terraform_range = 8;
			hyperkame.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			hyperkame.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			hyperkame.end_effect = "hyperboom";
			hyperkame.scale_start = 0.05f;
			hyperkame.scale_target = 0.4f;
          hyperkame.can_be_left_on_ground = false;
          hyperkame.can_be_blocked = false;
          AssetManager.projectiles.add(hyperkame);

            EquipmentAsset XenoMegaBomb = AssetManager.items.clone("XenoMegaBomb", "$range");
            XenoMegaBomb.has_locales = false;
            XenoMegaBomb.projectile = "hyperkame";
            XenoMegaBomb.base_stats["projectiles"] = 1f;
            XenoMegaBomb.path_slash_animation = "effects/slashes/slash_cannonball";
            XenoMegaBomb.show_in_meta_editor = false;
            XenoMegaBomb.show_in_knowledge_window = false;

                        EffectAsset kameboomtest = new EffectAsset();
kameboomtest.id = "kameboomtest";
kameboomtest.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
kameboomtest.use_basic_prefab = true;
kameboomtest.sorting_layer_id = "EffectsTop";
kameboomtest.sprite_path = "effects/kameboomtest";
kameboomtest.draw_light_area = true;
AssetManager.effects_library.add(kameboomtest);

            EffectAsset fx_trail_kame_t = new EffectAsset();
fx_trail_kame_t.id = "fx_trail_kame_t";
fx_trail_kame_t.use_basic_prefab = true;
fx_trail_kame_t.sorting_layer_id = "EffectsTop";
fx_trail_kame_t.sprite_path = "effects/fx_trail_kame_t";
fx_trail_kame_t.draw_light_area = true;
AssetManager.effects_library.add(fx_trail_kame_t);

  		ProjectileAsset thunderplasma = new ProjectileAsset();
            thunderplasma.id = "thunderplasma";
            thunderplasma.speed = 16f;
			thunderplasma.texture = "thunderplasma";
			thunderplasma.look_at_target = true;
			thunderplasma.look_at_target = true;
			thunderplasma.trail_effect_enabled = true;
			thunderplasma.trail_effect_id = "fx_trail_kame_t";
            thunderplasma.trail_effect_scale = 0.1f;
			thunderplasma.trail_effect_timer = 0.1f;
			thunderplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			thunderplasma.terraform_option = "AntiAirbomb";
			thunderplasma.terraform_range = 4;
			thunderplasma.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			thunderplasma.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			thunderplasma.end_effect = "kameboomtest";
			thunderplasma.scale_start = 0.4f;
			thunderplasma.scale_target = 0.4f;
          thunderplasma.can_be_left_on_ground = false;
          thunderplasma.can_be_blocked = false;
          AssetManager.projectiles.add(thunderplasma);

            EquipmentAsset XenoBeam = AssetManager.items.clone("XenoBeam", "$range");
            XenoBeam.has_locales = false;
            XenoBeam.projectile = "thunderplasma";
            XenoBeam.base_stats["projectiles"] = 1f;
            XenoBeam.path_slash_animation = "effects/slashes/slash_cannonball";
            XenoBeam.show_in_meta_editor = false;
            XenoBeam.show_in_knowledge_window = false;

            EquipmentAsset XenoPew = AssetManager.items.clone("XenoPew", "$range");
            XenoPew.has_locales = false;
            XenoPew.projectile = "plasma_ball";
            XenoPew.base_stats["projectiles"] = 1f;
            XenoPew.path_slash_animation = "effects/slashes/slash_cannonball";
            XenoPew.show_in_meta_editor = false;
            XenoPew.show_in_knowledge_window = false;



			ProjectileAsset Stone = new ProjectileAsset();
            Stone.id = "Stone";
            Stone.speed = ModernBoxPrefs.ScaleBalance(20f);
			Stone.texture = "Stone";
			Stone.look_at_target = true;
			Stone.texture_shadow = "shadows/projectiles/shadow_ball";
			Stone.terraform_option = "cannonball";
			Stone.terraform_range = 1;
			Stone.scale_start = 0.3f;
			Stone.scale_target = 0.3f;
          Stone.can_be_left_on_ground = true;
          Stone.can_be_blocked = true;
          AssetManager.projectiles.add(Stone);

            EquipmentAsset StoneThrow = AssetManager.items.clone("StoneThrow", "$range");
            StoneThrow.has_locales = false;
            StoneThrow.projectile = "Stone";
            StoneThrow.base_stats["projectiles"] = 1f;
            StoneThrow.path_slash_animation = "effects/slashes/slash_cannonball";
            StoneThrow.show_in_meta_editor = false;
            StoneThrow.show_in_knowledge_window = false;

ProjectileAsset bigsnowball = new ProjectileAsset();
            bigsnowball.id = "bigsnowball";
            bigsnowball.speed = ModernBoxPrefs.ScaleBalance(20f);
			bigsnowball.texture = "bigsnowball";
			bigsnowball.look_at_target = true;
			bigsnowball.texture_shadow = "shadows/projectiles/shadow_ball";
			bigsnowball.terraform_option = "cannonball";
			bigsnowball.terraform_range = 1;
			bigsnowball.scale_start = 0.3f;
			bigsnowball.scale_target = 0.3f;
			bigsnowball.hit_freeze = true;
          bigsnowball.can_be_left_on_ground = true;
          bigsnowball.can_be_blocked = true;
          AssetManager.projectiles.add(bigsnowball);

            EquipmentAsset SnowThrow = AssetManager.items.clone("SnowThrow", "$range");
            SnowThrow.has_locales = false;
            SnowThrow.projectile = "bigsnowball";
            SnowThrow.base_stats["projectiles"] = 1f;
            SnowThrow.path_slash_animation = "effects/slashes/slash_cannonball";
            SnowThrow.show_in_meta_editor = false;
            SnowThrow.show_in_knowledge_window = false;
            SnowThrow.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice");

			EquipmentAsset DavinciBarrage = AssetManager.items.clone("DavinciBarrage", "$range");
            DavinciBarrage.has_locales = false;
            DavinciBarrage.projectile = "cannonball";
            DavinciBarrage.base_stats["projectiles"] = 2f;
            DavinciBarrage.path_slash_animation = "effects/slashes/slash_cannonball";
            DavinciBarrage.show_in_meta_editor = false;
            DavinciBarrage.show_in_knowledge_window = false;


            EquipmentAsset FireBomb = AssetManager.items.clone("FireBomb", "$range");
            FireBomb.has_locales = false;
            FireBomb.projectile = "green_orb";
            FireBomb.base_stats["projectiles"] = 1f;
            FireBomb.path_slash_animation = "effects/slashes/slash_cannonball";
            FireBomb.show_in_meta_editor = false;
            FireBomb.show_in_knowledge_window = false;
            FireBomb.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

  EquipmentAsset GreenSpray = AssetManager.items.clone("GreenSpray", "$range");
            GreenSpray.has_locales = false;
            GreenSpray.projectile = "green_orb";
            GreenSpray.base_stats["projectiles"] = ModernBoxPrefs.ScaleBalance(10f);
            GreenSpray.path_slash_animation = "effects/slashes/slash_cannonball";
            GreenSpray.show_in_meta_editor = false;
            GreenSpray.show_in_knowledge_window = false;
            GreenSpray.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("slowness");

            EquipmentAsset IceSnipe = AssetManager.items.clone("IceSnipe", "$range");
            IceSnipe.has_locales = false;
            IceSnipe.projectile = "freeze_orb";
            IceSnipe.base_stats["projectiles"] = 1f;
            IceSnipe.path_slash_animation = "effects/slashes/slash_cannonball";
            IceSnipe.show_in_meta_editor = false;
            IceSnipe.show_in_knowledge_window = false;
            IceSnipe.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice");





			ProjectileAsset blueplasma = new ProjectileAsset();
			blueplasma.id = "blueplasma";
			blueplasma.speed = 15f;
			blueplasma.look_at_target = true;
			blueplasma.texture = "blueplasma";
			blueplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			blueplasma.terraform_option = "AntiAirbomb";
			blueplasma.terraform_range = 1;
			blueplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			blueplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			blueplasma.scale_start = 0.07f;
			blueplasma.scale_target = 0.07f;
			blueplasma.can_be_left_on_ground = false;
			blueplasma.can_be_blocked = false;
			AssetManager.projectiles.add(blueplasma);

			ProjectileAsset greenplasma = new ProjectileAsset();
			greenplasma.id = "greenplasma";
			greenplasma.speed = 15f;
			greenplasma.look_at_target = true;
			greenplasma.texture = "greenplasma";
			greenplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			greenplasma.terraform_option = "AntiAirbomb";
			greenplasma.terraform_range = 1;
			greenplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			greenplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			greenplasma.scale_start = 0.07f;
			greenplasma.scale_target = 0.07f;
			greenplasma.can_be_left_on_ground = false;
			greenplasma.can_be_blocked = false;
			AssetManager.projectiles.add(greenplasma);

			EffectAsset redmediumboom = new EffectAsset();
			redmediumboom.id = "redmediumboom";
			redmediumboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall";
			redmediumboom.use_basic_prefab = true;
			redmediumboom.sorting_layer_id = "EffectsTop";
			redmediumboom.sprite_path = "effects/redplasmaboom";
			redmediumboom.draw_light_area = true;
			AssetManager.effects_library.add(redmediumboom);


			EffectAsset greenmediumboom = new EffectAsset();
			greenmediumboom.id = "greenmediumboom";
			greenmediumboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall";
			greenmediumboom.use_basic_prefab = true;
			greenmediumboom.sorting_layer_id = "EffectsTop";
			greenmediumboom.sprite_path = "effects/greenplasmaboom";
			greenmediumboom.draw_light_area = true;
			AssetManager.effects_library.add(greenmediumboom);

			ProjectileAsset redmediumplasma = new ProjectileAsset();
			redmediumplasma.id = "redmediumplasma";
			redmediumplasma.speed = 15f;
			redmediumplasma.look_at_target = true;
			redmediumplasma.texture = "redplasma";
			redmediumplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			redmediumplasma.terraform_option = "cannonball";
			redmediumplasma.end_effect = "redmediumboom";
			redmediumplasma.terraform_range = 2;
			redmediumplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			redmediumplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			redmediumplasma.scale_start = 0.3f;
			redmediumplasma.scale_target = 0.3f;
			redmediumplasma.can_be_left_on_ground = false;
			redmediumplasma.can_be_blocked = false;
			AssetManager.projectiles.add(redmediumplasma);


			ProjectileAsset aerialredmediumplasma = new ProjectileAsset();
			aerialredmediumplasma.id = "aerialredmediumplasma";
			aerialredmediumplasma.speed = 15f;
			aerialredmediumplasma.look_at_target = true;
			aerialredmediumplasma.texture = "redplasma";
			aerialredmediumplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			aerialredmediumplasma.terraform_option = "AntiAirbomb";
			aerialredmediumplasma.end_effect = "redmediumboom";
			aerialredmediumplasma.terraform_range = 2;
			aerialredmediumplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			aerialredmediumplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			aerialredmediumplasma.scale_start = 0.3f;
			aerialredmediumplasma.scale_target = 0.3f;
			aerialredmediumplasma.can_be_left_on_ground = false;
			aerialredmediumplasma.can_be_blocked = false;
			AssetManager.projectiles.add(aerialredmediumplasma);


			ProjectileAsset greenmediumplasma = new ProjectileAsset();
			greenmediumplasma.id = "greenmediumplasma";
			greenmediumplasma.speed = 15f;
			greenmediumplasma.look_at_target = true;
			greenmediumplasma.texture = "greenplasma";
			greenmediumplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			greenmediumplasma.terraform_option = "cannonball";
			greenmediumplasma.end_effect = "greenmediumboom";
			greenmediumplasma.terraform_range = 2;
			greenmediumplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			greenmediumplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			greenmediumplasma.scale_start = 0.3f;
			greenmediumplasma.scale_target = 0.3f;
			greenmediumplasma.can_be_left_on_ground = false;
			greenmediumplasma.can_be_blocked = false;
			AssetManager.projectiles.add(greenmediumplasma);


			EffectAsset greenbigboom = new EffectAsset();
			greenbigboom.id = "greenbigboom";
			greenbigboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny";
			greenbigboom.use_basic_prefab = true;
			greenbigboom.sorting_layer_id = "EffectsTop";
			greenbigboom.sprite_path = "effects/greenbigboom";
			greenbigboom.draw_light_area = true;
			AssetManager.effects_library.add(greenbigboom);

			ProjectileAsset biggreenplasma = new ProjectileAsset();
			biggreenplasma.id = "biggreenplasma";
			biggreenplasma.speed = 15f;
			biggreenplasma.look_at_target = true;
			biggreenplasma.texture = "greenplasma";
			biggreenplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			biggreenplasma.terraform_option = "cannonball";
			biggreenplasma.end_effect = "greenbigboom";
			biggreenplasma.terraform_range = 2;
			biggreenplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			biggreenplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			biggreenplasma.scale_start = 0.6f;
			biggreenplasma.scale_target = 0.6f;
			biggreenplasma.can_be_left_on_ground = false;
			biggreenplasma.can_be_blocked = false;
			AssetManager.projectiles.add(biggreenplasma);


			EffectAsset bluebigboom = new EffectAsset();
			bluebigboom.id = "bluebigboom";
			bluebigboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny";
			bluebigboom.use_basic_prefab = true;
			bluebigboom.sorting_layer_id = "EffectsTop";
			bluebigboom.sprite_path = "effects/blueplasmaboom";
			bluebigboom.draw_light_area = true;
			AssetManager.effects_library.add(bluebigboom);

			ProjectileAsset bigblueplasma = new ProjectileAsset();
			bigblueplasma.id = "bigblueplasma";
			bigblueplasma.speed = 20f;
			bigblueplasma.look_at_target = true;
			bigblueplasma.texture = "blueplasma";
			bigblueplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			bigblueplasma.terraform_option = "cannonball";
			bigblueplasma.terraform_range = 2;
			bigblueplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			bigblueplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			bigblueplasma.end_effect = "bluebigboom";
			bigblueplasma.scale_start = 0.6f;
			bigblueplasma.scale_target = 0.6f;
			bigblueplasma.can_be_left_on_ground = true;
			bigblueplasma.can_be_blocked = true;
			AssetManager.projectiles.add(bigblueplasma);


			EffectAsset redbigboom = new EffectAsset();
			redbigboom.id = "redbigboom";
			redbigboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny";
			redbigboom.use_basic_prefab = true;
			redbigboom.sorting_layer_id = "EffectsTop";
			redbigboom.sprite_path = "effects/redbigboom";
			redbigboom.draw_light_area = true;
			AssetManager.effects_library.add(redbigboom);

			ProjectileAsset redbigplasma = new ProjectileAsset();
			redbigplasma.id = "redbigplasma";
			redbigplasma.speed = 20f;
			redbigplasma.look_at_target = true;
			redbigplasma.texture = "redplasma";
			redbigplasma.texture_shadow = "shadows/projectiles/shadow_ball";
			redbigplasma.terraform_option = "cannonball";
			redbigplasma.terraform_range = 2;
			redbigplasma.sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart";
			redbigplasma.sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand";
			redbigplasma.end_effect = "redbigboom";
			redbigplasma.scale_start = 0.6f;
			redbigplasma.scale_target = 0.6f;
			redbigplasma.can_be_left_on_ground = true;
			redbigplasma.can_be_blocked = true;
			AssetManager.projectiles.add(redbigplasma);

			EquipmentAsset blueplasmabig = AssetManager.items.clone("blueplasmabig", "$range");
			blueplasmabig.has_locales = false;
			blueplasmabig.projectile = "bigblueplasma";
			blueplasmabig.base_stats["projectiles"] = 1f;
			blueplasmabig.path_slash_animation = "effects/slashes/slash_cannonball";
			blueplasmabig.show_in_meta_editor = false;
			blueplasmabig.show_in_knowledge_window = false;

			EquipmentAsset redbigplasmashot = AssetManager.items.clone("redbigplasmashot", "$range");
			redbigplasmashot.has_locales = false;
			redbigplasmashot.projectile = "redbigplasma";
			redbigplasmashot.base_stats["projectiles"] = 1f;
			redbigplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			redbigplasmashot.show_in_meta_editor = false;
			redbigplasmashot.show_in_knowledge_window = false;
			redbigplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			EquipmentAsset blueplasmashot = AssetManager.items.clone("blueplasmashot", "$range");
			blueplasmashot.has_locales = false;
			blueplasmashot.projectile = "blueplasma";
			blueplasmashot.base_stats["projectiles"] = 1f;
			blueplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			blueplasmashot.show_in_meta_editor = false;
			blueplasmashot.show_in_knowledge_window = false;
			blueplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			EquipmentAsset greenplasmashot = AssetManager.items.clone("greenplasmashot", "$range");
			greenplasmashot.has_locales = false;
			greenplasmashot.projectile = "greenplasma";
			greenplasmashot.base_stats["projectiles"] = 1f;
			greenplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			greenplasmashot.show_in_meta_editor = false;
			greenplasmashot.show_in_knowledge_window = false;
			greenplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			EquipmentAsset greenmediumplasmashot = AssetManager.items.clone("greenmediumplasmashot", "$range");
			greenmediumplasmashot.has_locales = false;
			greenmediumplasmashot.projectile = "greenmediumplasma";
			greenmediumplasmashot.base_stats["projectiles"] = 1f;
			greenmediumplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			greenmediumplasmashot.show_in_meta_editor = false;
			greenmediumplasmashot.show_in_knowledge_window = false;
			greenmediumplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			EquipmentAsset redmediumplasmashot = AssetManager.items.clone("redmediumplasmashot", "$range");
			redmediumplasmashot.has_locales = false;
			redmediumplasmashot.projectile = "redmediumplasma";
			redmediumplasmashot.base_stats["projectiles"] = 1f;
			redmediumplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			redmediumplasmashot.show_in_meta_editor = false;
			redmediumplasmashot.show_in_knowledge_window = false;
			redmediumplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			EquipmentAsset Airredmediumplasmashot = AssetManager.items.clone("Airredmediumplasmashot", "$range");
			Airredmediumplasmashot.has_locales = false;
			Airredmediumplasmashot.projectile = "aerialredmediumplasma";
			Airredmediumplasmashot.base_stats["projectiles"] = 1f;
			Airredmediumplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			Airredmediumplasmashot.show_in_meta_editor = false;
			Airredmediumplasmashot.show_in_knowledge_window = false;
			Airredmediumplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			EquipmentAsset biggreenplasmashot = AssetManager.items.clone("biggreenplasmashot", "$range");
			biggreenplasmashot.has_locales = false;
			biggreenplasmashot.projectile = "biggreenplasma";
			biggreenplasmashot.base_stats["projectiles"] = 1f;
			biggreenplasmashot.path_slash_animation = "effects/slashes/slash_cannonball";
			biggreenplasmashot.show_in_meta_editor = false;
			biggreenplasmashot.show_in_knowledge_window = false;
			biggreenplasmashot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");

			ProjectileAsset cybermissileprojectile = new ProjectileAsset();
			cybermissileprojectile.id = "cybermissileprojectile";
			cybermissileprojectile.speed = 10f;
			cybermissileprojectile.look_at_target = true;
			cybermissileprojectile.texture = "cybermissileprojectile";
			cybermissileprojectile.texture_shadow = "shadows/projectiles/shadow_ball";
			cybermissileprojectile.terraform_option = "AntiAirbomb";
			cybermissileprojectile.terraform_range = 1;
			cybermissileprojectile.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			cybermissileprojectile.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			cybermissileprojectile.end_effect = "fx_firebomb_explosion";
			cybermissileprojectile.scale_start = 0.2f;
			cybermissileprojectile.scale_target = 0.2f;
			cybermissileprojectile.can_be_left_on_ground = true;
			cybermissileprojectile.can_be_blocked = true;
			AssetManager.projectiles.add(cybermissileprojectile);

			EquipmentAsset missilebarrage = AssetManager.items.clone("missilebarrage", "$range");
			missilebarrage.has_locales = false;
			missilebarrage.projectile = "cybermissileprojectile";
			missilebarrage.base_stats["projectiles"] = 1f;
			missilebarrage.path_slash_animation = "effects/slashes/slash_cannonball";
			missilebarrage.show_in_meta_editor = false;
			missilebarrage.show_in_knowledge_window = false;



			EffectAsset N2explosion = new EffectAsset();
			N2explosion.id = "N2explosion";
			N2explosion.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny";
			N2explosion.use_basic_prefab = true;
			N2explosion.sorting_layer_id = "EffectsTop";
			N2explosion.sprite_path = "effects/N2explosion";
			N2explosion.draw_light_area = true;
			AssetManager.effects_library.add(N2explosion);

			ProjectileAsset N2BOMB = new ProjectileAsset();
			N2BOMB.id = "N2BOMB";
			N2BOMB.speed = 20f;
			N2BOMB.texture = "N2";
			N2BOMB.look_at_target = true;
			N2BOMB.texture_shadow = "shadows/projectiles/shadow_ball";
			N2BOMB.terraform_option = "cannonball";
			N2BOMB.terraform_range = 10;
			N2BOMB.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			N2BOMB.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			N2BOMB.end_effect = "N2explosion";
			N2BOMB.scale_start = 0.2f;
			N2BOMB.scale_target = 0.2f;
			N2BOMB.can_be_left_on_ground = true;
			N2BOMB.can_be_blocked = true;
			AssetManager.projectiles.add(N2BOMB);

			EquipmentAsset N2Attack = AssetManager.items.clone("N2Attack", "$range");
			N2Attack.has_locales = false;
			N2Attack.projectile = "N2BOMB";
			N2Attack.base_stats["projectiles"] = 1f;
			N2Attack.path_slash_animation = "effects/slashes/slash_cannonball";
			N2Attack.show_in_meta_editor = false;
			N2Attack.show_in_knowledge_window = false;



			////////////////NUKE//////////////////

			ProjectileAsset NUKER = new ProjectileAsset();
            NUKER.id = "NUKER";
            NUKER.speed = ModernBoxPrefs.ScaleBalance(150f);
			NUKER.texture = "NUKER";
			NUKER.look_at_target = true;
			NUKER.texture_shadow = "shadows/projectiles/shadow_ball";
			NUKER.terraform_option = "atomic_bomb";
			NUKER.draw_light_area = true;
			NUKER.terraform_range = 20;
			NUKER.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			NUKER.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			NUKER.end_effect = "fx_explosion_nuke_atomic";
			NUKER.scale_start = 0.4f;
			NUKER.scale_target = 0.4f;
          NUKER.can_be_left_on_ground = false;
          NUKER.can_be_blocked = false;
		  NUKER.world_actions = (AttackAction)Delegate.Combine(NUKER.world_actions, new AttackAction(ActionLibrary.burnTile));
          AssetManager.projectiles.add(NUKER);











// BASE UNITS
//
//=============================================================================//


	var baseWarUnit = AssetManager.actor_library.clone("baseWarUnit","$basic_unit$");
	baseWarUnit.is_humanoid = false;
	baseWarUnit.civ = false;
	baseWarUnit.experience_given = 20;
	baseWarUnit.actor_size = ActorSize.S13_Human;
	baseWarUnit.visible_on_minimap = true;
	baseWarUnit.die_in_lava = true;
		baseWarUnit.can_have_subspecies = false;
        baseWarUnit.base_stats["mass_2"] = 600f;
        baseWarUnit.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		baseWarUnit.base_stats["lifespan"] = ModernBoxPrefs.ScaleBalance(15f);
        baseWarUnit.base_stats["scale"] = 0.3f;
        baseWarUnit.base_stats["size"] = 1f;
		baseWarUnit.base_stats["mass"] = 1000f;
        baseWarUnit.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		baseWarUnit.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(10f);
		baseWarUnit.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		baseWarUnit.base_stats["attack_speed"] = 1f;
		baseWarUnit.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		baseWarUnit.base_stats["knockback"] = 2f;
		baseWarUnit.base_stats["accuracy"] = 1f;
		baseWarUnit.base_stats["targets"] = 1f;
		baseWarUnit.base_stats["area_of_effect"] = 0.5f;
		baseWarUnit.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
		baseWarUnit.base_stats["critical_damage_multiplier"] = 2f;
		baseWarUnit.base_stats["multiplier_supply_timer"] = 1f;
        baseWarUnit.sound_hit = "event:/SFX/HIT/HitWood";
        baseWarUnit.base_throwing_range = 7f;
		baseWarUnit.affected_by_dust = false;
        baseWarUnit.inspect_children = false;
        baseWarUnit.default_attack = "base_attack";
        baseWarUnit.icon = "iconBoat";
        baseWarUnit.shadow_texture = "unitShadow_6";
        baseWarUnit.texture_asset = new ActorTextureSubAsset("actors/baseWarUnit/", false);
        baseWarUnit.special = true;
        baseWarUnit.has_advanced_textures = false;
        baseWarUnit.cost = new ConstructionCost(1, 0, 0, 1);
        baseWarUnit.animation_walk = ActorAnimationSequences.walk_0_3;
        baseWarUnit.animation_idle = ActorAnimationSequences.walk_0;
		baseWarUnit.animation_swim = ActorAnimationSequences.swim_0_3;
		baseWarUnit.name_template_sets = AssetLibrary<ActorAsset>.a<string>("assimilator_set");
		baseWarUnit.kingdom_id_civilization = string.Empty;
		baseWarUnit.build_order_template_id = string.Empty;
		baseWarUnit.disable_jump_animation = true;
		baseWarUnit.inspect_sex = false;
		baseWarUnit.inspect_show_species = false;
		baseWarUnit.inspect_generation = false;
		baseWarUnit.immune_to_injuries = true;
		baseWarUnit.show_on_meta_layer = false;
		baseWarUnit.show_in_knowledge_window = false;
		baseWarUnit.show_in_taxonomy_tooltip = false;
		baseWarUnit.needs_to_be_explored = false;
		baseWarUnit.need_colored_sprite = true;
        baseWarUnit.allowed_status_tiers = StatusTier.Basic;
		baseWarUnit.render_status_effects = false;
        baseWarUnit.inspect_avatar_scale = 3f;
		baseWarUnit.color_hex = "#000000";
			baseWarUnit.force_land_creature = true;
			baseWarUnit.inspect_home = true;
			baseWarUnit.can_edit_traits = true;
            baseWarUnit.disable_jump_animation = true;
			baseWarUnit.can_receive_traits = true;
			baseWarUnit.flying = false;
			//baseoffensiveunit.tech = "baseoffensiveunits";
			baseWarUnit.very_high_flyer = false;
			baseWarUnit.die_on_blocks = true;
			baseWarUnit.ignore_blocks = false;
            baseWarUnit.inspect_experience = true;
            baseWarUnit.inspect_kills = true;
            baseWarUnit.use_items = false;
			baseWarUnit.has_baby_form = false;
            baseWarUnit.take_items = false;
            baseWarUnit.name_locale = "baseWarUnit";
            baseWarUnit.job_citizen = Toolbox.a<string>("attacker");
		baseWarUnit.job_kingdom = Toolbox.a<string>("attacker");
		baseWarUnit.job_attacker = Toolbox.a<string>("attacker");
		   baseWarUnit.job = AssetLibrary<ActorAsset>.a<string>("decision");
           baseWarUnit.addDecision("check_swearing");
baseWarUnit.addDecision("warrior_try_join_army_group");
baseWarUnit.addDecision("city_walking_to_danger_zone");
baseWarUnit.addDecision("warrior_army_captain_idle_walking_city");
baseWarUnit.addDecision("warrior_army_captain_waiting");
baseWarUnit.addDecision("warrior_army_leader_move_random");
baseWarUnit.addDecision("warrior_army_leader_move_to_attack_target");
baseWarUnit.addDecision("warrior_army_follow_leader");
baseWarUnit.addDecision("warrior_random_move");
baseWarUnit.addDecision("check_warrior_transport");
baseWarUnit.addDecision("swim_to_island");
        baseWarUnit.collective_term = "group_gang";
        baseWarUnit.prevent_unconscious_rotation = true;
        baseWarUnit.use_phenotypes = false;
		baseWarUnit.unit_other = true;
		baseWarUnit.can_be_surprised = false;
        baseWarUnit.has_skin = false;
        baseWarUnit.disable_jump_animation = true;
		baseWarUnit.can_turn_into_mush = false;
		baseWarUnit.can_turn_into_tumor = false;
		baseWarUnit.can_turn_into_zombie = false;
		baseWarUnit.use_tool_items = false;
            baseWarUnit.kingdom_id_wild = "neutral_animals";
            baseWarUnit.can_flip = true;
            baseWarUnit.check_flip = (BaseSimObject _, WorldTile _) => true;
            //baseWarUnit.split_ai_update = false;
			baseWarUnit.allow_possession = true;
            baseWarUnit.can_talk_with = false;
			baseWarUnit.control_can_backstep = true;
			baseWarUnit.control_can_jump = true;
			baseWarUnit.control_can_kick = true;
			baseWarUnit.control_can_dash = true;
			baseWarUnit.control_can_talk = false;
			baseWarUnit.control_can_swear = true;
			baseWarUnit.control_can_steal = true;
			baseWarUnit.show_controllable_tip = true;
        baseWarUnit.update_z = true;
		baseWarUnit.can_be_killed_by_stuff = true;
		baseWarUnit.can_be_killed_by_life_eraser = true;
		baseWarUnit.can_attack_buildings = true;
		baseWarUnit.can_be_moved_by_powers = true;
		baseWarUnit.can_be_hurt_by_powers = true;
		baseWarUnit.effect_damage = true;
		baseWarUnit.immune_to_slowness = true;
		//baseWarUnit.can_flip = true;
		baseWarUnit.death_animation_angle = true;
		baseWarUnit.can_be_inspected = true;
		baseWarUnit.addTrait("Unitpotential");
		baseWarUnit.addTrait("immune");
		//baseWarUnit.addTrait("strong_minded");
		baseWarUnit.addTrait("light_lamp");
            AssetManager.actor_library.add(baseWarUnit);
			Localization.addLocalization(baseWarUnit.name_locale, baseWarUnit.name_locale);




/////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////MEDIEVAL////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////


	var humancavalry = AssetManager.actor_library.clone("humancavalry","baseWarUnit");
	humancavalry.die_in_lava = false;
        humancavalry.base_stats["mass_2"] = 600f;
        humancavalry.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        humancavalry.base_stats["scale"] = 0.1f;
        humancavalry.base_stats["size"] = 1f;
		humancavalry.base_stats["mass"] = 1000f;
        humancavalry.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		humancavalry.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		humancavalry.base_stats["armor"] = 5f;
		humancavalry.base_stats["attack_speed"] = 1f;
		humancavalry.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		humancavalry.base_stats["knockback"] = 0.01f;
		humancavalry.base_stats["accuracy"] = 0.8f;
		humancavalry.base_stats["targets"] = 2f;
		humancavalry.base_stats["area_of_effect"] = 0.5f;
		humancavalry.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        humancavalry.sound_hit = "event:/SFX/HIT/HitMetal";
        humancavalry.default_attack = "base_attack";
        humancavalry.icon = "iconBoat";
        humancavalry.shadow_texture = "unitShadow_6";
        humancavalry.texture_asset = new ActorTextureSubAsset("actors/humancavalry/", false);
        humancavalry.special = true;
        humancavalry.has_advanced_textures = false;
        humancavalry.animation_walk = ActorAnimationSequences.walk_0_3;
        humancavalry.animation_idle = ActorAnimationSequences.walk_0;
		humancavalry.animation_swim = ActorAnimationSequences.swim_0_3;
            humancavalry.name_locale = "Light Vehicle";
			humancavalry.addTrait("dodge");
			humancavalry.addTrait("dash");
            AssetManager.actor_library.add(humancavalry);
			Localization.addLocalization(humancavalry.name_locale, humancavalry.name_locale);




	var armoredwolf = AssetManager.actor_library.clone("armoredwolf","baseWarUnit");
	armoredwolf.die_in_lava = false;
        armoredwolf.base_stats["mass_2"] = 600f;
        armoredwolf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        armoredwolf.base_stats["scale"] = 0.1f;
        armoredwolf.base_stats["size"] = 1f;
		armoredwolf.base_stats["mass"] = 1000f;
        armoredwolf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		armoredwolf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		armoredwolf.base_stats["armor"] = 5f;
		armoredwolf.base_stats["attack_speed"] = 1f;
		armoredwolf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		armoredwolf.base_stats["knockback"] = 0.01f;
		armoredwolf.base_stats["accuracy"] = 0.8f;
		armoredwolf.base_stats["targets"] = 2f;
		armoredwolf.base_stats["area_of_effect"] = 0.5f;
		armoredwolf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        armoredwolf.sound_hit = "event:/SFX/HIT/HitMetal";
        armoredwolf.default_attack = "jaws";
        armoredwolf.icon = "iconBoat";
        armoredwolf.shadow_texture = "unitShadow_6";
        armoredwolf.texture_asset = new ActorTextureSubAsset("actors/armoredwolf/", false);
        armoredwolf.special = true;
        armoredwolf.has_advanced_textures = false;
        armoredwolf.animation_walk = ActorAnimationSequences.walk_0_2;
        armoredwolf.animation_idle = Vehicles.idle_0_2;
		armoredwolf.animation_swim = ActorAnimationSequences.swim_0_2;
            armoredwolf.name_locale = "Light Vehicle";
			armoredwolf.addTrait("dodge");
			armoredwolf.addTrait("dash");
			armoredwolf.addTrait("savage");
			armoredwolf.addTrait("flesh_eater");
            AssetManager.actor_library.add(armoredwolf);
			Localization.addLocalization(armoredwolf.name_locale, armoredwolf.name_locale);


	var ogreunit = AssetManager.actor_library.clone("ogreunit","baseWarUnit");
	ogreunit.die_in_lava = false;
        ogreunit.base_stats["mass_2"] = 600f;
        ogreunit.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        ogreunit.base_stats["scale"] = 0.3f;
        ogreunit.base_stats["size"] = 1f;
		ogreunit.base_stats["mass"] = 1000f;
        ogreunit.base_stats["health"] = ModernBoxPrefs.ScaleBalance(250f);
		ogreunit.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		ogreunit.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		ogreunit.base_stats["attack_speed"] = 1f;
		ogreunit.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		ogreunit.base_stats["knockback"] = 3f;
		ogreunit.base_stats["accuracy"] = 0.8f;
		ogreunit.base_stats["targets"] = 3f;
		ogreunit.base_stats["area_of_effect"] = 0.5f;
		ogreunit.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        ogreunit.sound_hit = "event:/SFX/HIT/HitMetal";
        ogreunit.default_attack = "base_attack";
        ogreunit.icon = "iconBoat";
        ogreunit.shadow_texture = "unitShadow_6";
        ogreunit.texture_asset = new ActorTextureSubAsset("actors/ogreunit/", false);
        ogreunit.special = true;
        ogreunit.has_advanced_textures = false;
        ogreunit.animation_walk = ActorAnimationSequences.walk_0_3;
        ogreunit.animation_idle = Vehicles.idle_0;
		ogreunit.animation_swim = ActorAnimationSequences.swim_0_3;
            ogreunit.name_locale = "Light Vehicle";
            ogreunit.addTrait("savage");
			ogreunit.addTrait("strong");
            AssetManager.actor_library.add(ogreunit);
			Localization.addLocalization(ogreunit.name_locale, ogreunit.name_locale);


	var golemgem = AssetManager.actor_library.clone("golemgem","baseWarUnit");
	golemgem.die_in_lava = false;
        golemgem.base_stats["mass_2"] = 600f;
        golemgem.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        golemgem.base_stats["scale"] = 0.1f;
        golemgem.base_stats["size"] = 1f;
		golemgem.base_stats["mass"] = 1000f;
        golemgem.base_stats["health"] = ModernBoxPrefs.ScaleBalance(160f);
		golemgem.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		golemgem.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		golemgem.base_stats["attack_speed"] = 1f;
		golemgem.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		golemgem.base_stats["knockback"] = 0.3f;
		golemgem.base_stats["accuracy"] = 0.8f;
		golemgem.base_stats["targets"] = 1f;
		golemgem.base_stats["area_of_effect"] = 0.5f;
		golemgem.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        golemgem.sound_hit = "event:/SFX/HIT/HitMetal";
        golemgem.default_attack = "base_attack";
        golemgem.icon = "iconBoat";
        golemgem.shadow_texture = "unitShadow_6";
        golemgem.texture_asset = new ActorTextureSubAsset("actors/golemgem/", false);
        golemgem.special = true;
        golemgem.has_advanced_textures = false;
        golemgem.animation_walk = ActorAnimationSequences.walk_0_3;
        golemgem.animation_idle = ActorAnimationSequences.walk_0;
		golemgem.animation_swim = ActorAnimationSequences.swim_0_3;
            golemgem.name_locale = "Light Vehicle";
			golemgem.addTrait("dodge");
			golemgem.addTrait("dash");
            AssetManager.actor_library.add(golemgem);
			Localization.addLocalization(golemgem.name_locale, golemgem.name_locale);


	var treant = AssetManager.actor_library.clone("treant","baseWarUnit");
	treant.die_in_lava = false;
        treant.base_stats["mass_2"] = 600f;
        treant.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        treant.base_stats["scale"] = 0.1f;
        treant.base_stats["size"] = 1f;
		treant.base_stats["mass"] = 1000f;
        treant.base_stats["health"] = ModernBoxPrefs.ScaleBalance(160f);
		treant.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		treant.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		treant.base_stats["attack_speed"] = 1f;
		treant.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		treant.base_stats["knockback"] = 0.3f;
		treant.base_stats["accuracy"] = 0.8f;
		treant.base_stats["targets"] = 1f;
		treant.base_stats["area_of_effect"] = 0.5f;
		treant.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        treant.sound_hit = "event:/SFX/HIT/HitMetal";
        treant.default_attack = "base_attack";
        treant.icon = "iconBoat";
        treant.shadow_texture = "unitShadow_6";
        treant.texture_asset = new ActorTextureSubAsset("actors/treant/", false);
        treant.special = true;
        treant.has_advanced_textures = false;
        treant.animation_walk = ActorAnimationSequences.walk_0_3;
        treant.animation_idle = ActorAnimationSequences.walk_0;
		treant.animation_swim = ActorAnimationSequences.swim_0_3;
            treant.name_locale = "Light Vehicle";
			treant.addTrait("dodge");
			treant.addTrait("dash");
            AssetManager.actor_library.add(treant);
			Localization.addLocalization(treant.name_locale, treant.name_locale);

var catapulta = AssetManager.actor_library.clone("catapulta","baseWarUnit");
	catapulta.die_in_lava = false;
        catapulta.base_stats["mass_2"] = 600f;
        catapulta.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        catapulta.base_stats["scale"] = 0.3f;
        catapulta.base_stats["size"] = 1f;
		catapulta.base_stats["mass"] = 1000f;
        catapulta.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		catapulta.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		catapulta.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		catapulta.base_stats["attack_speed"] = -20f;
		catapulta.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		catapulta.base_stats["knockback"] = 3f;
		catapulta.base_stats["accuracy"] = 0.15f;
		catapulta.base_stats["targets"] = 4f;
		catapulta.base_stats["area_of_effect"] = 3f;
		catapulta.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        catapulta.sound_hit = "event:/SFX/HIT/HitMetal";
        catapulta.default_attack = "StoneThrow";
        catapulta.icon = "iconBoat";
		catapulta.inspect_avatar_scale = 2f;
        catapulta.shadow_texture = "unitShadow_6";
        catapulta.texture_asset = new ActorTextureSubAsset("actors/catapulta/", false);
        catapulta.special = true;
        catapulta.has_advanced_textures = false;
        catapulta.animation_walk = ActorAnimationSequences.walk_0_2;
        catapulta.animation_idle = Vehicles.idle_0_2;
		catapulta.animation_swim = ActorAnimationSequences.swim_0_2;
            catapulta.name_locale = "Artillery";
            AssetManager.actor_library.add(catapulta);
			Localization.addLocalization(catapulta.name_locale, catapulta.name_locale);

var orcatapulta = AssetManager.actor_library.clone("orcatapulta","baseWarUnit");
	orcatapulta.die_in_lava = false;
        orcatapulta.base_stats["mass_2"] = 600f;
        orcatapulta.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        orcatapulta.base_stats["scale"] = 0.3f;
        orcatapulta.base_stats["size"] = 1f;
		orcatapulta.base_stats["mass"] = 1000f;
        orcatapulta.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		orcatapulta.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		orcatapulta.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		orcatapulta.base_stats["attack_speed"] = -20f;
		orcatapulta.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		orcatapulta.base_stats["knockback"] = 3f;
		orcatapulta.base_stats["accuracy"] = 0.15f;
		orcatapulta.base_stats["targets"] = 4f;
		orcatapulta.base_stats["area_of_effect"] = 3f;
		orcatapulta.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        orcatapulta.sound_hit = "event:/SFX/HIT/HitMetal";
        orcatapulta.default_attack = "StoneThrow";
        orcatapulta.icon = "iconBoat";
		orcatapulta.inspect_avatar_scale = 2f;
        orcatapulta.shadow_texture = "unitShadow_6";
        orcatapulta.texture_asset = new ActorTextureSubAsset("actors/orcatapulta/", false);
        orcatapulta.special = true;
        orcatapulta.has_advanced_textures = false;
        orcatapulta.animation_walk = ActorAnimationSequences.walk_0_3;
        orcatapulta.animation_idle = ActorAnimationSequences.idle_0_3;
		orcatapulta.animation_swim = ActorAnimationSequences.swim_0_3;
            orcatapulta.name_locale = "Artillery";
            AssetManager.actor_library.add(orcatapulta);
			Localization.addLocalization(orcatapulta.name_locale, orcatapulta.name_locale);



var santaguin = AssetManager.actor_library.clone("santaguin","baseWarUnit");
	santaguin.die_in_lava = false;
        santaguin.base_stats["mass_2"] = 600f;
        santaguin.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        santaguin.base_stats["scale"] = 0.3f;
        santaguin.base_stats["size"] = 1f;
		santaguin.base_stats["mass"] = 1000f;
        santaguin.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		santaguin.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		santaguin.base_stats["armor"] = 5f;
		santaguin.base_stats["attack_speed"] = -20f;
		santaguin.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		santaguin.base_stats["knockback"] = 3f;
		santaguin.base_stats["accuracy"] = 0.15f;
		santaguin.base_stats["targets"] = 4f;
		santaguin.base_stats["area_of_effect"] = 3f;
		santaguin.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        santaguin.sound_hit = "event:/SFX/HIT/HitMetal";
        santaguin.default_attack = "SnowThrow";
        santaguin.icon = "iconBoat";
		santaguin.inspect_avatar_scale = 2f;
        santaguin.shadow_texture = "unitShadow_6";
        santaguin.texture_asset = new ActorTextureSubAsset("actors/santaguin/", false);
        santaguin.special = true;
        santaguin.has_advanced_textures = false;
        santaguin.animation_walk = ActorAnimationSequences.walk_0_2;
        santaguin.animation_idle = ActorAnimationSequences.walk_0;
		santaguin.animation_swim = ActorAnimationSequences.swim_0_2;
            santaguin.name_locale = "Artillery";
            AssetManager.actor_library.add(santaguin);
			Localization.addLocalization(santaguin.name_locale, santaguin.name_locale);


var batteringram = AssetManager.actor_library.clone("batteringram","baseWarUnit");
	batteringram.die_in_lava = false;
        batteringram.base_stats["mass_2"] = 600f;
        batteringram.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        batteringram.base_stats["scale"] = 0.3f;
        batteringram.base_stats["size"] = 1f;
		batteringram.base_stats["mass"] = 1000f;
        batteringram.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		batteringram.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(15f);
		batteringram.base_stats["armor"] = 25f;
		batteringram.base_stats["attack_speed"] = -20f;
		batteringram.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(15f);
		batteringram.base_stats["knockback"] = 6f;
		batteringram.base_stats["accuracy"] = 0.15f;
		batteringram.base_stats["targets"] = ModernBoxPrefs.ScaleBalance(10f);
		batteringram.base_stats["area_of_effect"] = 3f;
		batteringram.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        batteringram.sound_hit = "event:/SFX/HIT/HitMetal";
        batteringram.default_attack = "base_attack";
        batteringram.icon = "iconBoat";
		batteringram.inspect_avatar_scale = 2f;
        batteringram.shadow_texture = "unitShadow_6";
        batteringram.texture_asset = new ActorTextureSubAsset("actors/batteringram/", false);
        batteringram.special = true;
        batteringram.has_advanced_textures = false;
        batteringram.animation_walk = ActorAnimationSequences.walk_0_2;
        batteringram.animation_idle = Vehicles.idle_0_2;
		batteringram.animation_swim = ActorAnimationSequences.swim_0_2;
            batteringram.name_locale = "Artillery";
            AssetManager.actor_library.add(batteringram);
			Localization.addLocalization(batteringram.name_locale, batteringram.name_locale);

var woolyrhino = AssetManager.actor_library.clone("woolyrhino","baseWarUnit");
	woolyrhino.die_in_lava = false;
        woolyrhino.base_stats["mass_2"] = 600f;
        woolyrhino.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        woolyrhino.base_stats["scale"] = 0.3f;
        woolyrhino.base_stats["size"] = 1f;
		woolyrhino.base_stats["mass"] = 1000f;
        woolyrhino.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		woolyrhino.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(15f);
		woolyrhino.base_stats["armor"] = 25f;
		woolyrhino.base_stats["attack_speed"] = -20f;
		woolyrhino.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(15f);
		woolyrhino.base_stats["knockback"] = 6f;
		woolyrhino.base_stats["accuracy"] = 0.15f;
		woolyrhino.base_stats["targets"] = ModernBoxPrefs.ScaleBalance(10f);
		woolyrhino.base_stats["area_of_effect"] = 3f;
		woolyrhino.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        woolyrhino.sound_hit = "event:/SFX/HIT/HitMetal";
        woolyrhino.default_attack = "base_attack";
        woolyrhino.icon = "iconBoat";
		woolyrhino.inspect_avatar_scale = 2f;
        woolyrhino.shadow_texture = "unitShadow_6";
        woolyrhino.texture_asset = new ActorTextureSubAsset("actors/woolyrhino/", false);
        woolyrhino.special = true;
        woolyrhino.has_advanced_textures = false;
        woolyrhino.animation_walk = ActorAnimationSequences.walk_0_2;
        woolyrhino.animation_idle = ActorAnimationSequences.walk_0;
		woolyrhino.animation_swim = ActorAnimationSequences.swim_0_2;
            woolyrhino.name_locale = "Artillery";
            AssetManager.actor_library.add(woolyrhino);
			Localization.addLocalization(woolyrhino.name_locale, woolyrhino.name_locale);
	var humanpaladin = AssetManager.actor_library.clone("humanpaladin","baseWarUnit");
	humanpaladin.die_in_lava = false;
        humanpaladin.base_stats["mass_2"] = 600f;
        humanpaladin.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        humanpaladin.base_stats["scale"] = 0.1f;
        humanpaladin.base_stats["size"] = 1f;
		humanpaladin.base_stats["mass"] = 1000f;
        humanpaladin.base_stats["health"] = ModernBoxPrefs.ScaleBalance(15f);
		humanpaladin.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		humanpaladin.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		humanpaladin.base_stats["attack_speed"] = 0.1f;
		humanpaladin.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		humanpaladin.base_stats["knockback"] = 4f;
		humanpaladin.base_stats["accuracy"] = 0.1f;
		humanpaladin.base_stats["targets"] = 2f;
		humanpaladin.base_stats["area_of_effect"] = 4f;
		humanpaladin.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        humanpaladin.sound_hit = "event:/SFX/HIT/HitMetal";
        humanpaladin.default_attack = "base_attack";
        humanpaladin.icon = "iconBoat";
        humanpaladin.shadow_texture = "unitShadow_6";
		humanpaladin.inspect_avatar_scale = 1f;
        humanpaladin.texture_asset = new ActorTextureSubAsset("actors/humanpaladin/", false);
        humanpaladin.special = true;
        humanpaladin.has_advanced_textures = false;
        humanpaladin.animation_walk = ActorAnimationSequences.walk_0_3;
        humanpaladin.animation_idle = ActorAnimationSequences.walk_0;
		humanpaladin.animation_swim = ActorAnimationSequences.swim_0_3;
            humanpaladin.name_locale = "Support Unit";
            humanpaladin.skip_fight_logic = true;
			humanpaladin.addTrait("fire_proof");
			humanpaladin.addTrait("heart_of_wizard");
		humanpaladin.addTrait("healing_aura");
			   humanpaladin.job = AssetLibrary<ActorAsset>.a<string>("decision");
           humanpaladin.addDecision("check_swearing");
humanpaladin.addDecision("warrior_try_join_army_group");
humanpaladin.addDecision("city_walking_to_danger_zone");
humanpaladin.addDecision("check_cure");
humanpaladin.addDecision("warrior_army_leader_move_random");
humanpaladin.addDecision("check_heal");
humanpaladin.addDecision("warrior_army_follow_leader");
humanpaladin.addDecision("warrior_random_move");
humanpaladin.addDecision("check_warrior_transport");
humanpaladin.addDecision("swim_to_island");
            AssetManager.actor_library.add(humanpaladin);
			Localization.addLocalization(humanpaladin.name_locale, humanpaladin.name_locale);





	var dwarfdoctor = AssetManager.actor_library.clone("dwarfdoctor","baseWarUnit");
	dwarfdoctor.die_in_lava = false;
        dwarfdoctor.base_stats["mass_2"] = 600f;
        dwarfdoctor.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        dwarfdoctor.base_stats["scale"] = 0.1f;
        dwarfdoctor.base_stats["size"] = 1f;
		dwarfdoctor.base_stats["mass"] = 1000f;
        dwarfdoctor.base_stats["health"] = ModernBoxPrefs.ScaleBalance(15f);
		dwarfdoctor.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		dwarfdoctor.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		dwarfdoctor.base_stats["attack_speed"] = 0.1f;
		dwarfdoctor.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		dwarfdoctor.base_stats["knockback"] = 4f;
		dwarfdoctor.base_stats["accuracy"] = 0.1f;
		dwarfdoctor.base_stats["targets"] = 2f;
		dwarfdoctor.base_stats["area_of_effect"] = 4f;
		dwarfdoctor.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        dwarfdoctor.sound_hit = "event:/SFX/HIT/HitMetal";
        dwarfdoctor.default_attack = "base_attack";
        dwarfdoctor.icon = "iconBoat";
        dwarfdoctor.shadow_texture = "unitShadow_6";
		dwarfdoctor.inspect_avatar_scale = 1f;
        dwarfdoctor.texture_asset = new ActorTextureSubAsset("actors/dwarfdoctor/", false);
        dwarfdoctor.special = true;
        dwarfdoctor.has_advanced_textures = false;
        dwarfdoctor.animation_walk = ActorAnimationSequences.walk_0_3;
        dwarfdoctor.animation_idle = ActorAnimationSequences.walk_0;
		dwarfdoctor.animation_swim = ActorAnimationSequences.swim_0_3;
            dwarfdoctor.name_locale = "Support Unit";
            dwarfdoctor.skip_fight_logic = true;
			dwarfdoctor.addTrait("fire_proof");
			dwarfdoctor.addTrait("heart_of_wizard");
		dwarfdoctor.addTrait("healing_aura");
			   dwarfdoctor.job = AssetLibrary<ActorAsset>.a<string>("decision");
           dwarfdoctor.addDecision("check_swearing");
dwarfdoctor.addDecision("warrior_try_join_army_group");
dwarfdoctor.addDecision("city_walking_to_danger_zone");
dwarfdoctor.addDecision("check_cure");
dwarfdoctor.addDecision("warrior_army_leader_move_random");
dwarfdoctor.addDecision("check_heal");
dwarfdoctor.addDecision("warrior_army_follow_leader");
dwarfdoctor.addDecision("warrior_random_move");
dwarfdoctor.addDecision("check_warrior_transport");
dwarfdoctor.addDecision("swim_to_island");
            AssetManager.actor_library.add(dwarfdoctor);
			Localization.addLocalization(dwarfdoctor.name_locale, dwarfdoctor.name_locale);


				var orcwarlock = AssetManager.actor_library.clone("orcwarlock","baseWarUnit");
	orcwarlock.die_in_lava = false;
        orcwarlock.base_stats["mass_2"] = 600f;
        orcwarlock.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        orcwarlock.base_stats["scale"] = 0.1f;
        orcwarlock.base_stats["size"] = 1f;
		orcwarlock.base_stats["mass"] = 1000f;
        orcwarlock.base_stats["health"] = ModernBoxPrefs.ScaleBalance(15f);
		orcwarlock.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		orcwarlock.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		orcwarlock.base_stats["attack_speed"] = 0.1f;
		orcwarlock.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		orcwarlock.base_stats["knockback"] = 4f;
		orcwarlock.base_stats["accuracy"] = 0.1f;
		orcwarlock.base_stats["targets"] = 2f;
		orcwarlock.base_stats["area_of_effect"] = 4f;
		orcwarlock.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        orcwarlock.sound_hit = "event:/SFX/HIT/HitMetal";
        orcwarlock.default_attack = "base_attack";
        orcwarlock.icon = "iconBoat";
        orcwarlock.shadow_texture = "unitShadow_6";
		orcwarlock.inspect_avatar_scale = 1f;
        orcwarlock.texture_asset = new ActorTextureSubAsset("actors/orcwarlock/", false);
        orcwarlock.special = true;
        orcwarlock.has_advanced_textures = false;
        orcwarlock.animation_walk = ActorAnimationSequences.walk_0_3;
        orcwarlock.animation_idle = ActorAnimationSequences.walk_0;
		orcwarlock.animation_swim = ActorAnimationSequences.swim_0_3;
            orcwarlock.name_locale = "Support Unit";
            orcwarlock.skip_fight_logic = true;
			orcwarlock.addTrait("fire_proof");
			orcwarlock.addTrait("heart_of_wizard");
		orcwarlock.addTrait("healing_aura");
			   orcwarlock.job = AssetLibrary<ActorAsset>.a<string>("decision");
           orcwarlock.addDecision("check_swearing");
orcwarlock.addDecision("warrior_try_join_army_group");
orcwarlock.addDecision("city_walking_to_danger_zone");
orcwarlock.addDecision("check_cure");
orcwarlock.addDecision("warrior_army_leader_move_random");
orcwarlock.addDecision("check_heal");
orcwarlock.addDecision("warrior_army_follow_leader");
orcwarlock.addDecision("warrior_random_move");
orcwarlock.addDecision("check_warrior_transport");
orcwarlock.addDecision("swim_to_island");
            AssetManager.actor_library.add(orcwarlock);
			Localization.addLocalization(orcwarlock.name_locale, orcwarlock.name_locale);

	var fairydragon = AssetManager.actor_library.clone("fairydragon","baseWarUnit");
	fairydragon.die_in_lava = false;
        fairydragon.base_stats["mass_2"] = 600f;
        fairydragon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        fairydragon.base_stats["scale"] = 0.1f;
        fairydragon.base_stats["size"] = 1f;
		fairydragon.base_stats["mass"] = 1000f;
        fairydragon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(15f);
		fairydragon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		fairydragon.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		fairydragon.base_stats["attack_speed"] = 0.1f;
		fairydragon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		fairydragon.base_stats["knockback"] = 4f;
		fairydragon.base_stats["accuracy"] = 0.1f;
		fairydragon.base_stats["targets"] = 2f;
		fairydragon.base_stats["area_of_effect"] = 4f;
		fairydragon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        fairydragon.sound_hit = "event:/SFX/HIT/HitMetal";
        fairydragon.default_attack = "base_attack";
        fairydragon.icon = "iconBoat";
        fairydragon.shadow_texture = "unitShadow_6";
		fairydragon.inspect_avatar_scale = 1f;
        fairydragon.texture_asset = new ActorTextureSubAsset("actors/fairydragon/", false);
        fairydragon.special = true;
        fairydragon.has_advanced_textures = false;
        fairydragon.animation_walk = ActorAnimationSequences.walk_0_3;
        fairydragon.animation_idle = ActorAnimationSequences.walk_0_3;
		fairydragon.animation_swim = ActorAnimationSequences.walk_0_3;
            fairydragon.name_locale = "Support Unit";
            fairydragon.skip_fight_logic = true;
			fairydragon.addTrait("fire_proof");
			fairydragon.addTrait("heart_of_wizard");
		fairydragon.addTrait("healing_aura");
			   fairydragon.job = AssetLibrary<ActorAsset>.a<string>("decision");
           fairydragon.addDecision("check_swearing");
fairydragon.addDecision("warrior_try_join_army_group");
fairydragon.addDecision("city_walking_to_danger_zone");
fairydragon.addDecision("check_cure");
fairydragon.addDecision("warrior_army_leader_move_random");
fairydragon.addDecision("check_heal");
fairydragon.addDecision("warrior_army_follow_leader");
fairydragon.addDecision("warrior_random_move");
fairydragon.addDecision("check_warrior_transport");
fairydragon.addDecision("swim_to_island");
            AssetManager.actor_library.add(fairydragon);
			Localization.addLocalization(fairydragon.name_locale, fairydragon.name_locale);






/////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////RENAISSANCE////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////

	var humancannon = AssetManager.actor_library.clone("humancannon","baseWarUnit");
	humancannon.die_in_lava = false;
        humancannon.base_stats["mass_2"] = 600f;
        humancannon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        humancannon.base_stats["scale"] = 0.1f;
        humancannon.base_stats["size"] = 1f;
		humancannon.base_stats["mass"] = 1000f;
        humancannon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		humancannon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(15f);
		humancannon.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		humancannon.base_stats["attack_speed"] = -10f;
		humancannon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		humancannon.base_stats["knockback"] = 2f;
		humancannon.base_stats["accuracy"] = 0.3f;
		humancannon.base_stats["targets"] = 2f;
		humancannon.base_stats["area_of_effect"] = 4f;
		humancannon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        humancannon.sound_hit = "event:/SFX/HIT/HitMetal";
        humancannon.default_attack = "boat_cannonball";
        humancannon.icon = "iconBoat";
		humancannon.inspect_avatar_scale = 2f;
        humancannon.shadow_texture = "unitShadow_6";
        humancannon.texture_asset = new ActorTextureSubAsset("actors/humancannon/", false);
        humancannon.special = true;
        humancannon.has_advanced_textures = false;
        humancannon.animation_walk = ActorAnimationSequences.walk_0_3;
        humancannon.animation_idle = ActorAnimationSequences.walk_0;
		humancannon.animation_swim = ActorAnimationSequences.swim_0_3;
            humancannon.name_locale = "Artillery";
            AssetManager.actor_library.add(humancannon);
			Localization.addLocalization(humancannon.name_locale, humancannon.name_locale);

var dwarfcannon = AssetManager.actor_library.clone("dwarfcannon","baseWarUnit");
	dwarfcannon.die_in_lava = false;
        dwarfcannon.base_stats["mass_2"] = 600f;
        dwarfcannon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        dwarfcannon.base_stats["scale"] = 0.1f;
        dwarfcannon.base_stats["size"] = 1f;
		dwarfcannon.base_stats["mass"] = 1000f;
        dwarfcannon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		dwarfcannon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(15f);
		dwarfcannon.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		dwarfcannon.base_stats["attack_speed"] = -10f;
		dwarfcannon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		dwarfcannon.base_stats["knockback"] = 2f;
		dwarfcannon.base_stats["accuracy"] = 0.3f;
		dwarfcannon.base_stats["targets"] = 2f;
		dwarfcannon.base_stats["area_of_effect"] = 4f;
		dwarfcannon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        dwarfcannon.sound_hit = "event:/SFX/HIT/HitMetal";
        dwarfcannon.default_attack = "boat_freeze_ball";
        dwarfcannon.icon = "iconBoat";
		dwarfcannon.inspect_avatar_scale = 2f;
        dwarfcannon.shadow_texture = "unitShadow_6";
        dwarfcannon.texture_asset = new ActorTextureSubAsset("actors/dwarfcannon/", false);
        dwarfcannon.special = true;
        dwarfcannon.has_advanced_textures = false;
        dwarfcannon.animation_walk = ActorAnimationSequences.walk_0_3;
        dwarfcannon.animation_idle = ActorAnimationSequences.walk_0;
		dwarfcannon.animation_swim = ActorAnimationSequences.swim_0_3;
            dwarfcannon.name_locale = "Artillery";
            AssetManager.actor_library.add(dwarfcannon);
			Localization.addLocalization(dwarfcannon.name_locale, dwarfcannon.name_locale);


	var elfcannon = AssetManager.actor_library.clone("elfcannon","baseWarUnit");
	elfcannon.die_in_lava = false;
        elfcannon.base_stats["mass_2"] = 600f;
        elfcannon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        elfcannon.base_stats["scale"] = 0.1f;
        elfcannon.base_stats["size"] = 1f;
		elfcannon.base_stats["mass"] = 1000f;
        elfcannon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		elfcannon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(15f);
		elfcannon.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		elfcannon.base_stats["attack_speed"] = -10f;
		elfcannon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		elfcannon.base_stats["knockback"] = 2f;
		elfcannon.base_stats["accuracy"] = 0.3f;
		elfcannon.base_stats["targets"] = 2f;
		elfcannon.base_stats["area_of_effect"] = 4f;
		elfcannon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        elfcannon.sound_hit = "event:/SFX/HIT/HitMetal";
        elfcannon.default_attack = "gaiatankpew";
        elfcannon.icon = "iconBoat";
		elfcannon.inspect_avatar_scale = 2f;
        elfcannon.shadow_texture = "unitShadow_6";
        elfcannon.texture_asset = new ActorTextureSubAsset("actors/elfcannon/", false);
        elfcannon.special = true;
        elfcannon.has_advanced_textures = false;
        elfcannon.animation_walk = ActorAnimationSequences.walk_0_3;
        elfcannon.animation_idle = ActorAnimationSequences.walk_0;
		elfcannon.animation_swim = ActorAnimationSequences.swim_0_3;
            elfcannon.name_locale = "Artillery";
            AssetManager.actor_library.add(elfcannon);
			Localization.addLocalization(elfcannon.name_locale, elfcannon.name_locale);


	var orccannon = AssetManager.actor_library.clone("orccannon","baseWarUnit");
	orccannon.die_in_lava = false;
        orccannon.base_stats["mass_2"] = 600f;
        orccannon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        orccannon.base_stats["scale"] = 0.1f;
        orccannon.base_stats["size"] = 1f;
		orccannon.base_stats["mass"] = 1000f;
        orccannon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		orccannon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(15f);
		orccannon.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		orccannon.base_stats["attack_speed"] = -10f;
		orccannon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		orccannon.base_stats["knockback"] = 2f;
		orccannon.base_stats["accuracy"] = 0.3f;
		orccannon.base_stats["targets"] = 2f;
		orccannon.base_stats["area_of_effect"] = 4f;
		orccannon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        orccannon.sound_hit = "event:/SFX/HIT/HitMetal";
        orccannon.default_attack = "boat_fireball";
        orccannon.icon = "iconBoat";
		orccannon.inspect_avatar_scale = 2f;
        orccannon.shadow_texture = "unitShadow_6";
        orccannon.texture_asset = new ActorTextureSubAsset("actors/orccannon/", false);
        orccannon.special = true;
        orccannon.has_advanced_textures = false;
        orccannon.animation_walk = ActorAnimationSequences.walk_0_3;
        orccannon.animation_idle = ActorAnimationSequences.walk_0;
		orccannon.animation_swim = ActorAnimationSequences.swim_0_3;
            orccannon.name_locale = "Artillery";
            AssetManager.actor_library.add(orccannon);
			Localization.addLocalization(orccannon.name_locale, orccannon.name_locale);


var davincitank = AssetManager.actor_library.clone("davincitank","baseWarUnit");
	davincitank.die_in_lava = false;
        davincitank.base_stats["mass_2"] = 600f;
        davincitank.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        davincitank.base_stats["scale"] = 0.3f;
        davincitank.base_stats["size"] = 1f;
		davincitank.base_stats["mass"] = 1000f;
        davincitank.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		davincitank.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		davincitank.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		davincitank.base_stats["attack_speed"] = 1f;
		davincitank.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		davincitank.base_stats["knockback"] = 2f;
		davincitank.base_stats["accuracy"] = 0.1f;
		davincitank.base_stats["targets"] = 1f;
		davincitank.base_stats["area_of_effect"] = 2f;
		davincitank.base_stats["range"] = ModernBoxPrefs.ScaleBalance(12f);
        davincitank.sound_hit = "event:/SFX/HIT/HitMetal";
        davincitank.default_attack = "DavinciBarrage";
        davincitank.icon = "iconBoat";
        davincitank.shadow_texture = "unitShadow_6";
        davincitank.texture_asset = new ActorTextureSubAsset("actors/davincitank/", false);
        davincitank.special = true;
		davincitank.inspect_avatar_scale = 2f;
        davincitank.has_advanced_textures = false;
        davincitank.animation_walk = ActorAnimationSequences.walk_0_3;
        davincitank.animation_idle = Vehicles.idle_0;
		davincitank.animation_swim = ActorAnimationSequences.swim_0_3;
            davincitank.name_locale = "Tank";
			davincitank.addTrait("block");
			davincitank.addTrait("deflect_projectile");
            AssetManager.actor_library.add(davincitank);
			Localization.addLocalization(davincitank.name_locale, davincitank.name_locale);

	var balloonunit = AssetManager.actor_library.clone("balloonunit","baseWarUnit");
	balloonunit.die_in_lava = false;
	balloonunit.animation_speed_based_on_walk_speed = false;
        balloonunit.base_stats["mass_2"] = 600f;
        balloonunit.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        balloonunit.base_stats["scale"] = 0.3f;
        balloonunit.base_stats["size"] = 1f;
		balloonunit.base_stats["mass"] = 1000f;
        balloonunit.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		balloonunit.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		balloonunit.base_stats["armor"] = 0f;
		balloonunit.base_stats["attack_speed"] = 1f;
		balloonunit.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		balloonunit.base_stats["knockback"] = 0f;
		balloonunit.base_stats["accuracy"] = 0.1f;
		balloonunit.base_stats["targets"] = 4f;
		balloonunit.base_stats["area_of_effect"] = 2f;
		balloonunit.base_stats["range"] = ModernBoxPrefs.ScaleBalance(3f);
        balloonunit.sound_hit = "event:/SFX/HIT/HitMetal";
        balloonunit.default_attack = "FireBomb";
        balloonunit.addDecision("burn_tumors");
        balloonunit.icon = "iconBoat";
        balloonunit.shadow_texture = "unitShadow_6";
        balloonunit.texture_asset = new ActorTextureSubAsset("actors/balloonunit/", false);
        balloonunit.special = true;
        balloonunit.has_advanced_textures = false;
        balloonunit.animation_walk = ActorAnimationSequences.walk_0_3;
        balloonunit.animation_idle = ActorAnimationSequences.walk_0_3;
		balloonunit.animation_swim = ActorAnimationSequences.walk_0_3;
            balloonunit.name_locale = "Helicopter";
			balloonunit.addTrait("fire_proof");
            balloonunit.addTrait("freeze_proof");
			balloonunit.flying = true;
			balloonunit.very_high_flyer = true;
			balloonunit.die_on_blocks = false;
			balloonunit.inspect_avatar_scale = 0.5f;
			balloonunit.ignore_blocks = true;
            AssetManager.actor_library.add(balloonunit);
			Localization.addLocalization(balloonunit.name_locale, balloonunit.name_locale);


	var bigfaerydragon = AssetManager.actor_library.clone("bigfaerydragon","baseWarUnit");
	bigfaerydragon.die_in_lava = false;
	bigfaerydragon.animation_speed_based_on_walk_speed = false;
        bigfaerydragon.base_stats["mass_2"] = 600f;
        bigfaerydragon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        bigfaerydragon.base_stats["scale"] = 0.3f;
        bigfaerydragon.base_stats["size"] = 1f;
		bigfaerydragon.base_stats["mass"] = 1000f;
        bigfaerydragon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		bigfaerydragon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		bigfaerydragon.base_stats["armor"] = 0f;
		bigfaerydragon.base_stats["attack_speed"] = 1f;
		bigfaerydragon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(2f);
		bigfaerydragon.base_stats["knockback"] = 0f;
		bigfaerydragon.base_stats["accuracy"] = 0.1f;
		bigfaerydragon.base_stats["targets"] = 1f;
		bigfaerydragon.base_stats["area_of_effect"] = 2f;
		bigfaerydragon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(3f);
        bigfaerydragon.sound_hit = "event:/SFX/HIT/HitMetal";
        bigfaerydragon.default_attack = "GreenSpray";
        bigfaerydragon.addDecision("burn_tumors");
        bigfaerydragon.icon = "iconBoat";
        bigfaerydragon.shadow_texture = "unitShadow_6";
        bigfaerydragon.texture_asset = new ActorTextureSubAsset("actors/bigfaerydragon/", false);
        bigfaerydragon.special = true;
        bigfaerydragon.has_advanced_textures = false;
        bigfaerydragon.animation_walk = Vehicles.walk_0_5;
        bigfaerydragon.animation_idle = Vehicles.walk_0_5;
		bigfaerydragon.animation_swim = Vehicles.walk_0_5;
            bigfaerydragon.name_locale = "Helicopter";
			bigfaerydragon.addTrait("fire_proof");
            bigfaerydragon.addTrait("freeze_proof");
			bigfaerydragon.flying = true;
			bigfaerydragon.very_high_flyer = true;
			bigfaerydragon.die_on_blocks = false;
			bigfaerydragon.inspect_avatar_scale = 0.5f;
			bigfaerydragon.ignore_blocks = true;
            AssetManager.actor_library.add(bigfaerydragon);
			Localization.addLocalization(bigfaerydragon.name_locale, bigfaerydragon.name_locale);


	var Gunship = AssetManager.actor_library.clone("Gunship","baseWarUnit");
	Gunship.die_in_lava = false;
	Gunship.animation_speed_based_on_walk_speed = false;
        Gunship.base_stats["mass_2"] = 600f;
        Gunship.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Gunship.base_stats["scale"] = 0.3f;
        Gunship.base_stats["size"] = 1f;
		Gunship.base_stats["mass"] = 1000f;
        Gunship.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		Gunship.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Gunship.base_stats["armor"] = 0f;
		Gunship.base_stats["attack_speed"] = 1f;
		Gunship.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		Gunship.base_stats["knockback"] = 0f;
		Gunship.base_stats["accuracy"] = 0.1f;
		Gunship.base_stats["targets"] = 4f;
		Gunship.base_stats["area_of_effect"] = 2f;
		Gunship.base_stats["range"] = ModernBoxPrefs.ScaleBalance(10f);
        Gunship.sound_hit = "event:/SFX/HIT/HitMetal";
        Gunship.default_attack = "IceSnipe";
        Gunship.addDecision("burn_tumors");
        Gunship.icon = "iconBoat";
        Gunship.shadow_texture = "unitShadow_6";
        Gunship.texture_asset = new ActorTextureSubAsset("actors/Gunship/", false);
        Gunship.special = true;
        Gunship.has_advanced_textures = false;
        Gunship.animation_walk = ActorAnimationSequences.walk_0_3;
        Gunship.animation_idle = ActorAnimationSequences.walk_0_3;
		Gunship.animation_swim = ActorAnimationSequences.walk_0_3;
            Gunship.name_locale = "Helicopter";
			Gunship.addTrait("fire_proof");
            Gunship.addTrait("freeze_proof");
			Gunship.flying = true;
			Gunship.very_high_flyer = true;
			Gunship.die_on_blocks = false;
			Gunship.inspect_avatar_scale = 0.5f;
			Gunship.ignore_blocks = true;
            AssetManager.actor_library.add(Gunship);
			Localization.addLocalization(Gunship.name_locale, Gunship.name_locale);





/////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////MODERN/////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////


	var modernhumvee_Human = AssetManager.actor_library.clone("modernhumvee_Human","baseWarUnit");
	modernhumvee_Human.die_in_lava = false;
        modernhumvee_Human.base_stats["mass_2"] = 600f;
        modernhumvee_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        modernhumvee_Human.base_stats["scale"] = 0.3f;
        modernhumvee_Human.base_stats["size"] = 1f;
		modernhumvee_Human.base_stats["mass"] = 1000f;
        modernhumvee_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		modernhumvee_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		modernhumvee_Human.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		modernhumvee_Human.base_stats["attack_speed"] = 10000f;
		modernhumvee_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(10f);
		modernhumvee_Human.base_stats["knockback"] = 0.01f;
		modernhumvee_Human.base_stats["accuracy"] = 0.5f;
		modernhumvee_Human.base_stats["targets"] = 1f;
		modernhumvee_Human.base_stats["area_of_effect"] = 0.5f;
		modernhumvee_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        modernhumvee_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        modernhumvee_Human.default_attack = "mountedmachinegun";
        modernhumvee_Human.icon = "iconBoat";
        modernhumvee_Human.shadow_texture = "unitShadow_6";
        modernhumvee_Human.texture_asset = new ActorTextureSubAsset("actors/modernhumvee_Human/", false);
        modernhumvee_Human.special = true;
        modernhumvee_Human.has_advanced_textures = false;
        modernhumvee_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        modernhumvee_Human.animation_idle = ActorAnimationSequences.walk_0;
		modernhumvee_Human.animation_swim = ActorAnimationSequences.swim_0_3;
            modernhumvee_Human.name_locale = "Light Vehicle";
			modernhumvee_Human.addTrait("dodge");
			modernhumvee_Human.addTrait("dash");
			modernhumvee_Human.addTrait("fire_proof");
            AssetManager.actor_library.add(modernhumvee_Human);
			Localization.addLocalization(modernhumvee_Human.name_locale, modernhumvee_Human.name_locale);



	var howitzer_Human = AssetManager.actor_library.clone("howitzer_Human","baseWarUnit");
	howitzer_Human.die_in_lava = false;
        howitzer_Human.base_stats["mass_2"] = 600f;
        howitzer_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        howitzer_Human.base_stats["scale"] = 0.3f;
        howitzer_Human.base_stats["size"] = 1f;
		howitzer_Human.base_stats["mass"] = 1000f;
        howitzer_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		howitzer_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Human.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Human.base_stats["attack_speed"] = -10f;
		howitzer_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		howitzer_Human.base_stats["knockback"] = 3f;
		howitzer_Human.base_stats["accuracy"] = 0.3f;
		howitzer_Human.base_stats["targets"] = 3f;
		howitzer_Human.base_stats["area_of_effect"] = 4f;
		howitzer_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(30f);
        howitzer_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        howitzer_Human.default_attack = "artilleryattack";
        howitzer_Human.icon = "iconBoat";
		howitzer_Human.inspect_avatar_scale = 2f;
        howitzer_Human.shadow_texture = "unitShadow_6";
        howitzer_Human.texture_asset = new ActorTextureSubAsset("actors/howitzer_Human/", false);
        howitzer_Human.special = true;
        howitzer_Human.has_advanced_textures = false;
        howitzer_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        howitzer_Human.animation_idle = ActorAnimationSequences.walk_0;
		howitzer_Human.animation_swim = ActorAnimationSequences.swim_0_3;
            howitzer_Human.name_locale = "Artillery";
			howitzer_Human.addTrait("fire_proof");
            AssetManager.actor_library.add(howitzer_Human);
			Localization.addLocalization(howitzer_Human.name_locale, howitzer_Human.name_locale);



	var Tank_Human = AssetManager.actor_library.clone("Tank_Human","baseWarUnit");
	Tank_Human.die_in_lava = false;
        Tank_Human.base_stats["mass_2"] = 600f;
        Tank_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        Tank_Human.base_stats["scale"] = 0.3f;
        Tank_Human.base_stats["size"] = 1f;
		Tank_Human.base_stats["mass"] = 1000f;
        Tank_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		Tank_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Human.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Human.base_stats["attack_speed"] = 0.1f;
		Tank_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		Tank_Human.base_stats["knockback"] = 4f;
		Tank_Human.base_stats["accuracy"] = 0.8f;
		Tank_Human.base_stats["targets"] = 2f;
		Tank_Human.base_stats["area_of_effect"] = 2f;
		Tank_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        Tank_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        Tank_Human.default_attack = "tankpew";
        Tank_Human.icon = "iconBoat";
        Tank_Human.shadow_texture = "unitShadow_6";
        Tank_Human.texture_asset = new ActorTextureSubAsset("actors/Tank_Human/", false);
        Tank_Human.special = true;
		Tank_Human.inspect_avatar_scale = 2f;
        Tank_Human.has_advanced_textures = false;
        Tank_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        Tank_Human.animation_idle = Vehicles.idle_0_2;
		Tank_Human.animation_swim = ActorAnimationSequences.swim_0_2;
            Tank_Human.name_locale = "Tank";
			Tank_Human.addTrait("fire_proof");
			Tank_Human.addTrait("block");
			Tank_Human.addTrait("deflect_projectile");
            AssetManager.actor_library.add(Tank_Human);
			Localization.addLocalization(Tank_Human.name_locale, Tank_Human.name_locale);


	var wheeledtank_Human = AssetManager.actor_library.clone("wheeledtank_Human","baseWarUnit");
	wheeledtank_Human.die_in_lava = false;
        wheeledtank_Human.base_stats["mass_2"] = 600f;
        wheeledtank_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        wheeledtank_Human.base_stats["scale"] = 0.3f;
        wheeledtank_Human.base_stats["size"] = 1f;
		wheeledtank_Human.base_stats["mass"] = 1000f;
        wheeledtank_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		wheeledtank_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		wheeledtank_Human.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		wheeledtank_Human.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		wheeledtank_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		wheeledtank_Human.base_stats["knockback"] = 0.01f;
		wheeledtank_Human.base_stats["accuracy"] = 0.5f;
		wheeledtank_Human.base_stats["targets"] = 1f;
		wheeledtank_Human.base_stats["area_of_effect"] = 0.5f;
		wheeledtank_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        wheeledtank_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        wheeledtank_Human.default_attack = "tankpew";
        wheeledtank_Human.icon = "iconBoat";
		wheeledtank_Human.inspect_avatar_scale = 2f;
        wheeledtank_Human.shadow_texture = "unitShadow_6";
        wheeledtank_Human.texture_asset = new ActorTextureSubAsset("actors/wheeledtank_Human/", false);
        wheeledtank_Human.special = true;
        wheeledtank_Human.has_advanced_textures = false;
        wheeledtank_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        wheeledtank_Human.animation_idle = ActorAnimationSequences.walk_0;
		wheeledtank_Human.animation_swim = ActorAnimationSequences.swim_0_3;
            wheeledtank_Human.name_locale = "Armored Car";
			wheeledtank_Human.addTrait("dodge");
			wheeledtank_Human.addTrait("dash");
			wheeledtank_Human.addTrait("fire_proof");
            AssetManager.actor_library.add(wheeledtank_Human);
			Localization.addLocalization(wheeledtank_Human.name_locale, wheeledtank_Human.name_locale);



DecisionAsset missileArtilleryDecision = new DecisionAsset();
missileArtilleryDecision.id = "missileArtilleryDecision";
missileArtilleryDecision.priority = NeuroLayer.Layer_1_Low;
missileArtilleryDecision.path_icon = "ui/icons/MIRV";
missileArtilleryDecision.cooldown = 1;
missileArtilleryDecision.unique = true;
missileArtilleryDecision.weight = 1f;
missileArtilleryDecision.action_check_launch = delegate(Actor pActor)
{
    return MissileArtilleryEffect(pActor, null);
};
AssetManager.decisions_library.add(missileArtilleryDecision);

DecisionAsset bomberForceReloadRtbDecision = new DecisionAsset();
bomberForceReloadRtbDecision.id = "bomber_force_reload_rtb";
bomberForceReloadRtbDecision.priority = NeuroLayer.Layer_4_Critical;
bomberForceReloadRtbDecision.path_icon = "ui/Icons/iconArrowDestination";
bomberForceReloadRtbDecision.cooldown = 1;
bomberForceReloadRtbDecision.unique = true;
bomberForceReloadRtbDecision.weight = 10f;
bomberForceReloadRtbDecision.action_check_launch = delegate(Actor pActor)
{
	return BomberForceReloadRtbDecisionEffect(pActor);
};
AssetManager.decisions_library.add(bomberForceReloadRtbDecision);

DecisionAsset bomberLandAndReloadDecision = new DecisionAsset();
bomberLandAndReloadDecision.id = "bomber_land_and_reload";
bomberLandAndReloadDecision.priority = NeuroLayer.Layer_3_High;
bomberLandAndReloadDecision.path_icon = "ui/Icons/iconSleep";
bomberLandAndReloadDecision.cooldown = 1;
bomberLandAndReloadDecision.unique = true;
bomberLandAndReloadDecision.weight = 8f;
bomberLandAndReloadDecision.action_check_launch = delegate(Actor pActor)
{
	return BomberLandAndReloadDecisionEffect(pActor);
};
AssetManager.decisions_library.add(bomberLandAndReloadDecision);

DecisionAsset bomberTakeoffForWarDecision = new DecisionAsset();
bomberTakeoffForWarDecision.id = "bomber_takeoff_for_war";
bomberTakeoffForWarDecision.priority = NeuroLayer.Layer_3_High;
bomberTakeoffForWarDecision.path_icon = "ui/Icons/iconArrowAttackTarget";
bomberTakeoffForWarDecision.cooldown = 2;
bomberTakeoffForWarDecision.unique = true;
bomberTakeoffForWarDecision.weight = 6f;
bomberTakeoffForWarDecision.action_check_launch = delegate(Actor pActor)
{
	return BomberTakeoffForWarDecisionEffect(pActor);
};
AssetManager.decisions_library.add(bomberTakeoffForWarDecision);

DecisionAsset bomberEngageEnemyTargetsDecision = new DecisionAsset();
bomberEngageEnemyTargetsDecision.id = "bomber_engage_enemy_targets";
bomberEngageEnemyTargetsDecision.priority = NeuroLayer.Layer_3_High;
bomberEngageEnemyTargetsDecision.path_icon = "ui/icons/MIRV";
bomberEngageEnemyTargetsDecision.cooldown = 1;
bomberEngageEnemyTargetsDecision.unique = true;
bomberEngageEnemyTargetsDecision.weight = 7f;
bomberEngageEnemyTargetsDecision.action_check_launch = delegate(Actor pActor)
{
	return BomberEngageEnemyTargetsDecisionEffect(pActor);
};
AssetManager.decisions_library.add(bomberEngageEnemyTargetsDecision);

DecisionAsset bomberPeaceStationDecision = new DecisionAsset();
bomberPeaceStationDecision.id = "bomber_peace_station";
bomberPeaceStationDecision.priority = NeuroLayer.Layer_2_Moderate;
bomberPeaceStationDecision.path_icon = "ui/Icons/iconCity";
bomberPeaceStationDecision.cooldown = 3;
bomberPeaceStationDecision.unique = true;
bomberPeaceStationDecision.weight = 4f;
bomberPeaceStationDecision.action_check_launch = delegate(Actor pActor)
{
	return BomberPeaceStationDecisionEffect(pActor);
};
AssetManager.decisions_library.add(bomberPeaceStationDecision);


	var MissileSystem_Human = AssetManager.actor_library.clone("MissileSystem_Human","baseWarUnit");
	MissileSystem_Human.die_in_lava = false;
        MissileSystem_Human.base_stats["mass_2"] = 600f;
        MissileSystem_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        MissileSystem_Human.base_stats["scale"] = 0.3f;
        MissileSystem_Human.base_stats["size"] = 1f;
		MissileSystem_Human.base_stats["mass"] = 1000f;
        MissileSystem_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		MissileSystem_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		MissileSystem_Human.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		MissileSystem_Human.base_stats["attack_speed"] = 0.1f;
		MissileSystem_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		MissileSystem_Human.base_stats["knockback"] = 4f;
		MissileSystem_Human.base_stats["accuracy"] = 0.1f;
		MissileSystem_Human.base_stats["targets"] = 2f;
		MissileSystem_Human.base_stats["area_of_effect"] = 4f;
		MissileSystem_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
		MissileSystem_Human.inspect_avatar_scale = 2f;
        MissileSystem_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        MissileSystem_Human.default_attack = "MissileSystemmissile";
        MissileSystem_Human.icon = "iconBoat";
        MissileSystem_Human.shadow_texture = "unitShadow_6";
MissileSystem_Human.job = AssetLibrary<ActorAsset>.a<string>("decision");
MissileSystem_Human.addDecision("check_swearing");
MissileSystem_Human.addDecision("warrior_random_move");
MissileSystem_Human.addDecision("missileArtilleryDecision");
// MissileSystem_Human.addDecision("city_idle_walking");
MissileSystem_Human.addDecision("swim_to_island");
        MissileSystem_Human.texture_asset = new ActorTextureSubAsset("actors/MissileSystem_Human/", false);
        MissileSystem_Human.special = true;
        MissileSystem_Human.has_advanced_textures = false;
        MissileSystem_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        MissileSystem_Human.animation_idle = Vehicles.idle_0;
		MissileSystem_Human.animation_swim = ActorAnimationSequences.swim_0_3;
            MissileSystem_Human.name_locale = "Missile System";
			MissileSystem_Human.addTrait("fire_proof");
            AssetManager.actor_library.add(MissileSystem_Human);
			Localization.addLocalization(MissileSystem_Human.name_locale, MissileSystem_Human.name_locale);

	var supporttruck_Human = AssetManager.actor_library.clone("supporttruck_Human","baseWarUnit");
	supporttruck_Human.die_in_lava = false;
        supporttruck_Human.base_stats["mass_2"] = 600f;
        supporttruck_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        supporttruck_Human.base_stats["scale"] = 0.3f;
        supporttruck_Human.base_stats["size"] = 1f;
		supporttruck_Human.base_stats["mass"] = 1000f;
        supporttruck_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		supporttruck_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		supporttruck_Human.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		supporttruck_Human.base_stats["attack_speed"] = 0.1f;
		supporttruck_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		supporttruck_Human.base_stats["knockback"] = 4f;
		supporttruck_Human.base_stats["accuracy"] = 0.1f;
		supporttruck_Human.base_stats["targets"] = 2f;
		supporttruck_Human.base_stats["area_of_effect"] = 4f;
		supporttruck_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
        supporttruck_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        supporttruck_Human.default_attack = "base_attack";
        supporttruck_Human.icon = "iconBoat";
        supporttruck_Human.shadow_texture = "unitShadow_6";
		supporttruck_Human.inspect_avatar_scale = 1f;
        supporttruck_Human.texture_asset = new ActorTextureSubAsset("actors/supporttruck_Human/", false);
        supporttruck_Human.special = true;
        supporttruck_Human.has_advanced_textures = false;
        supporttruck_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        supporttruck_Human.animation_idle = ActorAnimationSequences.walk_0;
		supporttruck_Human.animation_swim = ActorAnimationSequences.swim_0_3;
            supporttruck_Human.name_locale = "Support Unit";
            supporttruck_Human.skip_fight_logic = true;
			supporttruck_Human.addTrait("fire_proof");
			   supporttruck_Human.job = AssetLibrary<ActorAsset>.a<string>("decision");
           supporttruck_Human.addDecision("check_swearing");
supporttruck_Human.addDecision("warrior_try_join_army_group");
supporttruck_Human.addDecision("city_walking_to_danger_zone");
supporttruck_Human.addDecision("check_cure");
supporttruck_Human.addDecision("warrior_army_leader_move_random");
supporttruck_Human.addDecision("check_heal");
supporttruck_Human.addDecision("warrior_army_follow_leader");
supporttruck_Human.addDecision("warrior_random_move");
supporttruck_Human.addDecision("check_warrior_transport");
supporttruck_Human.addDecision("swim_to_island");
            AssetManager.actor_library.add(supporttruck_Human);
			Localization.addLocalization(supporttruck_Human.name_locale, supporttruck_Human.name_locale);

/////give it cast heal trait




		var Heli_Human = AssetManager.actor_library.clone("Heli_Human","baseWarUnit");
	Heli_Human.die_in_lava = false;
	Heli_Human.animation_speed_based_on_walk_speed = false;
        Heli_Human.base_stats["mass_2"] = 600f;
        Heli_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Heli_Human.base_stats["scale"] = 0.3f;
        Heli_Human.base_stats["size"] = 1f;
		Heli_Human.base_stats["mass"] = 1000f;
        Heli_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		Heli_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Heli_Human.base_stats["armor"] = 0f;
		Heli_Human.base_stats["attack_speed"] = 10000f;
		Heli_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		Heli_Human.base_stats["knockback"] = 0.01f;
		Heli_Human.base_stats["accuracy"] = 0.7f;
		Heli_Human.base_stats["targets"] = 1f;
		Heli_Human.base_stats["area_of_effect"] = 0.5f;
		Heli_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        Heli_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        Heli_Human.default_attack = "mountedmachinegun";
        Heli_Human.addDecision("burn_tumors");
        Heli_Human.icon = "iconBoat";
        Heli_Human.shadow_texture = "unitShadow_6";
        Heli_Human.texture_asset = new ActorTextureSubAsset("actors/Heli_Human/", false);
        Heli_Human.special = true;
        Heli_Human.has_advanced_textures = false;
        Heli_Human.animation_walk = ActorAnimationSequences.walk_0_3;
        Heli_Human.animation_idle = ActorAnimationSequences.walk_0_3;
		Heli_Human.animation_swim = ActorAnimationSequences.walk_0_3;
            Heli_Human.name_locale = "Helicopter";
			Heli_Human.addTrait("fire_proof");
            Heli_Human.addTrait("freeze_proof");
			Heli_Human.flying = true;
			Heli_Human.very_high_flyer = true;
			Heli_Human.die_on_blocks = false;
			Heli_Human.inspect_avatar_scale = 0.5f;
			Heli_Human.ignore_blocks = true;
            AssetManager.actor_library.add(Heli_Human);
			Localization.addLocalization(Heli_Human.name_locale, Heli_Human.name_locale);


		var Bomber_Human = AssetManager.actor_library.clone("Bomber_Human","baseWarUnit");
	Bomber_Human.die_in_lava = false;
	Bomber_Human.animation_speed_based_on_walk_speed = false;
        Bomber_Human.base_stats["mass_2"] = 600f;
        Bomber_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Bomber_Human.base_stats["scale"] = 0.3f;
        Bomber_Human.base_stats["size"] = 1f;
		Bomber_Human.base_stats["mass"] = 1000f;
        Bomber_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		Bomber_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		Bomber_Human.base_stats["armor"] = 0f;
		Bomber_Human.base_stats["attack_speed"] = 5f;
		Bomber_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		Bomber_Human.base_stats["knockback"] = 2f;
		Bomber_Human.base_stats["accuracy"] = 0.7f;
		Bomber_Human.base_stats["targets"] = 10f;
		Bomber_Human.base_stats["area_of_effect"] = 0.5f;
		Bomber_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
        Bomber_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        Bomber_Human.default_attack = "BomberAttack";
        Bomber_Human.icon = "iconBoat";
        Bomber_Human.shadow_texture = "unitShadow_6";
        Bomber_Human.texture_asset = new ActorTextureSubAsset("actors/Bomber_Human/", false);
        Bomber_Human.special = true;
        Bomber_Human.can_flip = false;
		Bomber_Human.skip_fight_logic = false;
        Bomber_Human.has_advanced_textures = false;
        Bomber_Human.animation_walk = Vehicles.idle_0_19;
        Bomber_Human.animation_idle = Vehicles.idle_0_19;
		Bomber_Human.animation_swim = Vehicles.idle_0_19;
            Bomber_Human.name_locale = "Bomber";
			Bomber_Human.decision_ids = new List<string>();
			Bomber_Human.addDecision("check_swearing");
			Bomber_Human.addDecision("bomber_force_reload_rtb");
			Bomber_Human.addDecision("bomber_land_and_reload");
			Bomber_Human.addDecision("bomber_takeoff_for_war");
			Bomber_Human.addDecision("bomber_engage_enemy_targets");
			Bomber_Human.addDecision("bomber_peace_station");
			Bomber_Human.addTrait("fire_proof");
            Bomber_Human.addTrait("freeze_proof");
			Bomber_Human.flying = true;
			Bomber_Human.very_high_flyer = true;
			Bomber_Human.die_on_blocks = false;
			Bomber_Human.ignore_blocks = true;
			Bomber_Human.inspect_avatar_scale = 0.5f;
            AssetManager.actor_library.add(Bomber_Human);
			Localization.addLocalization(Bomber_Human.name_locale, Bomber_Human.name_locale);

	var FighterJet_Human = AssetManager.actor_library.clone("FighterJet_Human","baseWarUnit");
	FighterJet_Human.die_in_lava = false;
	FighterJet_Human.animation_speed_based_on_walk_speed = false;
        FighterJet_Human.base_stats["mass_2"] = 600f;
        FighterJet_Human.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        FighterJet_Human.base_stats["scale"] = 0.3f;
        FighterJet_Human.base_stats["size"] = 1f;
		FighterJet_Human.base_stats["mass"] = 1000f;
        FighterJet_Human.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		FighterJet_Human.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		FighterJet_Human.base_stats["armor"] = 0f;
		FighterJet_Human.base_stats["attack_speed"] = 0.3f;
		FighterJet_Human.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FighterJet_Human.base_stats["knockback"] = 2f;
		FighterJet_Human.base_stats["accuracy"] = 0.7f;
		FighterJet_Human.base_stats["targets"] = 1f;
		FighterJet_Human.base_stats["area_of_effect"] = 0.5f;
		FighterJet_Human.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FighterJet_Human.inspect_avatar_scale = 0.5f;
        FighterJet_Human.sound_hit = "event:/SFX/HIT/HitMetal";
        FighterJet_Human.default_attack = "fighterattack";
        FighterJet_Human.icon = "iconBoat";
        FighterJet_Human.shadow_texture = "unitShadow_6";
        FighterJet_Human.texture_asset = new ActorTextureSubAsset("actors/FighterJet_Human/", false);
        FighterJet_Human.special = true;
        FighterJet_Human.can_flip = false;
        FighterJet_Human.has_advanced_textures = false;
        FighterJet_Human.animation_walk = Vehicles.idle_0_9;
        FighterJet_Human.animation_idle = Vehicles.idle_0_9;
		FighterJet_Human.animation_swim = Vehicles.idle_0_9;
            FighterJet_Human.name_locale = "Fighter Jet";
			FighterJet_Human.addTrait("fire_proof");
            FighterJet_Human.addTrait("freeze_proof");
			FighterJet_Human.flying = true;
			FighterJet_Human.very_high_flyer = true;
			FighterJet_Human.die_on_blocks = false;
			FighterJet_Human.ignore_blocks = true;
            AssetManager.actor_library.add(FighterJet_Human);
			Localization.addLocalization(FighterJet_Human.name_locale, FighterJet_Human.name_locale);


	var F55FighterJet = AssetManager.actor_library.clone("F55FighterJet","baseWarUnit");
	F55FighterJet.die_in_lava = false;
	F55FighterJet.animation_speed_based_on_walk_speed = false;
        F55FighterJet.base_stats["mass_2"] = 600f;
        F55FighterJet.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        F55FighterJet.base_stats["scale"] = 0.3f;
        F55FighterJet.base_stats["size"] = 1f;
		F55FighterJet.base_stats["mass"] = 1000f;
        F55FighterJet.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		F55FighterJet.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		F55FighterJet.base_stats["armor"] = 0f;
		F55FighterJet.base_stats["attack_speed"] = 0.3f;
		F55FighterJet.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		F55FighterJet.base_stats["knockback"] = 2f;
		F55FighterJet.base_stats["accuracy"] = 0.7f;
		F55FighterJet.base_stats["targets"] = 1f;
		F55FighterJet.base_stats["area_of_effect"] = 0.5f;
		F55FighterJet.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        F55FighterJet.sound_hit = "event:/SFX/HIT/HitMetal";
        F55FighterJet.default_attack = "fighterattack";
        F55FighterJet.icon = "iconBoat";
        F55FighterJet.can_flip = false;
        F55FighterJet.shadow_texture = "unitShadow_6";
        F55FighterJet.texture_asset = new ActorTextureSubAsset("actors/F55FighterJet/", false);
        F55FighterJet.special = true;
		F55FighterJet.inspect_avatar_scale = 0.5f;
        F55FighterJet.has_advanced_textures = false;
        F55FighterJet.animation_walk = Vehicles.idle_0_9;
        F55FighterJet.animation_idle = Vehicles.idle_0_9;
		F55FighterJet.animation_swim = Vehicles.idle_0_9;
            F55FighterJet.name_locale = "F55FighterJet";
			F55FighterJet.addTrait("fire_proof");
            F55FighterJet.addTrait("freeze_proof");
			F55FighterJet.flying = true;
			F55FighterJet.very_high_flyer = true;
			F55FighterJet.die_on_blocks = false;
			F55FighterJet.ignore_blocks = true;
            AssetManager.actor_library.add(F55FighterJet);
			Localization.addLocalization(F55FighterJet.name_locale, F55FighterJet.name_locale);



	var modernhumvee_Ork = AssetManager.actor_library.clone("modernhumvee_Ork","baseWarUnit");
	modernhumvee_Ork.die_in_lava = false;
        modernhumvee_Ork.base_stats["mass_2"] = 600f;
        modernhumvee_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        modernhumvee_Ork.base_stats["scale"] = 0.3f;
        modernhumvee_Ork.base_stats["size"] = 1f;
		modernhumvee_Ork.base_stats["mass"] = 1000f;
        modernhumvee_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		modernhumvee_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		modernhumvee_Ork.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		modernhumvee_Ork.base_stats["attack_speed"] = 10000f;
		modernhumvee_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(10f);
		modernhumvee_Ork.base_stats["knockback"] = 0.01f;
		modernhumvee_Ork.base_stats["accuracy"] = 0.5f;
		modernhumvee_Ork.base_stats["targets"] = 1f;
		modernhumvee_Ork.base_stats["area_of_effect"] = 0.5f;
		modernhumvee_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        modernhumvee_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        modernhumvee_Ork.default_attack = "hordemachinegun";
        modernhumvee_Ork.icon = "iconBoat";
        modernhumvee_Ork.shadow_texture = "unitShadow_6";
        modernhumvee_Ork.texture_asset = new ActorTextureSubAsset("actors/modernhumvee_Ork/", false);
        modernhumvee_Ork.special = true;
        modernhumvee_Ork.has_advanced_textures = false;
        modernhumvee_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        modernhumvee_Ork.animation_idle = ActorAnimationSequences.walk_0;
		modernhumvee_Ork.animation_swim = ActorAnimationSequences.swim_0_3;
            modernhumvee_Ork.name_locale = "Light Vehicle";
			modernhumvee_Ork.addTrait("dodge");
			modernhumvee_Ork.addTrait("dash");
			modernhumvee_Ork.addTrait("fire_proof");
            AssetManager.actor_library.add(modernhumvee_Ork);
			Localization.addLocalization(modernhumvee_Ork.name_locale, modernhumvee_Ork.name_locale);

	var howitzer_Ork = AssetManager.actor_library.clone("howitzer_Ork","baseWarUnit");
	howitzer_Ork.die_in_lava = false;
        howitzer_Ork.base_stats["mass_2"] = 600f;
        howitzer_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        howitzer_Ork.base_stats["scale"] = 0.3f;
        howitzer_Ork.base_stats["size"] = 1f;
		howitzer_Ork.base_stats["mass"] = 1000f;
        howitzer_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		howitzer_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Ork.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Ork.base_stats["attack_speed"] = 0.1f;
		howitzer_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		howitzer_Ork.base_stats["knockback"] = 3f;
		howitzer_Ork.base_stats["accuracy"] = 0.3f;
		howitzer_Ork.base_stats["targets"] = 3f;
		howitzer_Ork.base_stats["area_of_effect"] = 4f;
		howitzer_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(30f);
        howitzer_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        howitzer_Ork.default_attack = "hordeartilleryshell";
        howitzer_Ork.icon = "iconBoat";
		howitzer_Ork.inspect_avatar_scale = 2f;
        howitzer_Ork.shadow_texture = "unitShadow_6";
        howitzer_Ork.texture_asset = new ActorTextureSubAsset("actors/howitzer_Ork/", false);
        howitzer_Ork.special = true;
        howitzer_Ork.has_advanced_textures = false;
        howitzer_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        howitzer_Ork.animation_idle = ActorAnimationSequences.walk_0;
		howitzer_Ork.animation_swim = ActorAnimationSequences.swim_0_3;
            howitzer_Ork.name_locale = "Artillery";
			howitzer_Ork.addTrait("fire_proof");
            AssetManager.actor_library.add(howitzer_Ork);
			Localization.addLocalization(howitzer_Ork.name_locale, howitzer_Ork.name_locale);

	var Tank_Ork = AssetManager.actor_library.clone("Tank_Ork","baseWarUnit");
	Tank_Ork.die_in_lava = false;
        Tank_Ork.base_stats["mass_2"] = 600f;
        Tank_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        Tank_Ork.base_stats["scale"] = 0.3f;
        Tank_Ork.base_stats["size"] = 1f;
		Tank_Ork.base_stats["mass"] = 1000f;
        Tank_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		Tank_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Ork.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Ork.base_stats["attack_speed"] = 0.1f;
		Tank_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		Tank_Ork.base_stats["knockback"] = 4f;
		Tank_Ork.base_stats["accuracy"] = 0.8f;
		Tank_Ork.base_stats["targets"] = 2f;
		Tank_Ork.base_stats["area_of_effect"] = 2f;
		Tank_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        Tank_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        Tank_Ork.default_attack = "hordetankpew";
        Tank_Ork.icon = "iconBoat";
        Tank_Ork.shadow_texture = "unitShadow_6";
        Tank_Ork.texture_asset = new ActorTextureSubAsset("actors/Tank_Ork/", false);
        Tank_Ork.special = true;
		Tank_Ork.inspect_avatar_scale = 2f;
        Tank_Ork.has_advanced_textures = false;
        Tank_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        Tank_Ork.animation_idle = Vehicles.idle_0_2;
		Tank_Ork.animation_swim = ActorAnimationSequences.swim_0_2;
            Tank_Ork.name_locale = "Tank";
			Tank_Ork.addTrait("fire_proof");
			Tank_Ork.addTrait("block");
			Tank_Ork.addTrait("deflect_projectile");
            AssetManager.actor_library.add(Tank_Ork);
			Localization.addLocalization(Tank_Ork.name_locale, Tank_Ork.name_locale);

	var wheeledtank_Ork = AssetManager.actor_library.clone("wheeledtank_Ork","baseWarUnit");
	wheeledtank_Ork.die_in_lava = false;
        wheeledtank_Ork.base_stats["mass_2"] = 600f;
        wheeledtank_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        wheeledtank_Ork.base_stats["scale"] = 0.3f;
        wheeledtank_Ork.base_stats["size"] = 1f;
		wheeledtank_Ork.base_stats["mass"] = 1000f;
        wheeledtank_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		wheeledtank_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		wheeledtank_Ork.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		wheeledtank_Ork.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		wheeledtank_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		wheeledtank_Ork.base_stats["knockback"] = 0.01f;
		wheeledtank_Ork.base_stats["accuracy"] = 0.5f;
		wheeledtank_Ork.base_stats["targets"] = 1f;
		wheeledtank_Ork.base_stats["area_of_effect"] = 0.5f;
		wheeledtank_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        wheeledtank_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        wheeledtank_Ork.default_attack = "hordetankpew";
        wheeledtank_Ork.icon = "iconBoat";
		wheeledtank_Ork.inspect_avatar_scale = 2f;
        wheeledtank_Ork.shadow_texture = "unitShadow_6";
        wheeledtank_Ork.texture_asset = new ActorTextureSubAsset("actors/wheeledtank_Ork/", false);
        wheeledtank_Ork.special = true;
        wheeledtank_Ork.has_advanced_textures = false;
        wheeledtank_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        wheeledtank_Ork.animation_idle = ActorAnimationSequences.walk_0;
		wheeledtank_Ork.animation_swim = ActorAnimationSequences.swim_0_3;
            wheeledtank_Ork.name_locale = "Armored Car";
			wheeledtank_Ork.addTrait("dodge");
			wheeledtank_Ork.addTrait("dash");
			wheeledtank_Ork.addTrait("fire_proof");
            AssetManager.actor_library.add(wheeledtank_Ork);
			Localization.addLocalization(wheeledtank_Ork.name_locale, wheeledtank_Ork.name_locale);




			DecisionAsset HORDEmissileArtilleryDecision = new DecisionAsset();
HORDEmissileArtilleryDecision.id = "HORDEmissileArtilleryDecision";
HORDEmissileArtilleryDecision.priority = NeuroLayer.Layer_1_Low;
HORDEmissileArtilleryDecision.path_icon = "ui/icons/MIRV";
HORDEmissileArtilleryDecision.cooldown = 1;
HORDEmissileArtilleryDecision.unique = true;
HORDEmissileArtilleryDecision.weight = 1f;
HORDEmissileArtilleryDecision.action_check_launch = delegate(Actor pActor)
{
    return HORDEmissileArtilleryEffect(pActor, null);
};
AssetManager.decisions_library.add(HORDEmissileArtilleryDecision);

	var MissileSystem_Ork = AssetManager.actor_library.clone("MissileSystem_Ork","baseWarUnit");
	MissileSystem_Ork.die_in_lava = false;
        MissileSystem_Ork.base_stats["mass_2"] = 600f;
        MissileSystem_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        MissileSystem_Ork.base_stats["scale"] = 0.3f;
        MissileSystem_Ork.base_stats["size"] = 1f;
		MissileSystem_Ork.base_stats["mass"] = 1000f;
        MissileSystem_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		MissileSystem_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		MissileSystem_Ork.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		MissileSystem_Ork.base_stats["attack_speed"] = 0.1f;
		MissileSystem_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		MissileSystem_Ork.base_stats["knockback"] = 4f;
		MissileSystem_Ork.base_stats["accuracy"] = 0.1f;
		MissileSystem_Ork.base_stats["targets"] = 3f;
		MissileSystem_Ork.base_stats["area_of_effect"] = 4f;
		MissileSystem_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
		MissileSystem_Ork.inspect_avatar_scale = 2f;
        MissileSystem_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        MissileSystem_Ork.default_attack = "MissileSystemHorde";
        MissileSystem_Ork.icon = "iconBoat";
        MissileSystem_Ork.shadow_texture = "unitShadow_6";
MissileSystem_Ork.job = AssetLibrary<ActorAsset>.a<string>("decision");
MissileSystem_Ork.addDecision("check_swearing");
MissileSystem_Ork.addDecision("warrior_random_move");
MissileSystem_Ork.addDecision("HORDEmissileArtilleryDecision");
// MissileSystem_Ork.addDecision("city_idle_walking");
MissileSystem_Ork.addDecision("swim_to_island");
        MissileSystem_Ork.texture_asset = new ActorTextureSubAsset("actors/MissileSystem_Ork/", false);
        MissileSystem_Ork.special = true;
        MissileSystem_Ork.has_advanced_textures = false;
        MissileSystem_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        MissileSystem_Ork.animation_idle = Vehicles.idle_0;
		MissileSystem_Ork.animation_swim = ActorAnimationSequences.swim_0_3;
            MissileSystem_Ork.name_locale = "Missile System";
			MissileSystem_Ork.addTrait("fire_proof");
            AssetManager.actor_library.add(MissileSystem_Ork);
			Localization.addLocalization(MissileSystem_Ork.name_locale, MissileSystem_Ork.name_locale);

	var supporttruck_Ork = AssetManager.actor_library.clone("supporttruck_Ork","baseWarUnit");
	supporttruck_Ork.die_in_lava = false;
        supporttruck_Ork.base_stats["mass_2"] = 600f;
        supporttruck_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        supporttruck_Ork.base_stats["scale"] = 0.3f;
        supporttruck_Ork.base_stats["size"] = 1f;
		supporttruck_Ork.base_stats["mass"] = 1000f;
        supporttruck_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		supporttruck_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		supporttruck_Ork.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		supporttruck_Ork.base_stats["attack_speed"] = 0.1f;
		supporttruck_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		supporttruck_Ork.base_stats["knockback"] = 4f;
		supporttruck_Ork.base_stats["accuracy"] = 0.1f;
		supporttruck_Ork.base_stats["targets"] = 2f;
		supporttruck_Ork.base_stats["area_of_effect"] = 4f;
		supporttruck_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
        supporttruck_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        supporttruck_Ork.default_attack = "base_attack";
        supporttruck_Ork.icon = "iconBoat";
        supporttruck_Ork.shadow_texture = "unitShadow_6";
		supporttruck_Ork.inspect_avatar_scale = 1f;
        supporttruck_Ork.texture_asset = new ActorTextureSubAsset("actors/supporttruck_Ork/", false);
        supporttruck_Ork.special = true;
        supporttruck_Ork.has_advanced_textures = false;
        supporttruck_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        supporttruck_Ork.animation_idle = ActorAnimationSequences.walk_0;
		supporttruck_Ork.animation_swim = ActorAnimationSequences.swim_0_3;
            supporttruck_Ork.name_locale = "Support Unit";
            supporttruck_Ork.skip_fight_logic = true;
			supporttruck_Ork.addTrait("fire_proof");
			   supporttruck_Ork.job = AssetLibrary<ActorAsset>.a<string>("decision");
           supporttruck_Ork.addDecision("check_swearing");
supporttruck_Ork.addDecision("warrior_try_join_army_group");
supporttruck_Ork.addDecision("city_walking_to_danger_zone");
supporttruck_Ork.addDecision("check_cure");
supporttruck_Ork.addDecision("warrior_army_leader_move_random");
supporttruck_Ork.addDecision("check_heal");
supporttruck_Ork.addDecision("warrior_army_follow_leader");
supporttruck_Ork.addDecision("warrior_random_move");
supporttruck_Ork.addDecision("check_warrior_transport");
supporttruck_Ork.addDecision("swim_to_island");
            AssetManager.actor_library.add(supporttruck_Ork);
			Localization.addLocalization(supporttruck_Ork.name_locale, supporttruck_Ork.name_locale);

		var Heli_Ork = AssetManager.actor_library.clone("Heli_Ork","baseWarUnit");
	Heli_Ork.die_in_lava = false;
	Heli_Ork.animation_speed_based_on_walk_speed = false;
        Heli_Ork.base_stats["mass_2"] = 600f;
        Heli_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Heli_Ork.base_stats["scale"] = 0.3f;
        Heli_Ork.base_stats["size"] = 1f;
		Heli_Ork.base_stats["mass"] = 1000f;
        Heli_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		Heli_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Heli_Ork.base_stats["armor"] = 0f;
		Heli_Ork.base_stats["attack_speed"] = 10000f;
		Heli_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		Heli_Ork.base_stats["knockback"] = 0.01f;
		Heli_Ork.base_stats["accuracy"] = 0.7f;
		Heli_Ork.base_stats["targets"] = 1f;
		Heli_Ork.base_stats["area_of_effect"] = 0.5f;
		Heli_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        Heli_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        Heli_Ork.default_attack = "hordemachinegun";
        Heli_Ork.icon = "iconBoat";
		Heli_Ork.addDecision("burn_tumors");
        Heli_Ork.shadow_texture = "unitShadow_6";
        Heli_Ork.texture_asset = new ActorTextureSubAsset("actors/Heli_Ork/", false);
        Heli_Ork.special = true;
        Heli_Ork.has_advanced_textures = false;
        Heli_Ork.animation_walk = ActorAnimationSequences.walk_0_3;
        Heli_Ork.animation_idle = ActorAnimationSequences.walk_0_3;
		Heli_Ork.animation_swim = ActorAnimationSequences.walk_0_3;
            Heli_Ork.name_locale = "Helicopter";
			Heli_Ork.addTrait("fire_proof");
            Heli_Ork.addTrait("freeze_proof");
			Heli_Ork.flying = true;
			Heli_Ork.very_high_flyer = true;
			Heli_Ork.die_on_blocks = false;
			Heli_Ork.inspect_avatar_scale = 0.5f;
			Heli_Ork.ignore_blocks = true;
            AssetManager.actor_library.add(Heli_Ork);
			Localization.addLocalization(Heli_Ork.name_locale, Heli_Ork.name_locale);

		var Bomber_Ork = AssetManager.actor_library.clone("Bomber_Ork","baseWarUnit");
	Bomber_Ork.die_in_lava = false;
	Bomber_Ork.animation_speed_based_on_walk_speed = false;
        Bomber_Ork.base_stats["mass_2"] = 600f;
        Bomber_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Bomber_Ork.base_stats["scale"] = 0.3f;
        Bomber_Ork.base_stats["size"] = 1f;
		Bomber_Ork.base_stats["mass"] = 1000f;
        Bomber_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		Bomber_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		Bomber_Ork.base_stats["armor"] = 0f;
		Bomber_Ork.base_stats["attack_speed"] = 5f;
		Bomber_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		Bomber_Ork.base_stats["knockback"] = 2f;
		Bomber_Ork.base_stats["accuracy"] = 0.7f;
		Bomber_Ork.base_stats["targets"] = 10f;
		Bomber_Ork.base_stats["area_of_effect"] = 0.5f;
		Bomber_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
        Bomber_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        Bomber_Ork.default_attack = "BomberAttackHorde";
        Bomber_Ork.icon = "iconBoat";
        Bomber_Ork.shadow_texture = "unitShadow_6";
        Bomber_Ork.texture_asset = new ActorTextureSubAsset("actors/Bomber_Ork/", false);
        Bomber_Ork.special = true;
        Bomber_Ork.can_flip = false;
        Bomber_Ork.has_advanced_textures = false;
        Bomber_Ork.animation_walk = Vehicles.idle_0_15;
        Bomber_Ork.animation_idle = Vehicles.idle_0_15;
		Bomber_Ork.animation_swim = Vehicles.idle_0_15;
            Bomber_Ork.name_locale = "Bomber";
			Bomber_Ork.addTrait("fire_proof");
            Bomber_Ork.addTrait("freeze_proof");
			Bomber_Ork.flying = true;
			Bomber_Ork.very_high_flyer = true;
			Bomber_Ork.die_on_blocks = false;
			Bomber_Ork.ignore_blocks = true;
			Bomber_Ork.inspect_avatar_scale = 0.5f;
            AssetManager.actor_library.add(Bomber_Ork);
			Localization.addLocalization(Bomber_Ork.name_locale, Bomber_Ork.name_locale);

	var FighterJet_Ork = AssetManager.actor_library.clone("FighterJet_Ork","baseWarUnit");
	FighterJet_Ork.die_in_lava = false;
	FighterJet_Ork.animation_speed_based_on_walk_speed = false;
        FighterJet_Ork.base_stats["mass_2"] = 600f;
        FighterJet_Ork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        FighterJet_Ork.base_stats["scale"] = 0.3f;
        FighterJet_Ork.base_stats["size"] = 1f;
		FighterJet_Ork.base_stats["mass"] = 1000f;
        FighterJet_Ork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		FighterJet_Ork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		FighterJet_Ork.base_stats["armor"] = 0f;
		FighterJet_Ork.base_stats["attack_speed"] = 0.3f;
		FighterJet_Ork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FighterJet_Ork.base_stats["knockback"] = 2f;
		FighterJet_Ork.base_stats["accuracy"] = 0.7f;
		FighterJet_Ork.base_stats["targets"] = 1f;
		FighterJet_Ork.base_stats["area_of_effect"] = 0.5f;
		FighterJet_Ork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FighterJet_Ork.inspect_avatar_scale = 0.5f;
        FighterJet_Ork.sound_hit = "event:/SFX/HIT/HitMetal";
        FighterJet_Ork.default_attack = "fighterattackHorde";
        FighterJet_Ork.icon = "iconBoat";
        FighterJet_Ork.shadow_texture = "unitShadow_6";
        FighterJet_Ork.texture_asset = new ActorTextureSubAsset("actors/FighterJet_Ork/", false);
        FighterJet_Ork.special = true;
        FighterJet_Ork.can_flip = false;
        FighterJet_Ork.has_advanced_textures = false;
        FighterJet_Ork.animation_walk = Vehicles.idle_0_7;
        FighterJet_Ork.animation_idle = Vehicles.idle_0_7;
		FighterJet_Ork.animation_swim = Vehicles.idle_0_7;
            FighterJet_Ork.name_locale = "Fighter Jet";
			FighterJet_Ork.addTrait("fire_proof");
            FighterJet_Ork.addTrait("freeze_proof");
			FighterJet_Ork.flying = true;
			FighterJet_Ork.very_high_flyer = true;
			FighterJet_Ork.die_on_blocks = false;
			FighterJet_Ork.ignore_blocks = true;
            AssetManager.actor_library.add(FighterJet_Ork);
			Localization.addLocalization(FighterJet_Ork.name_locale, FighterJet_Ork.name_locale);



	var modernhumvee_Dwarf = AssetManager.actor_library.clone("modernhumvee_Dwarf","baseWarUnit");
	modernhumvee_Dwarf.die_in_lava = false;
        modernhumvee_Dwarf.base_stats["mass_2"] = 600f;
        modernhumvee_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        modernhumvee_Dwarf.base_stats["scale"] = 0.3f;
        modernhumvee_Dwarf.base_stats["size"] = 1f;
		modernhumvee_Dwarf.base_stats["mass"] = 1000f;
        modernhumvee_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		modernhumvee_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		modernhumvee_Dwarf.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		modernhumvee_Dwarf.base_stats["attack_speed"] = 10000f;
		modernhumvee_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(10f);
		modernhumvee_Dwarf.base_stats["knockback"] = 0.01f;
		modernhumvee_Dwarf.base_stats["accuracy"] = 0.5f;
		modernhumvee_Dwarf.base_stats["targets"] = 1f;
		modernhumvee_Dwarf.base_stats["area_of_effect"] = 0.5f;
		modernhumvee_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        modernhumvee_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        modernhumvee_Dwarf.default_attack = "icemachinegun";
        modernhumvee_Dwarf.icon = "iconBoat";
        modernhumvee_Dwarf.shadow_texture = "unitShadow_6";
        modernhumvee_Dwarf.texture_asset = new ActorTextureSubAsset("actors/modernhumvee_Dwarf/", false);
        modernhumvee_Dwarf.special = true;
        modernhumvee_Dwarf.has_advanced_textures = false;
        modernhumvee_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        modernhumvee_Dwarf.animation_idle = ActorAnimationSequences.walk_0;
		modernhumvee_Dwarf.animation_swim = ActorAnimationSequences.swim_0_3;
            modernhumvee_Dwarf.name_locale = "Light Vehicle";
			modernhumvee_Dwarf.addTrait("dodge");
			modernhumvee_Dwarf.addTrait("dash");
			modernhumvee_Dwarf.addTrait("fire_proof");
            AssetManager.actor_library.add(modernhumvee_Dwarf);
			Localization.addLocalization(modernhumvee_Dwarf.name_locale, modernhumvee_Dwarf.name_locale);

	var Tank_Dwarf = AssetManager.actor_library.clone("Tank_Dwarf","baseWarUnit");
	Tank_Dwarf.die_in_lava = false;
        Tank_Dwarf.base_stats["mass_2"] = 600f;
        Tank_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        Tank_Dwarf.base_stats["scale"] = 0.3f;
        Tank_Dwarf.base_stats["size"] = 1f;
		Tank_Dwarf.base_stats["mass"] = 1000f;
        Tank_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		Tank_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Dwarf.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Dwarf.base_stats["attack_speed"] = 0.1f;
		Tank_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		Tank_Dwarf.base_stats["knockback"] = 4f;
		Tank_Dwarf.base_stats["accuracy"] = 0.8f;
		Tank_Dwarf.base_stats["targets"] = 2f;
		Tank_Dwarf.base_stats["area_of_effect"] = 2f;
		Tank_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        Tank_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        Tank_Dwarf.default_attack = "crystaltankpew";
        Tank_Dwarf.icon = "iconBoat";
        Tank_Dwarf.shadow_texture = "unitShadow_6";
        Tank_Dwarf.texture_asset = new ActorTextureSubAsset("actors/Tank_Dwarf/", false);
        Tank_Dwarf.special = true;
		Tank_Dwarf.inspect_avatar_scale = 2f;
        Tank_Dwarf.has_advanced_textures = false;
        Tank_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        Tank_Dwarf.animation_idle = Vehicles.idle_0_2;
		Tank_Dwarf.animation_swim = ActorAnimationSequences.swim_0_2;
            Tank_Dwarf.name_locale = "Tank";
			Tank_Dwarf.addTrait("fire_proof");
			Tank_Dwarf.addTrait("block");
			Tank_Dwarf.addTrait("deflect_projectile");
            AssetManager.actor_library.add(Tank_Dwarf);
			Localization.addLocalization(Tank_Dwarf.name_locale, Tank_Dwarf.name_locale);



			DecisionAsset HARDENmissileArtilleryDecision = new DecisionAsset();
HARDENmissileArtilleryDecision.id = "HARDENmissileArtilleryDecision";
HARDENmissileArtilleryDecision.priority = NeuroLayer.Layer_1_Low;
HARDENmissileArtilleryDecision.path_icon = "ui/icons/MIRV";
HARDENmissileArtilleryDecision.cooldown = 1;
HARDENmissileArtilleryDecision.unique = true;
HARDENmissileArtilleryDecision.weight = 1f;
HARDENmissileArtilleryDecision.action_check_launch = delegate(Actor pActor)
{
    return HARDENmissileArtilleryEffect(pActor, null);
};
AssetManager.decisions_library.add(HARDENmissileArtilleryDecision);


	var MissileSystem_Dwarf = AssetManager.actor_library.clone("MissileSystem_Dwarf","baseWarUnit");
	MissileSystem_Dwarf.die_in_lava = false;
        MissileSystem_Dwarf.base_stats["mass_2"] = 600f;
        MissileSystem_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        MissileSystem_Dwarf.base_stats["scale"] = 0.3f;
        MissileSystem_Dwarf.base_stats["size"] = 1f;
		MissileSystem_Dwarf.base_stats["mass"] = 1000f;
        MissileSystem_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		MissileSystem_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		MissileSystem_Dwarf.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		MissileSystem_Dwarf.base_stats["attack_speed"] = 0.1f;
		MissileSystem_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		MissileSystem_Dwarf.base_stats["knockback"] = 4f;
		MissileSystem_Dwarf.base_stats["accuracy"] = 0.1f;
		MissileSystem_Dwarf.base_stats["targets"] = 2f;
		MissileSystem_Dwarf.base_stats["area_of_effect"] = 4f;
		MissileSystem_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
		MissileSystem_Dwarf.inspect_avatar_scale = 2f;
MissileSystem_Dwarf.job = AssetLibrary<ActorAsset>.a<string>("decision");
MissileSystem_Dwarf.addDecision("check_swearing");
MissileSystem_Dwarf.addDecision("warrior_random_move");
MissileSystem_Dwarf.addDecision("HARDENmissileArtilleryDecision");
// MissileSystem_Dwarf.addDecision("city_idle_walking");
MissileSystem_Dwarf.addDecision("swim_to_island");
        MissileSystem_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        MissileSystem_Dwarf.default_attack = "MissileSystemHarden";
        MissileSystem_Dwarf.icon = "iconBoat";
        MissileSystem_Dwarf.shadow_texture = "unitShadow_6";
        MissileSystem_Dwarf.texture_asset = new ActorTextureSubAsset("actors/MissileSystem_Dwarf/", false);
        MissileSystem_Dwarf.special = true;
        MissileSystem_Dwarf.has_advanced_textures = false;
        MissileSystem_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        MissileSystem_Dwarf.animation_idle = Vehicles.idle_0;
		MissileSystem_Dwarf.animation_swim = ActorAnimationSequences.swim_0_3;
            MissileSystem_Dwarf.name_locale = "Missile System";
			MissileSystem_Dwarf.addTrait("fire_proof");
            AssetManager.actor_library.add(MissileSystem_Dwarf);
			Localization.addLocalization(MissileSystem_Dwarf.name_locale, MissileSystem_Dwarf.name_locale);

	var supporttruck_Dwarf = AssetManager.actor_library.clone("supporttruck_Dwarf","baseWarUnit");
	supporttruck_Dwarf.die_in_lava = false;
        supporttruck_Dwarf.base_stats["mass_2"] = 600f;
        supporttruck_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        supporttruck_Dwarf.base_stats["scale"] = 0.3f;
        supporttruck_Dwarf.base_stats["size"] = 1f;
		supporttruck_Dwarf.base_stats["mass"] = 1000f;
        supporttruck_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		supporttruck_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		supporttruck_Dwarf.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		supporttruck_Dwarf.base_stats["attack_speed"] = 0.1f;
		supporttruck_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		supporttruck_Dwarf.base_stats["knockback"] = 4f;
		supporttruck_Dwarf.base_stats["accuracy"] = 0.1f;
		supporttruck_Dwarf.base_stats["targets"] = 3f;
		supporttruck_Dwarf.base_stats["area_of_effect"] = 4f;
		supporttruck_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
        supporttruck_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        supporttruck_Dwarf.default_attack = "base_attack";
        supporttruck_Dwarf.icon = "iconBoat";
        supporttruck_Dwarf.shadow_texture = "unitShadow_6";
		supporttruck_Dwarf.inspect_avatar_scale = 1f;
        supporttruck_Dwarf.texture_asset = new ActorTextureSubAsset("actors/supporttruck_Dwarf/", false);
        supporttruck_Dwarf.special = true;
        supporttruck_Dwarf.has_advanced_textures = false;
        supporttruck_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        supporttruck_Dwarf.animation_idle = ActorAnimationSequences.walk_0;
		supporttruck_Dwarf.animation_swim = ActorAnimationSequences.swim_0_3;
            supporttruck_Dwarf.name_locale = "Support Unit";
            supporttruck_Dwarf.skip_fight_logic = true;
			supporttruck_Dwarf.addTrait("fire_proof");
			   supporttruck_Dwarf.job = AssetLibrary<ActorAsset>.a<string>("decision");
           supporttruck_Dwarf.addDecision("check_swearing");
supporttruck_Dwarf.addDecision("warrior_try_join_army_group");
supporttruck_Dwarf.addDecision("city_walking_to_danger_zone");
supporttruck_Dwarf.addDecision("check_cure");
supporttruck_Dwarf.addDecision("warrior_army_leader_move_random");
supporttruck_Dwarf.addDecision("check_heal");
supporttruck_Dwarf.addDecision("warrior_army_follow_leader");
supporttruck_Dwarf.addDecision("warrior_random_move");
supporttruck_Dwarf.addDecision("check_warrior_transport");
supporttruck_Dwarf.addDecision("swim_to_island");
            AssetManager.actor_library.add(supporttruck_Dwarf);
			Localization.addLocalization(supporttruck_Dwarf.name_locale, supporttruck_Dwarf.name_locale);

		var Heli_Dwarf = AssetManager.actor_library.clone("Heli_Dwarf","baseWarUnit");
	Heli_Dwarf.die_in_lava = false;
	Heli_Dwarf.animation_speed_based_on_walk_speed = false;
        Heli_Dwarf.base_stats["mass_2"] = 600f;
        Heli_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Heli_Dwarf.base_stats["scale"] = 0.3f;
        Heli_Dwarf.base_stats["size"] = 1f;
		Heli_Dwarf.base_stats["mass"] = 1000f;
        Heli_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		Heli_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Heli_Dwarf.base_stats["armor"] = 0f;
		Heli_Dwarf.base_stats["attack_speed"] = 10000f;
		Heli_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		Heli_Dwarf.base_stats["knockback"] = 0.01f;
		Heli_Dwarf.base_stats["accuracy"] = 0.7f;
		Heli_Dwarf.base_stats["targets"] = 1f;
		Heli_Dwarf.base_stats["area_of_effect"] = 0.5f;
		Heli_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        Heli_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        Heli_Dwarf.default_attack = "icemachinegun";
        Heli_Dwarf.icon = "iconBoat";
        Heli_Dwarf.shadow_texture = "unitShadow_6";
        Heli_Dwarf.texture_asset = new ActorTextureSubAsset("actors/Heli_Dwarf/", false);
        Heli_Dwarf.special = true;
        Heli_Dwarf.has_advanced_textures = false;
        Heli_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        Heli_Dwarf.animation_idle = ActorAnimationSequences.walk_0_3;
		Heli_Dwarf.animation_swim = ActorAnimationSequences.walk_0_3;
            Heli_Dwarf.name_locale = "Helicopter";
            Heli_Dwarf.addDecision("burn_tumors");
			Heli_Dwarf.addTrait("fire_proof");
            Heli_Dwarf.addTrait("freeze_proof");
			Heli_Dwarf.flying = true;
			Heli_Dwarf.very_high_flyer = true;
			Heli_Dwarf.die_on_blocks = false;
			Heli_Dwarf.inspect_avatar_scale = 0.5f;
			Heli_Dwarf.ignore_blocks = true;
            AssetManager.actor_library.add(Heli_Dwarf);
			Localization.addLocalization(Heli_Dwarf.name_locale, Heli_Dwarf.name_locale);

		var Bomber_Dwarf = AssetManager.actor_library.clone("Bomber_Dwarf","baseWarUnit");
	Bomber_Dwarf.die_in_lava = false;
	Bomber_Dwarf.animation_speed_based_on_walk_speed = false;
        Bomber_Dwarf.base_stats["mass_2"] = 600f;
        Bomber_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Bomber_Dwarf.base_stats["scale"] = 0.3f;
        Bomber_Dwarf.base_stats["size"] = 1f;
		Bomber_Dwarf.base_stats["mass"] = 1000f;
        Bomber_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		Bomber_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		Bomber_Dwarf.base_stats["armor"] = 0f;
		Bomber_Dwarf.base_stats["attack_speed"] = 5f;
		Bomber_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		Bomber_Dwarf.base_stats["knockback"] = 2f;
		Bomber_Dwarf.base_stats["accuracy"] = 0.7f;
		Bomber_Dwarf.base_stats["targets"] = 10f;
		Bomber_Dwarf.base_stats["area_of_effect"] = 0.5f;
		Bomber_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
        Bomber_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        Bomber_Dwarf.default_attack = "BomberAttackHarden";
        Bomber_Dwarf.icon = "iconBoat";
        Bomber_Dwarf.shadow_texture = "unitShadow_6";
        Bomber_Dwarf.texture_asset = new ActorTextureSubAsset("actors/Bomber_Dwarf/", false);
        Bomber_Dwarf.special = true;
        Bomber_Dwarf.can_flip = false;
        Bomber_Dwarf.has_advanced_textures = false;
        Bomber_Dwarf.animation_walk = Vehicles.idle_0_7;
        Bomber_Dwarf.animation_idle = Vehicles.idle_0_7;
		Bomber_Dwarf.animation_swim = Vehicles.idle_0_7;
            Bomber_Dwarf.name_locale = "Bomber";
			Bomber_Dwarf.addTrait("fire_proof");
            Bomber_Dwarf.addTrait("freeze_proof");
			Bomber_Dwarf.flying = true;
			Bomber_Dwarf.very_high_flyer = true;
			Bomber_Dwarf.die_on_blocks = false;
			Bomber_Dwarf.ignore_blocks = true;
			Bomber_Dwarf.inspect_avatar_scale = 0.5f;
            AssetManager.actor_library.add(Bomber_Dwarf);
			Localization.addLocalization(Bomber_Dwarf.name_locale, Bomber_Dwarf.name_locale);

	var FighterJet_Dwarf = AssetManager.actor_library.clone("FighterJet_Dwarf","baseWarUnit");
	FighterJet_Dwarf.die_in_lava = false;
	FighterJet_Dwarf.animation_speed_based_on_walk_speed = false;
        FighterJet_Dwarf.base_stats["mass_2"] = 600f;
        FighterJet_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        FighterJet_Dwarf.base_stats["scale"] = 0.3f;
        FighterJet_Dwarf.base_stats["size"] = 1f;
		FighterJet_Dwarf.base_stats["mass"] = 1000f;
        FighterJet_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		FighterJet_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		FighterJet_Dwarf.base_stats["armor"] = 0f;
		FighterJet_Dwarf.base_stats["attack_speed"] = 0.3f;
		FighterJet_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FighterJet_Dwarf.base_stats["knockback"] = 2f;
		FighterJet_Dwarf.base_stats["accuracy"] = 0.7f;
		FighterJet_Dwarf.base_stats["targets"] = 1f;
		FighterJet_Dwarf.base_stats["area_of_effect"] = 0.5f;
		FighterJet_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FighterJet_Dwarf.inspect_avatar_scale = 0.5f;
        FighterJet_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        FighterJet_Dwarf.default_attack = "fighterattackHarden";
        FighterJet_Dwarf.icon = "iconBoat";
        FighterJet_Dwarf.shadow_texture = "unitShadow_6";
        FighterJet_Dwarf.texture_asset = new ActorTextureSubAsset("actors/FighterJet_Dwarf/", false);
        FighterJet_Dwarf.special = true;
        FighterJet_Dwarf.can_flip = false;
        FighterJet_Dwarf.has_advanced_textures = false;
        FighterJet_Dwarf.animation_walk = Vehicles.idle_0_9;
        FighterJet_Dwarf.animation_idle = Vehicles.idle_0_9;
		FighterJet_Dwarf.animation_swim = Vehicles.idle_0_9;
            FighterJet_Dwarf.name_locale = "Fighter Jet";
			FighterJet_Dwarf.addTrait("fire_proof");
            FighterJet_Dwarf.addTrait("freeze_proof");
			FighterJet_Dwarf.flying = true;
			FighterJet_Dwarf.very_high_flyer = true;
			FighterJet_Dwarf.die_on_blocks = false;
			FighterJet_Dwarf.ignore_blocks = true;
            AssetManager.actor_library.add(FighterJet_Dwarf);
			Localization.addLocalization(FighterJet_Dwarf.name_locale, FighterJet_Dwarf.name_locale);

var howitzer_Dwarf = AssetManager.actor_library.clone("howitzer_Dwarf","baseWarUnit");
	howitzer_Dwarf.die_in_lava = false;
        howitzer_Dwarf.base_stats["mass_2"] = 600f;
        howitzer_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        howitzer_Dwarf.base_stats["scale"] = 0.3f;
        howitzer_Dwarf.base_stats["size"] = 1f;
		howitzer_Dwarf.base_stats["mass"] = 1000f;
        howitzer_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		howitzer_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Dwarf.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Dwarf.base_stats["attack_speed"] = 0.1f;
		howitzer_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		howitzer_Dwarf.base_stats["knockback"] = 3f;
		howitzer_Dwarf.base_stats["accuracy"] = 0.3f;
		howitzer_Dwarf.base_stats["targets"] = 3f;
		howitzer_Dwarf.base_stats["area_of_effect"] = 4f;
		howitzer_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(30f);
        howitzer_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        howitzer_Dwarf.default_attack = "iceartilleryshell";
        howitzer_Dwarf.icon = "iconBoat";
		howitzer_Dwarf.inspect_avatar_scale = 2f;
        howitzer_Dwarf.shadow_texture = "unitShadow_6";
        howitzer_Dwarf.texture_asset = new ActorTextureSubAsset("actors/howitzer_Dwarf/", false);
        howitzer_Dwarf.special = true;
        howitzer_Dwarf.has_advanced_textures = false;
        howitzer_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        howitzer_Dwarf.animation_idle = ActorAnimationSequences.walk_0;
		howitzer_Dwarf.animation_swim = ActorAnimationSequences.swim_0_3;
            howitzer_Dwarf.name_locale = "Artillery";
			howitzer_Dwarf.addTrait("fire_proof");
            AssetManager.actor_library.add(howitzer_Dwarf);
			Localization.addLocalization(howitzer_Dwarf.name_locale, howitzer_Dwarf.name_locale);

			var wheeledtank_Dwarf = AssetManager.actor_library.clone("wheeledtank_Dwarf","baseWarUnit");
	wheeledtank_Dwarf.die_in_lava = false;
        wheeledtank_Dwarf.base_stats["mass_2"] = 600f;
        wheeledtank_Dwarf.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        wheeledtank_Dwarf.base_stats["scale"] = 0.3f;
        wheeledtank_Dwarf.base_stats["size"] = 1f;
		wheeledtank_Dwarf.base_stats["mass"] = 1000f;
        wheeledtank_Dwarf.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		wheeledtank_Dwarf.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		wheeledtank_Dwarf.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		wheeledtank_Dwarf.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		wheeledtank_Dwarf.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		wheeledtank_Dwarf.base_stats["knockback"] = 0.01f;
		wheeledtank_Dwarf.base_stats["accuracy"] = 0.5f;
		wheeledtank_Dwarf.base_stats["targets"] = 1f;
		wheeledtank_Dwarf.base_stats["area_of_effect"] = 0.5f;
		wheeledtank_Dwarf.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        wheeledtank_Dwarf.sound_hit = "event:/SFX/HIT/HitMetal";
        wheeledtank_Dwarf.default_attack = "crystaltankpew";
        wheeledtank_Dwarf.icon = "iconBoat";
		wheeledtank_Dwarf.inspect_avatar_scale = 2f;
        wheeledtank_Dwarf.shadow_texture = "unitShadow_6";
        wheeledtank_Dwarf.texture_asset = new ActorTextureSubAsset("actors/wheeledtank_Dwarf/", false);
        wheeledtank_Dwarf.special = true;
        wheeledtank_Dwarf.has_advanced_textures = false;
        wheeledtank_Dwarf.animation_walk = ActorAnimationSequences.walk_0_3;
        wheeledtank_Dwarf.animation_idle = ActorAnimationSequences.walk_0;
		wheeledtank_Dwarf.animation_swim = ActorAnimationSequences.swim_0_3;
            wheeledtank_Dwarf.name_locale = "Armored Car";
			wheeledtank_Dwarf.addTrait("dodge");
			wheeledtank_Dwarf.addTrait("dash");
			wheeledtank_Dwarf.addTrait("fire_proof");
            AssetManager.actor_library.add(wheeledtank_Dwarf);
			Localization.addLocalization(wheeledtank_Dwarf.name_locale, wheeledtank_Dwarf.name_locale);



	var modernhumvee_Gaia = AssetManager.actor_library.clone("modernhumvee_Gaia","baseWarUnit");
	modernhumvee_Gaia.die_in_lava = false;
        modernhumvee_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        modernhumvee_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        modernhumvee_Gaia.base_stats["scale"] = 0.3f;
        modernhumvee_Gaia.base_stats["size"] = 1f;
		modernhumvee_Gaia.base_stats["mass"] = 1000f;
        modernhumvee_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		modernhumvee_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		modernhumvee_Gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		modernhumvee_Gaia.base_stats["attack_speed"] = 10000f;
		modernhumvee_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(10f);
		modernhumvee_Gaia.base_stats["knockback"] = 0.01f;
		modernhumvee_Gaia.base_stats["accuracy"] = 0.5f;
		modernhumvee_Gaia.base_stats["targets"] = 1f;
		modernhumvee_Gaia.base_stats["area_of_effect"] = 0.5f;
		modernhumvee_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        modernhumvee_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        modernhumvee_Gaia.default_attack = "gaiamachinegun";
        modernhumvee_Gaia.icon = "iconBoat";
        modernhumvee_Gaia.shadow_texture = "unitShadow_6";
        modernhumvee_Gaia.texture_asset = new ActorTextureSubAsset("actors/modernhumvee_Gaia/", false);
        modernhumvee_Gaia.special = true;
        modernhumvee_Gaia.has_advanced_textures = false;
        modernhumvee_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        modernhumvee_Gaia.animation_idle = ActorAnimationSequences.walk_0;
		modernhumvee_Gaia.animation_swim = ActorAnimationSequences.swim_0_3;
            modernhumvee_Gaia.name_locale = "Light Vehicle";
			modernhumvee_Gaia.addTrait("dodge");
			modernhumvee_Gaia.addTrait("dash");
			modernhumvee_Gaia.addTrait("fire_proof");
            AssetManager.actor_library.add(modernhumvee_Gaia);
			Localization.addLocalization(modernhumvee_Gaia.name_locale, modernhumvee_Gaia.name_locale);

	var howitzer_Gaia = AssetManager.actor_library.clone("howitzer_Gaia","baseWarUnit");
	howitzer_Gaia.die_in_lava = false;
        howitzer_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        howitzer_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        howitzer_Gaia.base_stats["scale"] = 0.3f;
        howitzer_Gaia.base_stats["size"] = 1f;
		howitzer_Gaia.base_stats["mass"] = 1000f;
        howitzer_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		howitzer_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		howitzer_Gaia.base_stats["attack_speed"] = 0.1f;
		howitzer_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		howitzer_Gaia.base_stats["knockback"] = 3f;
		howitzer_Gaia.base_stats["accuracy"] = 0.3f;
		howitzer_Gaia.base_stats["targets"] = 3f;
		howitzer_Gaia.base_stats["area_of_effect"] = 4f;
		howitzer_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(30f);
        howitzer_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        howitzer_Gaia.default_attack = "gaiaartilleryshell";
        howitzer_Gaia.icon = "iconBoat";
		howitzer_Gaia.inspect_avatar_scale = 2f;
        howitzer_Gaia.shadow_texture = "unitShadow_6";
        howitzer_Gaia.texture_asset = new ActorTextureSubAsset("actors/howitzer_Gaia/", false);
        howitzer_Gaia.special = true;
        howitzer_Gaia.has_advanced_textures = false;
        howitzer_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        howitzer_Gaia.animation_idle = ActorAnimationSequences.walk_0;
		howitzer_Gaia.animation_swim = ActorAnimationSequences.swim_0_3;
            howitzer_Gaia.name_locale = "Artillery";
			howitzer_Gaia.addTrait("fire_proof");
            AssetManager.actor_library.add(howitzer_Gaia);
			Localization.addLocalization(howitzer_Gaia.name_locale, howitzer_Gaia.name_locale);

	var Tank_Gaia = AssetManager.actor_library.clone("Tank_Gaia","baseWarUnit");
	Tank_Gaia.die_in_lava = false;
        Tank_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        Tank_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        Tank_Gaia.base_stats["scale"] = 0.3f;
        Tank_Gaia.base_stats["size"] = 1f;
		Tank_Gaia.base_stats["mass"] = 1000f;
        Tank_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		Tank_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		Tank_Gaia.base_stats["attack_speed"] = 0.1f;
		Tank_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		Tank_Gaia.base_stats["knockback"] = 4f;
		Tank_Gaia.base_stats["accuracy"] = 0.8f;
		Tank_Gaia.base_stats["targets"] = 2f;
		Tank_Gaia.base_stats["area_of_effect"] = 2f;
		Tank_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        Tank_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        Tank_Gaia.default_attack = "gaiatankpew";
        Tank_Gaia.icon = "iconBoat";
        Tank_Gaia.shadow_texture = "unitShadow_6";
        Tank_Gaia.texture_asset = new ActorTextureSubAsset("actors/Tank_Gaia/", false);
        Tank_Gaia.special = true;
		Tank_Gaia.inspect_avatar_scale = 2f;
        Tank_Gaia.has_advanced_textures = false;
        Tank_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        Tank_Gaia.animation_idle = ActorAnimationSequences.walk_0;
		Tank_Gaia.animation_swim = ActorAnimationSequences.swim_0_2;
            Tank_Gaia.name_locale = "Tank";
			Tank_Gaia.addTrait("fire_proof");
			Tank_Gaia.addTrait("block");
			Tank_Gaia.addTrait("deflect_projectile");
            AssetManager.actor_library.add(Tank_Gaia);
			Localization.addLocalization(Tank_Gaia.name_locale, Tank_Gaia.name_locale);

	var wheeledtank_Gaia = AssetManager.actor_library.clone("wheeledtank_Gaia","baseWarUnit");
	wheeledtank_Gaia.die_in_lava = false;
        wheeledtank_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        wheeledtank_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        wheeledtank_Gaia.base_stats["scale"] = 0.3f;
        wheeledtank_Gaia.base_stats["size"] = 1f;
		wheeledtank_Gaia.base_stats["mass"] = 1000f;
        wheeledtank_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		wheeledtank_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		wheeledtank_Gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		wheeledtank_Gaia.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		wheeledtank_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		wheeledtank_Gaia.base_stats["knockback"] = 0.01f;
		wheeledtank_Gaia.base_stats["accuracy"] = 0.5f;
		wheeledtank_Gaia.base_stats["targets"] = 1f;
		wheeledtank_Gaia.base_stats["area_of_effect"] = 0.5f;
		wheeledtank_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        wheeledtank_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        wheeledtank_Gaia.default_attack = "gaiatankpew";
        wheeledtank_Gaia.icon = "iconBoat";
		wheeledtank_Gaia.inspect_avatar_scale = 2f;
        wheeledtank_Gaia.shadow_texture = "unitShadow_6";
        wheeledtank_Gaia.texture_asset = new ActorTextureSubAsset("actors/wheeledtank_Gaia/", false);
        wheeledtank_Gaia.special = true;
        wheeledtank_Gaia.has_advanced_textures = false;
        wheeledtank_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        wheeledtank_Gaia.animation_idle = ActorAnimationSequences.walk_0;
		wheeledtank_Gaia.animation_swim = ActorAnimationSequences.swim_0_3;
            wheeledtank_Gaia.name_locale = "Armored Car";
			wheeledtank_Gaia.addTrait("dodge");
			wheeledtank_Gaia.addTrait("dash");
			wheeledtank_Gaia.addTrait("fire_proof");
            AssetManager.actor_library.add(wheeledtank_Gaia);
			Localization.addLocalization(wheeledtank_Gaia.name_locale, wheeledtank_Gaia.name_locale);



			DecisionAsset GAIAmissileArtilleryDecision = new DecisionAsset();
GAIAmissileArtilleryDecision.id = "GAIAmissileArtilleryDecision";
GAIAmissileArtilleryDecision.priority = NeuroLayer.Layer_1_Low;
GAIAmissileArtilleryDecision.path_icon = "ui/icons/MIRV";
GAIAmissileArtilleryDecision.cooldown = 1;
GAIAmissileArtilleryDecision.unique = true;
GAIAmissileArtilleryDecision.weight = 1f;
GAIAmissileArtilleryDecision.action_check_launch = delegate(Actor pActor)
{
    return GAIAmissileArtilleryEffect(pActor, null);
};
AssetManager.decisions_library.add(GAIAmissileArtilleryDecision);


	var MissileSystem_Gaia = AssetManager.actor_library.clone("MissileSystem_Gaia","baseWarUnit");
	MissileSystem_Gaia.die_in_lava = false;
        MissileSystem_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        MissileSystem_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        MissileSystem_Gaia.base_stats["scale"] = 0.3f;
        MissileSystem_Gaia.base_stats["size"] = 1f;
		MissileSystem_Gaia.base_stats["mass"] = 1000f;
        MissileSystem_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		MissileSystem_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		MissileSystem_Gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		MissileSystem_Gaia.base_stats["attack_speed"] = 0.1f;
		MissileSystem_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		MissileSystem_Gaia.base_stats["knockback"] = 4f;
		MissileSystem_Gaia.base_stats["accuracy"] = 0.1f;
		MissileSystem_Gaia.base_stats["targets"] = 3f;
		MissileSystem_Gaia.base_stats["area_of_effect"] = 4f;
		MissileSystem_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
		MissileSystem_Gaia.inspect_avatar_scale = 2f;
        MissileSystem_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        MissileSystem_Gaia.default_attack = "MissileSystemGaia";
        MissileSystem_Gaia.icon = "iconBoat";
        MissileSystem_Gaia.shadow_texture = "unitShadow_6";
MissileSystem_Gaia.job = AssetLibrary<ActorAsset>.a<string>("decision");
MissileSystem_Gaia.addDecision("check_swearing");
MissileSystem_Gaia.addDecision("warrior_random_move");
MissileSystem_Gaia.addDecision("GAIAmissileArtilleryDecision");
// MissileSystem_Gaia.addDecision("city_idle_walking");
MissileSystem_Gaia.addDecision("swim_to_island");
        MissileSystem_Gaia.texture_asset = new ActorTextureSubAsset("actors/MissileSystem_Gaia/", false);
        MissileSystem_Gaia.special = true;
        MissileSystem_Gaia.has_advanced_textures = false;
        MissileSystem_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        MissileSystem_Gaia.animation_idle = Vehicles.idle_0;
		MissileSystem_Gaia.animation_swim = ActorAnimationSequences.swim_0_3;
            MissileSystem_Gaia.name_locale = "Missile System";
			MissileSystem_Gaia.addTrait("fire_proof");
            AssetManager.actor_library.add(MissileSystem_Gaia);
			Localization.addLocalization(MissileSystem_Gaia.name_locale, MissileSystem_Gaia.name_locale);

	var supporttruck_Gaia = AssetManager.actor_library.clone("supporttruck_Gaia","baseWarUnit");
	supporttruck_Gaia.die_in_lava = false;
        supporttruck_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        supporttruck_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        supporttruck_Gaia.base_stats["scale"] = 0.3f;
        supporttruck_Gaia.base_stats["size"] = 1f;
		supporttruck_Gaia.base_stats["mass"] = 1000f;
        supporttruck_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		supporttruck_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		supporttruck_Gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		supporttruck_Gaia.base_stats["attack_speed"] = 0.1f;
		supporttruck_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		supporttruck_Gaia.base_stats["knockback"] = 4f;
		supporttruck_Gaia.base_stats["accuracy"] = 0.1f;
		supporttruck_Gaia.base_stats["targets"] = 3f;
		supporttruck_Gaia.base_stats["area_of_effect"] = 4f;
		supporttruck_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
        supporttruck_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        supporttruck_Gaia.default_attack = "base_attack";
        supporttruck_Gaia.icon = "iconBoat";
        supporttruck_Gaia.shadow_texture = "unitShadow_6";
		supporttruck_Gaia.inspect_avatar_scale = 1f;
        supporttruck_Gaia.texture_asset = new ActorTextureSubAsset("actors/supporttruck_Gaia/", false);
        supporttruck_Gaia.special = true;
        supporttruck_Gaia.has_advanced_textures = false;
        supporttruck_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        supporttruck_Gaia.animation_idle = ActorAnimationSequences.walk_0;
		supporttruck_Gaia.animation_swim = ActorAnimationSequences.swim_0_3;
            supporttruck_Gaia.name_locale = "Support Unit";
            supporttruck_Gaia.skip_fight_logic = true;
			supporttruck_Gaia.addTrait("fire_proof");
			   supporttruck_Gaia.job = AssetLibrary<ActorAsset>.a<string>("decision");
           supporttruck_Gaia.addDecision("check_swearing");
supporttruck_Gaia.addDecision("warrior_try_join_army_group");
supporttruck_Gaia.addDecision("city_walking_to_danger_zone");
supporttruck_Gaia.addDecision("check_cure");
supporttruck_Gaia.addDecision("warrior_army_leader_move_random");
supporttruck_Gaia.addDecision("check_heal");
supporttruck_Gaia.addDecision("warrior_army_follow_leader");
supporttruck_Gaia.addDecision("warrior_random_move");
supporttruck_Gaia.addDecision("check_warrior_transport");
supporttruck_Gaia.addDecision("swim_to_island");
            AssetManager.actor_library.add(supporttruck_Gaia);
			Localization.addLocalization(supporttruck_Gaia.name_locale, supporttruck_Gaia.name_locale);

		var Heli_Gaia = AssetManager.actor_library.clone("Heli_Gaia","baseWarUnit");
	Heli_Gaia.die_in_lava = false;
	Heli_Gaia.animation_speed_based_on_walk_speed = false;
        Heli_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        Heli_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Heli_Gaia.base_stats["scale"] = 0.3f;
        Heli_Gaia.base_stats["size"] = 1f;
		Heli_Gaia.base_stats["mass"] = 1000f;
        Heli_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		Heli_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Heli_Gaia.base_stats["armor"] = 0f;
		Heli_Gaia.base_stats["attack_speed"] = 10000f;
		Heli_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		Heli_Gaia.base_stats["knockback"] = 0.01f;
		Heli_Gaia.base_stats["accuracy"] = 0.7f;
		Heli_Gaia.base_stats["targets"] = 1f;
		Heli_Gaia.base_stats["area_of_effect"] = 0.5f;
		Heli_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        Heli_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        Heli_Gaia.default_attack = "gaiamachinegun";
        Heli_Gaia.icon = "iconBoat";
        Heli_Gaia.addDecision("burn_tumors");
        Heli_Gaia.shadow_texture = "unitShadow_6";
        Heli_Gaia.texture_asset = new ActorTextureSubAsset("actors/Heli_Gaia/", false);
        Heli_Gaia.special = true;
        Heli_Gaia.has_advanced_textures = false;
        Heli_Gaia.animation_walk = ActorAnimationSequences.walk_0_3;
        Heli_Gaia.animation_idle = ActorAnimationSequences.walk_0_3;
		Heli_Gaia.animation_swim = ActorAnimationSequences.walk_0_3;
            Heli_Gaia.name_locale = "Helicopter";
			Heli_Gaia.addTrait("fire_proof");
            Heli_Gaia.addTrait("freeze_proof");
			Heli_Gaia.flying = true;
			Heli_Gaia.very_high_flyer = true;
			Heli_Gaia.die_on_blocks = false;
			Heli_Gaia.inspect_avatar_scale = 0.5f;
			Heli_Gaia.ignore_blocks = true;
            AssetManager.actor_library.add(Heli_Gaia);
			Localization.addLocalization(Heli_Gaia.name_locale, Heli_Gaia.name_locale);

		var Bomber_Gaia = AssetManager.actor_library.clone("Bomber_Gaia","baseWarUnit");
	Bomber_Gaia.die_in_lava = false;
	Bomber_Gaia.animation_speed_based_on_walk_speed = false;
        Bomber_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        Bomber_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Bomber_Gaia.base_stats["scale"] = 0.3f;
        Bomber_Gaia.base_stats["size"] = 1f;
		Bomber_Gaia.base_stats["mass"] = 1000f;
        Bomber_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		Bomber_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		Bomber_Gaia.base_stats["armor"] = 0f;
		Bomber_Gaia.base_stats["attack_speed"] = 5f;
		Bomber_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		Bomber_Gaia.base_stats["knockback"] = 2f;
		Bomber_Gaia.base_stats["accuracy"] = 0.7f;
		Bomber_Gaia.base_stats["targets"] = 10f;
		Bomber_Gaia.base_stats["area_of_effect"] = 0.5f;
		Bomber_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
        Bomber_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        Bomber_Gaia.default_attack = "BomberAttackGaia";
        Bomber_Gaia.icon = "iconBoat";
        Bomber_Gaia.shadow_texture = "unitShadow_6";
        Bomber_Gaia.texture_asset = new ActorTextureSubAsset("actors/Bomber_Gaia/", false);
        Bomber_Gaia.special = true;
        Bomber_Gaia.can_flip = false;
        Bomber_Gaia.has_advanced_textures = false;
        Bomber_Gaia.animation_walk = Vehicles.idle_0_19;
        Bomber_Gaia.animation_idle = Vehicles.idle_0_19;
		Bomber_Gaia.animation_swim = Vehicles.idle_0_19;
            Bomber_Gaia.name_locale = "Bomber";
			Bomber_Gaia.addTrait("fire_proof");
            Bomber_Gaia.addTrait("freeze_proof");
			Bomber_Gaia.flying = true;
			Bomber_Gaia.very_high_flyer = true;
			Bomber_Gaia.die_on_blocks = false;
			Bomber_Gaia.ignore_blocks = true;
			Bomber_Gaia.inspect_avatar_scale = 0.5f;
            AssetManager.actor_library.add(Bomber_Gaia);
			Localization.addLocalization(Bomber_Gaia.name_locale, Bomber_Gaia.name_locale);

	var FighterJet_Gaia = AssetManager.actor_library.clone("FighterJet_Gaia","baseWarUnit");
	FighterJet_Gaia.die_in_lava = false;
	FighterJet_Gaia.animation_speed_based_on_walk_speed = false;
        FighterJet_Gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        FighterJet_Gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        FighterJet_Gaia.base_stats["scale"] = 0.3f;
        FighterJet_Gaia.base_stats["size"] = 1f;
		FighterJet_Gaia.base_stats["mass"] = 1000f;
        FighterJet_Gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		FighterJet_Gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		FighterJet_Gaia.base_stats["armor"] = 0f;
		FighterJet_Gaia.base_stats["attack_speed"] = 0.3f;
		FighterJet_Gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FighterJet_Gaia.base_stats["knockback"] = 2f;
		FighterJet_Gaia.base_stats["accuracy"] = 0.7f;
		FighterJet_Gaia.base_stats["targets"] = 1f;
		FighterJet_Gaia.base_stats["area_of_effect"] = 0.5f;
		FighterJet_Gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FighterJet_Gaia.inspect_avatar_scale = 0.5f;
        FighterJet_Gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        FighterJet_Gaia.default_attack = "fighterattackGaia";
        FighterJet_Gaia.icon = "iconBoat";
        FighterJet_Gaia.shadow_texture = "unitShadow_6";
        FighterJet_Gaia.texture_asset = new ActorTextureSubAsset("actors/FighterJet_Gaia/", false);
        FighterJet_Gaia.special = true;
        FighterJet_Gaia.can_flip = false;
        FighterJet_Gaia.has_advanced_textures = false;
        FighterJet_Gaia.animation_walk = Vehicles.idle_0_7;
        FighterJet_Gaia.animation_idle = Vehicles.idle_0_7;
		FighterJet_Gaia.animation_swim = Vehicles.idle_0_7;
            FighterJet_Gaia.name_locale = "Fighter Jet";
			FighterJet_Gaia.addTrait("fire_proof");
            FighterJet_Gaia.addTrait("freeze_proof");
			FighterJet_Gaia.flying = true;
			FighterJet_Gaia.very_high_flyer = true;
			FighterJet_Gaia.die_on_blocks = false;
			FighterJet_Gaia.ignore_blocks = true;
            AssetManager.actor_library.add(FighterJet_Gaia);
			Localization.addLocalization(FighterJet_Gaia.name_locale, FighterJet_Gaia.name_locale);


            ////////////////////////////Special Races/////////////////////////////////
            	var demonscorpion = AssetManager.actor_library.clone("demonscorpion","baseWarUnit");
	demonscorpion.die_in_lava = false;
        demonscorpion.base_stats["mass_2"] = 600f;
        demonscorpion.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        demonscorpion.base_stats["scale"] = 0.3f;
        demonscorpion.base_stats["size"] = 1f;
		demonscorpion.base_stats["mass"] = 1000f;
        demonscorpion.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		demonscorpion.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		demonscorpion.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		demonscorpion.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		demonscorpion.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		demonscorpion.base_stats["knockback"] = 0.01f;
		demonscorpion.base_stats["accuracy"] = 0.5f;
		demonscorpion.base_stats["targets"] = 1f;
		demonscorpion.base_stats["area_of_effect"] = 0.5f;
		demonscorpion.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        demonscorpion.sound_hit = "event:/SFX/HIT/HitFlesh";
        demonscorpion.default_attack = "fire_hands";
        demonscorpion.icon = "iconBoat";
        demonscorpion.shadow_texture = "unitShadow_6";
        demonscorpion.texture_asset = new ActorTextureSubAsset("actors/demonscorpion/", false);
        demonscorpion.special = true;
        demonscorpion.has_advanced_textures = false;
        demonscorpion.animation_walk = ActorAnimationSequences.walk_0_2;
        demonscorpion.animation_idle = Vehicles.idle_0_2;
		demonscorpion.animation_swim = ActorAnimationSequences.swim_0_2;
            demonscorpion.name_locale = "Demon Scorpion";
			demonscorpion.addTrait("poisonous");
			demonscorpion.addTrait("dash");
			demonscorpion.addTrait("fire_proof");
            demonscorpion.addTrait("burning_feet");
            demonscorpion.addTrait("evil");
            AssetManager.actor_library.add(demonscorpion);
			Localization.addLocalization(demonscorpion.name_locale, demonscorpion.name_locale);

	var demoncroc = AssetManager.actor_library.clone("demoncroc","baseWarUnit");
	demoncroc.die_in_lava = false;
        demoncroc.base_stats["mass_2"] = 600f;
        demoncroc.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        demoncroc.base_stats["scale"] = 0.3f;
        demoncroc.base_stats["size"] = 1f;
		demoncroc.base_stats["mass"] = 1000f;
        demoncroc.base_stats["health"] = ModernBoxPrefs.ScaleBalance(300f);
		demoncroc.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		demoncroc.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		demoncroc.base_stats["attack_speed"] = 0.1f;
		demoncroc.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		demoncroc.base_stats["knockback"] = 4f;
		demoncroc.base_stats["accuracy"] = 0.8f;
		demoncroc.base_stats["targets"] = 2f;
		demoncroc.base_stats["area_of_effect"] = 2f;
		demoncroc.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        demoncroc.sound_hit = "event:/SFX/HIT/HitFlesh";
        demoncroc.default_attack = "hordetankpew";
        demoncroc.icon = "iconBoat";
        demoncroc.shadow_texture = "unitShadow_6";
        demoncroc.texture_asset = new ActorTextureSubAsset("actors/demoncroc/", false);
        demoncroc.special = true;
		demoncroc.inspect_avatar_scale = 2f;
        demoncroc.has_advanced_textures = false;
        demoncroc.animation_walk = ActorAnimationSequences.walk_0_3;
        demoncroc.animation_idle = ActorAnimationSequences.walk_0;
		demoncroc.animation_swim = ActorAnimationSequences.swim_0_3;
            demoncroc.name_locale = "Demon Crocodile";
			demoncroc.addTrait("fire_proof");
			demoncroc.addTrait("block");
			demoncroc.addTrait("deflect_projectile");
            demoncroc.addTrait("dash");
            demoncroc.addTrait("burning_feet");
            demoncroc.addTrait("evil");
            AssetManager.actor_library.add(demoncroc);
			Localization.addLocalization(demoncroc.name_locale, demoncroc.name_locale);

	var demongolem = AssetManager.actor_library.clone("demongolem","baseWarUnit");
	demongolem.die_in_lava = false;
        demongolem.base_stats["mass_2"] = 600f;
        demongolem.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        demongolem.base_stats["scale"] = 0.3f;
        demongolem.base_stats["size"] = 1f;
		demongolem.base_stats["mass"] = 1000f;
        demongolem.base_stats["health"] = ModernBoxPrefs.ScaleBalance(666f);
		demongolem.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		demongolem.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(50f);
		demongolem.base_stats["attack_speed"] = 0.1f;
		demongolem.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(230f);
		demongolem.base_stats["knockback"] = 4f;
		demongolem.base_stats["accuracy"] = 0.1f;
		demongolem.base_stats["targets"] = 2f;
		demongolem.base_stats["area_of_effect"] = 4f;
		demongolem.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
		demongolem.inspect_avatar_scale = 2f;
        demongolem.sound_hit = "event:/SFX/HIT/HitFlesh";
        demongolem.default_attack = "fire_hands";
        demongolem.icon = "iconBoat";
        demongolem.shadow_texture = "unitShadow_6";
        demongolem.texture_asset = new ActorTextureSubAsset("actors/demongolem/", false);
        demongolem.special = true;
        demongolem.has_advanced_textures = false;
        demongolem.animation_walk = ActorAnimationSequences.walk_0_3;
        demongolem.animation_idle = ActorAnimationSequences.walk_0;
		demongolem.animation_swim = ActorAnimationSequences.swim_0_3;
            demongolem.name_locale = "Demon Golem";
			demongolem.addTrait("fire_proof");
            demongolem.addTrait("block");
			demongolem.addTrait("deflect_projectile");
            demongolem.addTrait("dash");
            demongolem.addTrait("burning_feet");
            demongolem.addTrait("evil");
            AssetManager.actor_library.add(demongolem);
			Localization.addLocalization(demongolem.name_locale, demongolem.name_locale);

		var demonwyvern = AssetManager.actor_library.clone("demonwyvern","baseWarUnit");
	demonwyvern.die_in_lava = false;
	demonwyvern.animation_speed_based_on_walk_speed = false;
        demonwyvern.base_stats["mass_2"] = 600f;
        demonwyvern.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        demonwyvern.base_stats["scale"] = 0.3f;
        demonwyvern.base_stats["size"] = 1f;
		demonwyvern.base_stats["mass"] = 1000f;
        demonwyvern.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100f);
		demonwyvern.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		demonwyvern.base_stats["armor"] = 0f;
		demonwyvern.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		demonwyvern.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(30f);
		demonwyvern.base_stats["knockback"] = 2f;
		demonwyvern.base_stats["accuracy"] = 0.7f;
		demonwyvern.base_stats["targets"] = 1f;
		demonwyvern.base_stats["area_of_effect"] = 0.5f;
		demonwyvern.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        demonwyvern.sound_hit = "event:/SFX/HIT/HitFlesh";
        demonwyvern.default_attack = "hordetankpew";
        demonwyvern.icon = "iconBoat";
        demonwyvern.shadow_texture = "unitShadow_6";
        demonwyvern.texture_asset = new ActorTextureSubAsset("actors/demonwyvern/", false);
        demonwyvern.special = true;
        demonwyvern.has_advanced_textures = false;
        demonwyvern.animation_walk = Vehicles.walk_0_5;
        demonwyvern.animation_idle = Vehicles.idle_0_5;
		demonwyvern.animation_swim = Vehicles.walk_0_5;
            demonwyvern.name_locale = "Wyvern";
			demonwyvern.addTrait("fire_proof");
            demonwyvern.addTrait("freeze_proof");
            demonwyvern.addTrait("block");
			demonwyvern.addTrait("deflect_projectile");
            demonwyvern.addTrait("dash");
            demonwyvern.addTrait("burning_feet");
            demonwyvern.addTrait("evil");
			demonwyvern.flying = true;
			demonwyvern.very_high_flyer = true;
			demonwyvern.die_on_blocks = false;
			demonwyvern.inspect_avatar_scale = 0.5f;
			demonwyvern.ignore_blocks = true;
            AssetManager.actor_library.add(demonwyvern);
			Localization.addLocalization(demonwyvern.name_locale, demonwyvern.name_locale);

		var Bomber_Demon = AssetManager.actor_library.clone("Bomber_Demon","baseWarUnit");
	Bomber_Demon.die_in_lava = false;
	Bomber_Demon.animation_speed_based_on_walk_speed = false;
        Bomber_Demon.base_stats["mass_2"] = 600f;
        Bomber_Demon.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        Bomber_Demon.base_stats["scale"] = 0.3f;
        Bomber_Demon.base_stats["size"] = 1f;
		Bomber_Demon.base_stats["mass"] = 1000f;
        Bomber_Demon.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		Bomber_Demon.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		Bomber_Demon.base_stats["armor"] = 0f;
		Bomber_Demon.base_stats["attack_speed"] = 0.3f;
		Bomber_Demon.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		Bomber_Demon.base_stats["knockback"] = 4f;
		Bomber_Demon.base_stats["accuracy"] = 0.7f;
		Bomber_Demon.base_stats["targets"] = ModernBoxPrefs.ScaleBalance(10f);
		Bomber_Demon.base_stats["area_of_effect"] = 5f;
		Bomber_Demon.base_stats["range"] = ModernBoxPrefs.ScaleBalance(10f);
        Bomber_Demon.sound_hit = "event:/SFX/HIT/HitFlesh";
        Bomber_Demon.default_attack = "hordetankpew";
        Bomber_Demon.icon = "iconBoat";
        Bomber_Demon.shadow_texture = "unitShadow_6";
        Bomber_Demon.texture_asset = new ActorTextureSubAsset("actors/Bomber_Demon/", false);
        Bomber_Demon.special = true;
        Bomber_Demon.can_flip = false;
        Bomber_Demon.has_advanced_textures = false;
        Bomber_Demon.animation_walk = Vehicles.idle_0_13;
        Bomber_Demon.animation_idle = Vehicles.idle_0_13;
		Bomber_Demon.animation_swim = Vehicles.idle_0_13;
            Bomber_Demon.name_locale = "Dragon";
			Bomber_Demon.addTrait("fire_proof");
            Bomber_Demon.addTrait("freeze_proof");
            Bomber_Demon.addTrait("block");
			Bomber_Demon.addTrait("deflect_projectile");
            Bomber_Demon.addTrait("dash");
            Bomber_Demon.addTrait("burning_feet");
            Bomber_Demon.addTrait("evil");
			Bomber_Demon.flying = true;
			Bomber_Demon.very_high_flyer = true;
			Bomber_Demon.die_on_blocks = false;
			Bomber_Demon.ignore_blocks = true;
			Bomber_Demon.inspect_avatar_scale = 0.5f;
            AssetManager.actor_library.add(Bomber_Demon);
			Localization.addLocalization(Bomber_Demon.name_locale, Bomber_Demon.name_locale);

var demonreaver = AssetManager.actor_library.clone("demonreaver","baseWarUnit");
	demonreaver.die_in_lava = false;
        demonreaver.base_stats["mass_2"] = 600f;
        demonreaver.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        demonreaver.base_stats["scale"] = 0.3f;
        demonreaver.base_stats["size"] = 1f;
		demonreaver.base_stats["mass"] = 1000f;
        demonreaver.base_stats["health"] = ModernBoxPrefs.ScaleBalance(666f);
		demonreaver.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		demonreaver.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(20f);
		demonreaver.base_stats["attack_speed"] = 0.1f;
		demonreaver.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		demonreaver.base_stats["knockback"] = 3f;
		demonreaver.base_stats["accuracy"] = 0.3f;
		demonreaver.base_stats["targets"] = 3f;
		demonreaver.base_stats["area_of_effect"] = 4f;
		demonreaver.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        demonreaver.sound_hit = "event:/SFX/HIT/HitFlesh";
        demonreaver.default_attack = "fire_hands";
        demonreaver.icon = "iconBoat";
		demonreaver.inspect_avatar_scale = 2f;
        demonreaver.shadow_texture = "unitShadow_6";
        demonreaver.texture_asset = new ActorTextureSubAsset("actors/demonreaver/", false);
        demonreaver.special = true;
        demonreaver.has_advanced_textures = false;
        demonreaver.animation_walk = ActorAnimationSequences.walk_0_3;
        demonreaver.animation_idle = ActorAnimationSequences.walk_0;
		demonreaver.animation_swim = ActorAnimationSequences.swim_0_3;
            demonreaver.name_locale = "Demon Reaver";
			demonreaver.addTrait("fire_proof");
            demonreaver.addTrait("block");
			demonreaver.addTrait("deflect_projectile");
            demonreaver.addTrait("dash");
            demonreaver.addTrait("burning_feet");
            demonreaver.addTrait("evil");
            AssetManager.actor_library.add(demonreaver);
			Localization.addLocalization(demonreaver.name_locale, demonreaver.name_locale);


	var xenorailgun = AssetManager.actor_library.clone("xenorailgun","baseWarUnit");
	xenorailgun.die_in_lava = false;
        xenorailgun.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        xenorailgun.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        xenorailgun.base_stats["scale"] = 0.3f;
        xenorailgun.base_stats["size"] = 1f;
		xenorailgun.base_stats["mass"] = 1000f;
        xenorailgun.base_stats["health"] = ModernBoxPrefs.ScaleBalance(1000f);
		xenorailgun.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		xenorailgun.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(40f);
		xenorailgun.base_stats["attack_speed"] = 0.1f;
		xenorailgun.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		xenorailgun.base_stats["knockback"] = 4f;
		xenorailgun.base_stats["accuracy"] = 0.8f;
		xenorailgun.base_stats["targets"] = 2f;
		xenorailgun.base_stats["area_of_effect"] = 2f;
		xenorailgun.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
        xenorailgun.sound_hit = "event:/SFX/HIT/HitMetal";
        xenorailgun.default_attack = "XenoPew";
        xenorailgun.icon = "iconBoat";
        xenorailgun.shadow_texture = "unitShadow_6";
        xenorailgun.texture_asset = new ActorTextureSubAsset("actors/xenorailgun/", false);
        xenorailgun.special = true;
		xenorailgun.inspect_avatar_scale = 2f;
        xenorailgun.has_advanced_textures = false;
        xenorailgun.animation_walk = ActorAnimationSequences.walk_0_2;
        xenorailgun.animation_idle = ActorAnimationSequences.walk_0;
		xenorailgun.animation_swim = ActorAnimationSequences.swim_0_2;
            xenorailgun.name_locale = "Tank";
			xenorailgun.addTrait("fire_proof");
			xenorailgun.addTrait("block");
			xenorailgun.addTrait("deflect_projectile");
            xenorailgun.actor_size = ActorSize.S17_Dragon;
            xenorailgun.addTrait("fat");
            xenorailgun.addTrait("acid_blood");
            xenorailgun.addTrait("acid_proof");
            AssetManager.actor_library.add(xenorailgun);
			Localization.addLocalization(xenorailgun.name_locale, xenorailgun.name_locale);

	var xenolevitank = AssetManager.actor_library.clone("xenolevitank","baseWarUnit");
	xenolevitank.die_in_lava = false;
        xenolevitank.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        xenolevitank.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        xenolevitank.base_stats["scale"] = 0.3f;
        xenolevitank.base_stats["size"] = 1f;
		xenolevitank.base_stats["mass"] = 1000f;
        xenolevitank.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		xenolevitank.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(70f);
		xenolevitank.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		xenolevitank.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(10f);
		xenolevitank.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		xenolevitank.base_stats["knockback"] = 0.01f;
		xenolevitank.base_stats["accuracy"] = 0.5f;
		xenolevitank.base_stats["targets"] = 1f;
		xenolevitank.base_stats["area_of_effect"] = 0.5f;
		xenolevitank.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        xenolevitank.sound_hit = "event:/SFX/HIT/HitMetal";
        xenolevitank.default_attack = "XenoPew";
        xenolevitank.icon = "iconBoat";
		xenolevitank.inspect_avatar_scale = 2f;
        xenolevitank.shadow_texture = "unitShadow_6";
        xenolevitank.texture_asset = new ActorTextureSubAsset("actors/xenolevitank/", false);
        xenolevitank.special = true;
        xenolevitank.has_advanced_textures = false;
        xenolevitank.animation_walk = ActorAnimationSequences.idle_0_3;
        xenolevitank.animation_idle = ActorAnimationSequences.idle_0_3;
		xenolevitank.animation_swim = ActorAnimationSequences.idle_0_3;
            xenolevitank.name_locale = "Armored Car";
			xenolevitank.addTrait("dodge");
			xenolevitank.addTrait("dash");
			xenolevitank.addTrait("fire_proof");
            xenolevitank.actor_size = ActorSize.S17_Dragon;
            xenolevitank.addTrait("fat");
            xenolevitank.addTrait("acid_blood");
            xenolevitank.addTrait("acid_proof");
            AssetManager.actor_library.add(xenolevitank);
			Localization.addLocalization(xenolevitank.name_locale, xenolevitank.name_locale);

				var xenotripod = AssetManager.actor_library.clone("xenotripod","baseWarUnit");
	xenotripod.die_in_lava = false;
        xenotripod.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        xenotripod.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
        xenotripod.base_stats["scale"] = 0.3f;
        xenotripod.base_stats["size"] = 1f;
		xenotripod.base_stats["mass"] = 1000f;
        xenotripod.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		xenotripod.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		xenotripod.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		xenotripod.base_stats["attack_speed"] = 0.1f;
		xenotripod.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		xenotripod.base_stats["knockback"] = 4f;
		xenotripod.base_stats["accuracy"] = 0.1f;
		xenotripod.base_stats["targets"] = 3f;
		xenotripod.base_stats["area_of_effect"] = 4f;
		xenotripod.base_stats["range"] = ModernBoxPrefs.ScaleBalance(8f);
		xenotripod.inspect_avatar_scale = 2f;
        xenotripod.sound_hit = "event:/SFX/HIT/HitMetal";
        xenotripod.default_attack = "XenoBeam";
        xenotripod.icon = "iconBoat";
        xenotripod.shadow_texture = "unitShadow_6";
        xenotripod.texture_asset = new ActorTextureSubAsset("actors/xenotripod/", false);
        xenotripod.special = true;
        xenotripod.has_advanced_textures = false;
        xenotripod.animation_walk = Vehicles.walk_0_5;
        xenotripod.animation_idle = Vehicles.idle_0;
		xenotripod.animation_swim = Vehicles.swim_0_5;
            xenotripod.name_locale = "Missile System";
			xenotripod.addTrait("fire_proof");
            xenotripod.actor_size = ActorSize.S17_Dragon;
            xenotripod.addTrait("fat");
            xenotripod.addTrait("acid_blood");
            xenotripod.addTrait("acid_proof");
            xenotripod.addTrait("bubble_defense");
            AssetManager.actor_library.add(xenotripod);
			Localization.addLocalization(xenotripod.name_locale, xenotripod.name_locale);

			var xenoUFO = AssetManager.actor_library.clone("xenoUFO","baseWarUnit");
	xenoUFO.die_in_lava = false;
	xenoUFO.animation_speed_based_on_walk_speed = false;
        xenoUFO.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        xenoUFO.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        xenoUFO.base_stats["scale"] = 0.3f;
        xenoUFO.base_stats["size"] = 1f;
		xenoUFO.base_stats["mass"] = 1000f;
        xenoUFO.base_stats["health"] = ModernBoxPrefs.ScaleBalance(200f);
		xenoUFO.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		xenoUFO.base_stats["armor"] = 0f;
		xenoUFO.base_stats["attack_speed"] = ModernBoxPrefs.ScaleBalance(100f);
		xenoUFO.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		xenoUFO.base_stats["knockback"] = 0.01f;
		xenoUFO.base_stats["accuracy"] = 0.7f;
		xenoUFO.base_stats["targets"] = 1f;
		xenoUFO.base_stats["area_of_effect"] = 0.5f;
		xenoUFO.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
        xenoUFO.sound_hit = "event:/SFX/HIT/HitMetal";
        xenoUFO.default_attack = "XenoPew";
        xenoUFO.icon = "iconBoat";
        xenoUFO.shadow_texture = "unitShadow_6";
        xenoUFO.texture_asset = new ActorTextureSubAsset("actors/xenoUFO/", false);
        xenoUFO.special = true;
        xenoUFO.has_advanced_textures = false;
        xenoUFO.animation_walk = ActorAnimationSequences.idle_0_3;
        xenoUFO.animation_idle = ActorAnimationSequences.idle_0_3;
		xenoUFO.animation_swim = ActorAnimationSequences.idle_0_3;
            xenoUFO.name_locale = "Helicopter";
			xenoUFO.addTrait("fire_proof");
            xenoUFO.addTrait("freeze_proof");
            xenoUFO.actor_size = ActorSize.S17_Dragon;
            xenoUFO.addTrait("fat");
            xenoUFO.addTrait("acid_blood");
            xenoUFO.addTrait("acid_proof");
			xenoUFO.flying = true;
			xenoUFO.very_high_flyer = true;
			xenoUFO.die_on_blocks = false;
			xenoUFO.inspect_avatar_scale = 0.5f;
			xenoUFO.ignore_blocks = true;
            AssetManager.actor_library.add(xenoUFO);
			Localization.addLocalization(xenoUFO.name_locale, xenoUFO.name_locale);

		var xenoUFObomber = AssetManager.actor_library.clone("xenoUFObomber","baseWarUnit");
	xenoUFObomber.die_in_lava = false;
	xenoUFObomber.animation_speed_based_on_walk_speed = false;
        xenoUFObomber.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(200f);
        xenoUFObomber.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
        xenoUFObomber.base_stats["scale"] = 0.3f;
        xenoUFObomber.base_stats["size"] = 1f;
		xenoUFObomber.base_stats["mass"] = 1000f;
        xenoUFObomber.base_stats["health"] = ModernBoxPrefs.ScaleBalance(400f);
		xenoUFObomber.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		xenoUFObomber.base_stats["armor"] = 0f;
		xenoUFObomber.base_stats["attack_speed"] = 0.3f;
		xenoUFObomber.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		xenoUFObomber.base_stats["knockback"] = 2f;
		xenoUFObomber.base_stats["accuracy"] = 0.7f;
		xenoUFObomber.base_stats["targets"] = 5f;
		xenoUFObomber.base_stats["area_of_effect"] = 0.5f;
		xenoUFObomber.base_stats["range"] = ModernBoxPrefs.ScaleBalance(1f);
        xenoUFObomber.sound_hit = "event:/SFX/HIT/HitMetal";
        xenoUFObomber.default_attack = "XenoMegaBomb";
        xenoUFObomber.icon = "iconBoat";
        xenoUFObomber.shadow_texture = "unitShadow_6";
        xenoUFObomber.texture_asset = new ActorTextureSubAsset("actors/xenoUFObomber/", false);
        xenoUFObomber.special = true;
        xenoUFObomber.can_flip = false;
        xenoUFObomber.has_advanced_textures = false;
        xenoUFObomber.animation_walk = Vehicles.idle_0_8;
        xenoUFObomber.animation_idle = Vehicles.idle_0_8;
		xenoUFObomber.animation_swim = Vehicles.idle_0_8;
            xenoUFObomber.name_locale = "Bomber";
			xenoUFObomber.addTrait("fire_proof");
            xenoUFObomber.addTrait("freeze_proof");
            xenoUFObomber.actor_size = ActorSize.S17_Dragon;
            xenoUFObomber.addTrait("fat");
            xenoUFObomber.addTrait("acid_blood");
            xenoUFObomber.addTrait("acid_proof");
			xenoUFObomber.flying = true;
			xenoUFObomber.very_high_flyer = true;
			xenoUFObomber.die_on_blocks = false;
			xenoUFObomber.ignore_blocks = true;
			xenoUFObomber.inspect_avatar_scale = 0.5f;
            AssetManager.actor_library.add(xenoUFObomber);
			Localization.addLocalization(xenoUFObomber.name_locale, xenoUFObomber.name_locale);


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////NAVAL UNITS FOR DOCKS :DDDDDDDDDDDD//////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




DecisionAsset warBoatAttackDecision = new DecisionAsset();
warBoatAttackDecision.id = "warBoatAttackDecision";
warBoatAttackDecision.priority = NeuroLayer.Layer_1_Low;
warBoatAttackDecision.path_icon = "ui/icons/WarBoat";
warBoatAttackDecision.cooldown = 1;
warBoatAttackDecision.unique = true;
warBoatAttackDecision.weight = 1f;
AssetManager.decisions_library.add(warBoatAttackDecision);


BehaviourTaskActor warBoatAttackTask = new BehaviourTaskActor();
warBoatAttackTask.id = "warBoatAttackDecision";
warBoatAttackTask.setIcon("ui/icons/WarBoat");
warBoatAttackTask.addBeh(new BehWarBoatFindTarget());
warBoatAttackTask.addBeh(new BehGoToTileTarget());
warBoatAttackTask.addBeh(new BehWarBoatAttack());
warBoatAttackTask.addBeh(new BehEndJob());
AssetManager.tasks_actor.add(warBoatAttackTask);




			///////////////Alliance///////////////

	var CargoShip_alliance = AssetManager.actor_library.clone("CargoShip_alliance","$boat$");
	    CargoShip_alliance.id = "CargoShip_alliance";
		CargoShip_alliance.boat_type = "cargo_alliance_boat";
		CargoShip_alliance.can_be_inspected = false;
        CargoShip_alliance.skip_fight_logic = true;
		CargoShip_alliance.name_locale = "Cargo Ship";
		CargoShip_alliance.addDecision("boat_trading");
		CargoShip_alliance.has_avatar_prefab = false;
		CargoShip_alliance.animation_speed_based_on_walk_speed = false;
		CargoShip_alliance.can_flip = true;
        CargoShip_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CargoShip_alliance.is_boat = true;
		CargoShip_alliance.die_in_lava = false;
		CargoShip_alliance.has_override_sprite = false;
	    CargoShip_alliance.has_override_avatar_frames = false;
		CargoShip_alliance.base_stats["mass_2"] = 3000f;
		CargoShip_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CargoShip_alliance.base_stats["scale"] = 0.35f;
		CargoShip_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CargoShip_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CargoShip_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CargoShip_alliance.base_stats["attack_speed"] = 0.3f;
		CargoShip_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		CargoShip_alliance.base_stats["knockback"] = 2f;
		CargoShip_alliance.base_stats["accuracy"] = 0.7f;
		CargoShip_alliance.base_stats["targets"] = 1f;
		CargoShip_alliance.base_stats["area_of_effect"] = 0.5f;
		CargoShip_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		CargoShip_alliance.inspect_avatar_scale = 1f;
		CargoShip_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
		CargoShip_alliance.sound_spawn = null;
		CargoShip_alliance.sound_idle_loop = null;
		CargoShip_alliance.sound_death = null;
		CargoShip_alliance.default_attack = "boat_cannonball";
		CargoShip_alliance.icon = "iconBoat";
		CargoShip_alliance.shadow_texture = "unitShadow_6";
		CargoShip_alliance.cost = new ConstructionCost(1, 0, 0, 1);
		CargoShip_alliance.texture_asset = new ActorTextureSubAsset("actors/CargoShip_alliance/", false);
		CargoShip_alliance.special = true;
		CargoShip_alliance.has_advanced_textures = false;
		CargoShip_alliance.draw_boat_mark = true;
		CargoShip_alliance.actor_size = ActorSize.S16_Buffalo;
		CargoShip_alliance.animation_walk = ActorAnimationSequences.walk_0;
		CargoShip_alliance.animation_idle = ActorAnimationSequences.walk_0;
		CargoShip_alliance.animation_swim = ActorAnimationSequences.swim_0_2;
		CargoShip_alliance.addTrait("boat");
		CargoShip_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(CargoShip_alliance);
		Localization.addLocalization(CargoShip_alliance.name_locale, CargoShip_alliance.name_locale);

var Transporter_alliance = AssetManager.actor_library.clone("Transporter_alliance","$boat$");
	    Transporter_alliance.id = "Transporter_alliance";
		Transporter_alliance.boat_type = "transporter_alliance_boat";
		Transporter_alliance.can_be_inspected = false;
        Transporter_alliance.skip_fight_logic = true;
		Transporter_alliance.name_locale = "Cargo Ship";
		Transporter_alliance.addDecision("boat_transport_check");
		Transporter_alliance.has_avatar_prefab = false;
		Transporter_alliance.animation_speed_based_on_walk_speed = false;
		Transporter_alliance.can_flip = true;
        Transporter_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Transporter_alliance.is_boat = true;
		Transporter_alliance.die_in_lava = false;
		Transporter_alliance.has_override_sprite = false;
	    Transporter_alliance.has_override_avatar_frames = false;
		Transporter_alliance.base_stats["mass_2"] = 3000f;
		Transporter_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Transporter_alliance.base_stats["scale"] = 0.35f;
		Transporter_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		Transporter_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		Transporter_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Transporter_alliance.base_stats["attack_speed"] = 0.3f;
		Transporter_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		Transporter_alliance.base_stats["knockback"] = 2f;
		Transporter_alliance.base_stats["accuracy"] = 0.7f;
		Transporter_alliance.base_stats["targets"] = 1f;
		Transporter_alliance.base_stats["area_of_effect"] = 0.5f;
		Transporter_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		Transporter_alliance.inspect_avatar_scale = 1f;
		Transporter_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
		Transporter_alliance.sound_spawn = null;
		Transporter_alliance.sound_idle_loop = null;
		Transporter_alliance.sound_death = null;
		Transporter_alliance.default_attack = "boat_cannonball";
		Transporter_alliance.icon = "iconBoat";
		Transporter_alliance.shadow_texture = "unitShadow_6";
		Transporter_alliance.cost = new ConstructionCost(0, 0, 0, 0);
		Transporter_alliance.texture_asset = new ActorTextureSubAsset("actors/Transporter_alliance/", false);
		Transporter_alliance.special = true;
		Transporter_alliance.has_advanced_textures = false;
		Transporter_alliance.draw_boat_mark = true;
		Transporter_alliance.actor_size = ActorSize.S16_Buffalo;
		Transporter_alliance.animation_walk = ActorAnimationSequences.walk_0;
		Transporter_alliance.animation_idle = ActorAnimationSequences.walk_0;
		Transporter_alliance.animation_swim = ActorAnimationSequences.swim_0_2;
		Transporter_alliance.addTrait("boat");
		Transporter_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(Transporter_alliance);
		Localization.addLocalization(Transporter_alliance.name_locale, Transporter_alliance.name_locale);

	var aDestroyer_alliance = AssetManager.actor_library.clone("aDestroyer_alliance","$boat$");
	    aDestroyer_alliance.id = "aDestroyer_alliance";
	    aDestroyer_alliance.can_be_inspected = true;
		aDestroyer_alliance.boat_type = "destroyer_a_alliance_boat";
		aDestroyer_alliance.name_locale = "Destroyer Ship";
		aDestroyer_alliance.addDecision("warBoatAttackDecision");
		aDestroyer_alliance.has_avatar_prefab = false;
aDestroyer_alliance.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyer_avatar") };
aDestroyer_alliance.has_override_avatar_frames = true;
aDestroyer_alliance.inspect_avatar_scale = 4f;
aDestroyer_alliance.inspect_avatar_offset_y = 6f;
		aDestroyer_alliance.animation_speed_based_on_walk_speed = false;
		aDestroyer_alliance.can_flip = true;
        aDestroyer_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    aDestroyer_alliance.is_boat = true;
		aDestroyer_alliance.die_in_lava = false;
		aDestroyer_alliance.has_override_sprite = false;
		aDestroyer_alliance.base_stats["mass_2"] = 3000f;
		aDestroyer_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		aDestroyer_alliance.base_stats["scale"] = 0.35f;
		aDestroyer_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		aDestroyer_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		aDestroyer_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		aDestroyer_alliance.base_stats["attack_speed"] = 0.3f;
		aDestroyer_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		aDestroyer_alliance.base_stats["knockback"] = 2f;
		aDestroyer_alliance.base_stats["accuracy"] = 0.7f;
		aDestroyer_alliance.base_stats["targets"] = 1f;
		aDestroyer_alliance.base_stats["area_of_effect"] = 0.5f;
		aDestroyer_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		aDestroyer_alliance.inspect_avatar_scale = 1f;
		aDestroyer_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        aDestroyer_alliance.sound_spawn = null;
		aDestroyer_alliance.sound_idle_loop = null;
		aDestroyer_alliance.sound_death = null;
		aDestroyer_alliance.default_attack = "fighterattack";
		aDestroyer_alliance.icon = "iconBoat";
		aDestroyer_alliance.shadow_texture = "unitShadow_6";
		aDestroyer_alliance.cost = new ConstructionCost(1, 0, 0, 1);
		aDestroyer_alliance.texture_asset = new ActorTextureSubAsset("actors/Destroyer_alliance/", false);
		aDestroyer_alliance.special = true;
		aDestroyer_alliance.has_advanced_textures = false;
		aDestroyer_alliance.draw_boat_mark = true;
		aDestroyer_alliance.actor_size = ActorSize.S16_Buffalo;
		aDestroyer_alliance.animation_walk = ActorAnimationSequences.walk_0;
		aDestroyer_alliance.animation_idle = ActorAnimationSequences.walk_0;
		aDestroyer_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		aDestroyer_alliance.addTrait("boat");
		aDestroyer_alliance.addTrait("light_lamp");
		aDestroyer_alliance.addTrait("NavalUnit");
		AssetManager.actor_library.add(aDestroyer_alliance);
		Localization.addLocalization(aDestroyer_alliance.name_locale, aDestroyer_alliance.name_locale);

	var bDestroyer_alliance = AssetManager.actor_library.clone("bDestroyer_alliance","$boat$");
	    bDestroyer_alliance.id = "bDestroyer_alliance";
		bDestroyer_alliance.boat_type = "destroyer_b_alliance_boat";
		bDestroyer_alliance.can_be_inspected = true;
		bDestroyer_alliance.name_locale = "Destroyer Ship";
		bDestroyer_alliance.addDecision("warBoatAttackDecision");
		bDestroyer_alliance.has_avatar_prefab = false;
bDestroyer_alliance.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyer_avatar") };
bDestroyer_alliance.has_override_avatar_frames = true;
bDestroyer_alliance.inspect_avatar_scale = 4f;
bDestroyer_alliance.inspect_avatar_offset_y = 6f;
		bDestroyer_alliance.animation_speed_based_on_walk_speed = false;
		bDestroyer_alliance.can_flip = true;
        bDestroyer_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bDestroyer_alliance.is_boat = true;
		bDestroyer_alliance.die_in_lava = false;
		bDestroyer_alliance.has_override_sprite = false;
		bDestroyer_alliance.base_stats["mass_2"] = 3000f;
		bDestroyer_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bDestroyer_alliance.base_stats["scale"] = 0.35f;
		bDestroyer_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		bDestroyer_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		bDestroyer_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		bDestroyer_alliance.base_stats["attack_speed"] = 0.3f;
		bDestroyer_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		bDestroyer_alliance.base_stats["knockback"] = 2f;
		bDestroyer_alliance.base_stats["accuracy"] = 0.7f;
		bDestroyer_alliance.base_stats["targets"] = 1f;
		bDestroyer_alliance.base_stats["area_of_effect"] = 0.5f;
		bDestroyer_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		bDestroyer_alliance.inspect_avatar_scale = 1f;
		bDestroyer_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        bDestroyer_alliance.sound_spawn = null;
		bDestroyer_alliance.sound_idle_loop = null;
		bDestroyer_alliance.sound_death = null;
		bDestroyer_alliance.default_attack = "fighterattack";
		bDestroyer_alliance.icon = "iconBoat";
		bDestroyer_alliance.shadow_texture = "unitShadow_6";
		bDestroyer_alliance.cost = new ConstructionCost(1, 0, 0, 1);
		bDestroyer_alliance.texture_asset = new ActorTextureSubAsset("actors/Destroyer_alliance/", false);
		bDestroyer_alliance.special = true;
		bDestroyer_alliance.has_advanced_textures = false;
		bDestroyer_alliance.draw_boat_mark = true;
		bDestroyer_alliance.actor_size = ActorSize.S16_Buffalo;
		bDestroyer_alliance.animation_walk = ActorAnimationSequences.walk_0;
		bDestroyer_alliance.animation_idle = ActorAnimationSequences.walk_0;
		bDestroyer_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		bDestroyer_alliance.addTrait("boat");
		bDestroyer_alliance.addTrait("light_lamp");
		bDestroyer_alliance.addTrait("NavalUnit");
		AssetManager.actor_library.add(bDestroyer_alliance);
		Localization.addLocalization(bDestroyer_alliance.name_locale, bDestroyer_alliance.name_locale);

        ///////jet attack for carrier/no spawn

	var CarrierVessel_alliance = AssetManager.actor_library.clone("CarrierVessel_alliance","$boat$");
	    CarrierVessel_alliance.id = "CarrierVessel_alliance";
		CarrierVessel_alliance.boat_type = "carrier_alliance_boat";
		CarrierVessel_alliance.name_locale = "Cargo Ship";
		CarrierVessel_alliance.can_be_inspected = true;
		CarrierVessel_alliance.addDecision("warBoatAttackDecision");
CarrierVessel_alliance.has_avatar_prefab = false;
CarrierVessel_alliance.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Carrier_avatar") };
CarrierVessel_alliance.has_override_avatar_frames = true;
CarrierVessel_alliance.inspect_avatar_scale = 4f;
CarrierVessel_alliance.inspect_avatar_offset_y = 6f;
		CarrierVessel_alliance.animation_speed_based_on_walk_speed = false;
		CarrierVessel_alliance.can_flip = true;
        CarrierVessel_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CarrierVessel_alliance.is_boat = true;
		CarrierVessel_alliance.die_in_lava = false;
		CarrierVessel_alliance.has_override_sprite = false;
		CarrierVessel_alliance.base_stats["mass_2"] = 3000f;
		CarrierVessel_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CarrierVessel_alliance.base_stats["scale"] = 0.35f;
		CarrierVessel_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CarrierVessel_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CarrierVessel_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CarrierVessel_alliance.base_stats["attack_speed"] = 0.3f;
		CarrierVessel_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		CarrierVessel_alliance.base_stats["knockback"] = 2f;
		CarrierVessel_alliance.base_stats["accuracy"] = 0.7f;
		CarrierVessel_alliance.base_stats["targets"] = 1f;
		CarrierVessel_alliance.base_stats["area_of_effect"] = 0.5f;
		CarrierVessel_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(16f);
		CarrierVessel_alliance.inspect_avatar_scale = 1f;
		CarrierVessel_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        CarrierVessel_alliance.sound_spawn = null;
		CarrierVessel_alliance.sound_idle_loop = null;
		CarrierVessel_alliance.sound_death = null;
		CarrierVessel_alliance.default_attack = "AirstrikejetAttack_alliance";
		CarrierVessel_alliance.icon = "iconBoat";
		CarrierVessel_alliance.shadow_texture = "unitShadow_6";
		CarrierVessel_alliance.cost = new ConstructionCost(1, 0, 0, 1);
		CarrierVessel_alliance.texture_asset = new ActorTextureSubAsset("actors/CarrierVessel_alliance/", false);
		CarrierVessel_alliance.special = true;
		CarrierVessel_alliance.has_advanced_textures = false;
		CarrierVessel_alliance.draw_boat_mark = true;
		CarrierVessel_alliance.actor_size = ActorSize.S16_Buffalo;
		CarrierVessel_alliance.animation_walk = ActorAnimationSequences.walk_0;
		CarrierVessel_alliance.animation_idle = ActorAnimationSequences.walk_0;
		CarrierVessel_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		CarrierVessel_alliance.addTrait("boat");
		CarrierVessel_alliance.addTrait("light_lamp");
		CarrierVessel_alliance.addTrait("NavalUnit");
		AssetManager.actor_library.add(CarrierVessel_alliance);
		Localization.addLocalization(CarrierVessel_alliance.name_locale, CarrierVessel_alliance.name_locale);


DecisionAsset nuclearmissileDecision = new DecisionAsset();
nuclearmissileDecision.id = "nuclearmissileDecision";
nuclearmissileDecision.priority = NeuroLayer.Layer_1_Low;
nuclearmissileDecision.path_icon = "ui/icons/MIRV_nuke";
nuclearmissileDecision.cooldown = 300;
nuclearmissileDecision.unique = true;
nuclearmissileDecision.weight = 1f;
nuclearmissileDecision.action_check_launch = delegate(Actor pActor)
{
    return NuclearMissileArtilleryEffect(pActor, null);
};
AssetManager.decisions_library.add(nuclearmissileDecision);


DecisionAsset AntiBossNukeDecision = new DecisionAsset();
AntiBossNukeDecision.id = "AntiBossNukeDecision";
AntiBossNukeDecision.priority = NeuroLayer.Layer_1_Low;
AntiBossNukeDecision.path_icon = "ui/icons/MIRV_nuke";
AntiBossNukeDecision.cooldown = 300;
AntiBossNukeDecision.unique = true;
AntiBossNukeDecision.weight = 1f;
AntiBossNukeDecision.action_check_launch = delegate(Actor pActor)
{
    return AntiBossNuke(pActor, null);
};
AssetManager.decisions_library.add(AntiBossNukeDecision);




	var Submarine_alliance = AssetManager.actor_library.clone("Submarine_alliance","$boat$");
	    Submarine_alliance.id = "Submarine_alliance";
		Submarine_alliance.boat_type = "submarine_alliance_boat";
		Submarine_alliance.name_locale = "Cargo Ship";
		Submarine_alliance.can_be_inspected = true;
		Submarine_alliance.addDecision("missileArtilleryDecision");
		Submarine_alliance.addDecision("nuclearmissileDecision");
		Submarine_alliance.addDecision("AntiBossNukeDecision");
		Submarine_alliance.addDecision("random_swim");
Submarine_alliance.has_avatar_prefab = false;
Submarine_alliance.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Sub_avatar") };
Submarine_alliance.has_override_avatar_frames = true;
Submarine_alliance.inspect_avatar_scale = 4f;
Submarine_alliance.inspect_avatar_offset_y = 6f;
		Submarine_alliance.animation_speed_based_on_walk_speed = false;
		Submarine_alliance.can_flip = true;
        Submarine_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Submarine_alliance.is_boat = true;
		Submarine_alliance.die_in_lava = false;
		Submarine_alliance.has_override_sprite = false;
		Submarine_alliance.base_stats["mass_2"] = 3000f;
		Submarine_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Submarine_alliance.base_stats["scale"] = 0.35f;
		Submarine_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		Submarine_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Submarine_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Submarine_alliance.base_stats["attack_speed"] = 0.3f;
		Submarine_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		Submarine_alliance.base_stats["knockback"] = 2f;
		Submarine_alliance.base_stats["accuracy"] = 0.7f;
		Submarine_alliance.base_stats["targets"] = 1f;
		Submarine_alliance.base_stats["area_of_effect"] = 0.5f;
		Submarine_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(200f);
		Submarine_alliance.inspect_avatar_scale = 1f;
		Submarine_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
		Submarine_alliance.sound_spawn = null;
		Submarine_alliance.sound_idle_loop = null;
		Submarine_alliance.sound_death = null;
		Submarine_alliance.default_attack = "MissileSystemmissile";
		Submarine_alliance.icon = "iconBoat";
		Submarine_alliance.shadow_texture = "unitShadow_6";
		Submarine_alliance.cost = new ConstructionCost(1, 0, 0, 1);
		Submarine_alliance.texture_asset = new ActorTextureSubAsset("actors/Submarine_alliance/", false);
		Submarine_alliance.special = true;
		Submarine_alliance.has_advanced_textures = false;
		Submarine_alliance.draw_boat_mark = true;
		Submarine_alliance.actor_size = ActorSize.S16_Buffalo;
		Submarine_alliance.animation_walk = ActorAnimationSequences.walk_0;
		Submarine_alliance.animation_idle = ActorAnimationSequences.walk_0;
		Submarine_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		Submarine_alliance.addTrait("boat");
		Submarine_alliance.addTrait("light_lamp");
		Submarine_alliance.addTrait("NavalUnit");
		AssetManager.actor_library.add(Submarine_alliance);
		Localization.addLocalization(Submarine_alliance.name_locale, Submarine_alliance.name_locale);

	var FishingBoat_alliance = AssetManager.actor_library.clone("FishingBoat_alliance","$boat$");
	    FishingBoat_alliance.id = "FishingBoat_alliance";
		FishingBoat_alliance.boat_type = "fishing_alliance_boat";
        FishingBoat_alliance.skip_fight_logic = true;
        FishingBoat_alliance.can_be_inspected = false;
		FishingBoat_alliance.name_locale = "Cargo Ship";
		FishingBoat_alliance.addDecision("boat_fishing");
		FishingBoat_alliance.has_avatar_prefab = false;
		FishingBoat_alliance.animation_speed_based_on_walk_speed = false;
		FishingBoat_alliance.can_flip = true;
        FishingBoat_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    FishingBoat_alliance.is_boat = true;
		FishingBoat_alliance.die_in_lava = false;
		FishingBoat_alliance.has_override_sprite = false;
	    FishingBoat_alliance.has_override_avatar_frames = false;
		FishingBoat_alliance.base_stats["mass_2"] = 3000f;
		FishingBoat_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		FishingBoat_alliance.base_stats["scale"] = 0.35f;
		FishingBoat_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		FishingBoat_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		FishingBoat_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		FishingBoat_alliance.base_stats["attack_speed"] = 0.3f;
		FishingBoat_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FishingBoat_alliance.base_stats["knockback"] = 2f;
		FishingBoat_alliance.base_stats["accuracy"] = 0.7f;
		FishingBoat_alliance.base_stats["targets"] = 1f;
		FishingBoat_alliance.base_stats["area_of_effect"] = 0.5f;
		FishingBoat_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FishingBoat_alliance.inspect_avatar_scale = 1f;
		FishingBoat_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
		FishingBoat_alliance.sound_spawn = null;
		FishingBoat_alliance.sound_idle_loop = null;
		FishingBoat_alliance.sound_death = null;
		FishingBoat_alliance.default_attack = "boat_cannonball";
		FishingBoat_alliance.icon = "iconBoat";
		FishingBoat_alliance.shadow_texture = "unitShadow_6";
		FishingBoat_alliance.cost = new ConstructionCost(1, 0, 0, 1);
		FishingBoat_alliance.texture_asset = new ActorTextureSubAsset("actors/FishingBoat_alliance/", false);
		FishingBoat_alliance.special = true;
		FishingBoat_alliance.has_advanced_textures = false;
		FishingBoat_alliance.draw_boat_mark = true;
		FishingBoat_alliance.actor_size = ActorSize.S16_Buffalo;
		FishingBoat_alliance.animation_walk = ActorAnimationSequences.walk_0;
		FishingBoat_alliance.animation_idle = ActorAnimationSequences.walk_0;
		FishingBoat_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		FishingBoat_alliance.addTrait("boat");
		FishingBoat_alliance.addTrait("light_lamp");
		FishingBoat_alliance.addTrait("NavalUnit");
		AssetManager.actor_library.add(FishingBoat_alliance);
		Localization.addLocalization(FishingBoat_alliance.name_locale, FishingBoat_alliance.name_locale);



			var abrawler_alliance = AssetManager.actor_library.clone("abrawler_alliance","$boat$");
	    abrawler_alliance.id = "abrawler_alliance";
	    abrawler_alliance.can_be_inspected = false;
		abrawler_alliance.boat_type = "abrawler_alliance_boat";
		abrawler_alliance.name_locale = "Destroyer Ship";
		abrawler_alliance.addDecision("random_swim");
		abrawler_alliance.has_avatar_prefab = false;
		abrawler_alliance.animation_speed_based_on_walk_speed = false;
		abrawler_alliance.can_flip = true;
        abrawler_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    abrawler_alliance.is_boat = true;
		abrawler_alliance.die_in_lava = false;
		abrawler_alliance.has_override_sprite = false;
	    abrawler_alliance.has_override_avatar_frames = false;
		abrawler_alliance.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		abrawler_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		abrawler_alliance.base_stats["scale"] = 0.35f;
		abrawler_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		abrawler_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		abrawler_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		abrawler_alliance.base_stats["attack_speed"] = 4f;
		abrawler_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		abrawler_alliance.base_stats["knockback"] = 0f;
		abrawler_alliance.base_stats["accuracy"] = 1f;
		abrawler_alliance.base_stats["targets"] = 5f;
		abrawler_alliance.base_stats["area_of_effect"] = 4f;
		abrawler_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		abrawler_alliance.inspect_avatar_scale = 1f;
		abrawler_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        abrawler_alliance.sound_spawn = null;
		abrawler_alliance.sound_idle_loop = null;
		abrawler_alliance.sound_death = null;
		abrawler_alliance.default_attack = "mountedmachinegun";
		abrawler_alliance.icon = "iconBoat";
		abrawler_alliance.shadow_texture = "unitShadow_6";
		abrawler_alliance.cost = new ConstructionCost(0, 0, 0, 1);
		abrawler_alliance.texture_asset = new ActorTextureSubAsset("actors/Brawler_alliance/", false);
		abrawler_alliance.special = true;
		abrawler_alliance.has_advanced_textures = false;
		abrawler_alliance.draw_boat_mark = true;
		abrawler_alliance.actor_size = ActorSize.S16_Buffalo;
		abrawler_alliance.animation_walk = ActorAnimationSequences.walk_0;
		abrawler_alliance.animation_idle = ActorAnimationSequences.walk_0;
		abrawler_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		abrawler_alliance.addTrait("boat");
		abrawler_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(abrawler_alliance);
		Localization.addLocalization(abrawler_alliance.name_locale, abrawler_alliance.name_locale);

		var bbrawler_alliance = AssetManager.actor_library.clone("bbrawler_alliance","$boat$");
	    bbrawler_alliance.id = "bbrawler_alliance";
	    bbrawler_alliance.can_be_inspected = false;
		bbrawler_alliance.boat_type = "bbrawler_alliance_boat";
		bbrawler_alliance.name_locale = "Destroyer Ship";
		bbrawler_alliance.addDecision("random_swim");
		bbrawler_alliance.has_avatar_prefab = false;
		bbrawler_alliance.animation_speed_based_on_walk_speed = false;
		bbrawler_alliance.can_flip = true;
        bbrawler_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bbrawler_alliance.is_boat = true;
		bbrawler_alliance.die_in_lava = false;
		bbrawler_alliance.has_override_sprite = false;
	    bbrawler_alliance.has_override_avatar_frames = false;
		bbrawler_alliance.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		bbrawler_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bbrawler_alliance.base_stats["scale"] = 0.35f;
		bbrawler_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		bbrawler_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		bbrawler_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		bbrawler_alliance.base_stats["attack_speed"] = 4f;
		bbrawler_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		bbrawler_alliance.base_stats["knockback"] = 0f;
		bbrawler_alliance.base_stats["accuracy"] = 1f;
		bbrawler_alliance.base_stats["targets"] = 5f;
		bbrawler_alliance.base_stats["area_of_effect"] = 4f;
		bbrawler_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		bbrawler_alliance.inspect_avatar_scale = 1f;
		bbrawler_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        bbrawler_alliance.sound_spawn = null;
		bbrawler_alliance.sound_idle_loop = null;
		bbrawler_alliance.sound_death = null;
		bbrawler_alliance.default_attack = "mountedmachinegun";
		bbrawler_alliance.icon = "iconBoat";
		bbrawler_alliance.shadow_texture = "unitShadow_6";
		bbrawler_alliance.cost = new ConstructionCost(0, 0, 0, 1);
		bbrawler_alliance.texture_asset = new ActorTextureSubAsset("actors/Brawler_alliance/", false);
		bbrawler_alliance.special = true;
		bbrawler_alliance.has_advanced_textures = false;
		bbrawler_alliance.draw_boat_mark = true;
		bbrawler_alliance.actor_size = ActorSize.S16_Buffalo;
		bbrawler_alliance.animation_walk = ActorAnimationSequences.walk_0;
		bbrawler_alliance.animation_idle = ActorAnimationSequences.walk_0;
		bbrawler_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		bbrawler_alliance.addTrait("boat");
		bbrawler_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(bbrawler_alliance);
		Localization.addLocalization(bbrawler_alliance.name_locale, bbrawler_alliance.name_locale);

			var cbrawler_alliance = AssetManager.actor_library.clone("cbrawler_alliance","$boat$");
	    cbrawler_alliance.id = "cbrawler_alliance";
	    cbrawler_alliance.can_be_inspected = false;
		cbrawler_alliance.boat_type = "cbrawler_alliance_boat";
		cbrawler_alliance.name_locale = "Destroyer Ship";
		cbrawler_alliance.addDecision("random_swim");
		cbrawler_alliance.has_avatar_prefab = false;
		cbrawler_alliance.animation_speed_based_on_walk_speed = false;
		cbrawler_alliance.can_flip = true;
        cbrawler_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    cbrawler_alliance.is_boat = true;
		cbrawler_alliance.die_in_lava = false;
		cbrawler_alliance.has_override_sprite = false;
	    cbrawler_alliance.has_override_avatar_frames = false;
		cbrawler_alliance.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		cbrawler_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		cbrawler_alliance.base_stats["scale"] = 0.35f;
		cbrawler_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		cbrawler_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		cbrawler_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		cbrawler_alliance.base_stats["attack_speed"] = 4f;
		cbrawler_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		cbrawler_alliance.base_stats["knockback"] = 0f;
		cbrawler_alliance.base_stats["accuracy"] = 1f;
		cbrawler_alliance.base_stats["targets"] = 5f;
		cbrawler_alliance.base_stats["area_of_effect"] = 4f;
		cbrawler_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		cbrawler_alliance.inspect_avatar_scale = 1f;
		cbrawler_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        cbrawler_alliance.sound_spawn = null;
		cbrawler_alliance.sound_idle_loop = null;
		cbrawler_alliance.sound_death = null;
		cbrawler_alliance.default_attack = "mountedmachinegun";
		cbrawler_alliance.icon = "iconBoat";
		cbrawler_alliance.shadow_texture = "unitShadow_6";
		cbrawler_alliance.cost = new ConstructionCost(0, 0, 0, 1);
		cbrawler_alliance.texture_asset = new ActorTextureSubAsset("actors/Brawler_alliance/", false);
		cbrawler_alliance.special = true;
		cbrawler_alliance.has_advanced_textures = false;
		cbrawler_alliance.draw_boat_mark = true;
		cbrawler_alliance.actor_size = ActorSize.S16_Buffalo;
		cbrawler_alliance.animation_walk = ActorAnimationSequences.walk_0;
		cbrawler_alliance.animation_idle = ActorAnimationSequences.walk_0;
		cbrawler_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		cbrawler_alliance.addTrait("boat");
		cbrawler_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(cbrawler_alliance);
		Localization.addLocalization(cbrawler_alliance.name_locale, cbrawler_alliance.name_locale);

			var dbrawler_alliance = AssetManager.actor_library.clone("dbrawler_alliance","$boat$");
	    dbrawler_alliance.id = "dbrawler_alliance";
	    dbrawler_alliance.can_be_inspected = false;
		dbrawler_alliance.boat_type = "dbrawler_alliance_boat";
		dbrawler_alliance.name_locale = "Destroyer Ship";
		dbrawler_alliance.addDecision("random_swim");
		dbrawler_alliance.has_avatar_prefab = false;
		dbrawler_alliance.animation_speed_based_on_walk_speed = false;
		dbrawler_alliance.can_flip = true;
        dbrawler_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    dbrawler_alliance.is_boat = true;
		dbrawler_alliance.die_in_lava = false;
		dbrawler_alliance.has_override_sprite = false;
	    dbrawler_alliance.has_override_avatar_frames = false;
		dbrawler_alliance.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		dbrawler_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		dbrawler_alliance.base_stats["scale"] = 0.35f;
		dbrawler_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		dbrawler_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		dbrawler_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		dbrawler_alliance.base_stats["attack_speed"] = 4f;
		dbrawler_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		dbrawler_alliance.base_stats["knockback"] = 0f;
		dbrawler_alliance.base_stats["accuracy"] = 1f;
		dbrawler_alliance.base_stats["targets"] = 5f;
		dbrawler_alliance.base_stats["area_of_effect"] = 4f;
		dbrawler_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		dbrawler_alliance.inspect_avatar_scale = 1f;
		dbrawler_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        dbrawler_alliance.sound_spawn = null;
		dbrawler_alliance.sound_idle_loop = null;
		dbrawler_alliance.sound_death = null;
		dbrawler_alliance.default_attack = "mountedmachinegun";
		dbrawler_alliance.icon = "iconBoat";
		dbrawler_alliance.shadow_texture = "unitShadow_6";
		dbrawler_alliance.cost = new ConstructionCost(0, 0, 0, 1);
		dbrawler_alliance.texture_asset = new ActorTextureSubAsset("actors/Brawler_alliance/", false);
		dbrawler_alliance.special = true;
		dbrawler_alliance.has_advanced_textures = false;
		dbrawler_alliance.draw_boat_mark = true;
		dbrawler_alliance.actor_size = ActorSize.S16_Buffalo;
		dbrawler_alliance.animation_walk = ActorAnimationSequences.walk_0;
		dbrawler_alliance.animation_idle = ActorAnimationSequences.walk_0;
		dbrawler_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		dbrawler_alliance.addTrait("boat");
		dbrawler_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(dbrawler_alliance);
		Localization.addLocalization(dbrawler_alliance.name_locale, dbrawler_alliance.name_locale);

			var ebrawler_alliance = AssetManager.actor_library.clone("ebrawler_alliance","$boat$");
	    ebrawler_alliance.id = "ebrawler_alliance";
	    ebrawler_alliance.can_be_inspected = false;
		ebrawler_alliance.boat_type = "ebrawler_alliance_boat";
		ebrawler_alliance.name_locale = "Destroyer Ship";
		ebrawler_alliance.addDecision("random_swim");
		ebrawler_alliance.has_avatar_prefab = false;
		ebrawler_alliance.animation_speed_based_on_walk_speed = false;
		ebrawler_alliance.can_flip = true;
        ebrawler_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    ebrawler_alliance.is_boat = true;
		ebrawler_alliance.die_in_lava = false;
		ebrawler_alliance.has_override_sprite = false;
	    ebrawler_alliance.has_override_avatar_frames = false;
		ebrawler_alliance.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		ebrawler_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		ebrawler_alliance.base_stats["scale"] = 0.35f;
		ebrawler_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		ebrawler_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		ebrawler_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		ebrawler_alliance.base_stats["attack_speed"] = 4f;
		ebrawler_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		ebrawler_alliance.base_stats["knockback"] = 0f;
		ebrawler_alliance.base_stats["accuracy"] = 1f;
		ebrawler_alliance.base_stats["targets"] = 5f;
		ebrawler_alliance.base_stats["area_of_effect"] = 4f;
		ebrawler_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		ebrawler_alliance.inspect_avatar_scale = 1f;
		ebrawler_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        ebrawler_alliance.sound_spawn = null;
		ebrawler_alliance.sound_idle_loop = null;
		ebrawler_alliance.sound_death = null;
		ebrawler_alliance.default_attack = "mountedmachinegun";
		ebrawler_alliance.icon = "iconBoat";
		ebrawler_alliance.shadow_texture = "unitShadow_6";
		ebrawler_alliance.cost = new ConstructionCost(0, 0, 0, 1);
		ebrawler_alliance.texture_asset = new ActorTextureSubAsset("actors/Brawler_alliance/", false);
		ebrawler_alliance.special = true;
		ebrawler_alliance.has_advanced_textures = false;
		ebrawler_alliance.draw_boat_mark = true;
		ebrawler_alliance.actor_size = ActorSize.S16_Buffalo;
		ebrawler_alliance.animation_walk = ActorAnimationSequences.walk_0;
		ebrawler_alliance.animation_idle = ActorAnimationSequences.walk_0;
		ebrawler_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		ebrawler_alliance.addTrait("boat");
		ebrawler_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(ebrawler_alliance);
		Localization.addLocalization(ebrawler_alliance.name_locale, ebrawler_alliance.name_locale);

			var fbrawler_alliance = AssetManager.actor_library.clone("fbrawler_alliance","$boat$");
	    fbrawler_alliance.id = "fbrawler_alliance";
	    fbrawler_alliance.can_be_inspected = false;
		fbrawler_alliance.boat_type = "fbrawler_alliance_boat";
		fbrawler_alliance.name_locale = "Destroyer Ship";
		fbrawler_alliance.addDecision("random_swim");
		fbrawler_alliance.has_avatar_prefab = false;
		fbrawler_alliance.animation_speed_based_on_walk_speed = false;
		fbrawler_alliance.can_flip = true;
        fbrawler_alliance.check_flip = (BaseSimObject _, WorldTile _) => true;
	    fbrawler_alliance.is_boat = true;
		fbrawler_alliance.die_in_lava = false;
		fbrawler_alliance.has_override_sprite = false;
	    fbrawler_alliance.has_override_avatar_frames = false;
		fbrawler_alliance.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		fbrawler_alliance.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		fbrawler_alliance.base_stats["scale"] = 0.35f;
		fbrawler_alliance.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		fbrawler_alliance.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		fbrawler_alliance.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		fbrawler_alliance.base_stats["attack_speed"] = 4f;
		fbrawler_alliance.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		fbrawler_alliance.base_stats["knockback"] = 0f;
		fbrawler_alliance.base_stats["accuracy"] = 1f;
		fbrawler_alliance.base_stats["targets"] = 5f;
		fbrawler_alliance.base_stats["area_of_effect"] = 4f;
		fbrawler_alliance.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		fbrawler_alliance.inspect_avatar_scale = 1f;
		fbrawler_alliance.sound_hit = "event:/SFX/HIT/HitMetal";
        fbrawler_alliance.sound_spawn = null;
		fbrawler_alliance.sound_idle_loop = null;
		fbrawler_alliance.sound_death = null;
		fbrawler_alliance.default_attack = "mountedmachinegun";
		fbrawler_alliance.icon = "iconBoat";
		fbrawler_alliance.shadow_texture = "unitShadow_6";
		fbrawler_alliance.cost = new ConstructionCost(0, 0, 0, 1);
		fbrawler_alliance.texture_asset = new ActorTextureSubAsset("actors/Brawler_alliance/", false);
		fbrawler_alliance.special = true;
		fbrawler_alliance.has_advanced_textures = false;
		fbrawler_alliance.draw_boat_mark = true;
		fbrawler_alliance.actor_size = ActorSize.S16_Buffalo;
		fbrawler_alliance.animation_walk = ActorAnimationSequences.walk_0;
		fbrawler_alliance.animation_idle = ActorAnimationSequences.walk_0;
		fbrawler_alliance.animation_swim = ActorAnimationSequences.swim_0_3;
		fbrawler_alliance.addTrait("boat");
		fbrawler_alliance.addTrait("light_lamp");
		AssetManager.actor_library.add(fbrawler_alliance);
		Localization.addLocalization(fbrawler_alliance.name_locale, fbrawler_alliance.name_locale);














		///////////////////////HORDE////////////////////////

	var CargoShip_horde = AssetManager.actor_library.clone("CargoShip_horde","$boat$");
	    CargoShip_horde.id = "CargoShip_horde";
		CargoShip_horde.boat_type = "cargo_horde_boat";
		CargoShip_horde.can_be_inspected = false;
        CargoShip_horde.skip_fight_logic = true;
		CargoShip_horde.name_locale = "Cargo Ship";
		CargoShip_horde.addDecision("boat_trading");
		CargoShip_horde.has_avatar_prefab = false;
		CargoShip_horde.animation_speed_based_on_walk_speed = false;
		CargoShip_horde.can_flip = true;
        CargoShip_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CargoShip_horde.is_boat = true;
		CargoShip_horde.die_in_lava = false;
		CargoShip_horde.has_override_sprite = false;
	    CargoShip_horde.has_override_avatar_frames = false;
		CargoShip_horde.base_stats["mass_2"] = 3000f;
		CargoShip_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CargoShip_horde.base_stats["scale"] = 0.35f;
		CargoShip_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CargoShip_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CargoShip_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CargoShip_horde.base_stats["attack_speed"] = 0.3f;
		CargoShip_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		CargoShip_horde.base_stats["knockback"] = 2f;
		CargoShip_horde.base_stats["accuracy"] = 0.7f;
		CargoShip_horde.base_stats["targets"] = 1f;
		CargoShip_horde.base_stats["area_of_effect"] = 0.5f;
		CargoShip_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		CargoShip_horde.inspect_avatar_scale = 1f;
		CargoShip_horde.sound_hit = "event:/SFX/HIT/HitMetal";
		CargoShip_horde.sound_spawn = null;
		CargoShip_horde.sound_idle_loop = null;
		CargoShip_horde.sound_death = null;
		CargoShip_horde.default_attack = "boat_cannonball";
		CargoShip_horde.icon = "iconBoat";
		CargoShip_horde.shadow_texture = "unitShadow_6";
		CargoShip_horde.cost = new ConstructionCost(1, 0, 0, 1);
		CargoShip_horde.texture_asset = new ActorTextureSubAsset("actors/CargoShip_horde/", false);
		CargoShip_horde.special = true;
		CargoShip_horde.has_advanced_textures = false;
		CargoShip_horde.draw_boat_mark = true;
		CargoShip_horde.actor_size = ActorSize.S16_Buffalo;
		CargoShip_horde.animation_walk = ActorAnimationSequences.walk_0;
		CargoShip_horde.animation_idle = ActorAnimationSequences.walk_0;
		CargoShip_horde.animation_swim = ActorAnimationSequences.swim_0_2;
		CargoShip_horde.addTrait("boat");
		CargoShip_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(CargoShip_horde);
		Localization.addLocalization(CargoShip_horde.name_locale, CargoShip_horde.name_locale);


	var Transporter_horde = AssetManager.actor_library.clone("Transporter_horde","$boat$");
	    Transporter_horde.id = "Transporter_horde";
		Transporter_horde.boat_type = "transporter_horde_boat";
		Transporter_horde.can_be_inspected = false;
        Transporter_horde.skip_fight_logic = true;
		Transporter_horde.name_locale = "Cargo Ship";
		Transporter_horde.addDecision("boat_transport_check");
		Transporter_horde.has_avatar_prefab = false;
		Transporter_horde.animation_speed_based_on_walk_speed = false;
		Transporter_horde.can_flip = true;
        Transporter_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Transporter_horde.is_boat = true;
		Transporter_horde.die_in_lava = false;
		Transporter_horde.has_override_sprite = false;
	    Transporter_horde.has_override_avatar_frames = false;
		Transporter_horde.base_stats["mass_2"] = 3000f;
		Transporter_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Transporter_horde.base_stats["scale"] = 0.35f;
		Transporter_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		Transporter_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		Transporter_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Transporter_horde.base_stats["attack_speed"] = 0.3f;
		Transporter_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		Transporter_horde.base_stats["knockback"] = 2f;
		Transporter_horde.base_stats["accuracy"] = 0.7f;
		Transporter_horde.base_stats["targets"] = 1f;
		Transporter_horde.base_stats["area_of_effect"] = 0.5f;
		Transporter_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		Transporter_horde.inspect_avatar_scale = 1f;
		Transporter_horde.sound_hit = "event:/SFX/HIT/HitMetal";
		Transporter_horde.sound_spawn = null;
		Transporter_horde.sound_idle_loop = null;
		Transporter_horde.sound_death = null;
		Transporter_horde.default_attack = "boat_cannonball";
		Transporter_horde.icon = "iconBoat";
		Transporter_horde.shadow_texture = "unitShadow_6";
		Transporter_horde.cost = new ConstructionCost(0, 0, 0, 0);
		Transporter_horde.texture_asset = new ActorTextureSubAsset("actors/Transporter_horde/", false);
		Transporter_horde.special = true;
		Transporter_horde.has_advanced_textures = false;
		Transporter_horde.draw_boat_mark = true;
		Transporter_horde.actor_size = ActorSize.S16_Buffalo;
		Transporter_horde.animation_walk = ActorAnimationSequences.walk_0;
		Transporter_horde.animation_idle = ActorAnimationSequences.walk_0;
		Transporter_horde.animation_swim = ActorAnimationSequences.swim_0_2;
		Transporter_horde.addTrait("boat");
		Transporter_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(Transporter_horde);
		Localization.addLocalization(Transporter_horde.name_locale, Transporter_horde.name_locale);

	var aDestroyer_horde = AssetManager.actor_library.clone("aDestroyer_horde","$boat$");
	    aDestroyer_horde.id = "aDestroyer_horde";
	    aDestroyer_horde.can_be_inspected = true;
		aDestroyer_horde.boat_type = "destroyer_a_horde_boat";
		aDestroyer_horde.name_locale = "Destroyer Ship";
		aDestroyer_horde.addDecision("warBoatAttackDecision");
		aDestroyer_horde.has_avatar_prefab = false;
aDestroyer_horde.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyerhorde_avatar") };
aDestroyer_horde.has_override_avatar_frames = true;
aDestroyer_horde.inspect_avatar_scale = 4f;
aDestroyer_horde.inspect_avatar_offset_y = 6f;
		aDestroyer_horde.animation_speed_based_on_walk_speed = false;
		aDestroyer_horde.can_flip = true;
        aDestroyer_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    aDestroyer_horde.is_boat = true;
		aDestroyer_horde.die_in_lava = false;
		aDestroyer_horde.has_override_sprite = false;
		aDestroyer_horde.base_stats["mass_2"] = 3000f;
		aDestroyer_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		aDestroyer_horde.base_stats["scale"] = 0.35f;
		aDestroyer_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		aDestroyer_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		aDestroyer_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		aDestroyer_horde.base_stats["attack_speed"] = 0.3f;
		aDestroyer_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		aDestroyer_horde.base_stats["knockback"] = 2f;
		aDestroyer_horde.base_stats["accuracy"] = 0.7f;
		aDestroyer_horde.base_stats["targets"] = 1f;
		aDestroyer_horde.base_stats["area_of_effect"] = 0.5f;
		aDestroyer_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		aDestroyer_horde.inspect_avatar_scale = 1f;
		aDestroyer_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        aDestroyer_horde.sound_spawn = null;
		aDestroyer_horde.sound_idle_loop = null;
		aDestroyer_horde.sound_death = null;
		aDestroyer_horde.default_attack = "fighterattackHorde";
		aDestroyer_horde.icon = "iconBoat";
		aDestroyer_horde.shadow_texture = "unitShadow_6";
		aDestroyer_horde.cost = new ConstructionCost(1, 0, 0, 1);
		aDestroyer_horde.texture_asset = new ActorTextureSubAsset("actors/Destroyer_horde/", false);
		aDestroyer_horde.special = true;
		aDestroyer_horde.has_advanced_textures = false;
		aDestroyer_horde.draw_boat_mark = true;
		aDestroyer_horde.actor_size = ActorSize.S16_Buffalo;
		aDestroyer_horde.animation_walk = ActorAnimationSequences.walk_0;
		aDestroyer_horde.animation_idle = ActorAnimationSequences.walk_0;
		aDestroyer_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		aDestroyer_horde.addTrait("boat");
		aDestroyer_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(aDestroyer_horde);
		Localization.addLocalization(aDestroyer_horde.name_locale, aDestroyer_horde.name_locale);

	var bDestroyer_horde = AssetManager.actor_library.clone("bDestroyer_horde","$boat$");
	    bDestroyer_horde.id = "bDestroyer_horde";
		bDestroyer_horde.boat_type = "destroyer_b_horde_boat";
		bDestroyer_horde.can_be_inspected = true;
		bDestroyer_horde.name_locale = "Destroyer Ship";
		bDestroyer_horde.addDecision("warBoatAttackDecision");
		bDestroyer_horde.has_avatar_prefab = false;
		bDestroyer_horde.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyerhorde_avatar") };
bDestroyer_horde.has_override_avatar_frames = true;
bDestroyer_horde.inspect_avatar_scale = 4f;
bDestroyer_horde.inspect_avatar_offset_y = 6f;
		bDestroyer_horde.animation_speed_based_on_walk_speed = false;
		bDestroyer_horde.can_flip = true;
        bDestroyer_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bDestroyer_horde.is_boat = true;
		bDestroyer_horde.die_in_lava = false;
		bDestroyer_horde.has_override_sprite = false;
		bDestroyer_horde.base_stats["mass_2"] = 3000f;
		bDestroyer_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bDestroyer_horde.base_stats["scale"] = 0.35f;
		bDestroyer_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		bDestroyer_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		bDestroyer_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		bDestroyer_horde.base_stats["attack_speed"] = 0.3f;
		bDestroyer_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		bDestroyer_horde.base_stats["knockback"] = 2f;
		bDestroyer_horde.base_stats["accuracy"] = 0.7f;
		bDestroyer_horde.base_stats["targets"] = 1f;
		bDestroyer_horde.base_stats["area_of_effect"] = 0.5f;
		bDestroyer_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		bDestroyer_horde.inspect_avatar_scale = 1f;
		bDestroyer_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        bDestroyer_horde.sound_spawn = null;
		bDestroyer_horde.sound_idle_loop = null;
		bDestroyer_horde.sound_death = null;
		bDestroyer_horde.default_attack = "fighterattackHorde";
		bDestroyer_horde.icon = "iconBoat";
		bDestroyer_horde.shadow_texture = "unitShadow_6";
		bDestroyer_horde.cost = new ConstructionCost(1, 0, 0, 1);
		bDestroyer_horde.texture_asset = new ActorTextureSubAsset("actors/Destroyer_horde/", false);
		bDestroyer_horde.special = true;
		bDestroyer_horde.has_advanced_textures = false;
		bDestroyer_horde.draw_boat_mark = true;
		bDestroyer_horde.actor_size = ActorSize.S16_Buffalo;
		bDestroyer_horde.animation_walk = ActorAnimationSequences.walk_0;
		bDestroyer_horde.animation_idle = ActorAnimationSequences.walk_0;
		bDestroyer_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		bDestroyer_horde.addTrait("boat");
		bDestroyer_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(bDestroyer_horde);
		Localization.addLocalization(bDestroyer_horde.name_locale, bDestroyer_horde.name_locale);

        ///////jet attack for carrier/no spawn

	var CarrierVessel_horde = AssetManager.actor_library.clone("CarrierVessel_horde","$boat$");
	    CarrierVessel_horde.id = "CarrierVessel_horde";
		CarrierVessel_horde.boat_type = "carrier_horde_boat";
		CarrierVessel_horde.name_locale = "Cargo Ship";
		CarrierVessel_horde.can_be_inspected = true;
		CarrierVessel_horde.addDecision("warBoatAttackDecision");
		CarrierVessel_horde.has_avatar_prefab = false;
		CarrierVessel_horde.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Carrierhorde_avatar") };
CarrierVessel_horde.has_override_avatar_frames = true;
CarrierVessel_horde.inspect_avatar_scale = 4f;
CarrierVessel_horde.inspect_avatar_offset_y = 6f;
		CarrierVessel_horde.animation_speed_based_on_walk_speed = false;
		CarrierVessel_horde.can_flip = true;
        CarrierVessel_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CarrierVessel_horde.is_boat = true;
		CarrierVessel_horde.die_in_lava = false;
		CarrierVessel_horde.has_override_sprite = false;
		CarrierVessel_horde.base_stats["mass_2"] = 3000f;
		CarrierVessel_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CarrierVessel_horde.base_stats["scale"] = 0.35f;
		CarrierVessel_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CarrierVessel_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CarrierVessel_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CarrierVessel_horde.base_stats["attack_speed"] = 0.3f;
		CarrierVessel_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		CarrierVessel_horde.base_stats["knockback"] = 2f;
		CarrierVessel_horde.base_stats["accuracy"] = 0.7f;
		CarrierVessel_horde.base_stats["targets"] = 1f;
		CarrierVessel_horde.base_stats["area_of_effect"] = 0.5f;
		CarrierVessel_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(16f);
		CarrierVessel_horde.inspect_avatar_scale = 1f;
		CarrierVessel_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        CarrierVessel_horde.sound_spawn = null;
		CarrierVessel_horde.sound_idle_loop = null;
		CarrierVessel_horde.sound_death = null;
		CarrierVessel_horde.default_attack = "AirstrikejetAttack_horde";
		CarrierVessel_horde.icon = "iconBoat";
		CarrierVessel_horde.shadow_texture = "unitShadow_6";
		CarrierVessel_horde.cost = new ConstructionCost(1, 0, 0, 1);
		CarrierVessel_horde.texture_asset = new ActorTextureSubAsset("actors/CarrierVessel_horde/", false);
		CarrierVessel_horde.special = true;
		CarrierVessel_horde.has_advanced_textures = false;
		CarrierVessel_horde.draw_boat_mark = true;
		CarrierVessel_horde.actor_size = ActorSize.S16_Buffalo;
		CarrierVessel_horde.animation_walk = ActorAnimationSequences.walk_0;
		CarrierVessel_horde.animation_idle = ActorAnimationSequences.walk_0;
		CarrierVessel_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		CarrierVessel_horde.addTrait("boat");
		CarrierVessel_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(CarrierVessel_horde);
		Localization.addLocalization(CarrierVessel_horde.name_locale, CarrierVessel_horde.name_locale);

	var Submarine_horde = AssetManager.actor_library.clone("Submarine_horde","$boat$");
	    Submarine_horde.id = "Submarine_horde";
		Submarine_horde.boat_type = "submarine_horde_boat";
		Submarine_horde.name_locale = "Cargo Ship";
		Submarine_horde.can_be_inspected = true;
		Submarine_horde.addDecision("HORDEmissileArtilleryDecision");
		Submarine_horde.addDecision("nuclearmissileDecision");
		Submarine_horde.addDecision("AntiBossNukeDecision");
		Submarine_horde.addDecision("random_swim");
		Submarine_horde.has_avatar_prefab = false;
		Submarine_horde.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Subhorde_avatar") };
Submarine_horde.has_override_avatar_frames = true;
Submarine_horde.inspect_avatar_scale = 4f;
Submarine_horde.inspect_avatar_offset_y = 6f;
		Submarine_horde.animation_speed_based_on_walk_speed = false;
		Submarine_horde.can_flip = true;
        Submarine_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Submarine_horde.is_boat = true;
		Submarine_horde.die_in_lava = false;
		Submarine_horde.has_override_sprite = false;
		Submarine_horde.base_stats["mass_2"] = 3000f;
		Submarine_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Submarine_horde.base_stats["scale"] = 0.35f;
		Submarine_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		Submarine_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Submarine_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Submarine_horde.base_stats["attack_speed"] = 0.3f;
		Submarine_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		Submarine_horde.base_stats["knockback"] = 2f;
		Submarine_horde.base_stats["accuracy"] = 0.7f;
		Submarine_horde.base_stats["targets"] = 1f;
		Submarine_horde.base_stats["area_of_effect"] = 0.5f;
		Submarine_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(200f);
		Submarine_horde.inspect_avatar_scale = 1f;
		Submarine_horde.sound_hit = "event:/SFX/HIT/HitMetal";
		Submarine_horde.sound_spawn = null;
		Submarine_horde.sound_idle_loop = null;
		Submarine_horde.sound_death = null;
		Submarine_horde.default_attack = "MissileSystemHorde";
		Submarine_horde.icon = "iconBoat";
		Submarine_horde.shadow_texture = "unitShadow_6";
		Submarine_horde.cost = new ConstructionCost(1, 0, 0, 1);
		Submarine_horde.texture_asset = new ActorTextureSubAsset("actors/Submarine_horde/", false);
		Submarine_horde.special = true;
		Submarine_horde.has_advanced_textures = false;
		Submarine_horde.draw_boat_mark = true;
		Submarine_horde.actor_size = ActorSize.S16_Buffalo;
		Submarine_horde.animation_walk = ActorAnimationSequences.walk_0;
		Submarine_horde.animation_idle = ActorAnimationSequences.walk_0;
		Submarine_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		Submarine_horde.addTrait("boat");
		Submarine_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(Submarine_horde);
		Localization.addLocalization(Submarine_horde.name_locale, Submarine_horde.name_locale);

	var FishingBoat_horde = AssetManager.actor_library.clone("FishingBoat_horde","$boat$");
	    FishingBoat_horde.id = "FishingBoat_horde";
		FishingBoat_horde.boat_type = "fishing_horde_boat";
        FishingBoat_horde.skip_fight_logic = true;
        FishingBoat_horde.can_be_inspected = false;
		FishingBoat_horde.name_locale = "Cargo Ship";
		FishingBoat_horde.addDecision("boat_fishing");
		FishingBoat_horde.has_avatar_prefab = false;
		FishingBoat_horde.animation_speed_based_on_walk_speed = false;
		FishingBoat_horde.can_flip = true;
        FishingBoat_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    FishingBoat_horde.is_boat = true;
		FishingBoat_horde.die_in_lava = false;
		FishingBoat_horde.has_override_sprite = false;
	    FishingBoat_horde.has_override_avatar_frames = false;
		FishingBoat_horde.base_stats["mass_2"] = 3000f;
		FishingBoat_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		FishingBoat_horde.base_stats["scale"] = 0.35f;
		FishingBoat_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		FishingBoat_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		FishingBoat_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		FishingBoat_horde.base_stats["attack_speed"] = 0.3f;
		FishingBoat_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FishingBoat_horde.base_stats["knockback"] = 2f;
		FishingBoat_horde.base_stats["accuracy"] = 0.7f;
		FishingBoat_horde.base_stats["targets"] = 1f;
		FishingBoat_horde.base_stats["area_of_effect"] = 0.5f;
		FishingBoat_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FishingBoat_horde.inspect_avatar_scale = 1f;
		FishingBoat_horde.sound_hit = "event:/SFX/HIT/HitMetal";
		FishingBoat_horde.sound_spawn = null;
		FishingBoat_horde.sound_idle_loop = null;
		FishingBoat_horde.sound_death = null;
		FishingBoat_horde.default_attack = "boat_cannonball";
		FishingBoat_horde.icon = "iconBoat";
		FishingBoat_horde.shadow_texture = "unitShadow_6";
		FishingBoat_horde.cost = new ConstructionCost(1, 0, 0, 1);
		FishingBoat_horde.texture_asset = new ActorTextureSubAsset("actors/FishingBoat_horde/", false);
		FishingBoat_horde.special = true;
		FishingBoat_horde.has_advanced_textures = false;
		FishingBoat_horde.draw_boat_mark = true;
		FishingBoat_horde.actor_size = ActorSize.S16_Buffalo;
		FishingBoat_horde.animation_walk = ActorAnimationSequences.walk_0;
		FishingBoat_horde.animation_idle = ActorAnimationSequences.walk_0;
		FishingBoat_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		FishingBoat_horde.addTrait("boat");
		FishingBoat_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(FishingBoat_horde);
		Localization.addLocalization(FishingBoat_horde.name_locale, FishingBoat_horde.name_locale);

			var abrawler_horde = AssetManager.actor_library.clone("abrawler_horde","$boat$");
	    abrawler_horde.id = "abrawler_horde";
	    abrawler_horde.can_be_inspected = false;
		abrawler_horde.boat_type = "abrawler_horde_boat";
		abrawler_horde.name_locale = "Destroyer Ship";
		abrawler_horde.addDecision("random_swim");
		abrawler_horde.has_avatar_prefab = false;
		abrawler_horde.animation_speed_based_on_walk_speed = false;
		abrawler_horde.can_flip = true;
        abrawler_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    abrawler_horde.is_boat = true;
		abrawler_horde.die_in_lava = false;
		abrawler_horde.has_override_sprite = false;
	    abrawler_horde.has_override_avatar_frames = false;
		abrawler_horde.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		abrawler_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		abrawler_horde.base_stats["scale"] = 0.35f;
		abrawler_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		abrawler_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		abrawler_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		abrawler_horde.base_stats["attack_speed"] = 4f;
		abrawler_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		abrawler_horde.base_stats["knockback"] = 0f;
		abrawler_horde.base_stats["accuracy"] = 1f;
		abrawler_horde.base_stats["targets"] = 5f;
		abrawler_horde.base_stats["area_of_effect"] = 4f;
		abrawler_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		abrawler_horde.inspect_avatar_scale = 1f;
		abrawler_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        abrawler_horde.sound_spawn = null;
		abrawler_horde.sound_idle_loop = null;
		abrawler_horde.sound_death = null;
		abrawler_horde.default_attack = "mountedmachinegun";
		abrawler_horde.icon = "iconBoat";
		abrawler_horde.shadow_texture = "unitShadow_6";
		abrawler_horde.cost = new ConstructionCost(0, 0, 0, 1);
		abrawler_horde.texture_asset = new ActorTextureSubAsset("actors/Brawler_horde/", false);
		abrawler_horde.special = true;
		abrawler_horde.has_advanced_textures = false;
		abrawler_horde.draw_boat_mark = true;
		abrawler_horde.actor_size = ActorSize.S16_Buffalo;
		abrawler_horde.animation_walk = ActorAnimationSequences.walk_0;
		abrawler_horde.animation_idle = ActorAnimationSequences.walk_0;
		abrawler_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		abrawler_horde.addTrait("boat");
		abrawler_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(abrawler_horde);
		Localization.addLocalization(abrawler_horde.name_locale, abrawler_horde.name_locale);

		var bbrawler_horde = AssetManager.actor_library.clone("bbrawler_horde","$boat$");
	    bbrawler_horde.id = "bbrawler_horde";
	    bbrawler_horde.can_be_inspected = false;
		bbrawler_horde.boat_type = "bbrawler_horde_boat";
		bbrawler_horde.name_locale = "Destroyer Ship";
		bbrawler_horde.addDecision("random_swim");
		bbrawler_horde.has_avatar_prefab = false;
		bbrawler_horde.animation_speed_based_on_walk_speed = false;
		bbrawler_horde.can_flip = true;
        bbrawler_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bbrawler_horde.is_boat = true;
		bbrawler_horde.die_in_lava = false;
		bbrawler_horde.has_override_sprite = false;
	    bbrawler_horde.has_override_avatar_frames = false;
		bbrawler_horde.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		bbrawler_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bbrawler_horde.base_stats["scale"] = 0.35f;
		bbrawler_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		bbrawler_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		bbrawler_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		bbrawler_horde.base_stats["attack_speed"] = 4f;
		bbrawler_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		bbrawler_horde.base_stats["knockback"] = 0f;
		bbrawler_horde.base_stats["accuracy"] = 1f;
		bbrawler_horde.base_stats["targets"] = 5f;
		bbrawler_horde.base_stats["area_of_effect"] = 4f;
		bbrawler_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		bbrawler_horde.inspect_avatar_scale = 1f;
		bbrawler_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        bbrawler_horde.sound_spawn = null;
		bbrawler_horde.sound_idle_loop = null;
		bbrawler_horde.sound_death = null;
		bbrawler_horde.default_attack = "mountedmachinegun";
		bbrawler_horde.icon = "iconBoat";
		bbrawler_horde.shadow_texture = "unitShadow_6";
		bbrawler_horde.cost = new ConstructionCost(0, 0, 0, 1);
		bbrawler_horde.texture_asset = new ActorTextureSubAsset("actors/Brawler_horde/", false);
		bbrawler_horde.special = true;
		bbrawler_horde.has_advanced_textures = false;
		bbrawler_horde.draw_boat_mark = true;
		bbrawler_horde.actor_size = ActorSize.S16_Buffalo;
		bbrawler_horde.animation_walk = ActorAnimationSequences.walk_0;
		bbrawler_horde.animation_idle = ActorAnimationSequences.walk_0;
		bbrawler_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		bbrawler_horde.addTrait("boat");
		bbrawler_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(bbrawler_horde);
		Localization.addLocalization(bbrawler_horde.name_locale, bbrawler_horde.name_locale);

			var cbrawler_horde = AssetManager.actor_library.clone("cbrawler_horde","$boat$");
	    cbrawler_horde.id = "cbrawler_horde";
	    cbrawler_horde.can_be_inspected = false;
		cbrawler_horde.boat_type = "cbrawler_horde_boat";
		cbrawler_horde.name_locale = "Destroyer Ship";
		cbrawler_horde.addDecision("random_swim");
		cbrawler_horde.has_avatar_prefab = false;
		cbrawler_horde.animation_speed_based_on_walk_speed = false;
		cbrawler_horde.can_flip = true;
        cbrawler_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    cbrawler_horde.is_boat = true;
		cbrawler_horde.die_in_lava = false;
		cbrawler_horde.has_override_sprite = false;
	    cbrawler_horde.has_override_avatar_frames = false;
		cbrawler_horde.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		cbrawler_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		cbrawler_horde.base_stats["scale"] = 0.35f;
		cbrawler_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		cbrawler_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		cbrawler_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		cbrawler_horde.base_stats["attack_speed"] = 4f;
		cbrawler_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		cbrawler_horde.base_stats["knockback"] = 0f;
		cbrawler_horde.base_stats["accuracy"] = 1f;
		cbrawler_horde.base_stats["targets"] = 5f;
		cbrawler_horde.base_stats["area_of_effect"] = 4f;
		cbrawler_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		cbrawler_horde.inspect_avatar_scale = 1f;
		cbrawler_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        cbrawler_horde.sound_spawn = null;
		cbrawler_horde.sound_idle_loop = null;
		cbrawler_horde.sound_death = null;
		cbrawler_horde.default_attack = "mountedmachinegun";
		cbrawler_horde.icon = "iconBoat";
		cbrawler_horde.shadow_texture = "unitShadow_6";
		cbrawler_horde.cost = new ConstructionCost(0, 0, 0, 1);
		cbrawler_horde.texture_asset = new ActorTextureSubAsset("actors/Brawler_horde/", false);
		cbrawler_horde.special = true;
		cbrawler_horde.has_advanced_textures = false;
		cbrawler_horde.draw_boat_mark = true;
		cbrawler_horde.actor_size = ActorSize.S16_Buffalo;
		cbrawler_horde.animation_walk = ActorAnimationSequences.walk_0;
		cbrawler_horde.animation_idle = ActorAnimationSequences.walk_0;
		cbrawler_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		cbrawler_horde.addTrait("boat");
		cbrawler_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(cbrawler_horde);
		Localization.addLocalization(cbrawler_horde.name_locale, cbrawler_horde.name_locale);

			var dbrawler_horde = AssetManager.actor_library.clone("dbrawler_horde","$boat$");
	    dbrawler_horde.id = "dbrawler_horde";
	    dbrawler_horde.can_be_inspected = false;
		dbrawler_horde.boat_type = "dbrawler_horde_boat";
		dbrawler_horde.name_locale = "Destroyer Ship";
		dbrawler_horde.addDecision("random_swim");
		dbrawler_horde.has_avatar_prefab = false;
		dbrawler_horde.animation_speed_based_on_walk_speed = false;
		dbrawler_horde.can_flip = true;
        dbrawler_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    dbrawler_horde.is_boat = true;
		dbrawler_horde.die_in_lava = false;
		dbrawler_horde.has_override_sprite = false;
	    dbrawler_horde.has_override_avatar_frames = false;
		dbrawler_horde.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		dbrawler_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		dbrawler_horde.base_stats["scale"] = 0.35f;
		dbrawler_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		dbrawler_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		dbrawler_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		dbrawler_horde.base_stats["attack_speed"] = 4f;
		dbrawler_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		dbrawler_horde.base_stats["knockback"] = 0f;
		dbrawler_horde.base_stats["accuracy"] = 1f;
		dbrawler_horde.base_stats["targets"] = 5f;
		dbrawler_horde.base_stats["area_of_effect"] = 4f;
		dbrawler_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		dbrawler_horde.inspect_avatar_scale = 1f;
		dbrawler_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        dbrawler_horde.sound_spawn = null;
		dbrawler_horde.sound_idle_loop = null;
		dbrawler_horde.sound_death = null;
		dbrawler_horde.default_attack = "mountedmachinegun";
		dbrawler_horde.icon = "iconBoat";
		dbrawler_horde.shadow_texture = "unitShadow_6";
		dbrawler_horde.cost = new ConstructionCost(0, 0, 0, 1);
		dbrawler_horde.texture_asset = new ActorTextureSubAsset("actors/Brawler_horde/", false);
		dbrawler_horde.special = true;
		dbrawler_horde.has_advanced_textures = false;
		dbrawler_horde.draw_boat_mark = true;
		dbrawler_horde.actor_size = ActorSize.S16_Buffalo;
		dbrawler_horde.animation_walk = ActorAnimationSequences.walk_0;
		dbrawler_horde.animation_idle = ActorAnimationSequences.walk_0;
		dbrawler_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		dbrawler_horde.addTrait("boat");
		dbrawler_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(dbrawler_horde);
		Localization.addLocalization(dbrawler_horde.name_locale, dbrawler_horde.name_locale);

			var ebrawler_horde = AssetManager.actor_library.clone("ebrawler_horde","$boat$");
	    ebrawler_horde.id = "ebrawler_horde";
	    ebrawler_horde.can_be_inspected = false;
		ebrawler_horde.boat_type = "ebrawler_horde_boat";
		ebrawler_horde.name_locale = "Destroyer Ship";
		ebrawler_horde.addDecision("random_swim");
		ebrawler_horde.has_avatar_prefab = false;
		ebrawler_horde.animation_speed_based_on_walk_speed = false;
		ebrawler_horde.can_flip = true;
        ebrawler_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    ebrawler_horde.is_boat = true;
		ebrawler_horde.die_in_lava = false;
		ebrawler_horde.has_override_sprite = false;
	    ebrawler_horde.has_override_avatar_frames = false;
		ebrawler_horde.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		ebrawler_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		ebrawler_horde.base_stats["scale"] = 0.35f;
		ebrawler_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		ebrawler_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		ebrawler_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		ebrawler_horde.base_stats["attack_speed"] = 4f;
		ebrawler_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		ebrawler_horde.base_stats["knockback"] = 0f;
		ebrawler_horde.base_stats["accuracy"] = 1f;
		ebrawler_horde.base_stats["targets"] = 5f;
		ebrawler_horde.base_stats["area_of_effect"] = 4f;
		ebrawler_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		ebrawler_horde.inspect_avatar_scale = 1f;
		ebrawler_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        ebrawler_horde.sound_spawn = null;
		ebrawler_horde.sound_idle_loop = null;
		ebrawler_horde.sound_death = null;
		ebrawler_horde.default_attack = "mountedmachinegun";
		ebrawler_horde.icon = "iconBoat";
		ebrawler_horde.shadow_texture = "unitShadow_6";
		ebrawler_horde.cost = new ConstructionCost(0, 0, 0, 1);
		ebrawler_horde.texture_asset = new ActorTextureSubAsset("actors/Brawler_horde/", false);
		ebrawler_horde.special = true;
		ebrawler_horde.has_advanced_textures = false;
		ebrawler_horde.draw_boat_mark = true;
		ebrawler_horde.actor_size = ActorSize.S16_Buffalo;
		ebrawler_horde.animation_walk = ActorAnimationSequences.walk_0;
		ebrawler_horde.animation_idle = ActorAnimationSequences.walk_0;
		ebrawler_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		ebrawler_horde.addTrait("boat");
		ebrawler_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(ebrawler_horde);
		Localization.addLocalization(ebrawler_horde.name_locale, ebrawler_horde.name_locale);

			var fbrawler_horde = AssetManager.actor_library.clone("fbrawler_horde","$boat$");
	    fbrawler_horde.id = "fbrawler_horde";
	    fbrawler_horde.can_be_inspected = false;
		fbrawler_horde.boat_type = "fbrawler_horde_boat";
		fbrawler_horde.name_locale = "Destroyer Ship";
		fbrawler_horde.addDecision("random_swim");
		fbrawler_horde.has_avatar_prefab = false;
		fbrawler_horde.animation_speed_based_on_walk_speed = false;
		fbrawler_horde.can_flip = true;
        fbrawler_horde.check_flip = (BaseSimObject _, WorldTile _) => true;
	    fbrawler_horde.is_boat = true;
		fbrawler_horde.die_in_lava = false;
		fbrawler_horde.has_override_sprite = false;
	    fbrawler_horde.has_override_avatar_frames = false;
		fbrawler_horde.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		fbrawler_horde.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		fbrawler_horde.base_stats["scale"] = 0.35f;
		fbrawler_horde.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		fbrawler_horde.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		fbrawler_horde.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		fbrawler_horde.base_stats["attack_speed"] = 4f;
		fbrawler_horde.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		fbrawler_horde.base_stats["knockback"] = 0f;
		fbrawler_horde.base_stats["accuracy"] = 1f;
		fbrawler_horde.base_stats["targets"] = 5f;
		fbrawler_horde.base_stats["area_of_effect"] = 4f;
		fbrawler_horde.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		fbrawler_horde.inspect_avatar_scale = 1f;
		fbrawler_horde.sound_hit = "event:/SFX/HIT/HitMetal";
        fbrawler_horde.sound_spawn = null;
		fbrawler_horde.sound_idle_loop = null;
		fbrawler_horde.sound_death = null;
		fbrawler_horde.default_attack = "mountedmachinegun";
		fbrawler_horde.icon = "iconBoat";
		fbrawler_horde.shadow_texture = "unitShadow_6";
		fbrawler_horde.cost = new ConstructionCost(0, 0, 0, 1);
		fbrawler_horde.texture_asset = new ActorTextureSubAsset("actors/Brawler_horde/", false);
		fbrawler_horde.special = true;
		fbrawler_horde.has_advanced_textures = false;
		fbrawler_horde.draw_boat_mark = true;
		fbrawler_horde.actor_size = ActorSize.S16_Buffalo;
		fbrawler_horde.animation_walk = ActorAnimationSequences.walk_0;
		fbrawler_horde.animation_idle = ActorAnimationSequences.walk_0;
		fbrawler_horde.animation_swim = ActorAnimationSequences.swim_0_3;
		fbrawler_horde.addTrait("boat");
		fbrawler_horde.addTrait("light_lamp");
		AssetManager.actor_library.add(fbrawler_horde);
		Localization.addLocalization(fbrawler_horde.name_locale, fbrawler_horde.name_locale);


		////////////////////////////////////GAIA/////////////////////////////////////////////

        	var CargoShip_gaia = AssetManager.actor_library.clone("CargoShip_gaia","$boat$");
	    CargoShip_gaia.id = "CargoShip_gaia";
		CargoShip_gaia.boat_type = "cargo_gaia_boat";
		CargoShip_gaia.can_be_inspected = false;
        CargoShip_gaia.skip_fight_logic = true;
		CargoShip_gaia.name_locale = "Cargo Ship";
		CargoShip_gaia.addDecision("boat_trading");
		CargoShip_gaia.has_avatar_prefab = false;
		CargoShip_gaia.animation_speed_based_on_walk_speed = false;
		CargoShip_gaia.can_flip = true;
        CargoShip_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CargoShip_gaia.is_boat = true;
		CargoShip_gaia.die_in_lava = false;
		CargoShip_gaia.has_override_sprite = false;
	    CargoShip_gaia.has_override_avatar_frames = false;
		CargoShip_gaia.base_stats["mass_2"] = 3000f;
		CargoShip_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CargoShip_gaia.base_stats["scale"] = 0.35f;
		CargoShip_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CargoShip_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CargoShip_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CargoShip_gaia.base_stats["attack_speed"] = 0.3f;
		CargoShip_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		CargoShip_gaia.base_stats["knockback"] = 2f;
		CargoShip_gaia.base_stats["accuracy"] = 0.7f;
		CargoShip_gaia.base_stats["targets"] = 1f;
		CargoShip_gaia.base_stats["area_of_effect"] = 0.5f;
		CargoShip_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		CargoShip_gaia.inspect_avatar_scale = 1f;
		CargoShip_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
		CargoShip_gaia.sound_spawn = null;
		CargoShip_gaia.sound_idle_loop = null;
		CargoShip_gaia.sound_death = null;
		CargoShip_gaia.default_attack = "boat_cannonball";
		CargoShip_gaia.icon = "iconBoat";
		CargoShip_gaia.shadow_texture = "unitShadow_6";
		CargoShip_gaia.cost = new ConstructionCost(1, 0, 0, 1);
		CargoShip_gaia.texture_asset = new ActorTextureSubAsset("actors/CargoShip_gaia/", false);
		CargoShip_gaia.special = true;
		CargoShip_gaia.has_advanced_textures = false;
		CargoShip_gaia.draw_boat_mark = true;
		CargoShip_gaia.actor_size = ActorSize.S16_Buffalo;
		CargoShip_gaia.animation_walk = ActorAnimationSequences.walk_0;
		CargoShip_gaia.animation_idle = ActorAnimationSequences.walk_0;
		CargoShip_gaia.animation_swim = ActorAnimationSequences.swim_0_2;
		CargoShip_gaia.addTrait("boat");
		CargoShip_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(CargoShip_gaia);
		Localization.addLocalization(CargoShip_gaia.name_locale, CargoShip_gaia.name_locale);


	var Transporter_gaia = AssetManager.actor_library.clone("Transporter_gaia","$boat$");
	    Transporter_gaia.id = "Transporter_gaia";
		Transporter_gaia.boat_type = "transporter_gaia_boat";
		Transporter_gaia.can_be_inspected = false;
        Transporter_gaia.skip_fight_logic = true;
		Transporter_gaia.name_locale = "Cargo Ship";
		Transporter_gaia.addDecision("boat_transport_check");
		Transporter_gaia.has_avatar_prefab = false;
		Transporter_gaia.animation_speed_based_on_walk_speed = false;
		Transporter_gaia.can_flip = true;
        Transporter_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Transporter_gaia.is_boat = true;
		Transporter_gaia.die_in_lava = false;
		Transporter_gaia.has_override_sprite = false;
	    Transporter_gaia.has_override_avatar_frames = false;
		Transporter_gaia.base_stats["mass_2"] = 3000f;
		Transporter_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Transporter_gaia.base_stats["scale"] = 0.35f;
		Transporter_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		Transporter_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		Transporter_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Transporter_gaia.base_stats["attack_speed"] = 0.3f;
		Transporter_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		Transporter_gaia.base_stats["knockback"] = 2f;
		Transporter_gaia.base_stats["accuracy"] = 0.7f;
		Transporter_gaia.base_stats["targets"] = 1f;
		Transporter_gaia.base_stats["area_of_effect"] = 0.5f;
		Transporter_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		Transporter_gaia.inspect_avatar_scale = 1f;
		Transporter_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
		Transporter_gaia.sound_spawn = null;
		Transporter_gaia.sound_idle_loop = null;
		Transporter_gaia.sound_death = null;
		Transporter_gaia.default_attack = "boat_cannonball";
		Transporter_gaia.icon = "iconBoat";
		Transporter_gaia.shadow_texture = "unitShadow_6";
		Transporter_gaia.cost = new ConstructionCost(0, 0, 0, 0);
		Transporter_gaia.texture_asset = new ActorTextureSubAsset("actors/Transporter_gaia/", false);
		Transporter_gaia.special = true;
		Transporter_gaia.has_advanced_textures = false;
		Transporter_gaia.draw_boat_mark = true;
		Transporter_gaia.actor_size = ActorSize.S16_Buffalo;
		Transporter_gaia.animation_walk = ActorAnimationSequences.walk_0;
		Transporter_gaia.animation_idle = ActorAnimationSequences.walk_0;
		Transporter_gaia.animation_swim = ActorAnimationSequences.swim_0_2;
		Transporter_gaia.addTrait("boat");
		Transporter_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(Transporter_gaia);
		Localization.addLocalization(Transporter_gaia.name_locale, Transporter_gaia.name_locale);

	var aDestroyer_gaia = AssetManager.actor_library.clone("aDestroyer_gaia","$boat$");
	    aDestroyer_gaia.id = "aDestroyer_gaia";
	    aDestroyer_gaia.can_be_inspected = true;
		aDestroyer_gaia.boat_type = "destroyer_a_gaia_boat";
		aDestroyer_gaia.name_locale = "Destroyer Ship";
		aDestroyer_gaia.addDecision("warBoatAttackDecision");
		aDestroyer_gaia.has_avatar_prefab = false;
		aDestroyer_gaia.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyergaia_avatar") };
aDestroyer_gaia.has_override_avatar_frames = true;
aDestroyer_gaia.inspect_avatar_scale = 4f;
aDestroyer_gaia.inspect_avatar_offset_y = 6f;
		aDestroyer_gaia.animation_speed_based_on_walk_speed = false;
		aDestroyer_gaia.can_flip = true;
        aDestroyer_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    aDestroyer_gaia.is_boat = true;
		aDestroyer_gaia.die_in_lava = false;
		aDestroyer_gaia.has_override_sprite = false;
		aDestroyer_gaia.base_stats["mass_2"] = 3000f;
		aDestroyer_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		aDestroyer_gaia.base_stats["scale"] = 0.35f;
		aDestroyer_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		aDestroyer_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		aDestroyer_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		aDestroyer_gaia.base_stats["attack_speed"] = 0.3f;
		aDestroyer_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		aDestroyer_gaia.base_stats["knockback"] = 2f;
		aDestroyer_gaia.base_stats["accuracy"] = 0.7f;
		aDestroyer_gaia.base_stats["targets"] = 1f;
		aDestroyer_gaia.base_stats["area_of_effect"] = 0.5f;
		aDestroyer_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		aDestroyer_gaia.inspect_avatar_scale = 1f;
		aDestroyer_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        aDestroyer_gaia.sound_spawn = null;
		aDestroyer_gaia.sound_idle_loop = null;
		aDestroyer_gaia.sound_death = null;
		aDestroyer_gaia.default_attack = "fighterattackGaia";
		aDestroyer_gaia.icon = "iconBoat";
		aDestroyer_gaia.shadow_texture = "unitShadow_6";
		aDestroyer_gaia.cost = new ConstructionCost(1, 0, 0, 1);
		aDestroyer_gaia.texture_asset = new ActorTextureSubAsset("actors/Destroyer_gaia/", false);
		aDestroyer_gaia.special = true;
		aDestroyer_gaia.has_advanced_textures = false;
		aDestroyer_gaia.draw_boat_mark = true;
		aDestroyer_gaia.actor_size = ActorSize.S16_Buffalo;
		aDestroyer_gaia.animation_walk = ActorAnimationSequences.walk_0;
		aDestroyer_gaia.animation_idle = ActorAnimationSequences.walk_0;
		aDestroyer_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		aDestroyer_gaia.addTrait("boat");
		aDestroyer_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(aDestroyer_gaia);
		Localization.addLocalization(aDestroyer_gaia.name_locale, aDestroyer_gaia.name_locale);

	var bDestroyer_gaia = AssetManager.actor_library.clone("bDestroyer_gaia","$boat$");
	    bDestroyer_gaia.id = "bDestroyer_gaia";
		bDestroyer_gaia.boat_type = "destroyer_b_gaia_boat";
		bDestroyer_gaia.can_be_inspected = true;
		bDestroyer_gaia.name_locale = "Destroyer Ship";
		bDestroyer_gaia.addDecision("warBoatAttackDecision");
		bDestroyer_gaia.has_avatar_prefab = false;
bDestroyer_gaia.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyergaia_avatar") };
bDestroyer_gaia.has_override_avatar_frames = true;
bDestroyer_gaia.inspect_avatar_scale = 4f;
bDestroyer_gaia.inspect_avatar_offset_y = 6f;
		bDestroyer_gaia.animation_speed_based_on_walk_speed = false;
		bDestroyer_gaia.can_flip = true;
        bDestroyer_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bDestroyer_gaia.is_boat = true;
		bDestroyer_gaia.die_in_lava = false;
		bDestroyer_gaia.has_override_sprite = false;
		bDestroyer_gaia.base_stats["mass_2"] = 3000f;
		bDestroyer_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bDestroyer_gaia.base_stats["scale"] = 0.35f;
		bDestroyer_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		bDestroyer_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		bDestroyer_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		bDestroyer_gaia.base_stats["attack_speed"] = 0.3f;
		bDestroyer_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		bDestroyer_gaia.base_stats["knockback"] = 2f;
		bDestroyer_gaia.base_stats["accuracy"] = 0.7f;
		bDestroyer_gaia.base_stats["targets"] = 1f;
		bDestroyer_gaia.base_stats["area_of_effect"] = 0.5f;
		bDestroyer_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		bDestroyer_gaia.inspect_avatar_scale = 1f;
		bDestroyer_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        bDestroyer_gaia.sound_spawn = null;
		bDestroyer_gaia.sound_idle_loop = null;
		bDestroyer_gaia.sound_death = null;
		bDestroyer_gaia.default_attack = "fighterattackGaia";
		bDestroyer_gaia.icon = "iconBoat";
		bDestroyer_gaia.shadow_texture = "unitShadow_6";
		bDestroyer_gaia.cost = new ConstructionCost(1, 0, 0, 1);
		bDestroyer_gaia.texture_asset = new ActorTextureSubAsset("actors/Destroyer_gaia/", false);
		bDestroyer_gaia.special = true;
		bDestroyer_gaia.has_advanced_textures = false;
		bDestroyer_gaia.draw_boat_mark = true;
		bDestroyer_gaia.actor_size = ActorSize.S16_Buffalo;
		bDestroyer_gaia.animation_walk = ActorAnimationSequences.walk_0;
		bDestroyer_gaia.animation_idle = ActorAnimationSequences.walk_0;
		bDestroyer_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		bDestroyer_gaia.addTrait("boat");
		bDestroyer_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(bDestroyer_gaia);
		Localization.addLocalization(bDestroyer_gaia.name_locale, bDestroyer_gaia.name_locale);

        ///////jet attack for carrier/no spawn

	var CarrierVessel_gaia = AssetManager.actor_library.clone("CarrierVessel_gaia","$boat$");
	    CarrierVessel_gaia.id = "CarrierVessel_gaia";
		CarrierVessel_gaia.boat_type = "carrier_gaia_boat";
		CarrierVessel_gaia.name_locale = "Cargo Ship";
		CarrierVessel_gaia.can_be_inspected = true;
		CarrierVessel_gaia.addDecision("warBoatAttackDecision");
		CarrierVessel_gaia.has_avatar_prefab = false;
CarrierVessel_gaia.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Carriergaia_avatar") };
CarrierVessel_gaia.has_override_avatar_frames = true;
CarrierVessel_gaia.inspect_avatar_scale = 4f;
CarrierVessel_gaia.inspect_avatar_offset_y = 6f;
		CarrierVessel_gaia.animation_speed_based_on_walk_speed = false;
		CarrierVessel_gaia.can_flip = true;
        CarrierVessel_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CarrierVessel_gaia.is_boat = true;
		CarrierVessel_gaia.die_in_lava = false;
		CarrierVessel_gaia.has_override_sprite = false;
		CarrierVessel_gaia.base_stats["mass_2"] = 3000f;
		CarrierVessel_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CarrierVessel_gaia.base_stats["scale"] = 0.35f;
		CarrierVessel_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CarrierVessel_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CarrierVessel_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CarrierVessel_gaia.base_stats["attack_speed"] = 0.3f;
		CarrierVessel_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		CarrierVessel_gaia.base_stats["knockback"] = 2f;
		CarrierVessel_gaia.base_stats["accuracy"] = 0.7f;
		CarrierVessel_gaia.base_stats["targets"] = 1f;
		CarrierVessel_gaia.base_stats["area_of_effect"] = 0.5f;
		CarrierVessel_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(16f);
		CarrierVessel_gaia.inspect_avatar_scale = 1f;
		CarrierVessel_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        CarrierVessel_gaia.sound_spawn = null;
		CarrierVessel_gaia.sound_idle_loop = null;
		CarrierVessel_gaia.sound_death = null;
		CarrierVessel_gaia.default_attack = "AirstrikejetAttack_gaia";
		CarrierVessel_gaia.icon = "iconBoat";
		CarrierVessel_gaia.shadow_texture = "unitShadow_6";
		CarrierVessel_gaia.cost = new ConstructionCost(1, 0, 0, 1);
		CarrierVessel_gaia.texture_asset = new ActorTextureSubAsset("actors/CarrierVessel_gaia/", false);
		CarrierVessel_gaia.special = true;
		CarrierVessel_gaia.has_advanced_textures = false;
		CarrierVessel_gaia.draw_boat_mark = true;
		CarrierVessel_gaia.actor_size = ActorSize.S16_Buffalo;
		CarrierVessel_gaia.animation_walk = ActorAnimationSequences.walk_0;
		CarrierVessel_gaia.animation_idle = ActorAnimationSequences.walk_0;
		CarrierVessel_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		CarrierVessel_gaia.addTrait("boat");
		CarrierVessel_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(CarrierVessel_gaia);
		Localization.addLocalization(CarrierVessel_gaia.name_locale, CarrierVessel_gaia.name_locale);

	var Submarine_gaia = AssetManager.actor_library.clone("Submarine_gaia","$boat$");
	    Submarine_gaia.id = "Submarine_gaia";
		Submarine_gaia.boat_type = "submarine_gaia_boat";
		Submarine_gaia.name_locale = "Cargo Ship";
		Submarine_gaia.can_be_inspected = true;
		Submarine_gaia.addDecision("GAIAmissileArtilleryDecision");
		Submarine_gaia.addDecision("nuclearmissileDecision");
		Submarine_gaia.addDecision("AntiBossNukeDecision");
		Submarine_gaia.addDecision("random_swim");
		Submarine_gaia.has_avatar_prefab = false;
Submarine_gaia.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Subgaia_avatar") };
Submarine_gaia.has_override_avatar_frames = true;
Submarine_gaia.inspect_avatar_scale = 1f;
Submarine_gaia.inspect_avatar_offset_y = 6f;
		Submarine_gaia.animation_speed_based_on_walk_speed = false;
		Submarine_gaia.can_flip = true;
        Submarine_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Submarine_gaia.is_boat = true;
		Submarine_gaia.die_in_lava = false;
		Submarine_gaia.has_override_sprite = false;
		Submarine_gaia.base_stats["mass_2"] = 3000f;
		Submarine_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Submarine_gaia.base_stats["scale"] = 0.35f;
		Submarine_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		Submarine_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Submarine_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Submarine_gaia.base_stats["attack_speed"] = 0.3f;
		Submarine_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		Submarine_gaia.base_stats["knockback"] = 2f;
		Submarine_gaia.base_stats["accuracy"] = 0.7f;
		Submarine_gaia.base_stats["targets"] = 1f;
		Submarine_gaia.base_stats["area_of_effect"] = 0.5f;
		Submarine_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(200f);
		Submarine_gaia.inspect_avatar_scale = 1f;
		Submarine_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
		Submarine_gaia.sound_spawn = null;
		Submarine_gaia.sound_idle_loop = null;
		Submarine_gaia.sound_death = null;
		Submarine_gaia.default_attack = "MissileSystemGaia";
		Submarine_gaia.icon = "iconBoat";
		Submarine_gaia.shadow_texture = "unitShadow_6";
		Submarine_gaia.cost = new ConstructionCost(1, 0, 0, 1);
		Submarine_gaia.texture_asset = new ActorTextureSubAsset("actors/Submarine_gaia/", false);
		Submarine_gaia.special = true;
		Submarine_gaia.has_advanced_textures = false;
		Submarine_gaia.draw_boat_mark = true;
		Submarine_gaia.actor_size = ActorSize.S16_Buffalo;
		Submarine_gaia.animation_walk = ActorAnimationSequences.walk_0;
		Submarine_gaia.animation_idle = ActorAnimationSequences.walk_0;
		Submarine_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		Submarine_gaia.addTrait("boat");
		Submarine_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(Submarine_gaia);
		Localization.addLocalization(Submarine_gaia.name_locale, Submarine_gaia.name_locale);

	var FishingBoat_gaia = AssetManager.actor_library.clone("FishingBoat_gaia","$boat$");
	    FishingBoat_gaia.id = "FishingBoat_gaia";
		FishingBoat_gaia.boat_type = "fishing_gaia_boat";
        FishingBoat_gaia.skip_fight_logic = true;
        FishingBoat_gaia.can_be_inspected = false;
		FishingBoat_gaia.name_locale = "Cargo Ship";
		FishingBoat_gaia.addDecision("boat_fishing");
		FishingBoat_gaia.has_avatar_prefab = false;
		FishingBoat_gaia.animation_speed_based_on_walk_speed = false;
		FishingBoat_gaia.can_flip = true;
        FishingBoat_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    FishingBoat_gaia.is_boat = true;
		FishingBoat_gaia.die_in_lava = false;
		FishingBoat_gaia.has_override_sprite = false;
	    FishingBoat_gaia.has_override_avatar_frames = false;
		FishingBoat_gaia.base_stats["mass_2"] = 3000f;
		FishingBoat_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		FishingBoat_gaia.base_stats["scale"] = 0.35f;
		FishingBoat_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		FishingBoat_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		FishingBoat_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		FishingBoat_gaia.base_stats["attack_speed"] = 0.3f;
		FishingBoat_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FishingBoat_gaia.base_stats["knockback"] = 2f;
		FishingBoat_gaia.base_stats["accuracy"] = 0.7f;
		FishingBoat_gaia.base_stats["targets"] = 1f;
		FishingBoat_gaia.base_stats["area_of_effect"] = 0.5f;
		FishingBoat_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FishingBoat_gaia.inspect_avatar_scale = 1f;
		FishingBoat_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
		FishingBoat_gaia.sound_spawn = null;
		FishingBoat_gaia.sound_idle_loop = null;
		FishingBoat_gaia.sound_death = null;
		FishingBoat_gaia.default_attack = "boat_cannonball";
		FishingBoat_gaia.icon = "iconBoat";
		FishingBoat_gaia.shadow_texture = "unitShadow_6";
		FishingBoat_gaia.cost = new ConstructionCost(1, 0, 0, 1);
		FishingBoat_gaia.texture_asset = new ActorTextureSubAsset("actors/FishingBoat_gaia/", false);
		FishingBoat_gaia.special = true;
		FishingBoat_gaia.has_advanced_textures = false;
		FishingBoat_gaia.draw_boat_mark = true;
		FishingBoat_gaia.actor_size = ActorSize.S16_Buffalo;
		FishingBoat_gaia.animation_walk = ActorAnimationSequences.walk_0;
		FishingBoat_gaia.animation_idle = ActorAnimationSequences.walk_0;
		FishingBoat_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		FishingBoat_gaia.addTrait("boat");
		FishingBoat_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(FishingBoat_gaia);
		Localization.addLocalization(FishingBoat_gaia.name_locale, FishingBoat_gaia.name_locale);

			var abrawler_gaia = AssetManager.actor_library.clone("abrawler_gaia","$boat$");
	    abrawler_gaia.id = "abrawler_gaia";
	    abrawler_gaia.can_be_inspected = false;
		abrawler_gaia.boat_type = "abrawler_gaia_boat";
		abrawler_gaia.name_locale = "Destroyer Ship";
		abrawler_gaia.addDecision("random_swim");
		abrawler_gaia.has_avatar_prefab = false;
		abrawler_gaia.animation_speed_based_on_walk_speed = false;
		abrawler_gaia.can_flip = true;
        abrawler_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    abrawler_gaia.is_boat = true;
		abrawler_gaia.die_in_lava = false;
		abrawler_gaia.has_override_sprite = false;
	    abrawler_gaia.has_override_avatar_frames = false;
		abrawler_gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		abrawler_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		abrawler_gaia.base_stats["scale"] = 0.35f;
		abrawler_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		abrawler_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		abrawler_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		abrawler_gaia.base_stats["attack_speed"] = 4f;
		abrawler_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		abrawler_gaia.base_stats["knockback"] = 0f;
		abrawler_gaia.base_stats["accuracy"] = 1f;
		abrawler_gaia.base_stats["targets"] = 5f;
		abrawler_gaia.base_stats["area_of_effect"] = 4f;
		abrawler_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		abrawler_gaia.inspect_avatar_scale = 1f;
		abrawler_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        abrawler_gaia.sound_spawn = null;
		abrawler_gaia.sound_idle_loop = null;
		abrawler_gaia.sound_death = null;
		abrawler_gaia.default_attack = "mountedmachinegun";
		abrawler_gaia.icon = "iconBoat";
		abrawler_gaia.shadow_texture = "unitShadow_6";
		abrawler_gaia.cost = new ConstructionCost(0, 0, 0, 1);
		abrawler_gaia.texture_asset = new ActorTextureSubAsset("actors/Brawler_gaia/", false);
		abrawler_gaia.special = true;
		abrawler_gaia.has_advanced_textures = false;
		abrawler_gaia.draw_boat_mark = true;
		abrawler_gaia.actor_size = ActorSize.S16_Buffalo;
		abrawler_gaia.animation_walk = ActorAnimationSequences.walk_0;
		abrawler_gaia.animation_idle = ActorAnimationSequences.walk_0;
		abrawler_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		abrawler_gaia.addTrait("boat");
		abrawler_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(abrawler_gaia);
		Localization.addLocalization(abrawler_gaia.name_locale, abrawler_gaia.name_locale);

		var bbrawler_gaia = AssetManager.actor_library.clone("bbrawler_gaia","$boat$");
	    bbrawler_gaia.id = "bbrawler_gaia";
	    bbrawler_gaia.can_be_inspected = false;
		bbrawler_gaia.boat_type = "bbrawler_gaia_boat";
		bbrawler_gaia.name_locale = "Destroyer Ship";
		bbrawler_gaia.addDecision("random_swim");
		bbrawler_gaia.has_avatar_prefab = false;
		bbrawler_gaia.animation_speed_based_on_walk_speed = false;
		bbrawler_gaia.can_flip = true;
        bbrawler_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bbrawler_gaia.is_boat = true;
		bbrawler_gaia.die_in_lava = false;
		bbrawler_gaia.has_override_sprite = false;
	    bbrawler_gaia.has_override_avatar_frames = false;
		bbrawler_gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		bbrawler_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bbrawler_gaia.base_stats["scale"] = 0.35f;
		bbrawler_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		bbrawler_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		bbrawler_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		bbrawler_gaia.base_stats["attack_speed"] = 4f;
		bbrawler_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		bbrawler_gaia.base_stats["knockback"] = 0f;
		bbrawler_gaia.base_stats["accuracy"] = 1f;
		bbrawler_gaia.base_stats["targets"] = 5f;
		bbrawler_gaia.base_stats["area_of_effect"] = 4f;
		bbrawler_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		bbrawler_gaia.inspect_avatar_scale = 1f;
		bbrawler_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        bbrawler_gaia.sound_spawn = null;
		bbrawler_gaia.sound_idle_loop = null;
		bbrawler_gaia.sound_death = null;
		bbrawler_gaia.default_attack = "mountedmachinegun";
		bbrawler_gaia.icon = "iconBoat";
		bbrawler_gaia.shadow_texture = "unitShadow_6";
		bbrawler_gaia.cost = new ConstructionCost(0, 0, 0, 1);
		bbrawler_gaia.texture_asset = new ActorTextureSubAsset("actors/Brawler_gaia/", false);
		bbrawler_gaia.special = true;
		bbrawler_gaia.has_advanced_textures = false;
		bbrawler_gaia.draw_boat_mark = true;
		bbrawler_gaia.actor_size = ActorSize.S16_Buffalo;
		bbrawler_gaia.animation_walk = ActorAnimationSequences.walk_0;
		bbrawler_gaia.animation_idle = ActorAnimationSequences.walk_0;
		bbrawler_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		bbrawler_gaia.addTrait("boat");
		bbrawler_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(bbrawler_gaia);
		Localization.addLocalization(bbrawler_gaia.name_locale, bbrawler_gaia.name_locale);

			var cbrawler_gaia = AssetManager.actor_library.clone("cbrawler_gaia","$boat$");
	    cbrawler_gaia.id = "cbrawler_gaia";
	    cbrawler_gaia.can_be_inspected = false;
		cbrawler_gaia.boat_type = "cbrawler_gaia_boat";
		cbrawler_gaia.name_locale = "Destroyer Ship";
		cbrawler_gaia.addDecision("random_swim");
		cbrawler_gaia.has_avatar_prefab = false;
		cbrawler_gaia.animation_speed_based_on_walk_speed = false;
		cbrawler_gaia.can_flip = true;
        cbrawler_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    cbrawler_gaia.is_boat = true;
		cbrawler_gaia.die_in_lava = false;
		cbrawler_gaia.has_override_sprite = false;
	    cbrawler_gaia.has_override_avatar_frames = false;
		cbrawler_gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		cbrawler_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		cbrawler_gaia.base_stats["scale"] = 0.35f;
		cbrawler_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		cbrawler_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		cbrawler_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		cbrawler_gaia.base_stats["attack_speed"] = 4f;
		cbrawler_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		cbrawler_gaia.base_stats["knockback"] = 0f;
		cbrawler_gaia.base_stats["accuracy"] = 1f;
		cbrawler_gaia.base_stats["targets"] = 5f;
		cbrawler_gaia.base_stats["area_of_effect"] = 4f;
		cbrawler_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		cbrawler_gaia.inspect_avatar_scale = 1f;
		cbrawler_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        cbrawler_gaia.sound_spawn = null;
		cbrawler_gaia.sound_idle_loop = null;
		cbrawler_gaia.sound_death = null;
		cbrawler_gaia.default_attack = "mountedmachinegun";
		cbrawler_gaia.icon = "iconBoat";
		cbrawler_gaia.shadow_texture = "unitShadow_6";
		cbrawler_gaia.cost = new ConstructionCost(0, 0, 0, 1);
		cbrawler_gaia.texture_asset = new ActorTextureSubAsset("actors/Brawler_gaia/", false);
		cbrawler_gaia.special = true;
		cbrawler_gaia.has_advanced_textures = false;
		cbrawler_gaia.draw_boat_mark = true;
		cbrawler_gaia.actor_size = ActorSize.S16_Buffalo;
		cbrawler_gaia.animation_walk = ActorAnimationSequences.walk_0;
		cbrawler_gaia.animation_idle = ActorAnimationSequences.walk_0;
		cbrawler_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		cbrawler_gaia.addTrait("boat");
		cbrawler_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(cbrawler_gaia);
		Localization.addLocalization(cbrawler_gaia.name_locale, cbrawler_gaia.name_locale);

			var dbrawler_gaia = AssetManager.actor_library.clone("dbrawler_gaia","$boat$");
	    dbrawler_gaia.id = "dbrawler_gaia";
	    dbrawler_gaia.can_be_inspected = false;
		dbrawler_gaia.boat_type = "dbrawler_gaia_boat";
		dbrawler_gaia.name_locale = "Destroyer Ship";
		dbrawler_gaia.addDecision("random_swim");
		dbrawler_gaia.has_avatar_prefab = false;
		dbrawler_gaia.animation_speed_based_on_walk_speed = false;
		dbrawler_gaia.can_flip = true;
        dbrawler_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    dbrawler_gaia.is_boat = true;
		dbrawler_gaia.die_in_lava = false;
		dbrawler_gaia.has_override_sprite = false;
	    dbrawler_gaia.has_override_avatar_frames = false;
		dbrawler_gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		dbrawler_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		dbrawler_gaia.base_stats["scale"] = 0.35f;
		dbrawler_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		dbrawler_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		dbrawler_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		dbrawler_gaia.base_stats["attack_speed"] = 4f;
		dbrawler_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		dbrawler_gaia.base_stats["knockback"] = 0f;
		dbrawler_gaia.base_stats["accuracy"] = 1f;
		dbrawler_gaia.base_stats["targets"] = 5f;
		dbrawler_gaia.base_stats["area_of_effect"] = 4f;
		dbrawler_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		dbrawler_gaia.inspect_avatar_scale = 1f;
		dbrawler_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        dbrawler_gaia.sound_spawn = null;
		dbrawler_gaia.sound_idle_loop = null;
		dbrawler_gaia.sound_death = null;
		dbrawler_gaia.default_attack = "mountedmachinegun";
		dbrawler_gaia.icon = "iconBoat";
		dbrawler_gaia.shadow_texture = "unitShadow_6";
		dbrawler_gaia.cost = new ConstructionCost(0, 0, 0, 1);
		dbrawler_gaia.texture_asset = new ActorTextureSubAsset("actors/Brawler_gaia/", false);
		dbrawler_gaia.special = true;
		dbrawler_gaia.has_advanced_textures = false;
		dbrawler_gaia.draw_boat_mark = true;
		dbrawler_gaia.actor_size = ActorSize.S16_Buffalo;
		dbrawler_gaia.animation_walk = ActorAnimationSequences.walk_0;
		dbrawler_gaia.animation_idle = ActorAnimationSequences.walk_0;
		dbrawler_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		dbrawler_gaia.addTrait("boat");
		dbrawler_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(dbrawler_gaia);
		Localization.addLocalization(dbrawler_gaia.name_locale, dbrawler_gaia.name_locale);

			var ebrawler_gaia = AssetManager.actor_library.clone("ebrawler_gaia","$boat$");
	    ebrawler_gaia.id = "ebrawler_gaia";
	    ebrawler_gaia.can_be_inspected = false;
		ebrawler_gaia.boat_type = "ebrawler_gaia_boat";
		ebrawler_gaia.name_locale = "Destroyer Ship";
		ebrawler_gaia.addDecision("random_swim");
		ebrawler_gaia.has_avatar_prefab = false;
		ebrawler_gaia.animation_speed_based_on_walk_speed = false;
		ebrawler_gaia.can_flip = true;
        ebrawler_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    ebrawler_gaia.is_boat = true;
		ebrawler_gaia.die_in_lava = false;
		ebrawler_gaia.has_override_sprite = false;
	    ebrawler_gaia.has_override_avatar_frames = false;
		ebrawler_gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		ebrawler_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		ebrawler_gaia.base_stats["scale"] = 0.35f;
		ebrawler_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		ebrawler_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		ebrawler_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		ebrawler_gaia.base_stats["attack_speed"] = 4f;
		ebrawler_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		ebrawler_gaia.base_stats["knockback"] = 0f;
		ebrawler_gaia.base_stats["accuracy"] = 1f;
		ebrawler_gaia.base_stats["targets"] = 5f;
		ebrawler_gaia.base_stats["area_of_effect"] = 4f;
		ebrawler_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		ebrawler_gaia.inspect_avatar_scale = 1f;
		ebrawler_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        ebrawler_gaia.sound_spawn = null;
		ebrawler_gaia.sound_idle_loop = null;
		ebrawler_gaia.sound_death = null;
		ebrawler_gaia.default_attack = "mountedmachinegun";
		ebrawler_gaia.icon = "iconBoat";
		ebrawler_gaia.shadow_texture = "unitShadow_6";
		ebrawler_gaia.cost = new ConstructionCost(0, 0, 0, 1);
		ebrawler_gaia.texture_asset = new ActorTextureSubAsset("actors/Brawler_gaia/", false);
		ebrawler_gaia.special = true;
		ebrawler_gaia.has_advanced_textures = false;
		ebrawler_gaia.draw_boat_mark = true;
		ebrawler_gaia.actor_size = ActorSize.S16_Buffalo;
		ebrawler_gaia.animation_walk = ActorAnimationSequences.walk_0;
		ebrawler_gaia.animation_idle = ActorAnimationSequences.walk_0;
		ebrawler_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		ebrawler_gaia.addTrait("boat");
		ebrawler_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(ebrawler_gaia);
		Localization.addLocalization(ebrawler_gaia.name_locale, ebrawler_gaia.name_locale);

			var fbrawler_gaia = AssetManager.actor_library.clone("fbrawler_gaia","$boat$");
	    fbrawler_gaia.id = "fbrawler_gaia";
	    fbrawler_gaia.can_be_inspected = false;
		fbrawler_gaia.boat_type = "fbrawler_gaia_boat";
		fbrawler_gaia.name_locale = "Destroyer Ship";
		fbrawler_gaia.addDecision("random_swim");
		fbrawler_gaia.has_avatar_prefab = false;
		fbrawler_gaia.animation_speed_based_on_walk_speed = false;
		fbrawler_gaia.can_flip = true;
        fbrawler_gaia.check_flip = (BaseSimObject _, WorldTile _) => true;
	    fbrawler_gaia.is_boat = true;
		fbrawler_gaia.die_in_lava = false;
		fbrawler_gaia.has_override_sprite = false;
	    fbrawler_gaia.has_override_avatar_frames = false;
		fbrawler_gaia.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		fbrawler_gaia.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		fbrawler_gaia.base_stats["scale"] = 0.35f;
		fbrawler_gaia.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		fbrawler_gaia.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		fbrawler_gaia.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		fbrawler_gaia.base_stats["attack_speed"] = 4f;
		fbrawler_gaia.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		fbrawler_gaia.base_stats["knockback"] = 0f;
		fbrawler_gaia.base_stats["accuracy"] = 1f;
		fbrawler_gaia.base_stats["targets"] = 5f;
		fbrawler_gaia.base_stats["area_of_effect"] = 4f;
		fbrawler_gaia.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		fbrawler_gaia.inspect_avatar_scale = 1f;
		fbrawler_gaia.sound_hit = "event:/SFX/HIT/HitMetal";
        fbrawler_gaia.sound_spawn = null;
		fbrawler_gaia.sound_idle_loop = null;
		fbrawler_gaia.sound_death = null;
		fbrawler_gaia.default_attack = "mountedmachinegun";
		fbrawler_gaia.icon = "iconBoat";
		fbrawler_gaia.shadow_texture = "unitShadow_6";
		fbrawler_gaia.cost = new ConstructionCost(0, 0, 0, 1);
		fbrawler_gaia.texture_asset = new ActorTextureSubAsset("actors/Brawler_gaia/", false);
		fbrawler_gaia.special = true;
		fbrawler_gaia.has_advanced_textures = false;
		fbrawler_gaia.draw_boat_mark = true;
		fbrawler_gaia.actor_size = ActorSize.S16_Buffalo;
		fbrawler_gaia.animation_walk = ActorAnimationSequences.walk_0;
		fbrawler_gaia.animation_idle = ActorAnimationSequences.walk_0;
		fbrawler_gaia.animation_swim = ActorAnimationSequences.swim_0_3;
		fbrawler_gaia.addTrait("boat");
		fbrawler_gaia.addTrait("light_lamp");
		AssetManager.actor_library.add(fbrawler_gaia);
		Localization.addLocalization(fbrawler_gaia.name_locale, fbrawler_gaia.name_locale);



		//////////////////////////////////HARDEN////////////////////////////////

	var CargoShip_harden = AssetManager.actor_library.clone("CargoShip_harden","$boat$");
	    CargoShip_harden.id = "CargoShip_harden";
		CargoShip_harden.boat_type = "cargo_harden_boat";
		CargoShip_harden.can_be_inspected = false;
        CargoShip_harden.skip_fight_logic = true;
		CargoShip_harden.name_locale = "Cargo Ship";
		CargoShip_harden.addDecision("boat_trading");
		CargoShip_harden.has_avatar_prefab = false;
		CargoShip_harden.animation_speed_based_on_walk_speed = false;
		CargoShip_harden.can_flip = true;
        CargoShip_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CargoShip_harden.is_boat = true;
		CargoShip_harden.die_in_lava = false;
		CargoShip_harden.has_override_sprite = false;
	    CargoShip_harden.has_override_avatar_frames = false;
		CargoShip_harden.base_stats["mass_2"] = 3000f;
		CargoShip_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CargoShip_harden.base_stats["scale"] = 0.35f;
		CargoShip_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CargoShip_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CargoShip_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CargoShip_harden.base_stats["attack_speed"] = 0.3f;
		CargoShip_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		CargoShip_harden.base_stats["knockback"] = 2f;
		CargoShip_harden.base_stats["accuracy"] = 0.7f;
		CargoShip_harden.base_stats["targets"] = 1f;
		CargoShip_harden.base_stats["area_of_effect"] = 0.5f;
		CargoShip_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		CargoShip_harden.inspect_avatar_scale = 1f;
		CargoShip_harden.sound_hit = "event:/SFX/HIT/HitMetal";
		CargoShip_harden.sound_spawn = null;
		CargoShip_harden.sound_idle_loop = null;
		CargoShip_harden.sound_death = null;
		CargoShip_harden.default_attack = "boat_cannonball";
		CargoShip_harden.icon = "iconBoat";
		CargoShip_harden.shadow_texture = "unitShadow_6";
		CargoShip_harden.cost = new ConstructionCost(1, 0, 0, 1);
		CargoShip_harden.texture_asset = new ActorTextureSubAsset("actors/CargoShip_harden/", false);
		CargoShip_harden.special = true;
		CargoShip_harden.has_advanced_textures = false;
		CargoShip_harden.draw_boat_mark = true;
		CargoShip_harden.actor_size = ActorSize.S16_Buffalo;
		CargoShip_harden.animation_walk = ActorAnimationSequences.walk_0;
		CargoShip_harden.animation_idle = ActorAnimationSequences.walk_0;
		CargoShip_harden.animation_swim = ActorAnimationSequences.swim_0_2;
		CargoShip_harden.addTrait("boat");
		CargoShip_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(CargoShip_harden);
		Localization.addLocalization(CargoShip_harden.name_locale, CargoShip_harden.name_locale);


	var Transporter_harden = AssetManager.actor_library.clone("Transporter_harden","$boat$");
	    Transporter_harden.id = "Transporter_harden";
		Transporter_harden.boat_type = "transporter_harden_boat";
		Transporter_harden.can_be_inspected = false;
        Transporter_harden.skip_fight_logic = true;
		Transporter_harden.name_locale = "Cargo Ship";
		Transporter_harden.addDecision("boat_transport_check");
		Transporter_harden.has_avatar_prefab = false;
		Transporter_harden.animation_speed_based_on_walk_speed = false;
		Transporter_harden.can_flip = true;
        Transporter_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Transporter_harden.is_boat = true;
		Transporter_harden.die_in_lava = false;
		Transporter_harden.has_override_sprite = false;
	    Transporter_harden.has_override_avatar_frames = false;
		Transporter_harden.base_stats["mass_2"] = 3000f;
		Transporter_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Transporter_harden.base_stats["scale"] = 0.35f;
		Transporter_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		Transporter_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		Transporter_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Transporter_harden.base_stats["attack_speed"] = 0.3f;
		Transporter_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		Transporter_harden.base_stats["knockback"] = 2f;
		Transporter_harden.base_stats["accuracy"] = 0.7f;
		Transporter_harden.base_stats["targets"] = 1f;
		Transporter_harden.base_stats["area_of_effect"] = 0.5f;
		Transporter_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		Transporter_harden.inspect_avatar_scale = 1f;
		Transporter_harden.sound_hit = "event:/SFX/HIT/HitMetal";
		Transporter_harden.sound_spawn = null;
		Transporter_harden.sound_idle_loop = null;
		Transporter_harden.sound_death = null;
		Transporter_harden.default_attack = "boat_cannonball";
		Transporter_harden.icon = "iconBoat";
		Transporter_harden.shadow_texture = "unitShadow_6";
		Transporter_harden.cost = new ConstructionCost(0, 0, 0, 0);
		Transporter_harden.texture_asset = new ActorTextureSubAsset("actors/Transporter_harden/", false);
		Transporter_harden.special = true;
		Transporter_harden.has_advanced_textures = false;
		Transporter_harden.draw_boat_mark = true;
		Transporter_harden.actor_size = ActorSize.S16_Buffalo;
		Transporter_harden.animation_walk = ActorAnimationSequences.walk_0;
		Transporter_harden.animation_idle = ActorAnimationSequences.walk_0;
		Transporter_harden.animation_swim = ActorAnimationSequences.swim_0_2;
		Transporter_harden.addTrait("boat");
		Transporter_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(Transporter_harden);
		Localization.addLocalization(Transporter_harden.name_locale, Transporter_harden.name_locale);

	var aDestroyer_harden = AssetManager.actor_library.clone("aDestroyer_harden","$boat$");
	    aDestroyer_harden.id = "aDestroyer_harden";
	    aDestroyer_harden.can_be_inspected = true;
		aDestroyer_harden.boat_type = "destroyer_a_harden_boat";
		aDestroyer_harden.name_locale = "Destroyer Ship";
		aDestroyer_harden.addDecision("warBoatAttackDecision");
		aDestroyer_harden.has_avatar_prefab = false;
		aDestroyer_harden.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyerharden_avatar") };
aDestroyer_harden.has_override_avatar_frames = true;
aDestroyer_harden.inspect_avatar_scale = 1f;
aDestroyer_harden.inspect_avatar_offset_y = 6f;
		aDestroyer_harden.animation_speed_based_on_walk_speed = false;
		aDestroyer_harden.can_flip = true;
        aDestroyer_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    aDestroyer_harden.is_boat = true;
		aDestroyer_harden.die_in_lava = false;
		aDestroyer_harden.has_override_sprite = false;
		aDestroyer_harden.base_stats["mass_2"] = 3000f;
		aDestroyer_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		aDestroyer_harden.base_stats["scale"] = 0.35f;
		aDestroyer_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		aDestroyer_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		aDestroyer_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		aDestroyer_harden.base_stats["attack_speed"] = 0.3f;
		aDestroyer_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		aDestroyer_harden.base_stats["knockback"] = 2f;
		aDestroyer_harden.base_stats["accuracy"] = 0.7f;
		aDestroyer_harden.base_stats["targets"] = 1f;
		aDestroyer_harden.base_stats["area_of_effect"] = 0.5f;
		aDestroyer_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		aDestroyer_harden.inspect_avatar_scale = 1f;
		aDestroyer_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        aDestroyer_harden.sound_spawn = null;
		aDestroyer_harden.sound_idle_loop = null;
		aDestroyer_harden.sound_death = null;
		aDestroyer_harden.default_attack = "fighterattackHarden";
		aDestroyer_harden.icon = "iconBoat";
		aDestroyer_harden.shadow_texture = "unitShadow_6";
		aDestroyer_harden.cost = new ConstructionCost(1, 0, 0, 1);
		aDestroyer_harden.texture_asset = new ActorTextureSubAsset("actors/Destroyer_harden/", false);
		aDestroyer_harden.special = true;
		aDestroyer_harden.has_advanced_textures = false;
		aDestroyer_harden.draw_boat_mark = true;
		aDestroyer_harden.actor_size = ActorSize.S16_Buffalo;
		aDestroyer_harden.animation_walk = ActorAnimationSequences.walk_0;
		aDestroyer_harden.animation_idle = ActorAnimationSequences.walk_0;
		aDestroyer_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		aDestroyer_harden.addTrait("boat");
		aDestroyer_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(aDestroyer_harden);
		Localization.addLocalization(aDestroyer_harden.name_locale, aDestroyer_harden.name_locale);

	var bDestroyer_harden = AssetManager.actor_library.clone("bDestroyer_harden","$boat$");
	    bDestroyer_harden.id = "bDestroyer_harden";
		bDestroyer_harden.boat_type = "destroyer_b_harden_boat";
		bDestroyer_harden.can_be_inspected = true;
		bDestroyer_harden.name_locale = "Destroyer Ship";
		bDestroyer_harden.addDecision("warBoatAttackDecision");
		bDestroyer_harden.has_avatar_prefab = false;
bDestroyer_harden.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Destroyerharden_avatar") };
bDestroyer_harden.has_override_avatar_frames = true;
bDestroyer_harden.inspect_avatar_scale = 4f;
bDestroyer_harden.inspect_avatar_offset_y = 6f;
		bDestroyer_harden.animation_speed_based_on_walk_speed = false;
		bDestroyer_harden.can_flip = true;
        bDestroyer_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bDestroyer_harden.is_boat = true;
		bDestroyer_harden.die_in_lava = false;
		bDestroyer_harden.has_override_sprite = false;
		bDestroyer_harden.base_stats["mass_2"] = 3000f;
		bDestroyer_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bDestroyer_harden.base_stats["scale"] = 0.35f;
		bDestroyer_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		bDestroyer_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		bDestroyer_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		bDestroyer_harden.base_stats["attack_speed"] = 0.3f;
		bDestroyer_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		bDestroyer_harden.base_stats["knockback"] = 2f;
		bDestroyer_harden.base_stats["accuracy"] = 0.7f;
		bDestroyer_harden.base_stats["targets"] = 1f;
		bDestroyer_harden.base_stats["area_of_effect"] = 0.5f;
		bDestroyer_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		bDestroyer_harden.inspect_avatar_scale = 1f;
		bDestroyer_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        bDestroyer_harden.sound_spawn = null;
		bDestroyer_harden.sound_idle_loop = null;
		bDestroyer_harden.sound_death = null;
		bDestroyer_harden.default_attack = "fighterattackHarden";
		bDestroyer_harden.icon = "iconBoat";
		bDestroyer_harden.shadow_texture = "unitShadow_6";
		bDestroyer_harden.cost = new ConstructionCost(1, 0, 0, 1);
		bDestroyer_harden.texture_asset = new ActorTextureSubAsset("actors/Destroyer_harden/", false);
		bDestroyer_harden.special = true;
		bDestroyer_harden.has_advanced_textures = false;
		bDestroyer_harden.draw_boat_mark = true;
		bDestroyer_harden.actor_size = ActorSize.S16_Buffalo;
		bDestroyer_harden.animation_walk = ActorAnimationSequences.walk_0;
		bDestroyer_harden.animation_idle = ActorAnimationSequences.walk_0;
		bDestroyer_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		bDestroyer_harden.addTrait("boat");
		bDestroyer_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(bDestroyer_harden);
		Localization.addLocalization(bDestroyer_harden.name_locale, bDestroyer_harden.name_locale);

        ///////jet attack for carrier/no spawn

	var CarrierVessel_harden = AssetManager.actor_library.clone("CarrierVessel_harden","$boat$");
	    CarrierVessel_harden.id = "CarrierVessel_harden";
		CarrierVessel_harden.boat_type = "carrier_harden_boat";
		CarrierVessel_harden.name_locale = "Cargo Ship";
		CarrierVessel_harden.can_be_inspected = true;
		CarrierVessel_harden.addDecision("warBoatAttackDecision");
		CarrierVessel_harden.has_avatar_prefab = false;
CarrierVessel_harden.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Carrierharden_avatar") };
CarrierVessel_harden.has_override_avatar_frames = true;
CarrierVessel_harden.inspect_avatar_scale = 4f;
CarrierVessel_harden.inspect_avatar_offset_y = 6f;
		CarrierVessel_harden.animation_speed_based_on_walk_speed = false;
		CarrierVessel_harden.can_flip = true;
        CarrierVessel_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    CarrierVessel_harden.is_boat = true;
		CarrierVessel_harden.die_in_lava = false;
		CarrierVessel_harden.has_override_sprite = false;
		CarrierVessel_harden.base_stats["mass_2"] = 3000f;
		CarrierVessel_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		CarrierVessel_harden.base_stats["scale"] = 0.35f;
		CarrierVessel_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		CarrierVessel_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		CarrierVessel_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		CarrierVessel_harden.base_stats["attack_speed"] = 0.3f;
		CarrierVessel_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		CarrierVessel_harden.base_stats["knockback"] = 2f;
		CarrierVessel_harden.base_stats["accuracy"] = 0.7f;
		CarrierVessel_harden.base_stats["targets"] = 1f;
		CarrierVessel_harden.base_stats["area_of_effect"] = 0.5f;
		CarrierVessel_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(16f);
		CarrierVessel_harden.inspect_avatar_scale = 1f;
		CarrierVessel_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        CarrierVessel_harden.sound_spawn = null;
		CarrierVessel_harden.sound_idle_loop = null;
		CarrierVessel_harden.sound_death = null;
		CarrierVessel_harden.default_attack = "AirstrikejetAttack_harden";
		CarrierVessel_harden.icon = "iconBoat";
		CarrierVessel_harden.shadow_texture = "unitShadow_6";
		CarrierVessel_harden.cost = new ConstructionCost(1, 0, 0, 1);
		CarrierVessel_harden.texture_asset = new ActorTextureSubAsset("actors/CarrierVessel_harden/", false);
		CarrierVessel_harden.special = true;
		CarrierVessel_harden.has_advanced_textures = false;
		CarrierVessel_harden.draw_boat_mark = true;
		CarrierVessel_harden.actor_size = ActorSize.S16_Buffalo;
		CarrierVessel_harden.animation_walk = ActorAnimationSequences.walk_0;
		CarrierVessel_harden.animation_idle = ActorAnimationSequences.walk_0;
		CarrierVessel_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		CarrierVessel_harden.addTrait("boat");
		CarrierVessel_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(CarrierVessel_harden);
		Localization.addLocalization(CarrierVessel_harden.name_locale, CarrierVessel_harden.name_locale);

	var Submarine_harden = AssetManager.actor_library.clone("Submarine_harden","$boat$");
	    Submarine_harden.id = "Submarine_harden";
		Submarine_harden.boat_type = "submarine_harden_boat";
		Submarine_harden.name_locale = "Cargo Ship";
		Submarine_harden.can_be_inspected = true;
		Submarine_harden.addDecision("HARDENmissileArtilleryDecision");
		Submarine_harden.addDecision("nuclearmissileDecision");
		Submarine_harden.addDecision("AntiBossNukeDecision");
		Submarine_harden.addDecision("random_swim");
		Submarine_harden.has_avatar_prefab = false;
Submarine_harden.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Subharden_avatar") };
Submarine_harden.has_override_avatar_frames = true;
Submarine_harden.inspect_avatar_scale = 4f;
Submarine_harden.inspect_avatar_offset_y = 6f;
		Submarine_harden.animation_speed_based_on_walk_speed = false;
		Submarine_harden.can_flip = true;
        Submarine_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    Submarine_harden.is_boat = true;
		Submarine_harden.die_in_lava = false;
		Submarine_harden.has_override_sprite = false;
		Submarine_harden.base_stats["mass_2"] = 3000f;
		Submarine_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		Submarine_harden.base_stats["scale"] = 0.35f;
		Submarine_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		Submarine_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		Submarine_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		Submarine_harden.base_stats["attack_speed"] = 0.3f;
		Submarine_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		Submarine_harden.base_stats["knockback"] = 2f;
		Submarine_harden.base_stats["accuracy"] = 0.7f;
		Submarine_harden.base_stats["targets"] = 1f;
		Submarine_harden.base_stats["area_of_effect"] = 0.5f;
		Submarine_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(200f);
		Submarine_harden.inspect_avatar_scale = 1f;
		Submarine_harden.sound_hit = "event:/SFX/HIT/HitMetal";
		Submarine_harden.sound_spawn = null;
		Submarine_harden.sound_idle_loop = null;
		Submarine_harden.sound_death = null;
		Submarine_harden.default_attack = "MissileSystemGaia";
		Submarine_harden.icon = "iconBoat";
		Submarine_harden.shadow_texture = "unitShadow_6";
		Submarine_harden.cost = new ConstructionCost(1, 0, 0, 1);
		Submarine_harden.texture_asset = new ActorTextureSubAsset("actors/Submarine_harden/", false);
		Submarine_harden.special = true;
		Submarine_harden.has_advanced_textures = false;
		Submarine_harden.draw_boat_mark = true;
		Submarine_harden.actor_size = ActorSize.S16_Buffalo;
		Submarine_harden.animation_walk = ActorAnimationSequences.walk_0;
		Submarine_harden.animation_idle = ActorAnimationSequences.walk_0;
		Submarine_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		Submarine_harden.addTrait("boat");
		Submarine_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(Submarine_harden);
		Localization.addLocalization(Submarine_harden.name_locale, Submarine_harden.name_locale);

	var FishingBoat_harden = AssetManager.actor_library.clone("FishingBoat_harden","$boat$");
	    FishingBoat_harden.id = "FishingBoat_harden";
		FishingBoat_harden.boat_type = "fishing_harden_boat";
        FishingBoat_harden.skip_fight_logic = true;
        FishingBoat_harden.can_be_inspected = false;
		FishingBoat_harden.name_locale = "Cargo Ship";
		FishingBoat_harden.addDecision("boat_fishing");
		FishingBoat_harden.has_avatar_prefab = false;
		FishingBoat_harden.animation_speed_based_on_walk_speed = false;
		FishingBoat_harden.can_flip = true;
        FishingBoat_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    FishingBoat_harden.is_boat = true;
		FishingBoat_harden.die_in_lava = false;
		FishingBoat_harden.has_override_sprite = false;
	    FishingBoat_harden.has_override_avatar_frames = false;
		FishingBoat_harden.base_stats["mass_2"] = 3000f;
		FishingBoat_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		FishingBoat_harden.base_stats["scale"] = 0.35f;
		FishingBoat_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		FishingBoat_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		FishingBoat_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(30f);
		FishingBoat_harden.base_stats["attack_speed"] = 0.3f;
		FishingBoat_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		FishingBoat_harden.base_stats["knockback"] = 2f;
		FishingBoat_harden.base_stats["accuracy"] = 0.7f;
		FishingBoat_harden.base_stats["targets"] = 1f;
		FishingBoat_harden.base_stats["area_of_effect"] = 0.5f;
		FishingBoat_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(6f);
		FishingBoat_harden.inspect_avatar_scale = 1f;
		FishingBoat_harden.sound_hit = "event:/SFX/HIT/HitMetal";
		FishingBoat_harden.sound_spawn = null;
		FishingBoat_harden.sound_idle_loop = null;
		FishingBoat_harden.sound_death = null;
		FishingBoat_harden.default_attack = "boat_cannonball";
		FishingBoat_harden.icon = "iconBoat";
		FishingBoat_harden.shadow_texture = "unitShadow_6";
		FishingBoat_harden.cost = new ConstructionCost(1, 0, 0, 1);
		FishingBoat_harden.texture_asset = new ActorTextureSubAsset("actors/FishingBoat_harden/", false);
		FishingBoat_harden.special = true;
		FishingBoat_harden.has_advanced_textures = false;
		FishingBoat_harden.draw_boat_mark = true;
		FishingBoat_harden.actor_size = ActorSize.S16_Buffalo;
		FishingBoat_harden.animation_walk = ActorAnimationSequences.walk_0;
		FishingBoat_harden.animation_idle = ActorAnimationSequences.walk_0;
		FishingBoat_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		FishingBoat_harden.addTrait("boat");
		FishingBoat_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(FishingBoat_harden);
		Localization.addLocalization(FishingBoat_harden.name_locale, FishingBoat_harden.name_locale);


	var abrawler_harden = AssetManager.actor_library.clone("abrawler_harden","$boat$");
	    abrawler_harden.id = "abrawler_harden";
	    abrawler_harden.can_be_inspected = false;
		abrawler_harden.boat_type = "abrawler_harden_boat";
		abrawler_harden.name_locale = "Destroyer Ship";
		abrawler_harden.addDecision("random_swim");
		abrawler_harden.has_avatar_prefab = false;
		abrawler_harden.animation_speed_based_on_walk_speed = false;
		abrawler_harden.can_flip = true;
        abrawler_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    abrawler_harden.is_boat = true;
		abrawler_harden.die_in_lava = false;
		abrawler_harden.has_override_sprite = false;
	    abrawler_harden.has_override_avatar_frames = false;
		abrawler_harden.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		abrawler_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		abrawler_harden.base_stats["scale"] = 0.35f;
		abrawler_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		abrawler_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		abrawler_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		abrawler_harden.base_stats["attack_speed"] = 4f;
		abrawler_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		abrawler_harden.base_stats["knockback"] = 0f;
		abrawler_harden.base_stats["accuracy"] = 1f;
		abrawler_harden.base_stats["targets"] = 5f;
		abrawler_harden.base_stats["area_of_effect"] = 4f;
		abrawler_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		abrawler_harden.inspect_avatar_scale = 1f;
		abrawler_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        abrawler_harden.sound_spawn = null;
		abrawler_harden.sound_idle_loop = null;
		abrawler_harden.sound_death = null;
		abrawler_harden.default_attack = "mountedmachinegun";
		abrawler_harden.icon = "iconBoat";
		abrawler_harden.shadow_texture = "unitShadow_6";
		abrawler_harden.cost = new ConstructionCost(0, 0, 0, 1);
		abrawler_harden.texture_asset = new ActorTextureSubAsset("actors/Brawler_harden/", false);
		abrawler_harden.special = true;
		abrawler_harden.has_advanced_textures = false;
		abrawler_harden.draw_boat_mark = true;
		abrawler_harden.actor_size = ActorSize.S16_Buffalo;
		abrawler_harden.animation_walk = ActorAnimationSequences.walk_0;
		abrawler_harden.animation_idle = ActorAnimationSequences.walk_0;
		abrawler_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		abrawler_harden.addTrait("boat");
		abrawler_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(abrawler_harden);
		Localization.addLocalization(abrawler_harden.name_locale, abrawler_harden.name_locale);

		var bbrawler_harden = AssetManager.actor_library.clone("bbrawler_harden","$boat$");
	    bbrawler_harden.id = "bbrawler_harden";
	    bbrawler_harden.can_be_inspected = false;
		bbrawler_harden.boat_type = "bbrawler_harden_boat";
		bbrawler_harden.name_locale = "Destroyer Ship";
		bbrawler_harden.addDecision("random_swim");
		bbrawler_harden.has_avatar_prefab = false;
		bbrawler_harden.animation_speed_based_on_walk_speed = false;
		bbrawler_harden.can_flip = true;
        bbrawler_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    bbrawler_harden.is_boat = true;
		bbrawler_harden.die_in_lava = false;
		bbrawler_harden.has_override_sprite = false;
	    bbrawler_harden.has_override_avatar_frames = false;
		bbrawler_harden.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		bbrawler_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		bbrawler_harden.base_stats["scale"] = 0.35f;
		bbrawler_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		bbrawler_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		bbrawler_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		bbrawler_harden.base_stats["attack_speed"] = 4f;
		bbrawler_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		bbrawler_harden.base_stats["knockback"] = 0f;
		bbrawler_harden.base_stats["accuracy"] = 1f;
		bbrawler_harden.base_stats["targets"] = 5f;
		bbrawler_harden.base_stats["area_of_effect"] = 4f;
		bbrawler_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		bbrawler_harden.inspect_avatar_scale = 1f;
		bbrawler_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        bbrawler_harden.sound_spawn = null;
		bbrawler_harden.sound_idle_loop = null;
		bbrawler_harden.sound_death = null;
		bbrawler_harden.default_attack = "mountedmachinegun";
		bbrawler_harden.icon = "iconBoat";
		bbrawler_harden.shadow_texture = "unitShadow_6";
		bbrawler_harden.cost = new ConstructionCost(0, 0, 0, 1);
		bbrawler_harden.texture_asset = new ActorTextureSubAsset("actors/Brawler_harden/", false);
		bbrawler_harden.special = true;
		bbrawler_harden.has_advanced_textures = false;
		bbrawler_harden.draw_boat_mark = true;
		bbrawler_harden.actor_size = ActorSize.S16_Buffalo;
		bbrawler_harden.animation_walk = ActorAnimationSequences.walk_0;
		bbrawler_harden.animation_idle = ActorAnimationSequences.walk_0;
		bbrawler_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		bbrawler_harden.addTrait("boat");
		bbrawler_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(bbrawler_harden);
		Localization.addLocalization(bbrawler_harden.name_locale, bbrawler_harden.name_locale);

			var cbrawler_harden = AssetManager.actor_library.clone("cbrawler_harden","$boat$");
	    cbrawler_harden.id = "cbrawler_harden";
	    cbrawler_harden.can_be_inspected = false;
		cbrawler_harden.boat_type = "cbrawler_harden_boat";
		cbrawler_harden.name_locale = "Destroyer Ship";
		cbrawler_harden.addDecision("random_swim");
		cbrawler_harden.has_avatar_prefab = false;
		cbrawler_harden.animation_speed_based_on_walk_speed = false;
		cbrawler_harden.can_flip = true;
        cbrawler_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    cbrawler_harden.is_boat = true;
		cbrawler_harden.die_in_lava = false;
		cbrawler_harden.has_override_sprite = false;
	    cbrawler_harden.has_override_avatar_frames = false;
		cbrawler_harden.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		cbrawler_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		cbrawler_harden.base_stats["scale"] = 0.35f;
		cbrawler_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		cbrawler_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		cbrawler_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		cbrawler_harden.base_stats["attack_speed"] = 4f;
		cbrawler_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		cbrawler_harden.base_stats["knockback"] = 0f;
		cbrawler_harden.base_stats["accuracy"] = 1f;
		cbrawler_harden.base_stats["targets"] = 5f;
		cbrawler_harden.base_stats["area_of_effect"] = 4f;
		cbrawler_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		cbrawler_harden.inspect_avatar_scale = 1f;
		cbrawler_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        cbrawler_harden.sound_spawn = null;
		cbrawler_harden.sound_idle_loop = null;
		cbrawler_harden.sound_death = null;
		cbrawler_harden.default_attack = "mountedmachinegun";
		cbrawler_harden.icon = "iconBoat";
		cbrawler_harden.shadow_texture = "unitShadow_6";
		cbrawler_harden.cost = new ConstructionCost(0, 0, 0, 1);
		cbrawler_harden.texture_asset = new ActorTextureSubAsset("actors/Brawler_harden/", false);
		cbrawler_harden.special = true;
		cbrawler_harden.has_advanced_textures = false;
		cbrawler_harden.draw_boat_mark = true;
		cbrawler_harden.actor_size = ActorSize.S16_Buffalo;
		cbrawler_harden.animation_walk = ActorAnimationSequences.walk_0;
		cbrawler_harden.animation_idle = ActorAnimationSequences.walk_0;
		cbrawler_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		cbrawler_harden.addTrait("boat");
		cbrawler_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(cbrawler_harden);
		Localization.addLocalization(cbrawler_harden.name_locale, cbrawler_harden.name_locale);

			var dbrawler_harden = AssetManager.actor_library.clone("dbrawler_harden","$boat$");
	    dbrawler_harden.id = "dbrawler_harden";
	    dbrawler_harden.can_be_inspected = false;
		dbrawler_harden.boat_type = "dbrawler_harden_boat";
		dbrawler_harden.name_locale = "Destroyer Ship";
		dbrawler_harden.addDecision("random_swim");
		dbrawler_harden.has_avatar_prefab = false;
		dbrawler_harden.animation_speed_based_on_walk_speed = false;
		dbrawler_harden.can_flip = true;
        dbrawler_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    dbrawler_harden.is_boat = true;
		dbrawler_harden.die_in_lava = false;
		dbrawler_harden.has_override_sprite = false;
	    dbrawler_harden.has_override_avatar_frames = false;
		dbrawler_harden.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		dbrawler_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		dbrawler_harden.base_stats["scale"] = 0.35f;
		dbrawler_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		dbrawler_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		dbrawler_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		dbrawler_harden.base_stats["attack_speed"] = 4f;
		dbrawler_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		dbrawler_harden.base_stats["knockback"] = 0f;
		dbrawler_harden.base_stats["accuracy"] = 1f;
		dbrawler_harden.base_stats["targets"] = 5f;
		dbrawler_harden.base_stats["area_of_effect"] = 4f;
		dbrawler_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		dbrawler_harden.inspect_avatar_scale = 1f;
		dbrawler_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        dbrawler_harden.sound_spawn = null;
		dbrawler_harden.sound_idle_loop = null;
		dbrawler_harden.sound_death = null;
		dbrawler_harden.default_attack = "mountedmachinegun";
		dbrawler_harden.icon = "iconBoat";
		dbrawler_harden.shadow_texture = "unitShadow_6";
		dbrawler_harden.cost = new ConstructionCost(0, 0, 0, 1);
		dbrawler_harden.texture_asset = new ActorTextureSubAsset("actors/Brawler_harden/", false);
		dbrawler_harden.special = true;
		dbrawler_harden.has_advanced_textures = false;
		dbrawler_harden.draw_boat_mark = true;
		dbrawler_harden.actor_size = ActorSize.S16_Buffalo;
		dbrawler_harden.animation_walk = ActorAnimationSequences.walk_0;
		dbrawler_harden.animation_idle = ActorAnimationSequences.walk_0;
		dbrawler_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		dbrawler_harden.addTrait("boat");
		dbrawler_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(dbrawler_harden);
		Localization.addLocalization(dbrawler_harden.name_locale, dbrawler_harden.name_locale);

			var ebrawler_harden = AssetManager.actor_library.clone("ebrawler_harden","$boat$");
	    ebrawler_harden.id = "ebrawler_harden";
	    ebrawler_harden.can_be_inspected = false;
		ebrawler_harden.boat_type = "ebrawler_harden_boat";
		ebrawler_harden.name_locale = "Destroyer Ship";
		ebrawler_harden.addDecision("random_swim");
		ebrawler_harden.has_avatar_prefab = false;
		ebrawler_harden.animation_speed_based_on_walk_speed = false;
		ebrawler_harden.can_flip = true;
        ebrawler_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    ebrawler_harden.is_boat = true;
		ebrawler_harden.die_in_lava = false;
		ebrawler_harden.has_override_sprite = false;
	    ebrawler_harden.has_override_avatar_frames = false;
		ebrawler_harden.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		ebrawler_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		ebrawler_harden.base_stats["scale"] = 0.35f;
		ebrawler_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		ebrawler_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		ebrawler_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		ebrawler_harden.base_stats["attack_speed"] = 4f;
		ebrawler_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		ebrawler_harden.base_stats["knockback"] = 0f;
		ebrawler_harden.base_stats["accuracy"] = 1f;
		ebrawler_harden.base_stats["targets"] = 5f;
		ebrawler_harden.base_stats["area_of_effect"] = 4f;
		ebrawler_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		ebrawler_harden.inspect_avatar_scale = 1f;
		ebrawler_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        ebrawler_harden.sound_spawn = null;
		ebrawler_harden.sound_idle_loop = null;
		ebrawler_harden.sound_death = null;
		ebrawler_harden.default_attack = "mountedmachinegun";
		ebrawler_harden.icon = "iconBoat";
		ebrawler_harden.shadow_texture = "unitShadow_6";
		ebrawler_harden.cost = new ConstructionCost(0, 0, 0, 1);
		ebrawler_harden.texture_asset = new ActorTextureSubAsset("actors/Brawler_harden/", false);
		ebrawler_harden.special = true;
		ebrawler_harden.has_advanced_textures = false;
		ebrawler_harden.draw_boat_mark = true;
		ebrawler_harden.actor_size = ActorSize.S16_Buffalo;
		ebrawler_harden.animation_walk = ActorAnimationSequences.walk_0;
		ebrawler_harden.animation_idle = ActorAnimationSequences.walk_0;
		ebrawler_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		ebrawler_harden.addTrait("boat");
		ebrawler_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(ebrawler_harden);
		Localization.addLocalization(ebrawler_harden.name_locale, ebrawler_harden.name_locale);

			var fbrawler_harden = AssetManager.actor_library.clone("fbrawler_harden","$boat$");
	    fbrawler_harden.id = "fbrawler_harden";
	    fbrawler_harden.can_be_inspected = false;
		fbrawler_harden.boat_type = "fbrawler_harden_boat";
		fbrawler_harden.name_locale = "Destroyer Ship";
		fbrawler_harden.addDecision("random_swim");
		fbrawler_harden.has_avatar_prefab = false;
		fbrawler_harden.animation_speed_based_on_walk_speed = false;
		fbrawler_harden.can_flip = true;
        fbrawler_harden.check_flip = (BaseSimObject _, WorldTile _) => true;
	    fbrawler_harden.is_boat = true;
		fbrawler_harden.die_in_lava = false;
		fbrawler_harden.has_override_sprite = false;
	    fbrawler_harden.has_override_avatar_frames = false;
		fbrawler_harden.base_stats["mass_2"] = ModernBoxPrefs.ScaleBalance(100f);
		fbrawler_harden.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		fbrawler_harden.base_stats["scale"] = 0.35f;
		fbrawler_harden.base_stats["health"] = ModernBoxPrefs.ScaleBalance(150f);
		fbrawler_harden.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		fbrawler_harden.base_stats["armor"] = ModernBoxPrefs.ScaleBalance(10f);
		fbrawler_harden.base_stats["attack_speed"] = 4f;
		fbrawler_harden.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		fbrawler_harden.base_stats["knockback"] = 0f;
		fbrawler_harden.base_stats["accuracy"] = 1f;
		fbrawler_harden.base_stats["targets"] = 5f;
		fbrawler_harden.base_stats["area_of_effect"] = 4f;
		fbrawler_harden.base_stats["range"] = ModernBoxPrefs.ScaleBalance(5f);
		fbrawler_harden.inspect_avatar_scale = 1f;
		fbrawler_harden.sound_hit = "event:/SFX/HIT/HitMetal";
        fbrawler_harden.sound_spawn = null;
		fbrawler_harden.sound_idle_loop = null;
		fbrawler_harden.sound_death = null;
		fbrawler_harden.default_attack = "mountedmachinegun";
		fbrawler_harden.icon = "iconBoat";
		fbrawler_harden.shadow_texture = "unitShadow_6";
		fbrawler_harden.cost = new ConstructionCost(0, 0, 0, 1);
		fbrawler_harden.texture_asset = new ActorTextureSubAsset("actors/Brawler_harden/", false);
		fbrawler_harden.special = true;
		fbrawler_harden.has_advanced_textures = false;
		fbrawler_harden.draw_boat_mark = true;
		fbrawler_harden.actor_size = ActorSize.S16_Buffalo;
		fbrawler_harden.animation_walk = ActorAnimationSequences.walk_0;
		fbrawler_harden.animation_idle = ActorAnimationSequences.walk_0;
		fbrawler_harden.animation_swim = ActorAnimationSequences.swim_0_3;
		fbrawler_harden.addTrait("boat");
		fbrawler_harden.addTrait("light_lamp");
		AssetManager.actor_library.add(fbrawler_harden);
		Localization.addLocalization(fbrawler_harden.name_locale, fbrawler_harden.name_locale);






		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		///////////////////////////////////FUTURE EPOCH UNITS YAAAAAAAAAAAAAAY//////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


		var SpaceMarine = AssetManager.actor_library.clone("SpaceMarine","baseWarUnit");
		SpaceMarine.die_in_lava = false;
		SpaceMarine.base_stats["lifespan"] = 500f;
		SpaceMarine.base_stats["mass_2"] = 600f;
		SpaceMarine.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		SpaceMarine.base_stats["scale"] = 0.14f;
		SpaceMarine.base_stats["size"] = 1f;
		SpaceMarine.base_stats["mass"] = 1000f;
		SpaceMarine.base_stats["health"] = ModernBoxPrefs.ScaleBalance(500f);
		SpaceMarine.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		SpaceMarine.base_stats["armor"] = 40f;
		SpaceMarine.base_stats["attack_speed"] = 2f;
		SpaceMarine.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		SpaceMarine.base_stats["knockback"] = 0.2f;
		SpaceMarine.base_stats["accuracy"] = 0.8f;
		SpaceMarine.base_stats["targets"] = 1f;
		SpaceMarine.base_stats["area_of_effect"] = 0.5f;
		SpaceMarine.base_stats["range"] = ModernBoxPrefs.ScaleBalance(10f);
		SpaceMarine.sound_hit = "event:/SFX/HIT/HitMetal";
		SpaceMarine.default_attack = "mountedmachinegun";
		SpaceMarine.icon = "iconBoat";
		SpaceMarine.shadow_texture = "unitShadow_6";
		SpaceMarine.texture_asset = new ActorTextureSubAsset("actors/SpaceMarine/", false);
		SpaceMarine.special = true;
		SpaceMarine.has_advanced_textures = false;
		SpaceMarine.animation_walk = ActorAnimationSequences.walk_0_3;
		SpaceMarine.animation_idle = ActorAnimationSequences.walk_0;
		SpaceMarine.animation_swim = ActorAnimationSequences.swim_0_3;
		SpaceMarine.name_locale = "Elite Soldier";
		SpaceMarine.addTrait("dodge");
		SpaceMarine.addTrait("dash");
		AssetManager.actor_library.add(SpaceMarine);
		Localization.addLocalization(SpaceMarine.name_locale, SpaceMarine.name_locale);



		var spaceork = AssetManager.actor_library.clone("spaceork","baseWarUnit");
		spaceork.die_in_lava = false;
		spaceork.base_stats["lifespan"] = 500f;
		spaceork.base_stats["mass_2"] = 600f;
		spaceork.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		spaceork.base_stats["scale"] = 0.14f;
		spaceork.base_stats["size"] = 1f;
		spaceork.base_stats["mass"] = 1000f;
		spaceork.base_stats["health"] = ModernBoxPrefs.ScaleBalance(500f);
		spaceork.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		spaceork.base_stats["armor"] = 40f;
		spaceork.base_stats["attack_speed"] = 2f;
		spaceork.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(40f);
		spaceork.base_stats["knockback"] = 0.2f;
		spaceork.base_stats["accuracy"] = 0.8f;
		spaceork.base_stats["targets"] = 1f;
		spaceork.base_stats["area_of_effect"] = 0.5f;
		spaceork.base_stats["range"] = ModernBoxPrefs.ScaleBalance(10f);
		spaceork.sound_hit = "event:/SFX/HIT/HitMetal";
		spaceork.default_attack = "mountedmachinegun";
		spaceork.icon = "iconBoat";
		spaceork.shadow_texture = "unitShadow_6";
		spaceork.texture_asset = new ActorTextureSubAsset("actors/spaceork/", false);
		spaceork.special = true;
		spaceork.has_advanced_textures = false;
		spaceork.animation_walk = ActorAnimationSequences.walk_0_3;
		spaceork.animation_idle = ActorAnimationSequences.walk_0;
		spaceork.animation_swim = ActorAnimationSequences.swim_0_3;
		spaceork.name_locale = "Elite Soldier";
		spaceork.addTrait("dodge");
		spaceork.addTrait("dash");
		AssetManager.actor_library.add(spaceork);
		Localization.addLocalization(spaceork.name_locale, spaceork.name_locale);



		var teslatruckgun = AssetManager.actor_library.clone("teslatruckgun","baseWarUnit");
		teslatruckgun.die_in_lava = false;
		teslatruckgun.base_stats["lifespan"] = 50f;
		teslatruckgun.base_stats["mass_2"] = 200f;
		teslatruckgun.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		teslatruckgun.base_stats["scale"] = 0.17f;
		teslatruckgun.base_stats["size"] = 1f;
		teslatruckgun.base_stats["mass"] = 1000f;
		teslatruckgun.base_stats["health"] = ModernBoxPrefs.ScaleBalance(500f);
		teslatruckgun.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(100f);
		teslatruckgun.base_stats["armor"] = 35f;
		teslatruckgun.base_stats["attack_speed"] = 10000f;
		teslatruckgun.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(15f);
		teslatruckgun.base_stats["knockback"] = 0.01f;
		teslatruckgun.base_stats["accuracy"] = 0.6f;
		teslatruckgun.base_stats["targets"] = 1f;
		teslatruckgun.base_stats["area_of_effect"] = 0.5f;
		teslatruckgun.base_stats["range"] = ModernBoxPrefs.ScaleBalance(14f);
		teslatruckgun.sound_hit = "event:/SFX/HIT/HitMetal";
		teslatruckgun.default_attack = "blueplasmashot";
		teslatruckgun.icon = "iconBoat";
		teslatruckgun.shadow_texture = "unitShadow_6";
		teslatruckgun.texture_asset = new ActorTextureSubAsset("actors/teslatruckgun/", false);
		teslatruckgun.special = true;
		teslatruckgun.has_advanced_textures = false;
		teslatruckgun.animation_walk = ActorAnimationSequences.walk_0_3;
		teslatruckgun.animation_idle = ActorAnimationSequences.walk_0;
		teslatruckgun.animation_swim = ActorAnimationSequences.swim_0_3;
		teslatruckgun.name_locale = "Light Vehicle";
		teslatruckgun.addTrait("dodge");
		teslatruckgun.addTrait("dash");
		teslatruckgun.addTrait("fire_proof");
		AssetManager.actor_library.add(teslatruckgun);
		Localization.addLocalization(teslatruckgun.name_locale, teslatruckgun.name_locale);



		var Terran = AssetManager.actor_library.clone("Terran","baseWarUnit");
		Terran.die_in_lava = false;
		Terran.base_stats["lifespan"] = 50f;
		Terran.base_stats["mass_2"] = 200f;
		Terran.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		Terran.base_stats["scale"] = 0.1f;
		Terran.base_stats["size"] = 1f;
		Terran.base_stats["mass"] = 1000f;
		Terran.base_stats["health"] = ModernBoxPrefs.ScaleBalance(1000f);
		Terran.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(50f);
		Terran.base_stats["armor"] = 20f;
		Terran.base_stats["attack_speed"] = 2000f;
		Terran.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(25f);
		Terran.base_stats["knockback"] = 0.01f;
		Terran.base_stats["accuracy"] = 0.6f;
		Terran.base_stats["targets"] = 1f;
		Terran.base_stats["area_of_effect"] = 0.5f;
		Terran.base_stats["range"] = ModernBoxPrefs.ScaleBalance(16f);
		Terran.sound_hit = "event:/SFX/HIT/HitMetal";
		Terran.default_attack = "blueplasmashot";
		Terran.icon = "iconBoat";
		Terran.shadow_texture = "unitShadow_6";
		Terran.texture_asset = new ActorTextureSubAsset("actors/Terran/", false);
		Terran.special = true;
		Terran.has_advanced_textures = false;
		Terran.animation_walk = Vehicles.walk_0_5;
		Terran.animation_idle = ActorAnimationSequences.walk_0;
		Terran.animation_swim = ActorAnimationSequences.swim_0_2;
		Terran.name_locale = "Light Vehicle";
		Terran.addTrait("fire_proof");
		AssetManager.actor_library.add(Terran);
		Localization.addLocalization(Terran.name_locale, Terran.name_locale);



		var atstsniper = AssetManager.actor_library.clone("atstsniper","baseWarUnit");
		atstsniper.die_in_lava = false;
		atstsniper.base_stats["lifespan"] = 50f;
		atstsniper.base_stats["mass_2"] = 200f;
		atstsniper.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		atstsniper.base_stats["scale"] = 0.1f;
		atstsniper.base_stats["size"] = 1f;
		atstsniper.base_stats["mass"] = 1000f;
		atstsniper.base_stats["health"] = ModernBoxPrefs.ScaleBalance(600f);
		atstsniper.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		atstsniper.base_stats["armor"] = 0f;
		atstsniper.base_stats["attack_speed"] = 0.1f;
		atstsniper.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(100f);
		atstsniper.base_stats["knockback"] = 0.01f;
		atstsniper.base_stats["accuracy"] = 0.7f;
		atstsniper.base_stats["targets"] = 1f;
		atstsniper.base_stats["area_of_effect"] = 0.5f;
		atstsniper.base_stats["range"] = ModernBoxPrefs.ScaleBalance(50f);
		atstsniper.sound_hit = "event:/SFX/HIT/HitMetal";
		atstsniper.default_attack = "greenplasmashot";
		atstsniper.icon = "iconBoat";
		atstsniper.shadow_texture = "unitShadow_6";
		atstsniper.texture_asset = new ActorTextureSubAsset("actors/atstsniper/", false);
		atstsniper.special = true;
		atstsniper.has_advanced_textures = false;
		atstsniper.animation_walk = Vehicles.walk_0_5;
		atstsniper.animation_idle = ActorAnimationSequences.walk_0;
		atstsniper.animation_swim = ActorAnimationSequences.swim_0_3;
		atstsniper.name_locale = "Light Vehicle";
		atstsniper.addTrait("fire_proof");
		AssetManager.actor_library.add(atstsniper);
		Localization.addLocalization(atstsniper.name_locale, atstsniper.name_locale);




		var atst = AssetManager.actor_library.clone("atst","baseWarUnit");
		atst.die_in_lava = false;
		atst.base_stats["lifespan"] = 50f;
		atst.base_stats["mass_2"] = 200f;
		atst.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		atst.base_stats["scale"] = 0.1f;
		atst.base_stats["size"] = 1f;
		atst.base_stats["mass"] = 1000f;
		atst.base_stats["health"] = ModernBoxPrefs.ScaleBalance(1500f);
		atst.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		atst.base_stats["armor"] = 20f;
		atst.base_stats["attack_speed"] = 1f;
		atst.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		atst.base_stats["knockback"] = 0.01f;
		atst.base_stats["accuracy"] = 0.5f;
		atst.base_stats["targets"] = 3f;
		atst.base_stats["area_of_effect"] = 0.5f;
		atst.base_stats["range"] = ModernBoxPrefs.ScaleBalance(8f);
		atst.sound_hit = "event:/SFX/HIT/HitMetal";
		atst.default_attack = "redmediumplasmashot";
		atst.icon = "iconBoat";
		atst.shadow_texture = "unitShadow_6";
		atst.texture_asset = new ActorTextureSubAsset("actors/atst/", false);
		atst.special = true;
		atst.has_advanced_textures = false;
		atst.animation_walk = ActorAnimationSequences.walk_0_3;
		atst.animation_idle = ActorAnimationSequences.walk_0;
		atst.animation_swim = ActorAnimationSequences.swim_0_3;
		atst.name_locale = "Light Vehicle";
		atst.addTrait("fire_proof");
		AssetManager.actor_library.add(atst);
		Localization.addLocalization(atst.name_locale, atst.name_locale);


		var artilleryatst = AssetManager.actor_library.clone("artilleryatst","baseWarUnit");
		artilleryatst.die_in_lava = false;
		artilleryatst.base_stats["lifespan"] = 50f;
		artilleryatst.base_stats["mass_2"] = 200f;
		artilleryatst.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		artilleryatst.base_stats["scale"] = 0.1f;
		artilleryatst.base_stats["size"] = 1f;
		artilleryatst.base_stats["mass"] = 1000f;
		artilleryatst.base_stats["health"] = ModernBoxPrefs.ScaleBalance(600f);
		artilleryatst.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		artilleryatst.base_stats["armor"] = 0f;
		artilleryatst.base_stats["attack_speed"] = 0.01f;
		artilleryatst.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(300f);
		artilleryatst.base_stats["knockback"] = 0.01f;
		artilleryatst.base_stats["accuracy"] = 0.1f;
		artilleryatst.base_stats["targets"] = 6f;
		artilleryatst.base_stats["area_of_effect"] = 0.5f;
		artilleryatst.base_stats["range"] = ModernBoxPrefs.ScaleBalance(80f);
		artilleryatst.sound_hit = "event:/SFX/HIT/HitMetal";
		artilleryatst.default_attack = "biggreenplasmashot";
		artilleryatst.icon = "iconBoat";
		artilleryatst.shadow_texture = "unitShadow_6";
		artilleryatst.texture_asset = new ActorTextureSubAsset("actors/artilleryatst/", false);
		artilleryatst.special = true;
		artilleryatst.has_advanced_textures = false;
		artilleryatst.animation_walk = ActorAnimationSequences.walk_0_3;
		artilleryatst.animation_idle = Vehicles.idle_0;
		artilleryatst.animation_swim = ActorAnimationSequences.swim_0_3;
		artilleryatst.name_locale = "Light Vehicle";
		artilleryatst.addTrait("fire_proof");
		AssetManager.actor_library.add(artilleryatst);
		Localization.addLocalization(artilleryatst.name_locale, artilleryatst.name_locale);


		var supportatst = AssetManager.actor_library.clone("supportatst","baseWarUnit");
		supportatst.die_in_lava = false;
		supportatst.base_stats["lifespan"] = 50f;
		supportatst.base_stats["mass_2"] = 200f;
		supportatst.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(2000f);
		supportatst.base_stats["scale"] = 0.1f;
		supportatst.base_stats["size"] = 1f;
		supportatst.base_stats["mass"] = 1000f;
		supportatst.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		supportatst.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		supportatst.base_stats["armor"] = 30f;
		supportatst.base_stats["attack_speed"] = 1f;
		supportatst.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(50f);
		supportatst.base_stats["knockback"] = 0.01f;
		supportatst.base_stats["accuracy"] = 0.5f;
		supportatst.base_stats["targets"] = 3f;
		supportatst.base_stats["area_of_effect"] = 0.5f;
		supportatst.base_stats["range"] = ModernBoxPrefs.ScaleBalance(8f);
		supportatst.sound_hit = "event:/SFX/HIT/HitMetal";
		supportatst.default_attack = "redmediumplasmashot";
		supportatst.icon = "iconBoat";
		supportatst.shadow_texture = "unitShadow_6";
		supportatst.texture_asset = new ActorTextureSubAsset("actors/supportatst/", false);
		supportatst.special = true;
		supportatst.has_advanced_textures = false;
		supportatst.animation_walk = ActorAnimationSequences.walk_0_3;
		supportatst.animation_idle = ActorAnimationSequences.walk_0;
		supportatst.animation_swim = ActorAnimationSequences.swim_0_3;
		supportatst.name_locale = "Light Support Vehicle";
		supportatst.skip_fight_logic = true;
		supportatst.addTrait("fire_proof");
		supportatst.job = AssetLibrary<ActorAsset>.a<string>("decision");
		supportatst.addDecision("check_swearing");
		supportatst.addDecision("warrior_try_join_army_group");
		supportatst.addDecision("city_walking_to_danger_zone");
		supportatst.addDecision("check_cure");
		supportatst.addDecision("warrior_army_leader_move_random");
		supportatst.addDecision("check_heal");
		supportatst.addDecision("warrior_army_follow_leader");
		supportatst.addDecision("warrior_random_move");
		supportatst.addDecision("check_warrior_transport");
		supportatst.addDecision("swim_to_island");
		AssetManager.actor_library.add(supportatst);
		Localization.addLocalization(supportatst.name_locale, supportatst.name_locale);


		var HeliELite = AssetManager.actor_library.clone("HeliELite","baseWarUnit");
		HeliELite.die_in_lava = false;
		HeliELite.animation_speed_based_on_walk_speed = false;
		HeliELite.base_stats["lifespan"] = 100f;
		HeliELite.base_stats["mass_2"] = 600f;
		HeliELite.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		HeliELite.base_stats["scale"] = 0.13f;
		HeliELite.base_stats["size"] = 1f;
		HeliELite.base_stats["mass"] = 1000f;
		HeliELite.base_stats["health"] = ModernBoxPrefs.ScaleBalance(800f);
		HeliELite.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(80f);
		HeliELite.base_stats["armor"] = 30f;
		HeliELite.base_stats["attack_speed"] = 10000f;
		HeliELite.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(33f);
		HeliELite.base_stats["knockback"] = 0.05f;
		HeliELite.base_stats["accuracy"] = 0.2f;
		HeliELite.base_stats["targets"] = 4f;
		HeliELite.base_stats["area_of_effect"] = 0.5f;
		HeliELite.base_stats["range"] = ModernBoxPrefs.ScaleBalance(10f);
		HeliELite.sound_hit = "event:/SFX/HIT/HitMetal";
		HeliELite.default_attack = "missilebarrage";
		HeliELite.addDecision("burn_tumors");
		HeliELite.icon = "iconBoat";
		HeliELite.shadow_texture = "unitShadow_6";
		HeliELite.texture_asset = new ActorTextureSubAsset("actors/HeliELite/", false);
		HeliELite.special = true;
		HeliELite.has_advanced_textures = false;
		HeliELite.animation_walk = ActorAnimationSequences.walk_0_3;
		HeliELite.animation_idle = ActorAnimationSequences.idle_0_3;
		HeliELite.animation_swim = ActorAnimationSequences.walk_0_3;
		HeliELite.name_locale = "Helicopter thing";
		HeliELite.addTrait("fire_proof");
		HeliELite.addTrait("freeze_proof");
		HeliELite.flying = true;
		HeliELite.very_high_flyer = true;
		HeliELite.die_on_blocks = false;
		HeliELite.inspect_avatar_scale = 0.5f;
		HeliELite.ignore_blocks = true;
		AssetManager.actor_library.add(HeliELite);
		Localization.addLocalization(HeliELite.name_locale, HeliELite.name_locale);


		var FutureGunship = AssetManager.actor_library.clone("FutureGunship","baseWarUnit");
		FutureGunship.die_in_lava = false;
		FutureGunship.animation_speed_based_on_walk_speed = false;
		FutureGunship.base_stats["lifespan"] = 100f;
		FutureGunship.base_stats["mass_2"] = 600f;
		FutureGunship.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		FutureGunship.base_stats["scale"] = 0.13f;
		FutureGunship.base_stats["size"] = 1f;
		FutureGunship.base_stats["mass"] = 1000f;
		FutureGunship.base_stats["health"] = ModernBoxPrefs.ScaleBalance(700f);
		FutureGunship.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		FutureGunship.base_stats["armor"] = 50f;
		FutureGunship.base_stats["attack_speed"] = 3f;
		FutureGunship.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(20f);
		FutureGunship.base_stats["knockback"] = 0.05f;
		FutureGunship.base_stats["accuracy"] = 0.4f;
		FutureGunship.base_stats["targets"] = 4f;
		FutureGunship.base_stats["area_of_effect"] = 0.5f;
		FutureGunship.base_stats["range"] = ModernBoxPrefs.ScaleBalance(8f);
		FutureGunship.sound_hit = "event:/SFX/HIT/HitMetal";
		FutureGunship.default_attack = "greenmediumplasmashot";
		FutureGunship.addDecision("burn_tumors");
		FutureGunship.icon = "iconBoat";
		FutureGunship.shadow_texture = "unitShadow_6";
		FutureGunship.texture_asset = new ActorTextureSubAsset("actors/FutureGunship/", false);
		FutureGunship.special = true;
		FutureGunship.has_advanced_textures = false;
		FutureGunship.animation_walk = ActorAnimationSequences.walk_0_3;
		FutureGunship.animation_idle = ActorAnimationSequences.idle_0_3;
		FutureGunship.animation_swim = ActorAnimationSequences.walk_0_3;
		FutureGunship.name_locale = "Gunship";
		FutureGunship.addTrait("fire_proof");
		FutureGunship.addTrait("freeze_proof");
		FutureGunship.flying = true;
		FutureGunship.very_high_flyer = true;
		FutureGunship.die_on_blocks = false;
		FutureGunship.inspect_avatar_scale = 0.5f;
		FutureGunship.ignore_blocks = true;
		AssetManager.actor_library.add(FutureGunship);
		Localization.addLocalization(FutureGunship.name_locale, FutureGunship.name_locale);

		var TIEfighter = AssetManager.actor_library.clone("TIEfighter","baseWarUnit");
		TIEfighter.die_in_lava = false;
		TIEfighter.animation_speed_based_on_walk_speed = false;
		TIEfighter.base_stats["lifespan"] = 100f;
		TIEfighter.base_stats["mass_2"] = 600f;
		TIEfighter.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		TIEfighter.base_stats["scale"] = 0.13f;
		TIEfighter.base_stats["size"] = 1f;
		TIEfighter.base_stats["mass"] = 1000f;
		TIEfighter.base_stats["health"] = ModernBoxPrefs.ScaleBalance(600f);
		TIEfighter.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(100f);
		TIEfighter.base_stats["armor"] = 10f;
		TIEfighter.base_stats["attack_speed"] = 1f;
		TIEfighter.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		TIEfighter.base_stats["knockback"] = 4f;
		TIEfighter.base_stats["accuracy"] = 1f;
		TIEfighter.base_stats["targets"] = 3f;
		TIEfighter.base_stats["area_of_effect"] = 0.5f;
		TIEfighter.base_stats["range"] = ModernBoxPrefs.ScaleBalance(10f);
		TIEfighter.inspect_avatar_scale = 0.5f;
		TIEfighter.sound_hit = "event:/SFX/HIT/HitMetal";
		TIEfighter.default_attack = "Airredmediumplasmashot";
		TIEfighter.icon = "iconBoat";
		TIEfighter.shadow_texture = "unitShadow_6";
		TIEfighter.texture_asset = new ActorTextureSubAsset("actors/TIEfighter/", false);
		TIEfighter.special = true;
		TIEfighter.can_flip = false;
		TIEfighter.has_advanced_textures = false;
		TIEfighter.animation_walk = ActorAnimationSequences.walk_0_2;
		TIEfighter.animation_idle = Vehicles.idle_0_7;
		TIEfighter.animation_swim = ActorAnimationSequences.walk_0_2;
		TIEfighter.name_locale = "Fighter Jet";
		TIEfighter.addTrait("fire_proof");
		TIEfighter.addTrait("freeze_proof");
		TIEfighter.addTrait("dodge");
		TIEfighter.flying = true;
		TIEfighter.very_high_flyer = true;
		TIEfighter.die_on_blocks = false;
		TIEfighter.ignore_blocks = true;
		AssetManager.actor_library.add(TIEfighter);
		Localization.addLocalization(TIEfighter.name_locale, TIEfighter.name_locale);

		var EliteBomber = AssetManager.actor_library.clone("EliteBomber","baseWarUnit");
		EliteBomber.die_in_lava = false;
		EliteBomber.animation_speed_based_on_walk_speed = false;
		EliteBomber.base_stats["lifespan"] = 1000f;
		EliteBomber.base_stats["mass_2"] = 600f;
		EliteBomber.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(1000f);
		EliteBomber.base_stats["scale"] = 0.12f;
		EliteBomber.base_stats["size"] = 1f;
		EliteBomber.base_stats["mass"] = 1000f;
		EliteBomber.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		EliteBomber.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(60f);
		EliteBomber.base_stats["armor"] = 30f;
		EliteBomber.base_stats["attack_speed"] = 0.01f;
		EliteBomber.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(1000f);
		EliteBomber.base_stats["knockback"] = 15f;
		EliteBomber.base_stats["accuracy"] = 0.1f;
		EliteBomber.base_stats["targets"] = 20f;
		EliteBomber.base_stats["area_of_effect"] = 0.5f;
		EliteBomber.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		EliteBomber.sound_hit = "event:/SFX/HIT/HitMetal";
		EliteBomber.default_attack = "N2Attack";
		EliteBomber.icon = "iconBoat";
		EliteBomber.shadow_texture = "unitShadow_6";
		EliteBomber.texture_asset = new ActorTextureSubAsset("actors/EliteBomber/", false);
		EliteBomber.special = true;
		EliteBomber.can_flip = true;
		EliteBomber.has_advanced_textures = false;
		EliteBomber.animation_walk = ActorAnimationSequences.walk_0_3;
		EliteBomber.animation_idle = ActorAnimationSequences.idle_0_3;
		EliteBomber.animation_swim = ActorAnimationSequences.walk_0_3;
		EliteBomber.name_locale = "Bomber";
		EliteBomber.addTrait("fire_proof");
		EliteBomber.addTrait("freeze_proof");
		EliteBomber.flying = true;
		EliteBomber.very_high_flyer = true;
		EliteBomber.die_on_blocks = false;
		EliteBomber.ignore_blocks = true;
		EliteBomber.inspect_avatar_scale = 0.5f;
		AssetManager.actor_library.add(EliteBomber);
		Localization.addLocalization(EliteBomber.name_locale, EliteBomber.name_locale);


		var P9000 = AssetManager.actor_library.clone("P9000","baseWarUnit");
		P9000.die_in_lava = false;
		P9000.base_stats["lifespan"] = 300f;
		P9000.base_stats["mass_2"] = 600f;
		P9000.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		P9000.base_stats["scale"] = 0.1f;
		P9000.base_stats["size"] = 1f;
		P9000.base_stats["mass"] = 1000f;
		P9000.base_stats["health"] = ModernBoxPrefs.ScaleBalance(5000f);
		P9000.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(50f);
		P9000.base_stats["armor"] = 40f;
		P9000.base_stats["attack_speed"] = 0.1f;
		P9000.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(150f);
		P9000.base_stats["knockback"] = 5f;
		P9000.base_stats["accuracy"] = 0.8f;
		P9000.base_stats["targets"] = 4f;
		P9000.base_stats["area_of_effect"] = 2f;
		P9000.base_stats["range"] = ModernBoxPrefs.ScaleBalance(30f);
		P9000.sound_hit = "event:/SFX/HIT/HitMetal";
		P9000.default_attack = "blueplasmabig";
		P9000.icon = "iconBoat";
		P9000.shadow_texture = "unitShadow_6";
		P9000.texture_asset = new ActorTextureSubAsset("actors/P9000/", false);
		P9000.special = true;
		P9000.inspect_avatar_scale = 2f;
		P9000.has_advanced_textures = false;
		P9000.animation_walk = ActorAnimationSequences.walk_0_3;
		P9000.animation_idle = ActorAnimationSequences.walk_0_3;
		P9000.animation_swim = ActorAnimationSequences.walk_0_3;
		P9000.flying = true;
		P9000.name_locale = "Tank";
		P9000.addTrait("fire_proof");
		P9000.addTrait("block");
		P9000.addTrait("deflect_projectile");
		AssetManager.actor_library.add(P9000);
		Localization.addLocalization(P9000.name_locale, P9000.name_locale);

		var EliteP9000 = AssetManager.actor_library.clone("EliteP9000","baseWarUnit");
		EliteP9000.die_in_lava = false;
		EliteP9000.base_stats["lifespan"] = 1000f;
		EliteP9000.base_stats["mass_2"] = 600f;
		EliteP9000.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		EliteP9000.base_stats["scale"] = 0.2f;
		EliteP9000.base_stats["size"] = 1f;
		EliteP9000.base_stats["mass"] = 1000f;
		EliteP9000.base_stats["health"] = ModernBoxPrefs.ScaleBalance(10000f);
		EliteP9000.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(50f);
		EliteP9000.base_stats["armor"] = 50f;
		EliteP9000.base_stats["attack_speed"] = 0.1f;
		EliteP9000.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(500f);
		EliteP9000.base_stats["knockback"] = 5f;
		EliteP9000.base_stats["accuracy"] = 0.8f;
		EliteP9000.base_stats["targets"] = 4f;
		EliteP9000.base_stats["area_of_effect"] = 2f;
		EliteP9000.base_stats["range"] = ModernBoxPrefs.ScaleBalance(50f);
		EliteP9000.sound_hit = "event:/SFX/HIT/HitMetal";
		EliteP9000.default_attack = "redbigplasmashot";
		EliteP9000.icon = "iconBoat";
		EliteP9000.shadow_texture = "unitShadow_6";
		EliteP9000.texture_asset = new ActorTextureSubAsset("actors/EliteP9000/", false);
		EliteP9000.special = true;
		EliteP9000.inspect_avatar_scale = 2f;
		EliteP9000.has_advanced_textures = false;
		EliteP9000.animation_walk = ActorAnimationSequences.walk_0_3;
		EliteP9000.animation_idle = ActorAnimationSequences.walk_0_3;
		EliteP9000.animation_swim = ActorAnimationSequences.walk_0_3;
		EliteP9000.flying = true;
		EliteP9000.name_locale = "Tank";
		EliteP9000.addTrait("fire_proof");
		EliteP9000.addTrait("block");
		EliteP9000.addTrait("deflect_projectile");
		AssetManager.actor_library.add(EliteP9000);
		Localization.addLocalization(EliteP9000.name_locale, EliteP9000.name_locale);


		var Railgun = AssetManager.actor_library.clone("Railgun","baseWarUnit");
		Railgun.die_in_lava = false;
		Railgun.base_stats["lifespan"] = 300f;
		Railgun.base_stats["mass_2"] = 600f;
		Railgun.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		Railgun.base_stats["scale"] = 0.1f;
		Railgun.base_stats["size"] = 1f;
		Railgun.base_stats["mass"] = 1000f;
		Railgun.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		Railgun.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		Railgun.base_stats["armor"] = 10f;
		Railgun.base_stats["attack_speed"] = 0.01f;
		Railgun.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(700f);
		Railgun.base_stats["knockback"] = 0f;
		Railgun.base_stats["accuracy"] = 0.7f;
		Railgun.base_stats["targets"] = 2f;
		Railgun.base_stats["area_of_effect"] = 2f;
		Railgun.base_stats["range"] = ModernBoxPrefs.ScaleBalance(60f);
		Railgun.sound_hit = "event:/SFX/HIT/HitMetal";
		Railgun.default_attack = "greenmediumplasmashot";
		Railgun.icon = "iconBoat";
		Railgun.shadow_texture = "unitShadow_6";
		Railgun.texture_asset = new ActorTextureSubAsset("actors/Railgun/", false);
		Railgun.special = true;
		Railgun.inspect_avatar_scale = 2f;
		Railgun.has_advanced_textures = false;
		Railgun.animation_walk = ActorAnimationSequences.walk_0_3;
		Railgun.animation_idle = ActorAnimationSequences.walk_0;
		Railgun.animation_swim = ActorAnimationSequences.swim_0_3;
		Railgun.name_locale = "Railgun";
		Railgun.addTrait("fire_proof");
		Railgun.addTrait("block");
		Railgun.addTrait("deflect_projectile");
		AssetManager.actor_library.add(Railgun);
		Localization.addLocalization(Railgun.name_locale, Railgun.name_locale);


		var OmegaRailgun = AssetManager.actor_library.clone("OmegaRailgun","baseWarUnit");
		OmegaRailgun.die_in_lava = false;
		OmegaRailgun.base_stats["lifespan"] = 1000f;
		OmegaRailgun.base_stats["mass_2"] = 600f;
		OmegaRailgun.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		OmegaRailgun.base_stats["scale"] = 0.2f;
		OmegaRailgun.base_stats["size"] = 1f;
		OmegaRailgun.base_stats["mass"] = 1000f;
		OmegaRailgun.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		OmegaRailgun.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		OmegaRailgun.base_stats["armor"] = 20f;
		OmegaRailgun.base_stats["attack_speed"] = 0.01f;
		OmegaRailgun.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(1477f);
		OmegaRailgun.base_stats["knockback"] = 0f;
		OmegaRailgun.base_stats["accuracy"] = 0.6f;
		OmegaRailgun.base_stats["targets"] = 2f;
		OmegaRailgun.base_stats["area_of_effect"] = 2f;
		OmegaRailgun.base_stats["range"] = ModernBoxPrefs.ScaleBalance(80f);
		OmegaRailgun.sound_hit = "event:/SFX/HIT/HitMetal";
		OmegaRailgun.default_attack = "redbigplasmashot";
		OmegaRailgun.icon = "iconBoat";
		OmegaRailgun.shadow_texture = "unitShadow_6";
		OmegaRailgun.texture_asset = new ActorTextureSubAsset("actors/OmegaRailgun/", false);
		OmegaRailgun.special = true;
		OmegaRailgun.inspect_avatar_scale = 2f;
		OmegaRailgun.has_advanced_textures = false;
		OmegaRailgun.animation_walk = ActorAnimationSequences.walk_0_3;
		OmegaRailgun.animation_idle = ActorAnimationSequences.walk_0;
		OmegaRailgun.animation_swim = ActorAnimationSequences.swim_0_3;
		OmegaRailgun.name_locale = "Railgun";
		OmegaRailgun.addTrait("fire_proof");
		OmegaRailgun.addTrait("block");
		OmegaRailgun.addTrait("deflect_projectile");
		AssetManager.actor_library.add(OmegaRailgun);
		Localization.addLocalization(OmegaRailgun.name_locale, OmegaRailgun.name_locale);


		var AT9000 = AssetManager.actor_library.clone("AT9000","baseWarUnit");
		AT9000.die_in_lava = false;
		AT9000.base_stats["lifespan"] = 300f;
		AT9000.base_stats["mass_2"] = 600f;
		AT9000.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		AT9000.base_stats["scale"] = 0.12f;
		AT9000.base_stats["size"] = 1f;
		AT9000.base_stats["mass"] = 1000f;
		AT9000.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		AT9000.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		AT9000.base_stats["armor"] = 60f;
		AT9000.base_stats["attack_speed"] = 1f;
		AT9000.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(200f);
		AT9000.base_stats["knockback"] = 5f;
		AT9000.base_stats["accuracy"] = 0.3f;
		AT9000.base_stats["targets"] = 2f;
		AT9000.base_stats["area_of_effect"] = 2f;
		AT9000.base_stats["range"] = ModernBoxPrefs.ScaleBalance(50f);
		AT9000.sound_hit = "event:/SFX/HIT/HitMetal";
		AT9000.default_attack = "redmediumplasmashot";
		AT9000.icon = "iconBoat";
		AT9000.shadow_texture = "unitShadow_6";
		AT9000.texture_asset = new ActorTextureSubAsset("actors/AT9000/", false);
		AT9000.special = true;
		AT9000.inspect_avatar_scale = 2f;
		AT9000.has_advanced_textures = false;
		AT9000.animation_walk = Vehicles.walk_0_7;
		AT9000.animation_idle = ActorAnimationSequences.walk_0;
		AT9000.animation_swim = Vehicles.swim_0_7;
		AT9000.name_locale = "Heavy Land Support";
		AT9000.addTrait("fire_proof");
		AT9000.addTrait("block");
		AT9000.addTrait("deflect_projectile");
		AT9000.addDecision("check_cure");
		AT9000.addDecision("check_heal");
		AssetManager.actor_library.add(AT9000);
		Localization.addLocalization(AT9000.name_locale, AT9000.name_locale);


		var eliteAT9000 = AssetManager.actor_library.clone("eliteAT9000","baseWarUnit");
		eliteAT9000.die_in_lava = false;
		eliteAT9000.base_stats["lifespan"] = 1000f;
		eliteAT9000.base_stats["mass_2"] = 600f;
		eliteAT9000.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		eliteAT9000.base_stats["scale"] = 0.2f;
		eliteAT9000.base_stats["size"] = 1f;
		eliteAT9000.base_stats["mass"] = 1000f;
		eliteAT9000.base_stats["health"] = ModernBoxPrefs.ScaleBalance(8000f);
		eliteAT9000.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(30f);
		eliteAT9000.base_stats["armor"] = 70f;
		eliteAT9000.base_stats["attack_speed"] = 1f;
		eliteAT9000.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(400f);
		eliteAT9000.base_stats["knockback"] = 5f;
		eliteAT9000.base_stats["accuracy"] = 0.3f;
		eliteAT9000.base_stats["targets"] = 4f;
		eliteAT9000.base_stats["area_of_effect"] = 2f;
		eliteAT9000.base_stats["range"] = ModernBoxPrefs.ScaleBalance(70f);
		eliteAT9000.sound_hit = "event:/SFX/HIT/HitMetal";
		eliteAT9000.default_attack = "redbigplasmashot";
		eliteAT9000.icon = "iconBoat";
		eliteAT9000.shadow_texture = "unitShadow_6";
		eliteAT9000.texture_asset = new ActorTextureSubAsset("actors/eliteAT9000/", false);
		eliteAT9000.special = true;
		eliteAT9000.inspect_avatar_scale = 2f;
		eliteAT9000.has_advanced_textures = false;
		eliteAT9000.animation_walk = Vehicles.walk_0_7;
		eliteAT9000.animation_idle = ActorAnimationSequences.walk_0;
		eliteAT9000.animation_swim = Vehicles.swim_0_7;
		eliteAT9000.name_locale = "Heavy Land Support";
		eliteAT9000.addTrait("fire_proof");
		eliteAT9000.addTrait("block");
		eliteAT9000.addTrait("deflect_projectile");
		eliteAT9000.addDecision("check_cure");
		eliteAT9000.addDecision("check_heal");
		AssetManager.actor_library.add(eliteAT9000);
		Localization.addLocalization(eliteAT9000.name_locale, eliteAT9000.name_locale);



		var MA9000 = AssetManager.actor_library.clone("MA9000","baseWarUnit");
		MA9000.die_in_lava = false;
		MA9000.base_stats["lifespan"] = 300f;
		MA9000.base_stats["mass_2"] = 600f;
		MA9000.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		MA9000.base_stats["scale"] = 0.12f;
		MA9000.base_stats["size"] = 1f;
		MA9000.base_stats["mass"] = 1000f;
		MA9000.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		MA9000.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		MA9000.base_stats["armor"] = 20f;
		MA9000.base_stats["attack_speed"] = 1f;
		MA9000.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(1000f);
		MA9000.base_stats["knockback"] = 10f;
		MA9000.base_stats["accuracy"] = 0.1f;
		MA9000.base_stats["targets"] = 10f;
		MA9000.base_stats["area_of_effect"] = 2f;
		MA9000.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
		MA9000.sound_hit = "event:/SFX/HIT/HitMetal";
		MA9000.default_attack = "XenoBeam";
		MA9000.icon = "iconBoat";
		MA9000.shadow_texture = "unitShadow_6";
		MA9000.texture_asset = new ActorTextureSubAsset("actors/MA9000/", false);
		MA9000.special = true;
		MA9000.inspect_avatar_scale = 2f;
		MA9000.has_advanced_textures = false;
		MA9000.animation_walk = ActorAnimationSequences.walk_0_3;
		MA9000.animation_idle = Vehicles.idle_0;
		MA9000.animation_swim = ActorAnimationSequences.swim_0_3;
		MA9000.name_locale = "Heavy Artillery";
		MA9000.addTrait("fire_proof");
		MA9000.addTrait("block");
		MA9000.addTrait("deflect_projectile");
		AssetManager.actor_library.add(MA9000);
		Localization.addLocalization(MA9000.name_locale, MA9000.name_locale);

		var eliteMA9000 = AssetManager.actor_library.clone("eliteMA9000","baseWarUnit");
		eliteMA9000.die_in_lava = false;
		eliteMA9000.base_stats["lifespan"] = 1000f;
		eliteMA9000.base_stats["mass_2"] = 600f;
		eliteMA9000.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		eliteMA9000.base_stats["scale"] = 0.2f;
		eliteMA9000.base_stats["size"] = 1f;
		eliteMA9000.base_stats["mass"] = 1000f;
		eliteMA9000.base_stats["health"] = ModernBoxPrefs.ScaleBalance(4000f);
		eliteMA9000.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		eliteMA9000.base_stats["armor"] = 20f;
		eliteMA9000.base_stats["attack_speed"] = 1f;
		eliteMA9000.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(1000f);
		eliteMA9000.base_stats["knockback"] = 10f;
		eliteMA9000.base_stats["accuracy"] = 0.1f;
		eliteMA9000.base_stats["targets"] = 10f;
		eliteMA9000.base_stats["area_of_effect"] = 2f;
		eliteMA9000.base_stats["range"] = ModernBoxPrefs.ScaleBalance(100f);
		eliteMA9000.sound_hit = "event:/SFX/HIT/HitMetal";
		eliteMA9000.default_attack = "XenoMegaBomb";
		eliteMA9000.icon = "iconBoat";
		eliteMA9000.shadow_texture = "unitShadow_6";
		eliteMA9000.texture_asset = new ActorTextureSubAsset("actors/eliteMA9000/", false);
		eliteMA9000.special = true;
		eliteMA9000.inspect_avatar_scale = 2f;
		eliteMA9000.has_advanced_textures = false;
		eliteMA9000.animation_walk = ActorAnimationSequences.walk_0_3;
		eliteMA9000.animation_idle = Vehicles.idle_0;
		eliteMA9000.animation_swim = ActorAnimationSequences.swim_0_3;
		eliteMA9000.name_locale = "Heavy Artillery";
		eliteMA9000.addTrait("fire_proof");
		eliteMA9000.addTrait("block");
		eliteMA9000.addTrait("deflect_projectile");
		AssetManager.actor_library.add(eliteMA9000);
		Localization.addLocalization(eliteMA9000.name_locale, eliteMA9000.name_locale);



		var dreadnaught = AssetManager.actor_library.clone("dreadnaught","baseWarUnit");
		dreadnaught.die_in_lava = false;
		dreadnaught.base_stats["lifespan"] = 1000f;
		dreadnaught.base_stats["mass_2"] = 200f;
		dreadnaught.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		dreadnaught.base_stats["scale"] = 0.14f;
		dreadnaught.base_stats["size"] = 1f;
		dreadnaught.base_stats["mass"] = 1000f;
		dreadnaught.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		dreadnaught.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		dreadnaught.base_stats["armor"] = 70f;
		dreadnaught.base_stats["attack_speed"] = 2f;
		dreadnaught.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(77f);
		dreadnaught.base_stats["knockback"] = 2f;
		dreadnaught.base_stats["accuracy"] = 0.5f;
		dreadnaught.base_stats["targets"] = 2f;
		dreadnaught.base_stats["area_of_effect"] = 0.5f;
		dreadnaught.base_stats["range"] = ModernBoxPrefs.ScaleBalance(8f);
		dreadnaught.sound_hit = "event:/SFX/HIT/HitMetal";
		dreadnaught.default_attack = "tankpew";
		dreadnaught.icon = "iconBoat";
		dreadnaught.shadow_texture = "unitShadow_6";
		dreadnaught.texture_asset = new ActorTextureSubAsset("actors/dreadnaught/", false);
		dreadnaught.special = true;
		dreadnaught.has_advanced_textures = false;
		dreadnaught.animation_walk = ActorAnimationSequences.walk_0_3;
		dreadnaught.animation_idle = ActorAnimationSequences.walk_0;
		dreadnaught.animation_swim = ActorAnimationSequences.swim_0_2;
		dreadnaught.name_locale = "Armored Walker";
		dreadnaught.addTrait("fire_proof");
		dreadnaught.addTrait("block");
		dreadnaught.addTrait("deflect_projectile");
		AssetManager.actor_library.add(dreadnaught);
		Localization.addLocalization(dreadnaught.name_locale, dreadnaught.name_locale);



		var dreadnaught_brrt = AssetManager.actor_library.clone("dreadnaught_brrt","baseWarUnit");
		dreadnaught_brrt.die_in_lava = false;
		dreadnaught_brrt.base_stats["lifespan"] = 1000f;
		dreadnaught_brrt.base_stats["mass_2"] = 200f;
		dreadnaught_brrt.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		dreadnaught_brrt.base_stats["scale"] = 0.14f;
		dreadnaught_brrt.base_stats["size"] = 1f;
		dreadnaught_brrt.base_stats["mass"] = 1000f;
		dreadnaught_brrt.base_stats["health"] = ModernBoxPrefs.ScaleBalance(2000f);
		dreadnaught_brrt.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		dreadnaught_brrt.base_stats["armor"] = 70f;
		dreadnaught_brrt.base_stats["attack_speed"] = 20f;
		dreadnaught_brrt.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(33f);
		dreadnaught_brrt.base_stats["knockback"] = 0.5f;
		dreadnaught_brrt.base_stats["accuracy"] = 0.5f;
		dreadnaught_brrt.base_stats["targets"] = 1f;
		dreadnaught_brrt.base_stats["area_of_effect"] = 0.5f;
		dreadnaught_brrt.base_stats["range"] = ModernBoxPrefs.ScaleBalance(8f);
		dreadnaught_brrt.sound_hit = "event:/SFX/HIT/HitMetal";
		dreadnaught_brrt.default_attack = "mountedmachinegun";
		dreadnaught_brrt.icon = "iconBoat";
		dreadnaught_brrt.shadow_texture = "unitShadow_6";
		dreadnaught_brrt.texture_asset = new ActorTextureSubAsset("actors/dreadnaught_brrt/", false);
		dreadnaught_brrt.special = true;
		dreadnaught_brrt.has_advanced_textures = false;
		dreadnaught_brrt.animation_walk = ActorAnimationSequences.walk_0_3;
		dreadnaught_brrt.animation_idle = ActorAnimationSequences.walk_0;
		dreadnaught_brrt.animation_swim = ActorAnimationSequences.swim_0_2;
		dreadnaught_brrt.name_locale = "Armored Walker";
		dreadnaught_brrt.addTrait("fire_proof");
		dreadnaught_brrt.addTrait("block");
		dreadnaught_brrt.addTrait("deflect_projectile");
		AssetManager.actor_library.add(dreadnaught_brrt);
		Localization.addLocalization(dreadnaught_brrt.name_locale, dreadnaught_brrt.name_locale);



		var HumanTitan = AssetManager.actor_library.clone("HumanTitan","baseWarUnit");
		HumanTitan.die_in_lava = false;
		HumanTitan.base_stats["lifespan"] = 1000f;
		HumanTitan.base_stats["mass_2"] = 200f;
		HumanTitan.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		HumanTitan.base_stats["scale"] = 0.22f;
		HumanTitan.base_stats["size"] = 1f;
		HumanTitan.base_stats["mass"] = 1000f;
		HumanTitan.base_stats["health"] = ModernBoxPrefs.ScaleBalance(10000f);
		HumanTitan.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(40f);
		HumanTitan.base_stats["armor"] = 30f;
		HumanTitan.base_stats["attack_speed"] = 4f;
		HumanTitan.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(500f);
		HumanTitan.base_stats["knockback"] = 5f;
		HumanTitan.base_stats["accuracy"] = 0.2f;
		HumanTitan.base_stats["targets"] = 4f;
		HumanTitan.base_stats["area_of_effect"] = 0.5f;
		HumanTitan.base_stats["range"] = ModernBoxPrefs.ScaleBalance(20f);
		HumanTitan.sound_hit = "event:/SFX/HIT/HitMetal";
		HumanTitan.default_attack = "tankpew";
		HumanTitan.icon = "iconBoat";
		HumanTitan.shadow_texture = "unitShadow_6";
		HumanTitan.texture_asset = new ActorTextureSubAsset("actors/HumanTitan/", false);
		HumanTitan.special = true;
		HumanTitan.has_advanced_textures = false;
		HumanTitan.animation_walk = ActorAnimationSequences.walk_0_3;
		HumanTitan.animation_idle = ActorAnimationSequences.idle_0_3;
		HumanTitan.animation_swim = ActorAnimationSequences.swim_0_3;
		HumanTitan.name_locale = "Armored Walker";
		HumanTitan.addTrait("fire_proof");
		HumanTitan.addTrait("block");
		HumanTitan.addTrait("deflect_projectile");
		AssetManager.actor_library.add(HumanTitan);
		Localization.addLocalization(HumanTitan.name_locale, HumanTitan.name_locale);


		var HumanTitanElite = AssetManager.actor_library.clone("HumanTitanElite","baseWarUnit");
		HumanTitanElite.die_in_lava = false;
		HumanTitanElite.base_stats["lifespan"] = 1000f;
		HumanTitanElite.base_stats["mass_2"] = 200f;
		HumanTitanElite.base_stats["stamina"] = ModernBoxPrefs.ScaleBalance(500f);
		HumanTitanElite.base_stats["scale"] = 0.3f;
		HumanTitanElite.base_stats["size"] = 1f;
		HumanTitanElite.base_stats["mass"] = 1000f;
		HumanTitanElite.base_stats["health"] = ModernBoxPrefs.ScaleBalance(100000f);
		HumanTitanElite.base_stats["speed"] = ModernBoxPrefs.ScaleBalance(20f);
		HumanTitanElite.base_stats["armor"] = 40f;
		HumanTitanElite.base_stats["attack_speed"] = 4f;
		HumanTitanElite.base_stats["damage"] = ModernBoxPrefs.ScaleBalance(1000f);
		HumanTitanElite.base_stats["knockback"] = 5f;
		HumanTitanElite.base_stats["accuracy"] = 0.2f;
		HumanTitanElite.base_stats["targets"] = 4f;
		HumanTitanElite.base_stats["area_of_effect"] = 0.5f;
		HumanTitanElite.base_stats["range"] = ModernBoxPrefs.ScaleBalance(30f);
		HumanTitanElite.sound_hit = "event:/SFX/HIT/HitMetal";
		HumanTitanElite.default_attack = "tankpew";
		HumanTitanElite.icon = "iconBoat";
		HumanTitanElite.shadow_texture = "unitShadow_6";
		HumanTitanElite.texture_asset = new ActorTextureSubAsset("actors/HumanTitanElite/", false);
		HumanTitanElite.special = true;
		HumanTitanElite.has_advanced_textures = false;
		HumanTitanElite.animation_walk = ActorAnimationSequences.walk_0_3;
		HumanTitanElite.animation_idle = ActorAnimationSequences.walk_0;
		HumanTitanElite.animation_swim = ActorAnimationSequences.swim_0_3;
		HumanTitanElite.name_locale = "Armored Walker";
		HumanTitanElite.addTrait("fire_proof");
		HumanTitanElite.addTrait("block");
		HumanTitanElite.addTrait("deflect_projectile");
		AssetManager.actor_library.add(HumanTitanElite);
		Localization.addLocalization(HumanTitanElite.name_locale, HumanTitanElite.name_locale);




	

/////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////UNIT REGISTRATION//////////////////////////////////////////////////////
ApplyAirVehicleDecisionProfiles();
string[] unitNames = new string[]
{
    "EliteP9000", "OmegaRailgun", "eliteAT9000", "eliteMA9000", "dreadnaught_brrt", "HumanTitanElite", "SpaceMarine",
	"Terran", "teslatruckgun", "atst", "artilleryatst", "atstsniper",
    "modernhumvee_Human", "howitzer_Human", "Humvee", "humancavalry", "humancannon",
    "spaceork", "modernhumvee_Ork", "howitzer_Ork", "ogreunit", "orccannon", "armoredwolf",
    "modernhumvee_Dwarf", "howitzer_Dwarf", "dwarfcannon", "golemgem",
    "modernhumvee_Gaia", "howitzer_Gaia", "treant", "elfcannon", "demonscorpion",
    "demonwyvern", "xenolevitank", "xenoUFO", "P9000", "dreadnaught", "Railgun",
    "baseMA9000", "Tank_Human", "MissileSystem_Human", "wheeledtank_Human", "AbramTank",
    "shermanww", "tankie", "genericwwtank", "landship", "bigtankww", "davincitank",
    "catapulta", "batteringram", "Tank_Ork", "MissileSystem_Ork", "wheeledtank_Ork",
    "orcatapulta", "Tank_Dwarf", "MissileSystem_Dwarf", "wheeledtank_Dwarf", "santaguin",
    "Tank_Gaia", "MissileSystem_Gaia", "wheeledtank_Gaia", "woolyrhino", "demoncroc",
    "demongolem", "demonreaver", "xenorailgun", "xenotripod", "AT9000", "supportatst",
    "supporttruck_Human", "wwsupporttruck", "humanpaladin", "supporttruck_Ork",
    "orcwarlock", "supporttruck_Dwarf", "dwarfdoctor", "supporttruck_Gaia",
    "fairydragon", "HeliELite", "FutureGunship", "TIEfighter", "EliteBomber",
    "Heli_Human", "Bomber_Human", "FighterJet_Human", "F55FighterJet", "Zeppelin",
    "EliteZeppelin", "americanbomberww", "biplane", "fighterww", "balloonunit",
    "Heli_Ork", "Bomber_Ork", "FighterJet_Ork", "Gunship", "Heli_Dwarf", "Bomber_Dwarf",
    "FighterJet_Dwarf", "Heli_Gaia", "Bomber_Gaia", "FighterJet_Gaia", "bigfaerydragon",
    "Bomber_Demon", "xenoUFObomber", "HumanTitan", "MA9000", "crusaderdreadnaught"
};

foreach (string unitName in unitNames)
{
    UnitTracker.Instance.RegisterUnit(unitName);
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////FUTURE/////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////









        }	






		public static readonly string[] walk_0_7 = Toolbox.a<string>("walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7");


		public static readonly string[] swim_0_7 = Toolbox.a<string>("swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7");

public static readonly string[] idle_0 = Toolbox.a<string>("idle_0");

public static readonly string[] idle_0_2 = Toolbox.a<string>("idle_0", "idle_1", "idle_2");

public static readonly string[] idle_0_5 = Toolbox.a<string>("idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5" );

public static readonly string[] idle_0_7 = Toolbox.a<string>("idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7");

public static readonly string[] idle_0_8 = Toolbox.a<string>("idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8");

public static readonly string[] idle_0_9 = Toolbox.a<string>("idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9");

public static readonly string[] idle_0_13 = Toolbox.a<string>( "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9", "idle_10", "idle_11", "idle_12", "idle_13" );

public static readonly string[] idle_0_15 = Toolbox.a<string>( "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9", "idle_10", "idle_11", "idle_12", "idle_13", "idle_14", "idle_15" );

public static readonly string[] idle_0_19 = Toolbox.a<string>( "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9", "idle_10", "idle_11", "idle_12", "idle_13", "idle_14", "idle_15", "idle_16", "idle_17", "idle_18", "idle_19" );


public static readonly string[] walk_0_5 = Toolbox.a<string>("walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" );


public static readonly string[] swim_0_5 = Toolbox.a<string>("swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5" );



	public static void toggleNukes()
        {
            Main.modifyBoolOption("NukeOption", PowerButtons.GetToggleValue("nukes_toggle"));
            if (PowerButtons.GetToggleValue("nukes_toggle"))
            {
                turnOnNukes();
                return;
            }
            turnOffNukes();
        }

        public static void turnOnNukes()
        {
			nukesEnabled = true;
        }

        public static void turnOffNukes()
        {
			nukesEnabled = false;
        }

	public static void toggleBalls()
        {
            Main.modifyBoolOption("BallsOption", PowerButtons.GetToggleValue("nuketexttoggle"));
            if (PowerButtons.GetToggleValue("nuketexttoggle"))
            {
                turnOnBalls();
                return;
            }
            turnOffBalls();
        }

        public static void turnOnBalls()
        {
			balls = true;
        }

        public static void turnOffBalls()
        {
			balls = false;
        }






private static bool TryGetAirVehicleProfile(Actor actor, out AirVehicleProfile profile)
{
	profile = null;
	if (actor == null || actor.asset == null)
	{
		return false;
	}

	if (AirVehicleProfiles.TryGetValue(actor.asset.id, out profile))
	{
		return true;
	}

	List<string> decisionIds = actor.asset.decision_ids;
	if (decisionIds != null && decisionIds.Contains("bomber_force_reload_rtb"))
	{
		profile = DefaultAirVehicleProfile;
		return true;
	}

	return false;
}

private static AirVehicleProfile GetAirVehicleProfileOrDefault(Actor actor)
{
	if (TryGetAirVehicleProfile(actor, out AirVehicleProfile profile))
	{
		return profile;
	}

	return DefaultAirVehicleProfile;
}

private static bool IsAirVehicleActor(Actor actor)
{
	return TryGetAirVehicleProfile(actor, out _);
}

private static bool IsVehicleActor(Actor actor)
{
	if (actor == null)
	{
		return false;
	}

	if (actor.hasTrait("Unitpotential") || actor.hasTrait("boat"))
	{
		return true;
	}

	return actor.asset != null && actor.asset.is_boat;
}

private static bool TryGetLandVehicleAmmoProfile(Actor actor, out LandVehicleAmmoProfile profile)
{
	profile = null;
	if (actor == null || actor.asset == null || !actor.hasTrait("Unitpotential"))
	{
		return false;
	}

	if (IsAirVehicleActor(actor))
	{
		return false;
	}

	return LandVehicleAmmoProfiles.TryGetValue(actor.asset.id, out profile);
}

private static bool IsLandVehicleAmmoActor(Actor actor)
{
	return TryGetLandVehicleAmmoProfile(actor, out _);
}

private static bool ShouldProtectCivilianFromVehicle(Actor attacker, BaseSimObject target)
{
	if (attacker == null || target == null || !target.isActor())
	{
		return false;
	}

	if (WorldLawLibrary.world_law_angry_civilians.isEnabled())
	{
		return false;
	}

	if (!attacker.isKingdomCiv() || attacker.kingdom == null || !target.isKingdomCiv() || target.kingdom == null)
	{
		return false;
	}

	if (!attacker.kingdom.isEnemy(target.kingdom))
	{
		return false;
	}

	Actor targetActor = target.a;
	if (targetActor == null || targetActor.profession_asset == null || !targetActor.profession_asset.is_civilian)
	{
		return false;
	}

	if (targetActor.hasTrait("Unitpotential") || targetActor.hasTrait("boat") || targetActor.asset.is_boat || targetActor.isWarrior())
	{
		return false;
	}

	UnitProfession targetProfession = targetActor.profession_asset.profession_id;
	if (targetProfession == UnitProfession.King || targetProfession == UnitProfession.Leader || targetProfession == UnitProfession.Warrior)
	{
		return false;
	}

	if (attacker.hasStatusTantrum() || targetActor.hasStatusTantrum())
	{
		return false;
	}

	bool attackerXenophobic = attacker.hasXenophobic();
	bool targetXenophobic = targetActor.hasXenophobic();
	bool hasXenophobic = attackerXenophobic || targetXenophobic;
	bool hasXenophiles = attacker.hasXenophiles() || targetActor.hasXenophiles();
	bool sameCulture = attacker.hasCulture() && targetActor.hasCulture() && attacker.culture == targetActor.culture;
	bool sameSpecies = attacker.kingdom.getSpecies() == targetActor.kingdom.getSpecies();
	bool protectCivilian = ((sameSpecies || hasXenophiles) && !hasXenophobic) || (sameCulture && sameSpecies);
	return protectCivilian;
}

private static bool IsValidVehicleCaptureProxy(Actor candidate, Actor vehicle, City city)
{
	if (candidate == null || vehicle == null || city == null)
	{
		return false;
	}

	if (!candidate.isAlive() || candidate == vehicle)
	{
		return false;
	}

	if (IsVehicleActor(candidate))
	{
		return false;
	}

	if (!candidate.isKingdomCiv() || candidate.kingdom == null || candidate.kingdom != vehicle.kingdom)
	{
		return false;
	}

	if (candidate.city != city || candidate.profession_asset == null || !candidate.profession_asset.can_capture)
	{
		return false;
	}

	if (candidate.isInsideSomething())
	{
		return false;
	}

	return true;
}

private static Actor FindVehicleCaptureProxyActor(Actor vehicle, City city)
{
	if (vehicle == null || city == null)
	{
		return null;
	}

	if (city.hasLeader())
	{
		Actor leader = city.leader;
		if (IsValidVehicleCaptureProxy(leader, vehicle, city))
		{
			return leader;
		}
	}

	WorldTile centerTile = vehicle.current_tile ?? city.getTile();
	if (centerTile == null)
	{
		return null;
	}

	foreach (Actor candidate in Finder.getUnitsFromChunk(centerTile, 3, 28f))
	{
		if (IsValidVehicleCaptureProxy(candidate, vehicle, city))
		{
			return candidate;
		}
	}

	return null;
}

private static void EnsureLandVehicleAmmoState(Actor actor)
{
	if (!TryGetLandVehicleAmmoProfile(actor, out LandVehicleAmmoProfile profile))
	{
		return;
	}

	actor.data.get(LandVehicleAmmoCurrentKey, out int ammo, -1);
	if (ammo < 0)
	{
		actor.data.set(LandVehicleAmmoCurrentKey, profile.ammoMax);
	}

	actor.data.get(LandVehicleReloadTickKey, out int reloadTicks, -1);
	if (reloadTicks < 0)
	{
		actor.data.set(LandVehicleReloadTickKey, 0);
	}

	actor.data.get(LandVehicleNavTickKey, out int navTicks, -1);
	if (navTicks < 0)
	{
		actor.data.set(LandVehicleNavTickKey, 0);
	}

	actor.data.get(LandVehicleReloadTimerKey, out float reloadTimer, -1f);
	if (reloadTimer < 0f)
	{
		actor.data.set(LandVehicleReloadTimerKey, 0f);
	}
}

private static int GetLandVehicleAmmo(Actor actor)
{
	if (!TryGetLandVehicleAmmoProfile(actor, out LandVehicleAmmoProfile profile))
	{
		return 0;
	}

	actor.data.get(LandVehicleAmmoCurrentKey, out int ammo, profile.ammoMax);
	return Mathf.Clamp(ammo, 0, profile.ammoMax);
}

private static void SetLandVehicleAmmo(Actor actor, int ammo)
{
	if (!TryGetLandVehicleAmmoProfile(actor, out LandVehicleAmmoProfile profile))
	{
		return;
	}

	actor.data.set(LandVehicleAmmoCurrentKey, Mathf.Clamp(ammo, 0, profile.ammoMax));
}

private static bool GetLandVehicleBool(Actor actor, string key)
{
	actor.data.get(key, out bool state, pDefault: false);
	return state;
}

private static void SetLandVehicleBool(Actor actor, string key, bool state)
{
	actor.data.set(key, state);
}

private static void EnsureBomberState(Actor actor)
{
	AirVehicleProfile profile = GetAirVehicleProfileOrDefault(actor);
	actor.data.get(BomberAmmoCurrentKey, out int ammo, -1);
	if (ammo < 0)
	{
		actor.data.set(BomberAmmoCurrentKey, profile.ammoMax);
	}
	actor.data.get(BomberReloadTickKey, out int reloadTicks, -1);
	if (reloadTicks < 0)
	{
		actor.data.set(BomberReloadTickKey, 0);
	}
	actor.data.get(BomberFireTickKey, out int fireTicks, -1);
	if (fireTicks < 0)
	{
		actor.data.set(BomberFireTickKey, 0);
	}
	actor.data.get(BomberNavTickKey, out int navTicks, -1);
	if (navTicks < 0)
	{
		actor.data.set(BomberNavTickKey, 0);
	}

	actor.data.get(BomberReloadTimerKey, out float reloadTimer, -1f);
	if (reloadTimer < 0f)
	{
		actor.data.set(BomberReloadTimerKey, 0f);
	}

	actor.data.get(BomberTargetRefreshTickKey, out int refreshTick, -1);
	if (refreshTick < 0)
	{
		actor.data.set(BomberTargetRefreshTickKey, 0);
	}
}

private static bool AdvanceReloadTimer(Actor actor, string timerKey, float pElapsed, float durationSeconds)
{
	float clampedDuration = Mathf.Max(0.25f, durationSeconds);
	actor.data.get(timerKey, out float timer, 0f);
	if (timer <= 0f)
	{
		timer = clampedDuration;
		actor.makeStunned(clampedDuration + 0.5f);
	}

	timer -= Mathf.Max(0f, pElapsed);
	if (timer <= 0f)
	{
		actor.data.set(timerKey, 0f);
		return true;
	}

	actor.data.set(timerKey, timer);
	return false;
}

private static void ResetReloadTimer(Actor actor, string timerKey)
{
	actor.data.set(timerKey, 0f);
}

private static void RecoverHealthPercentOverTime(Actor actor, string poolKey, float pElapsed, float percentPerSecond)
{
	if (actor == null || !actor.isAlive() || percentPerSecond <= 0f)
	{
		return;
	}

	int maxHealth = actor.getMaxHealth();
	if (maxHealth <= 0 || actor.getHealth() >= maxHealth)
	{
		actor.data.set(poolKey, 0f);
		return;
	}

	float delta = Mathf.Max(0f, pElapsed) * (percentPerSecond / 100f) * maxHealth;
	if (delta <= 0f)
	{
		return;
	}

	actor.data.get(poolKey, out float pool, 0f);
	pool += delta;
	int wholeHeal = Mathf.FloorToInt(pool);
	if (wholeHeal > 0)
	{
		actor.restoreHealth(wholeHeal);
		pool -= wholeHeal;
	}

	actor.data.set(poolKey, pool);
}

private static int GetBomberAmmo(Actor actor)
{
	AirVehicleProfile profile = GetAirVehicleProfileOrDefault(actor);
	actor.data.get(BomberAmmoCurrentKey, out int ammo, profile.ammoMax);
	return Mathf.Clamp(ammo, 0, profile.ammoMax);
}

private static void SetBomberAmmo(Actor actor, int ammo)
{
	AirVehicleProfile profile = GetAirVehicleProfileOrDefault(actor);
	actor.data.set(BomberAmmoCurrentKey, Mathf.Clamp(ammo, 0, profile.ammoMax));
}

private static bool GetBomberBool(Actor actor, string key)
{
	actor.data.get(key, out bool state, pDefault: false);
	return state;
}

private static void SetBomberBool(Actor actor, string key, bool state)
{
	actor.data.set(key, state);
}

private static void ApplyAirVehicleDecisionProfiles()
{
	foreach (KeyValuePair<string, AirVehicleProfile> entry in AirVehicleProfiles)
	{
		ActorAsset asset = AssetManager.actor_library.get(entry.Key);
		if (asset == null)
		{
			continue;
		}

		asset.decision_ids = new List<string>();
		for (int i = 0; i < AirVehicleDecisionIds.Length; i++)
		{
			asset.addDecision(AirVehicleDecisionIds[i]);
		}
	}
}

private static Sprite GetBomberLandedSprite(Actor actor, AirVehicleProfile profile)
{
	if (actor == null || actor.asset == null)
	{
		return null;
	}

	string actorId = actor.asset.id;
	if (_airVehicleLandedSpriteCache.TryGetValue(actorId, out Sprite cachedSprite) && cachedSprite != null)
	{
		return cachedSprite;
	}

	Sprite[] sprites = SpriteTextureLoader.getSpriteList("actors/" + actorId + "/main");
	if (sprites == null || sprites.Length == 0)
	{
		return null;
	}

	string landedSpriteName = profile != null ? profile.landedSpriteName : "landed";

	for (int i = 0; i < sprites.Length; i++)
	{
		Sprite sprite = sprites[i];
		if (sprite != null && string.Equals(sprite.name, landedSpriteName, StringComparison.OrdinalIgnoreCase))
		{
			_airVehicleLandedSpriteCache[actorId] = sprite;
			return sprite;
		}
	}

	for (int i = 0; i < sprites.Length; i++)
	{
		Sprite sprite = sprites[i];
		if (sprite != null && sprite.name != null && sprite.name.StartsWith(landedSpriteName, StringComparison.OrdinalIgnoreCase))
		{
			_airVehicleLandedSpriteCache[actorId] = sprite;
			return sprite;
		}
	}

	for (int i = 0; i < sprites.Length; i++)
	{
		if (sprites[i] != null)
		{
			_airVehicleLandedSpriteCache[actorId] = sprites[i];
			return sprites[i];
		}
	}

	return null;
}

private static bool AdvanceBomberTick(Actor actor, string key, int interval)
{
	actor.data.get(key, out int tick, 0);
	tick++;
	if (tick >= interval)
	{
		actor.data.set(key, 0);
		return true;
	}
	actor.data.set(key, tick);
	return false;
}

private static bool IsNearTile(Actor actor, WorldTile tile, float distance)
{
	if (actor.current_tile == null || tile == null)
	{
		return false;
	}
	return Toolbox.Dist(actor.current_tile.x, actor.current_tile.y, tile.x, tile.y) <= distance;
}

private static bool IsBomberBaseBuilding(Building building)
{
	if (building == null || building.asset == null || string.IsNullOrEmpty(building.asset.type))
	{
		return false;
	}

	string buildingType = building.asset.type;
	return buildingType == "type_hall" || buildingType == "type_barracks" || buildingType == "type_training_dummies";
}

private static WorldTile FindCityBomberBaseTile(City city)
{
	if (city == null || !city.isAlive() || city.buildings == null)
	{
		return null;
	}

	foreach (Building building in city.buildings)
	{
		if (IsBomberBaseBuilding(building) && building.current_tile != null)
		{
			return building.current_tile;
		}
	}

	return null;
}

private static WorldTile FindBomberBaseTile(Actor actor)
{
	if (actor == null)
	{
		return null;
	}

	City city = actor.city;
	WorldTile cityBaseTile = FindCityBomberBaseTile(city);
	if (cityBaseTile != null)
	{
		return cityBaseTile;
	}

	if (city != null && city.isAlive())
	{
		WorldTile cityTile = city.getTile();
		if (cityTile != null)
		{
			return cityTile;
		}
	}

	Kingdom kingdom = actor.kingdom;
	if (kingdom != null)
	{
		WorldTile capitalBaseTile = FindCityBomberBaseTile(kingdom.capital);
		if (capitalBaseTile != null)
		{
			return capitalBaseTile;
		}

		foreach (City kingdomCity in kingdom.cities)
		{
			if (kingdomCity == null || kingdomCity == kingdom.capital || !kingdomCity.isAlive())
			{
				continue;
			}

			WorldTile kingdomCityBaseTile = FindCityBomberBaseTile(kingdomCity);
			if (kingdomCityBaseTile != null)
			{
				return kingdomCityBaseTile;
			}
		}

		if (kingdom.capital != null && kingdom.capital.isAlive())
		{
			WorldTile capitalTile = kingdom.capital.getTile();
			if (capitalTile != null)
			{
				return capitalTile;
			}
		}
		foreach (City kingdomCity in kingdom.cities)
		{
			if (kingdomCity != null && kingdomCity.isAlive())
			{
				WorldTile kingdomCityTile = kingdomCity.getTile();
				if (kingdomCityTile != null)
				{
					return kingdomCityTile;
				}
			}
		}
	}

	return actor.current_tile;
}

private static WorldTile FindCityBarracksTile(City city)
{
	if (city == null || !city.isAlive() || city.buildings == null)
	{
		return null;
	}

	Building primaryBarracks = city.getBuildingOfType("type_barracks");
	if (primaryBarracks != null && primaryBarracks.current_tile != null)
	{
		return primaryBarracks.current_tile;
	}

	foreach (Building building in city.buildings)
	{
		if (building == null || building.asset == null || building.current_tile == null)
		{
			continue;
		}

		if (building.asset.type == "type_barracks")
		{
			return building.current_tile;
		}
	}

	return null;
}

private static WorldTile FindLandVehicleReloadTile(Actor actor, out bool canReloadAtTarget)
{
	canReloadAtTarget = false;
	if (actor == null)
	{
		return null;
	}

	Kingdom kingdom = actor.kingdom;
	if (kingdom != null && kingdom.capital != null && kingdom.capital.isAlive())
	{
		WorldTile capitalBarracks = FindCityBarracksTile(kingdom.capital);
		if (capitalBarracks != null)
		{
			canReloadAtTarget = true;
			return capitalBarracks;
		}
	}

	if (actor.hasHomeBuilding())
	{
		Building homeBuilding = actor.getHomeBuilding();
		if (homeBuilding != null && homeBuilding.current_tile != null && !homeBuilding.isRekt())
		{
			canReloadAtTarget = true;
			return homeBuilding.current_tile;
		}
	}

	return null;
}

private static bool HasNearbyEnemy(Actor actor, float radius = 20f)
{
	if (actor == null || actor.current_tile == null || actor.kingdom == null)
	{
		return false;
	}
	foreach (Actor other in Finder.getUnitsFromChunk(actor.current_tile, 2, radius))
	{
		if (other == null || other == actor || !other.isAlive() || other.kingdom == null)
		{
			continue;
		}
		if (actor.kingdom.isEnemy(other.kingdom))
		{
			return true;
		}
	}
	return false;
}

private static Actor FindNearestEnemyActor(Actor actor, float radius = 35f)
{
	if (actor == null || actor.current_tile == null || actor.kingdom == null)
	{
		return null;
	}

	Actor nearest = null;
	float bestDist = float.MaxValue;
	foreach (Actor other in Finder.getUnitsFromChunk(actor.current_tile, 3, radius))
	{
		if (other == null || other == actor || !other.isAlive() || other.kingdom == null)
		{
			continue;
		}
		if (!actor.kingdom.isEnemy(other.kingdom))
		{
			continue;
		}
		float dist = Vector2.Distance(actor.current_position, other.current_position);
		if (dist < bestDist)
		{
			bestDist = dist;
			nearest = other;
		}
	}

	return nearest;
}

private static City FindNearestEnemyCity(Actor actor)
{
	if (actor == null || actor.kingdom == null || !actor.kingdom.hasEnemies())
	{
		return null;
	}

	City bestCity = null;
	float bestDist = float.MaxValue;
	using (var enemies = actor.kingdom.getEnemiesKingdoms())
	{
		foreach (Kingdom enemyKingdom in enemies)
		{
			if (enemyKingdom == null || enemyKingdom.cities == null || enemyKingdom.cities.Count == 0)
			{
				continue;
			}
			foreach (City enemyCity in enemyKingdom.cities)
			{
				if (enemyCity == null || !enemyCity.isAlive())
				{
					continue;
				}
				WorldTile cityTile = enemyCity.getTile();
				if (cityTile == null)
				{
					continue;
				}

				float dist = Vector2.Distance(actor.current_position, cityTile.pos);
				if (dist < bestDist)
				{
					bestDist = dist;
					bestCity = enemyCity;
				}
			}
		}
	}

	return bestCity;
}

private static Actor FindNearestEnemyAroundTile(Actor actor, WorldTile centerTile, float radius = 22f)
{
	if (actor == null || centerTile == null || actor.kingdom == null)
	{
		return null;
	}

	Actor nearest = null;
	float bestDist = float.MaxValue;
	foreach (Actor other in Finder.getUnitsFromChunk(centerTile, 3, radius))
	{
		if (other == null || !other.isAlive() || other.kingdom == null)
		{
			continue;
		}
		if (!actor.kingdom.isEnemy(other.kingdom))
		{
			continue;
		}

		float dist = Vector2.Distance(actor.current_position, other.current_position);
		if (dist < bestDist)
		{
			bestDist = dist;
			nearest = other;
		}
	}

	return nearest;
}

private static bool IsPreferredAirEnemyActor(Actor actor)
{
	if (actor == null || !actor.isAlive())
	{
		return false;
	}

	if (actor.hasTrait("Unitpotential") || actor.isWarrior() || actor.hasTrait("boat") || actor.asset.is_boat)
	{
		return true;
	}

	if (actor.profession_asset == null)
	{
		return false;
	}

	UnitProfession profession = actor.profession_asset.profession_id;
	return profession == UnitProfession.King || profession == UnitProfession.Leader || profession == UnitProfession.Warrior;
}

private static BaseSimObject FindPreferredAirTargetAroundTile(Actor actor, WorldTile centerTile, float radius = 24f)
{
	if (actor == null || centerTile == null || actor.kingdom == null)
	{
		return null;
	}

	BaseSimObject bestHighPriority = null;
	float bestHighPriorityDist = float.MaxValue;
	BaseSimObject bestMediumPriority = null;
	float bestMediumPriorityDist = float.MaxValue;
	BaseSimObject bestLowPriority = null;
	float bestLowPriorityDist = float.MaxValue;

	foreach (Actor other in Finder.getUnitsFromChunk(centerTile, 3, radius))
	{
		if (other == null || other == actor || !other.isAlive() || other.kingdom == null)
		{
			continue;
		}
		if (!actor.kingdom.isEnemy(other.kingdom))
		{
			continue;
		}
		if (ShouldProtectCivilianFromVehicle(actor, other))
		{
			continue;
		}
		if (!actor.canAttackTarget(other, pCheckForFactions: true, pAttackBuildings: true))
		{
			continue;
		}

		float dist = Vector2.Distance(actor.current_position, other.current_position);
		float score = dist + UnityEngine.Random.value * 4f;
		if (IsPreferredAirEnemyActor(other))
		{
			if (score < bestHighPriorityDist)
			{
				bestHighPriorityDist = score;
				bestHighPriority = other;
			}
		}
		else if (score < bestLowPriorityDist)
		{
			bestLowPriorityDist = score;
			bestLowPriority = other;
		}
	}

	int tileRadius = Mathf.Max(1, Mathf.CeilToInt(radius));
	foreach (Building building in Finder.getBuildingsFromChunk(centerTile, 3, tileRadius, pRandom: false))
	{
		if (building == null || !building.isAlive() || building.kingdom == null)
		{
			continue;
		}
		if (!actor.kingdom.isEnemy(building.kingdom))
		{
			continue;
		}
		if (!actor.canAttackTarget(building, pCheckForFactions: true, pAttackBuildings: true))
		{
			continue;
		}

		bool strategicBuilding = building.asset != null && (building.asset.tower || building.asset.city_building);
		float dist = Vector2.Distance(actor.current_position, building.current_position);
		float score = dist + UnityEngine.Random.value * 3f;
		if (strategicBuilding)
		{
			if (score < bestMediumPriorityDist)
			{
				bestMediumPriorityDist = score;
				bestMediumPriority = building;
			}
		}
		else if (score < bestLowPriorityDist)
		{
			bestLowPriorityDist = score;
			bestLowPriority = building;
		}
	}

	if (bestHighPriority != null)
	{
		return bestHighPriority;
	}
	if (bestMediumPriority != null)
	{
		return bestMediumPriority;
	}
	return bestLowPriority;
}

private static BaseSimObject GetValidExistingAirTarget(Actor actor)
{
	if (actor == null || !actor.has_attack_target || !actor.isEnemyTargetAlive() || actor.attack_target == null)
	{
		return null;
	}

	if (!actor.canAttackTarget(actor.attack_target, pCheckForFactions: true, pAttackBuildings: true))
	{
		return null;
	}

	if (ShouldProtectCivilianFromVehicle(actor, actor.attack_target))
	{
		return null;
	}

	return actor.attack_target;
}

private static bool ShouldRefreshAirTarget(Actor actor, BaseSimObject existingTarget)
{
	if (actor == null)
	{
		return true;
	}

	int refreshInterval = AirTargetRefreshInterval;
	if (existingTarget != null && !existingTarget.isActor())
	{
		refreshInterval = AirBuildingTargetRefreshInterval;
	}

	actor.data.get(BomberTargetRefreshTickKey, out int tick, 0);
	tick++;
	if (tick >= refreshInterval)
	{
		actor.data.set(BomberTargetRefreshTickKey, 0);
		return true;
	}

	actor.data.set(BomberTargetRefreshTickKey, tick);
	return false;
}

private static BaseSimObject AcquireAirEngageTarget(Actor actor, WorldTile searchCenter, float searchRadius)
{
	BaseSimObject existing = GetValidExistingAirTarget(actor);
	bool refreshNow = ShouldRefreshAirTarget(actor, existing);

	BaseSimObject preferred = FindPreferredAirTargetAroundTile(actor, searchCenter, searchRadius);
	if (preferred == null && actor != null && actor.current_tile != null && actor.current_tile != searchCenter)
	{
		preferred = FindPreferredAirTargetAroundTile(actor, actor.current_tile, Mathf.Max(searchRadius, 34f));
	}

	if (existing != null && !refreshNow)
	{
		if (!existing.isActor() && preferred != null && preferred.isActor())
		{
			return preferred;
		}
		return existing;
	}

	return preferred;
}

private static void ApplyAirEngageLock(Actor actor, BaseSimObject engageTarget)
{
	float engagementRange = Mathf.Max(6f, actor.stats["range"] + 4f);
	if (engageTarget != null)
	{
		float distToTarget = Vector2.Distance(actor.current_position, engageTarget.current_position);
		if (distToTarget <= engagementRange)
		{
			if (!actor.has_attack_target || actor.attack_target != engageTarget)
			{
				actor.setAttackTarget(engageTarget);
			}
		}
		else if (actor.has_attack_target)
		{
			actor.clearAttackTarget();
		}
	}
	else if (actor.has_attack_target)
	{
		actor.clearAttackTarget();
	}
}

private static bool TryFindEnemyPriorityTarget(Actor caster, out Vector2 targetPos)
{
	targetPos = default;
	if (caster == null || caster.current_tile == null || caster.kingdom == null)
	{
		return false;
	}

	Actor bestEnemy = null;
	float bestDist = float.MaxValue;
	foreach (Actor other in Finder.getUnitsFromChunk(caster.current_tile, 3, 35f))
	{
		if (other == null || other == caster || !other.isAlive() || other.kingdom == null)
		{
			continue;
		}
		if (!caster.kingdom.isEnemy(other.kingdom))
		{
			continue;
		}

		bool highValue = other.hasTrait("Unitpotential") || other.isWarrior();
		if (!highValue && other.profession_asset != null)
		{
			UnitProfession profession = other.profession_asset.profession_id;
			highValue = profession == UnitProfession.King || profession == UnitProfession.Leader;
		}
		if (!highValue)
		{
			continue;
		}

		float dist = Vector2.Distance(caster.current_position, other.current_position);
		if (dist < bestDist)
		{
			bestDist = dist;
			bestEnemy = other;
		}
	}

	if (bestEnemy != null)
	{
		targetPos = bestEnemy.current_position;
		return true;
	}

	using (var enemies = caster.kingdom.getEnemiesKingdoms())
	{
		foreach (var enemyKingdom in enemies)
		{
			if (enemyKingdom == null || enemyKingdom.cities.Count <= 0)
			{
				continue;
			}

			var targetCity = enemyKingdom.cities.GetRandom();
			if (targetCity == null)
			{
				continue;
			}

			float roll = UnityEngine.Random.value;
			if (roll < 0.33f && targetCity.hasLeader() && targetCity.leader.isAlive())
			{
				targetPos = targetCity.leader.current_position;
				return true;
			}
			if (roll < 0.66f && enemyKingdom.hasKing() && enemyKingdom.king.isAlive())
			{
				targetPos = enemyKingdom.king.current_position;
				return true;
			}
			if (targetCity.buildings.Count > 0)
			{
				var building = targetCity.buildings.GetRandom();
				if (building != null && building.current_tile != null)
				{
					targetPos = building.current_tile.pos;
					return true;
				}
			}

			var cityTile = targetCity.getTile();
			if (cityTile != null)
			{
				targetPos = cityTile.pos;
				return true;
			}
		}
	}

	return false;
}

private static bool BomberForceReloadRtbDecisionEffect(Actor actor)
{
	if (!TryGetAirVehicleProfile(actor, out _) || !actor.isAlive())
	{
		return false;
	}

	EnsureBomberState(actor);
	int ammo = GetBomberAmmo(actor);
	bool forceRtb = GetBomberBool(actor, BomberForceRtbKey);
	return ammo <= 0 || forceRtb;
}

private static bool BomberLandAndReloadDecisionEffect(Actor actor)
{
	if (!TryGetAirVehicleProfile(actor, out AirVehicleProfile profile) || !actor.isAlive())
	{
		return false;
	}

	EnsureBomberState(actor);
	int ammo = GetBomberAmmo(actor);
	bool forceRtb = GetBomberBool(actor, BomberForceRtbKey);
	return (forceRtb || ammo < profile.ammoMax) && GetBomberBool(actor, BomberLandedKey);
}

private static bool BomberTakeoffForWarDecisionEffect(Actor actor)
{
	if (!TryGetAirVehicleProfile(actor, out AirVehicleProfile profile) || !actor.isAlive())
	{
		return false;
	}

	EnsureBomberState(actor);
	if (GetBomberAmmo(actor) < profile.takeoffAmmoThreshold || GetBomberBool(actor, BomberForceRtbKey))
	{
		return false;
	}

	bool landed = GetBomberBool(actor, BomberLandedKey) || !actor.isFlying();
	bool warSignal = HasNearbyEnemy(actor, 35f);
	if (!landed || !warSignal)
	{
		return false;
	}
	return true;
}

private static bool BomberEngageEnemyTargetsDecisionEffect(Actor actor)
{
	if (!TryGetAirVehicleProfile(actor, out AirVehicleProfile profile) || !actor.isAlive())
	{
		return false;
	}

	EnsureBomberState(actor);
	if (GetBomberBool(actor, BomberForceRtbKey) || GetBomberAmmo(actor) < profile.takeoffAmmoThreshold)
	{
		return false;
	}
	if (!actor.isFlying())
	{
		return false;
	}
	if (actor.kingdom == null || !actor.kingdom.hasEnemies())
	{
		return false;
	}

	return HasNearbyEnemy(actor, 35f);
}

private static bool BomberPeaceStationDecisionEffect(Actor actor)
{
	if (!TryGetAirVehicleProfile(actor, out _) || !actor.isAlive())
	{
		return false;
	}

	EnsureBomberState(actor);
	if (GetBomberBool(actor, BomberForceRtbKey))
	{
		return false;
	}

	return !HasNearbyEnemy(actor, 35f);
}

private static void UpdateBomberHumanRuntime(Actor actor, float pElapsed)
{
	if (!TryGetAirVehicleProfile(actor, out AirVehicleProfile profile) || !actor.isAlive())
	{
		return;
	}

	EnsureBomberState(actor);

	int ammo = GetBomberAmmo(actor);
	bool forceRtb = GetBomberBool(actor, BomberForceRtbKey) || ammo <= 0;
	WorldTile baseTile = FindBomberBaseTile(actor);
	bool hasWar = actor.kingdom != null && actor.kingdom.hasEnemies();
	bool enemyNearby = HasNearbyEnemy(actor, 35f);
	bool canTakeoffFromLanded = ammo >= profile.takeoffAmmoThreshold && !forceRtb && (hasWar || enemyNearby);
	bool landed = GetBomberBool(actor, BomberLandedKey);

	if (landed && !canTakeoffFromLanded)
	{
		actor.clearAttackTarget();
		actor.stopMovement();
		actor.setFlying(false);
		SetBomberBool(actor, BomberLandedKey, true);
		RecoverHealthPercentOverTime(actor, BomberRepairPoolKey, pElapsed, AirVehicleRepairPercentPerSecond);
		if (GetBomberAmmo(actor) < profile.ammoMax && AdvanceReloadTimer(actor, BomberReloadTimerKey, pElapsed, profile.reloadDurationSeconds))
		{
			SetBomberAmmo(actor, profile.ammoMax);
			SetBomberBool(actor, BomberForceRtbKey, false);
		}
		else if (GetBomberAmmo(actor) >= profile.ammoMax)
		{
			ResetReloadTimer(actor, BomberReloadTimerKey);
		}
		return;
	}

	if (forceRtb)
	{
		SetBomberBool(actor, BomberForceRtbKey, true);
		actor.clearAttackTarget();

		if (baseTile != null && IsNearTile(actor, baseTile, profile.landingDistance))
		{
			actor.stopMovement();
			actor.setFlying(false);
			SetBomberBool(actor, BomberLandedKey, true);
			RecoverHealthPercentOverTime(actor, BomberRepairPoolKey, pElapsed, AirVehicleRepairPercentPerSecond);
			if (AdvanceReloadTimer(actor, BomberReloadTimerKey, pElapsed, profile.reloadDurationSeconds))
			{
				SetBomberAmmo(actor, profile.ammoMax);
				SetBomberBool(actor, BomberForceRtbKey, false);
			}
			return;
		}

		ResetReloadTimer(actor, BomberReloadTimerKey);
		actor.setFlying(true);
		SetBomberBool(actor, BomberLandedKey, false);
		if (baseTile != null && AdvanceBomberTick(actor, BomberNavTickKey, profile.navTickInterval))
		{
			actor.goTo(baseTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 8);
		}
		return;
	}

	WorldTile localDefenseCenter = actor.current_tile ?? baseTile;
	bool localDefenseMode = enemyNearby && ammo >= profile.takeoffAmmoThreshold;
	if (localDefenseMode)
	{
		actor.setFlying(true);
		SetBomberBool(actor, BomberLandedKey, false);
		ResetReloadTimer(actor, BomberReloadTimerKey);

		BaseSimObject localTarget = AcquireAirEngageTarget(actor, localDefenseCenter, 34f);
		ApplyAirEngageLock(actor, localTarget);

		WorldTile localNavTile = localTarget != null ? localTarget.current_tile : localDefenseCenter;
		if (localNavTile != null && !IsNearTile(actor, localNavTile, 6f) && AdvanceBomberTick(actor, BomberNavTickKey, profile.navTickInterval))
		{
			actor.goTo(localNavTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 10);
		}
		return;
	}

	City targetEnemyCity = FindNearestEnemyCity(actor);
	if (targetEnemyCity == null)
	{
		actor.clearAttackTarget();
		if (baseTile != null && IsNearTile(actor, baseTile, profile.landingDistance))
		{
			actor.stopMovement();
			actor.setFlying(false);
			SetBomberBool(actor, BomberLandedKey, true);
			RecoverHealthPercentOverTime(actor, BomberRepairPoolKey, pElapsed, AirVehicleRepairPercentPerSecond);
			if (GetBomberAmmo(actor) < profile.ammoMax && AdvanceReloadTimer(actor, BomberReloadTimerKey, pElapsed, profile.reloadDurationSeconds))
			{
				SetBomberAmmo(actor, profile.ammoMax);
				SetBomberBool(actor, BomberForceRtbKey, false);
			}
		}
		else if (baseTile != null)
		{
			ResetReloadTimer(actor, BomberReloadTimerKey);
			actor.setFlying(true);
			SetBomberBool(actor, BomberLandedKey, false);
			if (AdvanceBomberTick(actor, BomberNavTickKey, profile.navTickInterval))
			{
				actor.goTo(baseTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 8);
			}
		}
		return;
	}

	WorldTile enemyCityTile = targetEnemyCity.getTile();
	if (enemyCityTile == null)
	{
		return;
	}

	if (!hasWar && !enemyNearby)
	{
		if (baseTile != null && IsNearTile(actor, baseTile, profile.landingDistance))
		{
			actor.clearAttackTarget();
			actor.stopMovement();
			actor.setFlying(false);
			SetBomberBool(actor, BomberLandedKey, true);
			RecoverHealthPercentOverTime(actor, BomberRepairPoolKey, pElapsed, AirVehicleRepairPercentPerSecond);
			if (GetBomberAmmo(actor) < profile.ammoMax && AdvanceReloadTimer(actor, BomberReloadTimerKey, pElapsed, profile.reloadDurationSeconds))
			{
				SetBomberAmmo(actor, profile.ammoMax);
				SetBomberBool(actor, BomberForceRtbKey, false);
			}
		}
		else if (baseTile != null)
		{
			ResetReloadTimer(actor, BomberReloadTimerKey);
			actor.clearAttackTarget();
			actor.setFlying(true);
			SetBomberBool(actor, BomberLandedKey, false);
			if (AdvanceBomberTick(actor, BomberNavTickKey, profile.navTickInterval))
			{
				actor.goTo(baseTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 8);
			}
		}
		return;
	}

	actor.setFlying(true);
	SetBomberBool(actor, BomberLandedKey, false);
	ResetReloadTimer(actor, BomberReloadTimerKey);

	BaseSimObject engageTarget = AcquireAirEngageTarget(actor, enemyCityTile, 42f);
	ApplyAirEngageLock(actor, engageTarget);

	WorldTile navTile = engageTarget != null ? engageTarget.current_tile : enemyCityTile;
	if (navTile != null && !IsNearTile(actor, navTile, 6f) && AdvanceBomberTick(actor, BomberNavTickKey, profile.navTickInterval))
	{
		actor.goTo(navTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 10);
	}
}

private static void UpdateLandVehicleAmmoRuntime(Actor actor, float pElapsed)
{
	if (!TryGetLandVehicleAmmoProfile(actor, out LandVehicleAmmoProfile profile) || !actor.isAlive())
	{
		return;
	}

	EnsureLandVehicleAmmoState(actor);

	int ammo = GetLandVehicleAmmo(actor);
	bool forceReload = GetLandVehicleBool(actor, LandVehicleForceReloadKey) || ammo <= 0;
	if (!forceReload)
	{
		ResetReloadTimer(actor, LandVehicleReloadTimerKey);
		return;
	}

	SetLandVehicleBool(actor, LandVehicleForceReloadKey, true);
	actor.clearAttackTarget();

	WorldTile reloadTile = FindLandVehicleReloadTile(actor, out bool canReloadAtTarget);
	if (!canReloadAtTarget || reloadTile == null)
	{
		ResetReloadTimer(actor, LandVehicleReloadTimerKey);
		return;
	}

	if (IsNearTile(actor, reloadTile, profile.reloadDistance))
	{
		actor.stopMovement();
		RecoverHealthPercentOverTime(actor, LandVehicleRepairPoolKey, pElapsed, LandVehicleRepairPercentPerSecond);
		if (AdvanceReloadTimer(actor, LandVehicleReloadTimerKey, pElapsed, profile.reloadDurationSeconds))
		{
			SetLandVehicleAmmo(actor, profile.ammoMax);
			SetLandVehicleBool(actor, LandVehicleForceReloadKey, false);
		}
		return;
	}

	ResetReloadTimer(actor, LandVehicleReloadTimerKey);

	if (AdvanceBomberTick(actor, LandVehicleNavTickKey, profile.navTickInterval))
	{
		actor.goTo(reloadTile, pPathOnWater: false, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 8);
	}
}

public static bool MissileArtilleryEffect(BaseSimObject pTarget, WorldTile pTile = null)
{
    if (pTarget == null || !pTarget.isActor())
        return false;

    Actor caster = pTarget.a;
    if (!caster.isAlive() || !caster.kingdom.hasEnemies())
        return false;

    using (var enemies = caster.kingdom.getEnemiesKingdoms())
    {
        foreach (var enemyKingdom in enemies)
        {
            if (enemyKingdom.hasKing() && enemyKingdom.cities.Count > 0)
            {
                var targetCity = enemyKingdom.cities.GetRandom();
                if (targetCity != null)
                {

                    float roll = UnityEngine.Random.value;
                    Vector2? attackPos = null;

                    if (roll < 0.33f && targetCity.buildings.Count > 0)
                    {
                        var building = targetCity.buildings.GetRandom();
                        if (building != null && building.current_tile != null)
                            attackPos = building.current_tile.pos;
                    }

                    else if (roll < 0.66f && targetCity.hasLeader() && targetCity.leader.isAlive())
                    {
                        attackPos = targetCity.leader.current_position;
                    }

                    else if (enemyKingdom.hasKing() && enemyKingdom.king.isAlive())
                    {
                        attackPos = enemyKingdom.king.current_position;
                    }

                    if (attackPos == null)
                    {
                        var targetTile = targetCity.getTile();
                        if (targetTile != null)
                            attackPos = targetTile.pos;
                    }

                    if (attackPos != null)
                    {
                        Vector3 selfPos = caster.current_position;
                        float dist = Vector2.Distance(selfPos, attackPos.Value);
                        Vector3 attackVector = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, dist);
                        Vector3 startProjectile = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, caster.stats["size"]);
                        startProjectile.y += 0.5f;
                        World.world.projectiles.spawn(caster, null, "missileartillery", startProjectile, attackVector);
                        caster.punchTargetAnimation(attackVector, true, false, 45f);
                        return true;
                    }
                }
            }
        }
    }
    return false;
}


public static bool HORDEmissileArtilleryEffect(BaseSimObject pTarget, WorldTile pTile = null)
{
    if (pTarget == null || !pTarget.isActor())
        return false;

    Actor caster = pTarget.a;
    if (!caster.isAlive() || !caster.kingdom.hasEnemies())
        return false;

    using (var enemies = caster.kingdom.getEnemiesKingdoms())
    {
        foreach (var enemyKingdom in enemies)
        {
            if (enemyKingdom.hasKing() && enemyKingdom.cities.Count > 0)
            {
                var targetCity = enemyKingdom.cities.GetRandom();
                if (targetCity != null)
                {

                    float roll = UnityEngine.Random.value;
                    Vector2? attackPos = null;

                    if (roll < 0.33f && targetCity.buildings.Count > 0)
                    {
                        var building = targetCity.buildings.GetRandom();
                        if (building != null && building.current_tile != null)
                            attackPos = building.current_tile.pos;
                    }

                    else if (roll < 0.66f && targetCity.hasLeader() && targetCity.leader.isAlive())
                    {
                        attackPos = targetCity.leader.current_position;
                    }

                    else if (enemyKingdom.hasKing() && enemyKingdom.king.isAlive())
                    {
                        attackPos = enemyKingdom.king.current_position;
                    }

                    if (attackPos == null)
                    {
                        var targetTile = targetCity.getTile();
                        if (targetTile != null)
                            attackPos = targetTile.pos;
                    }

                    if (attackPos != null)
                    {
                        Vector3 selfPos = caster.current_position;
                        float dist = Vector2.Distance(selfPos, attackPos.Value);
                        Vector3 attackVector = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, dist);
                        Vector3 startProjectile = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, caster.stats["size"]);
                        startProjectile.y += 0.5f;
                        World.world.projectiles.spawn(caster, null, "fireboneartillery", startProjectile, attackVector);
                        caster.punchTargetAnimation(attackVector, true, false, 45f);
                        return true;
                    }
                }
            }
        }
    }
    return false;
}




public static bool GAIAmissileArtilleryEffect(BaseSimObject pTarget, WorldTile pTile = null)
{
    if (pTarget == null || !pTarget.isActor())
        return false;

    Actor caster = pTarget.a;
    if (!caster.isAlive() || !caster.kingdom.hasEnemies())
        return false;

    using (var enemies = caster.kingdom.getEnemiesKingdoms())
    {
        foreach (var enemyKingdom in enemies)
        {
            if (enemyKingdom.hasKing() && enemyKingdom.cities.Count > 0)
            {
                var targetCity = enemyKingdom.cities.GetRandom();
                if (targetCity != null)
                {

                    float roll = UnityEngine.Random.value;
                    Vector2? attackPos = null;

                    if (roll < 0.33f && targetCity.buildings.Count > 0)
                    {
                        var building = targetCity.buildings.GetRandom();
                        if (building != null && building.current_tile != null)
                            attackPos = building.current_tile.pos;
                    }

                    else if (roll < 0.66f && targetCity.hasLeader() && targetCity.leader.isAlive())
                    {
                        attackPos = targetCity.leader.current_position;
                    }

                    else if (enemyKingdom.hasKing() && enemyKingdom.king.isAlive())
                    {
                        attackPos = enemyKingdom.king.current_position;
                    }

                    if (attackPos == null)
                    {
                        var targetTile = targetCity.getTile();
                        if (targetTile != null)
                            attackPos = targetTile.pos;
                    }

                    if (attackPos != null)
                    {
                        Vector3 selfPos = caster.current_position;
                        float dist = Vector2.Distance(selfPos, attackPos.Value);
                        Vector3 attackVector = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, dist);
                        Vector3 startProjectile = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, caster.stats["size"]);
                        startProjectile.y += 0.5f;
                        World.world.projectiles.spawn(caster, null, "plantmissileartillery", startProjectile, attackVector);
                        caster.punchTargetAnimation(attackVector, true, false, 45f);
                        return true;
                    }
                }
            }
        }
    }
    return false;
}


public static bool HARDENmissileArtilleryEffect(BaseSimObject pTarget, WorldTile pTile = null)
{
    if (pTarget == null || !pTarget.isActor())
        return false;

    Actor caster = pTarget.a;
    if (!caster.isAlive() || !caster.kingdom.hasEnemies())
        return false;

    using (var enemies = caster.kingdom.getEnemiesKingdoms())
    {
        foreach (var enemyKingdom in enemies)
        {
            if (enemyKingdom.hasKing() && enemyKingdom.cities.Count > 0)
            {
                var targetCity = enemyKingdom.cities.GetRandom();
                if (targetCity != null)
                {

                    float roll = UnityEngine.Random.value;
                    Vector2? attackPos = null;

                    if (roll < 0.33f && targetCity.buildings.Count > 0)
                    {
                        var building = targetCity.buildings.GetRandom();
                        if (building != null && building.current_tile != null)
                            attackPos = building.current_tile.pos;
                    }

                    else if (roll < 0.66f && targetCity.hasLeader() && targetCity.leader.isAlive())
                    {
                        attackPos = targetCity.leader.current_position;
                    }

                    else if (enemyKingdom.hasKing() && enemyKingdom.king.isAlive())
                    {
                        attackPos = enemyKingdom.king.current_position;
                    }

                    if (attackPos == null)
                    {
                        var targetTile = targetCity.getTile();
                        if (targetTile != null)
                            attackPos = targetTile.pos;
                    }

                    if (attackPos != null)
                    {
                        Vector3 selfPos = caster.current_position;
                        float dist = Vector2.Distance(selfPos, attackPos.Value);
                        Vector3 attackVector = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, dist);
                        Vector3 startProjectile = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, caster.stats["size"]);
                        startProjectile.y += 0.5f;
                        World.world.projectiles.spawn(caster, null, "frostmissileartillery", startProjectile, attackVector);
                        caster.punchTargetAnimation(attackVector, true, false, 45f);
                        return true;
                    }
                }
            }
        }
    }
    return false;
}





public static bool NuclearMissileArtilleryEffect(BaseSimObject pTarget, WorldTile pTile = null)
{
	if (!nukesEnabled)
	{
	//	ModernBoxLogger.Log("Nukes disabled.");
		return false;
	}

    if (pTarget == null || !pTarget.isActor())
        return false;

    Actor caster = pTarget.a;
    if (!caster.isAlive() || !caster.kingdom.hasEnemies())
        return false;

    City ownerCity = caster.city;
    if (ownerCity == null || ownerCity.amount_gold < 50)
        return false;

    ownerCity.takeResource("gold", 50);

    using (var enemies = caster.kingdom.getEnemiesKingdoms())
    {
        foreach (var enemyKingdom in enemies)
        {
            if (enemyKingdom.hasKing() && enemyKingdom.cities.Count > 0)
            {
                var targetCity = enemyKingdom.cities.GetRandom();
                if (targetCity != null)
                {
                    float roll = UnityEngine.Random.value;
                    Vector2? attackPos = null;

                    if (roll < 0.33f && targetCity.buildings.Count > 0)
                    {
                        var building = targetCity.buildings.GetRandom();
                        if (building != null && building.current_tile != null)
                            attackPos = building.current_tile.pos;
                    }

                    else if (roll < 0.66f && targetCity.hasLeader() && targetCity.leader.isAlive())
                    {
                        attackPos = targetCity.leader.current_position;
                    }

                    else if (enemyKingdom.hasKing() && enemyKingdom.king.isAlive())
                    {
                        attackPos = enemyKingdom.king.current_position;
                    }

                    if (attackPos == null)
                    {
                        var targetTile = targetCity.getTile();
                        if (targetTile != null)
                            attackPos = targetTile.pos;
                    }

                    if (attackPos != null)
                    {
                        Vector3 selfPos = caster.current_position;
                        float dist = Vector2.Distance(selfPos, attackPos.Value);
                        Vector3 attackVector = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, dist);
                        Vector3 startProjectile = Toolbox.getNewPoint(selfPos.x, selfPos.y, attackPos.Value.x, attackPos.Value.y, caster.stats["size"]);
                        startProjectile.y += 0.5f;
						if (balls)
						{
							addNews("this wont work");
						}
                        World.world.projectiles.spawn(caster, null, "NUKER", startProjectile, attackVector);
						StatManager.Instance.SpawnUnit();
                        caster.punchTargetAnimation(attackVector, true, false, 45f);
                        return true;
                    }
                }
            }
        }
    }
    return false;
}

        public static void addNews(string news)
        {
            WorldLogMessage worldLogMessage = new WorldLogMessage { asset_id = "bigballs" };

            HistoryHud.instance.newHistory(worldLogMessage);
        }

public static bool AntiBossNuke(BaseSimObject pTarget, WorldTile pTile = null)
{
	if (!nukesEnabled)
	{
	//	ModernBoxLogger.Log("Nukes disabled.");
		return false;
	}

    if (pTarget == null || !pTarget.isActor())
        return false;

    Actor caster = pTarget.a;
    if (!caster.isAlive() || caster.kingdom == null)
        return false;

    City ownerCity = caster.city;
    if (ownerCity == null || ownerCity.amount_gold < 10)
        return false;

    ownerCity.takeResource("gold", 10);

    List<Actor> validTargets = new List<Actor>();
    foreach (var other in World.world.units)
    {
        if (other == null || !other.isAlive() || other == caster)
            continue;
        if (other.kingdom == null || caster.kingdom == null)
            continue;
        if (!caster.kingdom.isEnemy(other.kingdom))
            continue;
        if (other.stats["health"] >= 10000f)
            validTargets.Add(other);
    }

    if (validTargets.Count == 0)
        return false;

    Actor target = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];

    Vector3 start = caster.current_position;
    Vector3 end = target.current_position;
    float dist = Vector3.Distance(start, end);

    Vector3 attackVector = Toolbox.getNewPoint(start.x, start.y, end.x, end.y, dist);
    Vector3 startProjectile = Toolbox.getNewPoint(start.x, start.y, end.x, end.y, caster.stats["size"]);
    startProjectile.y += 0.5f;

    World.world.projectiles.spawn(caster, target, "NUKER", startProjectile, attackVector);
	StatManager.Instance.SpawnUnit();
    caster.punchTargetAnimation(attackVector, true, false, 45f);

    return true;
}

















		[HarmonyPatch(typeof(ActorAnimationLoader), nameof(ActorAnimationLoader.loadAnimationBoat))]
public static class Patch_ActorAnimationLoader_Fix
{
	static bool Prefix(string pTexturePath)
	{
		if (SpriteTextureLoader.getSpriteList("actors/boats/" + pTexturePath).Length == 0)
			return false;
		return true;
	}
}

		[HarmonyPatch(typeof(Actor), nameof(Actor.b6_updateAI))]
		public static class Patch_Actor_BomberHumanRuntime
		{
			[HarmonyPostfix]
			public static void Postfix(Actor __instance, float pElapsed)
			{
				try
				{
					UpdateBomberHumanRuntime(__instance, pElapsed);
					UpdateLandVehicleAmmoRuntime(__instance, pElapsed);
				}
				catch
				{
				}
			}
		}

		[HarmonyPatch(typeof(Actor), nameof(Actor.startAttackCooldown))]
		public static class Patch_Actor_BomberHumanAttackCounter
		{
			[HarmonyPostfix]
			public static void Postfix(Actor __instance)
			{
				if (!__instance.isAlive())
				{
					return;
				}

				if (TryGetAirVehicleProfile(__instance, out _))
				{
					EnsureBomberState(__instance);
					if (GetBomberBool(__instance, BomberForceRtbKey))
					{
						return;
					}

					int ammo = GetBomberAmmo(__instance);
					if (ammo <= 0)
					{
						SetBomberBool(__instance, BomberForceRtbKey, true);
						return;
					}

					ammo--;
					SetBomberAmmo(__instance, ammo);
					if (ammo <= 0)
					{
						SetBomberBool(__instance, BomberForceRtbKey, true);
						__instance.clearAttackTarget();
					}
					return;
				}

				if (!TryGetLandVehicleAmmoProfile(__instance, out _))
				{
					return;
				}

				EnsureLandVehicleAmmoState(__instance);
				if (GetLandVehicleBool(__instance, LandVehicleForceReloadKey))
				{
					__instance.clearAttackTarget();
					return;
				}

				int landAmmo = GetLandVehicleAmmo(__instance);
				if (landAmmo <= 0)
				{
					SetLandVehicleBool(__instance, LandVehicleForceReloadKey, true);
					__instance.clearAttackTarget();
					return;
				}

				landAmmo--;
				SetLandVehicleAmmo(__instance, landAmmo);
				if (landAmmo <= 0)
				{
					SetLandVehicleBool(__instance, LandVehicleForceReloadKey, true);
					__instance.clearAttackTarget();
				}
			}
		}

		[HarmonyPatch(typeof(Actor), nameof(Actor.setAttackTarget))]
		public static class Patch_Actor_LandVehicleAmmoPreventAttackTarget
		{
			[HarmonyPrefix]
			public static bool Prefix(Actor __instance, BaseSimObject pAttackTarget)
			{
				if (pAttackTarget == null)
				{
					return true;
				}

				if (IsVehicleActor(__instance) && ShouldProtectCivilianFromVehicle(__instance, pAttackTarget))
				{
					__instance.ignoreTarget(pAttackTarget);
					__instance.clearAttackTarget();
					return false;
				}

				if (!TryGetLandVehicleAmmoProfile(__instance, out _))
				{
					return true;
				}

				EnsureLandVehicleAmmoState(__instance);
				if (GetLandVehicleAmmo(__instance) > 0 && !GetLandVehicleBool(__instance, LandVehicleForceReloadKey))
				{
					return true;
				}

				__instance.clearAttackTarget();
				return false;
			}
		}

		[HarmonyPatch(typeof(City), "updateConquest")]
		public static class Patch_City_UpdateConquest_VehicleCaptureProxy
		{
			[HarmonyPrefix]
			public static bool Prefix(City __instance, Actor pActor)
			{
				if (pActor == null || !IsVehicleActor(pActor))
				{
					return true;
				}

				if (__instance == null || !pActor.isKingdomCiv() || pActor.kingdom == null || __instance.kingdom == null)
				{
					return false;
				}

				if (!(pActor.kingdom == __instance.kingdom || pActor.kingdom.isEnemy(__instance.kingdom)))
				{
					return false;
				}

				Actor proxy = FindVehicleCaptureProxyActor(pActor, __instance);
				if (proxy != null)
				{
					__instance.addCapturePoints(proxy, 1);
				}
				else
				{
					__instance.addCapturePoints(pActor.kingdom, 1);
				}

				return false;
			}
		}

		[HarmonyPatch(typeof(SimObjectsZones), "addUnit")]
		public static class Patch_SimObjectsZones_AddUnit_VehicleConquestBridge
		{
			[HarmonyPostfix]
			public static void Postfix(Actor pActor, WorldTile pTile)
			{
				if (pActor == null || !IsVehicleActor(pActor) || pActor.isInsideSomething())
				{
					return;
				}

				if (pActor.profession_asset != null && pActor.profession_asset.can_capture)
				{
					return;
				}

				City zoneCity = pTile != null ? pTile.zone_city : null;
				if (zoneCity == null || pActor.kingdom == null || !pActor.kingdom.isCiv() || zoneCity.kingdom == null)
				{
					return;
				}

				if (pActor.kingdom == zoneCity.kingdom || pActor.kingdom.isEnemy(zoneCity.kingdom))
				{
					zoneCity.updateConquest(pActor);
				}
			}
		}

		[HarmonyPatch(typeof(Actor), nameof(Actor.calculateMainSprite))]
		public static class Patch_Actor_BomberHumanLandedSprite
		{
			[HarmonyPostfix]
			public static void Postfix(Actor __instance, ref Sprite __result)
			{
				if (!TryGetAirVehicleProfile(__instance, out AirVehicleProfile profile))
				{
					return;
				}
				if (!GetBomberBool(__instance, BomberLandedKey))
				{
					return;
				}

				Sprite landedSprite = GetBomberLandedSprite(__instance, profile);
				if (landedSprite != null)
				{
					__result = landedSprite;
				}
			}
		}


       [HarmonyPatch(typeof(Actor), "setFamily")]
public static class Patch_Actor_Exclude_Unitpotential_Family
{
    static bool Prefix(Actor __instance, Family pObject)
    {
        if (__instance.hasTrait("Unitpotential"))
            return false;
        return true;
    }
}


[HarmonyPatch(typeof(Kingdom), "setKing")]
public static class Patch_Kingdom_Exclude_Unitpotential_King
{
    static bool Prefix(Kingdom __instance, Actor pActor)
    {
        if (pActor.hasTrait("Unitpotential"))
            return false;
        return true;
    }
}



[HarmonyPatch(typeof(City), "setLeader")]
public static class Patch_City_Exclude_Unitpotential_Leader
{
    static bool Prefix(City __instance, Actor pActor, bool pNew)
    {
        if (pActor.hasTrait("Unitpotential"))
            return false;
        return true;
    }
}



[HarmonyPatch(typeof(TileZone), nameof(TileZone.canBeClaimedByCity))]
public static class Patch_TileZone_CanBeClaimedByCity_Unitpotential
{
    static bool Prefix(TileZone __instance, City pCity, ref bool __result)
    {
        if (pCity != null && pCity.leader != null && pCity.leader.hasTrait("Unitpotential"))
        {
            __result = false;
            return false;
        }
        return true;
    }
}


[HarmonyPatch(typeof(TileZone), "isGoodForNewCity", new[] { typeof(Actor) })]
public static class Patch_TileZone_IsGoodForNewCity_Unitpotential
{
    static bool Prefix(TileZone __instance, Actor pActor, ref bool __result)
    {
        if (pActor != null && pActor.hasTrait("Unitpotential"))
        {
            __result = false;
            return false;
        }

        return true;
    }
}



[HarmonyPatch(typeof(Clan), "newClan")]
public static class Patch_Clan_NewClan
{
    static bool Prefix(Actor pFounder, bool pAddDefaultTraits)
    {
        return pFounder != null && !pFounder.hasTrait("Unitpotential");
    }
}


[HarmonyPatch(typeof(ai.behaviours.BehFightCheckEnemyIsOk), "execute")]
public static class BehFightCheckEnemyIsOk_Patch
{
    static bool Prefix(Actor pActor, ref BehResult __result)
    {
		if (!IsVehicleActor(pActor))
		{
			return true;
		}

        if (!pActor.has_attack_target || !pActor.isEnemyTargetAlive())
        {
            __result = BehResult.Stop;
            return false;
        }

		BaseSimObject tTarget = pActor.attack_target;
		if (tTarget == null)
        {
            __result = BehResult.Stop;
            return false;
        }

		if (!pActor.shouldContinueToAttackTarget())
        {
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

		if (ShouldProtectCivilianFromVehicle(pActor, tTarget))
        {
            pActor.ignoreTarget(tTarget);
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        if (!pActor.canAttackTarget(tTarget))
        {
            pActor.ignoreTarget(tTarget);
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        if (!pActor.isInAttackRange(tTarget))
        {
			if (pActor.isWaterCreature())
            {
				if ((!tTarget.isInLiquid() && !pActor.asset.force_land_creature) || tTarget.isFlying())
				{
					pActor.ignoreTarget(tTarget);
					pActor.clearAttackTarget();
					__result = BehResult.Stop;
					return false;
				}
			}
			else if ((tTarget.isInLiquid() && !pActor.isWaterCreature()) || tTarget.isFlying())
			{
				pActor.ignoreTarget(tTarget);
				pActor.clearAttackTarget();
				__result = BehResult.Stop;
				return false;
            }
        }

        if (Toolbox.Dist(pActor.chunk.x, pActor.chunk.y, tTarget.chunk.x, tTarget.chunk.y) >= SimGlobals.m.unit_chunk_sight_range + 1f)
        {
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        pActor.beh_actor_target = tTarget;
        __result = BehResult.Continue;
        return false;
    }
}


[HarmonyPatch(typeof(BehFindHouse), nameof(BehFindHouse.execute))]
public static class Patch_BehFindHouse_ExcludeVehicles
{
	static bool Prefix(Actor pActor, ref BehResult __result)
	{
		if (!IsVehicleActor(pActor))
		{
			return true;
		}

		if (pActor.hasHouse())
		{
			pActor.clearHomeBuilding();
		}

		__result = BehResult.Stop;
		return false;
	}
}


[HarmonyPatch(typeof(ai.behaviours.BehBuildingTargetHome), nameof(ai.behaviours.BehBuildingTargetHome.execute))]
public static class Patch_BehBuildingTargetHome_ExcludeVehicles
{
	static bool Prefix(Actor pActor, ref BehResult __result)
	{
		if (!IsVehicleActor(pActor))
		{
			return true;
		}

		if (pActor.hasHouse())
		{
			pActor.clearHomeBuilding();
		}

		__result = BehResult.Stop;
		return false;
	}
}


[HarmonyPatch(typeof(BehFindRandomFrontTileNearHouse), nameof(BehFindRandomFrontTileNearHouse.execute))]
public static class Patch_BehFindRandomFrontTileNearHouse_ExcludeVehicles
{
	static bool Prefix(Actor pActor, ref BehResult __result)
	{
		if (!IsVehicleActor(pActor))
		{
			return true;
		}

		__result = BehResult.Stop;
		return false;
	}
}



[HarmonyPatch(typeof(UtilityBasedDecisionSystem), "registerBasicDecisionLists")]
public static class Patch_UtilityBasedDecisionSystem_RegisterBasicDecisionLists
{
    static bool Prefix(Actor pActor, bool pGameplay)
    {
        if (pActor.asset.is_boat || pActor.hasTrait("Unitpotential"))
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.tryToCraftRandomWeapon))]
public class Patch_TryToCraftRandomWeapon
{
    static bool Prefix(Actor pActor, City pCity)
    {
        if (pActor.hasTrait("Unitpotential"))
        {
            return false;
        }

        if (pActor.equipment?.getSlot(EquipmentType.Weapon) == null)
        {
            return false;
        }

        return true;
    }
}


[HarmonyPatch]
public static class Patch_ItemCrafting_ExcludeUnitpotential
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.tryToCraftRandomWeapon))]
    public static bool Prefix_Weapon(Actor pActor, City pCity)
    {
        return isSafeToCraft(pActor, EquipmentType.Weapon);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.tryToCraftRandomArmor))]
    public static bool Prefix_Armor(Actor pActor, City pCity)
    {
        return isSafeToCraft(pActor, EquipmentType.Armor);
    }

    private static bool isSafeToCraft(Actor pActor, EquipmentType type)
    {
        if (pActor == null || pActor.hasTrait("Unitpotential"))
            return false;

        if (pActor.equipment == null)
            return false;

        if (pActor.equipment.getSlot(type) == null)
            return false;

        return true;
    }
}

/*
    [HarmonyPatch(typeof(Projectile), "targetReached")]
    public static class Projectile_TargetReached_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Projectile __instance)
        {
            var type = typeof(Projectile);
            var posField = type.GetField("_current_position_3d", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetField = type.GetField("_vector_target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (posField == null || targetField == null) return;

            Vector3 pos = (Vector3)posField.GetValue(__instance);
            Vector2 target = (Vector2)targetField.GetValue(__instance);

            if ((Mathf.Abs(pos.x - target.x) > 0.01f) || (Mathf.Abs(pos.y - target.y) > 0.01f))
            {
                pos.x = target.x;
                pos.y = target.y;
                posField.SetValue(__instance, pos);
            }
        }
    }


*/



        }
        }
