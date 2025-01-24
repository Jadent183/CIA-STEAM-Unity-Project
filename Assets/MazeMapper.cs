using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public struct MapNode
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public Vector3 position;
    public int nodeID;
}
public class MazeMapper : MonoBehaviour
{

    private class DirectionalHit
    {
        public bool hasHit;
        public Vector2 contactPoint;
        public float hitDistance;
        public LineRenderer Line;
    }

    // Line settings
    [Header("Line Settings")]
    [SerializeField] private float maxRayDistance = 50f;
    [SerializeField] private Color closeLineColor = Color.green;
    [SerializeField] private Color farLineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;

    private LayerMask wallLayer;

    private DirectionalHit northHit;
    private DirectionalHit southHit;
    private DirectionalHit eastHit;
    private DirectionalHit westHit;

    // Start is called before the first frame update
    void Start()
    {
        wallLayer = 1 << 0;

        // Initialize directional hits
        northHit = InitializeDirectionalHit("North");
        southHit = InitializeDirectionalHit("South");
        eastHit = InitializeDirectionalHit("East");
        westHit = InitializeDirectionalHit("West");

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
                if (rayHit.distance < 1f)
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
