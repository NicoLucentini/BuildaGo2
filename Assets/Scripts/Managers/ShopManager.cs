using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    public TextMeshProUGUI goldText;

    public List<ShopItem> shopItems = new();

    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        shopItems.ForEach(x => x.Setup(UpdateTextUI));
        
    }
    private void OnEnable()
    {
        GameManager.OnGameEnd += UpdateTextUI;
    }
    private void OnDisable()
    {
        GameManager.OnGameEnd -= UpdateTextUI;
    }
    void UpdateTextUI()
    {
        int gold = GameManager.instance.TryGetRoundPoints(BuildingType.Gold);
        goldText.text = gold.ToString();
    }
}
[Serializable]
public class ShopItem {
    Action OnBuy;
    public BuildingType type;
    public int cost;
    public TextMeshProUGUI costText;
    public Button button;
    public void Setup(Action OnBuy) {
        this.OnBuy = OnBuy;
        button.onClick.AddListener(Buy);
        costText.text = $"$ {cost}";
    }
    public void Buy() {
        if (GameManager.instance.TryGetRoundPoints(BuildingType.Gold) >= 0)
        {
            GameManager.instance.ConsumeRoundPoints(BuildingType.Gold, cost);
            OnBuy?.Invoke();
            GameManager.instance.AddPoints(type, 1);
        }
        else {
            Debug.Log("you dont have enough money");
        }
    }
}
//Btn
//text
//cost
