using UnityEngine;

public class PlanetaryGravity : MonoBehaviour
{
    public float planetMass = 34269.44f;
    private Rigidbody planetRigidbody;

    void Start()
    {
        planetRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        GameObject[] orbitables = GameObject.FindGameObjectsWithTag("Orbitable");
        foreach (GameObject obj in orbitables)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = transform.position - rb.position;
                float distance = direction.magnitude;
                if (distance < 1f) distance = 1f; // Clamp distance
                direction = direction.normalized; // Normalize after clamping
                float forceMagnitude = PhysicsConstants.GravitationalConstant * (planetMass * rb.mass) / (distance * distance);
                Vector3 force = direction * forceMagnitude;
                rb.AddForce(force);
                Debug.Log($"Applying force to {obj.name}: {forceMagnitude} (Distance: {distance}, Force Vector: {force})");
            }
        }
    }
}