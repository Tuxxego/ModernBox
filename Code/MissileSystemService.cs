using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Build-719 equivalent of the strategic MissileSystem controller used by
    /// modern ModernBox. The actor's displayed M2 range remains 150, while this
    /// decision deliberately selects a city belonging to a kingdom that is
    /// actually at war with the launcher. A dedicated decision is required here:
    /// ordinary attacker AI only searches its local range and allows additive
    /// attack-speed stats to turn M2's 0.1 rate into rapid fire.
    /// </summary>
    internal static class MissileSystemService
    {
        internal const string DecisionId = "modernbox_m2_missile_system_launch";
        internal const string StrategicProjectileId = "MIRVartilleryStrategic";
        internal const int LaunchCooldownSeconds = 10;

        internal static void RegisterDecision()
        {
            if (AssetManager.decisions_library.get(DecisionId) != null) return;
            DecisionAsset decision = new DecisionAsset
            {
                id = DecisionId,
                priority = NeuroLayer.Layer_1_Low,
                path_icon = "ui/Icons/MIRV",
                cooldown = LaunchCooldownSeconds,
                unique = true,
                weight = 1f,
                action_check_launch = TryLaunch
            };
            AssetManager.decisions_library.add(decision);
        }

        internal static void ConfigureActor(ActorAsset actor)
        {
            if (actor == null) return;
            string[] decisionJob = { "decision" };
            actor.job = decisionJob;
            actor.job_baby = decisionJob;
            actor.job_citizen = decisionJob;
            actor.job_kingdom = decisionJob;
            actor.job_attacker = decisionJob;
            actor.addDecision("warrior_random_move");
            actor.addDecision(DecisionId);
            actor.addDecision("swim_to_island");
        }

        internal static bool TryLaunch(Actor caster)
        {
            if (caster == null || caster.asset == null || caster.asset.id != "MissileSystem" ||
                !caster.isAlive() || caster.kingdom == null || caster.kingdom.wild ||
                !caster.kingdom.hasEnemies() || World.world == null || World.world.projectiles == null)
                return false;

            City targetCity = FindHostileCity(caster.kingdom);
            if (targetCity == null) return false;
            Kingdom targetKingdom = targetCity.kingdom;
            if (targetKingdom == null || targetKingdom == caster.kingdom ||
                !caster.kingdom.isInWarWith(targetKingdom)) return false;

            Vector2? target = PickTarget(targetCity, targetKingdom);
            if (!target.HasValue) return false;

            // Revalidate immediately before launching so a city captured while the
            // decision was choosing a target can never receive friendly fire.
            if (targetCity.kingdom != targetKingdom || targetKingdom == caster.kingdom ||
                !caster.kingdom.isInWarWith(targetKingdom)) return false;

            Vector3 source = caster.current_position;
            Vector3 targetPosition = target.Value;
            // Launch from the actual vehicle position. The previous forward/upward
            // offset was inherited from the modern strategic-launch approximation
            // and made the sprite appear to rise before travelling to its target.
            // The scoped projectile patch now draws the missile on the direct line
            // from this point to the hostile city target.
            World.world.projectiles.spawn(caster, null, StrategicProjectileId, source, targetPosition);
            caster.attack_timer = Mathf.Max(caster.attack_timer, LaunchCooldownSeconds);
            caster.punchTargetAnimation(targetPosition, true, false, 45f);
            return true;
        }

        private static City FindHostileCity(Kingdom attacker)
        {
            using (ListPool<Kingdom> enemies = attacker.getEnemiesKingdoms())
            {
                if (enemies == null || enemies.Count == 0) return null;
                int start = UnityEngine.Random.Range(0, enemies.Count);
                for (int offset = 0; offset < enemies.Count; offset++)
                {
                    Kingdom enemy = enemies[(start + offset) % enemies.Count];
                    if (enemy == null || enemy == attacker || enemy.wild ||
                        !attacker.isInWarWith(enemy) || enemy.cities == null || enemy.cities.Count == 0)
                        continue;
                    City city = enemy.cities[UnityEngine.Random.Range(0, enemy.cities.Count)];
                    if (city != null && city.kingdom == enemy) return city;
                }
            }
            return null;
        }

        private static Vector2? PickTarget(City city, Kingdom expectedKingdom)
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.33f && city.buildings != null && city.buildings.Count > 0)
            {
                int start = UnityEngine.Random.Range(0, city.buildings.Count);
                for (int offset = 0; offset < city.buildings.Count; offset++)
                {
                    Building building = city.buildings[(start + offset) % city.buildings.Count];
                    if (building != null && building.isAlive() && building.current_tile != null &&
                        building.kingdom == expectedKingdom) return building.current_tile.pos;
                }
            }
            if (roll < 0.66f && city.hasLeader() && city.leader != null && city.leader.isAlive() &&
                city.leader.kingdom == expectedKingdom) return city.leader.current_position;
            if (expectedKingdom.hasKing() && expectedKingdom.king != null && expectedKingdom.king.isAlive())
                return expectedKingdom.king.current_position;
            WorldTile tile = city.getTile(false);
            return tile == null ? (Vector2?)null : tile.pos;
        }
    }
}
