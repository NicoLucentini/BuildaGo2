using AYellowpaper.SerializedCollections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Button startButton;
    public Button closeTalentButton;
    public Button finishTurnButton;
    public GameObject startCanvas;
    public GameObject gameCanvas;
    public GameObject resourcesCanvas;
    public UIFinishGameCanvas finishGameCanvas;
    public GameObject talentCanvas;
    public GameObject bossEnd;


    public SerializedDictionary<ResourceType, TextMeshProUGUI> resourcesTexts;
    private void Awake()
    {
        instance = this; 
    }
    private void Start()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        closeTalentButton.onClick.AddListener(OnClickCloseTalents);
        finishTurnButton.onClick.AddListener(OnClickFinishTurn);
    }
    private void OnEnable()
    {
        EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        EventBus.Subscribe<GameStatusEvent>(OnChangeGameStatus);
        EventBus.Subscribe<ResourceChangedEvent>(OnResourceChange);
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<GameEndEvent>(OnGameEnd);
        EventBus.UnSubscribe<GameStatusEvent>(OnChangeGameStatus);
        EventBus.UnSubscribe<ResourceChangedEvent>(OnResourceChange);
    }

   

    private void OnClickFinishTurn()
    {
        EventBus.Publish(new ClickFinishTurnEvent());
    }
    private void OnClickStartButton()
    {
        EventBus.Publish(new ClickStartGameEvent());
    }

    private void OnClickCloseTalents()
    {
        CloseTalents();
    }

    private void OnResourceChange(ResourceChangedEvent e)
    {
        if (resourcesTexts.ContainsKey(e.type)) {
            if (e.type == ResourceType.Timer) {
                resourcesTexts[e.type].text = $"{e.amount} movements remaining";
            }
            else { 
                resourcesTexts[e.type].text = e.amount.ToString();
            }
        }
    }
    private void OnChangeGameStatus(GameStatusEvent e)
    {
        if (e.status == GameStatus.BOSS_END || e.status == GameStatus.TUTORIAL_END)
        {
            bossEnd.gameObject.SetActive(true);
        }
        else if (e.status == GameStatus.RESOURCES_EARNED)
        {
            FinishGame();
        }
        else if (e.status == GameStatus.TALENTS)
        {
            GoToTalents();
        }
        else if (e.status == GameStatus.WON)
        {
            bossEnd.GetComponentInChildren<TextMeshProUGUI>().text = $"You won!!";
        }
        else if (e.status == GameStatus.PLAYING) {
            startCanvas.SetActive(false);
            gameCanvas.SetActive(true);
        }
    }

    private void OnGameEnd(GameEndEvent e)
    {
        if (e.level == 0)
        {
            bossEnd.GetComponentInChildren<TextMeshProUGUI>().text = $"Congratulations!! \n Tutorial complete !!";
        }
        else {
            bossEnd.GetComponentInChildren<TextMeshProUGUI>().text = $"Congratulations!!\nLevel {e.level} complete ";
        }
       
        finishGameCanvas.Set(e.roundResources);
        
    }
    void FinishGame() {
        bossEnd.gameObject.SetActive(false);
        finishGameCanvas.gameObject.SetActive(true);
        gameCanvas.SetActive(false);
    }
    void CloseTalents() {
        talentCanvas.SetActive(false);
        startCanvas.SetActive(true);
        bossEnd.gameObject.SetActive(false);
    }
    
    public void GoToTalents() {
        finishGameCanvas.gameObject.SetActive(false);
        talentCanvas.gameObject.SetActive(true);
    }

   
}
