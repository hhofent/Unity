using UnityEngine;
using System.Collections.Generic;

// Manages the foundation's grid system for snapping and placement
public class FoundationBehavior : MonoBehaviour
{
    public int gridWidth { get; private set; }
    public int gridHeight { get; private set; }
    private float cellSize = 0.00635f; // Set to 1/4" in meters
    public bool[,] occupancyGrid;
    public Dictionary<Vector2Int, Vector3> gridWorldPositions;
    private Bounds bounds;
    private Dictionary<Vector2Int, GameObject> pencilMarks;
    public GameObject pencilMarkPrefab;
    private Dictionary<Vector2Int, TextMesh> dimensionTexts;

    // Initialize grid and snaps to terrain
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (!rend) { Debug.LogError("Foundation missing Renderer!", gameObject); return; }
        bounds = rend.bounds;

        Vector3 scale = transform.localScale;
        gridWidth = Mathf.CeilToInt(scale.x / cellSize);
        gridHeight = Mathf.CeilToInt(scale.z / cellSize);

        // Safety limit
        const int MAX_GRID_SIZE = 1000;
        if (gridWidth > MAX_GRID_SIZE || gridHeight > MAX_GRID_SIZE)
        {
            Debug.LogError($"Grid size too large: {gridWidth}x{gridHeight}, Check foundation scale {scale}, using smaller grid");
            gridWidth = Mathf.Min(gridWidth, MAX_GRID_SIZE);
            gridHeight = Mathf.Min(gridHeight, MAX_GRID_SIZE);
        }

        Debug.Log($"Grid size: {gridWidth}x{gridHeight}, Foundation Scale: {scale}");

        occupancyGrid = new bool[gridWidth, gridHeight];
        gridWorldPositions = new Dictionary<Vector2Int, Vector3>();
        pencilMarks = new Dictionary<Vector2Int, GameObject>();
        dimensionTexts = new Dictionary<Vector2Int, TextMesh>();

        Terrain terrain = FindFirstObjectByType<Terrain>();
        if (terrain)
        {
            Vector3 pos = transform.position;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + (scale.y / 2f);
            transform.position = pos;
        }
    }

    // Calculates local position for a grid point
    private Vector3 GetLocalPosition(int x, int y)
    {
        float localX = (x - (gridWidth - 1) / 2f) * cellSize;
        float localZ = (y - (gridHeight - 1) / 2f) * cellSize;
        float localY = (bounds.size.y / transform.localScale.y / 2f) + 0.5f;
        return new Vector3(localX, localY, localZ);
    }

    // Checks if grid point is free
    public bool CanPlaceObject(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;
        return !occupancyGrid[x, y];
    }

    // Snaps world position to nearest grid point
    public Vector3 GetNearestGridPoint(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / cellSize + (gridWidth - 1) / 2f);
        int y = Mathf.RoundToInt(localPos.z / cellSize + (gridHeight - 1) / 2f);
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);
        return gridWorldPositions[new Vector2Int(x, y)];
    }

    // Places a pencil mark at a grid point
    public void PlacePencilMark(Vector2Int gridPoint)
    {
        if (!pencilMarks.ContainsKey(gridPoint))
        {
            if (pencilMarkPrefab == null)
            {
                Debug.LogError("Pencil mark prefab not assigned!", this);
                return;
            }
            Vector3 worldPos = gridWorldPositions[gridPoint];
            GameObject mark = Instantiate(pencilMarkPrefab, worldPos, Quaternion.identity, transform);
            pencilMarks[gridPoint] = mark;
        }
    }

    // New: Mark perimeter for stud walls
    public void MarkPerimeterStuds(float spacing, out string action)
    {
        action = "";
        List<Vector2Int> perimeterPoints = new List<Vector2Int>();
        int xMin = 0, xMax = gridWidth - 1, yMin = 0, yMax = gridHeight - 1;
        int spacingCells = Mathf.RoundToInt(spacing / cellSize); // e.g., 16 inches / 1 inch = 16 cells

        // Top edge (y = yMax)
        for (int x = xMin; x <= xMax; x += spacingCells)
        {
            Vector2Int point = new Vector2Int(x, yMax);
            if (CanPlaceObject(x, yMax)) perimeterPoints.Add(point);
        }
        // Bottom edge (y = yMin)
        for (int x = xMin; x <= xMax; x += spacingCells)
        {
            Vector2Int point = new Vector2Int(x, yMin);
            if (CanPlaceObject(x, yMin)) perimeterPoints.Add(point);
        }
        // Left edge (x = xMin, exclude corners)
        for (int y = yMin + spacingCells; y <= yMax - spacingCells; y += spacingCells)
        {
            Vector2Int point = new Vector2Int(xMin, y);
            if (CanPlaceObject(xMin, y)) perimeterPoints.Add(point);
        }
        // Right edge (x = xMax, exclude corners)
        for (int y = yMin + spacingCells; y <= yMax - spacingCells; y += spacingCells)
        {
            Vector2Int point = new Vector2Int(xMax, y);
            if (CanPlaceObject(xMax, y)) perimeterPoints.Add(point);
        }

        foreach (var point in perimeterPoints)
        {
            PlacePencilMark(point);
            occupancyGrid[point.x, point.y] = true; // Reserve for studs
        }

        action = $"Marked {perimeterPoints.Count} stud positions at {spacing * 39.3701f:F2} inch spacing";
    }

    // place dimension text between points
    public void PlaceDimensionText(Vector2Int point1, Vector2Int point2, float length, int locationNumber)
    {
        Vector3 pos1 = gridWorldPositions[point1];
        Vector3 pos2 = gridWorldPositions[point2];
        Vector3 midPoint = (pos1 + pos2) / 2f;
        
        GameObject textObj = new GameObject("Dimension Text");
        textObj.transform.SetParent(transform, false);
        textObj.transform.position = midPoint;
        textObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        TextMesh text = textObj.AddComponent<TextMesh>();
        float lengthInInches = length * 39.3701f;
        text.text = $"Location {locationNumber}: {lengthInInches:F2} inches";
        text.fontSize = 50;
        text.color = Color.white;
        text.anchor = TextAnchor.MiddleCenter;
        text.characterSize = 0.02f;

        dimensionTexts[point1] = text;
        dimensionTexts[point2] = text;
    }

    // Clears all pencil marks and dimension text
    public void ClearPencilMarks()
    {
        foreach (var mark in pencilMarks.Values) Destroy(mark);
        pencilMarks.Clear();
        foreach (var text in new HashSet<TextMesh>(dimensionTexts.Values)) Destroy(text.gameObject);
        dimensionTexts.Clear();
    }
}