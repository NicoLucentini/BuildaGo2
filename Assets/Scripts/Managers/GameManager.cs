using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public enum GameStatus
{
    PLAYING,
    BOSS_END,
    TUTORIAL_END,
    TALENTS,
    RESOURCES_EARNED,
    WON
}
public class GameManager : MonoBehaviour, ISaver
{
    public static Action OnStartGame;
    public static Action OnGameEnd;

    public static GameManager instance;


    public LevelConfigurationSO levelConfiguration;
    public int currentLevel = 0;
    public List<LevelConfigurationSO> levels;

    private GameStatus status;

    //[SerializeField] public int extraTimeBeforeDestruction = 0;
    //[SerializeField] public int extraStartingGold = 0;


    public SerializedDictionary<ResourceType, int> resources = new();
    public SerializedDictionary<ResourceType, int> roundResources = new();

    public List<Building> buildings = new List<Building>();
    public SerializedDictionary<BuildingType, int> countBuildings = new();
    //public int baseRewardTime = 0;
    //public int baseRewardGold = 0;

    private Coroutine gameTimerCoroutine;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        instance = null;
    }
    private void OnEnable()
    {
        PlacementManager.OnBuildingPlaced += AddBuilding;
        PlacementManager.OnConstructionFinished += CheckForBossEnd;
        EventBus.Subscribe<ClickStartGameEvent>(OnClickedStartGame);
        EventBus.Subscribe<ClickFinishTurnEvent>(OnClickedFinishTurn);

        EventBus.Subscribe<TutorialEndedEvent>(OnTutorialEnded);

        EventBus.Subscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.Subscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.Subscribe<BuildingSameTypeEvent>(OnBuildingSameType);
        EventBus.Subscribe<BuildingNearRiverEvent>(OnBuildingNearRiver);
    }
    private void OnDisable()
    {
        PlacementManager.OnBuildingPlaced -= AddBuilding;
        PlacementManager.OnConstructionFinished -= CheckForBossEnd;
        EventBus.UnSubscribe<ClickStartGameEvent>(OnClickedStartGame);
        EventBus.UnSubscribe<ClickFinishTurnEvent>(OnClickedFinishTurn);

        EventBus.UnSubscribe<TutorialEndedEvent>(OnTutorialEnded);

        EventBus.UnSubscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.UnSubscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.UnSubscribe<BuildingSameTypeEvent>(OnBuildingSameType);
        EventBus.UnSubscribe<BuildingNearRiverEvent>(OnBuildingNearRiver);
    }
    private void Start()
    {
        SetResource(ResourceType.Houses, 0);
        SetResource(ResourceType.Farms, 0);
        SetResource(ResourceType.Industries, 0);
        SetResource(ResourceType.Gold, 0);
        SetResource(ResourceType.Timer, 0);

        roundResources = new SerializedDictionary<ResourceType, int> {
            { ResourceType.Houses, 0 },
            { ResourceType.Farms, 0 },
            { ResourceType.Industries, 0 },
            { ResourceType.Gold, 0 }
        };
        if (currentLevel == 0)// it means there is a tutorial
        {
            EventBus.Publish(new TutorialStartEvent());
        }
        else
        {
            SetLevel();
        }
    }
    private void OnClickedFinishTurn(ClickFinishTurnEvent @event)
    {
        EndGame(true, false);
    }

    private void OnClickedStartGame(ClickStartGameEvent @event)
    {
        StartGame();
    }



    void OnBuildingNearRiver(BuildingNearRiverEvent e)
    {
        switch (e.building.type) {
            case BuildingType.Houses: AddResourceReward(ResourceType.Gold, e.amount); break;
            case BuildingType.Farms: AddTimer(e.amount); break;
            case BuildingType.Industries: ConsumeResource(ResourceType.Gold, e.amount); break;
            default: break;
        }
    }

    void OnBuildingSameType(BuildingSameTypeEvent e)
    {
        if (e.building.type == BuildingType.Houses) {
            AddResourceReward(ResourceType.Houses, e.amount);
        }
        else if (e.building.type == BuildingType.Farms) {
            AddResourceReward(ResourceType.Farms, e.amount);
        }
        else if (e.building.type == BuildingType.Industries) {
            AddResourceReward(ResourceType.Industries, e.amount);
        }
    }

    void OnBuildingTrioEvent(BuildingTrioEvent e)
    {
        AddResourceReward(ResourceType.Gold, e.goldReward);
        AddTimer(e.timeReward);
    }
    void OnBuildingFinishedEvent(BuildingFinishedEvent e) {
        switch (e.building.type)
        {
            case BuildingType.Houses: AddResourceReward(ResourceType.Houses, 1); break;
            case BuildingType.Farms: { AddResourceReward(ResourceType.Farms, 1); ; AddTimer(1); break; }
            case BuildingType.Industries: { AddResourceReward(ResourceType.Gold, 1); AddResourceReward(ResourceType.Industries, 1); } break;
            default: break;
        }

        if (GetResource(ResourceType.Timer) <= 0) {
            EndGame();
        }
    }


    void CheckForBossEnd(Building building)
    {
        if (building.type != BuildingType.Boss) return;

        if (levelConfiguration.buildingAmount.ToList().All(x => buildings.Count(y => y.type == x.Key) >= x.Value)) {
            EndGame(false);
        }
    }
    void OnTutorialEnded(TutorialEndedEvent e) {
        EndGame(false, true);
    }

    void AddBuilding(Building building) {
        buildings.Add(building);
        countBuildings.TryAdd(building.type, 1);
    }
    public void RemoveBuilding(Building building)
    {
        if (buildings.Contains(building)) {
            buildings.Remove(building);
        }
        if (countBuildings.ContainsKey(building.type)) {
            countBuildings[building.type] -= 1;
        }
    }
    private bool SetLevel() {
        if (currentLevel < levels.Count) {
            levelConfiguration = levels[currentLevel];
            return true;
        }
        return false;
    }
    public void ChangeLevel() {
        currentLevel++;

        if (!SetLevel()) {
            ChangeStatus(GameStatus.WON);
        }
    }
    private void StartGame()
    {
        int gameTime = GetResource(ResourceType.Extra_Start_Timer) + levelConfiguration.timeBeforeDestruction;
        int startGold = GetResource(ResourceType.Extra_Start_Gold) + levelConfiguration.startingGold;

        roundResources[ResourceType.Houses] = 0;
        roundResources[ResourceType.Farms] = 0;
        roundResources[ResourceType.Industries] = 0;
        roundResources[ResourceType.Gold] = 0;

        SetResource(ResourceType.Gold, startGold);
        SetResource(ResourceType.Timer, gameTime);

        buildings.DestroyAndClearList();
        countBuildings.Clear();

        gameTimerCoroutine = StartCoroutine(GameTimerCoroutine(gameTime));

        ChangeStatus(GameStatus.PLAYING);
        OnStartGame?.Invoke();
    }
    IEnumerator GameTimerCoroutine(int duration) {
        //timer = duration;
        while (GetResource(ResourceType.Timer) > 0) {

            //timer--;
            //timerText.text = timer + " Movements Left";
            yield return null;
        }
    }

    void EndGame(bool normalFinish = true, bool isTutorial = false) {
        if (status != GameStatus.PLAYING) return;

        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);

        OnGameEnd?.Invoke();
        EventBus.Publish(new GameEndEvent(isTutorial ? 0 : currentLevel,  resources));

        if (normalFinish) {
            ChangeStatus(GameStatus.RESOURCES_EARNED);
        }
        else {
            ChangeStatus(isTutorial ? GameStatus.TUTORIAL_END : GameStatus.BOSS_END);
            ChangeLevel();
            Invoke("ChangeToFinishGame", 3f);
        }
    }
    void ChangeStatus(GameStatus gameStatus) {
        status = gameStatus;
        EventBus.Publish(new GameStatusEvent(status));
    }
    void ChangeToFinishGame() {
        ChangeStatus(GameStatus.RESOURCES_EARNED);
    }

    #region Data

    public int GetBuildingCount(BuildingType type) {
        if (countBuildings.ContainsKey(type))
            return countBuildings[type];

        return 0;
    }

    public void AddTimer(int amount) {
        AddResource(ResourceType.Timer, amount);
    }
    public void AddResourceReward(ResourceType type, int amount) {
        AddResource(type, amount);
        if (roundResources.ContainsKey(type)) {
            roundResources[type] += amount;
        }
    }
    public void AddResource(ResourceType type, int amount) {
        if (resources.ContainsKey(type)) {
            resources[type] += amount;
            EventBus.Publish(new ResourceChangedEvent(type, resources[type]));
        }
    }
    public void SetResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] = amount;
        }
        else { 
            resources.Add(type, amount);
        }
        EventBus.Publish(new ResourceChangedEvent(type, resources[type]));
    }
    public void ConsumeResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventBus.Publish(new ResourceChangedEvent(type, resources[type]));
        }
    }
    public bool HasResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            return resources[type] - amount >= 0;
        }
        return false;
    }
    public int GetResource(ResourceType type) {
        if (resources.ContainsKey(type))
        {
            return resources[type];
        }
        return 0;
    }
    public int TryGetRoundResource(ResourceType type) {
        if (roundResources.ContainsKey(type)) {
            return roundResources[type];
        }
        return 0;
    }
    public bool HasRoundResource(ResourceType type, int amount)
    {
        if (roundResources.ContainsKey(type))
        {
            return roundResources[type] - amount >= 0;
        }
        return false;
    }
    public void ConsumeRoundResource(ResourceType type, int amount)
    {
        if (roundResources.ContainsKey(type))
        {
            roundResources[type] -= amount;
        }
    }

    public void Save()
    {
        SaveableData data = new SaveableData()
        {
            level = currentLevel,
            points = resources,
        };
        data.SaveData("GameManager", data);

    }

    public void Load()
    {
        SaveableData data = new();
        data = data.LoadData("GameManager");
        //ver que hacer...
        currentLevel = data.level;
        resources = data.points;

    }
    #endregion
}
public enum ResourceType {
    Houses,
    Farms,
    Industries,
    Gold,
    Timer,
    Extra_Start_Timer,
    Extra_Start_Gold,
    Base_Trio_Gold,
    Base_Trio_Time
}
[Serializable]
public class SaveableData : ISaveable<SaveableData>
{
    public int level;
    public SerializedDictionary<ResourceType, int> points;
    public int extraTime;
    public int extraStartingGold;
    public int baseRewardTime;
    public int baseRewardGold;

    public override SaveableData GetDefault()
    {
        return null;
    }
}
