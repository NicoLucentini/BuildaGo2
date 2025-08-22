using UnityEngine;

public class House : MonoBehaviour {
    public int houseLevel = 1;
    public GameObject house;

    public int workForceNeededForNextLevel;
    public int workForceRemaining;
    public int reward;

    public void Init() {
        var entry = IncrementalGameManager.instance.housesTable.Get(houseLevel - 1); 
        house = Instantiate(entry.prefab, transform);
        workForceNeededForNextLevel = entry.workforceNeeeded;
        workForceRemaining = workForceNeededForNextLevel;
        reward = entry.reward;
    }

    public int ApplyWorkForce(int workForce) {
        workForceRemaining -= workForce;
        int workForceExtra = 0;

        if (workForceRemaining <= 0) {
            workForceExtra = Mathf.Abs(workForceRemaining);
            IncrementalGameManager.instance.AddReward(reward);
            GoForNextLevel();
        }
        return workForceExtra;
    }
    
    public void GoForNextLevel() {
        houseLevel++;
        if(houseLevel <8)
        { 
            Destroy(house);
            Init();
        }
    }
}
