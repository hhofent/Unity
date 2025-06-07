using UnityEngine;

public class PreviewManager
{
    public enum PreviewType { GameObject, Line}
    private GameObject activePreview;
    private LineRenderer lineRenderer;
    private Transform targetObject;
    private IGridObject gridObject;
    private int lockedAxisIndex;
    private readonly string[] axisNames = { "X", "Y", "Z" };

    public PreviewManager(int initialAxisIndex = 2) // Default to Z snapped
    {
        lockedAxisIndex = initialAxisIndex;
        Debug.Log($"Initial snapped axis: {axisNames[lockedAxisIndex]}");
    }

    public IGridObject GetGridObject()
    {
        return gridObject;
    }

    public int GetLockedAxisIndex()
    {
        return lockedAxisIndex;
    }

    public void UpdateAxis(int delta)
    {
        lockedAxisIndex = (lockedAxisIndex + delta + 3) % 3;
        Debug.Log($"Snapping on axis: {axisNames[lockedAxisIndex]}");
        UpdatePreviewRotation();
    }

    public string GetLockedAxisName()
    {
        return axisNames[lockedAxisIndex];
    }

    public Vector3? GetPreviewPosition()
    {
        return activePreview != null ? activePreview.transform.position : (Vector3?)null;
    }

    public void CreatePreview(GameObject previewPrefab, RaycastHit hit, IGridObject gridObj, PreviewType type)
    {
        if (gridObj == null) return;
        gridObject = gridObj;
        targetObject = hit.transform;

        if (type == PreviewType.GameObject)
        {
            DestroyPreview();
            activePreview = Object.Instantiate(previewPrefab, hit.point, gridObj.Transform.rotation);
            UpdatePreviewRotation();
            activePreview.SetActive(true);
        }
        else if (type == PreviewType.Line)
        {
            if (activePreview == null)
            {
                activePreview = new GameObject("LinePreview");
                lineRenderer = activePreview.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;
                lineRenderer.material = new Material(Shader.Find("Standard"));
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                
            }
        }

    }

    public void UpdatePreview(RaycastHit hit, Camera mainCamera)
    {
        if (gridObject == null || activePreview == null || hit.transform != targetObject) return;

        Vector3 snappedPosition = SnapToGrid(hit.point, gridObject.Transform, gridObject.CellSize, lockedAxisIndex);
        if (lineRenderer != null)
        {
            Vector3 direction = gridObject.Transform.TransformDirection(lockedAxisIndex == 0 ? Vector3.right : lockedAxisIndex == 1 ? Vector3.up : Vector3.forward);
            lineRenderer.SetPositions(new[] { snappedPosition - direction * 0.5f, snappedPosition + direction * 0.5f });
        }
        else
        {
            activePreview.transform.position = snappedPosition;
            UpdatePreviewRotation();
        }
    }
        
    public void HidePreview()
    {
        if (activePreview != null)
        {
            activePreview.SetActive(false);
        }
    }

    public void DestroyPreview()
    {
        if (activePreview != null)
        {
            Object.Destroy(activePreview);
            activePreview = null;
            lineRenderer = null;
            targetObject = null;
            gridObject = null;
        }
    }

    private void UpdatePreviewRotation()
    {
        if (activePreview == null || gridObject == null) return;

        // Set rotation based on the snapped axis
        if (lockedAxisIndex == 2)  // Z axis snapped: Default rotation
        {
            activePreview.transform.rotation = gridObject.Transform.rotation;
        }
        else if (lockedAxisIndex == 0)  // X axis snapped, Y and Z locked
        {
            Vector3 normal = gridObject.Transform.TransformDirection(Vector3.right); // X-axis normal
            Vector3 inPlaneAxis = gridObject.Transform.TransformDirection(Vector3.up); // Y-axis in plane
            activePreview.transform.rotation = Quaternion.LookRotation(inPlaneAxis, normal);
        }
        else  // Y axis snapped, X and Z locked
        {
            Vector3 normal = gridObject.Transform.TransformDirection(Vector3.up); // Y-axis normal
            Vector3 inPlaneAxis = gridObject.Transform.TransformDirection(Vector3.right); // X-axis in plane
            activePreview.transform.rotation = Quaternion.LookRotation(inPlaneAxis, gridObject.Transform.TransformDirection(Vector3.forward)); // Z-axis up
        }
    }

    private Vector3 SnapToGrid(Vector3 hitPoint, Transform transform, float cellSize, int axisIndex)
    {
        if (cellSize <= 0) return hitPoint;
        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
        Vector3 snappedLocalPoint = localHitPoint;

        if (axisIndex == 0) // X axis snapping, Y and Z locked
        {
            snappedLocalPoint.x = Mathf.Round(localHitPoint.x / cellSize) * cellSize;
        }
        else if (axisIndex == 1) // Y axis snapping, X and Z locked
        {
            snappedLocalPoint.y = Mathf.Round(localHitPoint.y / cellSize) * cellSize;
        }
        else // Z axis snapping, X and Y locked
        {
            snappedLocalPoint.z = Mathf.Round(localHitPoint.z / cellSize) * cellSize;
        }
        return transform.TransformPoint(snappedLocalPoint);
    }
}
