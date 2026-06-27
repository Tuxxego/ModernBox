using UnityEngine;
using UnityEngine.UI;

public class GayAndStupid : MonoBehaviour
{
    public Sprite forcedSprite;
    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void LateUpdate()
    {
        if (img != null && forcedSprite != null)
        {
            img.sprite = forcedSprite;
        }
    }
}