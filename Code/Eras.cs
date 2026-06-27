using System.Collections.Generic;
using System.Linq;

namespace ModernBox
{
    public class EraDefinition
    {
        public string key;
        public string name;
        public string description;

        public Dictionary<string, List<string>> bonfires = new();
        public Dictionary<string, List<string>> cityBuildings = new();
    }

    public static class EraLibrary
    {
        private static readonly string[] alliancecivs = { "human", "plague_doctor", "evil_mage", "civ_white_mage", "civ_cat", "civ_dog", "civ_chicken", "civ_sheep", "civ_acid_gentleman", "bandit" };
        private static readonly string[] hardencivs = { "dwarf", "cold_one", "snowman", "civ_armadillo", "civ_rhino", "civ_crab", "civ_penguin", "civ_turtle", "civ_crystal_golem", "civ_candy_man", "civ_goat" };
        private static readonly string[] gaiacivs = { "elf", "civ_rabbit", "civ_monkey", "civ_cow", "civ_buffalo", "civ_alpaca", "civ_capybara", "civ_frog", "civ_liliar", "druid", "fairy", "civ_garlic_man", "civ_lemon_man", "unicorn" };
        private static readonly string[] hordecivs = { "orc", "necromancer", "civ_fox", "civ_wolf", "civ_bear", "civ_hyena", "civ_rat", "civ_scorpion", "civ_crocodile", "civ_snake", "civ_piranha", "greg", "jumpy_skull" };

        public static List<EraDefinition> All = new List<EraDefinition>
        {
            new EraDefinition
            {
                key = "medieval",
                name = "Medieval",
                description = "Units have wagons, bows, swords, and horses.",

                bonfires = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "bonfire_alliance" },
                    ["harden"]   = new() { "bonfire_harden" },
                    ["gaia"]     = new() { "bonfire_gaia" },
                    ["horde"]    = new() { "bonfire_horde" }
                },

                cityBuildings = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new List<string> { "market_alliance" }
                        .Concat(alliancecivs.SelectMany(civ => new[] {
                            $"barracks_{civ}", $"house_{civ}_0", $"hall_{civ}_0",
                            $"temple_{civ}", $"library_{civ}", $"docks_{civ}", $"watch_tower_{civ}"
                        })).ToList(),

                    ["harden"] = new List<string> { "market_harden" }
                        .Concat(hardencivs.SelectMany(civ => new[] {
                            $"barracks_{civ}", $"house_{civ}_0", $"hall_{civ}_0",
                            $"temple_{civ}", $"library_{civ}", $"docks_{civ}", $"watch_tower_{civ}"
                        })).ToList(),

                    ["gaia"] = new List<string> { "market_gaia" }
                        .Concat(gaiacivs.SelectMany(civ => new[] {
                            $"barracks_{civ}", $"house_{civ}_0", $"hall_{civ}_0",
                            $"temple_{civ}", $"library_{civ}", $"docks_{civ}", $"watch_tower_{civ}"
                        })).ToList(),

                    ["horde"] = new List<string> { "market_horde" }
                        .Concat(hordecivs.SelectMany(civ => new[] {
                            $"barracks_{civ}", $"house_{civ}_0", $"hall_{civ}_0",
                            $"temple_{civ}", $"library_{civ}", $"docks_{civ}", $"watch_tower_{civ}"
                        })).ToList()
                }
            },

            new EraDefinition
            {
                key = "renaissance",
                name = "Renaissance",
                description = "Units have cannons, firearms, and advanced armor.",

                bonfires = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "bonfire_rain_alliance" },
                    ["harden"]   = new() { "bonfire_rain_harden" },
                    ["gaia"]     = new() { "bonfire_rain_gaia" },
                    ["horde"]    = new() { "bonfire_rain_horde" }
                },
                cityBuildings = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "House_rain_alliance", "Barracks_rain_alliance", "Hall_rain_alliance", "Temple_rain_alliance" },
                    ["harden"] = new() { "House_rain_harden", "Barracks_rain_harden", "Hall_rain_harden", "Temple_rain_harden" },
                    ["gaia"] = new() { "House_rain_gaia", "Barracks_rain_gaia", "Hall_rain_gaia", "Temple_rain_gaia" },
                    ["horde"] = new() { "House_rain_horde", "Barracks_rain_horde", "Hall_rain_horde", "Temple_rain_horde" }
                }
            },

            new EraDefinition
            {
                key = "modern",
                name = "Modern",
                description = "Units have tanks, aircraft, and modern weaponry.",

                bonfires = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "bonfire_modern_alliance" },
                    ["harden"]   = new() { "bonfire_modern_harden" },
                    ["gaia"]     = new() { "bonfire_modern_gaia" },
                    ["horde"]    = new() { "bonfire_modern_horde" }
                },

                cityBuildings = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "House_modern_alliance", "Barracks_modern_alliance", "Docks_modern_alliance", "watch_tower_modern_alliance", "mine_modern" },
                    ["harden"] = new() { "House_modern_harden", "Barracks_modern_harden", "Docks_modern_harden", "watch_tower_modern_harden", "mine_modern" },
                    ["gaia"] = new() { "House_modern_gaia", "Barracks_modern_gaia", "Docks_modern_gaia", "watch_tower_modern_gaia", "mine_modern" },
                    ["horde"] = new() { "House_modern_horde", "Barracks_modern_horde", "Docks_modern_horde", "watch_tower_modern_horde", "mine_modern" }
                }
            },

            new EraDefinition
            {
                key = "hyperfuture",
                name = "Hyperfuture",
                description = "Units have lasers, war mechs, and cyber technology.",

                bonfires = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "bonfire_future_alliance" },
                    ["harden"]   = new() { "bonfire_future_harden" },
                    ["gaia"]     = new() { "bonfire_future_gaia" },
                    ["horde"]    = new() { "bonfire_future_horde" }
                },

                cityBuildings = new Dictionary<string, List<string>>
                {
                    ["alliance"] = new() { "House_future_alliance", "Barracks_modern_alliance", "Docks_modern_alliance", "watch_tower_modern_alliance", "mine_modern" },
                    ["harden"]   = new() { "House_future_harden", "Barracks_modern_harden", "Docks_modern_harden", "watch_tower_modern_harden", "mine_modern" },
                    ["gaia"]     = new() { "House_future_gaia", "Barracks_modern_gaia", "Docks_modern_gaia", "watch_tower_modern_gaia", "mine_modern" },
                    ["horde"]    = new() { "House_future_horde", "Barracks_modern_horde", "Docks_modern_horde", "watch_tower_modern_horde", "mine_modern" }
                }
            }
        };
    }
}