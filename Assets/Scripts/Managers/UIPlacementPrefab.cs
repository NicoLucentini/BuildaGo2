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
        costText.text = item.goldCost.ToString();
    }
}
/*
public class Condition {
    public Func<bool> pred = ()=> true;
    bool and = false;
    Condition next;

    public Condition(Func<bool> pred) {
        this.pred = pred;
    }
    public Condition And(Condition condition) {
        next = condition;
        and = true;
        return this;
    }
    public Condition Or(Condition condition)
    {
        next = condition;
        and = false;
        return this;
    }
    public Condition And(Func<bool> condition) { 
        return And(new Condition(condition));
    }
    public Condition Or(Func<bool> condition) {
        return Or(new Condition(condition));
    }
   
    public bool Evaluate() {
        if (next != null)
        {
            return and ? pred.Invoke() && next.Evaluate() : pred.Invoke() || next.Evaluate();
        }
        else {
            return pred.Invoke();
        }
    }
}
*/