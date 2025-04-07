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
    public TMP_Text sliderText;
    public GameObject roverPrefab; 
    public Transform spawnPoint; 

    private bool isPlaying = true;
    private PlayerMovement[] Rovers;

    private Dictionary<PlayerMovement, Vector3> originalPositions = new(); // Original positions of rovers for resetting positions



    void Start()
    {
        // Add Listeners
        slider.onValueChanged.AddListener(OnSliderChanged);
        addRoverButton.onClick.AddListener(OnAddRoverClicked);
        playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        resetGameButton.onClick.AddListener(ResetMap);
        Rovers = FindObjectsOfType<PlayerMovement>();

        var players = FindObjectsOfType<PlayerMovement>();
        foreach (var player in players)
        {
            originalPositions[player] = player.transform.position;
        }

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
        Debug.Log("Add Rover button clicked!");

        // Choose a spawn position
        Vector3 spawnPos;

        if (spawnPoint == null){
            spawnPos = Vector3.zero;
        } else {
            spawnPos = spawnPoint.position;
        }

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
}
