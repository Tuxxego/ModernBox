using System;
using System.Collections.Generic;

namespace ModernBoxM1Rewrite
{
    internal sealed class M1TechDef
    {
        internal string Id;
        internal int RequiredLevel;
        internal float KnowledgeCost = 1f;
        internal bool Rare;
        internal bool Priority;
        internal float KnowledgeGain;
        internal string[] Requirements = Array.Empty<string>();
        internal string Name;
        internal bool IsM1;
    }

    // The old game's culture tech list (CultureTechLibrary), unchanged by M1, plus the
    // 18 techs from M1's Tech.cs. 0.51.2 removed culture techs, so the vanilla ones are
    // kept only so cultures reach M1's tech levels at the original pace.
    internal static class M1TechData
    {
        internal static readonly List<M1TechDef> All = new List<M1TechDef>();
        internal static readonly Dictionary<string, M1TechDef> ById = new Dictionary<string, M1TechDef>(StringComparer.Ordinal);

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

        static M1TechData()
        {
            Vanilla("culture_convert_chance_1", 8, 1f, false, false, 0f);
            Vanilla("culture_convert_chance_2", 10, 1f, false, false, 0f, "culture_convert_chance_1");
            Vanilla("culture_convert_chance_3", 20, 1f, false, false, 0f, "culture_convert_chance_2");
            Vanilla("culture_spread_speed_1", 15, 1f, false, false, 0f);
            Vanilla("culture_spread_speed_2", 18, 1f, false, false, 0f, "culture_spread_speed_1");
            Vanilla("culture_spread_speed_3", 20, 1f, false, false, 0f, "culture_spread_speed_2");
            Vanilla("nature_lovers", 20, 1f, true, false, 0f);
            Vanilla("ancestors_knowledge", 20, 1f, true, false, 0f);
            Vanilla("way_of_live", 20, 1f, true, false, 0f);
            Vanilla("heroes", 20, 1f, true, false, 0f);
            Vanilla("steal_resources", 20, 1f, true, false, 0f);
            Vanilla("steal_kids", 20, 1f, true, false, 0f);
            Vanilla("gems_water", 20, 1f, true, false, 0f);
            Vanilla("zones_1", 4, 1f, false, false, 0f, "house_tier_2");
            Vanilla("zones_2", 9, 1f, false, false, 0f, "building_statues");
            Vanilla("zones_3", 15, 1f, true, false, 0f, "building_watch_tower");
            Vanilla("housing_1", 0, 1f, false, false, 0f, "house_tier_2");
            Vanilla("housing_2", 10, 1f, false, false, 0f, "house_tier_4", "housing_1");
            Vanilla("housing_3", 15, 1f, false, false, 0f, "house_tier_5", "housing_2");
            Vanilla("governance_1", 10, 1f, false, false, 0f);
            Vanilla("governance_2", 0, 1f, false, false, 0f, "governance_1");
            Vanilla("governance_3", 20, 1f, true, false, 0f, "governance_2");
            Vanilla("knowledge_gain_1", 5, 1f, false, false, 0.5f);
            Vanilla("knowledge_gain_2", 10, 1f, false, false, 0.5f, "knowledge_gain_1");
            Vanilla("knowledge_gain_3", 15, 1f, false, false, 0.5f, "knowledge_gain_2");
            Vanilla("army_training_1", 5, 1f, false, false, 0f, "building_barracks");
            Vanilla("army_training_2", 5, 1f, false, false, 0f, "army_training_1");
            Vanilla("army_training_3", 5, 1f, false, false, 0f, "army_training_2");
            Vanilla("military_strategy", 20, 1f, true, false, 0f, "weapon_production", "armor_production");
            Vanilla("defense_strategy", 20, 1f, true, false, 0f, "armor_production");
            Vanilla("house_tier_0", 0, 0.3f, false, true, 0f);
            Vanilla("house_tier_1", 3, 0.4f, false, true, 0f, "house_tier_0");
            Vanilla("house_tier_2", 8, 1f, false, true, 0f, "house_tier_1");
            Vanilla("house_tier_3", 12, 1f, false, true, 0f, "house_tier_2");
            Vanilla("house_tier_4", 15, 1f, false, false, 0f, "house_tier_3");
            Vanilla("house_tier_5", 20, 1f, false, false, 0f, "house_tier_4");
            Vanilla("building_docks", 0, 1f, false, true, 0f, "house_tier_2");
            Vanilla("building_roads", 5, 1f, false, true, 0f);
            Vanilla("building_well", 12, 1f, false, false, 0f, "house_tier_2");
            Vanilla("building_watch_tower", 5, 1f, false, false, 0f, "house_tier_3", "weapon_bow");
            Vanilla("building_watch_tower_bonus", 20, 1f, true, false, 0f, "building_watch_tower");
            Vanilla("building_statues", 5, 1f, false, false, 0f, "house_tier_3");
            Vanilla("building_mine", 5, 1f, false, false, 0f, "house_tier_1");
            Vanilla("building_barracks", 5, 1f, false, false, 0f, "house_tier_2");
            Vanilla("building_windmill", 0, 1f, false, true, 0f, "house_tier_1");
            Vanilla("building_temple", 10, 1f, false, false, 0f, "house_tier_3");
            Vanilla("mining_efficiency", 3, 1f, false, false, 0f);
            Vanilla("sharp_axes", 2, 1f, false, false, 0f);
            Vanilla("weaponsmith", 20, 1f, true, false, 0f, "weapon_production");
            Vanilla("armorsmith", 20, 1f, true, false, 0f, "armor_production");
            Vanilla("trading", 7, 1f, false, false, 0f);
            Vanilla("trading_efficiency", 8, 1f, false, false, 0f, "trading");
            Vanilla("boats_trading", 4, 1f, false, false, 0f, "building_docks");
            Vanilla("boats_transport", 8, 1f, false, true, 0f, "building_docks");
            Vanilla("equipment_storage_1", 7, 1f, false, false, 0f, "weapon_production", "armor_production");
            Vanilla("equipment_storage_2", 0, 1f, false, false, 0f, "equipment_storage_1");
            Vanilla("equipment_storage_3", 0, 1f, false, false, 0f, "equipment_storage_2");
            Vanilla("weapon_sword", 3, 1f, false, true, 0f, "weapon_production");
            Vanilla("weapon_axe", 3, 1f, false, false, 0f, "weapon_production");
            Vanilla("weapon_hammer", 3, 1f, false, false, 0f, "weapon_production");
            Vanilla("weapon_spear", 3, 1f, false, false, 0f, "weapon_production");
            Vanilla("armor_production", 3, 1f, false, false, 0f, "weapon_production");
            Vanilla("weapon_production", 0, 1f, false, true, 0f);
            Vanilla("weapon_bow", 3, 1f, false, false, 0f, "weapon_production");
            Vanilla("material_copper", 4, 1f, false, false, 0f);
            Vanilla("material_bronze", 6, 1f, false, true, 0f, "material_copper");
            Vanilla("material_silver", 10, 1f, false, true, 0f, "material_bronze");
            Vanilla("material_iron", 20, 1f, false, true, 0f, "material_silver");
            Vanilla("material_steel", 30, 1f, false, false, 0f, "material_iron");
            Vanilla("material_mythril", 40, 1f, false, false, 0f, "material_steel");
            Vanilla("material_adamantine", 50, 1f, false, false, 0f, "material_mythril");

            // Tech.cs. None of them had requirements; only Cyberware was a priority tech.
            M1("MilitaryModern", 60, false, "Modern Military");
            M1("Firearms", 65, false, "Better Firearm Production");
            M1("LowFirearms", 55, false, "Pipe Guns");
            M1("Cyberware", 70, true, "Cyberware");
            M1("MIRV", 80, false, "Long Range Ballistic MIRVS");
            M1("BudgetMIRV", 65, false, "Short Range MIRVS");
            M1("DecentMIRV", 70, false, "Short Range MIRVS");
            M1("Skyscraper", 65, false, "tech_Skyscraper");
            M1("Casino", 40, false, "Casino");
            M1("STRONGMIRV", 90, false, "Nuclear MIRVs");
            M1("Tanks", 65, false, "Tanks");
            M1("Helicopter", 65, false, "Helicopters");
            M1("FighterJet", 65, false, "Fighter Jets");
            M1("MIRVBomber", 65, false, "Fighter Jets");
            M1("F55", 65, false, "F55");
            M1("Gunship", 70, false, "Gunship");
            M1("Drone", 65, false, "Drone");
            M1("Nukes", 65, false, "Nukes");
        }

        private static void Vanilla(string id, int level, float cost, bool rare, bool priority, float knowledgeGain, params string[] requirements)
        {
            Add(new M1TechDef
            {
                Id = id, RequiredLevel = level, KnowledgeCost = cost, Rare = rare, Priority = priority,
                KnowledgeGain = knowledgeGain, Requirements = requirements, Name = id
            });
        }

        private static void M1(string id, int level, bool priority, string name)
        {
            Add(new M1TechDef { Id = id, RequiredLevel = level, Priority = priority, Name = name, IsM1 = true });
        }

        private static void Add(M1TechDef def)
        {
            if (ById.ContainsKey(def.Id)) return;
            All.Add(def);
            ById[def.Id] = def;
        }
    }
}
