using System.Collections.Generic;

namespace ModernBox
{
    // Manu-Fix 016 (06.10.2026): Bildausschnitt im Einheitenfenster. Seit 0.50 verankert das Spiel animierte Einheiten unten
    // mittig im Ausschnitt (schwebende bei 25 %), inspect_avatar_offset_x/y wirkt nur noch bei eigenen Avatar-Bildern
    // (UnitAvatarLoader.showAnimation), und der Rahmen wird gegenskaliert (2,5 / inspect_avatar_scale). Mit den bisherigen
    // Skalen ragten grosse Einheiten weit ueber den Ausschnitt; im Fenster war oft nur ein Teil zu sehen (gemessen im Spiel:
    // 67 Arten, bis 85 % abgeschnitten). Neue Skala = groesste, bei der die ganze Figur in allen Lauf-/Schwimmbildern
    // hineinpasst, mit 5 % Rand; nur kleiner, nie groesser als der bisherige Wert.
    internal static class AvatarScaleFix
    {
        private static readonly Dictionary<string, float> Scales = new Dictionary<string, float>
        {
            { "artilleryatst", 0.58f }, // was 3.00: 63 % above the frame, 30/32 % at the sides
            { "AT9000", 0.39f }, // was 2.00: 68 % above the frame, 37/30 % at the sides
            { "atst", 0.49f }, // was 3.00: 68 % above the frame, 25/25 % at the sides
            { "atstsniper", 0.56f }, // was 3.00: 63 % above the frame, 33/23 % at the sides
            { "balloonunit", 0.13f }, // was 0.50: 69 % above the frame, 0/0 % at the sides
            { "baseWarUnit", 1.83f }, // was 3.00: 20 % above the frame, 11/11 % at the sides
            { "bigfaerydragon", 0.27f }, // was 0.50: 38 % above the frame, 0/0 % at the sides
            { "Bomber_Demon", 0.24f }, // was 0.50: 39 % above the frame, 56/0 % at the sides
            { "Bomber_Dwarf", 0.19f }, // was 0.50: 55 % above the frame, 16/16 % at the sides
            { "Bomber_Gaia", 0.11f }, // was 0.50: 72 % above the frame, 0/0 % at the sides
            { "Bomber_Human", 0.11f }, // was 0.50: 72 % above the frame, 0/0 % at the sides
            { "Bomber_Ork", 0.07f }, // was 0.50: 81 % above the frame, 30/30 % at the sides
            { "catapulta", 1.26f }, // was 2.00: 22 % above the frame, 13/13 % at the sides
            { "davincitank", 0.86f }, // was 2.00: 40 % above the frame, 26/26 % at the sides
            { "demoncroc", 1.26f }, // was 2.00: 22 % above the frame, 13/0 % at the sides
            { "demonreaver", 0.80f }, // was 2.00: 2 % above the frame, 16/16 % at the sides
            { "demonscorpion", 0.95f }, // was 3.00: 39 % above the frame, 28/28 % at the sides
            { "dreadnaught", 0.44f }, // was 3.00: 65 % above the frame, 39/32 % at the sides
            { "dreadnaught_brrt", 0.44f }, // was 3.00: 65 % above the frame, 38/31 % at the sides
            { "dwarfcannon", 1.02f }, // was 2.00: 32 % above the frame, 17/9 % at the sides
            { "elfcannon", 1.02f }, // was 2.00: 32 % above the frame, 0/0 % at the sides
            { "eliteAT9000", 0.39f }, // was 2.00: 68 % above the frame, 36/31 % at the sides
            { "EliteBomber", 0.11f }, // was 0.50: 72 % above the frame, 13/13 % at the sides
            { "eliteMA9000", 0.35f }, // was 2.00: 70 % above the frame, 41/39 % at the sides
            { "EliteP9000", 0.74f }, // was 2.00: 18 % above the frame, 30/30 % at the sides
            { "F55FighterJet", 0.23f }, // was 0.50: 42 % above the frame, 11/0 % at the sides
            { "FighterJet_Dwarf", 0.21f }, // was 0.50: 49 % above the frame, 3/0 % at the sides
            { "FighterJet_Gaia", 0.19f }, // was 0.50: 54 % above the frame, 0/0 % at the sides
            { "FighterJet_Human", 0.21f }, // was 0.50: 50 % above the frame, 0/0 % at the sides
            { "FighterJet_Ork", 0.21f }, // was 0.50: 50 % above the frame, 6/0 % at the sides
            { "FutureGunship", 0.24f }, // was 0.50: 43 % above the frame, 0/0 % at the sides
            { "golemgem", 1.80f }, // was 3.00: 0 % above the frame, 18/18 % at the sides
            { "Gunship", 0.23f }, // was 0.50: 46 % above the frame, 0/0 % at the sides
            { "Heli_Dwarf", 0.35f }, // was 0.50: 21 % above the frame, 0/0 % at the sides
            { "HeliELite", 0.19f }, // was 0.50: 54 % above the frame, 0/0 % at the sides
            { "humancavalry", 1.46f }, // was 3.00: 0 % above the frame, 25/21 % at the sides
            { "HumanTitan", 0.23f }, // was 3.00: 80 % above the frame, 46/32 % at the sides
            { "HumanTitanElite", 0.21f }, // was 3.00: 85 % above the frame, 40/37 % at the sides
            { "MA9000", 0.35f }, // was 2.00: 70 % above the frame, 41/39 % at the sides
            { "MissileSystem_Dwarf", 0.77f }, // was 2.00: 25 % above the frame, 26/18 % at the sides
            { "modernhumvee_Gaia", 1.83f }, // was 3.00: 20 % above the frame, 13/13 % at the sides
            { "ogreunit", 1.02f }, // was 3.00: 44 % above the frame, 20/20 % at the sides
            { "OmegaRailgun", 1.26f }, // was 2.00: 22 % above the frame, 17/17 % at the sides
            { "orcatapulta", 1.09f }, // was 2.00: 29 % above the frame, 16/16 % at the sides
            { "orccannon", 1.26f }, // was 2.00: 22 % above the frame, 13/0 % at the sides
            { "P9000", 0.74f }, // was 2.00: 18 % above the frame, 30/30 % at the sides
            { "Railgun", 1.26f }, // was 2.00: 22 % above the frame, 17/17 % at the sides
            { "santaguin", 1.09f }, // was 2.00: 29 % above the frame, 0/0 % at the sides
            { "SpaceMarine", 1.90f }, // was 3.00: 2 % above the frame, 17/17 % at the sides
            { "spaceork", 1.46f }, // was 3.00: 20 % above the frame, 24/24 % at the sides
            { "supportatst", 0.53f }, // was 3.00: 66 % above the frame, 8/31 % at the sides
            { "Tank_Gaia", 0.91f }, // was 2.00: 29 % above the frame, 16/12 % at the sides
            { "Tank_Ork", 1.26f }, // was 2.00: 22 % above the frame, 13/13 % at the sides
            { "Terran", 0.63f }, // was 3.00: 56 % above the frame, 21/21 % at the sides
            { "teslatruckgun", 1.26f }, // was 3.00: 0 % above the frame, 28/28 % at the sides
            { "TIEfighter", 0.27f }, // was 0.50: 36 % above the frame, 0/0 % at the sides
            { "wheeledtank_Dwarf", 1.22f }, // was 2.00: 2 % above the frame, 17/9 % at the sides
            { "wheeledtank_Gaia", 1.26f }, // was 2.00: 22 % above the frame, 0/0 % at the sides
            { "woolyrhino", 1.31f }, // was 2.00: 13 % above the frame, 12/16 % at the sides
            { "xenolevitank", 1.35f }, // was 2.00: 13 % above the frame, 15/8 % at the sides
            { "xenorailgun", 0.54f }, // was 2.00: 58 % above the frame, 34/34 % at the sides
            { "xenotripod", 0.20f }, // was 2.00: 82 % above the frame, 15/31 % at the sides
            { "xenoUFO", 0.27f }, // was 0.50: 39 % above the frame, 0/0 % at the sides
            { "xenoUFObomber", 0.26f }, // was 0.50: 40 % above the frame, 0/0 % at the sides
            { "zombieballoon", 0.21f }, // was 2.50: 83 % above the frame, 0/0 % at the sides
            { "zombiedruid", 0.96f }, // was 2.50: 42 % above the frame, 0/0 % at the sides
            { "zombietarantula", 1.65f }, // was 2.50: 7 % above the frame, 11/16 % at the sides
        };

        internal static void Apply()
        {
            foreach (KeyValuePair<string, float> kv in Scales)
            {
                ActorAsset asset = AssetManager.actor_library.get(kv.Key);
                if (asset != null && asset.inspect_avatar_scale > kv.Value) asset.inspect_avatar_scale = kv.Value;
            }
        }
    }

    // Manu-Fix 017 (06.10.2026): Einheitennamen. ModernBox meldet die Namen unter name_locale an ("Light Vehicle", "Tank" ...),
    // das Spiel fragt seit 0.50 aber ActorAsset.getLocaleID() ab = name_locale.Underscore() ("light_vehicle", "tank" ...).
    // Folge: "LocalizedTextManager: missing text" (Fehlerzeile, oeffnet die Konsole) und roher Schluessel als Name, sobald das
    // Einheitenfenster einer solchen Einheit geoeffnet wird (gemessen im Spiel: bomber, tank, light_vehicle, armored_walker).
    // Fuer jede Einheit dieser Mod, deren Name unter name_locale vorhanden ist und unter getLocaleID() fehlt, wird derselbe
    // Text auch unter getLocaleID() angemeldet. Vorhandene Texte (auch anderer Sprachen) bleiben unberuehrt.
    internal static class NameLocaleFix
    {
        internal static void Apply(int firstIndex)
        {
            List<ActorAsset> list = AssetManager.actor_library.list;
            for (int i = firstIndex < 0 ? 0 : firstIndex; i < list.Count; i++)
            {
                ActorAsset asset = list[i];
                if (asset == null || string.IsNullOrEmpty(asset.name_locale)) continue;
                string localeId = asset.getLocaleID();
                if (string.IsNullOrEmpty(localeId) || localeId == asset.name_locale) continue;
                if (LocalizedTextManager.stringExists(localeId) || !LocalizedTextManager.stringExists(asset.name_locale)) continue;
                TextFix.Register(localeId, LocalizedTextManager.getText(asset.name_locale));
            }
        }
    }

    // Manu-Fix 017 (Fortsetzung): weitere Texte, die das Spiel ueber getLocaleID() abfragt und die fehlten (gemessen im Spiel,
    // Namenspruefung aller Asset-Bibliotheken): die Gegenstaende dieser Mod (translation_key ist schon der Anzeigetext, z. B.
    // "Glock17, the bestest gun"), die sechs Reiter ("tab_modernbox") und die Aufgabe des Kriegsschiffs (Einheitenfenster und
    // Statuszeile zeigen actor.getTaskText()). Angemeldet fuer die aktuelle Sprache und als englischer Rueckfall.
    internal static class TextFix
    {
        internal static void ApplyStatic()
        {
            Register("tab_modernbox", "ModernBox");
            Register("task_unit_warBoatAttackDecision", "War boat attack");
        }

        internal static void ApplyItems(int firstIndex)
        {
            var list = AssetManager.items.list;
            for (int i = firstIndex < 0 ? 0 : firstIndex; i < list.Count; i++)
            {
                if (list[i] == null) continue;
                string key = list[i].getLocaleID();
                if (string.IsNullOrEmpty(key)) continue;
                string text = key.Replace('_', ' ').Trim();
                if (text.Length > 0) text = char.ToUpper(text[0]) + text.Substring(1);
                Register(key, text);
                // Manu-Fix 024: the equipment editor asks for descriptions even
                // when the mod supplies none. Keep that optional prose empty,
                // rather than displaying a missing key or inventing item facts.
                string description = list[i].getDescriptionID();
                if (!string.IsNullOrEmpty(description) && !LocalizedTextManager.stringExists(description))
                {
                    ModernBoxLocale.Register(description, "");
                    ModernBoxLocale.Register(description.Underscore(), "");
                }
            }
        }

        internal static void Register(string key, string text)
        {
            if (string.IsNullOrEmpty(key) || LocalizedTextManager.stringExists(key)) return;
            ModernBoxLocale.Register(key, text); // Manu-Fix 024: restore before the first text refresh.
        }
    }

    // Manu-Fix 022 (07.10.2026, Lauf mb_spawn_a_original): Jedes Ueberfahren und Anklicken eines ModernBox-Spawnknopfs warf eine
    // ArgumentNullException (LocalizedTextManager.getText <- ActorAsset.getLocalizedDescription, aus TooltipLibrary.showUnitGeneric
    // und WorldTip.showToolbarText). ActorAsset.getDescriptionID() fragt die Gottkraft der Einheit ab (power_id ?? base_asset_id ?? id)
    // und bekommt null: Vanilla setzt power_id nur einmal beim Verknuepfen der Bibliotheken (PowerLibrary: Kraft mit actor_asset_id),
    // die Spawnkraefte dieser Mod entstehen erst danach. Hier dieselbe Verknuepfung fuer die eigenen Kraefte nachholen - nur fuer
    // Einheiten, die noch gar keine Kraft finden - und die vorhandene Knopfbeschreibung ("Spawn the unit: ...") zusaetzlich unter dem
    // Schluessel anmelden, den das Spiel abfragt (GodPower.getDescriptionID()). Es wird kein neuer Text erfunden. Immer auch als
    // englischer Rueckfall: ButtonBuilder meldet seine Texte nur fuer die aktive Sprache an, nach einem Sprachwechsel zeigte der
    // Tooltip sonst den rohen Schluessel (gemessen Lauf mb_spawn_b_fix022: "spawn_spaceork_description").
    internal static class SpawnPowerLinkFix
    {
        internal static void Apply(int firstIndex)
        {
            List<GodPower> list = AssetManager.powers.list;
            int linked = 0;
            for (int i = firstIndex < 0 ? 0 : firstIndex; i < list.Count; i++)
            {
                GodPower power = list[i];
                if (power == null) continue;
                linked += Link(power, power.actor_asset_id);
                if (power.actor_asset_ids == null) continue;
                foreach (string actorId in power.actor_asset_ids) linked += Link(power, actorId);
            }
            ModernBoxLogger.Log($"[Fix022] Spawn powers linked to units: {linked}");
        }

        private static int Link(GodPower power, string actorId)
        {
            if (string.IsNullOrEmpty(actorId) || !AssetManager.actor_library.has(actorId)) return 0;
            ActorAsset actor = AssetManager.actor_library.get(actorId);
            if (actor == null || actor.power_id != null || actor.getGodPower() != null) return 0;
            actor.power_id = power.id;
            string key = power.getDescriptionID();
            if (string.IsNullOrEmpty(key)) return 1;
            string text;
            if (LocalizedTextManager.stringExists(key))
            {
                text = LocalizedTextManager.getText(key);
            }
            else
            {
                string source = power.id + "_description";
                if (!LocalizedTextManager.stringExists(source)) source = power.id + " Description";
                text = LocalizedTextManager.stringExists(source) ? LocalizedTextManager.getText(source) : actor.getLocalizedName();
                ModernBoxLocale.Register(key, text);
            }
            ModernBoxLocale.Register(key, text);
            return 1;
        }
    }
}
