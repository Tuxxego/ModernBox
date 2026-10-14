using System;
using System.Collections.Generic;
using System.IO;

namespace ModernBoxM2Rewrite
{
    /// <summary>
    /// The walk/idle/swim frame lists each original M2 actor set (or inherited from
    /// the M2 actor it was cloned from). Frames missing from GameResources are skipped.
    /// </summary>
    internal static class M2OriginalAnimations
    {
        // id -> { walk, idle, swim }; null means the original did not set it.
        private static readonly Dictionary<string, string[][]> Frames = new Dictionary<string, string[][]>(StringComparer.Ordinal)
        {
            { "AbramTank", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "americanbomberww", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7", "walk_8", "walk_9", "walk_10", "walk_11", "walk_12", "walk_13", "walk_14", "walk_15" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7", "walk_8", "walk_9", "walk_10", "walk_11", "walk_12", "walk_13", "walk_14", "walk_15" }, null } },
            { "armoredwolf", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "idle_0", "idle_1" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "artilleryatst", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "assimilarptor", new[] { new[] { "walk_0", "walk_1", "walk_2" }, null, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "assimilatrax", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "Assimilatus", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "assizeppelin", new[] { null, new[] { "idle_0", "idle_1", "idle_2" }, null } },
            { "AT9000", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7" } } },
            { "atst", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "atstsniper", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "balloonunit", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null } },
            { "basecrusader", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "baseMA9000", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "batteringram", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "idle_0", "idle_1", "idle_2" }, new[] { "swim_0" } } },
            { "bigfaerydragon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, null } },
            { "bigtankww", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "biplane", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" } } },
            { "catapulta", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "idle_0", "idle_1", "idle_2", "idle_1" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Cocytuswalker", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0" }, null } },
            { "crusaderdreadnaught", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "crusaderHeli", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "crusadermaus", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "davincitank", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "dreadnaught", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "Drone", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "Duneworm", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7", "walk_8", "walk_9" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7", "swim_8", "swim_9" } } },
            { "dwarfcannon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "dwarfdoctor", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "elfcannon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "eliteAT9000", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7" } } },
            { "EliteBomber", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3" }, null } },
            { "eliteGunship", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3" }, null } },
            { "EliteP9000", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "EliteZeppelin", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3" }, null } },
            { "EVA01", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "F55FighterJet", new[] { new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, null } },
            { "F55FighterJet1", new[] { new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, null } },
            { "fairelf", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "fairydragon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "FighterJet", new[] { new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "FighterJet1", new[] { new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "fighterww", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7" } } },
            { "fishing_boat_industrial", new[] { new[] { "swim_0", "swim_1", "swim_2", "swim_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "fishing_boat_modern", new[] { new[] { "swim_0", "swim_1", "swim_2", "swim_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "fishing_boat_renaissance", new[] { new[] { "swim_0", "swim_1", "swim_2", "swim_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "geckoid", new[] { new[] { "walk_0", "walk_1", "walk_2" }, null, null } },
            { "genericwwtank", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "glitchdrake", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, null } },
            { "glitchspectre", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0" }, null } },
            { "glitchtarantula", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0" }, null } },
            { "golemgem", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Gunship", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_1", "walk_2" } } },
            { "hashbrowncat", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, null } },
            { "Heli", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "HeliELite", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3" }, null } },
            { "helilator", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, null } },
            { "human_industrial_battleship", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_industrial_corvette1", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_industrial_corvette2", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_industrial_trading", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_modern_battleship", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_modern_corvette1", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_modern_corvette2", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_modern_gunboat", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_modern_submarine", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7", "swim_8", "swim_9", "swim_10" } } },
            { "human_modern_trading", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "human_renaissance_battleship", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_renaissance_corvette1", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_renaissance_corvette2", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "human_renaissance_trading", new[] { new[] { "swim_0", "swim_1", "swim_2", "swim_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "humancannon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "humancavalry", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "humanpaladin", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "HumanTitan", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "HumanTitanElite", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Humvee", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "icedracoid", new[] { null, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null } },
            { "landship", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "MA9000", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "MIRVBomber", new[] { new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9", "idle_10", "idle_11", "idle_12", "idle_13", "idle_14", "idle_15", "idle_16", "idle_17", "idle_18", "idle_19" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7", "idle_8", "idle_9", "idle_10", "idle_11", "idle_12", "idle_13", "idle_14", "idle_15", "idle_16", "idle_17", "idle_18", "idle_19" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "MissileSystem", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "modernhumvee", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "modernsupporttruck", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "ogreunit", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_0", "swim_1" } } },
            { "OmegaRailgun", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "orcatapulta", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2", "idle_1" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "orccannon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "orcwarlock", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "orcwarturtle", new[] { new[] { "walk_0" }, null, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "P9000", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" } } },
            { "pantherax", new[] { new[] { "walk_0", "walk_1", "walk_2" }, null, null } },
            { "peones", new[] { new[] { "walk_0", "walk_1", "walk_2" }, null, null } },
            { "pterax", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5" }, null } },
            { "Railgun", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "santaguin", new[] { new[] { "walk_0", "walk_1", "walk_2" }, null, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "scandid", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "idle_0", "idle_1", "idle_2" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "shermanww", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Soldier", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_0", "swim_1" } } },
            { "SpaceMarine", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "spaceork", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "supportatst", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Tank", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0", "idle_1", "idle_2" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "tankie", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Terlanius", new[] { new[] { "walk_0" }, null, new[] { "swim_0" } } },
            { "Terran", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "teslatruckgun", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "TIEfighter", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "idle_0", "idle_1", "idle_2", "idle_3", "idle_4", "idle_5", "idle_6", "idle_7" }, null } },
            { "treant", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "wheeledtank", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "woolyrhino", new[] { new[] { "walk_0", "walk_1", "walk_2" }, null, new[] { "swim_0", "swim_1", "swim_2" } } },
            { "wwartillery", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "wwsupporttruck", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, new[] { "swim_0", "swim_1", "swim_2", "swim_4" } } },
            { "xenodogo", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "idle_0", "idle_1", "idle_2" }, null } },
            { "newwalker", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "normalwalker", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null, new[] { "swim_0", "swim_1", "swim_2", "swim_3" } } },
            { "Xiexel", new[] { new[] { "walk_0" }, new[] { "walk_0" }, new[] { "walk_0" } } },
            { "Zeppelin", new[] { new[] { "walk_0", "walk_1", "walk_2" }, new[] { "walk_0" }, new[] { "walk_0", "walk_1", "walk_2" } } },
            { "zombieabomination", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombieacid", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombieacidman", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombieballoon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null } },
            { "zombieclawed", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiedemon", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiedoctor", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiedruid", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "idle_0" }, null } },
            { "zombieevilhorseman", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiefairy", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, null } },
            { "zombiefiremaniac", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiehulk", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombieicelich", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiemother", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiepoison", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiespeed", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiespikes", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombiestalker", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombietarantula", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
            { "zombietentacle", new[] { new[] { "walk_0", "walk_1", "walk_2", "walk_3" }, new[] { "walk_0" }, null } },
        };

        internal static void Apply(ActorAsset actor, string id, string textureFolder)
        {
            ApplyMovement(actor, id);
            string[][] frames;
            if (!Frames.TryGetValue(id, out frames)) return;
            string[] walk = Existing(textureFolder, frames[0]);
            string[] idle = Existing(textureFolder, frames[1]);
            string[] swim = Existing(textureFolder, frames[2]);
            if (walk != null) actor.animation_walk = walk;
            if (idle != null) actor.animation_idle = idle;
            if (swim != null) actor.animation_swim = swim;
        }

        // _mob set disableJumpAnimation, so almost every M2 land unit slid instead of hopping.
        private static readonly HashSet<string> NoHop = new HashSet<string>(StringComparer.Ordinal)
        {
            "AT9000", "AbramTank", "Cocytuswalker", "Drone", "Duneworm", "EVA01",
            "EliteBomber", "EliteP9000", "EliteZeppelin", "F55FighterJet", "F55FighterJet1", "FighterJet",
            "FighterJet1", "Gunship", "Heli", "HeliELite", "HumanTitan", "HumanTitanElite",
            "Humvee", "MA9000", "MIRVBomber", "MissileSystem", "OmegaRailgun", "P9000",
            "Railgun", "Soldier", "SpaceMarine", "TIEfighter", "Tank", "Terlanius",
            "Terran", "Xiexel", "Zeppelin", "alienwisp", "americanbomberww", "armoredwolf",
            "artilleryatst", "assimilatrax", "assizeppelin", "atst", "atstsniper", "balloonunit",
            "baseMA9000", "basecrusader", "batteringram", "bigfaerydragon", "bigtankww", "biplane",
            "buffrost", "catapulta", "crusaderHeli", "crusaderdreadnaught", "crusadermaus", "davincitank",
            "dreadnaught", "dwarfcannon", "dwarfdoctor", "elfcannon", "eliteAT9000", "eliteGunship",
            "fairelf", "fairydragon", "fighterww", "genericwwtank", "glitchdrake", "glitchspectre",
            "glitchtarantula", "golemgem", "hashbrowncat", "helilator", "humancannon", "humancavalry",
            "humanpaladin", "icedracoid", "landship", "modernhumvee", "modernsupporttruck", "newwalker",
            "normalwalker", "ogreunit", "orcatapulta", "orccannon", "orcwarlock", "pantherax",
            "peones", "pterax", "santaguin", "scandid", "shermanww", "spaceork",
            "supportatst", "tankie", "teslatruckgun", "treant", "wheeledtank", "woolyrhino",
            "wwartillery", "wwsupporttruck", "xenodogo", "zombieabomination", "zombieacid", "zombieacidman",
            "zombieballoon", "zombieclawed", "zombiedemon", "zombiedoctor", "zombiedruid", "zombieevilhorseman",
            "zombiefairy", "zombiefiremaniac", "zombiehulk", "zombieicelich", "zombiemother", "zombiepoison",
            "zombiespeed", "zombiespikes", "zombiestalker", "zombietarantula", "zombietentacle"
        };
        private static readonly HashSet<string> Hop = new HashSet<string>(StringComparer.Ordinal)
        {
            "Assimilatus", "assimilarptor", "geckoid", "rhinokinglor"
        };

        internal static void ApplyMovement(ActorAsset actor, string id)
        {
            if (NoHop.Contains(id)) actor.disable_jump_animation = true;
            else if (Hop.Contains(id)) actor.disable_jump_animation = false;
            // ModernMilitary.cs: EVA01.shadow = false.
            if (id == "EVA01") actor.shadow = false;
        }

        private static string[] Existing(string textureFolder, string[] names)
        {
            if (names == null) return null;
            string folder = Path.Combine(ModernBoxMod.ModFolder, "GameResources", "actors", textureFolder);
            // Units drawn from a vanilla sprite sheet keep the frames the port picked for it.
            if (!Directory.Exists(folder)) return null;
            List<string> result = new List<string>(names.Length);
            foreach (string name in names)
                if (File.Exists(Path.Combine(folder, name + ".png"))) result.Add(name);
            return result.Count == 0 ? null : result.ToArray();
        }
    }
}
