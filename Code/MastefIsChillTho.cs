using HarmonyLib;
using UnityEngine;
using System;

namespace ModernBox
{
    [HarmonyPatch(typeof(Actor), "calculateMainSprite")]
    public static class Patch_Actor_calculateMainSprite
    {
        private const bool Enabled = false;

        static bool Prepare()
        {
            return Enabled;
        }

        static bool Prefix(Actor __instance, ref Sprite __result)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        static Exception Finalizer(Actor __instance, Exception __exception, ref Sprite __result)
        {
            if (__exception != null)
            {
                __result = FuckMaxim(__instance);
                return null;
            }

            return null;
        }

        static Sprite FuckMaxim(Actor actor)
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
