using System;
using UnityEngine;

public static class GameEvents
{
    // I use VSCode, there you can find all scripts that subscribe to one of these events by right-clicking -> find all references.

    // I will still mention the subbed scripts for quick access.

    // AudioManager.cs | EventLogger.cs | Goal.cs | LevelTimer.cs | ScoreCalculator.cs | GuardController.cs | Laser.cs | Panel.cs | Loot.cs LootManager.cs | PlayerContactDetector.cs | PlayerMovement.cs | Powerup.cs | PowerupSystem.cs | UIController.cs
    public static event Action<GameState> OnGameStateChanged;

    // DetectionVegnetteFlash.cs | EventLogger.cs | GameManager.cs
    public static event Action OnPlayerDetected;

    // EventLogger.cs | GameManager.cs
    public static event Action OnGoalReached;

    // Goal.cs
    public static event Action OnAllLootCollected;

    // SfxPlayer.cs
    public static event Action<int, int> OnLootCollected;

    // EventLogger.cs | UIController.cs
    public static event Action<float> OnTimerUpdated;

    // SfxPlayer.cs
    public static event Action OnGuardNeutralized;

    // SfxPlayer.cs
    public static event Action OnPanelDisabled;

    //SfxPlayer.cs
    public static event Action OnPowerupCollected;

    // Game Manager.cs
    public static void RaiseGameStateChanged(GameState state)
    {
        OnGameStateChanged?.Invoke(state);
    }

    // LevelTimer.cs | GuardController.cs Laser.cs | PlayerContactDetector.cs
    public static void RaisePlayerDetected()
    {
        OnPlayerDetected?.Invoke();
    }

    //Goal.cs
    public static void RaiseGoalReached()
    {
        OnGoalReached?.Invoke();
    }

    // LootManager.cs
    public static void RaiseAllLootCollected()
    {
        OnAllLootCollected?.Invoke();
    }

    // LootManager.cs
    public static void RaiseLootCollected(int collectedCount, int totalCount)
    {
        OnLootCollected?.Invoke(collectedCount, totalCount);
    }

    // LevelTimer.cs
    public static void RaiseTimerUpdated(float elapsed)
    {
        OnTimerUpdated?.Invoke(elapsed);
    }

    //GuardController.cs
    public static void RaiseGuardNeutralized()
    {
        OnGuardNeutralized?.Invoke();
    }

    //Panel.cs
    public static void RaisePanelDisabled()
    {
        OnPanelDisabled?.Invoke();
    }

    // Powerup.cs
    public static void RaisePowerupCollected()
    {
        OnPowerupCollected?.Invoke();
    }
}
