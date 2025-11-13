using UnityEngine;
using UnityEngine.Events;

public class OnGridReplacedListener : MonoBehaviour
{
    public UnityEvent<Transform> unityEvent;
    private void OnEnable()
    {
        PlacementManager.OnGridReplaced += OnGridReplaced;
    }
    private void OnDisable()
    {
        PlacementManager.OnGridReplaced -= OnGridReplaced;
    }
    private void OnGridReplaced(Transform target)
    {
        unityEvent?.Invoke(target);
    }

   
}
