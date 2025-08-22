using TMPro;
using UnityEngine;

public class UIBuildingLabor : MonoBehaviour
{
    public TextMeshProUGUI amount;
    public LaborType type;

    private void Awake()
    {
        amount = transform.Find("Amount").GetComponent<TextMeshProUGUI>();
        transform.Find("Type").GetComponent<TextMeshProUGUI>().text = type.ToString(); ;
    }
    public void AddListener(Data<int> target) {
        target.onValueChanged += UpdateAmountText;
    }
    public void RemoveListener(Data<int> target)
    {
        target.onValueChanged -= UpdateAmountText;
    }
    void UpdateAmountText(int x) {
        amount.text = x.ToString();
    }
}
