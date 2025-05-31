using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Assign the moon GameObject here
    public Vector3 offset = new Vector3(0, 5, -10); // Adjust offset for desired camera position
    public float smoothSpeed = 0.125f; // Smoothing factor for camera movement

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired camera position
        Vector3 desiredPosition = target.position + offset;
        // Smoothly move the camera to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Look at the target (moon)
        transform.LookAt(target);
    }
}
