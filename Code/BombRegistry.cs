using System;
using System.Collections.Generic;
using NCMS.Utils;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class BombRegistry
    {
        private static readonly Dictionary<string, BombSpec> Drops = new Dictionary<string, BombSpec>(StringComparer.Ordinal);

        internal static void RegisterBombs()
        {
            RegisterCustomEffects();
            foreach (BombSpec spec in ContentRegistry.Bombs)
            {
                string dropId = "modernbox_drop_" + spec.Id;
                DropAsset drop = new DropAsset
                {
                    id = dropId,
                    type = DropType.DropBomb,
                    path_texture = spec.DropTexture,
                    default_scale = 0.2f,
                    random_frame = false,
                    random_flip = false,
                    sound_launch = "event:/SFX/DROPS/DropLaunchGrenadeHuge",
                    falling_speed = 3.2f,
                    falling_speed_random = 0.5f,
                    falling_height = new Vector2(60f, 70f),
                    falling_random_x_move = false,
                    surprises_units = true,
                    action_landed = OnBombLanded
                };
                AssetManager.drops.add(drop);
                Drops[dropId] = spec;

                string powerId = spec.Id + "button";
                GodPower power = new GodPower
                {
                    id = powerId,
                    name = powerId,
                    rank = PowerRank.Rank0_free,
                    path_icon = spec.IconPath,
                    hold_action = true,
                    show_tool_sizes = false,
                    ignore_cursor_icon = true,
                    falling_chance = 1f,
                    drop_id = dropId,
                    cached_drop_asset = drop,
                    click_power_action = SpawnBombDrop,
                    click_power_brush_action = null
                };
                AssetManager.powers.add(power);
                ModernLocalization.Add(powerId, spec.DisplayName);
                ModernLocalization.Add(powerId + "_description", BombDescription(spec));
            }
        }

        private static void RegisterCustomEffects()
        {
            RegisterEffect("fx_color_grenade", "effects/Colornade");
            RegisterEffect("fx_blood_lightning", "effects/BloodLightning");
            RegisterEffect("fx_explosion_blue", "effects/NoNuke");
            RegisterEffect("fx_explosion_dank", "effects/DankEffect");
            RegisterEffect("fx_dankymatter_effect", "effects/kameboomtesttest");
        }

        private static void RegisterEffect(string id, string path)
        {
            if (AssetManager.effects_library.get(id) != null) return;
            EffectAsset effect = new EffectAsset
            {
                id = id,
                use_basic_prefab = true,
                sorting_layer_id = "EffectsTop",
                sprite_path = path,
                show_on_mini_map = true,
                draw_light_area = true,
                draw_light_size = 2f,
                draw_light_area_offset_y = 5f,
                limit = 100
            };
            AssetManager.effects_library.add(effect);
        }

        private static bool SpawnBombDrop(WorldTile tile, GodPower power)
        {
            if (tile == null || power == null || power.cached_drop_asset == null) return false;
            return AssetManager.powers.spawnDrops(tile, power);
        }

        private static void OnBombLanded(WorldTile tile, string dropId)
        {
            BombSpec spec;
            if (tile != null && Drops.TryGetValue(dropId, out spec)) BombService.EnqueueBlast(tile, spec);
        }

        private static string BombDescription(BombSpec spec)
        {
            if (spec.Id == "Random") return "Drops one of M2's original random bomb sizes.";
            if (spec.Pattern == BombPattern.VisualOnly) return "A radius-30 visual effect that causes no damage.";
            return spec.DisplayName + " with M2's original radius of " + spec.Radius + ".";
        }
    }
}

