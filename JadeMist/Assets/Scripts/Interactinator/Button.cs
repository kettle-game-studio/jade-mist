using UnityEngine;

public class Button : MonoBehaviour, Interactinator
{
    public GameObject activatinator;

    Activatinator _activatinator;
    bool state = false;

    void Start ()
    {
        _activatinator = activatinator.GetComponent<Activatinator>();
    }

    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        if (activatinator == null) return;

        if (state)
            _activatinator.Deactivate();
        else
            _activatinator.Activate();

        state = !state;
    }
}
