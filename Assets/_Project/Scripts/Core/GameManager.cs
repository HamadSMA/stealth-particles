using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private LevelConfig _levelConfig;

    public GameState CurrentState { get; private set; } = GameState.Briefing;

    public LevelConfig LevelConfig => _levelConfig;

    private void OnEnable()
    {
        GameEvents.OnGoalReached += HandleGoalReached;
        GameEvents.OnPlayerDetected += HandlePlayerDetected;
    }

    private void OnDisable()
    {
        GameEvents.OnGoalReached -= HandleGoalReached;
        GameEvents.OnPlayerDetected -= HandlePlayerDetected;
    }

    public void StartLevel()
    {
        TransitionTo(GameState.Playing);
    }

    private void HandleGoalReached()
    {
        TransitionTo(GameState.Success);
    }

    private void HandlePlayerDetected()
    {
        TransitionTo(GameState.Fail);
    }

    private void TransitionTo(GameState newState)
    {
        if (newState == CurrentState)
        {
            return;
        }

        if (!IsValidTransition(CurrentState, newState))
        {
            Debug.LogWarning(
                $"[GameManager] Ignored invalid state transition {CurrentState} -> {newState}."
            );
            return;
        }

        CurrentState = newState;
        GameEvents.RaiseGameStateChanged(newState);
    }

    private static bool IsValidTransition(GameState from, GameState to)
    {
        switch (from)
        {
            case GameState.Briefing:
                return to == GameState.Playing;
            case GameState.Playing:
                return to == GameState.Success || to == GameState.Fail;
            case GameState.Success:
            case GameState.Fail:
                return false;
            default:
                return false;
        }
    }
}
