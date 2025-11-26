using System;
using UnityEngine;
using UnityEngine.Events;

public class AddPointsListener : MonoBehaviour
{
    public UnityEvent action;

    public ResourceType resourceType;
    private void OnValidate()
    {
        Debug.Log("AddPointsListener in " + gameObject.name);
    }
    private void OnEnable()
    {
        EventBus.Subscribe<ResourceChangedEvent>(OnResourceChanged);
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<ResourceChangedEvent>(OnResourceChanged);
    }

    private void OnResourceChanged(ResourceChangedEvent e)
    {
        if (resourceType == e.type)
            action?.Invoke();
    }
}
