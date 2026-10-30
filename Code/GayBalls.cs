using HarmonyLib;
using System;
using System.Collections.Generic;

namespace ModernBox
{
    [HarmonyPatch(typeof(ActorManager), nameof(ActorManager.prepareForMetaChecks))]
    public static class GayBalls
    {
        static bool Prefix(ActorManager __instance)
        {
            try
            {
                var wild = __instance.units_only_wild;
                var civ = __instance.units_only_civ;
                var alive = __instance.units_only_alive;
                var dying = __instance.units_only_dying;

                wild?.Clear();
                civ?.Clear();
                alive?.Clear();
                dying?.Clear();

                __instance.have_dying_units = false;

                List<Actor> simpleList = __instance.getSimpleList();
                if (simpleList == null)
                    return false;

                int count = simpleList.Count;
                for (int index = 0; index < count; ++index)
                {
                    Actor actor = simpleList[index];
                    if (actor == null)
                        continue;

                    if (actor.isAlive())
                    {
                        Kingdom kingdom = actor.kingdom;
                        if (kingdom != null && kingdom.wild)
                            wild?.Add(actor);
                        else
                            civ?.Add(actor);

                        alive?.Add(actor);
                    }
                    else
                    {
                        dying?.Add(actor);
                        __instance.have_dying_units = true;
                    }
                }
            }
            catch (Exception e)
            {
                ModernBoxLogger.Error($"[prepareForMetaChecks] Error: {e}");
            }

            return false;
        }
    }
}
