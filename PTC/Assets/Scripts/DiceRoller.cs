using UnityEngine;
using DG.Tweening; // Ensure this is included for DoTween
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    public float rollForce = 10f; // The upward force applied to the dice
    public float rotationAmount = 720f; // Total degrees of rotation on the X-axis
    public float rollDuration = 2f; // Duration of the roll in seconds

    public Rigidbody[] dices_rb; // Rigidbody of the dice
    public Transform[] dices_transform; // Transform of the dice

    public void RollDice()
    {
        rotationAmount += 90;

        for (int i = 0; i < dices_rb.Length; i++)
        {
            // Reset velocity and angular velocity to ensure consistent rolls
            dices_rb[i].velocity = Vector3.zero;
            dices_rb[i].angularVelocity = Vector3.zero;

            // Apply an upward force to make the dice "jump"
            dices_rb[i].AddForce(Vector3.up * rollForce, ForceMode.Impulse);

            // Rotate the dice on the X-axis
            StartCoroutine(RotateDiceOnX(i));
        }
    }

    private IEnumerator RotateDiceOnX(int index)
    {
        // Wait for a short duration to simulate the physics-based upward motion
        yield return new WaitForSeconds(0.1f);

        // Smoothly rotate along the X-axis
        dices_transform[index].DOLocalRotate(new Vector3(rotationAmount, 0, 0), rollDuration).SetEase(Ease.OutCubic);

        // Wait for the roll duration to complete
        yield return new WaitForSeconds(rollDuration);

        // Snap to a final rotation (ensure one face lands upwards)
        SnapToFinalRotation();
    }

    private void SnapToFinalRotation()
    {
        // Define possible face-up rotations for the dice
        Quaternion[] faceRotations = {
            Quaternion.Euler(0, 0, 0),       // Face 1 up
            Quaternion.Euler(90, 0, 0),     // Face 2 up
            Quaternion.Euler(180, 0, 0),    // Face 3 up
            Quaternion.Euler(270, 0, 0),    // Face 4 up
            Quaternion.Euler(0, 90, 0),     // Face 5 up
            Quaternion.Euler(0, 270, 0)     // Face 6 up
        };

        // Pick a random rotation
        Quaternion finalRotation = faceRotations[Random.Range(0, faceRotations.Length)];

        // Instantly snap to the final rotation
        transform.rotation = finalRotation;
    }
}
