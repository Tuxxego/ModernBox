using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace ModernBoxM2Rewrite
{
    // 0.51.2 has no tech screen, so the tab gets one: the selected (or most advanced)
    // culture's era, research and M2 techs, plus a short list of the other cultures.
    internal static class M2TechWindow
    {
        internal const string Id = "M2TechWindow";
        private const string Gold = "#FFD700";
        private const float Width = 190f;
        private const int IconsPerRow = 8;
        private const float IconSize = 22f;
        private static readonly Color TextColor = new Color(0.9f, 0.6f, 0f, 1f);
        private static readonly string[] Eras = { "Medieval", "Renaissance", "Industrial", "Modern", "Future" };
        private const string FallbackIcon = "icon_tech_ancestors_knowledge";
        private static ScrollWindow _window;
        private static Font _font;
        private static Culture _shown;

        internal static void Create()
        {
            const string titleKey = "m2_window_tech";
            ModernLocalization.Add(titleKey, "Technologies");
            ModernLocalization.Add("m2_tech_next", "Next culture");
            ModernLocalization.Add("m2_tech_next_description", "Show the next culture.");
            _window = WindowCreator.CreateEmptyWindow(Id, titleKey, "tabIconModernWarfare");
            if (_window == null) return;
            Transform titleTransform = _window.transform.Find("Background/Title");
            LocalizedText titleText = titleTransform == null ? null : titleTransform.GetComponent<LocalizedText>();
            if (titleText != null)
            {
                titleText.setKeyAndUpdate(titleKey);
                titleText.autoField = false;
            }
            _font = _window.titleText != null ? _window.titleText.font : null;
            PowerButton next = PowerButtonCreator.CreateSimpleButton("m2_tech_next", ShowNext,
                Resources.Load<Sprite>("ui/Icons/nextpage"), _window.transform, new Vector2(120f, 52f));
            next.transform.SetAsLastSibling();
        }

        internal static void Open()
        {
            if (_window == null) return;
            _shown = PickCulture();
            Refresh();
            ScrollWindow.showWindow(Id);
        }

        private static void ShowNext()
        {
            List<Culture> cultures = Cultures();
            if (cultures.Count == 0) return;
            int index = cultures.IndexOf(_shown);
            _shown = cultures[(index + 1) % cultures.Count];
            Refresh();
        }

        private static List<Culture> Cultures()
        {
            List<Culture> list = new List<Culture>();
            if (World.world == null || World.world.cultures == null) return list;
            foreach (Culture culture in World.world.cultures)
                if (M2Tech.IsResearching(culture) && !culture.isRekt()) list.Add(culture);
            return list.OrderByDescending(culture => M2Tech.Get(culture).Order.Count).ToList();
        }

        private static Culture PickCulture()
        {
            Culture selected = SelectedMetas.selected_culture;
            if (M2Tech.IsResearching(selected) && !selected.isRekt()) return selected;
            if (SelectedUnit.isSet() && SelectedUnit.unit != null && M2Tech.IsResearching(SelectedUnit.unit.culture))
                return SelectedUnit.unit.culture;
            return Cultures().FirstOrDefault();
        }

        private static void Refresh()
        {
            RectTransform content = Content();
            if (content == null) return;
            foreach (LayoutGroup group in content.GetComponents<LayoutGroup>()) group.enabled = false;
            foreach (ContentSizeFitter fitter in content.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
            for (int i = content.childCount - 1; i >= 0; i--) Object.Destroy(content.GetChild(i).gameObject);

            float y = 15f;
            if (_shown == null || _shown.isRekt())
            {
                y = AddText(content, "No human, orc, elf or dwarf culture yet.", y);
                content.sizeDelta = new Vector2(content.sizeDelta.x, y + 30f);
                return;
            }

            M2CultureTechs techs = M2Tech.Get(_shown);
            string researching = "nothing";
            M2TechDef def;
            if (!string.IsNullOrEmpty(techs.Researching) && M2TechData.ById.TryGetValue(techs.Researching, out def))
            {
                float cost = M2Tech.Cost(techs);
                int percent = cost <= 0f ? 100 : Mathf.Clamp(Mathf.FloorToInt(techs.Progress / cost * 100f), 0, 100);
                researching = def.Name + " (" + percent + "%)";
            }
            else if (techs.MaxReached) researching = "everything researched";

            string header = "<color='" + Gold + "'>" + _shown.name + "</color>\n" +
                            Capitalize(_shown.species_id) + " culture, " + ModernProgression.EraName(techs.Era) + " era\n" +
                            "Level " + M2Tech.Level(techs) + ", " + techs.Order.Count + " techs\n" +
                            "Researching: " + researching + "\n" +
                            "Knowledge: +" + M2Tech.KnowledgeGain(_shown, techs).ToString("0.0") + " every 5 seconds";
            if (!ModernBoxSettings.Get("ProgressionOption")) header += "\n(progression is turned off)";
            y = AddText(content, header, y);
            int speed = M2Tech.Speed;
            y = AddText(content, "<color='#7FDBFF'>[ Research speed: " + speed + "x" + (speed == 1 ? " (original)" : "") +
                                 " - click to change ]</color>", y, CycleSpeed);

            foreach (string era in Eras)
            {
                List<M2TechDef> group = M2TechData.All.Where(tech => InEra(tech, era)).ToList();
                int done = group.Count(tech => techs.Set.Contains(tech.Id));
                y = AddText(content, era + " (" + done + "/" + group.Count + ")", y + 4f);
                for (int i = 0; i < group.Count; i++)
                {
                    int column = i % IconsPerRow;
                    int row = i / IconsPerRow;
                    AddIcon(content, group[i], techs,
                        new Vector2(-Width / 2f + IconSize / 2f + column * (IconSize + 2f), -(y + row * (IconSize + 2f))));
                }
                y += Mathf.CeilToInt(group.Count / (float)IconsPerRow) * (IconSize + 2f);
            }

            List<Culture> others = Cultures().Where(culture => culture != _shown).Take(6).ToList();
            if (others.Count > 0)
            {
                string list = "<color='" + Gold + "'>Other cultures</color>";
                foreach (Culture culture in others)
                {
                    M2CultureTechs state = M2Tech.Get(culture);
                    list += "\n" + culture.name + ": " + ModernProgression.EraName(state.Era) + ", level " + M2Tech.Level(state);
                }
                y = AddText(content, list, y + 6f);
            }
            content.sizeDelta = new Vector2(content.sizeDelta.x, y + 40f);
        }

        private static bool InEra(M2TechDef tech, string era)
        {
            if (era == "Medieval") return !tech.IsM2;
            if (!tech.IsM2) return false;
            if (era == "Renaissance") return tech.RequiredLevel < 65;
            if (era == "Industrial") return tech.RequiredLevel >= 65 && tech.RequiredLevel < 80;
            if (era == "Modern") return tech.RequiredLevel >= 80 && tech.RequiredLevel < 90;
            return tech.RequiredLevel >= 90;
        }

        private static void CycleSpeed()
        {
            int index = System.Array.IndexOf(M2Tech.Speeds, M2Tech.Speed);
            M2Tech.Speed = M2Tech.Speeds[(index + 1) % M2Tech.Speeds.Length];
            Refresh();
        }

        private static float AddText(RectTransform content, string text, float y, UnityEngine.Events.UnityAction onClick = null)
        {
            GameObject textObject = new GameObject("M2TechText", typeof(RectTransform));
            textObject.transform.SetParent(content, false);
            Text label = textObject.AddComponent<Text>();
            label.font = _font;
            label.fontSize = 9;
            label.color = TextColor;
            label.alignment = TextAnchor.UpperCenter;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = onClick != null;
            label.text = text;
            if (onClick != null)
            {
                Button button = textObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = label;
                button.onClick.AddListener(onClick);
            }
            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(Width, 10f);
            float height = label.preferredHeight;
            rect.sizeDelta = new Vector2(Width, height);
            rect.anchoredPosition = new Vector2(0f, -y);
            return y + height + 2f;
        }

        private static void AddIcon(RectTransform content, M2TechDef tech, M2CultureTechs techs, Vector2 position)
        {
            bool researched = techs.Set.Contains(tech.Id);
            Sprite sprite = Resources.Load<Sprite>("ui/Icons/" + (tech.Icon ?? "icon_tech_" + tech.Id))
                            ?? Resources.Load<Sprite>("ui/Icons/" + FallbackIcon);
            if (sprite == null) return;
            GameObject iconObject = new GameObject("M2TechIcon_" + tech.Id, typeof(RectTransform));
            iconObject.transform.SetParent(content, false);
            Image image = iconObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;
            image.color = researched ? Color.white : new Color(0.25f, 0.25f, 0.25f, 0.6f);
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            rect.anchoredPosition = position;

            Button button = iconObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;
            string key = "m2_tech_tip_" + tech.Id;
            ModernLocalization.Add(key, tech.Name);
            ModernLocalization.Add(key + "_description", Describe(tech, techs, researched));
            TipButton tip = iconObject.AddComponent<TipButton>();
            tip.textOnClick = key;
            tip.textOnClickDescription = key + "_description";
            tip.type = "tip";
        }

        private static string Describe(M2TechDef tech, M2CultureTechs techs, bool researched)
        {
            string text = Effect(tech);
            string status;
            if (researched) status = "<color='#7CFC00'>Researched</color>";
            else if (techs.Researching == tech.Id) status = "<color='" + Gold + "'>Researching now</color>";
            else
            {
                status = "<color='#FF6347'>Not researched</color>, needs level " + tech.RequiredLevel;
                List<string> missing = tech.Requirements.Where(id => !techs.Set.Contains(id))
                    .Select(id => { M2TechDef req; return M2TechData.ById.TryGetValue(id, out req) ? req.Name : id; }).ToList();
                if (missing.Count > 0) status += "\nRequires: " + string.Join(", ", missing);
            }
            if (tech.Rare) status += "\nRare tech";
            return string.IsNullOrEmpty(text) ? status : text + "\n" + status;
        }

        private static string Effect(M2TechDef tech)
        {
            switch (tech.Id)
            {
                case "Renaissance": return "Renaissance buildings, barracks, docks and weapons.";
                case "Industrial": return "Industrial buildings.";
                case "Firearms": return "Industrial barracks, docks, rifles and uniforms.";
                case "Skyscraper": return "Modern houses and halls.";
                case "MilitaryModern": return "Modern military units, guns and armor. +5000 army size.";
                case "Future": return "Future buildings, units, blasters and armor.";
                case "Casino": return "Casinos.";
                case "Cyberware": return "Cyberware and drugs.";
                case "Nukes": return "Missile silos.";
            }
            switch (tech.Bonus)
            {
                case M2TechBonus.Mining: return "Miners can gather extra ore.";
                case M2TechBonus.Axes: return "Woodcutters can gather extra wood.";
                case M2TechBonus.BornLevel: return "Units are born 2 levels higher.";
                case M2TechBonus.Housing: return "+1 housing per house.";
                case M2TechBonus.Towers: return "+2 watch towers per city.";
                case M2TechBonus.Army: return "+10% army size.";
                case M2TechBonus.Damage: return "+10% damage.";
                case M2TechBonus.Armor: return "+10% armor.";
                case M2TechBonus.Trading: case M2TechBonus.Storage: case M2TechBonus.Armorsmith:
                case M2TechBonus.Weaponsmith: case M2TechBonus.Heroes:
                    return "Research step (no effect in 0.51.2).";
            }
            if (tech.KnowledgeGain != 0f) return (tech.KnowledgeGain > 0 ? "+" : "") + tech.KnowledgeGain.ToString("0.0") + " knowledge gain.";
            return tech.IsM2 ? "" : "Medieval tech, counts toward culture level.";
        }

        private static RectTransform Content()
        {
            if (_window == null) return null;
            if (_window.transform_content != null) return _window.transform_content;
            return _window.transform.Find("Background/Scroll View/Viewport/Content") as RectTransform;
        }

        private static string Capitalize(string text)
        {
            return string.IsNullOrEmpty(text) ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
        }
    }
}
