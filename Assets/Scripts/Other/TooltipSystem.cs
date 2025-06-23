using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText; // Or use `Text` if not TMP

    void Awake()
    {
        instance = this;
        Hide();
    }
    public void Show(string message)
    {
        tooltipText.text = message;
        tooltipPanel.transform.position = Input.mousePosition;
        tooltipPanel.SetActive(true);
    }
    public void Show(string message, Vector3 pos) {
        tooltipText.text = message;
        tooltipPanel.transform.position = pos;
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
