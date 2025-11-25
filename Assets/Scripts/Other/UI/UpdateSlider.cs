using UnityEngine;
using UnityEngine.UI;

public class UpdateSlider : MonoBehaviour
{
    public Image imageToUpdate;

    public void UpdateImage(float fill) {
        imageToUpdate.fillAmount = fill;
    }
}
