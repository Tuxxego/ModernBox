using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// 0.51.2's applyForceOnTile stuns every unit it pushes for about 4 s. The old
    /// engine's force only knocked units back, so M2 blasts and bombs keep the push
    /// but skip the stun.
    /// </summary>
    internal static class M2NoBlastStun
    {
        private static readonly HashSet<string> Terraforms = new HashSet<string>(StringComparer.Ordinal);
        private static int _suppress;

        internal static void Register(string terraformId)
        {
            if (!string.IsNullOrEmpty(terraformId)) Terraforms.Add(terraformId);
        }

        internal static bool IsM2(TerraformOptions options)
        {
            return options != null && options.id != null && Terraforms.Contains(options.id);
        }

        internal static void Begin() { _suppress++; }
        internal static void End() { if (_suppress > 0) _suppress--; }
        internal static bool Active => _suppress > 0;
    }

    [HarmonyPatch(typeof(MapBox), nameof(MapBox.applyForceOnTile))]
    internal static class M2NoBlastStunForcePatch
    {
        private static void Prefix(TerraformOptions pOptions, out bool __state)
        {
            __state = M2NoBlastStun.IsM2(pOptions);
            if (__state) M2NoBlastStun.Begin();
        }

        private static void Finalizer(bool __state)
        {
            if (__state) M2NoBlastStun.End();
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.makeStunned))]
    internal static class M2NoBlastStunActorPatch
    {
        private static bool Prefix()
        {
            return !M2NoBlastStun.Active;
        }
    }
}
