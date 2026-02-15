using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{
    public RectTransform container;
    public GameObject textPrefab;
    public GameObject buttonPrefab;
    public float perLetterLag = 0f;
    public UnityEngine.UI.Button skipButton;
    public Scrollbar scrollbar;

    [NonSerialized] public TextProvider textProvider;

    public bool IsRunning
    {
        get;
        private set;
    }

    int answerIdx;
    bool skipLine = false;

    void Start()
    {
        skipButton.onClick.AddListener(() =>
        {
            Debug.Log($"DialogUI Scrollbar Value = {scrollbar.value}");
            // skipLine = true;
        });
    }

    public void StartDialog(Dialoginator dialoginator)
    {
        IsRunning = true;
        StartCoroutine(DialogCoroutine(dialoginator));
    }

    IEnumerator DialogCoroutine(Dialoginator dialoginator)
    {
        var dialog = dialoginator.StartDialog(() => answerIdx);
        foreach (var d in dialog)
        {
            switch (d)
            {
                case Line line:
                    yield return ISay(line.textId);
                    break;

                case Question question:
                    yield return Ask(question);
                    break;
            }
        }

        StopDialog();
    }

    void StopDialog()
    {
        IsRunning = false;
        for (var i = 0; i < container.gameObject.transform.childCount; i++)
            Destroy(container.gameObject.transform.GetChild(i).gameObject);
        gameObject.SetActive(false);
    }

    IEnumerator ISay(string textId)
    {
        var prefix = "";
        return Say(textId, prefix);
    }

    IEnumerator YouSay(string textId)
    {
        var prefix = "<color=#FF0>";
        return Say(textId, prefix);
    }

    IEnumerator Say(string textId, string prefix, bool wait = true)
    {

        var instance = Instantiate(textPrefab, container);
        var textComponent = instance.GetComponent<DialogTextComponent>();

        var text = $"{prefix}{textProvider.GetText(textId)}";
        var setOnNextIteration = false;
        for (var i = prefix.Length + 1; i < text.Length + 1; i++)
        {
            if (skipLine)
            {
                textComponent.SetText(text);
                yield return new WaitForSeconds(perLetterLag);
                skipLine = false;
                yield break;
            }

            textComponent.SetText(text[..i]);
            var f = setOnNextIteration;

            setOnNextIteration = Mathf.Abs(scrollbar.value * container.rect.height) < 0.01;

            if (f)
            {
                scrollbar.value = 0;
            }

            if (wait)
                yield return new WaitForSeconds(perLetterLag);
        }
    }

    IEnumerator Ask(Question question)
    {
        answerIdx = -1;

        var scrollbarValue = scrollbar.value;

        var buttons = question.shortAnswerIds.Select((t, i) =>
        {
            var instance = Instantiate(buttonPrefab, container);
            var button = instance.GetComponent<DialogButtonComponent>();
            button.SetText($"{i + 1}. {textProvider.GetText(t)}");
            button.AddButtonClickEvent(() => { answerIdx = i; });
            return button;
        }
        ).ToList();

        var setOnNextIteration = Mathf.Abs(scrollbar.value * container.rect.height) < 0.01;
        Debug.Log($"DialogUI Ask: setOnNextIteration = {setOnNextIteration}");

        while (answerIdx == -1)
        {
            yield return null;
            var f = setOnNextIteration;

            setOnNextIteration = Mathf.Abs(scrollbar.value * container.rect.height) < 0.01;

            if (f)
            {
                scrollbar.value = 0;
            }
        }

        foreach (var b in buttons)
            b.Kill();

        yield return YouSay((question.fullAnswerIds ?? question.shortAnswerIds)[answerIdx]);
    }
}
