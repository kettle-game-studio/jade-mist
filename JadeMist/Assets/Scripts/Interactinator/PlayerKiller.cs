using System;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerKiller : MonoBehaviour, Interactinator
{
    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        player.Die(player.transform.position);
    }
}
