using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnPlayerDetected;
    public static event Action OnGoalReached;
    public static event Action OnAllLootCollected;
    public static event Action<int, int> OnLootCollected;
    public static event Action<float> OnTimerUpdated;
    public static event Action OnTapMove;
    public static event Action OnGuardNeutralized;
    public static event Action OnPanelDisabled;
    public static event Action OnPowerupCollected;
    public static event Action<Rank> OnRankRevealed;

    public static void RaiseGameStateChanged(GameState state)
    {
        OnGameStateChanged?.Invoke(state);
    }

    public static void RaisePlayerDetected()
    {
        OnPlayerDetected?.Invoke();
        if (OnPlayerDetected == null)
        {
            Debug.Log("No subscribers");
            return;
        }

        foreach (Delegate d in OnPlayerDetected.GetInvocationList())
        {
            // d.Target is the subscribing object (null if a static method).
            // d.Method.Name is the handler method's name.
            string owner = d.Target switch
            {
                UnityEngine.Object o => o.name, // e.g. the GameObject/component name
                null => "static method",
                _ => d.Target.GetType().Name
            };
            Debug.Log($"{owner} → {d.Method.Name}");
        }
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

    public static void RaiseTapMove()
    {
        OnTapMove?.Invoke();
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

    public static void RaiseRankRevealed(Rank rank)
    {
        OnRankRevealed?.Invoke(rank);
    }
}
