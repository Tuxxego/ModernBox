using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NCMS;
using tools;
using NCMS.Utils;

namespace ModernBox
{
    public class YourMomIsFat : MonoBehaviour
    {
        private static Color? mb_originalScrollColor = null;
        public GameObject e;
        public GameObject targetObj;
        private Image targetImg;
        private RectTransform targetRT;
        private Mask targetMask;
        private GameObject bombsTab;
        private GameObject ErasTab;
        private GameObject ItemsTab;
        private GameObject SpaceTab;
        private GameObject MainTab;
        private readonly List<Image> bombParticles = new List<Image>();
        private float particleTimer = 0f;
        private float lookupTimer = 0f;
        private const int MaxBombParticles = 60;

        void Update()
        {
            // Optimization: Only search scene hierarchy at most once every 0.5s when references are missing
            if (e == null || bombsTab == null || ErasTab == null || ItemsTab == null || SpaceTab == null || MainTab == null || targetObj == null)
            {
                lookupTimer -= Time.unscaledDeltaTime;
                if (lookupTimer <= 0f)
                {
                    lookupTimer = 0.5f;
                    if (e == null) e = GameObject.Find("ModernBoxUnits") ?? GameObjects.FindEvenInactive("ModernBoxUnits");
                    if (bombsTab == null) bombsTab = GameObject.Find("ModernBoxBombs") ?? GameObjects.FindEvenInactive("ModernBoxBombs");
                    if (ErasTab == null) ErasTab = GameObject.Find("ModernBoxEras") ?? GameObjects.FindEvenInactive("ModernBoxEras");
                    if (ItemsTab == null) ItemsTab = GameObject.Find("ModernBoxItems") ?? GameObjects.FindEvenInactive("ModernBoxItems");
                    if (SpaceTab == null) SpaceTab = GameObject.Find("ModernBoxSpace") ?? GameObjects.FindEvenInactive("ModernBoxSpace");
                    if (MainTab == null) MainTab = GameObject.Find("ModernBoxTab") ?? GameObjects.FindEvenInactive("ModernBoxTab");

                    if (targetObj == null)
                    {
                        targetObj = GameObject.Find("Canvas Container Main/Canvas - UI/General/CanvasBottom/BottomElements/BottomElementsMover/CanvasScrollView/Scroll View");
                        if (targetObj == null)
                        {
                            GameObject scrollParent = GameObjects.FindEvenInactive("CanvasScrollView");
                            if (scrollParent != null)
                            {
                                Transform t = scrollParent.transform.Find("Scroll View");
                                if (t != null) targetObj = t.gameObject;
                            }
                        }
                        if (targetObj == null) targetObj = GameObjects.FindEvenInactive("Scroll View");
                    }
                    if (targetObj != null)
                    {
                        targetImg = targetObj.GetComponent<Image>();
                        targetRT = targetObj.GetComponent<RectTransform>();
                    }
                }
            }

            if (targetObj == null) return;
            if (targetImg == null)
            {
                targetImg = targetObj.GetComponent<Image>();
                if (targetImg == null) return;
            }

            if (mb_originalScrollColor == null && targetImg.color != Color.red && targetImg.color != new Color(0.4f, 0f, 0f))
            {
                mb_originalScrollColor = targetImg.color;
            }

            bool bombsActive = (bombsTab != null && bombsTab.activeInHierarchy);
            if (bombsActive)
            {
                targetImg.color = new Color(0.4f, 0f, 0f, 0.6f);
                HandleBombParticles(targetObj);
                return;
            }

            if (bombParticles.Count > 0)
            {
                ClearBombParticles(targetObj);
            }

            if (e != null && e.activeInHierarchy)
            {
                targetImg.color = Color.red;
            }
            else if (ErasTab != null && ErasTab.activeInHierarchy)
            {
                targetImg.color = new Color(0f, 0f, 0.5450981f, 0.6f);
            }
            else if (ItemsTab != null && ItemsTab.activeInHierarchy)
            {
                targetImg.color = new Color(0f, 0f, 0f, 0.6f);
            }
            else if (SpaceTab != null && SpaceTab.activeInHierarchy)
            {
                targetImg.color = new Color(0.04f, 0.06f, 0.12f, 0.6f);
            }
            else if (MainTab != null && MainTab.activeInHierarchy)
            {
                targetImg.color = new Color(0f, 0f, 0f, 0.6f);
            }
            else if (mb_originalScrollColor != null)
            {
                targetImg.color = mb_originalScrollColor.Value;
            }
        }

        void HandleBombParticles(GameObject parent)
        {
            if (targetRT == null) targetRT = parent.GetComponent<RectTransform>();
            if (targetMask == null)
            {
                targetMask = parent.GetComponent<Mask>();
                if (targetMask == null)
                {
                    targetMask = parent.AddComponent<Mask>();
                    targetMask.showMaskGraphic = true;
                }
            }

            particleTimer += Time.deltaTime;

            if (particleTimer > 0.08f && bombParticles.Count < MaxBombParticles)
            {
                particleTimer = 0f;

                GameObject p = new GameObject("bomb_particle");
                p.transform.SetParent(parent.transform, false);
                p.transform.SetAsFirstSibling();

                var img = p.AddComponent<Image>();
                img.color = Color.yellow;
                img.raycastTarget = false;

                RectTransform rt = p.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(4, 4);

                float width = targetRT.rect.width;
                float height = targetRT.rect.height;

                rt.anchoredPosition = new Vector2(
                    UnityEngine.Random.Range(-width / 2f, width / 2f),
                    UnityEngine.Random.Range(-height / 2f, height / 2f)
                );

                bombParticles.Add(img);
            }

            float halfHeight = targetRT.rect.height / 2f;
            float xOffset = Mathf.Sin(Time.time * 5f) * 0.5f;
            float yOffset = 50f * Time.deltaTime;

            for (int i = bombParticles.Count - 1; i >= 0; i--)
            {
                var p = bombParticles[i];
                if (p == null) { bombParticles.RemoveAt(i); continue; }

                RectTransform rt = p.rectTransform;
                rt.anchoredPosition += new Vector2(xOffset, yOffset);

                if (rt.anchoredPosition.y > halfHeight)
                {
                    Vector2 pos = rt.anchoredPosition;
                    pos.y = -halfHeight;
                    rt.anchoredPosition = pos;
                }

                Color c = p.color;
                float normalizeY = (rt.anchoredPosition.y + halfHeight) / (halfHeight * 2f);
                c.a = Mathf.Lerp(1f, 0.2f, normalizeY);
                p.color = c;
            }
        }

        void ClearBombParticles(GameObject parent)
        {
            for (int i = 0; i < bombParticles.Count; i++)
            {
                if (bombParticles[i] != null)
                    GameObject.Destroy(bombParticles[i].gameObject);
            }
            bombParticles.Clear();

            if (targetMask == null && parent != null)
                targetMask = parent.GetComponent<Mask>();
            if (targetMask != null)
            {
                GameObject.Destroy(targetMask);
                targetMask = null;
            }
        }
    }
}
