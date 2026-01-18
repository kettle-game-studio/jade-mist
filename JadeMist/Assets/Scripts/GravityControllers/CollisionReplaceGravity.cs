using System;
using UnityEngine;

public class CollisionReplaceGravity : MonoBehaviour
{
    public Vector3 gravity = Vector3.down;
    Vector3 GlobalGravity => transform.rotation * gravity;
    Vector3 gravityCallback(Vector3 point) => GlobalGravity;

    void OnCollisionEnter(Collision collision)
    {
        // collision.GetContacts()
        if (collision.body.TryGetComponent<PlayerController>(out var player))
        {
            player.gravity.UpdateDefaultGravity(gravityCallback);
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + GlobalGravity.normalized);
    }
}
