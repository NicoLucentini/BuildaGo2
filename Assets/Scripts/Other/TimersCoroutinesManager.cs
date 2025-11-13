using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimersCoroutinesManager : MonoBehaviour
{
    private static TimersCoroutinesManager _instance;

    /// <summary>
    /// Singleton of the Instance
    /// </summary>
    public static TimersCoroutinesManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("TimersCoroutinesManager").AddComponent<TimersCoroutinesManager>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }
    private void OnDestroy()
    {
        _instance = null;
    }
    private readonly Dictionary<string, Coroutine> timers = new Dictionary<string, Coroutine>();

    public IEnumerator TimerCoroutine(Timer timer) {
        float time = 0;
        while (time < timer.duration) {
            time += Time.deltaTime;
            timer.onTimeElapsed?.Invoke(time);
            yield return null;
        }
        timer.onComplete?.Invoke();
        timers.Remove(timer.name);
    }
    public void Play(Timer timer) {
        Stop(timer.name);

        timers.Add(timer.name, StartCoroutine(TimerCoroutine(timer)));
    }
    public void Stop(string name) { 
        if (timers.TryGetValue(name, out Coroutine value))
        {
            if(value != null)
                StopCoroutine(value);
            timers.Remove(name);
        }
    }
    public void EndAllCoroutines()
    {
        StopAllCoroutines();
        timers.Clear();
    }

}
[SerializeField]
public class Timer {

    public string name;
    public float duration;
    public Action onComplete;
    public Action<float> onTimeElapsed;
    public Timer(string name, float duration, Action onComplete, Action<float> onTimeElapsed = null, bool playNow = false) { 
        this.name = name;
        this.duration = duration;
        this.onComplete = onComplete; 
        this.onTimeElapsed = onTimeElapsed;
        if(playNow)
        {
            Start();
        }
        
    }
    public void Start() {
        TimersCoroutinesManager.instance.Play(this);
    }
}
