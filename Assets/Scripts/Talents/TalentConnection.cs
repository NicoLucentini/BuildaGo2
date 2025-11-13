using UnityEngine;
using UnityEngine.UI.Extensions;

public class ItemConnection : MonoBehaviour {
    public UILineRenderer lineRenderer;
    public UITalentItem from;
    public UITalentItem to;
    public void Set(UITalentItem from, UITalentItem to)
    { 
        this.to = to;
        this.from = from;

        Vector2 start = GetAnchoredPosition(from.GetComponent<RectTransform>());
        Vector2 end = GetAnchoredPosition(to.GetComponent<RectTransform>());

        lineRenderer.Points = new Vector2[] { start, end };
    }
    Vector2 GetAnchoredPosition(RectTransform rect)
    {
        // Convert world to local canvas space
        RectTransform canvasRect = rect.parent.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, rect.position), null, out localPoint);
        return localPoint;
    }

}