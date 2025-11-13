using UnityEngine;

[System.Serializable]
public abstract class BaseUpgrade {
    public UpgradeType upgradeType;
    public UpgradeTarget target;
    public virtual void Upgrade() {
        TalentManager.instance.ShowPopup("Talent Applied");
    }
    public virtual void Apply(Building building = null) { } // this goes only for runtime upgrades
    public static float CalculatePercentageF(float initialValue, float percentage) {
        return initialValue * (percentage / 100);
    }
    public static int CalculatePercentageInt(int initialValue, float percentage)
    {
        return Mathf.RoundToInt(initialValue * (percentage / 100));
    }
}

[System.Serializable]
public class UpgradeStartingGold : BaseUpgrade
{
    public int amount;
    public override void Upgrade()
    {
        var amt = amount;
        if (upgradeType == UpgradeType.PERC)
        {
            amt = CalculatePercentageInt(GameManager.instance.extraStartingGold, amount);
        }
        GameManager.instance.AddStartingGold(amt);    
        base.Upgrade();
    }
}
public class DecreaseConstructionTime : BaseUpgrade
{
    public BuildingType type;
    public int amount;
    public override void Upgrade()
    {
        float value = (PlacementManager.instance.GetConstructionTime(type));
        var amt = (float)amount;
        if (upgradeType == UpgradeType.PERC)
        {
            amt = CalculatePercentageF(value, amount) ;
        }

        PlacementManager.instance.DecreaseConstructionTime(type, value - amt);
        base.Upgrade();
    }
}
public class DecreaseConstructionCost : BaseUpgrade
{
    public BuildingType type;
    public int amount;
    public override void Upgrade()
    {
        var value = PlacementManager.instance.GetConstructionCost(type);

        var amt = (float)amount;
        if (upgradeType == UpgradeType.PERC)
        {
            amt = CalculatePercentageF(value, amount);
        }

        PlacementManager.instance.DecreaseConstructionCost(type, Mathf.RoundToInt(value - amt));
        base.Upgrade();
    }
}
public class IncreaseStartingTimer : BaseUpgrade
{
    public int amount;
    public override void Upgrade()
    {
        float amt = amount;
        if (upgradeType == UpgradeType.PERC)
        {
            amt = CalculatePercentageInt(GameManager.instance.extraTimeBeforeDestruction, amount);
        }
        GameManager.instance.AddStartingTimer(Mathf.RoundToInt(amt));
        base.Upgrade();
    }
}
public class IncreaseRoundTimer : BaseUpgrade
{
    public BuildingType type;
    public int amount;
    public override void Upgrade()
    {
        PlacementManager.instance.AddUpgradeToPlacementPrefab(type, this);
        base.Upgrade();
    }
    public override void Apply(Building building = null)
    {
        GameManager.instance.AddTimer(amount);
    }
}
public class AddConstructionNear : BaseUpgrade {
    public BuildingType other;
    public BuildingType type;

    public override void Upgrade()
    {
        PlacementManager.instance.AddUpgradeToPlacementPrefab(type, this);
        base.Upgrade();
    }
    public override void Apply(Building building = null)
    {
        Vector3[] direction = new Vector3[8] { new Vector3(1,0,1), Vector3.right, Vector3.left, Vector3.forward, Vector3.back, new Vector3(-1,0,-1), new Vector3(-1, 0, 1), new Vector3(1, 0, -1) };
        direction.Shuffle();
        for (int i = 0; i < 8; i++) {
            var pos = building.transform.position + direction[i];
            /*
            if (PlacementManager.instance.IsPlaceFreeNonGrid(pos,PlacementManager.instance.placementPrefabs[type].item.prefab.size)){
                PlacementManager.instance.PlaceBuildingFromLowBar(pos, other);
                break;
            }
            */
        }
        base.Apply();
    }
}
public class UpgradeBaseRewardTime : BaseUpgrade {
    public int amount;
    public override void Upgrade()
    {
        GameManager.instance.AddBaseRewardTime(amount);
        base.Upgrade();
    }
}
public class UpgradeBaseRewardGold : BaseUpgrade
{
    public int amount;
    public override void Upgrade()
    {
        GameManager.instance.AddBaseRewardGold(amount);
        base.Upgrade();
    }
}
public class AddPlacementPrefab : BaseUpgrade {
    public GameObject prefab;
    public BuildingType type;

    public override void Upgrade()
    {
        PlacementManager.instance.AddPlacementPrefab(type, prefab);
        base.Upgrade();
    }
}
public enum UpgradeType
{
    ADD,
    PERC
}
public enum UpgradeTarget
{
    BUILDING_FINISHED,
    BUILDING_PLACED,
    ON_TALENT_UPGRADE
}