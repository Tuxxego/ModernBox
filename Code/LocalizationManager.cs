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
            { "cz", Language.Chinese }, // WorldBox simplified Chinese.
            { "ch", Language.Chinese }, // WorldBox traditional Chinese.
			{ "ko", Language.Korean },      
			{ "pt", Language.Portuguese },
            { "br", Language.Portuguese },
			{ "tr", Language.Turkish },  
			{ "boat", Language.Boat }      
			
		};



        
        public Language currentLanguage = Language.English;
        private string lastGameLanguage;

        
        private Dictionary<Language, Dictionary<string, string>> localizationDatabase;

        
        void Awake()
        {
            // Manu-Fix 032: the game language is already loaded before this mod.
            // A missing side file must not select Spanish or delay initialization.
            InitializeLocalizationDatabase();
            SyncGameLanguage();
        }

        private void SyncGameLanguage()
        {
            string code = LocalizedTextManager.instance?.language;
            if (string.IsNullOrEmpty(code) || code == "not_set")
            {
                string filePath = Path.Combine(Application.persistentDataPath, "language.txt");
                try { if (File.Exists(filePath)) code = File.ReadAllText(filePath).Trim(); }
                catch (Exception ex) { ModernBoxLogger.Warning($"Cannot read legacy language preference: {ex.Message}"); }
            }
            if (lastGameLanguage == code) return;
            lastGameLanguage = code;
            currentLanguage = code != null && languageCodeMap.TryGetValue(code, out Language parsed)
                ? parsed : Language.English;
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
            SyncGameLanguage();
            if (localizationDatabase == null) InitializeLocalizationDatabase();
            if (localizationDatabase.TryGetValue(currentLanguage, out var selected) && selected.TryGetValue(key, out string value))
                return value;
            if (localizationDatabase.TryGetValue(Language.English, out var english) && english.TryGetValue(key, out value))
                return value;
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
    { "stars_loading_message", "正在生成恒星系，可能需要一点时间。期间游戏短暂无响应属于正常现象。" },
    { "did_you_know", "你知道吗？" },
    { "random_fact", "*{0}*" },
    { "stars_unknown", "恒星数量: ???" },
    { "danger_unknown", "危险等级: ???/5" },
    { "stars_count", "恒星数量: {0}" },
    { "danger_rating", "危险等级: {0}/5" },
    { "no_information", "暂无可用信息。" },
    { "no_galaxy_selected", "尚未选择星系。" },
    { " no_results_found", "未找到匹配的恒星系。" },
    { "no_results_found", "未找到匹配的恒星系。" },
    { "name", "名称: {0}" },
    { "type", "类型: {0}" },
    { "planets", "行星数量: {0}" },
    { "journey_tracker", "航行足迹" },
    { "star_information", "恒星系信息" },
    { "planet_information", "行星信息" },
    { "tutorial_step1", "星图教程 - 第 1 步" },
    { "tutorial_step2", "星图教程 - 第 2 步" },
    { "options", "星图选项" },
    { "select_galaxy", "选择星系" },
    { "galaxy_description", "星系简介" },
    { "galaxy_info", "星系数据" },
    { "favorite_stars", "收藏的恒星系" },
    { "loading", "加载中" },
    { "filter_options", "筛选选项" },
    { "compendium", "宇宙百科" },
    { "filter", "筛选" },
    { "apply_filters", "应用筛选" },
    { "star_info_name", "名称" },
    { "star_info_type", "恒星类型" },
    { "star_info_planets", "行星数" },
    { "close_button_label", "关闭" },
    { "tutorial", "是否查看星图操作教程？" },
    { "apply", "应用" },
    { "cancel", "取消" },
    { "yes", "是" },
    { "no", "否" },
    { "tutorial1", "将鼠标悬停在恒星（彩色光点）上可查看名称，点击恒星即可打开详细的行星列表弹窗。" },
    { "tutorial2", "点击右上角的星系按钮，即可在已解锁的不同星系之间进行跃迁航行。" },
    { "next", "下一步" },
    { "finish", "完成" },
    { "skip", "跳过" },
    { "galaxy_select", "切换星系" },
    { "favorite_systems", "收藏夹" },
    { "search_stars", "搜索恒星" },
    { "exit", "退出星图" },
    { "close", "关闭" },
    { "current_star", "当前恒星系" },
    { "arg_redacted", "机密档案" },
    { "filter_buttons_label", "筛选" },
    { "search_stars_label", "搜索" },
    { "apply_filters_label", "启用筛选条件" },
    { "crabby_way", "巨蟹座星系（Crabby Way）拥有庞大的恒星群，也是你存档的起点。该星系环境稳定，包含丰富的宜居与非宜居行星。" },
    { "tuxxus", "图克苏斯（Tuxxus）是一个外星生命极其繁盛的大型星系，许多恒星系中都残留着远古外星帝国的遗迹。" },
    { "krummple", "克鲁姆普（Krummple）星系大部分区域荒无人烟，但蕴藏着大量气候温和的宜居行星。" },
    { "bluenight", "蓝夜星系（BlueNight）因富含蓝巨星而在夜空中呈现出迷人的蔚蓝色泽，在各行星上发现野生生物群落的概率极高。" },
    { "glass", "琉璃星系（Glass）是一个拥有七条旋臂的螺旋星系，但中央有一道巨大的撕裂裂隙，仿佛曾被某种不可名状的巨物像玻璃般击碎。" },
    { "centuga", "森图加（Centuga）曾受强大的“寰宇秩序”帝国统治，大战虽已令帝国覆灭，但残余势力与战争废墟仍遍布各个星球。" },
    { "dank", "幽暗星系（Dank）是一片笼罩在未知迷雾中的危险星域，由深暗虚空帝国所盘踞。" },
    { "visit", "登陆造访" },
    { "visited", "已探索" },
    { "parameters", "行星参数" },
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
