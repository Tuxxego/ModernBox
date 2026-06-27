using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace ModernBox
{
    [HarmonyPatch(typeof(PlayerControl), "isPointerOverUIObject")]
    public static class Patch_isPointerOverUIObject
    {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            if (PlayerControl._is_pointer_over_ui_object.HasValue)
            {
                __result = PlayerControl._is_pointer_over_ui_object.Value;
                return false;
            }

            if (EventSystem.current == null)
            {
                __result = false;
                return false;
            }

            __instance.updateCurrentPosition();

            if (__instance._event_data_current_position == null)
            {
                __result = false;
                return false;
            }

            List<RaycastResult> results = __instance._results ?? new List<RaycastResult>();

            results.Clear();

            EventSystem.current.RaycastAll(__instance._event_data_current_position, results);

            bool isOverUI = results.Count > 0;
            PlayerControl._is_pointer_over_ui_object = isOverUI;

            __result = isOverUI;
            return false;
        }
    }
}