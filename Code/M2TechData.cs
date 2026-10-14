using System;
using System.Collections.Generic;

namespace ModernBoxM2Rewrite
{
    // What an M2 tech gives the culture that researched it.
    internal enum M2TechBonus
    {
        None, Mining, Axes, Trading, Storage, Armorsmith, Weaponsmith, Heroes, BornLevel, Housing, Towers, Army, Damage, Armor
    }

    internal sealed class M2TechDef
    {
        internal string Id;
        internal int RequiredLevel;
        internal float KnowledgeCost = 1f;
        internal bool Rare;
        internal bool Priority;
        internal float KnowledgeGain;
        internal string[] Requirements = Array.Empty<string>();
        internal M2TechBonus Bonus;
        internal string Icon;
        internal string Name;
        internal bool IsM2;
    }

    // The old game's culture tech list (CultureTechLibrary) with ModernBox 2.1.0.1's
    // changes from Tech.cs: most vanilla techs cost -2, plus the M2 era techs.
    // 0.51.2 removed culture techs, so the vanilla ones are kept to give the
    // original research pace; their old effects are covered by 0.51.2's own systems.
    internal static class M2TechData
    {
        internal const int LevelOffset = 10; // Culture_GetCurrentLevel_Patch
        internal static readonly List<M2TechDef> All = new List<M2TechDef>();
        internal static readonly Dictionary<string, M2TechDef> ById = new Dictionary<string, M2TechDef>(StringComparer.Ordinal);

        // RaceLibrary values for the four civ races.
        internal static float KnowledgePerIntelligence(string race)
        {
            switch (race)
            {
                case "human": return 2.5f;
                case "orc": return 0.9f;
                case "elf": return 1.5f;
                case "dwarf": return 1.7f;
            }
            return 1f;
        }

        internal static int RareTechLimit(string race)
        {
            switch (race)
            {
                case "human": return 6;
                case "orc": return 3;
            }
            return 5;
        }

        internal static bool IsForbidden(string race, string techId)
        {
            return techId == "building_roads" && (race == "orc" || race == "elf");
        }

        static M2TechData()
        {
            Vanilla("culture_convert_chance_1", 8, -2f, false, false, 0f);
            Vanilla("culture_convert_chance_2", 10, -2f, false, false, 0f, "culture_convert_chance_1");
            Vanilla("culture_convert_chance_3", 20, -2f, false, false, 0f, "culture_convert_chance_2");
            Vanilla("culture_spread_speed_1", 15, -2f, false, false, 0f);
            Vanilla("culture_spread_speed_2", 18, -2f, false, false, 0f, "culture_spread_speed_1");
            Vanilla("culture_spread_speed_3", 20, -2f, false, false, 0f, "culture_spread_speed_2");
            Vanilla("nature_lovers", 20, 1f, true, false, 0f);
            Vanilla("ancestors_knowledge", 20, -2f, true, false, 0f);
            Vanilla("way_of_live", 20, -2f, true, false, 0f);
            Vanilla("heroes", 20, -2f, true, false, 0f);
            Vanilla("steal_resources", 20, 1f, true, false, 0f);
            Vanilla("steal_kids", 20, 1f, true, false, 0f);
            Vanilla("gems_water", 20, 1f, true, false, 0f);
            Vanilla("zones_1", 4, -2f, false, false, 0f, "house_tier_2");
            Vanilla("zones_2", 9, -2f, false, false, 0f, "building_statues");
            Vanilla("zones_3", 15, -2f, true, false, 0f, "building_watch_tower");
            Vanilla("housing_1", 0, -2f, false, false, 0f, "house_tier_2");
            Vanilla("housing_2", 10, -2f, false, false, 0f, "house_tier_4", "housing_1");
            Vanilla("housing_3", 15, -2f, false, false, 0f, "house_tier_5", "housing_2");
            Vanilla("governance_1", 10, -2f, false, false, 0f);
            Vanilla("governance_2", 0, -2f, false, false, 0f, "governance_1");
            Vanilla("governance_3", 20, -2f, true, false, 0f, "governance_2");
            Vanilla("knowledge_gain_1", 5, -2f, false, false, 0.5f);
            Vanilla("knowledge_gain_2", 10, -2f, false, false, 0.5f, "knowledge_gain_1");
            Vanilla("knowledge_gain_3", 15, -2f, false, false, 0.5f, "knowledge_gain_2");
            Vanilla("army_training_1", 5, -2f, false, false, 0f, "building_barracks");
            Vanilla("army_training_2", 5, -2f, false, false, 0f, "army_training_1");
            Vanilla("army_training_3", 5, -2f, false, false, 0f, "army_training_2");
            Vanilla("military_strategy", 20, -2f, true, false, 0f, "weapon_production", "armor_production");
            Vanilla("defense_strategy", 20, -2f, true, false, 0f, "armor_production");
            Vanilla("house_tier_0", 0, -2f, false, true, 0f);
            Vanilla("house_tier_1", 3, -2f, false, true, 0f, "house_tier_0");
            Vanilla("house_tier_2", 8, -2f, false, true, 0f, "house_tier_1");
            Vanilla("house_tier_3", 12, -2f, false, true, 0f, "house_tier_2");
            Vanilla("house_tier_4", 15, -2f, false, false, 0f, "house_tier_3");
            Vanilla("house_tier_5", 20, -2f, false, false, 0f, "house_tier_4");
            Vanilla("building_docks", 0, -2f, false, true, 0f, "house_tier_2");
            Vanilla("building_roads", 5, -2f, false, true, 0f);
            Vanilla("building_well", 12, -2f, false, false, 0f, "house_tier_2");
            Vanilla("building_watch_tower", 5, -2f, false, false, 0f, "house_tier_3", "weapon_bow");
            Vanilla("building_watch_tower_bonus", 20, -2f, true, false, 0f, "building_watch_tower");
            Vanilla("building_statues", 5, -2f, false, false, 0f, "house_tier_3");
            Vanilla("building_mine", 5, -2f, false, false, 0f, "house_tier_1");
            Vanilla("building_barracks", 5, -2f, false, false, 0f, "house_tier_2");
            Vanilla("building_windmill", 0, -2f, false, true, 0f, "house_tier_1");
            Vanilla("building_temple", 10, -2f, false, false, 0f, "house_tier_3");
            Vanilla("mining_efficiency", 3, -2f, false, false, 0f);
            Vanilla("sharp_axes", 2, -2f, false, false, 0f);
            Vanilla("weaponsmith", 20, -2f, true, false, 0f, "weapon_production");
            Vanilla("armorsmith", 20, -2f, true, false, 0f, "armor_production");
            Vanilla("trading", 7, -2f, false, false, 0f);
            Vanilla("trading_efficiency", 8, -2f, false, false, 0f, "trading");
            Vanilla("boats_trading", 4, -2f, false, false, 0f, "building_docks");
            Vanilla("boats_transport", 8, -2f, false, true, 0f, "building_docks");
            Vanilla("equipment_storage_1", 7, -2f, false, false, 0f, "weapon_production", "armor_production");
            Vanilla("equipment_storage_2", 0, -2f, false, false, 0f, "equipment_storage_1");
            Vanilla("equipment_storage_3", 0, -2f, false, false, 0f, "equipment_storage_2");
            Vanilla("weapon_sword", 3, -2f, false, true, 0f, "weapon_production");
            Vanilla("weapon_axe", 3, -2f, false, false, 0f, "weapon_production");
            Vanilla("weapon_hammer", 3, -2f, false, false, 0f, "weapon_production");
            Vanilla("weapon_spear", 3, -2f, false, false, 0f, "weapon_production");
            Vanilla("armor_production", 3, -2f, false, false, 0f, "weapon_production");
            Vanilla("weapon_production", 0, -2f, false, true, 0f);
            Vanilla("weapon_bow", 3, -2f, false, false, 0f, "weapon_production");
            Vanilla("material_copper", 4, -2f, false, false, 0f);
            Vanilla("material_bronze", 6, -2f, false, true, 0f, "material_copper");
            Vanilla("material_silver", 10, -2f, false, true, 0f, "material_bronze");
            Vanilla("material_iron", 20, -2f, false, true, 0f, "material_silver");
            Vanilla("material_steel", 30, -2f, false, false, 0f, "material_iron");
            Vanilla("material_mythril", 40, -2f, false, false, 0f, "material_steel");
            Vanilla("material_adamantine", 50, -2f, false, false, 0f, "material_mythril");

            Tier("Renaissance", 55, "Renaissance", "Renaissance era", "house_tier_5", null);
            M2("Casino", 60, "Casino", "Casino", M2TechBonus.None, "Renaissance");
            Tier("Industrial", 65, "Industrial", "Industrial era", "Renaissance_defense", "Firearms");
            M2("Cyberware", 65, "Cyberware", "Cyberware", M2TechBonus.None, "Industrial_heroes");
            M2("Nukes", 65, "Nuke", "Nukes", M2TechBonus.None, "Cyberware");
            Tier("Modern", 80, "Skyscraper", "Skyscraper (Modern era)", "Industrial_defense", "MilitaryModern");
            Tier("Future", 90, "Future", "Future era", "Modern_defense", null);
        }

        private static void Vanilla(string id, int level, float cost, bool rare, bool priority, float knowledgeGain, params string[] requirements)
        {
            Add(new M2TechDef
            {
                Id = id, RequiredLevel = level, KnowledgeCost = cost, Rare = rare, Priority = priority,
                KnowledgeGain = knowledgeGain, Requirements = requirements, Name = Pretty(id)
            });
        }

        private static M2TechDef M2(string id, int level, string icon, string name, M2TechBonus bonus, string requirement)
        {
            M2TechDef def = new M2TechDef
            {
                Id = id, RequiredLevel = level, Bonus = bonus, Icon = icon, Name = name, IsM2 = true,
                Requirements = requirement == null ? Array.Empty<string>() : new[] { requirement }
            };
            Add(def);
            return def;
        }

        // Every era got the same chain of techs in Tech.cs. Industrial has Firearms and
        // Modern has MilitaryModern (+5000 army) right after the era tech.
        private static void Tier(string era, int level, string icon, string name, string requirement, string second)
        {
            string head = era == "Modern" ? "Skyscraper" : era;
            string previous = M2(head, level, icon, name, M2TechBonus.None, requirement).Id;
            if (second == "Firearms")
                previous = M2("Firearms", level, "firearm", "Firearms", M2TechBonus.None, previous).Id;
            else if (second == "MilitaryModern")
                previous = M2("MilitaryModern", level, "force", "Modern military", M2TechBonus.Army, previous).Id;
            string p = era + "_";
            M2TechDef gain = Step(p + "knowledge_gain", level, "icon_tech_knowledge_gain3", "knowledge gain", M2TechBonus.None, previous, era);
            gain.KnowledgeGain = -0.3f;
            previous = gain.Id;
            previous = Step(p + "mining", level, "icon_tech_mining_efficiency", "mining", M2TechBonus.Mining, previous, era).Id;
            previous = Step(p + "axes", level, "icon_tech_sharp_axes", "axes", M2TechBonus.Axes, previous, era).Id;
            previous = Step(p + "trading", level, "icon_tech_trading-_efficiency", "trading", M2TechBonus.Trading, previous, era).Id;
            previous = Step(p + "storage", level, "icon_tech_city_storage_3", "storage", M2TechBonus.Storage, previous, era).Id;
            previous = Step(p + "armorsmith", level, "icon_tech_armorsmith", "armorsmith", M2TechBonus.Armorsmith, previous, era).Id;
            previous = Step(p + "weaponsmith", level, "icon_tech_weaponsmith", "weaponsmith", M2TechBonus.Weaponsmith, previous, era).Id;
            previous = Step(p + "heroes", level, "icon_tech_heroes", "heroes", M2TechBonus.Heroes, previous, era).Id;
            previous = Step(p + "knowledge", level, "icon_tech_ancestors_knowledge", "knowledge", M2TechBonus.BornLevel, previous, era).Id;
            previous = Step(p + "housing", level, "icon_tech_housing_3", "housing", M2TechBonus.Housing, previous, era).Id;
            previous = Step(p + "defenses", level, "icon_tech_watch_tower_bonus", "defenses", M2TechBonus.Towers, previous, era).Id;
            previous = Step(p + "army", level, "icon_tech_army_training_2", "army", M2TechBonus.Army, previous, era).Id;
            previous = Step(p + "strategy", level, "icon_tech_military_strategy", "strategy", M2TechBonus.Damage, previous, era).Id;
            Step(p + "defense", level, "icon_tech_defense_strategy", "defense", M2TechBonus.Armor, previous, era);
        }

        private static M2TechDef Step(string id, int level, string icon, string name, M2TechBonus bonus, string requirement, string era)
        {
            return M2(id, level, icon, era + " " + name, bonus, requirement);
        }

        private static void Add(M2TechDef def)
        {
            if (ById.ContainsKey(def.Id)) return;
            All.Add(def);
            ById[def.Id] = def;
        }

        private static string Pretty(string id)
        {
            string text = id.Replace('_', ' ');
            return char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        // MilitaryModern's +5000 replaces the usual +0.1 army step.
        internal static float ArmyValue(M2TechDef def)
        {
            return def.Id == "MilitaryModern" ? 5000f : 0.1f;
        }
    }
}
