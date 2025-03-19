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

    // Update is called once per frame
    void Update()
    {

            //Autonomous movement
            // if(Input.GetKey(KeyCode.I)){
            //     transform.position += vector * moveSpeed * Time.deltaTime;
            // }

            if(gameObject.name == "Player"){

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

            if(gameObject.name == "Player2"){

                if (Input.GetKey(KeyCode.RightArrow))
                {
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

            if(gameObject.name == "Player3"){

                if (Input.GetKey(KeyCode.L))
                {
                    transform.position += Vector3.right * moveSpeed * Time.deltaTime;

                }
                else if (Input.GetKey(KeyCode.J))
                {
                    transform.position += Vector3.right * -moveSpeed * Time.deltaTime;

                }

                else if (Input.GetKey(KeyCode.I))
                {
                    transform.position += Vector3.up * moveSpeed * Time.deltaTime;

                }
                else if (Input.GetKey(KeyCode.K))
                {
                    transform.position += Vector3.up * -moveSpeed * Time.deltaTime;

                }
            }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall"){
            
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            }

            isCollision = false;
        } else{
            isCollision = true;

            //move it back slightly
            transform.position += vector * -1 * moveSpeed * Time.deltaTime;

            //Change direction
            Vector3 rotated = new Vector3(-1 * vector.y, vector.x, 0);
            vector = rotated;

        }

    }


}
