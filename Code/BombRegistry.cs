using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    // Buttonz.cs: each bomb is a brush power that rains drops (1% chance per brush tile,
    // 0.1% for Random) which blow up where they land.
    internal static class BombRegistry
    {
        private static readonly Dictionary<string, BombSpec> Drops = new Dictionary<string, BombSpec>(StringComparer.Ordinal);

        internal static void RegisterBombs()
        {
            foreach (BombSpec spec in ContentRegistry.Bombs)
            {
                string dropId = "modernbox_m1_drop_" + spec.Id;
                DropAsset drop = new DropAsset
                {
                    id = dropId,
                    type = DropType.DropBomb,
                    path_texture = spec.DropTexture,
                    default_scale = 0.2f,
                    random_frame = spec.Id == "MOAB",
                    random_flip = false,
                    falling_speed = 3.2f,
                    falling_speed_random = 0.5f,
                    falling_height = new Vector2(60f, 70f),
                    falling_random_x_move = false,
                    action_landed = OnBombLanded
                };
                AssetManager.drops.add(drop);
                Drops[dropId] = spec;

                string powerId = spec.Id + "button";
                AssetManager.powers.add(new GodPower
                {
                    id = powerId,
                    name = powerId,
                    rank = PowerRank.Rank0_free,
                    path_icon = spec.IconPath,
                    hold_action = true,
                    show_tool_sizes = true,
                    unselect_when_window = false,
                    ignore_cursor_icon = true,
                    falling_chance = spec.Pattern == BombPattern.RandomLegacy ? 0.001f : 0.01f,
                    drop_id = dropId,
                    cached_drop_asset = drop,
                    click_power_action = SpawnBombDrop,
                    click_power_brush_action = AssetManager.powers.loopWithCurrentBrushPowerForDropsFull
                });
                ModernLocalization.Add(powerId, spec.DisplayName);
                ModernLocalization.Add(powerId + "_description", spec.Description);
            }
        }

        private static bool SpawnBombDrop(WorldTile tile, GodPower power)
        {
            if (tile == null || power == null || power.cached_drop_asset == null) return false;
            return AssetManager.powers.spawnDrops(tile, power);
        }

        private static void OnBombLanded(WorldTile tile, string dropId)
        {
            BombSpec spec;
            if (tile != null && Drops.TryGetValue(dropId, out spec)) BombService.Detonate(tile, spec);
        }
    }
}
