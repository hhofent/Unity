using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private GameObject heldObject;
    private Vector3 grabOffset;
    private float grabDistance = 2f;
    public string[] tools = { "None", "Tape Measure", "Saw", "Drill", "Hammer", "Impact Wrench", "Attachment", "Magic Tools" };
    public int currentToolIndex = 0;
    public Text toolText;
    public Text actionText;
    public FoundationBehavior foundation;
    public AttachmentTool attachmentTool;
    public TapeMeasureNotebookTool tapeMeasureTool;
    public MagicTools magicTools;
    public SawTool sawTool;
    public DrillTool drillTool;
    public HammerTool hammerTool;
    public ImpactWrenchTool impactWrenchTool;
    private Vector2Int[] attachmentPoints;
    public GameObject notebookPanel;
    public Text measurementEntryTemplate;
    private int selectedMeasurementIndex = -1;
    public Text contextText;

    // Serialized keybinds
    [SerializeField] private KeyCode resetMarksKey = KeyCode.R;
    [SerializeField] private KeyCode toggleNotebookKey = KeyCode.N;
    [SerializeField] private KeyCode saveMeasurementKey = KeyCode.Return;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateToolText();
        actionText.text = "Latest Action: None";
        notebookPanel.SetActive(false);
        contextText.text = "testes";
    }

    // Updates input for mouse look and object grabbing
    void Update()
    {
        // Switch tools
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            currentToolIndex = (currentToolIndex + 1) % tools.Length;
            UpdateToolText();
        }
        else if (scroll < 0f)
        {
            currentToolIndex = (currentToolIndex - 1 + tools.Length) % tools.Length;
            UpdateToolText();
        }

        // Update context text
        UpdateContextText();

        // Grab object
        if (tools[currentToolIndex] == "None") 
        {
            if (Input.GetMouseButtonDown(0) && heldObject == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f) && hit.transform.CompareTag("Stud"))
                {
                    heldObject = hit.transform.gameObject;
                    grabOffset = hit.transform.position - hit.point;
                    heldObject.GetComponent<Rigidbody>().isKinematic = true;
                    actionText.text = "Latest Action: Grabbed stud";
                }
            }

            // Release object
            else if (Input.GetMouseButtonUp(0) && heldObject != null)
            {
                heldObject.GetComponent<Rigidbody>().isKinematic = false;
                heldObject = null;
                actionText.text = "Latest Action: Released stud";
            }

            // Update object position
            if (heldObject != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPos = ray.origin + ray.direction * grabDistance + grabOffset;
                heldObject.transform.position = targetPos;
                float rotationSpeed = 10f;

                if (Input.GetKey(rotateLeftKey))
                {
                    heldObject.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
                }
                if (Input.GetKey(rotateRightKey))
                {
                    heldObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                }

                float wheel = Input.GetAxis("Mouse ScrollWheel");
                grabDistance += wheel * 0.1f;
                grabDistance = Mathf.Clamp(grabDistance, 1f, 5f);
            }

            // Select attachment points
            if (Input.GetMouseButtonDown(1) && heldObject != null && heldObject.CompareTag("Stud"))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 2.54f) && hit.transform == foundation.transform)
                {
                    Vector3 snapPoint = foundation.GetNearestGridPoint(hit.point);
                    Vector3 localPos = foundation.transform.InverseTransformPoint(snapPoint);
                    int x = Mathf.RoundToInt(localPos.x / 0.00635f + (foundation.gridWidth - 1) / 2f);
                    int y = Mathf.RoundToInt(localPos.z / 0.00635f + (foundation.gridHeight - 1) / 2f);
                    Vector2Int gridPoint = new Vector2Int(x, y);
                    
                    if (attachmentPoints == null) attachmentPoints = new Vector2Int[2];
                    if (attachmentPoints[0] == default) attachmentPoints[0] = gridPoint;
                    else if (attachmentPoints[1] == default && attachmentPoints[0] != gridPoint)
                    {
                        attachmentPoints[1] = gridPoint;
                        actionText.text = "Latest Action: Selected two points";
                    }
                }
            }
        }

        else if (tools[currentToolIndex] == "Tape Measure")
        {
            tapeMeasureTool.UseTool(out string action, resetMarksKey);
            actionText.text = $"Latest Action: {action}";
        }

        else if (tools[currentToolIndex] == "Magic Tools")
        {
            magicTools.UseTool(out string action);
            actionText.text = $"Latest Action: {action}";
        }

        else if (tools[currentToolIndex] == "Saw")
        {
            sawTool.CutStud(out string action);
            actionText.text = $"Latest Action: {action}";
        }

        else if (tools[currentToolIndex] == "Drill")
        {
            drillTool.DrillHole(out string action);
            actionText.text = $"Latest Action: {action}";
        }

        else if (tools[currentToolIndex] == "Hammer")
        {
            hammerTool.HammerBolt(out string action);
            actionText.text = $"Latest Action: {action}";
        }

        else if (tools[currentToolIndex] == "Impact Wrench")
        {
            impactWrenchTool.TightenBolt(out string action);
            actionText.text = $"Latest Action: {action}";
        }

        else if (tools[currentToolIndex] == "Attachment")
        {
            if (heldObject != null && attachmentPoints != null && attachmentPoints[1] != default)
            {
                if (attachmentTool.CanAttach(heldObject, foundation.transform, out string canAttachAction))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (attachmentTool.TryAttach(heldObject, attachmentPoints, out string attachAction))
                        {
                            actionText.text = $"Latest Action: {attachAction}";
                            heldObject = null;
                            attachmentPoints = null;
                        }
                        else
                        {
                            actionText.text = $"Latest Action: {attachAction}";
                        }
                    }
                }
                else 
                {
                    actionText.text = $"Latest Action: {canAttachAction}";
                }
            }
        }

        


        // toggle notebook ui
        if (Input.GetKeyDown(toggleNotebookKey) && (tools[currentToolIndex] == "Tape Measure" || tools[currentToolIndex] == "Saw"))
        {
            notebookPanel.SetActive(!notebookPanel.activeSelf);
            if (notebookPanel.activeSelf) UpdateNotebookUI();
        }

        // save measurement
        if (tools[currentToolIndex] == "Tape Measure" && Input.GetKeyDown(saveMeasurementKey))
        {
            tapeMeasureTool.SaveMeasurement(out string action);
            actionText.text = $"Latest Action: {action}";
        }

        // select measurement
        if (notebookPanel.activeSelf)
        {
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    var measurements = tapeMeasureTool.GetMeasurements();
                    if (i < measurements.Count)
                    {
                        selectedMeasurementIndex = i;
                        actionText.text = $"Latest Action: Selected location {measurements[i].locationNumber}: {measurements[i].length:F2} inches";
                    }
                }
            }
        }
    }

    // Update context text for clarity
    private void UpdateContextText()
    {
        string currentContextAction = "";
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitValid = Physics.Raycast(ray, out RaycastHit hit, 2.54f);

        if (tools[currentToolIndex] == "None")
        {
            if (hitValid && hit.transform.CompareTag("Stud"))
                currentContextAction = "Click to grab stud";
            else if (hitValid && hit.transform == foundation.transform && heldObject != null && heldObject.CompareTag("Stud"))
                currentContextAction = attachmentPoints == null || attachmentPoints[0] == default ? 
                    "Right-click to set first point" : "Right-click to set second point";
        }
        else if (tools[currentToolIndex] == "Tape Measure")
        {
            if (hitValid && hit.transform == foundation.transform)
                currentContextAction = tapeMeasureTool.GetCurrentMarks().Count == 0 ? 
                    "Click to set measuring start point" : "Click to set measuring end point";
            else if (hitValid && hit.transform.CompareTag("Stud"))
                {
                    float length = hit.transform.GetComponent<StudBehavior>().Length * 39.3701f;
                    currentContextAction = $"Stud length: {length:F2} inches";
                }
        }
        else if (tools[currentToolIndex] == "Attachment")
        {
            if (heldObject != null && attachmentPoints != null && attachmentPoints[1] != default)
                currentContextAction = "Click to attach stud";
        }

        contextText.text = currentContextAction;
    }
    
    // update notebook ui
    void UpdateNotebookUI()
    {
        foreach (Transform child in notebookPanel.transform)
        {
            if (child.gameObject != measurementEntryTemplate.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        var measurements = tapeMeasureTool.GetMeasurements();
        for (int i = 0; i < measurements.Count; i++)
        {
            Text entry = Instantiate(measurementEntryTemplate, notebookPanel.transform);
            entry.gameObject.SetActive(true);
            float lengthInInches = measurements[i].length * 39.3701f;
            entry.text = $"Location {measurements[i].locationNumber}: {lengthInInches:F2} inches";
        }
    }

    private void UpdateToolText()
    {
        toolText.text = $"Equipped: {tools[currentToolIndex]}";
    }
}
