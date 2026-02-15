using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Image crosshair;
    public TMPro.TextMeshProUGUI areaText;
    public float areaAnnouncementTime = 2f; 
    public TextProvider textProvider;
    public DialogUI dialogUI;

    Coroutine coroutine;

    void Start()
    {
        dialogUI.gameObject.SetActive(false);
        areaText.text = "";
    }

    public void StartDialog()
    {
        dialogUI.gameObject.SetActive(true);
        dialogUI.StartDialog();
    }

    public bool DialogRunning => dialogUI.IsRunning;

    public void LookingAtActivatable(bool ok)
    {
        crosshair.color = ok ? Color.red : Color.white;
    }

    public void DisplayAreaName(string areaID)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        var areaName = textProvider.GetText(areaID);
        coroutine = StartCoroutine(DisplayAreaNameCoroutine(areaName));
    }

    IEnumerator DisplayAreaNameCoroutine(string areaName)
    {
        float time = areaAnnouncementTime;
        while (time > 0)
        {
            var alpha = ((int)(time / areaAnnouncementTime * 255)).ToString("X");
            if (alpha.Length == 1)
                alpha = "0" + alpha;
            areaText.text = $"<alpha=#{alpha}>{areaName}";

            time -= Time.deltaTime;
            yield return null;
        }

        areaText.text = "";
        coroutine = null;
    }
}
