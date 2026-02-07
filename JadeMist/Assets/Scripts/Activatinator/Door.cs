using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Door : MonoBehaviour, Activatinator
{
    public float timeToOpen = 1f;

    public Transform door;
    public float desiredHeight;

    float startHeight;
    float desiredState = 0f;
    float state = 0f;

    void Start()
    {
        startHeight = door.position.y;
    }

    void Update()
    {
        if (Mathf.Abs(desiredState - state) > 0.001)
        {
            state += Math.Sign(desiredState - state) * Time.deltaTime;
            state = Mathf.Clamp01(state);
            var x = door.transform.position.x;
            var z = door.transform.position.z;
            door.transform.position = new Vector3(x, startHeight + Mathf.Lerp(0, desiredHeight, state), z);
        }
    }

    public void Activate()
    {
        desiredState = 1f;
    }

    public void Deactivate()
    {
        desiredState = 0f;
    }
}
