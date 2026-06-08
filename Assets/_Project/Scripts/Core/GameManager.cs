using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private LevelConfig _levelConfig;

    private ScoreCalculator _scoreCalculator;

    public GameState CurrentState { get; private set; } = GameState.Briefing;

    public LevelConfig LevelConfig => _levelConfig;

    public int LastScore => _scoreCalculator != null ? _scoreCalculator.LastScore : 0;

    public Rank LastRank => _scoreCalculator != null ? _scoreCalculator.LastRank : Rank.None;

    public float LastTime => _scoreCalculator != null ? _scoreCalculator.LastTime : 0f;

    private void Awake()
    {
        _scoreCalculator = FindAnyObjectByType<ScoreCalculator>();
    }

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
