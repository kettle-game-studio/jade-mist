using UnityEngine;

public class CameraJoint : MonoBehaviour
{
    [Range(0, 1)]
    public float a = 0.5f;
    [Range(0, 1)]
    public float b = 0.5f;
    Vector3 lastParentPosition;
    Vector3 lastCameraPosition;
    Vector3 localTargetPosition;

    void Start()
    {
        lastParentPosition = transform.parent.position;
        lastCameraPosition = transform.position;
        localTargetPosition = transform.localPosition;
    }

    void FixedUpdate()
    {
        Vector3 parentPosition = transform.parent.position;
        Vector3 targetPosition = transform.parent.TransformPoint(localTargetPosition);
        Vector3 playerDown = transform.parent.TransformVector(Vector3.down);
        Vector3 parentDelta = parentPosition - lastParentPosition;
        Vector3 cameraPosition = transform.position - parentDelta;
        float cameraProjection = Vector3.Dot(cameraPosition - targetPosition, playerDown);
        float lastProjection = Vector3.Dot(lastCameraPosition - targetPosition, playerDown);

        float newProjection = cameraProjection;
        newProjection += (newProjection - lastProjection) * b;
        newProjection = Mathf.Lerp(newProjection, 0, a);
        transform.localPosition = localTargetPosition + newProjection * Vector3.down;

        lastParentPosition = parentPosition;
        lastCameraPosition = cameraPosition;
    }

    // void Update()
    // {
    //     Vector3 parentPosition = transform.parent.position;
    //     Vector3 parentDelta = parentPosition - lastParentPosition;
    //     Vector3 worldTarget = transform.parent.TransformPoint(localTargetPosition);
    //     Vector3 cameraPosition = transform.position - parentDelta;

    //     Vector3 newPosition = cameraPosition;
    //     newPosition += (newPosition - lastCameraPosition) * b;
    //     newPosition = Vector3.Lerp(newPosition, worldTarget, a);
    //     transform.position = newPosition;

    //     lastParentPosition = parentPosition;
    //     lastCameraPosition = cameraPosition;
    // }
}
