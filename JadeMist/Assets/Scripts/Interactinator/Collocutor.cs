using UnityEngine;

[RequireComponent(typeof(Dialoginator))]
public class Collocutor : MonoBehaviour, Interactinator
{
    Dialoginator dialoginator;

    void Start()
    {
        dialoginator = GetComponent<Dialoginator>();
    }

    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        player.StartDialog(dialoginator);
    }
}
