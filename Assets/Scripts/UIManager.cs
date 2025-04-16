using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;



public class UIManager : MonoBehaviour
{
    // UI Elements
    public Slider slider;
    public Button addRoverButton;
    public Button playPauseButton;
    public Button resetGameButton;
    public Button setSpawnButton;
    public Button clearSpawnButton;
    public TMP_Text sliderText;
    public GameObject roverPrefab; 
    public Dropdown dropdown;



    // Rover Related Properties
    private PlayerMovement[] Rovers;
    private bool isPlaying = true;
    private Dictionary<PlayerMovement, Vector3> originalPositions = new(); // Original positions of rovers for resetting positions
    public GameObject xMarkerPrefab;
    private GameObject activeXMarker;
    private bool isPlacing = false;
    private bool spawnPointSet = false;
    private Vector2 spawnPoint;



    // Maze Related Properties
    private static GameObject maze;
    private MazeMapper mazeMapper;
    public bool openMaze;
    public string mazeName;

    public Dictionary<string, (string type, Vector2 spawnPoint)> Mazes = new Dictionary<string, (string, Vector2)> // Dictionary mapping maze names and if they're open or closed AND their spawnpoints {"name" -> "open/closed", spawnpoint}
    {
        //{ "name", ("status", spawnpoint) } Leave spawnpoint as Vector.zero if maze is closed

        { "10 by 10 orthogonal maze", ("open", new Vector2(0.05f, 5f)) },    // Example with predefined spawn point from original maze
        { "maze2", ("closed", Vector2.zero) },
    };


    void Update()
    {
        UpdateSpawnPointPlacement();
    }

    void Start()
    {
        // UI ELEMENT LISTENERS
        slider.onValueChanged.AddListener(OnSliderChanged);
        addRoverButton.onClick.AddListener(OnAddRoverClicked);
        playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        resetGameButton.onClick.AddListener(ResetMap);
        Rovers = FindObjectsOfType<PlayerMovement>();
        setSpawnButton.onClick.AddListener(StartPlacingSpawnPoint);
        clearSpawnButton.onClick.AddListener(OnClearSpawnPointClicked);

        // DROPDOWN LIST 
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });
        dropdown.ClearOptions();
        
        List<string> formattedOptions = new List<string>();
        foreach (var maze in Mazes)
        {
            formattedOptions.Add($"{maze.Key} ({maze.Value.type})"); // e.g., maze1 (open)
            Debug.Log(maze.Value.type);
        }
        dropdown.AddOptions(formattedOptions);
        
        

        // MAZE INFO
        mazeName = Mazes.Keys.First();
        maze = GameObject.Find(mazeName);

        if (maze == null)
        {
            Debug.LogError($"Could not find Maze named '{mazeName}'");
            return; // Early exit if maze not found
        }

        mazeMapper = maze.GetComponent<MazeMapper>();
        openMaze = Mazes[mazeName].type == "open";

        if (mazeMapper != null)
        {
            UpdateUIForMazeType();
        }
        else
        {
            Debug.LogWarning($"No MazeMapper component on '{mazeName}'");
        }



        // GET PLAYERS
        var players = FindObjectsOfType<PlayerMovement>();
        foreach (var player in players)
        {
            originalPositions[player] = player.transform.position;
        }

    }


    void UpdateUIForMazeType()
    {
        // Disable spawn button if maze is open
        setSpawnButton.interactable = !openMaze;
        clearSpawnButton.interactable = !openMaze;
        
        if (openMaze) // TODO:: CHANGE MAZE SCENE TO MATCH NEW MAZES WHEN MORE MAZES ARE IMPLEMENTED
        {
            Debug.Log("Added Marker and Updating Spawnpoint");
            // Clear any existing spawn points
            if (activeXMarker != null)
            {
                Destroy(activeXMarker);
                activeXMarker = null;
            }
            
            // Get the predefined spawn point from dictionary and snap it to grid
            Vector2 rawSpawnPoint = Mazes[mazeName].spawnPoint;
            Vector3 snappedPosition = mazeMapper.SnapToGrid(new Vector3(rawSpawnPoint.x, rawSpawnPoint.y, 0f));
            
            // Set the spawn point
            spawnPoint = snappedPosition;
            spawnPointSet = true;
            
            // Create marker at the predefined spawn point
            activeXMarker = Instantiate(xMarkerPrefab, spawnPoint, Quaternion.identity);
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
        isPlacing = true;
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

    void UpdateSpawnPointPlacement()
    {
        if (!isPlacing || mazeMapper == null) {
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
            isPlacing = false;
            Debug.Log("Spawn Point Set at: " + spawnPoint);
        }
    }

    void OnClearSpawnPointClicked()
    {
        if (!spawnPointSet)
        {
            Debug.LogWarning("No spawn point to clear!");
            return;
        }

        // Clear spawn point values
        spawnPointSet = false;
        spawnPoint = Vector3.zero;

        // Destroy the existing X marker if it exists
        if (activeXMarker != null)
        {
            Destroy(activeXMarker);
            activeXMarker = null;
        }

        Debug.Log("Spawn point has been cleared.");
    }



    void OnSliderChanged(float value)
    {
        Debug.Log("Slider Value: " + value);
        if (sliderText != null)
            sliderText.text = value.ToString("0");

        // Update all rovers movespeed to match the slider value
        foreach (var rover in Rovers) 
        {
            rover.moveSpeed = int.Parse(sliderText.text);
        }

    }

    void DropdownValueChanged(Dropdown change)
    {
        string fullLabel = change.options[change.value].text;
        string selectedMaze = fullLabel.Substring(0, fullLabel.LastIndexOf(" (")).Trim();
        
        Debug.Log("CHANGED NAMES HERE IS NEW: " + mazeName);
        // Update mazeName
        mazeName = selectedMaze;
        
        // Get maze status
        var mazeData = Mazes[mazeName];
        string mazeStatus = mazeData.type;
        
        Debug.Log($"Selected Maze: {mazeName}, Status: {mazeStatus}");
        
        


        // Update the maze reference
        maze = GameObject.Find(mazeName);
        if (maze != null)
        {
            mazeMapper = maze.GetComponent<MazeMapper>();
            Debug.Log($"Found maze: {mazeName}, MazeMapper: {(mazeMapper != null ? "Found" : "Not Found")}");
        }
        else
        {
            Debug.LogError($"Could not find GameObject named '{mazeName}'");
            return; // Exit early to prevent null reference exceptions
        }




        if (mazeStatus == "open")
        {
            Debug.Log("Maze is open");
            openMaze = true;
        }
        else if (mazeStatus == "closed")
        {
            Debug.Log("Maze is closed");
            openMaze = false;
            
            // Clear spawn point for closed mazes (user will set it)
            spawnPointSet = false;
            
            // Clear any existing marker
            if (activeXMarker != null)
            {
                Destroy(activeXMarker);
                activeXMarker = null;
            }
        }
        
        // Update UI elements based on the new maze type
        UpdateUIForMazeType();
    }


    void OnAddRoverClicked()
    {
        if (!spawnPointSet)
        {
            Debug.LogWarning("Spawn point has not been set yet!");
            return;
        }

        // Use the snapped spawnPoint set earlier
        Vector3 spawnPos = spawnPoint;

        // Instantiate the rover prefab
        GameObject newRover = Instantiate(roverPrefab, spawnPos, Quaternion.identity);

        // Optional: Set the slider speed immediately
        PlayerMovement pm = newRover.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.moveSpeed = int.Parse(sliderText.text);
        }

        // Refresh rover list
        UpdatePlayers();

        Debug.Log("Rover spawned at: " + spawnPos);
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
            foreach (var rover in Rovers)
            {
                rover.moveSpeed = 0;
            }
        }
        else
        {
            foreach (var rover in Rovers)
            {
                rover.moveSpeed = int.Parse(sliderText.text);
            }
        }
    }


    void ResetMap()
    {
        DestroyAllNodes();
        ResetPlayers();
    }

    void DestroyAllNodes()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Node "))
            {
                Destroy(obj);
            }
        }
    }

    void ResetPlayers()
    {
        foreach (var pair in originalPositions)
        {
            pair.Key.transform.position = pair.Value;
            //pair.Key.ResetMovement(); // Optional if you have internal states like pathing
        }
    }

    /*
        TODO::::

            - Fix reset positions code (talk w/ gavin)
            - Fix rover collisions (talk w/ gavin)
            - Implement maze switching (talk w/ ahmed)
            - 

        (smaller) TODO::::
            
            - Updated rover models/colors
            - Maze completion timer
            - Implementing stuff from "Toggles:" below
    */

    /* 
    
    User Generated Spawnpoint and Endpoint: (SPAWNPOINT DONE)
        - Generated on a user click (used for starting maze, resetting positions)
        - SnaptoGrid() -> Mazemapper.cs input: vector3 (clicks create a vector 2, just add a 0)
    
    UI Updates:
        - New rover models (different colors for multiple rovers)
        - Different color lines for raycasts (showing in progress vs found end of maze)
        - Showing all paths that lead to maze
        - Maze switching

    Toggles:
        - Show/hide nodes
        - Show/hide raycast lines
        - Show/hide entire mapped maze
        - Show/hide shortest path
    */
}
