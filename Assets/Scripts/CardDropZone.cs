using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("On Drop");
        if (eventData.pointerDrag.TryGetComponent(out Card card)) {

            if (GameManager.instance.HasEnergy(card.energyCost.Value) && !card.blocked)
            {
                card.droppedOnValidTarget = true;
                card.ApplyEffects();
            }
            else {
                Debug.Log("Not Enough Energy");
                card.droppedOnValidTarget = false;
            }
            card.OnEndDragActions();
        }

    }
}
