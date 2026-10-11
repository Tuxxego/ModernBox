using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace ModernBoxM1Rewrite
{
    // One culture's researched techs, loaded from and saved to its CultureData.
    internal sealed class M1CultureTechs
    {
        internal CultureData Data;
        internal readonly List<string> Order = new List<string>();
        internal readonly HashSet<string> Set = new HashSet<string>(StringComparer.Ordinal);
        internal string Researching = string.Empty;
        internal float Progress;
        internal float TechKnowledge;
        internal int Rare;
    }

    // The old game's culture research (Culture.updateProgress, findNextTechToResearch,
    // calculateKnowledgeGain), run by the mod because 0.51.2 dropped it.
    internal static class M1Tech
    {
        private const string Prefix = ModernBoxCatalog.Guid + ".tech.";
        private const string KeyTechs = Prefix + "list";
        private const string KeyResearching = Prefix + "researching";
        private const string KeyProgress = Prefix + "progress";
        private const float ProgressInterval = 5f; // CultureManager._progress_timer
        // Tech.cs: MilitaryModern gives the culture bonus_max_army +5000.
        internal const float MilitaryModernArmy = 5000f;

        private static readonly ConditionalWeakTable<Culture, M1CultureTechs> States = new ConditionalWeakTable<Culture, M1CultureTechs>();
        private static float _timer;

        internal static bool IsResearching(Culture culture)
        {
            return culture != null && culture.data != null && ModernBoxCatalog.IsSupportedRace(culture.species_id);
        }

        internal static M1CultureTechs Get(Culture culture)
        {
            if (culture == null || culture.data == null) return null;
            M1CultureTechs state = States.GetValue(culture, Load);
            // 0.51.2 recycles Culture objects when a world is cleared, so a reused
            // object must not keep the previous culture's techs.
            if (state.Data != culture.data)
            {
                States.Remove(culture);
                state = States.GetValue(culture, Load);
            }
            return state;
        }

        private static M1CultureTechs Load(Culture culture)
        {
            M1CultureTechs state = new M1CultureTechs { Data = culture.data };
            string list;
            culture.data.get(KeyTechs, out list, string.Empty);
            if (!string.IsNullOrEmpty(list))
                foreach (string id in list.Split(','))
                    if (M1TechData.ById.ContainsKey(id) && state.Set.Add(id)) state.Order.Add(id);
            culture.data.get(KeyResearching, out state.Researching, string.Empty);
            if (state.Researching == null || !M1TechData.ById.ContainsKey(state.Researching)) state.Researching = string.Empty;
            culture.data.get(KeyProgress, out state.Progress, 0f);
            Recalculate(state);
            return state;
        }

        private static void Save(Culture culture, M1CultureTechs state)
        {
            culture.data.set(KeyTechs, string.Join(",", state.Order));
            culture.data.set(KeyResearching, state.Researching ?? string.Empty);
            culture.data.set(KeyProgress, state.Progress);
        }

        private static void Recalculate(M1CultureTechs state)
        {
            state.TechKnowledge = 0f;
            state.Rare = 0;
            foreach (string id in state.Order)
            {
                M1TechDef def = M1TechData.ById[id];
                state.TechKnowledge += def.KnowledgeGain;
                if (def.Rare) state.Rare++;
            }
        }

        internal static bool Has(Culture culture, string techId)
        {
            if (string.IsNullOrEmpty(techId)) return true;
            if (!IsResearching(culture)) return false;
            M1CultureTechs state = Get(culture);
            return state != null && state.Set.Contains(techId);
        }

        internal static bool Has(City city, string techId)
        {
            return city != null && Has(city.culture, techId);
        }

        internal static float Army(City city)
        {
            if (city == null || city.kingdom == null || city.kingdom.wild) return 0f;
            return Has(city.culture, "MilitaryModern") ? MilitaryModernArmy : 0f;
        }

        // Culture.getCurrentLevel: the number of researched techs.
        internal static int Level(M1CultureTechs state)
        {
            return state.Order.Count;
        }

        internal static float Cost(M1CultureTechs state)
        {
            M1TechDef def;
            if (string.IsNullOrEmpty(state.Researching) || !M1TechData.ById.TryGetValue(state.Researching, out def)) return 0f;
            float cost = def.KnowledgeCost * 100f + 25f * Level(state);
            return def.Rare ? cost * 2f : cost;
        }

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.cultures == null) return;
            _timer -= elapsed;
            if (_timer > 0f) return;
            _timer = ProgressInterval;
            foreach (Culture culture in World.world.cultures)
            {
                if (!IsResearching(culture) || culture.isRekt()) continue;
                try { UpdateProgress(culture); }
                catch (Exception exception) { ModernBoxDiagnostics.Error("Research failed for " + culture.name + ": " + exception); }
            }
        }

        private static void UpdateProgress(Culture culture)
        {
            M1CultureTechs state = Get(culture);
            if (state == null) return;
            if (string.IsNullOrEmpty(state.Researching))
            {
                state.Researching = FindNext(culture, state);
                state.Progress = 0f;
                Save(culture, state);
                return;
            }
            state.Progress += KnowledgeGain(culture, state);
            if (state.Progress >= Cost(state))
            {
                string done = state.Researching;
                if (state.Set.Add(done))
                {
                    state.Order.Add(done);
                    Recalculate(state);
                    if (M1TechData.ById[done].IsM1) ModernBoxDiagnostics.Info(culture.name + " researched " + done + ".");
                }
                state.Progress = 0f;
                state.Researching = FindNext(culture, state);
            }
            Save(culture, state);
        }

        // Culture.calculateKnowledgeGain
        private static float KnowledgeGain(Culture culture, M1CultureTechs state)
        {
            string race = culture.species_id;
            float gain = 1f;
            foreach (City city in culture.cities)
            {
                Actor leader = city == null ? null : city.leader;
                if (leader == null || !leader.isAlive() || leader.culture != culture) continue;
                gain += (leader.stats["intelligence"] + 1f) * M1TechData.KnowledgePerIntelligence(race) * 0.1f;
            }
            return gain + state.TechKnowledge;
        }

        // Culture.findNextTechToResearch: priority techs first, otherwise any open tech.
        private static string FindNext(Culture culture, M1CultureTechs state)
        {
            string race = culture.species_id;
            int level = Level(state);
            List<M1TechDef> priority = new List<M1TechDef>();
            List<M1TechDef> any = new List<M1TechDef>();
            foreach (M1TechDef def in M1TechData.All)
            {
                if (def.RequiredLevel > level || state.Set.Contains(def.Id) || !HasRequirements(state, def)) continue;
                if (def.Rare && state.Rare >= M1TechData.RareTechLimit(race)) continue;
                if (M1TechData.IsForbidden(race, def.Id)) continue;
                if (def.Priority) priority.Add(def);
                any.Add(def);
            }
            if (priority.Count > 0) return priority[UnityEngine.Random.Range(0, priority.Count)].Id;
            if (any.Count > 0) return any[UnityEngine.Random.Range(0, any.Count)].Id;
            return string.Empty;
        }

        private static readonly Dictionary<string, string> M1Labels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Casino", "Casinos" }, { "LowFirearms", "Pipe Guns" }, { "MilitaryModern", "Modern Military" },
            { "Firearms", "Guns" }, { "Skyscraper", "Skyscrapers" }, { "Tanks", "Tanks" }, { "Helicopter", "Helicopters" },
            { "FighterJet", "Fighter Jets" }, { "MIRVBomber", "MIRV Bombers" }, { "F55", "F55" }, { "Drone", "Drones" },
            { "Nukes", "Nukes" }, { "BudgetMIRV", "Short Range MIRVs" }, { "Cyberware", "Cyberware" },
            { "DecentMIRV", "Decent Range MIRVs" }, { "Gunship", "Gunships" }, { "MIRV", "Long Range MIRVs" },
            { "STRONGMIRV", "Nuclear MIRVs" }
        };

        private static string Label(M1TechDef def)
        {
            string label;
            if (def.IsM1 && M1Labels.TryGetValue(def.Id, out label)) return label;
            return def.Id.Replace('_', ' ');
        }

        // The text for the Tech window: every culture that researches, highest level first.
        internal static string Report(string gold)
        {
            List<Culture> cultures = new List<Culture>();
            if (World.world != null && World.world.cultures != null)
                foreach (Culture culture in World.world.cultures)
                    if (IsResearching(culture) && !culture.isRekt() && Get(culture) != null) cultures.Add(culture);
            cultures.Sort((a, b) => Level(Get(b)).CompareTo(Level(Get(a))));

            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.Append("<color='").Append(gold).Append("'>[Tech ]</color>\n\n");
            text.Append("Cultures research one tech at a time. Their level is the number of techs they have.\n\n");
            if (cultures.Count == 0) text.Append("No human, orc, elf or dwarf cultures yet.\n\n");
            foreach (Culture culture in cultures)
            {
                M1CultureTechs state = Get(culture);
                int level = Level(state);
                string race = culture.species_id ?? string.Empty;
                if (race.Length > 0) race = char.ToUpper(race[0]) + race.Substring(1);
                text.Append("<color='").Append(gold).Append("'>").Append(culture.name).Append("</color> (").Append(race).Append(")\n");
                text.Append("Level ").Append(level);
                M1TechDef current;
                if (!string.IsNullOrEmpty(state.Researching) && M1TechData.ById.TryGetValue(state.Researching, out current))
                {
                    float cost = Cost(state);
                    int percent = cost <= 0f ? 0 : Math.Min(99, (int)(state.Progress / cost * 100f));
                    text.Append(", researching ").Append(Label(current)).Append(" (").Append(percent).Append("%)");
                }
                text.Append("\n");

                List<string> owned = new List<string>();
                int next = int.MaxValue;
                foreach (M1TechDef def in M1TechData.All)
                {
                    if (!def.IsM1) continue;
                    if (state.Set.Contains(def.Id)) owned.Add(Label(def));
                    else if (def.RequiredLevel > level && def.RequiredLevel < next) next = def.RequiredLevel;
                }
                text.Append("ModernBox: ").Append(owned.Count == 0 ? "none yet" : string.Join(", ", owned)).Append("\n");
                if (owned.Count == 0 && next != int.MaxValue) text.Append("First ModernBox tech at level ").Append(next).Append("\n");
                text.Append("\n");
            }

            text.Append("<color='").Append(gold).Append("'>Unlocks</color>\n");
            SortedDictionary<int, List<string>> byLevel = new SortedDictionary<int, List<string>>();
            foreach (M1TechDef def in M1TechData.All)
            {
                if (!def.IsM1) continue;
                List<string> names;
                if (!byLevel.TryGetValue(def.RequiredLevel, out names)) byLevel[def.RequiredLevel] = names = new List<string>();
                names.Add(Label(def));
            }
            foreach (KeyValuePair<int, List<string>> pair in byLevel)
                text.Append("Level ").Append(pair.Key).Append(": ").Append(string.Join(", ", pair.Value)).Append("\n");
            return text.ToString();
        }

        private static bool HasRequirements(M1CultureTechs state, M1TechDef def)
        {
            foreach (string requirement in def.Requirements)
                if (!state.Set.Contains(requirement)) return false;
            return true;
        }
    }

    // Everything that ran on the old game's clock runs on world time, so it keeps
    // pace with the game speed and stops while paused.
    [HarmonyPatch(typeof(MapBox), "updateWorldBehaviours")]
    internal static class M1WorldUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix(MapBox __instance, float pElapsed)
        {
            if (__instance == null || __instance.isPaused()) return;
            M1Tech.Update(pElapsed);
            ProductionService.Update(pElapsed);
        }
    }

    // City.getArmyMaxMultiplier: the culture's bonus_max_army, which MilitaryModern raised by 5000.
    [HarmonyPatch(typeof(City), nameof(City.getArmyMaxMultiplier))]
    internal static class M1ArmyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(City __instance, ref float __result)
        {
            __result += M1Tech.Army(__instance);
        }
    }
}
