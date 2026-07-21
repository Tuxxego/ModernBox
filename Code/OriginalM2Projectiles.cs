using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Direct 0.51.2 equivalents of M2's non-space projectile and ranged-attack
    /// assets.  The IDs, textures, effects, speeds, scales, terrain actions and
    /// item-to-projectile mappings come from the original M2 source.
    /// </summary>
    internal static class OriginalM2Projectiles
    {
        // M2 used speed 3 with the pre-2025 parabolic projectile implementation.
        // Build 719 applies 9.8 gravity to projectile velocity, so speed 3 can
        // travel only about one tile before landing. Fifty is the direct
        // ballistic equivalent for MissileSystem's original 150-tile range and
        // leaves margin for target-height differences without becoming hitscan.
        internal const float MirvArtillerySpeed = 50f;
        // M5's strategic MissileSystem projectile is a straight, speed-100 shot.
        // Keep that translation separate from M2's parabolic MIRV equipment round
        // so the strategic decision can cross a full map without falling short.
        internal const float StrategicMirvArtillerySpeed = 100f;

        private sealed class ProjectileSpec
        {
            internal string Id;
            internal string Texture;
            internal float Speed;
            internal float StartScale;
            internal float TargetScale;
            internal bool Parabolic;
            internal bool LookAtTarget = true;
            internal bool DrawLight;
            internal float LightSize = 1f;
            internal bool HitFreeze;
            internal string Terraform = string.Empty;
            internal int TerraformRange;
            internal string EndEffect = string.Empty;
            internal string Trail = string.Empty;
            internal float TrailScale = 0.1f;
            internal float TrailTimer = 0.1f;
            internal bool TrailEnabled;
            internal string SoundLaunch = string.Empty;
            internal string SoundImpact = string.Empty;
            internal bool BurnTile;
        }

        internal static readonly Dictionary<string, string> AttackProjectiles = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "incendiarybombing", "torch" },
            { "icebolt", "frostbolt" },
            { "snowthrow", "bigsnowball" },
            { "singleshot", "shotgun_bullet" },
            { "machinegunery", "shotgun_bullet" },
            { "artillerystriker", "artilleryshell" },
            { "missilelauncherlong", "MIRVartillery" },
            { "missilelaunchershort", "RPGload" },
            { "arrowstriker", "arrow" },
            { "cannonstriker", "cannonballprojectile" },
            { "bigbulletattack", "bigbullet" },
            { "tankshellattack", "tankshell" },
            { "bluetankplasma", "bluebigplasma" },
            { "redtankplasma", "redbigplasma" },
            { "greentankplasma", "greenbigplasma" },
            { "crabartillery", "crabartilleryshell" },
            { "bomberino", "bigbomb" },
            { "destroyerbot", "seismicrod" },
            { "JetRocket", "jetrocketprojectile" },
            { "heliRocket", "helirocketprojectile" },
            { "GunshipCannon", "shotgun_bullet" },
            { "bigplasmabomb", "big_plasma_bomb" },
            { "thunderartillery", "thunderplasma" },
            { "hyperartillery", "hyperkame" },
            { "rockthrow", "Stone" },
            { "snowballindaface", "yugesnowball" }
        };

        internal static readonly string[] CustomProjectileIds =
        {
            "frostbolt", "bigsnowball", "cybermissileprojectile", "artilleryshell", "cannonballprojectile",
            "tankshell", "bigbullet", "blueplasma", "redplasma", "greenplasma", "bluemediumplasma",
            "redmediumplasma", "greenmediumplasma", "bluebigplasma", "redbigplasma", "greenbigplasma",
            "crabartilleryshell", "bigbomb", "seismicrod", "RPGload", "jetrocketprojectile",
            "helirocketprojectile", "MIRVartillery", "MIRVartilleryStrategic", "Stone", "yugesnowball", "thunderplasma",
            "big_plasma_bomb", "hyperkame"
        };

        // WorldBox 0.51.2 no longer exposes legacy ProjectileAsset.parabolic.
        // Even the lower-angle mode still uses gravity, so preserve every M2
        // projectile which explicitly declared parabolic=false through the
        // scoped straight-line movement patch. Deliberate artillery, thrown,
        // bombing and MIRV arcs are intentionally absent from this set.
        internal static readonly HashSet<string> LegacyDirectProjectileIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "tankshell",
            "bigbullet",
            "blueplasma",
            "redplasma",
            "greenplasma",
            "bluemediumplasma",
            "redmediumplasma",
            "greenmediumplasma",
            "bluebigplasma",
            "redbigplasma",
            "greenbigplasma",
            "seismicrod",
            "RPGload",
            "jetrocketprojectile",
            "helirocketprojectile",
            "MIRVartilleryStrategic",
            "big_plasma_bomb"
        };

        internal static bool UsesLegacyDirectTrajectory(string projectileId)
        {
            return !string.IsNullOrEmpty(projectileId) && LegacyDirectProjectileIds.Contains(projectileId);
        }

        internal static string GetEquipmentProjectile(string id)
        {
            switch (id)
            {
                case "RocketLauncher": return "RPGload";
                case "MIRV": return "MIRVartillery";
                case "MIRVBomb": return "bigbomb";
                case "blueheavyblaster": return "bluemediumplasma";
                case "redheavyblaster": return "redmediumplasma";
                case "greenheavyblaster": return "greenmediumplasma";
                case "blueblastersniper":
                case "blueblaster":
                case "blueminigun":
                case "blueplasmagun": return "blueplasma";
                case "redblastersniper":
                case "redblaster":
                case "redminigun":
                case "redplasmagun": return "redplasma";
                case "greenblastersniper":
                case "greenblaster":
                case "greenminigun":
                case "greenplasmagun": return "greenplasma";
                default: return "shotgun_bullet";
            }
        }

        internal static void Register()
        {
            RegisterTerraform("nonannoyingbomb", true, false, 2, false, true);
            RegisterTerraform("nonannoyingbullet", true, false, 0, false, false);
            RegisterTerraform("antiairbomb", true, false, 2, true, false);

            RegisterEffect("groundshake", "effects/groundshake", string.Empty, 2f, 5f, 100);
            RegisterEffect("Shermanboom", "effects/Shermanboom", string.Empty, 1f, 0f, 80);
            RegisterEffect("frosttrail", "effects/frosttrail", string.Empty, 1f, 0f, 80);
            RegisterEffect("frostspell", "effects/frostspell", string.Empty, 0.2f, 0f, 80);
            RegisterEffect("icespikes", "effects/fx_basic/icespikes", string.Empty, 1f, 0f, 80);
            RegisterEffect("blueplasmaboom", "effects/blueplasmaboom", "event:/SFX/EXPLOSIONS/ExplosionSmall", 1f, 0f, 80);
            RegisterEffect("redplasmaboom", "effects/redplasmaboom", "event:/SFX/EXPLOSIONS/ExplosionSmall", 1f, 0f, 80);
            RegisterEffect("greenplasmaboom", "effects/greenplasmaboom", "event:/SFX/EXPLOSIONS/ExplosionSmall", 1f, 0f, 80);
            RegisterEffect("kameboom", "effects/kameboomtest", "event:/SFX/EXPLOSIONS/ExplosionSmall", 1f, 0f, 80);
            RegisterEffect("hyperboom", "effects/hyperboom", "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb", 1f, 0f, 80);
            RegisterEffect("greenbigboom", "effects/greenbigboom", "event:/SFX/EXPLOSIONS/ExplosionSmall", 1f, 0f, 80);
            RegisterEffect("redbigboom", "effects/redbigboom", "event:/SFX/EXPLOSIONS/ExplosionSmall", 1f, 0f, 80);

            RegisterProjectile(P("frostbolt", "frostbolt", 15f, 0.075f, 0.2f, true, "", 2, "icespikes", false, true, true, "frosttrail", true));
            RegisterProjectile(P("bigsnowball", "bigsnowball", 4f, 0.075f, 0.4f, true, "", 0, "", true, false, true));
            RegisterProjectile(P("cybermissileprojectile", "cybermissileprojectile", 30f, 0.2f, 0.2f, true, "antiairbomb", 1, "fx_boat_explosion", false, true, true, "", false, true));
            RegisterProjectile(P("artilleryshell", "artilleryshell", 19f, 0.2f, 0.2f, true, "nonannoyingbomb", 3, "fx_explosion_middle", false, true, true, "", false, true));
            RegisterProjectile(P("cannonballprojectile", "cannonballprojectile", 10f, 0.2f, 0.2f, true, "nonannoyingbomb", 3, "groundshake", false, true, true));
            RegisterProjectile(P("tankshell", "artilleryshell", 45f, 0.2f, 0.2f, false, "nonannoyingbomb", 3, "fx_boat_explosion", false, true, true, "", false, true));
            RegisterProjectile(P("bigbullet", "shotgun_bullet", 45f, 0.1f, 0.1f, false, "nonannoyingbullet", 1, "", false, true, true, "", false, false, "event:/SFX/WEAPONS/WeaponShotgunStart", "event:/SFX/WEAPONS/WeaponShotgunLand"));

            RegisterPlasma("blueplasma", "blueplasma", 0.07f, string.Empty);
            RegisterPlasma("redplasma", "redplasma", 0.07f, string.Empty);
            RegisterPlasma("greenplasma", "greenplasma", 0.07f, string.Empty);
            RegisterPlasma("bluemediumplasma", "blueplasma", 0.2f, "blueplasmaboom");
            RegisterPlasma("redmediumplasma", "redplasma", 0.2f, "redplasmaboom");
            RegisterPlasma("greenmediumplasma", "greenplasma", 0.2f, "greenplasmaboom");
            RegisterPlasma("bluebigplasma", "blueplasma", 0.4f, "kameboom", 20f);
            RegisterPlasma("redbigplasma", "redplasma", 0.4f, "redbigboom", 20f);
            RegisterPlasma("greenbigplasma", "greenplasma", 0.4f, "greenbigboom", 20f);

            RegisterProjectile(P("crabartilleryshell", "shotgun_bullet", 20f, 1f, 1f, true, "crab_bomb", 5, "fx_explosion_middle", false, true, true, "", false, true));
            RegisterProjectile(P("bigbomb", "bigbomb", 20f, 0.2f, 0.2f, true, "nonannoyingbomb", 3, "Shermanboom", false, true, true, "smoketrail", false, true));
            RegisterProjectile(P("seismicrod", "seismicrod", 10f, 0.3f, 0.3f, false, "antiairbomb", 20, "fx_explosion_middle", false, true, true, "smoketrail", false, true));
            RegisterProjectile(P("RPGload", "RPG", 40f, 0.3f, 0.3f, false, "nonannoyingbomb", 3, "fx_fireball_explosion", false, true, true));
            RegisterProjectile(P("jetrocketprojectile", "jetrocketprojectile", 40f, 0.3f, 0.3f, false, "antiairbomb", 3, "fx_fireball_explosion", false, true, true, "smoketrail"));
            RegisterProjectile(P("helirocketprojectile", "jetrocketprojectile", 30f, 0.1f, 0.1f, false, "antiairbomb", 3, "fx_fireball_explosion", false, true, true, "smoketrail"));
            RegisterProjectile(P("MIRVartillery", "MIRVartillery", MirvArtillerySpeed, 0.2f, 0.2f, true, "nonannoyingbomb", 4, "fx_explosion_meteorite", false, true, true, "smoketrail", false, true));
            RegisterProjectile(P("MIRVartilleryStrategic", "MIRVartillery", StrategicMirvArtillerySpeed, 0.2f, 0.2f, false, "nonannoyingbomb", 4, "fx_explosion_meteorite", false, true, true, "smoketrail", false, true));
            // Original M2 used the meteorite explosion without an explicit scale.
            // Build 719 renders that inherited effect much larger, obscuring most
            // of a battle. Reduce only the MissileSystem service's strategic visual;
            // its projectile damage, terraform option, and radius remain unchanged.
            AssetManager.projectiles.get("MIRVartilleryStrategic").end_effect_scale = 0.35f;
            RegisterProjectile(P("Stone", "Stone", 4f, 0.075f, 0.2f, true, "", 0, "groundshake", false, false, false));
            RegisterProjectile(P("yugesnowball", "snowball", 4f, 0.25f, 0.5f, true, "", 0, "", true, false, true));
            RegisterProjectile(P("thunderplasma", "thunderplasma", 20f, 0.3f, 0.3f, true, "nonannoyingbomb", 4, "kameboom", false, true, true, "", false, true));
            RegisterProjectile(P("big_plasma_bomb", "kame", 20f, 0.2f, 1f, false, "nonannoyingbomb", 4, "kameboom", false, true, true, "", false, true));
            RegisterProjectile(P("hyperkame", "hyperkame", 5f, 0.1f, 1f, true, "nonannoyingbomb", 5, "hyperboom", false, true, true, "", false, true));
        }

        internal static void RegisterAttackAssets()
        {
            RegisterAttack("incendiarybombing", Stats("targets", 2f, "range", 0f, "projectiles", 1f));
            RegisterAttack("icebolt", Stats("targets", 1f, "range", 0f, "projectiles", 1f, "critical_chance", 0.3f, "critical_damage_multiplier", 0.4f));
            RegisterAttack("snowthrow", Stats("targets", 10f, "range", 0f, "projectiles", 1f, "critical_chance", 0.3f, "critical_damage_multiplier", 0.8f));
            RegisterAttack("singleshot", Stats("projectiles", 1f, "attack_speed", 50f, "range", 0f, "targets", 1f, "damage", 10f, "damage_range", 0.5f));
            RegisterAttack("machinegunery", Stats("projectiles", 1f, "attack_speed", 10000f, "range", 0f, "targets", 1f, "damage", 3f, "damage_range", 0.1f));
            RegisterAttack("artillerystriker", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 1f, "damage", 60f, "damage_range", 0.7f));
            RegisterAttack("missilelauncherlong", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 1f, "damage", 0f, "damage_range", 0.7f));
            RegisterAttack("missilelaunchershort", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 1f, "damage", 60f, "damage_range", 0.7f));
            RegisterAttack("arrowstriker", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 1f, "damage", 0f, "damage_range", 0.7f));
            RegisterAttack("cannonstriker", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 4f, "damage", 0f, "damage_range", 0.7f));
            RegisterAttack("bigbulletattack", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 3f, "damage", 30f, "damage_range", 0.7f));
            RegisterAttack("tankshellattack", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 3f, "damage", 60f, "damage_range", 0.7f));
            RegisterAttack("bluetankplasma", Stats("attack_speed", -500f, "range", 0f, "targets", 4f, "damage", 0f, "damage_range", 0.7f));
            RegisterAttack("redtankplasma", Stats("attack_speed", -500f, "range", 0f, "targets", 4f, "damage", 0f, "damage_range", 0.7f));
            RegisterAttack("greentankplasma", Stats("attack_speed", -500f, "range", 0f, "targets", 4f, "damage", 0f, "damage_range", 0.7f));
            RegisterAttack("crabartillery", Stats("projectiles", 4f, "attack_speed", 0.1f, "range", 0f, "targets", 1f, "damage", 100f, "damage_range", 0.7f));
            RegisterAttack("bomberino", Stats("projectiles", 1f, "attack_speed", 0.01f, "range", 0f, "targets", 7f, "damage", 30f, "damage_range", 0.5f));
            RegisterAttack("destroyerbot", Stats("projectiles", 1f, "attack_speed", 0.1f, "range", 0f, "targets", 10f, "damage", 20f, "damage_range", 0.5f));
            RegisterAttack("JetRocket", Stats("projectiles", 2f, "attack_speed", -101f, "range", 0f, "targets", 2f, "damage", 60f, "damage_range", 0.5f, "accuracy", 400f));
            RegisterAttack("heliRocket", Stats("projectiles", 2f, "attack_speed", 1000f, "range", 0f, "targets", 1f, "damage", 16f, "damage_range", 0.5f, "accuracy", 400f));
            RegisterAttack("GunshipCannon", Stats("range", 0f, "accuracy", 400f, "attack_speed", 200f, "damage", 13f));
            RegisterAttack("bigplasmabomb", Stats("projectiles", 1f, "attack_speed", 90f, "range", 0f, "targets", 4f, "damage", 6f, "damage_range", 0.7f));
            RegisterAttack("thunderartillery", Stats("projectiles", 1f, "attack_speed", 90f, "range", 0f, "targets", 10f, "damage", 6f, "damage_range", 0.7f));
            RegisterAttack("hyperartillery", Stats("projectiles", 1f, "attack_speed", -20000f, "range", 0f, "targets", 20f, "damage", 6f, "damage_range", 0.7f));
            RegisterAttack("rockthrow", Stats("targets", 1f, "range", 16f, "projectiles", 1f));
            RegisterAttack("snowballindaface", Stats("targets", 10f, "range", 16f, "projectiles", 1f, "critical_chance", 0.3f, "critical_damage_multiplier", 0.8f));
        }

        private static ProjectileSpec P(string id, string texture, float speed, float startScale, float targetScale,
            bool parabolic, string terraform, int terraformRange, string endEffect, bool hitFreeze,
            bool drawLight, bool lookAtTarget, string trail = "", bool trailEnabled = false, bool burnTile = false,
            string soundLaunch = "", string soundImpact = "")
        {
            return new ProjectileSpec
            {
                Id = id,
                Texture = texture,
                Speed = speed,
                StartScale = startScale,
                TargetScale = targetScale,
                Parabolic = parabolic,
                Terraform = terraform,
                TerraformRange = terraformRange,
                EndEffect = endEffect,
                HitFreeze = hitFreeze,
                DrawLight = drawLight,
                LookAtTarget = lookAtTarget,
                Trail = trail,
                TrailEnabled = trailEnabled,
                BurnTile = burnTile,
                SoundLaunch = soundLaunch,
                SoundImpact = soundImpact
            };
        }

        private static void RegisterPlasma(string id, string texture, float scale, string endEffect, float speed = 15f)
        {
            RegisterProjectile(P(id, texture, speed, scale, scale, false, "nonannoyingbullet", 1, endEffect,
                false, true, true, "", false, false,
                "event:/SFX/WEAPONS/WeaponPlasmaBallStart", "event:/SFX/WEAPONS/WeaponPlasmaBallLand"));
        }

        private static void RegisterProjectile(ProjectileSpec spec)
        {
            ProjectileAsset projectile = AssetManager.projectiles.get(spec.Id);
            bool add = projectile == null;
            if (add) projectile = new ProjectileAsset { id = spec.Id };

            Sprite[] frames = Resources.LoadAll<Sprite>("effects/projectiles/" + spec.Texture);
            if (frames == null || frames.Length == 0)
            {
                Sprite frame = Resources.Load<Sprite>("effects/projectiles/" + spec.Texture + "/0");
                frames = frame == null ? new Sprite[0] : new[] { frame };
            }
            if (frames.Length == 0 || frames[0] == null)
                throw new InvalidOperationException("Missing original M2 projectile frames: " + spec.Id + " -> " + spec.Texture);

            projectile.id = spec.Id;
            projectile.texture = spec.Texture;
            projectile.frames = frames;
            projectile.animated = frames.Length > 1;
            projectile.animation_speed = 10f;
            projectile.speed = spec.Speed;
            projectile.speed_random = 0f;
            projectile.terraform_option = spec.Terraform ?? string.Empty;
            projectile.terraform_range = spec.TerraformRange;
            projectile.end_effect = spec.EndEffect ?? string.Empty;
            projectile.end_effect_scale = 1f;
            projectile.sound_launch = spec.SoundLaunch ?? string.Empty;
            projectile.sound_impact = spec.SoundImpact ?? string.Empty;
            projectile.look_at_target = spec.LookAtTarget;
            projectile.trail_effect_enabled = spec.TrailEnabled;
            projectile.trail_effect_id = spec.Trail ?? string.Empty;
            projectile.trail_effect_scale = spec.TrailScale;
            projectile.trail_effect_timer = spec.TrailTimer;
            projectile.hit_freeze = spec.HitFreeze;
            projectile.hit_shake = false;
            projectile.scale_start = spec.StartScale;
            projectile.scale_target = spec.TargetScale;
            projectile.texture_shadow = string.Empty;
            projectile.draw_light_area = spec.DrawLight;
            projectile.draw_light_size = spec.DrawLight ? spec.LightSize : 0f;
            projectile.trigger_on_collision = false;
            projectile.can_be_collided = true;
            projectile.can_be_left_on_ground = false;
            projectile.can_be_blocked = false;
            projectile.use_min_angle_height = spec.Parabolic;
            projectile.mass = 1f;
            projectile.size = 0.5f;
            projectile.impact_actions = null;
            projectile.world_actions = spec.BurnTile ? new AttackAction(ActionLibrary.burnTile) : null;
            if (add) AssetManager.projectiles.add(projectile);
        }

        private static void RegisterTerraform(string id, bool damageBuildings, bool explodeTile, int strength, bool highFlyers, bool setFire)
        {
            TerraformOptions terraform = AssetManager.terraform.get(id);
            bool add = terraform == null;
            if (add) terraform = new TerraformOptions { id = id };
            terraform.id = id;
            terraform.shake = false;
            terraform.explode_tile = explodeTile;
            terraform.damage_buildings = damageBuildings;
            terraform.damage = 0;
            terraform.explode_strength = strength;
            terraform.applies_to_high_flyers = highFlyers;
            terraform.set_fire = setFire;
            terraform.remove_ruins = false;
            terraform.attack_type = AttackType.Explosion;
            if (add) AssetManager.terraform.add(terraform);
        }

        private static void RegisterEffect(string id, string path, string sound, float lightSize, float lightOffsetY, int limit)
        {
            EffectAsset effect = AssetManager.effects_library.get(id);
            bool add = effect == null;
            if (add) effect = new EffectAsset { id = id };
            effect.id = id;
            effect.use_basic_prefab = true;
            effect.sorting_layer_id = "EffectsTop";
            effect.sprite_path = path;
            effect.show_on_mini_map = true;
            effect.draw_light_area = true;
            effect.draw_light_size = lightSize;
            effect.draw_light_area_offset_y = lightOffsetY;
            effect.sound_launch = sound ?? string.Empty;
            effect.limit = limit;
            if (add) AssetManager.effects_library.add(effect);
        }

        private static Dictionary<string, float> Stats(params object[] values)
        {
            Dictionary<string, float> stats = new Dictionary<string, float>(StringComparer.Ordinal);
            for (int i = 0; i + 1 < values.Length; i += 2)
                stats[(string)values[i]] = Convert.ToSingle(values[i + 1]);
            return stats;
        }

        private static void RegisterAttack(string id, Dictionary<string, float> stats)
        {
            if (ModernBoxCatalog.EquipmentIds.Contains(id)) return;
            string projectileId;
            if (!AttackProjectiles.TryGetValue(id, out projectileId))
                throw new InvalidOperationException("Missing original M2 attack projectile mapping: " + id);

            EquipmentAsset attack = AssetManager.items.get(id);
            if (attack == null) attack = AssetManager.items.clone(id, "$range");
            attack.id = id;
            attack.translation_key = id;
            attack.has_locales = false;
            // These are implementation-only actor attacks, not loot.  Exposing them
            // made EquipmentEditor create buttons for assets that intentionally have
            // no inventory icon, which crashed BaseUnlockableAsset.getSprite().
            attack.show_in_meta_editor = false;
            attack.show_in_knowledge_window = false;
            attack.show_for_unlockables_ui = false;
            attack.can_be_given = false;
            attack.group_id = "firearm";
            // Defensive fallback for any external UI that enumerates every item asset
            // without respecting the visibility flags.
            attack.path_icon = "ui/Icons/firearm";
            attack.projectile = projectileId;
            attack.is_pool_weapon = false;
            attack.pool_rate = 0;
            attack.equipment_value = id == "GunshipCannon" ? 300 : 0;
            attack.path_slash_animation = "effects/slashes/slash_punch";
            foreach (KeyValuePair<string, float> pair in stats) attack.base_stats[pair.Key] = pair.Value;
        }
    }
}
