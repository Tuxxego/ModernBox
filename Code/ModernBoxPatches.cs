using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    // Building.tech: cities only build an M1 building once their culture has its tech.
    // "Railguns" and "Boi" were never made, so those two factories are never built.
    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), nameof(ai.behaviours.CityBehBuild.calcPossibleBuildings))]
    internal static class M1BuildingTechPatch
    {
        [HarmonyPrefix]
        private static void Prefix(City pCity)
        {
            M1CityLeaderSafety.RepairLeader(pCity);
        }

        [HarmonyPostfix]
        private static void Postfix(City pCity)
        {
            List<BuildOrder> possible = ai.behaviours.CityBehBuild._possible_buildings;
            if (possible == null) return;
            for (int index = possible.Count - 1; index >= 0; index--)
            {
                BuildOrder order = possible[index];
                if (order == null || string.IsNullOrEmpty(order.id) || !order.id.StartsWith("order_m1_", System.StringComparison.Ordinal)) continue;
                BuildingSpec spec = ActorsAndBuildingsRegistry.SpecForOrder(order, pCity);
                if (spec == null || !M1Tech.Has(pCity, spec.Tech)) possible.RemoveAt(index);
            }
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), nameof(ai.behaviours.CityBehBuild.tryToBuild))]
    internal static class M1BuildingTechBuildPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(City pCity, BuildingAsset pBuildingAsset, ref Building __result)
        {
            BuildingSpec spec = pBuildingAsset == null ? null : ContentRegistry.FindBuilding(pBuildingAsset.id);
            if (spec == null || M1Tech.Has(pCity, spec.Tech)) return true;
            __result = null;
            return false;
        }
    }

    // Vehicles join cities, so 0.51.2 may pick one as a leader or king. Their asset has no
    // species, build orders or clan, which the game reads without checks.
    internal static class M1CityLeaderSafety
    {
        internal static bool RepairLeader(City city)
        {
            Actor leader = city == null ? null : city.leader;
            if (leader == null || leader.asset == null) return true;
            ModernUnitSpec spec = ContentRegistry.FindUnit(leader.asset.id);
            if (spec == null) return true;
            if (!spec.Humanoid) return RemoveLeader(city, leader);
            if (leader.subspecies == null)
            {
                Subspecies replacement = city.getSubspecies(spec.Race);
                if (replacement == null && city.units != null)
                    foreach (Actor candidate in city.units)
                        if (candidate != null && candidate != leader && candidate.subspecies != null) { replacement = candidate.subspecies; break; }
                if (replacement != null) leader.setSubspecies(replacement);
            }
            string template = leader.asset.build_order_template_id;
            if (leader.subspecies == null || string.IsNullOrEmpty(template) || AssetManager.city_build_orders.get(template) == null)
                return RemoveLeader(city, leader);
            return true;
        }

        private static bool RemoveLeader(City city, Actor leader)
        {
            city.removeLeader();
            if (leader.isAlive()) leader.setProfession(UnitProfession.Warrior, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.CityBehCheckLeader), nameof(ai.behaviours.CityBehCheckLeader.execute))]
    internal static class M1CityLeaderPatch
    {
        [HarmonyPrefix]
        private static void Prefix(City pCity)
        {
            M1CityLeaderSafety.RepairLeader(pCity);
        }
    }

    [HarmonyPatch(typeof(TileZone), nameof(TileZone.canBeClaimedByCity))]
    internal static class M1CityZoneLeaderPatch
    {
        [HarmonyPrefix]
        private static void Prefix(City pCity)
        {
            M1CityLeaderSafety.RepairLeader(pCity);
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.KingdomBehCheckKing), "checkClanCreation")]
    internal static class M1KingClanPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null) return false;
            if (pActor.hasClan() || pActor.subspecies != null) return true;
            ModernUnitSpec spec = pActor.asset == null ? null : ContentRegistry.FindUnit(pActor.asset.id);
            if (spec == null) return true;
            Subspecies replacement = null;
            if (spec.Humanoid && pActor.city != null) replacement = pActor.city.getSubspecies(spec.Race);
            if (replacement != null)
            {
                pActor.setSubspecies(replacement);
                return true;
            }
            Kingdom kingdom = pActor.kingdom;
            if (kingdom != null && kingdom.king == pActor) kingdom.removeKing();
            if (pActor.isAlive()) pActor.setProfession(UnitProfession.Warrior, true);
            return false;
        }
    }

    // _mob vehicles never slept.
    [HarmonyPatch(typeof(Actor), nameof(Actor.makeSleep))]
    internal static class M1VehicleSleepPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance, ref bool __result)
        {
            if (!ActorsAndBuildingsRegistry.IsVehicle(__instance)) return true;
            if (__instance.hasStatus("sleeping")) __instance.stopSleeping();
            __result = false;
            return false;
        }
    }

    // City.makeWarrior reads equipment.weapon, which itemless vehicles don't use.
    [HarmonyPatch(typeof(City), nameof(City.makeWarrior))]
    internal static class M1MakeWarriorPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(City __instance, Actor pActor)
        {
            if (__instance == null || pActor == null || pActor.asset == null) return true;
            ModernUnitSpec spec = ContentRegistry.FindUnit(pActor.asset.id);
            if (spec == null) return true;
            if (!spec.Equipment)
            {
                pActor.setProfession(UnitProfession.Warrior, true);
                if (__instance.status != null) __instance.status.warriors_current++;
                return false;
            }
            if (pActor.equipment == null || pActor.equipment.weapon == null) pActor.equipment = new ActorEquipment();
            return true;
        }
    }

    // Vehicles can briefly get the craft-an-item task from their job; they can't hold items.
    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.craftItem))]
    internal static class M1CraftItemSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor, City pCity, ref bool __result)
        {
            if (pActor == null || pActor.asset == null || pActor.equipment == null || pCity == null || pActor.kingdom == null ||
                (ModernBoxCatalog.UnitIds.Contains(pActor.asset.id) && !pActor.asset.use_items))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    // The family map and unit lists need an icon sprite; M1 units use their first frame.
    [HarmonyPatch(typeof(ActorAsset), nameof(ActorAsset.getSpriteIcon))]
    internal static class M1ActorIconFallbackPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ActorAsset __instance, ref Sprite __result)
        {
            if (__result != null || __instance == null || !ModernBoxCatalog.UnitIds.Contains(__instance.id)) return;
            if (__instance.texture_asset == null || __instance.animation_walk == null || __instance.animation_walk.Length == 0) return;
            __result = SpriteTextureLoader.getSprite(__instance.texture_asset.texture_path_main + "/" + __instance.animation_walk[0]);
            if (__result != null) __instance._cached_sprite = __result;
        }
    }

    // M1 units show one fixed picture in the unit window, which gives the game no position
    // for a held item. A soldier holding a gun then indexed past the end of that list every
    // frame, so the window doesn't draw the held item for these units.
    [HarmonyPatch(typeof(UnitAvatarLoader), "loadItemSprites")]
    internal static class M1AvatarItemPatch
    {
        [HarmonyPostfix]
        private static void Postfix(UnitAvatarLoader __instance)
        {
            ActorAvatarData data = __instance._data;
            if (data == null || data.asset == null || !data.asset.has_override_avatar_frames) return;
            if (!ModernBoxCatalog.UnitIds.Contains(data.asset.id)) return;
            __instance._show_item = false;
        }
    }

    // M1's buildings only have main frames; any state without a sprite shows the main one.
    [HarmonyPatch(typeof(Building), nameof(Building.calculateMainSprite))]
    internal static class M1BuildingSpritePatch
    {
        // Build 719 can pick an animation slot with no frame and index it without a check.
        [HarmonyPrefix]
        private static bool Prefix(Building __instance, ref Sprite __result)
        {
            if (__instance == null || __instance.asset == null) return true;
            Sprite fallback;
            if (!ActorsAndBuildingsRegistry.TryGetBuildingRenderFallback(__instance.asset, out fallback)) return true;
            BuildingAsset asset = __instance.asset;
            BuildingAnimationData animation = __instance.animData;
            bool safe;
            if (__instance.isUnderConstruction()) safe = asset.building_sprites != null && asset.building_sprites.construction != null;
            else if (__instance.check_spawn_animation || animation == null) safe = false;
            else if (asset.has_special_animation_state) safe = HasFrame(__instance.hasResourcesToCollect() ? animation.main : animation.special);
            else if (__instance.isRuin() && asset.has_ruins_graphics) safe = HasFrame(animation.ruins);
            else if (asset.can_be_abandoned && __instance.isAbandoned()) safe = HasFrame(animation.main_disabled) || HasFrame(animation.main);
            else safe = HasFrame(animation.main);
            if (safe) return true;
            __instance.last_main_sprite = fallback;
            __result = fallback;
            return false;
        }

        private static bool HasFrame(Sprite[] frames)
        {
            return frames != null && frames.Length > 0 && frames[0] != null;
        }

        [HarmonyPostfix]
        private static void Postfix(Building __instance, ref Sprite __result)
        {
            if (__result != null || __instance == null) return;
            Sprite fallback;
            if (ActorsAndBuildingsRegistry.TryGetBuildingRenderFallback(__instance.asset, out fallback)) __result = fallback;
        }
    }

    // Saved tasks can point at a ruin that is already gone; vanilla reads it without a check.
    [HarmonyPatch(typeof(ai.behaviours.BehRemoveRuins), nameof(ai.behaviours.BehRemoveRuins.execute))]
    internal static class M1RemoveRuinsSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor, ref ai.behaviours.BehResult __result)
        {
            if (pActor == null || (pActor.beh_building_target != null && pActor.beh_building_target.asset != null)) return true;
            pActor.beh_building_target = null;
            __result = ai.behaviours.BehResult.Stop;
            return false;
        }
    }
}
