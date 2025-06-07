using UnityEngine;
using System.Collections.Generic;

public class TapeMeasureNotebookTool : MonoBehaviour
{
    public FoundationBehavior foundation;
    public GameObject pencilMarkPrefab;
    private List<Vector2Int> currentMarks = new List<Vector2Int>();
    private List<(int locationNumber, float length)> measurements = new List<(int, float)>();
    private Dictionary<GameObject, List<float>> studMarks = new Dictionary<GameObject, List<float>>();
    private int currentLocationNumber = 1;
    private LineRenderer tapeLine;
    private GameObject markPreview;
    private float cellSize = 0.00635f;

    // Initialize LineRenderer for visualization
    void Start()
    {
        tapeLine = gameObject.AddComponent<LineRenderer>();
        Material yellowMaterial = new Material(Shader.Find("Sprites/Default"));
        yellowMaterial.color = Color.yellow;
        tapeLine.material = yellowMaterial;
        tapeLine.startColor = Color.yellow;
        tapeLine.endColor = Color.yellow;
        tapeLine.startWidth = 0.1f;
        tapeLine.endWidth = 0.1f;
        tapeLine.enabled = false;

        markPreview = Instantiate(pencilMarkPrefab, Vector3.zero, Quaternion.identity);
        markPreview.SetActive(false);
        var renderer = markPreview.GetComponent<Renderer>();
        renderer.material.color = Color.green;
    }

    // Handle tape measure functionality
    public void UseTool(out string action, KeyCode resetMarksKey)
    {
        action = "";
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 2.54f))
        {
            if (hit.transform.CompareTag("Stud"))
            {
                StudBehavior stud = hit.transform.GetComponent<StudBehavior>();
                Vector3 localHit = hit.transform.InverseTransformPoint(hit.point);
                float snapX = Mathf.Round(localHit.x / cellSize) * cellSize;
                snapX = Mathf.Clamp(snapX, -stud.Length / 2, stud.Length / 2);

                markPreview.SetActive(true);
                markPreview.transform.SetParent(hit.transform);
                markPreview.transform.localPosition = new Vector3(snapX, 0.0762f, 0);
                markPreview.transform.localRotation = Quaternion.Euler(0, 0, 90);

                if (Input.GetMouseButtonDown(0))
                {
                    if (!studMarks.ContainsKey(hit.transform.gameObject))
                        studMarks[hit.transform.gameObject] = new List<float>();
                    if (!studMarks[hit.transform.gameObject].Contains(snapX))
                    {
                        studMarks[hit.transform.gameObject].Add(snapX);
                        GameObject mark = Instantiate(pencilMarkPrefab, markPreview.transform.position, markPreview.transform.rotation, hit.transform);
                        stud.AddMark(mark);
                        action = $"Marked stud at {snapX * 39.3701f:F2} inches from center";
                    }
                }
            }

            else if (hit.transform == foundation.transform)
            {
                Vector3 snapPoint = foundation.GetNearestGridPoint(hit.point);
                Vector3 localPos = foundation.transform.InverseTransformPoint(snapPoint);
                int x = Mathf.RoundToInt(localPos.x / cellSize + (foundation.gridWidth - 1) / 2f);
                int y = Mathf.RoundToInt(localPos.z / cellSize + (foundation.gridHeight - 1) / 2f);
                Vector2Int gridPoint = new Vector2Int(x, y);
                
                // Update LineRenderer
                if (currentMarks.Count == 1)
                {
                    tapeLine.enabled = true;
                    tapeLine.positionCount = 2;
                    tapeLine.SetPosition(0, foundation.gridWorldPositions[currentMarks[0]]);
                    tapeLine.SetPosition(1, snapPoint);
                }

                // Place mark on click
                if (Input.GetMouseButtonDown(0) && currentMarks.Count < 2)
                {
                    currentMarks.Add(gridPoint);
                    foundation.PlacePencilMark(gridPoint);
                    action = $"Placed mark at ({gridPoint.x}, {gridPoint.y})";
                    if (currentMarks.Count == 2)
                    {
                        float length = Vector3.Distance(foundation.gridWorldPositions[currentMarks[0]], foundation.gridWorldPositions[currentMarks[1]]);
                        float lengthInInches = length * 39.3701f;
                        action = $"Measured length: {lengthInInches:F2} inches. Press Enter to save as location {currentLocationNumber}";
                    }
                }
            }  
        }
        else
        {
            markPreview.SetActive(false);
        }

        // Clear marks
        if (Input.GetKeyDown(resetMarksKey))
        {
            currentMarks.Clear();
            foreach (var stud in studMarks.Keys)
            {
                var studBehavior = stud.GetComponent<StudBehavior>();
                studBehavior.ClearMarks();
            }
            studMarks.Clear();
            tapeLine.enabled = false;
            foundation.ClearPencilMarks();
            action = "Cleared marks.";
        }
    }

    public List<Vector2Int> GetCurrentMarks() => currentMarks;
    public Dictionary<GameObject, List<float>> GetStudMarks() => studMarks;
    
    // Get all measurements
    public List<(int locationNumber, float length)> GetMeasurements()
    {
        return measurements;
    }
    // Save measurement with location number
    public void SaveMeasurement(out string action)
    {
        action = "";
        if (currentMarks.Count == 2)
        {
            float measuredLength = Vector3.Distance(foundation.gridWorldPositions[currentMarks[0]], foundation.gridWorldPositions[currentMarks[1]]);
            float lengthInInches = measuredLength * 39.3701f;
            measurements.Add((currentLocationNumber, measuredLength));
            foundation.PlaceDimensionText(currentMarks[0], currentMarks[1], measuredLength, currentLocationNumber);
            action = $"Saved location {currentLocationNumber}: {lengthInInches:F2} inches";
            Debug.Log($"Saved measurement: Location {currentLocationNumber}, Length {measuredLength}m ({lengthInInches:F2} inches)");
            currentLocationNumber++;
            currentMarks.Clear();
            tapeLine.enabled = false;
        }
        else
        {
            action = "Cannot save: Need two marks";
            Debug.Log("Save attempted with insufficient marks");
        }
    }
    
    // Clear all measurements
    public void ClearMeasurements()
    {
        measurements.Clear();
        currentMarks.Clear();
        currentLocationNumber = 1;
        tapeLine.enabled = false;
        foundation.ClearPencilMarks();
    }

}


