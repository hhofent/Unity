using UnityEngine;
using System.Collections.Generic;

// Manages the foundation’s grid system for snapping and placement.
public class FoundationBehavior : MonoBehaviour
{
    // Grid dimensions in 1/4-inch increments, accessible for other scripts.
    public int gridWidth { get; private set; }
    public int gridHeight { get; private set; }
    // Size of each grid cell (1/4 inch).
    private float cellSize = 0.25f;
    // Tracks occupied grid points to prevent overlapping placements.
    private bool[,] occupancyGrid;
    // Maps grid coordinates to world positions for snapping.
    public Dictionary<Vector2Int, Vector3> gridWorldPositions;
    // Bounds of the foundation for positioning calculations.
    private Bounds foundationBounds;

    // Initializes the foundation’s grid and snaps to terrain.
    void Start()
    {
        // Get the renderer to calculate bounds for grid positioning.
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Foundation missing Renderer component!", gameObject);
            return;
        }
        foundationBounds = renderer.bounds;

        // Calculate grid size based on foundation scale (4 cells per inch).
        Vector3 foundationScale = transform.localScale;
        gridWidth = Mathf.CeilToInt(foundationScale.x * 4f);
        gridHeight = Mathf.CeilToInt(foundationScale.z * 4f);

        // Initialize data structures for grid and occupancy.
        occupancyGrid = new bool[gridWidth, gridHeight];
        gridWorldPositions = new Dictionary<Vector2Int, Vector3>();

        // Populate grid world positions for snapping.
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 localPos = GetLocalPosition(x, y);
                Vector3 worldPos = transform.TransformPoint(localPos);
                gridWorldPositions[new Vector2Int(x, y)] = worldPos;
            }
        }

        // Snap foundation to terrain height for proper alignment.
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null)
        {
            Vector3 pos = transform.position;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + (foundationScale.y / 2f);
            transform.position = pos;
        }

        Debug.Log($"Foundation initialized: Scale {foundationScale}, Grid {gridWidth}x{gridHeight}, Cell Size: {cellSize}");
    }

    // Calculates the local position for a grid point relative to the foundation’s center.
    private Vector3 GetLocalPosition(int x, int y)
    {
        // Center the grid by offsetting from the middle and scale by cell size.
        float localX = (x - (gridWidth - 1) / 2f) * cellSize;
        float localZ = (y - (gridHeight - 1) / 2f) * cellSize;
        // Place on top surface with slight offset to avoid z-fighting.
        float localY = (foundationBounds.size.y / transform.localScale.y / 2f) + 0.05f;
        return new Vector3(localX, localY, localZ);
    }

    // Checks if a grid point is available for placement (not occupied).
    public bool CanPlaceObject(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;
        return !occupancyGrid[x, y];
    }

    // Snaps a world position to the nearest grid point on the foundation.
    public Vector3 GetNearestGridPoint(Vector3 worldPos)
    {
        // Convert world position to local space and calculate grid coordinates.
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / cellSize + (gridWidth - 1) / 2f);
        int y = Mathf.RoundToInt(localPos.z / cellSize + (gridHeight - 1) / 2f);
        // Clamp to grid bounds to prevent out-of-range access.
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);
        return gridWorldPositions[new Vector2Int(x, y)];
    }
}