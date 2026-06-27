using HarmonyLib;
using UnityEngine;

namespace ModernBox
{
    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), "upgradeBuilding")]
    public static class Patch_CityBehBuild_upgradeBuilding
    {
        public static bool Prefix(Building pBuilding, City pCity, ref bool __result)
        {
            try
            {
                if (pBuilding == null)
                {
                    Debug.LogWarning("[AlliancePatch] upgradeBuilding: pBuilding was null");
                    __result = false;
                    return false;
                }

                if (pBuilding.asset == null)
                {
                    Debug.LogWarning("[AlliancePatch] upgradeBuilding: building asset was null");
                    __result = false;
                    return false;
                }

                string upgradeTo = pBuilding.asset.upgrade_to;

                if (string.IsNullOrEmpty(upgradeTo))
                {
                    __result = false;
                    return false;
                }

                BuildingAsset buildingAsset = AssetManager.buildings.get(upgradeTo);

                if (buildingAsset == null)
                {
                    Debug.LogWarning("[AlliancePatch] upgrade asset missing: " + upgradeTo);
                    __result = false;
                    return false;
                }

                if (pCity == null)
                {
                    Debug.LogWarning("[AlliancePatch] upgradeBuilding: pCity was null");
                    __result = false;
                    return false;
                }

                if (!pCity.hasEnoughResourcesFor(buildingAsset.cost))
                {
                    __result = false;
                    return false;
                }

                bool upgraded = pBuilding.upgradeBuilding();

                if (!upgraded)
                {
                    __result = false;
                    return false;
                }

                pCity.spendResourcesForBuildingAsset(buildingAsset.cost);

                __result = true;
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[AlliancePatch] upgradeBuilding crashed: " + e);
                __result = false;
                return false;
            }
        }
    }
}