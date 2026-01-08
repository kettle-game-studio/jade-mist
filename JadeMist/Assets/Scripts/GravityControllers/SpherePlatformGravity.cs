using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SpherePlatformGravity : BasePlatformGravity
{
    SphereCollider sphereCollider;

    Vector3 Center => transform.TransformPoint(sphereCollider.center);
    Vector3 GlobalBaseGravity => transform.rotation * localBaseGravity;
    float Radius => sphereCollider.radius * Mathf.Max(
        Mathf.Abs(sphereCollider.transform.lossyScale.x),
        Mathf.Abs(sphereCollider.transform.lossyScale.y),
        Mathf.Abs(sphereCollider.transform.lossyScale.z)
    );
    protected override float PointIntensity(Vector3 point) => 1 - Mathf.Clamp(
        (Vector3.Distance(Center, point) - Radius * internalRadius) /
        (Radius * (1 - internalRadius)),
        0, 1
    );

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    void OnDrawGizmos()
    {
        // TODO: InitializeOnLoad? Startup?
        sphereCollider = GetComponent<SphereCollider>();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Center, Radius * internalRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Center, Radius);

        Gizmos.color = Color.red;
        Vector3 direction = Vector3.Cross(new Vector3(1, 1, 1), GlobalBaseGravity).normalized;
        for (float i = 0; i < 1; i += 0.03f)
        {
            Vector3 point = transform.position + direction * Radius * i;
            Gizmos.DrawLine(point, point + Gravity(point) / GlobalBaseGravity.magnitude);
        }
    }
}


