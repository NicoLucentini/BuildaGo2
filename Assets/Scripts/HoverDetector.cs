using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class HoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Action OnEnter;
    Action OnExit;
    public UnityEvent OnEnterUnityEvent;
    public UnityEvent OnExitUnityEvent;
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke();
        OnEnterUnityEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke();
        OnExitUnityEvent?.Invoke();
    }

    public void Set(Action OnEnter, Action OnExit) { 
        this.OnEnter = OnEnter;
        this.OnExit = OnExit;
    }
}
