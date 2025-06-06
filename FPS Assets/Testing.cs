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



// Wallmaker.cs
void PreviewWall()
{
    
}

void PlaceWall()
{
    
}

// FoundationBehavior.cs
using UnityEngine;
using System.Collections.Generic;

public class FoundationBehavior : MonoBehaviour
{
    [SerializeField] public float gridAccuracy = 0.25f;

    // Convert grid accuracy to meters
    private float cellSize = gridAccuracy * 0.0254f;

    void Start()
    {
        
    }


    public void CalculateGrid(type)
    {
        switch (type)
        {
            case "wall":
                {
                    Vector2Int grid = new Vector2Int();
                }
            
            case "pole":
                {

                }
                
            break;
            
        }       
    }
}
        
    
    