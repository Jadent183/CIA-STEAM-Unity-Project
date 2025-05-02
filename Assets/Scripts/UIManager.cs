using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEditor;

[Serializable]
public class MazePrefabEntry
{
    public string name;
    public GameObject prefab;
}

public class UIManager : MonoBehaviour
{
    // UI Elements
    public Slider slider;
    public Button addRoverButton;
    public Button playPauseButton;
    public Button resetMazeButton;
    public Button setEndButton;
    public Button setSpawnButton;
    public Button clearSpawnButton;
    public TMP_Text sliderText;
    public GameObject roverPrefab;
    public Dropdown dropdown;
    public Toggle nodesToggle;
    public Toggle raycastsToggle;
    public Toggle mappedMazeToggle;
    public Toggle shortestPathToggle;

    // Timers
    public TMP_Text startToEndTimerText;
    public TMP_Text mazeCompletionTimerText;
    private float startToEndTimer = 0f;
    private float mazeCompletionTimer = 0f;
    private bool startToEndTimerRunning = false;
    private bool mazeCompletionTimerRunning = false;
    private bool reachedEndpoint = false;


    // Maze Prefabs
    public List<MazePrefabEntry> mazePrefabs;
    private Dictionary<string, GameObject> mazePrefabDict = new();

    // State
    private GameObject currentMazeInstance;
    private static GameObject maze;
    private MazeMapper mazeMapper;
    public bool openMaze;
    public string mazeName;
    private bool isPlaying = false;

    // Rover Related
    private PlayerMovement[] Rovers;
    private Dictionary<PlayerMovement, Vector3> originalPositions = new();
    public GameObject xMarkerPrefab;
    private GameObject activeXMarker;
    public GameObject endMarkerPrefab;
    private GameObject activeEndMarker;

    private bool isPlacingSpawn = false;
    private bool spawnPointSet = false;
    private Vector2 spawnPoint;

    private bool isPlacingEnd = false;
    private bool endPointSet = false;
    public Vector2 endPoint;

    // UI Toggle States
    public bool showingNodes = false;
    public bool showingRaycasts = false;
    public bool showingMappedMaze = false;
    public bool showingShortestPath = false;

    // Maze Dictionary
    public Dictionary<string, (string type, Vector2 spawnPoint, Vector2 endPoint)> Mazes = new()
    {
        { "maze1", ("open", new Vector2(0.05f, 5f), new Vector2(1f, -5f)) },
        { "10 by 10 orthogonal maze", ("closed", new Vector2(-8f, -3f), new Vector2(-2f, -7f)) },
        { "maze2", ("open", new Vector2(0.25f, 4.81f), new Vector2(1f, -5f)) },
        { "Maze4", ("open", new Vector2(0.05f, 5f), new Vector2(0.05f, -3f)) },
        { "Maze5", ("closed", new Vector2(0,0), new Vector2(0,0)) },
        { "MazePhysical", ("closed", new Vector2(-1f, 5f), new Vector2(-1f, -5f)) },
        { "Maze1000", ("open", new Vector2(0.05f, 5f), new Vector2(2.74f, 1f)) },
    };

    void Awake()
    {
        foreach (var entry in mazePrefabs)
        {
            mazePrefabDict[entry.name] = entry.prefab;
        }
    }

    void Start()
    {
        resetMazeButton.onClick.AddListener(ResetMap);
        slider.onValueChanged.AddListener(OnSliderChanged);
        addRoverButton.onClick.AddListener(OnAddRoverClicked);
        setEndButton.onClick.AddListener(StartPlacingEndPoint);
        playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        setSpawnButton.onClick.AddListener(StartPlacingSpawnPoint);
        clearSpawnButton.onClick.AddListener(ClearSpawnAndEndPoints);
        nodesToggle.onValueChanged.AddListener(ToggleNodesVisibility);
        raycastsToggle.onValueChanged.AddListener(ToggleRaycastsVisibility);
        mappedMazeToggle.onValueChanged.AddListener(ToggleMappedMazeVisibility);
        shortestPathToggle.onValueChanged.AddListener(ToggleShortestPathVisibility);

        dropdown.ClearOptions();
        List<string> formattedOptions = new();
        foreach (var maze in Mazes)
        {
            formattedOptions.Add($"{maze.Key} ({maze.Value.type})");
        }
        dropdown.AddOptions(formattedOptions);
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });

        mazeName = Mazes.Keys.First();
        if (mazePrefabDict.TryGetValue(mazeName, out GameObject prefab))
        {
            currentMazeInstance = Instantiate(prefab);
            currentMazeInstance.name = mazeName;
            maze = currentMazeInstance;
            mazeMapper = maze.GetComponent<MazeMapper>();
            var (status, spawn, end) = Mazes[mazeName];
            spawnPoint = spawn;
            endPoint = end;
        }

        openMaze = Mazes[mazeName].type == "open";
        UpdateUIForMazeType();

        Rovers = FindObjectsOfType<PlayerMovement>();
        foreach (var player in Rovers)
        {
            originalPositions[player] = player.transform.position;
        }
    }

    void DropdownValueChanged(Dropdown change)
    {
        string fullLabel = change.options[change.value].text;
        string selectedMaze = fullLabel.Substring(0, fullLabel.LastIndexOf(" ("));
        Globals.mazeName = selectedMaze;
        mazeName = selectedMaze;


        if (currentMazeInstance != null)
            Destroy(currentMazeInstance);

        if (mazePrefabDict.TryGetValue(selectedMaze, out GameObject prefab))
        {
            currentMazeInstance = Instantiate(prefab);
            currentMazeInstance.name = selectedMaze;
            maze = currentMazeInstance;
            mazeMapper = maze.GetComponent<MazeMapper>();
        }
        else
        {
            Debug.LogError($"No prefab found for maze '{selectedMaze}'");
            return;
        }

        var (status, spawn, end) = Mazes[selectedMaze];
        openMaze = status == "open";
        spawnPoint = spawn;
        endPoint = end;
        ResetMap();
        UpdateUIForMazeType();

        if (!openMaze)
        {
            spawnPointSet = false;
            endPointSet = false;
            if (activeXMarker != null) Destroy(activeXMarker);
            if (activeEndMarker != null) Destroy(activeEndMarker);
        }


    }

    void UpdateUIForMazeType()
    {
        setSpawnButton.interactable = !openMaze;
        setEndButton.interactable = !openMaze;
        clearSpawnButton.interactable = !openMaze;

        if (openMaze)
        {
            if (activeXMarker != null) Destroy(activeXMarker);
            if (activeEndMarker != null) Destroy(activeEndMarker);

            // Spawn Marker
            Vector3 snapped = mazeMapper.SnapToGrid(new Vector3(spawnPoint.x, spawnPoint.y, 0f));
            spawnPoint = snapped;
            spawnPointSet = true;
            activeXMarker = Instantiate(xMarkerPrefab, spawnPoint, Quaternion.identity);

            // End Marker
            Vector3 snappedEnd = mazeMapper.SnapToGrid(new Vector3(endPoint.x, endPoint.y, 0f));
            endPoint = snappedEnd;
            endPointSet = true;
            activeEndMarker = Instantiate(endMarkerPrefab, endPoint, Quaternion.identity);
        }
    }
    void StartPlacingSpawnPoint()
    {
        // Only allow placement for closed mazes
        if (openMaze)
        {
            Debug.LogWarning("Cannot place spawn point in open mazes");
            return;
        }
        
        // Reset the flags for placement
        isPlacingSpawn = true;
        spawnPointSet = false;
        
        // Clear existing marker
        if (activeXMarker != null)
        {
            Destroy(activeXMarker);
            activeXMarker = null;
        }
        
        // Create initial marker at mouse position
        Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        activeXMarker = Instantiate(xMarkerPrefab, startPos, Quaternion.identity);
    }

    void StartPlacingEndPoint()
    {
        if (openMaze)
        {
            Debug.LogWarning("Cannot place end point in open mazes");
            return;
        }

        isPlacingEnd = true;
        endPointSet = false;

        if (activeEndMarker != null) Destroy(activeEndMarker);

        Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        activeEndMarker = Instantiate(endMarkerPrefab, startPos, Quaternion.identity);
    }

    void UpdateSpawnPointPlacement()
    {
        if (!isPlacingSpawn || mazeMapper == null) {
            return;
        } 

        Vector2 rawMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 snappedPosition = mazeMapper.SnapToGrid(new Vector3(rawMousePos.x, rawMousePos.y, 0f));

        // Update marker position
        if (activeXMarker != null)
        {
            activeXMarker.transform.position = snappedPosition;
        }

        // On left click, set the spawn point
        if (Input.GetMouseButtonDown(0))
        {
            spawnPoint = snappedPosition;
            spawnPointSet = true;
            isPlacingSpawn = false;
            Debug.Log("Spawn Point Set at: " + spawnPoint);
        }
    }

    void UpdateEndPointPlacement()
    {
        if (!isPlacingEnd || mazeMapper == null) {
            return;
        } 

        Vector2 rawMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 snappedPosition = mazeMapper.SnapToGrid(new Vector3(rawMousePos.x, rawMousePos.y, 0f));

        // Update marker position
        if (activeEndMarker != null)
        {
            activeEndMarker.transform.position = snappedPosition;
        }

        // On left click, set the spawn point
        if (Input.GetMouseButtonDown(0))
        {
            endPoint = snappedPosition;
            endPointSet = true;
            isPlacingEnd = false;
            Debug.Log("Spawn Point Set at: " + endPoint);
        }
    }

    void Update()
    {
        UpdateSpawnPointPlacement();
        UpdateEndPointPlacement();
        CheckMazeCompleted();
        UpdateTimers();
    }

    private void UpdateTimers()
    {
        if (startToEndTimerRunning && !reachedEndpoint)
        {
            startToEndTimer += Time.deltaTime;
            startToEndTimerText.text = FormatTime(startToEndTimer);
        }

        if (mazeCompletionTimerRunning && !Globals.mazeCompleted)
        {
            mazeCompletionTimer += Time.deltaTime;
            mazeCompletionTimerText.text = FormatTime(mazeCompletionTimer);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000);
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }

    public void OnRoverReachedEnd()
    {
        if (!reachedEndpoint)
        {
            startToEndTimerRunning = false;
            reachedEndpoint = true;
            startToEndTimerText.color = Color.green;
        }
    }

    public void ResetTimers()
    {
        // Timer
        startToEndTimer = 0f;
        startToEndTimerText.text = FormatTime(startToEndTimer);
        startToEndTimerText.color = Color.white;
        reachedEndpoint = false;
        startToEndTimerRunning = false;

        mazeCompletionTimer = 0f;
        mazeCompletionTimerText.text = FormatTime(startToEndTimer);
        mazeCompletionTimerText.color = Color.white;
        mazeCompletionTimerRunning = false;
        Globals.mazeCompleted = false;
    }

    public void CheckMazeCompleted()
    {
        if (Globals.mazeCompleted)
        {
            mazeCompletionTimerRunning = false;
            reachedEndpoint = true;
            startToEndTimerText.color = Color.green;
        }
    }


    void ClearSpawnAndEndPoints()
    {
        OnClearEndPointClicked();
        OnClearSpawnPointClicked();
    }

    void OnClearEndPointClicked()
    {
        if (!endPointSet) return;

        endPointSet = false;
        endPoint = Vector3.zero;
        if (activeEndMarker != null) Destroy(activeEndMarker);
    }

    void OnClearSpawnPointClicked()
    {
        if (!spawnPointSet) return;

        spawnPointSet = false;
        spawnPoint = Vector3.zero;
        if (activeXMarker != null) Destroy(activeXMarker);
    }

    void OnSliderChanged(float value)
    {
        if (sliderText != null)
            sliderText.text = value.ToString("0");

        foreach (var rover in Rovers)
            rover.moveSpeed = int.Parse(sliderText.text);
    }

    void OnAddRoverClicked()
    {
        if (!spawnPointSet) return;

        Vector3 spawnPos = spawnPoint;
        GameObject newRover = Instantiate(roverPrefab, spawnPos, Quaternion.identity);
        PlayerMovement pm = newRover.GetComponent<PlayerMovement>();

        if (pm != null)
        {
            pm.moveSpeed = int.Parse(sliderText.text);
            pm.isDriving = isPlaying;
        }

        UpdatePlayers();
    }

    void UpdatePlayers()
    {
        Rovers = FindObjectsOfType<PlayerMovement>();
    }

    void OnPlayPauseClicked()
    {
        isPlaying = !isPlaying;
        Debug.Log(isPlaying ? "Play" : "Pause");

        UpdatePlayers();

        if (!isPlaying)
        {        
            startToEndTimerRunning = false;
            mazeCompletionTimerRunning = false;
            foreach (var rover in Rovers)
            {
                rover.isDriving = false;
            }
        }
        else
        {
            if (!reachedEndpoint)
            {
                startToEndTimerRunning = true;
                mazeCompletionTimerRunning = true;
            }
            foreach (var rover in Rovers)
            {
                rover.moveSpeed = int.Parse(sliderText.text);
                rover.isDriving = true;
            }
        }
    }

    void ResetMap()
    {
        // Timer
        ResetTimers();

        // Simulation
        isPlaying = false;
        DestroyAllNodes();
        DestroyAllPlayers();
    }

    void DestroyAllNodes()
    {
        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.name.StartsWith("Node "))
                Destroy(obj);
        }
        mazeMapper.nodes.Clear();
    }

    void ResetPlayers()
    {
        foreach (var pair in originalPositions)
        {
            pair.Key.transform.position = pair.Value;
        }
    }

    void DestroyAllPlayers()
    {
        foreach (var rover in Rovers)
        {
            Destroy(rover.gameObject);
        }

        Rovers = new PlayerMovement[0];
        originalPositions.Clear();
    }

    void ToggleNodesVisibility(bool isOn)
    {
        showingNodes = isOn;

        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.name.StartsWith("Node "))
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Globals.nodeAlpha = isOn ? 0.7f : 0f;
                    Color currentColor = renderer.material.color;
                    currentColor.a = isOn ? 0.7f : 0f;
                    renderer.material.color = currentColor;
                }
            }
        }
    }

    void ToggleRaycastsVisibility(bool isOn)
    {
        showingRaycasts = isOn;

        foreach (var rover in Rovers)
        {
            Raycast raycastComponent = rover.GetComponent<Raycast>();
            if (raycastComponent != null)
            {
                raycastComponent.lineAlpha = isOn ? 0.3f : 0.0f;
            }
        }
    }

    void ToggleMappedMazeVisibility(bool isOn)
    {
        showingMappedMaze = isOn;
        Debug.Log("Mapped Maze Toggled: " + isOn);
    }

    void ToggleShortestPathVisibility(bool isOn)
    {
        showingShortestPath = isOn;
        Debug.Log("Shortest Path Toggled: " + isOn);
    }
}

    /*
        TODO::::

        (smaller) TODO::::
            
            - Updated rover models/colors
            - Implementing stuff from "Toggles:" below
    */

    /* 
    
    
    UI Updates:
        - New rover models (different colors for multiple rovers)
        - Different color lines for raycasts (showing in progress vs found end of maze)
        - Showing all paths that lead to maze
        - Maze switching

    Toggles:
        - Show/hide nodes (x)
        - Show/hide raycast lines (x)
        - Show/hide entire mapped maze
        - Show/hide shortest path
    */