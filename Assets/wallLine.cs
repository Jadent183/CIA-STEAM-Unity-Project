using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [System.Serializable]
    private class DirectionalHit
    {
        public bool hasClosestHit;
        public bool hasFurthestHit;
        public Vector2 closestPoint;
        public Vector2 furthestPoint;
        public float closestDistance;
        public float furthestDistance;
        public LineRenderer closestLine;
        public LineRenderer furthestLine;
    }

    [Header("Line Settings")]
    [SerializeField] private float maxRayDistance = 50f;
    [SerializeField] private Color closestLineColor = Color.green;
    [SerializeField] private Color furthestLineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    
    private LayerMask wallLayer;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugRays = true;
    [SerializeField] private Color debugRayColor = Color.yellow;

    private DirectionalHit northHit;
    private DirectionalHit southHit;
    private DirectionalHit eastHit;
    private DirectionalHit westHit;
    private DirectionalHit longestDirectionHit;

    private void Start()
    {
        wallLayer = 1 << 0;

        // Initialize directional hits
        northHit = InitializeDirectionalHit("North");
        southHit = InitializeDirectionalHit("South");
        eastHit = InitializeDirectionalHit("East");
        westHit = InitializeDirectionalHit("West");

        // Log initial position
        LogPosition("Starting position", transform.position);
    }

    private void LogPosition(string label, Vector2 position)
    {
        Debug.Log($"{label}: ({position.x:F2}, {position.y:F2})");
    }

    private void LogDirectionalHits(DirectionalHit hit, string direction)
    {
        if (hit.hasClosestHit)
        {
            Debug.Log($"\n{direction} Direction Hits:");
            Debug.Log($"  Origin: ({transform.position.x:F2}, {transform.position.y:F2})");
            Debug.Log($"  Closest Hit: ({hit.closestPoint.x:F2}, {hit.closestPoint.y:F2}) - Distance: {hit.closestDistance:F2}");
            
            if (hit.hasFurthestHit)
            {
                Debug.Log($"  Furthest Hit: ({hit.furthestPoint.x:F2}, {hit.furthestPoint.y:F2}) - Distance: {hit.furthestDistance:F2}");
            }
        }
    }

    private DirectionalHit InitializeDirectionalHit(string direction)
    {
        DirectionalHit hit = new DirectionalHit();
        
        GameObject closestLineObj = new GameObject($"{direction}ClosestLine");
        closestLineObj.transform.SetParent(transform);
        hit.closestLine = closestLineObj.AddComponent<LineRenderer>();
        SetupLineRenderer(hit.closestLine, closestLineColor);

        GameObject furthestLineObj = new GameObject($"{direction}FurthestLine");
        furthestLineObj.transform.SetParent(transform);
        hit.furthestLine = furthestLineObj.AddComponent<LineRenderer>();
        SetupLineRenderer(hit.furthestLine, furthestLineColor);

        return hit;
    }

    private void SetupLineRenderer(LineRenderer lr, Color color)
    {
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.positionCount = 2;
        lr.enabled = false;
    }

    private void Update()
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

        Debug.Log("Hello there!");
        Debug.Log(northHit + " Down: " + southHit);

        // Cast rays and log results for each direction
        CastDirectionalRay(Vector2.up, northHit, "North");
        CastDirectionalRay(Vector2.down, southHit, "South");
        CastDirectionalRay(Vector2.right, eastHit, "East");
        CastDirectionalRay(Vector2.left, westHit, "West");

        // Log all hits after casting
        LogAllHits();
    }

    private void LogAllHits()
    {
        Debug.Log("\n=== Wall Detection Results ===");
        LogDirectionalHits(northHit, "North");
        LogDirectionalHits(southHit, "South");
        LogDirectionalHits(eastHit, "East");
        LogDirectionalHits(westHit, "West");
        Debug.Log("===========================\n");
    }

    private void ResetDirectionalHit(DirectionalHit hit)
    {
        hit.hasClosestHit = false;
        hit.hasFurthestHit = false;
        hit.closestDistance = maxRayDistance;
        hit.furthestDistance = 0f;
    }

    private void CastDirectionalRay(Vector2 direction, DirectionalHit hit, string directionName)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction,
            maxRayDistance,
            wallLayer
        );

        if (showDebugRays)
        {
            Debug.DrawRay(transform.position, direction * maxRayDistance, debugRayColor, Time.deltaTime);
        }

        foreach (RaycastHit2D rayHit in hits)
        {
            if (rayHit.collider.gameObject == gameObject)
                continue;

            if (rayHit.collider != null)
            {
                if (rayHit.distance < hit.closestDistance)
                {
                    hit.closestDistance = rayHit.distance;
                    hit.closestPoint = rayHit.point;
                    hit.hasClosestHit = true;
                }

                if (rayHit.distance > hit.furthestDistance && rayHit.distance < maxRayDistance)
                {
                    hit.furthestDistance = rayHit.distance;
                    hit.furthestPoint = rayHit.point;
                    hit.hasFurthestHit = true;
                }
            }
        }
    }

    private void DrawLines()
    {
        // Find the longest direction first
        FindLongestDirection();
        
        // Draw all lines with appropriate colors
        DrawDirectionalLines(northHit);
        DrawDirectionalLines(southHit);
        DrawDirectionalLines(eastHit);
        DrawDirectionalLines(westHit);
    }

    private void FindLongestDirection()
    {
        float maxDistance = 0f;
        longestDirectionHit = null;

        // Check each direction to find the longest
        if (northHit.hasClosestHit && northHit.closestDistance > maxDistance)
        {
            maxDistance = northHit.closestDistance;
            longestDirectionHit = northHit;
        }
        if (southHit.hasClosestHit && southHit.closestDistance > maxDistance)
        {
            maxDistance = southHit.closestDistance;
            longestDirectionHit = southHit;
        }
        if (eastHit.hasClosestHit && eastHit.closestDistance > maxDistance)
        {
            maxDistance = eastHit.closestDistance;
            longestDirectionHit = eastHit;
        }
        if (westHit.hasClosestHit && westHit.closestDistance > maxDistance)
        {
            maxDistance = westHit.closestDistance;
            longestDirectionHit = westHit;
        }
    }

    private void DrawDirectionalLines(DirectionalHit hit)
    {
        if (hit.hasClosestHit && hit.closestLine != null)
        {
            hit.closestLine.enabled = true;
            hit.closestLine.SetPosition(0, transform.position);
            hit.closestLine.SetPosition(1, hit.closestPoint);
            
            // Set color based on whether this is the longest direction
            if (hit == longestDirectionHit)
            {
                hit.closestLine.startColor = furthestLineColor;
                hit.closestLine.endColor = furthestLineColor;
            }
            else
            {
                hit.closestLine.startColor = closestLineColor;
                hit.closestLine.endColor = closestLineColor;
            }
        }
        else if (hit.closestLine != null)
        {
            hit.closestLine.enabled = false;
        }

        if (hit.hasFurthestHit && hit.furthestLine != null)
        {
            hit.furthestLine.enabled = true;
            hit.furthestLine.SetPosition(0, transform.position);
            hit.furthestLine.SetPosition(1, hit.furthestPoint);
        }
        else if (hit.furthestLine != null)
        {
            hit.furthestLine.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        DrawDirectionalGizmos(northHit);
        DrawDirectionalGizmos(southHit);
        DrawDirectionalGizmos(eastHit);
        DrawDirectionalGizmos(westHit);
    }

    private void DrawDirectionalGizmos(DirectionalHit hit)
    {
        if (hit.hasClosestHit)
        {
            Gizmos.color = closestLineColor;
            Gizmos.DrawWireSphere(hit.closestPoint, 0.2f);
            
            if (hit.hasFurthestHit)
            {
                Gizmos.color = furthestLineColor;
                Gizmos.DrawWireSphere(hit.furthestPoint, 0.2f);
            }
        }
    }
}