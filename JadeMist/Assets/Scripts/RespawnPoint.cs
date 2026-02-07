using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RespawnPoint : MonoBehaviour
{
    public Transform animatableCube;
    public MeshRenderer meshRenderer;

    Material material;

    void Start()
    {
        material = meshRenderer.material;
        material.color = Color.gray;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;


        player.SetRespawnPoint(this);
        StartCoroutine(IndicateActivation());
        Activate();
    }

    public void Deactivate()
    {   
        material.color = Color.gray;
    }

    void Activate()
    {
        material.color = Color.red;
    }

    IEnumerator IndicateActivation()
    {
        var frames = 30;
        for (var i = 0; i < frames; i++)
        {
            yield return new WaitForSeconds(0.5f / frames);
            animatableCube.transform.Rotate(Vector3.up, 1f * 360f / frames);
        }
        animatableCube.transform.rotation = Quaternion.identity;
    }
}