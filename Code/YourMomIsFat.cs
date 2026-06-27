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
        GameObject bombsTab;
        GameObject ErasTab;
        GameObject ItemsTab;
        GameObject SpaceTab;
        GameObject MainTab;
        List<Image> bombParticles = new List<Image>();
        float particleTimer = 0f;

        void Update()
        {

            if (e == null)
            {
                e = GameObject.Find("ModernBoxUnits");
                if (e == null) e = GameObjects.FindEvenInactive("ModernBoxUnits");
            }

            if (bombsTab == null)
            {
                bombsTab = GameObject.Find("ModernBoxBombs");
                if (bombsTab == null) bombsTab = GameObjects.FindEvenInactive("ModernBoxBombs");
            }

            if (ErasTab == null)
            {
                ErasTab = GameObject.Find("ModernBoxEras");
                if (ErasTab == null) ErasTab = GameObjects.FindEvenInactive("ModernBoxEras");
            }

            if (ItemsTab == null)
            {
                ItemsTab = GameObject.Find("ModernBoxItems");
                if (ItemsTab == null) ItemsTab = GameObjects.FindEvenInactive("ModernBoxItems");
            }

            if (SpaceTab == null)
            {
                SpaceTab = GameObject.Find("ModernBoxSpace");
                if (SpaceTab == null) SpaceTab = GameObjects.FindEvenInactive("ModernBoxSpace");
            }

            if (MainTab == null)
            {
                MainTab = GameObject.Find("ModernBoxTab");
                if (MainTab == null) MainTab = GameObjects.FindEvenInactive("ModernBoxTab");
            }

            if (targetObj == null)
            {
                targetObj = GameObject.Find("Canvas Container Main/Canvas - UI/General/CanvasBottom/BottomElements/BottomElementsMover/CanvasScrollView/Scroll View");
            }

            if (targetObj == null)
            {
                GameObject scrollParent = GameObjects.FindEvenInactive("CanvasScrollView");
                if (scrollParent != null)
                {
                    Transform t = scrollParent.transform.Find("Scroll View");
                    if (t != null) targetObj = t.gameObject;
                }
            }

            if (targetObj == null)
            {
                targetObj = GameObjects.FindEvenInactive("Scroll View");
            }

            if (targetObj == null) return;

            var img = targetObj.GetComponent<UnityEngine.UI.Image>();
            if (img == null) return;

            if (mb_originalScrollColor == null && img.color != Color.red && img.color != new Color(0.4f, 0f, 0f))
            {
                mb_originalScrollColor = img.color;
            }

            bool unitsActive = (e != null && e.activeInHierarchy);
            bool bombsActive = (bombsTab != null && bombsTab.activeInHierarchy);
            bool ErasActive = (ErasTab != null && ErasTab.activeInHierarchy);
            bool ItemsActive = (ItemsTab != null && ItemsTab.activeInHierarchy);
            bool SpaceActive = (SpaceTab != null && SpaceTab.activeInHierarchy);
            bool MainActive = (MainTab != null && MainTab.activeInHierarchy);
            if (bombsActive)
            {
                img.color = new Color(0.4f, 0f, 0f, 0.6f); 

                HandleBombParticles(targetObj);
            }
            else if (unitsActive)
            {
                img.color = Color.red;
                ClearBombParticles(targetObj); 

            }
            else if (ErasActive)
            {
                img.color = new Color(0f, 0f, 0.5450981f, 0.6f);
                ClearBombParticles(targetObj); 

            }
            else if (ItemsActive)
            {
            //    img.color = new Color(1f, 0.5490196f, 0f, 1f);
                img.color = new Color(0f, 0f, 0f, 0.6f);
                ClearBombParticles(targetObj); 

            }
            else if (SpaceActive)
            {
                img.color = new Color(0.04f, 0.06f, 0.12f, 0.6f);
                ClearBombParticles(targetObj); 
            }
            else if (MainActive)
            {
                img.color = new Color(0f, 0f, 0f, 0.6f);
                ClearBombParticles(targetObj); 
            }
            else
            {

                if (mb_originalScrollColor != null)
                {
                    img.color = mb_originalScrollColor.Value;
                }
                ClearBombParticles(targetObj);
            }
        }

        void HandleBombParticles(GameObject parent)
        {
            RectTransform parentRT = parent.GetComponent<RectTransform>();

            if (parent.GetComponent<UnityEngine.UI.Mask>() == null)
            {
                var mask = parent.AddComponent<UnityEngine.UI.Mask>();
                mask.showMaskGraphic = true; 
            }

            particleTimer += Time.deltaTime;

            if (particleTimer > 0.05f && bombParticles.Count < 500)
            {
                particleTimer = 0f;

                GameObject p = new GameObject("bomb_particle");
                p.transform.SetParent(parent.transform, false);
                p.transform.SetAsFirstSibling();

                var img = p.AddComponent<UnityEngine.UI.Image>();
                img.color = Color.yellow;

                RectTransform rt = p.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(4, 4);

                float width = parentRT.rect.width;
                float height = parentRT.rect.height;

                rt.anchoredPosition = new Vector2(
                    UnityEngine.Random.Range(-width / 2f, width / 2f),
                    UnityEngine.Random.Range(-height / 2f, height / 2f)
                );

                bombParticles.Add(img);
            }

            for (int i = bombParticles.Count - 1; i >= 0; i--)
            {
                var p = bombParticles[i];
                if (p == null) { bombParticles.RemoveAt(i); continue; }

                RectTransform rt = p.rectTransform;

                rt.anchoredPosition += new Vector2(
                    Mathf.Sin(Time.time * 5f) * 0.5f, 
                    50f * Time.deltaTime
                );

                float halfHeight = parentRT.rect.height / 2f;
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

            var mask = parent.GetComponent<UnityEngine.UI.Mask>();
            if (mask != null)
            {
                GameObject.Destroy(mask);
            }
        }
    }
}