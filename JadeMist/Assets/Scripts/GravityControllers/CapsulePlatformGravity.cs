using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CapsulePlatformGravity : BasePlatformGravity
{
    CapsuleCollider capsuleCollider;

    struct GlobalParameters
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public float Radius;
    }

    GlobalParameters GetGlobalParameters()
    {
            var scale = capsuleCollider.transform.lossyScale;
            var radius = capsuleCollider.radius;
            var height = capsuleCollider.height;
            if (capsuleCollider.direction == 0) radius *= Mathf.Max(scale.y, scale.z);
            if (capsuleCollider.direction == 1) radius *= Mathf.Max(scale.x, scale.z);
            if (capsuleCollider.direction == 2) radius *= Mathf.Max(scale.x, scale.y);
            if (capsuleCollider.direction == 0) height *= scale.x;
            if (capsuleCollider.direction == 1) height *= scale.y;
            if (capsuleCollider.direction == 2) height *= scale.z;
            height = height - 2 * radius;
            height = height < 0 ? 0 : height;
            var direction = capsuleCollider.transform.rotation * new Vector3(
                capsuleCollider.direction == 0 ? height / 2 : 0,
                capsuleCollider.direction == 1 ? height / 2 : 0,
                capsuleCollider.direction == 2 ? height / 2 : 0
            );

            var center = capsuleCollider.transform.TransformPoint(capsuleCollider.center);
            return new GlobalParameters
            {
                Point1 = center - direction,
                Point2 = center + direction,
                Radius = radius,
            };
    }

    Vector3 GlobalBaseGravity => transform.rotation * localBaseGravity;

    float CapsuleBaseDistance(Vector3 point, GlobalParameters parameters)
    {
        Vector3 capsuleDirection = parameters.Point2 - parameters.Point1;
        Vector3 delta = point - parameters.Point1;
        delta -= capsuleDirection * Mathf.Clamp(Vector3.Dot(delta, capsuleDirection) * 1 / capsuleDirection.sqrMagnitude, 0, 1);
        return delta.magnitude;
    }

    protected override float PointIntensity(Vector3 point)
    {
        GlobalParameters parameters = GetGlobalParameters();
        return 1 - Mathf.Clamp(
            (CapsuleBaseDistance(point, parameters) - parameters.Radius * internalRadius) /
            (parameters.Radius * (1 - internalRadius)),
            0, 1
        );
    }

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void OnDrawGizmos()
    {
        // TODO: InitializeOnLoad? Startup?
        capsuleCollider = GetComponent<CapsuleCollider>();

        GlobalParameters parameters = GetGlobalParameters();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(parameters.Point1, parameters.Radius * internalRadius);
        Gizmos.DrawWireSphere(parameters.Point2, parameters.Radius * internalRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(parameters.Point1, parameters.Radius);
        Gizmos.DrawWireSphere(parameters.Point2, parameters.Radius);

        Gizmos.color = Color.red;
        Vector3 direction = Vector3.Cross(parameters.Point1 - parameters.Point2, GlobalBaseGravity).normalized;
        for (float i = 0; i < 1; i += 0.03f)
        {
            Vector3 point = transform.position + direction * parameters.Radius * i;
            Gizmos.DrawLine(point, point + Gravity(point) / GlobalBaseGravity.magnitude);
        }
    }
}
