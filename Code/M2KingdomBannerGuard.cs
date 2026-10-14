using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    // Kingdom banners come from the kingdom's species banner folder. A kingdom led by a
    // species with no banner pictures (an empty list) made GenericBannerLibrary index an
    // empty list and the nameplates threw every frame. Fall back to the human banners.
    internal static class M2KingdomBannerGuard
    {
        internal static string BannerId(Kingdom kingdom, bool icons)
        {
            ActorAsset asset = kingdom == null ? null : kingdom.getActorAsset();
            string id = asset == null ? null : asset.banner_id;
            if (Usable(id, icons)) return id;
            return "human";
        }

        private static bool Usable(string id, bool icons)
        {
            if (string.IsNullOrEmpty(id)) return false;
            BannerAsset banner = AssetManager.kingdom_banners_library.get(id);
            if (banner == null) return false;
            return icons ? banner.icons != null && banner.icons.Count > 0 : banner.backgrounds != null && banner.backgrounds.Count > 0;
        }

        internal static int Index(int index)
        {
            return index < 0 ? 0 : index;
        }
    }

    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.getElementBackground))]
    internal static class M2KingdomBannerBackgroundPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Kingdom __instance, ref Sprite __result)
        {
            __result = AssetManager.kingdom_banners_library.getSpriteBackground(
                M2KingdomBannerGuard.Index(__instance.data.banner_background_id), M2KingdomBannerGuard.BannerId(__instance, false));
            return false;
        }
    }

    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.getElementIcon))]
    internal static class M2KingdomBannerIconPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Kingdom __instance, ref Sprite __result)
        {
            __result = AssetManager.kingdom_banners_library.getSpriteIcon(
                M2KingdomBannerGuard.Index(__instance.data.banner_icon_id), M2KingdomBannerGuard.BannerId(__instance, true));
            return false;
        }
    }
}
