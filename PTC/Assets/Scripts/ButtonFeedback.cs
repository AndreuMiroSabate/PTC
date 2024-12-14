using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f); // Scale when hovered
    public float duration = 0.2f; // Duration for scaling animation

    private Vector3 originalScale;

    void Start()
    {
        // Store the original scale of the button
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up the button on hover
        transform.DOScale(hoverScale, duration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale back to original size when not hovering
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
}
