using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class ControlPanel : MonoBehaviour, Interactinator
{
    public MeshRenderer meshRenderer;
    public Texture2D metaTexture;
    public int buttonCount = 100;
    public GameObject[] targets;
    public string[] correctStrings;

    Texture2D controlTexture;
    Material controlMaterial;

    List<List<byte>> correct = new();

    void Start()
    {
        controlMaterial = meshRenderer.material;

        controlTexture = new Texture2D(buttonCount * 3, 1, TextureFormat.RGBA32, false, true, false);

        for (var i = 0; i < buttonCount * 3; i += 1)
            controlTexture.SetPixel(i, 0, Color.black);
        controlTexture.Apply();

        controlMaterial.SetTexture("_MetaTexture", metaTexture);
        controlMaterial.SetTexture("_ControlTexture", controlTexture);

        ParseCorrectStrings();
        CheckValue();
    }

    void ParseCorrectStrings()
    {
        foreach (var correctString in correctStrings)
        {
            var array = new List<byte>(correctString.Length);
            foreach (var ch in correctString)
            {
                if (ch == '1')
                    array.Add(1);
                else if (ch == '0')
                    array.Add(0);
                else
                    Debug.LogError($"Unexpected value in correctStrings: '{ch}'");
            }
            correct.Add(array);
        }
    }

    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        var pos = transform.InverseTransformPoint(raycastHitInfo.point) + new Vector3(0.5f, 0.5f, 0);
        var color = metaTexture.GetPixel(Mathf.RoundToInt(pos.x * metaTexture.width), Mathf.RoundToInt(pos.y * metaTexture.height));
        if (color.a != 1f) return;

        var id = Mathf.RoundToInt(color.r * 255);

        if (id == 255)
        {
            var data = CurrentStateString();
            GUIUtility.systemCopyBuffer = data;
            return;
        }
        if (id == 254)
        {
            for (var i = 0; i < buttonCount * 3; i += 1)
                controlTexture.SetPixel(i, 0, Color.black);

            controlTexture.Apply();
            controlMaterial.SetTexture("_ControlTexture", controlTexture);
            CheckValue();
        }

        if (controlTexture.GetPixel(id * 2, 0).r < 0.5)
        {
            controlTexture.SetPixel(id * 2, 0, Color.white);
        }
        else
        {
            controlTexture.SetPixel(id * 2, 0, Color.black);
        }

        controlTexture.Apply();
        controlMaterial.SetTexture("_ControlTexture", controlTexture);
        CheckValue();
    }

    void CheckValue()
    {
        var state = CurrentState();
        for (var i = 0; i < correct.Count; i += 1)
        {
            var flag = true;
            for (var j = 0; j < correct[i].Count; j += 1)
            {
                if (correct[i][j] != state[j])
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                targets[i].GetComponent<Activatinator>().Activate();
            }
            else
            {
                targets[i].GetComponent<Activatinator>().Deactivate();
            }

        }
    }

    string CurrentStateString()
    {
        return string.Join("", CurrentState().Select(s => s.ToString()).Take(buttonCount));
    }

    byte[] CurrentState()
    {
        var result = new byte[buttonCount];
        for (var i = 0; i < buttonCount; i++)
        {
            if (controlTexture.GetPixel(i * 2, 0).r > 0.5)
                result[i] = 1;
        }
        return result;
    }
}
