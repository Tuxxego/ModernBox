using System;
using System.Collections.Generic;
using NCMS.Utils;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class BombRegistry
    {
        private static readonly Dictionary<string, BombSpec> Drops = new Dictionary<string, BombSpec>(StringComparer.Ordinal);

        internal static void RegisterBombs()
        {
            foreach (BombSpec spec in ContentRegistry.Bombs)
            {
                string dropId = "modernbox_drop_" + spec.Id;
                DropAsset drop = new DropAsset
                {
                    id = dropId,
                    type = DropType.DropBomb,
                    path_texture = spec.DropTexture,
                    default_scale = 0.22f,
                    random_frame = true,
                    random_flip = true,
                    falling_speed = 8f,
                    falling_speed_random = 1f,
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
                    show_tool_sizes = true,
                    ignore_cursor_icon = true,
                    falling_chance = 1f,
                    drop_id = dropId,
                    cached_drop_asset = drop,
                    click_power_action = SpawnBombDrop,
                    click_power_brush_action = AssetManager.powers.loopWithCurrentBrushPowerForDropsFull
                };
                AssetManager.powers.add(power);
                ModernLocalization.Add(powerId, spec.DisplayName);
                ModernLocalization.Add(powerId + "_description", BombDescription(spec));
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
            if (tile != null && Drops.TryGetValue(dropId, out spec)) BombService.Enqueue(tile, spec);
        }

        private static string BombDescription(BombSpec spec)
        {
            if (spec.Id == "Random") return "Drops one of the original M1 random bomb sizes.";
            return spec.DisplayName + " with the original M1 radius of " + spec.Radius + ". Large blasts complete in safe frame-sized chunks.";
        }
    }
}
