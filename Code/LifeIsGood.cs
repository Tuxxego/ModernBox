using HarmonyLib;
using UnityEngine;

namespace ModernBox
{
    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), "upgradeBuilding")]
public static class Patch_CityBehBuild_upgradeBuilding
{
        public static bool Prefix(Building pBuilding, City pCity, ref bool __result)
        {
            return true;
        }
    }
}
