using UnityEngine;
using UnityEngine.Events;

public class BaseBuildingBusListener : MonoBehaviour
{
    public UnityEvent<BuildingTrioEvent> trioEvent;
    public UnityEvent<BuildingSameTypeEvent> sameTypeEvent;
    public UnityEvent<BuildingFinishedEvent> finishedEvent;
    public UnityEvent<BuildingQuad> quadEvent;
    public Toggleable<UnityEvent<BuildingFourInLine>> fourInLineEvent;
    [Toggleable]public UnityEvent<BuildingNearRiverEvent> nearRiverEvent;
    private void OnEnable()
    {
        if(trioEvent.GetPersistentEventCount() > 0)
            EventBus.Subscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        if (sameTypeEvent.GetPersistentEventCount() > 0)
            EventBus.Subscribe<BuildingSameTypeEvent>(OnBuildingSameTypeEvent);
        if (finishedEvent.GetPersistentEventCount() > 0)
            EventBus.Subscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        if (quadEvent.GetPersistentEventCount() > 0)
            EventBus.Subscribe<BuildingQuad>(OnBuildingQuadEvent);
        if (fourInLineEvent.enabled)
            EventBus.Subscribe<BuildingFourInLine>(OnBuildingFourInLineEvent);
        if (nearRiverEvent.GetPersistentEventCount() > 0)
            EventBus.Subscribe<BuildingNearRiverEvent>(OnBuildingNearRiverEvent);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<BuildingTrioEvent>(OnBuildingTrioEvent);
        EventBus.UnSubscribe<BuildingSameTypeEvent>(OnBuildingSameTypeEvent);
        EventBus.UnSubscribe<BuildingFinishedEvent>(OnBuildingFinishedEvent);
        EventBus.UnSubscribe<BuildingQuad>(OnBuildingQuadEvent);
        EventBus.UnSubscribe<BuildingFourInLine>(OnBuildingFourInLineEvent);
        EventBus.UnSubscribe<BuildingNearRiverEvent>(OnBuildingNearRiverEvent);
    }

    private void OnBuildingNearRiverEvent(BuildingNearRiverEvent e)
    {
        Debug.Log(e.GetType().ToString() + " Listened");
        nearRiverEvent?.Invoke(e);
    }

    private void OnBuildingFourInLineEvent(BuildingFourInLine e)
    {
        Debug.Log(e.GetType().ToString() + " Listened");
        fourInLineEvent?.value.Invoke(e);
    }

    private void OnBuildingQuadEvent(BuildingQuad e)
    {
        Debug.Log(e.GetType().ToString() + " Listened");
        quadEvent?.Invoke(e);
    }

    private void OnBuildingFinishedEvent(BuildingFinishedEvent e)
    {
        Debug.Log(e.GetType().ToString() + " Listened");
        finishedEvent?.Invoke(e);
    }

    private void OnBuildingTrioEvent(BuildingTrioEvent e)
    {
        Debug.Log(e.GetType().ToString() + " Listened");
        trioEvent?.Invoke(e);
    }
    private void OnBuildingSameTypeEvent(BuildingSameTypeEvent e)
    {
        Debug.Log(e.GetType().ToString() + " Listened");
        sameTypeEvent?.Invoke(e);
    }


}
