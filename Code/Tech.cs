//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Reflection;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using NCMS.Utils;
using System.Runtime.CompilerServices;
using DG.Tweening;
using EpPathFinding.cs;
using life.taxi;
using SleekRender;
using tools.debug;
using UnityEngine.EventSystems;
using WorldBoxConsole;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using System.ComponentModel;

namespace ModernBox
{
    public class Tech
    {
        public static void init(){

            Harmony harmony = new Harmony("Brug");
            MethodInfo original = AccessTools.Method(typeof(Culture), "haveRequiredTechFor");
            MethodInfo postfix = AccessTools.Method(typeof(Tech), "haveRequiredTechFor_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(postfix));
            





            CultureTechAsset modern_military = new CultureTechAsset();
            modern_military.id = "MilitaryModern";
            modern_military.path_icon = "force";
            modern_military.priority = false;
            modern_military.required_level = 60;
            modern_military.requirements = new List<string>();
            Localization.addLocalization("tech_MilitaryModern", "Modern Military");
            modern_military.stats.bonus_max_army.add(5000f);
            AssetManager.culture_tech.add(modern_military);

            CultureTechAsset Firearms = new CultureTechAsset();
            Firearms.id = "Firearms";
            Firearms.path_icon = "firearm";
            Firearms.priority = false;
            Firearms.required_level = 65;
            Localization.addLocalization("tech_Firearms", "Better Firearm Production");
            AssetManager.culture_tech.add(Firearms);
			
			CultureTechAsset LowFirearms = new CultureTechAsset();
            LowFirearms.id = "LowFirearms";
            LowFirearms.path_icon = "lowfirearm";
            LowFirearms.priority = false;
            LowFirearms.required_level = 55;
            Localization.addLocalization("tech_LowFirearms", "Pipe Guns");
            AssetManager.culture_tech.add(LowFirearms);


			
		    CultureTechAsset Cyberware = new CultureTechAsset();
            Cyberware.id = "Cyberware";
            Cyberware.path_icon = "Cyberware";
            Cyberware.priority = true;
            Cyberware.required_level = 70;
            Localization.addLocalization("tech_Cyberware", "Cyberware");
            AssetManager.culture_tech.add(Cyberware);

            CultureTechAsset MIRV = new CultureTechAsset();
            MIRV.id = "MIRV";
            MIRV.path_icon = "MIRV";
            MIRV.priority = false;
            MIRV.required_level = 80;
            Localization.addLocalization("tech_MIRV", "Long Range Ballistic MIRVS");
            AssetManager.culture_tech.add(MIRV);
			
			CultureTechAsset BudgetMIRV = new CultureTechAsset();
            BudgetMIRV.id = "BudgetMIRV";
            BudgetMIRV.path_icon = "BudgetMIRV";
            BudgetMIRV.priority = false;
            BudgetMIRV.required_level = 65;
            Localization.addLocalization("tech_BudgetMIRV", "Short Range MIRVS");
            AssetManager.culture_tech.add(BudgetMIRV);
			
			CultureTechAsset DecentMIRV = new CultureTechAsset();
            DecentMIRV.id = "DecentMIRV";
            DecentMIRV.path_icon = "DecentMIRV";
            DecentMIRV.priority = false;
            DecentMIRV.required_level = 70;
            Localization.addLocalization("tech_DecentMIRV", "Short Range MIRVS");
            AssetManager.culture_tech.add(DecentMIRV);

            CultureTechAsset Skyscrapers = new CultureTechAsset();
            Skyscrapers.id = "Skyscraper";
            Skyscrapers.path_icon = "Skyscraper";
            Skyscrapers.priority = false;
            Skyscrapers.required_level = 65;
            Localization.addLocalization("tech_Skyscrapers", "Skyscrapers");
            AssetManager.culture_tech.add(Skyscrapers);

            CultureTechAsset Casino = new CultureTechAsset();
            Casino.id = "Casino";
            Casino.path_icon = "Casino";
            Casino.priority = false;
            Casino.required_level = 40;
            Localization.addLocalization("tech_Casino", "Casino");
            AssetManager.culture_tech.add(Casino);
			
			CultureTechAsset STRONGMIRV = new CultureTechAsset();
            STRONGMIRV.id = "STRONGMIRV";
            STRONGMIRV.path_icon = "MIRV";
            STRONGMIRV.priority = false;
            STRONGMIRV.required_level = 90;
            Localization.addLocalization("tech_STRONGMIRV", "Nuclear MIRVs");
            AssetManager.culture_tech.add(STRONGMIRV);
			
			CultureTechAsset Tanks = new CultureTechAsset();
            Tanks.id = "Tanks";
            Tanks.path_icon = "Tank";
            Tanks.priority = false;
            Tanks.required_level = 65;
            Localization.addLocalization("tech_Tanks", "Tanks");
            AssetManager.culture_tech.add(Tanks);
			
			CultureTechAsset Helicopter = new CultureTechAsset();
            Helicopter.id = "Helicopter";
            Helicopter.path_icon = "Heli";
            Helicopter.priority = false;
            Helicopter.required_level = 65;
            Localization.addLocalization("tech_Helicopter", "Helicopters");
            AssetManager.culture_tech.add(Helicopter);
			
			CultureTechAsset FighterJet = new CultureTechAsset();
            FighterJet.id = "FighterJet";
            FighterJet.path_icon = "F22";
            FighterJet.priority = false;
            FighterJet.required_level = 65;
            Localization.addLocalization("tech_FighterJet", "Fighter Jets");
            AssetManager.culture_tech.add(FighterJet);

			CultureTechAsset MIRVBomber = new CultureTechAsset();
            MIRVBomber.id = "MIRVBomber";
            MIRVBomber.path_icon = "MIRVBomber";
            MIRVBomber.priority = false;
            MIRVBomber.required_level = 65;
            Localization.addLocalization("tech_MIRVBomber", "Fighter Jets");
            AssetManager.culture_tech.add(MIRVBomber);
			
			CultureTechAsset F55 = new CultureTechAsset();
            F55.id = "F55";
            F55.path_icon = "F55";
            F55.priority = false;
            F55.required_level = 65;
            Localization.addLocalization("tech_F55", "F55");
            AssetManager.culture_tech.add(F55);

			CultureTechAsset Gunship = new CultureTechAsset();
            Gunship.id = "Gunship";
            Gunship.path_icon = "Gunship";
            Gunship.priority = false;
            Gunship.required_level = 70;
            Localization.addLocalization("tech_Gunship", "Gunship");
            AssetManager.culture_tech.add(Gunship);
			
			CultureTechAsset Drone = new CultureTechAsset();
            Drone.id = "Drone";
            Drone.path_icon = "Drone";
            Drone.priority = false;
            Drone.required_level = 65;
            Localization.addLocalization("tech_Drone", "Drone");
            AssetManager.culture_tech.add(Drone);
			
			CultureTechAsset Nukes = new CultureTechAsset();
            Nukes.id = "Nukes";
            Nukes.path_icon = "Nuke";
            Nukes.priority = false;
            Nukes.required_level = 65;
            Localization.addLocalization("tech_Nukes", "Nukes");
            AssetManager.culture_tech.add(Nukes);
        }
        
        public static void haveRequiredTechFor_Postfix(Culture __instance, ref bool __result, CultureTechAsset pTech){
            if (pTech.requirements == null) return;
                for (int i = 0; i < pTech.requirements.Count; ++i) {
                    string item = pTech.requirements[i];
                    if (!__instance.data.list_tech_ids.Contains(item[0] == '!' ? item.Substring(1) : item)) {
                        if (item[0] != '!') {
                            __result = false;
                            return;
                        }
                    } else {
                        if (item[0] == '!') {
                            __result = false;
                            return;
                        }
                    }
                }
        __result = true;

        }

    }
}