using ai.behaviours;
using tools;

public class BehWarBoatFindTarget : BehBoat
{
    public override BehResult execute(Actor pActor)
    {
        var boat = pActor.getSimpleComponent<Boat>();
        var kingdom = pActor.kingdom;
        if (kingdom == null || !kingdom.hasEnemies())
            return BehResult.Stop;

        using (var enemies = kingdom.getEnemiesKingdoms())
        {
            foreach (var enemyKingdom in enemies)
            {
                City targetCity = null;
                if (enemyKingdom.hasKing() && enemyKingdom.king.isAlive() && enemyKingdom.king.city != null)
                    targetCity = enemyKingdom.king.city;
                else if (enemyKingdom.cities.Count > 0)
                    targetCity = enemyKingdom.cities.GetRandom();

                if (targetCity != null)
                {
                    WorldTile targetTile = null;
                    if (targetCity.hasAttackZoneOrder() && targetCity.target_attack_zone != null)
                        targetTile = targetCity.target_attack_zone.centerTile;
                    if (targetTile == null)
                        targetTile = targetCity.getTile();

                    if (targetTile != null)
                    {
                        WorldTile oceanTile = OceanHelper.findTileForBoat(pActor.current_tile, targetTile);
                        if (oceanTile != null)
                        {
                            pActor.beh_tile_target = oceanTile;
                            return BehResult.Continue;
                        }
                    }
                }
            }
        }
        return BehResult.Stop;
    }
}
