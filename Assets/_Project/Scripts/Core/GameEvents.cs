using System;

public static class GameEvents
{
    // To find where events are referenced, right-clicking on delegate function -> find all references. (VSCode)

    public static event Action<GameState> OnGameStateChanged;

    public static event Action OnPlayerDetected;

    public static event Action OnGoalReached;

    public static event Action OnAllLootCollected;

    public static event Action<int, int> OnLootCollected;

    public static event Action<float> OnTimerUpdated;

    public static event Action OnGuardNeutralized;

    public static event Action OnPanelDisabled;

    public static event Action OnPowerupCollected;

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

    public static void RaiseAllLootCollected()
    {
        OnAllLootCollected?.Invoke();
    }

    public static void RaiseLootCollected(int collectedCount, int totalCount)
    {
        OnLootCollected?.Invoke(collectedCount, totalCount);
    }

    public static void RaiseTimerUpdated(float elapsed)
    {
        OnTimerUpdated?.Invoke(elapsed);
    }

    public static void RaiseGuardNeutralized()
    {
        OnGuardNeutralized?.Invoke();
    }

    public static void RaisePanelDisabled()
    {
        OnPanelDisabled?.Invoke();
    }

    public static void RaisePowerupCollected()
    {
        OnPowerupCollected?.Invoke();
    }
}
