using System;
using System.Collections.Generic;

public class TestDialog2 : Dialoginator
{
    bool visited = false;

    override public IEnumerable<DialogEntry> StartDialog(Func<int> answerId)
    {
        prefix = "dialogs.test.black_box";

        if (visited)
            yield return NewLine("bb:second_visit");

        yield return NewLine("bb:introduction");
        yield return NewLine("bb:invitation");
        yield return NewQuestion(new[] { "pl:bye" }, new string[] { null });

        visited = true;
    }
}
