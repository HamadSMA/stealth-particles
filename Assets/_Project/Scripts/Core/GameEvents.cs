using System;

public static class GameEvents
{
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnPlayerDetected;
    public static event Action OnGoalReached;
    public static event Action<float> OnTimerUpdated;

    public static void RaiseGameStateChanged(GameState state)
    {
        OnGameStateChanged?.Invoke(state);
    }

    public static void RaisePlayerDetected()
    {
        OnPlayerDetected?.Invoke();
    }

    public static void RaiseGoalReached()
    {
        OnGoalReached?.Invoke();
    }

    public static void RaiseTimerUpdated(float elapsed)
    {
        OnTimerUpdated?.Invoke(elapsed);
    }
}
