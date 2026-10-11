using System.Collections.Generic;
using HarmonyLib;

namespace ModernBoxM1Rewrite
{
    /// M1's MissileSilo: a city tower that fires NUKER every 64-128 seconds at enemies
    /// within 25 chunks. Cities can only build it once the nuke toggle has been switched
    /// on, and switching it off again does nothing until the game is restarted.
    internal static class M1Silo
    {
        internal const string SiloId = "MissileSilo";
        private const int SiloChunkRange = 25;
        private static readonly Dictionary<CityBuildOrderAsset, BuildOrder> Orders = new Dictionary<CityBuildOrderAsset, BuildOrder>();
        private static bool _enabled;

        internal static void RegisterOrder(string race, CityBuildOrderAsset orders)
        {
            if (orders == null) return;
            string orderId = "order_m1_" + race + "_" + SiloId;
            BuildOrder order = orders.list.Find(candidate => candidate != null && candidate.id == orderId);
            if (order == null) order = orders.addBuilding(orderId, 1, 50, 16, false, false, 0);
            Orders[orders] = order;
            if (!_enabled) orders.list.Remove(order);
            orders.prepareForAssetGeneration();
        }

        // Commerce.turnOnNukes. Turning it on twice added the order twice in the original;
        // here the second time changes nothing.
        internal static void TurnOn()
        {
            if (_enabled) return;
            _enabled = true;
            foreach (KeyValuePair<CityBuildOrderAsset, BuildOrder> pair in Orders)
            {
                if (!pair.Key.list.Contains(pair.Value)) pair.Key.list.Add(pair.Value);
                pair.Key.prepareForAssetGeneration();
            }
        }

        // Commerce.findEnemyObjectTarget_prefix: the closest target 60% of the time,
        // searched over 25 chunks instead of the usual 1.
        internal static BaseSimObject FindTarget(Building silo)
        {
            if (silo == null || silo.current_tile == null || silo.kingdom == null) return null;
            EnemyFinderData data = EnemiesFinder.findEnemiesFrom(silo.current_tile, silo.kingdom, SiloChunkRange);
            if (data == null || data.isEmpty()) return null;
            return silo.checkObjectList(data.list, true, Randy.randomChance(0.6f), false);
        }
    }

    [HarmonyPatch(typeof(BuildingTower), "findTarget")]
    internal static class M1SiloTargetPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BuildingTower __instance, ref BaseSimObject __result)
        {
            Building building = __instance == null ? null : __instance.building;
            if (building == null || building.asset == null || building.asset.id != M1Silo.SiloId) return true;
            __result = M1Silo.FindTarget(building);
            return false;
        }
    }
}
