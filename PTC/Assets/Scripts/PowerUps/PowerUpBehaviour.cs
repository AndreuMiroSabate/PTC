using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Ensure you include the DOTween namespace

public class PowerUpBehaviour : MonoBehaviour
{
    [SerializeField] PowerUps myPowerUp;

    [Header("Movement Settings")]
    public float moveDistance = 2f; // Distance to move up and down
    public float moveDuration = 2f; // Time for one up-down cycle

    [Header("Rotation Settings")]
    public Vector3 rotationAngles = new Vector3(0, 180, 0); // Rotation per cycle
    public float rotationDuration = 2f; // Time for one rotation cycle

    [Header("Animation Settings")]
    public bool loop = true; // Should the animations loop
    public LoopType loopType = LoopType.Yoyo; // Loop type for movement

    public PowerUps GetPowerUp()
    {
        return myPowerUp;
    }

    private void Start()
    {
        AnimateObject();
    }

    private void AnimateObject()
    {
        // Move up and down
        transform.DOMoveY(transform.position.y + moveDistance, moveDuration)
            .SetEase(Ease.InOutSine) // Smooth easing for natural movement
            .SetLoops(loop ? -1 : 0, loopType); // Looping based on settings

        // Rotate continuously
        transform.DORotate(rotationAngles, rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear) // Linear easing for constant rotation
            .SetLoops(loop ? -1 : 0); // Looping based on settings
    }
}
