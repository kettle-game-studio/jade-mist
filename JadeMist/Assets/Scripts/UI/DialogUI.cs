using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DialogUI : MonoBehaviour
{
    public RectTransform container;
    public GameObject textPrefab;
    public GameObject buttonPrefab;

    public bool IsRunning
    {
        get;
        private set;
    }

    int answerIdx;

    public void StartDialog()
    {
        IsRunning = true;
        StartCoroutine(DialogCoroutine());
    }

    IEnumerator DialogCoroutine()
    {
        ISay("съешь же ещё этих мягких французских булок, да выпей чаю.");
        ISay("СЪЕШЬ ЖЕ ЕЩЁ ЭТИХ МЯГКИХ ФРАНЦУЗСКИХ БУЛОК, ДА ВЫПЕЙ ЧАЮ.");
        ISay("There");
        ISay("General");

        yield return Ask(new[] { "Kenoby?", "Windu?", "Гривус?" });

        var answers = new[] {
            "Классический вариант, правильно",
            "Ну хотя бы ты выбрал джедая, но он генерал только в легендах",
            "Чисто технически ты прав, но это всё равно довольно странно",
        };
        ISay(answers[answerIdx]);

        yield return Ask(new[] { "Чао-какао!" });

        StopDialog();
    }

    void StopDialog()
    {

        IsRunning = false;
        for (var i = 0; i < container.gameObject.transform.childCount; i++)
            Destroy(container.gameObject.transform.GetChild(i).gameObject);
        gameObject.SetActive(false);
    }

    void ISay(string text)
    {
        var instance = Instantiate(textPrefab, container);
        var textComponent = instance.GetComponent<DialogTextComponent>();
        textComponent.SetText(text);
    }

    void YouSay(string text)
    {
        var instance = Instantiate(textPrefab, container);
        var textComponent = instance.GetComponent<DialogTextComponent>();
        textComponent.SetText($"<color=#FF0>{text}");
    }

    IEnumerator Ask(string[] text)
    {
        answerIdx = -1;

        var buttons = text.Select((t, i) =>
        {
            var instance = Instantiate(buttonPrefab, container);
            var button = instance.GetComponent<DialogButtonComponent>();
            button.SetText($"{i + 1}. {t}");
            button.AddButtonClickEvent(() => { answerIdx = i; });
            return button;
        }
        ).ToList();

        while (answerIdx == -1)
            yield return null;

        foreach (var b in buttons)
            b.Kill();

        YouSay(text[answerIdx]);
    }
}
