using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Platform : MonoBehaviour
{
    public float returnForce = 10;
    Vector3 startPosition;
    Vector3 phase;
    Rigidbody body;

    void Start()
    {
        startPosition = transform.position;
        phase = Random.onUnitSphere;
        body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 target = startPosition;
        target = target
            + Vector3.up   * 0.3f * Mathf.Sin(phase.x + Time.time / 1.37f)
            + Vector3.left * 0.3f * Mathf.Sin(phase.y + Time.time / 1.77f)
            + Vector3.forward * 0.3f * Mathf.Sin(phase.z + Time.time / 1.91f);

        body.AddForce((target - transform.position) * returnForce);
    }
}
