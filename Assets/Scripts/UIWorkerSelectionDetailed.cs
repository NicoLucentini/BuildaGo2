using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkerSelectionDetailed : MonoBehaviour
{
    public TextMeshProUGUI workerName;
    public TextMeshProUGUI workerDescription;
    public TextMeshProUGUI workerEnergy;
    public TextMeshProUGUI workerCost;
    public Image workerPortrait;

    public UICardSimple uiCardSimplePrefab;
    public Transform skillsTransform;

    [Header("Status")]
    public WorkerSO workerSo;

    private List<GameObject> skillsCards = new();

    public void Set(WorkerSO workerSo) { 
        this.workerSo = workerSo;

        workerName.text = workerSo.workerName;
        workerDescription.text = workerSo.workerDescription;
        workerEnergy.text = "Energy: " + workerSo.energy.ToString();
        workerCost.text = "Cost: " + workerSo.weeklyCost.ToString();
        workerPortrait.sprite = workerSo.portrait;

        CleanSkillCards();

        foreach (var card in workerSo.specialCards) {
            var skillCard = Instantiate(uiCardSimplePrefab, skillsTransform);
            skillCard.Set(card);
            skillsCards.Add(skillCard.gameObject);
        }
      
    }
    void CleanSkillCards() {
        foreach (var go in skillsCards)
        {
            Destroy(go);
        }
        skillsCards.Clear();

    }
}
