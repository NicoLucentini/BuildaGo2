using UnityEngine;

public class ToggleableAttribute : PropertyAttribute
{
    // Optional: default toggle state
    public bool defaultState;
    public ToggleableAttribute(bool defaultState = true)
    {
        this.defaultState = defaultState;
    }
}
[System.Serializable]
public class Toggleable<T>
{
    public bool enabled;
    public T value;

    public Toggleable() { }
    public Toggleable(bool enabled, T value)
    {
        this.enabled = enabled;
        this.value = value;
    }
}