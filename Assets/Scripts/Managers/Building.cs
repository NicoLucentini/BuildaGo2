using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

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
        if (building == this || building.type == BuildingType.Boss) return;

        if (building.type == BuildingType.Road)
            StartConstruction();
    }
    void OnBuildingPlaced(Building building) {
        StartCoroutine(WaitForOneFrame(building));
    }
    IEnumerator WaitForOneFrame(Building building) {
        yield return new WaitForSeconds(0.5f);
        if (building == this) yield break;
        if (neighbours.Contains(building)) yield break;
        if (!FindExistingNeighbours().Contains(building)) yield break;

        neighbours.Add(building);

        //Aca puedo calcular el premio de vuelta, pero solo con uno


    }
    
    void OnBuildingConstructed(Building building) {

        //TODO MEJORAR EL SISTEMA DE ENCONTRAR LOS VECINOS, QUE SE AGREGUEN A UNA LISTA DE VECINOS EN FUNCION DE onbuildingplaced
        //aca puedo checkear de nuevo el systema de reward
        //ej
        if (type == BuildingType.Road || type == BuildingType.Environment || type == BuildingType.Boss || type == BuildingType.Pond) return;
        if (neighbours.Contains(building))
        {
            if (building.type == type) {
                //Add reward by 1
                GameManager.instance.AddReward(type, 1);
                CreateReward(GameManager.instance.rewardPrefab,
                1.ToString(),
                GetComponent<MeshRenderer>().material.color,
                transform.position.WithOffset(new Vector3(-0.5f, 1.5f, 0)));
            }

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
        if(upgrades == null || upgrades.Count == 0) return;

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
        var checkbox = new Vector3(size.x + area, 1, size.y + area) / 2;
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


        Debug.Log("Check counts for " + gameObject.name);
        count.ToList().ForEach(x => Debug.Log("Type " + x.Key + " Amount " + x.Value));
       

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
            GameManager.instance.AddReward(type, sumPoints);

            CreateReward(GameManager.instance.rewardPrefab, 
                sumPoints.ToString(), 
                GetComponent<MeshRenderer>().material.color, 
                transform.position.WithOffset(new Vector3(-0.5f, 1.5f, 0)));
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
        }
        if (sumMoney > 0 || sumPoints > 0) {
            var val = Mathf.Max(sumMoney, sumPoints);
            SoundManager.instance.PlaySfx(MathHelper.Map2(val,1,5, 0.3f, 0.7f));
        }
    }
    void CreateReward(UIReward prefab, string message, Color color, Vector3 pos) {
       var reward =  Instantiate(prefab);
        reward.Set(message, color, pos);
    }
    
    bool HasRoadsAlong() {
        return neighbours.Any(x => x.type == BuildingType.Road);
        //var colls =  Physics.OverlapBox(transform.position, Vector3.one, Quaternion.identity, 1 << 10);
        //return colls.Length > 0 && colls.ToList().Any(x => x.gameObject.GetComponent<Building>().type == BuildingType.Road);
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

        if (!HasRoadsAlong() && RequiresRoads(type)) return;

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
    bool RequiresRoads(BuildingType type) {
        switch (type) { 
            case BuildingType.Industries: return true;
            case BuildingType.Farm: return true;
            case BuildingType.Housing: return true;
            case BuildingType.Boss: return true;
            default : return false;
        } 
    }
    private void OnDrawGizmos()
    {
        var checkbox = new Vector3(size.x + area, 1, size.y + area) ;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, checkbox);  
    }
}
