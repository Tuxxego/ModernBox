using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    // Research trace for tuning: every finished tech, era change and a summary of all
    // cultures each minute of game time, written to M2Research.log next to Player.log.
    internal static class M2ResearchLog
    {
        private const float SummaryInterval = 60f;
        private static readonly Dictionary<Culture, float> Started = new Dictionary<Culture, float>();
        private static readonly HashSet<Culture> Stalled = new HashSet<Culture>();

        // Culture objects are recycled into the next world.
        internal static void ResetWorldState()
        {
            Started.Clear();
            Stalled.Clear();
        }
        private static string _path;
        private static float _gameTime;
        private static float _summaryTimer = SummaryInterval;

        private static string Path
        {
            get
            {
                if (_path != null) return _path;
                _path = System.IO.Path.Combine(Application.persistentDataPath, "M2Research.log");
                try { File.WriteAllText(_path, "ModernBox M2 research log, started " + DateTime.Now + "\n"); }
                catch { }
                return _path;
            }
        }

        private static void Write(string line)
        {
            try
            {
                File.AppendAllText(Path, "[y" + SafeYear() + " t" + _gameTime.ToString("0") + "s] " + line + "\n");
            }
            catch { }
        }

        private static int SafeYear()
        {
            try { return Date.getCurrentYear(); }
            catch { return 0; }
        }

        internal static void Tick(float elapsed)
        {
            _gameTime += elapsed;
            _summaryTimer -= elapsed;
            if (_summaryTimer > 0f) return;
            _summaryTimer = SummaryInterval;
            try { Summary(); }
            catch (Exception exception) { Write("summary failed: " + exception.Message); }
        }

        internal static void ResearchStarted(Culture culture, M2CultureTechs state)
        {
            Started[culture] = _gameTime;
            if (string.IsNullOrEmpty(state.Researching))
            {
                if (Stalled.Add(culture))
                    Write(Name(culture) + " has nothing left to research at level " + M2Tech.Level(state) +
                          (state.MaxReached ? " (all done)" : " (waiting for level/requirements)"));
                return;
            }
            Stalled.Remove(culture);
            M2TechDef def = M2TechData.ById[state.Researching];
            Write(Name(culture) + " started " + def.Id + (def.Rare ? " [rare]" : "") + (def.Priority ? " [priority]" : "") +
                  ", cost " + M2Tech.Cost(state).ToString("0") + ", gain " + state.KnowledgeGain.ToString("0.00") +
                  "/5s, eta ~" + Eta(state).ToString("0") + "s");
        }

        internal static void TechDone(Culture culture, M2CultureTechs state, string techId, M2Era oldEra)
        {
            float started;
            string took = Started.TryGetValue(culture, out started) ? (_gameTime - started).ToString("0") + "s" : "?";
            Write(Name(culture) + " FINISHED " + techId + " in " + took + " -> level " + M2Tech.Level(state) +
                  ", " + state.Order.Count + " techs, rare " + state.Rare);
            if (state.Era != oldEra)
                Write("=== " + Name(culture) + " ENTERED " + ModernProgression.EraName(state.Era).ToUpperInvariant() +
                      " ERA (was " + ModernProgression.EraName(oldEra) + ") ===");
        }

        internal static void Note(string line)
        {
            Write(line);
        }

        private static float Eta(M2CultureTechs state)
        {
            if (state.KnowledgeGain <= 0f) return -1f;
            return Mathf.Max(0f, M2Tech.Cost(state) - state.Progress) / state.KnowledgeGain * 5f;
        }

        private static string Name(Culture culture)
        {
            return culture.name + " (" + culture.species_id + ")";
        }

        private static void Summary()
        {
            if (World.world == null || World.world.cultures == null) return;
            List<Culture> cultures = new List<Culture>();
            foreach (Culture culture in World.world.cultures)
                if (M2Tech.IsResearching(culture) && !culture.isRekt()) cultures.Add(culture);
            StringBuilder text = new StringBuilder();
            text.Append("--- summary: " + cultures.Count + " cultures, research " +
                        (ModernBoxSettings.Get("ProgressionOption") ? "on" : "OFF") + ", speed " + M2Tech.Speed + "x ---\n");
            foreach (Culture culture in cultures.OrderByDescending(c => M2Tech.Get(c).Order.Count))
            {
                M2CultureTechs state = M2Tech.Get(culture);
                int cities = 0, leaders = 0;
                float intelligence = 0f;
                foreach (City city in culture.cities)
                {
                    if (city == null) continue;
                    cities++;
                    Actor leader = city.leader;
                    if (leader == null || !leader.isAlive() || leader.culture != culture) continue;
                    leaders++;
                    intelligence += leader.stats["intelligence"];
                }
                float cost = M2Tech.Cost(state);
                text.Append("    " + Name(culture) + ": " + ModernProgression.EraName(state.Era) +
                            " (military " + ModernProgression.EraName(state.MilitaryEra) + "), level " + M2Tech.Level(state) +
                            ", cities " + cities + ", leaders " + leaders +
                            (leaders > 0 ? " avg int " + (intelligence / leaders).ToString("0.0") : "") +
                            ", gain " + state.KnowledgeGain.ToString("0.00") + "/5s, researching " +
                            (string.IsNullOrEmpty(state.Researching) ? "-" :
                                state.Researching + " " + state.Progress.ToString("0") + "/" + cost.ToString("0")) + "\n");
            }
            Write(text.ToString().TrimEnd('\n'));
        }
    }
}
