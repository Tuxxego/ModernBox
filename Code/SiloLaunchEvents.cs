using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class SiloLaunchEvents
    {
        internal const string AssetId = "modernbox_missile_silo_launch";
        internal static long LaunchCount { get; private set; }

        internal static void Register()
        {
            WorldLogAsset asset = AssetManager.world_log_library.get(AssetId);
            if (asset == null)
            {
                asset = new WorldLogAsset
                {
                    id = AssetId,
                    locale_id = AssetId,
                    group = "wars",
                    path_icon = "ui/Icons/Nuke",
                    color = Toolbox.color_log_warning,
                    text_replacer = FormatText
                };
                AssetManager.world_log_library.add(asset);
            }
            ModernLocalization.Add(AssetId, "$kingdom$ launched a nuclear missile from $city$!");
        }

        private static void FormatText(WorldLogMessage message, ref string text)
        {
            AssetManager.world_log_library.updateText(ref text, message, "$kingdom$", 1);
            AssetManager.world_log_library.updateText(ref text, message, "$city$", 2);
        }

        internal static void Notify(Building silo, Vector3 targetPosition)
        {
            if (silo == null || silo.asset == null || silo.asset.id != "MissileSilo") return;
            WorldLogAsset asset = AssetManager.world_log_library.get(AssetId);
            if (asset == null) return;

            City city = silo.getCity();
            Kingdom kingdom = silo.kingdom ?? (city == null ? null : city.kingdom);
            string kingdomName = kingdom == null ? "An unknown nation" : kingdom.name;
            string cityName = city == null ? "an unknown silo" : city.name;
            WorldLogMessage message = new WorldLogMessage(asset, kingdomName, cityName, null)
            {
                location = new Vector2(targetPosition.x, targetPosition.y),
                kingdom = kingdom
            };
            if (kingdom != null && kingdom.getColor() != null)
            {
                Color textColor = kingdom.getColor().getColorText();
                message.color_special1 = textColor;
                message.color_special2 = textColor;
            }
            WorldLogMessageExtensions.add(message);
            LaunchCount++;
            ModernBoxDiagnostics.Info(kingdomName + " launched a silo nuclear missile from " + cityName + ".");
        }
    }
}
