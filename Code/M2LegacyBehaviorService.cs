using System;
using System.Collections.Generic;
using System.Linq;
using ai;
using NCMS.Utils;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Direct build-719 rewrite of M2's Unitpotential, Potential, boss-spawner,
    /// zombie-evolution, scrap, and era-appearance systems. Work is deliberately
    /// round-robin so large invasion populations cannot freeze the simulation.
    /// </summary>
    internal static class M2LegacyBehaviorService
    {
        private const int ActorsPerTick = 48;
        private const int SpawnerWorldCapPerType = 32;
        private const string ZombieWandererJobId = "ZombieWorse";
        private const string SpawnerFlagPrefix = "modernbox_m2_spawner_";
        private const string WalkerTowerSpawnerFlag = "modernbox_m2_spawner_walker_tower";
        private const string SpawnIceWalkerSpellId = "modernbox_m2_spawn_icewalker";
        private static readonly HashSet<string> ZombieSpawnerActors = new HashSet<string>(StringComparer.Ordinal)
        {
            "zombiestalker", "zombiemother", "zombiehulk", "zombieabomination", "zombieclawed", "zombieballoon"
        };
        private static readonly HashSet<string> AssimilatorSpawnerActors = new HashSet<string>(StringComparer.Ordinal)
        {
            "Assimilatus", "helilator", "assimilatrax", "assizeppelin"
        };
        private static readonly HashSet<string> IceSpawnerActors = new HashSet<string>(StringComparer.Ordinal)
        {
            "Cocytuswalker", "icedracoid"
        };
        private static readonly HashSet<string> PotentialActors = new HashSet<string>(StringComparer.Ordinal)
        {
            "newwalker", "normalwalker", "icedracoid", "buffrost", "assimilator", "assimilarptor",
            "basecrusader", "crusaderdreadnaught", "crusaderHeli", "crusadermaus"
        };
        private static readonly HashSet<string> WalkerEvolutionActors = new HashSet<string>(StringComparer.Ordinal)
        {
            "newwalker", "normalwalker", "icedracoid", "buffrost"
        };
        private static readonly string[] GeneralZombieMutations =
        {
            "zombiespeed", "zombieacid", "zombiestalker", "zombietentacle", "zombiefiremaniac", "zombieballoon",
            "zombiespikes", "zombiepoison", "zombiemother", "zombieabomination", "zombieclawed", "zombiehulk"
        };
        private static float _timer;
        private static int _cursor;
        private static int _worldKey;
        private static bool _zombieTraitCallbackAttached;

        internal static void AttachActorTraits()
        {
            RegisterOriginalZombieWandererJob();
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                ActorAsset asset = AssetManager.actor_library.get(spec.Id);
                if (asset == null) continue;
                if (spec.Role != M2UnitRole.Creature) asset.addTrait("Unitpotential");
                if (PotentialActors.Contains(spec.Id)) asset.addTrait("Potential");
                if (AssimilatorSpawnerActors.Contains(spec.Id)) asset.addTrait("AssimilatorSpawner");
                if (IceSpawnerActors.Contains(spec.Id)) asset.addTrait("IceTowerSpawner");
                if (spec.Id == "Cocytuswalker") asset.addTrait("Walker_Titan");
                if (ZombieSpawnerActors.Contains(spec.Id)) asset.addTrait("zombie_spawner");
                if (spec.Id.StartsWith("zombie", StringComparison.OrdinalIgnoreCase)) asset.addTrait("zombie");
                if (spec.Id == "helilator" || spec.Id == "assimilatrax" || spec.Id == "assizeppelin")
                    asset.addTrait("SolarPoweredCyberBody");
            }

            ConfigureOriginalZombieBalloonAsset();

            // M2 extended the current game's native Assimilator rather than
            // replacing it with a duplicate asset.
            ActorAsset assimilator = AssetManager.actor_library.get("assimilator");
            if (assimilator != null)
            {
                assimilator.addTrait("Potential");
                assimilator.addTrait("SolarPoweredCyberBody");
            }

            // Original M2 did not run zombie evolution from a custom timer. It
            // appended M2zombieEffect to the native zombie trait's periodic
            // action, so the game's own trait scheduler controls every check.
            ActorTrait zombieTrait = AssetManager.traits.get("zombie");
            if (!_zombieTraitCallbackAttached && zombieTrait != null)
            {
                zombieTrait.action_special_effect = (WorldAction)Delegate.Combine(
                    zombieTrait.action_special_effect,
                    new WorldAction(OnOriginalM2ZombieEffect));
                _zombieTraitCallbackAttached = true;
            }

            RegisterCocytusSummonSpell();
            RepairUnsafeZombieConversions();
        }

        private static void RegisterOriginalZombieWandererJob()
        {
            // Original M2's ZombieWorse job is deliberately non-combat. Global
            // enemy checks still interrupt ordinary zombies, while the peaceful
            // ZombieBalloon simply follows its kind, crosses to another island,
            // wanders, and waits. Preserve the original task order verbatim.
            if (AssetManager.job_actor.get(ZombieWandererJobId) != null) return;
            AssetManager.job_actor.add(new ActorJob { id = ZombieWandererJobId });
            AssetManager.job_actor.t.addTask("follow_same_race");
            AssetManager.job_actor.t.addTask("swim_to_island");
            AssetManager.job_actor.t.addTask("random_move");
            AssetManager.job_actor.t.addTask("wait10");
        }

        private static void ConfigureOriginalZombieBalloonAsset()
        {
            ActorAsset balloon = AssetManager.actor_library.get("zombieballoon");
            if (balloon == null) return;

            string[] jobs = { ZombieWandererJobId };
            balloon.job = jobs;
            balloon.job_baby = jobs;
            balloon.job_citizen = jobs;
            balloon.job_kingdom = jobs;
            balloon.job_attacker = jobs;
            balloon.can_flip = false;
            balloon.actor_size = ActorSize.S17_Dragon;
            balloon.force_land_creature = true;
            balloon.can_turn_into_mush = false;
            balloon.can_turn_into_zombie = false;
            balloon.zombie_auto_asset = false;
            balloon.zombie_id_internal = string.Empty;

            // These are the exact seven traits assigned by original M2. In
            // particular, peaceful is intentional: the balloon is a roaming
            // spawner and never acquires or attacks combat targets.
            foreach (string traitId in new[]
            {
                "zombie", "immortal", "stupid", "peaceful",
                "acid_blood", "acid_touch", "zombie_spawner"
            })
                balloon.addTrait(traitId);
        }

        internal static void EnsureOriginalZombieBalloonRuntime(Actor actor)
        {
            if (actor == null || actor.asset == null || actor.asset.id != "zombieballoon") return;
            foreach (string traitId in new[]
            {
                "zombie", "immortal", "stupid", "peaceful",
                "acid_blood", "acid_touch", "zombie_spawner"
            })
                if (!actor.hasTrait(traitId)) actor.addTrait(traitId, true);

            // Migrate balloons saved by older rewrite builds while they were
            // using the generic attacker job. Peaceful prevents future target
            // searches; clearing the saved target/task stops the old chase now.
            if (actor.has_attack_target) actor.clearAttackTarget();
            if (actor.isTask("fighting")) actor.clearBeh();
        }

        private static void RegisterCocytusSummonSpell()
        {
            SpellAsset spell = AssetManager.spells.get(SpawnIceWalkerSpellId);
            if (spell == null)
            {
                spell = new SpellAsset
                {
                    id = SpawnIceWalkerSpellId,
                    chance = 3f,
                    min_distance = 0f,
                    cast_target = CastTarget.Himself,
                    cast_entity = CastEntity.UnitsOnly,
                    can_be_used_in_combat = true,
                    action = CastSpawnIceWalker
                };
                AssetManager.spells.add(spell);
            }

            ActorAsset cocytus = AssetManager.actor_library.get("Cocytuswalker");
            if (cocytus == null) return;
            if (cocytus.spell_ids == null || !cocytus.spell_ids.Contains(SpawnIceWalkerSpellId))
                cocytus.addSpell(SpawnIceWalkerSpellId);
            if (cocytus.spells == null) cocytus.spells = new SpellHolder();
            if (!cocytus.spells.hasSpell(spell)) cocytus.spells.addSpell(spell);
        }

        private static void RepairUnsafeZombieConversions()
        {
            // The base game's automatic zombie assets are generated before NML
            // content is registered. Custom actors cloned from an auto-zombie
            // template can therefore inherit a zombie ID whose actor asset was
            // never generated; vanilla death conversion then passes null into
            // ActorTool.copyUnitToOtherUnit. Preserve conversions with real
            // targets and disable only invalid inherited conversions.
            ActorAsset iceZombie = AssetManager.actor_library.get("zombieicelich");
            if (iceZombie != null)
            {
                foreach (string id in WalkerEvolutionActors)
                {
                    ActorAsset walker = AssetManager.actor_library.get(id);
                    if (walker != null) walker.setCanTurnIntoZombieAsset(iceZombie.id, false);
                }
            }

            ActorAsset cocytus = AssetManager.actor_library.get("Cocytuswalker");
            if (cocytus != null)
            {
                cocytus.can_turn_into_zombie = false;
                cocytus.zombie_auto_asset = false;
                cocytus.zombie_id_internal = string.Empty;
            }

            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                ActorAsset asset = AssetManager.actor_library.get(spec.Id);
                if (asset == null || !asset.can_turn_into_zombie) continue;
                string zombieId = asset.getZombieID();
                if (!string.IsNullOrEmpty(zombieId) && AssetManager.actor_library.get(zombieId) != null) continue;
                asset.can_turn_into_zombie = false;
                asset.zombie_auto_asset = false;
                asset.zombie_id_internal = string.Empty;
            }
        }

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.units == null || World.world.isPaused()) return;
            int key = World.world.GetHashCode();
            if (key != _worldKey)
            {
                _worldKey = key;
                _cursor = 0;
                _timer = 0f;
            }
            _timer += elapsed;
            if (_timer < 1f) return;
            _timer = 0f;

            List<Actor> actors = new List<Actor>();
            foreach (Actor actor in World.world.units)
                if (actor != null && actor.isAlive() && actor.asset != null) actors.Add(actor);
            if (actors.Count == 0) return;
            if (_cursor >= actors.Count) _cursor = 0;

            // Boss/spawner and walker evolution behavior must remain responsive
            // even in a world containing thousands of civilian actors. These are
            // small bounded subsets, so process them once per second before the
            // general round-robin slice.
            foreach (Actor actor in actors)
            {
                string id = actor.asset.id;
                if (IceSpawnerActors.Contains(id)) TryPlaceIceSpawner(actor);
                if (WalkerEvolutionActors.Contains(id)) TryPotentialEvolution(actor);
            }

            int amount = Mathf.Min(ActorsPerTick, actors.Count);
            for (int index = 0; index < amount; index++)
            {
                Actor actor = actors[_cursor++];
                if (_cursor >= actors.Count) _cursor = 0;
                try { ProcessActor(actor); }
                catch (Exception exception)
                {
                    ModernBoxDiagnostics.Error("M2 legacy behavior failed for " + actor.asset.id + ": " + exception);
                }
            }
        }

        private static void ProcessActor(Actor actor)
        {
            if (actor == null || !actor.isAlive() || actor.asset == null) return;
            string id = actor.asset.id;
            ActorsAndBuildingsRegistry.EnsureInvasionIdentity(actor);

            if (id.StartsWith("zombie", StringComparison.OrdinalIgnoreCase))
            {
                if (ZombieSpawnerActors.Contains(id))
                    TryPlaceZombieSpawner(actor);
            }

            if (AssimilatorSpawnerActors.Contains(id))
                TryPlaceActorSpawner(actor, "missilecybercore", ActorsAndBuildingsRegistry.AssimilatorKingdomId, false);
            if (IceSpawnerActors.Contains(id)) TryPlaceIceSpawner(actor);

            if (!WalkerEvolutionActors.Contains(id)) TryPotentialEvolution(actor);
            TryVeteranVehicleUpgrade(actor);
            TryOrcWarTurtle(actor);
        }

        private static bool IsBaseZombie(string id)
        {
            return id == "zombie" || id == "zombie_human" || id == "zombie_orc" || id == "zombie_elf" || id == "zombie_dwarf";
        }

        private static bool OnOriginalM2ZombieEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target as Actor;
            if (actor == null || !actor.isAlive() || actor.asset == null || actor.data == null) return false;

            // These visuals and era-body traits are also part of the original
            // M2zombieEffect callback, not rewrite-specific scheduling.
            actor.spawnParticle(Toolbox.color_infected);
            if (UnityEngine.Random.value < 0.25f)
                actor.startShake(0.2f, 0.05f, true, false);
            UpdateZombieEraTraits(actor);

            return IsBaseZombie(actor.asset.id) && TryEvolveBaseZombie(actor);
        }

        private static bool TryEvolveBaseZombie(Actor actor)
        {
            if (actor == null || actor.data == null) return false;

            string[] candidates = null;
            if (actor.hasTrait("veteran"))
            {
                candidates = GeneralZombieMutations;
            }
            else if (UnityEngine.Random.value < 0.01f)
            {
                candidates = GeneralZombieMutations;
            }
            else if (actor.hasTrait("fat"))
            {
                candidates = new[] { "zombieballoon", "zombiemother", "zombieabomination", "zombiehulk" };
            }
            else if (actor.hasTrait("giant"))
            {
                candidates = new[] { "zombiestalker", "zombiemother", "zombieabomination", "zombiehulk" };
            }
            else if (actor.hasTrait("strong"))
            {
                candidates = new[] { "zombieclawed", "zombietentacle", "zombieabomination", "zombiehulk" };
            }
            else if (actor.hasTrait("bloodlust"))
            {
                candidates = new[] { "zombieclawed", "zombietentacle" };
            }
            if (candidates == null || candidates.Length == 0) return false;

            string targetId = candidates[UnityEngine.Random.Range(0, candidates.Length)];
            return TransformActor(actor, targetId) != null;
        }

        private static void TryPlaceZombieSpawner(Actor actor)
        {
            if (actor == null || actor.data == null || actor.current_tile == null) return;
            string flag = SpawnerFlagPrefix + "pileofcorpses";
            if (actor.data.hasFlag(flag)) return;

            // activeCorpseSpawnerEffect in original M2 only worked on land, capped
            // active corpse piles to three per chunk, and used separate 40% and
            // 60% rolls. Both successful rolls targeted the same tile, so this is
            // equivalent to a 76% chance of placing one pile on a given callback.
            WorldTile tile = actor.current_tile;
            if (tile.Type == null || tile.Type.liquid || tile.chunk == null ||
                CountBuildingsInChunk(tile.chunk, "pileofcorpses") >= 3) return;
            if (UnityEngine.Random.value >= 0.76f) return;
            Building placed = PlaceWorldBuilding("pileofcorpses", tile, ActorsAndBuildingsRegistry.NativeUndeadKingdomId);
            if (placed != null) actor.data.addFlag(flag);
        }

        private static void TryPotentialEvolution(Actor actor)
        {
            string id = actor.asset.id;
            int kills = actor.data == null ? 0 : actor.data.kills;
            if (id == "newwalker")
            {
                // Preserve M2's three sequential rolls rather than flattening them
                // into one equal-choice roll: 30%, then 30% of the remainder,
                // then 30% of what remains.
                if (UnityEngine.Random.value < 0.3f)
                {
                    TransformActor(actor, "icedracoid");
                    return;
                }
                if (UnityEngine.Random.value < 0.3f)
                {
                    TransformActor(actor, "buffrost");
                    return;
                }
                if (UnityEngine.Random.value < 0.3f)
                {
                    TransformActor(actor, "normalwalker");
                    return;
                }
            }

            if (WalkerEvolutionActors.Contains(id) &&
                (World.world_era == null || !World.world_era.overlay_winter) && UnityEngine.Random.value < 0.1f)
            {
                TransformActorWithoutCopy(actor, "fly");
                return;
            }
            if (id == "assimilator" && kills > 5)
            {
                TransformActor(actor, RandomOf("assimilarptor", "assimilatrax", "helilator", "assizeppelin"));
                return;
            }
            if (id == "assimilarptor" && kills > 5)
            {
                TransformActor(actor, RandomOf("assimilatrax", "helilator", "assizeppelin"));
                return;
            }
            if (id == "basecrusader" && kills > 5)
                TransformActor(actor, RandomOf("crusaderdreadnaught", "crusaderHeli", "crusadermaus"));
        }

        private static void TryVeteranVehicleUpgrade(Actor actor)
        {
            if (!actor.hasTrait("veteran")) return;
            string target = null;
            switch (actor.asset.id)
            {
                case "Railgun": target = "OmegaRailgun"; break;
                case "baseMA9000": target = "MA9000"; break;
                case "biplane": target = "fighterww"; break;
                case "Zeppelin": target = "EliteZeppelin"; break;
                case "P9000": target = "EliteP9000"; break;
                case "AT9000": target = "eliteAT9000"; break;
                case "HumanTitan": target = "HumanTitanElite"; break;
                case "SpaceMarine":
                    if (actor.hasTrait("skin_burns") || actor.hasTrait("crippled")) target = "dreadnaught";
                    break;
            }
            if (!string.IsNullOrEmpty(target)) TransformActor(actor, target);
        }

        private static void TryOrcWarTurtle(Actor actor)
        {
            if (actor.hasTrait("thorns") || actor.city == null || ModernProgression.GetRace(actor.city) != "orc") return;
            ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == actor.asset.id);
            if (spec == null || !spec.Boat) return;
            if (UnityEngine.Random.value < 0.1f) TransformActor(actor, "orcwarturtle");
            else actor.addTrait("thorns", true);
        }

        private static void UpdateZombieEraTraits(Actor actor)
        {
            if (World.world_era == null) return;
            ToggleTrait(actor, "ScorchedZombie", World.world_era.overlay_sun);
            ToggleTrait(actor, "FrostedZombie", World.world_era.overlay_winter);
            ToggleTrait(actor, "ChaosZombie", World.world_era.overlay_chaos);
            ToggleTrait(actor, "NightInfusedZombie", World.world_era.overlay_night || World.world_era.overlay_moon);
        }

        private static void ToggleTrait(Actor actor, string traitId, bool enabled)
        {
            if (enabled)
            {
                if (!actor.hasTrait(traitId)) actor.addTrait(traitId, true);
            }
            else if (actor.hasTrait(traitId)) actor.removeTrait(traitId);
        }

        private static void TryPlaceActorSpawner(Actor actor, string buildingId, string factionId, bool winterOnly)
        {
            if (actor.data == null || (winterOnly && (World.world_era == null || !World.world_era.overlay_winter))) return;
            string flag = SpawnerFlagPrefix + buildingId;
            if (actor.data.hasFlag(flag)) return;
            Building placed = PlaceWorldBuilding(buildingId, actor.current_tile, factionId);
            if (placed != null) actor.data.addFlag(flag);
        }

        private static void TryPlaceIceSpawner(Actor actor)
        {
            if (actor == null || actor.data == null || actor.current_tile == null ||
                World.world_era == null || !World.world_era.overlay_winter) return;
            if (actor.data.hasFlag(WalkerTowerSpawnerFlag)) return;
            MapChunk chunk = actor.current_tile.chunk;
            if (chunk == null || CountBuildingsInChunk(chunk, "icewatchtower", "newicetower") >= 1) return;

            string tower = UnityEngine.Random.value < 0.05f ? "icewatchtower" : "newicetower";
            Building placed = PlaceWorldBuilding(tower, actor.current_tile, ActorsAndBuildingsRegistry.WalkerKingdomId);
            if (placed != null) actor.data.addFlag(WalkerTowerSpawnerFlag);
        }

        private static int CountBuildingsInChunk(MapChunk chunk, params string[] ids)
        {
            if (chunk == null || MapBox.instance == null || MapBox.instance.buildings == null) return 0;
            int count = 0;
            foreach (Building building in MapBox.instance.buildings)
            {
                if (building == null || !building.isAlive() || building.asset == null || building.current_tile == null ||
                    building.current_tile.chunk != chunk || Array.IndexOf(ids, building.asset.id) < 0) continue;
                count++;
            }
            return count;
        }

        private static bool CastSpawnIceWalker(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            WorldTile center = target == null ? (self == null ? tile : self.current_tile) : target.current_tile;
            if (center == null || center.chunk == null || center.region == null || World.world == null || World.world.units == null) return false;
            int nearby = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || !actor.isAlive() || actor.asset == null || actor.current_tile == null) continue;
                if (actor.asset.id == "newwalker" && actor.current_tile.chunk == center.chunk && ++nearby > 6) return false;
            }

            List<WorldTile> tiles = center.region.tiles;
            if (tiles == null || tiles.Count == 0) return false;
            WorldTile spawnTile = null;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                WorldTile candidate = tiles[UnityEngine.Random.Range(0, tiles.Count)];
                if (candidate != null && candidate.Type != null && !candidate.Type.liquid)
                {
                    spawnTile = candidate;
                    break;
                }
            }
            if (spawnTile == null) return false;
            Actor walker = World.world.units.spawnNewUnit("newwalker", spawnTile, true, true, 0f, null, false, true);
            if (walker == null) return false;
            ActorsAndBuildingsRegistry.MakeInvasionActor(walker, ActorsAndBuildingsRegistry.WalkerKingdomId);
            walker.makeWait(1f);
            return true;
        }

        private static Building PlaceWorldBuilding(string buildingId, WorldTile tile, string factionId)
        {
            if (tile == null || tile.Type == null || tile.Type.liquid || tile.building != null || MapBox.instance == null || MapBox.instance.buildings == null) return null;
            if (CountBuildings(buildingId) >= SpawnerWorldCapPerType) return null;
            Building building = MapBox.instance.buildings.addBuilding(buildingId, tile, false, true, BuildPlacingType.New);
            if (building == null) return null;
            Kingdom faction = ActorsAndBuildingsRegistry.GetInvasionKingdom(factionId);
            if (faction != null) building.setKingdom(faction);
            return building;
        }

        private static int CountBuildings(string id)
        {
            int count = 0;
            foreach (Building building in MapBox.instance.buildings)
            {
                if (building != null && building.isAlive() && building.asset != null && building.asset.id == id) count++;
            }
            return count;
        }

        private static Actor TransformActor(Actor original, string targetId)
        {
            if (original == null || !original.isAlive() || original.current_tile == null || AssetManager.actor_library.get(targetId) == null) return null;
            ModernUnitSpec targetSpec = ContentRegistry.Units.Find(candidate => candidate.Id == targetId);
            float height = targetSpec != null && targetSpec.Flying ? 2f : 0f;
            Actor replacement = World.world.units.spawnNewUnit(targetId, original.current_tile, true, true, height, null, false, true);
            if (replacement == null) return null;
            try
            {
                ActorTool.copyUnitToOtherUnit(original, replacement);
                if (original.kingdom != null) replacement.setKingdom(original.kingdom);
                if (original.city != null) replacement.setCity(original.city);
                if (original.home_building != null) replacement.setHomeBuilding(original.home_building);
                if (original.data != null) replacement.setProfession(original.data.profession, true);
                ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(replacement);
                if (targetSpec != null && targetSpec.Role != M2UnitRole.Creature && !replacement.hasTrait("spawnedvehicle"))
                    replacement.addTrait("spawnedvehicle", true);
                EffectsLibrary.spawn("fx_spawn", replacement.current_tile);
                ActionLibrary.removeUnit(original);
                return replacement;
            }
            catch (Exception exception)
            {
                ModernBoxDiagnostics.Error("M2 transformation " + original.asset.id + " -> " + targetId + " failed: " + exception);
                if (replacement.isAlive()) replacement.die(true, AttackType.Other, false, false);
                return null;
            }
        }

        private static Actor TransformActorWithoutCopy(Actor original, string targetId)
        {
            if (original == null || !original.isAlive() || original.current_tile == null || AssetManager.actor_library.get(targetId) == null) return null;
            Actor replacement = World.world.units.spawnNewUnit(targetId, original.current_tile, true, true, 0f, null, false, true);
            if (replacement == null) return null;
            EffectsLibrary.spawn("fx_spawn", replacement.current_tile);
            ActionLibrary.removeUnit(original);
            return replacement;
        }

        private static string RandomOf(params string[] ids)
        {
            return ids[UnityEngine.Random.Range(0, ids.Length)];
        }

        internal static bool OnUnitPotentialDeath(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target as Actor;
            if (actor == null || actor.asset == null) return false;
            ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == actor.asset.id);
            if (spec == null || string.IsNullOrEmpty(spec.ScrapBuilding)) return false;
            return PlaceWorldBuilding(spec.ScrapBuilding, tile ?? actor.current_tile, null) != null;
        }

        internal static bool OnWalkerTitanDeath(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target as Actor;
            return PlaceWorldBuilding("walkercorpse", tile ?? (actor == null ? null : actor.current_tile), ActorsAndBuildingsRegistry.WalkerKingdomId) != null;
        }

        internal static bool OnZombieSpawnerDeath(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target as Actor;
            // This is the original balloon/stalker crash behavior: the actor may
            // fly across water, but its corpse pile is a land building and is only
            // created when the death tile can actually hold it.
            return PlaceWorldBuilding("pileofcorpses", tile ?? (actor == null ? null : actor.current_tile), ActorsAndBuildingsRegistry.NativeUndeadKingdomId) != null;
        }

        internal static void RefreshCultureSprites(Culture culture)
        {
            if (culture == null || World.world == null || World.world.units == null) return;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || actor.city == null || actor.city.culture != culture) continue;
                actor.dirty_sprite_main = true;
                actor.dirty_sprite_head = true;
                actor.setStatsDirty();
            }
        }

        internal static bool TryGetEraTexture(Actor actor, out string path)
        {
            path = null;
            if (actor == null || actor.asset == null || actor.data == null || !ModernBoxCatalog.IsSupportedRace(actor.asset.id)) return false;
            Culture culture = actor.city == null ? null : actor.city.culture;
            if (culture == null && World.world != null && World.world.cultures != null)
                culture = World.world.cultures.get(actor.data.culture);
            M2Era era = ModernProgression.GetEra(culture);
            if (era < M2Era.Renaissance) return false;

            string folder = null;
            string race = actor.asset.id;
            switch (actor.data.profession)
            {
                case UnitProfession.Warrior:
                    folder = "Soldier_" + (era == M2Era.Future ? "future" : era == M2Era.Modern ? "modern" : era == M2Era.Industrial ? "industrial" : "medieval") + "_" + race;
                    break;
                case UnitProfession.Leader:
                    folder = "Leader_" + (era >= M2Era.Modern ? "modern" : "rain") + "_" + race;
                    break;
                case UnitProfession.King:
                    folder = "King_" + (era >= M2Era.Modern ? "modern" : "rain") + "_" + race;
                    break;
                case UnitProfession.Unit:
                    folder = "Unit_" + (era >= M2Era.Modern ? "modern" : "rain") + "_" + race;
                    break;
            }
            if (string.IsNullOrEmpty(folder)) return false;
            path = "actors/" + folder;
            return true;
        }

        internal static bool IsEraCivilizationTexturePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith("actors/Soldier_", StringComparison.Ordinal) ||
                   path.StartsWith("actors/Leader_", StringComparison.Ordinal) ||
                   path.StartsWith("actors/King_", StringComparison.Ordinal) ||
                   path.StartsWith("actors/Unit_", StringComparison.Ordinal);
        }
    }
}
