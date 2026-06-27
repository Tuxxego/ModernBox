using HarmonyLib;
using ReflectionUtility;
using System;
using NCMS.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Reflection.Emit;

namespace ModernBox
{
    class ModernKingdoms
    {
               
        internal void init()
        {
            
         
            AssetManager.kingdoms.add(new KingdomAsset
            {
                id = "empty",
                civ = true
            });
            AssetManager.kingdoms.add(new KingdomAsset
            {
                id = "nomads_empty",
                nomads = false,
                mobs = false
            });


            #region ModernKingdom
            KingdomAsset ModernKingdom = AssetManager.kingdoms.clone("ModernKingdom", "empty");
            ModernKingdom.addTag("civ");
            ModernKingdom.addTag("ModernKingdom");
            ModernKingdom.addEnemyTag("ModernKingdom");
            ModernKingdom.addEnemyTag("nomads_ModernKingdom");
            ModernKingdom.addEnemyTag("human");
            ModernKingdom.addEnemyTag("orc");
            ModernKingdom.addEnemyTag("elf");
            ModernKingdom.addEnemyTag("dwarf");
            newHiddenKingdom(ModernKingdom);

            KingdomAsset ModernKingdom1 = AssetManager.kingdoms.clone("nomads_ModernKingdom", "nomads_empty");
            ModernKingdom1.addTag("civ");
            ModernKingdom1.addTag("ModernKingdom");
            ModernKingdom1.addEnemyTag("ModernKingdom");
            ModernKingdom1.addEnemyTag("nomads_ModernKingdom");
            ModernKingdom1.addEnemyTag("human");
            ModernKingdom1.addEnemyTag("orc");
            ModernKingdom1.addEnemyTag("elf");
            ModernKingdom1.addEnemyTag("dwarf");
            newHiddenKingdom(ModernKingdom1);
            
        var kingdomAsset = AssetManager.kingdoms.get("human");
            kingdomAsset.addEnemyTag("ModernKingdom");
            kingdomAsset = AssetManager.kingdoms.get("elf");
            kingdomAsset.addEnemyTag("ModernKingdom");
            kingdomAsset = AssetManager.kingdoms.get("dwarf");
            kingdomAsset.addEnemyTag("ModernKingdom");
            kingdomAsset = AssetManager.kingdoms.get("orc");
            kingdomAsset.addEnemyTag("ModernKingdom");
               

                var kingdomNomadsAsset = AssetManager.kingdoms.get("nomads_human");
            kingdomNomadsAsset.addEnemyTag("ModernKingdom");
            kingdomNomadsAsset.addEnemyTag("nomads_ModernKingdom");
            kingdomNomadsAsset = AssetManager.kingdoms.get("nomads_elf");
            kingdomNomadsAsset.addEnemyTag("ModernKingdom");
            kingdomNomadsAsset.addEnemyTag("nomads_ModernKingdom");
            kingdomNomadsAsset = AssetManager.kingdoms.get("nomads_orc");
            kingdomNomadsAsset.addEnemyTag("ModernKingdom");
            kingdomNomadsAsset.addEnemyTag("nomads_ModernKingdom");
            kingdomNomadsAsset = AssetManager.kingdoms.get("nomads_dwarf");
            kingdomNomadsAsset.addEnemyTag("ModernKingdom");
            kingdomNomadsAsset.addEnemyTag("nomads_ModernKingdom");


                  kingdomNomadsAsset = AssetManager.kingdoms.get("nomads_ModernKingdom");
            kingdomNomadsAsset.addEnemyTag("ModernKingdom");
            kingdomNomadsAsset.addEnemyTag("elf");
            kingdomNomadsAsset.addEnemyTag("human");
            kingdomNomadsAsset.addEnemyTag("orc");
            kingdomNomadsAsset.addEnemyTag("dwarf");

            #endregion
			  


        }
        private Kingdom newHiddenKingdom(KingdomAsset pAsset)
        {
            Kingdom kingdom = World.world.kingdoms.newObject(pAsset.id);
            kingdom.asset = pAsset;
            kingdom.createHidden();
           // kingdom.id = pAsset.id;
            kingdom.data.name = pAsset.id;
            KingdomManager kingdomManager = MapBox.instance.kingdoms;
            kingdomManager.setupKingdom(kingdom, false);
            return kingdom;
        }
        
        }}