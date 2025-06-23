using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string message;
    public Vector3 offsetInPixels;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.instance.Show(message, transform.position + offsetInPixels);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.instance.Hide();
    }
}