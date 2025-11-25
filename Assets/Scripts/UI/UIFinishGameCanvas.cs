using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFinishGameCanvas : MonoBehaviour
{
    public Button okButton;

    public SerializedDictionary<BuildingType, TextMeshProUGUI> pointItems = new();

    private void Awake()
    {
        okButton.onClick.AddListener(OnClickOkButton);
    }
    public void Set(SerializedDictionary<BuildingType, int> points) {
        foreach (var point in points) {
            if(pointItems.ContainsKey(point.Key))
                pointItems[point.Key].text = point.Value.ToString();
        }
    }
    public void OnClickOkButton() {
        GameManager.instance.GoToTalents();
    }
}
