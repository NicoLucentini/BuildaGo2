using UnityEngine;

[System.Serializable]
public abstract class BaseUpgrade {
    public UpgradeType upgradeType;
    public UpgradeTarget target;
    public virtual string GetDescription() {
        return "";
    }
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
    public override string GetDescription()
    {
        return $"Upgrade starting Gold by {amount} {upgradeType.ToStringOverride()} \n";
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
    public override string GetDescription()
    {
        return $"Decrease construction time of {type} by {amount} {upgradeType.ToStringOverride()} \n";
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
    public override string GetDescription()
    {
        return $"Decrease construction cost of {type} by {amount} {upgradeType.ToStringOverride()} \n";
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
    public override string GetDescription()
    {
        return $"Increase starting timer by {amount} {upgradeType.ToStringOverride()} \n";
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
    public override string GetDescription()
    {
        return $"Increase round timer by {amount} {upgradeType.ToStringOverride() } when building {type} is finished \n";
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
        for (int i = 0; i < direction.Length; i++) {

            var size = PlacementManager.instance.placementPrefabs[other].item.prefab.size;
            var sizedDirection = new Vector3(direction[i].x * size.x, 0, direction[i].z * size.y);
            var pos = building.transform.position + direction[i];
            var gridPointCorrected = PlacementManager.instance.GetCorrectedPointForBuilding(pos, size);
            if (PlacementManager.instance.IsPlaceFreeAndObjectIsOnGrid(gridPointCorrected, size)){
                PlacementManager.instance.PlaceBuildingFromLowBar(gridPointCorrected, other);
                break;
            }
            
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
    public override string GetDescription()
    {
        return $"Upgrade Base reward time by {amount} {upgradeType.ToStringOverride()} when 1 house, 1 farm and 1 industries are put together \n";
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
    public override string GetDescription()
    {
        return $"Upgrade Base reward gold by {amount} {upgradeType.ToStringOverride()} when 1 house, 1 farm and 1 industries are put together \n";
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
    public override string GetDescription()
    {
        return $"Creates a new building to place of type {type} \n";
    }
}
public class AddCheckFourChance : BaseUpgrade
{
    public BuildingType type;
    public int chance;
    public int goldEarn;

    public override void Upgrade()
    {
        //PlacementManager.instance.AddPlacementPrefab(type, prefab);
        PlacementManager.instance.AddUpgradeToPlacementPrefab(type, this);
        base.Upgrade();
    }
    public override void Apply(Building building = null)
    {
        if(building.type == type)
        {
            if (Random.Range(0, 100) > chance) return;

            if(building.HasFourInLine())
            {
                GameManager.instance.AddPoints(BuildingType.Gold, goldEarn);
            }
            base.Apply(building);
        }
    }
    public override string GetDescription()
    {
        return $"If there are 4 of the type: {type} you have a chance: {chance}% that those building are destroyed and win gold: {goldEarn}\n";
    }
}
public enum UpgradeType
{
    ADD,
    PERC,
    OTHER
}

public enum UpgradeTarget
{
    BUILDING_FINISHED,
    BUILDING_PLACED,
    ON_TALENT_UPGRADE
}
public static class UpgradeTypeExtension {
    public static string ToStringOverride(this UpgradeType type) {
        if (type == UpgradeType.ADD) {
            return "";
        }
        else if(type == UpgradeType.PERC)
        {
            return "percent";
        }
        return "";
    }
}