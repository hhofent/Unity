using UnityEngine;

public class FoundationMethods : MonoBehaviour, IGridObject
{
    [SerializeField] public float gridAccuracy = 1f;
    public string gridType = "wall";
    public float cellSize;

    void Start()
    {
        // Convert grid accuracy to meters
        cellSize = gridAccuracy * 0.0254f;
    }

    public float CellSize => cellSize;
    public string GridType => gridType;
    public Transform Transform => transform;
}
