using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 



public class UIManager : MonoBehaviour
{
    public Slider slider;
    public Button addRoverButton;
    public Button playPauseButton;
    public Button resetGameButton;
    public Button setSpawnButton;
    public Button clearSpawnButton;
    public TMP_Text sliderText;
    public GameObject roverPrefab; 
    //public Transform spawnPoint; 


    private bool isPlaying = true;
    private PlayerMovement[] Rovers;

    private Dictionary<PlayerMovement, Vector3> originalPositions = new(); // Original positions of rovers for resetting positions


    public GameObject xMarkerPrefab; // Assign your "X" marker prefab in the Inspector
    private GameObject activeXMarker;
    private bool isPlacing = false;
    private bool spawnPointSet = false;
    private Vector2 spawnPoint;


    private static GameObject maze;
    private MazeMapper mazeMapper;



    void Update()
    {
        UpdateSpawnPointPlacement();
    }

    void Start()
    {
        // Add Listeners
        slider.onValueChanged.AddListener(OnSliderChanged);
        addRoverButton.onClick.AddListener(OnAddRoverClicked);
        playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        resetGameButton.onClick.AddListener(ResetMap);
        Rovers = FindObjectsOfType<PlayerMovement>();
        setSpawnButton.onClick.AddListener(StartPlacingSpawnPoint);
            clearSpawnButton.onClick.AddListener(OnClearSpawnPointClicked);
        
        maze = GameObject.Find("10 by 10 orthogonal maze");
        mazeMapper = maze.GetComponent<MazeMapper>();

        var players = FindObjectsOfType<PlayerMovement>();
        foreach (var player in players)
        {
            originalPositions[player] = player.transform.position;
        }

    }

    void StartPlacingSpawnPoint()
    {
        if (!spawnPointSet)
        {
            isPlacing = true;
            if (activeXMarker == null)
            {
                Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                activeXMarker = Instantiate(xMarkerPrefab, startPos, Quaternion.identity);
            }
        }
    }


    void UpdateSpawnPointPlacement()
    {
        if (isPlacing && !spawnPointSet)
        {
            Vector2 rawMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 snappedPosition = mazeMapper.SnapToGrid(new Vector3(rawMousePos.x, rawMousePos.y, 0f));

            if (activeXMarker != null)
            {
                activeXMarker.transform.position = snappedPosition;
            }

            if (Input.GetMouseButtonDown(0))
            {
                spawnPoint = snappedPosition;
                spawnPointSet = true;
                isPlacing = false;
                Debug.Log("Spawn Point Set at: " + spawnPoint);
            }
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

    void ResetPlayers() // 
    {
        foreach (var pair in originalPositions)
        {
            pair.Key.transform.position = pair.Value;
            //pair.Key.ResetMovement(); // Optional if you have internal states like pathing
        }
    }

    // SnaptoGrid() -> Mazemapper.cs input: vector3 (clicks create a vector 2, just add a 0)
    // different colors for different rover
    // clicking for spawnpoints
    // Showing all paths that lead to the end
    // Timer to show how long the maze is taking to complete


    /* 
    
    User Generated Spawnpoint and Endpoint:
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
