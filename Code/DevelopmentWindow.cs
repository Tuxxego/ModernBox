using NCMS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ModernBox
{
    // Manu-Fix 018: Das Ergebnis der Simulation ist im vorhandenen Epochen-Reiter nachlesbar.
    internal static class DevelopmentWindow
    {
        private static ScrollWindow _window;
        private static Text _text;
        private static RectTransform _content;
        internal static void Init()
        {
            _window=ModernBox.ModernBoxLocale.Window("manu_mb_forschung",Development.T("Forschung und Aufbau","Research and development"));
            var scroll=_window.transform.Find("Background/Scroll View");scroll.gameObject.SetActive(true);
            _content=scroll.Find("Viewport/Content").GetComponent<RectTransform>();
            var go=new GameObject("Forschungsstand",typeof(RectTransform),typeof(Text));go.transform.SetParent(_content,false);
            _text=go.GetComponent<Text>();_text.font=_window.transform.Find("Background/Name").GetComponent<Text>().font;
            _text.fontSize=8;_text.color=Color.white;_text.supportRichText=true;_text.alignment=TextAnchor.UpperLeft;
            _text.horizontalOverflow=HorizontalWrapMode.Wrap;_text.verticalOverflow=VerticalWrapMode.Overflow;
            _text.raycastTarget=false;
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.5f,1);rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-12);rect.sizeDelta=new Vector2(190,500);
            new ButtonBuilder("manu_mb_forschung_oeffnen").SetSprite(Resources.Load<Sprite>("ui/icons/Renaissance"))
                .SetTitle(Development.T("Forschung und Aufbau","Research and development")).SetDescription(Development.T("Forschung, Voraussetzungen, Staedte und Heere aller Reiche.","Research, requirements, cities and armies of every kingdom."))
                .SetPosition(0,0).SetType(ButtonType.Click).SetTransform(Buttonz.getPowersTab("ModernBoxEras").transform)
                .SetFunction(()=>{ Refresh(true);Windows.ShowWindow("manu_mb_forschung"); }).Build();
            // Manu-Fix 018b (07.10.2026, Lauf mb_spawn_b_fix022): CreateNewWindow meldet den Fenstertitel nur fuer die aktive Sprache an;
            // nach einem Sprachwechsel fehlte er ("missing text: manu_mb_forschung"). Jetzt wie die Knoepfe fuer de und en.
            Locale("manu_mb_forschung","Forschung und Aufbau","Research and development");
            Locale("manu_mb_forschung_oeffnen","Forschung und Aufbau","Research and development");
            Locale("manu_mb_forschung_oeffnen_description","Forschung, Voraussetzungen, Staedte und Heere aller Reiche.","Research, requirements, cities and armies of every kingdom.");
            Locale("era_no_set","Automatische Forschung","Automatic research");
            Locale("era_no_set_description","Hebt den Goettereingriff auf und aktiviert alle Epochen fuer den Forschungsweg.","Clears the god override and enables all eras for research progression.");
            foreach(var entry in new[]{new[]{"era_mediaval_set","Medieval"},new[]{"era_renaissance_set","Renaissance"},new[]{"era_modern_set","Modern"},new[]{"era_hyperfuture_set","Hyperfuture"}})
                Locale(entry[0],"Goettereingriff: "+entry[1],"God override: "+entry[1]);
        }
        private static void Locale(string id,string de,string en)
        {
            ModernBoxLocale.Bilingual(id,de,en);
            // Manu-Fix 024: NCMS tooltips use " Description", GodPower uses "_description".
            if (id.EndsWith("_description")) ModernBoxLocale.Bilingual(id.Substring(0,id.Length-12)+" Description",de,en);
        }
        internal static void Refresh(bool force=false)
        {
            if(_text==null||(!force&&!_window.gameObject.activeInHierarchy))return;
            _window.titleText.text=Development.T("Forschung und Aufbau","Research and development");
            _text.text=Development.Report();float height=Mathf.Max(210,_text.preferredHeight+30);
            _text.rectTransform.sizeDelta=new Vector2(190,height);_content.sizeDelta=new Vector2(_content.sizeDelta.x,height+20);
        }
    }
}
