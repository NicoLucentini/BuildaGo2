
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

public class BaseEvent : IGameEvent
{
    
}
public class BaseBuildingEvent : IGameEvent {
    public readonly Building building;
    public BaseBuildingEvent(Building building)
    {
        this.building = building;
    }

}
public class BuildingTrioEvent : BaseBuildingEvent
{
    public int goldReward;
    public int timeReward;
    public BuildingTrioEvent(Building building, int goldReward, int timeReward) : base(building)
    {
        this.goldReward = goldReward;
        this.timeReward = timeReward;
    }
}
public class BuildingSameTypeEvent : BaseBuildingEvent
{
    public int amount;
    public BuildingSameTypeEvent(Building building, int amount ) : base(building) 
    {
        this.amount = amount;
    }
}

public class BuildingFinishedEvent : BaseBuildingEvent
{
    public BuildingFinishedEvent(Building building) : base(building) { }
}
public class BuildingNearRiverEvent : BaseBuildingEvent {
    public int amount;
    public BuildingNearRiverEvent(Building building, int amount) : base(building)
    {
        this.amount = amount;
    }
}
public class BuildingFourInLine : BaseBuildingEvent {
    public List<Building> buildings;

    public BuildingFourInLine(Building building, List<Building> buildings) : base(building)
    {
        this.buildings = buildings;
    }

}
public class BuildingQuad : BaseBuildingEvent
{
    public List<Building> buildings;
    public Building prefabToInstantiate;

    public BuildingQuad(Building building, List<Building> buildings, Building prefabToInstantiate ) : base(building)
    {
        this.buildings = buildings;
        this.prefabToInstantiate = prefabToInstantiate;

    }
}

public class ClickStartGameEvent : IGameEvent { }
public class ClickFinishTurnEvent : IGameEvent { }

public class GameEndEvent : IGameEvent {
    public int level;
    public SerializedDictionary<ResourceType, int> roundResources;

    public GameEndEvent(int level,SerializedDictionary<ResourceType, int> roundResources)
    {
        this.level = level;
        this.roundResources = roundResources;
    }
}
public class GameStatusEvent : IGameEvent {
    public GameStatus status;

    public GameStatusEvent(GameStatus status)
    {
        this.status = status;
    }
}
public class ResourceChangedEvent : IGameEvent {
    public ResourceType type;
    public int amount;

    public ResourceChangedEvent(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}


