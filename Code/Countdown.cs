using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace ModernBox
{
    public class Countdown : MonoBehaviour
    {
        private class CountdownEntry
        {
            public Text text;
            public DateTime targetDate;
            public float baseScale = 1f;
            public float pulseAmount = 0.1f;
            public float pulseSpeed = 2f;
        }

        private static Countdown instance;
        private List<CountdownEntry> activeCountdowns = new List<CountdownEntry>();

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void RegisterCountdown(Text uiText, DateTime targetDate)
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CountdownManager");
                instance = obj.AddComponent<Countdown>();
                DontDestroyOnLoad(obj);
            }

            CountdownEntry entry = new CountdownEntry
            {
                text = uiText,
                targetDate = targetDate
            };

            instance.activeCountdowns.Add(entry);
        }

        void Update()
        {
            foreach (var entry in activeCountdowns)
            {
                if (entry.text == null) continue;

                TimeSpan remaining = entry.targetDate - DateTime.UtcNow;

                if (remaining.TotalSeconds < 0)
                {
                    entry.text.text = "<color=red>Countdown Finished!</color>";
                    continue;
                }

                string formatted = string.Format(
                    "<color=#FFD700>{0:D2}d</color> : <color=#FFAA00>{1:D2}h</color> : <color=#FF8800>{2:D2}m</color> : <color=#FF5500>{3:D2}s</color>",
                    remaining.Days, remaining.Hours, remaining.Minutes, remaining.Seconds
                );

                entry.text.text = formatted;

                float scale = entry.baseScale + Mathf.Sin(Time.time * entry.pulseSpeed) * entry.pulseAmount;
                entry.text.rectTransform.localScale = new Vector3(scale, scale, 1f);

                float t = (Mathf.Sin(Time.time * 2f) + 1f) / 2f;
                entry.text.color = Color.Lerp(new Color(1f, 0.8f, 0.2f), new Color(1f, 0.5f, 0.1f), t);
            }
        }
    }
}