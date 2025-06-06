//Foundation
//    Types
//        wall
//        pole
//        blank
//
//Walls
//    Types
//        exterior
//            paneling
//            studs
//            drywall
//        interior
//            drywall
//            studs
//            drywall
//    
//    Parts
//        Paneling
//            4' x 8' sheets
//            1/2" thickness
//        Drywall
//            4' x 8' sheets
//            1/2" thickness
//        Studs
//            Dimensions
//                2x4
//                2x6
//            Lengths
//                8 ft
//                16 ft
//        Screws



// Wallmaker
// void PreviewWall()
// {
    
// }

// void PlaceWall()
// {
    
// }

using UnityEngine;

public class MagicTools : MonoBehaviour
{
    [SerializeField] private KeyCode testingKey = KeyCode.F;
    public Camera mainCamera;
    public PlayerController playerController;
    private FoundationMethods foundation;
    private StudMethods stud;
    public GameObject sawPreview;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void UseTool(out string action)
    {
        action = "";
        if (Input.GetKeyDown(testingKey)) 
        {
            action = "Calculating grid";
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Foundation")) // Use CompareTag for better performance
                {
                    foundation = hit.transform.GetComponent<FoundationMethods>();
                    if (foundation != null)
                    {
                        Debug.Log("Foundation set.");
                        if (CalculateGrid("Foundation") != Vector3.zero)
                        {
                            Vector3 grid = CalculateGrid("Foundation");
                            Debug.Log($"Grid size: {grid.x}x{grid.y}x{grid.z}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Hit object has 'Foundation' tag but no FoundationMethods component.");
                        action = "No FoundationMethods component found.";
                    }
                }
                else if (hit.transform.CompareTag("Stud"))
                {
                    stud = hit.transform.GetComponent<StudMethods>();
                    if (stud != null)
                    {
                        Debug.Log("Stud set.");
                        if (CalculateGrid("Stud") != Vector3.zero)
                        {
                            Vector3 grid = CalculateGrid("Stud");
                            GameObject cutPreview = Instantiate(sawPreview, hit.point, Quaternion.identity);
                            cutPreview.SetActive(true);
                            Debug.Log($"Grid size: {grid.x}x{grid.y}x{grid.z}");
                        }
                        action = "Stud set.";
                    }
                    else
                    {
                        Debug.LogWarning("Hit object has 'Stud' tag but no StudMethods component.");
                        action = "No StudMethods component found.";
                    }
                }
                   else
                {
                    Debug.Log("Raycast did not hit a Foundation object.");
                    action = "No Foundation hit.";
                }
            }
            else
            {
                Debug.Log("Raycast missed.");
                action = "Raycast missed.";
            }
        }
    }

    public Vector3 CalculateGrid(string tag)
    {
        if (tag == "Foundation")
        {
            if (foundation.gridType == "wall")
            {
                float cellSize = foundation.cellSize;
                Vector3 grid = new Vector3((foundation.transform.localScale.x / cellSize), foundation.transform.localScale.z / cellSize, foundation.transform.localScale.y / cellSize);
                return grid;
            }
        }
        else if (tag == "Stud")
        {
            float cellSize = stud.cellSize;
            Vector3 grid = new Vector3((stud.transform.localScale.x / cellSize), stud.transform.localScale.z / cellSize, stud.transform.localScale.y / cellSize);
            return grid;
        }
        return Vector3.zero;
    }
}
    