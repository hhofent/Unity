using UnityEngine;

public class OrbitalVelocity : MonoBehaviour
{
    public Transform planet; // Assign the GasGiant GameObject
    public float planetMass = 34269.44f;
    public float speedMultiplier = 1f; // Reduced for circular orbit
    private Vector3 startPosition;
    private bool orbitCompleted = false;
    private float startTime;

    void Start()
    {
        Time.timeScale = 20f; // Speed up simulation for testing
        Time.fixedDeltaTime = 0.005f;
        if (planet == null)
        {
            Debug.LogError("Planet not assigned in OrbitalVelocity!");
            return;
        }

        startPosition = transform.position;
        startTime = Time.time;
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 directionToPlanet = (planet.position - transform.position).normalized;
        float distance = Vector3.Distance(planet.position, transform.position);
        if (distance < 1f)
        {
            Debug.LogWarning("Moon too close to planet, adjusting position.");
            transform.position = planet.position + new Vector3(50f, 0, 0);
            directionToPlanet = (planet.position - transform.position).normalized;
            distance = 50f;
        }

        float orbitalSpeed = Mathf.Sqrt(PhysicsConstants.GravitationalConstant * planetMass / distance) * speedMultiplier;
        Vector3 tangentialVelocity = Vector3.Cross(directionToPlanet, Vector3.up).normalized * orbitalSpeed;
        rb.linearVelocity = tangentialVelocity; // Changed to linearVelocity
        float orbitalPeriod = 2 * Mathf.PI * distance / orbitalSpeed;
        Debug.Log($"Moon Initial Position: {transform.position}, Velocity: {rb.linearVelocity} (Magnitude: {rb.linearVelocity.magnitude}), Distance: {distance}, Orbital Period: {orbitalPeriod:F2} seconds");
    }

    void Update()
    {
        float distance = Vector3.Distance(planet.position, transform.position);
        float orbitalSpeed = GetComponent<Rigidbody>().linearVelocity.magnitude;
        float orbitalPeriod = 2 * Mathf.PI * distance / orbitalSpeed;
        Debug.Log($"Moon Position: {transform.position}, Velocity: {orbitalSpeed}, Distance: {distance}, Orbital Period: {orbitalPeriod:F2} seconds");

        if (!orbitCompleted && Vector3.Distance(transform.position, startPosition) < 1f && Time.time > startTime + 1f)
        {
            float orbitTime = Time.time - startTime;
            Debug.Log($"Orbit completed! Time: {orbitTime:F2} seconds, Position: {transform.position}");
            orbitCompleted = true;
            Time.timeScale = 1f; // Reset timescale after orbit
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f; // Reset timescale when object is destroyed
        Time.fixedDeltaTime = 0.02f;
    }
}