using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIBuildingLaborSetup : MonoBehaviour
{
    [SerializeField]
    private List<UIBuildingLabor> uiBuildingLabors = new();

    public void RemoveListeners(Building oldBuilding) {

        foreach (var labor in oldBuilding.labors.Keys)
        {
            var ui = uiBuildingLabors.First(x => x.type == labor);
            ui.RemoveListener(oldBuilding.labors[labor]);
            ui.amount.text = oldBuilding.labors[labor].Value.ToString();
        }
    }
    public void Setup(SerializedDictionary<LaborType, Data<int>> labors) {

        foreach (var labor in labors.Keys)
        {
            var ui = uiBuildingLabors.First(x => x.type == labor);
            ui.AddListener(labors[labor]);
            ui.amount.text = labors[labor].Value.ToString();
        }
    }
}
