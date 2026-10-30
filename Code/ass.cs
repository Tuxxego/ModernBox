using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModernBox
{
    [HarmonyPatch(typeof(WindowPreloader), "instantiateWindow")]
    public static class GayestThingEver
    {
        private static readonly FieldInfo PreloadingOpsField =
            AccessTools.Field(typeof(WindowPreloader), "_windows_preloading_operations");

        static bool Prefix(string pWindowID)
        {
            if (PreloadingOpsField == null) return true;
            var ops = PreloadingOpsField.GetValue(null) as Dictionary<string, AsyncInstantiateOperation<ScrollWindow>>;
            if (ops != null && ops.ContainsKey(pWindowID))
            {
                return false;
            }
            return true;
        }
    }
}
