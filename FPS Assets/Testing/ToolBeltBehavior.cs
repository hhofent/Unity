using UnityEngine;
using UnityEngine.UI;

public class ToolBeltBehavior : MonoBehaviour
{
    [SerializeField] private KeyCode tool1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode tool2Key = KeyCode.Alpha2;
    [SerializeField] private KeyCode tool3Key = KeyCode.Alpha3;

    private int toolIndex = 0;
    public Text toolText;
    public Text contextText;

    [SerializeField] private KeyCode rotationModifierKey = KeyCode.R;
    [SerializeField] private KeyCode grabKey = KeyCode.F;
    [SerializeField] private KeyCode grabDistancePlusKey = KeyCode.E;
    [SerializeField] private KeyCode grabDistanceMinusKey = KeyCode.Q;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private float minGrabDistance = 1f;
    [SerializeField] private float maxGrabDistance = 10f;
    [SerializeField] private float grabSpeed = 20f;
    [SerializeField] private float grabDistanceSpeed = 3f;
    [SerializeField] private float grabRotationSpeed = 10f;

    private Camera mainCamera;
    private Collider heldObjectCollider;
    private GameObject heldObject = null;
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;
    private Vector3 rotationAxis;
    private bool handsEquipped = false;
    private float currentRotationAngle;
    private bool isRotationAxisSet = false;

    void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(tool1Key)) 
        {
            EquipHands();
        }

        if (handsEquipped)
        {
            // Grabbing
            if (Input.GetKeyDown(grabKey))
            {
                if (heldObject == null)
                {
                    GrabObject();
                }
                else
                {
                    ReleaseObject();
                }
            }

            // Holding
            if (heldObject != null)
            {
                // Rotation
                bool isRotating = Input.GetKey(rotationModifierKey);
                if (Input.GetKeyDown(rotationModifierKey))
                {
                    SetRotationAxisFromObjectBelow();
                }
                if (Input.GetKeyDown(grabDistancePlusKey))
                {
                    if (isRotating && isRotationAxisSet)
                    {
                        RotateObject(true);
                    }
                    else
                    {
                        grabDistance = Mathf.Clamp(grabDistance + grabDistanceSpeed, minGrabDistance, maxGrabDistance);
                    }
                }
                else if (Input.GetKeyDown(grabDistanceMinusKey))
                {
                    if (isRotating && isRotationAxisSet)
                    {
                        RotateObject(false);
                    }
                    else
                    {
                        grabDistance = Mathf.Clamp(grabDistance - grabDistanceSpeed, minGrabDistance, maxGrabDistance);
                    }
                }
                HoldObject();
            }
        }
       
        // Tool selection
        else if (Input.GetKeyDown(tool2Key)) EquipMagicTools();
        else if (Input.GetKeyDown(tool3Key)) EquipTapeMeasure();
    }

    
    
    
    private void EquipHands()
    {
        handsEquipped = true;
        toolText.text = "Equipped: Hands";
    }

    private void GrabObject()
    {
        // Raycast from camera to mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance) && hit.transform.CompareTag("Grabbable"))
        {
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            heldObjectCollider = hit.transform.GetComponent<Collider>();
            if (rb != null && heldObjectCollider != null)
            {
                heldObject = hit.transform.gameObject;
                grabOffset = hit.transform.position - hit.point;
                Quaternion cameraRotation = Quaternion.LookRotation(mainCamera.transform.forward);
                grabRotationOffset = Quaternion.Inverse(cameraRotation) * heldObject.transform.rotation;
                currentRotationAngle = 0f;
                rotationAxis = Vector3.zero;
                isRotationAxisSet = false;
                rb.isKinematic = true;
                Physics.IgnoreCollision(playerCollider, heldObjectCollider, true);
                Debug.Log($"Grabbed {heldObject.name}");

            }
        }
    }   
    
    private void HoldObject()
    {
        // Raycast from camera to mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos = mainCamera.transform.position + ray.direction * grabDistance + grabOffset;
        heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, targetPos, Time.deltaTime * grabSpeed);
        
        Quaternion angleRotation = Quaternion.AngleAxis(currentRotationAngle, rotationAxis);
        Quaternion targetRotation = isRotationAxisSet ? angleRotation : grabRotationOffset;
        heldObject.transform.rotation = Quaternion.Slerp(
            heldObject.transform.rotation,
            targetRotation,
            Time.deltaTime * grabRotationSpeed
        );
    }

    private void ReleaseObject()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            Physics.IgnoreCollision(playerCollider, heldObjectCollider, false);
            heldObjectCollider = null;
            heldObject = null;
            currentRotationAngle = 0f;
            rotationAxis = Vector3.zero;
            isRotationAxisSet = false;
            Debug.Log("Released object");
        }
    }

    private void RotateObject(bool rotateUp)
    {
        currentRotationAngle += rotateUp ? 90f : -90f;
        currentRotationAngle = Mathf.Repeat(currentRotationAngle, 360f);
        Debug.Log($"Rotated stud to angle: {currentRotationAngle} around axis: {rotationAxis}");
    }

    private void SetRotationAxisFromObjectBelow()
    {
        if (heldObject == null) return;

        Ray ray = new Ray(heldObject.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Transform hitTransform = hit.transform;

            heldObject.transform.rotation = hitTransform.rotation;
            rotationAxis = hitTransform.right;
            Quaternion cameraRotation = Quaternion.LookRotation(mainCamera.transform.forward);
            grabRotationOffset = Quaternion.Inverse(cameraRotation) * heldObject.transform.rotation;
            currentRotationAngle = 0f;
            isRotationAxisSet = true;
            Debug.Log($"Aligned stud to {hitTransform.name}'s rotation: {hitTransform.rotation.eulerAngles}, axis: {rotationAxis}");
        }
        else
        {
            // Fallback: Align with player rotation
            heldObject.transform.rotation = transform.rotation;
            rotationAxis = transform.right;
            Quaternion cameraRotation = Quaternion.LookRotation(mainCamera.transform.forward);
            grabRotationOffset = Quaternion.Inverse(cameraRotation) * heldObject.transform.rotation;
            isRotationAxisSet = true;
            currentRotationAngle = 0f;
            Debug.Log("No object below, using player rotation");
        }
    }

    private void EquipMagicTools()
    {
        toolIndex = 1;
        toolText.text = "Equipped: Magic Tools";
    }

    private void EquipTapeMeasure()
    {
        toolIndex = 2;
        toolText.text = "Equipped: Tape Measure";
    }
}
