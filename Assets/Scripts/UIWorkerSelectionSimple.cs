using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIWorkerSelectionSimple : MonoBehaviour , IPointerClickHandler
{
    public WorkerSO workerSo;

    public TextMeshProUGUI workerName;
    public Image workerPortrait;
    void Awake() {
        if (workerSo != null)
        {
            Set(workerSo);
        }
    }
    public void Set(WorkerSO workerSo) { 
        this.workerSo = workerSo;
        workerName.text = workerSo.workerName;
        workerPortrait.sprite = workerSo.portrait;
    }
    public void Select()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        WorkerSelectionManager.instance.Select(this);
    }
}
