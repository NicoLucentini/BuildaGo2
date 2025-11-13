using UnityEngine;
using UnityEngine.Events;

public class BaseBuildingBusListener : MonoBehaviour
{
    public UnityEvent<BuildingTrioEvent> trioEvent;
    public UnityEvent<BuildingSameTypeEvent> sameTypeEvent;

    private void OnEnable()
    {
        EventBus.Subscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.Subscribe<BuildingSameTypeEvent>(OnBuildingSameTypeEvent);
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.UnSubscribe<BuildingSameTypeEvent>(OnBuildingSameTypeEvent);
    }
    private void OnBuildingTrioEvent(BuildingTrioEvent e)
    {
        trioEvent?.Invoke(e);
    }
    private void OnBuildingSameTypeEvent(BuildingSameTypeEvent e)
    {
        Debug.Log("OnBuildingSameTypeEvent listened");
        sameTypeEvent?.Invoke(e);
    }


}
