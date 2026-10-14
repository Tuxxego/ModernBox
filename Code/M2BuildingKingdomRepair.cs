using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    // Saves can hold buildings whose kingdom no longer loads (M2 wild kingdoms are made on demand).
    // The game then crashes every frame reading their colour, so give them their wild kingdom back.
    [HarmonyPatch(typeof(WildKingdomsManager), "updateDirtyBuildings")]
    internal static class M2BuildingKingdomRepair
    {
        private static readonly HashSet<string> Logged = new HashSet<string>();

        [HarmonyPrefix]
        private static void Prefix()
        {
            Repair();
        }

        internal static void Repair()
        {
            if (World.world == null || World.world.buildings == null || World.world.kingdoms_wild == null) return;
            foreach (Building building in World.world.buildings)
                if (building != null && building.kingdom == null) RepairOne(building);
        }

        internal static void RepairOne(Building building)
        {
            if (building == null || building.kingdom != null || building.asset == null || World.world?.kingdoms_wild == null) return;
            Kingdom kingdom = M2Creatures.EnsureWildKingdom(building.asset.kingdom) ?? World.world.kingdoms_wild.get("nature");
            if (kingdom == null) return;
            building.kingdom = kingdom;
            if (Logged.Add(building.asset.id))
                Debug.Log("[ModernBox] fixed building with no kingdom: " + building.asset.id + " -> " + kingdom.id);
        }
    }

    // The minimap and chunk lists can reach these buildings before the pass above runs.
    [HarmonyPatch(typeof(Building), nameof(Building.getColorForMinimap))]
    internal static class M2BuildingKingdomRepairMinimap
    {
        [HarmonyPrefix]
        private static void Prefix(Building __instance)
        {
            if (__instance.kingdom == null) M2BuildingKingdomRepair.RepairOne(__instance);
        }
    }

    [HarmonyPatch(typeof(ChunkObjectContainer), nameof(ChunkObjectContainer.addBuilding))]
    internal static class M2BuildingKingdomRepairChunks
    {
        [HarmonyPrefix]
        private static void Prefix(Building pBuilding)
        {
            if (pBuilding != null && pBuilding.kingdom == null) M2BuildingKingdomRepair.RepairOne(pBuilding);
        }
    }

    // Last line of defence: the building renderer reads every visible building's kingdom colour.
    [HarmonyPatch(typeof(BuildingManager), "precalculateRenderDataParallel")]
    internal static class M2BuildingKingdomRepairRender
    {
        private static readonly AccessTools.FieldRef<BuildingManager, Building[]> Visible =
            AccessTools.FieldRefAccess<BuildingManager, Building[]>("_array_visible_buildings");
        private static readonly AccessTools.FieldRef<BuildingManager, int> VisibleCount =
            AccessTools.FieldRefAccess<BuildingManager, int>("_visible_buildings_count");

        [HarmonyPrefix]
        private static void Prefix(BuildingManager __instance)
        {
            Building[] buildings = Visible(__instance);
            int count = VisibleCount(__instance);
            if (buildings == null) return;
            for (int i = 0; i < count && i < buildings.Length; i++)
                if (buildings[i] != null && buildings[i].kingdom == null) M2BuildingKingdomRepair.RepairOne(buildings[i]);
        }
    }
}
