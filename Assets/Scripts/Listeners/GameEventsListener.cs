using System;
using UnityEngine;
using UnityEngine.Events;

public class GameEventsListener : MonoBehaviour
{
    public UnityEvent OnGameEndAction;
    public UnityEvent OnGameStartAction;

    private void OnEnable()
    {
        GameManager.OnGameEnd += OnGameEnd;
        GameManager.OnStartGame += OnStartGame;
    }
    private void OnDisable()
    {
        GameManager.OnGameEnd -= OnGameEnd;
        GameManager.OnStartGame -= OnStartGame;
    }

    private void OnGameEnd()
    {
        OnGameEndAction?.Invoke();
    }

    private void OnStartGame()
    {
        OnGameStartAction?.Invoke();
    }
}
