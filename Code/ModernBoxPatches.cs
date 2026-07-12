using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxRewrite
{
    [HarmonyPatch(typeof(Actor), nameof(Actor.checkTraitMutationOnBirth))]
    internal static class DefaultIdeologyBirthPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            // This runs after both parent inheritance and random spawn traits, so
            // inherited ideologies win and only people still missing one are filled.
            EquipmentAndTraitsRegistry.EnsureDefaultIdeology(__instance);
        }
    }

    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.getItemAssetToCraft))]
    internal static class ItemCraftingFilterPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref List<EquipmentAsset> pItemList, City pCity)
        {
            if (pItemList == null) return;
            List<EquipmentAsset> filtered = new List<EquipmentAsset>(pItemList.Count);
            foreach (EquipmentAsset item in pItemList)
            {
                if (item == null) continue;
                if (!ModernBoxCatalog.EquipmentIds.Contains(item.id) || ProductionService.IsEquipmentEnabled(item.id, pCity)) filtered.Add(item);
            }
            pItemList = filtered;
        }

        [HarmonyPostfix]
        private static void Postfix(City pCity, ref EquipmentAsset __result)
        {
            // A second guard covers preferred/culture candidates selected by the
            // original method outside the list passed through the prefix.
            if (__result != null && ModernBoxCatalog.EquipmentIds.Contains(__result.id) &&
                !ProductionService.IsEquipmentEnabled(__result.id, pCity)) __result = null;
        }
    }

    [HarmonyPatch(typeof(ActorEquipmentSlot), nameof(ActorEquipmentSlot.setItem))]
    internal static class DisabledMirvEquipPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Item pItem)
        {
            // Covers actor/save loading and every non-crafting equip path.
            return ModernBoxSettings.Get("MIRVOption") || !ProductionService.IsMirvItem(pItem);
        }
    }

    [HarmonyPatch(typeof(CityEquipment), nameof(CityEquipment.loadFromSave))]
    internal static class DisabledMirvStorageLoadPatch
    {
        [HarmonyPostfix]
        private static void Postfix(City pCity)
        {
            ProductionService.RemoveDisabledMirvsFromCityStorage(pCity);
        }
    }

    [HarmonyPatch(typeof(City), nameof(City.giveItem))]
    internal static class DisabledMirvCityGivePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(City __instance, List<long> pItems, ref bool __result)
        {
            if (ModernBoxSettings.Get("MIRVOption")) return true;
            ProductionService.RemoveDisabledMirvsFromCityStorage(__instance);
            if (pItems != null && pItems.Count > 0) return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.craftItem))]
    internal static class ItemCraftingSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor, City pCity, ref bool __result)
        {
            // Build 719's craftItem assumes every caller has equipment, a city,
            // and a kingdom. Modern vehicles intentionally use no equipment but
            // can still briefly receive BehMakeItem from the attacker job.
            if (pActor == null || pActor.asset == null || pActor.equipment == null || pCity == null || pActor.kingdom == null)
            {
                __result = false;
                return false;
            }
            if (ModernBoxCatalog.UnitIds.Contains(pActor.asset.id) && !pActor.asset.use_items)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), nameof(ai.behaviours.CityBehBuild.calcPossibleBuildings))]
    internal static class ModernConstructionWeightPatch
    {
        private const int ModernOrderWeight = 3;

        [HarmonyPostfix]
        private static void Postfix(City pCity)
        {
            if (!ModernProgression.IsHumanCity(pCity)) return;
            List<BuildOrder> possible = ai.behaviours.CityBehBuild._possible_buildings;
            if (possible == null || possible.Count == 0) return;
            int originalCount = possible.Count;
            for (int index = 0; index < originalCount; index++)
            {
                BuildOrder order = possible[index];
                if (order == null || string.IsNullOrEmpty(order.id) || !order.id.StartsWith("order_", System.StringComparison.Ordinal)) continue;
                bool modernHouseUpgrade = order.id == "order_modernbox_house_upgrade";
                string buildingId = order.id.Substring("order_".Length);
                if (!ModernBoxCatalog.CivilianBuildingIds.Contains(buildingId) &&
                    !ModernBoxCatalog.FactoryBuildingIds.Contains(buildingId) &&
                    buildingId != "MissileSilo" && !modernHouseUpgrade) continue;
                // Build 719 chooses uniformly from this list and exposes no order
                // priority. Repeating an already eligible native order supplies an
                // explicit weight without bypassing resource, tier, limit, or tile checks.
                for (int copy = 1; copy < ModernOrderWeight; copy++) possible.Add(order);
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.start))]
    internal static class MissileSiloLaunchEventPatch
    {
        [HarmonyPostfix]
        private static void Postfix(BaseSimObject pInitiator, Vector3 pTargetPosition, string pAssetID)
        {
            if (pAssetID != "NUKER") return;
            Building silo = pInitiator as Building;
            if (silo == null || silo.asset == null || silo.asset.id != "MissileSilo") return;
            SiloLaunchEvents.Notify(silo, pTargetPosition);
        }
    }
}
