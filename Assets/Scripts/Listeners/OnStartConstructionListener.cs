using UnityEngine;
using UnityEngine.Events;

public class OnStartConstructionListener : MonoBehaviour
{
    public UnityEvent<float> unityEventFloat;
      
    private void OnEnable()
    {
        Building.OnStartConstruction += OnStartConstruction;
    }
    private void OnDisable()
    {
        Building.OnStartConstruction -= OnStartConstruction;
    }

    private void OnStartConstruction(Building building, float duration)
    {
        if (building.type != BuildingType.Boss) return;

        new Timer("OnStartConstruction" + gameObject.GetInstanceID(), duration,  null, (x) => unityEventFloat?.Invoke(x/duration)).Start();
    }
   
}
