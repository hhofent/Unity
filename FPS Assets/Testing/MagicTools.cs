using UnityEngine;
using UnityEngine.UI;

public class MagicTools : MonoBehaviour
{
    [SerializeField] private KeyCode testingKey = KeyCode.F;
    [SerializeField] private KeyCode axisCycleKey = KeyCode.R;
    public Camera mainCamera;
    public PlayerController playerController;
    public GameObject sawPreview;
    private PreviewManager previewManager;
    private bool isPreviewActive;
    private Text contextText;

    void Start()
    {
        mainCamera = Camera.main;
        previewManager = new PreviewManager();
        contextText = playerController.contextText;
        if (contextText != null)
            contextText.text = "";
    }

    void Update()
    {
        // Cycle axis with axisCycleKey
        if (Input.GetKeyDown(axisCycleKey))
        {
            previewManager.UpdateAxis(1); 
        }

        // Handle preview toggle
        if (Input.GetKeyDown(testingKey))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!isPreviewActive && Physics.Raycast(ray, out RaycastHit hit))
            {
                IGridObject gridObj = hit.transform.GetComponent<IGridObject>();
                if (gridObj != null && hit.transform.CompareTag("Stud"))
                {
                    previewManager.CreatePreview(sawPreview, hit, gridObj, PreviewManager.PreviewType.GameObject);
                    isPreviewActive = true;
                }
            }
            else if (isPreviewActive)
            {
                previewManager.DestroyPreview();
                isPreviewActive = false;
                if (contextText != null)
                    contextText.text = "";
                // TODO: cut stud
            }
        }

        // Update preview position
        if (isPreviewActive && contextText != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                previewManager.UpdatePreview(hit, mainCamera);
                UpdateContextText();
            }
            else
            {
                previewManager.HidePreview();
                contextText.text = "Preview hidden";
            }
        }
    }

    public void UseTool(out string action)
    {
        action = "";
        if (Input.GetKeyDown(testingKey) && !isPreviewActive)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                IGridObject gridObj = hit.transform.GetComponent<IGridObject>();
                if (gridObj != null)
                {
                    Vector3 grid = CalculateGrid(gridObj);
                    if (grid != Vector3.zero)
                    {
                        action = $"Grid size: {grid.x} x {grid.y} x {grid.z}, snapping on {previewManager.GetLockedAxisName()} axis";
                    }
                }
                else
                {
                    action = "No grid component found";
                }
            }
            else
            {
                action = "Raycast missed";
            }
        }
        else if (isPreviewActive)
        {
            action = $"Preview active, snapping on {previewManager.GetLockedAxisName()} axis";
        }
    }

    private void UpdateContextText()
    {
        IGridObject gridObject = previewManager.GetGridObject();
        
        if (gridObject == null || previewManager.GetPreviewPosition() == null) return;
        Vector3? previewPos = previewManager.GetPreviewPosition();
        Vector3 localPos = gridObject.Transform.InverseTransformPoint(previewPos.Value);
        int axisIndex = previewManager.GetLockedAxisIndex();  // Get snapped axis
        float totalLength = gridObject.Transform.localScale[axisIndex]; // Length along snapped axis
        float cutPos = localPos[axisIndex]; // Cut position relative to center
        
        // Calculate distance from each end
        float distFromStart = (totalLength / 2) + cutPos;
        float distFromEnd = (totalLength / 2) - cutPos;

        // Calculate total length to feet and inches
        int totalInches = Mathf.FloorToInt(totalLength / 0.00635f);
        int feet = totalInches / 12;
        int inches = totalInches % 12;

        // Format text
        contextText.text = $"Total length: {feet} ft, {inches} in ({totalInches}in, Proposed Cut: {distFromStart}in    <--- | --->    {distFromEnd}in";

    }

    private Vector3 CalculateGrid(IGridObject gridObj)
    {
        if (gridObj == null) return Vector3.zero;
        float cellSize = gridObj.CellSize;
        if (cellSize <= 0) return Vector3.zero;
        return new Vector3(
            gridObj.Transform.localScale.x / cellSize,
            gridObj.Transform.localScale.z / cellSize,
            gridObj.Transform.localScale.y / cellSize
        );
    }
}