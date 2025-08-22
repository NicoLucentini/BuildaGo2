using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IncrementalGameManager : MonoBehaviour
{
    public static IncrementalGameManager instance;

    public bool gameRunning;
    public bool gamePaused;

    public int days; // one day = 24 seg 
    public int hours;
    public float builderWorkForce = 2;
    public float workCooldown = 5f;
    public float manualWorkForce = 3f;

    public Data<int> money = new Data<int>(15);
    public Data<int> builderCost = new Data<int>(10);
    public Data<int> builders = new Data<int>(0);
    public Data<int> housesDone = new Data<int>(0);
    public Data<float> workForce = new Data<float>(0);

    public Button buyBuilder;
    public Button startGameButton;
    public Button workButton;

    public TextMeshProUGUI hoursText;
    public TextMeshProUGUI daysText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI buildersText;
    public TextMeshProUGUI workforceText;
    public TextMeshProUGUI housesDoneText;
    public TextMeshProUGUI buyBuilderCostText;

    public Image manualWorkImage;

    public GameObject gameCanvas;

    public int maxHouses = 8;

    public List<House> houses = new();
    public HousesTable housesTable;

    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        startGameButton.onClick.AddListener(OnClickStartGame);
        buyBuilder.onClick.AddListener(OnClickBuyBuilder);
        workButton.onClick.AddListener(OnClickWork);

        money.onValueChanged += (x) => moneyText.text = $"Money: {x}";
        builderCost.onValueChanged += (x) => buyBuilderCostText.text = $"$ {x}";
        builders.onValueChanged += (x) => buildersText.text = $"Builders: {x}";
        workForce.onValueChanged += (x) => workforceText.text = $"Workforce: {x}";
        housesDone.onValueChanged += (x) => housesDoneText.text = $"{x}";
    }
    public void OnClickStartGame() {

        startGameButton.gameObject.SetActive(false);
        gameCanvas.SetActive(true);

        StartGame();
    }
    public void StartGame() {

        gameRunning = true;
        gamePaused = false;
        money.Value = 15;
        builderCost.Value = 10;
        housesDone.Value = 0;
        workForce.Value = 0;
        builders.Value = 0;

        InitHouses();

        StartCoroutine(Run());
    }
    public void InitHouses() { 
        foreach(var house in houses)
        {
            house.Init();
        }
    }
    IEnumerator Run() {

        while(gameRunning)
        {
            if (gamePaused)
            {
                yield return null;
            }
            else { 
                UpdateHour();
                yield return new WaitForSeconds(1);
            }
        }
    }
    void UpdateHour() {
        hours++;
        hoursText.text = $"Hours: {hours}";
        ApplyWorkforce(workForce.Value);

        if (hours % 24 == 0) {
            days++;
            hours = 0;
            daysText.text = $"Days: {days}";
        }
    }
    public void ApplyWorkforce(float force) {
        
        var random = new System.Random();
        var wf = force;
        while (wf > 0){

            var l = houses
                .GroupBy(x => x.houseLevel)
                .OrderBy(k => k.Key)
                .Select(g => new {
                    Level = g.Key,
                    Houses = g.OrderBy(x => random.Next()).ToList()
                })
                .ToList();

            foreach (var g in l) {
                
                foreach (var h in g.Houses) { 
                    wf = h.ApplyWorkForce((int)wf);
                }
            }
            //Ordenarlas por level
           //houses.OrderBy(x => x.houseLevel).ToList().ForEach(
              //  y => wf = y
        }

    }
    public void AddReward(int value) {
        money.Value += value;

    }
    public void OnClickBuyBuilder() {
        if (money.Value >= builderCost.Value) {
            BuyBuilder();
        }
    }
    public void BuyBuilder() {
        builders.Value++;
        money.Value -= builderCost.Value;
        builderCost.Value *= 2;
        workForce.Value = builders.Value * builderWorkForce;
    }
    public void OnClickWork() {
        if (workButton.interactable) {
            Work();
        }
    }
    public void Work() {
        StartCoroutine(WorkButtonAnimationAndCooldwon());
        ApplyWorkforce(manualWorkForce);
    }
    IEnumerator WorkButtonAnimationAndCooldwon() {
        workButton.interactable = false;
        var timer = 0f;

        while (timer < workCooldown) {
            timer += Time.deltaTime;
            manualWorkImage.fillAmount = (timer / workCooldown);
            yield return null;
        }

        workButton.interactable = true;
    }
}

