using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public enum ConstructionStatus {
    VISUAL,
    PLACED,
    ON_CONSTRUCTION,
    FINISHED
}
public class BaseBuildingEvent : IGameEvent{}
public class BuildingTrioEvent : BaseBuildingEvent{}
public class BuildingSameTypeEvent : BaseBuildingEvent { }

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

    private void Awake()
    {
        tempModel = GetComponent<MeshRenderer>();
    }
    public static Dictionary<BuildingType, int> CreateEmptyCounts() {
        Dictionary<BuildingType, int> dic = new Dictionary<BuildingType, int>
        {
            { BuildingType.Housing, 0 },
            { BuildingType.Road, 0 },
            { BuildingType.Farm, 0 },
            { BuildingType.Industries, 0 }//,
            //{ BuildingType.Pond, 0 }
        };
        return dic;
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

        if (building.type == BuildingType.Road)
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
       
        if (type == BuildingType.Road || type == BuildingType.Environment || type == BuildingType.Boss || type == BuildingType.Pond) return;
        if (neighbours.Contains(building))
        {
            /*
            if (building.type == type) {
                //Add reward by 1
                GameManager.instance.AddReward(type, 1);
                CreateReward(GameManager.instance.rewardPrefab,
                1.ToString(),
                GetComponent<MeshRenderer>().material.color,
                transform.position.WithOffset(new Vector3(-0.5f, 1.5f, 0)));
            }
            */
            /*
            if (HasNeighborsOfTypes(building, BuildingType.Housing, BuildingType.Farm, BuildingType.Industries)) {
                GameManager.instance.AddReward(BuildingType.Gold, 1);
                GameManager.instance.AddTimer(1);

                CreateReward(GameManager.instance.rewardPrefab, 1.ToString(), Color.yellow, transform.position.WithOffset(new Vector3(0.5f, 1.5f, 0)));
                CreateReward(GameManager.instance.timeRewardPrefab, 1.ToString(), Color.black, transform.position.WithOffset(new Vector3(1f, 1.5f, 0)));
            }
            */

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
        if (type == BuildingType.Road || type == BuildingType.Environment || type == BuildingType.Boss || type == BuildingType.Pond) return;

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

            //esto podria ir para otro lado
            GameManager.instance.AddReward(type, sumPoints);
            CreateReward(GameManager.instance.rewardPrefab, 
                sumPoints.ToString(), 
                GetComponent<MeshRenderer>().material.color, 
                transform.position.WithOffset(new Vector3(-0.5f, 1.5f, 0)));

            EventBus.Publish(new BuildingSameTypeEvent());
        }
        switch (type)
        {
            case BuildingType.Housing: sumMoney += Mathf.Min(count[BuildingType.Farm], count[BuildingType.Industries]); break;
            case BuildingType.Farm: sumMoney += Mathf.Min(count[BuildingType.Housing], count[BuildingType.Industries]); break;
            case BuildingType.Industries: sumMoney += Mathf.Min(count[BuildingType.Farm], count[BuildingType.Housing]); break;
            default: break;
        }
        if (sumMoney > 0)
        {
            timeValue += sumMoney;
            goldValue += sumMoney;

            GameManager.instance.AddReward(BuildingType.Gold, goldValue);
            GameManager.instance.AddTimer(timeValue);

            CreateReward(GameManager.instance.rewardPrefab, goldValue.ToString(), Color.yellow, transform.position.WithOffset(new Vector3(0.5f, 1.5f, 0)));
            CreateReward(GameManager.instance.timeRewardPrefab, timeValue.ToString(), Color.black, transform.position.WithOffset(new Vector3(1f, 1.5f, 0)));

            EventBus.Publish(new BuildingTrioEvent());
        }
        if (sumMoney > 0 || sumPoints > 0) {
            var val = Mathf.Max(sumMoney, sumPoints);
            SoundManager.instance.PlaySfx(MathHelper.Map2(val,1,5, 0.3f, 0.7f));
        }
    }
    public bool HasFourInLine() {

        var countL = CountDirection(transform.position, Vector3.left);
        var countR = CountDirection(transform.position, Vector3.right);

        if (countL.Count + countR.Count +1 >= 4) {

            countL.ForEach(x => x.DestroyBuilding());
            countR.ForEach(x => x.DestroyBuilding());
            DestroyBuilding();
            return true;
        }
       

        var countF = CountDirection(transform.position, Vector3.forward);
        var countB = CountDirection(transform.position, Vector3.back);
        if (countF.Count + countB.Count + 1 >= 4) {
            countF.ForEach(x => x.DestroyBuilding());
            countB.ForEach(x => x.DestroyBuilding());
            DestroyBuilding();
            return true;
        }
        return false;
    }
    List<Building> CountDirection(Vector3 start, Vector3 dir)
    {
        int c = 0;
        Vector3 p = start + dir;
        List<Building> n = new List<Building>();
        while (c <=4)
        {
            var b = PlacementManager.instance.TryGetBuilding(p);

            if (b == null || b.constructionStatus != ConstructionStatus.FINISHED || b.type != type) break;

            c++;
            p += dir;
            n.Add(b);

        }
        return n;
    }
    void CreateReward(UIReward prefab, string message, Color color, Vector3 pos) {
       var reward =  Instantiate(prefab);
       reward.Set(message, color, pos);
    }
    
    bool HasRoadsNear() => neighbours.Any(x => x.type == BuildingType.Road);
    public static bool RequiresRoads(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Industries: return true;
            case BuildingType.Farm: return true;
            case BuildingType.Housing: return true;
            case BuildingType.Boss: return true;
            default: return false;
        }
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

        if (!HasRoadsNear() && RequiresRoads(type)) return;

        constructionStatus = ConstructionStatus.ON_CONSTRUCTION;
        PlacementManager.OnConstructionFinished -= CheckForConstruction;
        TriggerStartConstructionEvent();
        new Timer("StartConstruction"+ gameObject.GetInstanceID(), constructionTime, OnConstructionEnded).Start();
    }
    public void OnConstructionEnded()
    {
        if (model != null)
            model.SetActive(true);

        tempModel.enabled = false;

        OnConstructionEndedEvent?.Invoke();
        constructionStatus = ConstructionStatus.FINISHED;

        ApplyUpgrades(UpgradeTarget.BUILDING_FINISHED);
        CalculateAndAddReward();
        PlacementManager.OnConstructionFinished?.Invoke(this);
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
}
