using TMPro;
using UnityEngine;

public class UIWorkerSelection : MonoBehaviour
{
    public TextMeshProUGUI workerNameText;
    public TextMeshProUGUI energyCostText;
    public TextMeshProUGUI descriptionText;

    private WorkerSO workerSo;
    private void Awake()
    {
        if(workerSo != null)
            Set(workerSo);
    }
    public void Set(WorkerSO workerSo) {
        this.workerSo = workerSo;
        workerNameText.text = workerSo.workerName;
        energyCostText.text = workerSo.energy.ToString();
    }
}
