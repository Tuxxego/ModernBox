using ai.behaviours;
using tools;

public class BehWarBoatAttack : BehBoat
{
    public override BehResult execute(Actor pActor)
    {

        foreach (var actor in World.world.units)
        {
            if (actor.isAlive() && actor.kingdom != null && actor.kingdom.isEnemy(pActor.kingdom))
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
