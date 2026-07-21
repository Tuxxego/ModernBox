using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class SiloLaunchEvents
    {
        internal const string AssetId = "modernbox_missile_silo_launch";
        private static readonly Dictionary<int, float> LastLaunch = new Dictionary<int, float>();
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
            ModernLocalization.Add(AssetId, "$kingdom$ launched a nuclear missile from $city$ at $target$!");
        }

        private static void FormatText(WorldLogMessage message, ref string text)
        {
            AssetManager.world_log_library.updateText(ref text, message, "$kingdom$", 1);
            AssetManager.world_log_library.updateText(ref text, message, "$city$", 2);
            AssetManager.world_log_library.updateText(ref text, message, "$target$", 3);
        }

        internal static void Update()
        {
            if (!ModernBoxSettings.Get("NukeOption") || World.world == null || World.world.isPaused()) return;
            foreach (City city in World.world.cities.list.ToArray())
            {
                if (!ModernProgression.IsSupportedCity(city) || city.kingdom == null || !city.kingdom.hasEnemies()) continue;
                List<Building> silos = city.getBuildingListOfID("MissileSilo");
                if (silos == null) continue;
                foreach (Building silo in silos.ToArray())
                {
                    if (silo == null || !silo.isAlive() || !silo.isUsable() || silo.isUnderConstruction()) continue;
                    int key = silo.GetHashCode();
                    float last;
                    if (LastLaunch.TryGetValue(key, out last) && Time.time - last < 32f) continue;
                    Kingdom attacker = city.kingdom;
                    City targetCity = FindWarTarget(attacker);
                    if (targetCity == null) continue;
                    Kingdom targetKingdom = targetCity.kingdom;
                    if (targetKingdom == null || targetKingdom == attacker || !attacker.isInWarWith(targetKingdom)) continue;
                    Building target = targetCity.buildings.FirstOrDefault(building =>
                        building != null && building.isAlive() && building.kingdom == targetKingdom);
                    WorldTile targetTile = target == null ? targetCity.getTile(false) : target.current_tile;
                    if (targetTile == null) continue;
                    Vector3 targetPosition = target == null ? targetTile.posV3 : target.current_position;

                    // Revalidate immediately before launch. getEnemiesKingdoms can
                    // contain stale entries while wars/captures are being resolved.
                    // A null projectile target also freezes the selected hostile
                    // coordinate instead of following a building whose ownership
                    // changes while the missile is in flight.
                    if (city.kingdom != attacker || targetCity.kingdom != targetKingdom ||
                        targetKingdom == attacker || !attacker.isInWarWith(targetKingdom)) continue;
                    World.world.projectiles.spawn(silo, null, "NUKER", silo.current_position, targetPosition);
                    LastLaunch[key] = Time.time;
                    Notify(silo, targetPosition, targetKingdom.name);
                    break;
                }
            }
        }

        private static City FindWarTarget(Kingdom attacker)
        {
            if (attacker == null) return null;
            ListPool<Kingdom> enemies = attacker.getEnemiesKingdoms();
            if (enemies == null || enemies.Count == 0) return null;
            for (int index = 0; index < enemies.Count; index++)
            {
                Kingdom enemy = enemies[index];
                if (enemy == null || enemy == attacker || enemy.wild || !attacker.isInWarWith(enemy) || enemy.cities.Count == 0) continue;
                City candidate = enemy.cities[UnityEngine.Random.Range(0, enemy.cities.Count)];
                if (candidate == null || candidate.kingdom != enemy || candidate.kingdom == attacker) continue;
                return candidate;
            }
            return null;
        }

        internal static void Notify(Building silo, Vector3 targetPosition)
        {
            Notify(silo, targetPosition, "an enemy kingdom");
        }

        internal static void Notify(Building silo, Vector3 targetPosition, string targetName)
        {
            if (silo == null || silo.asset == null || silo.asset.id != "MissileSilo") return;
            WorldLogAsset asset = AssetManager.world_log_library.get(AssetId);
            if (asset == null) return;
            City city = silo.getCity();
            Kingdom kingdom = silo.kingdom ?? (city == null ? null : city.kingdom);
            string kingdomName = kingdom == null ? "An unknown nation" : kingdom.name;
            string cityName = city == null ? "an unknown silo" : city.name;
            WorldLogMessage message = new WorldLogMessage(asset, kingdomName, cityName, targetName)
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
            ModernBoxDiagnostics.Info(kingdomName + " launched a silo nuclear missile from " + cityName + " at " + targetName + ".");
        }
    }
}
