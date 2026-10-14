using System;
using System.Collections.Generic;
using ai.behaviours;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// The old follow_same_race task, removed in 0.51.2. M2's zombie, dune critter and
    /// crusader jobs used it to walk towards an older unit of the same race on the
    /// same island, which is what pulled zombies together into hordes.
    /// </summary>
    internal static class M2FollowSameRace
    {
        internal const string TaskId = "follow_same_race";

        // Old engine races of the units that run this task. Everything else falls
        // back to its own asset id.
        private static readonly Dictionary<string, string> Races = new Dictionary<string, string>(StringComparer.Ordinal);

        internal static void Register(IEnumerable<string> undead, IEnumerable<string> good, IEnumerable<string> duneMonsters)
        {
            foreach (string id in undead) Races[id] = "undead";
            foreach (string id in good) Races[id] = "good";
            foreach (string id in duneMonsters) Races[id] = "dunemonster";

            if (AssetManager.tasks_actor.get(TaskId) != null) return;
            BehaviourTaskActor task = new BehaviourTaskActor { id = TaskId, locale_key = "task_unit_follow_family" };
            AssetManager.tasks_actor.add(task);
            task.addBeh(new BehFindSameRaceActor());
            task.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameRegion));
        }

        internal static string RaceOf(ActorAsset asset)
        {
            return Races.TryGetValue(asset.id, out string race) ? race : asset.id;
        }

        private sealed class BehFindSameRaceActor : BehaviourActionActor
        {
            public override BehResult execute(Actor pActor)
            {
                List<Actor> actors = pActor.current_tile.region.island.actors;
                actors.ShuffleOne();
                string race = RaceOf(pActor.asset);
                double created = pActor.data.created_time;
                // The original kept the last match while walking forwards, so walking
                // backwards and taking the first match picks the same unit.
                for (int i = actors.Count - 1; i >= 0; i--)
                {
                    Actor actor = actors[i];
                    if (actor == pActor || !actor.isAlive()) continue;
                    if (!actor.current_tile.isSameIsland(pActor.current_tile)) continue;
                    if (RaceOf(actor.asset) != race || actor.data.created_time > created) continue;
                    pActor.beh_actor_target = actor;
                    return BehResult.Continue;
                }
                pActor.beh_actor_target = null;
                return BehResult.Stop;
            }
        }
    }
}
