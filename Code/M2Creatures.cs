using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Creature content from M2's Creatures.cs: per-actor setup, the trait-driven
    /// evolution and spawner effects, zombie conversions, boss spells, jobs and
    /// creature kingdoms. Runs after units and buildings are registered.
    /// </summary>
    internal static class M2Creatures
    {
        internal const string ZombieWandererJobId = "ZombieWorse";
        internal const string DuneCritterJobId = "dune_critter";
        internal const string VaticanForceJobId = "VaticanForce";
        internal const string TerlaniusKingdomId = "TerlaniusKingdom";
        internal const string DuneMonsterKingdomId = "dunemonster";
        private const string ExhaustedSpawnerFlag = "exhaustedSpawner";
        private static readonly string[] LegacySpawnerFlags =
        {
            "modernbox_m2_spawner_pileofcorpses", "modernbox_m2_spawner_missilecybercore", "modernbox_m2_spawner_walker_tower"
        };
        // CheckAndTransformByEraAndMob; "walker" is cold_one in 0.51.2.
        private static readonly HashSet<string> WalkerRelatedMobs = new HashSet<string>(StringComparer.Ordinal)
        {
            "cold_one", "icedracoid", "buffrost", "normalwalker", "newwalker"
        };
        private static readonly Dictionary<string, string> NaturalFactions = new Dictionary<string, string>(StringComparer.Ordinal);

        internal static bool Chance(float chance)
        {
            return chance >= UnityEngine.Random.value;
        }

        /// <summary>Wild kingdom for creatures that keep their own (non-invasion) faction.</summary>
        internal static string WildFaction(string actorId)
        {
            string faction;
            return actorId != null && NaturalFactions.TryGetValue(actorId, out faction) ? faction : null;
        }

        internal static void Register()
        {
            RegisterJobs();
            RegisterKingdoms();
            RegisterSpells();
            ConfigureTraits();
            ConfigureCreatures();
            ConfigureZombieConversions();
            ConfigureSpawnerBuildings();
            InvasionService.RegisterDisasters();
            M2Biomes.Register();
        }

        #region jobs, kingdoms, spells

        private static void RegisterJobs()
        {
            // follow_same_race is rebuilt from the old engine; check_hunger_animal and
            // water_feeding no longer exist in 0.51.2 and are skipped.
            M2FollowSameRace.Register(
                new[]
                {
                    "zombie", "zombie_human", "zombiespeed", "zombiespikes", "zombiepoison", "zombieacid", "zombietentacle",
                    "zombiestalker", "zombiemother", "zombiefiremaniac", "zombiehulk", "zombieabomination", "zombieclawed",
                    "zombieballoon", "zombieacidman", "zombiedemon", "zombiedoctor", "zombiedruid", "zombieevilhorseman",
                    "zombieicelich", "zombiefairy", "zombietarantula"
                },
                new[] { "basecrusader", "crusaderdreadnaught", "crusaderHeli", "crusadermaus" },
                new[] { "Duneworm" });
            AddJob(ZombieWandererJobId, "follow_same_race", "swim_to_island", "random_move", "wait10");
            AddJob(DuneCritterJobId, "random_swim", "crab_danger_check", "follow_same_race", "swim_to_island",
                "crab_danger_check", "random_move", "check_hunger_animal", "water_feeding", "crab_danger_check", "wait10");
            AddJob(VaticanForceJobId, "follow_same_race", "swim_to_island", "random_move", "check_cure", "burn_tumors",
                "random_move_towards_civ_building", "check_heal", "wait");
        }

        private static void AddJob(string id, params string[] tasks)
        {
            if (AssetManager.job_actor.get(id) != null) return;
            AssetManager.job_actor.add(new ActorJob { id = id });
            foreach (string task in tasks)
                if (AssetManager.tasks_actor.has(task)) AssetManager.job_actor.t.addTask(task);
        }

        private static void SetJob(ActorAsset asset, string jobId)
        {
            string[] jobs = { jobId };
            asset.job = jobs;
            asset.job_baby = jobs;
            asset.job_citizen = jobs;
            asset.job_kingdom = jobs;
            asset.job_attacker = jobs;
        }

        private static void RegisterKingdoms()
        {
            KingdomAsset terlanius = MobKingdom(TerlaniusKingdomId);
            terlanius.addTag("Terlanius");
            terlanius.addFriendlyTag("Terlanius");
            terlanius.addFriendlyTag("NoneFriendlyTag");
            // The original passed these as literal strings, so they never matched a tag.
            terlanius.addEnemyTag("SK.neutral");
            terlanius.addEnemyTag("SK.good");

            KingdomAsset dune = MobKingdom(DuneMonsterKingdomId);
            dune.count_as_danger = true;
            dune.always_attack_each_other = false;
            dune.addTag(DuneMonsterKingdomId);
            dune.addFriendlyTag(DuneMonsterKingdomId);
            dune.addEnemyTag("civ");
            ColorAsset duneColor = ColorAsset.tryMakeNewColorAsset("#BACADD");
            if (duneColor != null)
            {
                duneColor.id = "kingdom_library_color_" + DuneMonsterKingdomId;
                dune.default_kingdom_color = duneColor;
            }

            // In M2 the evolved assimilators and walkers shared the vanilla
            // "assimilators"/"walkers" kingdoms. The port keeps separate wild
            // kingdoms, so make them allies of their vanilla counterparts.
            Befriend(ActorsAndBuildingsRegistry.AssimilatorKingdomId, "assimilators", "assimilators", "aliens");
            Befriend(ActorsAndBuildingsRegistry.WalkerKingdomId, "cold_one", "snow");
            foreach (KingdomAsset kingdom in AssetManager.kingdoms.list) kingdom._cached_enemies.Clear();

            EnsureWildKingdom(TerlaniusKingdomId);
            EnsureWildKingdom(DuneMonsterKingdomId);
        }

        private static KingdomAsset MobKingdom(string id)
        {
            KingdomAsset kingdom = AssetManager.kingdoms.get(id);
            if (kingdom == null) kingdom = AssetManager.kingdoms.clone(id, "$TEMPLATE_MOB$");
            kingdom.id = id;
            kingdom.civ = false;
            kingdom.nomads = false;
            kingdom.neutral = false;
            kingdom.nature = false;
            kingdom.mobs = true;
            kingdom.friendly_tags.Clear();
            kingdom.enemy_tags.Clear();
            kingdom.list_tags.Clear();
            kingdom._cached_enemies.Clear();
            // Tags every kingdom receives in KingdomLibrary.post_init.
            kingdom.addTag("everyone");
            foreach (KingdomAsset other in AssetManager.kingdoms.list)
                if (other.friendship_for_everyone && !other.brain) kingdom.addFriendlyTag(other.id);
            return kingdom;
        }

        private static void Befriend(string m2KingdomId, string vanillaKingdomId, params string[] tags)
        {
            KingdomAsset m2 = AssetManager.kingdoms.get(m2KingdomId);
            KingdomAsset vanilla = AssetManager.kingdoms.get(vanillaKingdomId);
            if (m2 == null || vanilla == null) return;
            foreach (string tag in tags)
            {
                m2.addTag(tag);
                m2.addFriendlyTag(tag);
            }
            m2.addFriendlyTag(vanillaKingdomId);
            vanilla.addFriendlyTag(m2KingdomId);
        }

        internal static Kingdom EnsureWildKingdom(string id)
        {
            if (string.IsNullOrEmpty(id) || World.world == null || World.world.kingdoms_wild == null) return null;
            Kingdom kingdom = World.world.kingdoms_wild.get(id);
            if (kingdom != null) return kingdom;
            KingdomAsset asset = AssetManager.kingdoms.get(id);
            if (asset == null) return null;
            EnsureKingdomColor(asset);
            kingdom = World.world.kingdoms_wild.newWildKingdom(asset);
            if (kingdom != null && kingdom.data != null && string.IsNullOrEmpty(kingdom.data.original_actor_asset))
                kingdom.data.original_actor_asset = "human";
            return kingdom;
        }

        // 0.51.2 wild kingdoms need a colour; the old game didn't.
        internal static void EnsureKingdomColor(KingdomAsset asset)
        {
            if (asset == null || asset.default_kingdom_color != null) return;
            ColorAsset color = ColorAsset.tryMakeNewColorAsset("#8C8C8C");
            if (color == null) return;
            color.id = "kingdom_library_color_" + asset.id;
            asset.default_kingdom_color = color;
        }

        private static void RegisterSpells()
        {
            AddSpawnSpell("spawnicewalker", CastSpawnIceWalker);
            AddSpawnSpell("cybercopter", CastCybercopter);
            AddSpawnSpell("spawnassimilatorzeppelin", CastSpawnAssimilatorZeppelin);
        }

        private static void AddSpawnSpell(string id, AttackAction action)
        {
            if (AssetManager.spells.get(id) != null) return;
            AssetManager.spells.add(new SpellAsset
            {
                id = id,
                chance = 3f,
                min_distance = 0f,
                cast_target = CastTarget.Himself,
                can_be_used_in_combat = true,
                action = action
            });
        }

        private static void SetSpells(ActorAsset asset, params string[] ids)
        {
            asset.spell_ids = new List<string>();
            asset.spells = new SpellHolder();
            foreach (string id in ids)
            {
                SpellAsset spell = AssetManager.spells.get(id);
                if (spell == null) continue;
                asset.spell_ids.Add(id);
                asset.spells.addSpell(spell);
            }
        }

        #endregion

        #region traits

        private static void ConfigureTraits()
        {
            ActorTrait potential = Trait("Potential", "Potential to evolve into different things. Based on traits, level, kill count and other factors. Example, if your mother is very fat she will need to go to the ocean so gravity do not crushes her body, creating a whale. Also, if a human earns veteran trait while killing the starwars desert civ, it will evolve into a drone");
            if (potential != null)
            {
                potential.action_special_effect = PotentialEffect;
                potential.can_be_given = true;
                potential.rate_inherit = 100;
            }

            ActorTrait assimilatorSpawner = Trait("AssimilatorSpawner", "Spawns one of many Assimilator-related buildings");
            if (assimilatorSpawner != null) assimilatorSpawner.action_special_effect = ActiveAssimilatorSpawnerEffect;

            ActorTrait iceSpawner = Trait("IceTowerSpawner", "Spawns one of many Walker-related buildings");
            if (iceSpawner != null) iceSpawner.action_special_effect = ActiveIceTowerSpawnerEffect;

            ActorTrait corpseSpawner = Trait("zombie_spawner", "[REDACTED]");
            if (corpseSpawner != null)
            {
                corpseSpawner.action_special_effect = ActiveCorpseSpawnerEffect;
                corpseSpawner.action_death = PileOfCorpsesEffect;
                corpseSpawner.rate_inherit = 0;
            }

            ActorTrait titan = Trait("Walker_Titan", "Great Embassador of the Ice Walker kind");
            if (titan != null)
            {
                titan.action_death = WalkerCorpseEffect;
                titan.rate_inherit = 0;
            }

            ActorTrait solar = Trait("SolarPoweredCyberBody", "Needs Sun for power, which makes it succeptible to changes on weather, told him to go nuclear but didn't want to listen, dumb terminators xD");
            if (solar != null) solar.action_special_effect = SolarPoweredCyberBodyEffect;

            ActorTrait frozen = Trait("frozenmachinary", "Letting Go");
            if (frozen != null)
            {
                frozen.base_stats = new BaseStats();
                frozen.base_stats["speed"] = -10f * M2OriginalStats.SpeedScale;
                frozen.action_special_effect = ConstantFrozenEffect;
            }

            ActorTrait unpowered = Trait("unpoweredmachinery", "Needs to recharge");
            if (unpowered != null)
            {
                unpowered.base_stats = new BaseStats();
                unpowered.base_stats["speed"] = -100f * M2OriginalStats.SpeedScale;
                unpowered.base_stats["range"] = -20f;
                unpowered.base_stats["accuracy"] = -100f;
                unpowered.action_special_effect = RandomWaitEffect;
            }

            ActorTrait freezer = Trait("Freezer", "Let it gooo! Let it gooooo!");
            if (freezer != null)
            {
                freezer.action_attack_target = ActionLibrary.addFrozenEffectOnTarget;
                freezer.can_be_given = true;
            }

            ActorTrait night = Trait("NightInfusedZombie", "The darkness powers up the disease");
            if (night != null)
            {
                night.base_stats = new BaseStats();
                night.base_stats["speed"] = 40f * M2OriginalStats.SpeedScale;
                night.base_stats["attack_speed"] = M2AttackSpeed.Delta(50f, M2AttackSpeed.OldZombieBase);
                ZombieEraTrait(night);
            }

            ActorTrait chaos = Trait("ChaosZombie", "The chaos energy powers up the disease");
            if (chaos != null)
            {
                chaos.base_stats = new BaseStats();
                chaos.base_stats["speed"] = 20f * M2OriginalStats.SpeedScale;
                chaos.base_stats["scale"] = 0.05f;
                chaos.base_stats["multiplier_health"] = 0.3f;
                chaos.base_stats["attack_speed"] = M2AttackSpeed.Delta(30f, M2AttackSpeed.OldZombieBase);
                chaos.action_attack_target = ActionLibrary.restoreHealthOnHit;
                ZombieEraTrait(chaos);
            }

            ActorTrait frosted = Trait("FrostedZombie", "Letting Go");
            if (frosted != null)
            {
                frosted.base_stats = new BaseStats();
                frosted.base_stats["speed"] = -30f * M2OriginalStats.SpeedScale;
                frosted.base_stats["attack_speed"] = M2AttackSpeed.Delta(-50f, M2AttackSpeed.OldZombieBase);
                frosted.action_special_effect = ConstantFrozenEffect;
                ZombieEraTrait(frosted);
            }

            ActorTrait scorched = Trait("ScorchedZombie", "The Sun is weakening the rotting body of the zombie");
            if (scorched != null)
            {
                scorched.base_stats = new BaseStats();
                scorched.base_stats["speed"] = -100f * M2OriginalStats.SpeedScale;
                scorched.base_stats["range"] = -20f;
                scorched.base_stats["accuracy"] = -100f;
                scorched.action_special_effect = RandomWaitEffect;
                ZombieEraTrait(scorched);
            }
        }

        private static ActorTrait Trait(string id, string description)
        {
            ActorTrait trait = AssetManager.traits.get(id);
            if (trait == null) return null;
            trait.rate_inherit = 10;
            trait.has_localized_id = true;
            trait.has_description_1 = true;
            ModernLocalization.Add("trait_" + id + "_info", description);
            return trait;
        }

        private static void ZombieEraTrait(ActorTrait trait)
        {
            trait.rate_inherit = 0;
            trait.can_be_given = false;
        }

        #endregion

        #region creature assets

        private static ActorAsset Actor(string id)
        {
            return AssetManager.actor_library.get(id);
        }

        private static void AddTraits(ActorAsset asset, params string[] ids)
        {
            foreach (string id in ids)
            {
                if (AssetManager.traits.get(id) == null) continue;
                if (asset.traits != null && asset.traits.Contains(id)) continue;
                asset.addTrait(id);
            }
        }

        private static void RemoveTraits(ActorAsset asset, params string[] ids)
        {
            if (asset.traits == null) return;
            foreach (string id in ids) asset.traits.Remove(id);
        }

        // Original fmod_* paths use the 0.51.2 "event:/SFX/UNITS/<id>/<kind>" layout.
        private static void Sounds(ActorAsset asset, string unitFolder, string hit, string theme = null)
        {
            string path = "event:/SFX/UNITS/" + unitFolder;
            asset.sound_spawn = path + "/spawn";
            asset.sound_attack = path + "/attack";
            asset.sound_idle = path + "/idle";
            asset.sound_death = path + "/death";
            if (hit != null) asset.sound_hit = hit;
            if (theme != null) asset.music_theme = theme;
        }

        private static void Faction(ActorAsset asset, string kingdomId)
        {
            asset.kingdom_id_wild = kingdomId;
            EnsureWildKingdom(kingdomId);
        }

        private static void ConfigureCreatures()
        {
            const string HitMetal = "event:/SFX/HIT/HitMetal";
            const string HitGeneric = "event:/SFX/HIT/HitGeneric";
            const string HitFlesh = "event:/SFX/HIT/HitFlesh";
            string assimilators = ActorsAndBuildingsRegistry.AssimilatorKingdomId;
            string walkers = ActorsAndBuildingsRegistry.WalkerKingdomId;

            ActorAsset a;
            if ((a = Actor("assimilator")) != null) AddTraits(a, "Potential", "SolarPoweredCyberBody");

            if ((a = Actor("glitchtarantula")) != null)
            {
                a.take_items = false;
                a.use_items = false;
                a.actor_size = ActorSize.S16_Buffalo;
                a.default_weapons = Array.Empty<string>();
            }
            if ((a = Actor("glitchdrake")) != null)
            {
                Faction(a, "undead");
                a.actor_size = ActorSize.S17_Dragon;
                a.can_be_killed_by_divine_light = true;
                a.can_be_killed_by_life_eraser = true;
                a.can_be_moved_by_powers = true;
                a.can_turn_into_zombie = false;
                a.can_turn_into_mush = false;
                a.can_turn_into_tumor = false;
                a.can_attack_buildings = true;
                a.damaged_by_ocean = false;
                a.force_land_creature = true;
                a.take_items = false;
                a.use_items = false;
                a.has_skin = false;
                a.has_soul = false;
                a.die_in_lava = false;
                a.hovering = true;
                a.flying = false;
                a.very_high_flyer = false;
                a.hovering_min = 0f;
                a.hovering_max = 0f;
                a.move_from_block = true;
                a.die_on_blocks = false;
                a.disable_jump_animation = true;
                a.action_death = (WorldAction)Delegate.Combine(a.action_death, new WorldAction(ActionLibrary.dragonSlayer));
                AddTraits(a, "fire_proof", "giant", "tough");
            }
            if ((a = Actor("glitchspectre")) != null)
            {
                Faction(a, "undead");
                a.can_attack_buildings = false;
                a.damaged_by_ocean = false;
                a.force_land_creature = true;
                a.die_in_lava = false;
                AddTraits(a, "fire_proof");
            }

            if ((a = Actor("Assimilatus")) != null)
            {
                Faction(a, assimilators);
                Sounds(a, "assimilator", HitMetal);
                a.immune_to_slowness = true;
                SetSpells(a, "cybercopter", "cybercopter", "cybercopter");
                a.actor_size = ActorSize.S17_Dragon;
                a.run_to_water_when_on_fire = true;
                a.can_be_killed_by_divine_light = false;
                a.can_be_killed_by_life_eraser = true;
                a.can_turn_into_zombie = false;
                a.can_turn_into_mush = false;
                a.can_turn_into_tumor = false;
                a.damaged_by_ocean = true;
                a.immune_to_injuries = false;
                a.has_skin = false;
                a.has_soul = false;
                a.die_in_lava = true;
                a.max_random_amount = 1;
                // basecrusader's setup in the original wrote its armor onto Assimilatus.
                a.base_stats["armor"] = 25f;
                AddTraits(a, "AssimilatorSpawner", "fire_proof", "bubble_defense", "SolarPoweredCyberBody");
                RemoveTraits(a, "weightless", "ugly");
            }

            if ((a = Actor("Cocytuswalker")) != null)
            {
                Faction(a, walkers);
                Sounds(a, "cold_one", HitGeneric);
                a.immune_to_slowness = true;
                SetSpells(a, "spawnicewalker", "spawnicewalker", "spawnicewalker");
                a.actor_size = ActorSize.S17_Dragon;
                a.base_stats["targets"] = 10f;
                a.base_stats["knockback"] = 1f;
                a.can_be_killed_by_divine_light = false;
                a.can_be_killed_by_life_eraser = true;
                a.can_turn_into_mush = false;
                a.can_turn_into_tumor = false;
                a.damaged_by_ocean = false;
                a.immune_to_injuries = false;
                a.has_skin = true;
                a.has_soul = false;
                a.die_in_lava = true;
                a.disable_jump_animation = true;
                a.max_random_amount = 1;
                AddTraits(a, "Walker_Titan", "IceTowerSpawner", "regeneration", "weightless", "freeze_proof", "cold_aura", "Freezer");
            }

            foreach (string id in new[] { "newwalker", "normalwalker" })
            {
                if ((a = Actor(id)) == null) continue;
                Faction(a, walkers);
                Sounds(a, "cold_one", HitGeneric);
                AddTraits(a, "Potential");
            }
            if ((a = Actor("icedracoid")) != null)
            {
                Faction(a, walkers);
                Sounds(a, "cold_one", HitGeneric, "Units_ColdOne");
                a.base_stats["targets"] = 1f;
                a.hovering_min = 10f;
                a.hovering_max = 10f;
                a.move_from_block = true;
                a.die_on_blocks = false;
                AddTraits(a, "IceTowerSpawner", "Potential");
            }
            if ((a = Actor("buffrost")) != null)
            {
                Faction(a, walkers);
                Sounds(a, "cold_one", HitGeneric);
                a.base_stats["knockback"] = 2f;
                a.base_stats["targets"] = 4f;
                a.actor_size = ActorSize.S16_Buffalo;
                AddTraits(a, "Freezer", "Potential");
            }

            if ((a = Actor("helilator")) != null)
            {
                Faction(a, assimilators);
                Sounds(a, "assimilator", HitMetal);
                a.damaged_by_ocean = true;
                a.hovering_min = 2f;
                a.hovering_max = 2f;
                a.take_items = false;
                a.use_items = false;
                a.move_from_block = true;
                a.die_on_blocks = false;
                a.can_turn_into_mush = false;
                AddTraits(a, "AssimilatorSpawner", "Potential", "SolarPoweredCyberBody");
            }
            if ((a = Actor("assimilarptor")) != null)
            {
                Faction(a, assimilators);
                Sounds(a, "assimilator", HitMetal);
                a.damaged_by_ocean = true;
                a.take_items = false;
                a.use_items = false;
                a.can_turn_into_mush = false;
                a.disable_jump_animation = false;
                AddTraits(a, "Potential", "SolarPoweredCyberBody");
            }
            if ((a = Actor("assimilatrax")) != null)
            {
                Faction(a, assimilators);
                Sounds(a, "assimilator", HitMetal);
                a.base_stats["knockback"] = 2f;
                a.base_stats["targets"] = 4f;
                a.damaged_by_ocean = true;
                a.take_items = false;
                a.use_items = false;
                a.can_turn_into_mush = false;
                AddTraits(a, "AssimilatorSpawner", "SolarPoweredCyberBody");
            }
            if ((a = Actor("assizeppelin")) != null)
            {
                Faction(a, assimilators);
                Sounds(a, "assimilator", HitMetal);
                a.base_stats["size"] = 2f;
                a.damaged_by_ocean = true;
                a.hovering_min = 5f;
                a.hovering_max = 5f;
                a.take_items = false;
                a.use_items = false;
                a.move_from_block = true;
                a.immune_to_slowness = true;
                a.die_on_blocks = false;
                a.can_turn_into_mush = false;
                AddTraits(a, "AssimilatorSpawner", "fire_blood", "bubble_defense", "fire_proof", "weightless", "SolarPoweredCyberBody");
            }

            if ((a = Actor("Terlanius")) != null)
            {
                Faction(a, TerlaniusKingdomId);
                SetJob(a, "skeleton_job");
                a.run_to_water_when_on_fire = true;
                a.can_be_killed_by_stuff = true;
                a.can_be_killed_by_life_eraser = true;
                a.can_attack_buildings = true;
                a.can_be_moved_by_powers = true;
                a.can_be_hurt_by_powers = true;
                a.can_turn_into_zombie = false;
                a.can_be_inspected = true;
                a.use_items = false;
                a.take_items = false;
                a.inspect_home = false;
                a.disable_jump_animation = true;
                a.has_soul = true;
                a.force_land_creature = false;
                a.can_turn_into_demon_in_age_of_chaos = true;
                a.can_turn_into_ice_one = true;
                a.can_turn_into_tumor = true;
                a.can_turn_into_mush = false;
                a.die_in_lava = true;
                a.die_on_blocks = false;
                a.die_by_lightning = true;
                a.damaged_by_ocean = true;
                a.flying = true;
                a.very_high_flyer = false;
                a.can_be_killed_by_divine_light = true;
                a.actor_size = ActorSize.S15_Bear;
                a.base_stats["lifespan"] = 1000f;
                a.base_stats["critical_chance"] = 0.1f;
                a.base_stats["knockback"] = 0.1f;
                a.base_stats["targets"] = 1f;
                AddTraits(a, "immortal");
            }

            if ((a = Actor("scandid")) != null)
            {
                SetJob(a, DuneCritterJobId);
                Sounds(a, "crab", null);
                a.max_random_amount = 4;
                a.has_skin = true;
                AddTraits(a, "venomous");
            }
            if ((a = Actor("Duneworm")) != null)
            {
                Faction(a, DuneMonsterKingdomId);
                SetJob(a, DuneCritterJobId);
                Sounds(a, "crab", null);
                a.max_random_amount = 2;
                a.has_skin = true;
                AddTraits(a, "giant");
            }

            if ((a = Actor("alienwisp")) != null)
            {
                a.can_attack_buildings = false;
                AddTraits(a, "light_lamp");
            }
            if ((a = Actor("pantherax")) != null)
            {
                Sounds(a, "crocodile", null);
                a.base_stats["targets"] = 6f;
                a.force_land_creature = true;
                AddTraits(a, "strong", "flesh_eater");
            }
            if ((a = Actor("pterax")) != null)
            {
                Sounds(a, "penguin", null);
                a.max_random_amount = 6;
                a.move_from_block = true;
                a.die_on_blocks = false;
                AddTraits(a, "weightless", "gluttonous");
            }
            if ((a = Actor("rhinokinglor")) != null)
            {
                Sounds(a, "rhino", null);
                AddTraits(a, "giant", "tough");
            }
            if ((a = Actor("geckoid")) != null)
            {
                Sounds(a, "buffalo", null);
                AddTraits(a, "thorns");
            }
            if ((a = Actor("hashbrowncat")) != null) a.force_land_creature = true;
            foreach (string id in new[] { "peones", "xenodogo" })
            {
                if ((a = Actor(id)) == null) continue;
                a.force_land_creature = true;
                AddTraits(a, "bubble_defense");
            }

            if ((a = Actor("basecrusader")) != null)
            {
                Faction(a, ActorsAndBuildingsRegistry.CrusaderKingdomId);
                SetJob(a, VaticanForceJobId);
                Sounds(a, "plague_doctor", null);
                ActorAsset doctor = Actor("plague_doctor");
                if (doctor != null) a.base_stats["armor"] = doctor.base_stats["armor"];
                a.base_stats["knockback"] = 2f;
                a.base_stats["targets"] = 5f;
                AddTraits(a, "Potential", "regeneration", "immune", "fire_proof");
            }
            foreach (string id in new[] { "crusaderdreadnaught", "crusaderHeli", "crusadermaus" })
            {
                if ((a = Actor(id)) == null) continue;
                Faction(a, ActorsAndBuildingsRegistry.CrusaderKingdomId);
                SetJob(a, VaticanForceJobId);
                a.immune_to_injuries = true;
                AddTraits(a, "Potential", "regeneration", "immune", "fire_proof", "light_lamp");
            }
            if ((a = Actor("crusaderHeli")) != null)
            {
                a.base_stats["accuracy"] = 100f;
                AddTraits(a, "Helicopter", "freeze_proof");
            }
            if ((a = Actor("crusadermaus")) != null) AddTraits(a, "Tank");

            ConfigureZombies(HitFlesh);

            NaturalFactions.Clear();
            foreach (string id in new[] { "Terlanius", "Duneworm", "scandid", "glitchdrake", "glitchspectre", "glitchtarantula", "hashbrowncat" })
            {
                ActorAsset creature = Actor(id);
                if (creature == null || string.IsNullOrEmpty(creature.kingdom_id_wild)) continue;
                NaturalFactions[id] = creature.kingdom_id_wild;
                EnsureWildKingdom(creature.kingdom_id_wild);
            }
        }

        private static void ConfigureZombies(string hitFlesh)
        {
            // M2 made the vanilla zombie tougher, item-using and a wanderer.
            foreach (string id in new[] { "zombie", "zombie_human" })
            {
                ActorAsset vanilla = Actor(id);
                if (vanilla == null) continue;
                vanilla.take_items = true;
                vanilla.use_items = true;
                vanilla.base_stats["health"] = 200f;
                vanilla.base_stats["damage"] = 30f;
                vanilla.job = new[] { ZombieWandererJobId };
            }

            Zombie("zombiespeed", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "fast");
            Zombie("zombiespikes", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "thorns");
            Zombie("zombiepoison", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "venomous", "poisonous", "poison_immune");
            Zombie("zombieacid", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "acid_touch", "acid_blood", "acid_proof");
            Zombie("zombietentacle", "zombie", hitFlesh, true, "zombie", "immortal", "agile");
            Zombie("zombiestalker", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "giant", "zombie_spawner");
            Zombie("zombiemother", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "fat", "giant", "acid_blood", "zombie_spawner");
            Zombie("zombiefiremaniac", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "fire_proof", "pyromaniac");
            Zombie("zombiehulk", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "giant", "fat", "zombie_spawner");
            Zombie("zombieabomination", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "giant", "fat", "zombie_spawner");
            Zombie("zombieclawed", "zombie", hitFlesh, true, "zombie", "immortal", "stupid", "giant", "strong", "zombie_spawner");
            Zombie("zombieballoon", "zombie", hitFlesh, false, "zombie", "immortal", "stupid", "peaceful", "acid_blood", "acid_touch", "zombie_spawner");
            Zombie("zombieacidman", "zombie", hitFlesh, false, "zombie", "immortal", "stupid", "acid_blood", "acid_proof", "acid_touch");
            Zombie("zombiedemon", "zombie", hitFlesh, true, "zombie", "immortal", "stupid");
            Zombie("zombiedoctor", "zombie", hitFlesh, true, "zombie", "immortal", "stupid");
            Zombie("zombiedruid", "zombie_animal", hitFlesh, true, "zombie", "immortal", "stupid");
            Zombie("zombieevilhorseman", "zombie", hitFlesh, true, "zombie", "immortal", "stupid");
            Zombie("zombieicelich", "zombie", hitFlesh, true, "zombie", "immortal", "stupid");
            Zombie("zombiefairy", "zombie", hitFlesh, true, "zombie", "immortal", "stupid");
            // "small" has no 0.51.2 equivalent.
            Zombie("zombietarantula", "zombie_animal", hitFlesh, false, "zombie", "immortal", "venomous");

            ActorAsset a;
            if ((a = Actor("zombieballoon")) != null)
            {
                a.flying = true;
                a.very_high_flyer = true;
                a.can_flip = false;
            }
            if ((a = Actor("zombiedemon")) != null)
            {
                // The original attached these to zombiedemon while building zombiedoctor.
                a.action_death = (WorldAction)Delegate.Combine(a.action_death, new WorldAction(ActionLibrary.mageSlayerCheck));
                SetSpells(a, "cast_blood_rain");
            }
            if ((a = Actor("zombiedruid")) != null) SetSpells(a, "cast_blood_rain");
            if ((a = Actor("zombieevilhorseman")) != null)
            {
                a.effect_teleport = "fx_teleport_red";
                a.effect_cast_top = "fx_cast_top_red";
                a.effect_cast_ground = "fx_cast_ground_red";
                SetSpells(a, "teleport", "summon_lightning", "summon_tornado", "cast_blood_rain", "cast_blood_rain", "cast_fire");
                a.base_stats["targets"] = 1f;
            }
            if ((a = Actor("zombieicelich")) != null)
            {
                a.effect_teleport = "fx_teleport_blue";
                a.effect_cast_top = "fx_cast_top_blue";
                a.effect_cast_ground = "fx_cast_ground_blue";
                SetSpells(a, "teleport", "cast_blood_rain", "cast_shield");
                a.disable_jump_animation = true;
                a.base_stats["targets"] = 1f;
            }
            if ((a = Actor("zombiedoctor")) != null) a.base_stats["targets"] = 1f;
            if ((a = Actor("zombiefairy")) != null)
            {
                a.hovering = true;
                a.disable_jump_animation = true;
                a.move_from_block = true;
                a.die_on_blocks = false;
            }
        }

        private static void Zombie(string id, string soundFolder, string hit, bool takesItems, params string[] traits)
        {
            ActorAsset a = Actor(id);
            if (a == null) return;
            Faction(a, ActorsAndBuildingsRegistry.NativeUndeadKingdomId);
            SetJob(a, ZombieWandererJobId);
            Sounds(a, soundFolder, hit, "Units_Zombie");
            a.color_hex = "#24803E";
            a.can_be_killed_by_divine_light = true;
            a.take_items = takesItems;
            a.use_items = takesItems;
            a.body_separate_part_hands = false;
            a.can_attack_buildings = true;
            a.can_attack_brains = true;
            a.force_land_creature = true;
            a.force_ocean_creature = false;
            a.can_turn_into_mush = false;
            a.can_turn_into_zombie = false;
            a.zombie_auto_asset = false;
            a.zombie_id_internal = string.Empty;
            a.default_weapons = Array.Empty<string>();
            AddTraits(a, traits);
        }

        private static void ConfigureZombieConversions()
        {
            Convert("snowman", "zombieacidman");
            Convert("demon", "zombiedemon");
            Convert("plague_doctor", "zombiedoctor");
            Convert("druid", "zombiedruid");
            Convert("evil_mage", "zombieevilhorseman");
            Convert("white_mage", "zombieicelich");
            Convert("cold_one", "zombieicelich");
            Convert("fairy", "zombiefairy");
            foreach (string insect in new[] { "fly", "butterfly", "grasshopper", "beetle" })
                Convert(insect, "zombietarantula");
        }

        private static void Convert(string actorId, string zombieId)
        {
            ActorAsset source = Actor(actorId);
            if (source == null || Actor(zombieId) == null) return;
            source.setCanTurnIntoZombieAsset(zombieId, false);
        }

        private static void ConfigureSpawnerBuildings()
        {
            BuildingAsset b;
            if ((b = AssetManager.buildings.get("missilecybercore")) != null)
            {
                b.kingdom = ActorsAndBuildingsRegistry.AssimilatorKingdomId;
                b.spawn_units = true;
                b.spawn_units_asset = "assimilarptor";
                b.housing_slots = 5;
                b.base_stats["health"] = 200f;
                b.base_stats["damage"] = 3f;
                b.base_stats["knockback"] = 0.2f;
                b.draw_light_area = true;
                b.draw_light_size = 0.2f;
                b.draw_light_area_offset_y = 2f;
                b.transform_tiles_to_top_tiles = "cybertile_low";
                b.grow_creep = true;
                b.grow_creep_type = "biome_cybertile";
                b.grow_creep_steps_max = 20;
                b.grow_creep_workers = 6;
                b.grow_creep_step_interval = 2f;
                b.grow_creep_movement_type = CreepWorkerMovementType.Direction;
                b.grow_creep_steps_before_new_direction = 7;
                b.grow_creep_direction_random_position = false;
                b.grow_creep_random_new_direction = true;
                b.damaged_by_rain = true;
                b.burnable = false;
                b.material = "building";
                b.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleCybercore";
                b.sound_hit = "event:/SFX/HIT/HitMetal";
                b.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingRobotic";
                b.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingRobotic";
            }
            if ((b = AssetManager.buildings.get("icewatchtower")) != null)
            {
                b.base_stats["health"] = 200f;
                b.base_stats["damage"] = 10f;
                b.base_stats["knockback"] = 0.5f;
                b.draw_light_area = true;
                b.draw_light_size = 0.5f;
                b.draw_light_area_offset_y = 8f;
                b.burnable = false;
                b.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
            }
            if ((b = AssetManager.buildings.get("newicetower")) != null)
            {
                b.spawn_units_asset = "newwalker";
                b.housing_slots = 5;
                b.base_stats["health"] = 500f;
                b.base_stats["damage"] = 10f;
                b.draw_light_area = true;
                b.draw_light_size = 0.5f;
                b.draw_light_area_offset_y = 8f;
                b.burnable = false;
                b.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
            }
            if ((b = AssetManager.buildings.get("walkercorpse")) != null)
            {
                b.kingdom = "nature";
                b.civ_kingdom = "nature";
                b.spawn_units_asset = "fly";
                b.housing_slots = 5;
                b.base_stats["health"] = 10000f;
                b.can_be_placed_on_liquid = true;
                b.burnable = false;
                b.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBeehive";
                b.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingGeneric";
                b.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
            }
            if ((b = AssetManager.buildings.get("pileofcorpses")) != null)
            {
                b.spawn_units_asset = "zombie";
                b.housing_slots = 5;
                b.base_stats["health"] = 1000f;
                b.burnable = true;
                b.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingGeneric";
                b.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
            }
        }

        #endregion

        #region trait effects

        private static bool PotentialEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || !actor.isAlive() || actor.data == null) return false;
            if (CheckAndTransformByEraAndMob(actor)) return true;
            return CheckAndTransformByTraitsAgeKills(actor, actor.data.kills);
        }

        private static bool CheckAndTransformByTraitsAgeKills(Actor actor, int kills)
        {
            switch (actor.asset.id)
            {
                case "newwalker":
                    if (Chance(0.3f)) { M2LegacyBehaviorService.TransformActor(actor, "icedracoid"); return true; }
                    if (Chance(0.3f)) { M2LegacyBehaviorService.TransformActor(actor, "buffrost"); return true; }
                    if (Chance(0.3f)) { M2LegacyBehaviorService.TransformActor(actor, "normalwalker"); return true; }
                    break;
                case "assimilator":
                    if (kills > 5) { M2LegacyBehaviorService.TransformActor(actor, RandomOf("assimilarptor", "assimilatrax", "helilator", "assizeppelin")); return true; }
                    break;
                case "assimilarptor":
                    if (kills > 5) { M2LegacyBehaviorService.TransformActor(actor, RandomOf("assimilatrax", "helilator", "assizeppelin")); return true; }
                    break;
                case "basecrusader":
                    if (kills > 5) { M2LegacyBehaviorService.TransformActor(actor, RandomOf("crusaderdreadnaught", "crusaderHeli", "crusadermaus")); return true; }
                    break;
            }
            return false;
        }

        // Walkers melt into flies outside winter (TransformActorToUFO spawned "fly").
        private static bool CheckAndTransformByEraAndMob(Actor actor)
        {
            if (World.world_era == null || !WalkerRelatedMobs.Contains(actor.asset.id)) return false;
            if (World.world_era.overlay_winter || !Chance(0.1f)) return false;
            M2LegacyBehaviorService.TransformActorWithoutCopy(actor, "fly");
            return true;
        }

        private static string RandomOf(params string[] ids)
        {
            return ids[UnityEngine.Random.Range(0, ids.Length)];
        }

        private static bool IsExhausted(Actor actor)
        {
            if (actor.data == null) return true;
            if (actor.data.hasFlag(ExhaustedSpawnerFlag)) return true;
            foreach (string flag in LegacySpawnerFlags)
                if (actor.data.hasFlag(flag)) return true;
            return false;
        }

        private static bool ActiveAssimilatorSpawnerEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || tile == null || tile.chunk == null) return false;
            if (CountBuildingsInChunk(tile.chunk, "missilecybercore", "cybercore") >= 3) return false;
            if (tile.Type == null || tile.Type.liquid) return false;
            if (IsExhausted(actor)) return true;
            WorldTile here = actor.current_tile;
            if (here == null || here.building != null) return true;
            bool missile = Chance(0.4f);
            bool core = Chance(0.6f);
            if ((missile && PlaceBuilding("missilecybercore", here, false) != null) ||
                (core && PlaceBuilding("cybercore", here, false) != null))
                actor.data.addFlag(ExhaustedSpawnerFlag);
            return true;
        }

        private static bool ActiveIceTowerSpawnerEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || tile == null) return false;
            if (World.world_era == null || !World.world_era.overlay_winter) return false;
            if (tile.chunk == null || CountBuildingsInChunk(tile.chunk, "icewatchtower", "newicetower") >= 1) return false;
            if (tile.Type == null || tile.Type.liquid) return false;
            if (IsExhausted(actor)) return true;
            WorldTile here = actor.current_tile;
            if (here == null || here.building != null) return true;
            string tower = Chance(0.05f) ? "icewatchtower" : "newicetower";
            if (PlaceBuilding(tower, here, false) != null) actor.data.addFlag(ExhaustedSpawnerFlag);
            return true;
        }

        private static bool ActiveCorpseSpawnerEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || tile == null || tile.chunk == null) return false;
            if (CountBuildingsInChunk(tile.chunk, "pileofcorpses") >= 3) return false;
            if (tile.Type == null || tile.Type.liquid) return false;
            if (IsExhausted(actor)) return true;
            WorldTile here = actor.current_tile;
            if (here == null || here.building != null) return true;
            // Two rolls (40% then 60%) for the same tile; the first success wins.
            bool first = Chance(0.4f);
            bool second = Chance(0.6f);
            if ((first || second) && PlaceBuilding("pileofcorpses", here, false) != null)
                actor.data.addFlag(ExhaustedSpawnerFlag);
            return true;
        }

        internal static bool PileOfCorpsesEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || actor.current_tile == null) return false;
            if (actor.current_tile.building == null) PlaceBuilding("pileofcorpses", actor.current_tile, false);
            return true;
        }

        internal static bool WalkerCorpseEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || actor.current_tile == null) return false;
            if (actor.current_tile.building == null) PlaceBuilding("walkercorpse", actor.current_tile, false);
            return true;
        }

        private static bool SolarPoweredCyberBodyEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || !actor.isAlive() || World.world_era == null) return false;
            M2LegacyBehaviorService.ToggleTrait(actor, "fast", World.world_era.overlay_sun);
            M2LegacyBehaviorService.ToggleTrait(actor, "frozenmachinary", World.world_era.overlay_winter);
            M2LegacyBehaviorService.ToggleTrait(actor, "madness", World.world_era.overlay_chaos);
            M2LegacyBehaviorService.ToggleTrait(actor, "unpoweredmachinery", World.world_era.overlay_night || World.world_era.overlay_moon);
            return true;
        }

        private static bool RandomWaitEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null) return false;
            if (Chance(0.2f)) actor.makeWait(10f);
            return true;
        }

        private static bool ConstantFrozenEffect(BaseSimObject target, WorldTile tile = null)
        {
            Actor actor = target == null ? null : target.a;
            if (actor == null || actor.current_tile == null) return false;
            if (!actor.hasStatus("frozen") && !actor.current_tile.Type.lava && !actor.current_tile.isOnFire())
                actor.addStatusEffect("frozen");
            return true;
        }

        #endregion

        #region spells

        private static bool CastSpawnIceWalker(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            return CastSummon(target, tile, "newwalker");
        }

        private static bool CastCybercopter(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            return CastSummon(target, tile, "helilator");
        }

        private static bool CastSpawnAssimilatorZeppelin(BaseSimObject self, BaseSimObject target, WorldTile tile)
        {
            return CastSummon(target, tile, "assizeppelin");
        }

        private static bool CastSummon(BaseSimObject target, WorldTile tile, string unitId)
        {
            if (target != null) tile = target.current_tile;
            if (tile == null || World.world == null) return false;
            int nearby = 0;
            foreach (Actor unit in Finder.findSpeciesAroundTileChunk(tile, unitId))
                if (unit != null) nearby++;
            if (nearby > 6) return false;
            WorldTile spawnTile = tile.region == null ? null : tile.region.getRandomTile();
            if (spawnTile == null) return false;
            Actor spawned = World.world.units.createNewUnit(unitId, spawnTile);
            if (spawned != null) spawned.makeWait(1f);
            return spawned != null;
        }

        #endregion

        #region buildings

        internal static Building PlaceBuilding(string buildingId, WorldTile tile, bool requireLand)
        {
            BuildingAsset asset = AssetManager.buildings.get(buildingId);
            if (asset == null || tile == null || tile.Type == null || tile.building != null || World.world == null || World.world.buildings == null) return null;
            if ((requireLand || !asset.can_be_placed_on_liquid) && tile.Type.liquid) return null;
            Building building = World.world.buildings.addBuilding(asset, tile, false, false, BuildPlacingType.New);
            if (building == null) return null;
            Kingdom kingdom = EnsureWildKingdom(asset.kingdom) ?? World.world.kingdoms_wild?.get("nature");
            if (kingdom != null) building.setKingdom(kingdom);
            return building;
        }

        private static int CountBuildingsInChunk(MapChunk chunk, params string[] ids)
        {
            if (chunk == null || World.world == null || World.world.buildings == null) return 0;
            int count = 0;
            foreach (Building building in World.world.buildings)
            {
                if (building == null || !building.isAlive() || building.asset == null || building.current_tile == null ||
                    building.current_tile.chunk != chunk || building.isRuin() || Array.IndexOf(ids, building.asset.id) < 0) continue;
                count++;
            }
            return count;
        }

        #endregion
    }
}
