using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBox
{
    [HarmonyPatch(typeof(WindowPreloader), "instantiateWindow")]
    public static class GayestThingEver
    {
        static bool Prefix(string pWindowID)
        {
            var dumb = AccessTools.Field(typeof(WindowPreloader), "_windows_preloading_operations");
            var shit = (Dictionary<string, AsyncInstantiateOperation<ScrollWindow>>)dumb.GetValue(null);

            if (shit.ContainsKey(pWindowID))
            {
                return false;
            }

            return true;
        }
    }
}