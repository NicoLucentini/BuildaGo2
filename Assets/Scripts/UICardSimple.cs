using System.Linq;
using TMPro;
using UnityEngine;

public class UICardSimple : MonoBehaviour
{
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardDescription;
    public TextMeshProUGUI cardEnergyCost;

    public CardSO cardSo;

    public void Set(CardSO cardSo) { 
        cardName.text = cardSo.cardName;
        cardEnergyCost.text = cardSo.energyCost.ToString();

        string generalDescription = string.Join(", ", cardSo.effects.Select(e => e.GetDescription()));
        cardDescription.text = cardSo.cardDescription + ".\n " + generalDescription;
    }
}
