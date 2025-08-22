
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HousesTable", menuName = "Scriptable Objects/HousesTable")]
public class HousesTable : ScriptableObject
{
    public List<HousesTableEntry> houses = new();

    public HousesTableEntry Get(int index) {
        return houses[index];
    }
}
[Serializable]
public class HousesTableEntry {
    public GameObject prefab;
    public int workforceNeeeded;
    public int reward;
}
public static class HouseLevelTable
{

    public static int HouseWorkforcePerLevel(int level)
    {
        switch (level)
        {
            case 2: return 5;
            case 3: return 8;
            case 4: return 13;
            case 5: return 21;
            case 6: return 34;
            case 7: return 55;
            case 8: return 89;
        }
        return 10;
    }
}
