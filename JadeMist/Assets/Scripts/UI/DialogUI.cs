using System.Collections;
using TMPro;
using UnityEngine;

public class DialogUI : MonoBehaviour
{
    public RectTransform container;
    public GameObject textPrefab;
    public GameObject buttonPrefab;

    float offset = 10;

    void Start()
    {
        StartCoroutine(DialogCoroutine());
    }

    IEnumerator DialogCoroutine()
    {
        Say("Hello");
        Say("There");
        Say("General");
        Ask("Kenoby?");
        yield break;
    }

    void Say(string text)
    {
        var instance = Instantiate(textPrefab, container);
        var textComponent = instance.GetComponent<DialogTextComponent>();
        textComponent.SetText(text);
    }

    void Ask(string text)
    {
        var instance = Instantiate(buttonPrefab, container);
        var textComponent = instance.GetComponent<DialogTextComponent>();
        textComponent.SetText(text);
    }
}
