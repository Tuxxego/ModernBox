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
        // Optimization: Only assign sprite when it actually differs to avoid dirtying UI Image every frame.
        if (img != null && forcedSprite != null && img.sprite != forcedSprite)
        {
            img.sprite = forcedSprite;
        }
    }
}
