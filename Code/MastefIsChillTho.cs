using HarmonyLib;
using UnityEngine;
using System;

namespace ModernBox
{
    [HarmonyPatch(typeof(Actor), "calculateMainSprite")]
    public static class Patch_Actor_calculateMainSprite
    {
        // Optimization: Removed empty no-op Prefix that ran on every Actor every frame.
        static Exception Finalizer(Actor __instance, Exception __exception, ref Sprite __result)
        {
            if (__exception != null)
            {
                __result = FallbackIdleSprite(__instance);
                return null;
            }

            return null;
        }

        static Sprite FallbackIdleSprite(Actor actor)
        {
            if (actor?.animation_container?.idle?.frames != null &&
                actor.animation_container.idle.frames.Length > 0)
            {
                return actor.animation_container.idle.frames[0];
            }

            return null;
        }
    }
}
