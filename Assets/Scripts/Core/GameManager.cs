using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    EventPopup,
    LevelComplete,
    GameOver
}

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private GameState initialState = GameState.Playing;

    private GameState _currentState;

    public static event Action<GameState, GameState> OnStateChanged;

    public static GameState CurrentState =>
        _instance != null ? _instance._currentState : GameState.Menu;

    public static bool IsPlaying =>
        _instance != null && _instance._currentState == GameState.Playing;

    private void OnEnable()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        SetState(initialState);
    }

    public static bool TryGetInstance(out GameManager manager)
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        }

        manager = _instance;
        return manager != null;
    }

    public static void SetState(GameState newState)
    {
        if (_instance == null)
        {
            Debug.LogWarning("GameManager instance not found.");
            return;
        }

        GameState previousState = _instance._currentState;
        if (previousState == newState)
        {
            return;
        }

        _instance._currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
            case GameState.EventPopup:
            case GameState.Menu:
            case GameState.LevelComplete:
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }

        OnStateChanged?.Invoke(previousState, newState);
    }

    public static void Pause()
    {
        if (CurrentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    public static void Resume()
    {
        if (CurrentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    public static void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
        else if (CurrentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }
}
