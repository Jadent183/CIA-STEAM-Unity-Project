using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x / Globals.gridSize) * Globals.gridSize, Mathf.Round(pos.y / Globals.gridSize) * Globals.gridSize, 0);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // transform.Translate(2, 2, 0);
        // StartCoroutine(ExampleCoroutine());
    }

    public int moveSpeed = 5;
    bool isCollision = false;

    //controls direction 

    int directionRight = 0;
    int directionUp = -1;
    
    Vector3 vector = new Vector3( 0 , -1 , 0 );

    string oppositeDirection = "N";
    // Update is called once per frame
    void Update()
    {

            // Autonomous movement
            if(Input.GetKey(KeyCode.RightControl)){
                transform.position += vector * moveSpeed * Time.deltaTime;
            }

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
    public void changeDirection(char input) {
        switch (input)
        {
            case 'N':
                vector = Vector3.up;
                oppositeDirection = "S";
                break;

            case 'S':
                vector = Vector3.down;
                oppositeDirection = "N";
                break;

            case 'E':
                vector = Vector3.right;
                oppositeDirection = "W";
                break;

            case 'W':
                vector = Vector3.left;
                oppositeDirection = "E";
                break;

            default:
                vector = new Vector3( 0 , 0 , 0 );
                break;
        }
    }
    public string getOppositeDirection() {
        return oppositeDirection;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        vector = new Vector3( 0 , 0 , 0 );
        transform.position = SnapToGrid(transform.position);
    }


}
