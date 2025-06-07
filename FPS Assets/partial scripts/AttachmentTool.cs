using UnityEngine;
using System.Collections.Generic;

public class AttachmentTool : MonoBehaviour
{
    public FoundationBehavior foundation;
    private Dictionary<(string objectTag, string surfaceTag), bool> attachmentRules;

    // Initialize attachment rules
    void Start()
    {
        attachmentRules = new Dictionary<(string, string), bool>
        {
            { ("Stud", "Foundation"), true },
            { ("Stud", "Stud"), true },
            { ("Plywood", "Foundation"), false },
            { ("Plywood", "Stud"), true },
            { ("Plywood", "Plywood"), true },
        };
    }

    // Checks if object can attach to surface
    public bool CanAttach(GameObject obj, Transform surface, out string action)
    {
        action = "";
        string objTag = obj.tag;
        string surfaceTag = surface.tag;
        if (attachmentRules.TryGetValue((objTag, surfaceTag), out bool allowed))
        {
            action = allowed ? $"Can attach {objTag} to {surfaceTag}" : $"Cannot attach {objTag} to {surfaceTag}";
            return allowed;
        }
        action = $"No rule for {objTag} on {surfaceTag}";
        return false;
    }

    // Attempts to attach object at grid points
    public bool TryAttach(GameObject obj, Vector2Int[] corners, out string action)
    {
        action = "";
        if (corners.Length != 2)
        {
            action = "Invalid corners";
            return false;
        }

        foreach (var corner in corners)
        {
            if (!foundation.CanPlaceObject(corner.x, corner.y))
            {
                action = $"Grid point ({corner.x}, {corner.y}) occupied";
                return false;
            }
        }

        Vector3 pos1 = foundation.gridWorldPositions[corners[0]];
        Vector3 pos2 = foundation.gridWorldPositions[corners[1]];
        Vector3 direction = (pos2 - pos1).normalized;
        Vector3 midPoint = (pos1 + pos2) / 2f + Vector3.up * 0.0762f;
        float length = Vector3.Distance(pos1, pos2);
        if (length < obj.transform.localScale.x)
        {
            action = $"Stud too short ({length * 39.3701f:F2} inches > {obj.transform.localScale.x * 39.3701:F2} inches)";
            return false;
        }

        obj.transform.position = midPoint;
        obj.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        obj.transform.SetParent(foundation.transform, true);

        foreach (var corner in corners)
        {
            foundation.occupancyGrid[corner.x, corner.y] = true;
        }

        action = $"Attached {obj.tag} at ({corners[0]}, {corners[1]})";
        return true;
    }       
}
