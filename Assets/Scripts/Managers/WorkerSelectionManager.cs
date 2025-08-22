using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerSelectionManager : MonoBehaviour
{
    public static WorkerSelectionManager instance;

    public UIWorkerSelectionSimple uiWorkerSelectionSimplePrefab;

    public List<UIWorkerSelectionSimple> workers = new();

    public Transform selectionTransform;

    public UIWorkerSelectionDetailed uiWorkerSelectionDetailed;

    private UIWorkerSelectionSimple current = null;

    public static System.Action<WorkerSO> OnChooseWorker;

    public Button chooseButton;

   private void Awake()
   {
       instance = this;
        chooseButton.onClick.AddListener(Choose);
   }
    public void Select(UIWorkerSelectionSimple selected)
    {
        current = selected;
        uiWorkerSelectionDetailed.Set(selected.workerSo);
    }
    public void Choose() {
        if(current != null)
            OnChooseWorker?.Invoke(current.workerSo);
    }
    public void Next() { 
    }
    public void Previous() { 
    
    }
}
