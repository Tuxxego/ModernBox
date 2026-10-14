using System;
using System.Collections.Generic;
using System.Linq;
using ai;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Zombie evolution (attached to the vanilla zombie trait like M2zombieEffect),
    /// veteran vehicle upgrades and era appearance. Creature evolution and the
    /// spawner traits live in M2Creatures and run from the traits themselves.
    /// </summary>
    internal static class M2LegacyBehaviorService
    {
        private const int ActorsPerTick = 48;
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
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                ActorAsset asset = AssetManager.actor_library.get(spec.Id);
                if (asset == null) continue;
                if (spec.Role != M2UnitRole.Creature) asset.addTrait("Unitpotential");
            }

            ConfigureOriginalZombieBalloonAsset();

            ActorTrait zombieTrait = AssetManager.traits.get("zombie");
            if (!_zombieTraitCallbackAttached && zombieTrait != null)
            {
                zombieTrait.action_special_effect = (WorldAction)Delegate.Combine(
                    zombieTrait.action_special_effect,
                    new WorldAction(OnOriginalM2ZombieEffect));
                _zombieTraitCallbackAttached = true;
            }

            RepairUnsafeZombieConversions();
        }

        private static void ConfigureOriginalZombieBalloonAsset()
        {
            ActorAsset balloon = AssetManager.actor_library.get("zombieballoon");
            if (balloon == null) return;

            string[] jobs = { M2Creatures.ZombieWandererJobId };
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

            if (actor.has_attack_target) actor.clearAttackTarget();
            if (actor.isTask("fighting")) actor.clearBeh();
        }

        private static void RepairUnsafeZombieConversions()
        {
            // Auto zombie assets are generated before mods load; custom clones of an
            // auto-zombie template can point at an ID that was never created.
            ActorAsset iceZombie = AssetManager.actor_library.get("zombieicelich");
            if (iceZombie != null)
            {
                foreach (string id in new[] { "newwalker", "normalwalker", "icedracoid", "buffrost" })
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

            // Walk the game's own alive list; copying every unit each tick is too slow on big worlds.
            List<Actor> actors = World.world.units.units_only_alive;
            if (actors == null || actors.Count == 0) return;
            if (_cursor >= actors.Count) _cursor = 0;

            int amount = Mathf.Min(ActorsPerTick, actors.Count);
            for (int index = 0; index < amount; index++)
            {
                Actor actor = actors[_cursor++];
                if (_cursor >= actors.Count) _cursor = 0;
                if (actor == null || !actor.isAlive() || actor.asset == null) continue;
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
            // Madness (SolarPoweredCyberBody in chaos ages) moves the actor to the mad kingdom.
            if (!actor.hasTrait("madness")) ActorsAndBuildingsRegistry.EnsureInvasionIdentity(actor);
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

            actor.spawnParticle(Toolbox.color_infected);
            if (M2Creatures.Chance(0.25f))
                actor.startShake(0.2f, 0.05f, true, false);
            UpdateZombieEraTraits(actor);

            return IsBaseZombie(actor.asset.id) && TryEvolveBaseZombie(actor);
        }

        private static bool TryEvolveBaseZombie(Actor actor)
        {
            if (actor == null || actor.data == null) return false;

            string[] candidates = null;
            if (actor.hasTrait("veteran"))
                candidates = GeneralZombieMutations;
            else if (M2Creatures.Chance(0.01f))
                candidates = GeneralZombieMutations;
            else if (actor.hasTrait("fat"))
                candidates = new[] { "zombieballoon", "zombiemother", "zombieabomination", "zombiehulk" };
            else if (actor.hasTrait("giant"))
                candidates = new[] { "zombiestalker", "zombiemother", "zombieabomination", "zombiehulk" };
            else if (actor.hasTrait("strong"))
                candidates = new[] { "zombieclawed", "zombietentacle", "zombieabomination", "zombiehulk" };
            else if (actor.hasTrait("bloodlust"))
                candidates = new[] { "zombieclawed", "zombietentacle" };
            if (candidates == null || candidates.Length == 0) return false;

            TransformActor(actor, candidates[UnityEngine.Random.Range(0, candidates.Length)]);
            return true;
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
            ModernUnitSpec spec = ContentRegistry.FindUnit(actor.asset.id);
            if (spec == null || !spec.Boat) return;
            // HandleOrcBoatTransformations: the boat stays and a war turtle joins it.
            if (UnityEngine.Random.value < 0.1f)
            {
                if (AssetManager.actor_library.get("orcwarturtle") == null || actor.current_tile == null) return;
                Actor turtle = World.world.units.createNewUnit("orcwarturtle", actor.current_tile);
                if (turtle == null) return;
                if (actor.kingdom != null) turtle.setKingdom(actor.kingdom);
                if (actor.city != null) turtle.setCity(actor.city);
                ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(turtle);
                EffectsLibrary.spawn("fx_spawn", turtle.current_tile);
            }
            else actor.addTrait("thorns", true);
        }

        // M2zombieEffect toggled these on every zombie-trait tick.
        private static void UpdateZombieEraTraits(Actor actor)
        {
            if (World.world_era == null) return;
            ToggleTrait(actor, "ScorchedZombie", World.world_era.overlay_sun);
            ToggleTrait(actor, "FrostedZombie", World.world_era.overlay_winter);
            ToggleTrait(actor, "ChaosZombie", World.world_era.overlay_chaos);
            ToggleTrait(actor, "NightInfusedZombie", World.world_era.overlay_night || World.world_era.overlay_moon);
        }

        internal static void ToggleTrait(Actor actor, string traitId, bool enabled)
        {
            if (enabled)
            {
                if (!actor.hasTrait(traitId)) actor.addTrait(traitId, true);
            }
            else if (actor.hasTrait(traitId)) actor.removeTrait(traitId);
        }

        internal static Actor TransformActor(Actor original, string targetId)
        {
            if (original == null || !original.isAlive() || original.current_tile == null || original.asset.id == targetId ||
                AssetManager.actor_library.get(targetId) == null) return null;
            ModernUnitSpec targetSpec = ContentRegistry.FindUnit(targetId);
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

        // TransformActorToUFO in the original: a fresh unit, nothing copied.
        internal static Actor TransformActorWithoutCopy(Actor original, string targetId)
        {
            if (original == null || !original.isAlive() || original.current_tile == null || original.asset.id == targetId ||
                AssetManager.actor_library.get(targetId) == null) return null;
            Actor replacement = World.world.units.createNewUnit(targetId, original.current_tile);
            if (replacement == null) return null;
            EffectsLibrary.spawn("fx_spawn", replacement.current_tile);
            ActionLibrary.removeUnit(original);
            return replacement;
        }

        internal static bool OnUnitPotentialDeath(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target as Actor;
            if (actor == null || actor.asset == null) return false;
            ModernUnitSpec spec = ContentRegistry.FindUnit(actor.asset.id);
            if (spec == null || string.IsNullOrEmpty(spec.ScrapBuilding)) return false;
            return M2Creatures.PlaceBuilding(spec.ScrapBuilding, tile ?? actor.current_tile, true) != null;
        }

        internal static bool OnWalkerTitanDeath(BaseSimObject target, WorldTile tile = null)
        {
            return M2Creatures.WalkerCorpseEffect(target, tile);
        }

        internal static bool OnZombieSpawnerDeath(BaseSimObject target, WorldTile tile = null)
        {
            return M2Creatures.PileOfCorpsesEffect(target, tile);
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

        // Runs every time a visible unit changes animation frame, so keep it allocation free.
        private static readonly Dictionary<string, string[]> EraTexturePaths = new Dictionary<string, string[]>(StringComparer.Ordinal);

        internal static bool TryGetEraTexture(Actor actor, out string path)
        {
            path = null;
            if (actor == null || actor.asset == null || actor.data == null || !ModernBoxCatalog.IsSupportedRace(actor.asset.id)) return false;
            // The original read the actor's own culture (data.culture).
            Culture culture = actor.culture ?? (actor.city == null ? null : actor.city.culture);
            if (!M2Tech.IsResearching(culture)) return false;
            M2CultureTechs techs = M2Tech.Get(culture);
            if (techs == null) return false;

            // Commerce.cs texture patch: soldiers follow Future/MilitaryModern/Industrial/Renaissance,
            // leaders, kings and citizens follow MilitaryModern/Renaissance.
            bool modern = techs.Set.Contains("MilitaryModern");
            bool renaissance = techs.Set.Contains("Renaissance");
            int slot;
            switch (actor.data.profession)
            {
                case UnitProfession.Warrior:
                    if (techs.Set.Contains("Future")) slot = 3;
                    else if (modern) slot = 2;
                    else if (techs.Set.Contains("Industrial")) slot = 1;
                    else if (renaissance) slot = 0;
                    else return false;
                    break;
                case UnitProfession.Leader: if (!modern && !renaissance) return false; slot = modern ? 5 : 4; break;
                case UnitProfession.King: if (!modern && !renaissance) return false; slot = modern ? 7 : 6; break;
                case UnitProfession.Unit: if (!modern && !renaissance) return false; slot = modern ? 9 : 8; break;
                default: return false;
            }
            string race = actor.asset.id;
            string[] paths;
            if (!EraTexturePaths.TryGetValue(race, out paths))
            {
                paths = new[]
                {
                    "actors/Soldier_medieval_" + race, "actors/Soldier_industrial_" + race,
                    "actors/Soldier_modern_" + race, "actors/Soldier_future_" + race,
                    "actors/Leader_rain_" + race, "actors/Leader_modern_" + race,
                    "actors/King_rain_" + race, "actors/King_modern_" + race,
                    "actors/Unit_rain_" + race, "actors/Unit_modern_" + race
                };
                EraTexturePaths[race] = paths;
            }
            path = paths[slot];
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
