using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // transform.Translate(2, 2, 0);
        // StartCoroutine(ExampleCoroutine());
    }

    int moveSpeed = 5;

    //  x, y, z | -x left, x right, -y down, y up, z we dont care about it's 2D
    Vector3 vector = new Vector3( 0 , -1 , 0 );

    // Vector store last changed direction
    Vector3 changeDir = new Vector3(0, 0, 0);


    // Update is called once per frame
    void Update()
    {

        //Autonomous movement
        if(Input.GetKey(KeyCode.I)){

            // transform.position += Vector3.right * (moveSpeed * directionRight) * Time.deltaTime;
            // transform.position += Vector3.up * (moveSpeed * directionUp) * Time.deltaTime;
            transform.position += vector * moveSpeed * Time.deltaTime;
        }

        
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.right * -moveSpeed * Time.deltaTime;

        }

        else if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.up * -moveSpeed * Time.deltaTime;

        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //move it back slightly
        transform.position += vector * -1 * moveSpeed * Time.deltaTime;

        //Change direction
        Vector3 rotated = new Vector3(-1 * vector.y, vector.x, 0);
        vector = rotated;

        //Store last point, if point already exists, lets draw a line!!!
        if(changeDir != (new Vector3(0,0,0))){
            //draw a line w the new position!!
            Debug.Log("Position 1: " + changeDir + " | Position 2: " + transform.position);

            GameObject lineObject = new GameObject("Line");
            
            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>(); 
            
            lineRenderer.startWidth = 0.5F;
            
            lineRenderer.SetPosition(0, changeDir + new Vector3(25,0,0)); // Set the starting point
            lineRenderer.SetPosition(1, transform.position + new Vector3(25,0,0)); // Set the ending point

            changeDir = transform.position;

        } else{
           changeDir = transform.position;
        }

    }

}
