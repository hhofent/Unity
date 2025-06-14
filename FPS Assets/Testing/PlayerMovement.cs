using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float mouseSensitivityX = 4f;
    [SerializeField] private float mouseSensitivityY = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float reachHeight = 1.5f; // Renamed from crouchHeight
    [SerializeField] private float reachCamMinHeight = 0.3f; // Renamed from crouchCamMinHeight
    [SerializeField] private float reachCamMaxHeight = 2.1f; // Renamed from crouchCamMaxHeight
    [SerializeField] private float camScrollSpeed = 0.7f;

    [SerializeField] private KeyCode reachKey = KeyCode.LeftControl; // Renamed from crouchKey
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    private CharacterController controller;
    private Transform cam;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float defaultHeight = 2f;
    private Vector3 defaultCenter;
    private Vector3 defaultCamPos;
    private bool wasReaching = false; // Renamed from wasCrouching
    private bool isReaching = false; // Renamed from isCrouching
    private bool isSprinting = false;
    private bool isGrounded = false;
    private float currentCamReachHeight; // Renamed from currentCamCrouchHeight, added missing declaration

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>()?.transform;
        if (cam == null)
        {
            Debug.LogError("Camera not found in child of PlayerMovement");
            enabled = false;
            return;
        }
        defaultHeight = controller.height;
        defaultCenter = controller.center;
        defaultCamPos = cam.localPosition;
        currentCamReachHeight = reachHeight / 2; // Initialize reaching camera height
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isGrounded = Physics.SphereCast(
            transform.position + controller.center,
            controller.radius * 0.9f,
            Vector3.down,
            out _,
            (controller.height / 2) + 0.1f // Fixed from (controller.height / 2) + -0.1f
        );

        Look();
        Move();
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleReaching()
    {
        bool wasReachingLastFrame = wasReaching;
        isReaching = Input.GetKey(reachKey);

        if (isReaching)
        {
            controller.height = reachHeight;
            controller.center = new Vector3(0, reachHeight / 2, 0);
            if (!wasReachingLastFrame)
            {
                transform.position += Vector3.up * (defaultHeight - reachHeight) * 0.5f;
                currentCamReachHeight = reachHeight / 2;
            }
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            currentCamReachHeight += scrollInput * camScrollSpeed;
            currentCamReachHeight = Mathf.Clamp(currentCamReachHeight, reachCamMinHeight, reachCamMaxHeight);
            cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, currentCamReachHeight, Time.deltaTime * 10f), cam.localPosition.z);
        }
        else
        {
            bool canStand = !Physics.Raycast(transform.position, Vector3.up, defaultHeight - controller.height + 0.1f);
            if (canStand)
            {
                controller.height = defaultHeight;
                controller.center = defaultCenter;
                cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, defaultCamPos.y, Time.deltaTime * 10f), cam.localPosition.z);
            }
            else
            {
                isReaching = true;
            }
        }
        wasReaching = isReaching;
    }

    private void HandleJumping()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded && velocity.y <= 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isReaching = false; // Unreach on jump
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        float speed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(direction * speed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Move()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -0.5f;
        }

        isSprinting = Input.GetKey(sprintKey) && isGrounded && !isReaching;

        HandleReaching();
        HandleJumping();
        HandleMovement();
    }
}