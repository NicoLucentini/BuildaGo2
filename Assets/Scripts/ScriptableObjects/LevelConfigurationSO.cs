using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfigurationSO", menuName = "Scriptable Objects/LevelConfigurationSO")]
public class LevelConfigurationSO : ScriptableObject
{
    public float environmentAmount;

    public SerializedDictionary<BuildingType, int> buildingAmount = new SerializedDictionary<BuildingType, int>();

    public Vector2Int gridSize = new Vector2Int(10,10);

    public int timeBeforeDestruction;

    public int startingGold;

    public Building bossPrefab;

}
