using UnityEngine;
using DG.Tweening;

public class ScaleTween : MonoBehaviour
{
    [SerializeField] private Vector3 targetScale = new Vector3(2f, 2f, 2f); // Target scale size
    [SerializeField] private float duration = 1f; // Duration of the scaling animation

    private Vector3 originalScale;

    void Start()
    {
        // Save the original scale of the object
        originalScale = transform.localScale;

        // Start the scaling tween
        StartScaling();
    }

    private void StartScaling()
    {
        // Scale to the target size and back to the original size repeatedly
        transform.DOScale(targetScale, duration)
            .SetEase(Ease.InOutSine) // Smooth easing for the animation
            .OnComplete(() =>
            {
                // Scale back to original size after reaching target
                transform.DOScale(originalScale, duration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(StartScaling); // Loop the scaling
            });
    }
}
