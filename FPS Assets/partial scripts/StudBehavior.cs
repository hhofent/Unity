using UnityEngine;
using System.Collections.Generic;

public class StudBehavior : MonoBehaviour
{
    public float Length => transform.localScale.x;
    private Bounds bounds;
    private List<GameObject> marks = new List<GameObject>();

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (!rend) { Debug.LogError("Stud missing Renderer!", gameObject); return; }
        bounds = rend.bounds;
    }

    public Vector3 GetTopSurfacePosition()
    {
        return transform.position + transform.up * (bounds.size.y / 2f);
    }

    public void AddMark(GameObject mark)
    {
        marks.Add(mark);
    }

    public void ClearMarks()
    {
        foreach (var mark in marks) Destroy(mark);
        marks.Clear();
    }

    public List<float> GetMarkPositions()
    {
        List<float> positions = new List<float>();
        foreach (var mark in marks)
        {
            float localX = transform.InverseTransformPoint(mark.transform.position).x;
            positions.Add(localX);
        }
        return positions;
    }

    public GameObject[] SplitAtMark(float localX)
    {
        float length = Length;
        if (Mathf.Abs(localX) >= length / 2) return new[] { gameObject };

        float leftLength = (length / 2 + localX); // From -length/2 to localX
        float rightLength = (length / 2 - localX); // From localX to length/2

        GameObject leftStud = Instantiate(gameObject, transform.parent);
        leftStud.transform.localScale = new Vector3(leftLength, transform.localScale.y, transform.localScale.z);
        leftStud.transform.position = transform.TransformPoint(new Vector3(-length / 4 - localX / 2, 0, 0));
        leftStud.GetComponent<StudBehavior>().ClearMarks();

        GameObject rightStud = Instantiate(gameObject, transform.parent);
        rightStud.transform.localScale = new Vector3(rightLength, transform.localScale.y, transform.localScale.z);
        rightStud.transform.position = transform.TransformPoint(new Vector3(length / 4 - localX / 2, 0, 0));
        rightStud.GetComponent<StudBehavior>().ClearMarks();

        Destroy(gameObject);
        return new[] { leftStud, rightStud };
    }
}