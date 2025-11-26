using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlacementPrefab : MonoBehaviour{
    public Button button;
    public TextMeshProUGUI costText;
    public PlacementPrefabs item;
    private void OnValidate()
    {
        button = GetComponentInChildren<Button>();
        costText = transform.Find("Cost").GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        item.OnChanged += UpdateUI;
    }
    private void OnDisable()
    {
        item.OnChanged -= UpdateUI;
    }
    public void UpdateUI(PlacementPrefabs pref)
    {
        costText.text = item.GetCostToString();
        button.GetComponentInChildren<TextMeshProUGUI>().text = pref.type.ToString();
    }
    public void Init(BuildingType type, Action OnButtonClick)
    {
        item.type = type;
        button.onClick.AddListener(() => OnButtonClick?.Invoke());
        button.gameObject.AddComponent<HoverDetector>().Set(
            () => TooltipSystem.instance.ShowWithOffset(item.GetDescription(), button.GetComponent<RectTransform>(), new Vector3(25, 150, 0)),
            () => TooltipSystem.instance.Hide());
        UpdateUI(item);
    }
}