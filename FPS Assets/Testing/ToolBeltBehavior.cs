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

    [SerializeField] private KeyCode grabKey = KeyCode.F;

    private Camera mainCamera;
    private GameObject heldObject = null;
    private Vector3 grabOffset;
    private float grabDistance = 2f;
    private bool handsEquipped = false;

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

            if (heldObject != null)
            {
                HoldObject();
            }
        }
       
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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance) && hit.transform.CompareTag("Grabbable"))
        {
            heldObject = hit.transform.gameObject;
            grabOffset = hit.transform.position - hit.point;
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log($"Grabbed {heldObject.name}");
        }
    }   
    
    private void ReleaseObject()
    {
        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        heldObject = null;
        Debug.Log("Released object");
    }

    private void HoldObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos = ray.origin + ray.direction * grabDistance + grabOffset;
        heldObject.transform.position = targetPos;
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
