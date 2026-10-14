using HarmonyLib;

namespace ModernBoxM2Rewrite
{
    // Static state keyed by ids or recycled objects must not carry into the next world.
    [HarmonyPatch(typeof(MapBox), nameof(MapBox.clearWorld))]
    internal static class M2WorldReset
    {
        private static void Postfix()
        {
            ProductionService.ResetWorldState();
            M2SpaceUnits.ResetWorldState();
            M2ResearchLog.ResetWorldState();
        }
    }
}
