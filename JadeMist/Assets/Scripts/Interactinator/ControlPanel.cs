using System;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ControlPanel : MonoBehaviour, Interactinator
{
    public Texture2D metaTexture;
    public GameObject[] targets;

    Texture2D controlTexture;

    MeshRenderer meshRenderer;
    Material controlMaterial;

    const int controlSize = 100;

    byte[][] correct = {
        new byte[]{1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        new byte[]{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        new byte[]{1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
    };


    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        controlMaterial = meshRenderer.material;

        controlTexture = new Texture2D(controlSize, 1, TextureFormat.RGBA32, false);
        for (var i = 0; i < controlSize; i += 1)
            controlTexture.SetPixel(i, 0, Color.black);
        controlTexture.Apply();

        controlMaterial.SetTexture("_ControlTexture", controlTexture);

        CheckValue();
    }

    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        var pos = transform.InverseTransformPoint(raycastHitInfo.point) + new Vector3(0.5f, 0.5f, 0);
        var color = metaTexture.GetPixel(Mathf.RoundToInt(pos.x * metaTexture.width), Mathf.RoundToInt(pos.y * metaTexture.height));
        if (color.a != 1f) return;

        var id = Mathf.RoundToInt(color.r * 255);

        if (id == 255)
        {
            for (var i = 0; i < controlSize; i += 1)
                controlTexture.SetPixel(i, 0, Color.black);
            controlTexture.Apply();
            controlMaterial.SetTexture("_ControlTexture", controlTexture);
            CheckValue();

            // var data = $"{{{string.Join(", ", CurrentState().Select(s => s.ToString()))}}}; ";
            // GUIUtility.systemCopyBuffer = data;
            // Debug.Log();
            return;
        }

        var c = "";
        if (controlTexture.GetPixel(id, 0).r < 0.5)
        {
            controlTexture.SetPixel(id, 0, Color.white);
            c = "white";
        }
        else
        {
            controlTexture.SetPixel(id, 0, Color.black);
            c = "black";
        }

        controlTexture.Apply();

        controlMaterial.SetTexture("_ControlTexture", controlTexture);
        // Debug.Log($"ControlPanel Interact {id} = {c}");

        CheckValue();
    }

    void CheckValue()
    {
        var state = CurrentState();
        for (var i = 0; i < correct.Length; i += 1)
        {
            var flag = true;
            for (var j = 0; j < correct[i].Length; j += 1)
            {
                if (correct[i][j] != state[j])
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                targets[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
            else
            {
                targets[i].GetComponent<MeshRenderer>().material.color = Color.black;
            }

        }
    }

    byte[] CurrentState()
    {
        var result = new byte[controlSize];
        for (var i = 0; i < controlSize; i++)
        {
            if (controlTexture.GetPixel(i, 0).r > 0.5)
                result[i] = 1;
        }
        return result;
    }
}
