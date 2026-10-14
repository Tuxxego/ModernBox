using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    // One culture's researched techs, loaded from and saved to its CultureData.
    internal sealed class M2CultureTechs
    {
        internal CultureData Data;
        internal readonly List<string> Order = new List<string>();
        internal readonly HashSet<string> Set = new HashSet<string>(StringComparer.Ordinal);
        internal string Researching = string.Empty;
        internal float Progress;
        internal float KnowledgeGain;
        internal bool MaxReached;
        internal readonly int[] Bonus = new int[Enum.GetValues(typeof(M2TechBonus)).Length];
        internal float Army;
        internal float TechKnowledge;
        internal int Rare;
        internal M2Era Era;
        internal M2Era MilitaryEra;

        internal int Count(M2TechBonus bonus)
        {
            return Bonus[(int)bonus];
        }
    }

    // The old game's culture research (Culture.updateProgress, findNextTechToResearch,
    // calculateKnowledgeGain), run by the mod because 0.51.2 dropped it.
    internal static class M2Tech
    {
        private const string Prefix = ModernBoxCatalog.Guid + ".tech.";
        private const string KeyTechs = Prefix + "list";
        private const string KeyResearching = Prefix + "researching";
        private const string KeyProgress = Prefix + "progress";
        private const string KeyMigrated = Prefix + "migrated";
        private const float ProgressInterval = 5f; // CultureManager._progress_timer

        private static readonly ConditionalWeakTable<Culture, M2CultureTechs> States = new ConditionalWeakTable<Culture, M2CultureTechs>();
        private static float _timer;
        private const string SpeedKey = ModernBoxCatalog.Guid + ".research_speed";
        internal static readonly int[] Speeds = { 1, 2, 5, 10 };

        // Not in the original: a multiplier for players who want eras sooner. 1 is the original pace.
        internal static int Speed
        {
            get { return Mathf.Max(1, PlayerPrefs.GetInt(SpeedKey, 1)); }
            set { PlayerPrefs.SetInt(SpeedKey, value); PlayerPrefs.Save(); }
        }

        internal static bool IsResearching(Culture culture)
        {
            return culture != null && culture.data != null && ModernBoxCatalog.IsSupportedRace(culture.species_id);
        }

        internal static M2CultureTechs Get(Culture culture)
        {
            if (culture == null || culture.data == null) return null;
            M2CultureTechs state = States.GetValue(culture, Load);
            // 0.51.2 recycles Culture objects when a world is cleared, so a reused
            // object must not keep the previous culture's techs.
            if (state.Data != culture.data)
            {
                States.Remove(culture);
                state = States.GetValue(culture, Load);
            }
            return state;
        }

        private static M2CultureTechs Load(Culture culture)
        {
            M2CultureTechs state = new M2CultureTechs { Data = culture.data };
            string list;
            culture.data.get(KeyTechs, out list, string.Empty);
            if (!string.IsNullOrEmpty(list))
                foreach (string id in list.Split(','))
                    if (M2TechData.ById.ContainsKey(id) && state.Set.Add(id)) state.Order.Add(id);
            culture.data.get(KeyResearching, out state.Researching, string.Empty);
            if (state.Researching == null || !M2TechData.ById.ContainsKey(state.Researching)) state.Researching = string.Empty;
            culture.data.get(KeyProgress, out state.Progress, 0f);
            Recalculate(state);
            return state;
        }

        private static void Save(Culture culture, M2CultureTechs state)
        {
            culture.data.set(KeyTechs, string.Join(",", state.Order));
            culture.data.set(KeyResearching, state.Researching ?? string.Empty);
            culture.data.set(KeyProgress, state.Progress);
        }

        private static void Recalculate(M2CultureTechs state)
        {
            Array.Clear(state.Bonus, 0, state.Bonus.Length);
            state.Army = 0f;
            state.TechKnowledge = 0f;
            state.Rare = 0;
            foreach (string id in state.Order)
            {
                M2TechDef def = M2TechData.ById[id];
                state.Bonus[(int)def.Bonus]++;
                if (def.Bonus == M2TechBonus.Army) state.Army += M2TechData.ArmyValue(def);
                state.TechKnowledge += def.KnowledgeGain;
                if (def.Rare) state.Rare++;
            }
            state.Era = state.Set.Contains("Future") ? M2Era.Future
                : state.Set.Contains("Skyscraper") ? M2Era.Modern
                : state.Set.Contains("Industrial") ? M2Era.Industrial
                : state.Set.Contains("Renaissance") ? M2Era.Renaissance
                : M2Era.Medieval;
            // Traits.GetRoleType / GetUnitCandidates use their own ladder.
            state.MilitaryEra = state.Set.Contains("Future") ? M2Era.Future
                : state.Set.Contains("MilitaryModern") ? M2Era.Modern
                : state.Set.Contains("Firearms") ? M2Era.Industrial
                : state.Set.Contains("Renaissance") ? M2Era.Renaissance
                : M2Era.Medieval;
        }

        internal static bool Has(Culture culture, string techId)
        {
            M2CultureTechs state = Get(culture);
            return state != null && state.Set.Contains(techId);
        }

        internal static bool Has(City city, string techId)
        {
            return city != null && Has(city.culture, techId);
        }

        internal static M2Era Era(Culture culture)
        {
            if (!IsResearching(culture)) return M2Era.Medieval;
            M2CultureTechs state = Get(culture);
            return state == null ? M2Era.Medieval : state.Era;
        }

        internal static M2Era MilitaryEra(Culture culture)
        {
            if (!IsResearching(culture)) return M2Era.Medieval;
            M2CultureTechs state = Get(culture);
            return state == null ? M2Era.Medieval : state.MilitaryEra;
        }

        internal static int Level(M2CultureTechs state)
        {
            return state.Order.Count + M2TechData.LevelOffset;
        }

        internal static float Cost(M2CultureTechs state)
        {
            M2TechDef def;
            if (string.IsNullOrEmpty(state.Researching) || !M2TechData.ById.TryGetValue(state.Researching, out def)) return 0f;
            float cost = def.KnowledgeCost * 100f + 25f * Level(state);
            return def.Rare ? cost * 2f : cost;
        }

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.cultures == null) return;
            M2ResearchLog.Tick(elapsed);
            _timer -= elapsed;
            if (_timer > 0f) return;
            _timer = ProgressInterval;
            MigrateOldSave();
            if (!ModernBoxSettings.Get("ProgressionOption")) return;
            foreach (Culture culture in World.world.cultures)
            {
                if (!IsResearching(culture) || culture.isRekt()) continue;
                try { UpdateProgress(culture); }
                catch (Exception exception) { ModernBoxDiagnostics.Error("M2 research failed for " + culture.name + ": " + exception); }
            }
        }

        private static void UpdateProgress(Culture culture)
        {
            M2CultureTechs state = Get(culture);
            if (state == null) return;
            state.KnowledgeGain = KnowledgeGain(culture, state);
            if (string.IsNullOrEmpty(state.Researching))
            {
                state.Researching = FindNext(culture, state);
                state.Progress = 0f;
                M2ResearchLog.ResearchStarted(culture, state);
                Save(culture, state);
                return;
            }
            state.Progress += state.KnowledgeGain * Speed;
            if (state.Progress >= Cost(state))
            {
                M2Era oldEra = state.Era;
                string done = state.Researching;
                AddTech(culture, state, done);
                M2ResearchLog.TechDone(culture, state, done, oldEra);
                state.Progress = 0f;
                state.Researching = FindNext(culture, state);
                M2ResearchLog.ResearchStarted(culture, state);
            }
            Save(culture, state);
        }

        // Culture.calculateKnowledgeGain
        internal static float KnowledgeGain(Culture culture, M2CultureTechs state)
        {
            string race = culture.species_id;
            float gain = 1f;
            foreach (City city in culture.cities)
            {
                Actor leader = city == null ? null : city.leader;
                if (leader == null || !leader.isAlive() || leader.culture != culture) continue;
                gain += (leader.stats["intelligence"] + 1f) * M2TechData.KnowledgePerIntelligence(race) * 0.1f;
            }
            return gain + state.TechKnowledge;
        }

        // Culture.findNextTechToResearch
        private static string FindNext(Culture culture, M2CultureTechs state)
        {
            string race = culture.species_id;
            int level = Level(state);
            List<M2TechDef> priority = new List<M2TechDef>();
            List<M2TechDef> any = new List<M2TechDef>();
            foreach (M2TechDef def in M2TechData.All)
            {
                if (def.RequiredLevel > level || state.Set.Contains(def.Id) || !HasRequirements(state, def)) continue;
                if (def.Rare && state.Rare >= M2TechData.RareTechLimit(race)) continue;
                if (M2TechData.IsForbidden(race, def.Id)) continue;
                if (def.Priority) priority.Add(def);
                any.Add(def);
            }
            state.MaxReached = any.Count == 0;
            if (priority.Count > 0) return priority[UnityEngine.Random.Range(0, priority.Count)].Id;
            if (any.Count > 0) return any[UnityEngine.Random.Range(0, any.Count)].Id;
            return string.Empty;
        }

        private static bool HasRequirements(M2CultureTechs state, M2TechDef def)
        {
            foreach (string requirement in def.Requirements)
                if (!state.Set.Contains(requirement)) return false;
            return true;
        }

        private static void AddTech(Culture culture, M2CultureTechs state, string techId)
        {
            if (string.IsNullOrEmpty(techId) || !state.Set.Add(techId)) return;
            M2Era oldEra = state.Era;
            M2Era oldMilitary = state.MilitaryEra;
            state.Order.Add(techId);
            Recalculate(state);
            if (state.Era != oldEra || state.MilitaryEra != oldMilitary || M2TechData.ById[techId].IsM2)
                OnCultureChanged(culture);
            if (state.Era != oldEra)
                ModernBoxDiagnostics.Info(culture.name + " entered the " + ModernProgression.EraName(state.Era) + " era.");
        }

        private static void OnCultureChanged(Culture culture)
        {
            M2LegacyBehaviorService.RefreshCultureSprites(culture);
        }

        // Saves made before the tech rebuild advanced the whole world on a timer.
        // Give their cultures the techs up to the era they had, so nothing goes back.
        private static void MigrateOldSave()
        {
            SaveCustomData data = ModernProgression.WorldData();
            if (data == null) return;
            bool migrated;
            data.get(KeyMigrated, out migrated, false);
            if (migrated) return;
            data.set(KeyMigrated, true);
            M2Era era = ModernProgression.LegacyWorldEra();
            if (era <= M2Era.Medieval) return;
            foreach (Culture culture in World.world.cultures)
            {
                if (!IsResearching(culture) || culture.isRekt()) continue;
                M2CultureTechs state = Get(culture);
                if (state == null || state.Order.Count > 0) continue;
                GrantUpTo(culture, state, era);
            }
            M2ResearchLog.Note("Old save migration: gave cultures techs up to the " + ModernProgression.EraName(era) + " era.");
            ModernBoxDiagnostics.Info("Gave cultures in this older save the techs up to the " + ModernProgression.EraName(era) + " era.");
        }

        internal static void GrantUpTo(Culture culture, M2CultureTechs state, M2Era era)
        {
            string headId = era == M2Era.Future ? "Future" : era == M2Era.Modern ? "Skyscraper" : era == M2Era.Industrial ? "Industrial" : "Renaissance";
            int gate = M2TechData.ById[headId].RequiredLevel;
            string race = culture.species_id;
            int rare = 0;
            foreach (M2TechDef def in M2TechData.All)
            {
                if (def.Id != headId && def.RequiredLevel >= gate) continue;
                if (M2TechData.IsForbidden(race, def.Id)) continue;
                if (def.Rare && rare >= M2TechData.RareTechLimit(race)) continue;
                if (def.Rare) rare++;
                if (state.Set.Add(def.Id)) state.Order.Add(def.Id);
            }
            state.Researching = string.Empty;
            state.Progress = 0f;
            Recalculate(state);
            Save(culture, state);
            OnCultureChanged(culture);
        }

        // Landed colonists bring their culture's techs (Buttonz.PasteUnit added the saved
        // tech list to the colony's culture).
        internal static List<string> Snapshot(Culture culture)
        {
            M2CultureTechs state = Get(culture);
            return state == null ? new List<string>() : new List<string>(state.Order);
        }

        internal static void AddTechs(Culture culture, IEnumerable<string> techIds)
        {
            M2CultureTechs state = Get(culture);
            if (state == null || techIds == null) return;
            bool changed = false;
            foreach (string id in techIds)
                if (M2TechData.ById.ContainsKey(id) && state.Set.Add(id))
                {
                    state.Order.Add(id);
                    changed = true;
                }
            if (!changed) return;
            Recalculate(state);
            if (!string.IsNullOrEmpty(state.Researching) && state.Set.Contains(state.Researching))
            {
                state.Researching = string.Empty;
                state.Progress = 0f;
            }
            Save(culture, state);
            OnCultureChanged(culture);
        }

        // For the tech window and debugging.
        internal static void ForceResearch(Culture culture)
        {
            M2CultureTechs state = Get(culture);
            if (state == null) return;
            if (string.IsNullOrEmpty(state.Researching)) state.Researching = FindNext(culture, state);
            if (string.IsNullOrEmpty(state.Researching)) return;
            AddTech(culture, state, state.Researching);
            state.Progress = 0f;
            state.Researching = FindNext(culture, state);
            Save(culture, state);
        }
    }

    [HarmonyPatch(typeof(MapBox), "updateWorldBehaviours")]
    internal static class M2TechUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix(MapBox __instance, float pElapsed)
        {
            if (__instance == null || __instance.isPaused()) return;
            M2Tech.Update(pElapsed);
            ProductionService.GameUpdate(pElapsed);
            // World time, so these follow game speed and stop while paused.
            ProductionService.Update(pElapsed);
            BombService.Update();
            M2LegacyBehaviorService.Update(pElapsed);
        }
    }
}
