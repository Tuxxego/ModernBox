using System.Collections.Generic;
using HarmonyLib;

namespace ModernBoxM2Rewrite
{
    /// Original M2 MissileSilo: a city tower that fires NUKER every 32 seconds at
    /// enemies within 150 chunks. Cities only queue it while the nuke toggle is on.
    internal static class SiloLaunchEvents
    {
        internal const string SiloId = "MissileSilo";
        private const int SiloChunkRange = 150;
        private const int MinimumTargetHealth = 8000;
        private static readonly Dictionary<CityBuildOrderAsset, BuildOrder> Orders = new Dictionary<CityBuildOrderAsset, BuildOrder>();
        private static bool? _ordersActive;
        internal static long LaunchCount { get; private set; }

        internal static void RegisterOrder(string race, CityBuildOrderAsset orders)
        {
            if (orders == null) return;
            string orderId = "order_m2_" + race + "_" + SiloId;
            BuildOrder order = orders.list.Find(candidate => candidate != null && candidate.id == orderId);
            if (order == null)
            {
                // Commerce.turnOnNukes: one silo from 50 people and 16 buildings, nothing else.
                order = orders.addBuilding(orderId, 1, 50, 16, false, false, 0);
            }
            Orders[orders] = order;
            _ordersActive = null;
            SyncOrders();
        }

        internal static void SyncOrders()
        {
            bool active = ModernBoxSettings.Get("NukeOption");
            if (_ordersActive == active) return;
            _ordersActive = active;
            foreach (KeyValuePair<CityBuildOrderAsset, BuildOrder> pair in Orders)
            {
                bool present = pair.Key.list.Contains(pair.Value);
                if (active && !present) pair.Key.list.Add(pair.Value);
                else if (!active && present) pair.Key.list.Remove(pair.Value);
                pair.Key.prepareForAssetGeneration();
            }
        }

        internal static BaseSimObject FindTarget(Building silo)
        {
            if (silo == null || silo.current_tile == null || silo.kingdom == null) return null;
            EnemyFinderData data = EnemiesFinder.findEnemiesFrom(silo.current_tile, silo.kingdom, SiloChunkRange);
            if (data == null || data.isEmpty()) return null;
            BaseSimObject candidate = silo.checkObjectList(data.list, silo.asset.tower_attack_buildings, Randy.randomChance(0.6f), false);
            if (candidate == null || candidate.kingdom == silo.kingdom) return null;
            if (candidate.isActor() && candidate.getHealth() < MinimumTargetHealth) return null;
            LaunchCount++;
            return candidate;
        }
    }

    [HarmonyPatch(typeof(BuildingTower), "findTarget")]
    internal static class MissileSiloTargetPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BuildingTower __instance, ref BaseSimObject __result)
        {
            Building building = __instance == null ? null : __instance.building;
            if (building == null || building.asset == null || building.asset.id != SiloLaunchEvents.SiloId) return true;
            __result = SiloLaunchEvents.FindTarget(building);
            return false;
        }
    }
}
