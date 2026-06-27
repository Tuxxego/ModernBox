//========= MODERNBOX 2.1.0.1 ============//
// Made by Tuxxego
//========================================//

using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using NCMS.Utils;
using NCMS;
using System.Linq;

namespace ModernBox
{
    public static class CustomItemsList
    {
        public static List<EquipmentAsset> CustomWeapons = new List<EquipmentAsset>();
        public static readonly Dictionary<string, string> WeaponEras = new Dictionary<string, string>
        {
            { "Glock17", "Modern" },
            { "AK", "Modern" },
            { "RPG", "Modern" },
            { "Minigun", "Modern" },
            { "Sniper", "Modern" },
            { "FAMAS", "Modern" },
            { "M4A1", "Modern" },
            { "ThompsonM1A1", "Modern" },
            { "SGT44", "Modern" },
            { "XM8", "Modern" },
            { "AK103", "Modern" },
            { "Uzi", "Modern" },
            { "Malorian", "Modern" },
            { "DesertEagle", "Modern" },
            { "M16", "Modern" },
            { "HK416", "Modern" },
            { "MP7", "Modern" },
            { "M32", "Modern" },
            { "Sluggershotgun", "Modern" },
            { "Americanshotgun", "Modern" },
            { "Flamethrower", "Modern" },
            { "vrifle", "Modern" },
            { "bigboy", "Modern" },
            { "grifle", "Modern" },
            { "MGL", "Modern" },
            { "greenheavyblaster", "Hyperfuture" },
            { "Musket", "Renaissance" },
            { "Flintlock", "Renaissance" },
            { "Crossbow", "Renaissance" },

            { "BudgetMIRV", "Modern" },
            { "DecentMIRV", "Modern" },
            { "MIRV", "Modern" },
            { "MIRVBomb", "Modern" },
            { "STRONGMIRV", "Modern" },

            { "BathSalts", "Modern" },
            { "Fentanyl", "Modern" },
            { "Morphine", "Modern" },
            { "Oxycodone", "Modern" },
            { "Ritalin", "Modern" }
        };
        public static bool GunsAllowed;
        public static bool MirvsAllowed;
        public static bool DrugsAllowed;
        public static bool MGLAllowed;
        
        public static readonly HashSet<string> Kys = new HashSet<string>
        {
            "BudgetMIRV",
            "DecentMIRV",
            "MIRV",
            "MIRVBomb",
            "STRONGMIRV"
        };

        public static readonly HashSet<string> Druggies = new HashSet<string>
        {
            "BathSalts",
            "Fentanyl",
            "Morphine",
            "Oxycodone",
            "Ritalin"
        };

        public static readonly HashSet<string> MGLs = new HashSet<string>
        {
            "MGL"
        };

        public static void InitCustomItems()
        {
            if (!AssetManager.items.dict.ContainsKey("Glock17"))
                return;

            CustomWeapons.Clear();
            CustomWeapons.Add(AssetManager.items.get("Glock17"));
            CustomWeapons.Add(AssetManager.items.get("AK"));
            CustomWeapons.Add(AssetManager.items.get("RPG"));
            CustomWeapons.Add(AssetManager.items.get("Minigun"));
            CustomWeapons.Add(AssetManager.items.get("Sniper"));
            CustomWeapons.Add(AssetManager.items.get("FAMAS"));
            CustomWeapons.Add(AssetManager.items.get("M4A1"));
            CustomWeapons.Add(AssetManager.items.get("ThompsonM1A1"));
            CustomWeapons.Add(AssetManager.items.get("SGT44"));
            CustomWeapons.Add(AssetManager.items.get("XM8"));
            CustomWeapons.Add(AssetManager.items.get("AK103"));
            CustomWeapons.Add(AssetManager.items.get("Uzi"));
            CustomWeapons.Add(AssetManager.items.get("Malorian"));
            CustomWeapons.Add(AssetManager.items.get("DesertEagle"));
            CustomWeapons.Add(AssetManager.items.get("M16"));
            CustomWeapons.Add(AssetManager.items.get("HK416"));
            CustomWeapons.Add(AssetManager.items.get("MP7"));
            CustomWeapons.Add(AssetManager.items.get("M32"));
            CustomWeapons.Add(AssetManager.items.get("Sluggershotgun"));
            CustomWeapons.Add(AssetManager.items.get("Americanshotgun"));
            CustomWeapons.Add(AssetManager.items.get("Flamethrower"));
            CustomWeapons.Add(AssetManager.items.get("vrifle"));
            CustomWeapons.Add(AssetManager.items.get("bigboy"));
            CustomWeapons.Add(AssetManager.items.get("grifle"));
            CustomWeapons.Add(AssetManager.items.get("MGL"));
            CustomWeapons.Add(AssetManager.items.get("BudgetMIRV"));
            CustomWeapons.Add(AssetManager.items.get("DecentMIRV"));
            CustomWeapons.Add(AssetManager.items.get("MIRV"));
            CustomWeapons.Add(AssetManager.items.get("MIRVBomb"));
            CustomWeapons.Add(AssetManager.items.get("STRONGMIRV"));
            CustomWeapons.Add(AssetManager.items.get("greenheavyblaster"));
            CustomWeapons.Add(AssetManager.items.get("Musket"));
            CustomWeapons.Add(AssetManager.items.get("Flintlock"));
            CustomWeapons.Add(AssetManager.items.get("Crossbow"));

            CustomWeapons.Add(AssetManager.items.get("BathSalts"));
            CustomWeapons.Add(AssetManager.items.get("Fentanyl"));
            CustomWeapons.Add(AssetManager.items.get("Morphine"));
            CustomWeapons.Add(AssetManager.items.get("Oxycodone"));
            CustomWeapons.Add(AssetManager.items.get("Ritalin"));
        }

        public static string GetEra()
        {

            string currentEra = StatManager.Instance.currentEra;

            if (string.IsNullOrEmpty(currentEra) || currentEra == "none")
                return "medieval";

            return currentEra;
        }

        public static bool IsWeaponAllowedForEra(EquipmentAsset asset, string era)
        {
            if (asset == null || era == null)
                return false;

            if (!WeaponEras.TryGetValue(asset.id, out string weaponEra))
                return false;

            return weaponEra == era;
        }

        public static void turnOnGuns() => GunsAllowed = true;
        public static void turnOffGuns() => GunsAllowed = false;

        public static void turnOnMIRVs() => MirvsAllowed = true;
        public static void turnOffMIRVs() => MirvsAllowed = false;
    
        public static void turnOnDrugs() => DrugsAllowed = true;
        public static void turnOffDrugs() => DrugsAllowed = false;
    
        public static void turnOnMGL() => MGLAllowed = true;
        public static void turnOffMGL() => MGLAllowed = false;

public static void toggleGuns()
        {
            Main.modifyBoolOption("GunOption", PowerButtons.GetToggleValue("gun_toggle"));
            if (PowerButtons.GetToggleValue("gun_toggle"))
            {
                turnOnGuns();
                return;
            }
            turnOffGuns();
        }
    

public static void toggleMIRVs()
        {
            Main.modifyBoolOption("MIRVOption", PowerButtons.GetToggleValue("mirv_toggle"));
            if (PowerButtons.GetToggleValue("mirv_toggle"))
            {
                turnOnMIRVs();
                return;
            }
            turnOffMIRVs();
        }
    

public static void toggleDrugs()
        {
            Main.modifyBoolOption("DrugsOption", PowerButtons.GetToggleValue("drugs_toggle"));
            if (PowerButtons.GetToggleValue("drugs_toggle"))
            {
                turnOnDrugs();
                return;
            }
            turnOffDrugs();
        }
    

public static void toggleMGL()
        {
            Main.modifyBoolOption("ChemOption", PowerButtons.GetToggleValue("mgltoggle"));
            if (PowerButtons.GetToggleValue("mgltoggle"))
            {
                turnOnMGL();
                return;
            }
            turnOffMGL();
        }
    }
    [HarmonyPatch(typeof(Culture), "getPreferredWeaponSubtypeIDs")]
    public class Patch_Culture_PreferredWeaponSubtypes
    {
        static bool Prefix(Culture __instance, ref string __result)
        {
            if (!CustomItemsList.GunsAllowed || CustomItemsList.CustomWeapons.Count == 0)
                return true;

            if (!CustomItemsList.MirvsAllowed && CustomItemsList.Kys.Contains(__result))
                return false;

            if (!CustomItemsList.DrugsAllowed && CustomItemsList.Druggies.Contains(__result))
                return false;

            if (!CustomItemsList.MGLAllowed && CustomItemsList.MGLs.Contains(__result))
                return false;

            __result = "firearm";
            return false;
        }
    }

    [HarmonyPatch(typeof(Culture), "getPreferredWeaponAssets")]
    public class Patch_Culture_PreferredWeaponAssets
    {
        static bool Prefix(Culture __instance, ref List<EquipmentAsset> __result)
        {
            if (!CustomItemsList.GunsAllowed || CustomItemsList.CustomWeapons.Count == 0)
                return true;

            string era = CustomItemsList.GetEra();
            if (era == null)
            {
                __result = new List<EquipmentAsset>();
                return false;
            }

            IEnumerable<EquipmentAsset> weapons =
                CustomItemsList.CustomWeapons
                    .Where(w => CustomItemsList.IsWeaponAllowedForEra(w, era));

            if (!CustomItemsList.MirvsAllowed)
                weapons = weapons.Where(w => !CustomItemsList.Kys.Contains(w.id));

            if (!CustomItemsList.DrugsAllowed)
                weapons = weapons.Where(w => !CustomItemsList.Druggies.Contains(w.id));
            
            if (!CustomItemsList.MGLAllowed)
                weapons = weapons.Where(w => !CustomItemsList.MGLs.Contains(w.id));

            __result = weapons.ToList();
            return false;
        }
    }

    [HarmonyPatch(typeof(Culture), "hasPreferredWeaponsToCraft")]
    public class Patch_Culture_HasPreferredWeaponsToCraft
    {
        static bool Prefix(Culture __instance, ref bool __result)
        {
            if (!CustomItemsList.GunsAllowed || CustomItemsList.CustomWeapons.Count == 0)
                return true;

            string era = CustomItemsList.GetEra();
            if (era == null)
            {
                __result = false;
                return false;
            }

            var validWeapons = CustomItemsList.CustomWeapons
                .Where(w => CustomItemsList.IsWeaponAllowedForEra(w, era));

            if (!CustomItemsList.MirvsAllowed)
                validWeapons = validWeapons.Where(w => !CustomItemsList.Kys.Contains(w.id));

            if (!CustomItemsList.DrugsAllowed)
                validWeapons = validWeapons.Where(w => !CustomItemsList.Druggies.Contains(w.id));

            if (!CustomItemsList.MGLAllowed)
                validWeapons = validWeapons.Where(w => !CustomItemsList.MGLs.Contains(w.id));

            __result = validWeapons.Any();
            return false;
        }
    }

    public class WeaponsProjectilesEffects : MonoBehaviour
    {
        public static void init()
        {
            // Deferred on purpose: sprites are filled lazily by EquipmentAsset_GetSpritesLazyLoadPatch.
        }

        public static void FixAllWeapons()
        {
      //      ModernBoxLogger.Log("[FixAllWeapons] Starting weapon sprite fix pass...");

            int totalChecked = 0;
            int totalFixed = 0;
            int totalSkipped = 0;

            foreach (var kvp in AssetManager.items.list)
            {
                string id = kvp.id;
                EquipmentAsset asset = kvp;

                if (asset == null)
                {
                    totalSkipped++;
                    continue;
                }

                totalChecked++;

                if (asset.gameplay_sprites == null || asset.gameplay_sprites.Length == 0)
                {
                    var sprites = FetchSprites(id);
                    asset.gameplay_sprites = sprites;

                    if (sprites != null && sprites.Length > 0)
                        totalFixed++;
                }
                else
                {
                    totalSkipped++;
                }
            }

         //   ModernBoxLogger.Log($"[FixAllWeapons] Done. Checked: {totalChecked}, Fixed: {totalFixed}, Skipped: {totalSkipped}");
        }

        public static Sprite[] FetchSprites(string id)
        {
            EquipmentAsset item = AssetManager.items.get(id);
            if (item == null)
                return Array.Empty<Sprite>();

            if (item.animated)
            {
                List<Sprite> spriteList = new List<Sprite>();
                int frameIndex = 0;
                bool foundFrames = false;

                while (true)
                {
                    string[] paths = new[]
                    {
                        $"weapons/{id}_{frameIndex}",
                        $"weapons/{id}{frameIndex}",
                        $"weapons/{id}/main_0_{frameIndex}"
                    };

                    bool frameFound = false;
                    foreach (string path in paths)
                    {
                        Sprite sprite = Resources.Load<Sprite>(path);
                        if (sprite != null)
                        {
                            spriteList.Add(sprite);
                            foundFrames = true;
                            frameIndex++;
                            frameFound = true;
                            break;
                        }
                    }

                    if (!frameFound) break;
                    if (frameIndex > 20) break;
                }

                if (!foundFrames)
                {
                    var fallback = Resources.LoadAll<Sprite>("weapons/" + id);
                    if (fallback != null && fallback.Length > 0)
                        return fallback;
                    else
                        return Array.Empty<Sprite>();
                }

                return spriteList.ToArray();
            }
            else
            {
                var sprite = Resources.Load<Sprite>("weapons/" + id);
                return sprite != null ? new Sprite[] { sprite } : Array.Empty<Sprite>();
            }
        }

        [HarmonyPatch(typeof(EquipmentAsset), nameof(EquipmentAsset.getSprites))]
        private static class EquipmentAsset_GetSpritesLazyLoadPatch
        {
            [HarmonyPostfix]
            private static void Postfix(EquipmentAsset __instance, ref Sprite[] __result)
            {
                if (__instance == null)
                {
                    return;
                }

                if (__result != null && __result.Length > 0)
                {
                    return;
                }

                if (__instance.gameplay_sprites != null && __instance.gameplay_sprites.Length > 0)
                {
                    __result = __instance.gameplay_sprites;
                    return;
                }

                if (string.IsNullOrEmpty(__instance.id))
                {
                    __result = Array.Empty<Sprite>();
                    return;
                }

                Sprite[] sprites = FetchSprites(__instance.id);
                __instance.gameplay_sprites = sprites ?? Array.Empty<Sprite>();
                __result = __instance.gameplay_sprites;
            }
        }
    }
}

