using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ModernBox
{
    public class StupidWorldboxia : MonoBehaviour
    {
        private Canvas canvas;
        private Text textField;
        private Button okButton;
        private Button joinButton;
        private List<GameObject> disabledObjects = new List<GameObject>();

        private string textywexty = @"I was going to take this alert down due to some agreements but certain people in the WorldBox server continued to be a hinderance.
        
        Hi! Sorry for this interruption (a button will appear soon to allow you to skip), but this is important.

        The Worldbox discord server is a filthy place with horrible leadership and horrible members, who have hindered modding for a long time. 
        Modders like Melvin and Myoppie (also known as ahoyos) have been previously been banned from the server and unable to share their mods. 
        ModernBox itself is no longer available on Gamebanana because it would be taken down if this update were on it.
        (due to calling out the server).

        I will be releasing a huge update for ModernBox soon, and a special person is returning, but Opus (which adds Multiplayer) will NOT be released until the MWGA movement is allowed to stay in Worldboxia.


        the MWGA movement (Make Worldboxia Great Again) is a slogan to bring light on how modding has been affected by the Worldbox server. I also propose moving modding away from there & Gamebanana and moving entirely to Steam, so the WorldBox server has no say in which mods get to stay and which don't.
        If you agree with any of this, go to the WorldBox server's #general channel and type 'Make Worldboxia Great Again', or, if that is automodded, say something similar. If all else fails, just say 'for ModernBox'.
        I do not think we should allow someone like Adiniz who literally shipped me (minor) with a adult to be a mod manager.
        ";


        void Awake()
        {
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj != this.gameObject)
                {
                    disabledObjects.Add(obj);
                    obj.SetActive(false);
                }
            }

            GameObject canvasGO = new GameObject("IntroCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject bg = new GameObject("BlackBG");
            bg.transform.SetParent(canvas.transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = Color.black;
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            GameObject textGO = new GameObject("TypewriterText");
            textGO.transform.SetParent(canvas.transform, false);
            textField = textGO.AddComponent<Text>();
            textField.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textField.color = Color.white;
            textField.alignment = TextAnchor.MiddleCenter;
            textField.fontSize = 18;
            textField.horizontalOverflow = HorizontalWrapMode.Wrap;
            textField.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.1f, 0.2f);
            textRT.anchorMax = new Vector2(0.9f, 0.8f);
            textRT.sizeDelta = Vector2.zero;

            GameObject buttonGO = CreateButton("OKButton", "OK", new Vector2(-90, 0), OnOKPressed);
            okButton = buttonGO.GetComponent<Button>();

            GameObject joinButtonGO = CreateButton("JoinButton", "Join a BETTER Server", new Vector2(90, 0), OnJoinPressed);
            joinButton = joinButtonGO.GetComponent<Button>();

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            StartCoroutine(ShowTypewriterText());
        }

        private GameObject CreateButton(string name, string label, Vector2 offset, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(canvas.transform, false);

            Image img = buttonGO.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0);
            Button btn = buttonGO.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 60);
            rt.anchorMin = new Vector2(0.5f, 0.1f);
            rt.anchorMax = new Vector2(0.5f, 0.1f);
            rt.anchoredPosition = offset;

            GameObject txtGO = new GameObject("ButtonText");
            txtGO.transform.SetParent(buttonGO.transform, false);
            Text txt = txtGO.AddComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.color = new Color(0, 0, 0, 0);
            txt.alignment = TextAnchor.MiddleCenter;
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;

            return buttonGO;
        }

        private IEnumerator ShowTypewriterText()
        {
            textField.text = "";
            foreach (char c in textywexty)
            {
                textField.text += c;
                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(10f);
            StartCoroutine(FadeInButtons());
        }

        private IEnumerator FadeInButtons()
        {
            Image okImg = okButton.GetComponent<Image>();
            Text okText = okButton.GetComponentInChildren<Text>();

            Image joinImg = joinButton.GetComponent<Image>();
            Text joinText = joinButton.GetComponentInChildren<Text>();

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = Mathf.SmoothStep(0, 1, t);

                okImg.color = new Color(1, 1, 1, a);
                okText.color = new Color(0, 0, 0, a);

                joinImg.color = new Color(1, 1, 1, a);
                joinText.color = new Color(0, 0, 0, a);

                yield return null;
            }
        }

        private void OnOKPressed()
        {
            foreach (GameObject obj in disabledObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
            Destroy(canvas.gameObject);
            Destroy(this);
        }

        private void OnJoinPressed()
        {
            Application.OpenURL("https://discord.gg/c2fSfqvcdV");
        }
    }
}