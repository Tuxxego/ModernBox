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
                GodPower power = new GodPower
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
                };
                AssetManager.powers.add(power);
                ModernLocalization.Add(powerId, spec.DisplayName);
                ModernLocalization.Add(powerId + "_description", BombDescription(spec.Id));
            }
        }

        private static void RegisterCustomEffects()
        {
            const string strike = "event:/SFX/EXPLOSIONS/ExplosionLightningStrike";
            RegisterEffect("fx_color_grenade", "effects/Colornade", strike);
            RegisterEffect("fx_blood_lightning", "effects/BloodLightning", strike);
            RegisterEffect("fx_explosion_blue", "effects/NoNuke", strike);
            RegisterEffect("fx_explosion_dank", "effects/DankEffect", strike);
            RegisterEffect("fx_dankymatter_effect", "effects/kameboomtesttest", null);
        }

        private static void RegisterEffect(string id, string path, string sound)
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
                limit = 100,
                sound_launch = sound
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

        private static string BombDescription(string id)
        {
            string text;
            return Descriptions.TryGetValue(id, out text) ? text : "";
        }

        private static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "MOAB", "Also known as the 'Lag Bomb'." },
            { "Cobalt", "Small Mushroom but huge radius, watch out with this one." },
            { "Ultron", "WOOOAH" },
            { "Death", "Such an original name." },
            { "Xenium", "You thought the ultron bomb was big? This thing is HUGE." },
            { "Mini", "Small nukes, great for minor scuffles." },
            { "Proton", "wtf is this?" },
            { "Jupiter", "The new monster." },
            { "Eraser", "also known as the overcompensating bomb." },
            { "Random", "You could be dropping a proton bomb, or a mini nuke, it's random!" },
            { "AtomicGrenade", "A warcrime in the palm of your hand." },
            { "FuryOfTuxia", "Dank told me to stop making nukes, I instead decided to create this monstrosity (if your computer survives this, you're cool!)" },
            { "ZeussRage", "Tremble in fear Kratos." },
            { "NotSoAtomic", "The bomb that wanted to become atomic but failed the test in 12th grade to become atomic" },
            { "ColorBomb", "Its a bomb with a colorfull effect" },
            { "DankyBomb", "There's really no hard limit to how long these achievement names can be and to be quite honest I'm rather curious to see how far we can go. Adolphus W. Green (1844 to 1917) started as the Principal of the Groton School in 1864. By 1865, he became second assistant librarian at the New York Mercantile Library; from 1867 to 1869, he was promoted to full librarian. From 1869 to 1873, he worked for Evarts, Southmayd & Choate, a law firm co-founded by William M. Evarts, Charles Ferdinand Southmayd and Joseph Hodges Choate. He was admitted to the New York State Bar Association in 1873. Anyway, how's your day been?" },
            { "BloodLightning", "Forgive me for I have gone mad -Zeus" },
            { "NoDamage", "Its a bomb that looks cool and thats it. have Fun :)" },
            { "ClusterNuke", "EXACTLY what the title says." },
            { "ClusterStrike", "Damn is it Stormy bro, or am i just trippin?" },
            { "Spreader", "Quickly spreads to engulf your whole world in fire." }
        };
    }
}
