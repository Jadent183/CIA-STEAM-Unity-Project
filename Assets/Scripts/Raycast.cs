using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Raycast : MonoBehaviour
{

    // Line settings
    [Header("Line Settings")]
    [SerializeField] private float maxRayDistance = 50f;
    [SerializeField] private Color closeLineColor = Color.green;
    [SerializeField] private Color farLineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    // public float wallThreshold = 0.9f;

    private LayerMask wallLayer;

    private Dictionary<string,DirectionalHit> hitTable = new Dictionary<string,DirectionalHit>();
    private DirectionalHit northHit;
    private DirectionalHit southHit;
    private DirectionalHit eastHit;
    private DirectionalHit westHit;

    private static GameObject maze; // = GameObject.Find("10 by 10 orthogonal maze");
    private MazeMapper mazeMapper;// maze.GetComponent<MazeMapper>();

    // Start is called before the first frame update
    void Start()
    {
        wallLayer = LayerMask.GetMask("MazeLayer");

        // Initialize directional hits
        northHit = InitializeDirectionalHit("North");
        southHit = InitializeDirectionalHit("South");
        eastHit = InitializeDirectionalHit("East");
        westHit = InitializeDirectionalHit("West");
        hitTable.Add("N",northHit);
        hitTable.Add("S",southHit);
        hitTable.Add("E",eastHit);
        hitTable.Add("W",westHit);
        
        maze = GameObject.Find("10 by 10 orthogonal maze");
        mazeMapper = maze.GetComponent<MazeMapper>();
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

          if ((Mathf.Abs(transform.position.x) % Globals.gridSize < 0.25f || Mathf.Abs(transform.position.x) % Globals.gridSize > 0.75f) && (Mathf.Abs(transform.position.y) % Globals.gridSize < 0.25f || Mathf.Abs(transform.position.y) % Globals.gridSize > 0.75f))
        {
                if (hitTable["N"].hitDistance < Globals.wallThreshold && hitTable["S"].hitDistance < Globals.wallThreshold && hitTable["E"].hitDistance > Globals.wallThreshold && hitTable["W"].hitDistance > Globals.wallThreshold)
            {
                // Debug.Log("Invalid node position NS. " + gameObject.name);
                return;
            } else if (hitTable["N"].hitDistance > Globals.wallThreshold && hitTable["S"].hitDistance > Globals.wallThreshold && hitTable["E"].hitDistance < Globals.wallThreshold && hitTable["W"].hitDistance < Globals.wallThreshold)
            {
                // Debug.Log("Invalid node position EW." + gameObject.name);
                return;
            } else
            {
                mazeMapper.AddNode(transform.position, hitTable, gameObject.name);
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