using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Add this component to your node GameObject
public class NodeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MapNode nodeData;
    private Canvas tooltipCanvas;
    private Text tooltipText;
    private bool isTooltipVisible = false; 
    
    private static GameObject maze;
    private MazeMapper mazeMapper;
    
    public void SetNodeData(MapNode node)
    {
        this.nodeData = node;
        // UpdateTooltipText();
    }
    
    void Start()
    {
        // Create tooltip UI elements if not already created
        CreateTooltip();
        
        // Hide tooltip initially
        ShowTooltip(false);

        maze = GameObject.Find("10 by 10 orthogonal maze");
        mazeMapper = maze.GetComponent<MazeMapper>();
    }
    
    private void CreateTooltip()
    {
        // Create canvas for tooltip
        GameObject canvasObj = new GameObject("TooltipCanvas");
        tooltipCanvas = canvasObj.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.WorldSpace;
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, 0.7f, -5);
        canvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        
        // Add canvas scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // Create background panel
        GameObject panelObj = new GameObject("TooltipPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        panelObj.transform.localPosition = Vector3.zero;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Create text
        GameObject textObj = new GameObject("TooltipText");
        textObj.transform.SetParent(panelObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        
        tooltipText = textObj.AddComponent<Text>();
        tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipText.fontSize = 1;
        tooltipText.color = Color.white;
        tooltipText.alignment = TextAnchor.MiddleCenter;
        tooltipText.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Set layout
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(8, 6);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(20, 14);
        
        // Hide initially
        ShowTooltip(false);
    }

    private void UpdateTooltipText()
    {


        MapNode tempNode = mazeMapper.getNode(this.nodeData.position);
        Debug.Log(tempNode.nodeID);

        if (nodeData is MapNode node && tooltipText != null)
        {
            tooltipText.text = $"Node ID: {tempNode.nodeID}\n" +
                              $"Position: {tempNode.position}\n" + 
                              $"Unexplored: {tempNode.mapUnexplored}\n" + 
                              $"DeadEnd: {tempNode.mapDeadEnd}\n" + 
                              $"WIP: {tempNode.mapWIP}\n" + 
                              $"Completed: {tempNode.mapCompleted}\n" +
                              $"Last Updated: {System.DateTime.Now}";
            
            // Add any other node properties you want to display
        }
    }
    
    private void ShowTooltip(bool show)
    {
        
        isTooltipVisible = show;
        if (tooltipCanvas != null)
        {
            tooltipCanvas.gameObject.SetActive(show);
        }
    }
    
    // Called when pointer enters the node collider
    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateTooltipText();
        ShowTooltip(true);
    }
    
    // Called when pointer exits the node collider
    public void OnPointerExit(PointerEventData eventData)
    {
        ShowTooltip(false);
    }

    void Update()
    {
        if (isTooltipVisible) // Only update when tooltip is visible
        {
            // UpdateTooltipText();
        }
    }

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     UpdateTooltipText();
    //     ShowTooltip(true);
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     ShowTooltip(false);
    // }



}