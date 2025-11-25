using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIRewardManager : MonoBehaviour
{
    public static UIRewardManager instance;
    public UIReward timeRewardPrefab;
    public UIReward goldRewardPrefab;
    public UIReward buildingRewardPrefab;
    private void Awake()
    {
        instance = this;
    }
    private void OnEnable()
    {
        EventBus.Subscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.Subscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.Subscribe<BuildingSameTypeEvent>(OnBuildingSameType);
        EventBus.Subscribe<BuildingNearRiverEvent>(OnBuildingNearRiver);
    }

    

    private void OnDisable()
    {
        EventBus.UnSubscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.UnSubscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.UnSubscribe<BuildingSameTypeEvent>(OnBuildingSameType);
        EventBus.UnSubscribe<BuildingNearRiverEvent>(OnBuildingNearRiver);
    }
    private void OnBuildingNearRiver(BuildingNearRiverEvent e)
    {
        var pos = e.building.transform.position;
        switch (e.building.type) {
            case BuildingType.Houses: CreateGoldReward(pos.WithOffset(new Vector3(-0.5f, .5f, 0)), Color.white, e.amount.ToString()); break;
            case BuildingType.Farms: CreateTimerReward(pos.WithOffset(new Vector3(0.5f, .5f, 0)), Color.black, e.amount.ToString()); break;
            case BuildingType.Industries: CreateGoldReward(pos.WithOffset(new Vector3(-0.5f, .5f, 0)), Color.white, (-e.amount).ToString()); break;
        }
    }
    private void OnBuildingSameType(BuildingSameTypeEvent e)
    {
        var pos = e.building.transform.position;
        CreateBuildingReward(
             pos.WithOffset(new Vector3(0f, .5f, 0)),
            e.building.GetComponent<MeshRenderer>().material.color,
                 e.amount.ToString());
    }

    private void OnBuildingTrioEvent(BuildingTrioEvent e)
    {
        var pos = e.building.transform.position;
        CreateGoldReward(pos.WithOffset(new Vector3(-0.5f, .25f, 0)),Color.white, e.goldReward.ToString());
        CreateTimerReward(pos.WithOffset(new Vector3(0.5f, .25f, 0)), Color.black, e.timeReward.ToString());
    }

    private void OnBuildingFinishedEvent(BuildingFinishedEvent e)
    {
        var pos = e.building.transform.position;
        switch (e.building.type)
        {
            case BuildingType.Houses:
                CreateBuildingReward(
             pos.WithOffset(new Vector3(0f, .25f, 0)),
            e.building.GetComponent<MeshRenderer>().material.color,
                 "1".ToString()); ; break;

            case BuildingType.Farms:
                CreateBuildingReward(
            pos.WithOffset(new Vector3(0f, .25f, 0)),
            e.building.GetComponent<MeshRenderer>().material.color,
                 "1".ToString()); 
                CreateTimerReward(pos.WithOffset(new Vector3(0.25f, .25f, 0)), Color.black, "1".ToString()); ; break;

            case BuildingType.Industries:
                CreateBuildingReward(
            pos.WithOffset(new Vector3(0f, .25f, 0)),
            e.building.GetComponent<MeshRenderer>().material.color,
                 "1".ToString());
                CreateGoldReward(pos.WithOffset(new Vector3(-0.25f, .25f, 0)), Color.white, "1".ToString()); ; break;

            default: break;
        }
    }
    public void CreateGoldReward(Vector3 pos, Color color, string message = "") {
        CreateReward(goldRewardPrefab, pos, color, message);
    }
    public void CreateBuildingReward(Vector3 pos, Color color, string message = "")
    {
        CreateReward(buildingRewardPrefab, pos, color, message);
    }
    public void CreateTimerReward(Vector3 pos, Color color, string message = "")
    {
        CreateReward(timeRewardPrefab, pos, color, message);
    }
    public void CreateReward(UIReward prefab, Vector3 pos, Color color, string message = "" ) {
        var reward = Instantiate(prefab);
        reward.Set(message, color, pos);
    }
}
