using UnityEngine;

public class StudMethods : MonoBehaviour
{
    [SerializeField] public float gridAccuracy = 0.25f;

    public float cellSize;

    void Start()
    {
        // Convert grid accuracy to meters
        cellSize = gridAccuracy * 0.0254f;
    }
    
}
