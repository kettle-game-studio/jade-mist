using UnityEngine;

public class ToCenterReplaceGravity : MonoBehaviour
{
    public float gravity = 50;

    public Vector3 center = Vector3.zero;
    public Vector3 GlobalCenter => transform.TransformPoint(center);


    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            player.gravity.UpdateDefaultGravity((Vector3 point) => gravity * (GlobalCenter - point).normalized);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GlobalCenter, 0.3f);
    }
}
