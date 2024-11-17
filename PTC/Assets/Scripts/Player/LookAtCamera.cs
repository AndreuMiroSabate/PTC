using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            // Calculate the direction from the object to the camera
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;

            // Invert the direction to look away
            Vector3 lookAwayDirection = -directionToCamera;

            // Rotate the object to look away
            transform.rotation = Quaternion.LookRotation(lookAwayDirection);
        }
    }
}
