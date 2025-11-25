using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    [Header("Default shake")]
    [SerializeField] float defaultDuration = 0.35f;
    [SerializeField] float defaultMagnitude = 0.35f;
    [SerializeField] bool useLocalPosition = true;

    Transform camTransform;
    Vector3 initialPos;
    Coroutine runningShake;


    void Awake()
    {
        instance = this;
        camTransform = transform;
        initialPos = useLocalPosition ? camTransform.localPosition : camTransform.position;
    }

    /// <summary>Use defaultDuration/defaultMagnitude</summary>
    public void Shake() => Shake(defaultDuration, defaultMagnitude);

    /// <summary>Start a shake. Interrupts any running shake.</summary>
    public void Shake(float duration, float magnitude)
    {
        if (runningShake != null) StopCoroutine(runningShake);
        runningShake = StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        initialPos = useLocalPosition ? camTransform.localPosition : camTransform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float damper = 1f - Mathf.Clamp01(elapsed / duration); // optional fade out
            Vector3 offset = Random.insideUnitSphere * magnitude * damper;

            if (useLocalPosition)
                camTransform.localPosition = initialPos + new Vector3(offset.x, offset.y, 0f);
            else
                camTransform.position = initialPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // restore exact initial position
        if (useLocalPosition) camTransform.localPosition = initialPos;
        else camTransform.position = initialPos;

        runningShake = null;
    }

    /// <summary>Stop the current shake immediately and restore position</summary>
    public void StopShake()
    {
        if (runningShake != null)
        {
            StopCoroutine(runningShake);
            runningShake = null;
        }

        if (useLocalPosition) camTransform.localPosition = initialPos;
        else camTransform.position = initialPos;
    }
}
