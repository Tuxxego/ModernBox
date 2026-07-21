using System.Collections.Generic;
using HarmonyLib;
using NCMS.Utils;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    [HarmonyPatch(typeof(Actor), nameof(Actor.makeSleep))]
    internal static class M2MechanicalSleepPreventionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance, ref bool __result)
        {
            if (!ActorsAndBuildingsRegistry.IsNonSleepingMechanicalActor(__instance)) return true;

            // Machines in original M2 and M5 never enter WorldBox's biological
            // sleeping status. Block both autonomous sleep and the sleep-rain
            // power while leaving humanoid soldiers and invasion creatures native.
            if (__instance.hasStatus("sleeping")) __instance.stopSleeping();
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(EnemiesFinder), nameof(EnemiesFinder.findEnemiesFrom))]
    internal static class M2ZombieVehicleEnemyPoolPatch
    {
        [HarmonyPostfix]
        private static void Postfix(
            WorldTile pTile,
            Kingdom pKingdom,
            int pChunkRange,
            EnemyFinderData __result)
        {
            // Append only omitted, hostile M2 civilization actors to the same
            // cached candidate list vanilla already uses. Selection, reachability,
            // same-island rules, peaceful traits, and attack validation remain
            // entirely native after this point.
            ZombieVehicleTargetingService.IncludeNearbyCivilizationUnits(
                pTile, pKingdom, pChunkRange, __result);
        }
    }

    internal static class M2CityIdentitySafety
    {
        internal static bool RepairLeader(City city)
        {
            Actor leader = city == null ? null : city.leader;
            if (leader == null) return true;
            ActorAsset leaderAsset = leader.asset;
            ModernUnitSpec spec = leaderAsset == null
                ? null
                : ContentRegistry.Units.Find(candidate => candidate.Id == leaderAsset.id);

            // Vehicles, aircraft, ships and invasion creatures must never drive a
            // civilization's species, biome or construction identity.
            if (leaderAsset == null || (spec != null && !spec.Humanoid))
                return RemoveInvalidLeader(city, leader);

            if (leader.subspecies == null)
            {
                Subspecies replacement = null;
                if (spec != null && !string.IsNullOrEmpty(spec.Race))
                    replacement = city.getSubspecies(spec.Race);
                if (replacement == null)
                {
                    List<Actor> citizens = city.units;
                    if (citizens != null)
                    {
                        for (int index = 0; index < citizens.Count; index++)
                        {
                            Actor candidate = citizens[index];
                            if (candidate == null || candidate == leader || candidate.subspecies == null) continue;
                            replacement = candidate.subspecies;
                            break;
                        }
                    }
                }
                if (replacement != null) leader.setSubspecies(replacement);
            }

            string template = leaderAsset.build_order_template_id;
            if (string.IsNullOrEmpty(template) || AssetManager.city_build_orders.get(template) == null)
            {
                ActorAsset identity = spec != null && !string.IsNullOrEmpty(spec.Race)
                    ? AssetManager.actor_library.get(spec.Race)
                    : city.getFounderSpecies();
                if (identity != null && !string.IsNullOrEmpty(identity.build_order_template_id) &&
                    AssetManager.city_build_orders.get(identity.build_order_template_id) != null)
                {
                    leaderAsset.build_order_template_id = identity.build_order_template_id;
                }
            }

            // TileZone.canBeClaimedByCity dereferences leader.subspecies and
            // CityBehBuild dereferences the selected template without null checks.
            // If an old save has neither recoverable identity, let vanilla elect a
            // sound native citizen instead of allowing both simulation loops to fail.
            template = leaderAsset.build_order_template_id;
            if (leader.subspecies == null || string.IsNullOrEmpty(template) ||
                AssetManager.city_build_orders.get(template) == null)
                return RemoveInvalidLeader(city, leader);
            return true;
        }

        private static bool RemoveInvalidLeader(City city, Actor leader)
        {
            city.removeLeader();
            if (leader != null && leader.isAlive()) leader.setProfession(UnitProfession.Warrior, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.CityBehCheckLeader), nameof(ai.behaviours.CityBehCheckLeader.execute))]
    internal static class M2CityLeaderSubspeciesSafetyPatch
    {
        [HarmonyPrefix]
        private static void Prefix(City pCity)
        {
            M2CityIdentitySafety.RepairLeader(pCity);
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.KingdomBehCheckKing), "checkClanCreation")]
    internal static class M2KingClanFounderSubspeciesSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null) return false;
            if (pActor.hasClan() || pActor.subspecies != null) return true;

            ModernUnitSpec spec = pActor.asset == null
                ? null
                : ContentRegistry.Units.Find(candidate => candidate.Id == pActor.asset.id);
            if (spec != null && !spec.Humanoid)
                return RetireInvalidKing(pActor);

            Subspecies replacement = null;
            if (pActor.city != null)
            {
                if (spec != null && !string.IsNullOrEmpty(spec.Race))
                    replacement = pActor.city.getSubspecies(spec.Race);
                if (replacement == null && pActor.city.units != null)
                {
                    for (int index = 0; index < pActor.city.units.Count; index++)
                    {
                        Actor candidate = pActor.city.units[index];
                        if (candidate == null || candidate == pActor || candidate.subspecies == null) continue;
                        replacement = candidate.subspecies;
                        break;
                    }
                }
            }
            if (replacement == null && pActor.kingdom != null)
            {
                foreach (Actor candidate in pActor.kingdom.getUnits())
                {
                    if (candidate == null || candidate == pActor || candidate.subspecies == null) continue;
                    replacement = candidate.subspecies;
                    break;
                }
            }
            if (replacement != null)
            {
                pActor.setSubspecies(replacement);
                return true;
            }
            return RetireInvalidKing(pActor);
        }

        private static bool RetireInvalidKing(Actor actor)
        {
            Kingdom kingdom = actor == null ? null : actor.kingdom;
            if (kingdom != null && kingdom.king == actor) kingdom.removeKing();
            if (actor != null && actor.isAlive()) actor.setProfession(UnitProfession.Warrior, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.getElementBackground))]
    internal static class M2KingdomBannerBackgroundSafetyPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Kingdom __instance)
        {
            ActorsAndBuildingsRegistry.RepairKingdomBanner(__instance);
        }
    }

    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.getElementIcon))]
    internal static class M2KingdomBannerIconSafetyPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Kingdom __instance)
        {
            ActorsAndBuildingsRegistry.RepairKingdomBanner(__instance);
        }
    }

    [HarmonyPatch(typeof(TileZone), nameof(TileZone.canBeClaimedByCity))]
    internal static class M2CityZoneLeaderIdentitySafetyPatch
    {
        [HarmonyPrefix]
        private static void Prefix(City pCity)
        {
            M2CityIdentitySafety.RepairLeader(pCity);
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.BehRemoveRuins), nameof(ai.behaviours.BehRemoveRuins.execute))]
    internal static class M2RemoveRuinsTargetSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor, ref ai.behaviours.BehResult __result)
        {
            // The vanilla action immediately dereferences beh_building_target and
            // its asset. Saved AI tasks can outlive a removed ruin, including when
            // an M2 invasion actor evolves into a base-game asset, so actor-ID or
            // kingdom-based filtering cannot reliably identify every stale task.
            // Only intercept the invalid state; valid vanilla and M2 cleanup keeps
            // running through the original method unchanged.
            if (pActor != null &&
                pActor.beh_building_target != null &&
                pActor.beh_building_target.asset != null)
            {
                // Build 719 also dereferences cost for civilization-building
                // targets. Old saves may contain a legacy M2 structure created
                // before its explicit zero cost was registered, so repair that
                // asset in place and let normal cleanup finish.
                BuildingAsset targetAsset = pActor.beh_building_target.asset;
                if (targetAsset.building_type == BuildingType.Building_Civ && targetAsset.cost == null)
                    targetAsset.cost = new ConstructionCost(0, 0, 0, 0);
                return true;
            }

            if (pActor != null)
                pActor.beh_building_target = null;
            __result = ai.behaviours.BehResult.Stop;
            return false;
        }
    }

    [HarmonyPatch(typeof(BuildingAsset), nameof(BuildingAsset.getBoatAssetIDFromType))]
    internal static class M2DockBoatAssetResolverPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BuildingAsset __instance, string pSpeciesBoat, ref string __result)
        {
            if (__instance == null || string.IsNullOrEmpty(pSpeciesBoat)) return true;
            BuildingSpec dock = ContentRegistry.Buildings.Find(candidate =>
                candidate.Id == __instance.id && candidate.Type == "dock");
            if (dock == null) return true;
            ActorAsset boat = AssetManager.actor_library.get(pSpeciesBoat);
            if (boat == null || !boat.is_boat) return true;
            __result = boat.id;
            return false;
        }
    }

    [HarmonyPatch(typeof(City), nameof(City.makeWarrior))]
    internal static class M2CityWarriorEquipmentSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(City __instance, Actor pActor)
        {
            if (__instance == null || pActor == null || pActor.asset == null) return true;
            ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == pActor.asset.id);
            if (spec == null) return true;

            // Current City.makeWarrior always dereferences equipment.weapon. M2
            // machines intentionally cannot equip handheld items, so complete the
            // promotion without entering that item path. This remains scoped to M2
            // actors and leaves ordinary WorldBox citizens entirely unchanged.
            if (!spec.Equipment && spec.Role != M2UnitRole.Creature)
            {
                pActor.setProfession(UnitProfession.Warrior, true);
                if (__instance.status != null) __instance.status.warriors_current++;
                return false;
            }

            if (pActor.equipment == null || pActor.equipment.weapon == null)
                pActor.equipment = new ActorEquipment();
            return true;
        }
    }

    [HarmonyPatch(typeof(ActorAsset), nameof(ActorAsset.getSpriteIcon))]
    internal static class M2ActorIconFallbackPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ActorAsset __instance, ref Sprite __result)
        {
            if (__result != null || __instance == null ||
                !ModernBoxCatalog.UnitIds.Contains(__instance.id)) return;

            // The species-family map requires a non-null sprite. M2's legacy unit
            // definitions commonly use their first body frame as the icon rather
            // than a separate ui/Icons asset, so resolve that frame lazily after
            // NML has loaded the mod atlas and cache it on the current ActorAsset.
            if (__instance.texture_asset == null || __instance.animation_walk == null ||
                __instance.animation_walk.Length == 0) return;
            string path = __instance.texture_asset.texture_path_main + "/" + __instance.animation_walk[0];
            __result = SpriteTextureLoader.getSprite(path);
            if (__result != null) __instance._cached_sprite = __result;
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.calculateMainSprite))]
    internal static class M2BuildingMainSpriteSafetyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Building __instance, ref Sprite __result)
        {
            if (__instance == null || __instance.asset == null) return true;
            Sprite fallback;
            if (!ActorsAndBuildingsRegistry.TryGetBuildingRenderFallback(__instance.asset, out fallback) || fallback == null)
                return true;

            BuildingAsset asset = __instance.asset;
            BuildingAnimationData animation = __instance.animData;
            bool safe;
            if (__instance.isUnderConstruction())
                safe = asset.building_sprites != null && asset.building_sprites.construction != null;
            else if (__instance.check_spawn_animation)
                safe = false;
            else if (animation == null)
                safe = false;
            else if (asset.has_special_animation_state)
                safe = HasFrame(__instance.hasResourcesToCollect() ? animation.main : animation.special);
            else if (__instance.isRuin() && asset.has_ruins_graphics)
                safe = HasFrame(animation.ruins);
            else if (asset.spawn_drops)
                safe = __instance.data != null &&
                    (__instance.data.hasFlag("stop_spawn_drops") ? HasFrame(animation.main_disabled) : HasFrame(animation.main));
            else if (asset.can_be_abandoned && __instance.isAbandoned())
                safe = HasFrame(animation.main_disabled) || HasFrame(animation.main);
            else
                safe = HasFrame(animation.main);

            if (safe) return true;
            __instance.last_main_sprite = fallback;
            __result = fallback;
            return false;
        }

        [HarmonyPostfix]
        private static void Postfix(Building __instance, ref Sprite __result)
        {
            if (__result != null || __instance == null) return;
            Sprite fallback;
            if (ActorsAndBuildingsRegistry.TryGetBuildingRenderFallback(__instance.asset, out fallback))
                __result = fallback;
        }

        private static bool HasFrame(Sprite[] frames)
        {
            return frames != null && frames.Length > 0 && frames[0] != null;
        }
    }

    [HarmonyPatch(typeof(ActorTextureSubAsset), nameof(ActorTextureSubAsset.getUnitTexturePath))]
    internal static class M2EraCivilizationTexturePatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor pActor, ref string __result)
        {
            string path;
            if (M2LegacyBehaviorService.TryGetEraTexture(pActor, out path)) __result = path;
        }
    }

    [HarmonyPatch(typeof(Actor), "checkSpriteHead")]
    internal static class M2EraCivilizationWorldHeadPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance)
        {
            string path;
            if (!M2LegacyBehaviorService.TryGetEraTexture(__instance, out path)) return true;

            // M2's era outfits are precomposed: the face/head is already painted
            // into walk_N.png. Their tiny walk_N_head.png files are empty legacy
            // frame metadata. Letting build 719 run its advanced vanilla head
            // renderer adds a second head at the metadata origin (the actor's feet).
            __instance.dirty_sprite_head = false;
            __instance.cached_sprite_head = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(DynamicActorSpriteCreatorUI), nameof(DynamicActorSpriteCreatorUI.getSpriteHeadForUI))]
    internal static class M2EraCivilizationPreviewHeadPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(AnimationContainerUnit pContainer, ref Sprite __result)
        {
            if (pContainer == null || !M2LegacyBehaviorService.IsEraCivilizationTexturePath(pContainer.id))
                return true;

            // Keep the normal dynamic recoloring of the precomposed body, but do
            // not ask the current UI to fetch and composite a separate vanilla
            // head. Besides drawing it at the floor, that request indexes head
            // arrays which these legacy containers intentionally do not contain.
            __result = null;
            return false;
        }
    }

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

    [HarmonyPatch(typeof(Actor), "startAttackCooldown")]
    internal static class MissileSystemMinimumCooldownPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            // The strategic decision has its own cooldown, but this also covers
            // possession and legacy actors that enter an ordinary attack task.
            // Additive era/trait stats therefore cannot turn the launcher into an
            // automatic weapon.
            if (__instance != null && __instance.asset != null && __instance.asset.id == "MissileSystem")
                __instance.attack_timer = Mathf.Max(__instance.attack_timer, MissileSystemService.LaunchCooldownSeconds);
        }
    }

    /// <summary>
    /// Build 719 applies gravity to every ProjectileAsset. Its
    /// use_min_angle_height field only chooses the lower or higher ballistic
    /// solution; it is not the replacement for legacy parabolic=false. Keep the
    /// port's legacy parabolic=false projectiles on their original direct lines
    /// while leaving M2 artillery, bombs, arrows and other deliberate arcs alone.
    /// </summary>
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.updateVelocity))]
    internal static class ModernBoxDirectProjectileTrajectoryPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Projectile __instance, float pElapsed)
        {
            if (!UsesDirectTrajectory(__instance)) return true;
            UpdateDirect(__instance, pElapsed);
            return false;
        }

        private static bool UsesDirectTrajectory(Projectile projectile)
        {
            if (projectile == null || projectile.asset == null) return false;
            if (OriginalM2Projectiles.UsesLegacyDirectTrajectory(projectile.asset.id))
                return true;

            // Covers possessed/legacy MissileSystem actors which can still enter
            // an ordinary attack task and emit the original MIRVartillery asset.
            Actor launcher = projectile.by_who as Actor;
            return projectile.asset.id == "MIRVartillery" && launcher != null &&
                   launcher.asset != null && launcher.asset.id == "MissileSystem";
        }

        private static void UpdateDirect(Projectile projectile, float elapsed)
        {
            if (projectile._is_target_reached) return;

            float targetHeight = 0f;
            if (projectile.isMainTargetStillValid())
                targetHeight = Mathf.Max(0f, projectile._main_target.getHeight());

            Vector3 target = new Vector3(projectile._vector_target.x, projectile._vector_target.y, targetHeight);
            Vector3 delta = target - projectile._current_position_3d;
            float distance = delta.magnitude;
            float mass = projectile.asset.mass <= 0f ? 1f : projectile.asset.mass;
            float travel = Mathf.Max(0.01f, projectile._speed / mass) * Mathf.Max(0f, elapsed);
            bool reached = distance <= Mathf.Max(0.05f, travel);

            Vector3 direction = distance <= 0.0001f ? Vector3.zero : delta / distance;
            projectile._velocity = direction * (projectile._speed / mass);
            projectile._current_position_3d = reached
                ? target
                : projectile._current_position_3d + direction * travel;

            if (projectile._velocity.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(projectile._velocity.y + projectile._velocity.z,
                    projectile._velocity.x) * Mathf.Rad2Deg;
                projectile.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            if (projectile._collision_timeout != 0f && !reached) return;

            AttackDataResult result = projectile.checkHitOnNearbyUnits();
            if (reached)
                result = TryHitDesignatedTargetAtEndpoint(projectile, result);
            switch (result.state)
            {
                case ApplyAttackState.Hit:
                    projectile.setState(ProjectileState.ToRemove);
                    if (projectile._kill_action != null) projectile._kill_action();
                    projectile.targetReached();
                    return;
                case ApplyAttackState.Miss:
                    EffectsLibrary.spawnAt("fx_miss", projectile._current_position_3d, 0.1f);
                    projectile.setState(projectile.asset.can_be_left_on_ground
                        ? ProjectileState.AlphaAnimation
                        : ProjectileState.ToRemove);
                    projectile.targetReached();
                    return;
                case ApplyAttackState.Block:
                    projectile.getCollided(projectile._vector_target);
                    return;
                case ApplyAttackState.Deflect:
                    projectile.getDeflected(projectile._vector_start, result);
                    return;
                case ApplyAttackState.Continue:
                    if (!reached) return;
                    projectile.setState(ProjectileState.ToRemove);
                    if (projectile._kill_action != null) projectile._kill_action();
                    projectile.targetReached();
                    return;
            }
        }

        private static AttackDataResult TryHitDesignatedTargetAtEndpoint(Projectile projectile,
            AttackDataResult originalResult)
        {
            if (originalResult.state == ApplyAttackState.Hit ||
                originalResult.state == ApplyAttackState.Block ||
                originalResult.state == ApplyAttackState.Deflect ||
                projectile.by_who == null || World.world == null ||
                !projectile.isMainTargetStillValid())
                return originalResult;

            BaseSimObject target = projectile._main_target;
            if (target == null || target.current_tile == null || target.stats == null)
                return originalResult;

            // This only corrects the sub-tile mismatch between build 719's visual
            // target coordinate and its collision coordinate. It must not turn a
            // missed shot into a homing hit if the target has moved away.
            float tolerance = Mathf.Max(0.75f,
                projectile.asset.size + target.stats["size"] + 0.5f);
            if (Vector2.Distance(projectile._vector_target, target.current_position) > tolerance)
                return originalResult;

            Vector3 hitPosition = new Vector3(target.current_position.x,
                target.current_position.y, Mathf.Max(0f, target.getHeight()));
            AttackData attack = new AttackData(
                projectile.by_who,
                target.current_tile,
                hitPosition,
                projectile.by_who.current_position,
                target,
                projectile.kingdom,
                AttackType.Weapon,
                false,
                false,
                true,
                projectile.asset.id,
                null,
                0f);
            AttackDataResult endpointResult = MapBox.checkAttackFor(attack, target);
            return endpointResult.state == ApplyAttackState.Continue
                ? originalResult
                : endpointResult;
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
            // M2's MIRV toggle controls new crafting only. Existing equipment and
            // MissileSystem combat must remain functional when crafting is disabled.
            return true;
        }
    }

    [HarmonyPatch(typeof(CityEquipment), nameof(CityEquipment.loadFromSave))]
    internal static class DisabledMirvStorageLoadPatch
    {
        [HarmonyPostfix]
        private static void Postfix(City pCity)
        {
        }
    }

    [HarmonyPatch(typeof(City), nameof(City.giveItem))]
    internal static class DisabledMirvCityGivePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(City __instance, List<long> pItems, ref bool __result)
        {
            return true;
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

        [HarmonyPrefix]
        private static void Prefix(City pCity)
        {
            M2CityIdentitySafety.RepairLeader(pCity);
        }

        [HarmonyPostfix]
        private static void Postfix(City pCity)
        {
            if (!ModernProgression.IsSupportedCity(pCity)) return;
            List<BuildOrder> possible = ai.behaviours.CityBehBuild._possible_buildings;
            if (possible == null || possible.Count == 0) return;
            M2Era era = ModernProgression.GetEra(pCity);
            for (int index = possible.Count - 1; index >= 0; index--)
            {
                BuildOrder order = possible[index];
                if (order == null || string.IsNullOrEmpty(order.id) || !order.id.StartsWith("order_m2_", System.StringComparison.Ordinal)) continue;
                BuildingSpec spec = null;
                const string upgradePrefix = "order_m2_upgrade_";
                if (order.id.StartsWith(upgradePrefix, System.StringComparison.Ordinal))
                    spec = ContentRegistry.Buildings.Find(candidate => candidate.Id == order.id.Substring(upgradePrefix.Length));
                else
                {
                    string prefix = "order_m2_" + ModernProgression.GetRace(pCity) + "_";
                    if (order.id.StartsWith(prefix, System.StringComparison.Ordinal))
                        spec = ContentRegistry.Buildings.Find(candidate => candidate.Id == order.id.Substring(prefix.Length));
                }
                if (!ModernBoxSettings.Get("ConstructionOption") || spec == null || era < spec.Era) possible.RemoveAt(index);
            }
            int originalCount = possible.Count;
            for (int index = 0; index < originalCount; index++)
            {
                BuildOrder order = possible[index];
                if (order == null || string.IsNullOrEmpty(order.id) || !order.id.StartsWith("order_m2_", System.StringComparison.Ordinal)) continue;
                // Build 719 chooses uniformly from this list and exposes no order
                // priority. Repeating an already eligible native order supplies an
                // explicit weight without bypassing resource, tier, limit, or tile checks.
                for (int copy = 1; copy < ModernOrderWeight; copy++) possible.Add(order);
            }
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.canBeUpgraded))]
    internal static class M2EraUpgradeGatePatch
    {
        [HarmonyPostfix]
        private static void Postfix(Building __instance, ref bool __result)
        {
            if (!__result || __instance == null || __instance.asset == null ||
                string.IsNullOrEmpty(__instance.asset.upgrade_to)) return;

            BuildingSpec target = ContentRegistry.Buildings.Find(candidate =>
                candidate.UpgradeOnly && candidate.Id == __instance.asset.upgrade_to);
            if (target == null) return;

            City city = __instance.city;
            if (!ModernBoxSettings.Get("ConstructionOption") ||
                !ModernProgression.IsSupportedCity(city) ||
                ModernProgression.GetEra(city) < target.Era)
            {
                // CityBehBuild.upgradeRandomBuilding follows BuildingAsset.upgrade_to
                // directly and never consults the custom build-order list. This is
                // the definitive guard that stops a Renaissance city from walking
                // the linked M2 chain all the way to a Future skyscraper.
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), nameof(ai.behaviours.CityBehBuild.tryToBuild))]
    internal static class M2DirectConstructionEraGatePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(City pCity, BuildingAsset pBuildingAsset, ref Building __result)
        {
            if (pBuildingAsset == null) return true;
            BuildingSpec spec = ContentRegistry.Buildings.Find(candidate => candidate.Id == pBuildingAsset.id);
            if (spec == null) return true;
            if (ModernBoxSettings.Get("ConstructionOption") &&
                ModernProgression.IsSupportedCity(pCity) &&
                ModernProgression.GetEra(pCity) >= spec.Era) return true;

            // Keep every route, including vanilla AI and other ordinary city
            // construction calls, from bypassing M2's current-culture era gate.
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.start))]
    internal static class MissileSiloLaunchEventPatch
    {
        [HarmonyPostfix]
        private static void Postfix(BaseSimObject pInitiator, Vector3 pTargetPosition, string pAssetID)
        {
            // SiloLaunchEvents.Update owns M2 silo launches and already records the
            // attacker and chosen wartime target. The hook remains intentionally
            // empty so a projectile cannot create a duplicate world-log entry.
        }
    }
}

