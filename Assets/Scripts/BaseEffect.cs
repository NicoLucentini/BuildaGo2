using UnityEngine;

[System.Serializable]
public abstract class BaseEffect
{
    public string name;
    public float duration;
    public EffectType effectType;
}
[System.Serializable]
public class DmgEffect : BaseEffect {
    public float dmg;
}
[System.Serializable]
public class SpeedEffect : BaseEffect
{
    public float speed;
}
public enum EffectType { 
    ICE,
    WIND,
    FIRE,
    WATER,
    NATURE
}