using ai.behaviours;
using tools;

public class BehWarBoatAttack : BehBoat
{
    public override BehResult execute(Actor pActor)
    {
        if (pActor == null || pActor.current_tile == null || pActor.kingdom == null || !pActor.kingdom.hasEnemies())
        {
            return BehResult.Stop;
        }

        // Optimization: Query only nearby map chunks instead of scanning all World.world.units globally every frame
        foreach (Actor actor in Finder.getUnitsFromChunk(pActor.current_tile, 3))
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
