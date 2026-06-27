//

//

using System;
using tools;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using HarmonyLib;
using ai;
using ai.behaviours;
using System.Text.RegularExpressions;
using Beebyte.Obfuscator;
using NeoModLoader.General;
using TuxModLoader.Reflection;

namespace ModernBox {
  class Traits : MonoBehaviour {
    public static bool vehiclesAllowed;
        public static bool allowATAT;
        public static bool allowDreadnought;
        public static bool allowGunship;
        public static bool allowTIEFighter;
        public static bool allowGoliath;
        public static bool allowMechagodzilla;
        public static bool allowEarthquaker;
        public static bool allowSpaceMarines;
    private static readonly HashSet<string> _loggedCartTransformIssues =
        new HashSet<string>();

    private static void LogCartIssueOnce(string key, string message) {
      if (_loggedCartTransformIssues.Add(key)) {
        ModernBoxLogger.Warning(message);
      }
    }

    private static string PickValidCartCandidate(List<string> candidates,
                                                 string eraKey,
                                                 string civGroup,
                                                 string role,
                                                 string leaderSpeciesId) {
      if (candidates == null || candidates.Count == 0)
        return null;

      int startIndex = UnityEngine.Random.Range(0, candidates.Count);
      for (int i = 0; i < candidates.Count; i++) {
        string candidateId = candidates[(startIndex + i) % candidates.Count];
        if (string.IsNullOrEmpty(candidateId)) {
          LogCartIssueOnce(
              "cart-empty-id|" + eraKey + "|" + civGroup + "|" + role,
              "[CartTransform] Empty candidate id found for era='" + eraKey +
                  "' civ_group='" + civGroup + "' role='" + role + "'");
          continue;
        }

        ActorAsset actorAsset = AssetManager.actor_library.get(candidateId);
        if (actorAsset == null) {
          LogCartIssueOnce(
              "cart-missing-actor|" + candidateId,
              "[CartTransform] Missing actor asset id='" + candidateId +
                  "' era='" + eraKey + "' civ_group='" + civGroup +
                  "' role='" + role + "' leader_species='" + leaderSpeciesId +
                  "'");
          continue;
        }

        string defaultAttackId = actorAsset.default_attack;
        if (string.IsNullOrEmpty(defaultAttackId)) {
          LogCartIssueOnce(
              "cart-empty-default-attack|" + candidateId,
              "[CartTransform] Actor id='" + candidateId +
                  "' has empty default_attack era='" + eraKey +
                  "' civ_group='" + civGroup + "' role='" + role + "'");
          continue;
        }

        if (AssetManager.items.get(defaultAttackId) == null) {
          LogCartIssueOnce(
              "cart-missing-default-attack|" + candidateId + "|" +
                  defaultAttackId,
              "[CartTransform] Actor id='" + candidateId +
                  "' default_attack='" + defaultAttackId +
                  "' is missing in item library era='" + eraKey +
                  "' civ_group='" + civGroup + "' role='" + role + "'");
          continue;
        }

        return candidateId;
      }

      string joined = string.Join(",", candidates.Where(x => !string.IsNullOrEmpty(x)));
      LogCartIssueOnce(
          "cart-no-valid-candidates|" + eraKey + "|" + civGroup + "|" + role +
              "|" + leaderSpeciesId,
          "[CartTransform] No valid candidates found era='" + eraKey +
              "' civ_group='" + civGroup + "' role='" + role +
              "' leader_species='" + leaderSpeciesId + "' candidates='" +
              joined + "'");
      return null;
    }

    public static void init() {
      CoolStuff();
    }

    private static void CoolStuff() {
      CartTransformations.InitCartTransformations();

//

      ActorTraitGroupAsset Ideology = new ActorTraitGroupAsset();
      Ideology.id = "Ideology";
      Ideology.name = "trait_group_Ideology";
      Ideology.color = "#5EFFFF";
      AssetManager.trait_groups.add(Ideology);
      LM.AddToCurrentLocale("trait_group_Ideology", "Ideology");

////////ideology must be changed so it does not override existing
///////ideology must be changed so it does not override existing
//////ideology must be changed so it does not override existing
/////ideology must be changed so it does not override existing
////ideology must be changed so it does not override existing
///ideology must be changed so it does not override existing
//ideology must be changed so it does not override existing

//ideology of populace if they already had, only in rare occasions. Also,

//leaders should obtain a different ideology on a rare chance as they age

//or gain traits in a %. And reintroduction of ideological diplomacy.

      ActorTrait Dynastic = new ActorTrait();
      Dynastic.id = "Dynastic";
      Dynastic.needs_to_be_explored = false;
      Dynastic.rarity = Rarity.R0_Normal;
      Dynastic.path_icon = "ui/icons/Dynastic";
      Dynastic.rate_inherit = 5;
      Dynastic.group_id = "Ideology";
      Dynastic.action_special_effect = new WorldAction(VehicleSummonEffect);
      Dynastic.unlocked_with_achievement = false;
      Dynastic.addOpposite("Martial");
      Dynastic.addOpposite("Mercantile");
      Dynastic.addOpposite("Peoplewoven");
      Dynastic.addOpposite("Chaosvolt");
      AssetManager.traits.add(Dynastic);
      LM.AddToCurrentLocale("trait_Dynastic", "Dynastic Ideology");
      LM.AddToCurrentLocale("trait_Dynastic_info", "LONG LIVE THE KING!!!!");
      LM.AddToCurrentLocale("trait_Dynastic_info_2",
                            "Nepo bebes and cake loving royals");

      ActorTrait Martial = new ActorTrait();
      Martial.id = "Martial";
      Martial.needs_to_be_explored = false;
      Martial.rarity = Rarity.R0_Normal;
      Martial.path_icon = "ui/icons/Martial";
      Martial.rate_inherit = 5;
      Martial.group_id = "Ideology";
      Martial.unlocked_with_achievement = false;
      Martial.action_special_effect = new WorldAction(VehicleSummonEffect);
      Martial.addOpposite("Dynastic");
      Martial.addOpposite("Mercantile");
      Martial.addOpposite("Peoplewoven");
      Martial.addOpposite("Chaosvolt");
      AssetManager.traits.add(Martial);
      LM.AddToCurrentLocale("trait_Martial", "Martial Ideology");
      LM.AddToCurrentLocale(
          "trait_Martial_info",
          "All others will learn of our peaceful ways, BY FORCE!!!");
      LM.AddToCurrentLocale("trait_Martial_info_2", "HOI4 enjoyers");

      ActorTrait Peoplewoven = new ActorTrait();
      Peoplewoven.id = "Peoplewoven";
      Peoplewoven.needs_to_be_explored = false;
      Peoplewoven.rarity = Rarity.R0_Normal;
      Peoplewoven.path_icon = "ui/icons/Peoplewoven";
      Peoplewoven.rate_inherit = 5;
      Peoplewoven.group_id = "Ideology";
      Peoplewoven.unlocked_with_achievement = false;
      Peoplewoven.action_special_effect = new WorldAction(VehicleSummonEffect);
      Peoplewoven.addOpposite("Martial");
      Peoplewoven.addOpposite("Mercantile");
      Peoplewoven.addOpposite("Chaosvolt");
      Peoplewoven.addOpposite("Dynastic");
      AssetManager.traits.add(Peoplewoven);
      LM.AddToCurrentLocale("trait_Peoplewoven", "Peoplewoven Ideology");
      LM.AddToCurrentLocale("trait_Peoplewoven_info", "FOR THE PEASANTS!!");
      LM.AddToCurrentLocale("trait_Peoplewoven_info_2",
                            "HOI4 enjoyers as well");

      ActorTrait Mercantile = new ActorTrait();
      Mercantile.id = "Mercantile";
      Mercantile.needs_to_be_explored = false;
      Mercantile.rarity = Rarity.R0_Normal;
      Mercantile.path_icon = "ui/icons/Mercantile";
      Mercantile.rate_inherit = 5;
      Mercantile.group_id = "Ideology";
      Mercantile.unlocked_with_achievement = false;
      Mercantile.action_special_effect = new WorldAction(VehicleSummonEffect);
      Mercantile.addOpposite("Dynastic");
      Mercantile.addOpposite("Chaosvolt");
      Mercantile.addOpposite("Peoplewoven");
      Mercantile.addOpposite("Martial");
      AssetManager.traits.add(Mercantile);
      LM.AddToCurrentLocale("trait_Mercantile", "Mercantile Ideology");
      LM.AddToCurrentLocale(
          "trait_Mercantile_info",
          "Not selling my own family for coins was the friends we made along the way");
      LM.AddToCurrentLocale("trait_Mercantile_info_2",
                            "bro, want some Maximcoin?");

      ActorTrait Chaosvolt = new ActorTrait();
      Chaosvolt.id = "Chaosvolt";
      Chaosvolt.needs_to_be_explored = false;
      Chaosvolt.rarity = Rarity.R0_Normal;
      Chaosvolt.path_icon = "ui/icons/Chaosvolt";
      Chaosvolt.rate_inherit = 5;
      Chaosvolt.group_id = "Ideology";
      Chaosvolt.unlocked_with_achievement = false;
      Chaosvolt.action_special_effect = new WorldAction(VehicleSummonEffect);
      Chaosvolt.addOpposite("Martial");
      Chaosvolt.addOpposite("Mercantile");
      Chaosvolt.addOpposite("Peoplewoven");
      Chaosvolt.addOpposite("Dynastic");
      AssetManager.traits.add(Chaosvolt);
      LM.AddToCurrentLocale("trait_Chaosvolt", "Chaosvolt Ideology");
      LM.AddToCurrentLocale("trait_Chaosvolt_info",
                            "REVOLT! REVOLT!! REVOLUTION!!!");
      LM.AddToCurrentLocale("trait_Chaosvolt_info_2",
                            "Mostly peaceful nuclear civil wars");

      ActorTrait Unitpotential = new ActorTrait();
      Unitpotential.id = "Unitpotential";
      Unitpotential.needs_to_be_explored = false;
      Unitpotential.rarity = Rarity.R0_Normal;
      Unitpotential.path_icon = "ui/icons/UnitpotentialIcon";
      Unitpotential.group_id = "special";
      Unitpotential.is_mutation_box_allowed = false;
      Unitpotential.can_be_given = false;
      Unitpotential.unlocked_with_achievement = false;
      Unitpotential.addOpposite("madness");
      if (Unitpotential.base_stats == null)
        Unitpotential.base_stats = new BaseStats();

      Unitpotential.action_special_effect =
          new WorldAction(UnitpotentialEffect);
      AssetManager.traits.add(Unitpotential);
      LM.AddToCurrentLocale("trait_Unitpotential", "Vehicle/War Unit");
      LM.AddToCurrentLocale("trait_Unitpotential_info",
                            "Enables lots of fun :3");
      LM.AddToCurrentLocale("trait_Unitpotential_info_2",
                            "helicopter helicopter");

      ActorTrait NavalUnit = new ActorTrait();
      NavalUnit.id = "NavalUnit";
      NavalUnit.needs_to_be_explored = false;
      NavalUnit.rarity = Rarity.R0_Normal;
      NavalUnit.path_icon = "ui/icons/WarBoat";
      NavalUnit.group_id = "special";
      NavalUnit.is_mutation_box_allowed = false;
      NavalUnit.can_be_given = false;
      NavalUnit.unlocked_with_achievement = false;
      AssetManager.traits.add(NavalUnit);
      LM.AddToCurrentLocale("trait_NavalUnit", "Naval Unit");
      LM.AddToCurrentLocale("trait_NavalUnit_info", "Enables lots of fun :3");
      LM.AddToCurrentLocale("trait_NavalUnit_info_2",
                            "Big boats, big cannons, and lots of fun :D");

      ActorTrait Gay = new ActorTrait();
      Gay.id = "Gay";
      Gay.needs_to_be_explored = false;
      Gay.rarity = Rarity.R0_Normal;
      Gay.path_icon = "ui/icons/HOMO!";
      Gay.rate_inherit = 5;
      Gay.group_id = "mind";
      Gay.unlocked_with_achievement = false;
      AssetManager.traits.add(Gay);
      LM.AddToCurrentLocale("trait_Gay", "Gay");
      LM.AddToCurrentLocale(
          "trait_Gay_info",
          "THis is only needed for the homo bomb, this person likes the same sex.");
      LM.AddToCurrentLocale("trait_Gay_info_2", "literally dank btw");

      LM.ApplyLocale(true);
    }

/////////skin colors for vehicles being given based on ideology, ideology
////////skin colors for vehicles being given based on ideology, ideology
///////skin colors for vehicles being given based on ideology, ideology
//////skin colors for vehicles being given based on ideology, ideology
/////skin colors for vehicles being given based on ideology, ideology
////skin colors for vehicles being given based on ideology, ideology
///skin colors for vehicles being given based on ideology, ideology
//skin colors for vehicles being given based on ideology, ideology

//diplomacy, unique vehicles for demons, aliens and angels, and, limit of

//spawned carts based on untis with unitpotential on village, and now

//instead of king, leaders are the ones that create vehicles, and race

//checked is of leader of home village, spawning of vehicles uses resources

//like boat do

    public static class CartTransformations {
      public static Dictionary<string, Dictionary<string, List<string>>>
          CartTransformationsRenaissanceRoles = new();

      public static Dictionary<string, Dictionary<string, List<string>>>
          CartTransformationsMedievalRoles = new();

      public static Dictionary<string, Dictionary<string, List<string>>>
          CartTransformationsModernRoles = new();

      public static Dictionary<string, Dictionary<string, List<string>>>
          CartTransformationsHyperFutureRoles = new();

      public static void InitCartTransformations() {
        string[] alliancecivs = {
          "human",       "plague_doctor", "evil_mage",
          "white_mage",  "civ_cat",       "civ_dog",
          "civ_chicken", "civ_sheep",     "civ_acid_gentleman",
          "bandit"
        };
        string[] hardencivs = {
          "dwarf",         "cold_one",   "snowman",
          "civ_armadillo", "civ_rhino",  "civ_crab",
          "civ_penguin",   "civ_turtle", "civ_crystal_golem",
          "civ_goat"
        };
        string[] gaiacivs = { "elf",           "civ_rabbit",  "civ_monkey",
                              "civ_cow",       "civ_buffalo", "civ_alpaca",
                              "civ_capybara",  "civ_frog",    "civ_liliar",
                              "druid",         "fairy",       "civ_garlic_man",
                              "civ_lemon_man", "unicorn" };
        string[] hordecivs = { "orc",         "necromancer",  "civ_fox",
                               "civ_wolf",    "civ_bear",     "civ_hyena",
                               "civ_rat",     "civ_scorpion", "civ_crocodile",
                               "civ_snake",   "civ_piranha",  "greg",
                               "jumpy_skull", "skeleton" };
        string[] demoncivs = { "miniciv_demon", "demon" };
        string[] aliencivs = { "civ_alien", "alien" };

        void AddCivs(
            string[] ids, string baseId, string era,
            Dictionary<string, Dictionary<string, List<string>>> targetDict) {
          foreach (var id in ids) {
            var roles = new Dictionary<string, List<string>> {
              { "offensive",
                CartTransformations.OffensiveOptions[baseId].GetValueOrDefault(
                    era, new List<string>()) },
              { "heavy",
                CartTransformations.HeavyOptions[baseId].GetValueOrDefault(
                    era, new List<string>()) },
              { "support",
                CartTransformations.SupportOptions[baseId].GetValueOrDefault(
                    era, new List<string>()) },
              { "air", CartTransformations.AirOptions[baseId].GetValueOrDefault(
                           era, new List<string>()) }
            };

            if (era == "hyperfuture" &&
                CartTransformations.TitanOptions.TryGetValue(
                    baseId, out var titanEras) &&
                titanEras.TryGetValue("hyperfuture", out var titanList)) {
              roles["titan"] = titanList;
            }

            targetDict[id] = roles;
          }
        }

        foreach (var era in new[] { "medieval", "renaissance", "modern",
                                    "hyperfuture" }) {
          Dictionary<string, Dictionary<string, List<string>>> targetDict =
              era switch {
                "medieval" => CartTransformationsMedievalRoles,
                "renaissance" => CartTransformationsRenaissanceRoles,
                "modern" => CartTransformationsModernRoles,
                "hyperfuture" =>
                    CartTransformations.CartTransformationsHyperFutureRoles,
                _ => null
              };

          AddCivs(alliancecivs, "human", era, targetDict);
          AddCivs(hardencivs, "dwarf", era, targetDict);
          AddCivs(gaiacivs, "elf", era, targetDict);
          AddCivs(hordecivs, "orc", era, targetDict);
          AddCivs(demoncivs, "demon", era, targetDict);
          AddCivs(aliencivs, "aliens", era, targetDict);
        }
      }

      public static Dictionary<string, Dictionary<string, List<string>>> OffensiveOptions =
      new Dictionary<string, Dictionary<string, List<string>>>
      {
        { "human", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "SpaceMarine" , "Terran" , "teslatruckgun" , "atst" ,  "SpaceMarine" , "artilleryatst" , "atstsniper" } },
          { "modern",      new List<string>{ "modernhumvee_Human" , "howitzer_Human" } },
          { "industrial",  new List<string>{ "Humvee" , "howitzer_Human" } },
          { "renaissance", new List<string>{ "humancavalry" , "humancannon" } },
          { "medieval",    new List<string>{ "humancavalry" } }
        }},
        { "orc", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "spaceork" , "Terran" , "teslatruckgun" , "atst" ,  "spaceork" , "artilleryatst" , "atstsniper" } },
          { "modern",      new List<string>{ "modernhumvee_Ork" , "howitzer_Ork" } },
          { "industrial",  new List<string>{ "Humvee" , "howitzer_Human" } },
          { "renaissance", new List<string>{ "ogreunit", "orccannon", "armoredwolf" } },
          { "medieval",    new List<string>{ "ogreunit" , "armoredwolf" } }
        }},
        { "dwarf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "SpaceMarine" , "Terran" , "teslatruckgun" , "atst" ,  "SpaceMarine" , "artilleryatst" , "atstsniper" } },
          { "modern",      new List<string>{ "modernhumvee_Dwarf" , "howitzer_Dwarf" } },
          { "industrial",  new List<string>{ "Humvee" , "howitzer_Human" } },
          { "renaissance", new List<string>{ "dwarfcannon" } },
          { "medieval",    new List<string>{ "golemgem" } }
        }},
        { "elf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "SpaceMarine" , "Terran" , "teslatruckgun" , "atst" ,  "SpaceMarine" , "artilleryatst" , "atstsniper" } },
          { "modern",      new List<string>{ "modernhumvee_Gaia" , "howitzer_Gaia" } },
          { "industrial",  new List<string>{ "Humvee" , "howitzer_Human" } },
          { "renaissance", new List<string>{ "treant" , "elfcannon" } },
          { "medieval",    new List<string>{ "treant" } }
        }},
        { "demon", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "SpaceMarine" , "Terran" , "teslatruckgun" , "atst" ,  "SpaceMarine" , "artilleryatst" , "atstsniper" } },
          { "modern",      new List<string>{ "demonscorpion" , "demonwyvern" } },
          { "industrial",  new List<string>{ "Humvee" , "howitzer_Human" } },
          { "renaissance", new List<string>{ "treant" , "elfcannon" } },
          { "medieval",    new List<string>{ "demonscorpion" , "demonwyvern" } }
        }},
        { "aliens", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "SpaceMarine" , "Terran" , "teslatruckgun" , "atst" ,  "SpaceMarine" , "artilleryatst" , "atstsniper" } },
          { "modern",      new List<string>{ "xenolevitank" , "xenoUFO" } },
          { "industrial",  new List<string>{ "Humvee" , "howitzer_Human" } },
          { "renaissance", new List<string>{ "treant" , "elfcannon" } },
          { "medieval",    new List<string>{ "xenolevitank" , "xenoUFO" } }
        }}
      };


      public static Dictionary<string, Dictionary<string, List<string>>> HeavyOptions =
      new Dictionary<string, Dictionary<string, List<string>>>
      {
        { "human", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "P9000" , "dreadnaught" , "dreadnaught_brrt", "Railgun" , "MA9000", "HumanTitan", "AT9000" } },
          { "modern",      new List<string>{ "Tank_Human" , "MissileSystem_Human" , "wheeledtank_Human" } },
          { "industrial",  new List<string>{ "AbramTank" , "shermanww" , "tankie" , "genericwwtank" , "landship" , "bigtankww" } },
          { "renaissance", new List<string>{ "davincitank" } },
          { "medieval",    new List<string>{ "catapulta" , "batteringram" } }
        }},
        { "orc", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "P9000" , "dreadnaught" , "dreadnaught_brrt", "Railgun" , "MA9000", "HumanTitan", "AT9000" } },
          { "modern",      new List<string>{ "Tank_Ork" , "MissileSystem_Ork" , "wheeledtank_Ork" } },
          { "industrial",  new List<string>{ "AbramTank" , "shermanww" , "tankie" , "genericwwtank" , "landship" , "bigtankww" } },
          { "renaissance", new List<string>{ "davincitank" } },
          { "medieval",    new List<string>{ "orcatapulta" } }
        }},
        { "dwarf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "P9000" , "dreadnaught" , "dreadnaught_brrt", "Railgun" , "MA9000", "HumanTitan", "AT9000" } },
          { "modern",      new List<string>{ "Tank_Dwarf" , "MissileSystem_Dwarf" , "wheeledtank_Dwarf" } },
          { "industrial",  new List<string>{ "AbramTank" , "shermanww" , "tankie" , "genericwwtank" , "landship" , "bigtankww" } },
          { "renaissance", new List<string>{ "davincitank" } },
          { "medieval",    new List<string>{ "santaguin" } }
        }},
        { "elf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "P9000" , "dreadnaught" , "dreadnaught_brrt", "Railgun" , "MA9000", "HumanTitan", "AT9000" } },
          { "modern",      new List<string>{ "Tank_Gaia" , "MissileSystem_Gaia" , "wheeledtank_Gaia" } },
          { "industrial",  new List<string>{ "AbramTank" , "shermanww" , "tankie" , "genericwwtank" , "landship" , "bigtankww" } },
          { "renaissance", new List<string>{ "davincitank" } },
          { "medieval",    new List<string>{ "woolyrhino" } }
        }},
        { "demon", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "P9000" , "dreadnaught" , "dreadnaught_brrt", "Railgun" , "MA9000", "HumanTitan", "AT9000" } },
          { "modern",      new List<string>{ "demoncroc" , "demongolem" , "demonreaver" } },
          { "industrial",  new List<string>{ "AbramTank" , "shermanww" , "tankie" , "genericwwtank" , "landship" , "bigtankww" } },
          { "renaissance", new List<string>{ "davincitank" } },
          { "medieval",    new List<string>{ "demoncroc" , "demongolem" , "demonreaver" } }
        }},
        { "aliens", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "P9000" , "dreadnaught" , "dreadnaught_brrt", "Railgun" , "MA9000", "HumanTitan", "AT9000" } },
          { "modern",      new List<string>{ "xenorailgun" , "xenotripod" } },
          { "industrial",  new List<string>{ "AbramTank" , "shermanww" , "tankie" , "genericwwtank" , "landship" , "bigtankww" } },
          { "renaissance", new List<string>{ "davincitank" } },
          { "medieval",    new List<string>{ "xenorailgun" , "xenotripod" } }
        }}
      };

      public static Dictionary<string, Dictionary<string, List<string>>> SupportOptions =
      new Dictionary<string, Dictionary<string, List<string>>>
      {
        { "human", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "FutureGunship" , "supportatst" } },
          { "modern",      new List<string>{ "supporttruck_Human" } },
          { "industrial",  new List<string>{ "wwsupporttruck" } },
          { "renaissance", new List<string>{ "humanpaladin" } },
          { "medieval",    new List<string>{ "humanpaladin" } }
        }},
        { "orc", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "FutureGunship" , "supportatst" } },
          { "modern",      new List<string>{ "supporttruck_Ork" } },
          { "industrial",  new List<string>{ "wwsupporttruck" } },
          { "renaissance", new List<string>{ "orcwarlock" } },
          { "medieval",    new List<string>{ "orcwarlock" } }
        }},
        { "dwarf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "FutureGunship" , "supportatst" } },
          { "modern",      new List<string>{ "supporttruck_Dwarf" } },
          { "industrial",  new List<string>{ "wwsupporttruck" } },
          { "renaissance", new List<string>{ "dwarfdoctor" } },
          { "medieval",    new List<string>{ "dwarfdoctor" } }
        }},
        { "elf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "FutureGunship" , "supportatst" } },
          { "modern",      new List<string>{ "supporttruck_Gaia"} },
          { "industrial",  new List<string>{ "wwsupporttruck" } },
          { "renaissance", new List<string>{ "fairydragon" } },
          { "medieval",    new List<string>{ "fairydragon" } }
        }},
        { "demon", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "FutureGunship" , "supportatst" } },
          { "modern",      new List<string>{ "demonscorpion"} },
          { "industrial",  new List<string>{ "wwsupporttruck" } },
          { "renaissance", new List<string>{ "fairydragon" } },
          { "medieval",    new List<string>{ "demonscorpion" } }
        }},
        { "aliens", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "FutureGunship" , "supportatst" } },
          { "modern",      new List<string>{ "xenolevitank" , "xenoUFO"} },
          { "industrial",  new List<string>{ "wwsupporttruck" } },
          { "renaissance", new List<string>{ "fairydragon" } },
          { "medieval",    new List<string>{ "xenolevitank" , "xenoUFO"} }
        }}
      };

      public static Dictionary<string, Dictionary<string, List<string>>> AirOptions =
      new Dictionary<string, Dictionary<string, List<string>>>
      {
        { "human", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HeliELite" , "FutureGunship" , "TIEfighter" , "EliteBomber" } },
          { "modern",      new List<string>{ "Heli_Human" , "Bomber_Human" , "FighterJet_Human" , "F55FighterJet" } },
          { "industrial",  new List<string>{ "Zeppelin" , "EliteZeppelin" , "americanbomberww" , "biplane" , "fighterww" } },
          { "renaissance", new List<string>{ "balloonunit" } }
        }},
        { "orc", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HeliELite" , "FutureGunship" , "TIEfighter" , "EliteBomber" } },
          { "modern",      new List<string>{ "Heli_Ork" , "Bomber_Ork" , "FighterJet_Ork" } },
          { "industrial",  new List<string>{ "Zeppelin" , "EliteZeppelin" , "americanbomberww" , "biplane" , "fighterww" } },
          { "renaissance", new List<string>{ "orccannon", "armoredwolf" } }
        }},
        { "dwarf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HeliELite" , "FutureGunship" , "TIEfighter" , "EliteBomber" } },
          { "modern",      new List<string>{ "Heli_Dwarf" , "Bomber_Dwarf" , "FighterJet_Dwarf" } },
          { "industrial",  new List<string>{ "Zeppelin" , "EliteZeppelin" , "americanbomberww" , "biplane" , "fighterww" } },
          { "renaissance", new List<string>{ "Gunship" } }
        }},
        { "elf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HeliELite" , "FutureGunship" , "TIEfighter" , "EliteBomber" } },
          { "modern",      new List<string>{ "Heli_Gaia" , "Bomber_Gaia" , "FighterJet_Gaia" } },
          { "industrial",  new List<string>{ "Zeppelin" , "EliteZeppelin" , "americanbomberww" , "biplane" , "fighterww" } },
          { "renaissance", new List<string>{ "bigfaerydragon" } }
        }},
        { "demon", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HeliELite" , "FutureGunship" , "TIEfighter" , "EliteBomber" } },
          { "modern",      new List<string>{ "Bomber_Demon" } },
          { "industrial",  new List<string>{ "Zeppelin" , "EliteZeppelin" , "americanbomberww" , "biplane" , "fighterww" } },
          { "renaissance", new List<string>{ "Bomber_Demon" } },
          { "medieval",    new List<string>{ "Bomber_Demon" } }
        }},
        { "aliens", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HeliELite" , "FutureGunship" , "TIEfighter" , "EliteBomber" } },
          { "modern",      new List<string>{ "xenoUFObomber" , "xenoUFO" } },
          { "industrial",  new List<string>{ "Zeppelin" , "EliteZeppelin" , "americanbomberww" , "biplane" , "fighterww" } },
          { "renaissance", new List<string>{ "xenoUFObomber" , "xenoUFO" } },
          { "medieval",    new List<string>{ "xenoUFObomber" , "xenoUFO"} }
        }}
      };


      public static Dictionary<string, Dictionary<string, List<string>>> TitanOptions =
      new Dictionary<string, Dictionary<string, List<string>>>
      {
        { "human", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HumanTitanElite", "OmegaRailgun", "EliteP9000", "eliteMA9000", "eliteAT9000" } }
        }},
        { "orc", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HumanTitanElite", "OmegaRailgun", "EliteP9000", "eliteMA9000", "eliteAT9000" } }
        }},
        { "dwarf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HumanTitanElite", "OmegaRailgun", "EliteP9000", "eliteMA9000", "eliteAT9000" } }
        }},
        { "elf", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HumanTitanElite", "OmegaRailgun", "EliteP9000", "eliteMA9000", "eliteAT9000" } }
        }},
        { "demon", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HumanTitanElite", "OmegaRailgun", "EliteP9000", "eliteMA9000", "eliteAT9000" } }
        }},
        { "aliens", new Dictionary<string, List<string>> {
          { "hyperfuture",      new List<string>{ "HumanTitanElite", "OmegaRailgun", "EliteP9000", "eliteMA9000", "eliteAT9000" } }
        }}
      };
    }

    public static bool hyperfutureAllowed = false;

    public static void turnOnHyperfuture() => hyperfutureAllowed = true;
    public static void turnOffHyperfuture() => hyperfutureAllowed = false;

    public static void toggleHyperfuture() {
      Main.modifyBoolOption("HyperfutureOption",
                            PowerButtons.GetToggleValue("warhammerstuff"));
      if (PowerButtons.GetToggleValue("warhammerstuff")) {
        turnOnHyperfuture();
        return;
      }
      turnOffHyperfuture();
    }

        public static void turnOnATAT() => allowATAT = true;
    public static void turnOffATAT() => allowATAT = false;

    public static void toggleATAT() {
      Main.modifyBoolOption("ATATOption",
                            PowerButtons.GetToggleValue("atat"));
      if (PowerButtons.GetToggleValue("atat")) {
        turnOnATAT();
        return;
      }
      turnOffATAT();
    }

    public static void turnOnDread() => allowDreadnought = true;
    public static void turnOffDread() => allowDreadnought = false;

    public static void toggleDread() {
      Main.modifyBoolOption("DreadOption",
                            PowerButtons.GetToggleValue("dread"));
      if (PowerButtons.GetToggleValue("dread")) {
        turnOnDread();
        return;
      }
      turnOffDread();
    }

    public static void turnOnGunship() => allowGunship = true;
    public static void turnOffGunship() => allowGunship = false;

    public static void toggleGunship() {
      Main.modifyBoolOption("GunshipOption",
                            PowerButtons.GetToggleValue("gunship"));
      if (PowerButtons.GetToggleValue("gunship")) {
        turnOnGunship();
        return;
      }
      turnOffGunship();
    }

    public static void turnOnTIEFighter() => allowTIEFighter = true;
    public static void turnOffTIEFighter() => allowTIEFighter = false;

    public static void toggleTIEFighter() {
      Main.modifyBoolOption("TIEFighterOption",
                            PowerButtons.GetToggleValue("tiefighter"));
      if (PowerButtons.GetToggleValue("tiefighter")) {
        turnOnTIEFighter();
        return;
      }
      turnOffTIEFighter();
    }

    public static void turnOnGoliath() => allowGoliath = true;
    public static void turnOffGoliath() => allowGoliath = false;

    public static void toggleGoliath() {
      Main.modifyBoolOption("GoliathOption",
                            PowerButtons.GetToggleValue("goliath"));
      if (PowerButtons.GetToggleValue("goliath")) {
        turnOnGoliath();
        return;
      }
      turnOffGoliath();
    }

    public static void turnOnMechagodzilla() => allowMechagodzilla = true;
    public static void turnOffMechagodzilla() => allowMechagodzilla = false;

    public static void toggleMechagodzilla() {
      Main.modifyBoolOption("MechagodzillaOption",
                            PowerButtons.GetToggleValue("mechagodzilla_goliath"));
      if (PowerButtons.GetToggleValue("mechagodzilla_goliath")) {
        turnOnMechagodzilla();
        return;
      }
      turnOffMechagodzilla();
    }

    public static void turnOnEarthquaker() => allowEarthquaker = true;
    public static void turnOffEarthquaker() => allowEarthquaker = false;

    public static void toggleEarthquaker() {
      Main.modifyBoolOption("EarthquakerOption",
                            PowerButtons.GetToggleValue("earthquaker"));
      if (PowerButtons.GetToggleValue("earthquaker")) {
        turnOnEarthquaker();
        return;
      }
      turnOffEarthquaker();
    }

    public static void turnOnSpaceMarines() => allowSpaceMarines = true;
    public static void turnOffSpaceMarines() => allowSpaceMarines = false;

    public static void toggleSpaceMarines() {
      Main.modifyBoolOption("SpaceMarinesOption",
                            PowerButtons.GetToggleValue("spacemarines"));
      if (PowerButtons.GetToggleValue("spacemarines")) {
        turnOnSpaceMarines();
        return;
      }
      turnOffSpaceMarines();
    }

    private static string GetRoleType(float roll) {
      if (hyperfutureAllowed) {
        if (roll < 0.05f)
          return "titan";
        if (roll < 0.15f)
          return "support";
        if (roll < 0.20f)
          return "air";
        if (roll < 0.30f)
          return "heavy";
        return "offensive";
      } else {
        if (roll < 0.10f)
          return "support";
        if (roll < 0.20f)
          return "air";
        if (roll < 0.35f)
          return "heavy";
        return "offensive";
      }
    }

    public static bool UnitpotentialEffect(BaseSimObject pTarget,
                                           WorldTile pTile = null) {
      Actor actor = pTarget?.a;
      if (actor == null || actor.isRekt() || !actor.hasTrait("Unitpotential"))
        return false;

      bool didSomething = false;

      if (HandleCartTransformation(actor, pTile))
        didSomething = true;

      return didSomething;
    }

    public static void changeAllBuildings(string building, string newBuilding) {

      BuildingAsset newAsset = AssetManager.buildings.get(newBuilding);
      if (newAsset == null) {
        ModernBoxLogger.Error($"New building asset not found: {newBuilding}");
        return;
      }

      foreach (City city in (
                   CoreSystemManager<City, CityData>)World.world.cities) {

        List<Building> matching = city.getBuildingListOfID(building);
        if (matching == null || matching.Count == 0)
          continue;

        List<Building> toChange = new List<Building>(matching);

        foreach (Building current in toChange) {
          if (current == null)
            continue;

          WorldTile tile = current.current_tile;

          ReflectionHelper.InvokeMethod(current, "setBuilding", tile, newAsset,
                                        null);
        }
      }
    }

    public static void SetEraCity(string eraKey, string civGroup) {
      var targetEra = EraLibrary.All.FirstOrDefault(e => e.key == eraKey);
      if (targetEra == null || !targetEra.cityBuildings.ContainsKey(civGroup))
        return;

      var targetList = targetEra.cityBuildings[civGroup];
      HashSet<string> targetIds = new HashSet<string>(targetList);
      HashSet<string> targetRoles = new HashSet<string>(
          targetList.Select(id => bitch(id)).Where(role => role != "building"));

      foreach (var era in EraLibrary.All) {
        if (!era.cityBuildings.ContainsKey(civGroup))
          continue;

        foreach (var id in era.cityBuildings[civGroup]) {
          string role = bitch(id);
          if (role == "building")
            continue;

          var building = AssetManager.buildings.get(id);

          if (building == null) {
            continue;
          }

          if (!targetRoles.Contains(role)) {
            building.can_be_upgraded = false;
            continue;
          }

          building.can_be_upgraded = !targetIds.Contains(id);
        }
      }
    }

    public static bool ApplyEraOverrideToWorld(string eraKey = null) {
      var enabledEras =
          EraLibrary.All
              .Where(e => {
                return e.key switch {
                  "medieval" => StatManager.Instance.enableMedieval,
                  "renaissance" => StatManager.Instance.enableRenaissance,
                  "modern" => StatManager.Instance.enableModern,
                  "hyperfuture" => StatManager.Instance.enableHyperfuture,
                  _ => false
                };
              })
              .ToList();

      if (enabledEras.Count == 0)
        return false;

      string requestedEra = string.IsNullOrWhiteSpace(eraKey)
                                ? StatManager.Instance.eraoverride
                                : eraKey;
      requestedEra = requestedEra?.ToLowerInvariant();

      if (string.IsNullOrEmpty(requestedEra))
        return false;

      EraDefinition targetEra =
          enabledEras.FirstOrDefault(e => e.key == requestedEra) ??
          enabledEras.FirstOrDefault();

      if (targetEra == null)
        return false;

      string[] civGroups =
          new string[] { "alliance", "harden", "gaia", "horde" };

      foreach (var civGroup in civGroups) {
        SetEraCity(targetEra.key, civGroup);

        foreach (var era in EraLibrary.All) {
          if (!era.bonfires.ContainsKey(civGroup) ||
              !targetEra.bonfires.ContainsKey(civGroup))
            continue;

          foreach (var oldId in era.bonfires[civGroup]) {
            string replacement = targetEra.bonfires[civGroup][0];
            if (oldId != replacement) {
              changeAllBuildings(oldId, replacement);
            }
          }
        }

        foreach (var era in EraLibrary.All) {
          if (!era.cityBuildings.ContainsKey(civGroup) ||
              !targetEra.cityBuildings.ContainsKey(civGroup))
            continue;

          var sourceList = era.cityBuildings[civGroup];
          var targetList = targetEra.cityBuildings[civGroup];

          foreach (string oldId in sourceList) {
            string functionalKeyword = bitch(oldId);
            if (functionalKeyword == "building") {
              continue;
            }

            string replacement = targetList.FirstOrDefault(
                newId =>
                    newId.IndexOf(functionalKeyword,
                                  StringComparison.OrdinalIgnoreCase) >= 0);

            if (string.IsNullOrEmpty(replacement) || oldId == replacement) {
              continue;
            }

            changeAllBuildings(oldId, replacement);
          }
        }
      }

      StatManager.Instance.currentEra = targetEra.name;
      StatManager.Instance.currentEraDescription = targetEra.description;
      return true;
    }

    public static bool HandleCartTransformation(Actor actor,
                                                WorldTile pTile = null) {
      if (actor.asset.id != "baseWarUnit")
        return false;

      if (!actor.inMapBorder() || actor.kingdom == null || actor.city == null ||
          !actor.city.hasLeader())
        return false;

      Actor leader = actor.city.leader;
      if (leader == null || leader.asset == null)
        return false;

      string leaderId = leader.asset.id;

      string[] alliancecivs = {
        "human",          "plague_doctor", "evil_mage",
        "civ_white_mage", "civ_cat",       "civ_dog",
        "civ_chicken",    "civ_sheep",     "civ_acid_gentleman",
        "bandit"
      };
      string[] hardencivs = {
        "dwarf",         "cold_one",   "snowman",
        "civ_armadillo", "civ_rhino",  "civ_crab",
        "civ_penguin",   "civ_turtle", "civ_crystal_golem",
        "civ_candy_man", "civ_goat"
      };
      string[] gaiacivs = { "elf",           "civ_rabbit",  "civ_monkey",
                            "civ_cow",       "civ_buffalo", "civ_alpaca",
                            "civ_capybara",  "civ_frog",    "civ_liliar",
                            "druid",         "fairy",       "civ_garlic_man",
                            "civ_lemon_man", "unicorn" };
      string[] hordecivs = { "orc",        "necromancer",  "civ_fox",
                             "civ_wolf",   "civ_bear",     "civ_hyena",
                             "civ_rat",    "civ_scorpion", "civ_crocodile",
                             "civ_snake",  "civ_piranha",  "greg",
                             "jumpy_skull" };

      string[] civGroups =
          new string[] { "alliance", "harden", "gaia", "horde" };

      string civGroup = alliancecivs.Contains(leaderId) ? "alliance"
                        : hardencivs.Contains(leaderId) ? "harden"
                        : gaiacivs.Contains(leaderId)   ? "gaia"
                        : hordecivs.Contains(leaderId)  ? "horde"
                                                        : "alliance";

      Building bonfire = actor.city.getBuildingOfType("type_bonfire");
      if (bonfire == null || bonfire.asset == null)
        return false;

      int bonfireLevel = bonfire.asset.upgrade_level;
      string currentBonfireId = bonfire.asset.id;

      var enabledEras =
          EraLibrary.All
              .Where(e => {
                return e.key switch {
                  "medieval" => StatManager.Instance.enableMedieval,
                  "renaissance" => StatManager.Instance.enableRenaissance,
                  "modern" => StatManager.Instance.enableModern,
                  "hyperfuture" => StatManager.Instance.enableHyperfuture,
                  _ => false
                };
              })
              .ToList();

      if (enabledEras.Count == 0)
        return false;

      var detectedEra = EraLibrary.All.FirstOrDefault(
          e => e.bonfires.ContainsKey(civGroup) &&
               e.bonfires[civGroup].Contains(currentBonfireId));

      string overrideEra = StatManager.Instance.eraoverride?.ToLower();

      EraDefinition targetEra = null;

      if (!string.IsNullOrEmpty(overrideEra)) {
        targetEra = enabledEras.FirstOrDefault(e => e.key == overrideEra);

        if (targetEra == null) {

          int index = Mathf.Clamp(bonfireLevel, 0, enabledEras.Count - 1);
          targetEra = enabledEras[index];
        }
      } else {
        int index = Mathf.Clamp(bonfireLevel, 0, enabledEras.Count - 1);
        targetEra = enabledEras[index];
      }

      if (targetEra == null) {
        ModernBoxLogger.Error("era is null");
        return false;
      }

      foreach (var civGroupa in civGroups) {
        SetEraCity(targetEra.key, civGroupa);
      }

      bool needsFix = detectedEra == null ||
                      !enabledEras.Contains(detectedEra) ||
                      detectedEra.key != targetEra.key;

      if (needsFix) {

        foreach (var era in EraLibrary.All) {
          if (!era.bonfires.ContainsKey(civGroup))
            continue;

          foreach (var oldId in era.bonfires[civGroup]) {
            string replacement = targetEra.bonfires[civGroup][0];
            changeAllBuildings(oldId, replacement);
          }
        }

        foreach (var era in EraLibrary.All) {
          if (!era.cityBuildings.ContainsKey(civGroup) ||
              !targetEra.cityBuildings.ContainsKey(civGroup))
            continue;

          var sourceList = era.cityBuildings[civGroup];
          var targetList = targetEra.cityBuildings[civGroup];

          foreach (string oldId in sourceList) {

            string functionalKeyword = bitch(oldId);
            if (functionalKeyword == "building") {
              continue;
            }

            string replacement = targetList.FirstOrDefault(
                newId =>
                    newId.IndexOf(functionalKeyword,
                                  StringComparison.OrdinalIgnoreCase) >= 0);

            if (string.IsNullOrEmpty(replacement)) {
              continue;
            }

            if (!string.IsNullOrEmpty(replacement) && oldId != replacement) {
              changeAllBuildings(oldId, replacement);
            }
          }
        }

      }

      StatManager.Instance.currentEra = targetEra.name;
      StatManager.Instance.currentEraDescription = targetEra.description;

      string leaderSpeciesId = leader.subspecies?.data?.species_id;
      if (string.IsNullOrEmpty(leaderSpeciesId)) {
        leaderSpeciesId = leader.asset.id;
      }
      string role = GetRoleType(UnityEngine.Random.Range(0f, 1f));

      Dictionary<string, Dictionary<string, List<string>>> roleMapDict =
          targetEra.key switch {
            "medieval" => CartTransformations.CartTransformationsMedievalRoles,
            "renaissance" =>
                CartTransformations.CartTransformationsRenaissanceRoles,
            "modern" => CartTransformations.CartTransformationsModernRoles,
            "hyperfuture" =>
                CartTransformations.CartTransformationsHyperFutureRoles,
            _ => null
          };

      if (roleMapDict == null)
        return false;

      if (!roleMapDict.TryGetValue(leaderSpeciesId, out var roleMap))
        return false;

      if (!roleMap.TryGetValue(role, out var candidates) ||
          candidates.Count == 0)
        return false;

        var filteredCandidates = SuckDick(candidates);

        if (filteredCandidates.Count == 0)
            return false;

      string newActorId = PickValidCartCandidate(filteredCandidates, targetEra.key,
                                                 civGroup, role,
                                                 leaderSpeciesId);
      if (string.IsNullOrEmpty(newActorId))
        return false;

      Actor transformed = World.world.units.createNewUnit(
          newActorId, actor.current_tile, pMiracleSpawn: false, 0f, null, null,
          pSpawnWithItems: false);

      if (transformed == null)
        return false;

      transformed.setKingdom(actor.kingdom);
      transformed.setCity(actor.city);

      EffectsLibrary.spawn("fx_spawn", transformed.current_tile);
      ActionLibrary.removeUnit(actor);
      actor.setTransformed();

      return true;
    }

        private static List<string> SuckDick(List<string> units) {
          List<string> filtered = new List<string>();

          foreach (var unit in units) {
              if (!allowATAT && (unit.Contains("atst") || unit.Contains("AT9000")))
                  continue;

              if (!allowDreadnought && unit.Contains("dreadnaught"))
                  continue;

              if (!allowGunship && (unit.Contains("Gunship") || unit.Contains("Heli") || unit.Contains("bomber")))
                  continue;

              if (!allowTIEFighter && unit.Contains("TIEfighter"))
                  continue;

              filtered.Add(unit);
          }

          return filtered;
      }

    private static void TransformUnit(Actor originalActor, string newActorId,
                                      WorldTile pTile) {
      Actor newActor = World.world.units.createNewUnit(newActorId, pTile);
      if (newActor == null) {
        return;
      }
      ActorTool.copyUnitToOtherUnit(originalActor, newActor);
      if (originalActor.kingdom != null) {
        newActor.setKingdom(originalActor.kingdom);
      }
      newActor.setCity(originalActor.city);
      ActionLibrary.removeUnit(originalActor);
    }

    private static string bitch(string id) {
      string lowerId = id.ToLower();
      if (lowerId.Contains("house"))
        return "house";
      if (lowerId.Contains("barracks"))
        return "barracks";
      if (lowerId.Contains("tower") || lowerId.Contains("watch"))
        return "tower";
      if (lowerId.Contains("hall") || lowerId.Contains("bonfire"))
        return "hall";
      if (lowerId.Contains("dock"))
        return "dock";
      if (lowerId.Contains("mine"))
        return "mine";
      if (lowerId.Contains("market"))
        return "market";

      return "building";  

    }

    public static void toggleVehicles() {
      Main.modifyBoolOption("FactoriesOption",
                            PowerButtons.GetToggleValue("vehicle_toggle"));
      if (PowerButtons.GetToggleValue("vehicle_toggle")) {
        turnOnVehicles();
        return;
      }
      turnOffVehicles();
    }

    public static void turnOnVehicles() {
      vehiclesAllowed = true;
    }

    public static void turnOffVehicles() {
      vehiclesAllowed = false;
    }

    public static bool VehicleSummonEffect(BaseSimObject pTarget,
                                           WorldTile pTile = null) {
      if (pTarget == null || !pTarget.isActor())
        return false;

      if (!vehiclesAllowed)
        return false;

      Actor caster = pTarget.a;
      if (!caster.isAlive() || !caster.isCityLeader() ||
          !caster.kingdom.isCiv() || !caster.hasCity())
        return false;

      City city = caster.city;
      if (!city.hasBuildingType("type_hall"))
        return false;

      int pop = city.getPopulationPeople();
      int vehicleLimit = 0;
      if (pop >= 200)
        vehicleLimit = 25;
      else if (pop >= 120)
        vehicleLimit = 20;
      else if (pop >= 80)
        vehicleLimit = 15;
      else if (pop >= 50)
        vehicleLimit = 10;
      else if (pop >= 30)
        vehicleLimit = 8;
      else if (pop >= 20)
        vehicleLimit = 5;
      else
        return false;

      int currentVehicles = 0;
      foreach (Actor unit in city.units) {
        if (unit.hasTrait("Unitpotential"))
          currentVehicles++;
      }

      if (currentVehicles >= vehicleLimit)
        return false;

      if (pTile == null)
        pTile = caster.current_tile;

      WorldTile spawnTile = pTile?.region?.tiles.GetRandom();
      if (spawnTile == null || spawnTile.Type.block || spawnTile.Type.ocean)
        return false;

      Actor vehicle = World.world.units.createNewUnit(
          "baseWarUnit", spawnTile, pMiracleSpawn: false, 0f, null, null,
          pSpawnWithItems: false);

      if (vehicle == null)
        return false;

      vehicle.makeWait(1f);
      vehicle.setKingdom(caster.kingdom);
      vehicle.setCity(city);
      return true;
    }

    [HarmonyPatch(typeof(KingdomBehCheckKing), "execute")]
    public static class KingdomBehCheckKing_IdeologyPatch {
      static void Postfix(Kingdom pKingdom) {
        if (pKingdom == null || !pKingdom.hasKing())
          return;

        Actor king = pKingdom.king;
        if (king == null || !king.isAlive())
          return;

        if (king.city != null) {
          Building bonfire = king.city.getBuildingOfType("type_bonfire");
          if (bonfire != null && bonfire.asset != null &&
              bonfire.asset.upgrade_level >= 2) {
            if (king.subspecies != null &&
                !king.subspecies.hasTrait("adaptation_wasteland")) {
              king.subspecies.addTrait("adaptation_wasteland");
            }
          }
        }

        string[] commonIdeologies =
            new string[] { "Peoplewoven", "Martial", "Mercantile", "Dynastic" };

        bool kingHasIdeology = false;
        foreach (string trait in commonIdeologies) {
          if (king.hasTrait(trait)) {
            kingHasIdeology = true;
            break;
          }
        }

        bool kingHasChaosvolt = king.hasTrait("Chaosvolt");
        if (kingHasChaosvolt)
          kingHasIdeology = true;

        if (!kingHasIdeology) {
          string chosenIdeology =
              commonIdeologies[Randy.randomInt(0, commonIdeologies.Length)];
          king.addTrait(chosenIdeology);
          kingHasIdeology = true;
        }

        string kingIdeology = null;
        foreach (string trait in commonIdeologies) {
          if (king.hasTrait(trait)) {
            kingIdeology = trait;
            break;
          }
        }

        if (string.IsNullOrEmpty(kingIdeology) && kingHasChaosvolt)
          kingIdeology = "Chaosvolt";

        if (string.IsNullOrEmpty(kingIdeology))
          return;

        List<Actor> kingdomUnits = pKingdom.units;

        foreach (Actor actor in kingdomUnits) {
          if (actor == null || !actor.isAlive() || actor == king)
            continue;

          bool actorHasIdeology = false;
          foreach (string trait in commonIdeologies) {
            if (actor.hasTrait(trait)) {
              actorHasIdeology = true;
              break;
            }
          }

          if (actor.hasTrait("Chaosvolt"))
            actorHasIdeology = true;

          if (kingHasChaosvolt) {
            if (!actor.hasTrait("Chaosvolt")) {
              if (!actorHasIdeology) {
                actor.addTrait("Chaosvolt");
              } else if (Randy.randomChance(0.4f)) {
                actor.addTrait("Chaosvolt");
              }
            }
          } else {
            if (!actorHasIdeology) {
              actor.addTrait(kingIdeology);
            } else if (!actor.hasTrait(kingIdeology) &&
                       Randy.randomChance(0.4f)) {
              actor.addTrait(kingIdeology);
            }
          }
        }
      }
    }

    public static void addTraitToLocalizedLibrary(string id,
                                                  string description) {
      string language = Reflection.GetField(
          LocalizedTextManager.instance.GetType(),
          LocalizedTextManager.instance, "language") as string;
      Dictionary<string, string> localizedText =
          Reflection.GetField(LocalizedTextManager.instance.GetType(),
                              LocalizedTextManager.instance, "localizedText")
              as Dictionary<string, string>;
      localizedText.Add("trait_" + id, id);
      localizedText.Add("trait_" + id + "_info", description);
    }
  }
}
