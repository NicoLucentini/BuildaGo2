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
        int gold = GameManager.instance.TryGetRoundResource(ResourceType.Gold);
        goldText.text = gold.ToString();
    }
}
[Serializable]
public class ShopItem {
    Action OnBuy;
    public ResourceType type;
    public int cost;
    public TextMeshProUGUI costText;
    public Button button;
    public void Setup(Action OnBuy) {
        this.OnBuy = OnBuy;
        button.onClick.AddListener(Buy);
        costText.text = $"$ {cost}";
    }
    public void Buy() {
        if (GameManager.instance.HasRoundResource(ResourceType.Gold, cost))
        {
            GameManager.instance.ConsumeRoundResource(ResourceType.Gold, cost);
            GameManager.instance.AddResource(type, 1);
            OnBuy?.Invoke();
        }
        else {
            Debug.Log("you dont have enough money");
        }
    }
}
//Btn
//text
//cost
