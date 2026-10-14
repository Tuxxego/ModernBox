using NeoModLoader.General;

namespace ModernBoxM2Rewrite
{
    internal static class ModernLocalization
    {
        internal static void Add(string key, string value)
        {
            LM.AddToCurrentLocale(key, value);
            string normalized = key.ToLowerInvariant();
            if (normalized != key) LM.AddToCurrentLocale(normalized, value);
            string gameKey = KeyForGame(key);
            if (gameKey != key && gameKey != normalized) LM.AddToCurrentLocale(gameKey, value);
        }

        internal static string KeyForGame(string key)
        {
            return StringExtension.Underscore(key);
        }

        internal static void Apply()
        {
            LM.ApplyLocale(true);
        }
    }
}

