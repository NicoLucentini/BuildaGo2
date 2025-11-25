using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Button okButton;
    public float duration = 3f;
    private void Awake()
    {
        okButton.onClick.AddListener(Close);
    }
    private void OnEnable()
    {
        GameManager.OnStartGame += Close;
    }
    private void OnDisable()
    {
        GameManager.OnStartGame -= Close;
    }
    public void Set(string message, float duration = 3f) {
        this.duration = duration;
        gameObject.SetActive(true);
        text.text = message;
        new Timer("Popup", duration, Close).Start();
    }
    void Close() {
        TimersCoroutinesManager.instance.Stop("Popup");
        gameObject.SetActive(false);
    }
}
