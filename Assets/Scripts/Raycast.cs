using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


public class Raycast : MonoBehaviour
{
    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x / Globals.gridSize) * Globals.gridSize, Mathf.Round(pos.y / Globals.gridSize) * Globals.gridSize, 0);
    }
    // Line settings
    [Header("Line Settings")]
    [SerializeField] public float maxRayDistance = 50f;
    [SerializeField] public Color closeLineColor = Color.green;
    [SerializeField] public Color farLineColor = Color.red;
    [SerializeField] public float lineWidth = 0.1f;
    // public float wallThreshold = 0.9f;
    public float lineAlpha = 0f;

    private LayerMask wallLayer;

    private Dictionary<string,DirectionalHit> hitTable = new Dictionary<string,DirectionalHit>();
    private DirectionalHit northHit;
    private DirectionalHit southHit;
    private DirectionalHit eastHit;
    private DirectionalHit westHit;

    private static GameObject maze; // = GameObject.Find("10 by 10 orthogonal maze");
    private MazeMapper mazeMapper;// maze.GetComponent<MazeMapper>();

    private bool paused = false;
    private char input = ' ';
    private float x;
    private float y;
    
    // Start is called before the first frame update
    void Start()
    {
        wallLayer = LayerMask.GetMask("MazeLayer");
        farLineColor.a = lineAlpha;        
        closeLineColor.a = lineAlpha;        
        // Initialize directional hits
        northHit = InitializeDirectionalHit("North");
        southHit = InitializeDirectionalHit("South");
        eastHit = InitializeDirectionalHit("East");
        westHit = InitializeDirectionalHit("West");
        hitTable.Add("N",northHit);
        hitTable.Add("S",southHit);
        hitTable.Add("E",eastHit);
        hitTable.Add("W",westHit);
        
        // maze = GameObject.Find(Globals.mazeName); //CHANGE THIS TO DYNAMIC
        // mazeMapper = maze.GetComponent<MazeMapper>();
        StartCoroutine(WaitForMazeAndInit());
    }


    IEnumerator WaitForMazeAndInit()
    {
        // Wait until the maze GameObject is present in the scene
        while (GameObject.Find(Globals.mazeName) == null)
        {
            yield return null; // wait 1 frame
        }

        maze = GameObject.Find(Globals.mazeName);
        mazeMapper = maze.GetComponent<MazeMapper>();

        if (mazeMapper == null)
        {
            Debug.LogError("MazeMapper component missing on maze prefab.");
        }
    }


    private DirectionalHit InitializeDirectionalHit(string direction)
    {
        DirectionalHit hit = new DirectionalHit();
        
        GameObject closestLineObj = new GameObject($"{direction}ClosestLine");
        closestLineObj.transform.SetParent(transform);
        hit.Line = closestLineObj.AddComponent<LineRenderer>();
        SetupLineRenderer(hit.Line);

        return hit;
    }

    private void SetupLineRenderer(LineRenderer lr)
    {
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.positionCount = 2;
        lr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CastRayToWalls();
        DrawLines();
        if (Mathf.Abs(transform.position.x+transform.position.y-x-y) > .75) {
            paused = false;
        }
        if ((Mathf.Abs(transform.position.x) % Globals.gridSize < 0.25f || Mathf.Abs(transform.position.x) % Globals.gridSize > 0.75f) && (Mathf.Abs(transform.position.y) % Globals.gridSize < 0.25f || Mathf.Abs(transform.position.y) % Globals.gridSize > 0.75f))
        {
            if (hitTable["N"].hitDistance < Globals.wallThreshold && hitTable["S"].hitDistance < Globals.wallThreshold && hitTable["E"].hitDistance > Globals.wallThreshold && hitTable["W"].hitDistance > Globals.wallThreshold)
            {
                // Debug.Log("Invalid node position NS. " + gameObject.name);
                paused = false;
                return;
            } else if (hitTable["N"].hitDistance > Globals.wallThreshold && hitTable["S"].hitDistance > Globals.wallThreshold && hitTable["E"].hitDistance < Globals.wallThreshold && hitTable["W"].hitDistance < Globals.wallThreshold)
            {
                // Debug.Log("Invalid node position EW." + gameObject.name);
                paused = false;
                return;
            } else if ((hitTable["E"].hitDistance > 49f || hitTable["W"].hitDistance > 49f) && (hitTable["N"].hitDistance > 49f || hitTable["S"].hitDistance > 49f))
            { 
                paused = false;
                return;
            } else
            {
                if (paused == false) {
                    paused = true;
                    
                    transform.position = SnapToGrid(transform.position);
                    x = transform.position.x;
                    y = transform.position.y;
                    string direction = gameObject.GetComponent<PlayerMovement>().getOppositeDirection();
                    input = mazeMapper.AddNode(transform.position, hitTable, gameObject.name, direction);
                    gameObject.GetComponent<PlayerMovement>().changeDirection(input);
                }
            }
        } 
    }



    private void CastRayToWalls()
    {
        Vector2 rayOrigin = transform.position;

        ResetDirectionalHit(northHit);
        ResetDirectionalHit(southHit);
        ResetDirectionalHit(eastHit);
        ResetDirectionalHit(westHit);

        // Cast rays and log results for each direction
        CastDirectionalRay(Vector2.up, northHit, "North");
        CastDirectionalRay(Vector2.down, southHit, "South");
        CastDirectionalRay(Vector2.right, eastHit, "East");
        CastDirectionalRay(Vector2.left, westHit, "West");
    }

    private void ResetDirectionalHit(DirectionalHit hit)
    {
        hit.hasHit = false;
        hit.hitDistance = maxRayDistance;
    }

    private void CastDirectionalRay(Vector2 direction, DirectionalHit hit, string directionName)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction,
            maxRayDistance,
            wallLayer
        );

        foreach (RaycastHit2D rayHit in hits)
        {
            if (rayHit.collider.gameObject == gameObject)
                continue;

            if (rayHit.collider != null)
            {
                hit.hitDistance = rayHit.distance;
                hit.contactPoint = rayHit.point;
                hit.hasHit = true;
                closeLineColor.a = lineAlpha;
                farLineColor.a = lineAlpha;
                if (rayHit.distance < Globals.wallThreshold)
                {
                    hit.Line.startColor = closeLineColor;
                    hit.Line.endColor = closeLineColor;
                } else
                {
                    hit.Line.startColor = farLineColor;
                    hit.Line.endColor = farLineColor;
                }
            }
        }
    }

    private void DrawLines()
    {
        DrawDirectionalLines(northHit);
        DrawDirectionalLines(southHit);
        DrawDirectionalLines(eastHit);
        DrawDirectionalLines(westHit);
    }

    private void DrawDirectionalLines(DirectionalHit hit)
    {
        if (hit.hasHit && hit.Line != null)
        {
            hit.Line.enabled = true;
            hit.Line.SetPosition(0, transform.position);
            hit.Line.SetPosition(1, hit.contactPoint);
        }
        else if (hit.Line != null)
        {
            hit.Line.enabled = false;
        }
    }

}