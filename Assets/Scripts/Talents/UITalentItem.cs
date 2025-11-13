using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UITalentItem : MonoBehaviour , IPointerClickHandler , IPointerEnterHandler, IPointerExitHandler{

    public TalentItem item;
    public Image image;
    public GameObject blocked;
    public Outline outline;

    public List<UITalentItem> nodes;
    private List<ItemConnection> connections = new();
    public TextMeshProUGUI talentLevel;

    void OnEnable() {
        item.OnTalentModified += OnTalentModified;
        UpdateText();
    }
    void OnDisable() {
        item.OnTalentModified -= OnTalentModified;
    }
    void OnTalentModified() {
        OnStatusChanged(item.status);
        UpdateText();
    }
    void OnStatusChanged(TalentStatus status) {
        blocked.SetActive(false);
        switch (status) {
            case TalentStatus.CAN_BE_USED: { UpdateText(); outline.effectColor = Color.green; } break;
            case TalentStatus.LOCKED: outline.effectColor = Color.red; break;
            case TalentStatus.USED : outline.effectColor = Color.gray;break;
            case TalentStatus.HIDE: blocked.SetActive(true); break;

        }
    }
    void UpdateText() {
        talentLevel.gameObject.SetActive(true);
        talentLevel.text = item.status == TalentStatus.USED ? "MAX" : $" {(item.upgradeAmount - item.remainingUpgrades)}/{item.upgradeAmount}";
    }
    public void OnValidate()
    {
        //item.from?.Clear();
        item.to?.Clear();
        foreach (var node in nodes) {
            AddTo(node.item);
            //node.AddFrom(item);
        }
        OnTalentModified();
    }
    public void AddTo(TalentItem to) {
        item.to.Add(to);
    }
    /*
    public void AddFrom(TalentItem from) {
        item.from.Add(from);
    }*/
    public void Set(TalentItem item) {
        this.item = item;
        image.sprite = item.img;
    }
    public void DrawConnections(ItemConnection prefab)
    {
        RemoveConnections();
        if (nodes == null) return;
        foreach(var node in nodes)
        {
            DrawConnection(node,prefab);
        }
    }
    public void DrawConnection(UITalentItem node, ItemConnection prefab) {
        var exists = connections.Exists(x => x.to == node.GetComponent<RectTransform>());
        if (exists) return;

        var go = Instantiate(prefab, transform.parent);
        go.transform.SetAsFirstSibling();
        go.Set(this, node);
        connections.Add(go);

        UnityEditor.EditorUtility.SetDirty(go);
        var so = new SerializedObject(go);
        so.ApplyModifiedProperties();
        var so2 = new SerializedObject(this);
        so2.ApplyModifiedProperties();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
       UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    public void RemoveConnections() {
        connections.DestroyAndClearList(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        item.Upgrade();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item.status == TalentStatus.USED || item.status == TalentStatus.HIDE) return; 
        TooltipSystem.instance.Show(item.description, GetComponent<RectTransform>().position + new Vector3(150,150,0));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (item.status == TalentStatus.USED || item.status == TalentStatus.HIDE) return;
        TooltipSystem.instance.Hide();
    }
}
[System.Serializable]
public class TalentItem // Podria ser scriptable object
{
    [SerializeReference]
    public List<ITalentRequirement> requirements = new List<ITalentRequirement>();

    public Action OnTalentModified;
    public string description;
    public Sprite img;
    public int upgradeAmount = 0;
    public int remainingUpgrades;// just 1
    public TalentStatus status;
    public SerializedDictionary<BuildingType, int> cost;
    [SerializeReference] public List<BaseUpgrade> upgrades = new List<BaseUpgrade>();    
    //[NonSerialized] public List<TalentItem> from = new(); // 
    [NonSerialized] public List<TalentItem> to = new();


    public void Show() {
        ChangeStatus(TalentStatus.LOCKED);
    } // it can be discovered but not used
    public void Unlock() {
        ChangeStatus(TalentStatus.CAN_BE_USED);
        remainingUpgrades = upgradeAmount;
        to.ForEach(x => x.Show());
    } // it can be used
    public void Upgrade() {
        if (status == TalentStatus.LOCKED) { TalentManager.instance.ShowPopup("Talent is locked"); return; }
        if (status == TalentStatus.USED) { TalentManager.instance.ShowPopup("Talent already used"); return; }
        if (!HasEnough()) { TalentManager.instance.ShowPopup("You dont have enough resources"); return; }

        cost.ToList().ForEach(x => GameManager.instance.UsePoints(x.Key, x.Value));

        to.ForEach(x => x.Unlock());

        upgrades.ForEach(x=>x.Upgrade());
        remainingUpgrades--;
        if (remainingUpgrades == 0) {
            ChangeStatus(TalentStatus.USED);
        }
        
        OnTalentModified?.Invoke();
    } 
    bool HasEnough() {
        return TalentManager.instance.cheat ||  cost.All(x => GameManager.instance.HasPoints(x.Key, x.Value));
    }
    void ChangeStatus(TalentStatus newStatus) {
        status = newStatus;
        OnTalentModified?.Invoke();
    }

}
public enum TalentStatus { 
    HIDE,
    LOCKED,
    CAN_BE_USED,
    USED
}

public interface ITalentRequirement {
    bool Evaluate();
}
[Serializable]
public class LevelRequirement : ITalentRequirement
{
    public int levelAmount;
    public bool Evaluate()
    {
        return true;
    }
}
[Serializable]
public class BossRequirement : ITalentRequirement
{
    public int amount = 1;
    public bool Evaluate()
    {
        return true;
    }
}