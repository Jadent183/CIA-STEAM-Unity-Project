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
    }

    void Update()
    {
        MeasureDistances();
        //next two lines is current attempt at using DetermineDirection, however currentDirection is showing up as the 0 vector, which is calling the initialization case of DetermineDirection
        currentDirection = DetermineDirection(currentDirection, leftWallDistance, rightWallDistance, upWallDistance, downWallDistance) * moveSpeed * Time.deltaTime;
        transform.position += currentDirection;
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
        //object is currently in motion, keep going the same direction unless a new direction is found
        if(currentDirection.magnitude >= .001f)
        {
            //current direction is left, check up and down to see if there is a longer distance
            if(currentDirection == -transform.right)
            {
                if(upWallDistance > leftWallDistance)
                {
                    return transform.up;
                }
                else if(downWallDistance > leftWallDistance)
                {
                    return -transform.up;
                }
                else
                {
                    return -transform.right;
                }
            }
            //current direction is right, check up and down to see if there is a longer distance
            if (currentDirection == transform.right)
            {
                if (upWallDistance > rightWallDistance)
                {
                    return transform.up;
                }
                else if (downWallDistance > rightWallDistance)
                {
                    return -transform.up;
                }
                else
                {
                    return transform.right;
                }
            }
            //current direction is up, check right and left to see if there is a longer distance
            if (currentDirection == transform.up)
            {
                if (rightWallDistance > upWallDistance)
                {
                    return transform.right;
                }
                else if (leftWallDistance > upWallDistance)
                {
                    return -transform.right;
                }
                else
                {
                    return transform.up;
                }
            }
            //current direction is down, check right and left to see if there is a longer distance
            if (currentDirection == -transform.up)
            {
                if (rightWallDistance > downWallDistance)
                {
                    return transform.right;
                }
                else if (leftWallDistance > downWallDistance)
                {
                    return -transform.right;
                }
                else
                {
                    return -transform.up;
                }
            }
        }
        //object is not currently moving, find longest hallway and go that way THIS IS TRUE EVERY TIME
        else
        {
            //left is longest direction, go that way
            if(lDist > rDist && lDist > uDist && lDist > dDist)
            {
                return -transform.right;
            }
            //right is longest direction, go that way
            else if (rDist > lDist && rDist > uDist && rDist > dDist)
            {
                return transform.right;
            }
            //up is longest direction, go that way
            else if (uDist > rDist && uDist > lDist && uDist > dDist)
            {
                return transform.up;
            }
            //down is longest direction, go that way
            else if (dDist > rDist && dDist > uDist && dDist > lDist)
            {
                return -transform.up;
            }
            return Vector3.zero;
        }
        return Vector3.zero;
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
