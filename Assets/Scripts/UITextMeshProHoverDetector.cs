using TMPro;
using UnityEngine;

[RequireComponent(typeof(HoverDetector))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextMeshProHoverDetector : MonoBehaviour
{
    private TextMeshProUGUI text;
    public Color hover;
    public Color exit;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    public void OnEnter()
    {
        text.color = hover;
    }
    public void OnExit()
    {
        text.color = exit;
    }
}