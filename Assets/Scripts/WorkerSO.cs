using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorkerSO", menuName = "Scriptable Objects/WorkerSO")]
public class WorkerSO : ScriptableObject
{
    public string workerName;
    public int energy;
    public int weeklyCost;
    public Sprite portrait;
    public SerializedDictionary<LaborType, int> labor = new();
    public List<CardSO> specialCards; 
    public string workerDescription;
}
