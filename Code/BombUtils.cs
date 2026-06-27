using System.Collections;
using UnityEngine;
using System.Linq; 

using System.Collections.Generic;

namespace ModernBox
{
    public class BombUtilities : MonoBehaviour
    {
        public static BombUtilities Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

        }

        public void action_MOABClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "moab");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }

        public bool action_MOABClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 1.4f, 1.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 90))
                return false;

            Explode(pTile, 50f, 1000f, null, null, "czar_bomba", false);
            ExplodeInFIRE(pTile, 90f);
            return true;
        }

        public void action_ClusterClick(WorldTile pTile, string pPowerID)
        {

            World.world.StartCoroutine(ClusterNukeCoroutine(pTile));
        }

        public void action_IceClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "icebomb");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }

        public bool action_IceClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_ice_bomb", pTile, 1.4f, 1.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 30))
                return false;

            ExplodeInIce(pTile, 30f);
            return true;
        }

        public void action_FireBombClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "firebomb");
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 120))
                return;

            ExplodeInFIRE(pTile, 120f);
            return;
        }

        public void action_TuxiumClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "tuxium");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_TuxiumClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 13.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 290))
                return false;

            Explode(pTile, 190f, 1500f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_HomoClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "homo");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_HomoClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 5.4f, 7.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;

            Explode(pTile, 70f, 1000f, null, null, "czar_bomba", false, "Gay", false, true);
            return true;
        }

        public void action_FuryClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "fury");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_FuryClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 16.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 1900))
                return false;
            Explode(pTile, 1900f, 5000f, null, "deep_ocean", "czar_bomba", true);

            return true;
        }

        public void action_OverloadClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "overload");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_OverloadClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {

            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            World.world.StartCoroutine(RepBombsCoroutine(pTile, 190f, 1000f, null, null, 5, 7.4f, 9.6f, "czar_bomba", false, 5, "fx_explosion_huge"));
            return true;
        }

        public void action_XenoClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "xeno");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_XenoClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 130f, 1300f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_JupiterClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "jupiter");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_JupiterClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 170f, 1500f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_MiniClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "mini");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_MiniClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 4.4f, 6.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 30f, 1000f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_DeathClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "death");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_DeathClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_ground_explosion", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 80f, 900f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_CobaltClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "cobalt");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_CobaltClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 4.4f, 7.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 110f, 1000f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_NoDmgClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "nodmg");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_NoDmgClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_blue", pTile, 5.4f, 8.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 1f, 1f, null, null, "nothing", false);
            return true;
        }

        public void action_AGrenadeClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "agrenade");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_AGrenadeClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_small", pTile, 0.4f, 1.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 20f, 1000f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_ColorGClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "colorg");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_ColorGClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 190f, 1000f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_EraserClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "eraser");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_EraserClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_antimatter_effect", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 350f, 2500f, null, "deep_ocean", "eraser_explosion", true);

            return true;
        }

        public void action_AgonyClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "agony");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_AgonyClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 190f, 1000f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_UltronClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "ultron");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_UltronClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 190f, 1000f, null, null, "czar_bomba", false);
            return true;
        }

        public void action_NSAClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "nsa");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_NSAClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 7.4f, 15.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 1f, 1000f, null, null, "nothing", false);
            return true;
        }

        public void action_DickClick(WorldTile pTile, string pPowerID)
        {
            SpaceManager.DeleteBomb();
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                obj.SetActive(false);
            }

            GameObject endScreenManager = new GameObject("EndScreenManager");
            endScreenManager.AddComponent<UniverseDestructionManager>();
        }
        public bool action_DickClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {

            return true;
        }

        public void action_TestClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "test");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }
        public bool action_TestClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_nuke_atomic", pTile, 4.4f, 6.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 100f, 1300f, null, null, "czar_bomba", false);

            return true;
        }

        public void action_ZombieClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawn("fx_nuke_flash", pTile, "zombie");
            World.world.startShake(pIntensity: 2.5f, pShakeX: true);
        }

        public bool action_ZombieClickOTHER(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 4.4f, 6.6f);
            StatManager.Instance.DropBomb();
            if (World.world.explosion_checker.checkNearby(pTile, 190))
                return false;
            Explode(pTile, 100f, 1300f, null, null, "czar_bomba", false, "infected", true, true);

            return true;
        }

        public void Explode(WorldTile centerTile, float radius, float tilesPerFrame, string toptiletype, string tiletype, string terraform, bool coolThing, string traitAdd = null, bool zombie = false, bool StopTerraform = false)
        {
            if (centerTile == null)
                return;

            World.world.StartCoroutine(NoLongerStupid(centerTile, radius, tilesPerFrame, toptiletype, tiletype, terraform, coolThing, traitAdd, zombie, StopTerraform));
        }

private IEnumerator NoLongerStupid(WorldTile centerTile, float radius, float tilesPerFrame, string toptiletype, string tiletype, string terraform = "czar_bomba", bool coolThing = false, string traitAdd = null, bool zombie = false, bool StopTerraform = false)
        {
            Vector2Int center = centerTile.pos;
            int intRadius = Mathf.CeilToInt(radius);
            int inttilesPerFrame = Mathf.Max(1, Mathf.CeilToInt(tilesPerFrame)); 

            TileType balls = null;
            TopTileType tiley = null;

            if (toptiletype != null)
            {
                tiley = AssetManager.top_tiles.get(toptiletype);
            }
            if (tiletype != null)
            {
                balls = AssetManager.tiles.get(tiletype);
            }

            TerraformOptions terry = AssetManager.terraform.get(terraform);
            bool ct = coolThing;

            int tilesProcessed = 0;

            for (int r = 0; r <= intRadius; r++)
            {
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (Mathf.RoundToInt(Vector2.Distance(new Vector2(x, y), Vector2.zero)) != r)
                            continue;

                        int checkX = center.x + x;
                        int checkY = center.y + y;

                        WorldTile tile = World.world.GetTile(checkX, checkY);
                        if (tile != null)
                        {

                            List<Actor> actorsOnTile = World.world.units.Cast<Actor>()
                                .Where(a => a != null && a.isAlive() && a.current_tile == tile)
                                .ToList();

                            foreach (Actor actor in actorsOnTile)
                            {
                                if (!string.IsNullOrEmpty(traitAdd))
                                {
                                    actor.addTrait(traitAdd);
                                }

                                if (zombie)
                                {
                                    ActionLibrary.turnIntoZombie(actor);
                                    continue; 

                                }
                            }
                            if (StopTerraform)
                            {
                                continue; 

                            }
                            if (!ct)
                            {
                                MapAction.applyTileDamage(tile, radius, terry);
                            }
                            else
                            {
                                tile.setTopTileType(null);
                                tile.setTileType(balls);
                                if (toptiletype != null)
                                {
                                    tile.setTopTileType(tiley);
                                }
                            }

                            tilesProcessed++;

                            if (tilesProcessed >= inttilesPerFrame)
                            {
                                tilesProcessed = 0;
                                yield return null;
                            }
                        }
                    }
                }

            }

            if (tilesProcessed > 0)
                yield return null;
        }

        private static IEnumerator RepBombsCoroutine(WorldTile centerTile, float radius, float tilesPerFrame, string toptiletype, string tiletype, int ammountOfBombs, float smallestSize, float largestSize,  string terraform = "czar_bomba", bool coolThing = false, int timeBetweenBombs = 3, string effect = "fx_explopsion_huge")
        {
            float TBB = timeBetweenBombs;
            int AOB = ammountOfBombs;

            for (int i = 0; i < AOB; i++)
            {
                StartEffect(centerTile, effect, smallestSize, largestSize);
                BombUtilities.Instance.Explode(centerTile, radius, tilesPerFrame, toptiletype, tiletype, terraform, coolThing);
                yield return new WaitForSeconds(TBB);
            }

        }

        private static void StartEffect(WorldTile pTile, string effect, float smallestSize, float largestSize)
        {
            EffectsLibrary.spawnAtTileRandomScale(effect, pTile, smallestSize, largestSize);
        }

        private IEnumerator ClusterNukeCoroutine(WorldTile pTile)
        {
            float duration = 5f;
            float interval = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += interval;

                WorldTile randomTile = GetRandomTileWithinRadius(pTile, 35);
                if (randomTile != null)
                {
                    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", randomTile, 0.4f, 0.6f);
                    World.world.startShake(0.3f, 0.01f, 2f, true, true);
                    if (World.world.explosion_checker.checkNearby(randomTile, 20))
                        yield return new WaitForSeconds(interval);
                    StatManager.Instance.DropBomb();
                    Explode(pTile, 50f, 1000f, null, null, "czar_bomba", false);
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private WorldTile GetRandomTileWithinRadius(WorldTile centerTile, int radius)
        {
            int x = centerTile.x + UnityEngine.Random.Range(-radius, radius + 1);
            int y = centerTile.y + UnityEngine.Random.Range(-radius, radius + 1);
            return World.world.GetTile(x, y);
        }

        public void ExplodeInIce(WorldTile centerTile, float radius)
        {
            if (centerTile == null)
                return;

            World.world.StartCoroutine(ThisIceIsSoStupid(centerTile, radius));
        }

        private IEnumerator ThisIceIsSoStupid(WorldTile centerTile, float radius)
        {
            Vector2Int center = centerTile.pos;
            int intRadius = Mathf.CeilToInt(radius);
            int inttilesPerFrame = 400;

            for (int r = 0; r <= intRadius; r++)
            {
                int tilesProcessed = 0;
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (Mathf.RoundToInt(Vector2.Distance(new Vector2(x, y), Vector2.zero)) != r)
                            continue;

                        int checkX = center.x + x;
                        int checkY = center.y + y;

                        WorldTile tile = World.world.GetTile(checkX, checkY);
                        if (tile != null)
                        {
                            tile.freeze();
                            tilesProcessed++;
                        }

                        if (tilesProcessed >= inttilesPerFrame)
                        {
                            tilesProcessed = 0;
                            yield return null;
                        }
                    }
                }
                yield return null;
            }
        }

        public void ExplodeInFIRE(WorldTile centerTile, float radius)
        {
            if (centerTile == null)
                return;

            World.world.StartCoroutine(ThisShitIsSoFire(centerTile, radius));
        }

        private IEnumerator ThisShitIsSoFire(WorldTile centerTile, float radius)
        {
            Vector2Int center = centerTile.pos;
            int intRadius = Mathf.CeilToInt(radius);
            int inttilesPerFrame = 400;

            for (int r = 0; r <= intRadius; r++)
            {
                int tilesProcessed = 0;
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (Mathf.RoundToInt(Vector2.Distance(new Vector2(x, y), Vector2.zero)) != r)
                            continue;

                        int checkX = center.x + x;
                        int checkY = center.y + y;

                        WorldTile tile = World.world.GetTile(checkX, checkY);
                        if (tile != null)
                        {
                            tile.setFireData(true);
                            tilesProcessed++;
                        }

                        if (tilesProcessed >= inttilesPerFrame)
                        {
                            tilesProcessed = 0;
                            yield return null;
                        }
                    }
                }
                yield return null;
            }
        }

        private IEnumerator PotentiallyWorkingAntiMatterBombFixUpdateFromTuxThisDOESNOTWORKButImLeavingItInCuzWhoKnowsMaybeIllReturnToItAndFixIt(WorldTile centerTile, float radius, string tileType)
        {
            yield return null;
        }
    }
}