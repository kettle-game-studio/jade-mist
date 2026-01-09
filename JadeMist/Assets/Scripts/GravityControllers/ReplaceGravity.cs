using UnityEngine;

public class ReplaceGravity : MonoBehaviour
{
    public Vector3 gravity = Vector3.down;
    Vector3 GlobalGravity => transform.rotation * gravity;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            player.gravity.UpdateDefaultGravity((Vector3) => GlobalGravity);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + GlobalGravity.normalized);
    }
}
