using System;
using UnityEngine;
using UnityEngine.Events;
using NCMS.Utils;
using NeoModLoader.General;
using ReflectionUtility;
using System.Reflection;
using ModernBox;
public class BombEffects
{
    public static void Init() 
    {
        LoadEffects();
    }
    public static TerraformOptions EraserTerraform;

    //Effects In Vanilla Game
    //
    //fx_spores
    //fx_fireball_explosion
    //fx_firebomb_explosion
    //fx_plasma_ball_explosion
    //fx_cast_ground_blue
    //fx_cast_top_blue
    //fx_cast_ground_red
    //fx_cast_top_red
    //fx_cast_ground_green
    //fx_cast_ground_purple
    //fx_cast_top_green
    //fx_create_skeleton
    //fx_teleport_blue
    //fx_teleport_red
    //fx_shield_hit
    //fx_dodge
    //fx_block
    //fx_drowning
    //fx_water_splash
    //fx_grin_reaper
    //fx_monolith_launch
    //fx_monolith_launch_bottom
    //fx_monolith_glow_1
    //fx_monolith_glow_2
    //fx_waypoint_alien_mold_launch_bottom
    //fx_waypoint_computer_launch_bottom
    //fx_waypoint_golden_egg_launch_bottom
    //fx_waypoint_harp_launch_bottom
    //fx_bad_place
    //fx_debug_tile
    //fx_move
    //fx_plasma_trail
    //fx_building_sparkle
    //fx_fire_smoke
    //fx_boulder_charge
    //fx_spark
    //fx_lightning_big
    //fx_lightning_medium
    //fx_lightning_small
    //fx_spawn
    //fx_teleport_singularity
    //fx_spawn_big
    //fx_land_explosion_old
    //fx_explosion_crab_bomb
    //fx_explosion_tiny
    //fx_explosion_small
    //fx_explosion_ufo
    //fx_explosion_meteorite
    //fx_explosion_middle
    //fx_explosion_nuke_atomic
    //fx_explosion_huge                                     LITTERALY THE MOST USED IN THIS MOD!!!!!!!!!!!!!!!!!!(for now)
    //fx_explosion_wave
    //fx_fireworks
    //fx_hearts
    //fx_new_border
    //fx_money_got_loot
    //fx_money_got_money
    //fx_money_paid_tax
    //fx_money_paid_tribute
    //fx_money_spend_money
    //fx_conversion_religion
    //fx_conversion_culture
    //fx_conversion_language
    //fx_experience_gain
    //fx_change_happiness_positive
    //fx_change_happiness_negative
    //fx_hmm
    //fx_plot_progress
    //fx_nuke_flash
    //fx_napalm_flash
    //fx_thunder_flash
    //fx_boulder_impact
    //fx_boulder_impact_water
    //fx_antimatter_effect
    //fx_infinity_coin
    //fx_status_particle
    //fx_weapon_particle
    //fx_slash
    //fx_hit
    //fx_miss
    //fx_jump
    //fx_walk
    //fx_hit_critical
    //fx_boat_explosion
    //fx_fishnet
    //fx_tile_effect
    //fx_cloud
    //fx_meteorite
    //fx_boulder
    //fx_santa
    //fx_zone_highlight
    //fx_tornado
    //
    //Effects Added In This Mod
    //fx_ice_bomb
    //fx_dankymatter_effect
    //fx_color_grenade
    //fx_blood_lightning
    //fx_explosion_blue
    //fx_explosion_dank
    //fx_testy_westy
    //fx_kwell_1
    //fx_kwell_2
    //fx_upward_explosion
    //fx_filled_map
    //fx_ground_explosion


    private static void LoadEffects()
    {
        EffectAsset IceBomb = new EffectAsset();
        IceBomb.id = "fx_ice_bomb";
        IceBomb.use_basic_prefab = true;
        IceBomb.sorting_layer_id = "EffectsTop";
        IceBomb.sprite_path = "Effects/IceBomb";
        IceBomb.show_on_mini_map = true;
        IceBomb.draw_light_area = true;
        IceBomb.draw_light_size = 2f;
        IceBomb.draw_light_area_offset_y = 5f;
        IceBomb.limit = 100;
        IceBomb.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(IceBomb);

        EffectAsset DankyMatter = new EffectAsset();
        DankyMatter.id = "fx_dankymatter_effect";
        DankyMatter.use_basic_prefab = true;
        DankyMatter.sorting_layer_id = "EffectsTop";
        DankyMatter.sprite_path = "Effects/DankyMatter";
        DankyMatter.draw_light_area = true;
        DankyMatter.limit = 100;
        DankyMatter.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(DankyMatter);

        EffectAsset ColorNade = new EffectAsset();
        ColorNade.id = "fx_color_grenade";
        ColorNade.use_basic_prefab = true;
        ColorNade.sorting_layer_id = "EffectsTop";
        ColorNade.sprite_path = "Effects/Colornade";
        ColorNade.show_on_mini_map = true;
        ColorNade.draw_light_area = true;
        ColorNade.draw_light_size = 2f;
        ColorNade.draw_light_area_offset_y = 5f;
        ColorNade.limit = 100;
        ColorNade.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(ColorNade);

        EffectAsset BloodLightning = new EffectAsset();
        BloodLightning.id = "fx_blood_lightning";
        BloodLightning.use_basic_prefab = true;
        BloodLightning.sorting_layer_id = "EffectsTop";
        BloodLightning.sprite_path = "Effects/BloodLightning";
        BloodLightning.show_on_mini_map = true;
        BloodLightning.draw_light_area = true;
        BloodLightning.draw_light_size = 2f;
        BloodLightning.draw_light_area_offset_y = 5f;
        BloodLightning.limit = 100;
        BloodLightning.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(BloodLightning);

        EffectAsset NoNuke = new EffectAsset();
        NoNuke.id = "fx_explosion_blue";
        NoNuke.use_basic_prefab = true;
        NoNuke.sorting_layer_id = "EffectsTop";
        NoNuke.sprite_path = "Effects/NoNuke";
        NoNuke.show_on_mini_map = true;
        NoNuke.draw_light_area = true;
        NoNuke.draw_light_size = 2f;
        NoNuke.draw_light_area_offset_y = 5f;
        NoNuke.limit = 100;
        NoNuke.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(NoNuke);

        EffectAsset DankEffect = new EffectAsset();
        DankEffect.id = "fx_explosion_dank";
        DankEffect.use_basic_prefab = true;
        DankEffect.sorting_layer_id = "EffectsTop";
        DankEffect.sprite_path = "Effects/DankEffect";
        DankEffect.show_on_mini_map = true;
        DankEffect.draw_light_area = true;
        DankEffect.draw_light_size = 2f;
        DankEffect.draw_light_area_offset_y = 5f;
        DankEffect.limit = 100;
        DankEffect.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(DankEffect);

        EffectAsset TestyWesty = new EffectAsset();
        TestyWesty.id = "fx_testy_westy";
        TestyWesty.use_basic_prefab = true;
        TestyWesty.sorting_layer_id = "EffectsTop";
        TestyWesty.sprite_path = "effects/DankEffect";
        TestyWesty.show_on_mini_map = true;
        TestyWesty.draw_light_area = true;
        TestyWesty.draw_light_size = 2f;
        TestyWesty.draw_light_area_offset_y = 5f;
        TestyWesty.limit = 100;
        TestyWesty.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(TestyWesty);

        EffectAsset Kwell = new EffectAsset();
        Kwell.id = "fx_kwell_1";
        Kwell.use_basic_prefab = true;
        Kwell.sorting_layer_id = "EffectsTop";
        Kwell.sprite_path = "effects/greenplasmaboom";
        Kwell.show_on_mini_map = true;
        Kwell.draw_light_area = true;
        Kwell.draw_light_size = 2f;
        Kwell.draw_light_area_offset_y = 5f;
        Kwell.limit = 100;
        Kwell.sound_launch = "event:/SFX/POWERS/TsarBomb";
        AssetManager.effects_library.add(Kwell);

        EffectAsset Kwell2 = new EffectAsset();
        Kwell2.id = "fx_kwell_2";
        Kwell2.use_basic_prefab = true;
        Kwell2.sorting_layer_id = "EffectsTop";
        Kwell2.sprite_path = "effects/blueplasmaboom";
        Kwell2.show_on_mini_map = true;
        Kwell2.draw_light_area = true;
        Kwell2.draw_light_size = 2f;
        Kwell2.draw_light_area_offset_y = 5f;
        Kwell2.limit = 100;
        Kwell2.sound_launch = "event:/SFX/POWERS/TsarBomb";
        AssetManager.effects_library.add(Kwell2);

        EffectAsset StraightUp = new EffectAsset();
        StraightUp.id = "fx_upward_explosion";
        StraightUp.use_basic_prefab = true;
        StraightUp.sorting_layer_id = "EffectsTop";
        StraightUp.sprite_path = "effects/N2explosion";
        StraightUp.show_on_mini_map = true;
        StraightUp.draw_light_area = true;
        StraightUp.draw_light_size = 2f;
        StraightUp.draw_light_area_offset_y = 5f;
        StraightUp.limit = 100;
        StraightUp.sound_launch = "event:/SFX/POWERS/TsarBomb";
        AssetManager.effects_library.add(StraightUp);

        EffectAsset FilledMap = new EffectAsset();
        FilledMap.id = "fx_filled_map";
        FilledMap.use_basic_prefab = true;
        FilledMap.sorting_layer_id = "EffectsTop";
        FilledMap.sprite_path = "effects/groundshake";
        FilledMap.show_on_mini_map = true;
        FilledMap.draw_light_area = true;
        FilledMap.draw_light_size = 2f;
        FilledMap.draw_light_area_offset_y = 5f;
        FilledMap.limit = 100;
        FilledMap.sound_launch = "event:/SFX/POWERS/TsarBomb";
        AssetManager.effects_library.add(FilledMap);

        EffectAsset GroundExplosion = new EffectAsset();
        GroundExplosion.id = "fx_ground_explosion";
        GroundExplosion.use_basic_prefab = true;
        GroundExplosion.sorting_layer_id = "EffectsTop";
        GroundExplosion.sprite_path = "effects/explosion";
        GroundExplosion.show_on_mini_map = true;
        GroundExplosion.draw_light_area = true;
        GroundExplosion.draw_light_size = 2f;
        GroundExplosion.draw_light_area_offset_y = 5f;
        GroundExplosion.limit = 100;
        GroundExplosion.sound_launch = "event:/SFX/EXPLOSIONS/TsarBomb";
        AssetManager.effects_library.add(GroundExplosion);
    }

    private static void LoadTerraforms()
    {
        EraserTerraform = new TerraformOptions();
        EraserTerraform.damage = 10000;
        EraserTerraform.explode_strength = 1;
        EraserTerraform.id = "eraser_explosion";
        EraserTerraform.remove_trees_fully = true;
        EraserTerraform.destroy_buildings = true;
        EraserTerraform.applies_to_high_flyers = true;
        EraserTerraform.shake = false;
        EraserTerraform.bomb_action = BombUtilities.Instance.action_EraserClickOTHER;
    }
    


}
