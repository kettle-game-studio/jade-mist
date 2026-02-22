using UnityEngine;

public class CollisionKiller : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.body.TryGetComponent<PlayerController>(out var player))
            player.Die(collision.contacts[0].point);
    }
}
