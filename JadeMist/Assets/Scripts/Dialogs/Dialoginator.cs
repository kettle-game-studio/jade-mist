using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public abstract class DialogEntry { };

public class Line : DialogEntry
{
    public string textId;
}

public class Question : DialogEntry
{
    public List<string> shortAnswerIds;
    public List<string> fullAnswerIds;
}

public abstract class Dialoginator : MonoBehaviour
{
    protected string prefix;

    protected Line NewLine(string s) => new() { textId = $"{prefix}.{s}" };

    protected Question NewQuestion(IReadOnlyList<string> q, IReadOnlyList<string> a = null)
    {
        var question = new Question { shortAnswerIds = q.Select(s => $"{prefix}.{s}").ToList() };

        if (a != null)
            question.fullAnswerIds = a.Select(s => s != null ? $"{prefix}.{s}" : "").ToList();

        return question;
    }

    public abstract IEnumerable<DialogEntry> StartDialog(Func<int> answerId);
}
