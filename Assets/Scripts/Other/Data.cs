using UnityEngine;

[System.Serializable]
public class Data<T>
{

    public System.Action<T> onValueChanged;

    [SerializeField]
    private T value;

    private T defaultValue;

    public Data(T value)
    {
        this.value = value;
        defaultValue = value;
    }
    public Data(T value, T defaultValue)
    {
        this.value = value;
        this.defaultValue = defaultValue;
    }

    public virtual T Value
    {
        get { return value; }
        set { this.value = value; onValueChanged?.Invoke(this.value); }
    }
    public override string ToString()
    {
        return value.ToString();
    }
    public void Reset()
    {
        Value = defaultValue;
    }
}

public enum LaborType { 
CONSTRUCCION,
PINTURA,
PLOMERIA,
JARDINERIA,
ARQUITECTURA
}