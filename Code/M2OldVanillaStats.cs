using System;
using System.Collections.Generic;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// Old-engine combat stats of the game's own traits and statuses, taken from the
    /// decompiled pre-0.51 Assembly-CSharp. 0.51.2 rebalanced most of them into
    /// percentage multipliers (strong: +3 damage became +50% damage), which on M2's
    /// high-stat vehicles is a far bigger effect than the original had.
    /// Columns: health, damage, armor, speed, attack_speed, range, critical_chance,
    /// max_age, mod_health, mod_damage, mod_armor, mod_speed, mod_attack_speed, mod_crit.
    /// </summary>
    internal static class M2OldVanillaStats
    {
        internal const int Health = 0, Damage = 1, Armor = 2, Speed = 3, AttackSpeed = 4, Range = 5, Crit = 6, MaxAge = 7;
        internal const int ModHealth = 8, ModDamage = 9, ModArmor = 10, ModSpeed = 11, ModAttackSpeed = 12, ModCrit = 13;
        internal const int Count = 14;

        // Every trait and status the old engine had. Any of these on an M2 unit gets its
        // old stats instead of its 0.51.2 ones; traits new in 0.51.2 keep their own.
        internal static readonly HashSet<string> TraitIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "acid_blood", "acid_proof", "acid_touch", "agile", "ambitious", "attractive", "blessed", "bloodlust",
            "boat", "bomberman", "bubble_defense", "burning_feet", "cold_aura", "content", "crippled", "cursed",
            "death_bomb", "death_mark", "death_nuke", "deceitful", "dragonslayer", "eagle_eyed", "energized", "evil",
            "eyepatch", "fast", "fat", "fertile", "fire_blood", "fire_proof", "flesh_eater", "flower_prints",
            "freeze_proof", "genius", "giant", "gluttonous", "golden_tooth", "greedy", "healing_aura", "honest",
            "immortal", "immune", "infected", "infertile", "kingslayer", "light_lamp", "long_liver", "lucky",
            "lustful", "madness", "mageslayer", "mega_heartbeat", "miner", "miracle_born", "moonchild", "mush_spores",
            "nightchild", "pacifist", "paranoid", "peaceful", "plague", "poison_immune", "poisonous", "pyromaniac",
            "rat", "ratKing", "regeneration", "savage", "scar_of_divinity", "shiny", "short_sighted", "skin_burns",
            "slow", "strong", "strong_minded", "stupid", "super_health", "thorns", "tiny", "tough",
            "tumor_infection", "ugly", "unlucky", "venomous", "veteran", "voices_in_my_head", "weak", "weightless",
            "whirlwind", "wise", "zombie",
        };

        internal static readonly HashSet<string> StatusIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "ash_fever", "burning", "caffeinated", "cough", "cursed", "enchanted", "frozen", "invincible",
            "poisoned", "powerup", "rage", "shield", "slowness",
        };

        internal static readonly Dictionary<string, float[]> Traits = new Dictionary<string, float[]>(StringComparer.Ordinal)
        {
            { "agile", new[] { 0f, 0f, 0f, 0f, 30f, 0f, 0f, 3f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "ambitious", new[] { 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "attractive", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "blessed", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 5f, 0.5f, 0.5f, 0.1f, 0.5f, 0f, 0.1f } },
            { "crippled", new[] { 0f, 0f, 0f, -15f, -15f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "cursed", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, -10f, -0.5f, -0.5f, -0.5f, -0.2f, -0.5f, 0f } },
            { "dragonslayer", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.04f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "eagle_eyed", new[] { 0f, 0f, 0f, 0f, 0f, 3f, 0.15f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "energized", new[] { 0f, 0f, 0f, 10f, 0f, 0f, 0f, 7f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "eyepatch", new[] { 0f, 0f, 0f, 0f, -15f, 0f, -0.15f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "fast", new[] { 0f, 0f, 0f, 10f, 5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "fat", new[] { 0f, 0f, 0f, 0f, -10f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "fertile", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 2f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "giant", new[] { 0f, 0f, 0f, 0f, -5f, 0f, 0f, 0f, 0.5f, 0f, 0f, 0f, 0f, 0f } },
            { "immune", new[] { 0f, 0f, 3f, 0f, 0f, 0f, 0f, 10f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "infected", new[] { 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "kingslayer", new[] { 0f, 0f, 0f, 0f, 5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "long_liver", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "lucky", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.3f, 7f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "madness", new[] { 0f, 1f, 0f, 5f, 10f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "mageslayer", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.03f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "miracle_born", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 20f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "moonchild", new[] { 0f, 10f, 10f, 5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "mush_spores", new[] { 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "nightchild", new[] { 0f, 5f, 0f, 5f, 3f, 0f, 0.03f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "plague", new[] { 0f, -3f, -1f, -10f, 0f, 0f, 0f, -30f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "regeneration", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 12f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "scar_of_divinity", new[] { 69f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "short_sighted", new[] { 0f, 0f, 0f, 0f, 0f, -3f, -0.05f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "skin_burns", new[] { 0f, 0f, 0f, -5f, -5f, 0f, 0f, -5f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "slow", new[] { 0f, 0f, 0f, -10f, -5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "strong", new[] { 0f, 3f, 0f, 0f, 0f, 0f, 0f, 3f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "super_health", new[] { 9999f, 0f, 0f, 0f, 0f, 0f, 0f, 100f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "tiny", new[] { 0f, 0f, 0f, 5f, 10f, 0f, 0f, 0f, -0.5f, 0f, 0f, 0f, 0f, 0f } },
            { "tough", new[] { 0f, 0f, 10f, 0f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "tumor_infection", new[] { 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "unlucky", new[] { 0f, 0f, 0f, 0f, 0f, 0f, -0.3f, -13f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "veteran", new[] { 30f, 3f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "weak", new[] { 0f, -1f, 0f, 0f, 0f, 0f, 0f, -6f, 0f, 0f, 0f, 0f, 0f, 0f } },
        };

        internal static readonly Dictionary<string, float[]> Statuses = new Dictionary<string, float[]>(StringComparer.Ordinal)
        {
            { "ash_fever", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, -45f, -0.6f, 0f, 0f, -0.1f, 0f, 0f } },
            { "caffeinated", new[] { 0f, 0f, 0f, 200f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 5.69f, 0f } },
            { "cough", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, -15f, -0.1f, 0f, 0f, -0.1f, -0.5f, 0f } },
            { "cursed", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, -10f, -0.5f, -0.5f, -0.5f, -0.2f, -0.5f, 0f } },
            { "enchanted", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 10f, 0f, 0.77f, 0.1f, 0.1f, 0f, 0.1f } },
            { "frozen", new[] { 0f, 0f, -20f, -10000f, -10000f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "powerup", new[] { 0f, 5f, 5f, 0f, 5f, 0f, 0f, 0f, 0f, 0.5f, 0.5f, 0f, 0.5f, 0.5f } },
            { "rage", new[] { 0f, 0f, 0f, 0f, 20f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f } },
            { "shield", new[] { 0f, 0f, 90f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "slowness", new[] { 0f, 0f, 0f, -100f, -50f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
        };

        // Trait ids the game itself registers, captured before M2 adds its own.
        private static readonly HashSet<string> GameTraits = new HashSet<string>(StringComparer.Ordinal);

        internal static void SnapshotGameTraits()
        {
            GameTraits.Clear();
            foreach (ActorTrait trait in AssetManager.traits.list) GameTraits.Add(trait.id);
        }

        internal static bool IsOldGameTrait(ActorTrait trait)
        {
            return GameTraits.Contains(trait.id) && TraitIds.Contains(trait.id);
        }
    }
}
