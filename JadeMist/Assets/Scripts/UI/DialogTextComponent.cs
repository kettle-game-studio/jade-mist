using TMPro;
using UnityEngine;

public class DialogTextComponent : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetText(string t)
    {
        text.text = t;
    }
}
