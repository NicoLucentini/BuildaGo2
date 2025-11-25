using UnityEngine;

public class UIAnimation :MonoBehaviour{

    public Vector3 initScale;
    public float targetScale;
    private void Awake()
    {
        initScale = transform.localScale;
    }
    public void TriggerAnim()
    {
        new Timer("UIAnimation" + gameObject.GetInstanceID(), 0.5f, ResetScale, OnDuration, true);
    }

    private void OnDuration(float timeElapsed)
    {
        transform.localScale = Vector3.Slerp(initScale, initScale * targetScale, timeElapsed / 0.5f);
    }

    void ResetScale() {
        transform.localScale = initScale;
    }

}