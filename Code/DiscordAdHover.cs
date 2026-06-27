using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiscordAdHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isHovered = false;
    private Image image;
    private Vector3 maxScale = Vector3.one * 1.25f;
    private Color hoverColor = Color.yellow;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (image != null)
        {
            image.color = hoverColor;
            image.transform.localScale = maxScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
