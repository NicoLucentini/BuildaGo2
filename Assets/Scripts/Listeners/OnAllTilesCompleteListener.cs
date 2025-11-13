using UnityEngine;
using UnityEngine.Events;

public class OnAllTilesCompleteListener : MonoBehaviour
{
    public UnityEvent action;
    private void Awake()
    {
        PlacementManager.AllTilesComplete += AllTilesComplete;
    }
    private void OnDisable()
    {
        PlacementManager.AllTilesComplete -= AllTilesComplete;
    }
    void AllTilesComplete() {
        Debug.Log("Heard All Tiles Complete in boss");
        action?.Invoke();
    }
}
