using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    public int energyCost;
    public string cardName;
    public string cardDescription;
    public bool retain;
    public bool isNegative;
    public bool removeOnPlay;
    public ApplyEffectsOn applyEffectsOn;

    [SerializeReference]public List<BaseEffect> effects = new();
}
