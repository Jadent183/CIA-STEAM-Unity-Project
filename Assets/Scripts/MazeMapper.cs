using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.Experimental.GlobalIllumination;
using System.Linq.Expressions;
using Unity.Mathematics;

public static class Globals
{
    public static float wallThreshold = 0.9f;
    public static float gridSize = 1f;
    public static string mazeName = "maze1";
    public static int expectedNodeCount = 68;
    public static float nodeAlpha = 0.0f;
    public static bool mazeCompleted = false; 
    public static bool showConnections = false;
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
    public Dictionary<string, string> nodeHistory;
    public float distance;
}

public class MazeMapper : MonoBehaviour
{   
    // Dictionary stores all nodes
    public Dictionary<Vector3, MapNode> nodes = new Dictionary<Vector3, MapNode>();
    
    // Connection lines for entire maze
    public List<LineRenderer> connectionLines = new List<LineRenderer>();

    // Shortest path lines 
    public List<LineRenderer> pathLines = new List<LineRenderer>();

    public MapNode getNode(Vector3 pos){
        return nodes[pos];
    }

    // Put the position into normalized grid
    public Vector3 SnapToGrid(Vector3 pos)
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
public char AddNode(Vector3 position, Dictionary<string,DirectionalHit> hitTable, String name, string direction)
    {
        // Check if postion is already in dictionary by position
        position = SnapToGrid(position);
        if (nodes.ContainsKey(position) != true)
        {
            //Debug.Log(name + ": Creating Node: " + position);
            MapNode node = new MapNode();
            node.position = position;
            node.nodeID = nodeID;
            node.connections = new List<MapNode>();
            nodeID++;
            node.mapDeadEnd = "";
            node.mapUnexplored = "";
            node.mapWIP = "";
            node.mapCompleted = "";
            node.nodeHistory = new Dictionary<string, string>();
            node.distance = -1;
            foreach (KeyValuePair<string,DirectionalHit> kvp in hitTable) {
                if (kvp.Value.hitDistance < Globals.wallThreshold) {
                    node.mapDeadEnd += kvp.Key;
                }
                else if (kvp.Value.hitDistance > 20) {
                    node.mapDeadEnd += kvp.Key;
                }
                else {
                    node.mapUnexplored += kvp.Key;
                }
            }
            
            
            //Debug.Log("Node Unexplored: " + node.mapUnexplored);
            //Debug.Log("Node WIP: " + node.mapWIP);
            drawNodes(node);
            nodes.Add(node.position, node);
        }
        char returnValue = ' ';
        MapNode tempNode = nodes[position];
        if (tempNode.mapUnexplored.Length + tempNode.mapWIP.Length > 1) {
            if (tempNode.mapUnexplored.Contains(direction) || tempNode.mapWIP.Contains(direction)) {
                tempNode.mapWIP = tempNode.mapWIP.Replace(direction, String.Empty);
                tempNode.mapWIP += direction;
                tempNode.mapUnexplored = tempNode.mapUnexplored.Replace(direction, String.Empty);
            }
        }
        try {
            tempNode.nodeHistory.Add(name, "");
        }
        catch {}
        
        if (tempNode.mapUnexplored.Length > 0) {
            returnValue = tempNode.mapUnexplored[0];
            tempNode.mapUnexplored = tempNode.mapUnexplored.Substring(1);
            tempNode.mapWIP += returnValue;
        }
        else if (tempNode.mapWIP.Length > 0) {
            returnValue = tempNode.mapWIP[0];
            tempNode.mapWIP = tempNode.mapWIP.Substring(1);
            tempNode.mapWIP += returnValue;
        }
        else if (tempNode.mapCompleted.Length > 0) {
            returnValue = tempNode.mapCompleted[0];
            tempNode.mapCompleted = tempNode.mapCompleted.Substring(1);
            tempNode.mapCompleted += returnValue;
        }
        if (tempNode.nodeHistory[name].Contains(direction)) {
            tempNode.mapCompleted += direction;
            tempNode.mapWIP = tempNode.mapWIP.Replace(direction, String.Empty);
        }
        try {
            tempNode.nodeHistory[name] += returnValue;
        }
        catch {}
        nodes[position] = tempNode;

        if (tempNode.nodeID == 67)
        {   
            Debug.Log("--------------MAP COMPLETED--------------");
        }
        if (IsMazeFullyMapped()) // MAZE HAS BEEN COMPLETED
        {
            Debug.Log("MAZE COMPLETE");
            Globals.mazeCompleted = true;
            
        }

        //Debug.Log("Node: " + tempNode.nodeID + " Completed: " + tempNode.mapCompleted);

        return returnValue;
    }
    // Update is called once per frame
    void Update()
    {

        // AddNode(transform.position);


    }
    
    private void drawPath(Vector3 position, float distance) {
        // To use: drawPath(target position, 0);
        // Recursively assigns distance 
        position = SnapToGrid(position);
        findNeighbors(nodes[position]);
        MapNode tempNode = nodes[position];
        tempNode.distance = distance;
        nodes[position] = tempNode;
        foreach (MapNode node in tempNode.connections) {
            if (node.distance == -1) {
                float dist = Vector3.Distance(position, node.position) + distance;
                drawPath(node.position, dist);
            }
        }
    }
   

    // Draw the nodes for visual aide
    // private void drawNodes(MapNode node) 
    // {
    //     GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //     nodeObj.name = $"Node {node.nodeID}";
    //     nodeObj.transform.position = node.position;
    //     nodeObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    //     Renderer r = nodeObj.GetComponent<Renderer>();
    //     r.material.color = Color.blue;

    // }

    private void drawNodes(MapNode node)
{
    GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    nodeObj.name = $"Node {node.nodeID}";
    nodeObj.transform.position = node.position;
    nodeObj.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
    nodeObj.layer = LayerMask.NameToLayer("Default");
    
    Renderer r = nodeObj.GetComponent<Renderer>();

    // Set transparency-supporting shader mode
    Material mat = r.material;
    mat.shader = Shader.Find("Standard");
    mat.SetFloat("_Mode", 3); // 3 = Transparent
    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    mat.SetInt("_ZWrite", 0);
    mat.DisableKeyword("_ALPHATEST_ON");
    mat.EnableKeyword("_ALPHABLEND_ON");
    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    mat.renderQueue = 3000;

    Color color = new Color(0f, 0f, 1f, Globals.nodeAlpha); // Default alpha
    mat.color = color;

    
    // Add collider if not already there
    if (nodeObj.GetComponent<Collider>() == null)
    {
        nodeObj.AddComponent<SphereCollider>();
    }
    
    // Add EventSystem if it doesn't exist in the scene
    if (FindObjectOfType<EventSystem>() == null)
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
    
    // Add our tooltip component and set the node data
    NodeTooltip tooltip = nodeObj.AddComponent<NodeTooltip>();
    tooltip.SetNodeData(node);
    
    // Add Physics Raycaster to the camera if not already there
    Camera mainCamera = Camera.main;
    if (mainCamera != null && mainCamera.GetComponent<PhysicsRaycaster>() == null)
    {
        mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
    }
}

    private void findNeighbors(MapNode node)
    {
        int maxDistance = 10; // Limit for how far to search for neighbors
        // Check if node other node exists along open direction
        if ((node.mapUnexplored+node.mapWIP+node.mapCompleted).Contains("N")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 up = new Vector3(node.position.x, node.position.y + (i * Globals.gridSize), 0);
                if (nodes.ContainsKey(up))
                {
                    //Debug.Log("Found up neightbor");
                    node.connections.Add(nodes[up]);
                    break;
                }
            }
        }
        if ((node.mapUnexplored+node.mapWIP+node.mapCompleted).Contains("S")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 down = new Vector3(node.position.x, node.position.y - (i * Globals.gridSize), 0);
                if (nodes.ContainsKey(down))
                {
                    //Debug.Log("Found down neighbor");
                    node.connections.Add(nodes[down]);
                    break;
                }
            }
        }
        if ((node.mapUnexplored+node.mapWIP+node.mapCompleted).Contains("W")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 left = new Vector3(node.position.x - (i * Globals.gridSize), node.position.y, 0);
                if (nodes.ContainsKey(left))
                {
                    //Debug.Log("Found left neighbor");
                    node.connections.Add(nodes[left]);
                    break;
                }
            }
        }
        if ((node.mapUnexplored+node.mapWIP+node.mapCompleted).Contains("E")) {
            for (int i = 1; i < maxDistance; i++)
            {
                Vector3 right = new Vector3(node.position.x + (i * Globals.gridSize), node.position.y, 0);
                if (nodes.ContainsKey(right))
                {
                    //Debug.Log("Found right neighbor");
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
            // Create the line only if it doesn't already exist between these nodes
            bool lineExists = false;
            foreach (LineRenderer existingLine in connectionLines)
            {
                Vector3 pos0 = existingLine.GetPosition(0);
                Vector3 pos1 = existingLine.GetPosition(1);
                
                // Check if this connection already exists (in either direction)
                if ((pos0 == node.position && pos1 == connection.position) ||
                    (pos0 == connection.position && pos1 == node.position))
                {
                    lineExists = true;
                    break;
                }
            }
            
            // Only create a new line if it doesn't exist
            if (!lineExists)
            {
                Color lineColor = Color.yellow;

                lineColor.a = 0.7f;
                GameObject line = new GameObject("Edge");
                LineRenderer lr = line.AddComponent<LineRenderer>();
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lineColor;
                lr.endColor = lineColor;
                lr.positionCount = 2;
                lr.SetPosition(0, node.position);
                lr.SetPosition(1, connection.position);
                lr.enabled = Globals.showConnections;
                
                // Add to our list for tracking
                connectionLines.Add(lr);
            }
        }
    }

    public void UpdateConnectionsVisibility()
    {
        if (Globals.showConnections)
        {
            if (connectionLines.Count == 0)
            {
                foreach (KeyValuePair<Vector3, MapNode> kvp in nodes)
                {
                    MapNode node = kvp.Value;
                    if (node.connections.Count == 0)
                    {
                        findNeighbors(node);
                    }
                    drawConnections(node);
                }
            }
            
            foreach (LineRenderer lr in connectionLines)
            {
                lr.enabled = true;
            }
        }
        else
        {
            foreach (LineRenderer lr in connectionLines)
            {
                lr.enabled = false;
            }
        }
    }

    public void ClearConnections()
    {
        foreach (LineRenderer lr in connectionLines)
        {
            if (lr != null && lr.gameObject != null)
                Destroy(lr.gameObject);
        }
        connectionLines.Clear();
    }



    // Calculate distances from end node to all reachable nodes
    public void CalculateDistances(Vector3 endPosition) {
        // Reset all distances to -1 (using a separate loop to avoid modifying during enumeration)
        List<Vector3> nodeKeys = new List<Vector3>(nodes.Keys);
        
        foreach (Vector3 key in nodeKeys) {
            MapNode node = nodes[key];
            node.distance = -1;
            nodes[key] = node;
        }
        
        // Start from end position with distance 0
        ResetNodeDistancesAndCalculateFrom(endPosition);
    }

    private void ResetNodeDistancesAndCalculateFrom(Vector3 targetPosition) {
        targetPosition = SnapToGrid(targetPosition);
        
        // Check if target position exists in nodes
        if (!nodes.ContainsKey(targetPosition)) {
            Debug.LogError("Target position not found in nodes dictionary: " + targetPosition);
            return;
        }
        
        // Use a queue for breadth-first search (more reliable for shortest path)
        Queue<Vector3> positionsToProcess = new Queue<Vector3>();
        positionsToProcess.Enqueue(targetPosition);
        
        // Set distance of target node to 0
        MapNode targetNode = nodes[targetPosition];
        targetNode.distance = 0;
        nodes[targetPosition] = targetNode;
        
        // Process all reachable nodes
        while (positionsToProcess.Count > 0) {
            Vector3 currentPos = positionsToProcess.Dequeue();
            MapNode currentNode = nodes[currentPos];
            
            // Make sure connections are populated
            if (currentNode.connections.Count == 0) {
                findNeighbors(currentNode);
                // Update the node after finding neighbors
                nodes[currentPos] = currentNode;
                // Re-fetch the node with updated connections
                currentNode = nodes[currentPos];
            }
            
            // Process each connected node
            foreach (MapNode connectedNode in currentNode.connections) {
                // Skip if this is a struct reference problem (connection to self)
                if (connectedNode.position == currentNode.position) continue;
                
                // Get the actual node from the dictionary to ensure we're working with the current data
                if (!nodes.ContainsKey(connectedNode.position)) {
                    Debug.LogError("Connected node position not found in nodes dictionary: " + connectedNode.position);
                    continue;
                }
                
                MapNode actualConnectedNode = nodes[connectedNode.position];
                
                // If node hasn't been assigned a distance yet, or we found a shorter path
                float distanceThroughCurrent = Vector3.Distance(currentPos, actualConnectedNode.position) + currentNode.distance;
                
                if (actualConnectedNode.distance == -1 || distanceThroughCurrent < actualConnectedNode.distance) {
                    // Update the distance
                    actualConnectedNode.distance = distanceThroughCurrent;
                    // Save back to dictionary
                    nodes[actualConnectedNode.position] = actualConnectedNode;
                    // Add to queue for processing its connections
                    positionsToProcess.Enqueue(actualConnectedNode.position);
                }
            }
        }
    }

    // Find and draw the shortest path from start to end
    public void FindAndDrawShortestPath(Vector3 startPosition, Vector3 endPosition) {
        // First make sure distance values are calculated from end
        CalculateDistances(endPosition);
        
        // Clear any existing path lines
        ClearPathLines();
        
        startPosition = SnapToGrid(startPosition);
        endPosition = SnapToGrid(endPosition);
        
        // Check if start and end positions are in nodes
        if (!nodes.ContainsKey(startPosition)) {
            Debug.LogError("Start position not found in nodes: " + startPosition);
            return;
        }
        
        if (!nodes.ContainsKey(endPosition)) {
            Debug.LogError("End position not found in nodes: " + endPosition);
            return;
        }
        
        List<Vector3> pathPositions = new List<Vector3>();
        pathPositions.Add(startPosition);
        
        Vector3 currentPosition = startPosition;
        
        // Keep finding the next step in the path until we reach the end
        int safetyCounter = 0; // Prevent infinite loops
        while (currentPosition != endPosition && safetyCounter < 1000) {
            safetyCounter++;
            MapNode currentNode = nodes[currentPosition];
            
            // Find the neighbor with the lowest distance
            float lowestDistance = float.MaxValue;
            Vector3 nextPosition = currentPosition; // Default to staying in place if no better option
            
            // Ensure connections are populated
            if (currentNode.connections.Count == 0) {
                findNeighbors(currentNode);
                // Need to update the node in the dictionary and re-fetch it
                nodes[currentPosition] = currentNode;
                currentNode = nodes[currentPosition];
            }
            
            foreach (MapNode neighbor in currentNode.connections) {
                // Need to get the actual current node from the dictionary
                if (!nodes.ContainsKey(neighbor.position)) continue;
                MapNode actualNeighbor = nodes[neighbor.position];
                
                // Skip if neighbor hasn't been assigned a distance
                if (actualNeighbor.distance == -1) continue;
                
                // Check if this neighbor has a better (lower) distance
                if (actualNeighbor.distance < lowestDistance) {
                    lowestDistance = actualNeighbor.distance;
                    nextPosition = actualNeighbor.position;
                }
            }
            
            // If we couldn't find a better position, we're stuck
            if (nextPosition == currentPosition) {
                Debug.LogWarning("No path found to end position! Stopped at: " + currentPosition);
                break;
            }
            
            // Add this position to our path
            pathPositions.Add(nextPosition);
            currentPosition = nextPosition;
        }
        
        // Now draw the path using line renderers
        for (int i = 0; i < pathPositions.Count - 1; i++) {
            CreatePathLine(pathPositions[i], pathPositions[i + 1]);
        }

        if(IsMazeFullyMapped()){
            Debug.Log("Starting sendInstructions");
            sendInstructions(pathPositions);
        }
        
        if (safetyCounter >= 1000) {
            Debug.LogError("Possible infinite loop in path finding. Path creation aborted.");
        }
    }

    // Create a line between two positions for the path
    private void CreatePathLine(Vector3 start, Vector3 end) {
        Color pathColor = Color.green;
        pathColor.a = 0.8f;
        
        GameObject line = new GameObject("PathLine");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.startWidth = 0.1f; 
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = pathColor;
        lr.endColor = pathColor;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        
        // Add to our list for tracking
        pathLines.Add(lr);
    }

    // Clear existing path lines
    public void ClearPathLines() {
        foreach (LineRenderer lr in pathLines) {
            if (lr != null && lr.gameObject != null)
                Destroy(lr.gameObject);
        }
        pathLines.Clear();
    }

    public bool IsMazeFullyMapped()
    {
        return nodes.Count >= Globals.expectedNodeCount;
    }
    // public bool AreAllNodesCompleted()
    // {
    //     return AreAllNodesConnected();
    //     // foreach (var node in nodes.Values)
    //     // {
    //     //     if (node.mapWIP + node.mapUnexplored != "")
    //     //     {
    //     //         Debug.Log("Nodes are not finished: ID: " + node.nodeID + " Position: " + node.position);
    //     //         //return false;
    //     //     }
    //     // }
    //     // return true;
    // }


    public void sendInstructions(List<Vector3> pathPositions)
    {
        string directions = "";


        string lastDirection = "E"; //Either North/South or East/West

        // Movement options are foward,left,right
        // Going from NS to EW means left/right change
        // Going from EW to NS means left/right change

        // if last direction E ()
        for (int i = 0; i < pathPositions.Count - 1; i++) {
            Vector3 start = pathPositions[i];
            Vector3 end = pathPositions[i+1];
            Vector3 posDiff = start-end;
            // Debug.Log("Vector start: " + start);
            // Debug.Log("Vector End: " + end);

            Debug.Log("lastDirection: " + lastDirection + " Vector Diff: " + (start-end));
            if(lastDirection.Equals("N")){
                if(posDiff.x > 0){
                    directions += "l";
                    lastDirection = "W";
                }
                else if(posDiff.x < 0){
                    directions += "r";
                    lastDirection = "E";
                }
                else{
                    directions += "f";
                    lastDirection = "N";
                }
            }
            else if(lastDirection.Equals("S")){
                if(posDiff.x > 0){
                    directions += "r";
                    lastDirection = "W";
                }
                else if(posDiff.x < 0){
                    directions += "l";
                    lastDirection = "E";
                }
                else{
                    directions += "f";
                    lastDirection = "S";
                }
            }
            else if(lastDirection.Equals("E")){
                if(posDiff.y > 0){
                    directions += "r";
                    lastDirection = "S"; 
                }
                else if(posDiff.y < 0){
                    directions += "l";
                    lastDirection = "N";
                }
                else{
                    directions += "f";
                    lastDirection = "E";
                }
            }
            else if(lastDirection.Equals("W")){
                if(posDiff.y > 0){
                    directions += "l";
                    lastDirection = "S";
                }
                else if(posDiff.y < 0){
                    directions += "r";
                    lastDirection = "N";
                }
                else{
                    directions += "f";
                    lastDirection = "W";
                }
            }

            if(math.abs((int)posDiff.x) > 1){
                int mult = math.abs((int)posDiff.x);
                for(int j = 1; j<mult;j++){
                        directions += "f";
                }
            }else if(math.abs((int)posDiff.y) > 1){
                int mult = math.abs((int)posDiff.y);
                for(int j = 1; j<mult;j++){
                        directions += "f";
                }
            }

            Debug.Log("Turning: " + directions);
        }
        
        Debug.Log("Directions is: " + directions);
        string pathToFile = Application.persistentDataPath + "/directions.txt"; //Change this per user to a better hardcoded location (ETHAN)
        string directions2 = "rrlflf";

        using (StreamWriter writer = new StreamWriter(pathToFile, false))
        {
            writer.WriteLine(directions);
        }

        Debug.Log(pathToFile);
        return;
    }


    public void writeInstructions(Vector3 start, Vector3 end)
    {
        
        Debug.Log("Vector start: " + start);
        Debug.Log("Vector End: " + end);
        
        return;
    }

}