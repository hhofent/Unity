using UnityEngine;

public class SawBehavior : MonoBehaviour
{
    public TapeMeasureNotebookTool tapeMeasureTool;

    public void CutStud(out string action)
    {
        action = "";
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 2.54f) && hit.transform.CompareTag("Stud"))
        {
            StudBehavior stud = hit.transform.GetComponent<StudBehavior>();
            Vector3 localHit = hit.transform.InverseTransformPoint(hit.point);
            float cellSize = 0.00635f;
            float snapX = Mathf.Round(localHit.x / cellSize) * cellSize;
            snapX = Mathf.Clamp(snapX, -stud.Length / 2, stud.Length / 2);

            var studMarks = tapeMeasureTool.GetStudMarks();
            if (studMarks.ContainsKey(hit.transform.gameObject) && studMarks[hit.transform.gameObject].Contains(snapX))
            {
                GameObject[] newStuds = stud.SplitAtMark(snapX);
                action = $"Cut stud at {snapX * 39.3701f:F2} inches from center";
                studMarks.Remove(hit.transform.gameObject);
                foreach (var newStud in newStuds)
                    studMarks[newStud] = new List<float>();
            }
            else
            {
                action = "No mark at this position";
            }
        }
    }
}