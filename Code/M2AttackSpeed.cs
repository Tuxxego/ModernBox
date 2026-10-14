using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// M2's attack_speed numbers were written for the old engine, where the stat is
    /// clamped to 1..300 and the cooldown is (300 - s) / (100 + s) seconds. 0.51.2
    /// clamps it to 0.5..10 and uses 1 / s. Every M2 value goes through here so units,
    /// guns and traits attack as often as they did in the original.
    /// </summary>
    internal static class M2AttackSpeed
    {
        // Old-engine base attack_speed of civ units (_unit) and of the vanilla zombie.
        internal const float OldHumanBase = 60f;
        internal const float OldZombieBase = 110f;

        // Old attack_speed of M2's unit-only attacks, folded into the unit stat instead.
        private static readonly Dictionary<string, float> LegacyAttacks = new Dictionary<string, float>(StringComparer.Ordinal);

        internal static float OldCooldown(float oldValue)
        {
            float s = Mathf.Clamp(oldValue, 1f, 300f);
            return (300f - s) / (100f + s);
        }

        internal static float ToNew(float oldValue)
        {
            float cooldown = OldCooldown(oldValue);
            return cooldown <= 0.1f ? 10f : 1f / cooldown;
        }

        // What an item or trait adds on top of a unit with the given old base.
        internal static float Delta(float oldValue, float oldBase)
        {
            return ToNew(oldBase + oldValue) - ToNew(oldBase);
        }

        internal static void RecordLegacyAttack(string attackId, float oldValue)
        {
            LegacyAttacks[attackId] = oldValue;
        }

        // The 0.51.2 attack_speed for a generated unit whose original value was unitOld.
        internal static float ForUnit(float unitOld, string attackId)
        {
            float attackOld;
            if (attackId != null && LegacyAttacks.TryGetValue(attackId, out attackOld))
                return ToNew(unitOld + attackOld);
            EquipmentSpec weapon = attackId == null ? null : ContentRegistry.Equipment.Find(spec => spec.Id == attackId);
            if (weapon != null && weapon.BaseStats.TryGetValue("attack_speed", out attackOld))
                return ToNew(unitOld + attackOld) - Delta(attackOld, OldHumanBase);
            return ToNew(unitOld);
        }

        // A unit's whole old-scale attack_speed (its own value plus its attack's).
        internal static float OldTotal(float unitOld, string attackId)
        {
            float attackOld;
            if (attackId != null && LegacyAttacks.TryGetValue(attackId, out attackOld)) return unitOld + attackOld;
            EquipmentSpec weapon = attackId == null ? null : ContentRegistry.Equipment.Find(spec => spec.Id == attackId);
            if (weapon != null && weapon.BaseStats.TryGetValue("attack_speed", out attackOld)) return unitOld + attackOld;
            return unitOld;
        }

        // Old crit = damage * critical_damage_multiplier (M2 used 0.4-0.8, i.e. weaker than
        // a normal hit). 0.51.2 adds item values to a base of 2 and truncates to an int, so
        // the closest it can get is x1. Returns the item value that lands on that total.
        internal static float CritMultiplier(float oldValue)
        {
            return Mathf.Max(1, Mathf.RoundToInt(oldValue)) - 2f;
        }

        // The old engine's slowest attack was every 2.96 s, 0.51.2 stops at 2 s.
        internal static void AllowOriginalSlowest()
        {
            BaseStatAsset stat = AssetManager.base_stats_library.get("attack_speed");
            float slowest = ToNew(1f);
            if (stat != null && stat.normalize_min > slowest) stat.normalize_min = slowest;
        }
    }
}
