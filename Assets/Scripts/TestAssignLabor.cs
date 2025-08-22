using UnityEngine;

public class TestAssignLabor : MonoBehaviour
{
    public LaborType type;
    public int amount;
    public void AssignLabor() {
        if(GameManager.instance.selectedBuilding != null)
            GameManager.instance.selectedBuilding.AssignLabor(type, amount);
    }
}
