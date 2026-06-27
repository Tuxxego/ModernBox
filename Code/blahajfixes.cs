using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace ModernBox
{
    [HarmonyPatch(typeof(ItemCrafting), "getItemAssetToCraft")]
    internal static class FanpatchGetItemToCraftPatch
    {
        private static void Prefix(ref List<EquipmentAsset> pItemList)
        {
            if (pItemList == null || pItemList.Count == 0)
            {
                return;
            }

            try
            {
                bool gunsAllowed = CustomItemsList.GunsAllowed;
                bool mirvsAllowed = CustomItemsList.MirvsAllowed;
                string currentEra = CustomItemsList.GetEra();
                Dictionary<string, string> weaponEras = CustomItemsList.WeaponEras;

                for (int i = pItemList.Count - 1; i >= 0; i--)
                {
                    EquipmentAsset item = pItemList[i];
                    if (item == null)
                    {
                        continue;
                    }

                    if (!gunsAllowed && weaponEras.ContainsKey(item.id))
                    {
                        pItemList.RemoveAt(i);
                        continue;
                    }

                    if (!mirvsAllowed && CustomItemsList.Kys.Contains(item.id))
                    {
                        pItemList.RemoveAt(i);
                        continue;
                    }

                    if (weaponEras.ContainsKey(item.id) && !CustomItemsList.IsWeaponAllowedForEra(item, currentEra))
                    {
                        pItemList.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[FanpatchFixes] getItemAssetToCraft patch failed: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(Itemz), "LoadItems")]
    internal static class FanpatchExplosionFixPatch
    {
        private static void Postfix()
        {
            try
            {
                const string projectileId = "shotgun_bullet";
                EquipmentAsset m4a1 = AssetManager.items.get("M4A1");
                if (m4a1 != null)
                {
                    m4a1.projectile = projectileId;
                }

                EquipmentAsset greenHeavyBlaster = AssetManager.items.get("greenheavyblaster");
                if (greenHeavyBlaster != null)
                {
                    greenHeavyBlaster.projectile = projectileId;
                }
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[FanpatchFixes] explosion fix patch failed: {ex.Message}");
            }
        }
    }

    [HarmonyPatch]
    internal static class FanpatchTimeFixYearPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(typeof(MapStats), "year_obsolete");
        }

        private static bool Prefix(MapStats __instance, int value)
        {
            double adjustedValue = value * 60.0;
            if (__instance.world_time < adjustedValue - 1.0)
            {
                __instance.world_time = adjustedValue;
            }

            return false;
        }
    }

    [HarmonyPatch]
    internal static class FanpatchTimeFixMonthPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(typeof(MapStats), "month_obsolete");
        }

        private static bool Prefix(MapStats __instance, int value)
        {
            double adjustedValue = value * 5.0;
            if (__instance.world_time > 0 && Math.Abs(__instance.world_time % 60.0) < 0.1)
            {
                __instance.world_time += adjustedValue;
            }
            else if (__instance.world_time < adjustedValue - 1.0)
            {
                __instance.world_time = adjustedValue;
            }

            return false;
        }
    }

    [HarmonyPatch]
    internal static class FanpatchTimeFixWorldTimePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(typeof(MapStats), "worldTime_obsolete");
        }

        private static bool Prefix(MapStats __instance, double value)
        {
            if (__instance.world_time < value - 1.0)
            {
                __instance.world_time = value;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Projectile), "update")]
    internal static class FanpatchProjectileCleanupPatch
    {
        private static readonly ConditionalWeakTable<Projectile, ProjectileLifetime> Timers = new ConditionalWeakTable<Projectile, ProjectileLifetime>();

        private sealed class ProjectileLifetime
        {
            public float age;
        }

        private static void Prefix(Projectile __instance, float pElapsed)
        {
            if (__instance?.asset == null)
            {
                return;
            }

            string id = __instance.asset.id;
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (!id.Contains("shell") && !id.Contains("missile") && !id.Contains("rocket") && !id.Contains("bullet"))
            {
                return;
            }

            ProjectileLifetime data = Timers.GetOrCreateValue(__instance);
            data.age += pElapsed;
            if (data.age > 2.5f)
            {
                __instance.setState(ProjectileState.ToRemove);
            }
        }

        [HarmonyPatch(typeof(Projectile), "start")]
        [HarmonyPostfix]
        private static void ResetTimerOnStart(Projectile __instance)
        {
            if (__instance == null)
            {
                return;
            }

            Timers.Remove(__instance);
        }
    }

    [HarmonyPatch(typeof(StatManager), "Update")]
    internal static class FanpatchEraHeartbeatPatch
    {
        private static float eraUpdateTimer;

        private static void Postfix(StatManager __instance)
        {
            if (__instance == null)
            {
                return;
            }

            eraUpdateTimer += Time.deltaTime;
            if (eraUpdateTimer < 3f)
            {
                return;
            }

            eraUpdateTimer = 0f;
            UpdateGlobalEra(__instance);
        }

        private static void UpdateGlobalEra(StatManager statManager)
        {
            try
            {
                if (!string.IsNullOrEmpty(statManager.eraoverride))
                {
                    return;
                }

                List<EraDefinition> enabledEras = EraLibrary.All
                    .Where(e => e.key switch
                    {
                        "medieval" => statManager.enableMedieval,
                        "renaissance" => statManager.enableRenaissance,
                        "modern" => statManager.enableModern,
                        "hyperfuture" => statManager.enableHyperfuture,
                        _ => false
                    })
                    .ToList();

                if (enabledEras.Count == 0 || World.world?.cities?.list == null)
                {
                    return;
                }

                int maxLevel = 0;
                foreach (City city in World.world.cities.list)
                {
                    if (city == null)
                    {
                        continue;
                    }

                    Building bonfire = city.getBuildingOfType("type_bonfire");
                    if (bonfire?.asset != null && bonfire.asset.upgrade_level > maxLevel)
                    {
                        maxLevel = bonfire.asset.upgrade_level;
                    }
                }

                int index = Mathf.Clamp(maxLevel, 0, enabledEras.Count - 1);
                EraDefinition targetEra = enabledEras[index];

                if (targetEra == null)
                {
                    return;
                }

                statManager.currentEra = targetEra.name;
                statManager.currentEraDescription = targetEra.description;

                string[] civGroups = { "alliance", "harden", "gaia", "horde" };
                foreach (string civGroup in civGroups)
                {
                    Traits.SetEraCity(targetEra.key, civGroup);
                }
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[FanpatchFixes] era heartbeat patch failed: {ex.Message}");
            }
        }
    }
}
