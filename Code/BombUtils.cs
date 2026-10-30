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

        // Manu-Fix 033: MapBox survives loading/generation, but clearWorld advances this seed.
        // A blast belongs only to the world that started it; never stop unrelated coroutines.
        private static bool IsBombWorldCurrent(MapBox bombWorld, int bombSeed)
        {
            return bombWorld != null && object.ReferenceEquals(World.world, bombWorld)
                && MapBox.current_world_seed_id == bombSeed;
        }

        public void Explode(WorldTile centerTile, float radius, float tilesPerFrame, string toptiletype, string tiletype, string terraform, bool coolThing, string traitAdd = null, bool zombie = false, bool StopTerraform = false)
        {
            if (centerTile == null)
                return;

            World.world.StartCoroutine(NoLongerStupid(centerTile, radius, tilesPerFrame, toptiletype, tiletype, terraform, coolThing, traitAdd, zombie, StopTerraform));
        }

        private static Dictionary<WorldTile, List<Actor>> BuildTileActorLookup(MapBox bombWorld, int minX, int maxX, int minY, int maxY)
        {
            var map = new Dictionary<WorldTile, List<Actor>>();
            if (bombWorld?.units == null) return map;
            foreach (Actor a in bombWorld.units)
            {
                if (a == null || !a.isAlive()) continue;
                WorldTile t = a.current_tile;
                if (t == null || t.x < minX || t.x > maxX || t.y < minY || t.y > maxY) continue;
                if (!map.TryGetValue(t, out List<Actor> list))
                {
                    list = new List<Actor>(2);
                    map[t] = list;
                }
                list.Add(a);
            }
            return map;
        }

        private IEnumerator NoLongerStupid(WorldTile centerTile, float radius, float tilesPerFrame, string toptiletype, string tiletype, string terraform = "czar_bomba", bool coolThing = false, string traitAdd = null, bool zombie = false, bool StopTerraform = false)
        {
            MapBox bombWorld = World.world;
            int bombSeed = MapBox.current_world_seed_id;
            if (centerTile == null || !IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
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
            bool needActors = !string.IsNullOrEmpty(traitAdd) || zombie;

            int minMapX = Mathf.Max(0, center.x - intRadius);
            int maxMapX = center.x + intRadius;
            int minMapY = Mathf.Max(0, center.y - intRadius);
            int maxMapY = center.y + intRadius;

            // Optimization: Only build a tile->actors lookup once per frame when traitAdd or zombie is actually used,
            // instead of scanning all 20,000+ units in bombWorld.units via LINQ for every single tile!
            Dictionary<WorldTile, List<Actor>> actorsByTile = needActors
                ? BuildTileActorLookup(bombWorld, minMapX, maxMapX, minMapY, maxMapY)
                : null;

            int tilesProcessed = 0;

            for (int r = 0; r <= intRadius; r++)
            {
                int minD2 = r > 0 ? (r * r - r + 1) : 0;
                int maxD2 = r * r + r;

                for (int x = -r; x <= r; x++)
                {
                    int checkX = center.x + x;
                    if (checkX < 0) continue;

                    int x2 = x * x;
                    int maxY2 = maxD2 - x2;
                    if (maxY2 < 0) continue;

                    int maxAbsY = Mathf.Min(r, (int)Mathf.Sqrt(maxY2));
                    int minY2 = minD2 - x2;
                    int minAbsY = minY2 <= 0 ? 0 : ((int)Mathf.Sqrt(minY2 - 1) + 1);

                    for (int y = -maxAbsY; y <= maxAbsY; y++)
                    {
                        if (minAbsY > 0 && y == -minAbsY + 1)
                        {
                            y = minAbsY - 1;
                            continue;
                        }

                        int checkY = center.y + y;
                        if (checkY < 0) continue;

                        if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                        WorldTile tile = bombWorld.GetTile(checkX, checkY);
                        if (tile != null)
                        {
                            if (needActors && actorsByTile != null && actorsByTile.TryGetValue(tile, out List<Actor> actorsOnTile))
                            {
                                for (int i = 0; i < actorsOnTile.Count; i++)
                                {
                                    Actor actor = actorsOnTile[i];
                                    if (actor == null || !actor.isAlive() || actor.current_tile != tile) continue;
                                    if (!string.IsNullOrEmpty(traitAdd))
                                    {
                                        actor.addTrait(traitAdd);
                                    }
                                    if (zombie)
                                    {
                                        ActionLibrary.turnIntoZombie(actor);
                                    }
                                }
                            }

                            // Manu-Fix 033: infection-only blasts need the same per-frame tile budget.
                            if (!StopTerraform)
                            {
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
                            }

                            tilesProcessed++;

                            if (tilesProcessed >= inttilesPerFrame)
                            {
                                tilesProcessed = 0;
                                yield return null;
                                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                                if (needActors)
                                {
                                    actorsByTile = BuildTileActorLookup(bombWorld, minMapX, maxMapX, minMapY, maxMapY);
                                }
                            }
                        }
                    }
                }
            }

            if (tilesProcessed > 0)
            {
                yield return null;
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            }
        }

        private static IEnumerator RepBombsCoroutine(WorldTile centerTile, float radius, float tilesPerFrame, string toptiletype, string tiletype, int ammountOfBombs, float smallestSize, float largestSize,  string terraform = "czar_bomba", bool coolThing = false, int timeBetweenBombs = 3, string effect = "fx_explopsion_huge")
        {
            MapBox bombWorld = World.world;
            int bombSeed = MapBox.current_world_seed_id;
            if (centerTile == null || !IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            float TBB = timeBetweenBombs;
            int AOB = ammountOfBombs;

            for (int i = 0; i < AOB; i++)
            {
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                StartEffect(centerTile, effect, smallestSize, largestSize);
                BombUtilities.Instance.Explode(centerTile, radius, tilesPerFrame, toptiletype, tiletype, terraform, coolThing);
                yield return new WaitForSeconds(TBB);
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            }

        }

        private static void StartEffect(WorldTile pTile, string effect, float smallestSize, float largestSize)
        {
            EffectsLibrary.spawnAtTileRandomScale(effect, pTile, smallestSize, largestSize);
        }

        private IEnumerator ClusterNukeCoroutine(WorldTile pTile)
        {
            MapBox bombWorld = World.world;
            int bombSeed = MapBox.current_world_seed_id;
            if (pTile == null || !IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            float duration = 5f;
            float interval = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                elapsed += interval;

                WorldTile randomTile = GetRandomTileWithinRadius(pTile, 35, bombWorld);
                if (randomTile != null)
                {
                    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", randomTile, 0.4f, 0.6f);
                    bombWorld.startShake(0.3f, 0.01f, 2f, true, true);
                    if (bombWorld.explosion_checker.checkNearby(randomTile, 20))
                    {
                        yield return new WaitForSeconds(interval);
                        if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                    }
                    StatManager.Instance.DropBomb();
                    Explode(pTile, 50f, 1000f, null, null, "czar_bomba", false);
                }

                yield return new WaitForSeconds(interval);
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            }
        }

        private WorldTile GetRandomTileWithinRadius(WorldTile centerTile, int radius, MapBox bombWorld)
        {
            int x = centerTile.x + UnityEngine.Random.Range(-radius, radius + 1);
            int y = centerTile.y + UnityEngine.Random.Range(-radius, radius + 1);
            return bombWorld.GetTile(x, y);
        }

        public void ExplodeInIce(WorldTile centerTile, float radius)
        {
            if (centerTile == null)
                return;

            World.world.StartCoroutine(ThisIceIsSoStupid(centerTile, radius));
        }

        private IEnumerator ThisIceIsSoStupid(WorldTile centerTile, float radius)
        {
            MapBox bombWorld = World.world;
            int bombSeed = MapBox.current_world_seed_id;
            if (centerTile == null || !IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            Vector2Int center = centerTile.pos;
            int intRadius = Mathf.CeilToInt(radius);
            int inttilesPerFrame = 400;

            for (int r = 0; r <= intRadius; r++)
            {
                int tilesProcessed = 0;
                int minD2 = r > 0 ? (r * r - r + 1) : 0;
                int maxD2 = r * r + r;
                for (int x = -r; x <= r; x++)
                {
                    if (center.x + x < 0) continue;
                    int x2 = x * x;
                    int maxY2 = maxD2 - x2;
                    if (maxY2 < 0) continue;
                    int maxAbsY = Mathf.Min(r, (int)Mathf.Sqrt(maxY2));
                    int minY2 = minD2 - x2;
                    int minAbsY = minY2 <= 0 ? 0 : ((int)Mathf.Sqrt(minY2 - 1) + 1);
                    for (int y = -maxAbsY; y <= maxAbsY; y++)
                    {
                        if (minAbsY > 0 && y == -minAbsY + 1) { y = minAbsY - 1; continue; }

                        int checkX = center.x + x;
                        int checkY = center.y + y;

                        if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                        WorldTile tile = bombWorld.GetTile(checkX, checkY);
                        if (tile != null)
                        {
                            tile.freeze();
                            tilesProcessed++;
                        }

                        if (tilesProcessed >= inttilesPerFrame)
                        {
                            tilesProcessed = 0;
                            yield return null;
                            if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                        }
                    }
                }
                yield return null;
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
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
            MapBox bombWorld = World.world;
            int bombSeed = MapBox.current_world_seed_id;
            if (centerTile == null || !IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            Vector2Int center = centerTile.pos;
            int intRadius = Mathf.CeilToInt(radius);
            int inttilesPerFrame = 400;

            for (int r = 0; r <= intRadius; r++)
            {
                int tilesProcessed = 0;
                int minD2 = r > 0 ? (r * r - r + 1) : 0;
                int maxD2 = r * r + r;
                for (int x = -r; x <= r; x++)
                {
                    if (center.x + x < 0) continue;
                    int x2 = x * x;
                    int maxY2 = maxD2 - x2;
                    if (maxY2 < 0) continue;
                    int maxAbsY = Mathf.Min(r, (int)Mathf.Sqrt(maxY2));
                    int minY2 = minD2 - x2;
                    int minAbsY = minY2 <= 0 ? 0 : ((int)Mathf.Sqrt(minY2 - 1) + 1);
                    for (int y = -maxAbsY; y <= maxAbsY; y++)
                    {
                        if (minAbsY > 0 && y == -minAbsY + 1) { y = minAbsY - 1; continue; }

                        int checkX = center.x + x;
                        int checkY = center.y + y;

                        if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                        WorldTile tile = bombWorld.GetTile(checkX, checkY);
                        if (tile != null)
                        {
                            tile.setFireData(true);
                            tilesProcessed++;
                        }

                        if (tilesProcessed >= inttilesPerFrame)
                        {
                            tilesProcessed = 0;
                            yield return null;
                            if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
                        }
                    }
                }
                yield return null;
                if (!IsBombWorldCurrent(bombWorld, bombSeed)) yield break;
            }
        }

        private IEnumerator PotentiallyWorkingAntiMatterBombFixUpdateFromTuxThisDOESNOTWORKButImLeavingItInCuzWhoKnowsMaybeIllReturnToItAndFixIt(WorldTile centerTile, float radius, string tileType)
        {
            yield return null;
        }
    }
}
