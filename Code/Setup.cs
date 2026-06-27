using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ModernBox
{

    public class ModernBoxShitty : MonoBehaviour
    {

        static readonly Color C_BG          = new Color(0.03f, 0.02f, 0.02f, 1f);
        static readonly Color C_PANEL       = new Color(0.06f, 0.03f, 0.03f, 0.98f);
        static readonly Color C_RED         = new Color(0.82f, 0.07f, 0.07f, 1f);
        static readonly Color C_RED_BRIGHT  = new Color(1.00f, 0.18f, 0.12f, 1f);
        static readonly Color C_RED_DIM     = new Color(0.38f, 0.04f, 0.04f, 1f);
        static readonly Color C_WHITE       = new Color(0.94f, 0.91f, 0.91f, 1f);
        static readonly Color C_GREY        = new Color(0.40f, 0.32f, 0.32f, 1f);
        static readonly Color C_CARD_IDLE   = new Color(0.10f, 0.06f, 0.06f, 1f);
        static readonly Color C_CARD_SEL    = new Color(0.60f, 0.05f, 0.05f, 1f);
        static readonly Color C_CARD_HOV    = new Color(0.18f, 0.09f, 0.09f, 1f);
        static readonly Color C_SCANLINE    = new Color(0f, 0f, 0f, 0.14f);
        static readonly Color C_CLEAR       = Color.clear;

        Canvas        _canvas;
        RectTransform _contentRoot;   

        RectTransform _mainPanel;
        Image         _panelBorder;
        Image         _progressFill;
        Text          _logoText;

        readonly List<GameObject> _tabs      = new List<GameObject>();
        int _currentTab = 0;

        readonly List<Image> _eraCards = new List<Image>();
        readonly List<Image> _balCards = new List<Image>();
        readonly List<Image> _disCards = new List<Image>();   

        readonly List<RectTransform> _particles = new List<RectTransform>();
        const int PARTICLE_COUNT = 36;

        RectTransform _scanlineRT;
        float _glitchTimer = 0f;
        float _glitchNext  = 5f;

        Font _font;

        void Start()
        {
            ModernBoxPrefs.Load();
            _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystem();
            BuildCanvas();
            BuildBackground();
            BuildParticles();
            BuildScanlines();
            BuildMainPanel();

            BuildTab1_Welcome();
            BuildTab2_Era();
            BuildTab3_Balance();
            BuildTab4_Discord();
            BuildTab5_Done();

            LayoutTabs();
            RefreshAllSelections();
            UpdateProgress(animate: false);

            StartCoroutine(CR_Particles());
            StartCoroutine(CR_PulseBorder());
            StartCoroutine(CR_ScrollScanlines());
            StartCoroutine(CR_FadeIn(_mainPanel.gameObject, 0.55f));
        }

        void Update()
        {
            if (_logoText == null) return;
            _glitchTimer += Time.deltaTime;
            if (_glitchTimer >= _glitchNext)
            {
                _glitchTimer = 0f;
                _glitchNext  = Random.Range(4f, 9f);
                StartCoroutine(CR_GlitchText(_logoText));
            }
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        void BuildCanvas()
        {
            var go = new GameObject("MB_Canvas");
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;               

            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1280, 720);
            cs.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
        }

        void BuildBackground()
        {

            var bg = UIRect("BG", _canvas.transform);
            Stretch(bg);
            var bgi = bg.gameObject.AddComponent<Image>();
            bgi.color = C_BG;
            bgi.raycastTarget = false;

            var vig = UIRect("Vignette", _canvas.transform);
            Stretch(vig);
            var vi = vig.gameObject.AddComponent<Image>();
            vi.sprite = MakeRadialGradientSprite(256, C_CLEAR, new Color(0.5f, 0f, 0f, 0.20f));
            vi.color  = Color.white;
            vi.raycastTarget = false;

            var gl = UIRect("BottomGlow", _canvas.transform);
            gl.anchorMin = new Vector2(0, 0);
            gl.anchorMax = new Vector2(1, 0);
            gl.sizeDelta = new Vector2(0, 2);
            gl.anchoredPosition = new Vector2(0, 1);
            var gli = gl.gameObject.AddComponent<Image>();
            gli.color = C_RED;
            gli.raycastTarget = false;
        }

        void BuildParticles()
        {
            var holder = UIRect("Particles", _canvas.transform);
            Stretch(holder);

            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                var p  = UIRect("P" + i, holder);
                float sz = Random.Range(1.2f, 3.2f);
                p.sizeDelta = new Vector2(sz, sz);
                p.anchoredPosition = new Vector2(Random.Range(-640f, 640f), Random.Range(-360f, 360f));

                var pi = p.gameObject.AddComponent<Image>();
                pi.color = new Color(C_RED.r, C_RED.g, C_RED.b, Random.Range(0.10f, 0.45f));
                pi.raycastTarget = false;
                _particles.Add(p);
            }
        }

        void BuildScanlines()
        {
            var go = new GameObject("Scanlines");
            _scanlineRT = go.AddComponent<RectTransform>();
            _scanlineRT.SetParent(_canvas.transform, false);
            Stretch(_scanlineRT);

            var img = go.AddComponent<Image>();
            img.sprite = MakeScanlineSprite(4);
            img.type   = Image.Type.Tiled;
            img.color  = C_SCANLINE;
            img.raycastTarget = false;
        }

        void BuildMainPanel()
        {
            var panel = UIRect("MB_Panel", _canvas.transform);
            _mainPanel = panel;
            panel.sizeDelta = new Vector2(660, 530);
            panel.anchoredPosition = Vector2.zero;

            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = C_PANEL;
            bg.raycastTarget = false;

            var bgo = UIRect("PanelBorder", panel);
            bgo.anchorMin = Vector2.zero;
            bgo.anchorMax = Vector2.one;
            bgo.sizeDelta = new Vector2(4, 4);
            _panelBorder = bgo.gameObject.AddComponent<Image>();
            _panelBorder.sprite     = MakeBorderSprite(4);
            _panelBorder.type       = Image.Type.Sliced;
            _panelBorder.color      = C_RED;
            _panelBorder.fillCenter = false;
            _panelBorder.raycastTarget = false;

            BuildHeader(panel);
            BuildProgressBar(panel);
            AddCornerAccents(panel);

            var vp = UIRect("Viewport", panel);
            vp.anchorMin        = new Vector2(0.5f, 0.5f);
            vp.anchorMax        = new Vector2(0.5f, 0.5f);
            vp.pivot            = new Vector2(0.5f, 0.5f);
            vp.sizeDelta        = new Vector2(660, 415);
            vp.anchoredPosition = new Vector2(0, -20);
            vp.gameObject.AddComponent<RectMask2D>();

            var content = UIRect("ContentStrip", vp);
            content.anchorMin        = new Vector2(0.5f, 0.5f);
            content.anchorMax        = new Vector2(0.5f, 0.5f);
            content.pivot            = new Vector2(0.5f, 0.5f);
            content.sizeDelta        = new Vector2(660, 415);
            content.anchoredPosition = Vector2.zero;
            _contentRoot = content;
        }

        void BuildHeader(RectTransform panel)
        {

            var hdr = UIRect("Header", panel);
            hdr.anchorMin       = new Vector2(0, 1);
            hdr.anchorMax       = new Vector2(1, 1);
            hdr.sizeDelta       = new Vector2(0, 62);
            hdr.anchoredPosition = new Vector2(0, -31);
            var hi = hdr.gameObject.AddComponent<Image>();
            hi.color = new Color(0.09f, 0.04f, 0.04f, 1f);
            hi.raycastTarget = false;

            var stripe = UIRect("HeaderStripe", hdr);
            stripe.anchorMin        = new Vector2(0, 0);
            stripe.anchorMax        = new Vector2(0, 1);
            stripe.sizeDelta        = new Vector2(4, 0);
            stripe.anchoredPosition = Vector2.zero;
            stripe.gameObject.AddComponent<Image>().color = C_RED_BRIGHT;
            stripe.GetComponent<Image>().raycastTarget = false;

            _logoText = MakeText(hdr.gameObject, "Sup", 26, true,
                new Vector2(-10, 0), new Vector2(500, 56));
            _logoText.color     = C_WHITE;
            _logoText.alignment = TextAnchor.MiddleCenter;

            var chip = UIRect("VerChip", hdr);
            chip.anchorMin       = new Vector2(1, 0.5f);
            chip.anchorMax       = new Vector2(1, 0.5f);
            chip.sizeDelta       = new Vector2(80, 20);
            chip.anchoredPosition = new Vector2(-48, 0);
            var ci = chip.gameObject.AddComponent<Image>();
            ci.sprite = MakeRoundedRectSprite(5);
            ci.type   = Image.Type.Sliced;
            ci.color  = C_RED_DIM;
            ci.raycastTarget = false;
            var ct = MakeText(chip.gameObject, "v 5.01", 10, false, Vector2.zero, new Vector2(78, 20));
            ct.color = C_WHITE;

            var sep = UIRect("HeaderSep", panel);
            sep.anchorMin        = new Vector2(0, 1);
            sep.anchorMax        = new Vector2(1, 1);
            sep.sizeDelta        = new Vector2(0, 2);
            sep.anchoredPosition = new Vector2(0, -62);
            sep.gameObject.AddComponent<Image>().color = C_RED;
            sep.GetComponent<Image>().raycastTarget = false;
        }

        void BuildProgressBar(RectTransform panel)
        {
            var barBg = UIRect("ProgressBG", panel);
            barBg.anchorMin        = new Vector2(0, 1);
            barBg.anchorMax        = new Vector2(1, 1);
            barBg.sizeDelta        = new Vector2(-28, 5);
            barBg.anchoredPosition = new Vector2(0, -68);
            var bgi = barBg.gameObject.AddComponent<Image>();
            bgi.color = new Color(0.14f, 0.07f, 0.07f);
            bgi.raycastTarget = false;

            var fill = UIRect("ProgressFill", barBg);
            fill.anchorMin        = new Vector2(0, 0);
            fill.anchorMax        = new Vector2(0, 1);
            fill.sizeDelta        = Vector2.zero;
            fill.pivot            = new Vector2(0, 0.5f);
            fill.anchoredPosition = Vector2.zero;
            _progressFill = fill.gameObject.AddComponent<Image>();
            _progressFill.color = C_RED_BRIGHT;
            _progressFill.raycastTarget = false;
        }

        void AddCornerAccents(RectTransform panel)
        {

            var corners = new Vector2[] { new Vector2(0,1), new Vector2(1,1), new Vector2(0,0), new Vector2(1,0) };
            for (int c = 0; c < 4; c++)
            {
                float sx = (c % 2 == 0) ? 1 : -1;
                float sy = (c < 2) ? -1 : 1;

                MakeCornerBar(panel, "CH" + c, corners[c], new Vector2(sx * 22, sy * 2));
                MakeCornerBar(panel, "CV" + c, corners[c], new Vector2(sx * 2, sy * 22));
            }
        }

        void MakeCornerBar(RectTransform parent, string name, Vector2 anchor, Vector2 size)
        {
            var rt = UIRect(name, parent);
            rt.anchorMin        = anchor;
            rt.anchorMax        = anchor;
            rt.pivot            = anchor;
            rt.sizeDelta        = size;
            rt.anchoredPosition = Vector2.zero;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = C_RED_BRIGHT;
            img.raycastTarget = false;
        }

        void BuildTab1_Welcome()
        {
            var tab = NewTabContainer("Tab_Welcome");

            var title = MakeText(tab, "Welcome to M5", 38, true, new Vector2(0, 130), new Vector2(580, 60));
            title.color = C_WHITE;
            Decor_HLine(tab, new Vector2(0, 98), 200);

            var sub = MakeText(tab, "Configure your ModernBox experience", 13, false, new Vector2(0, 76), new Vector2(420, 26));
            sub.color = C_GREY;

            var body = MakeText(tab, "Welcome to ModernBox, change these settings or you'll be executed by the state.", 14, false, new Vector2(0, 20), new Vector2(520, 72));
            body.color = new Color(0.72f, 0.62f, 0.62f);

            MakePrimaryButton(tab, "BEGIN  →", new Vector2(0, -148), NextTab);
        }

        void BuildTab2_Era()
        {
            var tab = NewTabContainer("Tab_Era");
            SectionHeader(tab, "TECHNOLOGY PROGRESSION", "How fast should kingdoms advance through eras?", 1);

            string[] titles = { "RAPID", "STANDARD", "SLOW", "REALISM" };
            string[] descs  =
            {
                "Modern tech arrives almost immediately.",
                "50–200 years between eras.",
                "Things take longer than usual.",
                "Thousands of years may pass."
            };
            EraProgressMode[] modes = { EraProgressMode.Fast, EraProgressMode.Default, EraProgressMode.Slow, EraProgressMode.Realism };

            float totalW = 4 * 138f + 3 * 6f;
            float startX = -totalW / 2f + 69f;

            for (int i = 0; i < 4; i++)
            {
                EraProgressMode capturedMode = modes[i];
                var card = MakeOptionCard(tab, titles[i], descs[i], new Vector2(startX + i * 144f, -28f), new Vector2(138, 162));
                _eraCards.Add(card.GetComponent<Image>());
                var btn = card.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectEra(capturedMode));
            }

            MakePrimaryButton(tab, "NEXT  →", new Vector2(0, -188), NextTab);
        }

        void BuildTab3_Balance()
        {
            var tab = NewTabContainer("Tab_Balance");
            SectionHeader(tab, "COMBAT BALANCE", "How powerful should modern weapons be?", 2);

            string[] titles = { "WEAK", "VANILLA", "ASYM.", "OG MODE", "CARNAGE" };
            string[] descs  =
            {
                "Vanilla units still hold their own.",
                "As if the devs shipped this.",
                "Modern armies dominate older ones.",
                "Classic 2023 chaos.",
                "Pure destruction. Lag warning."
            };
            BalanceMode[] modes =
            {
                BalanceMode.Weak, BalanceMode.VanillaFriendly,
                BalanceMode.AssymetricalWarfare, BalanceMode.OGModernBox, BalanceMode.Carnage
            };

            float totalW = 5 * 112f + 4 * 5f;
            float startX = -totalW / 2f + 56f;

            for (int i = 0; i < 5; i++)
            {
                BalanceMode capturedMode = modes[i];
                var card = MakeOptionCard(tab, titles[i], descs[i], new Vector2(startX + i * 117f, -28f), new Vector2(112, 162));
                _balCards.Add(card.GetComponent<Image>());
                card.GetComponent<Button>().onClick.AddListener(() => SelectBalance(capturedMode));
            }

            MakePrimaryButton(tab, "NEXT  →", new Vector2(0, -188), () => { ModernBoxPrefs.Save(); NextTab(); });
        }

        void BuildTab4_Discord()
        {
            var tab = NewTabContainer("Tab_Discord");
            SectionHeader(tab, "JOIN THE COMMUNITY", "Want to join the Discord server?", 3);

            var note = MakeText(tab, "Get patch notes, report bugs, and chat with other people who play the mod.", 13, false, new Vector2(0, 66), new Vector2(480, 28));
            note.color = C_GREY;

            var yesCard = MakeOptionCard(tab, "JOIN", "Open Discord\nand join the\nserver now.", new Vector2(-120, -28f), new Vector2(188, 162));

            _disCards.Add(yesCard.GetComponent<Image>());

            yesCard.GetComponent<Button>().onClick.AddListener(() => { Application.OpenURL("https://discord.gg/hUFdZPXbYE");  });

            SelectDiscord(false);

            MakePrimaryButton(tab, "SAVE & CONTINUE  →", new Vector2(0, -188), () => { ModernBoxPrefs.Save(); NextTab(); });
        }

        void BuildTab5_Done()
        {
            var tab = NewTabContainer("Tab_Done");

            var title = MakeText(tab, "RESTART REQUIRED", 30, true, new Vector2(0, 130), new Vector2(580, 56));
            title.color = C_RED_BRIGHT;
            Decor_HLine(tab, new Vector2(0, 100), 270);

            var body = MakeText(tab,
                "Settings saved successfully.\n\n" +
                "This UI disables other game systems to render.\n" +
                "Please exit and relaunch to apply your configuration.",
                14, false, new Vector2(0, 14), new Vector2(520, 112));
            body.color = new Color(0.72f, 0.62f, 0.62f);

            MakePrimaryButton(tab, "EXIT GAME", new Vector2(0, -148), Application.Quit, danger: true);
        }

        void SelectEra(EraProgressMode mode)
        {
            ModernBoxPrefs.EraMode = mode;
            ModernBoxPrefs.Save();
            int index = (int)mode;
            for (int i = 0; i < _eraCards.Count; i++)
                StartCoroutine(CR_TweenColor(_eraCards[i], i == index ? C_CARD_SEL : C_CARD_IDLE, 0.14f));
        }

        void SelectBalance(BalanceMode mode)
        {
            ModernBoxPrefs.Balance = mode;
            ModernBoxPrefs.Save();
            int index = (int)mode;
            for (int i = 0; i < _balCards.Count; i++)
                StartCoroutine(CR_TweenColor(_balCards[i], i == index ? C_CARD_SEL : C_CARD_IDLE, 0.14f));
        }

        void SelectDiscord(bool join)
        {
            if (_disCards.Count < 2) return;
            StartCoroutine(CR_TweenColor(_disCards[0], join  ? C_CARD_SEL : C_CARD_IDLE, 0.14f));
            StartCoroutine(CR_TweenColor(_disCards[1], !join ? C_CARD_SEL : C_CARD_IDLE, 0.14f));

            if (join)
                Application.OpenURL("https://discord.gg/hUFdZPXbYE"); 

        }

        void RefreshAllSelections()
        {
            SelectEra(ModernBoxPrefs.EraMode);
            SelectBalance(ModernBoxPrefs.Balance);
            SelectDiscord(false);
        }

        void LayoutTabs()
        {
            for (int i = 0; i < _tabs.Count; i++)
                _tabs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 660f, 0);
        }

        public void NextTab()
        {
            if (_currentTab >= _tabs.Count - 1) return;
            _currentTab++;
            UpdateProgress(animate: true);
            StopCoroutine("CR_SlideToTab");
            StartCoroutine(CR_SlideToTab(_currentTab));
        }

        void UpdateProgress(bool animate)
        {
            if (_progressFill == null) return;
            float target = _currentTab / (float)(_tabs.Count - 1);
            if (animate)
                StartCoroutine(CR_TweenProgressBar(target));
            else
                _progressFill.GetComponent<RectTransform>().anchorMax = new Vector2(target, 1);
        }

        IEnumerator CR_SlideToTab(int index)
        {
            StartCoroutine(CR_FlashBorder());

            Vector2 from = _contentRoot.anchoredPosition;
            Vector2 to   = new Vector2(-index * 660f, 0);
            float elapsed = 0f, dur = 0.42f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t  = elapsed / dur;

                float s  = t < 0.5f ? 4 * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                _contentRoot.anchoredPosition = Vector2.Lerp(from, to, s);
                yield return null;
            }
            _contentRoot.anchoredPosition = to;
        }

        IEnumerator CR_TweenProgressBar(float target)
        {
            var rt    = _progressFill.GetComponent<RectTransform>();
            float cur = rt.anchorMax.x;
            float t   = 0f;
            while (t < 0.38f)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(cur, target, Mathf.SmoothStep(0, 1, t / 0.38f));
                rt.anchorMax = new Vector2(v, 1);
                yield return null;
            }
            rt.anchorMax = new Vector2(target, 1);
        }

        IEnumerator CR_PulseBorder()
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * 1.6f) + 1f) * 0.5f;
                if (_panelBorder != null)
                    _panelBorder.color = Color.Lerp(C_RED_DIM, C_RED_BRIGHT, t);
                yield return null;
            }
        }

        IEnumerator CR_FlashBorder()
        {
            if (_panelBorder == null) yield break;
            _panelBorder.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            _panelBorder.color = C_RED_BRIGHT;
        }

        IEnumerator CR_FadeIn(GameObject go, float duration)
        {
            var cg   = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            float t  = 0f;
            while (t < duration)
            {
                t       += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(t / duration);
                yield return null;
            }
            cg.alpha = 1f;
        }

        IEnumerator CR_GlitchText(Text txt)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#%^&*<>?/|";
            string original    = txt.text;
            for (int g = 0; g < 3; g++)
            {
                char[] arr = original.ToCharArray();
                arr[Random.Range(0, arr.Length)] = chars[Random.Range(0, chars.Length)];
                txt.text  = new string(arr);
                txt.color = C_RED_BRIGHT;
                yield return new WaitForSeconds(0.04f);
                txt.text  = original;
                txt.color = C_WHITE;
                yield return new WaitForSeconds(0.03f);
            }
        }

        IEnumerator CR_TweenColor(Image img, Color target, float duration)
        {
            if (img == null) yield break;
            Color start = img.color;
            float t     = 0f;
            while (t < duration)
            {
                t        += Time.deltaTime;
                img.color = Color.Lerp(start, target, t / duration);
                yield return null;
            }
            img.color = target;
        }

        IEnumerator CR_Particles()
        {
            var speeds = new float[PARTICLE_COUNT];
            var drifts = new float[PARTICLE_COUNT];
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                speeds[i] = Random.Range(14f, 52f);
                drifts[i] = Random.Range(-6f, 6f);
            }
            while (true)
            {
                for (int i = 0; i < _particles.Count; i++)
                {
                    var pos  = _particles[i].anchoredPosition;
                    pos.y   += speeds[i] * Time.deltaTime;
                    pos.x   += drifts[i] * Time.deltaTime * 0.28f;
                    if (pos.y > 390f) { pos.y = -390f; pos.x = Random.Range(-640f, 640f); }
                    _particles[i].anchoredPosition = pos;
                }
                yield return null;
            }
        }

        IEnumerator CR_ScrollScanlines()
        {
            float offset = 0f;
            while (true)
            {
                offset                       += Time.deltaTime * 16f;
                _scanlineRT.anchoredPosition  = new Vector2(0, -(offset % 4f));
                yield return null;
            }
        }

        GameObject NewTabContainer(string name)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(_contentRoot, false);
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(660, 415);
            rt.anchoredPosition = Vector2.zero;
            _tabs.Add(go);
            return go;
        }

        void SectionHeader(GameObject tab, string title, string subtitle, int step)
        {
            var t = MakeText(tab, title, 24, true, new Vector2(0, 168), new Vector2(580, 42));
            t.color = C_WHITE;
            Decor_HLine(tab, new Vector2(0, 144), 300);

            var st = MakeText(tab, subtitle, 12, false, new Vector2(0, 122), new Vector2(520, 24));
            st.color = C_GREY;

            var sl = MakeText(tab, "STEP " + step + " / 4", 10, false, new Vector2(236, 168), new Vector2(100, 22));
            sl.color     = C_RED_DIM;
            sl.alignment = TextAnchor.MiddleRight;
        }

        GameObject MakeOptionCard(GameObject parent, string title, string desc, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Card_" + title);
            go.transform.SetParent(parent.transform, false);

            var rt          = go.AddComponent<RectTransform>();
            rt.sizeDelta    = size;
            rt.anchoredPosition = pos;

            var img    = go.AddComponent<Image>();
            img.sprite = MakeRoundedRectSprite(7);
            img.type   = Image.Type.Sliced;
            img.color  = C_CARD_IDLE;

            var btn    = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor      = C_CARD_IDLE;
            colors.highlightedColor = C_CARD_HOV;
            colors.pressedColor     = C_RED_DIM;
            colors.fadeDuration     = 0.12f;
            btn.colors = colors;

            var brt = UIRect("Border", rt);
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.sizeDelta = new Vector2(2, 2);
            var bi = brt.gameObject.AddComponent<Image>();
            bi.sprite     = MakeBorderSprite(2);
            bi.type       = Image.Type.Sliced;
            bi.color      = new Color(0.28f, 0.14f, 0.14f);
            bi.fillCenter = false;
            bi.raycastTarget = false;

            var top = UIRect("TopAccent", rt);
            top.anchorMin        = new Vector2(0, 1);
            top.anchorMax        = new Vector2(1, 1);
            top.sizeDelta        = new Vector2(0, 3);
            top.anchoredPosition = Vector2.zero;
            var ti = top.gameObject.AddComponent<Image>();
            ti.color = C_RED_DIM;
            ti.raycastTarget = false;

            var tit = MakeText(go, title, 12, true, new Vector2(0, size.y * 0.22f), new Vector2(size.x - 12, 26));
            tit.color = C_WHITE;

            var dsc = MakeText(go, desc, 9, false, new Vector2(0, -size.y * 0.12f), new Vector2(size.x - 14, size.y * 0.55f));
            dsc.color = C_GREY;

            return go;
        }

        GameObject MakePrimaryButton(GameObject parent, string label, Vector2 pos,
            UnityEngine.Events.UnityAction onClick, bool danger = false)
        {
            var go = new GameObject("PrimaryBtn");
            go.transform.SetParent(parent.transform, false);

            var rt          = go.AddComponent<RectTransform>();
            rt.sizeDelta    = new Vector2(220, 46);
            rt.anchoredPosition = pos;

            var img    = go.AddComponent<Image>();
            img.sprite = MakeRoundedRectSprite(6);
            img.type   = Image.Type.Sliced;
            img.color  = danger ? new Color(0.48f, 0.04f, 0.04f) : C_RED;

            var btn    = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor      = danger ? new Color(0.48f, 0.04f, 0.04f) : C_RED;
            colors.highlightedColor = C_RED_BRIGHT;
            colors.pressedColor     = C_RED_DIM;
            colors.fadeDuration     = 0.10f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var shine = UIRect("Shine", rt);
            shine.anchorMin = new Vector2(0, 0.5f); shine.anchorMax = new Vector2(1, 1);
            shine.sizeDelta = Vector2.zero;
            var si = shine.gameObject.AddComponent<Image>();
            si.color = new Color(1, 1, 1, 0.05f);
            si.raycastTarget = false;

            var t = MakeText(go, label, 13, true, Vector2.zero, new Vector2(210, 46));
            t.color = C_WHITE;

            return go;
        }

        void Decor_HLine(GameObject parent, Vector2 pos, float width)
        {
            var rt = UIRect("HLine", parent.GetComponent<RectTransform>());
            rt.anchoredPosition = pos;
            rt.sizeDelta        = new Vector2(width, 2);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = C_RED;
            img.raycastTarget = false;
        }

        Text MakeText(GameObject parent, string text, int size, bool bold, Vector2 pos, Vector2 area)
        {
            var go = new GameObject("Txt");
            go.transform.SetParent(parent.transform, false);
            var t = go.AddComponent<Text>();
            t.text              = text;
            t.font              = _font;
            t.fontSize          = size;
            t.color             = C_WHITE;
            t.alignment         = TextAnchor.MiddleCenter;
            t.fontStyle         = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow   = VerticalWrapMode.Overflow;
            t.raycastTarget     = false;   

            var rt              = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = area;
            return t;
        }

        static RectTransform UIRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        static Sprite MakeRoundedRectSprite(int radius)
        {
            int size = radius * 2 + 4;
            var tex  = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
            var ctr  = new Vector2(size / 2f, size / 2f);
            float hr = size / 2f - radius;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx    = Mathf.Max(0, Mathf.Abs(x - ctr.x) - hr);
                    float dy    = Mathf.Max(0, Mathf.Abs(y - ctr.y) - hr);
                    float alpha = Mathf.Clamp01(1f - (Mathf.Sqrt(dx * dx + dy * dy) - radius + 0.5f));
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            tex.Apply();
            float r = radius;
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f,
                100, 0, SpriteMeshType.Tight, new Vector4(r, r, r, r));
        }

        static Sprite MakeBorderSprite(int thickness)
        {
            int size = 16;
            var tex  = new Texture2D(size, size) { filterMode = FilterMode.Point };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool border = x < thickness || x >= size - thickness ||
                                y < thickness || y >= size - thickness;
                    tex.SetPixel(x, y, border ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f,
                100, 0, SpriteMeshType.Tight, new Vector4(thickness, thickness, thickness, thickness));
        }

        static Sprite MakeScanlineSprite(int lineHeight)
        {
            int w = 4, h = lineHeight * 2;
            var tex = new Texture2D(w, h) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, y < lineHeight ? Color.black : Color.clear);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f,
                100, 0, SpriteMeshType.FullRect, new Vector4(0, 0, 0, 0));
        }

        static Sprite MakeRadialGradientSprite(int size, Color inner, Color outer)
        {
            var tex    = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
            var center = new Vector2(size / 2f, size / 2f);
            float maxD = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float t = Vector2.Distance(new Vector2(x, y), center) / maxD;
                    tex.SetPixel(x, y, Color.Lerp(inner, outer, Mathf.Clamp01(t)));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        }
    }
}