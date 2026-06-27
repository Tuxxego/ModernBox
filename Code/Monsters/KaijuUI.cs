using UnityEngine;

using System.Collections.Generic;
using UnityEngine.Events;

using UnityEngine.UI;
using System.Collections;
using NCMS.Utils;
using NCMS;
using ReflectionUtility;
using TuxModLoader.Reflection;
using System.Reflection;
using System;

namespace ModernBox
{
	public class KaijuUI
	{

		public void init()
		{
			PowersTab tab = KaijugetPowersTab("Tab_kaiju");
      if (tab == null)
      {
        return;
      }

			GameObject largeImageObject = new GameObject("LargeImage");
			largeImageObject.transform.SetParent(tab.transform);
			largeImageObject.transform.localPosition = new Vector3(396, 18, 0);
			largeImageObject.transform.localScale = Vector3.one;

			Image largeImage = largeImageObject.AddComponent<Image>();
			largeImage.sprite = Resources.Load<Sprite>("ui/Icons/TabTextKaiju");

			RectTransform imageRect = largeImageObject.GetComponent<RectTransform>();
			imageRect.sizeDelta = new Vector2(200, 100);
			imageRect.anchorMin = new Vector2(0.5f, 0.5f);
			imageRect.anchorMax = new Vector2(0.5f, 0.5f);




             ////////////////////////ANGELS///////////////////////////////////////


new ButtonBuilder("spawnRamiel")
    .AsUnitSpawner("Ramiel")
      .SetTitle("Ramiel")
      .SetDescription("Thunder of God")
    .SetGodPowerIconPath("actors/Avatars/Ramiel_avatar")
    .SetPosition(0, 0)
    .SetTransform(tab.transform)
    .Build();


    new ButtonBuilder("spawnGaghiel")
    .AsUnitSpawner("Gaghiel")
    .SetTitle("Gaghiel")
    .SetDescription("Thunder of God")
    .SetGodPowerIconPath("actors/Avatars/Gaghiel_avatar")
    .SetPosition(0, 1)
    .SetTransform(tab.transform)
    .Build();



    new ButtonBuilder("spawnSachiel")
    .AsUnitSpawner("Sachiel")
    .SetTitle("Sachiel")
    .SetDescription("Thunder of God")
    .SetGodPowerIconPath("actors/Avatars/Sachiel_avatar")
    .SetPosition(1, 0)
    .SetTransform(tab.transform)
    .Build();


    new ButtonBuilder("spawnZeruel")
    .AsUnitSpawner("Zeruel")
    .SetTitle("Zeruel")
    .SetDescription("Thunder of God")
    .SetGodPowerIconPath("actors/Avatars/Zeruel_avatar")
    .SetPosition(1, 1)
    .SetTransform(tab.transform)
    .Build();

            ///////////////////////////KAIJU////////////////////////


new ButtonBuilder("spawnGojira")
    .AsUnitSpawner("Gojira")
    .SetGodPowerName("Bad Godzilla")
    .SetDescription("Corrupted King of the Monsters")
    .SetGodPowerIconPath("actors/Kaiju/Gojira/main/walk_0")
    .SetPosition(14, 0)
    .SetTransform(tab.transform)
    .Build();

    new ButtonBuilder("spawnLonglegder")
    .AsUnitSpawner("Longlegder")
    .SetGodPowerName("Longlegder")
    .SetDescription("Rightful King of the Monsters")
    .SetGodPowerIconPath("actors/Avatars/Longlegder_avatar")
    .SetPosition(14, 1)
    .SetTransform(tab.transform)
    .Build();

    new ButtonBuilder("spawnRodanix")
    .AsUnitSpawner("Rodanix")
    .SetGodPowerName("Rodanix")
    .SetDescription("Rightful King of the Monsters")
    .SetGodPowerIconPath("actors/Avatars/Rodanix_avatar")
    .SetPosition(15, 0)
    .SetTransform(tab.transform)
    .Build();

    new ButtonBuilder("spawnInvaderax")
    .AsUnitSpawner("Invaderax")
    .SetGodPowerName("Ghidorah")
    .SetDescription("Storm dragon of the monsters")
    .SetGodPowerIconPath("actors/Kaiju/Invaderax/main/walk_0")
    .SetPosition(15, 1)
    .SetTransform(tab.transform)
    .Build();


    new ButtonBuilder("spawnPanKong")
    .AsUnitSpawner("PanKong")
    .SetGodPowerName("PanKong")
    .SetDescription("Rightful King of the Monsters")
    .SetGodPowerIconPath("actors/Avatars/PanKong_avatar")
    .SetPosition(16, 0)
    .SetTransform(tab.transform)
    .Build();


    new ButtonBuilder("spawnMegaGojira")
    .AsUnitSpawner("MegaGojira")
    .SetGodPowerName("Godzilla Earth")
    .SetDescription("Planet-sized King of the Monsters")
    .SetGodPowerIconPath("actors/Kaiju/MegaGojira/main/walk_0")
    .SetPosition(16, 1)
    .SetTransform(tab.transform)
    .Build();

    new ButtonBuilder("spawnSkullcrawler")
    .AsUnitSpawner("Skullcrawler")
    .SetGodPowerName("Skullcrawler")
    .SetDescription("Rightful King of the Monsters")
    .SetGodPowerIconPath("actors/Avatars/Skullcrawler_avatar")
    .SetPosition(17, 0)
    .SetTransform(tab.transform)
    .Build();


    new ButtonBuilder("spawncrabzilord")
    .AsUnitSpawner("crabzilord")
    .SetGodPowerName("crabzilord")
    .SetDescription("Rightful King of the Monsters")
    .SetGodPowerIconPath("actors/Avatars/crabzilord_avatar")
    .SetPosition(17, 1)
    .SetTransform(tab.transform)
    .Build();

    new ButtonBuilder("spawnmechacrabzilla")
    .AsUnitSpawner("mechacrabzilla")
    .SetGodPowerName("mechacrabzilla")
    .SetDescription("Rightful King of the Monsters")
    .SetGodPowerIconPath("actors/Avatars/mechacrabzilla_avatar")
    .SetPosition(20, 0)
    .SetTransform(tab.transform)
    .Build();

            int archiveIndex = 0;
            foreach (ArchiveKaijuSpawnEntry entry in Kaiju.GetArchiveKaijuSpawnEntries())
            {
                int column = 21 + (archiveIndex / 6);
                int row = archiveIndex % 6;
                new ButtonBuilder(entry.PowerId)
                    .AsUnitSpawner(entry.ActorId)
                    .SetGodPowerName(entry.DisplayName)
                    .SetDescription(entry.Description)
                    .SetGodPowerIconPath(entry.IconPath)
                    .SetPosition(column, row)
                    .SetTransform(tab.transform)
                    .Build();
                archiveIndex++;
            }


		}





          public static PowersTab KaijugetPowersTab(string id) {
          GameObject gameObject = GameObjects.FindEvenInactive(id);
          if (gameObject == null)
          {
            return null;
          }
          return gameObject.GetComponent<PowersTab>();
        }

    }

}
