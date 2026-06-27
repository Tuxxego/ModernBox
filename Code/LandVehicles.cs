//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
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

namespace ModernBox
{
    class LandVehicles : MonoBehaviour
    {
		



        public static void init()
        {
            loadAssets();

        }
		
		


        private static void loadAssets()
        {
			

			

			

			




			
			var Tank = AssetManager.actor_library.clone("Tank","_mob");
			//ActorAsset heli = new ActorAsset();
           // Tank.get_override_sprite = AssetManager.actor_library.get("_boat").get_override_sprite;
			Tank.race = "human";
			Tank.kingdom = "ModernKingdom";
            Tank.base_stats[S.health] = 200f;
            Tank.base_stats[S.speed] = 100f;
            Tank.base_stats[S.armor] = 100f;
            Tank.base_stats[S.damage] = 40f;
            Tank.base_stats[S.scale] = 0.25f;
            Tank.base_stats[S.attack_speed] = 0;
			Tank.base_stats[S.range] = 150f;
			Tank.base_stats[S.knockback_reduction] = 300f;
            Tank.drawBoatMark_big = true;
            Tank.skipFightLogic = false;
            Tank.inspect_stats = true;
			Tank.landCreature = true;
			Tank.inspect_home = true;
			Tank.hideOnMinimap = false;
            Tank.drawBoatMark = true;
			Tank.can_edit_traits = false;
			Tank.canReceiveTraits = false;
			Tank.flying = false;
			//Tank.tech = "Tanks";
			Tank.very_high_flyer = false;
			Tank.defaultAttack = "RocketLauncher";
            Tank.isBoat = false;
			Tank.dieOnBlocks = true;
			Tank.ignoreBlocks = false;
			//Tank.moveFromBlock = false;
			Tank.procreate = false;
		    Tank.inspect_children = false;
            Tank.inspect_experience = true;
			Tank.canBeCitizen = true;
            Tank.inspect_kills = true;
            Tank.use_items = false;
			Tank.oceanCreature = false;
            Tank.take_items = false;
            Tank.nameLocale = "Tank";
            Tank.nameTemplate = "Jet_Names";
			Tank.job = "attacker";
            Tank.icon = "iconTank";
			AssetManager.actor_library.CallMethod("addTrait", "Vehicle");
			AssetManager.actor_library.CallMethod("addTrait", "Tank");
			AssetManager.actor_library.CallMethod("loadShadow", Tank);
			AssetManager.actor_library.CallMethod("addTrait", "immortal");
            Tank.animation_walk = "walk_0,walk_1,walk_2,walk_3";
            Tank.animation_swim = "walk_0,walk_1,walk_2,walk_3";
            Tank.texture_path = "Tank";
			AssetManager.actor_library.addColorSet("heliColor");
			Tank.color = Toolbox.makeColor("#33724D");
            AssetManager.actor_library.add(Tank);
			Localization.addLocalization(Tank.nameLocale, Tank.nameLocale);

			var MissileSystem = AssetManager.actor_library.clone("MissileSystem","_mob");
			//ActorAsset heli = new ActorAsset();
           // MissileSystem.get_override_sprite = AssetManager.actor_library.get("_boat").get_override_sprite;
			MissileSystem.race = "human";
			MissileSystem.kingdom = "ModernKingdom";
            MissileSystem.base_stats[S.health] = 200f;
            MissileSystem.base_stats[S.speed] = 100f;
            MissileSystem.base_stats[S.armor] = 100f;
            MissileSystem.base_stats[S.damage] = 40f;
            MissileSystem.base_stats[S.scale] = 0.3f;
            MissileSystem.base_stats[S.attack_speed] = 0;
			MissileSystem.base_stats[S.range] = 150f;
			MissileSystem.base_stats[S.knockback_reduction] = 300f;
            MissileSystem.drawBoatMark_big = true;
            MissileSystem.skipFightLogic = false;
            MissileSystem.inspect_stats = true;
			MissileSystem.landCreature = true;
			MissileSystem.inspect_home = true;
			MissileSystem.hideOnMinimap = false;
            MissileSystem.drawBoatMark = true;
			MissileSystem.can_edit_traits = false;
			MissileSystem.canReceiveTraits = false;
			MissileSystem.flying = false;
			//MissileSystem.tech = "MissileSystems";
			MissileSystem.very_high_flyer = false;
			MissileSystem.defaultAttack = "MIRV";
            MissileSystem.isBoat = false;
			MissileSystem.dieOnBlocks = false;
			MissileSystem.ignoreBlocks = true;
			//MissileSystem.moveFromBlock = false;
			MissileSystem.procreate = false;
		    MissileSystem.inspect_children = false;
            MissileSystem.inspect_experience = true;
			MissileSystem.canBeCitizen = true;
            MissileSystem.inspect_kills = true;
            MissileSystem.use_items = false;
			MissileSystem.oceanCreature = false;
            MissileSystem.take_items = false;
            MissileSystem.nameLocale = "MissileSystem";
            MissileSystem.nameTemplate = "Jet_Names";
			MissileSystem.job = "attacker";
            MissileSystem.icon = "iconMissileSystem";
			AssetManager.actor_library.CallMethod("addTrait", "Vehicle");
			//AssetManager.actor_library.CallMethod("addTrait", "MissileSystem");
			AssetManager.actor_library.CallMethod("loadShadow", MissileSystem);
			AssetManager.actor_library.CallMethod("addTrait", "immortal");
            MissileSystem.animation_walk = "walk_0,walk_1,walk_2,walk_3";
            MissileSystem.animation_swim = "walk_0,walk_1,walk_2,walk_3";
            MissileSystem.texture_path = "MissileSystem";
			AssetManager.actor_library.addColorSet("heliColor");
			MissileSystem.color = Toolbox.makeColor("#33724D");
            AssetManager.actor_library.add(MissileSystem);
			Localization.addLocalization(MissileSystem.nameLocale, MissileSystem.nameLocale);

            var Railgun = AssetManager.actor_library.clone("Railgun", "_mob");
            //ActorAsset heli = new ActorAsset();
            // Tank.get_override_sprite = AssetManager.actor_library.get("_boat").get_override_sprite;
            Railgun.race = "human";
            Railgun.kingdom = "ModernKingdom";
            Railgun.base_stats[S.health] = 600f;
            Railgun.base_stats[S.speed] = 80f;
            Railgun.base_stats[S.armor] = 100f;
            Railgun.base_stats[S.damage] = 60f;
            Railgun.base_stats[S.scale] = 0.25f;
            Railgun.base_stats[S.attack_speed] = 0;
            Railgun.base_stats[S.range] = 150f;
            Railgun.base_stats[S.knockback_reduction] = 320f;
            Railgun.drawBoatMark_big = true;
            Railgun.skipFightLogic = false;
            Railgun.inspect_stats = true;
            Railgun.landCreature = true;
            Railgun.inspect_home = true;
            Railgun.hideOnMinimap = false;
            Railgun.drawBoatMark = true;
            Railgun.can_edit_traits = false;
            Railgun.canReceiveTraits = false;
            Railgun.flying = false;
            //Tank.tech = "Tanks";
            Railgun.very_high_flyer = false;
            Railgun.defaultAttack = "RocketLauncher";
            Railgun.isBoat = false;
            Railgun.dieOnBlocks = true;
            Railgun.ignoreBlocks = false;
            //Tank.moveFromBlock = false;
            Railgun.procreate = false;
            Railgun.inspect_children = false;
            Railgun.inspect_experience = true;
            Railgun.canBeCitizen = true;
            Railgun.inspect_kills = true;
            Railgun.use_items = false;
            Railgun.oceanCreature = false;
            Railgun.take_items = false;
            Railgun.nameLocale = "Railgun";
            Railgun.nameTemplate = "Jet_Names";
            Railgun.job = "attacker";
            Railgun.icon = "iconRailgun";
            AssetManager.actor_library.CallMethod("addTrait", "Vehicle");
            AssetManager.actor_library.CallMethod("addTrait", "Railgun");
            AssetManager.actor_library.CallMethod("loadShadow", Railgun);
            AssetManager.actor_library.CallMethod("addTrait", "immortal");
            Railgun.animation_walk = "walk_0,walk_1,walk_2,walk_3";
            Railgun.animation_idle = "walk_0";
            Railgun.animation_swim = "walk_0,walk_1,walk_2,walk_3";
            Railgun.texture_path = "Railgun";
            AssetManager.actor_library.addColorSet("heliColor");
            Railgun.color = Toolbox.makeColor("#33724D");
            AssetManager.actor_library.add(Railgun);
            Localization.addLocalization(Railgun.nameLocale, Railgun.nameLocale);

            var Humvee = AssetManager.actor_library.clone("Humvee","_mob");
			//ActorAsset heli = new ActorAsset();
           // Humvee.get_override_sprite = AssetManager.actor_library.get("_boat").get_override_sprite;
			Humvee.race = "human";
			Humvee.kingdom = "ModernKingdom";
            Humvee.base_stats[S.health] = 200f;
            Humvee.base_stats[S.speed] = 100f;
            Humvee.base_stats[S.armor] = 100f;
            Humvee.base_stats[S.damage] = 40f;
            Humvee.base_stats[S.scale] = 0.2f;
            Humvee.base_stats[S.attack_speed] = 0;
			Humvee.base_stats[S.range] = 150f;
			Humvee.base_stats[S.knockback_reduction] = 300f;
            Humvee.drawBoatMark_big = true;
            Humvee.skipFightLogic = false;
            Humvee.inspect_stats = true;
			Humvee.landCreature = true;
			Humvee.inspect_home = true;
			Humvee.hideOnMinimap = false;
            Humvee.drawBoatMark = true;
			Humvee.can_edit_traits = false;
			Humvee.canReceiveTraits = false;
			Humvee.flying = false;
			//Humvee.tech = "Humvees";
			Humvee.very_high_flyer = false;
            Humvee.isBoat = false;
			Humvee.dieOnBlocks = false;
			Humvee.ignoreBlocks = false;
			Humvee.moveFromBlock = true;
			Humvee.procreate = false;
		    Humvee.inspect_children = false;
            Humvee.inspect_experience = true;
			Humvee.defaultAttack = "Minigun";
			Humvee.canBeCitizen = true;
            Humvee.inspect_kills = true;
            Humvee.use_items = false;
			Humvee.oceanCreature = false;
            Humvee.take_items = false;
            Humvee.nameLocale = "Humvee";
            Humvee.nameTemplate = "Humvee_Names";
			Humvee.job = "attacker";
            Humvee.icon = "iconHumvee";
			AssetManager.actor_library.CallMethod("addTrait", "Humvee");
			AssetManager.actor_library.CallMethod("addTrait", "Vehicle");
			AssetManager.actor_library.CallMethod("loadShadow", Humvee);
			AssetManager.actor_library.CallMethod("addTrait", "immortal");
            Humvee.animation_walk = "walk_0,walk_1,walk_2,walk_3";
			Humvee.animation_idle = "walk_0";
            Humvee.animation_swim = "walk_0,walk_1,walk_2,walk_3";
            Humvee.texture_path = "Humvee";
			AssetManager.actor_library.addColorSet("heliColor");
			Humvee.color = Toolbox.makeColor("#33724D");
            AssetManager.actor_library.add(Humvee);
			Localization.addLocalization(Humvee.nameLocale, Humvee.nameLocale);
		  

		  


		}
		}
}