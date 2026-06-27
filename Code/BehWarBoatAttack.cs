using ai.behaviours;
using tools;
using UnityEngine;

public class BehWarBoatAttack : BehBoat
{
    public override BehResult execute(Actor pActor)
    {
        if (pActor == null || pActor.current_tile == null || World.world?.units == null)
        {
            return BehResult.Stop;
        }

        float attackRange = Mathf.Max(8f, pActor.getAttackRange() + 3f);
        int chunkRadius = Mathf.Clamp(Mathf.CeilToInt(attackRange / 12f), 1, 5);

        foreach (Actor actor in Finder.getUnitsFromChunk(pActor.current_tile, chunkRadius, attackRange, false))
        {
            if (actor != null && actor.isAlive() && actor.kingdom != null && actor.kingdom.isEnemy(pActor.kingdom))
            {
                if (pActor.isInAttackRange(actor))
                {
                    pActor.tryToAttack(actor);
                    return BehResult.Continue;
                }
            }
        }
      if (pActor.beh_tile_target != null && Toolbox.DistTile(pActor.current_tile, pActor.beh_tile_target) <= 2)
        {
            return BehResult.Continue;
        }

        return BehResult.Stop;
    }
}
