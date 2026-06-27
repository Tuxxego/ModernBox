using System;
using tools;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using HarmonyLib;
using System.Text.RegularExpressions;
using Beebyte.Obfuscator;
using ai;
using ai.behaviours;
using NeoModLoader;
using NeoModLoader.General;

namespace ModernBox
{
    class Zombies
    {
        private const float ZombieCountRefreshSeconds = 1.5f;
        private static float nextZombieCountRefreshAt;
        private static object cachedZombieWorldToken;
        private static int cachedZombieCount;

        public static void init(){
          create_Zombies();
        }

          public static void create_Zombies(){

            ActorTrait zombie_spawner = new ActorTrait();
            zombie_spawner.id = "zombie_spawner";
            zombie_spawner.needs_to_be_explored=false;
            zombie_spawner.rarity = Rarity.R0_Normal;
            zombie_spawner.path_icon = "ui/Icons/XenoInfectionIcon";
            zombie_spawner.group_id = "special";
            zombie_spawner.is_mutation_box_allowed = false;
            zombie_spawner.can_be_given = true;
            zombie_spawner.unlocked_with_achievement = false;
            zombie_spawner.addOpposite("madness");
            if (zombie_spawner.base_stats == null)
            zombie_spawner.base_stats = new BaseStats();

            zombie_spawner.action_special_effect = new WorldAction(zombie_spawnerEffect);
            AssetManager.traits.add(zombie_spawner);
            LM.AddToCurrentLocale("trait_zombie_spawner", "Zombie Beacon");
            LM.AddToCurrentLocale("trait_zombie_spawner_info", "They rally around him...");
            LM.AddToCurrentLocale("trait_zombie_spawner_info_2", "helicopter helicopter");

          AssetManager.job_actor.add(new ActorJob{id = "ZombieWorse"});
          AssetManager.job_actor.t.addTask("follow_same_race");
          AssetManager.job_actor.t.addTask("swim_to_island");
          AssetManager.job_actor.t.addTask("random_move");
          AssetManager.job_actor.t.addTask("wait10");

           var zombie = AssetManager.actor_library.get("zombie");
        zombie.take_items = true;
                zombie.use_items = true;
        zombie.base_stats["health"] = 200f;
            zombie.base_stats["damage"] = 30f;
       zombie.job = AssetLibrary<ActorAsset>.a<string>("ZombieWorse");

            new ActorBuilder("zombiespeed", "zombie")
                .SetNameLocale("Speed Zombie")
                .SetTexturePath("actors/zombiespeed/")
                .SetAnimationWalk(new[] { "walk_0", "walk_1", "walk_2", "walk_3" })
                .SetAnimationIdle(new[] { "walk_0" })
                .SetHealth(200f)
                .SetDamage(30f)
                .SetSpeed(100f)
                .SetAttackSpeed(310f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("fast")
                .Build();

            new ActorBuilder("zombiespikes", "zombie")
                .SetNameLocale("Spiked Zombie")
                .SetTexturePath("actors/zombiespikes/")
                .SetAnimationWalk(new[] { "walk_0", "walk_1" })
                .SetAnimationIdle(new[] { "walk_0" })
                .SetHealth(350f)
                .SetDamage(50f)
                .SetSpeed(50f)
                .SetAttackSpeed(110f)
                .SetArmor(15f)
                .SetKnockback(0f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("thorns")
                .Build();

            new ActorBuilder("zombiepoison", "zombie")
                .SetNameLocale("Poison Zombie")
                .SetTexturePath("actors/zombiepoison/")
                .SetHealth(350f)
                .SetDamage(54f)
                .SetSpeed(50f)
                .SetAttackSpeed(510f)
                .SetArmor(0f)
                .SetKnockback(0f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("venomous")
                .AddTrait("poisonous")
                .AddTrait("poison_immune")
                .Build();

            new ActorBuilder("zombieacid", "zombie")
                .SetNameLocale("Acid Zombie")
                .SetTexturePath("actors/zombieacid/")
                .SetHealth(350f)
                .SetDamage(64f)
                .SetSpeed(60f)
                .SetAttackSpeed(210f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S16_Buffalo)
                .AddTrait("zombie")
                .AddTrait("acid_touch")
                .AddTrait("acid_blood")
                .AddTrait("acid_proof")
                .Build();

            new ActorBuilder("zombietentacle", "zombie")
                .SetNameLocale("Tentacle Zombie")
                .SetTexturePath("actors/zombietentacle/")
                .SetHealth(400f)
                .SetDamage(33f)
                .SetSpeed(70f)
                .SetAttackSpeed(2110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("agile")
                .AddTrait("zombie_spawner")
                .Build();

            new ActorBuilder("zombiestalker", "zombie")
                .SetNameLocale("Stalker Zombie")
                .SetTexturePath("actors/zombiestalker/")
                .SetHealth(1050f)
                .SetDamage(104f)
                .SetSpeed(60f)
                .SetAttackSpeed(210f)
                .SetArmor(10f)
                .SetKnockback(2f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("giant")
                .AddTrait("zombie_spawner")
                .Build();

            new ActorBuilder("zombiemother", "zombie")
                .SetNameLocale("Mother Zombie")
                .SetTexturePath("actors/zombiemother/")
                .SetHealth(2000f)
                .SetDamage(204f)
                .SetSpeed(30f)
                .SetAttackSpeed(150f)
                .SetArmor(0f)
                .SetKnockback(8f)
                .SetActorSize(ActorSize.S17_Dragon)
                .AddTrait("zombie")
                .AddTrait("fat")
                .AddTrait("giant")
                .AddTrait("acid_blood")
                .AddTrait("zombie_spawner")
                .Build();

            new ActorBuilder("zombiefiremaniac", "zombie")
                .SetNameLocale("Fire Maniac Zombie")
                .SetTexturePath("actors/zombiefiremaniac/")
                .SetHealth(450f)
                .SetDamage(70f)
                .SetSpeed(70f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("fire_proof")
                .AddTrait("pyromaniac")
                .AddTrait("fire_hands")
                .Build();

            new ActorBuilder("zombiehulk", "zombie")
                .SetNameLocale("Hulk Zombie")
                .SetTexturePath("actors/zombiehulk/")
                .SetHealth(1000f)
                .SetDamage(240f)
                .SetSpeed(40f)
                .SetAttackSpeed(300f)
                .SetArmor(20f)
                .SetKnockback(3f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("giant")
                .AddTrait("fat")
                .AddTrait("zombie_spawner")
                .Build();

            new ActorBuilder("zombieabomination", "zombie")
                .SetNameLocale("Abomination Zombie")
                .SetTexturePath("actors/zombieabomination/")
                .SetHealth(1300f)
                .SetDamage(240f)
                .SetSpeed(40f)
                .SetAttackSpeed(300f)
                .SetArmor(30f)
                .SetKnockback(3f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("giant")
                .AddTrait("fat")
                .AddTrait("zombie_spawner")
                .AddTrait("jaws")
                .Build();

            new ActorBuilder("zombieclawed", "zombie")
                .SetNameLocale("Clawed Zombie")
                .SetTexturePath("actors/zombieclawed/")
                .SetHealth(1300f)
                .SetDamage(240f)
                .SetSpeed(40f)
                .SetAttackSpeed(300f)
                .SetArmor(35f)
                .SetKnockback(6f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("giant")
                .AddTrait("strong")
                .AddTrait("zombie_spawner")
                .Build();

            new ActorBuilder("zombieballoon", "zombie")
                .SetNameLocale("Balloon Zombie")
                .SetTexturePath("actors/zombieballoon/")
                .SetHealth(100f)
                .SetDamage(10f)
                .SetSpeed(40f)
                .SetAttackSpeed(300f)
                .SetArmor(0f)
                .SetKnockback(6f)
                .SetActorSize(ActorSize.S17_Dragon)
                .AddTrait("zombie")
                .AddTrait("peaceful")
                .AddTrait("acid_blood")
                .AddTrait("acid_touch")
                .AddTrait("zombie_spawner")
                .AddTrait("flying")
                .Build();

            new ActorBuilder("zombieacidman", "zombie")
                .SetNameLocale("Acid Man Zombie")
                .SetTexturePath("actors/zombieacidman/")
                .SetHealth(300f)
                .SetDamage(204f)
                .SetSpeed(40f)
                .SetAttackSpeed(70f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("acid_blood")
                .AddTrait("acid_proof")
                .AddTrait("acid_touch")
                .Build();

            new ActorBuilder("zombiedemon", "zombie")
                .SetNameLocale("Demon Zombie")
                .SetTexturePath("actors/zombiedemon/")
                .SetHealth(717f)
                .SetDamage(10f)
                .SetSpeed(50f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("bloodRain")
                .Build();

            new ActorBuilder("zombiedoctor", "zombie")
                .SetNameLocale("Doctor Zombie")
                .SetTexturePath("actors/zombiedoctor/")
                .SetHealth(600f)
                .SetDamage(1f)
                .SetSpeed(40f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("mageSlayer")
                .Build();

            new ActorBuilder("zombiedruid", "zombie")
                .SetNameLocale("Druid Zombie")
                .SetTexturePath("actors/zombiedruid/")
                .SetHealth(600f)
                .SetDamage(1f)
                .SetSpeed(50f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("bloodRain")
                .Build();

            new ActorBuilder("zombieevilhorseman", "zombie")
                .SetNameLocale("Evil Horseman Zombie")
                .SetTexturePath("actors/zombieevilhorseman/")
                .SetHealth(600f)
                .SetDamage(1f)
                .SetSpeed(80f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("teleport")
                .AddTrait("lightning")
                .AddTrait("tornado")
                .AddTrait("bloodRain")
                .AddTrait("bloodRain")
                .AddTrait("fire")
                .Build();

            new ActorBuilder("zombieicelich", "zombie")
                .SetNameLocale("Ice Lich Zombie")
                .SetTexturePath("actors/zombieicelich/")
                .SetHealth(400f)
                .SetDamage(1f)
                .SetSpeed(40f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("icebolt")
                .AddTrait("teleport")
                .AddTrait("bloodRain")
                .AddTrait("shield")
                .Build();

            new ActorBuilder("zombiefairy", "zombie")
                .SetNameLocale("Fairy Zombie")
                .SetTexturePath("actors/zombiefairy/")
                .SetHealth(150f)
                .SetDamage(24f)
                .SetSpeed(50f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human)
                .AddTrait("zombie")
                .AddTrait("flying")
                .Build();

            new ActorBuilder("zombietarantula", "zombie")
                .SetNameLocale("Tarantula Zombie")
                .SetTexturePath("actors/zombietarantula/")
                .SetHealth(150f)
                .SetDamage(24f)
                .SetSpeed(40f)
                .SetAttackSpeed(110f)
                .SetArmor(0f)
                .SetKnockback(1f)
                .SetActorSize(ActorSize.S13_Human) 
                .AddTrait("zombie")
                .AddTrait("small")
                .AddTrait("venomous")

                .Build();

          }

        public static bool zombie_spawnerEffect(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (GetZombieCount() > 3000)
                return false;

            Actor actor = pTarget?.a;
            if (actor == null || actor.isRekt())
                return false;

            if (actor.hasTrait("zombie_spawner"))
            {
                float spawnRoll = UnityEngine.Random.Range(0f, 1f);
                if (spawnRoll > 0.85f)
                    return false;

                return ZombieTransformation.HandleZombieTransformation(actor, pTile);
            }

            if (actor.hasTrait("zombie"))
            {
                float chance = UnityEngine.Random.Range(0f, 1f);
                if (chance <= 0.45f)
                {

                    return ZombieTransformation.HandleZombieTransformation(actor, pTile);
                }
            }

            return false;
        }

        private static int GetZombieCount()
        {
            object worldToken = World.world;
            if (!ReferenceEquals(cachedZombieWorldToken, worldToken))
            {
                cachedZombieWorldToken = worldToken;
                nextZombieCountRefreshAt = 0f;
                cachedZombieCount = 0;
            }

            if (Time.time < nextZombieCountRefreshAt)
            {
                return cachedZombieCount;
            }

            nextZombieCountRefreshAt = Time.time + ZombieCountRefreshSeconds;
            cachedZombieCount = 0;

            if (MapBox.instance?.units == null)
            {
                return 0;
            }

            foreach (Actor actor in MapBox.instance.units)
            {
                if (actor != null && actor.hasTrait("zombie"))
                {
                    cachedZombieCount++;
                }
            }

            return cachedZombieCount;
        }

    }

public static class ZombieTransformation
{
    public static readonly Dictionary<string, List<string>> ZombieCandidateTable = new()
    {

        { "zombiespeed",       new List<string> { "zombietentacle", "zombiestalker", "zombieacidman", "zombieclawed", "zombietarantula" } },
        { "zombiespikes",      new List<string> { "zombieclawed", "zombiehulk", "zombietentacle", "zombieabomination" } },
        { "zombiepoison",      new List<string> { "zombietarantula", "zombieacidman", "zombiefairy", "zombiedruid" } },
        { "zombieacid",        new List<string> { "zombieacidman", "zombiemother", "zombieclawed", "zombieabomination" } },

        { "zombietentacle",    new List<string> { "zombiespeed", "zombiestalker", "zombiehulk", "zombieabomination", "zombiemother" } },
        { "zombiestalker",     new List<string> { "zombietentacle", "zombieabomination", "zombiehulk", "zombieclawed", "zombiemother" } },
        { "zombiefairy",       new List<string> { "zombieicelich", "zombiedruid", "zombiedemon", "zombiespeed" } },
        { "zombietarantula",   new List<string> { "zombiespeed", "zombieacidman", "zombieclawed", "zombiehulk" } },

        { "zombieacidman",     new List<string> { "zombieacid", "zombiemother", "zombieabomination", "zombieclawed" } },
        { "zombiefiremaniac",  new List<string> { "zombiedemon", "zombieevilhorseman", "zombiespikes", "zombiehulk" } },
        { "zombiedemon",       new List<string> { "zombieicelich", "zombieevilhorseman", "zombiestalker" } },
        { "zombiedoctor",      new List<string> { "zombieicelich", "zombieabomination", "zombiespikes" } },
        { "zombiedruid",       new List<string> { "zombiefairy", "zombieevilhorseman", "zombieicelich" } },
        { "zombieicelich",     new List<string> { "zombieevilhorseman", "zombiemother", "zombiedemon" } },

        { "zombiehulk",        new List<string> { "zombiemother", "zombieabomination", "zombieclawed" } },
        { "zombieclawed",      new List<string> { "zombiehulk", "zombiemother", "zombieabomination" } },
        { "zombieabomination", new List<string> { "zombiehulk", "zombiemother", "zombieevilhorseman" } },

        { "zombiemother",      new List<string> { "zombieevilhorseman", "zombieabomination", "zombieicelich" } },
        { "zombieballoon",     new List<string> { "zombieacidman", "zombiepoison", "zombiefairy" } },

        { "zombieevilhorseman",new List<string> { "zombieicelich", "zombiedemon" } },
    };

    public static readonly Dictionary<string, List<(string id, float weight)>> WeightedZombieTypes 
        = new()
    {
        {
            "zombie",
            new List<(string id, float weight)>
            {
                ("zombiespeed",       0.12f),
                ("zombiespikes",      0.12f),
                ("zombiepoison",      0.12f),
                ("zombieacid",        0.12f),

                ("zombietentacle",    0.10f),
                ("zombiestalker",     0.08f),
                ("zombiefiremaniac",  0.06f),
                ("zombieacidman",     0.06f),
                ("zombiespikes",      0.05f),
                ("zombieclawed",      0.05f),
                ("zombietarantula",   0.04f),
                ("zombiefairy",       0.04f),

                ("zombiehulk",        0.02f),
                ("zombieabomination", 0.02f),
                ("zombiemother",      0.01f),
                ("zombiedemon",       0.01f),
                ("zombiedoctor",      0.01f),
                ("zombiedruid",       0.01f),
                ("zombieicelich",     0.01f),
                ("zombieevilhorseman",0.01f)
            }
        }
    };

    public static readonly List<string> FallbackCandidates = new()
    {
        "zombiespeed",
        "zombietentacle",
        "zombiepoison"
    };

    public static bool HandleZombieTransformation(Actor actor, WorldTile pTile = null)
    {

        if (!IsZombie(actor))
        {

            return false;
        }

        string zombieType = DetermineZombieType(actor);

        List<string> candidates = GetCandidatesForZombieType(zombieType);

        if (candidates == null || candidates.Count == 0)
        {

            return false;
        }

        string newActorId = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        Actor transformed = World.world.units.createNewUnit(
            newActorId,
            actor.current_tile,
            pMiracleSpawn: false,
            0f,
            null,
            null,
            pSpawnWithItems: false
        );

        if (transformed == null)
            return false;

        EffectsLibrary.spawn("fx_spawn", transformed.current_tile);

        if (UnityEngine.Random.Range(0f, 1f) <= 0.65f)
        {
            ActionLibrary.removeUnit(actor);
            actor.setTransformed();
        }

        return true;
    }

    private static bool IsZombie(Actor actor)
    {

        return actor.asset.id.Contains("zombie");
    }

    private static string DetermineZombieType(Actor actor)
    {

        if (WeightedZombieTypes.ContainsKey(actor.asset.id))
            return WeightedRandom(WeightedZombieTypes[actor.asset.id]);

        return actor.asset.id;
    }

    private static List<string> GetCandidatesForZombieType(string zombieType)
    {
        if (ZombieCandidateTable.TryGetValue(zombieType, out var list))
            return list;

        return FallbackCandidates;
    }

    private static string WeightedRandom(List<(string id, float weight)> options)
    {
        float total = options.Sum(o => o.weight);
        float roll = UnityEngine.Random.Range(0f, total);
        float accum = 0f;

        foreach (var option in options)
        {
            accum += option.weight;
            if (roll <= accum)
                return option.id;
        }

        return options.Last().id; 
    }
}
}
