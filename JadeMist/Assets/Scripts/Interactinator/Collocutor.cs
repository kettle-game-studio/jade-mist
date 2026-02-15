using UnityEngine;

public class Collocutor : MonoBehaviour, Interactinator
{
    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        player.StartDialog();
    }
}
