using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public LayerMask wallLayerMask;  // Define what layers are considered walls
    public float raycastDistance = 20.0f;  // Distance for raycasting
    public float moveSpeed = 0.5f;  // Object speed when moving

    // Variables to store distances to walls in all directions
    private float leftWallDistance = 0f;
    private float rightWallDistance = 0f;
    private float upWallDistance = 0f;
    private float downWallDistance = 0f;
    private Vector3 currentDirection = new Vector3(0,0,0);

    private Vector3 corridorStartPoint;
    private Vector3 corridorEndPoint;
    private bool detectingCorridor = false;

    // Start is called before the first frame update
    void Start()
    {
        wallLayerMask = LayerMask.GetMask("MazeWalls");
        currentDirection = -transform.up;
    }

    void Update()
    {
        MeasureDistances();

        // Check if there's an obstacle in the current direction
        if (Physics2D.Raycast(transform.position, currentDirection, 0.1f, wallLayerMask))
        {
            // Recalculate direction if an obstacle is near
            currentDirection = DetermineDirection(currentDirection, leftWallDistance, rightWallDistance, upWallDistance, downWallDistance);
        }

        // Move continuously in the current direction
        transform.position += currentDirection * moveSpeed * Time.deltaTime;
    }

    // Measure the distance to walls in all 4 cardinal directions
    void MeasureDistances()
    {
        RaycastHit2D hit;

        // Measure distance to the left wall
        hit = Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, wallLayerMask);
        if (hit.collider != null)
        {
            leftWallDistance = hit.distance;
        }
        else
        {
            leftWallDistance = raycastDistance;  // No hit, set to max distance
        }

        // Draw the left ray (in red)
        Debug.DrawRay(transform.position, Vector2.left * raycastDistance, Color.red);

        // Measure distance to the right wall
        hit = Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, wallLayerMask);
        if (hit.collider != null)
        {
            rightWallDistance = hit.distance;
        }
        else
        {
            rightWallDistance = raycastDistance;
        }

        // Draw the right ray (in green)
        Debug.DrawRay(transform.position, Vector2.right * raycastDistance, Color.green);

        // Measure distance upwards
        hit = Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, wallLayerMask);
        if (hit.collider != null)
        {
            upWallDistance = hit.distance;
        }
        else
        {
            upWallDistance = raycastDistance;
        }

        // Draw the upward ray (in blue)
        Debug.DrawRay(transform.position, Vector2.up * raycastDistance, Color.blue);

        // Measure distance downwards
        hit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, wallLayerMask);
        if (hit.collider != null)
        {
            downWallDistance = hit.distance;
        }
        else
        {
            downWallDistance = raycastDistance;
        }

        // Draw the downward ray (in yellow)
        Debug.DrawRay(transform.position, Vector2.down * raycastDistance, Color.yellow);

        // For debugging, display distances in the Unity Console
        Debug.Log("Left Distance: " + leftWallDistance + " | Right Distance: " + rightWallDistance);
        Debug.Log("Up Distance: " + upWallDistance + " | Down Distance: " + downWallDistance);
    }

    Vector3 DetermineDirection(Vector3 currentDirection, float lDist, float rDist, float uDist, float dDist)
    {
        Debug.Log("current inputs" + lDist + " , " + rDist + " , " + uDist +  " , " + dDist);
        // Check perpendicular distances based on the current direction
        if (currentDirection == transform.right) // Moving right, check up and down
        {
            if (uDist > rDist && uDist > dDist) return transform.up;    // Prefer up if it has a longer distance
            if (dDist > rDist && dDist > uDist) return -transform.up;   // Prefer down if it has a longer distance
        }
        else if (currentDirection == -transform.right) // Moving left, check up and down
        {
            if (uDist > lDist && uDist > dDist) return transform.up;    // Prefer up if it has a longer distance
            if (dDist > lDist && dDist > uDist) return -transform.up;   // Prefer down if it has a longer distance
        }
        else if (currentDirection == transform.up) // Moving up, check right and left
        {
            if (lDist > uDist && lDist > rDist) return -transform.right; // Prefer left if it has a longer distance
            if (rDist > uDist && rDist > lDist) return transform.right;  // Prefer right if it has a longer distance
        }
        else if (currentDirection == -transform.up) // Moving down, check right and left
        {
            if (lDist > dDist && lDist > rDist) return -transform.right; // Prefer left if it has a longer distance
            if (rDist > dDist && rDist > lDist) return transform.right;  // Prefer right if it has a longer distance
        }

        // If no perpendicular direction has a longer distance, continue in the current direction
        return currentDirection;

    }

    // Draw the raycasts for visualization in the Unity Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.right * raycastDistance); // Left ray
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.right * raycastDistance); // Right ray
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * raycastDistance); // Front ray
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.up * raycastDistance); // Back ray
    }

}
