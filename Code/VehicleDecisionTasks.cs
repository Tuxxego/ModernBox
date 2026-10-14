using System;
using ai.behaviours;

namespace ModernBox
{
    // Manu-Fix 031: WorldBox evaluates action_check_launch for every eligible candidate,
    // then selects one decision and applies only that decision's cooldown. Firing in the
    // check bypassed this selection. Decision names also require real task assets in 0.51.2.
    internal static class VehicleDecisionTasks
    {
        internal static void Install()
        {
            Fire("missileArtilleryDecision", "Missile artillery", a => Vehicles.MissileArtilleryEffect(a), 0);
            Fire("HORDEmissileArtilleryDecision", "Horde missile artillery", a => Vehicles.HORDEmissileArtilleryEffect(a), 0);
            Fire("HARDENmissileArtilleryDecision", "Harden missile artillery", a => Vehicles.HARDENmissileArtilleryEffect(a), 0);
            Fire("GAIAmissileArtilleryDecision", "Gaia missile artillery", a => Vehicles.GAIAmissileArtilleryEffect(a), 0);
            Fire("nuclearmissileDecision", "Nuclear missile strike", a => Vehicles.NuclearMissileArtilleryEffect(a), 50);
            Fire("AntiBossNukeDecision", "Nuclear strike against a powerful enemy", a => Vehicles.AntiBossNuke(a), 10, true);

            // Flight, navigation, repair and reload timers already run in the existing
            // Actor.b6_updateAI postfix. These named finite state tasks deliberately do
            // not run that controller a second time or add another elapsed-time tick.
            State("bomber_force_reload_rtb", "Return to base for ammunition");
            State("bomber_land_and_reload", "Land and reload");
            State("bomber_takeoff_for_war", "Take off for combat");
            State("bomber_engage_enemy_targets", "Engage enemy targets");
            State("bomber_peace_station", "Station aircraft in peacetime");

            var navalTask = AssetManager.tasks_actor.get("warBoatAttackDecision");
            if (navalTask != null) RegisterTaskText(navalTask, "Naval attack");
        }

        private static void Fire(string id, string label, Func<Actor, bool> fire, int gold, bool boss = false)
        {
            DecisionAsset decision = AssetManager.decisions_library.get(id);
            if (decision == null) throw new InvalidOperationException("ModernBox decision missing: " + id);
            decision.action_check_launch = actor => CanFire(actor, gold, boss);
            AddTask(decision, label, new FireAction(fire));
        }

        private static void State(string id, string label)
        {
            DecisionAsset decision = AssetManager.decisions_library.get(id);
            if (decision == null) throw new InvalidOperationException("ModernBox decision missing: " + id);
            AddTask(decision, label, new FlightStateAction());
        }

        private static void AddTask(DecisionAsset decision, string label, BehaviourActionActor action)
        {
            var task = new BehaviourTaskActor { id = decision.id };
            task.setIcon(decision.path_icon);
            task.addBeh(action);
            AssetManager.tasks_actor.add(task);
            decision.task_id = task.id;
            RegisterTaskText(task, label);
        }

        private static void RegisterTaskText(BehaviourTaskActor task, string label)
        {
            // DecisionAsset.getLocalizedText uses StringExtension.Localize, which
            // underscores its argument; actor task text also reads the literal key.
            ModernBoxLocale.Register(task.getLocaleID(), label);
            ModernBoxLocale.Register(task.getLocaleID().Underscore(), label);
        }

        // No RNG, projectile, money, cooldown, target assignment or movement changes.
        // In particular, City.getTile() is avoided: its cache miss can shuffle buildings.
        private static bool CanFire(Actor actor, int gold, bool boss)
        {
            if (actor == null || !actor.isAlive() || actor.kingdom == null) return false;
            if (gold > 0 && (!Vehicles.nukesEnabled || !Development.NuclearAllowed(actor.kingdom) ||
                actor.city == null || actor.city.amount_gold < gold)) return false;
            if (boss)
            {
                foreach (Actor target in World.world.units)
                    if (target != null && target != actor && target.isAlive() && target.kingdom != null &&
                        actor.kingdom.isEnemy(target.kingdom) && target.stats["health"] >= 10000f) return true;
                return false;
            }
            // Anti-boss defense also targets hostile wild kingdoms without a declared war.
            // Only the city-artillery branch requires the original diplomatic war condition.
            if (!actor.kingdom.hasEnemies()) return false;
            using (var enemies = actor.kingdom.getEnemiesKingdoms())
                foreach (Kingdom enemy in enemies)
                {
                    if (!enemy.hasKing()) continue;
                    foreach (City city in enemy.cities)
                    {
                        if (city == null) continue;
                        if (enemy.king.isAlive() || (city.hasLeader() && city.leader.isAlive())) return true;
                        foreach (Building building in city.buildings)
                            if (building != null && building.current_tile != null) return true;
                        foreach (TileZone zone in city.zones)
                            if (zone?.centerTile != null && !zone.centerTile.Type.ocean) return true;
                    }
                }
            return false;
        }

        private sealed class FireAction : BehaviourActionActor
        {
            private readonly Func<Actor, bool> _fire;
            internal FireAction(Func<Actor, bool> fire) { _fire = fire; }
            public override BehResult execute(Actor actor)
            {
                // Re-check the live target and funds in the original action at execution time.
                return actor != null && actor.isAlive() && _fire(actor) ? BehResult.Continue : BehResult.Stop;
            }
        }

        private sealed class FlightStateAction : BehaviourActionActor
        {
            public override BehResult execute(Actor actor)
            {
                // AiSystem.run advances action_index on Continue and ends this one-action
                // task on its next pass, like a vanilla finite task. The flight controller
                // remains the sole owner of movement and elapsed repair/reload time.
                return BehResult.Continue;
            }
        }
    }
}
