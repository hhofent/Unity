using UnityEngine;

public class StudMethods : MonoBehaviour, IGridObject
{
    [SerializeField] public float gridAccuracy = 0.25f;
    public string gridType = "stud";
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
