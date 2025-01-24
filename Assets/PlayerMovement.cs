// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;


// public class PlayerMovement : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
//         // transform.Translate(2, 2, 0);
//         // StartCoroutine(ExampleCoroutine());
//     }

//     int moveSpeed = 5;

//     //  x, y, z | -x left, x right, -y down, y up, z we dont care about it's 2D
//     Vector3 vector = new Vector3( 0 , -1 , 0 );

//     // Vector store last changed direction
//     Vector3 changeDir = new Vector3(0, 0, 0);


//     // Update is called once per frame
//     void Update()
//     {

//         //Autonomous movement
//         if(Input.GetKey(KeyCode.I)){

//             // transform.position += Vector3.right * (moveSpeed * directionRight) * Time.deltaTime;
//             // transform.position += Vector3.up * (moveSpeed * directionUp) * Time.deltaTime;
//             transform.position += vector * moveSpeed * Time.deltaTime;
//         }

//         if(name.Equals("Player")){

//             if (Input.GetKey(KeyCode.D))
//             {
//                 // Debug.Log(name);
//                 transform.position += Vector3.right * moveSpeed * Time.deltaTime;

//             }
//             else if (Input.GetKey(KeyCode.A))
//             {
//                 transform.position += Vector3.right * -moveSpeed * Time.deltaTime;

//             }

//             else if (Input.GetKey(KeyCode.W))
//             {
//                 transform.position += Vector3.up * moveSpeed * Time.deltaTime;

//             }
//             else if (Input.GetKey(KeyCode.S))
//             {
//                 transform.position += Vector3.up * -moveSpeed * Time.deltaTime;

//             }

//         }

//         if(name.Equals("Second Player")){

//             if (Input.GetKey(KeyCode.RightArrow))
//             {
//                 // Debug.Log(name);
//                 transform.position += Vector3.right * moveSpeed * Time.deltaTime;

//             }
//             else if (Input.GetKey(KeyCode.LeftArrow))
//             {
//                 transform.position += Vector3.right * -moveSpeed * Time.deltaTime;

//             }

//             else if (Input.GetKey(KeyCode.UpArrow))
//             {
//                 transform.position += Vector3.up * moveSpeed * Time.deltaTime;

//             }
//             else if (Input.GetKey(KeyCode.DownArrow))
//             {
//                 transform.position += Vector3.up * -moveSpeed * Time.deltaTime;

//             }

//         }
//     }

//     void OnCollisionEnter2D(Collision2D collision)
//     {
//         //move it back slightly
//         transform.position += vector * -1 * moveSpeed * Time.deltaTime;

//         //Change direction
//         Vector3 rotated = new Vector3(-1 * vector.y, vector.x, 0);
//         vector = rotated;

//         //Store last point, if point already exists, lets draw a line!!!
//         if(changeDir != (new Vector3(0,0,0))){
//             //draw a line w the new position!!
//             Debug.Log("Position 1: " + changeDir + " | Position 2: " + transform.position);

//             GameObject lineObject = new GameObject("Line");

//             LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>(); 

//             lineRenderer.startWidth = 0.5F;

//             lineRenderer.SetPosition(0, changeDir + new Vector3(25,0,0)); // Set the starting point
//             lineRenderer.SetPosition(1, transform.position + new Vector3(25,0,0)); // Set the ending point

//             changeDir = transform.position;

//         } else{
//            changeDir = transform.position;
//         }

//     }

// }


//////// DIFFERENT
///

using UnityEngine;
using System.Collections.Generic;


public class PlayerMovement : MonoBehaviour
{
    void Start()
    {
        // Create a PathVisualizer for mapping the robots trail
        GameObject visualizerObject = new GameObject("PathVisualizer");
        pathVisualizer = visualizerObject.AddComponent<PathVisualizer>();
    }

    int moveSpeed = 5;

    //  x, y, z | -x left, x right, -y down, y up, z we dont care about it's 2D
    Vector3 vector = new Vector3(0, -1, 0);



    private PathVisualizer pathVisualizer;

    private float updateInterval = 0.05f; // How often to update the path (in seconds)

    private float timeSinceLastUpdate = 0f;


    // Update is called once per frame
    void Update()
    {
        bool moving = false;

        // Autonomous movement
        if (Input.GetKey(KeyCode.I))
        {
            // transform.position += Vector3.right * (moveSpeed * directionRight) * Time.deltaTime;
            // transform.position += Vector3.up * (moveSpeed * directionUp) * Time.deltaTime;
            transform.position += vector * moveSpeed * Time.deltaTime;
            moving = true;
        }



        if (name.Equals("Player"))
        {
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += Vector3.right * moveSpeed * Time.deltaTime;
                moving = true;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                transform.position += Vector3.right * -moveSpeed * Time.deltaTime;
                moving = true;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                transform.position += Vector3.up * moveSpeed * Time.deltaTime;
                moving = true;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position += Vector3.up * -moveSpeed * Time.deltaTime;
                moving = true;
            }
        }

        if (name.Equals("Second Player"))
        {

            if (Input.GetKey(KeyCode.RightArrow))
            {
                // Debug.Log(name);
                transform.position += Vector3.right * moveSpeed * Time.deltaTime;

            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += Vector3.right * -moveSpeed * Time.deltaTime;

            }

            else if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.position += Vector3.up * -moveSpeed * Time.deltaTime;

            }

        }

        // Updates the trail path if the robot is moving
        if (moving)
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                // Adds the current position vector into the list of vectors to draw the path
                pathVisualizer.UpdatePath(transform.position);
                timeSinceLastUpdate = 0f;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Move back slightly
        transform.position += vector * -1 * moveSpeed * Time.deltaTime;

        // Change direction
        Vector3 rotated = new Vector3(-1 * vector.y, vector.x, 0);
        vector = rotated;

        // Updates the path when changing direction
        // pathVisualizer.UpdatePath(transform.position);
    }
}


public class PathVisualizer : MonoBehaviour
{
    private List<Vector3> pathPoints = new List<Vector3>();
    private Vector3 visualizationOffset = new Vector3(25, 0, 0);
    private float lineWidth = 0.5f;
    private Material lineMaterial;
    private LineRenderer continuousLine;
    private float minimumDistanceThreshold = 0.1f; // Reduces overlapping lines

    void Start()
    {
        // Create a single continuous line that follows the robot
        GameObject lineObject = new GameObject("ContinuousPath");
        lineObject.transform.parent = transform;

        // Set default colors, materials, and width for the trail line
        continuousLine = lineObject.AddComponent<LineRenderer>();
        continuousLine.material = new Material(Shader.Find("Sprites/Default"));
        continuousLine.material.color = Color.green;
        continuousLine.startWidth = lineWidth;
        continuousLine.endWidth = lineWidth;
        continuousLine.useWorldSpace = true;
    }

    public void UpdatePath(Vector3 newPoint)
    {
        // Check if we should add the new point 
        if (pathPoints.Count == 0 || Vector3.Distance(pathPoints[pathPoints.Count - 1], newPoint) >= minimumDistanceThreshold)
        {
            pathPoints.Add(newPoint);
            UpdateLineRenderer();
        }
    }

    private void UpdateLineRenderer()
    {
        // Update the line renderer with all points
        continuousLine.positionCount = pathPoints.Count;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            continuousLine.SetPosition(i, pathPoints[i] + visualizationOffset);
        }
    }

    public void ClearPath()
    {
        pathPoints.Clear();
        continuousLine.positionCount = 0;
    }
}
