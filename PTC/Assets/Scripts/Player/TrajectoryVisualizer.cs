using System.Collections.Generic;
using UnityEngine;

public class TrajectoryVisualizer : MonoBehaviour
{
    public Transform firePoint; // Starting point of the projectile
    public float projectileSpeed = 10f; // Speed of the projectile
    public int maxBounces = 2; // Maximum number of bounces
    public LineRenderer lineRenderer; // Line Renderer to visualize the trajectory
    public LayerMask wallLayer; // Layer for walls to detect collisions

    void Update()
    {
        if (Input.GetMouseButton(0))
            DrawTrajectory();
        if (Input.GetMouseButtonUp(0))
            ClearTrajectory();
    }

    void DrawTrajectory()
    {
        // Initialize variables
        Vector3 currentPosition = firePoint.position;
        Vector3 direction = firePoint.forward; // Initial direction (local forward)
        List<Vector3> points = new List<Vector3>(); // List to store trajectory points

        points.Add(currentPosition); // Start from the firePoint

        for (int i = 0; i <= maxBounces; i++)
        {
            // Perform a raycast to detect the next collision
            if (Physics.Raycast(currentPosition, direction, out RaycastHit hit, Mathf.Infinity, wallLayer))
            {
                // Add the collision point to the trajectory
                points.Add(hit.point);

                // Reflect the direction vector based on the collision normal
                direction = Vector3.Reflect(direction, hit.normal);

                // Update the current position to the hit point
                currentPosition = hit.point;
            }
            else
            {
                // If no collision, extend the trajectory to the max range
                points.Add(currentPosition + direction * 100f); // Arbitrary large value for max range
                break;
            }
        }

        // Pass the calculated points to the LineRenderer
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    void ClearTrajectory()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0; // Reset the LineRenderer by clearing all positions
        }
    }
}
