using UnityEngine;
using System.Collections.Generic;

public class FoundationBehavior : MonoBehaviour
{
    public int gridWidth { get; private set; }
    public int gridHeight { get; private set; }
    private float cellSize = 0.00635f; // 1/4 inch
    private bool[,] occupancyGrid;
    public Dictionary<Vector2Int, Vector3> gridWorldPositions;
    private Bounds bounds;
    private Dictionary<Vector2Int, GameObject> pencilMarks;
    public GameObject pencilMarkPrefab;
    private Dictionary<Vector2Int, TextMesh> dimensionTexts;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (!rend) { Debug.LogError("Foundation missing Renderer!", gameObject); return; }
        bounds = rend.bounds;

        Vector3 scale = transform.localScale;
        gridWidth = Mathf.CeilToInt(scale.x / cellSize);
        gridHeight = Mathf.CeilToInt(scale.z / cellSize);

        const int maxGridSize = 1000;
        if (gridWidth > maxGridSize || gridHeight > maxGridSize)
        {
            Debug.LogError($"Grid size too large: {gridWidth}x{gridHeight}. Check scale: {scale}.");
            gridWidth = Mathf.Min(gridWidth, maxGridSize);
            gridHeight = Mathf.Min(gridHeight, maxGridSize);
        }

        Debug.Log($"Grid Size: {gridWidth}x{gridHeight}, Foundation Scale: {scale}");

        occupancyGrid = new bool[gridWidth, gridHeight];
        gridWorldPositions = new Dictionary<Vector2Int, Vector3>();
        pencilMarks = new Dictionary<Vector2Int, GameObject>();
        dimensionTexts = new Dictionary<Vector2Int, TextMesh>();

        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain)
        {
            Vector3 pos = transform.position;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + (scale.y / 2f);
            transform.position = pos;
        }
    }

    private Vector3 GetLocalPosition(int x, int y)
    {
        float localX = (x - (gridWidth - 1) / 2f) * cellSize;
        float localZ = (y - (gridHeight - 1) / 2f) * cellSize;
        float localY = (bounds.size.y / transform.localScale.y / 2f) + 0.0127f; // 1/2 inch
        return new Vector3(localX, localY, localZ);
    }

    public bool CanPlaceObject(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;
        return !occupancyGrid[x, y];
    }

    public Vector3 GetNearestGridPoint(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / cellSize + (gridWidth - 1) / 2f);
        int y = Mathf.RoundToInt(localPos.z / cellSize + (gridHeight - 1) / 2f);
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);
        Vector2Int gridPoint = new Vector2Int(x, y);

        if (!gridWorldPositions.ContainsKey(gridPoint))
        {
            Vector3 localGridPos = GetLocalPosition(x, y);
            gridWorldPositions[gridPoint] = transform.TransformPoint(localGridPos);
            Debug.Log($"Generated grid point ({x}, {y}) at {gridWorldPositions[gridPoint]}");
        }
        return gridWorldPositions[gridPoint];
    }

    public void PlacePencilMark(Vector2Int gridPoint)
    {
        if (!pencilMarks.ContainsKey(gridPoint))
        {
            if (pencilMarkPrefab == null)
            {
                Debug.LogError("PencilMarkPrefab is not assigned!", this);
                return;
            }
            Vector3 worldPos = gridWorldPositions[gridPoint];
            GameObject mark = Instantiate(pencilMarkPrefab, worldPos, Quaternion.identity, transform);
            mark.transform.localRotation = Quaternion.Euler(0, 0, 90); // Align with X-axis
            pencilMarks[gridPoint] = mark;
        }
    }

    public void PlaceDimensionText(Vector2Int point1, Vector2Int point2, float length, int locationNumber)
    {
        Vector3 pos1 = gridWorldPositions[point1];
        Vector3 pos2 = gridWorldPositions[point2];
        Vector3 midPoint = (pos1 + pos2) / 2f + Vector3.up * 0.0254f;

        GameObject textObj = new GameObject("DimensionText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.position = midPoint;
        textObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        TextMesh text = textObj.AddComponent<TextMesh>();
        float lengthInInches = length * 39.3701f;
        text.text = $"Location {locationNumber}: {lengthInInches:F2} inches";
        text.fontSize = 50;
        text.color = Color.white;
        text.anchor = TextAnchor.MiddleCenter;
        text.characterSize = 0.00254f;

        dimensionTexts[point1] = text;
        dimensionTexts[point2] = text;
    }

    public void ClearPencilMarks()
    {
        foreach (var mark in pencilMarks.Values) Destroy(mark);
        pencilMarks.Clear();
        foreach (var text in new HashSet<TextMesh>(dimensionTexts.Values)) Destroy(text.gameObject);
        dimensionTexts.Clear();
    }
}