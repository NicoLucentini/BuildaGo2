using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public BaseBuildingBusListener baseBuildingBusListener;
    public GameObject canvas;
    private void OnEnable()
    {
        EventBus.Subscribe<TutorialStartEvent>(StartTutorial);
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<TutorialStartEvent>(StartTutorial);
    }
    public void OnEndTutorial() { 
        Destroy(baseBuildingBusListener);
        EventBus.Publish(new TutorialEndedEvent());
    }
    public void StartTutorial(TutorialStartEvent e) {
        Debug.Log("Start tutorial listened");
        canvas.SetActive(true);
    }
}
public class TutorialEndedEvent : IGameEvent { }
public class TutorialStartEvent : IGameEvent { }