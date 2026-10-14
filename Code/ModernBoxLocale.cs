using System.Collections.Generic;
using HarmonyLib;
using NCMS.Utils;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;

namespace ModernBox
{
    // Manu-Fix 024: NCMS stores runtime strings only in the current language. Register the
    // mod's own strings explicitly, including NCMS and GodPower aliases and late windows.
    // Restore them after loading the locale, BEFORE vanilla refreshes its text components.
    // Do not copy the whole game's text dictionary or capture another mod's registrations.
    internal static class ModernBoxLocale
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> German = new Dictionary<string, string>();

        internal static void Register(string key, string text)
        {
            if (string.IsNullOrEmpty(key)) return;
            English[key] = text ?? "";
            LM.Add("en", key, text ?? "");
            LM.AddToCurrentLocale(key, Value(key));
        }

        internal static void Bilingual(string key, string de, string en)
        {
            English[key] = en;
            German[key] = de;
            LM.Add("en", key, en);
            LM.Add("de", key, de);
            LM.AddToCurrentLocale(key, Value(key));
        }

        private static string Value(string key)
        {
            return LocalizedTextManager.instance.language == "de" && German.TryGetValue(key, out string de)
                ? de : English[key];
        }

        internal static void Apply()
        {
            if (LocalizedTextManager.instance == null) return;
            foreach (string key in English.Keys) LM.AddToCurrentLocale(key, Value(key));
        }

        internal static ScrollWindow Window(string id, string title)
        {
            if (!English.ContainsKey(id)) Register(id, title);
            // Manu-Fix 025: NCMS initializes a new hidden window via hide(),
            // which clears WorldBox's current-window pointer even for a different
            // visible window. Keep that window tracked so the next navigation
            // can close it instead of leaving an orphaned overlay in front.
            ScrollWindow visible = ScrollWindow.getCurrentWindow();
            ScrollWindow window = Windows.CreateNewWindow(id, Value(id));
            if (visible != null && visible.gameObject.activeInHierarchy && ScrollWindow.getCurrentWindow() != visible)
                AccessTools.Method(typeof(ScrollWindow), "setCurrentWindow").Invoke(null, new object[] { visible });
            return window;
        }

        internal static PowerButton Button(string id, Sprite sprite, string title, string description,
            Vector2 position, ButtonType type = ButtonType.Click, Transform parent = null, UnityAction call = null)
        {
            ButtonText(id, title, description);
            if (type == ButtonType.GodPower)
            {
                GodPower power = AssetManager.powers.get(id);
                if (power != null)
                {
                    Register(power.getLocaleID(), title ?? id);
                    Register(power.getDescriptionID(), description ?? title ?? id);
                }
            }
            return PowerButtons.CreateButton(id, sprite, Value(id), Value(id + " Description"), position, type, parent, call);
        }

        internal static void ButtonText(string id, string title, string description)
        {
            Register(id, title ?? id);
            Register(id + " Description", description ?? title ?? id);
            Register(id + "_description", description ?? title ?? id);
            // Build 719's PowerButton reads underscored names even for ordinary NCMS buttons.
            Register(id.Underscore(), title ?? id);
            Register(id.Underscore() + "_description", description ?? title ?? id);
        }
    }

    [HarmonyPatch(typeof(LocalizedTextManager), nameof(LocalizedTextManager.loadLocalizedText))]
    internal static class ModernBoxLocaleReload
    {
        private static void Postfix() { ModernBoxLocale.Apply(); }
    }
}
