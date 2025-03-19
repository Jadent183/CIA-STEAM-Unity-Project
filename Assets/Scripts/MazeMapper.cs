using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Globals
{
    public static float wallThreshold = 0.9f;

    public static float gridSize = 1f;
}

public class DirectionalHit
{
    public bool hasHit;
    public Vector2 contactPoint; // Point on wall that is hit
    public float hitDistance;
    public LineRenderer Line;
}

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
    // Dictionary stores all nodes
    private Dictionary<Vector3, MapNode> nodes = new Dictionary<Vector3, MapNode>();

    // Put the position into normalized grid
    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x / Globals.gridSize) * Globals.gridSize, Mathf.Round(pos.y / Globals.gridSize) * Globals.gridSize, 0);
    }


    // Start is called before the first frame update
    void Start()
    {

    }



    // int that tracks the number of nodes/ids
    private int nodeID = 0;


    // Check for valid node position and add to dictionary
    public void AddNode(Vector3 position, Dictionary<string,DirectionalHit> hitTable, String name)
    {
        // Check if postion is already in dictionary by position
        if (nodes.ContainsKey(SnapToGrid(position)))
        {
            // Debug.Log("Node already exists at this position.");
                
            return;
        }
        // Check if directions/position are valid to place a node

        Debug.Log(name + ": Creating Node: " + position);
        MapNode node = new MapNode();
        node.position = SnapToGrid(position);
        node.nodeID = nodeID;
        node.connections = new List<MapNode>();
        nodeID++;
        node.mapDeadEnd = "";
        node.mapUnexplored = "";
        foreach (KeyValuePair<string,DirectionalHit> kvp in hitTable) {
            if (kvp.Value.hitDistance < Globals.wallThreshold) {
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

    // Update is called once per frame
    void Update()
    {

        // AddNode(transform.position);

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
                Vector3 up = new Vector3(node.position.x, node.position.y + (i * Globals.gridSize), 0);
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
                Vector3 down = new Vector3(node.position.x, node.position.y - (i * Globals.gridSize), 0);
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
                Vector3 left = new Vector3(node.position.x - (i * Globals.gridSize), node.position.y, 0);
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
                Vector3 right = new Vector3(node.position.x + (i * Globals.gridSize), node.position.y, 0);
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
