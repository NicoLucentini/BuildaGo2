using UnityEngine;

[System.Serializable]
public abstract class BaseUpgrade {
    public UpgradeTarget target;
    public virtual string GetDescription() {
        return "";
    }
    public virtual void Upgrade() {
        TalentManager.instance.ShowPopup("Talent Applied");
    }
    public virtual void PreApply() { 
        
    }
    public virtual void Apply(Building building = null) { } // this goes only for runtime upgrades
    
}

[System.Serializable]
public class UpgradeStartingGold : BaseUpgrade
{
    public int amount;
    public AddType addType;
    public override void Upgrade()
    {
        var amt = GameManager.instance.GetResource(ResourceType.Extra_Start_Gold).Get(amount, addType);
        GameManager.instance.AddResource(ResourceType.Extra_Start_Gold,amt);    
        base.Upgrade();
    }
    public override string GetDescription()
    {
        return $"Upgrade starting Gold by {addType.ToStringOverride()} {amount}  \n";
    }
}
public class DecreaseConstructionTime : BaseUpgrade
{
    public BuildingType type;
    public AddType addType;
    public int amount;
    public override void Upgrade()
    {
        var value = PlacementManager.instance.GetConstructionTime(type);
        var amt = value.Get(amount, addType);
        PlacementManager.instance.DecreaseConstructionTime(type, value - amt);
        base.Upgrade();
    }
    public override string GetDescription()
    {
        return $"Decrease construction time of {type} by {addType.ToStringOverride()} {amount}  \n";
    }
}
public class DecreaseConstructionCost : BaseUpgrade
{
    public BuildingType type;
    public ResourceType resourceType;
    public int amount;
    public AddType addType;
    public override void Upgrade()
    {
        var value = PlacementManager.instance.GetConstructionCost(type, resourceType);

        var amt = value.Get(amount, addType);

        PlacementManager.instance.DecreaseConstructionCost(type, resourceType, Mathf.RoundToInt(value - amt));
        base.Upgrade();
    }
    public override string GetDescription()
    {
        return $"Decrease construction cost of {type} by {addType.ToStringOverride()} {amount}  \n";
    }
}
public class IncreaseStartingTimer : BaseUpgrade
{
    public int amount;
    public AddType addType;
    public override void Upgrade()
    {
        var amt = GameManager.instance.GetResource(ResourceType.Extra_Start_Timer).Get(amount, addType);
        GameManager.instance.AddResource(ResourceType.Extra_Start_Timer, amt);
        base.Upgrade();
    }
    public override string GetDescription()
    {
        return $"Increase starting timer by {addType.ToStringOverride()} {amount}  \n";
    }
}
public class IncreaseRoundTimer : BaseUpgrade
{
    public BuildingType type;
    public int amount;

    public AddType addType;
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
        return $"Increase round timer by {addType.ToStringOverride()} {amount}   when building {type} is finished \n";
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
    public AddType addType;
    public int amount;
    public override void Upgrade()
    {
        GameManager.instance.AddResource(ResourceType.Base_Trio_Time, amount);
        base.Upgrade();
    }
    public override string GetDescription()
    {
        return $"Upgrade Base reward time by {addType.ToStringOverride()} {amount}  when 1 house, 1 farm and 1 industries are put together \n";
    }
}
public class UpgradeBaseRewardGold : BaseUpgrade
{
    public int amount;
    public AddType addType;
    public override void Upgrade()
    {
        GameManager.instance.AddResource(ResourceType.Base_Trio_Gold, amount);
        base.Upgrade();
    }
    public override string GetDescription()
    {
        return $"Upgrade Base reward gold by {addType.ToStringOverride()} {amount}  when 1 house, 1 farm and 1 industries are put together \n";
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
public class FreeItemChance : BaseUpgrade {
    public BuildingType type;
    public int chance;
    public int increment;
    public override void Upgrade()
    {
        var pp = PlacementManager.instance.TryGetPlacementPrefab(type);
        if (!pp.upgrades.Contains(this))
        {
            PlacementManager.instance.AddUpgradeToPlacementPrefab(type, this);
        }
        else {
            chance += increment;
        }
    }
    public override void PreApply()
    {
       if(Random.Range(0, 100) < chance)
        {
            var pp = PlacementManager.instance.TryGetPlacementPrefab(type);
            if (pp != null) {
                pp.isFree = true;
            }
        }
    }
    public override void Apply(Building building = null)
    {
        var pp = PlacementManager.instance.TryGetPlacementPrefab(type);
        if (pp != null)
        {
            pp.isFree = false;
        }
        base.Apply(building);
    }
    public override string GetDescription()
    {
        return $"Building {type} has a chance: {chance} to be free when applied, it increments by {increment} every upgrade \n";
    }
}
public class AddCheckFourChance : BaseUpgrade
{
    public BuildingType type;
    public int chance;
    public int goldEarn;

    public override void Upgrade()
    {
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
                GameManager.instance.AddResource(ResourceType.Gold, goldEarn);
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
    ON_TALENT_UPGRADE,
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