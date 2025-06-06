using UnityEngine;

public class FoundationMethods : MonoBehaviour
{
    [SerializeField] public float gridAccuracy = 1f;
    public string gridType = "wall";

    public float cellSize;

    void Start()
    {
        // Convert grid accuracy to meters
        cellSize = gridAccuracy * 0.0254f;
    }
}
