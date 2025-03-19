using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Struct to store node data
public struct MapNode
{
    public string mapDeadEnd;
    public string mapCompleted;
    public string mapUnexplored;
    public string mapWIP;
    public Vector3 position;
    public int nodeID;
    public List<MapNode> connections;
}
public class MazeMapper : MonoBehaviour
{
    private class DirectionalHit
    {
        public bool hasHit;
        public Vector2 contactPoint; // Point on wall that is hit
        public float hitDistance;
        public LineRenderer Line;
    }

    // Line settings
    [Header("Line Settings")]
    [SerializeField] private float maxRayDistance = 50f;
    [SerializeField] private Color closeLineColor = Color.green;
    [SerializeField] private Color farLineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    private float wallThreshold = 0.9f;

    private LayerMask wallLayer;

    private Dictionary<string,DirectionalHit> hitTable = new Dictionary<string,DirectionalHit>();
    private DirectionalHit northHit;
    private DirectionalHit southHit;
    private DirectionalHit eastHit;
    private DirectionalHit westHit;
    
    // Dictionary stores all nodes
    private Dictionary<Vector3, MapNode> nodes = new Dictionary<Vector3, MapNode>();

    // Put the position into normalized grid
    private float gridSize = 1f;
    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x / gridSize) * gridSize, Mathf.Round(pos.y / gridSize) * gridSize, 0);
    }


    // Start is called before the first frame update
    void Start()
    {
        wallLayer = 1 << 0;

        // Initialize directional hits
        northHit = InitializeDirectionalHit("North");
        southHit = InitializeDirectionalHit("South");
        eastHit = InitializeDirectionalHit("East");
        westHit = InitializeDirectionalHit("West");
        hitTable.Add("N",northHit);
        hitTable.Add("S",southHit);
        hitTable.Add("E",eastHit);
        hitTable.Add("W",westHit);
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

    // int that tracks the number of nodes/ids
    private int nodeID = 0;


    // Check for valid node position and add to dictionary
    private void AddNode(Vector3 position)
    {
        // Check if postion is already in dictionary by position
        if (nodes.ContainsKey(SnapToGrid(position)))
        {
            // Debug.Log("Node already exists at this position.");
            return;
        }
        // Check if directions/position are valid to place a node

        if ((Mathf.Abs(position.x) % gridSize < 0.25f || Mathf.Abs(position.x) % gridSize > 0.75f) && (Mathf.Abs(position.y) % gridSize < 0.25f || Mathf.Abs(position.y) % gridSize > 0.75f))
        {
                if (northHit.hitDistance < wallThreshold && southHit.hitDistance < wallThreshold && eastHit.hitDistance > wallThreshold && westHit.hitDistance > wallThreshold)
            {
                // Debug.Log("Invalid node position.");
                return;
            } else if (northHit.hitDistance > wallThreshold && southHit.hitDistance > wallThreshold && eastHit.hitDistance < wallThreshold && westHit.hitDistance < wallThreshold)
            {
                // Debug.Log("Invalid node position.");
                return;
            } else
            {
                MapNode node = new MapNode();
                node.position = SnapToGrid(position);
                node.nodeID = nodeID;
                node.connections = new List<MapNode>();
                nodeID++;
                node.mapDeadEnd = "";
                node.mapUnexplored = "";
                foreach (KeyValuePair<string,DirectionalHit> kvp in hitTable) {
                    if (kvp.Value.hitDistance < wallThreshold) {
                        node.mapDeadEnd += kvp.Key;
                    }
                    else {
                        node.mapUnexplored += kvp.Key;
                    }
                }
                
                nodes.Add(node.position, node);
                Debug.Log($"Rover at {position}");
                Debug.Log($"Node added at {node.position}");
                Debug.Log($"Node ID: {node.nodeID}");
                Debug.Log("Node open in:" + node.mapUnexplored);
                drawNodes(node);
            }
        } 

    }

    // Update is called once per frame
    void Update()
    {
        CastRayToWalls();
        DrawLines();
        AddNode(transform.position);

        // On return, find neighbors for each node
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Finding neighbors...");
            foreach (KeyValuePair<Vector3, MapNode> node in nodes)
            {
                Debug.Log($"Finding neighbors for node {node.Value.position}");
                findNeighbors(node.Value);
                drawConnections(node.Value);
                Debug.Log(node.Value.connections.Count);
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
                if (rayHit.distance < wallThreshold)
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

    // Draw the nodes for visual aide
    private void drawNodes(MapNode node) 
    {
        GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeObj.name = $"Node {node.nodeID}";
        nodeObj.transform.position = node.position;
        nodeObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Renderer r = nodeObj.GetComponent<Renderer>();
        r.material.color = Color.blue;

    }

    private void findNeighbors(MapNode node)
    {
        int maxDistance = 10; // Limit for how far to search for neighbors
        // Check if node other node exists along open direction
        if (node.mapUnexplored.Contains("N")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 up = new Vector3(node.position.x, node.position.y + (i * gridSize), 0);
                if (nodes.ContainsKey(up))
                {
                    Debug.Log("Found up neightbor");
                    node.connections.Add(nodes[up]);
                    break;
                }
            }
        }
        if (node.mapUnexplored.Contains("S")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 down = new Vector3(node.position.x, node.position.y - (i * gridSize), 0);
                if (nodes.ContainsKey(down))
                {
                    Debug.Log("Found down neighbor");
                    node.connections.Add(nodes[down]);
                    break;
                }
            }
        }
        if (node.mapUnexplored.Contains("W")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 left = new Vector3(node.position.x - (i * gridSize), node.position.y, 0);
                if (nodes.ContainsKey(left))
                {
                    Debug.Log("Found left neighbor");
                    node.connections.Add(nodes[left]);
                    break;
                }
            }
        }
        if (node.mapUnexplored.Contains("E")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 right = new Vector3(node.position.x + (i * gridSize), node.position.y, 0);
                if (nodes.ContainsKey(right))
                {
                    Debug.Log("Found right neighbor");
                    node.connections.Add(nodes[right]);
                    break;
                }
            }
        }
    }

    // Draw connections between nodes
    private void drawConnections(MapNode node)
    {
        foreach (MapNode connection in node.connections)
        {
            GameObject line = new GameObject("Edge");
            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.positionCount = 2;
            lr.SetPosition(0, node.position);
            lr.SetPosition(1, connection.position);
            lr.enabled = true;
        }
    }
}
