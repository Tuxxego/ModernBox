using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ModernBoxM1Rewrite
{
    internal static class M1OriginalStats
    {
        // Old engine: tiles/s = speed * 0.1. 0.51.2: speed * 0.4 * unit_speed_multiplier (0.5) = speed * 0.2.
        // Every M1 speed value and speed bonus is halved so units move as fast as in the original.
        internal const float SpeedScale = 0.5f;

        // knockback_reduction of the M1 items (0.51.2 dropped the stat).
        internal static readonly Dictionary<string, float> ItemKnockbackReduction = new Dictionary<string, float>(StringComparer.Ordinal);

        // The old engine pushed a unit by force * (1 - knockback_reduction), never below 0.
        internal static float ForceMultiplier(Actor actor)
        {
            if (actor == null || actor.asset == null) return 1f;
            ModernUnitSpec spec = ContentRegistry.FindUnit(actor.asset.id);
            float reduction = spec == null ? 0f : spec.KnockbackReduction;
            if (actor.equipment != null)
            {
                foreach (ActorEquipmentSlot slot in actor.equipment)
                {
                    Item item = slot == null ? null : slot.getItem();
                    float value;
                    if (item != null && item.asset != null && ItemKnockbackReduction.TryGetValue(item.asset.id, out value))
                        reduction += value;
                }
            }
            return Math.Max(0f, 1f - reduction);
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.calculateForce))]
    internal static class M1KnockbackReductionPatch
    {
        private static bool Prefix(Actor __instance, ref float pForceAmountDirection, ref float pForceHeight)
        {
            float multiplier = M1OriginalStats.ForceMultiplier(__instance);
            if (multiplier >= 1f) return true;
            if (multiplier <= 0f) return false;
            pForceAmountDirection *= multiplier;
            pForceHeight *= multiplier;
            return true;
        }
    }

    // M1 unit stat fixes after each recalculation:
    // - 0.51.2 takes base stats from the subspecies and ignores the asset's own values. The
    //   Soldier gets its city's subspecies, so swap the subspecies base back to the Soldier's.
    // - Level bonuses, traits and statuses follow the old engine.
    [HarmonyPatch(typeof(Actor), nameof(Actor.updateStats))]
    internal static class M1UnitStatsPatch
    {
        private static readonly string[] Keys =
        {
            "health", "damage", "speed", "armor", "attack_speed", "range", "lifespan",
            "targets", "knockback", "projectiles", "accuracy"
        };

        // updateStats returns early when the stats are not dirty; only correct a fresh recalculation.
        private static void Prefix(Actor __instance, out bool __state)
        {
            __state = __instance != null && __instance.isStatsDirty();
        }

        private static void Postfix(Actor __instance, bool __state)
        {
            if (!__state || __instance == null || __instance.asset == null || __instance.data == null || !__instance.isAlive()) return;
            ModernUnitSpec spec = ContentRegistry.FindUnit(__instance.asset.id);
            if (spec == null) return;
            BaseStats stats = __instance.stats;
            Subspecies subspecies = __instance.subspecies;
            if (spec.Humanoid && subspecies != null)
            {
                BaseStats sexed = __instance.isSexMale() ? subspecies.base_stats_male : subspecies.base_stats_female;
                foreach (string key in Keys)
                {
                    float fromSubspecies = subspecies.base_stats[key] + (sexed == null ? 0f : sexed[key]);
                    float delta = __instance.asset.base_stats[key] - fromSubspecies;
                    if (delta != 0f) stats[key] += delta;
                }
            }

            // 0.51.2 rebalanced the game's own traits and statuses (mostly into percentage
            // multipliers), gives clans, languages and cultures combat stats, adds warfare / 5
            // to damage and gives level * 5% health. None of that is what the old engine did.
            // Swap all of it for the old engine's numbers, including its per-level bonuses
            // (+20 health, +1/2 damage, +1/3 armor, +1 old-scale attack_speed).
            int level = __instance.data.level;
            float newLevel = 1f + level * SimGlobals.m.level_mod_bonus_health;
            if (level > 0 && newLevel > 0f) stats["health"] /= newLevel;
            stats["damage"] -= stats["warfare"] / 5f;

            float[] newAdd = new float[M1OldVanillaStats.Count];
            float[] newMult = new float[M1OldVanillaStats.Count];
            float[] oldStats = new float[M1OldVanillaStats.Count];
            float newAccuracy = 0f;
            CollectVanilla(__instance, newAdd, newMult, oldStats, ref newAccuracy);
            int bonusLevels = Math.Max(0, level - 1);

            Swap(stats, "health", "multiplier_health", M1OldVanillaStats.Health, M1OldVanillaStats.ModHealth, newAdd, newMult, oldStats, bonusLevels * 20);
            Swap(stats, "damage", "multiplier_damage", M1OldVanillaStats.Damage, M1OldVanillaStats.ModDamage, newAdd, newMult, oldStats, bonusLevels / 2);
            Swap(stats, "armor", null, M1OldVanillaStats.Armor, M1OldVanillaStats.ModArmor, newAdd, newMult, oldStats, bonusLevels / 3);
            Swap(stats, "speed", "multiplier_speed", M1OldVanillaStats.Speed, M1OldVanillaStats.ModSpeed, newAdd, newMult, oldStats, 0f, M1OriginalStats.SpeedScale);
            Swap(stats, "critical_chance", "multiplier_crit", M1OldVanillaStats.Crit, M1OldVanillaStats.ModCrit, newAdd, newMult, oldStats, 0f);
            Swap(stats, "lifespan", "multiplier_lifespan", M1OldVanillaStats.MaxAge, -1, newAdd, newMult, oldStats, 0f);
            // Both engines scale a ranged unit's range by the world era afterwards.
            float era = __instance.hasRangeAttack() && World.world_era != null ? World.world_era.range_weapons_multiplier : 0f;
            if (1f + era > 0.01f)
                stats["range"] = (stats["range"] / (1f + era) - newAdd[M1OldVanillaStats.Range] + oldStats[M1OldVanillaStats.Range]) * (1f + era);
            // The old engine ignored accuracy.
            stats["accuracy"] -= newAccuracy;

            // attack_speed: strip the 0.51.2 bonuses, then add what the old bonuses and levels
            // did to the unit's old-scale value (old: (value + level - 1) * (1 + mods)).
            float attackMultiplier = stats["multiplier_attack_speed"];
            if (1f + attackMultiplier > 0.01f)
            {
                float m1Only = (stats["attack_speed"] / (1f + attackMultiplier) - newAdd[M1OldVanillaStats.AttackSpeed]) *
                               (1f + attackMultiplier - newMult[M1OldVanillaStats.AttackSpeed]);
                float oldBase = M1AttackSpeed.OldTotal(spec.AttackSpeed, spec.Attack);
                float oldTotal = (oldBase + oldStats[M1OldVanillaStats.AttackSpeed] + bonusLevels) *
                                 (1f + oldStats[M1OldVanillaStats.ModAttackSpeed]);
                stats["attack_speed"] = m1Only + M1AttackSpeed.ToNew(oldTotal) - M1AttackSpeed.ToNew(oldBase);
            }
            stats.normalize();
            if (__instance.getHealth() > __instance.getMaxHealth()) __instance.setMaxHealth();
        }

        // Value before multipliers: remove the 0.51.2 sources, add the old ones (old flat speed
        // is scaled like every other M1 speed), then reapply the multipliers.
        private static void Swap(BaseStats stats, string key, string multiplierKey, int index, int modIndex,
            float[] newAdd, float[] newMult, float[] oldStats, float oldExtra, float oldScale = 1f)
        {
            float multiplier = multiplierKey == null ? 0f : stats[multiplierKey];
            if (1f + multiplier <= 0.01f) return;
            float baseValue = stats[key] / (1f + multiplier) - newAdd[index] + oldStats[index] * oldScale + oldExtra;
            float oldMod = modIndex < 0 ? 0f : oldStats[modIndex];
            stats[key] = baseValue * (1f + multiplier - newMult[index] + oldMod);
        }

        // Same column order as M1OldVanillaStats (health .. max_age).
        private static readonly string[] NewKeys =
        {
            "health", "damage", "armor", "speed", "attack_speed", "range", "critical_chance", "lifespan"
        };
        private static readonly string[] NewMultiplierKeys =
        {
            "multiplier_health", "multiplier_damage", null, "multiplier_speed", "multiplier_attack_speed", null, "multiplier_crit", "multiplier_lifespan"
        };

        private static void AddNew(BaseStats source, float[] newAdd, float[] newMult, ref float accuracy)
        {
            if (source == null) return;
            for (int i = 0; i < NewKeys.Length; i++)
            {
                newAdd[i] += source[NewKeys[i]];
                if (NewMultiplierKeys[i] != null) newMult[i] += source[NewMultiplierKeys[i]];
            }
            accuracy += source["accuracy"];
        }

        private static void AddOld(float[] values, float[] oldStats)
        {
            for (int i = 0; i < values.Length && i < oldStats.Length; i++) oldStats[i] += values[i];
        }

        private static void CollectVanilla(Actor actor, float[] newAdd, float[] newMult, float[] oldStats, ref float accuracy)
        {
            float[] values;
            foreach (ActorTrait trait in actor.getTraits())
            {
                if (trait.only_active_on_era_flag && ((trait.era_active_moon && !World.world_era.flag_moon) ||
                    (trait.era_active_night && !World.world_era.overlay_darkness))) continue;
                if (!M1OldVanillaStats.IsOldGameTrait(trait)) continue;
                AddNew(trait.base_stats, newAdd, newMult, ref accuracy);
                if (M1OldVanillaStats.Traits.TryGetValue(trait.id, out values)) AddOld(values, oldStats);
            }
            if (actor.hasAnyStatusEffect())
                foreach (Status status in actor.getStatuses())
                {
                    if (status.asset == null || !M1OldVanillaStats.StatusIds.Contains(status.asset.id)) continue;
                    AddNew(status.asset.base_stats, newAdd, newMult, ref accuracy);
                    if (M1OldVanillaStats.Statuses.TryGetValue(status.asset.id, out values)) AddOld(values, oldStats);
                }
            // Clans, languages and cultures gave no combat stats in the old engine.
            if (actor.hasClan())
            {
                AddNew(actor.clan.base_stats, newAdd, newMult, ref accuracy);
                AddNew(actor.isSexMale() ? actor.clan.base_stats_male : actor.clan.base_stats_female, newAdd, newMult, ref accuracy);
            }
            if (actor.hasLanguage()) AddNew(actor.language.base_stats, newAdd, newMult, ref accuracy);
            if (actor.hasCulture()) AddNew(actor.culture.base_stats, newAdd, newMult, ref accuracy);
        }
    }

    // Old Actor.updateAge ran once a year and killed a unit past max_age with a 13% roll
    // (a max_age of 0 meant it never died of age). 0.51.2's checkNaturalDeath runs many
    // times a second, so M1 units get the old yearly roll instead.
    [HarmonyPatch(typeof(Actor), nameof(Actor.checkNaturalDeath))]
    internal static class M1OldAgePatch
    {
        private const string LastRollKey = "modernbox_m1_age_roll";

        private static bool Prefix(Actor __instance, ref bool __result)
        {
            if (__instance == null || __instance.asset == null || __instance.data == null ||
                ContentRegistry.FindUnit(__instance.asset.id) == null) return true;
            __result = false;
            if (!WorldLawLibrary.world_law_old_age.isEnabled() || __instance.hasTrait("immortal")) return false;
            float lifespan = __instance.stats["lifespan"];
            int age = __instance.getAge();
            if (lifespan == 0f || age < lifespan) return false;
            float lastRoll;
            __instance.data.get(LastRollKey, out lastRoll, -1f);
            if (lastRoll >= age) return false;
            __instance.data.set(LastRollKey, (float)age);
            if (!Randy.randomChance(0.13f)) return false;
            __instance.getHitFullHealth(AttackType.Age);
            __result = true;
            return false;
        }
    }
}
