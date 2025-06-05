using UnityEngine;

// Manages properties and behavior of a 2x6 stud.
public class StudBehavior : MonoBehaviour
{
    // Gets the current length of the stud (X-scale in inches).
    public float Length { get { return transform.localScale.x; } }
    // Stores the stud’s bounds for positioning calculations.
    private Bounds studBounds;

    // Initializes the stud and its bounds.
    void Start()
    {
        // Get the renderer to calculate bounds for top surface positioning.
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Stud missing Renderer component!", gameObject);
            return;
        }
        studBounds = renderer.bounds;
    }

    // Returns the world position of the stud’s top surface.
    public Vector3 GetTopSurfacePosition()
    {
        // Calculate the top surface position (center + half height).
        return transform.position + Vector3.up * (studBounds.size.y / 2f);
    }
}