using System.Collections;
using System.Collections.Generic;
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
    bool isCollision = false;

    //controls direction 

    int directionRight = 0;
    int directionUp = -1;

    Vector3 vector = new Vector3( 0 , -1 , 0 );

// void Update()
// {
//     player.transform.position += vector * moveSpeed * Time.DeltaTime;
// }
// void OnCollisionEnter2D(Colision2D collision) {
//     Vector3 rotated = new Vector3(vector.z, 0, vector.x);
//     vector = rotated;
// }





    // Update is called once per frame
    void Update()
    {
        // transform.Translate(1,1,0);

        // StartCoroutine(ExampleCoroutine());


            // transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            // Debug.Log(transform.position);
            // Debug.Log(Physics.CheckBox(transform.position + vector * moveSpeed * Time.deltaTime,
            //                             collider.bounds.size * 0.5,
            //                             (1,1,0)));


            

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
        if(collision.gameObject.tag == "Wall"){
            //Debug.Log("HAHAHAHAHA");
            
            foreach (ContactPoint2D contact in collision.contacts)
            {
                //Debug.Log("HAHAHAHAHA");
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            }

            isCollision = false;
        } else{
            isCollision = true;
            //Debug.Log("NAURRR");

            //move it back slightly
            transform.position += vector * -1 * moveSpeed * Time.deltaTime;

            //Change direction
            Vector3 rotated = new Vector3(-1 * vector.y, vector.x, 0);
            vector = rotated;

        }


        //if we hit something
        //if the direction was go down, we instead go right


    }

    // IEnumerator ExampleCoroutine()
    // {
    //     //Print the time of when the function is first called.
    //     Debug.Log("Started Coroutine at timestamp : " + Time.time);

    //     //yield on a new YieldInstruction that waits for 5 seconds.
    //     transform.Translate(1,1,0);
    //     yield return new WaitForSeconds(5);

    //     //After we have waited 5 seconds print the time again.
    //     Debug.Log("Finished Coroutine at timestamp : " + Time.time);


    //     // StartCoroutine(ExampleCoroutine());


    // }

}
