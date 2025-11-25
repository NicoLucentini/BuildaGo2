using UnityEngine;
using UnityEngine.Events;

public class AddPointsListener : MonoBehaviour
{
    public UnityEvent action;

    public BuildingType targetPoints;
    private void OnEnable()
    {
        EventBus.Subscribe<PointsAddedEvent>(OnAddedPoints);
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<PointsAddedEvent>(OnAddedPoints);
    }

    private void OnAddedPoints(PointsAddedEvent e)
    {
        if(targetPoints == e.type)
        action?.Invoke();
    }
}
