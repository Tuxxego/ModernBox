using HarmonyLib;
using UnityEngine;
using System;

[HarmonyPatch(typeof(Actor), "calculateColoredSprite")]
public static class Patch_Actor_calculateColoredSprite
{
    private const bool Enabled = false;

    static bool Prepare()
    {
        return Enabled;
    }

    static Exception Finalizer(Actor __instance, Sprite pMainSprite, Exception __exception, ref Sprite __result)
    {
        if (__exception != null)
        {
            __result = pMainSprite;
            return null;
        }

        return null;
    }
}
