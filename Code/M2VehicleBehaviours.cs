using System;
using System.Collections.Generic;
using System.Linq;
using ai.behaviours;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// Per-vehicle details from original M2 (Vehicles/*.cs, Traits.cs, NewJobs.cs):
    /// asset traits, wild kingdoms, lifespans, the jet fly-around job, naval
    /// launch spells, Goliath death explosions, support auras and the nomad check.
    internal static class M2VehicleBehaviours
    {
        internal const string JetJobId = "jet_job";
        private const string JetTaskId = "jet";
        private static readonly string[] GoliathDeathExplosionActors = { "P9000", "baseMA9000", "MA9000", "AT9000", "eliteAT9000" };
        private static readonly string[] NegativeTraits = { "plague", "crippled", "mush_spores", "tumor_infection", "cursed", "skin_burns", "eyepatch", "infected" };
        private static readonly string[] NegativeStatuses = { "burning", "frozen", "poisoned", "slowness", "cough", "ash_fever" };
        private static readonly string[] BuffStatuses = { "shield", "caffeinated", "rage", "enchanted", "invincible" };

        private sealed class OriginalActor
        {
            internal readonly string[] Traits;
            internal readonly string Kingdom;
            internal readonly string SoundHit;
            internal readonly float Lifespan;
            internal readonly float Targets;
            internal readonly float Knockback;
            internal readonly float Projectiles;
            internal readonly bool Jet;

            internal OriginalActor(string[] traits, string kingdom, string soundHit, float lifespan, float targets, float knockback, float projectiles, bool jet)
            {
                Traits = traits;
                Kingdom = kingdom;
                SoundHit = soundHit;
                Lifespan = lifespan;
                Targets = targets;
                Knockback = knockback;
                Projectiles = projectiles;
                Jet = jet;
            }
        }

        private static readonly Dictionary<string, OriginalActor> OriginalActors = new Dictionary<string, OriginalActor>(StringComparer.Ordinal)
        {
            { "Heli", new OriginalActor(new[] { "Helicopter", "light_lamp", "Unitpotential", "fire_proof", "freeze_proof" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "HeliELite", new OriginalActor(new[] { "Helicopter", "light_lamp", "Unitpotential", "fire_proof", "freeze_proof" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "Gunship", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "balloonunit", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "bigfaerydragon", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "eliteGunship", new OriginalActor(new[] { "light_lamp", "Unitpotential", "fire_proof", "freeze_proof" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "Drone", new OriginalActor(new[] { "light_lamp", "Unitpotential", "fire_proof", "freeze_proof" }, "ModernKingdom", "", 5f, 0f, 0f, 0f, false) },
            { "TIEfighter", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "Zeppelin", new OriginalActor(new[] { "Zeppelin", "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "EliteZeppelin", new OriginalActor(new[] { "Zeppelin", "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "MIRVBomber", new OriginalActor(new[] { "Jet", "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "americanbomberww", new OriginalActor(new[] { "Jet", "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "EliteBomber", new OriginalActor(new[] { "Jet", "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "FighterJet", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "biplane", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "fighterww", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "F55FighterJet", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "Unitpotential" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, true) },
            { "FighterJet1", new OriginalActor(new[] { "fire_proof", "freeze_proof", "death_mark" }, "ModernKingdom", "", 1f, 0f, 0f, 0f, true) },
            { "F55FighterJet1", new OriginalActor(new[] { "light_lamp", "fire_proof", "freeze_proof", "death_mark" }, "ModernKingdom", "", 1f, 0f, 0f, 0f, true) },
            { "P9000", new OriginalActor(new[] { "Unitpotential", "P9000", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "baseMA9000", new OriginalActor(new[] { "Unitpotential", "baseMA9000", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "MA9000", new OriginalActor(new[] { "Unitpotential", "MA9000", "fire_proof", "light_lamp" }, "ModernKingdom", "", 200f, 0f, 0f, 0f, false) },
            { "AT9000", new OriginalActor(new[] { "Unitpotential", "AT9000", "fire_proof", "SupportRole", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 2f, false) },
            { "eliteAT9000", new OriginalActor(new[] { "Unitpotential", "eliteAT9000", "fire_proof", "SupportRole", "light_lamp" }, "ModernKingdom", "", 400f, 0f, 0f, 0f, false) },
            { "EliteP9000", new OriginalActor(new[] { "Unitpotential", "EliteP9000", "bubble_defense", "light_lamp", "fire_proof" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "Terran", new OriginalActor(new[] { "Unitpotential", "Terran", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "atst", new OriginalActor(new[] { "Unitpotential", "atst", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "supportatst", new OriginalActor(new[] { "Unitpotential", "SupportRole", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "artilleryatst", new OriginalActor(new[] { "Unitpotential", "artilleryatst", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "atstsniper", new OriginalActor(new[] { "Unitpotential", "atstsniper", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "dreadnaught", new OriginalActor(new[] { "Unitpotential", "dreadnaught", "fire_proof", "light_lamp" }, "ModernKingdom", "", 5000f, 0f, 0f, 2f, false) },
            { "HumanTitan", new OriginalActor(new[] { "Unitpotential", "HumanTitan", "fire_proof", "light_lamp" }, "ModernKingdom", "", 10000f, 0f, 0f, 1f, false) },
            { "HumanTitanElite", new OriginalActor(new[] { "Unitpotential", "HumanTitanElite", "bubble_defense", "fire_proof", "light_lamp" }, "ModernKingdom", "", 10000f, 0f, 0f, 1f, false) },
            { "Tank", new OriginalActor(new[] { "Unitpotential", "Tank", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "AbramTank", new OriginalActor(new[] { "Unitpotential", "Tank", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "tankie", new OriginalActor(new[] { "Unitpotential", "tankie", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "wwartillery", new OriginalActor(new[] { "Unitpotential", "wwartillery", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "genericwwtank", new OriginalActor(new[] { "Unitpotential", "genericwwtank", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "landship", new OriginalActor(new[] { "Unitpotential", "landship", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "bigtankww", new OriginalActor(new[] { "Unitpotential", "bigtankww", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "davincitank", new OriginalActor(new[] { "Unitpotential", "davincitank", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "MissileSystem", new OriginalActor(new[] { "Unitpotential", "light_lamp", "fire_proof" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "shermanww", new OriginalActor(new[] { "Unitpotential", "light_lamp", "fire_proof" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 5f, false) },
            { "Railgun", new OriginalActor(new[] { "Unitpotential", "Railgun", "light_lamp", "fire_proof" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "OmegaRailgun", new OriginalActor(new[] { "Unitpotential", "Railgun", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "Humvee", new OriginalActor(new[] { "Humvee", "Unitpotential", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "wwsupporttruck", new OriginalActor(new[] { "wwsupporttruck", "Unitpotential", "SupportRole", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "modernsupporttruck", new OriginalActor(new[] { "modernsupporttruck", "Unitpotential", "SupportRole", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "modernhumvee", new OriginalActor(new[] { "modernhumvee", "Unitpotential", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "teslatruckgun", new OriginalActor(new[] { "teslatruckgun", "Unitpotential", "fire_proof", "light_lamp" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "wheeledtank", new OriginalActor(new[] { "Humvee", "Unitpotential", "light_lamp", "fire_proof" }, "ModernKingdom", "", 50f, 0f, 0f, 0f, false) },
            { "catapulta", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "orcatapulta", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "santaguin", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "woolyrhino", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 6f, 3f, 0f, false) },
            { "batteringram", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 6f, 3f, 0f, false) },
            { "humancavalry", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 50f, 2f, 0f, 0f, false) },
            { "ogreunit", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 50f, 4f, 0f, 0f, false) },
            { "golemgem", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 50f, 4f, 0f, 0f, false) },
            { "treant", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 50f, 1f, 0f, 0f, false) },
            { "armoredwolf", new OriginalActor(new[] { "Unitpotential", "nightchild", "savage", "tough", "flesh_eater" }, "ModernKingdom", "", 50f, 0f, 0.8f, 0f, false) },
            { "humanpaladin", new OriginalActor(new[] { "Unitpotential", "bubble_defense", "SupportRole" }, "ModernKingdom", "", 50f, 1f, 0f, 0f, false) },
            { "orcwarlock", new OriginalActor(new[] { "Unitpotential", "bubble_defense", "SupportRole" }, "ModernKingdom", "", 50f, 1f, 0f, 0f, false) },
            { "dwarfdoctor", new OriginalActor(new[] { "Unitpotential", "bubble_defense", "SupportRole" }, "ModernKingdom", "", 50f, 1f, 0f, 0f, false) },
            { "fairydragon", new OriginalActor(new[] { "Unitpotential", "bubble_defense", "SupportRole" }, "ModernKingdom", "", 50f, 1f, 0f, 0f, false) },
            { "fairelf", new OriginalActor(new[] { "Unitpotential", "bubble_defense", "SupportRole" }, "ModernKingdom", "", 50f, 1f, 0f, 0f, false) },
            { "humancannon", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "orccannon", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "elfcannon", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "dwarfcannon", new OriginalActor(new[] { "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "", 50f, 0f, 0f, 0f, false) },
            { "Soldier", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 0f, 0f, 0f, 0f, false) },
            { "EVA01", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 0f, 0f, 0f, 0f, false) },
            { "SpaceMarine", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 500f, 0f, 0f, 0f, false) },
            { "spaceork", new OriginalActor(new[] { "Unitpotential" }, "ModernKingdom", "", 200f, 0f, 0f, 0f, false) },
            { "Xiexel", new OriginalActor(new string[0], "ModernKingdom", "", 0f, 0f, 0f, 0f, false) },
            { "orcwarturtle", new OriginalActor(new[] { "light_lamp", "nightchild", "savage", "regeneration", "slow", "weightless", "genius", "Unitpotential" }, "", "", 25f, 0f, 0.8f, 0f, false) },
            { "human_renaissance_battleship", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 5f, false) },
            { "human_renaissance_trading", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 5f, false) },
            { "fishing_boat_renaissance", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 5f, false) },
            { "human_renaissance_corvette1", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 3f, false) },
            { "human_renaissance_corvette2", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 3f, false) },
            { "human_industrial_battleship", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 1f, false) },
            { "human_industrial_trading", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 2f, false) },
            { "fishing_boat_industrial", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 5f, false) },
            { "human_industrial_corvette1", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 1f, false) },
            { "human_industrial_corvette2", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 1f, false) },
            { "human_modern_battleship", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 2f, false) },
            { "human_modern_trading", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 2f, false) },
            { "fishing_boat_modern", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitWood", 25f, 0f, 0.8f, 5f, false) },
            { "human_modern_gunboat", new OriginalActor(new[] { "Unitpotential", "death_mark" }, "MissileLauncherFULLRANGETARGETTING", "", 4f, 0f, 0f, 0f, false) },
            { "human_modern_corvette1", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 1f, false) },
            { "human_modern_corvette2", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "MissileLauncherFULLRANGETARGETTING", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 1f, false) },
            { "human_modern_submarine", new OriginalActor(new[] { "light_lamp", "Unitpotential" }, "", "event:/SFX/HIT/HitMetal", 25f, 0f, 0.8f, 1f, false) },
        };

        internal static void RegisterAll()
        {
            RegisterJetJob();
            RegisterNavalSpells();
            RegisterExplosionEffect();
        }

        /// Applies the original per-actor fields and returns the asset traits to add.
        internal static List<string> ConfigureOriginalActor(ActorAsset actor, ModernUnitSpec spec)
        {
            List<string> traits = new List<string>();
            OriginalActor original;
            if (!OriginalActors.TryGetValue(spec.Id, out original))
            {
                foreach (string trait in spec.Traits)
                    if (trait != "spawnedvehicle" && trait != "MIRVBoat") traits.Add(trait);
                return traits;
            }

            actor.kingdom_id_wild = string.IsNullOrEmpty(original.Kingdom) ? ActorsAndBuildingsRegistry.ModernKingdomId : original.Kingdom;
            if (original.Lifespan > 0f) actor.base_stats["lifespan"] = original.Lifespan;
            if (original.Targets > 0f) actor.base_stats["targets"] = original.Targets;
            if (original.Knockback > 0f) actor.base_stats["knockback"] = original.Knockback;
            if (original.Projectiles > 0f) actor.base_stats["projectiles"] = original.Projectiles;
            if (!string.IsNullOrEmpty(original.SoundHit)) actor.sound_hit = original.SoundHit;
            ApplyOriginalSounds(actor, spec);
            if (original.Jet) SetJob(actor, JetJobId);
            if (Array.IndexOf(GoliathDeathExplosionActors, spec.Id) >= 0)
                actor.action_death = (WorldAction)Delegate.Combine(actor.action_death, new WorldAction(StartDeathExplosionBig));
            if (spec.Id == "human_modern_gunboat")
            {
                actor.force_land_creature = false;
                actor.force_ocean_creature = true;
            }
            foreach (string trait in original.Traits)
                if (AssetManager.traits.get(trait) != null) traits.Add(trait);
            return traits;
        }

        private static void ApplyOriginalSounds(ActorAsset actor, ModernUnitSpec spec)
        {
            // Original _mob vehicles were silent apart from these copies of
            // vanilla creature sounds. Native boats keep their boat sounds.
            string source = spec.Id == "armoredwolf" ? "wolf" : spec.Id == "orcwarturtle" ? "turtle" : null;
            ActorAsset sound = source == null ? null : AssetManager.actor_library.get(source);
            if (sound != null)
            {
                actor.sound_spawn = sound.sound_spawn;
                actor.sound_attack = sound.sound_attack;
                actor.sound_idle = sound.sound_idle;
                actor.sound_death = sound.sound_death;
                return;
            }
            if (spec.Boat) return;
            actor.sound_spawn = null;
            actor.sound_attack = null;
            actor.sound_idle = null;
            actor.sound_idle_loop = null;
            actor.sound_death = null;
        }

        private static void SetJob(ActorAsset actor, string jobId)
        {
            string[] jobs = { jobId };
            actor.job = jobs;
            actor.job_baby = jobs;
            actor.job_citizen = jobs;
            actor.job_kingdom = jobs;
            actor.job_attacker = jobs;
        }

        private static void RegisterJetJob()
        {
            if (AssetManager.tasks_actor.get(JetTaskId) == null)
            {
                // NewJobs.loadBeh: fly a random 8-direction path while fighting.
                BehaviourTaskActor task = new BehaviourTaskActor { id = JetTaskId, in_combat = true, locale_key = "task_unit_fight" };
                AssetManager.tasks_actor.add(task);
                task.addBeh(new BehFindRandomTile8Directions());
                task.addBeh(new BehGoToTileTarget());
                task.addBeh(new BehFightCheckEnemyIsOk());
                task.addBeh(new BehFindRandomTile8Directions());
                task.addBeh(new BehGoToTileTarget());
                task.addBeh(new BehFindRandomTile8Directions());
                task.addBeh(new BehGoToTileTarget());
                task.addBeh(new BehRestartTask());
            }
            if (AssetManager.job_actor.get(JetJobId) == null)
            {
                ActorJob job = new ActorJob { id = JetJobId };
                AssetManager.job_actor.add(job);
                job.addTask(JetTaskId);
            }
        }

        private static void RegisterNavalSpells()
        {
            AddSpell("jet22", 0.1f, CastJet22);
            AddSpell("jet55", 0.2f, CastJet55);
            AddSpell("gunboat", 1f, CastGunboat);
            SetSpells("human_modern_battleship", "jet22", "jet22", "jet22", "jet22", "jet55", "jet22", "jet22", "jet22", "jet22", "jet55", "jet55", "jet55", "jet55");
            SetSpells("human_modern_corvette1", "gunboat", "gunboat", "gunboat", "gunboat", "gunboat");
            SetSpells("human_modern_corvette2", "gunboat", "gunboat", "gunboat", "gunboat", "gunboat");
        }

        private static void AddSpell(string id, float chance, AttackAction action)
        {
            if (AssetManager.spells.get(id) != null) return;
            AssetManager.spells.add(new SpellAsset
            {
                id = id,
                chance = chance,
                min_distance = 0f,
                cast_target = CastTarget.Himself,
                cast_entity = CastEntity.UnitsOnly,
                can_be_used_in_combat = true,
                action = action
            });
        }

        private static void SetSpells(string actorId, params string[] spells)
        {
            ActorAsset actor = AssetManager.actor_library.get(actorId);
            if (actor == null) return;
            actor.spell_ids = new List<string>(spells);
            // Actor spells are linked during library init, before NML content.
            actor.spells = new SpellHolder();
            actor.spells.mergeWith(actor.spell_ids);
        }

        private static bool CastJet22(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            return LaunchFromShip(self, target, tile, "FighterJet1");
        }

        private static bool CastJet55(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            return LaunchFromShip(self, target, tile, "F55FighterJet1");
        }

        private static bool CastGunboat(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            return LaunchFromShip(self, target, tile, "human_modern_gunboat");
        }

        private static bool LaunchFromShip(BaseSimObject self, BaseSimObject target, WorldTile tile, string unitId)
        {
            BaseSimObject source = target ?? self;
            WorldTile center = source == null ? tile : source.current_tile;
            Actor ship = source == null ? null : source.a;
            if (center == null || center.chunk == null || center.region == null || ship == null) return false;
            if (CountUnitsNear(center, unitId) > 6) return false;
            List<WorldTile> tiles = center.region.tiles;
            if (tiles == null || tiles.Count == 0) return false;
            WorldTile spawnTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
            Actor launched = World.world.units.createNewUnit(unitId, spawnTile);
            if (launched == null) return false;
            if (ship.kingdom != null) launched.setKingdom(ship.kingdom);
            if (ship.city != null) launched.joinCity(ship.city);
            ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(launched);
            launched.makeWait(1f);
            return true;
        }

        private static int CountUnitsNear(WorldTile center, string unitId)
        {
            int count = 0;
            foreach (Actor actor in Finder.getUnitsFromChunk(center, 1))
                if (actor.asset != null && actor.asset.id == unitId) count++;
            return count;
        }

        private static void RegisterExplosionEffect()
        {
            if (AssetManager.effects_library.get("explosion") != null) return;
            AssetManager.effects_library.add(new EffectAsset
            {
                id = "explosion",
                use_basic_prefab = true,
                sorting_layer_id = "EffectsTop",
                sprite_path = "effects/explosion",
                draw_light_area = true,
                limit = 80
            });
        }

        private static bool StartDeathExplosionBig(BaseSimObject target, WorldTile tile)
        {
            WorldTile at = tile ?? (target == null ? null : target.current_tile);
            if (at != null) EffectsLibrary.spawn("explosion", at);
            return true;
        }

        internal static bool FriendlyAuraEffect(BaseSimObject target, WorldTile tile)
        {
            Actor owner = target == null ? null : target.a;
            WorldTile center = tile ?? (owner == null ? null : owner.current_tile);
            if (owner == null || center == null || center.chunk == null || !Randy.randomChance(0.4f)) return true;
            foreach (Actor actor in Finder.getUnitsFromChunk(center, 1, 4f).ToList())
            {
                if (actor == owner || actor.kingdom != owner.kingdom || actor.getHealth() >= actor.getMaxHealth()) continue;
                actor.restoreHealth(40);
                actor.spawnParticle(Toolbox.color_heal);
                foreach (string trait in NegativeTraits)
                    if (actor.hasTrait(trait)) actor.removeTrait(trait);
                foreach (string status in NegativeStatuses)
                    actor.finishStatusEffect(status);
                if (Randy.randomChance(0.5f))
                    actor.addStatusEffect(BuffStatuses[UnityEngine.Random.Range(0, BuffStatuses.Length)]);
            }
            return true;
        }

        // A city-built vehicle that lost its nation but still has a living city or home base goes back to it
        // instead of being treated as an orphan.
        private static void TryRehome(Actor actor)
        {
            if (actor.kingdom != null && actor.kingdom.isCiv()) return;
            City city = actor.city;
            if ((city == null || city.isRekt()) && actor.home_building != null && actor.home_building.isAlive())
                city = actor.home_building.city;
            if (city == null || city.isRekt() || city.kingdom == null || !city.kingdom.isCiv()) return;
            if (actor.city != city) actor.joinCity(city);
            if (actor.kingdom != city.kingdom) actor.joinKingdom(city.kingdom);
        }

        /// Traits.NomadHandlerEffect: units die once their owner is a nomad or
        /// hidden M2 kingdom, has no cities left, or once they go mad.
        internal static bool NomadHandlerEffect(BaseSimObject target, WorldTile tile)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || !actor.isAlive() || actor.asset == null || actor.getAge() < 2) return false;
            if (!ModernBoxCatalog.UnitIds.Contains(actor.asset.id)) return false;
            TryRehome(actor);
            Kingdom kingdom = actor.kingdom;
            string kingdomId = kingdom == null || kingdom.asset == null ? null : kingdom.asset.id;
            bool orphaned = (kingdom != null && kingdom.asset != null && kingdom.asset.nomads) ||
                            kingdomId == ActorsAndBuildingsRegistry.ModernKingdomId ||
                            kingdomId == ActorsAndBuildingsRegistry.MissileLauncherKingdomId ||
                            (kingdom != null && kingdom.cities.Count == 0);
            // A unit still serving a living city of its own nation is never an orphan.
            if (orphaned && actor.city != null && !actor.city.isRekt() && actor.city.kingdom == kingdom &&
                kingdom != null && kingdom.isCiv()) orphaned = false;
            bool mad = actor.hasTrait("madness");
            if (!orphaned && !mad) return true;
            if (mad) actor.removeTrait("madness");
            if (orphaned && actor.asset.id == "armoredwolf")
            {
                Actor wolf = World.world.units.createNewUnit("wolf", actor.current_tile);
                if (wolf != null) EffectsLibrary.spawn("fx_spawn", wolf.current_tile);
                ActionLibrary.removeUnit(actor);
                return true;
            }
            actor.getHit(10000000000f, true, AttackType.Other);
            return true;
        }
    }
}
