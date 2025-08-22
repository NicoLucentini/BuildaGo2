using AYellowpaper.SerializedCollections;
using System.Linq;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializedDictionary("Labor", "Amount")]
    public SerializedDictionary<LaborType, Data<int>> labors = new();

    public static event System.Action<Building> OnBuildingComplete;

    public bool isStarted = false;

    public int effectsChance = 50;
    public CardSO effect;
    public void Start()
    {
        //Esto es en el start que es raro pero despues se tiene que modificar
       

    }
    public void SelectBuilding() {
        Debug.Log("Is Selected");
    }
    public void StartBuilding() {
        isStarted = true;
       
    }
    public void AssignLabor(LaborType type, int amount) {
        Debug.Log($"Is trying to assign labor {type} : {amount}");
        if (!labors.ContainsKey(type)) return;

        labors[type].Value = Mathf.Max(labors[type].Value - amount, 0);

        if (CheckIfComplete()) {
            OnBuildingComplete.Invoke(this);
        }
    }
    public CardSO CheckForBadEffect() {
        if (Random.Range(0, 100) < effectsChance) {
            return effect;
        }
        return null;
    }


    //Do Somethings...

    //Prepare For new turn
    bool CheckIfComplete() {
       return  labors.Sum(x => x.Value.Value) == 0;
    }
}
