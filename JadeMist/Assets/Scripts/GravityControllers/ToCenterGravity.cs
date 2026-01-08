using UnityEngine;

public class ToCenterGravity : MonoBehaviour
{
    public float gravity = 50;

    public Vector3 center = Vector3.zero;
    public Vector3 GlobalCenter => transform.TransformPoint(center);


    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.gravity.vector += gravity * (GlobalCenter - player.transform.position).normalized;
            player.gravity.count += 1;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GlobalCenter, 0.3f);
    }
}
