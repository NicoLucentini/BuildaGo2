using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static event System.Action OnEndTurn;
    public static event System.Action OnStartTurn;

    public Building selectedBuilding;

    public Data<int> energy = new Data<int>(0);
    public Data<int> money = new Data<int>(0);
    private int totalEnergy = 0;

    [Header("UI")]

    [SerializeField]
    private UIBuildingLaborSetup uiBuildingLaborSetup;
    [SerializeField]
    private Button startLaborBtn;
    [SerializeField]
    private Button endWeekBtn;
    [SerializeField]
    private TextMeshProUGUI energyText;
    

    private void Awake()
    {
        instance = this;

        startLaborBtn.onClick.AddListener(StartBuilding);
        endWeekBtn.onClick.AddListener(EndTurn);
    }
    private void OnEnable()
    {
        WorkerSelectionManager.OnChooseWorker += OnChooseWorker;
        CardManager.OnEndCardDraw += OnEndCardDraw;
    }
    private void OnDestroy()
    {
        WorkerSelectionManager.OnChooseWorker -= OnChooseWorker;
        CardManager.OnEndCardDraw -= OnEndCardDraw;
    }
    private void Start()
    {
        if (energyText == null) energyText = GameObject.Find("EnergyText").GetComponent<TextMeshProUGUI>();

        energy.onValueChanged += (x) => energyText.text = $"Energy: {x}";
    }

    public void ConsumeEnergy(int amount) {
        energy.Value -= amount;
    }
    public void AddMoney(int amount)
    {
        money.Value += amount;
    }
    public bool HasEnergy(int amount) => energy.Value >= amount;

    public void Update()
    {
        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, 1 << 10))
            {
                var clickedBuilding = hit.collider.gameObject.GetComponent<Building>();

                BuildingSelection(clickedBuilding);
            }
        }
    }
    public void BuildingSelection(Building building) {
        if (building == selectedBuilding) return;
        if (selectedBuilding != null && selectedBuilding.isStarted) return;

        if(selectedBuilding != null)
            uiBuildingLaborSetup.RemoveListeners(selectedBuilding);

        selectedBuilding = building;
        selectedBuilding.SelectBuilding();
        uiBuildingLaborSetup.Setup(selectedBuilding.labors);

        CameraMovement.instance.LookAtTarget(selectedBuilding.transform, false, true);
    }
    public void StartBuilding() {
        if (selectedBuilding == null) return;
        if (selectedBuilding.isStarted) return;

        CameraMovement.instance.LookAtTarget(selectedBuilding.transform, true, true);
        CardManager.instance.InitialDraw();
        CardManager.instance.TurnDrawCards();
        selectedBuilding.StartBuilding();
    }
    public void EndTurn() {
        if (selectedBuilding == null || !selectedBuilding.isStarted) return;

        OnEndTurn?.Invoke();
        //Wait--->

        //
        StartCoroutine( DoBuildingActions());
    }
    IEnumerator DoBuildingActions() {

        var card = selectedBuilding.CheckForBadEffect();
        if (card != null)
        {
            var c = CardManager.instance.CreateCard(card);
            CardManager.instance.AddCardToHand(c);
        }
        yield return new WaitForSeconds(1f);
        StartTurn();
    }
    public void StartTurn() {
        energy.Value = totalEnergy;
        OnStartTurn?.Invoke();
        CardManager.instance.TurnDrawCards();
    }
    void OnEndCardDraw() {

        Debug.Log("GameManager: OnEndCardDraw");
        foreach (var c in CardManager.instance.hand) {
            c.ApplyEffectsOnStartTurn();
        }
    }
    void OnChooseWorker(WorkerSO workerSo) {
        Debug.Log($"The Worker {workerSo.workerName} has been choosen");
        foreach (var card in workerSo.specialCards) {
            CardManager.instance.CreateCard(card);
        }
        totalEnergy += workerSo.energy;
        energy.Value = totalEnergy;
    }
}
