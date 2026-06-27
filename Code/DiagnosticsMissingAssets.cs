using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// what the fuck is this ai slop morfos did
namespace ModernBox
{
    public static class MissingAssetDiagnostics
    {
        private const bool Enabled = false;
        private static readonly HashSet<string> _seenProjectileMissing = new HashSet<string>();
        private static readonly HashSet<string> _seenNullSpriteSet = new HashSet<string>();
        private static readonly HashSet<string> _seenActorNullRender = new HashSet<string>();
        private static readonly HashSet<string> _seenSpritePathMissing = new HashSet<string>();
        private static readonly HashSet<string> _seenSpriteListPathMissing = new HashSet<string>();
        private static readonly HashSet<string> _seenNullItemAction = new HashSet<string>();
        private static readonly HashSet<string> _seenCalibrateSkips = new HashSet<string>();
        private static readonly HashSet<string> _seenBuildingRecolorInputNull = new HashSet<string>();
        private static readonly HashSet<string> _seenBuildingRecolorExceptions = new HashSet<string>();
        private static readonly HashSet<string> _seenDynamicBuildingRecolorInputNull = new HashSet<string>();

        private static bool ShouldLog(HashSet<string> set, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "<empty-key>";
            }
            return set.Add(key);
        }

        private static string GetInitiatorAssetId(BaseSimObject initiator)
        {
            try
            {
                Building building = initiator as Building;
                if (building != null && building.asset != null)
                {
                    return building.asset.id;
                }

                Actor actor = initiator as Actor;
                if (actor != null && actor.asset != null)
                {
                    return actor.asset.id;
                }
            }
            catch
            {
            }

            return "n/a";
        }

        private static string GetBuildingAssetId(Building building)
        {
            try
            {
                if (building != null && building.asset != null)
                {
                    return building.asset.id;
                }
            }
            catch
            {
            }

            return "null";
        }

        private static string GetAtlasAssetId(DynamicSpritesAsset atlas)
        {
            if (atlas == null)
            {
                return "null";
            }

            try
            {
                return atlas.id;
            }
            catch
            {
                return "<atlas-id-error>";
            }
        }

        private static int GetKingdomColorIndex(Building building)
        {
            try
            {
                if (building != null && building.kingdom != null)
                {
                    ColorAsset color = building.kingdom.getColor();
                    if (color != null)
                    {
                        return color.index_id;
                    }
                }
            }
            catch
            {
            }

            return -1;
        }

        private static string GetObjectPath(Transform t)
        {
            if (t == null)
            {
                return "<null-transform>";
            }

            string path = t.name;
            Transform cur = t.parent;
            int guard = 0;
            while (cur != null && guard < 25)
            {
                path = cur.name + "/" + path;
                cur = cur.parent;
                guard++;
            }
            return path;
        }

        private static bool IsInterestingPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.StartsWith("items/")
                || path.StartsWith("weapons/")
                || path.StartsWith("effects/projectiles/")
                || path.StartsWith("effects/")
                || path.StartsWith("buildings/")
                || path.StartsWith("units/")
                || path.StartsWith("ui/");
        }

        [HarmonyPatch(typeof(ProjectileManager), "spawn")]
        private static class Patch_ProjectileManager_Spawn_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static void Prefix(BaseSimObject pInitiator, BaseSimObject pTargetObject, string pAssetID, Vector3 pLaunchPosition, Vector3 pTargetPosition)
            {
                try
                {
                    if (string.IsNullOrEmpty(pAssetID) || AssetManager.projectiles.get(pAssetID) == null)
                    {
                        string initiatorType = pInitiator != null ? pInitiator.GetType().Name : "null";
                        string targetType = pTargetObject != null ? pTargetObject.GetType().Name : "null";
                        string initiatorAsset = GetInitiatorAssetId(pInitiator);
                        string key = pAssetID + "|" + initiatorType + "|" + initiatorAsset + "|" + targetType;

                        if (ShouldLog(_seenProjectileMissing, key))
                        {
                            ModernBoxLogger.Error(
                                "[Diag.ProjectileMissing] projectile_id='" + pAssetID +
                                "' initiator_type='" + initiatorType +
                                "' initiator_asset='" + initiatorAsset +
                                "' target_type='" + targetType +
                                "' launch=('" + pLaunchPosition.x + "," + pLaunchPosition.y + "," + pLaunchPosition.z +
                                "') target=('" + pTargetPosition.x + "," + pTargetPosition.y + "," + pTargetPosition.z + "')"
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.ProjectileMissing] logger failed: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "checkSpriteToRender")]
        private static class Patch_Actor_CheckSpriteToRender_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static void Postfix(Actor __instance, ref Sprite __result)
            {
                try
                {
                    if (__result == null)
                    {
                        string actorAsset = (__instance != null && __instance.asset != null) ? __instance.asset.id : "null";
                        string actorType = __instance != null ? __instance.GetType().Name : "null";
                        string key = actorType + "|" + actorAsset;

                        if (ShouldLog(_seenActorNullRender, key))
                        {
                            bool alive = __instance != null && __instance.isAlive();
                            ModernBoxLogger.Error(
                                "[Diag.ActorRenderSpriteNull] actor_type='" + actorType +
                                "' actor_asset='" + actorAsset +
                                "' alive='" + alive + "'"
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.ActorRenderSpriteNull] logger failed: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(GroupSpriteObject), "setSprite")]
        private static class Patch_GroupSpriteObject_SetSprite_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static void Prefix(GroupSpriteObject __instance, Sprite pSprite)
            {
                try
                {
                    if (pSprite == null)
                    {
                        string objectPath = "<null-object>";
                        if (__instance != null)
                        {
                            objectPath = GetObjectPath(__instance.transform);
                        }

                        string key = objectPath;
                        if (ShouldLog(_seenNullSpriteSet, key))
                        {
                            ModernBoxLogger.Error(
                                "[Diag.GroupSpriteSetNull] object_path='" + objectPath +
                                "' hint='caller passed null sprite into GroupSpriteObject.setSprite'"
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.GroupSpriteSetNull] logger failed: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(SpriteTextureLoader), "getSprite", new Type[] { typeof(string) })]
        private static class Patch_SpriteTextureLoader_GetSprite_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static void Postfix(string pPath, ref Sprite __result)
            {
                try
                {
                    if (__result == null && IsInterestingPath(pPath) && ShouldLog(_seenSpritePathMissing, pPath))
                    {
                        ModernBoxLogger.Error("[Diag.SpriteMissing] path='" + pPath + "'");
                    }
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.SpriteMissing] logger failed: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(SpriteTextureLoader), "getSpriteList", new Type[] { typeof(string), typeof(bool) })]
        private static class Patch_SpriteTextureLoader_GetSpriteList_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static void Postfix(string pPath, bool pSkipIfEmpty, ref Sprite[] __result)
            {
                try
                {
                    bool missing = __result == null || __result.Length == 0;
                    if (missing && IsInterestingPath(pPath) && ShouldLog(_seenSpriteListPathMissing, pPath))
                    {
                        ModernBoxLogger.Error(
                            "[Diag.SpriteListMissing] path='" + pPath +
                            "' skip_if_empty='" + pSkipIfEmpty + "'"
                        );
                    }
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.SpriteListMissing] logger failed: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "addDefaultItemAttackActions")]
        private static class Patch_Actor_AddDefaultItemAttackActions_Guard
        {
            private static bool Prepare() => Enabled;

            private static bool Prefix(Actor __instance, ItemAsset pItemAsset)
            {
                try
                {
                    if (pItemAsset != null)
                    {
                        return true;
                    }

                    string actorAsset = (__instance != null && __instance.asset != null) ? __instance.asset.id : "null";
                    string defaultAttack = (__instance != null && __instance.asset != null) ? __instance.asset.default_attack : "null";
                    string key = actorAsset + "|" + defaultAttack + "|default";

                    if (ShouldLog(_seenNullItemAction, key))
                    {
                        ModernBoxLogger.Error(
                            "[Diag.NullItemAction] addDefaultItemAttackActions got null item asset actor='" + actorAsset +
                            "' default_attack='" + defaultAttack + "'"
                        );
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.NullItemAction] default guard failed: " + ex.Message);
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "addItemActions")]
        private static class Patch_Actor_AddItemActions_Guard
        {
            private static bool Prepare() => Enabled;

            private static bool Prefix(Actor __instance, ItemAsset pItemAsset)
            {
                try
                {
                    if (pItemAsset != null)
                    {
                        return true;
                    }

                    string actorAsset = (__instance != null && __instance.asset != null) ? __instance.asset.id : "null";
                    string defaultAttack = (__instance != null && __instance.asset != null) ? __instance.asset.default_attack : "null";
                    string key = actorAsset + "|" + defaultAttack + "|generic";

                    if (ShouldLog(_seenNullItemAction, key))
                    {
                        ModernBoxLogger.Error(
                            "[Diag.NullItemAction] addItemActions got null item asset actor='" + actorAsset +
                            "' default_attack='" + defaultAttack + "'"
                        );
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.NullItemAction] generic guard failed: " + ex.Message);
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "checkCalibrateTargetPosition")]
        private static class Patch_Actor_CheckCalibrateTargetPosition_Guard
        {
            private static bool Prepare() => Enabled;

            private static bool Prefix(Actor __instance)
            {
                try
                {
                    if (__instance == null)
                    {
                        return false;
                    }

                    BaseSimObject target = Traverse.Create(__instance).Field("beh_actor_target").GetValue<BaseSimObject>();
                    if (target == null || !target.isActor())
                    {
                        return true;
                    }

                    Actor targetActor = target.a;
                    WorldTile tileTarget = Traverse.Create(__instance).Field("tile_target").GetValue<WorldTile>();

                    if (targetActor == null || targetActor.current_tile == null || tileTarget == null)
                    {
                        string selfId = (__instance.asset != null) ? __instance.asset.id : "null";
                        string targetId = (targetActor != null && targetActor.asset != null) ? targetActor.asset.id : "null";
                        string key = selfId + "|" + targetId;

                        if (ShouldLog(_seenCalibrateSkips, key))
                        {
                            ModernBoxLogger.Warning(
                                "[Diag.CalibrateSkip] Skipping checkCalibrateTargetPosition for actor='" + selfId +
                                "' target='" + targetId + "' because target tile or tile_target is null"
                            );
                        }

                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.CalibrateSkip] guard failed: " + ex.Message);
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Building), "calculateColoredSprite")]
        private static class Patch_Building_CalculateColoredSprite_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static bool Prefix(Building __instance, Sprite pMainSprite, ref Sprite __result)
            {
                try
                {
                    BuildingAsset asset = __instance != null ? __instance.asset : null;
                    DynamicSpritesAsset atlas = asset != null ? asset.atlas_asset : null;

                    if (pMainSprite == null || atlas == null)
                    {
                        string buildingId = GetBuildingAssetId(__instance);
                        string spriteName = pMainSprite != null ? pMainSprite.name : "null";
                        string atlasId = GetAtlasAssetId(atlas);
                        int colorIndex = GetKingdomColorIndex(__instance);
                        string reason = pMainSprite == null ? "main_sprite_null" : "atlas_asset_null";
                        string key = buildingId + "|" + reason + "|" + spriteName + "|" + atlasId;

                        if (ShouldLog(_seenBuildingRecolorInputNull, key))
                        {
                            ModernBoxLogger.Error(
                                "[Diag.BuildingRecolorInputNull] building_id='" + buildingId +
                                "' reason='" + reason +
                                "' main_sprite='" + spriteName +
                                "' atlas_asset='" + atlasId +
                                "' color_index='" + colorIndex + "'"
                            );
                        }

                        __result = pMainSprite;
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.BuildingRecolorInputNull] guard failed: " + ex.Message);
                    __result = pMainSprite;
                    return false;
                }
            }

            private static Exception Finalizer(Building __instance, Sprite pMainSprite, Exception __exception, ref Sprite __result)
            {
                if (__exception == null)
                {
                    return null;
                }

                try
                {
                    BuildingAsset asset = __instance != null ? __instance.asset : null;
                    DynamicSpritesAsset atlas = asset != null ? asset.atlas_asset : null;
                    string buildingId = GetBuildingAssetId(__instance);
                    string spriteName = pMainSprite != null ? pMainSprite.name : "null";
                    string atlasId = GetAtlasAssetId(atlas);
                    int colorIndex = GetKingdomColorIndex(__instance);
                    string key = buildingId + "|" + __exception.GetType().Name + "|" + __exception.Message;

                    if (ShouldLog(_seenBuildingRecolorExceptions, key))
                    {
                        ModernBoxLogger.Error(
                            "[Diag.BuildingRecolorException] building_id='" + buildingId +
                            "' main_sprite='" + spriteName +
                            "' atlas_asset='" + atlasId +
                            "' color_index='" + colorIndex +
                            "' ex_type='" + __exception.GetType().Name +
                            "' ex_msg='" + __exception.Message + "'"
                        );
                    }
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.BuildingRecolorException] logger failed: " + ex.Message);
                }

                __result = pMainSprite;
                return null;
            }
        }

        [HarmonyPatch(typeof(DynamicSprites), nameof(DynamicSprites.getRecoloredBuilding))]
        private static class Patch_DynamicSprites_GetRecoloredBuilding_Diagnostics
        {
            private static bool Prepare() => Enabled;

            private static bool Prefix(Sprite pBuildingSprite, ColorAsset pColor, DynamicSpritesAsset pAtlasAsset, ref Sprite __result)
            {
                try
                {
                    if (pBuildingSprite != null && pAtlasAsset != null)
                    {
                        return true;
                    }

                    string spriteName = pBuildingSprite != null ? pBuildingSprite.name : "null";
                    string atlasId = GetAtlasAssetId(pAtlasAsset);
                    int colorIndex = pColor != null ? pColor.index_id : -1;
                    string reason = pBuildingSprite == null ? "main_sprite_null" : "atlas_asset_null";
                    string key = reason + "|" + spriteName + "|" + atlasId + "|" + colorIndex;

                    if (ShouldLog(_seenDynamicBuildingRecolorInputNull, key))
                    {
                        ModernBoxLogger.Error(
                            "[Diag.DynamicBuildingRecolorInputNull] reason='" + reason +
                            "' main_sprite='" + spriteName +
                            "' atlas_asset='" + atlasId +
                            "' color_index='" + colorIndex + "'"
                        );
                    }

                    __result = pBuildingSprite;
                    return false;
                }
                catch (Exception ex)
                {
                    ModernBoxLogger.Warning("[Diag.DynamicBuildingRecolorInputNull] guard failed: " + ex.Message);
                    __result = pBuildingSprite;
                    return false;
                }
            }
        }
    }
}
