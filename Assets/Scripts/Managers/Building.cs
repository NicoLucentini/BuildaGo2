using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum ConstructionStatus {
    VISUAL,
    PLACED,
    ON_CONSTRUCTION,
    FINISHED
}


public class Building : MonoBehaviour {
    public static Action<Building, float> OnStartConstruction;
    public BuildingType type;
    public GameObject model;

    private MeshRenderer tempModel;
    public float constructionTime;
    [SerializeReference]public List<BaseUpgrade> upgrades = new List<BaseUpgrade>();
    public ConstructionStatus constructionStatus = ConstructionStatus.VISUAL;

    public float area = 1;
    public List<Building> neighbours = new();
    public Vector2Int size = new Vector2Int(1,1);
    public UnityEvent OnConstructionEndedEvent;

    public List<Vector3> posOccupied = new();

    public Building mega;

    public SerializedDictionary<BuildingType, int> basedRewards = new();

    [SerializeReference]
    public List<IPlacementRequirement> placementRequirements = new();

    private void Awake()
    {
        tempModel = GetComponent<MeshRenderer>();
    }
    public static Dictionary<BuildingType, int> CreateEmptyCounts() {
        Dictionary<BuildingType, int> dic = new Dictionary<BuildingType, int>
        {
            { BuildingType.Houses, 0 },
            { BuildingType.Roads, 0 },
            { BuildingType.Farms, 0 },
            { BuildingType.Industries, 0 }//,
            //{ BuildingType.Pond, 0 }
        };
        return dic;
    }
    public bool AllRequirementsMet(PlacementManager placement, GameManager game, Building b, Vector3 point) {
        return placementRequirements.All(x => x.Evaluate(placement, game, b, point));
    }
    void SubscribeToEvents() {
        PlacementManager.OnConstructionFinished += OnBuildingConstructed;
        PlacementManager.OnConstructionFinished += CheckForConstruction;
        PlacementManager.OnBuildingPlaced += OnBuildingPlaced;
        GameManager.OnGameEnd += OnGameEnd;
    }
    private void OnDisable()
    {
        PlacementManager.OnConstructionFinished -= OnBuildingConstructed;
        PlacementManager.OnConstructionFinished -= CheckForConstruction;
        PlacementManager.OnBuildingPlaced -= OnBuildingPlaced;
        GameManager.OnGameEnd -= OnGameEnd;
    }
    void OnGameEnd() {
        TimersCoroutinesManager.instance.Stop("StartConstruction" + gameObject.GetInstanceID());
    }
    void CheckForConstruction(Building building) {
        if (building == this || type == BuildingType.Boss) return;

        if (building.type == BuildingType.Roads)
            StartConstruction();
    }
    void OnBuildingPlaced(Building building) {
        if (building == this) return;
        if (neighbours.Contains(building)) return;

        StartCoroutine(WaitForOneFrame(building));
    }
    IEnumerator WaitForOneFrame(Building building) {
        yield return new WaitForSeconds(0.5f);
      
        if (!FindExistingNeighbours().Contains(building)) yield break;

        neighbours.Add(building);
    }
    
    void OnBuildingConstructed(Building building) {
       
        if (type == BuildingType.Roads || type == BuildingType.Environment || type == BuildingType.Boss || type == BuildingType.Pond) return;
        if (neighbours.Contains(building))
        {
           
        }

    }
    bool HasNeighborsOfTypes(Building building, params BuildingType[] requiredTypes)
    {
        foreach (var type in requiredTypes)
        {
            if (building.type == type) continue;
            if (!building.neighbours.Any(n => n.type == type))
                return false;
        }
        return true;
    }
    
    void ApplyUpgrades(UpgradeTarget target) { 
        if(upgrades == null) return;

        upgrades.ForEach(x =>
        {
            if (x.target == target)
            {
                x.Apply(this);
            }
        });
    }
    List<Building> FindExistingNeighbours()
    {
        var checkbox = new Vector3(size.x + area, 1, size.y + area) / 2f;
        Collider[] colliders = Physics.OverlapBox(transform.position, checkbox, Quaternion.identity, 1 << 10);
        return colliders.Select(x => x.GetComponent<Building>()).Where(x=> x != this && x.constructionStatus != ConstructionStatus.VISUAL).ToList();
    }
    void CalculateAndAddReward() {
        if (type == BuildingType.Roads || type == BuildingType.Environment || type == BuildingType.Boss || type == BuildingType.Pond || type == BuildingType.River) return;

        Dictionary<BuildingType, int> count =  CreateEmptyCounts();
       
        neighbours.ForEach(x => {
            if (x.constructionStatus == ConstructionStatus.FINISHED)
                count.AddOrUpgrade(x.type, 1);
        } );

        //Points reward
        
        int sumPoints = count[type];
        //Money reward in runtime
        int sumMoney = 0;
        int timeValue = GameManager.instance.baseRewardTime;
        int goldValue = GameManager.instance.baseRewardGold;
        int riverReward = 0;



        if (CheckQuad()) {
            Debug.Log("Quad");
        }
        if(count.ContainsKey(BuildingType.River) ){
            riverReward = count[BuildingType.River];
            EventBus.Publish(new BuildingNearRiverEvent(this, riverReward));
        }
        if (count.ContainsKey(BuildingType.Pond))
        {
            //timeValue += 1;
            sumPoints += 1;
            //goldValue += 1;
            sumMoney += 1;
        }

        //Same Type buildings
        if (sumPoints > 0)
        {
            EventBus.Publish(new BuildingSameTypeEvent(this, sumPoints));
        }

        //Trio event
        switch (type)
        {
            case BuildingType.Houses: sumMoney += Mathf.Min(count[BuildingType.Farms], count[BuildingType.Industries]); break;
            case BuildingType.Farms: sumMoney += Mathf.Min(count[BuildingType.Houses], count[BuildingType.Industries]); break;
            case BuildingType.Industries: sumMoney += Mathf.Min(count[BuildingType.Farms], count[BuildingType.Houses]); break;
            default: break;
        }
        if (sumMoney > 0)
        {
            timeValue += sumMoney;
            goldValue += sumMoney;
            EventBus.Publish(new BuildingTrioEvent(this, goldValue, timeValue));
        }
        if (sumMoney > 0 || sumPoints > 0) {
            var val = Mathf.Max(sumMoney, sumPoints);
            SoundManager.instance.PlaySfx(MathHelper.Map2(val,1,5, 0.3f, 0.7f));
        }
        //this could be the building finished...
        //this is like the standar reward
        foreach (var baser in basedRewards) { 
            GameManager.instance.AddReward(baser.Key, baser.Value);

            UIRewardManager.instance.CreateBuildingReward(transform.position.WithOffset(0.75f, 0.25f, 0), 
                GetComponent<MeshRenderer>().material.color, 
                baser.Value.ToString());
            if (baser.Key == BuildingType.Gold) {
                UIRewardManager.instance.CreateGoldReward(transform.position.WithOffset(-0.75f, 0.25f, 0), Color.white, baser.Value.ToString());
            }
        }
    }
    public bool CheckQuad() {
        Vector3[] dirs = new Vector3[] { new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(-0.5f, 0, -0.5f) };

        for(int i = 0; i < dirs.Length; i++)
        {
            Collider[] col = Physics.OverlapBox(transform.position + dirs[i], Vector3.one / 2f, Quaternion.identity, 1 << 10);

            var bs = col.Select(x => x.GetComponent<Building>()).ToList();
            if (bs.All(x => x.constructionStatus == ConstructionStatus.FINISHED && x.type == type) && bs.Count() == 4) {
                bs.ForEach(x => x.DestroyBuilding());
                PlacementManager.instance.PlaceBuilding(mega, transform.position + dirs[i]);
                EventBus.Publish(new BuildingQuad(this, bs, mega));
                return true;
            }

        }
        return false;
    }
    public bool HasFourInLine() {

        // Horizontal check
        if (CheckAndDestroy(Vector3.left, Vector3.right))
            return true;

        // Vertical check
        if (CheckAndDestroy(Vector3.forward, Vector3.back))
            return true;

        return false;
    }
    private bool CheckAndDestroy(Vector3 dirA, Vector3 dirB)
    {
        var listA = CountDirection(transform.position, dirA);
        var listB = CountDirection(transform.position, dirB);

        if (listA.Count + listB.Count + 1 < 4)
            return false;

        listA.AddRange(listB);

        EventBus.Publish(new BuildingFourInLine(this, listA));

        // Destroy all Buildings in both directions
        foreach (var b in listA) b.DestroyBuilding();
        //foreach (var b in listB) b.DestroyBuilding();
        DestroyBuilding();

        return true;
    }
    private List<Building> CountDirection(Vector3 start, Vector3 dir)
    {
        List<Building> result = new();
        Vector3 p = start + dir;

        while (true)
        {
            var b = PlacementManager.instance.TryGetBuilding(p);

            if (b == null || b.constructionStatus != ConstructionStatus.FINISHED || b.type != type) break;

            result.Add(b);
            p += dir;
        }

        return result;
    }

    public void Place(float constructionTime = 0, List<BaseUpgrade> upgrades = null, bool startConstruction = true)
    {
        this.constructionTime = constructionTime;
        this.upgrades = upgrades;
        gameObject.name = "Building" + type + GameManager.instance.buildings.Count;

        constructionStatus = ConstructionStatus.PLACED;

        PlacementManager.OnBuildingPlaced?.Invoke(this);

        ApplyUpgrades(UpgradeTarget.BUILDING_PLACED);

        neighbours.AddRange(FindExistingNeighbours());

        SubscribeToEvents();
        if (startConstruction)
        {
            StartConstruction();
        }


    }
    //its used in editor
    public void StartConstruction() {
        if (constructionStatus != ConstructionStatus.PLACED) return;

        //esto ya lo chequeo en el placement system
        //if (!HasRoadsNear() && RequiresRoads(type)) return;

        constructionStatus = ConstructionStatus.ON_CONSTRUCTION;
        PlacementManager.OnConstructionFinished -= CheckForConstruction;
        
        TriggerStartConstructionEvent();
        new Timer("StartConstruction"+ gameObject.GetInstanceID(), constructionTime, OnConstructionEnded).Start();
    }
    public void OnConstructionEnded()
    {
        TurnOnModel();
        TurnOffVisualModel();

        OnConstructionEndedEvent?.Invoke();
        EventBus.Publish(new BuildingFinishedEvent(this));
        constructionStatus = ConstructionStatus.FINISHED;

        ApplyUpgrades(UpgradeTarget.BUILDING_FINISHED);
        CalculateAndAddReward();
        PlacementManager.OnConstructionFinished?.Invoke(this);
    }
    void TurnOnModel() {
        if (model != null)
            model.SetActive(true);
    }
    void TurnOffVisualModel() {
        tempModel.enabled = false;
    }
    //Its used in editor
    public void TriggerStartConstructionEvent() {
        OnStartConstruction?.Invoke(this, constructionTime);
    }
    public void DestroyBuilding() {
        PlacementManager.instance.RemoveBuilding(this);
        Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        var checkbox = new Vector3(size.x + area, 1, size.y + area) ;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, checkbox);  
    }
    public static bool RequiresRoads(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Industries: return true;
            case BuildingType.Farms: return true;
            case BuildingType.Houses: return true;
            case BuildingType.Boss: return true;
            default: return false;
        }
    }
}
