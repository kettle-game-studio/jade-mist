using UnityEngine;

public abstract class BasePlatformGravity : MonoBehaviour
{
    [Range(0, 1)]
    public float internalRadius = 0.5f;
    public Vector3 localBaseGravity = Vector3.down;

    Vector3 GlobalBaseGravity => transform.rotation * localBaseGravity;
    protected abstract float PointIntensity(Vector3 point);
    public Vector3 Gravity(Vector3 point) => Vector3.Lerp(Vector3.zero, GlobalBaseGravity, PointIntensity(point));

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            Vector3 point = player.transform.position;
            player.gravity.vector += Gravity(point);
            player.gravity.count += PointIntensity(point);
        }
    }
}
