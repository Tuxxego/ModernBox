using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// The M2 era techs' culture stats, applied the way the old game applied culture
    /// stats: +0.1 damage/armor and +0.1 army per strategy/defense/army tech (+5000 army
    /// from MilitaryModern), +1 housing per house, +2 watch towers, +0.2 gathering chance
    /// with +1 amount, and +2 starting level per knowledge tech.
    internal static class M2EraBonuses
    {
        internal static M2CultureTechs Of(Culture culture)
        {
            return M2Tech.IsResearching(culture) ? M2Tech.Get(culture) : null;
        }

        internal static M2CultureTechs Of(City city)
        {
            if (city == null || city.kingdom == null || city.kingdom.wild) return null;
            return Of(city.culture);
        }
    }

    [HarmonyPatch(typeof(Actor), "updateStats")]
    internal static class M2EraActorStatsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            if (__instance == null || !__instance.hasCulture()) return;
            M2CultureTechs techs = M2EraBonuses.Of(__instance.culture);
            if (techs == null) return;
            // ActorBase.updateStats: stat += stat * culture.stats.bonus_damage
            int damage = techs.Count(M2TechBonus.Damage);
            int armor = techs.Count(M2TechBonus.Armor);
            if (damage > 0) __instance.stats["damage"] *= 1f + 0.1f * damage;
            if (armor > 0) __instance.stats["armor"] *= 1f + 0.1f * armor;
            // City.getLimitOfBuildingsType added culture bonus_watch_towers; 0.51.2 reads the leader's bonus_towers.
            int towers = techs.Count(M2TechBonus.Towers);
            if (towers > 0) __instance.stats["bonus_towers"] += 2f * towers;
        }
    }

    [HarmonyPatch(typeof(City), nameof(City.getArmyMaxMultiplier))]
    internal static class M2EraArmyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(City __instance, ref float __result)
        {
            M2CultureTechs techs = M2EraBonuses.Of(__instance);
            if (techs != null) __result += techs.Army;
        }
    }

    [HarmonyPatch(typeof(City), "updateCityStatus")]
    internal static class M2EraHousingPatch
    {
        [HarmonyPostfix]
        private static void Postfix(City __instance)
        {
            M2CultureTechs techs = M2EraBonuses.Of(__instance);
            int housing = techs == null ? 0 : techs.Count(M2TechBonus.Housing);
            if (housing <= 0 || __instance.buildings == null) return;
            int houses = 0;
            foreach (Building building in __instance.buildings)
                if (building != null && !building.isUnderConstruction() && building.asset.hasHousingSlots()) houses++;
            CityStatus status = __instance.status;
            status.housing_total += houses * housing;
            status.housing_occupied = Mathf.Min(status.population, status.housing_total);
            status.housing_free = status.housing_total - status.housing_occupied;
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.BehExtractResourcesFromBuilding), nameof(ai.behaviours.BehExtractResourcesFromBuilding.execute))]
    internal static class M2EraGatheringPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Actor pActor, out BuildingAsset __state)
        {
            __state = pActor == null || pActor.beh_building_target == null ? null : pActor.beh_building_target.asset;
        }

        [HarmonyPostfix]
        private static void Postfix(Actor pActor, BuildingAsset __state)
        {
            if (pActor == null || __state == null || __state.resources_given == null || !pActor.hasCulture()) return;
            M2CultureTechs techs = M2EraBonuses.Of(pActor.culture);
            if (techs == null) return;
            int count;
            if (__state.building_type == BuildingType.Building_Mineral) count = techs.Count(M2TechBonus.Mining);
            else if (__state.building_type == BuildingType.Building_Tree) count = techs.Count(M2TechBonus.Axes);
            else return;
            if (count <= 0 || !Randy.randomChance(0.2f * count)) return;
            foreach (ResourceContainer resource in __state.resources_given)
                pActor.addToInventory(resource.id, count);
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.checkTraitMutationOnBirth))]
    internal static class M2EraBornLevelPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            if (__instance == null || __instance.data == null || !__instance.hasCulture()) return;
            M2CultureTechs techs = M2EraBonuses.Of(__instance.culture);
            if (techs == null) return;
            // Culture.getBornLevel: 1 + bonus_born_level, each M2 knowledge tech gave 2.
            int level = 1 + 2 * techs.Count(M2TechBonus.BornLevel);
            if (__instance.data.level < level) __instance.data.level = level;
        }
    }
}
