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
                __instance.units_only_wild?.Clear();
                __instance.units_only_civ?.Clear();
                __instance.units_only_alive?.Clear();
                __instance.units_only_dying?.Clear();

                __instance.have_dying_units = false;

                List<Actor> simpleList = null;

                try
                {
                    simpleList = __instance.getSimpleList();
                }
                catch
                {
                    simpleList = null;
                }

                if (simpleList == null)
                    return false;

                for (int index = 0; index < simpleList.Count; ++index)
                {
                    Actor actor = simpleList[index];

                    if (actor == null)
                        continue;

                    Kingdom kingdom = null;

                    try
                    {
                        kingdom = actor.kingdom;
                    }
                    catch
                    {
                        kingdom = null;
                    }

                    bool isAlive = false;

                    try
                    {
                        isAlive = actor.isAlive();
                    }
                    catch
                    {
                        continue;
                    }

                    if (isAlive)
                    {
                        if (kingdom != null && kingdom.wild)
                            __instance.units_only_wild?.Add(actor);
                        else
                            __instance.units_only_civ?.Add(actor);

                        __instance.units_only_alive?.Add(actor);
                    }
                    else
                    {
                        __instance.units_only_dying?.Add(actor);
                        __instance.have_dying_units = true;
                    }
                }
            }
            catch (Exception e)
            {
                ModernBoxLogger.Error($"Gay shit incoming: {e}");
            }

            return false;
        }
    }
}