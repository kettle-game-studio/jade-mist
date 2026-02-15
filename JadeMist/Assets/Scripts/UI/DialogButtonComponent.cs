using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogButtonComponent : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UnityEngine.UI.Button button;

    UnityAction _listener;

    public void SetText(string t)
    {
        text.text = t;
    }

    public void AddButtonClickEvent(UnityAction listener)
    {
        if (_listener != null)
        {
            button.onClick.RemoveListener(_listener);
        }
        _listener = listener;
        button.onClick.AddListener(listener);
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        if (_listener != null)
        {
            button.onClick.RemoveListener(_listener);
        }
    }
}
