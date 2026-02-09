using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Area : MonoBehaviour
{
    public string areaID;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var playerController))
            playerController.EnterArea(areaID);
    }
}
