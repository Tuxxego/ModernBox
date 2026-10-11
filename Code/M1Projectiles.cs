using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    // The projectiles and blasts M1 used (Resourcez.cs, guns.cs, MIRV.cs).
    internal static class M1Projectiles
    {
        // The old engine's shotgun_bullet: speed 20+-7, straight, never blocked. 0.51.2's is
        // speed 30, ballistic and blockable, so M1 guns get their own copy of the old one.
        internal const string LegacyBulletId = "modernbox_m1_bullet";
        // The old fireball (RPG and MIRVs): speed 15 and straight. 0.51.2's is speed 25 and ballistic.
        internal const string LegacyFireballId = "modernbox_m1_fireball";
        internal const string NukerExplodeId = "NUKERExplode";
        internal const string GunshotId = "gunshot";

        private static readonly HashSet<string> DirectIds = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> ArcIds = new HashSet<string>(StringComparer.Ordinal);

        internal static bool UsesLegacyTrajectory(string id)
        {
            return !string.IsNullOrEmpty(id) && (DirectIds.Contains(id) || ArcIds.Contains(id));
        }

        internal static bool UsesLegacyArc(string id)
        {
            return !string.IsNullOrEmpty(id) && ArcIds.Contains(id);
        }

        internal static void Register()
        {
            ProjectileAsset bullet = AssetManager.projectiles.get(LegacyBulletId) ??
                AssetManager.projectiles.clone(LegacyBulletId, "shotgun_bullet");
            bullet.speed = 20f;
            bullet.speed_random = 7f;
            bullet.can_be_blocked = false;
            bullet.can_be_collided = false;
            bullet.end_effect_scale = 0.25f;
            DirectIds.Add(LegacyBulletId);

            ProjectileAsset fireball = AssetManager.projectiles.get(LegacyFireballId) ??
                AssetManager.projectiles.clone(LegacyFireballId, "fireball");
            fireball.speed = 15f;
            fireball.speed_random = 0f;
            fireball.can_be_blocked = false;
            fireball.can_be_collided = false;
            // The old Projectile.targetReached always spawned the end effect at 0.25.
            fireball.end_effect_scale = 0.25f;
            DirectIds.Add(LegacyFireballId);

            // gunshot: a 1-tile blast for 30 damage that leaves buildings alone.
            AddTerraform(new TerraformOptions
            {
                id = GunshotId,
                damage = 30,
                damage_buildings = false,
                explode_tile = true,
                explosion_pixel_effect = true,
                remove_borders = false
            });

            // NUKERExplode: damage 10000, fire, wasteland, strength 1. It never set
            // applies_to_high_flyers, so flying units are spared.
            AddTerraform(new TerraformOptions
            {
                id = NukerExplodeId,
                flash = true,
                damage_buildings = true,
                damage = 10000,
                apply_force = true,
                force_power = 1.5f,
                explode_and_set_random_fire = true,
                explode_tile = true,
                explosion_pixel_effect = true,
                explode_strength = 1,
                transform_to_wasteland = true,
                shake = true,
                shake_duration = 0.3f,
                shake_interval = 0.01f,
                shake_intensity = 2f,
                remove_ruins = false,
                remove_tornado = false
            });

            // The silo missile: slow, arcing, grows from 0.01 to 0.2 on the way.
            ProjectileAsset nuker = NewProjectile("NUKER", "effects/projectiles/NUKER");
            nuker.speed = 3.5f;
            nuker.terraform_option = NukerExplodeId;
            nuker.terraform_range = 43;
            nuker.end_effect = "fx_explosion_nuke_atomic";
            nuker.hit_shake = true;
            nuker.shake_duration = 0.3f;
            nuker.shake_interval = 0.01f;
            nuker.shake_intensity = 2f;
            nuker.scale_start = 0.01f;
            nuker.scale_target = 0.2f;
            ArcIds.Add(nuker.id);

            // The gunship cannon's bullet: speed 800, arcing, a gunshot blast where it lands.
            ProjectileAsset gunship = NewProjectile("GunshipBullet", "ItemTextures/w_bullet");
            gunship.speed = 800f;
            gunship.terraform_option = GunshotId;
            gunship.terraform_range = 1;
            gunship.sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow";
            gunship.sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand";
            gunship.texture_shadow = "shadows/projectiles/shadow_arrow";
            ArcIds.Add(gunship.id);
        }

        private static void AddTerraform(TerraformOptions options)
        {
            if (AssetManager.terraform.get(options.id) == null) AssetManager.terraform.add(options);
            M1NoBlastStun.Register(options.id);
        }

        private static ProjectileAsset NewProjectile(string id, string framePath)
        {
            Sprite[] frames = Resources.LoadAll<Sprite>(framePath);
            if (frames == null || frames.Length == 0)
            {
                Sprite frame = Resources.Load<Sprite>(framePath);
                frames = frame == null ? new Sprite[0] : new[] { frame };
            }
            if (frames.Length == 0 || frames[0] == null)
                throw new InvalidOperationException("Missing projectile frames for " + id + " at " + framePath + ".");
            ProjectileAsset projectile = new ProjectileAsset
            {
                id = id,
                texture = framePath,
                frames = frames,
                animated = frames.Length > 1,
                // Old projectiles changed frame every 0.1 s.
                animation_speed = 10f,
                speed_random = 0f,
                terraform_option = string.Empty,
                end_effect = string.Empty,
                end_effect_scale = 0.25f,
                sound_launch = string.Empty,
                sound_impact = string.Empty,
                trail_effect_id = string.Empty,
                texture_shadow = string.Empty,
                look_at_target = false,
                trigger_on_collision = false,
                can_be_collided = false,
                can_be_blocked = false,
                can_be_left_on_ground = false,
                use_min_angle_height = true,
                mass = 1f,
                size = 0.5f
            };
            AssetManager.projectiles.add(projectile);
            return projectile;
        }
    }

    /// <summary>
    /// Build 719 applies gravity to every ProjectileAsset, so the old straight shots fell
    /// short and the old slow arcs (the silo missile) could not reach their target. M1's
    /// projectiles are flown the way the old engine flew them instead.
    /// </summary>
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.updateVelocity))]
    internal static class M1ProjectileTrajectoryPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Projectile __instance, float pElapsed)
        {
            if (__instance == null || __instance.asset == null || !M1Projectiles.UsesLegacyTrajectory(__instance.asset.id)) return true;
            Update(__instance, pElapsed);
            return false;
        }

        private static void Update(Projectile projectile, float elapsed)
        {
            if (projectile._is_target_reached) return;

            float targetHeight = 0f;
            if (projectile.isMainTargetStillValid())
                targetHeight = Mathf.Max(0f, projectile._main_target.getHeight());

            if (M1Projectiles.UsesLegacyArc(projectile.asset.id))
            {
                // Old Projectile.update: lerp start->target over (distance + targetZ) / speed
                // seconds, lifted by Toolbox.Parabola with a height of 4 x that time.
                Vector2 start = projectile._vector_start;
                Vector2 end = projectile._vector_target;
                float total = Vector2.Distance(start, end);
                float flightTime = (total + targetHeight) / Mathf.Max(0.01f, projectile._speed);
                if (targetHeight > 0f && flightTime < 0.7f) flightTime = 0.7f;
                Vector2 ground = new Vector2(projectile._current_position_3d.x, projectile._current_position_3d.y);
                float step = total / Mathf.Max(0.0001f, flightTime) * Mathf.Max(0f, elapsed);
                bool arrived = Vector2.Distance(ground, end) <= Mathf.Max(0.05f, step);
                Vector2 nextGround = arrived ? end : Vector2.MoveTowards(ground, end, step);
                float progress = total <= 0.0001f ? 1f : Mathf.Clamp01(1f - Vector2.Distance(nextGround, end) / total);
                float arcHeight = flightTime * 4f;
                float z = -4f * arcHeight * progress * progress + 4f * arcHeight * progress + targetHeight * progress;
                Vector3 next = new Vector3(nextGround.x, nextGround.y, z);
                Vector3 moved = next - projectile._current_position_3d;
                projectile._velocity = elapsed > 0f ? moved / elapsed : Vector3.zero;
                projectile._current_position_3d = next;
                if (projectile.asset.look_at_target && moved.sqrMagnitude > 0.000001f)
                {
                    float arcAngle = Mathf.Atan2(moved.y + moved.z, moved.x) * Mathf.Rad2Deg;
                    projectile.rotation = Quaternion.AngleAxis(arcAngle, Vector3.forward);
                }
                // The old engine only hit at the end of an arc.
                if (!arrived) return;
                ResolveHit(projectile, TryHitTargetAtEndpoint(projectile, projectile.checkHitOnNearbyUnits()), true);
                return;
            }

            Vector3 target = new Vector3(projectile._vector_target.x, projectile._vector_target.y, targetHeight);
            Vector3 delta = target - projectile._current_position_3d;
            float distance = delta.magnitude;
            float mass = projectile.asset.mass <= 0f ? 1f : projectile.asset.mass;
            float travel = Mathf.Max(0.01f, projectile._speed / mass) * Mathf.Max(0f, elapsed);
            bool reached = distance <= Mathf.Max(0.05f, travel);

            Vector3 direction = distance <= 0.0001f ? Vector3.zero : delta / distance;
            projectile._velocity = direction * (projectile._speed / mass);
            projectile._current_position_3d = reached ? target : projectile._current_position_3d + direction * travel;

            if (projectile._velocity.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(projectile._velocity.y + projectile._velocity.z, projectile._velocity.x) * Mathf.Rad2Deg;
                projectile.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            if (projectile._collision_timeout != 0f && !reached) return;

            AttackDataResult result = projectile.checkHitOnNearbyUnits();
            if (reached) result = TryHitTargetAtEndpoint(projectile, result);
            ResolveHit(projectile, result, reached);
        }

        private static void ResolveHit(Projectile projectile, AttackDataResult result, bool reached)
        {
            switch (result.state)
            {
                case ApplyAttackState.Hit:
                    projectile.setState(ProjectileState.ToRemove);
                    if (projectile._kill_action != null) projectile._kill_action();
                    projectile.targetReached();
                    return;
                case ApplyAttackState.Miss:
                    EffectsLibrary.spawnAt("fx_miss", projectile._current_position_3d, 0.1f);
                    projectile.setState(projectile.asset.can_be_left_on_ground ? ProjectileState.AlphaAnimation : ProjectileState.ToRemove);
                    projectile.targetReached();
                    return;
                case ApplyAttackState.Block:
                    projectile.getCollided(projectile._vector_target);
                    return;
                case ApplyAttackState.Deflect:
                    projectile.getDeflected(projectile._vector_start, result);
                    return;
                case ApplyAttackState.Continue:
                    if (!reached) return;
                    projectile.setState(ProjectileState.ToRemove);
                    if (projectile._kill_action != null) projectile._kill_action();
                    projectile.targetReached();
                    return;
            }
        }

        // Only corrects the sub-tile gap between build 719's visual target point and its
        // collision point. It never turns a shot at a target that has moved away into a hit.
        private static AttackDataResult TryHitTargetAtEndpoint(Projectile projectile, AttackDataResult originalResult)
        {
            if (originalResult.state == ApplyAttackState.Hit || originalResult.state == ApplyAttackState.Block ||
                originalResult.state == ApplyAttackState.Deflect || projectile.by_who == null || World.world == null ||
                !projectile.isMainTargetStillValid())
                return originalResult;

            BaseSimObject target = projectile._main_target;
            if (target == null || target.current_tile == null || target.stats == null) return originalResult;
            float tolerance = Mathf.Max(0.75f, projectile.asset.size + target.stats["size"] + 0.5f);
            if (Vector2.Distance(projectile._vector_target, target.current_position) > tolerance) return originalResult;

            Vector3 hitPosition = new Vector3(target.current_position.x, target.current_position.y, Mathf.Max(0f, target.getHeight()));
            AttackData attack = new AttackData(projectile.by_who, target.current_tile, hitPosition, projectile.by_who.current_position,
                target, projectile.kingdom, AttackType.Weapon, false, false, true, projectile.asset.id, null, 0f);
            AttackDataResult endpointResult = MapBox.checkAttackFor(attack, target);
            return endpointResult.state == ApplyAttackState.Continue ? originalResult : endpointResult;
        }
    }
}
