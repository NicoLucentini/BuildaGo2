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
}