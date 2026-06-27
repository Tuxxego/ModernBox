using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ModernBox
{  
    public class FirstTimeSetup : MonoBehaviour
    {
        private Canvas canvas;
        private Image spriteImage;
        private Image progressBarBG;
        private Image progressBarFill;
        private Text statusText;

        private Text thanksLabel;
        private Text currentName;
        private Text nextName;

        private string[] credits = new string[]
        {

            "Lead Developer:",
            "Tuxxego",

            "<color=yellow>Developer Team:</color>",
            "Dankmrgreen6444",
            "MORFOS",
            "Full Auto Sherman",
            "Goosefang",
            "Trike",
            "Ariel",
            "Mr. P",
            "Melvin Shwuaner",
            "Nico_the_Nine",
            "Myoppie",

            "M2 Dev Team:",
            "Fakher",
            "LonelyFear",
            "PlayerCro7",

            "Other Mod Developers to Credit:",
            "alexnitaly (Gunsmith Book)",
            "haydar_kara (Hivemind)",
            "3m1rh4n (RifleActions/WestActions)",

            "M1 Contributors:",
            "arielp2",
            "immortalglitch5500",
            "thedesertroad",
            "luck"
        };

        void Awake()
        {
            if (PlayerPrefs.HasKey("FirstTimeSetupDone6"))
            {
                Destroy(this);
                return;
            }

            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj != this.gameObject)
                    obj.SetActive(false);
            }

            GameObject canvasGO = new GameObject("SetupCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject bgGO = new GameObject("BlackBG");
            bgGO.transform.SetParent(canvas.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.black;
            bgImage.raycastTarget = false;
            RectTransform bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            StartCoroutine(RunSetup());
        }

        private IEnumerator RunSetup()
        {

            Sprite loadedSprite = Resources.Load<Sprite>("ui/icon");
            GameObject spriteGO = new GameObject("SetupSprite");
            spriteGO.transform.SetParent(canvas.transform, false);
            spriteImage = spriteGO.AddComponent<Image>();
            spriteImage.sprite = loadedSprite;
            spriteImage.color = new Color(1, 1, 1, 0);
            RectTransform rt = spriteGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(96, 66);
            rt.anchoredPosition = new Vector2(0, 30);
            rt.localScale = Vector3.zero;

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.2f;
                float alpha = Mathf.SmoothStep(0, 1, t);
                float scale = Mathf.SmoothStep(0.85f, 1f, t);
                spriteImage.color = new Color(1, 1, 1, alpha);
                rt.localScale = Vector3.one * scale;
                yield return null;
            }

            GameObject barBG = new GameObject("ProgressBarBG");
            barBG.transform.SetParent(canvas.transform, false);
            progressBarBG = barBG.AddComponent<Image>();
            progressBarBG.color = new Color(0.15f, 0.15f, 0.15f);
            RectTransform bgRT = barBG.GetComponent<RectTransform>();
            bgRT.sizeDelta = new Vector2(200, 12);
            bgRT.anchoredPosition = new Vector2(0, -20);
            progressBarBG.type = Image.Type.Sliced;

            GameObject barFill = new GameObject("ProgressBarFill");
            barFill.transform.SetParent(barBG.transform, false);
            progressBarFill = barFill.AddComponent<Image>();
            progressBarFill.color = new Color(0.2f, 1f, 0.5f);
            RectTransform fillRT = barFill.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0);
            fillRT.anchorMax = new Vector2(0, 1);
            fillRT.sizeDelta = new Vector2(0, 0);
            fillRT.anchoredPosition = Vector2.zero;

            GameObject textGO = new GameObject("StatusText");
            textGO.transform.SetParent(canvas.transform, false);
            statusText = textGO.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.fontSize = 14;
            statusText.color = new Color(0.9f, 0.9f, 0.9f);
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(250, 30);
            textRT.anchoredPosition = new Vector2(0, -50);

            string message = "Performing first time setup...";
            statusText.text = "";
            foreach (char c in message)
            {
                statusText.text += c;
                yield return new WaitForSeconds(0.04f);
            }

            GameObject thanksGO = new GameObject("ThanksLabel");
            thanksGO.transform.SetParent(canvas.transform, false);
            thanksLabel = thanksGO.AddComponent<Text>();
            thanksLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            thanksLabel.alignment = TextAnchor.MiddleCenter;
            thanksLabel.fontSize = 12;
            thanksLabel.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform thanksRT = thanksGO.GetComponent<RectTransform>();
            thanksRT.sizeDelta = new Vector2(300, 20);
            thanksRT.anchoredPosition = new Vector2(0, -100);
            thanksLabel.text = "THANKS TO";

            currentName = CreateNameText("CurrentName", new Vector2(0, -125));
            nextName = CreateNameText("NextName", new Vector2(0, -125));
            nextName.color = new Color(1, 1, 1, 0);

            StartCoroutine(RunCredits());

            float duration = 10f;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime + Random.Range(-0.01f, 0.01f);
                float progress = Mathf.Clamp01(elapsed / duration);
                fillRT.anchorMax = new Vector2(progress, 1);

                progressBarFill.color = Color.Lerp(
                    new Color(0.2f, 1f, 0.5f),
                    new Color(0.4f, 1f, 0.7f),
                    Mathf.PingPong(Time.time * 2f, 1f));

                yield return null;
            }

            GameObject balls = new GameObject("BAWLS");
            balls.AddComponent<ModernBoxShitty>();

            PlayerPrefs.SetInt("FirstTimeSetupDone6", 1);
            PlayerPrefs.Save();

        //    Application.Quit();
        }

        private Text CreateNameText(string name, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(canvas.transform, false);
            Text txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 14;
            txt.color = Color.white;
            RectTransform rt = txt.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 25);
            rt.anchoredPosition = anchoredPos;
            return txt;
        }

        private IEnumerator RunCredits()
        {
            int index = 0;
            currentName.text = credits[index];

            while (true)
            {
                yield return new WaitForSeconds(1f);
                index = (index + 1) % credits.Length;
                nextName.text = credits[index];

                yield return StartCoroutine(SlideTransition(currentName, nextName));

                Text temp = currentName;
                currentName = nextName;
                nextName = temp;
            }
        }

        private IEnumerator SlideTransition(Text oldText, Text newText)
        {
            float t = 0;
            RectTransform oldRT = oldText.GetComponent<RectTransform>();
            RectTransform newRT = newText.GetComponent<RectTransform>();

            newRT.anchoredPosition = new Vector2(0, -145);
            newText.color = new Color(1, 1, 1, 0);

            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                float fadeIn = Mathf.SmoothStep(0, 1, t);
                float fadeOut = Mathf.SmoothStep(1, 0, t);

                oldRT.anchoredPosition = new Vector2(0, -125 + t * 20f);
                oldText.color = new Color(1, 1, 1, fadeOut);

                float slide = Mathf.SmoothStep(-145, -125, t);
                newRT.anchoredPosition = new Vector2(0, slide);
                newText.color = new Color(1, 1, 1, fadeIn);

                yield return null;
            }
        }
    }
}