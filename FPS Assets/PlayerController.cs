using UnityEngine;
using TMPro;

// Controls player movement, tool switching, and UI updates.
public class PlayerController : MonoBehaviour
{
    // Player’s Rigidbody for physics-based movement.
    private Rigidbody rb;
    // Main camera transform for mouse look.
    private Transform cameraTransform;
    // Movement speed in units per second.
    public float moveSpeed = 5f;
    // Mouse sensitivity for camera rotation.
    public float mouseSensitivity = 100f;
    // Vertical rotation angle for camera to limit up/down look.
    private float xRotation = 0f;
    // Reference to the foundation for grid access.
    public FoundationBehavior foundation;
    // Currently held stud (if any) for grabbing.
    private GameObject heldObject;
    // Offset for grabbing objects to maintain distance.
    private Vector3 grabOffset;
    // Distance at which held objects are positioned.
    private float grabDistance = 2f;
    // List of available tools (initially empty hand only).
    private string[] tools = { "None" };
    // Index of the currently selected tool.
    private int currentToolIndex = 0;
    // UI text for displaying the equipped tool.
    public TextMeshProUGUI toolText;
    // UI text for displaying the latest action.
    public TextMeshProUGUI actionText;

    // Initializes the player, UI, and locks the cursor.
    void Start()
    {
        // Get the Rigidbody for movement.
        rb = GetComponent<Rigidbody>();
        // Get the main camera for mouse look.
        cameraTransform = GetComponentInChildren<Camera>().transform;
        // Lock the cursor to the game window for first-person control.
        Cursor.lockState = CursorLockMode.Locked;
        // Initialize UI text with default values.
        UpdateToolText();
        actionText.text = "Latest Action: None";
    }

    // Updates player input and movement.
    void Update()
    {
        // Handle mouse look for camera rotation.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Switch tools using scroll wheel (placeholder for future tools).
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

        // Empty hand: Grab and manipulate studs.
        if (tools[currentToolIndex] == "None")
        {
            // Grab a stud on left-click.
            if (Input.GetMouseButtonDown(0) && heldObject == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    if (hit.transform.CompareTag("Stud"))
                    {
                        heldObject = hit.transform.gameObject;
                        grabOffset = hit.transform.position - hit.point;
                        heldObject.GetComponent<Rigidbody>().isKinematic = true;
                        actionText.text = "Latest Action: Grabbed stud";
                    }
                }
            }
            // Release the stud on left-click release.
            else if (Input.GetMouseButtonUp(0) && heldObject != null)
            {
                heldObject.GetComponent<Rigidbody>().isKinematic = false;
                actionText.text = "Latest Action: Released stud";
                heldObject = null;
            }

            // Update held stud position.
            if (heldObject != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPos = ray.origin + ray.direction * grabDistance + grabOffset;
                heldObject.transform.position = targetPos;
            }
        }
    }

    // Updates player movement using physics.
    void FixedUpdate()
    {
        // Apply movement based on input, preserving vertical velocity.
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized * moveSpeed;
        move.y = rb.velocity.y;
        rb.velocity = move;
    }

    // Updates the UI text to show the currently equipped tool.
    private void UpdateToolText()
    {
        toolText.text = $"Equipped: {tools[currentToolIndex]}";
    }
}