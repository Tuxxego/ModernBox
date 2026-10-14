using System;
using System.Collections.Generic;

namespace ModernBoxM2Rewrite
{
    // The culture tech each M2 building and item needed in the original
    // (BuildingAsset.tech in UpgradesUwU.cs/Commerce.cs, ItemAsset.tech_needed in guns.cs,
    // cyberware.cs, Drugs.cs and Resourcez.cs). Anything not listed falls back to its era.
    internal static class M2TechGates
    {
        private static readonly Dictionary<string, string> Buildings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "MissileSilo", "Nukes" },
            { "Barracks_rain_human", "Renaissance" }, { "Barracks_industrial_human", "Firearms" },
            { "Barracks_modern_human", "MilitaryModern" }, { "Barracks_future_human", "Future" },
            { "dock_rain_human", "Renaissance" }, { "dock_industrial_human", "Firearms" }, { "dock_modern_human", "MilitaryModern" },
            { "hall_industrial_human", "Industrial" }, { "hall_modern_human", "Skyscraper" }, { "hall_future_human", "Future" },
            { "house_industrial_human", "Industrial" }, { "house_modern_human", "Skyscraper" }, { "house_future_human", "Future" },
            { "mine_rain_human", "Renaissance" }, { "mine_industrial_human", "Industrial" },
            { "mine_modern_human", "MilitaryModern" }, { "mine_future_human", "Future" },
            { "temple_rain_human", "Renaissance" }, { "temple_industrial_human", "Industrial" },
            { "temple_modern_human", "MilitaryModern" }, { "temple_future_human", "Future" },
            { "watch_tower_rain_human", "Renaissance" }, { "watch_tower_industrial_human", "Industrial" },
            { "watch_tower_modern_human", "MilitaryModern" }, { "watch_tower_future_human", "Future" }
        };

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Crack", "Cyberware" }, { "Meth", "Cyberware" }, { "Sandevistan", "Cyberware" }, { "TurboBooster", "Cyberware" },
            { "Gun", "Firearms" }, { "PipeGun", "Renaissance" },
            { "Musket", "Renaissance_knowledge" },
            { "PipePistol", "Renaissance" }, { "PipeRifle", "Renaissance" }, { "PipeShotgun", "Renaissance" },
            { "piratpistol", "Renaissance" }, { "pristinearmor", "Renaissance" }, { "pristineboots", "Renaissance" },
            { "pristinehelmet", "Renaissance" }, { "shieldedaxe", "Renaissance" }, { "shieldedhammer", "Renaissance" },
            { "shieldedspear", "Renaissance" }, { "shieldedsword", "Renaissance" },
            { "Americanshotgun", "Firearms" }, { "m1garand", "Firearms" },
            { "wwarmor", "Firearms" }, { "wwboots", "Firearms" }, { "wwhelmet", "Firearms" },
            { "AK103", "MilitaryModern" }, { "AK47", "MilitaryModern" }, { "DesertEagle", "MilitaryModern" },
            { "FAMAS", "MilitaryModern" }, { "Glock17", "MilitaryModern" }, { "HK416", "MilitaryModern" },
            { "M16", "MilitaryModern" }, { "M4A1", "MilitaryModern" }, { "MP7", "MilitaryModern" },
            { "Minigun", "MilitaryModern" }, { "RocketLauncher", "MilitaryModern" }, { "SGT44", "MilitaryModern" },
            { "Sniper", "MilitaryModern" }, { "ThompsonM1A1", "MilitaryModern" }, { "Uzi", "MilitaryModern" },
            { "XM8", "MilitaryModern" }, { "malorian", "MilitaryModern" },
            { "modernarmor", "MilitaryModern" }, { "modernboots", "MilitaryModern" }, { "modernhelmet", "MilitaryModern" },
            { "blueblaster", "Future" }, { "blueblastersniper", "Future" }, { "blueheavyblaster", "Future" },
            { "bluelightsaber", "Future" }, { "blueminigun", "Future" }, { "blueplasmagun", "Future" },
            { "greenblaster", "Future" }, { "greenblastersniper", "Future" }, { "greenheavyblaster", "Future" },
            { "greenlightsaber", "Future" }, { "greenminigun", "Future" }, { "greenplasmagun", "Future" },
            { "redblaster", "Future" }, { "redblastersniper", "Future" }, { "redheavyblaster", "Future" },
            { "redlightsaber", "Future" }, { "redminigun", "Future" }, { "redplasmagun", "Future" },
            { "chainsaw", "Future" }, { "futurearmor", "Future" }, { "futureboots", "Future" }, { "futurehelmet", "Future" }
        };

        private static readonly string[] RaceSuffixes = { "_orc", "_elf", "_dwarf" };

        internal static string ForBuilding(BuildingSpec spec)
        {
            if (spec == null) return null;
            return ForBuilding(spec.SourceId) ?? ForBuilding(spec.Id);
        }

        internal static string ForBuilding(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string tech;
            if (Buildings.TryGetValue(id, out tech)) return tech;
            // Other races use the human chain with their own suffix.
            foreach (string suffix in RaceSuffixes)
                if (id.EndsWith(suffix, StringComparison.Ordinal) &&
                    Buildings.TryGetValue(id.Substring(0, id.Length - suffix.Length) + "_human", out tech)) return tech;
            return null;
        }

        internal static string ForItem(string id)
        {
            string tech;
            return id != null && Items.TryGetValue(id, out tech) ? tech : null;
        }

        internal static bool CityAllowsBuilding(City city, BuildingSpec spec)
        {
            if (spec == null || !ModernProgression.IsSupportedCity(city)) return false;
            string tech = ForBuilding(spec);
            return tech != null ? M2Tech.Has(city, tech) : ModernProgression.GetEra(city) >= spec.Era;
        }

        internal static bool CityAllowsBuilding(City city, string buildingId, M2Era era)
        {
            if (!ModernProgression.IsSupportedCity(city)) return false;
            string tech = ForBuilding(buildingId);
            return tech != null ? M2Tech.Has(city, tech) : ModernProgression.GetEra(city) >= era;
        }

        internal static bool CityAllowsItem(City city, string itemId, M2Era era)
        {
            if (!ModernProgression.IsSupportedCity(city)) return false;
            string tech = ForItem(itemId);
            return tech != null ? M2Tech.Has(city, tech) : ModernProgression.GetEra(city) >= era;
        }
    }
}
