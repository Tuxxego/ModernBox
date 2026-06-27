using System;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using life;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Config;
using System.Reflection;
using UnityEngine.Tilemaps;
using System.IO;
 
namespace ModernBox
{
    public static class Ideologies
    {
		
		public static List<string> IdeologiesID = new List<string>();

        //Traits
        public static void init()
        { 
 

            IdeologiesID.Add("Capitalist");
            IdeologiesID.Add("Communist");
            IdeologiesID.Add("Liberal");
            IdeologiesID.Add("Conservative");
            IdeologiesID.Add("Fascist");
            IdeologiesID.Add("Democratic");
            IdeologiesID.Add("Technocrat");
            IdeologiesID.Add("Luddite");
            IdeologiesID.Add("Environmental Steward");
            IdeologiesID.Add("Anarchist");
            IdeologiesID.Add("Primalism");
			
			string[] allIdeologies = { "Capitalist", "Communist", "Liberal", "Conservative", "Fascist", "Democratic", "Technocrat", "Luddite", "Environmental Steward", "Anarchist", "Primalism" };

      ActorTrait capital = new ActorTrait {
        id = "Capitalist",
        path_icon = "ui/icons/Capitalist",
        birth = 37f,
        inherit = 100f,
        can_be_given = true,
        group_id = "IdeologyBox"
      };
	  capital.base_stats[S.fertility] = 0.0f;
      capital.base_stats[S.max_children] = 0f;
      capital.base_stats[S.max_age] = 0f;
      capital.base_stats[S.attack_speed] = 0;
      capital.base_stats[S.damage] = 0;
      capital.base_stats[S.speed] = 0f;
      capital.base_stats[S.health] = 0;
      capital.base_stats[S.accuracy] = 0f;
      capital.base_stats[S.range] = 0;
      capital.base_stats[S.armor] = 0;
      capital.base_stats[S.scale] = 0.0f;
      capital.base_stats[S.dodge] = 0f;
      capital.base_stats[S.targets] = 0f;
      capital.base_stats[S.critical_chance] = 0.0f;
      capital.base_stats[S.knockback] = 0f;
      capital.base_stats[S.knockback_reduction] = 0f;
      capital.base_stats[S.intelligence] = 0;
      capital.base_stats[S.warfare] = 0;
      capital.base_stats[S.diplomacy] = 5;
      capital.base_stats[S.stewardship] = 0;
      capital.base_stats[S.opinion] = 0f;
      capital.base_stats[S.loyalty_traits] = 0f;
      capital.base_stats[S.cities] = 3;
      capital.base_stats[S.zone_range] = 0;
	  string[] oppositeIdeologyCapi = { "Communist"};
	  capital.oppositeArr = allIdeologies.Except(new[] { "Capitalist" }).ToArray();
      AssetManager.traits.add(capital);
	  addIdeologyToLocalizedLibrary(capital.id, "He likes capitalism.");
      PlayerConfig.unlockTrait(capital.id);

      ActorTrait commie = new ActorTrait {
        id = "Communist",
        path_icon = "ui/icons/Communist",
        birth = 37f,
        inherit = 100f,
        can_be_given = true,
        group_id = "IdeologyBox"
      };
         commie.base_stats[S.fertility] = 0.0f;
         commie.base_stats[S.max_children] = 0f;
         commie.base_stats[S.max_age] = 0f;
         commie.base_stats[S.attack_speed] = 0;
         commie.base_stats[S.damage] = 0;
         commie.base_stats[S.speed] = 0f;
         commie.base_stats[S.health] = 0;
         commie.base_stats[S.accuracy] = 0f;
         commie.base_stats[S.range] = 0;
         commie.base_stats[S.armor] = 0;
         commie.base_stats[S.scale] = 0.0f;
         commie.base_stats[S.dodge] = 0f;
         commie.base_stats[S.targets] = 0f;
         commie.base_stats[S.critical_chance] = 0.0f;
         commie.base_stats[S.knockback] = 0f;
         commie.base_stats[S.knockback_reduction] = 0f;
         commie.base_stats[S.intelligence] = 0;
         commie.base_stats[S.warfare] = 7;
         commie.base_stats[S.diplomacy] = 1;
         commie.base_stats[S.stewardship] = 0;
         commie.base_stats[S.opinion] = 0f;
         commie.base_stats[S.loyalty_traits] = 0f;
         commie.base_stats[S.cities] = 8;
         commie.base_stats[S.zone_range] = 0;
		 string[] oppositeIdeologyCommie = { "Capitalist", "Conservative" };
		 commie.oppositeArr = allIdeologies.Except(new[] { "Communist" }).ToArray();
      AssetManager.traits.add(commie);
	  addIdeologyToLocalizedLibrary(commie.id, "He likes communism.");
      PlayerConfig.unlockTrait(commie.id);
	  
	    ActorTrait lib = new ActorTrait {
        id = "Liberal",
        path_icon = "ui/icons/Liberal",
        birth = 37f,
        inherit = 100f,
        can_be_given = true,
        group_id = "IdeologyBox"
      };
         lib.base_stats[S.fertility] = 0.0f;
         lib.base_stats[S.max_children] = 0f;
         lib.base_stats[S.max_age] = 0f;
         lib.base_stats[S.attack_speed] = 0;
         lib.base_stats[S.damage] = 0;
         lib.base_stats[S.speed] = 0f;
         lib.base_stats[S.health] = 0;
         lib.base_stats[S.accuracy] = 0f;
         lib.base_stats[S.range] = 0;
         lib.base_stats[S.armor] = 0;
         lib.base_stats[S.scale] = 0.0f;
         lib.base_stats[S.dodge] = 0f;
         lib.base_stats[S.targets] = 0f;
         lib.base_stats[S.critical_chance] = 0.0f;
         lib.base_stats[S.knockback] = 0f;
         lib.base_stats[S.knockback_reduction] = 0f;
         lib.base_stats[S.intelligence] = 5;
         lib.base_stats[S.warfare] = 0;
         lib.base_stats[S.diplomacy] = 1;
         lib.base_stats[S.stewardship] = 4;
         lib.base_stats[S.opinion] = 2f;
         lib.base_stats[S.loyalty_traits] = 0f;
         lib.base_stats[S.cities] = 2;
         lib.base_stats[S.zone_range] = 0;
		 string[] oppositeIdeologyLib = { "Conservative", "Facist" };
		 lib.oppositeArr = allIdeologies.Except(new[] { "Liberal" }).ToArray();
      AssetManager.traits.add(lib);
      addIdeologyToLocalizedLibrary(lib.id, "Private properties and a market economy are the dream for this person.");
      PlayerConfig.unlockTrait(lib.id);
	  
	  	ActorTrait cons = new ActorTrait {
        id = "Conservative",
        path_icon = "ui/icons/Conservative",
        birth = 37f,
        inherit = 100f,
        can_be_given = true,
        group_id = "IdeologyBox"
      };
         cons.base_stats[S.fertility] = 0.0f;
         cons.base_stats[S.max_children] = 0f;
         cons.base_stats[S.max_age] = 0f;
         cons.base_stats[S.attack_speed] = 0;
         cons.base_stats[S.damage] = 0;
         cons.base_stats[S.speed] = 0f;
         cons.base_stats[S.health] = 0;
         cons.base_stats[S.accuracy] = 0f;
         cons.base_stats[S.range] = 0;
         cons.base_stats[S.armor] = 0;
         cons.base_stats[S.scale] = 0.0f;
         cons.base_stats[S.dodge] = 0f;
         cons.base_stats[S.targets] = 0f;
         cons.base_stats[S.critical_chance] = 0.0f;
         cons.base_stats[S.knockback] = 0f;
         cons.base_stats[S.knockback_reduction] = 0f;
         cons.base_stats[S.intelligence] = 5;
         cons.base_stats[S.warfare] = 3;
         cons.base_stats[S.diplomacy] = 1;
         cons.base_stats[S.stewardship] = 4;
         cons.base_stats[S.opinion] = 0f;
         cons.base_stats[S.loyalty_traits] = 0f;
         cons.base_stats[S.cities] = 2;
         cons.base_stats[S.zone_range] = 0;
		 string[] oppositeIdeologyCon = { "Liberal" };
		 cons.oppositeArr = allIdeologies.Except(new[] { "Conservative" }).ToArray();
      AssetManager.traits.add(cons);
         addIdeologyToLocalizedLibrary(cons.id, "Traditional values are the dream for this person.");
      PlayerConfig.unlockTrait(cons.id);
	  
	  	  	ActorTrait fac = new ActorTrait {
        id = "Fascist",
        path_icon = "ui/icons/Facist",
        birth = 37f,
        inherit = 100f,
        can_be_given = true,
        group_id = "IdeologyBox"
      };
         fac.base_stats[S.fertility] = 0.0f;
         fac.base_stats[S.max_children] = 0f;
         fac.base_stats[S.max_age] = 0f;
         fac.base_stats[S.attack_speed] = 0;
         fac.base_stats[S.damage] = 0;
         fac.base_stats[S.speed] = 0f;
         fac.base_stats[S.health] = 0;
         fac.base_stats[S.accuracy] = 0f;
         fac.base_stats[S.range] = 0;
         fac.base_stats[S.armor] = 0;
         fac.base_stats[S.scale] = 0.0f;
         fac.base_stats[S.dodge] = 0f;
         fac.base_stats[S.targets] = 0f;
         fac.base_stats[S.critical_chance] = 0.0f;
         fac.base_stats[S.knockback] = 0f;
         fac.base_stats[S.knockback_reduction] = 0f;
         fac.base_stats[S.intelligence] = 5;
         fac.base_stats[S.warfare] = 15;
         fac.base_stats[S.diplomacy] = -7;
         fac.base_stats[S.stewardship] = 4;
         fac.base_stats[S.opinion] = -2f;
         fac.base_stats[S.loyalty_traits] = 0f;
         fac.base_stats[S.cities] = 12;
         fac.base_stats[S.zone_range] = 0;
		 string[] oppositeIdeologyFac = { "Democratic" };
		 fac.oppositeArr = allIdeologies.Except(new[] { "Fascist" }).ToArray();
      AssetManager.traits.add(fac);
         addIdeologyToLocalizedLibrary(fac.id, "An eternal leader is the dream for this person.");
		 PlayerConfig.unlockTrait(fac.id);
	  

		ActorTrait dem = new ActorTrait {
        id = "Democratic",
        path_icon = "ui/icons/Democratic",
        birth = 37f,
        inherit = 100f,
        can_be_given = true,
        group_id = "IdeologyBox"
      };
         dem.base_stats[S.fertility] = 0.0f;
         dem.base_stats[S.max_children] = 0f;
         dem.base_stats[S.max_age] = 0f;
         dem.base_stats[S.attack_speed] = 0;
         dem.base_stats[S.damage] = 0;
         dem.base_stats[S.speed] = 0f;
         dem.base_stats[S.health] = 0;
         dem.base_stats[S.accuracy] = 0f;
         dem.base_stats[S.range] = 0;
         dem.base_stats[S.armor] = 0;
         dem.base_stats[S.scale] = 0.0f;
         dem.base_stats[S.dodge] = 0f;
         dem.base_stats[S.targets] = 0f;
         dem.base_stats[S.critical_chance] = 0.0f;
         dem.base_stats[S.knockback] = 0f;
         dem.base_stats[S.knockback_reduction] = 0f;
         dem.base_stats[S.intelligence] = 5;
         dem.base_stats[S.warfare] = 20;
         dem.base_stats[S.diplomacy] = 15;
         dem.base_stats[S.stewardship] = 15;
         dem.base_stats[S.opinion] = 20f;
         dem.base_stats[S.loyalty_traits] = 20f;
         dem.base_stats[S.cities] = 5;
         dem.base_stats[S.zone_range] = 6;
		 string[] oppositeIdeologyDem = { "Facist", "Democratic"};
		 dem.oppositeArr = allIdeologies.Except(new[] { "Democratic" }).ToArray();
      AssetManager.traits.add(dem);
         addIdeologyToLocalizedLibrary(dem.id, "A government run by the people is the dream for this person.");
		 PlayerConfig.unlockTrait(dem.id);
	  

		 
		ActorTrait technocrat = new ActorTrait {
			id = "Technocrat",
			path_icon = "ui/icons/Technocrat",
			birth = 37f,
			inherit = 100f,
			can_be_given = true,
			group_id = "IdeologyBox"
		};
		technocrat.base_stats[S.fertility] = 0.0f;
		technocrat.base_stats[S.max_children] = 0f;
		technocrat.base_stats[S.max_age] = 0f;
		technocrat.base_stats[S.attack_speed] = 0;
		technocrat.base_stats[S.damage] = 0;
		technocrat.base_stats[S.speed] = 0f;
		technocrat.base_stats[S.health] = 0;
		technocrat.base_stats[S.accuracy] = 0f;
		technocrat.base_stats[S.range] = 0;
		technocrat.base_stats[S.armor] = 0;
		technocrat.base_stats[S.scale] = 0.0f;
		technocrat.base_stats[S.dodge] = 0f;
		technocrat.base_stats[S.targets] = 0f;
		technocrat.base_stats[S.critical_chance] = 0.0f;
		technocrat.base_stats[S.knockback] = 0f;
		technocrat.base_stats[S.knockback_reduction] = 0f;
		technocrat.base_stats[S.intelligence] = 10;
		technocrat.base_stats[S.warfare] = 2;
		technocrat.base_stats[S.diplomacy] = 5;
		technocrat.base_stats[S.stewardship] = 7;
		technocrat.base_stats[S.opinion] = 5f;
		technocrat.base_stats[S.loyalty_traits] = 0f;
		technocrat.base_stats[S.cities] = 4;
		technocrat.base_stats[S.zone_range] = 0;
		string[] oppositeIdeologyTechno = { "Luddite" };
		technocrat.oppositeArr = allIdeologies.Except(new[] { "Technocrat" }).ToArray();
		AssetManager.traits.add(technocrat);
		addIdeologyToLocalizedLibrary(technocrat.id, "Believes in governance by experts and technological advancement.");
		PlayerConfig.unlockTrait(technocrat.id);
				 

		ActorTrait luddite = new ActorTrait {
			id = "Luddite",
			path_icon = "ui/icons/Luddite",
			birth = 37f,
			inherit = 100f,
			can_be_given = true,
			group_id = "IdeologyBox"
		};
		luddite.base_stats[S.fertility] = 0.0f;
		luddite.base_stats[S.max_children] = 0f;
		luddite.base_stats[S.max_age] = 0f;
		luddite.base_stats[S.attack_speed] = 0;
		luddite.base_stats[S.damage] = 0;
		luddite.base_stats[S.speed] = 0f;
		luddite.base_stats[S.health] = 0;
		luddite.base_stats[S.accuracy] = 0f;
		luddite.base_stats[S.range] = 0;
		luddite.base_stats[S.armor] = 0;
		luddite.base_stats[S.scale] = 0.0f;
		luddite.base_stats[S.dodge] = 0f;
		luddite.base_stats[S.targets] = 0f;
		luddite.base_stats[S.critical_chance] = 0.0f;
		luddite.base_stats[S.knockback] = 0f;
		luddite.base_stats[S.knockback_reduction] = 0f;
		luddite.base_stats[S.intelligence] = -5;
		luddite.base_stats[S.warfare] = -3;
		luddite.base_stats[S.diplomacy] = -5;
		luddite.base_stats[S.stewardship] = -7;
		luddite.base_stats[S.opinion] = -5f;
		luddite.base_stats[S.loyalty_traits] = 0f;
		luddite.base_stats[S.cities] = 1;
		luddite.base_stats[S.zone_range] = 0;
		string[] oppositeIdeologyLuddite = { "Technocrat" };
		luddite.oppositeArr = allIdeologies.Except(new[] { "Luddite" }).ToArray();
		AssetManager.traits.add(luddite);
		addIdeologyToLocalizedLibrary(luddite.id, "Rejects technology and advocates for a simpler, non-technological lifestyle.");
		PlayerConfig.unlockTrait(luddite.id);
				 

		ActorTrait environmentalist = new ActorTrait {
			id = "Environmental Steward",
			path_icon = "ui/icons/EnvironmentalSteward",
			birth = 37f,
			inherit = 100f,
			can_be_given = true,
			group_id = "IdeologyBox"
		};
		environmentalist.base_stats[S.fertility] = 0.0f;
		environmentalist.base_stats[S.max_children] = 0f;
		environmentalist.base_stats[S.max_age] = 0f;
		environmentalist.base_stats[S.attack_speed] = 0;
		environmentalist.base_stats[S.damage] = 0;
		environmentalist.base_stats[S.speed] = 0f;
		environmentalist.base_stats[S.health] = 0;
		environmentalist.base_stats[S.accuracy] = 0f;
		environmentalist.base_stats[S.range] = 0;
		environmentalist.base_stats[S.armor] = 0;
		environmentalist.base_stats[S.scale] = 0.0f;
		environmentalist.base_stats[S.dodge] = 0f;
		environmentalist.base_stats[S.targets] = 0f;
		environmentalist.base_stats[S.critical_chance] = 0.0f;
		environmentalist.base_stats[S.knockback] = 0f;
		environmentalist.base_stats[S.knockback_reduction] = 0f;
		environmentalist.base_stats[S.intelligence] = 3;
		environmentalist.base_stats[S.warfare] = -5;
		environmentalist.base_stats[S.diplomacy] = 5;
		environmentalist.base_stats[S.stewardship] = 10;
		environmentalist.base_stats[S.opinion] = 5f;
		environmentalist.base_stats[S.loyalty_traits] = 0f;
		environmentalist.base_stats[S.cities] = 2;
		environmentalist.base_stats[S.zone_range] = 0;
		environmentalist.oppositeArr = allIdeologies.Except(new[] { "Environmental Steward" }).ToArray();
		AssetManager.traits.add(environmentalist);
		addIdeologyToLocalizedLibrary(environmentalist.id, "Prioritizes the protection of the environment and sustainable practices.");
		PlayerConfig.unlockTrait(environmentalist.id);
				 

		ActorTrait anarchist = new ActorTrait {
			id = "Anarchist",
			path_icon = "ui/icons/Anarchist",
			birth = 37f,
			inherit = 100f,
			can_be_given = true,
			group_id = "IdeologyBox"
		};
		anarchist.base_stats[S.fertility] = 0.0f;
		anarchist.base_stats[S.max_children] = 0f;
		anarchist.base_stats[S.max_age] = 0f;
		anarchist.base_stats[S.attack_speed] = 0;
		anarchist.base_stats[S.damage] = 0;
		anarchist.base_stats[S.speed] = 0f;
		anarchist.base_stats[S.health] = 0;
		anarchist.base_stats[S.accuracy] = 0f;
		anarchist.base_stats[S.range] = 0;
		anarchist.base_stats[S.armor] = 0;
		anarchist.base_stats[S.scale] = 0.0f;
		anarchist.base_stats[S.dodge] = 0f;
		anarchist.base_stats[S.targets] = 0f;
		anarchist.base_stats[S.critical_chance] = 0.0f;
		anarchist.base_stats[S.knockback] = 0f;
		anarchist.base_stats[S.knockback_reduction] = 0f;
		anarchist.base_stats[S.intelligence] = -3;
		anarchist.base_stats[S.warfare] = 5;
		anarchist.base_stats[S.diplomacy] = -100;
		anarchist.base_stats[S.stewardship] = 10;
		anarchist.base_stats[S.opinion] = -100f;
		anarchist.base_stats[S.loyalty_traits] = 0f;
		anarchist.base_stats[S.cities] = -1000;
		anarchist.base_stats[S.zone_range] = 0;
		anarchist.oppositeArr = allIdeologies.Except(new[] { "Anarchist" }).ToArray();
		AssetManager.traits.add(anarchist);
		addIdeologyToLocalizedLibrary(anarchist.id, "Hates goverments.");
		PlayerConfig.unlockTrait(anarchist.id);
				 		 
		ActorTrait primalism = new ActorTrait {
			id = "Primalism",
			path_icon = "ui/icons/Primalism",
			birth = 37f,
			inherit = 100f,
			can_be_given = true,
			group_id = "IdeologyBox"
		};
		primalism.base_stats[S.fertility] = 0.0f;
		primalism.base_stats[S.max_children] = 99999999999f;
		primalism.base_stats[S.max_age] = 0f;
		primalism.base_stats[S.attack_speed] = 0;
		primalism.base_stats[S.damage] = 0;
		primalism.base_stats[S.speed] = 0f;
		primalism.base_stats[S.health] = 0;
		primalism.base_stats[S.accuracy] = 0f;
		primalism.base_stats[S.range] = 0;
		primalism.base_stats[S.armor] = 0;
		primalism.base_stats[S.scale] = 0.0f;
		primalism.base_stats[S.dodge] = 0f;
		primalism.base_stats[S.targets] = 0f;
		primalism.base_stats[S.critical_chance] = 0.0f;
		primalism.base_stats[S.knockback] = 0f;
		primalism.base_stats[S.knockback_reduction] = 0f;
		primalism.base_stats[S.intelligence] = -99999999999999999;
		primalism.base_stats[S.warfare] = 99999999;
		primalism.base_stats[S.diplomacy] = -100;
		primalism.base_stats[S.stewardship] = 10;
		primalism.base_stats[S.opinion] = -100f;
		primalism.base_stats[S.loyalty_traits] = 0f;
		primalism.base_stats[S.cities] = -99999999;
		primalism.base_stats[S.zone_range] = 0;
		primalism.oppositeArr = allIdeologies.Except(new[] { "Primalism" }).ToArray();
		AssetManager.traits.add(primalism);
		addIdeologyToLocalizedLibrary(primalism.id, "Why tf would anyone want this?"); // i should change description
		PlayerConfig.unlockTrait(primalism.id);
		
		}
 
	
		public static void toggleIdeologies()
        {
            Main.modifyBoolOption("IdeologiesOption", PowerButtons.GetToggleValue("Ideologies_toggle"));
            if (PowerButtons.GetToggleValue("Ideologies_toggle"))
            {
                turnOnIdeologies();
                return;
            }
            turnOffIdeologies();
        }
			public static void turnOnIdeologies()
			{

            // Iterate through ideologies and perform actions
            foreach (string ideologyID in IdeologiesID)
            {
                ActorTrait ideology = AssetManager.traits.get(ideologyID);
                if (ideology != null)
                {
                    ideology.birth = 37f;
                }
            }				

            }
        public static void turnOffIdeologies()
        {
            // Iterate through ideologies and perform actions
            foreach (string ideologyID in IdeologiesID)
            {
                ActorTrait ideology = AssetManager.traits.get(ideologyID);
                if (ideology != null)
                {
                    ideology.birth = 0f;
                }
            }
        }
        public static void addIdeologyToLocalizedLibrary(string id, string description)
        {
        string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
        Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
        localizedText.Add("trait_" + id, id);
        localizedText.Add("trait_" + id + "_info", description);
        }
    }
}
