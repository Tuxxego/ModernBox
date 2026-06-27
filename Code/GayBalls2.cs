using HarmonyLib;
namespace ModernBox
{
    [HarmonyPatch(typeof(BaseAssetLibrary), nameof(BaseAssetLibrary.logAssetError))]
    public static class Patch_SuppressAssetSpam
    {
        static bool Prefix(string pMessage, string pRightPart)
        {
            if (string.IsNullOrEmpty(pMessage))
                return true;

            if (pMessage.Contains("duplicate asset"))
                return false;

            if (pMessage.Contains("animation"))
                return false;

            if (pMessage.Contains("Shadow size is too small") ||
                pMessage.Contains("Egg shadow size is too small") ||
                pMessage.Contains("Baby shadow size is too small"))
                return false;

            return true;
        }
    }
}