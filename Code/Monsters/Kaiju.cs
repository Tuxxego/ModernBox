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

namespace ModernBox
{
    internal sealed class ArchiveKaijuSpawnEntry
    {
        public string PowerId;
        public string ActorId;
        public string DisplayName;
        public string Description;
        public string IconPath;
    }

    class Kaiju
    {
        private sealed class KaijuDecisionFlags
        {
            public bool HasBossAttackAnimation;
            public bool HasAlpha;
            public bool HasSpecial;
            public bool HasMapwideAggro;
            public bool HasBurrow;
            public bool IsManagedKaiju;
        }

        private static readonly Dictionary<string, KaijuDecisionFlags> _kaijuDecisionFlagsCache = new Dictionary<string, KaijuDecisionFlags>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Sprite[]> _bossAttackFramesCache = new Dictionary<string, Sprite[]>(StringComparer.Ordinal);
        private static bool _kaijuInitQueued;
        private static bool _kaijuInitialized;

        public static void init(){
          if (_kaijuInitialized || _kaijuInitQueued)
          {
              return;
          }

          _kaijuInitQueued = true;
          if (IsWorldReadyForKaijuBootstrap())
          {
              CompleteKaijuInitialization();
              return;
          }

          if (Main.instance != null)
          {
              Main.instance.StartCoroutine(WaitForWorldAndInitializeKaijus());
              return;
          }

          _kaijuInitQueued = false;
          ModernBoxLogger.Error("[Kaiju] Main.instance was null before kaiju bootstrap could start.");
        }

        private static IEnumerator WaitForWorldAndInitializeKaijus()
        {
            const int maxFramesToWait = 1800;
            int waited = 0;
            while (waited < maxFramesToWait && !IsWorldReadyForKaijuBootstrap())
            {
                waited++;
                yield return null;
            }

            if (!IsWorldReadyForKaijuBootstrap())
            {
                _kaijuInitQueued = false;
                ModernBoxLogger.Error("[Kaiju] World was not ready in time. Kaiju bootstrap skipped to avoid startup crashes.");
                yield break;
            }

            CompleteKaijuInitialization();
        }

        private static bool IsWorldReadyForKaijuBootstrap()
        {
            return World.world != null
                && World.world.kingdoms_wild != null
                && AssetManager.actor_library != null
                && AssetManager.kingdoms != null;
        }

        private static void CompleteKaijuInitialization()
        {
            if (_kaijuInitialized)
            {
                return;
            }

            try
            {
                create_Kaijus();
                _kaijuInitialized = true;
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"[Kaiju] Bootstrap failed: {ex.Message}");
                ModernBoxLogger.Error($"[Kaiju] Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                _kaijuInitQueued = false;
            }
        }

          public static void create_Kaijus(){



    DecisionAsset alphaDecision = new DecisionAsset();
    alphaDecision.id = "kaiju_alpha_state_decision";
    alphaDecision.priority = NeuroLayer.Layer_2_Moderate;
    alphaDecision.path_icon = "ui/Icons/Godzilla";
    alphaDecision.cooldown = 1;
    alphaDecision.unique = true;
    alphaDecision.weight = 5f;
    alphaDecision.action_check_launch = delegate(Actor pActor)
    {
        return UpdateKaijuAlphaState(pActor);
    };
    AssetManager.decisions_library.add(alphaDecision);



    DecisionAsset specialAttackDecision = new DecisionAsset();
    specialAttackDecision.id = "kaiju_special_attack_decision";
    specialAttackDecision.priority = NeuroLayer.Layer_3_High;
    specialAttackDecision.path_icon = "ui/Icons/Godzilla";
    specialAttackDecision.cooldown = 3;
    specialAttackDecision.unique = true;
    specialAttackDecision.weight = 7f;
    specialAttackDecision.action_check_launch = delegate(Actor pActor)
    {
        return KaijuSpecialAttackDecisionEffect(pActor);
    };
    AssetManager.decisions_library.add(specialAttackDecision);


    DecisionAsset bossAttackAnimationDecision = new DecisionAsset();
    bossAttackAnimationDecision.id = "boss_attack_animation_decision";
    bossAttackAnimationDecision.priority = NeuroLayer.Layer_1_Low;
    bossAttackAnimationDecision.path_icon = "ui/Icons/sword";
    bossAttackAnimationDecision.cooldown = 1;
    bossAttackAnimationDecision.unique = true;
    bossAttackAnimationDecision.weight = 1f;
    bossAttackAnimationDecision.action_check_launch = delegate(Actor pActor)
    {
        return BossAttackAnimationDecisionEffect(pActor);
    };
    AssetManager.decisions_library.add(bossAttackAnimationDecision);

    DecisionAsset kaijuMapwideAggroOverrideDecision = new DecisionAsset();
    kaijuMapwideAggroOverrideDecision.id = "kaiju_mapwide_aggro_override_decision";
    kaijuMapwideAggroOverrideDecision.priority = NeuroLayer.Layer_3_High;
    kaijuMapwideAggroOverrideDecision.path_icon = "ui/Icons/Godzilla";
    kaijuMapwideAggroOverrideDecision.cooldown = 2;
    kaijuMapwideAggroOverrideDecision.unique = true;
    kaijuMapwideAggroOverrideDecision.weight = 8f;
    kaijuMapwideAggroOverrideDecision.action_check_launch = delegate(Actor pActor)
    {
        return KaijuMapwideAggroOverrideDecisionEffect(pActor);
    };
    AssetManager.decisions_library.add(kaijuMapwideAggroOverrideDecision);

    DecisionAsset kaijuCrabBurrowDecision = new DecisionAsset();
    kaijuCrabBurrowDecision.id = KaijuBurrowDecisionId;
    kaijuCrabBurrowDecision.priority = NeuroLayer.Layer_3_High;
    kaijuCrabBurrowDecision.path_icon = "ui/Icons/Godzilla";
    kaijuCrabBurrowDecision.cooldown = 1;
    kaijuCrabBurrowDecision.unique = true;
    kaijuCrabBurrowDecision.weight = 9f;
    kaijuCrabBurrowDecision.action_check_launch = delegate(Actor pActor)
    {
        return KaijuBurrowDecisionEffect(pActor);
    };
    AssetManager.decisions_library.add(kaijuCrabBurrowDecision);




EffectAsset Atomboom = new EffectAsset();
Atomboom.id = "Atomboom";
Atomboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
Atomboom.use_basic_prefab = true;
Atomboom.sorting_layer_id = "EffectsTop";
Atomboom.sprite_path = "effects/Atomboom";
Atomboom.draw_light_area = true;
AssetManager.effects_library.add(Atomboom);




EffectAsset AtomBeam_trail = new EffectAsset();
AtomBeam_trail.id = "AtomBeam_trail";
AtomBeam_trail.use_basic_prefab = true;
AtomBeam_trail.sorting_layer_id = "EffectsTop";
AtomBeam_trail.sprite_path = "effects/AtomBeam_trail_t";
AtomBeam_trail.draw_light_area = true;
AtomBeam_trail.show_on_mini_map = true;
AtomBeam_trail.limit = 15;
AssetManager.effects_library.add(AtomBeam_trail);


var atombeamterra = AssetManager.terraform.clone("atombeamterra", "bomb");
		atombeamterra.damage = 2000;
        atombeamterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("Gojira_wild");
		atombeamterra.explode_strength = 1;
		atombeamterra.transform_to_wasteland = false;
		atombeamterra.applies_to_high_flyers = true;
		atombeamterra.shake = true;
        AssetManager.terraform.add(atombeamterra);



            	ProjectileAsset AtomBeam = new ProjectileAsset();
            AtomBeam.id = "AtomBeam";
            AtomBeam.speed = 60f;
			AtomBeam.texture = "AtomBeam";
			AtomBeam.trail_effect_enabled = true;
            AtomBeam.trail_effect_id = "AtomBeam_trail";
            AtomBeam.trail_effect_scale = 0.25f;
			AtomBeam.trail_effect_timer = 0.1f;
			AtomBeam.texture_shadow = "shadows/projectiles/shadow_ball";
			AtomBeam.terraform_option = "atombeamterra";
			AtomBeam.draw_light_area = true;
			AtomBeam.terraform_range = 10;
			AtomBeam.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
			AtomBeam.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
			AtomBeam.end_effect = "Atomboom";
			AtomBeam.scale_start = 0.4f;
			AtomBeam.scale_target = 0.4f;
			AtomBeam.look_at_target = true;
          AtomBeam.can_be_left_on_ground = false;
          AtomBeam.can_be_blocked = false;
		  AtomBeam.world_actions = (AttackAction)Delegate.Combine(AtomBeam.world_actions, new AttackAction(ActionLibrary.burnTile));
          AssetManager.projectiles.add(AtomBeam);

          EquipmentAsset AtomBeam_attack = AssetManager.items.clone("AtomBeam_attack", "$range");
          AtomBeam_attack.has_locales = false;
          AtomBeam_attack.projectile = "AtomBeam";
          AtomBeam_attack.base_stats["projectiles"] = 1f;
          AtomBeam_attack.path_slash_animation = "effects/slashes/slash_cannonball";
          AtomBeam_attack.show_in_meta_editor = false;
          AtomBeam_attack.show_in_knowledge_window = false;
          AtomBeam_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame", "stun");

var Gojira_Boss = AssetManager.kingdoms.clone("Gojira_Boss", "$TEMPLATE_MOB$");
Gojira_Boss.concept = false;
Gojira_Boss.id = "Gojira_Boss";
Gojira_Boss.default_kingdom_color = new ColorAsset("#679ead");
Gojira_Boss.mobs = true;
Gojira_Boss.always_attack_each_other = true;
Gojira_Boss.force_look_all_chunks = true;
Gojira_Boss.units_always_looking_for_enemies = true;
Gojira_Boss.setIcon("ui/Icons/Godzilla");
Gojira_Boss.addTag("sliceable");
Gojira_Boss.addTag("Kaiju");
Gojira_Boss.addFriendlyTag("nature_creature");
Gojira_Boss.addEnemyTag("civ");
Gojira_Boss.addFriendlyTag("Gojira_wild");
Gojira_Boss.addFriendlyTag("crocodile");
Gojira_Boss.addFriendlyTag("civ_crocodile");
AssetManager.kingdoms.add(Gojira_Boss);
World.world.kingdoms_wild.newWildKingdom(Gojira_Boss);

var Gojira_wild = AssetManager.kingdoms.clone("Gojira_wild", "$TEMPLATE_ANIMAL$");
Gojira_wild.concept = false;
Gojira_wild.id = "Gojira_wild";
Gojira_wild.default_kingdom_color = new ColorAsset("#679ead");
Gojira_wild.units_always_looking_for_enemies = true;
Gojira_wild.force_look_all_chunks = true;
Gojira_wild.setIcon("actors/Kaiju/Gojira/main/walk_0");
Gojira_wild.addTag("sliceable");
Gojira_wild.addTag("nature_creature");
Gojira_wild.addFriendlyTag("nature_creature");
Gojira_wild.addTag("neutral_animals");
Gojira_wild.addTag("neutral");
Gojira_wild.addTag("Gojira_wild");
Gojira_wild.addTag("Kaiju");
Gojira_wild.addEnemyTag("civ");
Gojira_wild.addEnemyTag("Kaiju");
Gojira_wild.addFriendlyTag("crocodile");
Gojira_wild.addFriendlyTag("civ_crocodile");
AssetManager.kingdoms.add(Gojira_wild);
World.world.kingdoms_wild.newWildKingdom(Gojira_wild);


          var Gojira = AssetManager.actor_library.clone("Gojira", "$mob$");
          Gojira.is_humanoid = false;
	      Gojira.civ = false;
          Gojira.name_locale = "Bad Godzilla";
          Gojira.animation_speed_based_on_walk_speed = false;
          Gojira.has_avatar_prefab = false;
          Gojira.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Kaiju/Gojira/main/walk_0") };
          Gojira.has_override_avatar_frames = true;
          Gojira.inspect_avatar_scale = 1f;
          Gojira.inspect_avatar_offset_y = 6f;
          Gojira.shadow_texture = "unitShadow_6";
          Gojira.immune_to_slowness = true;
          Gojira.effect_damage = true;
          Gojira.unit_other = true;
          Gojira.collective_term = "group_den";
          Gojira.setSocialStructure("group_den", 10);
          Gojira.default_attack = "base_attack";
          Gojira.affected_by_dust = true;
          Gojira.inspect_children = true;
          Gojira.kingdom_id_civilization = string.Empty;
		  Gojira.build_order_template_id = string.Empty;
          Gojira.show_on_meta_layer = true;
          Gojira.show_in_knowledge_window = true;
		  Gojira.show_in_taxonomy_tooltip = true;
          Gojira.render_status_effects = true;
          Gojira.use_phenotypes = true;
          Gojira.death_animation_angle = true;
          Gojira.can_be_inspected = true;
          Gojira.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
          Gojira.kingdom_id_wild = "Gojira_wild";
          Gojira.update_z = true;
          Gojira.job = AssetLibrary<ActorAsset>.a<string>("attacker");
          Gojira.addDecision("kaiju_alpha_state_decision");
          Gojira.addDecision("kaiju_special_attack_decision");
          Gojira.addDecision("kaiju_mapwide_aggro_override_decision");
          Gojira.base_stats["lifespan"] = 200f;
          Gojira.base_stats["mass_2"] = 100000f;
          Gojira.base_stats["mass"] = 2000f;
          Gojira.base_stats["stamina"] = 500f;
          Gojira.base_stats["scale"] = 0.18f;
          Gojira.base_stats["size"] = 1.75f;
          Gojira.base_stats["health"] = 2000f;
          Gojira.base_stats["speed"] = 40f;
          Gojira.base_stats["armor"] = 20f;
          Gojira.base_stats["attack_speed"] = 0.4f;
          Gojira.base_stats["damage"] = 1000f;
          Gojira.base_stats["knockback"] = 4f;
          Gojira.base_stats["accuracy"] = 1f;
          Gojira.base_stats["targets"] = 10f;
          Gojira.base_stats["area_of_effect"] = 5f;
          Gojira.base_stats["range"] = 7f;
          Gojira.base_stats["critical_damage_multiplier"] = 10f;
          Gojira.base_stats["multiplier_supply_timer"] = 1f;
          Gojira.disable_jump_animation = true;
          Gojira.can_be_moved_by_powers = true;
          Gojira.actor_size = ActorSize.S16_Buffalo;
        Gojira.animation_walk = Kaiju.walk_0_5;
        Gojira.animation_idle = ActorAnimationSequences.walk_0;
		Gojira.animation_swim = Kaiju.swim_0_5;
          Gojira.can_flip = true;
          Gojira.check_flip = (BaseSimObject _, WorldTile _) => true;
          Gojira.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Gojira/", false);
          Gojira.icon = "Gojira_avatar";
          Gojira.die_in_lava = false;
          Gojira.visible_on_minimap = true;
          Gojira.experience_given = 20;
          Gojira.can_have_subspecies = true;
          Gojira.affected_by_dust = false;
          Gojira.special = true;
          Gojira.has_advanced_textures = false;
          Gojira.inspect_sex = true;
		  Gojira.inspect_show_species = true;
		  Gojira.inspect_generation = true;
          Gojira.needs_to_be_explored = false;
          Gojira.force_land_creature = true;
          Gojira.has_baby_form = true;
          Gojira.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
          Gojira.addSubspeciesTrait("stomach");
          Gojira.addSubspeciesTrait("reproduction_strategy_oviparity");
		Gojira.addSubspeciesTrait("egg_shell_plain");
        Gojira.addSubspeciesTrait("diet_xylophagy");
        Gojira.addSubspeciesTrait("diet_algivore");
        Gojira.addSubspeciesTrait("death_grow_mythril");
        Gojira.addSubspeciesTrait("bioproduct_gems");
      Gojira.addSubspeciesTrait("long_lifespan");
        Gojira.addSubspeciesTrait("reproduction_hermaphroditic");
       Gojira.addSubspeciesTrait("population_minimal");
       Gojira.addSubspeciesTrait("photosynthetic_skin");
		Gojira.addSubspeciesTrait("parental_care");
        Gojira.addSubspeciesTrait("heat_resistance");
        Gojira.kingdom_id_civilization = string.Empty;
        Gojira.build_order_template_id = string.Empty;
        Gojira.unit_other = true;
        Gojira.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
          Gojira.animal_breeding_close_units_limit = 4;
          Gojira.can_evolve_into_new_species = false;
		  Gojira.color_hex = "#679ead";
          Gojira.addTrait("tough");
          Gojira.addTrait("regeneration");
          Gojira.addTrait("fire_proof");
          Gojira.name_taxonomic_kingdom = "animalia";
		Gojira.name_taxonomic_phylum = "chordata";
		Gojira.name_taxonomic_class = "reptilia";
		Gojira.name_taxonomic_order = "Archosauria";
		Gojira.name_taxonomic_family = "Titanus";
		Gojira.name_taxonomic_genus = "Gojira";
        Gojira.addResource("adamantine", 2);
		Gojira.addResource("gold", 10);
        Gojira.source_meat = true;
Gojira.phenotypes_dict = new Dictionary<string, List<string>>() {
    { "default_color", new List<string> { "gray_black" } },
    { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
    { "biome_swamp", new List<string> { "swamp" } },
    { "biome_corrupted", new List<string> { "corrupted" } },
    { "biome_desert", new List<string> { "desert" } },
    { "biome_infernal", new List<string> { "infernal" } },
    { "biome_lemon", new List<string> { "lemon" } },
    { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
    { "biome_sand", new List<string> { "dark_orange", "wood" } },
    { "biome_singularity", new List<string> { "bright_violet" } },
    { "biome_garlic", new List<string> { "mid_gray" } },
    { "biome_maple", new List<string> { "dark_orange" } },
    { "biome_permafrost", new List<string> { "polar" } },
    { "biome_rocklands", new List<string> { "gray_black" } },
    { "biome_celestial", new List<string> { "bright_purple" } }
};



Gojira.phenotypes_list = new List<string> {
    "gray_black",
    "savanna",
    "dark_orange",
    "swamp",
    "corrupted",
    "desert",
    "infernal",
    "lemon",
    "pink_yellow_mushroom",
    "wood",
    "bright_violet",
    "mid_gray",
    "polar",
    "bright_purple"
};
 AssetManager.actor_library.add(Gojira);
            Localization.addLocalization(Gojira.name_locale, Gojira.name_locale);
            Localization.addLocalization("Gojira", Gojira.name_locale);
            Localization.addLocalization("spawnGojira", Gojira.name_locale);
            Localization.addLocalization("spawnGojira_description", "Rightful King of the Monsters");






            EffectAsset purpleAtomboom = new EffectAsset();
            purpleAtomboom.id = "purpleAtomboom";
            purpleAtomboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
            purpleAtomboom.use_basic_prefab = true;
            purpleAtomboom.sorting_layer_id = "EffectsTop";
            purpleAtomboom.sprite_path = "effects/purpleAtomboom";
            purpleAtomboom.draw_light_area = true;
            AssetManager.effects_library.add(purpleAtomboom);




            EffectAsset purpleAtomBeam_trail = new EffectAsset();
            purpleAtomBeam_trail.id = "purpleAtomBeam_trail";
            purpleAtomBeam_trail.use_basic_prefab = true;
            purpleAtomBeam_trail.sorting_layer_id = "EffectsTop";
            purpleAtomBeam_trail.sprite_path = "effects/purpleAtomBeam_trail_t";
            purpleAtomBeam_trail.draw_light_area = true;
            purpleAtomBeam_trail.show_on_mini_map = true;
            purpleAtomBeam_trail.limit = 15;
            AssetManager.effects_library.add(purpleAtomBeam_trail);


            var purpleAtombeamterra = AssetManager.terraform.clone("purpleAtombeamterra", "bomb");
            purpleAtombeamterra.damage = 4000;
            purpleAtombeamterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("MegaGojira_wild");
            purpleAtombeamterra.explode_strength = 1;
            purpleAtombeamterra.transform_to_wasteland = true;
            purpleAtombeamterra.applies_to_high_flyers = true;
            purpleAtombeamterra.shake = true;
            AssetManager.terraform.add(purpleAtombeamterra);



            ProjectileAsset purpleAtomBeam = new ProjectileAsset();
            purpleAtomBeam.id = "purpleAtomBeam";
            purpleAtomBeam.speed = 60f;
            purpleAtomBeam.texture = "purpleAtomBeam";
            purpleAtomBeam.trail_effect_enabled = true;
            purpleAtomBeam.trail_effect_id = "purpleAtomBeam_trail";
            purpleAtomBeam.trail_effect_scale = 0.25f;
            purpleAtomBeam.trail_effect_timer = 0.1f;
            purpleAtomBeam.texture_shadow = "shadows/projectiles/shadow_ball";
            purpleAtomBeam.terraform_option = "purpleAtombeamterra";
            purpleAtomBeam.draw_light_area = true;
            purpleAtomBeam.terraform_range = 20;
            purpleAtomBeam.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            purpleAtomBeam.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            purpleAtomBeam.end_effect = "purpleAtomboom";
            purpleAtomBeam.scale_start = 0.4f;
            purpleAtomBeam.scale_target = 0.4f;
            purpleAtomBeam.look_at_target = true;
            purpleAtomBeam.can_be_left_on_ground = false;
            purpleAtomBeam.can_be_blocked = false;
            purpleAtomBeam.world_actions = (AttackAction)Delegate.Combine(purpleAtomBeam.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(purpleAtomBeam);

            EquipmentAsset purpleAtomBeam_attack = AssetManager.items.clone("purpleAtomBeam_attack", "$range");
            purpleAtomBeam_attack.has_locales = false;
            purpleAtomBeam_attack.projectile = "purpleAtomBeam";
            purpleAtomBeam_attack.base_stats["projectiles"] = 1f;
            purpleAtomBeam_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            purpleAtomBeam_attack.show_in_meta_editor = false;
            purpleAtomBeam_attack.show_in_knowledge_window = false;
            purpleAtomBeam_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame", "stun");


            var MegaGojira_wild = AssetManager.kingdoms.clone("MegaGojira_wild", "$TEMPLATE_ANIMAL$");
            MegaGojira_wild.concept = false;
            MegaGojira_wild.id = "MegaGojira_wild";
            MegaGojira_wild.default_kingdom_color = new ColorAsset("#679ead");
            MegaGojira_wild.units_always_looking_for_enemies = true;
            MegaGojira_wild.force_look_all_chunks = true;
            MegaGojira_wild.setIcon("actors/Kaiju/MegaGojira/main/walk_0");
            MegaGojira_wild.addTag("sliceable");
            MegaGojira_wild.addTag("nature_creature");
            MegaGojira_wild.addFriendlyTag("nature_creature");
            MegaGojira_wild.addTag("neutral_animals");
            MegaGojira_wild.addTag("neutral");
            MegaGojira_wild.addTag("MegaGojira_wild");
            MegaGojira_wild.addTag("Kaiju");
            MegaGojira_wild.addEnemyTag("civ");
            MegaGojira_wild.addEnemyTag("Kaiju");
            MegaGojira_wild.addFriendlyTag("crocodile");
            MegaGojira_wild.addFriendlyTag("civ_crocodile");
            AssetManager.kingdoms.add(MegaGojira_wild);
            World.world.kingdoms_wild.newWildKingdom(MegaGojira_wild);


            var MegaGojira = AssetManager.actor_library.clone("MegaGojira", "$mob$");
            MegaGojira.is_humanoid = false;
            MegaGojira.civ = false;
            MegaGojira.name_locale = "Godzilla Earth";
            MegaGojira.animation_speed_based_on_walk_speed = false;
            MegaGojira.has_avatar_prefab = false;
            MegaGojira.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Kaiju/MegaGojira/main/walk_0") };
            MegaGojira.has_override_avatar_frames = true;
            MegaGojira.inspect_avatar_scale = 1f;
            MegaGojira.inspect_avatar_offset_y = 6f;
            MegaGojira.shadow_texture = "unitShadow_6";
            MegaGojira.immune_to_slowness = true;
            MegaGojira.effect_damage = true;
            MegaGojira.unit_other = true;
            MegaGojira.collective_term = "group_den";
            MegaGojira.setSocialStructure("group_den", 10);
            MegaGojira.default_attack = "base_attack";
            MegaGojira.affected_by_dust = true;
            MegaGojira.inspect_children = true;
            MegaGojira.kingdom_id_civilization = string.Empty;
            MegaGojira.build_order_template_id = string.Empty;
            MegaGojira.show_on_meta_layer = true;
            MegaGojira.show_in_knowledge_window = true;
            MegaGojira.show_in_taxonomy_tooltip = true;
            MegaGojira.render_status_effects = true;
            MegaGojira.use_phenotypes = true;
            MegaGojira.death_animation_angle = true;
            MegaGojira.can_be_inspected = true;
            MegaGojira.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
            MegaGojira.kingdom_id_wild = "MegaGojira_wild";
            MegaGojira.update_z = true;
            MegaGojira.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            MegaGojira.addDecision("kaiju_alpha_state_decision");
            MegaGojira.addDecision("kaiju_special_attack_decision");
            MegaGojira.addDecision("kaiju_mapwide_aggro_override_decision");
            MegaGojira.base_stats["lifespan"] = 200f;
            MegaGojira.base_stats["mass_2"] = 100000f;
            MegaGojira.base_stats["mass"] = 2000f;
            MegaGojira.base_stats["stamina"] = 500f;
            MegaGojira.base_stats["scale"] = 0.34f;
            MegaGojira.base_stats["size"] = 3.4f;
            MegaGojira.base_stats["health"] = 8000f;
            MegaGojira.base_stats["speed"] = 50f;
            MegaGojira.base_stats["armor"] = 20f;
            MegaGojira.base_stats["attack_speed"] = 0.2f;
            MegaGojira.base_stats["damage"] = 4000f;
            MegaGojira.base_stats["knockback"] = 8f;
            MegaGojira.base_stats["accuracy"] = 1f;
            MegaGojira.base_stats["targets"] = 50f;
            MegaGojira.base_stats["area_of_effect"] = 10f;
            MegaGojira.base_stats["range"] = 14f;
            MegaGojira.base_stats["critical_damage_multiplier"] = 10f;
            MegaGojira.base_stats["multiplier_supply_timer"] = 1f;
            MegaGojira.disable_jump_animation = true;
            MegaGojira.can_be_moved_by_powers = true;
            MegaGojira.actor_size = ActorSize.S16_Buffalo;
            MegaGojira.animation_walk = Kaiju.walk_0_5;
            MegaGojira.animation_idle = ActorAnimationSequences.walk_0;
            MegaGojira.animation_swim = Kaiju.swim_0_5;
            MegaGojira.can_flip = true;
            MegaGojira.check_flip = (BaseSimObject _, WorldTile _) => true;
            MegaGojira.texture_asset = new ActorTextureSubAsset("actors/Kaiju/MegaGojira/", false);
            MegaGojira.icon = "MegaGojira_avatar";
            MegaGojira.die_in_lava = false;
            MegaGojira.visible_on_minimap = true;
            MegaGojira.experience_given = 20;
            MegaGojira.can_have_subspecies = true;
            MegaGojira.affected_by_dust = false;
            MegaGojira.special = true;
            MegaGojira.has_advanced_textures = false;
            MegaGojira.inspect_sex = true;
            MegaGojira.inspect_show_species = true;
            MegaGojira.inspect_generation = true;
            MegaGojira.needs_to_be_explored = false;
            MegaGojira.force_land_creature = true;
            MegaGojira.has_baby_form = true;
            MegaGojira.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            MegaGojira.addSubspeciesTrait("stomach");
            MegaGojira.addSubspeciesTrait("reproduction_strategy_oviparity");
            MegaGojira.addSubspeciesTrait("egg_shell_plain");
            MegaGojira.addSubspeciesTrait("diet_xylophagy");
            MegaGojira.addSubspeciesTrait("diet_algivore");
            MegaGojira.addSubspeciesTrait("death_grow_mythril");
            MegaGojira.addSubspeciesTrait("bioproduct_gems");
            MegaGojira.addSubspeciesTrait("long_lifespan");
            MegaGojira.addSubspeciesTrait("reproduction_hermaphroditic");
            MegaGojira.addSubspeciesTrait("population_minimal");
            MegaGojira.addSubspeciesTrait("photosynthetic_skin");
            MegaGojira.addSubspeciesTrait("parental_care");
            MegaGojira.addSubspeciesTrait("heat_resistance");
            MegaGojira.kingdom_id_civilization = string.Empty;
            MegaGojira.build_order_template_id = string.Empty;
            MegaGojira.unit_other = true;
            MegaGojira.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            MegaGojira.animal_breeding_close_units_limit = 4;
            MegaGojira.can_evolve_into_new_species = false;
            MegaGojira.color_hex = "#679ead";
            MegaGojira.addTrait("tough");
            MegaGojira.addTrait("regeneration");
            MegaGojira.addTrait("fire_proof");
            MegaGojira.name_taxonomic_kingdom = "animalia";
            MegaGojira.name_taxonomic_phylum = "chordata";
            MegaGojira.name_taxonomic_class = "reptilia";
            MegaGojira.name_taxonomic_order = "Archosauria";
            MegaGojira.name_taxonomic_family = "Titanus";
            MegaGojira.name_taxonomic_genus = "MegaGojira";
            MegaGojira.addResource("adamantine", 2);
            MegaGojira.addResource("gold", 10);
            MegaGojira.source_meat = true;
            MegaGojira.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "gray_black" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };



            MegaGojira.phenotypes_list = new List<string> {
                "gray_black",
                "savanna",
                "dark_orange",
                "swamp",
                "corrupted",
                "desert",
                "infernal",
                "lemon",
                "pink_yellow_mushroom",
                "wood",
                "bright_violet",
                "mid_gray",
                "polar",
                "bright_purple"
            };
            AssetManager.actor_library.add(MegaGojira);
            Localization.addLocalization(MegaGojira.name_locale, MegaGojira.name_locale);
            Localization.addLocalization("MegaGojira", MegaGojira.name_locale);
            Localization.addLocalization("spawnMegaGojira", MegaGojira.name_locale);
            Localization.addLocalization("spawnMegaGojira_description", "Rightful King of the Monsters");





            EffectAsset spida_boom = new EffectAsset();
            spida_boom.id = "spida_boom";
            spida_boom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
            spida_boom.use_basic_prefab = true;
            spida_boom.sorting_layer_id = "EffectsTop";
            spida_boom.sprite_path = "effects/spida_boom";
            spida_boom.draw_light_area = true;
            AssetManager.effects_library.add(spida_boom);


            var spida_terra = AssetManager.terraform.clone("spida_terra", "bomb");
            spida_terra.damage = 1000;
            spida_terra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("Longlegder_wild");
            spida_terra.explode_strength = 1;
            spida_terra.transform_to_wasteland = false;
            spida_terra.applies_to_high_flyers = true;
            spida_terra.explode_and_set_random_fire = true;
            spida_terra.shake = true;
            AssetManager.terraform.add(spida_terra);


            ProjectileAsset spida = new ProjectileAsset();
            spida.id = "spida";
            spida.speed = 60f;
            spida.texture = "spida";
            spida.terraform_option = "spida_terra";
            spida.trail_effect_enabled = false;
            spida.texture_shadow = "shadows/projectiles/shadow_ball";
            spida.draw_light_area = true;
            spida.terraform_range = 10;
            spida.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            spida.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            spida.end_effect = "spida_boom";
            spida.scale_start = 0.4f;
            spida.scale_target = 0.4f;
            spida.look_at_target = true;
            spida.can_be_left_on_ground = false;
            spida.can_be_blocked = false;
            AssetManager.projectiles.add(spida);

            EquipmentAsset spida_attack = AssetManager.items.clone("spida_attack", "$range");
            spida_attack.has_locales = false;
            spida_attack.projectile = "spida";
            spida_attack.base_stats["projectiles"] = 1f;
            spida_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            spida_attack.show_in_meta_editor = false;
            spida_attack.show_in_knowledge_window = false;
            spida_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("slowness", "stun");

            var Longlegder_wild = AssetManager.kingdoms.clone("Longlegder_wild", "$TEMPLATE_ANIMAL$");
            Longlegder_wild.concept = false;
            Longlegder_wild.id = "Longlegder_wild";
            Longlegder_wild.default_kingdom_color = new ColorAsset("#679ead");
            Longlegder_wild.units_always_looking_for_enemies = true;
            Longlegder_wild.force_look_all_chunks = true;
            Longlegder_wild.setIcon("actors/Avatars/Longlegder_avatar");
            Longlegder_wild.addTag("sliceable");
            Longlegder_wild.addTag("nature_creature");
            Longlegder_wild.addFriendlyTag("nature_creature");
            Longlegder_wild.addTag("neutral_animals");
            Longlegder_wild.addTag("neutral");
            Longlegder_wild.addTag("Longlegder_wild");
            Longlegder_wild.addTag("Kaiju");
            Longlegder_wild.addEnemyTag("civ");
            Longlegder_wild.addEnemyTag("Kaiju");
            Longlegder_wild.addFriendlyTag("crocodile");
            Longlegder_wild.addFriendlyTag("civ_crocodile");
            AssetManager.kingdoms.add(Longlegder_wild);
            World.world.kingdoms_wild.newWildKingdom(Longlegder_wild);


            var Longlegder = AssetManager.actor_library.clone("Longlegder", "$mob$");
            Longlegder.is_humanoid = false;
            Longlegder.civ = false;
            Longlegder.name_locale = "Longlegder";
            Longlegder.animation_speed_based_on_walk_speed = false;
            Longlegder.has_avatar_prefab = false;
            Longlegder.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Longlegder_avatar") };
            Longlegder.has_override_avatar_frames = true;
            Longlegder.inspect_avatar_scale = 1f;
            Longlegder.inspect_avatar_offset_y = 6f;
            Longlegder.shadow_texture = "unitShadow_6";
            Longlegder.immune_to_slowness = true;
            Longlegder.effect_damage = true;
            Longlegder.unit_other = true;
            Longlegder.collective_term = "group_den";
            Longlegder.setSocialStructure("group_den", 10);
            Longlegder.default_attack = "jaws";
            Longlegder.affected_by_dust = true;
            Longlegder.inspect_children = true;
            Longlegder.kingdom_id_civilization = string.Empty;
            Longlegder.build_order_template_id = string.Empty;
            Longlegder.show_on_meta_layer = true;
            Longlegder.show_in_knowledge_window = true;
            Longlegder.show_in_taxonomy_tooltip = true;
            Longlegder.render_status_effects = true;
            Longlegder.use_phenotypes = true;
            Longlegder.death_animation_angle = true;
            Longlegder.can_be_inspected = true;
            Longlegder.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
            Longlegder.kingdom_id_wild = "Longlegder_wild";
            Longlegder.update_z = true;
            Longlegder.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Longlegder.addDecision("kaiju_alpha_state_decision");
            Longlegder.addDecision("kaiju_special_attack_decision");
            Longlegder.addDecision("kaiju_mapwide_aggro_override_decision");
            Longlegder.base_stats["lifespan"] = 200f;
            Longlegder.base_stats["mass_2"] = 100000f;
            Longlegder.base_stats["mass"] = 2000f;
            Longlegder.base_stats["stamina"] = 500f;
            Longlegder.base_stats["scale"] = 0.1f;
            Longlegder.base_stats["size"] = 1f;
            Longlegder.base_stats["health"] = 2000f;
            Longlegder.base_stats["speed"] = 60f;
            Longlegder.base_stats["armor"] = 20f;
            Longlegder.base_stats["attack_speed"] = 1f;
            Longlegder.base_stats["damage"] = 1000f;
            Longlegder.base_stats["knockback"] = 4f;
            Longlegder.base_stats["accuracy"] = 1f;
            Longlegder.base_stats["targets"] = 10f;
            Longlegder.base_stats["area_of_effect"] = 5f;
            Longlegder.base_stats["range"] = 7f;
            Longlegder.base_stats["critical_damage_multiplier"] = 10f;
            Longlegder.base_stats["multiplier_supply_timer"] = 1f;
            Longlegder.disable_jump_animation = true;
            Longlegder.can_be_moved_by_powers = true;
            Longlegder.actor_size = ActorSize.S16_Buffalo;
            Longlegder.animation_walk = ActorAnimationSequences.walk_0_3;
            Longlegder.animation_swim = ActorAnimationSequences.swim_0_3;
            Longlegder.animation_idle = ActorAnimationSequences.walk_0;
            Longlegder.can_flip = true;
            Longlegder.check_flip = (BaseSimObject _, WorldTile _) => true;
            Longlegder.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Longlegder/", false);
            Longlegder.icon = "Longlegder_avatar";
            Longlegder.die_in_lava = false;
            Longlegder.visible_on_minimap = true;
            Longlegder.experience_given = 20;
            Longlegder.can_have_subspecies = true;
            Longlegder.affected_by_dust = false;
            Longlegder.special = true;
            Longlegder.has_advanced_textures = false;
            Longlegder.inspect_sex = true;
            Longlegder.inspect_show_species = true;
            Longlegder.inspect_generation = true;
            Longlegder.needs_to_be_explored = false;
            Longlegder.force_land_creature = true;
            Longlegder.has_baby_form = true;
            Longlegder.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            Longlegder.addSubspeciesTrait("stomach");
            Longlegder.addSubspeciesTrait("reproduction_strategy_oviparity");
            Longlegder.addSubspeciesTrait("egg_cocoon");
            Longlegder.addSubspeciesTrait("diet_insectivore");
            Longlegder.addSubspeciesTrait("diet_hematophagy");
            Longlegder.addSubspeciesTrait("death_grow_mythril");
            Longlegder.addSubspeciesTrait("diet_cannibalism");
            Longlegder.addSubspeciesTrait("long_lifespan");
            Longlegder.addSubspeciesTrait("reproduction_hermaphroditic");
            Longlegder.addSubspeciesTrait("population_minimal");
            Longlegder.addSubspeciesTrait("parental_care");
            Longlegder.addSubspeciesTrait("heat_resistance");
            Longlegder.kingdom_id_civilization = string.Empty;
            Longlegder.build_order_template_id = string.Empty;
            Longlegder.unit_other = true;
            Longlegder.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            Longlegder.animal_breeding_close_units_limit = 4;
            Longlegder.can_evolve_into_new_species = false;
            Longlegder.color_hex = "#679ead";
            Longlegder.addTrait("hard_skin");
            Longlegder.addTrait("slow");
            Longlegder.addTrait("regeneration");
            Longlegder.addTrait("poison_immune");
            Longlegder.addTrait("venomous");
            Longlegder.addTrait("weightless");
            Longlegder.addTrait("fire_proof");
            Longlegder.name_taxonomic_kingdom = "animalia";
            Longlegder.name_taxonomic_phylum = "chordata";
            Longlegder.name_taxonomic_class = "reptilia";
            Longlegder.name_taxonomic_order = "Archosauria";
            Longlegder.name_taxonomic_family = "Titanus";
            Longlegder.name_taxonomic_genus = "Longlegder";
            Longlegder.addResource("adamantine", 2);
            Longlegder.addResource("gold", 10);
            Longlegder.source_meat = true;
            Longlegder.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "black_blue" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };

            Longlegder.phenotypes_list = new List<string> {
                "black_blue",
                "soil"
            };
            AssetManager.actor_library.add(Longlegder);
            Localization.addLocalization(Longlegder.name_locale, Longlegder.name_locale);
            Localization.addLocalization("Longlegder", Longlegder.name_locale);
            Localization.addLocalization("spawnLonglegder", Longlegder.name_locale);
            Localization.addLocalization("spawnLonglegder_description", "Rightful King of the Monsters");











            EffectAsset firepower_boom = new EffectAsset();
            firepower_boom.id = "firepower_boom";
            firepower_boom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
            firepower_boom.use_basic_prefab = true;
            firepower_boom.sorting_layer_id = "EffectsTop";
            firepower_boom.sprite_path = "effects/firepower_boom";
            firepower_boom.draw_light_area = true;
            AssetManager.effects_library.add(firepower_boom);


            var FieryShockterra = AssetManager.terraform.clone("FieryShockterra", "bomb");
            FieryShockterra.damage = 2000;
            FieryShockterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("Rodanix_wild");
            FieryShockterra.explode_strength = 1;
            FieryShockterra.transform_to_wasteland = false;
            FieryShockterra.applies_to_high_flyers = true;
            FieryShockterra.set_fire = true;
            FieryShockterra.apply_force = true;
            FieryShockterra.add_burned = true;
            FieryShockterra.force_power = 2.5f;
            FieryShockterra.attack_type = AttackType.Fire;
            AssetManager.terraform.add(FieryShockterra);


            ProjectileAsset FieryShock = new ProjectileAsset();
            FieryShock.id = "FieryShock";
            FieryShock.speed = 60f;
            FieryShock.texture = "FieryShock";
            FieryShock.trail_effect_enabled = false;
            FieryShock.texture_shadow = "shadows/projectiles/shadow_ball";
            FieryShock.terraform_option = "FieryShockterra";
            FieryShock.draw_light_area = true;
            FieryShock.terraform_range = 15;
            FieryShock.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            FieryShock.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            FieryShock.end_effect = "firepower_boom";
            FieryShock.scale_start = 0.4f;
            FieryShock.scale_target = 0.4f;
            FieryShock.look_at_target = true;
            FieryShock.can_be_left_on_ground = false;
            FieryShock.can_be_blocked = false;
            FieryShock.world_actions = (AttackAction)Delegate.Combine(FieryShock.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(FieryShock);


            EquipmentAsset Fiery_attack = AssetManager.items.clone("Fiery_attack", "$range");
            Fiery_attack.has_locales = false;
            Fiery_attack.projectile = "FieryShock";
            Fiery_attack.base_stats["projectiles"] = 1f;
            Fiery_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            Fiery_attack.show_in_meta_editor = false;
            Fiery_attack.show_in_knowledge_window = false;
            Fiery_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame", "stun");


            var Rodanix_wild = AssetManager.kingdoms.clone("Rodanix_wild", "$TEMPLATE_ANIMAL$");
            Rodanix_wild.concept = false;
            Rodanix_wild.id = "Rodanix_wild";
            Rodanix_wild.default_kingdom_color = new ColorAsset("#679ead");
            Rodanix_wild.units_always_looking_for_enemies = true;
            Rodanix_wild.force_look_all_chunks = true;
            Rodanix_wild.setIcon("actors/Avatars/Rodanix_avatar");
            Rodanix_wild.addTag("sliceable");
            Rodanix_wild.addTag("nature_creature");
            Rodanix_wild.addFriendlyTag("nature_creature");
            Rodanix_wild.addTag("neutral_animals");
            Rodanix_wild.addTag("neutral");
            Rodanix_wild.addTag("Rodanix_wild");
            Rodanix_wild.addTag("Kaiju");
            Rodanix_wild.addEnemyTag("civ");
            Rodanix_wild.addEnemyTag("Kaiju");
            Rodanix_wild.addFriendlyTag("crocodile");
            Rodanix_wild.addFriendlyTag("civ_crocodile");
            AssetManager.kingdoms.add(Rodanix_wild);
            World.world.kingdoms_wild.newWildKingdom(Rodanix_wild);


            var Rodanix = AssetManager.actor_library.clone("Rodanix", "$mob$");
            Rodanix.is_humanoid = false;
            Rodanix.civ = false;
            Rodanix.name_locale = "Rodanix";
            Rodanix.animation_speed_based_on_walk_speed = false;
            Rodanix.has_avatar_prefab = false;
            Rodanix.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Rodanix_avatar") };
            Rodanix.has_override_avatar_frames = true;
            Rodanix.inspect_avatar_scale = 1f;
            Rodanix.inspect_avatar_offset_y = 6f;
            Rodanix.shadow_texture = "unitShadow_6";
            Rodanix.immune_to_slowness = true;
            Rodanix.effect_damage = true;
            Rodanix.unit_other = true;
            Rodanix.collective_term = "group_den";
            Rodanix.setSocialStructure("group_den", 10);
            Rodanix.default_attack = "base_attack";
            Rodanix.affected_by_dust = true;
            Rodanix.inspect_children = true;
            Rodanix.kingdom_id_civilization = string.Empty;
            Rodanix.build_order_template_id = string.Empty;
            Rodanix.show_on_meta_layer = true;
            Rodanix.show_in_knowledge_window = true;
            Rodanix.show_in_taxonomy_tooltip = true;
            Rodanix.render_status_effects = true;
            Rodanix.use_phenotypes = true;
            Rodanix.death_animation_angle = true;
            Rodanix.can_be_inspected = true;
            Rodanix.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
            Rodanix.kingdom_id_wild = "Rodanix_wild";
            Rodanix.update_z = true;
            Rodanix.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Rodanix.addDecision("kaiju_alpha_state_decision");
            Rodanix.addDecision("kaiju_special_attack_decision");
            Rodanix.addDecision("kaiju_mapwide_aggro_override_decision");
            Rodanix.base_stats["lifespan"] = 200f;
            Rodanix.base_stats["mass_2"] = 100000f;
            Rodanix.base_stats["mass"] = 2000f;
            Rodanix.base_stats["stamina"] = 500f;
            Rodanix.base_stats["scale"] = 0.1f;
            Rodanix.base_stats["size"] = 1f;
            Rodanix.base_stats["health"] = 2000f;
            Rodanix.base_stats["speed"] = 70f;
            Rodanix.base_stats["armor"] = 20f;
            Rodanix.base_stats["attack_speed"] = 0.4f;
            Rodanix.base_stats["damage"] = 1000f;
            Rodanix.base_stats["knockback"] = 4f;
            Rodanix.base_stats["accuracy"] = 1f;
            Rodanix.base_stats["targets"] = 10f;
            Rodanix.base_stats["area_of_effect"] = 5f;
            Rodanix.base_stats["range"] = 7f;
            Rodanix.base_stats["critical_damage_multiplier"] = 10f;
            Rodanix.base_stats["multiplier_supply_timer"] = 1f;
            Rodanix.disable_jump_animation = true;
            Rodanix.can_be_moved_by_powers = true;
            Rodanix.actor_size = ActorSize.S16_Buffalo;
            Rodanix.animation_walk = ActorAnimationSequences.walk_0_2;
            Rodanix.animation_idle = Kaiju.idle_0_6;
            Rodanix.animation_swim = ActorAnimationSequences.walk_0_2;
            Rodanix.can_flip = true;
            Rodanix.check_flip = (BaseSimObject _, WorldTile _) => true;
            Rodanix.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Rodanix/", false);
            Rodanix.icon = "Rodanix_avatar";
            Rodanix.die_in_lava = false;
            Rodanix.visible_on_minimap = true;
            Rodanix.experience_given = 20;
            Rodanix.can_have_subspecies = true;
            Rodanix.affected_by_dust = false;
            Rodanix.special = true;
            Rodanix.has_advanced_textures = false;
            Rodanix.inspect_sex = true;
            Rodanix.inspect_show_species = true;
            Rodanix.inspect_generation = true;
            Rodanix.needs_to_be_explored = false;
            Rodanix.force_land_creature = true;
            Rodanix.has_baby_form = true;
            Rodanix.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            Rodanix.addSubspeciesTrait("stomach");
            Rodanix.addSubspeciesTrait("reproduction_strategy_oviparity");
            Rodanix.addSubspeciesTrait("egg_flames");
            Rodanix.addSubspeciesTrait("diet_lithotroph");
            Rodanix.addSubspeciesTrait("diet_carnivore");
            Rodanix.addSubspeciesTrait("fenix_born");
            Rodanix.addSubspeciesTrait("death_grow_mythril");
            Rodanix.addSubspeciesTrait("bioproduct_gems");
            Rodanix.addSubspeciesTrait("long_lifespan");
            Rodanix.addSubspeciesTrait("reproduction_hermaphroditic");
            Rodanix.addSubspeciesTrait("population_minimal");
            Rodanix.addSubspeciesTrait("photosynthetic_skin");
            Rodanix.addSubspeciesTrait("parental_care");
            Rodanix.addSubspeciesTrait("heat_resistance");
            Rodanix.addSubspeciesTrait("gift_of_fire");
            Rodanix.addSubspeciesTrait("gift_of_air");
            Rodanix.addSubspeciesTrait("hovering");
            Rodanix.addSubspeciesTrait("spicy_kids");
            Rodanix.kingdom_id_civilization = string.Empty;
            Rodanix.build_order_template_id = string.Empty;
            Rodanix.unit_other = true;
            Rodanix.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            Rodanix.animal_breeding_close_units_limit = 4;
            Rodanix.can_evolve_into_new_species = false;
            Rodanix.color_hex = "#679ead";
            Rodanix.addTrait("tough");
            Rodanix.addTrait("regeneration");
            Rodanix.addTrait("fire_proof");
            Rodanix.name_taxonomic_kingdom = "animalia";
            Rodanix.name_taxonomic_phylum = "chordata";
            Rodanix.name_taxonomic_class = "reptilia";
            Rodanix.name_taxonomic_order = "Archosauria";
            Rodanix.name_taxonomic_family = "Titanus";
            Rodanix.name_taxonomic_genus = "Rodanix";
            Rodanix.addResource("adamantine", 2);
            Rodanix.addResource("gold", 10);
            Rodanix.source_meat = true;
            Rodanix.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "wood" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };



            Rodanix.phenotypes_list = new List<string> {
                "wood",
                "savanna",
                "dark_orange",
                "swamp",
                "corrupted",
                "desert",
                "infernal",
                "lemon",
                "pink_yellow_mushroom",
                "wood",
                "bright_violet",
                "mid_gray",
                "polar",
                "bright_purple"
            };
            AssetManager.actor_library.add(Rodanix);
            Localization.addLocalization(Rodanix.name_locale, Rodanix.name_locale);
            Localization.addLocalization("Rodanix", Rodanix.name_locale);
            Localization.addLocalization("spawnRodanix", Rodanix.name_locale);
            Localization.addLocalization("spawnRodanix_description", "Rightful King of the Monsters");



            var ElectroBeamterra = AssetManager.terraform.clone("ElectroBeamterra", "bomb");
            ElectroBeamterra.damage = 2000;
            ElectroBeamterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("Invaderax_wild");
            ElectroBeamterra.explode_strength = 1;
            ElectroBeamterra.transform_to_wasteland = false;
            ElectroBeamterra.applies_to_high_flyers = true;
            ElectroBeamterra.shake = true;
            AssetManager.terraform.add(ElectroBeamterra);

            EffectAsset ElectroBeam_trail = new EffectAsset();
            ElectroBeam_trail.id = "ElectroBeam_trail";
            ElectroBeam_trail.use_basic_prefab = true;
            ElectroBeam_trail.sorting_layer_id = "EffectsTop";
            ElectroBeam_trail.sprite_path = "effects/ElectroBeam_trail";
            ElectroBeam_trail.draw_light_area = true;
            ElectroBeam_trail.show_on_mini_map = true;
            ElectroBeam_trail.limit = 15;
            AssetManager.effects_library.add(ElectroBeam_trail);

            ProjectileAsset ElectroBeam = new ProjectileAsset();
            ElectroBeam.id = "ElectroBeam";
            ElectroBeam.speed = 60f;
            ElectroBeam.texture = "ElectroBeam";
            ElectroBeam.trail_effect_enabled = true;
            ElectroBeam.trail_effect_id = "ElectroBeam_trail";
            ElectroBeam.trail_effect_scale = 0.25f;
            ElectroBeam.trail_effect_timer = 0.1f;
            ElectroBeam.texture_shadow = "shadows/projectiles/shadow_ball";
            ElectroBeam.terraform_option = "ElectroBeamterra";
            ElectroBeam.draw_light_area = true;
            ElectroBeam.terraform_range = 10;
            ElectroBeam.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            ElectroBeam.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            ElectroBeam.end_effect = "fx_lightning_big";
            ElectroBeam.scale_start = 0.4f;
            ElectroBeam.scale_target = 0.4f;
            ElectroBeam.look_at_target = true;
            ElectroBeam.can_be_left_on_ground = false;
            ElectroBeam.can_be_blocked = false;
            ElectroBeam.world_actions = (AttackAction)Delegate.Combine(ElectroBeam.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(ElectroBeam);


            EquipmentAsset Ghido_attack = AssetManager.items.clone("Ghido_attack", "$range");
            Ghido_attack.has_locales = false;
            Ghido_attack.projectile = "ElectroBeam";
            Ghido_attack.base_stats["projectiles"] = 1f;
            Ghido_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            Ghido_attack.show_in_meta_editor = false;
            Ghido_attack.show_in_knowledge_window = false;
            Ghido_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame", "stun");


            var Invaderax_wild = AssetManager.kingdoms.clone("Invaderax_wild", "$TEMPLATE_ANIMAL$");
            Invaderax_wild.concept = false;
            Invaderax_wild.id = "Invaderax_wild";
            Invaderax_wild.default_kingdom_color = new ColorAsset("#679ead");
            Invaderax_wild.units_always_looking_for_enemies = true;
            Invaderax_wild.force_look_all_chunks = true;
            Invaderax_wild.setIcon("actors/Kaiju/Invaderax/main/walk_0");
            Invaderax_wild.addTag("sliceable");
            Invaderax_wild.addTag("nature_creature");
            Invaderax_wild.addFriendlyTag("nature_creature");
            Invaderax_wild.addTag("neutral_animals");
            Invaderax_wild.addTag("neutral");
            Invaderax_wild.addTag("Invaderax_wild");
            Invaderax_wild.addTag("Kaiju");
            Invaderax_wild.addEnemyTag("civ");
            Invaderax_wild.addEnemyTag("Kaiju");
            Invaderax_wild.addFriendlyTag("crocodile");
            Invaderax_wild.addFriendlyTag("civ_crocodile");
            AssetManager.kingdoms.add(Invaderax_wild);
            World.world.kingdoms_wild.newWildKingdom(Invaderax_wild);


            var Invaderax = AssetManager.actor_library.clone("Invaderax", "$mob$");
            Invaderax.is_humanoid = false;
            Invaderax.civ = false;
            Invaderax.name_locale = "Ghidorah";
            Invaderax.animation_speed_based_on_walk_speed = false;
            Invaderax.has_avatar_prefab = false;
            Invaderax.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Kaiju/Invaderax/main/walk_0") };
            Invaderax.has_override_avatar_frames = true;
            Invaderax.inspect_avatar_scale = 1f;
            Invaderax.inspect_avatar_offset_y = 6f;
            Invaderax.shadow_texture = "unitShadow_6";
            Invaderax.immune_to_slowness = true;
            Invaderax.effect_damage = true;
            Invaderax.unit_other = true;
            Invaderax.collective_term = "group_den";
            Invaderax.setSocialStructure("group_den", 10);
            Invaderax.default_attack = "jaws";
            Invaderax.affected_by_dust = true;
            Invaderax.inspect_children = true;
            Invaderax.kingdom_id_civilization = string.Empty;
            Invaderax.build_order_template_id = string.Empty;
            Invaderax.show_on_meta_layer = true;
            Invaderax.show_in_knowledge_window = true;
            Invaderax.show_in_taxonomy_tooltip = true;
            Invaderax.render_status_effects = true;
            Invaderax.use_phenotypes = true;
            Invaderax.death_animation_angle = true;
            Invaderax.can_be_inspected = true;
            Invaderax.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
            Invaderax.kingdom_id_wild = "Invaderax_wild";
            Invaderax.update_z = true;
            Invaderax.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Invaderax.addDecision("kaiju_alpha_state_decision");
            Invaderax.addDecision("kaiju_special_attack_decision");
            Invaderax.addDecision("kaiju_mapwide_aggro_override_decision");
            Invaderax.base_stats["lifespan"] = 200f;
            Invaderax.base_stats["mass_2"] = 100000f;
            Invaderax.base_stats["mass"] = 2000f;
            Invaderax.base_stats["stamina"] = 500f;
            Invaderax.base_stats["scale"] = 0.22f;
            Invaderax.base_stats["size"] = 2.2f;
            Invaderax.base_stats["health"] = 2000f;
            Invaderax.base_stats["speed"] = 70f;
            Invaderax.base_stats["armor"] = 40f;
            Invaderax.base_stats["attack_speed"] = 3f;
            Invaderax.base_stats["damage"] = 1000f;
            Invaderax.base_stats["knockback"] = 4f;
            Invaderax.base_stats["accuracy"] = 1f;
            Invaderax.base_stats["targets"] = 10f;
            Invaderax.base_stats["area_of_effect"] = 5f;
            Invaderax.base_stats["range"] = 5f;
            Invaderax.base_stats["critical_damage_multiplier"] = 10f;
            Invaderax.base_stats["multiplier_supply_timer"] = 1f;
            Invaderax.disable_jump_animation = true;
            Invaderax.can_be_moved_by_powers = true;
            Invaderax.actor_size = ActorSize.S16_Buffalo;
            Invaderax.animation_walk = ActorAnimationSequences.walk_0_5;
            Invaderax.animation_idle = Kaiju.idle_0;
            Invaderax.animation_swim = ActorAnimationSequences.walk_0_5;
            Invaderax.can_flip = true;
            Invaderax.check_flip = (BaseSimObject _, WorldTile _) => true;
            Invaderax.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Invaderax/", false);
            Invaderax.icon = "Invaderax_avatar";
            Invaderax.die_in_lava = false;
            Invaderax.visible_on_minimap = true;
            Invaderax.experience_given = 20;
            Invaderax.can_have_subspecies = true;
            Invaderax.affected_by_dust = false;
            Invaderax.special = true;
            Invaderax.has_advanced_textures = false;
            Invaderax.inspect_sex = true;
            Invaderax.inspect_show_species = true;
            Invaderax.inspect_generation = true;
            Invaderax.needs_to_be_explored = false;
            Invaderax.force_land_creature = true;
            Invaderax.has_baby_form = true;
            Invaderax.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            Invaderax.addSubspeciesTrait("stomach");
            Invaderax.addSubspeciesTrait("reproduction_strategy_oviparity");
            Invaderax.addSubspeciesTrait("egg_crystal");
            Invaderax.addSubspeciesTrait("diet_carnivore");
            Invaderax.addSubspeciesTrait("diet_cannibalism");
            Invaderax.addSubspeciesTrait("diet_hematophagy");
            Invaderax.addSubspeciesTrait("long_lifespan");
            Invaderax.addSubspeciesTrait("reproduction_hermaphroditic");
            Invaderax.addSubspeciesTrait("population_minimal");
            Invaderax.addSubspeciesTrait("photosynthetic_skin");
            Invaderax.addSubspeciesTrait("parental_care");
            Invaderax.addSubspeciesTrait("heat_resistance");
            Invaderax.addSubspeciesTrait("gift_of_thunder");
            Invaderax.addSubspeciesTrait("gift_of_air");
            Invaderax.addSubspeciesTrait("hovering");
            Invaderax.addSubspeciesTrait("bioproduct_gold");
            Invaderax.addSubspeciesTrait("voracious");
            Invaderax.addSubspeciesTrait("aggressive");
            Invaderax.addSubspeciesTrait("hotheaded");
            Invaderax.kingdom_id_civilization = string.Empty;
            Invaderax.build_order_template_id = string.Empty;
            Invaderax.unit_other = true;
            Invaderax.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            Invaderax.animal_breeding_close_units_limit = 4;
            Invaderax.can_evolve_into_new_species = false;
            Invaderax.color_hex = "#679ead";
            Invaderax.addTrait("tough");
            Invaderax.addTrait("regeneration");
            Invaderax.addTrait("fire_proof");
            Invaderax.addTrait("flesh_eater");
            Invaderax.addTrait("evil");
            Invaderax.addTrait("gluttonous");
            Invaderax.name_taxonomic_kingdom = "animalia";
            Invaderax.name_taxonomic_phylum = "chordata";
            Invaderax.name_taxonomic_class = "reptilia";
            Invaderax.name_taxonomic_order = "Archosauria";
            Invaderax.name_taxonomic_family = "Titanus";
            Invaderax.name_taxonomic_genus = "Invaderax";
            Invaderax.addResource("adamantine", 2);
            Invaderax.addResource("gold", 10);
            Invaderax.source_meat = true;
            Invaderax.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "dark_yellow" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };



            Invaderax.phenotypes_list = new List<string> {
                "dark_yellow",
                "savanna",
                "dark_orange",
                "swamp",
                "corrupted",
                "desert",
                "infernal",
                "lemon",
                "pink_yellow_mushroom",
                "wood",
                "bright_violet",
                "mid_gray",
                "polar",
                "bright_purple"
            };
            AssetManager.actor_library.add(Invaderax);
            Localization.addLocalization(Invaderax.name_locale, Invaderax.name_locale);
            Localization.addLocalization("Invaderax", Invaderax.name_locale);
            Localization.addLocalization("spawnInvaderax", Invaderax.name_locale);
            Localization.addLocalization("spawnInvaderax_description", "Rightful King of the Monsters");






            EffectAsset SmellySplash = new EffectAsset();
            SmellySplash.id = "SmellySplash";
            SmellySplash.use_basic_prefab = true;
            SmellySplash.sorting_layer_id = "EffectsTop";
            SmellySplash.sprite_path = "effects/SmellySplash";
            SmellySplash.draw_light_area = false;
            AssetManager.effects_library.add(SmellySplash);



            var BigBigMassiveBoulderterra = AssetManager.terraform.clone("BigBigMassiveBoulderterra", "flash");
            BigBigMassiveBoulderterra.damage = 1000;
            BigBigMassiveBoulderterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("PanKong_wild");
            BigBigMassiveBoulderterra.flash = false;
            BigBigMassiveBoulderterra.explode_strength = 1;
            BigBigMassiveBoulderterra.explode_and_set_random_fire = true;
            BigBigMassiveBoulderterra.explode_tile = true;
            BigBigMassiveBoulderterra.explosion_pixel_effect = true;
            BigBigMassiveBoulderterra.remove_tornado = true;
            BigBigMassiveBoulderterra.apply_force = true;
            BigBigMassiveBoulderterra.damage_buildings = true;
            BigBigMassiveBoulderterra.transform_to_wasteland = false;
            BigBigMassiveBoulderterra.applies_to_high_flyers = true;
            BigBigMassiveBoulderterra.shake = true;
            AssetManager.terraform.add(BigBigMassiveBoulderterra);



            ProjectileAsset BigBigMassiveBoulder = new ProjectileAsset();
            BigBigMassiveBoulder.id = "BigBigMassiveBoulder";
            BigBigMassiveBoulder.speed = 60f;
            BigBigMassiveBoulder.texture = "BigBigMassiveBoulder";
            BigBigMassiveBoulder.texture_shadow = "shadows/projectiles/shadow_ball";
            BigBigMassiveBoulder.terraform_option = "BigBigMassiveBoulderterra";
            BigBigMassiveBoulder.draw_light_area = true;
            BigBigMassiveBoulder.terraform_range = 10;
            BigBigMassiveBoulder.sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow";
            BigBigMassiveBoulder.sound_impact = "event:/SFX/WEAPONS/WeaponRockLand";
            BigBigMassiveBoulder.end_effect = "SmellySplash";
            BigBigMassiveBoulder.scale_start = 0.2f;
            BigBigMassiveBoulder.scale_target = 0.2f;
            BigBigMassiveBoulder.look_at_target = true;
            BigBigMassiveBoulder.can_be_left_on_ground = true;
            BigBigMassiveBoulder.can_be_blocked = false;
            AssetManager.projectiles.add(BigBigMassiveBoulder);

            EquipmentAsset BigBigMassiveBoulder_attack = AssetManager.items.clone("BigBigMassiveBoulder_attack", "$range");
            BigBigMassiveBoulder_attack.has_locales = false;
            BigBigMassiveBoulder_attack.projectile = "BigBigMassiveBoulder";
            BigBigMassiveBoulder_attack.base_stats["projectiles"] = 1f;
            BigBigMassiveBoulder_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            BigBigMassiveBoulder_attack.show_in_meta_editor = false;
            BigBigMassiveBoulder_attack.show_in_knowledge_window = false;
            BigBigMassiveBoulder_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("stun");



            var PanKong_wild = AssetManager.kingdoms.clone("PanKong_wild", "$TEMPLATE_ANIMAL$");
            PanKong_wild.concept = false;
            PanKong_wild.id = "PanKong_wild";
            PanKong_wild.default_kingdom_color = new ColorAsset("#679ead");
            PanKong_wild.units_always_looking_for_enemies = true;
            PanKong_wild.force_look_all_chunks = true;
            PanKong_wild.setIcon("actors/Avatars/PanKong_avatar");
            PanKong_wild.addTag("sliceable");
            PanKong_wild.addTag("nature_creature");
            PanKong_wild.addFriendlyTag("nature_creature");
            PanKong_wild.addTag("neutral_animals");
            PanKong_wild.addTag("neutral");
            PanKong_wild.addTag("PanKong_wild");
            PanKong_wild.addTag("Kaiju");
            PanKong_wild.addEnemyTag("civ");
            PanKong_wild.addEnemyTag("Kaiju");
            PanKong_wild.addFriendlyTag("monkey");
            PanKong_wild.addFriendlyTag("miniciv_monkey");
            PanKong_wild.addFriendlyTag("civ_monkey");
            AssetManager.kingdoms.add(PanKong_wild);
            World.world.kingdoms_wild.newWildKingdom(PanKong_wild);


            var PanKong = AssetManager.actor_library.clone("PanKong", "$mob$");
            PanKong.is_humanoid = false;
            PanKong.civ = false;
            PanKong.name_locale = "PanKong";
            PanKong.animation_speed_based_on_walk_speed = false;
            PanKong.has_avatar_prefab = false;
            PanKong.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/PanKong_avatar") };
            PanKong.has_override_avatar_frames = true;
            PanKong.inspect_avatar_scale = 1f;
            PanKong.inspect_avatar_offset_y = 6f;
            PanKong.shadow_texture = "unitShadow_6";
            PanKong.immune_to_slowness = true;
            PanKong.effect_damage = true;
            PanKong.unit_other = true;
            PanKong.collective_term = "group_den";
            PanKong.setSocialStructure("group_den", 10);
            PanKong.default_attack = "base_attack";
            PanKong.affected_by_dust = true;
            PanKong.inspect_children = true;
            PanKong.kingdom_id_civilization = string.Empty;
            PanKong.build_order_template_id = string.Empty;
            PanKong.show_on_meta_layer = true;
            PanKong.show_in_knowledge_window = true;
            PanKong.show_in_taxonomy_tooltip = true;
            PanKong.render_status_effects = true;
            PanKong.use_phenotypes = true;
            PanKong.death_animation_angle = true;
            PanKong.can_be_inspected = true;
            PanKong.name_template_sets = AssetLibrary<ActorAsset>.a<string>("monkey_set");
            PanKong.kingdom_id_wild = "PanKong_wild";
            PanKong.update_z = true;
            PanKong.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            PanKong.addDecision("kaiju_alpha_state_decision");
            PanKong.addDecision("kaiju_special_attack_decision");
            PanKong.addDecision("kaiju_mapwide_aggro_override_decision");
            PanKong.base_stats["lifespan"] = 200f;
            PanKong.base_stats["mass_2"] = 100000f;
            PanKong.base_stats["mass"] = 2000f;
            PanKong.base_stats["stamina"] = 500f;
            PanKong.base_stats["scale"] = 0.3f;
            PanKong.base_stats["size"] = 3f;
            PanKong.base_stats["health"] = 900f;
            PanKong.base_stats["speed"] = 60f;
            PanKong.base_stats["armor"] = 20f;
            PanKong.base_stats["attack_speed"] = 3f;
            PanKong.base_stats["damage"] = 1200f;
            PanKong.base_stats["knockback"] = 4f;
            PanKong.base_stats["accuracy"] = 1f;
            PanKong.base_stats["targets"] = 10f;
            PanKong.base_stats["area_of_effect"] = 5f;
            PanKong.base_stats["range"] = 7f;
            PanKong.base_stats["critical_damage_multiplier"] = 10f;
            PanKong.base_stats["multiplier_supply_timer"] = 1f;
            PanKong.disable_jump_animation = true;
            PanKong.can_be_moved_by_powers = true;
            PanKong.actor_size = ActorSize.S16_Buffalo;
            PanKong.animation_walk = Kaiju.walk_0_4;
            PanKong.animation_idle = ActorAnimationSequences.walk_0;
            PanKong.animation_swim = Kaiju.swim_0_4;
            PanKong.can_flip = true;
            PanKong.check_flip = (BaseSimObject _, WorldTile _) => true;
            PanKong.texture_asset = new ActorTextureSubAsset("actors/Kaiju/PanKong/", false);
            PanKong.icon = "PanKong_avatar";
            PanKong.die_in_lava = false;
            PanKong.visible_on_minimap = true;
            PanKong.experience_given = 20;
            PanKong.can_have_subspecies = true;
            PanKong.affected_by_dust = false;
            PanKong.special = true;
            PanKong.has_advanced_textures = false;
            PanKong.inspect_sex = true;
            PanKong.inspect_show_species = true;
            PanKong.inspect_generation = true;
            PanKong.needs_to_be_explored = false;
            PanKong.force_land_creature = true;
            PanKong.has_baby_form = true;
            PanKong.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            PanKong.addSubspeciesTrait("stomach");
            PanKong.addSubspeciesTrait("long_lifespan");
            PanKong.addSubspeciesTrait("population_minimal");
            PanKong.addSubspeciesTrait("parental_care");
            PanKong.addSubspeciesTrait("heat_resistance");
            PanKong.addSubspeciesTrait("reproduction_strategy_viviparity");
            PanKong.addSubspeciesTrait("gestation_extremely_long");
            PanKong.addSubspeciesTrait("reproduction_sexual");
            PanKong.addSubspeciesTrait("population_moderate");
            PanKong.addSubspeciesTrait("nimble");
            PanKong.addSubspeciesTrait("shiny_love");
            PanKong.addSubspeciesTrait("diet_herbivore");
            PanKong.addSubspeciesTrait("voracious");
            PanKong.kingdom_id_civilization = string.Empty;
            PanKong.build_order_template_id = string.Empty;
            PanKong.unit_other = true;
            PanKong.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            PanKong.animal_breeding_close_units_limit = 4;
            PanKong.can_evolve_into_new_species = false;
            PanKong.color_hex = "#679ead";
            PanKong.addTrait("agile");
            PanKong.addTrait("genius");
            PanKong.addTrait("regeneration");
            PanKong.addTrait("fire_proof");
            PanKong.name_taxonomic_kingdom = "animalia";
            PanKong.name_taxonomic_phylum = "chordata";
            PanKong.name_taxonomic_class = "reptilia";
            PanKong.name_taxonomic_order = "Archosauria";
            PanKong.name_taxonomic_family = "Titanus";
            PanKong.name_taxonomic_genus = "PanKong";
            PanKong.addResource("adamantine", 2);
            PanKong.addResource("gold", 10);
            PanKong.source_meat = true;
            PanKong.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "mid_gray" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };



            PanKong.phenotypes_list = new List<string> {
                "mid_gray",
                "savanna",
                "dark_orange",
                "swamp",
                "corrupted",
                "desert",
                "infernal",
                "lemon",
                "pink_yellow_mushroom",
                "wood",
                "bright_violet",
                "mid_gray",
                "polar",
                "bright_purple"
            };
            AssetManager.actor_library.add(PanKong);
            Localization.addLocalization(PanKong.name_locale, PanKong.name_locale);
            Localization.addLocalization("PanKong", PanKong.name_locale);
            Localization.addLocalization("spawnPanKong", PanKong.name_locale);
            Localization.addLocalization("spawnPanKong_description", "Rightful King of the Monsters");




            var Skullcrawler_wild = AssetManager.kingdoms.clone("Skullcrawler_wild", "$TEMPLATE_ANIMAL$");
            Skullcrawler_wild.concept = false;
            Skullcrawler_wild.id = "Skullcrawler_wild";
            Skullcrawler_wild.default_kingdom_color = new ColorAsset("#679ead");
            Skullcrawler_wild.units_always_looking_for_enemies = true;
            Skullcrawler_wild.force_look_all_chunks = true;
            Skullcrawler_wild.setIcon("actors/Avatars/Skullcrawler_avatar");
            Skullcrawler_wild.addTag("sliceable");
            Skullcrawler_wild.addTag("nature_creature");
            Skullcrawler_wild.addFriendlyTag("nature_creature");
            Skullcrawler_wild.addTag("neutral_animals");
            Skullcrawler_wild.addTag("neutral");
            Skullcrawler_wild.addTag("Skullcrawler_wild");
            Skullcrawler_wild.addTag("Kaiju");
            Skullcrawler_wild.addEnemyTag("civ");
            Skullcrawler_wild.addEnemyTag("Kaiju");
            Skullcrawler_wild.addFriendlyTag("crocodile");
            Skullcrawler_wild.addFriendlyTag("civ_crocodile");
            AssetManager.kingdoms.add(Skullcrawler_wild);
            World.world.kingdoms_wild.newWildKingdom(Skullcrawler_wild);


            var Skullcrawler = AssetManager.actor_library.clone("Skullcrawler", "$mob$");
            Skullcrawler.is_humanoid = false;
            Skullcrawler.civ = false;
            Skullcrawler.name_locale = "Skullcrawler";
            Skullcrawler.animation_speed_based_on_walk_speed = false;
            Skullcrawler.has_avatar_prefab = false;
            Skullcrawler.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Skullcrawler_avatar") };
            Skullcrawler.has_override_avatar_frames = true;
            Skullcrawler.inspect_avatar_scale = 1f;
            Skullcrawler.inspect_avatar_offset_y = 6f;
            Skullcrawler.shadow_texture = "unitShadow_6";
            Skullcrawler.immune_to_slowness = true;
            Skullcrawler.effect_damage = true;
            Skullcrawler.unit_other = true;
            Skullcrawler.collective_term = "group_den";
            Skullcrawler.setSocialStructure("group_den", 10);
            Skullcrawler.default_attack = "jaws";
            Skullcrawler.affected_by_dust = true;
            Skullcrawler.inspect_children = true;
            Skullcrawler.kingdom_id_civilization = string.Empty;
            Skullcrawler.build_order_template_id = string.Empty;
            Skullcrawler.show_on_meta_layer = true;
            Skullcrawler.show_in_knowledge_window = true;
            Skullcrawler.show_in_taxonomy_tooltip = true;
            Skullcrawler.render_status_effects = true;
            Skullcrawler.use_phenotypes = true;
            Skullcrawler.death_animation_angle = true;
            Skullcrawler.can_be_inspected = true;
            Skullcrawler.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
            Skullcrawler.kingdom_id_wild = "Skullcrawler_wild";
            Skullcrawler.update_z = true;
            Skullcrawler.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Skullcrawler.addDecision("kaiju_alpha_state_decision");
            Skullcrawler.addDecision("kaiju_special_attack_decision");
            Skullcrawler.addDecision("kaiju_mapwide_aggro_override_decision");
            Skullcrawler.base_stats["lifespan"] = 200f;
            Skullcrawler.base_stats["mass_2"] = 100000f;
            Skullcrawler.base_stats["mass"] = 2000f;
            Skullcrawler.base_stats["stamina"] = 500f;
            Skullcrawler.base_stats["scale"] = 0.1f;
            Skullcrawler.base_stats["size"] = 1f;
            Skullcrawler.base_stats["health"] = 3600f;
            Skullcrawler.base_stats["speed"] = 80f;
            Skullcrawler.base_stats["armor"] = 20f;
            Skullcrawler.base_stats["attack_speed"] = 2f;
            Skullcrawler.base_stats["damage"] = 2000f;
            Skullcrawler.base_stats["knockback"] = 0f;
            Skullcrawler.base_stats["accuracy"] = 1f;
            Skullcrawler.base_stats["targets"] = 12f;
            Skullcrawler.base_stats["area_of_effect"] = 5f;
            Skullcrawler.base_stats["range"] = 10f;
            Skullcrawler.base_stats["critical_damage_multiplier"] = 10f;
            Skullcrawler.base_stats["multiplier_supply_timer"] = 1f;
            Skullcrawler.disable_jump_animation = true;
            Skullcrawler.can_be_moved_by_powers = true;
            Skullcrawler.actor_size = ActorSize.S16_Buffalo;
            Skullcrawler.animation_walk = Kaiju.walk_0_4;
            Skullcrawler.animation_idle = ActorAnimationSequences.idle_0_3;
            Skullcrawler.animation_swim = Kaiju.swim_0_4;
            Skullcrawler.can_flip = true;
            Skullcrawler.check_flip = (BaseSimObject _, WorldTile _) => true;
            Skullcrawler.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Skullcrawler/", false);
            Skullcrawler.icon = "Skullcrawler_avatar";
            Skullcrawler.die_in_lava = false;
            Skullcrawler.visible_on_minimap = true;
            Skullcrawler.experience_given = 20;
            Skullcrawler.can_have_subspecies = true;
            Skullcrawler.affected_by_dust = false;
            Skullcrawler.special = true;
            Skullcrawler.has_advanced_textures = false;
            Skullcrawler.inspect_sex = true;
            Skullcrawler.inspect_show_species = true;
            Skullcrawler.inspect_generation = true;
            Skullcrawler.needs_to_be_explored = false;
            Skullcrawler.force_land_creature = true;
            Skullcrawler.has_baby_form = true;
            Skullcrawler.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            Skullcrawler.addSubspeciesTrait("stomach");
            Skullcrawler.addSubspeciesTrait("big_stomach");
            Skullcrawler.addSubspeciesTrait("long_lifespan");
            Skullcrawler.addSubspeciesTrait("population_minimal");
            Skullcrawler.addSubspeciesTrait("parental_care");
            Skullcrawler.addSubspeciesTrait("heat_resistance");
            Skullcrawler.addSubspeciesTrait("reproduction_strategy_oviparity");
            Skullcrawler.addSubspeciesTrait("gestation_extremely_long");
            Skullcrawler.addSubspeciesTrait("reproduction_sexual");
            Skullcrawler.addSubspeciesTrait("egg_shell_plain");
            Skullcrawler.addSubspeciesTrait("nimble");
            Skullcrawler.addSubspeciesTrait("diet_carnivore");
            Skullcrawler.addSubspeciesTrait("diet_cannibalism");
            Skullcrawler.addSubspeciesTrait("voracious");
            Skullcrawler.addSubspeciesTrait("hotheaded");
            Skullcrawler.kingdom_id_civilization = string.Empty;
            Skullcrawler.build_order_template_id = string.Empty;
            Skullcrawler.unit_other = true;
            Skullcrawler.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            Skullcrawler.animal_breeding_close_units_limit = 4;
            Skullcrawler.can_evolve_into_new_species = false;
            Skullcrawler.color_hex = "#679ead";
            Skullcrawler.addTrait("agile");
            Skullcrawler.addTrait("genius");
            Skullcrawler.addTrait("regeneration");
            Skullcrawler.addTrait("fire_proof");
            Skullcrawler.name_taxonomic_kingdom = "animalia";
            Skullcrawler.name_taxonomic_phylum = "chordata";
            Skullcrawler.name_taxonomic_class = "reptilia";
            Skullcrawler.name_taxonomic_order = "Archosauria";
            Skullcrawler.name_taxonomic_family = "Titanus";
            Skullcrawler.name_taxonomic_genus = "Skullcrawler";
            Skullcrawler.addResource("adamantine", 2);
            Skullcrawler.addResource("gold", 10);
            Skullcrawler.source_meat = true;
            Skullcrawler.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "mid_gray" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };



            Skullcrawler.phenotypes_list = new List<string> {
                "mid_gray",
                "savanna",
                "dark_orange",
                "swamp",
                "corrupted",
                "desert",
                "infernal",
                "lemon",
                "pink_yellow_mushroom",
                "wood",
                "bright_violet",
                "mid_gray",
                "polar",
                "bright_purple"
            };
            AssetManager.actor_library.add(Skullcrawler);
            Localization.addLocalization(Skullcrawler.name_locale, Skullcrawler.name_locale);
            Localization.addLocalization("Skullcrawler", Skullcrawler.name_locale);
            Localization.addLocalization("spawnSkullcrawler", Skullcrawler.name_locale);
            Localization.addLocalization("spawnSkullcrawler_description", "Rightful King of the Monsters");





            EffectAsset CrabBomba = new EffectAsset();
            CrabBomba.id = "CrabBomba";
            CrabBomba.use_basic_prefab = true;
            CrabBomba.sorting_layer_id = "EffectsTop";
            CrabBomba.sprite_path = "effects/CrabBomba";
            CrabBomba.draw_light_area = false;
            AssetManager.effects_library.add(CrabBomba);



            var CrabLordPOWERterra = AssetManager.terraform.clone("CrabLordPOWERterra", "flash");
            CrabLordPOWERterra.damage = 2800;
            CrabLordPOWERterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("crabzilord_wild");
            CrabLordPOWERterra.flash = false;
            CrabLordPOWERterra.explode_strength = 1;
            CrabLordPOWERterra.explode_tile = true;
            CrabLordPOWERterra.explosion_pixel_effect = true;
            CrabLordPOWERterra.remove_tornado = true;
            CrabLordPOWERterra.apply_force = true;
            CrabLordPOWERterra.damage_buildings = true;
            CrabLordPOWERterra.transform_to_wasteland = false;
            CrabLordPOWERterra.applies_to_high_flyers = true;
            CrabLordPOWERterra.shake = true;
            AssetManager.terraform.add(CrabLordPOWERterra);



            ProjectileAsset CrabLordPOWER = new ProjectileAsset();
            CrabLordPOWER.id = "CrabLordPOWER";
            CrabLordPOWER.speed = 60f;
            CrabLordPOWER.texture = "kame";
            CrabLordPOWER.texture_shadow = "shadows/projectiles/shadow_ball";
            CrabLordPOWER.terraform_option = "CrabLordPOWERterra";
            CrabLordPOWER.draw_light_area = true;
            CrabLordPOWER.terraform_range = 10;
            CrabLordPOWER.sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow";
            CrabLordPOWER.sound_impact = "event:/SFX/WEAPONS/WeaponRockLand";
            CrabLordPOWER.end_effect = "CrabBomba";
            CrabLordPOWER.scale_start = 0.8f;
            CrabLordPOWER.scale_target = 0.8f;
            CrabLordPOWER.look_at_target = true;
            CrabLordPOWER.can_be_left_on_ground = true;
            CrabLordPOWER.can_be_blocked = false;
            AssetManager.projectiles.add(CrabLordPOWER);

            EquipmentAsset CrabLordPOWER_attack = AssetManager.items.clone("CrabLordPOWER_attack", "$range");
            CrabLordPOWER_attack.has_locales = false;
            CrabLordPOWER_attack.projectile = "CrabLordPOWER";
            CrabLordPOWER_attack.base_stats["projectiles"] = 1f;
            CrabLordPOWER_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            CrabLordPOWER_attack.show_in_meta_editor = false;
            CrabLordPOWER_attack.show_in_knowledge_window = false;
            CrabLordPOWER_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("stun");



            var crabzilord_wild = AssetManager.kingdoms.clone("crabzilord_wild", "$TEMPLATE_ANIMAL$");
            crabzilord_wild.concept = false;
            crabzilord_wild.id = "crabzilord_wild";
            crabzilord_wild.default_kingdom_color = new ColorAsset("#679ead");
            crabzilord_wild.units_always_looking_for_enemies = true;
            crabzilord_wild.force_look_all_chunks = true;
            crabzilord_wild.setIcon("actors/Avatars/crabzilord_avatar");
            crabzilord_wild.addTag("sliceable");
            crabzilord_wild.addTag("nature_creature");
            crabzilord_wild.addFriendlyTag("nature_creature");
            crabzilord_wild.addTag("neutral_animals");
            crabzilord_wild.addTag("neutral");
            crabzilord_wild.addTag("crabzilord_wild");
            crabzilord_wild.addTag("Kaiju");
            crabzilord_wild.addEnemyTag("civ");
            crabzilord_wild.addEnemyTag("Kaiju");
            crabzilord_wild.addFriendlyTag("crab");
            crabzilord_wild.addFriendlyTag("miniciv_crab");
            crabzilord_wild.addFriendlyTag("civ_crab");
            AssetManager.kingdoms.add(crabzilord_wild);
            World.world.kingdoms_wild.newWildKingdom(crabzilord_wild);


            var crabzilord = AssetManager.actor_library.clone("crabzilord", "$mob$");
            crabzilord.is_humanoid = false;
            crabzilord.civ = false;
            crabzilord.name_locale = "crabzilord";
            crabzilord.animation_speed_based_on_walk_speed = false;
            crabzilord.has_avatar_prefab = false;
            crabzilord.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/crabzilord_avatar") };
            crabzilord.has_override_avatar_frames = true;
            crabzilord.inspect_avatar_scale = 1f;
            crabzilord.inspect_avatar_offset_y = 6f;
            crabzilord.shadow_texture = "unitShadow_6";
            crabzilord.immune_to_slowness = true;
            crabzilord.effect_damage = true;
            crabzilord.unit_other = true;
            crabzilord.collective_term = "group_den";
            crabzilord.setSocialStructure("group_den", 10);
            crabzilord.default_attack = "base_attack";
            crabzilord.affected_by_dust = true;
            crabzilord.inspect_children = true;
            crabzilord.kingdom_id_civilization = string.Empty;
            crabzilord.build_order_template_id = string.Empty;
            crabzilord.show_on_meta_layer = true;
            crabzilord.show_in_knowledge_window = true;
            crabzilord.show_in_taxonomy_tooltip = true;
            crabzilord.render_status_effects = true;
            crabzilord.use_phenotypes = true;
            crabzilord.death_animation_angle = true;
            crabzilord.can_be_inspected = true;
            crabzilord.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crab_set");
            crabzilord.kingdom_id_wild = "crabzilord_wild";
            crabzilord.update_z = true;
            crabzilord.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            crabzilord.addDecision("kaiju_alpha_state_decision");
            crabzilord.addDecision("kaiju_special_attack_decision");
            crabzilord.addDecision("kaiju_mapwide_aggro_override_decision");
            crabzilord.addDecision(KaijuBurrowDecisionId);
            crabzilord.base_stats["lifespan"] = 200f;
            crabzilord.base_stats["mass_2"] = 100000f;
            crabzilord.base_stats["mass"] = 2000f;
            crabzilord.base_stats["stamina"] = 500f;
            crabzilord.base_stats["scale"] = 0.15f;
            crabzilord.base_stats["size"] = 2f;
            crabzilord.base_stats["health"] = 3000f;
            crabzilord.base_stats["speed"] = 60f;
            crabzilord.base_stats["armor"] = 20f;
            crabzilord.base_stats["attack_speed"] = 3f;
            crabzilord.base_stats["damage"] = 800f;
            crabzilord.base_stats["knockback"] = 6f;
            crabzilord.base_stats["accuracy"] = 1f;
            crabzilord.base_stats["targets"] = 20f;
            crabzilord.base_stats["area_of_effect"] = 5f;
            crabzilord.base_stats["range"] = 10f;
            crabzilord.base_stats["critical_damage_multiplier"] = 10f;
            crabzilord.base_stats["multiplier_supply_timer"] = 1f;
            crabzilord.disable_jump_animation = true;
            crabzilord.can_be_moved_by_powers = true;
            crabzilord.actor_size = ActorSize.S16_Buffalo;
            crabzilord.animation_walk = Kaiju.walk_0_5;
            crabzilord.animation_idle = Kaiju.idle_0_4;
            crabzilord.animation_swim = Kaiju.swim_0_5;
            crabzilord.can_flip = true;
            crabzilord.check_flip = (BaseSimObject _, WorldTile _) => true;
            crabzilord.texture_asset = new ActorTextureSubAsset("actors/Kaiju/crabzilord/", false);
            crabzilord.icon = "crabzilord_avatar";
            crabzilord.die_in_lava = false;
            crabzilord.visible_on_minimap = true;
            crabzilord.experience_given = 20;
            crabzilord.can_have_subspecies = true;
            crabzilord.affected_by_dust = false;
            crabzilord.special = true;
            crabzilord.has_advanced_textures = false;
            crabzilord.inspect_sex = true;
            crabzilord.inspect_show_species = true;
            crabzilord.inspect_generation = true;
            crabzilord.needs_to_be_explored = false;
            crabzilord.force_land_creature = true;
            crabzilord.has_baby_form = true;
            crabzilord.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
            crabzilord.addSubspeciesTrait("stomach");
            crabzilord.addSubspeciesTrait("long_lifespan");
            crabzilord.addSubspeciesTrait("population_minimal");
            crabzilord.addSubspeciesTrait("parental_care");
            crabzilord.addSubspeciesTrait("heat_resistance");
            crabzilord.addSubspeciesTrait("reproduction_strategy_oviparity");
            crabzilord.addSubspeciesTrait("exoskeleton");
            crabzilord.addSubspeciesTrait("egg_roe");
            crabzilord.addSubspeciesTrait("reproduction_sexual");
            crabzilord.addSubspeciesTrait("amygdala");
            crabzilord.addSubspeciesTrait("diet_algivore");
            crabzilord.addSubspeciesTrait("fins");
            crabzilord.kingdom_id_civilization = string.Empty;
            crabzilord.build_order_template_id = string.Empty;
            crabzilord.unit_other = true;
            crabzilord.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
            crabzilord.animal_breeding_close_units_limit = 4;
            crabzilord.can_evolve_into_new_species = false;
            crabzilord.color_hex = "#679ead";
            crabzilord.addTrait("weightless");
            crabzilord.addTrait("hard_skin");
            crabzilord.addTrait("regeneration");
            crabzilord.addTrait("fire_proof");
            crabzilord.name_taxonomic_kingdom = "animalia";
            crabzilord.name_taxonomic_phylum = "arthropoda";
            crabzilord.name_taxonomic_class = "malacostraca";
            crabzilord.name_taxonomic_order = "decapoda";
            crabzilord.name_taxonomic_family = "portunidae";
            crabzilord.name_taxonomic_genus = "carcinus";
            crabzilord.name_taxonomic_genus = "crabzilord";
            crabzilord.addResource("adamantine", 2);
            crabzilord.addResource("gold", 10);
            crabzilord.source_meat = true;
            crabzilord.phenotypes_dict = new Dictionary<string, List<string>>() {
                { "default_color", new List<string> { "bright_orange" } },
                { "biome_savanna", new List<string> { "savanna", "dark_orange" } },
                { "biome_swamp", new List<string> { "swamp" } },
                { "biome_corrupted", new List<string> { "corrupted" } },
                { "biome_desert", new List<string> { "desert" } },
                { "biome_infernal", new List<string> { "infernal" } },
                { "biome_lemon", new List<string> { "lemon" } },
                { "biome_mushroom", new List<string> { "pink_yellow_mushroom" } },
                { "biome_sand", new List<string> { "dark_orange", "wood" } },
                { "biome_singularity", new List<string> { "bright_violet" } },
                { "biome_garlic", new List<string> { "mid_gray" } },
                { "biome_maple", new List<string> { "dark_orange" } },
                { "biome_permafrost", new List<string> { "polar" } },
                { "biome_rocklands", new List<string> { "gray_black" } },
                { "biome_celestial", new List<string> { "bright_purple" } }
            };



            crabzilord.phenotypes_list = new List<string> {
                "bright_salmon",
                "bright_orange",
                "dark_orange"
            };
            AssetManager.actor_library.add(crabzilord);
            Localization.addLocalization(crabzilord.name_locale, crabzilord.name_locale);
            Localization.addLocalization("crabzilord", crabzilord.name_locale);
            Localization.addLocalization("spawncrabzilord", crabzilord.name_locale);
            Localization.addLocalization("spawncrabzilord_description", "Rightful King of the Monsters");
































            var angelproterra = AssetManager.terraform.clone("angelproterra", "bomb");
            angelproterra.damage = 1000;
            angelproterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("Angel_Apostle");
            angelproterra.explode_strength = 1;
            angelproterra.transform_to_wasteland = false;
            angelproterra.applies_to_high_flyers = true;
            angelproterra.shake = true;
            AssetManager.terraform.add(angelproterra);

            var ramiel_exterminatus_terra = AssetManager.terraform.clone("ramiel_exterminatus_terra", "bomb");
            ramiel_exterminatus_terra.damage = 5000;
            ramiel_exterminatus_terra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("Angel_Apostle");
            ramiel_exterminatus_terra.explode_strength = 2;
            ramiel_exterminatus_terra.transform_to_wasteland = false;
            ramiel_exterminatus_terra.applies_to_high_flyers = true;
            ramiel_exterminatus_terra.shake = true;
            ramiel_exterminatus_terra.remove_lava = true;
            ramiel_exterminatus_terra.make_ruins = true;
            ramiel_exterminatus_terra.remove_top_tile = true;
            ramiel_exterminatus_terra.remove_roads = true;
            ramiel_exterminatus_terra.remove_frozen = true;
            ramiel_exterminatus_terra.remove_water = true;
            ramiel_exterminatus_terra.flash = true;
            ramiel_exterminatus_terra.remove_borders = true;
            ramiel_exterminatus_terra.remove_tornado = true;
            ramiel_exterminatus_terra.add_burned = true;
            ramiel_exterminatus_terra.set_fire = true;
            ramiel_exterminatus_terra.apply_force = true;
            ramiel_exterminatus_terra.force_power = 2.5f;
            ramiel_exterminatus_terra.add_trait = "madness";
            AssetManager.terraform.add(ramiel_exterminatus_terra);

            EffectAsset angelboom = new EffectAsset();
            angelboom.id = "angelboom";
            angelboom.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny";
            angelboom.use_basic_prefab = true;
            angelboom.sorting_layer_id = "EffectsTop";
            angelboom.sprite_path = "effects/angelboom";
            angelboom.draw_light_area = true;
            AssetManager.effects_library.add(angelboom);


            EffectAsset angelboombig = new EffectAsset();
            angelboombig.id = "angelboombig";
            angelboombig.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny";
            angelboombig.use_basic_prefab = true;
            angelboombig.sorting_layer_id = "EffectsTop";
            angelboombig.sprite_path = "effects/angelboombig";
            angelboombig.draw_light_area = true;
            AssetManager.effects_library.add(angelboombig);

            ProjectileAsset ramiel_exterminatus = new ProjectileAsset();
            ramiel_exterminatus.id = "ramiel_exterminatus";
            ramiel_exterminatus.speed = 200f;
            ramiel_exterminatus.texture = "angelpro";
            ramiel_exterminatus.trail_effect_enabled = false;
            ramiel_exterminatus.texture_shadow = "shadows/projectiles/shadow_ball";
            ramiel_exterminatus.terraform_option = "ramiel_exterminatus_terra";
            ramiel_exterminatus.draw_light_area = true;
            ramiel_exterminatus.terraform_range = 20;
            ramiel_exterminatus.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            ramiel_exterminatus.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            ramiel_exterminatus.end_effect = "angelboombig";
            ramiel_exterminatus.scale_start = 0.4f;
            ramiel_exterminatus.scale_target = 0.4f;
            ramiel_exterminatus.look_at_target = true;
            ramiel_exterminatus.can_be_left_on_ground = false;
            ramiel_exterminatus.can_be_blocked = false;
            ramiel_exterminatus.world_actions = (AttackAction)Delegate.Combine(ramiel_exterminatus.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(ramiel_exterminatus);

            EquipmentAsset ramiel_exterminatus_attack = AssetManager.items.clone("ramiel_exterminatus_attack", "$range");
            ramiel_exterminatus_attack.has_locales = false;
            ramiel_exterminatus_attack.projectile = "ramiel_exterminatus";
            ramiel_exterminatus_attack.base_stats["projectiles"] = 1f;
            ramiel_exterminatus_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            ramiel_exterminatus_attack.show_in_meta_editor = false;
            ramiel_exterminatus_attack.show_in_knowledge_window = false;
            ramiel_exterminatus_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame", "stun");


            ProjectileAsset angelpro = new ProjectileAsset();
            angelpro.id = "angelpro";
            angelpro.speed = 200f;
            angelpro.texture = "angelpro";
            angelpro.trail_effect_enabled = false;
            angelpro.texture_shadow = "shadows/projectiles/shadow_ball";
            angelpro.terraform_option = "angelproterra";
            angelpro.draw_light_area = true;
            angelpro.terraform_range = 10;
            angelpro.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            angelpro.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            angelpro.end_effect = "angelboom";
            angelpro.scale_start = 0.4f;
            angelpro.scale_target = 0.4f;
            angelpro.look_at_target = true;
            angelpro.can_be_left_on_ground = false;
            angelpro.can_be_blocked = false;
            angelpro.world_actions = (AttackAction)Delegate.Combine(angelpro.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(angelpro);



            EquipmentAsset Angel_massacre = AssetManager.items.clone("Angel_massacre", "$range");
            Angel_massacre.has_locales = false;
            Angel_massacre.projectile = "angelpro";
            Angel_massacre.base_stats["projectiles"] = 1f;
            Angel_massacre.path_slash_animation = "effects/slashes/slash_cannonball";
            Angel_massacre.show_in_meta_editor = false;
            Angel_massacre.show_in_knowledge_window = false;



            var Angel_Apostle = AssetManager.kingdoms.clone("Angel_Apostle", "$TEMPLATE_MOB$");
            Angel_Apostle.concept = false;
            Angel_Apostle.id = "Angel_Apostle";
            Angel_Apostle.default_kingdom_color = new ColorAsset("#679ead");
            Angel_Apostle.mobs = true;
            Angel_Apostle.always_attack_each_other = false;
            Angel_Apostle.force_look_all_chunks = true;
            Angel_Apostle.setIcon("ui/Icons/Ramiel");
            Angel_Apostle.addTag("sliceable");
            Angel_Apostle.addTag("Angel");
            Angel_Apostle.addFriendlyTag("Angel");
            Angel_Apostle.addFriendlyTag("nature_creature");
            Angel_Apostle.addFriendlyTag("neutral_animals");
            Angel_Apostle.addFriendlyTag("neutral");
            Angel_Apostle.addEnemyTag("civ");
            Angel_Apostle.addEnemyTag("Kaiju");
            AssetManager.kingdoms.add(Angel_Apostle);
            World.world.kingdoms_wild.newWildKingdom(Angel_Apostle);





            var Ramiel = AssetManager.actor_library.clone("Ramiel", "$mob$");
            Ramiel.is_humanoid = false;
            Ramiel.civ = false;
            Ramiel.name_locale = "Ramiel";
            Ramiel.animation_speed_based_on_walk_speed = false;
            Ramiel.has_avatar_prefab = false;
            Ramiel.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Ramiel_avatar") };
            Ramiel.has_override_avatar_frames = true;
            Ramiel.inspect_avatar_scale = 1f;
            Ramiel.inspect_avatar_offset_y = 6f;
            Ramiel.shadow_texture = "unitShadow_6";
            Ramiel.immune_to_slowness = true;
            Ramiel.effect_damage = true;
            Ramiel.unit_other = true;
            Ramiel.collective_term = "group_den";
            Ramiel.default_attack = "Angel_massacre";
            Ramiel.affected_by_dust = false;
            Ramiel.has_baby_form = false;
            Ramiel.kingdom_id_civilization = string.Empty;
            Ramiel.build_order_template_id = string.Empty;
            Ramiel.show_on_meta_layer = false;
            Ramiel.show_in_knowledge_window = false;
            Ramiel.show_in_taxonomy_tooltip = false;
            Ramiel.render_status_effects = true;
            Ramiel.use_phenotypes = false;
            Ramiel.death_animation_angle = true;
            Ramiel.can_be_inspected = true;
            Ramiel.name_template_sets = AssetLibrary<ActorAsset>.a<string>("evil_mage_set");
            Ramiel.kingdom_id_wild = "Angel_Apostle";
            Ramiel.power_id = "spawnRamiel";
            Ramiel.update_z = true;
            Ramiel.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Ramiel.addDecision("random_move_towards_civ_building");
            Ramiel.addDecision("boss_attack_animation_decision");
            Ramiel.addDecision("kaiju_special_attack_decision");
            Ramiel.addDecision("kaiju_mapwide_aggro_override_decision");
            Ramiel.base_stats["lifespan"] = 20000f;
            Ramiel.base_stats["mass_2"] = 100000f;
            Ramiel.base_stats["mass"] = 2000f;
            Ramiel.base_stats["stamina"] = 500f;
            Ramiel.base_stats["scale"] = 0.2f;
            Ramiel.base_stats["size"] = 2f;
            Ramiel.base_stats["health"] = 16000f;
            Ramiel.base_stats["speed"] = 30f;
            Ramiel.base_stats["armor"] = 80f;
            Ramiel.base_stats["attack_speed"] = 0.4f;
            Ramiel.base_stats["damage"] = 1000f;
            Ramiel.base_stats["knockback"] = 4f;
            Ramiel.base_stats["accuracy"] = 1f;
            Ramiel.base_stats["targets"] = 100f;
            Ramiel.base_stats["area_of_effect"] = 5f;
            Ramiel.base_stats["range"] = 1000f;
            Ramiel.base_stats["critical_damage_multiplier"] = 10f;
            Ramiel.base_stats["multiplier_supply_timer"] = 1f;
            Ramiel.disable_jump_animation = true;
            Ramiel.can_be_moved_by_powers = true;
            Ramiel.actor_size = ActorSize.S16_Buffalo;
            Ramiel.animation_walk = Kaiju.walk_0_14;
            Ramiel.animation_idle = Kaiju.idle_0;
            Ramiel.animation_swim = Kaiju.walk_0_14;
            Ramiel.can_flip = true;
            Ramiel.check_flip = (BaseSimObject _, WorldTile _) => true;
            Ramiel.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Ramiel/", false);
            Ramiel.icon = "Ramiel_avatar";
            Ramiel.die_in_lava = false;
            Ramiel.visible_on_minimap = true;
            Ramiel.experience_given = 1000000;
            Ramiel.can_have_subspecies = false;
            Ramiel.affected_by_dust = false;
            Ramiel.inspect_children = false;
            Ramiel.special = true;
            Ramiel.has_advanced_textures = false;
            Ramiel.inspect_sex = false;
            Ramiel.inspect_show_species = false;
            Ramiel.inspect_generation = false;
            Ramiel.needs_to_be_explored = false;
            Ramiel.force_land_creature = true;
            Ramiel.color_hex = "#679ead";
            Ramiel.addTrait("chosen_one");
            Ramiel.addTrait("regeneration");
            Ramiel.addTrait("tough");
            Ramiel.addTrait("blessed");
            Ramiel.addTrait("fire_proof");
            Ramiel.addTrait("bubble_defense");
            Ramiel.flying = true;
            Ramiel.addResource("adamantine", 100);
            Ramiel.addResource("gold", 2000);
            AssetManager.actor_library.add(Ramiel);
            Localization.addLocalization(Ramiel.name_locale, Ramiel.name_locale);
            Localization.addLocalization("Ramiel", Ramiel.name_locale);
            Localization.addLocalization("ramiel", Ramiel.name_locale);
            Localization.addLocalization("spawnRamiel", Ramiel.name_locale);
            Localization.addLocalization("spawnRamiel_description", "Thunder of God");



            var Gaghiel = AssetManager.actor_library.clone("Gaghiel", "$mob$");
            Gaghiel.is_humanoid = false;
            Gaghiel.civ = false;
            Gaghiel.name_locale = "Gaghiel";
            Gaghiel.animation_speed_based_on_walk_speed = false;
            Gaghiel.has_avatar_prefab = false;
            Gaghiel.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Gaghiel_avatar") };
            Gaghiel.has_override_avatar_frames = true;
            Gaghiel.inspect_avatar_scale = 1f;
            Gaghiel.inspect_avatar_offset_y = 6f;
            Gaghiel.shadow_texture = "unitShadow_6";
            Gaghiel.immune_to_slowness = true;
            Gaghiel.effect_damage = true;
            Gaghiel.unit_other = true;
            Gaghiel.collective_term = "group_den";
            Gaghiel.default_attack = "base_attack";
            Gaghiel.affected_by_dust = false;
            Gaghiel.kingdom_id_civilization = string.Empty;
            Gaghiel.build_order_template_id = string.Empty;
            Gaghiel.show_on_meta_layer = false;
            Gaghiel.show_in_knowledge_window = false;
            Gaghiel.show_in_taxonomy_tooltip = false;
            Gaghiel.render_status_effects = true;
            Gaghiel.use_phenotypes = false;
            Gaghiel.death_animation_angle = true;
            Gaghiel.can_be_inspected = true;
            Gaghiel.name_template_sets = AssetLibrary<ActorAsset>.a<string>("evil_mage_set");
            Gaghiel.kingdom_id_wild = "Angel_Apostle";
            Gaghiel.power_id = "spawnGaghiel";
            Gaghiel.update_z = true;
            Gaghiel.has_baby_form = false;
            Gaghiel.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Gaghiel.addDecision("random_move_towards_civ_building");
            Gaghiel.addDecision("boss_attack_animation_decision");
            Gaghiel.addDecision("kaiju_mapwide_aggro_override_decision");
            Gaghiel.base_stats["lifespan"] = 20000f;
            Gaghiel.base_stats["mass_2"] = 100000f;
            Gaghiel.base_stats["mass"] = 2000f;
            Gaghiel.base_stats["stamina"] = 500f;
            Gaghiel.base_stats["scale"] = 0.2f;
            Gaghiel.base_stats["size"] = 2f;
            Gaghiel.base_stats["health"] = 16000f;
            Gaghiel.base_stats["speed"] = 30f;
            Gaghiel.base_stats["armor"] = 80f;
            Gaghiel.base_stats["attack_speed"] = 1.5f;
            Gaghiel.base_stats["damage"] = 5000f;
            Gaghiel.base_stats["knockback"] = 6f;
            Gaghiel.base_stats["accuracy"] = 1f;
            Gaghiel.base_stats["targets"] = 100f;
            Gaghiel.base_stats["area_of_effect"] = 5f;
            Gaghiel.base_stats["range"] = 10f;
            Gaghiel.base_stats["critical_damage_multiplier"] = 10f;
            Gaghiel.base_stats["multiplier_supply_timer"] = 1f;
            Gaghiel.disable_jump_animation = true;
            Gaghiel.can_be_moved_by_powers = true;
            Gaghiel.actor_size = ActorSize.S16_Buffalo;
            Gaghiel.animation_walk = ActorAnimationSequences.walk_0;
            Gaghiel.animation_idle = ActorAnimationSequences.walk_0;
            Gaghiel.animation_swim = Kaiju.swim_0_19;
            Gaghiel.can_flip = true;
            Gaghiel.check_flip = (BaseSimObject _, WorldTile _) => true;
            Gaghiel.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Gaghiel/", false);
            Gaghiel.icon = "Gaghiel_avatar";
            Gaghiel.die_in_lava = false;
            Gaghiel.visible_on_minimap = true;
            Gaghiel.experience_given = 1000000;
            Gaghiel.can_have_subspecies = false;
            Gaghiel.affected_by_dust = false;
            Gaghiel.inspect_children = false;
            Gaghiel.special = true;
            Gaghiel.has_advanced_textures = false;
            Gaghiel.inspect_sex = false;
            Gaghiel.inspect_show_species = false;
            Gaghiel.inspect_generation = false;
            Gaghiel.needs_to_be_explored = false;
            Gaghiel.force_land_creature = false;
            Gaghiel.force_ocean_creature = true;
            Gaghiel.color_hex = "#679ead";
            Gaghiel.addTrait("chosen_one");
            Gaghiel.addTrait("regeneration");
            Gaghiel.addTrait("blessed");
            Gaghiel.addTrait("fire_proof");
            Gaghiel.addResource("adamantine", 100);
            Gaghiel.addResource("gold", 2000);
            AssetManager.actor_library.add(Gaghiel);
            Localization.addLocalization(Gaghiel.name_locale, Gaghiel.name_locale);
            Localization.addLocalization("Gaghiel", Gaghiel.name_locale);
            Localization.addLocalization("Gaghiel", Gaghiel.name_locale);
            Localization.addLocalization("spawnGaghiel", Gaghiel.name_locale);
            Localization.addLocalization("spawnGaghiel_description", "Roaring Beast of God");

            var Sachiel = AssetManager.actor_library.clone("Sachiel", "$mob$");
            Sachiel.is_humanoid = false;
            Sachiel.civ = false;
            Sachiel.name_locale = "Sachiel";
            Sachiel.animation_speed_based_on_walk_speed = false;
            Sachiel.has_avatar_prefab = false;
            Sachiel.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Sachiel_avatar") };
            Sachiel.has_override_avatar_frames = true;
            Sachiel.inspect_avatar_scale = 1f;
            Sachiel.inspect_avatar_offset_y = 6f;
            Sachiel.shadow_texture = "unitShadow_6";
            Sachiel.immune_to_slowness = true;
            Sachiel.effect_damage = true;
            Sachiel.unit_other = true;
            Sachiel.collective_term = "group_den";
            Sachiel.default_attack = "base_attack";
            Sachiel.affected_by_dust = false;
            Sachiel.kingdom_id_civilization = string.Empty;
            Sachiel.build_order_template_id = string.Empty;
            Sachiel.show_on_meta_layer = false;
            Sachiel.show_in_knowledge_window = false;
            Sachiel.show_in_taxonomy_tooltip = false;
            Sachiel.render_status_effects = true;
            Sachiel.use_phenotypes = false;
            Sachiel.death_animation_angle = true;
            Sachiel.can_be_inspected = true;
            Sachiel.name_template_sets = AssetLibrary<ActorAsset>.a<string>("evil_mage_set");
            Sachiel.kingdom_id_wild = "Angel_Apostle";
            Sachiel.power_id = "spawnSachiel";
            Sachiel.update_z = true;
            Sachiel.has_baby_form = false;
            Sachiel.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Sachiel.addDecision("random_move_towards_civ_building");
            Sachiel.addDecision("boss_attack_animation_decision");
            Sachiel.addDecision("kaiju_special_attack_decision");
            Sachiel.addDecision("kaiju_mapwide_aggro_override_decision");
            Sachiel.base_stats["lifespan"] = 20000f;
            Sachiel.base_stats["mass_2"] = 100000f;
            Sachiel.base_stats["mass"] = 2000f;
            Sachiel.base_stats["stamina"] = 500f;
            Sachiel.base_stats["scale"] = 0.2f;
            Sachiel.base_stats["size"] = 2f;
            Sachiel.base_stats["health"] = 16000f;
            Sachiel.base_stats["speed"] = 40f;
            Sachiel.base_stats["armor"] = 80f;
            Sachiel.base_stats["attack_speed"] = 4f;
            Sachiel.base_stats["damage"] = 1000f;
            Sachiel.base_stats["knockback"] = 4f;
            Sachiel.base_stats["accuracy"] = 1f;
            Sachiel.base_stats["targets"] = 100f;
            Sachiel.base_stats["area_of_effect"] = 5f;
            Sachiel.base_stats["range"] = 12f;
            Sachiel.base_stats["critical_damage_multiplier"] = 10f;
            Sachiel.base_stats["multiplier_supply_timer"] = 1f;
            Sachiel.disable_jump_animation = true;
            Sachiel.can_be_moved_by_powers = true;
            Sachiel.actor_size = ActorSize.S16_Buffalo;
            Sachiel.animation_walk = Kaiju.walk_0_5;
            Sachiel.animation_idle = Kaiju.idle_0_4;
            Sachiel.animation_swim = ActorAnimationSequences.swim_0_3;
            Sachiel.can_flip = true;
            Sachiel.check_flip = (BaseSimObject _, WorldTile _) => true;
            Sachiel.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Sachiel/", false);
            Sachiel.icon = "Sachiel_avatar";
            Sachiel.die_in_lava = false;
            Sachiel.visible_on_minimap = true;
            Sachiel.experience_given = 1000000;
            Sachiel.can_have_subspecies = false;
            Sachiel.affected_by_dust = false;
            Sachiel.inspect_children = false;
            Sachiel.special = true;
            Sachiel.has_advanced_textures = false;
            Sachiel.inspect_sex = false;
            Sachiel.inspect_show_species = false;
            Sachiel.inspect_generation = false;
            Sachiel.needs_to_be_explored = false;
            Sachiel.force_land_creature = true;
            Sachiel.color_hex = "#679ead";
            Sachiel.addTrait("chosen_one");
            Sachiel.addTrait("regeneration");
            Sachiel.addTrait("blessed");
            Sachiel.addTrait("fire_proof");
            Sachiel.addTrait("bubble_defense");
            Sachiel.addResource("adamantine", 100);
            Sachiel.addResource("gold", 2000);
            AssetManager.actor_library.add(Sachiel);
            Localization.addLocalization(Sachiel.name_locale, Sachiel.name_locale);
            Localization.addLocalization("Sachiel", Sachiel.name_locale);
            Localization.addLocalization("Sachiel", Sachiel.name_locale);
            Localization.addLocalization("spawnSachiel", Sachiel.name_locale);
            Localization.addLocalization("spawnSachiel_description", "Thunder of God");



            var Zeruel = AssetManager.actor_library.clone("Zeruel", "$mob$");
            Zeruel.is_humanoid = false;
            Zeruel.civ = false;
            Zeruel.has_baby_form = false;
            Zeruel.name_locale = "Zeruel";
            Zeruel.animation_speed_based_on_walk_speed = false;
            Zeruel.has_avatar_prefab = false;
            Zeruel.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/Zeruel_avatar") };
            Zeruel.has_override_avatar_frames = true;
            Zeruel.inspect_avatar_scale = 1f;
            Zeruel.inspect_avatar_offset_y = 6f;
            Zeruel.shadow_texture = "unitShadow_6";
            Zeruel.immune_to_slowness = true;
            Zeruel.effect_damage = true;
            Zeruel.unit_other = true;
            Zeruel.collective_term = "group_den";
            Zeruel.default_attack = "base_attack";
            Zeruel.affected_by_dust = false;
            Zeruel.kingdom_id_civilization = string.Empty;
            Zeruel.build_order_template_id = string.Empty;
            Zeruel.show_on_meta_layer = false;
            Zeruel.show_in_knowledge_window = false;
            Zeruel.show_in_taxonomy_tooltip = false;
            Zeruel.render_status_effects = true;
            Zeruel.use_phenotypes = false;
            Zeruel.death_animation_angle = true;
            Zeruel.can_be_inspected = true;
            Zeruel.name_template_sets = AssetLibrary<ActorAsset>.a<string>("evil_mage_set");
            Zeruel.kingdom_id_wild = "Angel_Apostle";
            Zeruel.power_id = "spawnZeruel";
            Zeruel.update_z = true;
            Zeruel.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            Zeruel.addDecision("random_move_towards_civ_building");
            Zeruel.addDecision("boss_attack_animation_decision");
            Zeruel.addDecision("kaiju_special_attack_decision");
            Zeruel.addDecision("kaiju_mapwide_aggro_override_decision");
            Zeruel.base_stats["lifespan"] = 20000f;
            Zeruel.base_stats["mass_2"] = 100000f;
            Zeruel.base_stats["mass"] = 2000f;
            Zeruel.base_stats["stamina"] = 500f;
            Zeruel.base_stats["scale"] = 0.2f;
            Zeruel.base_stats["size"] = 2f;
            Zeruel.base_stats["health"] = 16000f;
            Zeruel.base_stats["speed"] = 30f;
            Zeruel.base_stats["armor"] = 80f;
            Zeruel.base_stats["attack_speed"] = 1f;
            Zeruel.base_stats["damage"] = 1000f;
            Zeruel.base_stats["knockback"] = 4f;
            Zeruel.base_stats["accuracy"] = 1f;
            Zeruel.base_stats["targets"] = 100f;
            Zeruel.base_stats["area_of_effect"] = 5f;
            Zeruel.base_stats["range"] = 40f;
            Zeruel.base_stats["critical_damage_multiplier"] = 10f;
            Zeruel.base_stats["multiplier_supply_timer"] = 1f;
            Zeruel.disable_jump_animation = true;
            Zeruel.can_be_moved_by_powers = true;
            Zeruel.actor_size = ActorSize.S16_Buffalo;
            Zeruel.animation_walk = Kaiju.walk_0_5;
            Zeruel.animation_idle = Kaiju.walk_0_5;
            Zeruel.animation_swim = Kaiju.walk_0_5;
            Zeruel.can_flip = true;
            Zeruel.check_flip = (BaseSimObject _, WorldTile _) => true;
            Zeruel.texture_asset = new ActorTextureSubAsset("actors/Kaiju/Zeruel/", false);
            Zeruel.icon = "Zeruel_avatar";
            Zeruel.die_in_lava = false;
            Zeruel.visible_on_minimap = true;
            Zeruel.experience_given = 1000000;
            Zeruel.can_have_subspecies = false;
            Zeruel.affected_by_dust = false;
            Zeruel.inspect_children = false;
            Zeruel.special = true;
            Zeruel.has_advanced_textures = false;
            Zeruel.inspect_sex = false;
            Zeruel.inspect_show_species = false;
            Zeruel.inspect_generation = false;
            Zeruel.needs_to_be_explored = false;
            Zeruel.force_land_creature = true;
            Zeruel.color_hex = "#679ead";
            Zeruel.addTrait("chosen_one");
            Zeruel.addTrait("regeneration");
            Zeruel.addTrait("tough");
            Zeruel.addTrait("blessed");
            Zeruel.addTrait("fire_proof");
            Zeruel.addTrait("bubble_defense");
            Zeruel.flying = true;
            Zeruel.addResource("adamantine", 100);
            Zeruel.addResource("gold", 2000);
            AssetManager.actor_library.add(Zeruel);
            Localization.addLocalization(Zeruel.name_locale, Zeruel.name_locale);
            Localization.addLocalization("Zeruel", Zeruel.name_locale);
            Localization.addLocalization("Zeruel", Zeruel.name_locale);
            Localization.addLocalization("spawnZeruel", Zeruel.name_locale);
            Localization.addLocalization("spawnZeruel_description", "Thunder of God");












            EffectAsset redbigboom_mecha = new EffectAsset();
            redbigboom_mecha.id = "redbigboom_mecha";
            redbigboom_mecha.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb";
            redbigboom_mecha.use_basic_prefab = true;
            redbigboom_mecha.sorting_layer_id = "EffectsTop";
            redbigboom_mecha.sprite_path = "effects/redbigboom";
            redbigboom_mecha.draw_light_area = true;
            AssetManager.effects_library.add(redbigboom_mecha);




            EffectAsset MechaCrabBeam_trail = new EffectAsset();
            MechaCrabBeam_trail.id = "MechaCrabBeam_trail";
            MechaCrabBeam_trail.use_basic_prefab = true;
            MechaCrabBeam_trail.sorting_layer_id = "EffectsTop";
            MechaCrabBeam_trail.sprite_path = "effects/MechaCrabBeam_trail";
            MechaCrabBeam_trail.draw_light_area = true;
            MechaCrabBeam_trail.show_on_mini_map = true;
            MechaCrabBeam_trail.limit = 15;
            AssetManager.effects_library.add(MechaCrabBeam_trail);


            var MechaCrabBeamterra = AssetManager.terraform.clone("MechaCrabBeamterra", "bomb");
            MechaCrabBeamterra.damage = 3500;
            MechaCrabBeamterra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("AntiKaiju_Machine");
            MechaCrabBeamterra.explode_strength = 1;
            MechaCrabBeamterra.transform_to_wasteland = false;
            MechaCrabBeamterra.applies_to_high_flyers = true;
            MechaCrabBeamterra.shake = true;
            AssetManager.terraform.add(MechaCrabBeamterra);



            ProjectileAsset MechaCrabBeam = new ProjectileAsset();
            MechaCrabBeam.id = "MechaCrabBeam";
            MechaCrabBeam.speed = 60f;
            MechaCrabBeam.texture = "redplasma";
            MechaCrabBeam.trail_effect_enabled = true;
            MechaCrabBeam.trail_effect_id = "MechaCrabBeam_trail";
            MechaCrabBeam.trail_effect_scale = 0.25f;
            MechaCrabBeam.trail_effect_timer = 0.1f;
            MechaCrabBeam.texture_shadow = "shadows/projectiles/shadow_ball";
            MechaCrabBeam.terraform_option = "MechaCrabBeamterra";
            MechaCrabBeam.draw_light_area = true;
            MechaCrabBeam.terraform_range = 10;
            MechaCrabBeam.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            MechaCrabBeam.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            MechaCrabBeam.end_effect = "redbigboom_mecha";
            MechaCrabBeam.scale_start = 0.5f;
            MechaCrabBeam.scale_target = 0.5f;
            MechaCrabBeam.look_at_target = true;
            MechaCrabBeam.can_be_left_on_ground = false;
            MechaCrabBeam.can_be_blocked = false;
            MechaCrabBeam.world_actions = (AttackAction)Delegate.Combine(MechaCrabBeam.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(MechaCrabBeam);

            EquipmentAsset MechaCrabBeam_attack = AssetManager.items.clone("MechaCrabBeam_attack", "$range");
            MechaCrabBeam_attack.has_locales = false;
            MechaCrabBeam_attack.projectile = "MechaCrabBeam";
            MechaCrabBeam_attack.base_stats["projectiles"] = 2f;
            MechaCrabBeam_attack.path_slash_animation = "effects/slashes/slash_cannonball";
            MechaCrabBeam_attack.show_in_meta_editor = false;
            MechaCrabBeam_attack.show_in_knowledge_window = false;
            MechaCrabBeam_attack.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame", "stun");



            var MechaCrab_terra = AssetManager.terraform.clone("MechaCrab_terra", "bomb");
            MechaCrab_terra.damage = 3000;
            MechaCrab_terra.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("AntiKaiju_Machine");
            MechaCrab_terra.explode_strength = 1;
            MechaCrab_terra.transform_to_wasteland = true;
            MechaCrab_terra.applies_to_high_flyers = true;
            MechaCrab_terra.shake = true;
            AssetManager.terraform.add(MechaCrab_terra);


            ProjectileAsset Mecha_Artillery = new ProjectileAsset();
            Mecha_Artillery.id = "Mecha_Artillery";
            Mecha_Artillery.speed = 30f;
            Mecha_Artillery.texture = "mininuke";
            Mecha_Artillery.trail_effect_enabled = false;
            Mecha_Artillery.texture_shadow = "shadows/projectiles/shadow_ball";
            Mecha_Artillery.terraform_option = "MechaCrab_terra";
            Mecha_Artillery.draw_light_area = true;
            Mecha_Artillery.terraform_range = 3;
            Mecha_Artillery.sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart";
            Mecha_Artillery.sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand";
            Mecha_Artillery.end_effect = "N2explosion";
            Mecha_Artillery.scale_start = 0.3f;
            Mecha_Artillery.scale_target = 0.3f;
            Mecha_Artillery.look_at_target = true;
            Mecha_Artillery.can_be_left_on_ground = false;
            Mecha_Artillery.can_be_blocked = false;
            Mecha_Artillery.world_actions = (AttackAction)Delegate.Combine(Mecha_Artillery.world_actions, new AttackAction(ActionLibrary.burnTile));
            AssetManager.projectiles.add(Mecha_Artillery);



            EquipmentAsset Mecha_Artillery_Attack = AssetManager.items.clone("Mecha_Artillery_Attack", "$range");
            Mecha_Artillery_Attack.has_locales = false;
            Mecha_Artillery_Attack.projectile = "Mecha_Artillery";
            Mecha_Artillery_Attack.base_stats["projectiles"] = 4f;
            Mecha_Artillery_Attack.path_slash_animation = "effects/slashes/slash_cannonball";
            Mecha_Artillery_Attack.show_in_meta_editor = false;
            Mecha_Artillery_Attack.show_in_knowledge_window = false;



            var AntiKaiju_Machine = AssetManager.kingdoms.clone("AntiKaiju_Machine", "$TEMPLATE_MOB$");
            AntiKaiju_Machine.concept = false;
            AntiKaiju_Machine.id = "AntiKaiju_Machine";
            AntiKaiju_Machine.default_kingdom_color = new ColorAsset("#679ead");
            AntiKaiju_Machine.mobs = true;
            AntiKaiju_Machine.always_attack_each_other = false;
            AntiKaiju_Machine.force_look_all_chunks = true;
            AntiKaiju_Machine.setIcon("ui/Icons/mechacrabzilla_avatar");
            AntiKaiju_Machine.addTag("sliceable");
            AntiKaiju_Machine.addTag("Guardian");
            AntiKaiju_Machine.addEnemyTag("Angel");
            AntiKaiju_Machine.addFriendlyTag("nature_creature");
            AntiKaiju_Machine.addFriendlyTag("neutral_animals");
            AntiKaiju_Machine.addFriendlyTag("neutral");
            AntiKaiju_Machine.addFriendlyTag("civ");
            AntiKaiju_Machine.addEnemyTag("Kaiju");
            AssetManager.kingdoms.add(AntiKaiju_Machine);
            World.world.kingdoms_wild.newWildKingdom(AntiKaiju_Machine);



            var mechacrabzilla = AssetManager.actor_library.clone("mechacrabzilla", "$mob$");
            mechacrabzilla.is_humanoid = false;
            mechacrabzilla.civ = false;
            mechacrabzilla.has_baby_form = false;
            mechacrabzilla.name_locale = "mechacrabzilla";
            mechacrabzilla.animation_speed_based_on_walk_speed = false;
            mechacrabzilla.has_avatar_prefab = false;
            mechacrabzilla.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite("actors/Avatars/mechacrabzilla_avatar") };
            mechacrabzilla.has_override_avatar_frames = true;
            mechacrabzilla.inspect_avatar_scale = 1f;
            mechacrabzilla.inspect_avatar_offset_y = 6f;
            mechacrabzilla.shadow_texture = "unitShadow_6";
            mechacrabzilla.immune_to_slowness = true;
            mechacrabzilla.effect_damage = true;
            mechacrabzilla.unit_other = true;
            mechacrabzilla.collective_term = "group_den";
            mechacrabzilla.default_attack = "Mecha_Artillery_Attack";
            mechacrabzilla.affected_by_dust = false;
            mechacrabzilla.kingdom_id_civilization = string.Empty;
            mechacrabzilla.build_order_template_id = string.Empty;
            mechacrabzilla.show_on_meta_layer = false;
            mechacrabzilla.show_in_knowledge_window = false;
            mechacrabzilla.show_in_taxonomy_tooltip = false;
            mechacrabzilla.render_status_effects = true;
            mechacrabzilla.use_phenotypes = false;
            mechacrabzilla.death_animation_angle = true;
            mechacrabzilla.can_be_inspected = true;
            mechacrabzilla.name_template_sets = AssetLibrary<ActorAsset>.a<string>("assimilator_set");
            mechacrabzilla.kingdom_id_wild = "AntiKaiju_Machine";
            mechacrabzilla.power_id = "spawnmechacrabzilla";
            mechacrabzilla.update_z = true;
            mechacrabzilla.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            mechacrabzilla.addDecision("random_move_towards_civ_building");
            mechacrabzilla.addDecision("boss_attack_animation_decision");
            mechacrabzilla.addDecision("kaiju_special_attack_decision");
            mechacrabzilla.addDecision("kaiju_mapwide_aggro_override_decision");
            mechacrabzilla.addDecision(KaijuBurrowDecisionId);
            mechacrabzilla.base_stats["lifespan"] = 20000f;
            mechacrabzilla.base_stats["mass_2"] = 100000f;
            mechacrabzilla.base_stats["mass"] = 2000f;
            mechacrabzilla.base_stats["stamina"] = 500f;
            mechacrabzilla.base_stats["scale"] = 0.3f;
            mechacrabzilla.base_stats["size"] = 3f;
            mechacrabzilla.base_stats["health"] = 55000f;
            mechacrabzilla.base_stats["speed"] = 60f;
            mechacrabzilla.base_stats["armor"] = 80f;
            mechacrabzilla.base_stats["attack_speed"] = 0.3f;
            mechacrabzilla.base_stats["damage"] = 5000f;
            mechacrabzilla.base_stats["knockback"] = 10f;
            mechacrabzilla.base_stats["accuracy"] = 0.1f;
            mechacrabzilla.base_stats["targets"] = 100f;
            mechacrabzilla.base_stats["area_of_effect"] = 10f;
            mechacrabzilla.base_stats["range"] = 100f;
            mechacrabzilla.base_stats["critical_damage_multiplier"] = 10f;
            mechacrabzilla.base_stats["multiplier_supply_timer"] = 1f;
            mechacrabzilla.disable_jump_animation = true;
            mechacrabzilla.can_be_moved_by_powers = true;
            mechacrabzilla.actor_size = ActorSize.S16_Buffalo;
            mechacrabzilla.animation_walk = Kaiju.walk_0_5;
            mechacrabzilla.animation_idle = ActorAnimationSequences.walk_0;
            mechacrabzilla.animation_swim = Kaiju.swim_0_5;
            mechacrabzilla.can_flip = true;
            mechacrabzilla.check_flip = (BaseSimObject _, WorldTile _) => true;
            mechacrabzilla.texture_asset = new ActorTextureSubAsset("actors/Kaiju/mechacrabzilla/", false);
            mechacrabzilla.icon = "mechacrabzilla_avatar";
            mechacrabzilla.die_in_lava = false;
            mechacrabzilla.visible_on_minimap = true;
            mechacrabzilla.experience_given = 1000000;
            mechacrabzilla.can_have_subspecies = false;
            mechacrabzilla.affected_by_dust = false;
            mechacrabzilla.inspect_children = false;
            mechacrabzilla.special = true;
            mechacrabzilla.has_advanced_textures = false;
            mechacrabzilla.inspect_sex = false;
            mechacrabzilla.inspect_show_species = false;
            mechacrabzilla.inspect_generation = false;
            mechacrabzilla.needs_to_be_explored = false;
            mechacrabzilla.force_land_creature = true;
            mechacrabzilla.color_hex = "#679ead";
            mechacrabzilla.immune_to_injuries = true;
            mechacrabzilla.addTrait("strong");
            mechacrabzilla.addTrait("tough");
            mechacrabzilla.addTrait("blessed");
            mechacrabzilla.addTrait("fire_proof");
            mechacrabzilla.addTrait("poison_immune");
            mechacrabzilla.addTrait("immune");
            mechacrabzilla.addTrait("hotheaded");
            mechacrabzilla.addTrait("death_nuke");
            mechacrabzilla.addResource("adamantine", 100);
            mechacrabzilla.addResource("gold", 2000);
            AssetManager.actor_library.add(mechacrabzilla);
            Localization.addLocalization(mechacrabzilla.name_locale, mechacrabzilla.name_locale);
            Localization.addLocalization("mechacrabzilla", mechacrabzilla.name_locale);
            Localization.addLocalization("mechacrabzilla", mechacrabzilla.name_locale);
            Localization.addLocalization("spawnmechacrabzilla", mechacrabzilla.name_locale);
            Localization.addLocalization("spawnmechacrabzilla_description", "Thunder of God");

            RegisterArchiveKaijus();













          }








          public static readonly string[] idle_0 = Toolbox.a<string>("idle_0");

          public static readonly string[] idle_0_4 = Toolbox.a<string>( "idle_0", "idle_1", "idle_2", "idle_3", "idle_4" );

          public static readonly string[] idle_0_6 = Toolbox.a<string>( "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6" );

          public static readonly string[] walk_0_4 = Toolbox.a<string>( "walk_0", "walk_1", "walk_2", "walk_3", "walk_4" );

          public static readonly string[] walk_0_5 = Toolbox.a<string>( "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" );

          public static readonly string[] swim_0_4 = Toolbox.a<string>( "swim_0", "swim_1", "swim_2", "swim_3", "swim_4" );

          public static readonly string[] swim_0_5 = Toolbox.a<string>( "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5" );

          public static readonly string[] swim_0_19 = Toolbox.a<string>( "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7", "swim_8", "swim_9", "swim_10", "swim_11", "swim_12", "swim_13", "swim_14", "swim_15", "swim_16", "swim_17", "swim_18", "swim_19" );

          public static readonly string[] walk_0_14 = Toolbox.a<string>( "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" , "walk_6", "walk_7", "walk_8", "walk_9", "walk_10" , "walk_11", "walk_12", "walk_13", "walk_14" );





        private sealed class ArchiveKaijuDefinition
        {
            public string Id;
            public string DisplayName;
            public string TextureFolderName;
            public string Description;
            public int WalkFrames;
            public int SwimFrames;
            public float WorldboxScale;
            public float WorldboxSize;
            public bool AggressiveToHumanoids = true;

            public string IconPath
            {
                get
                {
                    string folder = string.IsNullOrWhiteSpace(TextureFolderName) ? Id : TextureFolderName;
                    return $"actors/Kaiju/{folder}/main/walk_0";
                }
            }
        }

        private static readonly ArchiveKaijuDefinition[] AdditionalArchiveKaijuDefinitions = new ArchiveKaijuDefinition[]
        {
            new ArchiveKaijuDefinition { Id = "Anguirus", DisplayName = "Anguirus", TextureFolderName = "Anguirus", Description = "Armored archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.14f, WorldboxSize = 1.35f },
            new ArchiveKaijuDefinition { Id = "Bagan", DisplayName = "Bagan", TextureFolderName = "Bagan", Description = "Ancient archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.24f, WorldboxSize = 2.4f },
            new ArchiveKaijuDefinition { Id = "Battra", DisplayName = "Battra", TextureFolderName = "Battra", Description = "Dark moth archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.16f, WorldboxSize = 1.6f },
            new ArchiveKaijuDefinition { Id = "BigBiolante", DisplayName = "Big Biolante", TextureFolderName = "BigBiolante", Description = "Overgrown archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.28f, WorldboxSize = 2.8f },
            new ArchiveKaijuDefinition { Id = "QueenMuto", DisplayName = "Queen MUTO", TextureFolderName = "QueenMuto", Description = "Titanic parasite archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.22f, WorldboxSize = 2.2f },
            new ArchiveKaijuDefinition { Id = "Crystalac", DisplayName = "Crystalac", TextureFolderName = "Crystalac", Description = "Crystal archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.15f, WorldboxSize = 1.5f },
            new ArchiveKaijuDefinition { Id = "Desghidorah", DisplayName = "Desghidorah", TextureFolderName = "Desghidorah", Description = "Void dragon archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.20f, WorldboxSize = 2.0f },
            new ArchiveKaijuDefinition { Id = "Destoroyah", DisplayName = "Destoroyah", TextureFolderName = "Destoroyah", Description = "Demonic archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.22f, WorldboxSize = 2.1f },
            new ArchiveKaijuDefinition { Id = "Gamera", DisplayName = "Gamera", TextureFolderName = "Gamera", Description = "Guardian archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.18f, WorldboxSize = 1.8f },
            new ArchiveKaijuDefinition { Id = "GiantSquid", DisplayName = "Giant Squid", TextureFolderName = "GiantSquid", Description = "Abyssal archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.17f, WorldboxSize = 1.7f },
            new ArchiveKaijuDefinition { Id = "GiganOld", DisplayName = "Gigan Old", TextureFolderName = "GiganOld", Description = "Retro cyborg archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.18f, WorldboxSize = 1.75f },
            new ArchiveKaijuDefinition { Id = "GoodGodzilla", DisplayName = "Good Godzilla", TextureFolderName = "GoodGodzilla", Description = "Heroic archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.17f, WorldboxSize = 1.7f, AggressiveToHumanoids = false },
            new ArchiveKaijuDefinition { Id = "Hedorah", DisplayName = "Hedorah", TextureFolderName = "Hedorah", Description = "Toxic archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.19f, WorldboxSize = 1.9f },
            new ArchiveKaijuDefinition { Id = "Iris", DisplayName = "Iris", TextureFolderName = "Iris", Description = "Organic archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.19f, WorldboxSize = 1.85f },
            new ArchiveKaijuDefinition { Id = "KiryuMech", DisplayName = "Kiryu Mech", TextureFolderName = "KiryuMech", Description = "Mechanical archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.18f, WorldboxSize = 1.8f },
            new ArchiveKaijuDefinition { Id = "Kong", DisplayName = "Kong", TextureFolderName = "Kong", Description = "Titan ape archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.18f, WorldboxSize = 1.75f },
            new ArchiveKaijuDefinition { Id = "Legion", DisplayName = "Legion", TextureFolderName = "Legion", Description = "Hive archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.22f, WorldboxSize = 2.0f },
            new ArchiveKaijuDefinition { Id = "LpgKaiju", DisplayName = "LPG Kaiju", TextureFolderName = "LpgKaiju", Description = "Archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.15f, WorldboxSize = 1.5f },
            new ArchiveKaijuDefinition { Id = "MechaGhidorah", DisplayName = "Mecha Ghidorah", TextureFolderName = "MechaGhidorah", Description = "Cyber dragon archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.20f, WorldboxSize = 2.0f },
            new ArchiveKaijuDefinition { Id = "Megalon", DisplayName = "Megalon", TextureFolderName = "Megalon", Description = "Burrowing archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.17f, WorldboxSize = 1.7f },
            new ArchiveKaijuDefinition { Id = "OldMechagodzilla", DisplayName = "Old Mechagodzilla", TextureFolderName = "OldMechagodzilla", Description = "Classic mechanical archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.19f, WorldboxSize = 1.9f },
            new ArchiveKaijuDefinition { Id = "Shimo", DisplayName = "Shimo", TextureFolderName = "Shimo", Description = "Frozen archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.26f, WorldboxSize = 2.6f },
            new ArchiveKaijuDefinition { Id = "SkerBuffalo", DisplayName = "Sker Buffalo", TextureFolderName = "SkerBuffalo", Description = "Horned archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.14f, WorldboxSize = 1.4f },
            new ArchiveKaijuDefinition { Id = "FemaleMuto", DisplayName = "Female MUTO", TextureFolderName = "FemaleMuto", Description = "Winged archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.19f, WorldboxSize = 1.9f },
            new ArchiveKaijuDefinition { Id = "MaleMuto", DisplayName = "Male MUTO", TextureFolderName = "MaleMuto", Description = "Swift archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.14f, WorldboxSize = 1.4f },
            new ArchiveKaijuDefinition { Id = "SpaceGodzilla", DisplayName = "SpaceGodzilla", TextureFolderName = "SpaceGodzilla", Description = "Cosmic archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.21f, WorldboxSize = 2.1f },
            new ArchiveKaijuDefinition { Id = "SporeMantis", DisplayName = "Spore Mantis", TextureFolderName = "SporeMantis", Description = "Fungal archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.15f, WorldboxSize = 1.5f },
            new ArchiveKaijuDefinition { Id = "SuperMechagodzilla", DisplayName = "Super Mechagodzilla", TextureFolderName = "SuperMechagodzilla", Description = "Weaponized archive kaiju", WalkFrames = 6, SwimFrames = 6, WorldboxScale = 0.22f, WorldboxSize = 2.15f }
        };

        internal static IEnumerable<ArchiveKaijuSpawnEntry> GetArchiveKaijuSpawnEntries()
        {
            for (int i = 0; i < AdditionalArchiveKaijuDefinitions.Length; i++)
            {
                ArchiveKaijuDefinition definition = AdditionalArchiveKaijuDefinitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                {
                    continue;
                }

                yield return new ArchiveKaijuSpawnEntry
                {
                    PowerId = "spawn" + definition.Id,
                    ActorId = definition.Id,
                    DisplayName = definition.DisplayName,
                    Description = definition.Description,
                    IconPath = definition.IconPath
                };
            }
        }

        private static void RegisterArchiveKaijus()
        {
            for (int i = 0; i < AdditionalArchiveKaijuDefinitions.Length; i++)
            {
                RegisterArchiveKaiju(AdditionalArchiveKaijuDefinitions[i]);
            }
        }

        private static void RegisterArchiveKaiju(ArchiveKaijuDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
            {
                return;
            }

            if (AssetManager.actor_library.get(definition.Id) != null)
            {
                return;
            }

            string textureFolder = string.IsNullOrWhiteSpace(definition.TextureFolderName)
                ? definition.Id
                : definition.TextureFolderName;
            string wildKingdomId = definition.Id + "_wild";

            if (AssetManager.kingdoms.get(wildKingdomId) == null)
            {
                var wildKingdom = AssetManager.kingdoms.clone(wildKingdomId, "$TEMPLATE_ANIMAL$");
                if (wildKingdom != null)
                {
                    wildKingdom.concept = false;
                    wildKingdom.id = wildKingdomId;
                    wildKingdom.default_kingdom_color = new ColorAsset("#679ead");
                    wildKingdom.units_always_looking_for_enemies = true;
                    wildKingdom.force_look_all_chunks = true;
                    wildKingdom.setIcon(definition.IconPath);
                    wildKingdom.addTag("sliceable");
                    wildKingdom.addTag("nature_creature");
                    wildKingdom.addFriendlyTag("nature_creature");
                    wildKingdom.addTag("neutral_animals");
                    wildKingdom.addTag("neutral");
                    wildKingdom.addTag("Kaiju");
                    wildKingdom.addTag(wildKingdomId);
                    if (definition.AggressiveToHumanoids)
                    {
                        wildKingdom.addEnemyTag("civ");
                    }
                    wildKingdom.addEnemyTag("Kaiju");
                    AssetManager.kingdoms.add(wildKingdom);

                    if (World.world?.kingdoms_wild != null)
                    {
                        World.world.kingdoms_wild.newWildKingdom(wildKingdom);
                    }
                }
            }

            var actor = AssetManager.actor_library.clone(definition.Id, "$mob$");
            if (actor == null)
            {
                return;
            }

            actor.is_humanoid = false;
            actor.civ = false;
            actor.name_locale = definition.DisplayName;
            actor.animation_speed_based_on_walk_speed = false;
            actor.has_avatar_prefab = false;
            actor.get_override_avatar_frames = pActor => LoadArchiveKaijuAvatar(definition);
            actor.has_override_avatar_frames = true;
            actor.inspect_avatar_scale = 1f;
            actor.inspect_avatar_offset_y = 6f;
            actor.shadow_texture = "unitShadow_6";
            actor.immune_to_slowness = true;
            actor.effect_damage = true;
            actor.unit_other = true;
            actor.collective_term = "group_den";
            actor.setSocialStructure("group_den", 10);
            actor.default_attack = "base_attack";
            actor.inspect_children = false;
            actor.kingdom_id_civilization = string.Empty;
            actor.build_order_template_id = string.Empty;
            actor.show_on_meta_layer = true;
            actor.show_in_knowledge_window = true;
            actor.show_in_taxonomy_tooltip = true;
            actor.render_status_effects = true;
            actor.use_phenotypes = false;
            actor.death_animation_angle = true;
            actor.can_be_inspected = true;
            actor.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
            actor.kingdom_id_wild = wildKingdomId;
            actor.update_z = true;
            actor.job = AssetLibrary<ActorAsset>.a<string>("attacker");
            actor.base_stats["lifespan"] = 200f;
            actor.base_stats["mass_2"] = 100000f;
            actor.base_stats["mass"] = 2000f;
            actor.base_stats["stamina"] = 500f;
            actor.base_stats["scale"] = definition.WorldboxScale > 0f ? definition.WorldboxScale : 0.12f;
            actor.base_stats["size"] = definition.WorldboxSize > 0f ? definition.WorldboxSize : 1.5f;
            actor.base_stats["health"] = 1800f;
            actor.base_stats["speed"] = 35f;
            actor.base_stats["armor"] = 18f;
            actor.base_stats["attack_speed"] = 0.6f;
            actor.base_stats["damage"] = 900f;
            actor.base_stats["knockback"] = 4f;
            actor.base_stats["accuracy"] = 1f;
            actor.base_stats["targets"] = 8f;
            actor.base_stats["area_of_effect"] = 4f;
            actor.base_stats["range"] = 6f;
            actor.base_stats["critical_damage_multiplier"] = 6f;
            actor.base_stats["multiplier_supply_timer"] = 1f;
            actor.disable_jump_animation = true;
            actor.can_be_moved_by_powers = true;
            actor.actor_size = ActorSize.S16_Buffalo;
            actor.animation_walk = BuildAnimationFrames("walk", definition.WalkFrames);
            actor.animation_idle = ActorAnimationSequences.walk_0;
            actor.animation_swim = BuildAnimationFrames("swim", definition.SwimFrames > 0 ? definition.SwimFrames : definition.WalkFrames);
            actor.can_flip = true;
            actor.check_flip = (BaseSimObject _, WorldTile _) => true;
            actor.texture_asset = new ActorTextureSubAsset($"actors/Kaiju/{textureFolder}/", false);
            actor.icon = definition.Id;
            actor.die_in_lava = false;
            actor.visible_on_minimap = true;
            actor.experience_given = 20;
            actor.can_have_subspecies = false;
            actor.special = true;
            actor.has_advanced_textures = false;
            actor.inspect_sex = false;
            actor.inspect_show_species = true;
            actor.inspect_generation = false;
            actor.needs_to_be_explored = false;
            actor.force_land_creature = true;
            actor.has_baby_form = false;
            actor.can_evolve_into_new_species = false;
            actor.color_hex = "#679ead";
            actor.addTrait("tough");
            actor.addTrait("regeneration");
            actor.addTrait("fire_proof");
            actor.name_taxonomic_kingdom = "animalia";
            actor.name_taxonomic_phylum = "chordata";
            actor.name_taxonomic_class = "reptilia";
            actor.name_taxonomic_order = "Archosauria";
            actor.name_taxonomic_family = "Titanus";
            actor.name_taxonomic_genus = definition.Id;
            actor.addResource("adamantine", 2);
            actor.addResource("gold", 10);
            actor.source_meat = true;

            AssetManager.actor_library.add(actor);
            Localization.addLocalization(actor.name_locale, actor.name_locale);
            Localization.addLocalization(definition.Id, actor.name_locale);
            Localization.addLocalization("spawn" + definition.Id, actor.name_locale);
            Localization.addLocalization("spawn" + definition.Id + "_description", definition.Description);
        }

        private static Sprite[] LoadArchiveKaijuAvatar(ArchiveKaijuDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            Sprite sprite = SpriteTextureLoader.getSprite(definition.IconPath);
            if (sprite == null)
            {
                string folder = string.IsNullOrWhiteSpace(definition.TextureFolderName) ? definition.Id : definition.TextureFolderName;
                sprite = SpriteTextureLoader.getSprite($"actors/Kaiju/{folder}/main/swim_0");
            }

            return sprite == null ? null : new[] { sprite };
        }

        private static string[] BuildAnimationFrames(string prefix, int frameCount)
        {
            int safeCount = Mathf.Max(1, frameCount);
            string[] frames = new string[safeCount];
            for (int i = 0; i < safeCount; i++)
            {
                frames[i] = prefix + "_" + i;
            }

            return frames;
        }

    private static bool KaijuSpecialAttackDecisionEffect(Actor actor)
    {
        return ExecuteKaijuSpecialAttack(actor, pFromPossessionKick: false);
    }

    private static bool KaijuMapwideAggroOverrideDecisionEffect(Actor actor)
    {
        if (actor == null || !actor.isAlive() || actor.isEgg() || actor.isBaby())
        {
            return false;
        }

        if (IsKaijuBurrowBusy(actor))
        {
            return false;
        }

        Actor target = GetMapwideAggroOverrideTarget(actor);
        if (target == null)
        {
            return false;
        }

        ApplyMapwideAggroOverride(actor, target);
        return true;
    }

    private static Actor GetMapwideAggroOverrideTarget(Actor actor)
    {
        if (actor == null || !actor.isAlive())
        {
            return null;
        }

        Actor currentTarget = GetCurrentEnemyActorTarget(actor, float.MaxValue);
        if (IsValidMapwideTarget(actor, currentTarget))
        {
            CacheMapwideTarget(actor, currentTarget);
            return currentTarget;
        }

        if (TryGetCachedMapwideTarget(actor, out Actor cachedTarget))
        {
            return cachedTarget;
        }

        bool hasSubspecies = actor.hasSubspecies();
        bool isAngelTagged = HasKingdomTag(actor, "Angel");
        bool isKaijuTagged = HasKingdomTag(actor, "Kaiju");

        if (hasSubspecies)
        {
            UpdateKaijuAlphaState(actor);
            if (!IsKaijuAlpha(actor))
            {
                Actor nonAlphaFallback = FindMapwideTarget(actor, candidate => HasKingdomTag(candidate, "Kaiju") || HasKingdomTag(candidate, "Angel"));
                if (nonAlphaFallback != null)
                {
                    CacheMapwideTarget(actor, nonAlphaFallback);
                    return nonAlphaFallback;
                }

                Actor finalFallback = FindMapwideTarget(actor, candidate => true);
                CacheMapwideTarget(actor, finalFallback);
                return finalFallback;
            }

            Actor alphaPreferred = FindMapwideTarget(actor, candidate => IsMobSubspeciesAlpha(candidate) || HasKingdomTag(candidate, "Angel"));
            if (alphaPreferred != null)
            {
                CacheMapwideTarget(actor, alphaPreferred);
                return alphaPreferred;
            }

            Actor alphaFallback = FindMapwideTarget(actor, candidate => HasKingdomTag(candidate, "Kaiju") || HasKingdomTag(candidate, "Angel"));
            if (alphaFallback != null)
            {
                CacheMapwideTarget(actor, alphaFallback);
                return alphaFallback;
            }

            Actor fallback = FindMapwideTarget(actor, candidate => true);
            CacheMapwideTarget(actor, fallback);
            return fallback;
        }

        if (isAngelTagged)
        {
            Actor angelTarget = FindMapwideTarget(actor, candidate => HasKingdomTag(candidate, "Kaiju"));
            CacheMapwideTarget(actor, angelTarget);
            return angelTarget;
        }

        if (isKaijuTagged)
        {
            Actor kaijuPreferred = FindMapwideTarget(actor, candidate => HasKingdomTag(candidate, "Kaiju") || HasKingdomTag(candidate, "Angel") || IsMobSubspeciesAlpha(candidate));
            if (kaijuPreferred != null)
            {
                CacheMapwideTarget(actor, kaijuPreferred);
                return kaijuPreferred;
            }

            Actor fallback = FindMapwideTarget(actor, candidate => true);
            CacheMapwideTarget(actor, fallback);
            return fallback;
        }

        if (!isKaijuTagged)
        {
            Actor outsiderTarget = FindMapwideTarget(actor, candidate => IsMobSubspeciesAlpha(candidate) || HasKingdomTag(candidate, "Angel"));
            CacheMapwideTarget(actor, outsiderTarget);
            return outsiderTarget;
        }

        return null;
    }

    private static Actor FindMapwideTarget(Actor actor, Func<Actor, bool> targetFilter)
    {
        if (actor == null || !actor.isAlive() || targetFilter == null || World.world?.units == null)
        {
            return null;
        }

        RefreshActorCachesIfNeeded();
        Actor best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < CachedLivingActors.Count; i++)
        {
            Actor unit = CachedLivingActors[i];
            if (unit == null || unit == actor || !unit.isAlive())
            {
                continue;
            }

            if (IsKaijuBurrowed(unit))
            {
                continue;
            }

            if (!actor.areFoes(unit))
            {
                continue;
            }

            if (!targetFilter(unit))
            {
                continue;
            }

            float dist = Vector2.Distance(actor.current_position, unit.current_position);
            if (dist >= bestDist)
            {
                continue;
            }

            best = unit;
            bestDist = dist;
        }

        return best;
    }

    private static bool IsMobSubspeciesAlpha(Actor actor)
    {
        if (actor == null || !actor.isAlive() || !actor.hasSubspecies() || !HasKaijuAlphaDecision(actor))
        {
            return false;
        }

        return IsOldestManagedKaijuInSubspecies(actor);
    }

    private static bool HasKingdomTag(Actor actor, string tag)
    {
        return actor != null
            && actor.kingdom != null
            && actor.kingdom.asset != null
            && actor.kingdom.asset.list_tags != null
            && !string.IsNullOrEmpty(tag)
            && actor.kingdom.asset.list_tags.Contains(tag);
    }

    private static void ApplyMapwideAggroOverride(Actor actor, Actor target)
    {
        if (actor == null || target == null)
        {
            return;
        }

        bool alreadyTargeting = actor.has_attack_target
            && actor.attack_target != null
            && actor.isEnemyTargetAlive()
            && ReferenceEquals(actor.attack_target.a, target);

        if (!alreadyTargeting)
        {
            AccessTools.Method(typeof(BaseSimObject), "clearIgnoreTargets")?.Invoke(actor, null);

            if (World.world?.units != null)
            {
                MethodInfo ignoreTargetMethod = AccessTools.Method(typeof(BaseSimObject), "ignoreTarget");
                if (ignoreTargetMethod != null)
                {
                    RefreshActorCachesIfNeeded();
                    for (int i = 0; i < CachedLivingActors.Count; i++)
                    {
                        Actor unit = CachedLivingActors[i];
                        if (unit == null || unit == actor || unit == target || !unit.isAlive())
                        {
                            continue;
                        }

                        if (!actor.areFoes(unit))
                        {
                            continue;
                        }

                        ignoreTargetMethod.Invoke(actor, new object[] { unit });
                    }
                }
            }

            actor.finishAngryStatus();
            actor.addAggro(target);
            actor.setAttackTarget(target);
            actor.startFightingWith(target);
        }

        CacheMapwideTarget(actor, target);

        if (target.current_tile != null)
        {
            float desiredEngageDistance = Mathf.Clamp(actor.getAttackRange() * 0.35f, 10f, 24f);
            float distToTargetTile = Vector2.Distance(actor.current_position, target.current_tile.pos);
            if (distToTargetTile > desiredEngageDistance)
            {
                ExecuteEvent moveEvent = actor.goTo(target.current_tile);
                if (moveEvent == ExecuteEvent.False)
                {
                    actor.goTo(target.current_tile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 0);
                }
            }
        }
    }

    private static readonly Dictionary<string, string> KaijuSpecialAttackAssetByActorId = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        { "Gojira", "AtomBeam_attack" },
        { "Longlegder", "spida_attack" },
        { "Invaderax", "Ghido_attack" },
        { "Rodanix", "Fiery_attack" },
        { "PanKong", "BigBigMassiveBoulder_attack" },
        { "MegaGojira", "purpleAtomBeam_attack" },
        { "crabzilord", "CrabLordPOWER_attack" },
        { "Sachiel", "Angel_massacre" },
        { "Zeruel", "Angel_massacre" },
        { "Ramiel", "ramiel_exterminatus_attack" },
        { "mechacrabzilla", "MechaCrabBeam_attack" }

    };

    private const float KaijuSpecialTargetRange = 100f;
    private const string KaijuBurrowDecisionId = "kaiju_crab_burrow_decision";
    private const float KaijuBurrowLowHealthThreshold = 0.5f;
    private const float KaijuBurrowThreatRange = 30f;
    private const float KaijuBurrowSafeRange = 80f;
    private const float KaijuBurrowRetreatDistance = 100f;
    private const float KaijuBurrowRetreatTimeoutSeconds = 8f;
    private const float KaijuBurrowHealTickSeconds = 1f;
    private const float KaijuBurrowHealPerTick = 0.01f;
    private const float KaijuBurrowRandomChance = 0.02f;
    private const float KaijuBurrowRandomMinDurationSeconds = 10f;
    private const float KaijuBurrowRandomLockoutAfterExitSeconds = 20f;
    private const float KaijuForcedAdvanceDurationSeconds = 8f;
    private const float KaijuForcedAdvanceMoveIntervalSeconds = 0.5f;
    private const float KaijuForcedAdvanceDistanceMin = 6f;
    private const float KaijuForcedAdvanceDistanceMax = 18f;
    private const float KaijuPursuitDistanceMin = 10f;
    private const float KaijuPursuitDistanceMax = 24f;
    private const float KaijuSpecialPursuitMoveIntervalSeconds = 0.75f;
    private const float KaijuActorCacheRefreshSeconds = 1.5f;
    private const float KaijuMapwideRetargetCooldownSeconds = 1.5f;
    private const string KaijuSpecialPursuitNextMoveKey = "mb_kaiju_special_pursuit_next_move";
    private const string KaijuForcedAdvanceUntilKey = "mb_kaiju_forced_advance_until";
    private const string KaijuForcedAdvanceTileXKey = "mb_kaiju_forced_advance_tile_x";
    private const string KaijuForcedAdvanceTileYKey = "mb_kaiju_forced_advance_tile_y";
    private const string KaijuForcedAdvanceNextMoveKey = "mb_kaiju_forced_advance_next_move";
    private const string KaijuMapwideTargetIdKey = "mb_kaiju_mapwide_target_id";
    private const string KaijuMapwideRetargetAtKey = "mb_kaiju_mapwide_retarget_at";
    private const string KaijuAlphaPromotionRefillPendingKey = "mb_kaiju_alpha_promotion_refill_pending";
    private const string KaijuAlphaScalePreBonusKey = "mb_kaiju_alpha_scale_pre_bonus";
    private const string KaijuAlphaScaleBonusAppliedKey = "mb_kaiju_alpha_scale_bonus_applied";
    private const float KaijuAlphaScaleBonus = 0.2f;
    private const string KaijuBurrowStateKey = "mb_kaiju_burrowed";
    private const string KaijuBurrowRetreatStateKey = "mb_kaiju_burrow_retreat";
    private const string KaijuBurrowRetreatStartKey = "mb_kaiju_burrow_retreat_start";
    private const string KaijuBurrowNextHealKey = "mb_kaiju_burrow_next_heal";
    private const string KaijuBurrowMinUntilKey = "mb_kaiju_burrow_min_until";
    private const string KaijuBurrowRandomLockUntilKey = "mb_kaiju_burrow_random_lock";

    private static readonly HashSet<string> KaijuBurrowActorIds = new HashSet<string>(StringComparer.Ordinal)
    {
        "crabzilord",
        "mechacrabzilla"
    };

    private sealed class PendingSpecialAttackContext
    {
        public string projectileId;
        public AttackAction attackAction;
        public double expiresAt;
    }

    private static readonly Dictionary<long, PendingSpecialAttackContext> PendingSpecialAttackByActorId = new Dictionary<long, PendingSpecialAttackContext>();
    private static readonly object PendingSpecialAttackLock = new object();
    private static readonly List<Actor> CachedLivingActors = new List<Actor>(256);
    private static readonly Dictionary<long, Actor> CachedActorsById = new Dictionary<long, Actor>();
    private static readonly Dictionary<object, long> CachedOldestKaijuBySubspecies = new Dictionary<object, long>();
    private static object _cachedActorWorldToken;
    private static float _nextActorCacheRefreshAt;
    private static readonly HashSet<string> KaijuBaseOverlayActorIds = new HashSet<string>(StringComparer.Ordinal)
    {
        "Gojira",
        "Longlegder"
    };

    private static void RefreshActorCachesIfNeeded(bool force = false)
    {
        object worldToken = World.world;
        if (!ReferenceEquals(_cachedActorWorldToken, worldToken))
        {
            _cachedActorWorldToken = worldToken;
            _nextActorCacheRefreshAt = 0f;
        }

        if (!force && Time.time < _nextActorCacheRefreshAt)
        {
            return;
        }

        _nextActorCacheRefreshAt = Time.time + KaijuActorCacheRefreshSeconds;
        CachedLivingActors.Clear();
        CachedActorsById.Clear();
        CachedOldestKaijuBySubspecies.Clear();

        if (World.world?.units == null)
        {
            return;
        }

        foreach (Actor unit in World.world.units)
        {
            if (unit == null || !unit.isAlive())
            {
                continue;
            }

            CachedLivingActors.Add(unit);

            long unitId = GetActorId(unit);
            if (unitId != long.MinValue)
            {
                CachedActorsById[unitId] = unit;
            }

            if (unit.isEgg() || unit.isBaby() || unit.subspecies == null || !HasKaijuAlphaDecision(unit))
            {
                continue;
            }

            if (!CachedOldestKaijuBySubspecies.TryGetValue(unit.subspecies, out long oldestId))
            {
                CachedOldestKaijuBySubspecies[unit.subspecies] = unitId;
                continue;
            }

            Actor oldestActor = ResolveActorById(oldestId, false);
            if (oldestActor == null)
            {
                CachedOldestKaijuBySubspecies[unit.subspecies] = unitId;
                continue;
            }

            long oldestActorId = GetActorId(oldestActor);
            if (unit.age > oldestActor.age || (Mathf.Abs(unit.age - oldestActor.age) <= 0.0001f && unitId < oldestActorId))
            {
                CachedOldestKaijuBySubspecies[unit.subspecies] = unitId;
            }
        }
    }

    private static long GetActorId(Actor actor)
    {
        return actor == null ? long.MinValue : actor.getID();
    }

    private static Actor ResolveActorById(long actorId, bool refreshCache = true)
    {
        if (actorId == long.MinValue)
        {
            return null;
        }

        if (refreshCache)
        {
            RefreshActorCachesIfNeeded();
        }

        if (!CachedActorsById.TryGetValue(actorId, out Actor actor) || actor == null || !actor.isAlive())
        {
            return null;
        }

        return actor;
    }

    private static bool IsValidMapwideTarget(Actor actor, Actor target)
    {
        return actor != null
            && actor.isAlive()
            && target != null
            && target != actor
            && target.isAlive()
            && !IsKaijuBurrowed(target)
            && actor.areFoes(target);
    }

    private static bool TryGetCachedMapwideTarget(Actor actor, out Actor target)
    {
        target = null;
        if (actor?.data == null)
        {
            return false;
        }

        actor.data.get(KaijuMapwideRetargetAtKey, out float retargetAt, 0f);
        if (Time.time >= retargetAt)
        {
            return false;
        }

        actor.data.get(KaijuMapwideTargetIdKey, out string actorIdText, string.Empty);
        if (!long.TryParse(actorIdText, out long actorId))
        {
            return false;
        }

        target = ResolveActorById(actorId);
        return IsValidMapwideTarget(actor, target);
    }

    private static void CacheMapwideTarget(Actor actor, Actor target)
    {
        if (actor?.data == null || target == null)
        {
            return;
        }

        actor.data.set(KaijuMapwideTargetIdKey, GetActorId(target).ToString());
        actor.data.set(KaijuMapwideRetargetAtKey, Time.time + KaijuMapwideRetargetCooldownSeconds);
    }

    private static bool KaijuBurrowDecisionEffect(Actor actor)
    {
        if (actor == null || actor.asset == null || !actor.isAlive() || actor.isEgg() || !IsKaijuBurrowActor(actor))
        {
            return false;
        }

        if (IsKaijuBurrowed(actor))
        {
            return UpdateKaijuBurrowedState(actor);
        }

        if (IsKaijuBurrowRetreating(actor))
        {
            return UpdateKaijuBurrowRetreat(actor);
        }

        if (ShouldStartLowHealthBurrowRetreat(actor))
        {
            StartKaijuBurrowRetreat(actor);
            return true;
        }

        if (ShouldStartRandomBurrow(actor))
        {
            EnterKaijuBurrow(actor, KaijuBurrowRandomMinDurationSeconds);
            return true;
        }

        return false;
    }

    private static bool IsKaijuBurrowActor(Actor actor)
    {
        return actor != null
            && actor.asset != null
            && !string.IsNullOrEmpty(actor.asset.id)
            && KaijuBurrowActorIds.Contains(actor.asset.id)
            && HasKaijuBurrowDecision(actor);
    }

    private static bool IsKaijuBurrowed(Actor actor)
    {
        if (actor?.data == null || !IsKaijuBurrowActor(actor))
        {
            return false;
        }

        actor.data.get(KaijuBurrowStateKey, out bool isBurrowed, false);
        return isBurrowed;
    }

    private static bool IsKaijuBurrowRetreating(Actor actor)
    {
        if (actor?.data == null || !IsKaijuBurrowActor(actor))
        {
            return false;
        }

        actor.data.get(KaijuBurrowRetreatStateKey, out bool isRetreating, false);
        return isRetreating;
    }

    private static bool IsKaijuBurrowBusy(Actor actor)
    {
        return IsKaijuBurrowed(actor) || IsKaijuBurrowRetreating(actor);
    }

    private static bool ShouldStartLowHealthBurrowRetreat(Actor actor)
    {
        if (actor == null || actor.getHealthRatio() > KaijuBurrowLowHealthThreshold)
        {
            return false;
        }

        if (actor.attackedBy != null && !actor.attackedBy.isRekt() && actor.attackedBy.isActor() && actor.areFoes(actor.attackedBy))
        {
            return true;
        }

        return FindNearestAggressiveFoe(actor, KaijuBurrowThreatRange) != null;
    }

    private static bool ShouldStartRandomBurrow(Actor actor)
    {
        if (actor?.data == null)
        {
            return false;
        }

        actor.data.get(KaijuBurrowRandomLockUntilKey, out float randomLockUntil, 0f);
        if (Time.time < randomLockUntil)
        {
            return false;
        }

        return Randy.randomChance(KaijuBurrowRandomChance);
    }

    private static void StartKaijuBurrowRetreat(Actor actor)
    {
        if (actor?.data == null)
        {
            return;
        }

        actor.data.set(KaijuBurrowRetreatStateKey, pData: true);
        actor.data.set(KaijuBurrowRetreatStartKey, Time.time);
        ForceKaijuBurrowDisengage(actor, pStopMovement: false);
        MoveKaijuAwayFromThreats(actor);
    }

    private static bool UpdateKaijuBurrowRetreat(Actor actor)
    {
        if (actor?.data == null)
        {
            return false;
        }

        if (!IsKaijuBurrowRetreating(actor))
        {
            return false;
        }

        ForceKaijuBurrowDisengage(actor, pStopMovement: false);

        if (!HasAggressiveFoeInRange(actor, KaijuBurrowSafeRange))
        {
            EnterKaijuBurrow(actor, 1.5f);
            return true;
        }

        actor.data.get(KaijuBurrowRetreatStartKey, out float retreatStartedAt, Time.time);
        if (Time.time - retreatStartedAt >= KaijuBurrowRetreatTimeoutSeconds)
        {
            EnterKaijuBurrow(actor, 1.5f);
            return true;
        }

        MoveKaijuAwayFromThreats(actor);
        return true;
    }

    private static void EnterKaijuBurrow(Actor actor, float minDurationSeconds)
    {
        if (actor?.data == null)
        {
            return;
        }

        actor.data.set(KaijuBurrowRetreatStateKey, pData: false);
        actor.data.set(KaijuBurrowStateKey, pData: true);
        actor.data.set(KaijuBurrowNextHealKey, Time.time + KaijuBurrowHealTickSeconds);
        actor.data.set(KaijuBurrowMinUntilKey, Time.time + Mathf.Max(0f, minDurationSeconds));
        actor.data.set(KaijuBurrowRandomLockUntilKey, Time.time + KaijuBurrowRandomLockoutAfterExitSeconds);

        ForceKaijuBurrowDisengage(actor, pStopMovement: true);
        actor.clearGraphicsFully();
        actor.dirty_sprite_main = true;
    }

    private static bool UpdateKaijuBurrowedState(Actor actor)
    {
        if (actor?.data == null)
        {
            return false;
        }

        if (!IsKaijuBurrowed(actor))
        {
            return false;
        }

        ForceKaijuBurrowDisengage(actor, pStopMovement: true);

        actor.data.get(KaijuBurrowNextHealKey, out float nextHealAt, Time.time);
        if (Time.time >= nextHealAt)
        {
            actor.restoreHealth(actor.getMaxHealthPercent(KaijuBurrowHealPerTick));
            actor.data.set(KaijuBurrowNextHealKey, Time.time + KaijuBurrowHealTickSeconds);
        }

        actor.data.get(KaijuBurrowMinUntilKey, out float minUntil, 0f);
        bool minDurationPassed = Time.time >= minUntil;
        if (minDurationPassed && (actor.getHealthRatio() >= 0.999f || actor.needsFood()))
        {
            ExitKaijuBurrow(actor);
        }

        return true;
    }

    private static void ExitKaijuBurrow(Actor actor)
    {
        if (actor?.data == null)
        {
            return;
        }

        actor.data.set(KaijuBurrowStateKey, pData: false);
        actor.data.set(KaijuBurrowRetreatStateKey, pData: false);
        actor.data.set(KaijuBurrowRandomLockUntilKey, Time.time + KaijuBurrowRandomLockoutAfterExitSeconds);
        actor.clearGraphicsFully();
        actor.dirty_sprite_main = true;
    }

    private static void ForceKaijuBurrowDisengage(Actor actor, bool pStopMovement)
    {
        if (actor == null)
        {
            return;
        }

        actor.finishAngryStatus();
        actor.clearAttackTarget();
        if (pStopMovement)
        {
            actor.stopMovement();
        }
    }

    private static bool HasAggressiveFoeInRange(Actor actor, float maxRange)
    {
        return FindNearestAggressiveFoe(actor, maxRange) != null;
    }

    private static Actor FindNearestAggressiveFoe(Actor actor, float maxRange)
    {
        if (actor == null || World.world?.units == null)
        {
            return null;
        }

        if (actor.current_tile != null)
        {
            int chunkRadius = Mathf.Clamp(Mathf.CeilToInt(maxRange / 12f), 1, 6);
            float bestNearbyDistSq = maxRange * maxRange;
            Actor bestNearby = null;

            foreach (Actor unit in Finder.getUnitsFromChunk(actor.current_tile, chunkRadius, maxRange, false))
            {
                if (unit == null || unit == actor || !unit.isAlive() || !actor.areFoes(unit))
                {
                    continue;
                }

                bool isAggressiveNearby = unit.hasStatus("angry")
                    || (unit.has_attack_target && unit.attack_target != null && ReferenceEquals(unit.attack_target.a, actor))
                    || (actor.attackedBy != null && actor.attackedBy.isActor() && ReferenceEquals(actor.attackedBy.a, unit));
                if (!isAggressiveNearby)
                {
                    continue;
                }

                float distSq = Toolbox.SquaredDistVec2Float(actor.current_position, unit.current_position);
                if (distSq >= bestNearbyDistSq)
                {
                    continue;
                }

                bestNearbyDistSq = distSq;
                bestNearby = unit;
            }

            if (bestNearby != null)
            {
                return bestNearby;
            }
        }

        RefreshActorCachesIfNeeded();
        float maxRangeSq = maxRange * maxRange;
        float bestDistSq = maxRangeSq;
        Actor nearest = null;

        for (int i = 0; i < CachedLivingActors.Count; i++)
        {
            Actor unit = CachedLivingActors[i];
            if (unit == null || unit == actor || !unit.isAlive() || !actor.areFoes(unit))
            {
                continue;
            }

            bool isAggressive = unit.hasStatus("angry")
                || (unit.has_attack_target && ReferenceEquals(unit.attack_target, actor))
                || (actor.attackedBy != null && actor.attackedBy.isActor() && ReferenceEquals(actor.attackedBy.a, unit));
            if (!isAggressive)
            {
                continue;
            }

            float distSq = Toolbox.SquaredDistVec2Float(actor.current_position, unit.current_position);
            if (distSq >= bestDistSq)
            {
                continue;
            }

            bestDistSq = distSq;
            nearest = unit;
        }

        return nearest;
    }

    private static bool MoveKaijuAwayFromThreats(Actor actor)
    {
        if (actor == null || World.world?.units == null)
        {
            return false;
        }

        Vector2 fleeDirection = Vector2.zero;
        float threatRangeSq = KaijuBurrowThreatRange * KaijuBurrowThreatRange;

        if (actor.current_tile != null)
        {
            int chunkRadius = Mathf.Clamp(Mathf.CeilToInt(KaijuBurrowThreatRange / 12f), 1, 6);
            foreach (Actor unit in Finder.getUnitsFromChunk(actor.current_tile, chunkRadius, KaijuBurrowThreatRange, false))
            {
                if (unit == null || unit == actor || !unit.isAlive() || !actor.areFoes(unit))
                {
                    continue;
                }

                bool isAggressive = unit.hasStatus("angry")
                    || (unit.has_attack_target && unit.attack_target != null && ReferenceEquals(unit.attack_target.a, actor));
                if (!isAggressive)
                {
                    continue;
                }

                Vector2 away = actor.current_position - unit.current_position;
                float distSq = away.sqrMagnitude;
                if (distSq <= 0.0001f || distSq > threatRangeSq)
                {
                    continue;
                }

                fleeDirection += away.normalized / Mathf.Max(1f, distSq);
            }
        }

        if (fleeDirection.sqrMagnitude < 0.0001f)
        {
            RefreshActorCachesIfNeeded();
            for (int i = 0; i < CachedLivingActors.Count; i++)
            {
                Actor unit = CachedLivingActors[i];
                if (unit == null || unit == actor || !unit.isAlive() || !actor.areFoes(unit))
                {
                    continue;
                }

                bool isAggressive = unit.hasStatus("angry")
                    || (unit.has_attack_target && unit.attack_target != null && ReferenceEquals(unit.attack_target.a, actor));
                if (!isAggressive)
                {
                    continue;
                }

                Vector2 away = actor.current_position - unit.current_position;
                float distSq = away.sqrMagnitude;
                if (distSq <= 0.0001f || distSq > threatRangeSq)
                {
                    continue;
                }

                fleeDirection += away.normalized / Mathf.Max(1f, distSq);
            }
        }

        if (fleeDirection.sqrMagnitude < 0.0001f)
        {
            float angle = Randy.randomFloat(0f, Mathf.PI * 2f);
            fleeDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        fleeDirection.Normalize();

        if (!TryFindFleeTile(actor, fleeDirection, out WorldTile fleeTile))
        {
            return false;
        }

        ExecuteEvent moveEvent = actor.goTo(fleeTile);
        if (moveEvent == ExecuteEvent.False)
        {
            actor.goTo(fleeTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 0);
        }

        return true;
    }

    private static bool TryFindFleeTile(Actor actor, Vector2 baseDirection, out WorldTile fleeTile)
    {
        fleeTile = null;
        if (actor == null || World.world == null || baseDirection.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        float[] angleOffsets = { 0f, 25f, -25f, 50f, -50f, 75f, -75f, 110f, -110f };
        for (int i = 0; i < angleOffsets.Length; i++)
        {
            float radians = angleOffsets[i] * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            Vector2 dir = new Vector2(baseDirection.x * cos - baseDirection.y * sin, baseDirection.x * sin + baseDirection.y * cos);

            for (float distance = KaijuBurrowRetreatDistance; distance >= 6f; distance -= 2f)
            {
                Vector2 targetPos = actor.current_position + dir * distance;
                WorldTile tile = World.world.GetTile(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.y));
                if (tile == null)
                {
                    continue;
                }

                fleeTile = tile;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetKaijuBurrowSprite(Actor actor, out Sprite sprite)
    {
        sprite = null;
        if (!IsKaijuBurrowed(actor) || actor?.asset?.texture_asset == null)
        {
            return false;
        }

        ActorTextureSubAsset textureAsset = actor.asset.texture_asset;
        string textureRoot = textureAsset.texture_path_base;
        if (string.IsNullOrEmpty(textureRoot))
        {
            textureRoot = textureAsset.texture_path_main;
        }

        if (string.IsNullOrEmpty(textureRoot))
        {
            return false;
        }

        textureRoot = textureRoot.TrimEnd('/');

        if (actor.isBaby())
        {
            return TryLoadSprite(textureRoot + "/child/burrow", out sprite);
        }

        if (IsKaijuAlpha(actor))
        {
            if (TryLoadSprite(textureRoot + "/Alpha/burrow", out sprite))
            {
                return true;
            }

            if (TryLoadSprite(textureRoot + "/alpha/burrow", out sprite))
            {
                return true;
            }
        }

        return TryLoadSprite(textureRoot + "/main/burrow", out sprite);
    }

    private static bool TryLoadSprite(string path, out Sprite sprite)
    {
        sprite = null;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        sprite = SpriteTextureLoader.getSprite(path);
        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>(path);
        }

        return sprite != null;
    }

    private static bool ExecuteKaijuSpecialAttack(Actor actor, bool pFromPossessionKick)
    {
        if (actor == null || actor.asset == null || actor.asset.skip_fight_logic || !actor.isAttackReady())
        {
            return false;
        }

        if (IsKaijuBurrowBusy(actor))
        {
            return false;
        }

        if (!HasKaijuSpecialAttackDecision(actor) || !IsEligibleForKaijuSpecialAttack(actor))
        {
            return false;
        }

        if (!TryGetKaijuSpecialAttackAsset(actor, out EquipmentAsset specialAttackAsset))
        {
            return false;
        }

        if (pFromPossessionKick)
        {
            return TryExecutePossessionSpecialAttack(actor, specialAttackAsset);
        }

        return TryExecuteDecisionSpecialAttack(actor, specialAttackAsset);
    }

    private static bool TryExecuteDecisionSpecialAttack(Actor actor, EquipmentAsset specialAttackAsset)
    {
        if (actor == null || specialAttackAsset == null)
        {
            return false;
        }

        Actor target = GetSpecialDecisionEnemyActorTarget(actor, KaijuSpecialTargetRange);

        if (target == null)
        {
            return false;
        }

        return LaunchSpecialProjectile(actor, specialAttackAsset, target.current_position, target.current_tile, actor.kingdom, target, pMarkPossessionAttackHappened: false);
    }

    private static bool IsEligibleForKaijuSpecialAttack(Actor actor)
    {
        if (actor == null || !actor.isAlive() || actor.isEgg() || actor.isBaby())
        {
            return false;
        }

        if (!actor.hasSubspecies())
        {
            return true;
        }

        if (!HasKaijuAlphaDecision(actor))
        {
            return false;
        }

        UpdateKaijuAlphaState(actor);
        return IsKaijuAlpha(actor);
    }

    private static Actor GetSpecialDecisionEnemyActorTarget(Actor actor, float maxRange)
    {
        if (actor == null || !actor.isAlive())
        {
            return null;
        }

        Actor target = GetCurrentEnemyActorTarget(actor, maxRange);
        if (target != null)
        {
            return target;
        }

        return FindNearestEnemy(actor, maxRange);
    }

    private static bool TryGetKaijuSpecialAttackAsset(Actor actor, out EquipmentAsset specialAttackAsset)
    {
        specialAttackAsset = null;

        if (actor?.asset == null || string.IsNullOrEmpty(actor.asset.id))
        {
            return false;
        }

        if (!KaijuSpecialAttackAssetByActorId.TryGetValue(actor.asset.id, out string attackAssetId))
        {
            return false;
        }

        specialAttackAsset = AssetManager.items.get(attackAssetId);
        if (specialAttackAsset == null || string.IsNullOrEmpty(specialAttackAsset.projectile))
        {
            return false;
        }

        if (!AssetManager.projectiles.has(specialAttackAsset.projectile))
        {
            return false;
        }

        return true;
    }

    private static bool TryExecutePossessionSpecialAttack(Actor actor, EquipmentAsset specialAttackAsset)
    {
        if (actor == null || specialAttackAsset == null)
        {
            return false;
        }

        Vector2 attackPos = GetPossessionAttackPos(actor);
        Actor actorTargetAttack = GetPossessionActorTargetAttack(actor, KaijuSpecialTargetRange);
        WorldTile clickedTile = World.world?.GetTile(Mathf.RoundToInt(attackPos.x), Mathf.RoundToInt(attackPos.y));
        WorldTile hitTile = actorTargetAttack?.current_tile ?? clickedTile ?? actor.current_tile;
        Kingdom forceKingdom = World.world?.kingdoms_wild?.get("possessed") ?? actor.kingdom;
        if (actorTargetAttack != null)
        {
            attackPos = actorTargetAttack.current_position;
        }

        return LaunchSpecialProjectile(actor, specialAttackAsset, attackPos, hitTile, forceKingdom, actorTargetAttack, pMarkPossessionAttackHappened: true);
    }

    private static bool LaunchSpecialProjectile(Actor actor, EquipmentAsset specialAttackAsset, Vector2 attackPos, WorldTile hitTile, Kingdom forceKingdom, BaseSimObject target, bool pMarkPossessionAttackHappened)
    {
        if (actor == null || specialAttackAsset == null || string.IsNullOrEmpty(specialAttackAsset.projectile) || !AssetManager.projectiles.has(specialAttackAsset.projectile))
        {
            return false;
        }

        if (target != null && target.isActor() && IsKaijuBurrowed(target.a))
        {
            return false;
        }

        FieldInfo attackAssetField = AccessTools.Field(typeof(Actor), "_attack_asset");
        FieldInfo attackTargetActionsField = AccessTools.Field(typeof(Actor), "s_action_attack_target");
        EquipmentAsset originalAttackAsset = attackAssetField?.GetValue(actor) as EquipmentAsset;
        AttackAction originalAttackTargetActions = (AttackAction)attackTargetActionsField?.GetValue(actor);
        float originalProjectiles = actor.stats["projectiles"];

        bool launched = false;
        WorldTile resolvedHitTile = hitTile ?? actor.current_tile;
        try
        {
            ApplySpecialAttackItemContext(actor, specialAttackAsset, pReplaceAttackTargetActions: true);
            attackAssetField?.SetValue(actor, specialAttackAsset);
            RegisterPendingSpecialAttackContext(actor, specialAttackAsset);

            float forcedProjectiles = Mathf.Max(1f, specialAttackAsset.base_stats["projectiles"]);
            if (specialAttackAsset.item_modifiers != null)
            {
                for (int i = 0; i < specialAttackAsset.item_modifiers.Length; i++)
                {
                    ItemModAsset itemModAsset = specialAttackAsset.item_modifiers[i];
                    if (itemModAsset == null)
                    {
                        continue;
                    }

                    forcedProjectiles = Mathf.Max(forcedProjectiles, itemModAsset.base_stats["projectiles"]);
                }
            }

            actor.stats["projectiles"] = forcedProjectiles;

            AccessTools.Method(typeof(Actor), "startAttackCooldown")?.Invoke(actor, null);

            Vector3 hitPosition = new Vector3(attackPos.x, attackPos.y, 0f);
            Kingdom resolvedKingdom = forceKingdom ?? actor.kingdom;
            float bonusAreaOfEffect = Mathf.Max(0f, actor.getAttackRange()) * 0.2f;

            AttackData attackData = new AttackData(
                actor,
                resolvedHitTile,
                hitPosition,
                actor.current_position,
                target,
                resolvedKingdom,
                AttackType.Weapon,
                pMetallicWeapon: false,
                pSkipShake: true,
                pProjectile: true,
                pProjectileID: specialAttackAsset.projectile,
                pKillAction: null,
                pBonusAreOfEffect: bonusAreaOfEffect);

            launched = CombatActionLibrary.combat_attack_range.action(attackData);
        }
        finally
        {
            actor.stats["projectiles"] = originalProjectiles;
            attackAssetField?.SetValue(actor, originalAttackAsset);
            attackTargetActionsField?.SetValue(actor, originalAttackTargetActions);
        }

        if (!launched)
        {
            return false;
        }

        if (target != null)
        {
            actor.setAttackTarget(target);
        }

        ApplySpecialAttackPursuit(actor, target, attackPos, resolvedHitTile);

        if (pMarkPossessionAttackHappened)
        {
            actor.setPossessionAttackHappened();
        }

        if (HasBossAttackAnimationDecision(actor))
        {
            StartBossAttackAnimation(actor, "attack_special", "attack");
        }

        return true;
    }

    private static void ApplySpecialAttackPursuit(Actor actor, BaseSimObject target, Vector2 attackPos, WorldTile hitTile)
    {
        if (actor == null || !actor.isAlive() || actor.data == null || IsKaijuBurrowBusy(actor))
        {
            return;
        }

        if (target != null && target.isActor() && target.a != null && target.a.isAlive() && !IsKaijuBurrowed(target.a))
        {
            actor.addAggro(target.a);
            actor.setAttackTarget(target.a);
            actor.startFightingWith(target.a);
            CommitForcedAdvance(actor, target.current_tile, KaijuForcedAdvanceDurationSeconds);
        }

        WorldTile moveTile = hitTile;
        if (target != null && target.current_tile != null)
        {
            moveTile = target.current_tile;
        }

        if (moveTile == null)
        {
            moveTile = World.world?.GetTile(Mathf.RoundToInt(attackPos.x), Mathf.RoundToInt(attackPos.y));
        }

        if (moveTile == null || actor.current_tile == null)
        {
            return;
        }

        CommitForcedAdvance(actor, moveTile, KaijuForcedAdvanceDurationSeconds);

        float desiredEngageDistance = Mathf.Clamp(actor.getAttackRange() * 0.35f, KaijuPursuitDistanceMin, KaijuPursuitDistanceMax);
        float distToMoveTile = Vector2.Distance(actor.current_position, moveTile.pos);
        if (distToMoveTile <= desiredEngageDistance)
        {
            return;
        }

        actor.data.get(KaijuSpecialPursuitNextMoveKey, out float nextMoveAt, 0f);
        if (Time.time < nextMoveAt)
        {
            return;
        }

        actor.data.set(KaijuSpecialPursuitNextMoveKey, Time.time + KaijuSpecialPursuitMoveIntervalSeconds);

        ExecuteEvent moveEvent = actor.goTo(moveTile);
        if (moveEvent == ExecuteEvent.False)
        {
            actor.goTo(moveTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 0);
        }
    }

    private static void CommitForcedAdvance(Actor actor, WorldTile moveTile, float durationSeconds)
    {
        if (actor?.data == null || moveTile == null)
        {
            return;
        }

        actor.data.set(KaijuForcedAdvanceUntilKey, Time.time + Mathf.Max(1f, durationSeconds));
        actor.data.set(KaijuForcedAdvanceTileXKey, moveTile.x);
        actor.data.set(KaijuForcedAdvanceTileYKey, moveTile.y);
    }

    private static void ClearForcedAdvance(Actor actor)
    {
        if (actor?.data == null)
        {
            return;
        }

        actor.data.set(KaijuForcedAdvanceUntilKey, 0f);
    }

    private static void UpdateForcedAdvance(Actor actor)
    {
        if (actor == null || !actor.isAlive() || actor.data == null || actor.current_tile == null || IsKaijuBurrowBusy(actor))
        {
            return;
        }

        actor.data.get(KaijuForcedAdvanceUntilKey, out float forcedUntil, 0f);
        if (forcedUntil <= 0f || Time.time > forcedUntil)
        {
            return;
        }

        WorldTile moveTile = null;
        if (actor.has_attack_target && actor.attack_target != null && actor.isEnemyTargetAlive() && actor.attack_target.current_tile != null)
        {
            moveTile = actor.attack_target.current_tile;
            CommitForcedAdvance(actor, moveTile, forcedUntil - Time.time);
        }

        if (moveTile == null)
        {
            actor.data.get(KaijuForcedAdvanceTileXKey, out int moveX, int.MinValue);
            actor.data.get(KaijuForcedAdvanceTileYKey, out int moveY, int.MinValue);
            if (moveX != int.MinValue && moveY != int.MinValue)
            {
                moveTile = World.world?.GetTile(moveX, moveY);
            }
        }

        if (moveTile == null)
        {
            ClearForcedAdvance(actor);
            return;
        }

        float desiredDistance = Mathf.Clamp(actor.getAttackRange() * 0.2f, KaijuForcedAdvanceDistanceMin, KaijuForcedAdvanceDistanceMax);
        float distToMoveTile = Vector2.Distance(actor.current_position, moveTile.pos);
        if (distToMoveTile <= desiredDistance)
        {
            ClearForcedAdvance(actor);
            return;
        }

        actor.data.get(KaijuForcedAdvanceNextMoveKey, out float nextMoveAt, 0f);
        if (Time.time < nextMoveAt)
        {
            return;
        }

        actor.data.set(KaijuForcedAdvanceNextMoveKey, Time.time + KaijuForcedAdvanceMoveIntervalSeconds);

        ExecuteEvent moveEvent = actor.goTo(moveTile);
        if (moveEvent == ExecuteEvent.False)
        {
            actor.goTo(moveTile, pPathOnWater: true, pWalkOnBlocks: true, pWalkOnLava: false, pLimitPathfindingRegions: 0);
        }
    }

    private static Vector2 GetPossessionAttackPos(Actor actor)
    {
        return ControllableUnit.getClickVector();
    }

    private static Actor GetPossessionActorTargetAttack(Actor actor, float pRange)
    {
        float range = Mathf.Max(1f, pRange);
        Vector2 attackPos = GetPossessionAttackPos(actor);
        WorldTile worldTile = World.world.GetTile((int)attackPos.x, (int)attackPos.y);
        if (worldTile == null)
        {
            worldTile = actor.current_tile;
        }

        float bestDist = float.MaxValue;
        Actor result = null;
        float squaredRange = range * range;

        foreach (Actor unit in Finder.getUnitsFromChunk(worldTile, 0, range, pRandom: true))
        {
            if (unit == null || unit == actor)
            {
                continue;
            }

            if (IsKaijuBurrowed(unit))
            {
                continue;
            }

            if (!actor.areFoes(unit))
            {
                continue;
            }

            float sqDist = Toolbox.SquaredDistVec2Float(attackPos, unit.current_position);
            if (sqDist > squaredRange || sqDist >= bestDist)
            {
                continue;
            }

            result = unit;
            bestDist = sqDist;
        }

        return result;
    }

    private static AttackAction BuildSpecialAttackActions(EquipmentAsset specialAttackAsset)
    {
        if (specialAttackAsset == null)
        {
            return null;
        }

        AttackAction combinedAttackAction = null;
        if (specialAttackAsset.action_attack_target != null)
        {
            combinedAttackAction = (AttackAction)Delegate.Combine(combinedAttackAction, specialAttackAsset.action_attack_target);
        }

        if (specialAttackAsset.item_modifiers != null)
        {
            for (int i = 0; i < specialAttackAsset.item_modifiers.Length; i++)
            {
                ItemModAsset itemModAsset = specialAttackAsset.item_modifiers[i];
                if (itemModAsset == null)
                {
                    continue;
                }

                if (itemModAsset.action_attack_target != null)
                {
                    combinedAttackAction = (AttackAction)Delegate.Combine(combinedAttackAction, itemModAsset.action_attack_target);
                }
            }
        }

        return combinedAttackAction;
    }

    private static void RegisterPendingSpecialAttackContext(Actor actor, EquipmentAsset specialAttackAsset)
    {
        if (actor == null || specialAttackAsset == null || string.IsNullOrEmpty(specialAttackAsset.projectile))
        {
            return;
        }

        AttackAction specialAction = BuildSpecialAttackActions(specialAttackAsset);
        if (specialAction == null)
        {
            return;
        }

        double now = World.world?.getCurWorldTime() ?? 0.0;
        PendingSpecialAttackContext context = new PendingSpecialAttackContext
        {
            projectileId = specialAttackAsset.projectile,
            attackAction = specialAction,
            expiresAt = now + 8.0
        };

        lock (PendingSpecialAttackLock)
        {
            PendingSpecialAttackByActorId[actor.getID()] = context;
        }
    }

    private static bool TryGetPendingSpecialAttackAction(Actor actor, string projectileId, out AttackAction specialAction)
    {
        specialAction = null;
        if (actor == null || string.IsNullOrEmpty(projectileId))
        {
            return false;
        }

        lock (PendingSpecialAttackLock)
        {
            if (!PendingSpecialAttackByActorId.TryGetValue(actor.getID(), out PendingSpecialAttackContext context) || context == null)
            {
                return false;
            }

            double now = World.world?.getCurWorldTime() ?? 0.0;
            if (now > context.expiresAt)
            {
                PendingSpecialAttackByActorId.Remove(actor.getID());
                return false;
            }

            if (!string.Equals(context.projectileId, projectileId, StringComparison.Ordinal))
            {
                return false;
            }

            specialAction = context.attackAction;
            return specialAction != null;
        }
    }

    private static void ApplySpecialAttackItemContext(Actor actor, EquipmentAsset specialAttackAsset, bool pReplaceAttackTargetActions = false)
    {
        if (actor == null || specialAttackAsset == null)
        {
            return;
        }

        AttackAction combinedAttackAction = null;
        if (!pReplaceAttackTargetActions)
        {
            combinedAttackAction = (AttackAction)AccessTools.Field(typeof(Actor), "s_action_attack_target")?.GetValue(actor);
        }
        if (specialAttackAsset.action_attack_target != null)
        {
            combinedAttackAction = (AttackAction)Delegate.Combine(combinedAttackAction, specialAttackAsset.action_attack_target);
        }

        if (specialAttackAsset.item_modifiers != null)
        {
            for (int i = 0; i < specialAttackAsset.item_modifiers.Length; i++)
            {
                ItemModAsset itemModAsset = specialAttackAsset.item_modifiers[i];
                if (itemModAsset == null)
                {
                    continue;
                }

                if (itemModAsset.action_attack_target != null)
                {
                    combinedAttackAction = (AttackAction)Delegate.Combine(combinedAttackAction, itemModAsset.action_attack_target);
                }
            }
        }

        AccessTools.Field(typeof(Actor), "s_action_attack_target")?.SetValue(actor, combinedAttackAction);
    }

    private static bool BossAttackAnimationDecisionEffect(Actor actor)
    {
        return false;
    }

    private static bool HasBossAttackAnimationDecision(Actor actor)
    {
        KaijuDecisionFlags flags = GetKaijuDecisionFlags(actor);
        return flags != null && flags.HasBossAttackAnimation;
    }

    private static bool HasKaijuAlphaDecision(Actor actor)
    {
        KaijuDecisionFlags flags = GetKaijuDecisionFlags(actor);
        return flags != null && flags.HasAlpha;
    }

    private static bool HasKaijuSpecialAttackDecision(Actor actor)
    {
        KaijuDecisionFlags flags = GetKaijuDecisionFlags(actor);
        return flags != null && flags.HasSpecial;
    }

    private static bool HasKaijuMapwideAggroOverrideDecision(Actor actor)
    {
        KaijuDecisionFlags flags = GetKaijuDecisionFlags(actor);
        return flags != null && flags.HasMapwideAggro;
    }

    private static bool HasKaijuBurrowDecision(Actor actor)
    {
        KaijuDecisionFlags flags = GetKaijuDecisionFlags(actor);
        return flags != null && flags.HasBurrow;
    }

    private static bool HasDecision(Actor actor, string decisionId)
    {
        return actor != null
            && actor.asset != null
            && actor.asset.decision_ids != null
            && actor.asset.decision_ids.Contains(decisionId);
    }

    private static KaijuDecisionFlags GetKaijuDecisionFlags(Actor actor)
    {
        if (actor == null || actor.asset == null)
        {
            return null;
        }

        string actorId = actor.asset.id;
        if (string.IsNullOrEmpty(actorId))
        {
            return null;
        }

        if (_kaijuDecisionFlagsCache.TryGetValue(actorId, out KaijuDecisionFlags cached))
        {
            return cached;
        }

        List<string> decisionIds = actor.asset.decision_ids;
        KaijuDecisionFlags flags = new KaijuDecisionFlags
        {
            HasBossAttackAnimation = decisionIds != null && decisionIds.Contains("boss_attack_animation_decision"),
            HasAlpha = decisionIds != null && decisionIds.Contains("kaiju_alpha_state_decision"),
            HasSpecial = decisionIds != null && decisionIds.Contains("kaiju_special_attack_decision"),
            HasMapwideAggro = decisionIds != null && decisionIds.Contains("kaiju_mapwide_aggro_override_decision"),
            HasBurrow = decisionIds != null && decisionIds.Contains(KaijuBurrowDecisionId)
        };
        flags.IsManagedKaiju = flags.HasAlpha || flags.HasSpecial || flags.HasMapwideAggro || flags.HasBurrow;
        _kaijuDecisionFlagsCache[actorId] = flags;
        return flags;
    }

    private static Sprite[] GetBossAttackFrames(string attackPath)
    {
        if (string.IsNullOrEmpty(attackPath))
        {
            return Array.Empty<Sprite>();
        }

        if (_bossAttackFramesCache.TryGetValue(attackPath, out Sprite[] cachedFrames))
        {
            return cachedFrames;
        }

        Sprite[] loaded = SpriteTextureLoader.getSpriteList(attackPath);
        if (loaded == null || loaded.Length == 0)
        {
            loaded = Resources.LoadAll<Sprite>(attackPath);
        }

        if (loaded == null || loaded.Length == 0)
        {
            Sprite[] empty = Array.Empty<Sprite>();
            _bossAttackFramesCache[attackPath] = empty;
            return empty;
        }

        List<Sprite> valid = new List<Sprite>(loaded.Length);
        for (int i = 0; i < loaded.Length; i++)
        {
            Sprite sprite = loaded[i];
            if (sprite != null)
            {
                valid.Add(sprite);
            }
        }

        if (valid.Count == 0)
        {
            Sprite[] empty = Array.Empty<Sprite>();
            _bossAttackFramesCache[attackPath] = empty;
            return empty;
        }

        Sprite[] result = valid.ToArray();
        Array.Sort(result, CompareBossAttackFrames);
        _bossAttackFramesCache[attackPath] = result;
        return result;
    }

    private static bool StartBossAttackAnimation(Actor actor, string animationFolder, string fallbackFolder = null)
    {
        if (!HasBossAttackAnimationDecision(actor) || actor == null || actor.data == null || !actor.isAlive() || actor.isEgg() || actor.isBaby())
        {
            return false;
        }

        ActorTextureSubAsset textureAsset = actor.asset.texture_asset;
        if (textureAsset == null)
        {
            return false;
        }

        string textureRoot = textureAsset.texture_path_base;
        if (string.IsNullOrEmpty(textureRoot))
        {
            textureRoot = textureAsset.texture_path_main;
        }

        if (string.IsNullOrEmpty(textureRoot))
        {
            return false;
        }

        string selectedPath = null;
        if (!string.IsNullOrEmpty(animationFolder))
        {
            string primaryPath = textureRoot.TrimEnd('/') + "/" + animationFolder;
            Sprite[] primaryFrames = GetBossAttackFrames(primaryPath);
            if (primaryFrames != null && primaryFrames.Length > 0)
            {
                selectedPath = primaryPath;
            }
        }

        if (selectedPath == null && !string.IsNullOrEmpty(fallbackFolder))
        {
            string fallbackPath = textureRoot.TrimEnd('/') + "/" + fallbackFolder;
            Sprite[] fallbackFrames = GetBossAttackFrames(fallbackPath);
            if (fallbackFrames != null && fallbackFrames.Length > 0)
            {
                selectedPath = fallbackPath;
            }
        }

        if (selectedPath == null)
        {
            actor.data.set("mb_boss_attack_anim_path", string.Empty);
            return false;
        }

        actor.data.set("mb_boss_attack_anim_path", selectedPath);
        actor.data.set("mb_boss_attack_anim_start", Time.time);
        actor.data.set("mb_boss_attack_anim_frame_time", 0.06f);

        actor.dirty_sprite_main = true;
        return true;
    }

    private static int CompareBossAttackFrames(Sprite a, Sprite b)
    {
        if (ReferenceEquals(a, b))
        {
            return 0;
        }

        if (a == null)
        {
            return 1;
        }

        if (b == null)
        {
            return -1;
        }

        bool aIsNum = int.TryParse(a.name, out int aNum);
        bool bIsNum = int.TryParse(b.name, out int bNum);

        if (aIsNum && bIsNum)
        {
            return aNum.CompareTo(bNum);
        }

        if (aIsNum)
        {
            return -1;
        }

        if (bIsNum)
        {
            return 1;
        }

        return string.CompareOrdinal(a.name, b.name);
    }

    private static bool UpdateKaijuAlphaState(Actor actor)
    {
        if (!HasKaijuAlphaDecision(actor))
        {
            return false;
        }

        bool shouldBeAlpha = false;
        if (actor.isAlive() && !actor.isEgg() && !actor.isBaby() && actor.subspecies != null)
        {
            shouldBeAlpha = IsOldestManagedKaijuInSubspecies(actor);
        }

        actor.data.get("mb_kaiju_alpha", out bool isAlpha, false);
        if (isAlpha == shouldBeAlpha)
        {
            return false;
        }

        actor.data.set("mb_kaiju_alpha", pData: shouldBeAlpha);
        if (shouldBeAlpha)
        {
            AccessTools.Field(typeof(BaseSimObject), "event_full_stats")?.SetValue(actor, true);
            actor.data.set(KaijuAlphaPromotionRefillPendingKey, pData: true);
        }
        else
        {
            actor.data.set(KaijuAlphaPromotionRefillPendingKey, pData: false);
        }
        actor.setStatsDirty();
        actor.clearGraphicsFully();
        actor.dirty_sprite_main = true;
        actor.checkAnimationContainer();
        return true;
    }

    private static bool IsOldestManagedKaijuInSubspecies(Actor actor)
    {
        if (actor == null || actor.subspecies == null || World.world?.units == null)
        {
            return false;
        }

        RefreshActorCachesIfNeeded();
        if (!CachedOldestKaijuBySubspecies.TryGetValue(actor.subspecies, out long oldestId))
        {
            return false;
        }

        return oldestId == GetActorId(actor);
    }

    private static Actor GetCurrentEnemyActorTarget(Actor actor, float maxRange)
    {
        if (actor == null || !actor.isAlive() || !actor.has_attack_target || !actor.isEnemyTargetAlive() || actor.attack_target == null)
        {
            return null;
        }

        Actor target = actor.attack_target.a;
        if (target == null || !target.isAlive())
        {
            return null;
        }

        if (IsKaijuBurrowed(target))
        {
            return null;
        }

        if (!actor.areFoes(target))
        {
            return null;
        }

        float dist = Vector2.Distance(actor.current_position, target.current_position);
        if (dist > maxRange)
        {
            return null;
        }

        return target;
    }

    private static Actor FindNearestEnemy(Actor actor, float maxRange)
    {
        if (actor == null || !actor.isAlive() || World.world?.units == null)
        {
            return null;
        }

        if (actor.current_tile != null)
        {
            int chunkRadius = Mathf.Clamp(Mathf.CeilToInt(maxRange / 12f), 1, 6);
            Actor bestNearby = null;
            float bestNearbyDist = maxRange + 0.01f;

            foreach (Actor unit in Finder.getUnitsFromChunk(actor.current_tile, chunkRadius, maxRange, false))
            {
                if (unit == null || unit == actor || !unit.isAlive())
                {
                    continue;
                }

                if (IsKaijuBurrowed(unit))
                {
                    continue;
                }

                if (!actor.areFoes(unit))
                {
                    continue;
                }

                float dist = Vector2.Distance(actor.current_position, unit.current_position);
                if (dist >= bestNearbyDist)
                {
                    continue;
                }

                bestNearby = unit;
                bestNearbyDist = dist;
            }

            if (bestNearby != null)
            {
                return bestNearby;
            }
        }

        RefreshActorCachesIfNeeded();
        Actor best = null;
        float bestDist = maxRange + 0.01f;

        for (int i = 0; i < CachedLivingActors.Count; i++)
        {
            Actor unit = CachedLivingActors[i];
            if (unit == null || unit == actor || !unit.isAlive())
            {
                continue;
            }

            if (IsKaijuBurrowed(unit))
            {
                continue;
            }

            if (!actor.areFoes(unit))
            {
                continue;
            }

            float dist = Vector2.Distance(actor.current_position, unit.current_position);
            if (dist >= bestDist)
            {
                continue;
            }

            best = unit;
            bestDist = dist;
        }

        return best;
    }

    private static bool IsKaijuDecisionActor(Actor actor)
    {
        KaijuDecisionFlags flags = GetKaijuDecisionFlags(actor);
        return flags != null && flags.IsManagedKaiju;
    }

    private static bool IsKaijuAlpha(Actor actor)
    {
        if (!IsKaijuDecisionActor(actor) || actor.data == null)
        {
            return false;
        }

        actor.data.get("mb_kaiju_alpha", out bool isAlpha, false);
        return isAlpha;
    }

    private static bool ShouldOverlayKaijuBaseStats(Actor actor)
    {
        if (actor == null || actor.asset == null || !actor.isAlive())
        {
            return false;
        }

        if (!actor.hasSubspecies())
        {
            return false;
        }

        return KaijuBaseOverlayActorIds.Contains(actor.asset.id);
    }

    private static bool NeedsKaijuStatsPatch(Actor actor)
    {
        if (actor == null || actor.asset == null)
        {
            return false;
        }

        return IsKaijuDecisionActor(actor) || KaijuBaseOverlayActorIds.Contains(actor.asset.id);
    }

    private static void ApplyKaijuAlphaScaleBonus(Actor actor, bool isAlpha)
    {
        if (actor?.data == null)
        {
            return;
        }

        float currentScale = actor.stats["scale"];
        actor.data.get(KaijuAlphaScalePreBonusKey, out float previousPreBonusScale, currentScale);
        actor.data.get(KaijuAlphaScaleBonusAppliedKey, out float previousAppliedBonus, 0f);

        float preBonusScale = currentScale;
        if (Mathf.Abs(currentScale - (previousPreBonusScale + previousAppliedBonus)) <= 0.0001f)
        {
            preBonusScale = previousPreBonusScale;
        }

        float newBonus = isAlpha ? KaijuAlphaScaleBonus : 0f;
        float finalScale = Mathf.Max(0.01f, preBonusScale + newBonus);

        actor.data.set(KaijuAlphaScalePreBonusKey, preBonusScale);
        actor.data.set(KaijuAlphaScaleBonusAppliedKey, newBonus);

        actor.stats["scale"] = finalScale;
        AccessTools.Field(typeof(Actor), "target_scale")?.SetValue(actor, finalScale);
    }

    [HarmonyPatch(typeof(Actor), "updateStats")]
    private static class KaijuAlphaStatsPatch
    {
        private struct PatchState
        {
            public bool hasSnapshot;
            public int preHealth;
            public int preMaxHealth;
        }

        [HarmonyPrefix]
        private static void Prefix(Actor __instance, ref PatchState __state)
        {
            __state = default;

            if (!NeedsKaijuStatsPatch(__instance))
            {
                return;
            }

            if (!__instance.isAlive() || !IsKaijuAlpha(__instance))
            {
                return;
            }

            int preMaxHealth = __instance.getMaxHealth();
            if (preMaxHealth <= 0)
            {
                return;
            }

            __state.hasSnapshot = true;
            __state.preMaxHealth = preMaxHealth;
            __state.preHealth = Mathf.Clamp(__instance.getHealth(), 0, preMaxHealth);
        }

        [HarmonyPostfix]
        private static void Postfix(Actor __instance, ref PatchState __state)
        {
            if (!NeedsKaijuStatsPatch(__instance))
            {
                return;
            }

            if (ShouldOverlayKaijuBaseStats(__instance))
            {
                __instance.stats.mergeStats(__instance.asset.base_stats);
            }

            if (!IsKaijuAlpha(__instance))
            {
                ApplyKaijuAlphaScaleBonus(__instance, isAlpha: false);
                return;
            }

            __instance.stats["armor"] += 20f;
            __instance.stats["health"] += 30000f;
            __instance.stats["multiplier_damage"] += 0.8f;
            __instance.stats["multiplier_crit"] += 0.5f;
            ApplyKaijuAlphaScaleBonus(__instance, isAlpha: true);

            bool restoredPromotionRefill = false;
            if (__instance.data != null)
            {
                __instance.data.get(KaijuAlphaPromotionRefillPendingKey, out bool promotionRefillPending, false);
                if (promotionRefillPending)
                {
                    __instance.setMaxHealth();
                    __instance.data.set(KaijuAlphaPromotionRefillPendingKey, pData: false);
                    restoredPromotionRefill = true;
                }
            }

            if (!restoredPromotionRefill && __state.hasSnapshot && __instance.isAlive())
            {
                int alphaMaxHealth = __instance.getMaxHealth();
                if (alphaMaxHealth > 0)
                {
                    float preservedRatio = Mathf.Clamp01((float)__state.preHealth / Mathf.Max(1, __state.preMaxHealth));
                    int desiredHealth = Mathf.Clamp(Mathf.RoundToInt(alphaMaxHealth * preservedRatio), 1, alphaMaxHealth);
                    if (__instance.getHealth() < desiredHealth)
                    {
                        __instance.setHealth(desiredHealth);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ActorTextureSubAsset), nameof(ActorTextureSubAsset.getUnitTexturePath))]
    private static class KaijuAlphaTexturePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor, ref string __result)
        {
            if (!IsKaijuAlpha(pActor))
            {
                return true;
            }

            if (pActor.isEgg() || pActor.isBaby())
            {
                return true;
            }

            ActorTextureSubAsset textureAsset = pActor.asset.texture_asset;
            if (textureAsset == null)
            {
                return true;
            }

            string textureRoot = textureAsset.texture_path_base;
            if (string.IsNullOrEmpty(textureRoot))
            {
                textureRoot = textureAsset.texture_path_main;
            }

            if (string.IsNullOrEmpty(textureRoot))
            {
                return true;
            }

            __result = textureRoot.TrimEnd('/') + "/Alpha";
            return false;
        }
    }

    [HarmonyPatch(typeof(Actor), "startAttackCooldown")]
    private static class BossAttackAnimationStartPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            StartBossAttackAnimation(__instance, "attack");
        }
    }

    [HarmonyPatch(typeof(MapBox), "applyAttack")]
    private static class KaijuSpecialAttackHitPipelinePatch
    {
        private struct PatchState
        {
            public Actor actor;
            public AttackAction originalAttackActions;
            public bool patched;
        }

        [HarmonyPrefix]
        private static void Prefix(AttackData pData, ref PatchState __state)
        {
            __state = default;

            Actor initiator = pData.initiator?.a;
            if (initiator == null)
            {
                return;
            }

            if (!TryGetPendingSpecialAttackAction(initiator, pData.projectile_id, out AttackAction pendingSpecialAction))
            {
                return;
            }

            FieldInfo attackTargetActionsField = AccessTools.Field(typeof(Actor), "s_action_attack_target");
            if (attackTargetActionsField == null)
            {
                return;
            }

            AttackAction originalActions = (AttackAction)attackTargetActionsField.GetValue(initiator);
            AttackAction combinedActions = (AttackAction)Delegate.Combine(originalActions, pendingSpecialAction);
            attackTargetActionsField.SetValue(initiator, combinedActions);

            __state.actor = initiator;
            __state.originalAttackActions = originalActions;
            __state.patched = true;
        }

        [HarmonyPostfix]
        private static void Postfix(ref PatchState __state)
        {
            if (!__state.patched || __state.actor == null)
            {
                return;
            }

            AccessTools.Field(typeof(Actor), "s_action_attack_target")?.SetValue(__state.actor, __state.originalAttackActions);
        }
    }

    [HarmonyPatch(typeof(StatusLibrary), "checkPossessedAttackRight")]
    private static class KaijuPossessionKickSpecialAttackPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null || !ControllableUnit.isControllingUnit(pActor))
            {
                return true;
            }

            if (IsKaijuBurrowed(pActor))
            {
                return false;
            }

            if (!HasKaijuSpecialAttackDecision(pActor))
            {
                return true;
            }

            if (!ControllableUnit.isAttackJustPressedRight() || pActor.asset == null || !pActor.asset.control_can_kick || pActor.asset.skip_fight_logic || !pActor.isAttackReady())
            {
                return true;
            }

            ExecuteKaijuSpecialAttack(pActor, pFromPossessionKick: true);
            return false;
        }
    }

    [HarmonyPatch(typeof(StatusLibrary), "checkPossessedAttackLeft")]
    private static class KaijuPossessionLeftAttackSuppressOnKickPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null || !ControllableUnit.isControllingUnit(pActor))
            {
                return true;
            }

            if (IsKaijuBurrowed(pActor))
            {
                return false;
            }

            if (!HasKaijuSpecialAttackDecision(pActor))
            {
                return true;
            }

            if (ControllableUnit.isAttackJustPressedRight())
            {
                return false;
            }

            if (ControllableUnit.isAttackPressedRight())
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.calculateMainSprite))]
    private static class BossAttackAnimationOverlayPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance, ref Sprite __result)
        {
            if (__instance == null || __instance.data == null)
            {
                return;
            }

            KaijuDecisionFlags flags = GetKaijuDecisionFlags(__instance);
            if (flags == null || (!flags.IsManagedKaiju && !flags.HasBossAttackAnimation))
            {
                return;
            }

            if (TryGetKaijuBurrowSprite(__instance, out Sprite burrowSprite))
            {
                __result = burrowSprite;
                return;
            }

            if (!HasBossAttackAnimationDecision(__instance))
            {
                return;
            }

            if (!__instance.isAlive() || __instance.isEgg() || __instance.isBaby())
            {
                __instance.data.set("mb_boss_attack_anim_path", string.Empty);
                return;
            }

            __instance.data.get("mb_boss_attack_anim_path", out string attackPath, string.Empty);
            if (string.IsNullOrEmpty(attackPath))
            {
                return;
            }

            Sprite[] frames = GetBossAttackFrames(attackPath);
            if (frames == null || frames.Length == 0)
            {
                __instance.data.set("mb_boss_attack_anim_path", string.Empty);
                return;
            }

            __instance.data.get("mb_boss_attack_anim_start", out float startedAt, 0f);
            __instance.data.get("mb_boss_attack_anim_frame_time", out float frameTime, 0.06f);
            if (frameTime <= 0f)
            {
                frameTime = 0.06f;
            }

            float elapsed = Time.time - startedAt;
            int frameIndex = Mathf.FloorToInt(elapsed / frameTime);
            if (frameIndex < 0)
            {
                frameIndex = 0;
            }

            if (frameIndex >= frames.Length)
            {
                __instance.data.set("mb_boss_attack_anim_path", string.Empty);
                return;
            }

            Sprite frame = frames[frameIndex];
            if (frame == null)
            {
                __instance.data.set("mb_boss_attack_anim_path", string.Empty);
                return;
            }

            __result = frame;
        }
    }

        [HarmonyPatch(typeof(Actor), nameof(Actor.b6_updateAI))]
        private static class KaijuBurrowHardLockAIPatch
        {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance)
        {
            if (!IsKaijuDecisionActor(__instance))
            {
                return true;
            }

            if (!IsKaijuBurrowed(__instance))
            {
                return true;
            }

            ForceKaijuBurrowDisengage(__instance, pStopMovement: true);
            UpdateKaijuBurrowedState(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.b6_updateAI))]
    private static class KaijuForcedAdvanceAIPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            if (__instance == null || !__instance.isAlive() || __instance.isEgg() || __instance.isBaby())
            {
                return;
            }

            if (!HasKaijuSpecialAttackDecision(__instance) && !HasKaijuMapwideAggroOverrideDecision(__instance))
            {
                return;
            }

            UpdateForcedAdvance(__instance);
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.setAttackTarget))]
        private static class KaijuBurrowSuppressAttackTargetPatch
        {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance)
        {
            if (!IsKaijuDecisionActor(__instance))
            {
                return true;
            }

            if (!IsKaijuBurrowed(__instance))
            {
                return true;
            }

            __instance.clearAttackTarget();
            return false;
        }
    }

    [HarmonyPatch(typeof(Actor), "isAttackPossible")]
        private static class KaijuBurrowAttackSuppressionPatch
        {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance, ref bool __result)
        {
            if (!IsKaijuDecisionActor(__instance))
            {
                return true;
            }

            if (!IsKaijuBurrowed(__instance))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(BaseSimObject), "canAttackTarget")]
        private static class KaijuBurrowTargetImmunityPatch
        {
        [HarmonyPrefix]
        private static bool Prefix(BaseSimObject pTarget, ref bool __result)
        {
            if (pTarget != null && pTarget.isActor() && IsKaijuDecisionActor(pTarget.a) && IsKaijuBurrowed(pTarget.a))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

	public static BaseEffect spawnAtTile(string pID, WorldTile pTile, float pScale)
	{
		BaseEffect tEffect = spawn(pID, pTile);
		if (tEffect == null)
		{
			return null;
		}
		tEffect.prepare(pTile, pScale);
		return tEffect;
	}

public static BaseEffect spawn(string pID, WorldTile pTile = null, string pParam1 = null, string pParam2 = null, float pFloatParam1 = 0f, float pX = -1f, float pY = -1f, Actor pActor = null)
	{
		BaseEffect tEffect = check(pID);
		if (tEffect == null)
		{
			return null;
		}
		EffectAsset tAsset = AssetManager.effects_library.get(pID);
		if (tAsset.spawn_action != null)
		{
			tAsset.spawn_action(tEffect, pTile, pParam1, pParam2, pFloatParam1, pActor);
		}
		if (tAsset.has_sound_launch)
		{
			float tX = pX;
			float tY = pY;
			if (pTile != null && tX == -1f && tY == -1f)
			{
				tX = pTile.x;
				tY = pTile.y;
			}
			MusicBox.playSound(tAsset.sound_launch, tX, tY);
		}
		if (pX != -1f && pY != -1f)
		{
			tEffect.transform.position = new Vector3(pX, pY, 0f);
		}
		if (tAsset.has_sound_loop_idle)
		{
			tEffect.fmod_instance = MusicBox.attachToObject(tAsset.sound_loop_idle, tEffect.gameObject, tEffect);
		}
		return tEffect;
	}

private static BaseEffect check(string pID)
	{
		EffectAsset tAsset = AssetManager.effects_library.get(pID);
		if (tAsset == null)
		{
			return null;
		}
		if (tAsset.cooldown_interval > 0.0 && tAsset.checkIsUnderCooldown())
		{
			return null;
		}
		if (!tAsset.show_on_mini_map && MapBox.isRenderMiniMap())
		{
			return null;
		}
		return World.world.stack_effects.get(pID).spawnNew();
	}









    }
}
