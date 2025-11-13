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
    public Vector2Int gridSize = new Vector2Int(20, 20);

    public GameObject gridModel;
    public Building environmentPrefab;
    public float percentageOfEnvironment;

    //public SerializedDictionary<BuildingType, PlacementPrefabs> placementPrefabs;
    public SerializedDictionary<BuildingType, UIPlacementPrefab> placementPrefabs;

    PlacementPrefabs current;
    private Building prefabVisual;

    public Building boss;
    public int tilesOccupied = 0;
    public List<Vector3> posOccupied = new List<Vector3>();
    public Transform placementPrefabsTransform;

    public Vector2 gridOffset = new Vector2(0, 0);
    public Vector3 gridCenter = new Vector3(0, 0, 0);
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        placementPrefabs.ToList().ForEach(x => {
            x.Value.item.type = x.Key;
            x.Value.button.onClick.AddListener(() => OnSelectPrefab(x.Value.item));
            x.Value.button.gameObject.AddComponent<HoverDetector>().Set(
                ()=>TooltipSystem.instance.ShowWithOffset(x.Value.item.GetDescription(), x.Value.button.GetComponent<RectTransform>(), new Vector3(25, 150, 0)),
                ()=>TooltipSystem.instance.Hide());
            x.Value.UpdateUI(x.Value.item);
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
    }
    void OnStartGame() {

        gridSize = GameManager.instance.levelConfiguration.gridSize;
        percentageOfEnvironment = GameManager.instance.levelConfiguration.environmentAmount;

        DoGrid();
        tilesOccupied = 0;
        posOccupied.Clear();
        DoBoss();
        DoEnvironment();
    }
    void DoGrid()
    {
        gridModel.transform.localScale = new Vector3(gridSize.x, 0.1f, gridSize.y);

        float moveX = gridSize.x % 2 == 0 ? 0 : 0.5f;
        float moveY = gridSize.y % 2 == 0 ? 0 : 0.5f;
        gridOffset = new Vector2(moveX, moveY);

        gridCenter = new Vector3(gridSize.x / 2f, 0, gridSize.y / 2f);

        //gridModel.transform.position += new Vector3(gridOffset.x, 0, gridOffset.y) ;
        gridModel.transform.position = new Vector3(gridCenter.x, 0, gridCenter.z);

        OnGridReplaced?.Invoke(gridModel.transform);
        gridModel.GetComponent<MeshRenderer>().material.mainTextureScale = gridSize / 2;
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
            while (posOccupied.Exists(x => x.Equals(GetCorrectedPointForBuilding( ToGridPosition(new Vector3(vector.x, 0.5f, vector.y)), environmentPrefab.size)))
            && !IsGridComplete()
            );

            PlaceEnvironment(environmentPrefab, GetCorrectedPointForBuilding(ToGridPosition(new Vector3(vector.x, 0, vector.y)),environmentPrefab.size));
            amount--;
        }
    }
    void DoBoss()
    {
        boss = GameManager.instance.levelConfiguration.bossPrefab;
        PlaceBoss(boss, GetCorrectedPointForBuilding( ToGridPosition(new Vector3(gridSize.x / 2f, 0, gridSize.y / 2f)), boss.size));
    }
    
    void AddBuildingPlaced(Building building) {
        posOccupied.AddRange(GetPosWithSize(building.transform.position, building.size));
        int buildingArea = building.size.x * building.size.y;
        tilesOccupied += buildingArea;

    }
    List<Vector3> GetPosWithSize(Vector3 pos, Vector2Int size) { 
        var list =new List<Vector3>();
        Vector2Int initX = GetWithSize(size.x);
        Vector2Int initY = GetWithSize(size.y);

        float offsetX = size.x % 2 == 0 ? -0.5f : 0;
        float offsetY = size.y % 2 == 0 ? -0.5f : 0;

        for (int i = initX.x; i < initX.y; i++) {
            for (int j = initY.x; j < initY.y; j++)
            { 
                list.Add(new Vector3(pos.x + i + offsetX, pos.y, pos.z + j + offsetY) );
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

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1<<6))
        {
            var gridPoint = ToGridPosition(hit.point);
            
            var gridPointForBuilding = GetCorrectedPointForBuilding(gridPoint, prefabVisual.size);

            if (prefabVisual != null)
            {   
                prefabVisual.transform.position = gridPointForBuilding;
            }
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

    public Vector3 ToGridPosition(Vector3 hit) {
        var x = Mathf.FloorToInt(hit.x);
        var y = Mathf.FloorToInt(hit.z);
        return new Vector3(x+ cellSize / 2 , 0.5f, y + cellSize / 2 );
    }
    //Despues lo veo
    public Vector3 GetCorrectedPointForBuilding(Vector3 gridPoint, Vector2Int size)
    {
        float moveX = size.x % 2 == 0 ? -0.5f : 0f;
        float moveY = size.y % 2 == 0 ? -0.5f : 0f;

        if (gridPoint.x + size.x / 2f > gridSize.x)
        {
            moveX = gridSize.x - (gridPoint.x + size.x / 2f);
        }
        else if (gridPoint.x - size.x / 2f < 0)
        {
            moveX = size.x / 2f - 0.5f;
        }
        if (gridPoint.z + size.y / 2f > gridSize.y)
        {
            moveY = gridSize.y - (gridPoint.z + size.y / 2f);
        }
        else if (gridPoint.z - size.y / 2f < 0)
        {
            moveY = size.y / 2f - 0.5f;
        }
        return gridPoint + new Vector3(moveX, 0, moveY);
    }
    
    public bool OnGroundClick(Vector3 point)
    {
        if (prefabVisual == null || current == null) return false;
        if (current.onCooldown) return false;
        if (!GameManager.instance.HasGold(current.goldCost)) return false;
        if (!IsPlaceFreeAndObjectIsOnGrid(point, prefabVisual.size)) return false;
        if (!AreNeighboursOfDifferentType(point)) return false;


        PlaceBuildingFromLowBar(point, current.type);
        GameManager.instance.UseGold(current.goldCost);
        current.OnCooldown(true);
        new Timer("BuildingCooldown" + gameObject.GetInstanceID(), current.cooldown, () => current.OnCooldown(false)).Start();
        return true;
    }
    public bool IsPlaceFreeAndObjectIsOnGrid(Vector3 gridPointCorrected, Vector2Int size) {
        if (!IsPlaceFree(gridPointCorrected, size)) { Debug.Log("Place is Occupied"); return false; }
        if (!IsObjectOnGrid(gridPointCorrected,size)) { Debug.Log("Object Outside grid"); return false; };
        return true;
    }
    
    bool IsPlaceFree(Vector3 hit, Vector2Int size)
    {
        drawHit = hit;
        drawSize = new Vector3(size.x, 1, size.y) * 0.8f;

        return Physics.OverlapBox(hit, new Vector3(size.x, 1, size.y) * 0.4f, Quaternion.identity, 1 << 10)
            .Select(x => x.GetComponent<Building>())
            .Count(x => x.constructionStatus != ConstructionStatus.VISUAL) == 0 && IsOnGrid(hit);
    }
    private bool IsOnGrid(Vector3 hit) =>  hit.x > 0 && hit.x < gridSize.x && hit.z > 0 && hit.z < gridSize.y;

  
    void CheckForAllTiles(Building building) {
        new Timer("Check Fo All", 1f, CheckForAllTilesCompleteExceptBoss).Start();
    }
    bool IsGridComplete() => tilesOccupied == gridSize.x * gridSize.y;
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
   
    private Vector3 drawHit;
    private Vector3 drawSize;
   
    private bool IsObjectOnGrid(Vector3 hit, Vector2Int buildingSize) {
        var pos = GetPosWithSize(hit, buildingSize);
        bool isOnGrid = true;
        for (int i = 0; i < pos.Count; i++) {
            isOnGrid = IsOnGrid(pos[i]);
            if (!isOnGrid)
            {
                Debug.Log("Object outside grid");
                return false;
            }
        }
        return true;
    }
    
    bool AreNeighboursOfDifferentType(Vector3 hit) {
        Collider[] colliders = Physics.OverlapSphere(hit, 1, 1 << 10);
        var t = prefabVisual.GetComponent<Building>().type;
        return !(colliders.Length == 9 && colliders.Select(x => x.gameObject.GetComponent<Building>()).All(x => x.type == t));
    }


    void PlaceEnvironment(Building prefab, Vector3 point)
    {
        var go = Instantiate(prefab);
        go.transform.position = point;
        go.Place(go.constructionTime, null);
        AddBuildingPlaced(go);
    }

    void PlaceBoss(Building prefab, Vector3 point)
    {
        var go = Instantiate(prefab);
        go.transform.position = point;
        go.Place(go.constructionTime, null, false);
        AddBuildingPlaced(go);
    }
    //....

    #region PUBLIC METHODS
    public void PlaceBuildingFromLowBar(Vector3 point, BuildingType type)
    {
        var pref = placementPrefabs[type];
        var go = Instantiate(pref.item.prefab);
        //go.transform.position = GetCorrectedPointForBuilding(point, go.size); // ya esta corregido
        go.transform.position = point;
        go.Place(pref.item.constructionTime, pref.item.upgrades);
        AddBuildingPlaced(go);

    }
    public void DecreaseConstructionTime(BuildingType type, float newAmount)
    {
        placementPrefabs[type].item.constructionTime = newAmount;
    }
    public void DecreaseConstructionCost(BuildingType type, int newAmount)
    {
        placementPrefabs[type].item.SetGold(newAmount);
    }
    public float GetConstructionTime(BuildingType type)
    {
        if (!placementPrefabs.ContainsKey(type)) return 0;

        return placementPrefabs[type].item.constructionTime;
    }
    public float GetConstructionCost(BuildingType type)
    {
        if (!placementPrefabs.ContainsKey(type)) return 0;

        return placementPrefabs[type].item.goldCost;
    }
    public void AddUpgradeToPlacementPrefab(BuildingType type, BaseUpgrade upgrade)
    {
        if (!placementPrefabs.ContainsKey(type)) return;
        placementPrefabs[type].item.AddUpgrade(upgrade);
    }
    public void AddPlacementPrefab(BuildingType type, GameObject prefab) {
        var go = Instantiate(prefab, placementPrefabsTransform).GetComponent<UIPlacementPrefab>();

        go.item.type = type;
        go.button.onClick.AddListener(() => OnSelectPrefab(go.item));
        go.button.gameObject.AddComponent<HoverDetector>().Set(
            () => TooltipSystem.instance.ShowWithOffset(go.item.GetDescription(), go.button.GetComponent<RectTransform>(), new Vector3(25, 150, 0)),
            () => TooltipSystem.instance.Hide());
        go.UpdateUI(go.item);
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
    Housing,
    Farm,
    Industries,
    Road,
    Gold,
    Pond,
    Environment,
    Boss
}

//Podria ser scriptable object...

[Serializable]
public class PlacementPrefabs {
    public Building prefab;
    public float cooldown;
    public bool onCooldown = false;
    public float constructionTime;
    public int goldCost;
    [SerializeReference]public List<BaseUpgrade> upgrades = new();
    public string description;
    public BuildingType type;

    public Action<PlacementPrefabs> OnChanged;
    public string GetDescription() {
        if(description == "")
        { 
            description = $"Place a {type}\n" +
                $"Construction Time: {constructionTime} \n" +
                $"Cost: {goldCost}";
        }
        return description;

    }
    public void OnCooldown(bool on) {
        onCooldown = on;
    }
    public void SetGold(int goldAmount) {
        this.goldCost = goldAmount;
        OnChanged?.Invoke(this);
    }
    public void AddUpgrade(BaseUpgrade upgrade)
    {
        upgrades.Add(upgrade);
    }
   
}
/*
public class Condition {
    public Func<bool> pred = ()=> true;
    bool and = false;
    Condition next;

    public Condition(Func<bool> pred) {
        this.pred = pred;
    }
    public Condition And(Condition condition) {
        next = condition;
        and = true;
        return this;
    }
    public Condition Or(Condition condition)
    {
        next = condition;
        and = false;
        return this;
    }
    public Condition And(Func<bool> condition) { 
        return And(new Condition(condition));
    }
    public Condition Or(Func<bool> condition) {
        return Or(new Condition(condition));
    }
   
    public bool Evaluate() {
        if (next != null)
        {
            return and ? pred.Invoke() && next.Evaluate() : pred.Invoke() || next.Evaluate();
        }
        else {
            return pred.Invoke();
        }
    }
}
*/