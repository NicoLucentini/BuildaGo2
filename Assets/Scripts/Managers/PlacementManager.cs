using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    public static Action<Transform> OnGridReplaced;
    public static PlacementManager instance;

    public static Action<Building> OnConstructionFinished;
    public static Action<Building> OnBuildingPlaced;
    public static Action AllTilesComplete;

    public float cellSize = 1f;
  
    public GameObject gridModel;
    public Building environmentPrefab;

    public SerializedDictionary<Vector3, Building> posOccupied = new SerializedDictionary<Vector3, Building>();

    [Header("UI")]
    public SerializedDictionary<BuildingType, UIPlacementPrefab> placementPrefabs;
    public Transform placementPrefabsTransform;


    PlacementPrefabs current;
    private Building prefabVisual;
    private Building boss;
    private Vector2Int gridSize = new Vector2Int(20, 20);
    private float percentageOfEnvironment;
    private int tilesOccupied = 0;

    private Vector3 gridCenter = new Vector3(0, 0, 0);

    bool enableControls = false;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        placementPrefabs.ToList().ForEach(x => {
            x.Value.Init(x.Key, ()=>OnSelectPrefab(x.Value.item));
        }); ;
    }

    void OnEnable()
    {
        GameManager.OnStartGame += OnStartGame;
        GameManager.OnGameEnd += OnGameEnd;
        OnConstructionFinished += CheckForAllTiles;

    }
    void OnDisable()
    {
        GameManager.OnStartGame -= OnStartGame;
        GameManager.OnGameEnd -= OnGameEnd;
        OnConstructionFinished -= CheckForAllTiles;
    }
   
    void OnGameEnd()
    {
        DestroyPrefabVisual();
        enableControls = false;
    }
    void OnStartGame() {

        gridSize = GameManager.instance.levelConfiguration.gridSize;
        percentageOfEnvironment = GameManager.instance.levelConfiguration.environmentAmount;

        enableControls = true;

        tilesOccupied = 0;
        posOccupied.Clear();

        DoGrid();
        DoBoss();
        DoEnvironment();
    }
    void DoGrid()
    {
        gridModel.transform.localScale = new Vector3(gridSize.x, 0.1f, gridSize.y);

        gridCenter = new Vector3(gridSize.x / 2f, 0, gridSize.y / 2f);

        gridModel.transform.position = new Vector3(gridCenter.x, -0.01f, gridCenter.z);

        OnGridReplaced?.Invoke(gridModel.transform);
        gridModel.GetComponent<MeshRenderer>().material.mainTextureScale =  new Vector2(gridSize.x / 2f, gridSize.y/2f);
    }
    void DoEnvironment()
    {
        int amount = Mathf.RoundToInt((gridSize.x * gridSize.y) * percentageOfEnvironment);
        while (amount > 0 && !IsGridComplete())
        {
            var x = 0;
            var y = 0;
            Vector2Int vector = new Vector2Int(x, y);
            Vector3 pos = ToGridPosition(new Vector3(vector.x, 0.5f, vector.y));

            do
            {
                x = UnityEngine.Random.Range(0, gridSize.x);
                y = UnityEngine.Random.Range(0, gridSize.y);
                vector = new Vector2Int(x, y);
            }
            while (posOccupied.ContainsKey(GetCorrectedPointForBuilding(new Vector3(vector.x, 0.5f, vector.y), environmentPrefab.size))
            && !IsGridComplete());

            PlaceEnvironment(environmentPrefab, GetCorrectedPointForBuilding(new Vector3(vector.x, 0, vector.y), environmentPrefab.size));
            amount--;
        }
    }
    void DoBoss()
    {
        boss = GameManager.instance.levelConfiguration.bossPrefab;
        PlaceBoss(boss, GetCorrectedPointForBuilding(new Vector3(gridSize.x / 2f, 0, gridSize.y / 2f), boss.size));
    }

    void AddBuildingPlaced(Building building) {
        var pos = GetPosWithSize(building.transform.position, building.size);
        pos.ForEach(x => posOccupied.Add(x, building));
        int buildingArea = building.size.x * building.size.y;
        tilesOccupied += buildingArea;
        building.posOccupied = pos;

    }
    List<Vector3> GetPosWithSize(Vector3 pos, Vector2Int size) {
        var list = new List<Vector3>();
        Vector2Int initX = GetWithSize(size.x);
        Vector2Int initY = GetWithSize(size.y);

        float offsetX = size.x % 2 == 0 ? -0.5f : 0;
        float offsetY = size.y % 2 == 0 ? -0.5f : 0;

        for (int i = initX.x; i < initX.y; i++) {
            for (int j = initY.x; j < initY.y; j++)
            {
                list.Add(new Vector3(pos.x + i + offsetX, pos.y, pos.z + j + offsetY));
            }
        }
        return list;
    }
    Vector2Int GetWithSize(int size) {
        switch (size)
        {
            case 1: return new Vector2Int(0, 1);
            case 2: return new Vector2Int(0, 2);
            case 3: return new Vector2Int(-1, 2);
            case 4: return new Vector2Int(-1, 3);
            case 5: return new Vector2Int(-2, 3);
            case 6: return new Vector2Int(-2, 4);
            case 7: return new Vector2Int(-3, 4);
        }
        return new Vector2Int(0, 1);
    }
    private void OnSelectPrefab(PlacementPrefabs item)
    {
        DestroyPrefabVisual();

        current = item;
        prefabVisual = Instantiate(item.prefab);
    }
    void DestroyPrefabVisual()
    {
        if (prefabVisual != null && prefabVisual.gameObject != null)
            Destroy(prefabVisual.gameObject);
    }


    private void Update()
    {
        if (prefabVisual == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
        {

            var gridPointForBuilding = GetCorrectedPointForBuilding(hit.point, prefabVisual.size);

            if (prefabVisual != null)
            {
                prefabVisual.transform.position = gridPointForBuilding;

                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    OnGroundClick(gridPointForBuilding);
                }

            }

        }
        if (!enableControls) return;
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            OnSelectPrefab(placementPrefabs[BuildingType.Houses].item);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            OnSelectPrefab(placementPrefabs[BuildingType.Farms].item);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            OnSelectPrefab(placementPrefabs[BuildingType.Industries].item);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            OnSelectPrefab(placementPrefabs[BuildingType.Roads].item);
        }
    }

    public Vector3 ToGridPosition(Vector3 hit) {
        var x = Mathf.FloorToInt(hit.x);
        var y = Mathf.FloorToInt(hit.z);
        return new Vector3(x + cellSize / 2, 0.5f, y + cellSize / 2);
    }
    public Vector3 GetCorrectedPointForBuilding(Vector3 normalPoint, Vector2Int size)
    {
        var gridPoint = ToGridPosition(normalPoint);
        float moveX = size.x % 2 == 0 ? -0.5f : 0f;
        float moveY = size.y % 2 == 0 ? -0.5f : 0f;

        if (gridPoint.x + size.x / 2f > gridSize.x)
        {
            moveX = gridSize.x - (gridPoint.x + size.x / 2f);
        }
        else if (gridPoint.x - size.x / 2f < 0)
        {
            moveX = -gridPoint.x + size.x / 2f ;
        }
        if (gridPoint.z + size.y / 2f > gridSize.y)
        {
            moveY = gridSize.y - (gridPoint.z + size.y / 2f);
        }
        else if (gridPoint.z - size.y / 2f < 0)
        {
            moveY = -gridPoint.z + size.y / 2f ;
        }
        return gridPoint + new Vector3(moveX, 0, moveY);
    }

    public bool OnGroundClick(Vector3 point)
    {
        if (current.onCooldown) return false;
        //if (!GameManager.instance.HasGold(current.goldCost)) return false;
        if (!current.HasEnough()) { Debug.Log("Not Enough money"); return false; }
        if (!IsPlaceFreeAndObjectIsOnGrid(point, prefabVisual.size)) return false;
        if (!prefabVisual.AllRequirementsMet(this, GameManager.instance, prefabVisual, point)) return false;

        PlaceBuildingFromLowBar(prefabVisual.transform.position, current.type);
        //GameManager.instance.UseGold(current.goldCost);
        current.ConsumePoints();
        current.OnCooldown(true);
        new Timer("BuildingCooldown" + gameObject.GetInstanceID(), current.cooldown, () => current.OnCooldown(false)).Start();
        return true;
    }
    public bool IsPlaceFreeAndObjectIsOnGrid(Vector3 gridPointCorrected, Vector2Int size) {
        if (!IsPlaceFree(gridPointCorrected, size)) { Debug.Log("Place is Occupied"); return false; }
        if (!IsObjectOnGrid(gridPointCorrected, size)) { Debug.Log("Object Outside grid"); return false; }; 
        return true;
    }

    private Vector3 drawHit;
    private Vector3 drawSize;
    bool IsPlaceFree(Vector3 gridPoint, Vector2Int size)
    {
        drawHit = gridPoint;
        drawSize = new Vector3(size.x, 1, size.y) * 0.8f;

        return Physics.OverlapBox(gridPoint, new Vector3(size.x, 1, size.y) * 0.4f, Quaternion.identity, 1 << 10)
            .Select(x => x.GetComponent<Building>())
            .Count(x => x.constructionStatus != ConstructionStatus.VISUAL) == 0;
    }
    bool IsPointOnGrid(Vector3 hit) => hit.x > 0 && hit.x < gridSize.x && hit.z > 0 && hit.z < gridSize.y;
    bool IsObjectOnGrid(Vector3 hit, Vector2Int buildingSize)
    {
        var pos = GetPosWithSize(hit, buildingSize);

        for (int i = 0; i < pos.Count; i++)
        {
            if (!IsPointOnGrid(pos[i]))
            {
                return false;
            }
        }
        return true;
    }
    bool IsGridComplete() => tilesOccupied == gridSize.x * gridSize.y;
    void CheckForAllTiles(Building building) {
        new Timer("Check Fo All", 1f, CheckForAllTilesCompleteExceptBoss).Start();
    }
    
    public void CheckForAllTilesCompleteExceptBoss() {

        Debug.Log("CheckForAllTilesCompleteExceptBoss");

        if (IsGridComplete() && AllBuildingsConstructedExceptBoss()) {
            Debug.Log("AllTilesComplete");
            AllTilesComplete?.Invoke();
        }
    }
    bool AllBuildingsConstructedExceptBoss() {
        return GameManager.instance.buildings.Where(x => x.type != BuildingType.Boss).All(x => x.constructionStatus == ConstructionStatus.FINISHED);
    }

    void PlaceEnvironment(Building prefab, Vector3 point)
    {
        PlaceBuilding(prefab, point, prefab.constructionTime, true, null);
    }

    void PlaceBoss(Building prefab, Vector3 point)
    {
        PlaceBuilding(prefab, point, prefab.constructionTime, false, null);
    }

    #region PUBLIC METHODS
    public void PlaceBuildingFromLowBar(Vector3 point, BuildingType type)
    {
        var pref = placementPrefabs[type];

        GameManager.instance.ConsumeResource(ResourceType.Timer, 1);
        PlaceBuilding(pref.item.prefab, point, pref.item.constructionTime, true, pref.item.upgrades);
    }
    public void PlaceBuilding(Building prefab, Vector3 point, float constructionTime = 0, bool startConstruction = true, List<BaseUpgrade> upgrades = null) {
        var go = Instantiate(prefab);
        go.transform.position = point;
        go.Place(constructionTime, upgrades, startConstruction);
        AddBuildingPlaced(go);
    }
    public void RemoveBuilding(Building building)
    {
        for (int i = posOccupied.Count; --i >= 0;)
        {
            foreach (var pos in building.posOccupied) {
                if (posOccupied.ContainsKey(pos)) {
                    posOccupied.Remove(pos);
                }
            }
        }
        tilesOccupied -= building.size.x * building.size.y;
        GameManager.instance.RemoveBuilding(building);
    }
    public Building TryGetBuilding(Vector3 pos) {
        if (posOccupied.ContainsKey(pos)) {
            return posOccupied[pos];
        }
        return null;
    }
    //Modifiers...

    public PlacementPrefabs TryGetPlacementPrefab(BuildingType type) {
        if (placementPrefabs.ContainsKey(type)) {
            return placementPrefabs[type].item;
        }
        return null;
    }
    
    public void DecreaseConstructionTime(BuildingType type, float newAmount)
    {
        placementPrefabs[type].item.constructionTime = newAmount;
    }
    public void DecreaseConstructionCost(BuildingType type, ResourceType resource, int newAmount)
    {
        placementPrefabs[type].item.SetCost(resource, newAmount);
    }
    public float GetConstructionTime(BuildingType type)
    {
        if (!placementPrefabs.ContainsKey(type)) return 0;

        return placementPrefabs[type].item.constructionTime;
    }
    public float GetConstructionCost(BuildingType type, ResourceType resourceType)
    {
        if (!placementPrefabs.ContainsKey(type)) return 0;
        if (!placementPrefabs[type].item.cost.ContainsKey(resourceType)) return 0;

        return placementPrefabs[type].item.cost[resourceType];
    }
    public void AddUpgradeToPlacementPrefab(BuildingType type, BaseUpgrade upgrade)
    {
        if (!placementPrefabs.ContainsKey(type)) return;
        placementPrefabs[type].item.AddUpgrade(upgrade);
    }
    public void AddPlacementPrefab(BuildingType type, GameObject prefab) {
        var go = Instantiate(prefab, placementPrefabsTransform).GetComponent<UIPlacementPrefab>();
        go.Init(type, () => OnSelectPrefab(go.item));
        placementPrefabs.TryAdd(type, go);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(drawHit, drawSize);
    }
    #endregion
}

public enum BuildingType { 
    Houses,
    Farms,
    Industries,
    Roads,
    Gold,
    Pond,
    Environment,
    Boss,
    River
}

//Podria ser scriptable object...

[Serializable]
public class PlacementPrefabs  {
    public SerializedDictionary<ResourceType, int> cost = new SerializedDictionary<ResourceType, int>();
    public Building prefab;
    public float cooldown;
    public bool onCooldown = false;
    public float constructionTime;
    [SerializeReference] public List<BaseUpgrade> upgrades = new();
    public string description;
    [TextArea(1,5)]public string extraDescription;
    public BuildingType type;
    public bool isFree = false;

    public Action<PlacementPrefabs> OnChanged;

    public string GetDescription() {
        if (description == "")
        {
            description = $"Place a {type}\n" +
                $"Construction Time: {constructionTime}\n" +
                $"Cost: {GetCostToString()}";
        }
        return description + "\n" + extraDescription; ;

    }
    public void OnCooldown(bool on) {
        onCooldown = on;
    }
   
    public string GetCostToString() {
        string val = "";

        foreach (var c in cost.Keys) {
            val += $"{c} : {cost[c]}\n";
        }
        val = val.Trim();
        return val;
    }
    public void SetCost(ResourceType type, int newValue) { 
        if(cost.ContainsKey(type))
        {
            cost[type] = newValue;
            OnChanged?.Invoke(this);
        }
    }
    public void AddUpgrade(BaseUpgrade upgrade)
    {
        upgrades.Add(upgrade);
    }
    public bool HasEnough() {
        if (isFree) return true;

        return cost.Keys.All(x => GameManager.instance.HasResource(x, cost[x]));
    }
    public void ConsumePoints() {
        if (isFree) return;

        foreach (var k in cost.Keys)
        {
            GameManager.instance.ConsumeResource(k, cost[k]);
        }
    }
}


public interface IPlacementRequirement {
    bool Evaluate(PlacementManager placement, GameManager game, Building b, Vector3 point);
}
public class CrossCondition : IPlacementRequirement
{
    public BuildingType type;
    public bool Evaluate(PlacementManager placement, GameManager game, Building b, Vector3 point)
    {
        if (b.type != type) return true;
        if (game.GetBuildingCount(type) == 0) return true;
        Vector3[] arr = new Vector3[4] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

        for (int i = 0; i < 4; i++)
        {
            var v = placement.TryGetBuilding(point + arr[i]);
            if (v != null && v.type == type)
            {
                return true;
            }
        }
        return false;
    }
}
public class RoadsNear : IPlacementRequirement
{
    public bool Evaluate(PlacementManager placement, GameManager game, Building b, Vector3 point)
    {
        return Physics.OverlapBox(point, (Vector3.one + new Vector3(b.size.x, 0, b.size.y)) / 2f, Quaternion.identity, 1 << 10)
            .Any(x => x.GetComponent<Building>().type == BuildingType.Roads);
    }
}