using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Transform cameraTransform;
    private float moveSpeed = 0.127f; // 5 inches/sec
    private float mouseSensitivity = 100f;
    private float xRotation = 0f;
    private GameObject heldObject;
    private Vector3 grabOffset;
    private float grabDistance = 0.0508f; // 2 inches
    private bool isGrabbing = false;
    private string[] tools = { "None", "Tape Measure", "Saw", "Attachment" };
    private int currentToolIndex = 0;
    public Text toolText;
    public Text actionText;
    public TextMeshProUGUI contextText;
    public FoundationBehavior foundation;
    public AttachmentTool attachmentTool;
    public TapeMeasureNotebookTool tapeMeasureTool;
    public SawBehavior sawTool;
    public DrillTool drillTool;
    public HammerTool hammerTool;
    public ImpactWrenchTool impactWrenchTool;
    private Vector2Int[] attachmentPoints;
    public GameObject notebookPanel;
    public Text measurementEntryTemplate;
    private int selectedMeasurementIndex = -1;
    private float defaultHeight = 0.0508f;
    private float crouchHeight = 0.0254f;
    private bool isCrouching = false;

    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode resetMarksKey = KeyCode.R;
    [SerializeField] private KeyCode toggleNotebookKey = KeyCode.N;
    [SerializeField] private KeyCode saveMeasurementKey = KeyCode.Return;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
        UpdateToolText();
        actionText.text = "Latest Action: None";
        contextText.text = "";
        notebookPanel.SetActive(false);
        defaultHeight = GetComponent<CapsuleCollider>().height;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f && !isCrouching)
        {
            currentToolIndex = (currentToolIndex + 1) % tools.Length;
            UpdateToolText();
        }
        else if (scroll < 0f && !isCrouching)
        {
            currentToolIndex = (currentToolIndex - 1 + tools.Length) % tools.Length;
            UpdateToolText();
        }

        UpdateContextText();

        if (tools[currentToolIndex] == "None")
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!isGrabbing && heldObject == null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 0.127f) && hit.transform.CompareTag("Stud"))
                    {
                        heldObject = hit.transform.gameObject;
                        grabOffset = hit.transform.position - hit.point;
                        heldObject.GetComponent<Rigidbody>().isKinematic = true;
                        isGrabbing = true;
                        actionText.text = "Latest Action: Grabbed stud";
                    }
                }
                else if (isGrabbing && heldObject != null)
                {
                    heldObject.GetComponent<Rigidbody>().isKinematic = false;
                    heldObject = null;
                    isGrabbing = false;
                    actionText.text = "Latest Action: Released stud";
                }
            }

            if (heldObject != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPos = ray.origin + ray.direction * grabDistance + grabOffset;
                heldObject.transform.position = targetPos;

                if (Input.GetKey(rotateLeftKey))
                    heldObject.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
                if (Input.GetKey(rotateRightKey))
                    heldObject.transform.Rotate(Vector3.up, -90f * Time.deltaTime);

                if (isCrouching)
                {
                    float scrollAdjust = Input.GetAxis("Mouse ScrollWheel");
                    if (scrollAdjust != 0)
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        crouchHeight += scrollAdjust * 0.01f;
                        crouchHeight = Mathf.Clamp(crouchHeight, 0.0127f, defaultHeight);
                        collider.height = crouchHeight;
                        collider.center = new Vector3(0, crouchHeight / 2, 0);
                        cameraTransform.localPosition = new Vector3(0, crouchHeight / 2, 0);
                    }
                }
                else
                {
                    float scrollAdjust = Input.GetAxis("Mouse ScrollWheel");
                    grabDistance += scrollAdjust * 0.1f;
                    grabDistance = Mathf.Clamp(grabDistance, 0.0254f, 0.254f);
                }
            }

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
                    if (attachmentPoints[0] == default)
                    {
                        attachmentPoints[0] = gridPoint;
                        actionText.text = "Latest Action: Selected first point";
                    }
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

        if (Input.GetKeyDown(toggleNotebookKey) && (tools[currentToolIndex] == "Tape Measure" || tools[currentToolIndex] == "Saw"))
        {
            notebookPanel.SetActive(!notebookPanel.activeSelf);
            if (notebookPanel.activeSelf) UpdateNotebookUI();
        }

        if (tools[currentToolIndex] == "Tape Measure" && Input.GetKeyDown(saveMeasurementKey))
        {
            tapeMeasureTool.SaveMeasurement(out string action);
            actionText.text = $"Latest Action: {action}";
        }

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
                        actionText.text = $"Latest Action: Selected location {measurements[i].locationNumber}: {measurements[i].length * 39.3701f:F2} inches";
                    }
                }
            }
        }

        if (Input.GetKeyDown(crouchKey))
        {
            isCrouching = true;
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            collider.height = crouchHeight;
            collider.center = new Vector3(0, crouchHeight / 2, 0);
            cameraTransform.localPosition = new Vector3(0, crouchHeight / 2, 0);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            isCrouching = false;
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            collider.height = defaultHeight;
            collider.center = new Vector3(0, defaultHeight / 2, 0);
            cameraTransform.localPosition = new Vector3(0, defaultHeight / 2, 0);
        }
    }

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
                currentContextAction = $"Stud Length: {length:F2} inches";
            }
        }
        else if (tools[currentToolIndex] == "Attachment")
        {
            if (heldObject != null && attachmentPoints != null && attachmentPoints[1] != default)
                currentContextAction = "Click to attach stud";
        }

        contextText.text = currentContextAction;
    }

    void UpdateNotebookUI()
    {
        foreach (Transform child in notebookPanel.transform)
        {
            if (child.gameObject != measurementEntryTemplate.gameObject)
                Destroy(child.gameObject);
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

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized * moveSpeed;
        move.y = rb.linearVelocity.y;
        rb.linearVelocity = move;
    }

    private void UpdateToolText()
    {
        toolText.text = $"Equipped: {tools[currentToolIndex]}";
    }
}