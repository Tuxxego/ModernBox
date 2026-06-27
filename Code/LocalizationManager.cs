using System;
using NCMS.Utils;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.IO.Compression;

namespace ModernBox
{
    public class LocalizationManager : MonoBehaviour
    {
        
		public enum Language
		{
			English,
			Spanish,
			French,
			German,
			Japanese,
			Russian,      
			Chinese,      
			Korean,       
			Portuguese,   
			Turkish,
			Boat,		
		}


		
		
		private readonly Dictionary<string, Language> languageCodeMap = new Dictionary<string, Language>()
		{
			{ "en", Language.English },
			{ "es", Language.Spanish },
			{ "fr", Language.French },
			{ "de", Language.German },
			{ "ja", Language.Japanese },
			{ "ru", Language.Russian },     
			{ "zh", Language.Chinese },     
			{ "ko", Language.Korean },      
			{ "pt", Language.Portuguese },  
			{ "tr", Language.Turkish },  
			{ "boat", Language.Boat }      
			
		};



        
        public Language currentLanguage = Language.Spanish;

        
        private Dictionary<Language, Dictionary<string, string>> localizationDatabase;

        
        void Start()
        {
            ModernBoxLogger.Log("Localization Manager started.");
            LoadLanguageFromFile();
            InitializeLocalizationDatabase();
			
        }

    
    private void LoadLanguageFromFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "language.txt");
        if (File.Exists(filePath))
        {
            try
            {
                string savedLanguageCode = File.ReadAllText(filePath).Trim();
                if (languageCodeMap.TryGetValue(savedLanguageCode, out Language parsedLanguage))
                {
                    currentLanguage = parsedLanguage;
                    ModernBoxLogger.Log($"Language loaded from file: {currentLanguage}");
                }
                else
                {
                    ModernBoxLogger.Warning($"Invalid language code in file: {savedLanguageCode}");
                }
            }
            catch (Exception ex)
            {
                ModernBoxLogger.Error($"Failed to load language from file: {ex.Message}");
            }
        }
        else
        {
            ModernBoxLogger.Warning("Language file not found. Defaulting to English.");
        }
    }
		
        private bool isLanguageMenuVisible = false; 

        
        public void ShowLanguageMenu()
        {
            isLanguageMenuVisible = true;
            ModernBoxLogger.Log("Language menu displayed.");
        }

        
        public void SetLanguage(Language newLanguage)
        {
            if (!localizationDatabase.ContainsKey(newLanguage))
            {
                ModernBoxLogger.Error($"Attempted to set unsupported language: {newLanguage}");
                return;
            }

            currentLanguage = newLanguage;
            ModernBoxLogger.Log($"Language set to: {newLanguage}");

            
            isLanguageMenuVisible = false;
            ModernBoxLogger.Log("Language menu closed after selection.");
        }

        
        public string Localize(string key)
        {
            if (localizationDatabase[currentLanguage].TryGetValue(key, out string value))
            {
             
                return value;
            }

            ModernBoxLogger.Warning($"Localization key not found: {key} for language: {currentLanguage}");
            return key; 
        }

        
        private void InitializeLocalizationDatabase()
        {
            localizationDatabase = new Dictionary<Language, Dictionary<string, string>>();

            try
            {
localizationDatabase[Language.English] = new Dictionary<string, string>
{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

localizationDatabase[Language.Spanish] = new Dictionary<string, string>
{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

localizationDatabase[Language.French] = new Dictionary<string, string>
{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};



				localizationDatabase[Language.German] = new Dictionary<string, string>
				{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

				localizationDatabase[Language.Turkish] = new Dictionary<string, string>
				{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

				localizationDatabase[Language.Russian] = new Dictionary<string, string>
				{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

				localizationDatabase[Language.Chinese] = new Dictionary<string, string>
				{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

				localizationDatabase[Language.Korean] = new Dictionary<string, string>
				{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

				localizationDatabase[Language.Portuguese] = new Dictionary<string, string>
				{
    { "stars_loading_message", "Stars are generating. This may take some time. Your game will stop responding; this is normal." },
    { "did_you_know", "DID YOU KNOW?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "Stars: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "Stars: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "No galaxy selected." },
    { "name", "Name: {0}" },
    { "type", "Type: {0}" },
    { "planets", "Planets: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "Select Galaxy" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "Do you want to view the tutorial?" },
    { "apply", "Apply" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "If you hover over stars (the colored dots), you'll see a brief pop up with it's name, if you click on it, you'll get an info popup." },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner." },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where your save starts. Crabby Way is mostly stable, and it has a large variety of habitable and inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Krummple is a mostly uninhabited galaxy, but stocked with systems that contain habitable planets." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "Glass is a spiral galaxy with seven arms, but through the middle, a rough squiggly line separates some of the arms from each other, like something massive came through and tore a giant hole through, like it's shattered like glass." },
    { "centuga", "Centuga was once a very powerful galaxy controlled by an empire known as the Universal Order, but during a great war the empire was destroyed, but pockets of it remain and ruins scatter planets." },
    { "dank", "Dank galaxy is a dark and ominous place, shrouded in mystery." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};

				localizationDatabase[Language.Boat] = new Dictionary<string, string>
				{
    { "stars_loading_message", "ARRRRRG YER STARS ARE GENERATIN!!" },
    { "did_you_know", "DID YA KNO?" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "BIG BALLS: ???" },
    { "danger_unknown", "Danger: ???/5" },
    { "stars_count", "BIG BALLS: {0}" },
    { "danger_rating", "Danger: {0}/5" },
    { "no_information", "No information available." },
    { "no_galaxy_selected", "YER GALAXY NOT THERE ARRRGG" },
    { "name", "ARGGG NAME: {0}" },
    { "type", "ARGG TYPE?: {0}" },
    { "planets", "BIG BALLS: {0}" },
    { "journey_tracker", "Journey Tracker" },
    { "star_information", "Star Information" },
    { "planet_information", "Planet Information" },
    { "tutorial_step1", "Tutorial - Step 1" },
    { "tutorial_step2", "Tutorial - Step 2" },
    { "options", "Options" },
    { "select_galaxy", "ARGGG GALAXY" },
    { "galaxy_description", "Galaxy Description" },
    { "galaxy_info", "Galaxy Info" },
    { "favorite_stars", "Favorite Stars" },
    { "loading", "Loading" },
    { "filter_options", "Filter Options" },
    { "compendium", "Compendium" },
    { "filter", "Filter" },
    { "apply_filters", "Apply Filters" },
    { "star_info_name", "Name:" },
    { "star_info_type", "Type:" },
    { "star_info_planets", "Planets:" },
    { "close_button_label", "Close" },
    { "tutorial", "WANT THE ARGGGGGGG TUTORIAL MATE?" },
    { "apply", "WALK THE PLANK" },
    { "cancel", "Cancel" },
    { "yes", "Yes" },
    { "no", "No" },
    { "tutorial1", "HOVER OVER STARS TO SEE INFO ARGGGGGG!!!!" },
    { "tutorial2", "You can travel to different galaxies by pressing the button in the top right corner. ARGGGGGGGG" },
    { "next", "Next" },
    { "skip", "Skip" },
    { "galaxy_select", "Select Galaxy" },
    { "favorite_systems", "Favorites" },
    { "search_stars", "Search Stars" },
    { "exit", "Exit Starmap" },
    { "close", "Close" },
    { "current_star", "Current Star" },
    { "arg_redacted", "REDACTED" },
    { "filter_buttons_label", "FIlter" },
    { "search_stars_label", "Search" },
    { "apply_filters_label", "Apply" },
    { "crabby_way", "Crabby Way has a large amount stars in it. It's where yer save starts. Crabby Way be mostly stable, 'n it has a large variety o' habitable 'n inhabitable planets." },
    { "tuxxus", "Tuxxus is a large galaxy with the largest abundance of alien life. It has leftover ruins from an ancient alien empire on many systems." },
    { "krummple", "Tuxxus be a vast galaxy churnin' with the greatest treasure o' alien life. It has remnants o' an ancient alien empire scattered 'cross many systems." },
    { "bluenight", "BlueNight is a galaxy that, due to many Blue Giants, gives off a blue hue in the night sky in many systems. It's beautiful, with a high chance of finding fauna on many planets." },
    { "glass", "BlueNight be a galaxy what, 'cause o' many Blue Giants, gives off a blue glow in the night sky o' many systems. 'Tis a sight t' behold, with fair chances o' spottin' fauna on many a planet." },
    { "centuga", "Centuga be once a mightily strong galaxy ruled by an empire called the Universal Order, but in a fierce war, the empire met its doom, yet bits o' it linger and wrecks be strewn 'round the planets." },
    { "dank", "The dank galaxy be a murky an' foreboding spot, cloaked in secrets." },
    { "visit", "Visit" },
    { "visited", "VISITED" },
    { "parameters", "Parameters" },

};


				ModernBoxLogger.Log("Localization database initialized successfully.");
			}
            catch (System.Exception ex)
            {
                ModernBoxLogger.Error($"Error initializing localization database: {ex.Message}");
            }
        }

        
        void OnGUI()
        {
            if (isLanguageMenuVisible)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 300));

                
                GUILayout.Label(Localize("greeting"));

                
                GUILayout.Label("Select Language:");
                if (GUILayout.Button("English"))
                {
                    SetLanguage(Language.English);
                }
                if (GUILayout.Button("Spanish"))
                {
                    SetLanguage(Language.Spanish);
                }
                if (GUILayout.Button("French"))
                {
                    SetLanguage(Language.French);
                }
                if (GUILayout.Button("German"))
                {
                    SetLanguage(Language.German);
                }
                if (GUILayout.Button("Japanese"))
                {
                    SetLanguage(Language.Japanese);
                }

                GUILayout.EndArea();
            }
        }
    }
	
[HarmonyPatch(typeof(LocalizedTextManager), "setLanguage")]
public class SetLanguagePatch
{
    static void Postfix(string pLanguage)
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "language.txt");
            File.WriteAllText(filePath, pLanguage); 
            ModernBoxLogger.Log($"Language saved to {filePath}: {pLanguage}");
        }
        catch (Exception ex)
        {
            ModernBoxLogger.Error($"Failed to save language: {ex.Message}");
        }
    }
}
}
