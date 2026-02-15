using System;
using System.Collections.Generic;

public class TestDialog1 : Dialoginator
{
    public override IEnumerable<DialogEntry> StartDialog(Func<int> answerId)
    {
        prefix = "dialogs.test.yellow_box";

        yield return NewLine("yb:eat_pastries");
        yield return NewLine("yb:EAT_PASTRIES");
        yield return NewLine("yb:there");
        yield return NewLine("yb:general");

        yield return NewQuestion(new[] { "pl:kenobi", "pl:windu", "pl:grivus" });

        var answers = new[] { "yb:kenobi", "yb:windu", "yb:grivus" };
        yield return NewLine(answers[answerId()]);

        var sas = new List<string> { "pl:bye", "pl:what", "pl:ask_for_long_text" };
        var fas = new List<string> { null, "pl:what:f", "pl:ask_for_long_text" };

        while (true)
        {
            yield return NewQuestion(sas, fas);
            var aid = sas[answerId()];

            if (aid == "pl:bye")
                yield break;
            if (aid == "pl:what")
                yield return NewLine("yb:explanation");
            if (aid == "pl:ask_for_long_text")
                yield return NewLine("yb:long_text");

            sas.RemoveAt(answerId());
            fas.RemoveAt(answerId());
        }
    }
}
