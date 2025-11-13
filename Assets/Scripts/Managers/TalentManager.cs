using System.Collections.Generic;
using UnityEngine;

public class TalentManager : MonoBehaviour
{
    public static TalentManager instance;
    public List<UITalentItem> items = new();
    public ItemConnection uiLinePrefab;
    public UIPopup popup;
    public bool cheat;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        if (cheat) { 
            foreach(var item in items)
            {
                item.item.Unlock();
            }
        }
    }
    internal void UpdateNodes()
    {
        foreach (var item in items)
        {
            item.DrawConnections(uiLinePrefab);
        }
    }
    internal void ClearNodes() {
        foreach (var item in items)
        {
            item.RemoveConnections();
        }
    }
    public void ShowPopup(string message, float duration = 3f) { 
        popup.Set(message, duration);
    }
}

