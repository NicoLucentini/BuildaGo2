using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    enum GameStatus
    {
        PLAYING,
        BOSS_END,
        TUTORIAL_END,
        TALENTS,
        RESOURCES_EARNED
    }
    public static Action OnStartGame;
    public static Action OnGameEnd;

    public static GameManager instance;


    public LevelConfigurationSO levelConfiguration;
    public int currentLevel = 0;
    public List<LevelConfigurationSO> levels; 

    private GameStatus status;

    [SerializeField]public int extraTimeBeforeDestruction = 0;
    [SerializeField]public int extraStartingGold = 0;
    [SerializeField]public int timer;

    public SerializedDictionary<BuildingType, int> points;
    public SerializedDictionary<BuildingType, int> roundPoints;
   
    public List<Building> buildings=new List<Building>();

    public int baseRewardTime = 0;
    public int baseRewardGold = 0;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public Button startButton;
    public Button closeTalentButton;
    public Button finishTurn;
    public GameObject startCanvas;
    public GameObject gameCanvas;
    public GameObject resourcesCanvas;
    public UIFinishGameCanvas finishGameCanvas;
    public GameObject talentCanvas;
    public GameObject bossEnd;
    private Coroutine gameTimerCoroutine;

    public SerializedDictionary<BuildingType, TextMeshProUGUI> pointTexts; // sacar el oro de aca poque no es un BuildingType

    public UIReward rewardPrefab;
    public UIReward timeRewardPrefab;

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
        EventBus.Subscribe<TutorialEndedEvent>(OnTutorialEnded);
        EventBus.Subscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.Subscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.Subscribe<BuildingSameTypeEvent>(OnBuildingSameType);
    }
    private void OnDisable()
    {
        PlacementManager.OnBuildingPlaced -= AddBuilding;
        PlacementManager.OnConstructionFinished -= CheckForBossEnd;
        EventBus.UnSubscribe<TutorialEndedEvent>(OnTutorialEnded);
        EventBus.UnSubscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.UnSubscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.UnSubscribe<BuildingSameTypeEvent>(OnBuildingSameType);
    }

    void OnBuildingSameType(BuildingSameTypeEvent e)
    {
        AddReward(e.type, e.amount);
    }

    void OnBuildingTrioEvent(BuildingTrioEvent e)
    {
        
        AddReward(BuildingType.Gold, e.goldReward);
        AddTimer(e.timeReward);
    }
    void OnBuildingFinishedEvent(BuildingFinishedEvent e) {
        if (e.building.type == BuildingType.Industries)
        {
            AddReward(BuildingType.Gold, 1);
        }
        else if (e.building.type == BuildingType.Housing) {
            AddReward(BuildingType.Housing, 1);
        }
        else if (e.building.type == BuildingType.Farm)
        {
            AddReward(BuildingType.Farm, 1);
        }

        if(timer <= 0) { 
            EndGame();
        }
    }
    void AddPoints(BuildingFinishedEvent b) {
        if(b.building.type == BuildingType.Housing || b.building.type == BuildingType.Industries || b.building.type == BuildingType.Farm)
        { 
            AddReward(b.building.type, b.building.type == BuildingType.Housing ? 2 : 1);

            //granja da comida 2
            //industrias da 1 oro
            if (b.building.type == BuildingType.Industries) {
                if ((GetPoints(BuildingType.Housing) - GetPoints(BuildingType.Farm) - GetPoints(BuildingType.Industries) >= 0)){
                    AddPoints(BuildingType.Gold, 1);
                }
            }
        }
    }
    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        closeTalentButton.onClick.AddListener(CloseTalents);
        finishTurn.onClick.AddListener(() => EndGame(true, false));
        points = new SerializedDictionary<BuildingType, int>();

        points.Add(BuildingType.Housing, 0);
        points.Add(BuildingType.Farm, 0);
        points.Add(BuildingType.Industries, 0);
        points.Add(BuildingType.Gold, extraStartingGold);

        roundPoints = new SerializedDictionary<BuildingType, int>
        {
            { BuildingType.Housing, 0 },
            { BuildingType.Farm, 0 },
            { BuildingType.Industries, 0 },
            { BuildingType.Gold, 0 }
        };

        if (currentLevel == 0)// it means there is a tutorial
        {
            EventBus.Publish(new TutorialStartEvent());
        }
        else {
            SetLevel();
        } 
    }

    private void CheckForBossEnd(Building building)
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
        timer--;
    }
    public void RemoveBuilding(Building building)
    {
        if (buildings.Contains(building)){ 
            buildings.Remove(building);
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
            bossEnd.GetComponentInChildren<TextMeshProUGUI>().text = $"You won!!";
        }
    }
    private void StartGame()
    {
        int gameTime = extraTimeBeforeDestruction + levelConfiguration.timeBeforeDestruction;
        int startGold = extraStartingGold + levelConfiguration.startingGold;

        roundPoints[BuildingType.Housing] = 0;
        roundPoints[BuildingType.Farm] = 0;
        roundPoints[BuildingType.Industries] = 0;
        roundPoints[BuildingType.Gold] = 0;

        points[BuildingType.Gold] = startGold;
        /*
        roundPoints = new SerializedDictionary<BuildingType, int>
        {
            { BuildingType.Housing, 0 },
            { BuildingType.Farm, 0 },
            { BuildingType.Industries, 0 },
            { BuildingType.Gold, 0 }
        };

        points[BuildingType.Gold] = startGold;
        */
        buildings.DestroyAndClearList();
        startCanvas.SetActive(false);
        gameCanvas.SetActive(true);

        pointTexts.ToList().ForEach(x => UpdateUI(x.Key));

        gameTimerCoroutine = StartCoroutine(GameTimerCoroutine(gameTime));

        ChangeStatus(GameStatus.PLAYING);
        OnStartGame?.Invoke();
    }
    IEnumerator GameTimerCoroutine(int duration) {
        timer = duration;
        while (timer > 0) {

            //timer--;
            timerText.text =  timer + " Movements Left";
            yield return null;
        }
    }
   
    void EndGame(bool normalFinish = true, bool isTutorial = false) {
        if (status != GameStatus.PLAYING) return;

        if (gameTimerCoroutine != null) 
            StopCoroutine(gameTimerCoroutine);

        OnGameEnd?.Invoke();

        if (normalFinish)
        {
            GoToFinishGame();
            Debug.Log("Lose Game");
        }
        else {
            //MostrarCosas de boss etceeteraaa
            //This is level finished
            bossEnd.gameObject.SetActive(true);
            if (isTutorial)
            {
                bossEnd.GetComponentInChildren<TextMeshProUGUI>().text = $"Congratulations!! \n Tutorial complete !!";
                ChangeStatus(GameStatus.TUTORIAL_END);
            }
            else { 
                bossEnd.GetComponentInChildren<TextMeshProUGUI>().text = $"Congratulations!!\nLevel {currentLevel} complete ";
                ChangeStatus(GameStatus.BOSS_END);
            }
            ChangeLevel();
            Invoke("CloseTalents", 3f);
        }
        
    }
    void ChangeStatus(GameStatus gameStatus) { 
        status = gameStatus;
    }
    void GoToFinishGame() {
        bossEnd.gameObject.SetActive(false);
        ChangeStatus(GameStatus.RESOURCES_EARNED);
        finishGameCanvas.gameObject.SetActive(true);
        finishGameCanvas.Set(roundPoints);
        gameCanvas.SetActive(false);
    }
    internal void GoToTalents()
    {
        ChangeStatus(GameStatus.TALENTS);
        finishGameCanvas.gameObject.SetActive(false);
        talentCanvas.gameObject.SetActive(true);
    }
    void CloseTalents() {
        talentCanvas.SetActive(false);
        startCanvas.SetActive(true);
        bossEnd.gameObject.SetActive(false);
    }

    #region Data
    public bool HasPoints(BuildingType type, int amount)
    {
        return points[type] >= amount;
    }
    public void UsePoints(BuildingType type, int amount)
    {
        points[type] -= amount;
        UpdateUI(type);
    }
    public int TryGetRoundPoints(BuildingType type) {
        if (roundPoints == null) return 0;

        if (roundPoints.ContainsKey(type)) { 
            return roundPoints[type];
        }
        return 0;
    }
    public void ConsumeRoundPoints(BuildingType type,int amount)
    {
        if (roundPoints == null) return;

        if (roundPoints.ContainsKey(type))
        {
            roundPoints[type] -= amount;
        }
    }

    public bool HasGold(int amount)
    {
        return HasPoints(BuildingType.Gold, amount);
    }
    public void UseGold(int amount)
    {
        UsePoints(BuildingType.Gold, amount);
    }
    private void UpdateUI(BuildingType type)
    {
        pointTexts[type].text = points[type].ToString();
    }

    public void AddReward(BuildingType type, int amount)
    {
        AddPoints(type, amount);
        roundPoints[type] += amount;
    }
    public int GetPoints(BuildingType type)
    {
        return (int)points[type];
    }
    public void AddPoints(BuildingType type, int amount)
    {
        points[type] += amount;
        UpdateUI(type);
    }
    public void AddTimer(int amount) {
        timer += amount;
    }
    public void AddStartingGold(int amount) {
        extraStartingGold += amount;
    }
    public void AddStartingTimer(int amount) {
        extraTimeBeforeDestruction += amount;
    }
    public void AddBaseRewardTime(int amount) { 
        baseRewardTime += amount;
    }
    public void AddBaseRewardGold(int amount) {
        baseRewardGold += amount;
    }
    #endregion
}
