using System;

namespace ModernBoxM2Rewrite
{
    internal static partial class M2Space
    {
        internal static void Register()
        {
            try
            {
                M2SpaceManager.EnsureHost();
                M2PlanetGenerator.Register();
                M2SpaceUnits.Register();
                ModernLocalization.Apply();
                ModernBoxDiagnostics.Info("Space systems registered (star map, planets, unit transport, TUDDS).");
            }
            catch (Exception ex)
            {
                ModernBoxDiagnostics.Error("Space systems failed to register: " + ex);
            }
        }

        static partial void OpenStarMapImpl(ref bool handled)
        {
            handled = true;
            M2SpaceManager.EnableSpace();
        }
    }
}
