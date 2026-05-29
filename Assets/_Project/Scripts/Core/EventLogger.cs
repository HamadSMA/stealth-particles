using UnityEngine;

public class EventLogger : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnPlayerDetected += HandlePlayerDetected;
        GameEvents.OnGoalReached += HandleGoalReached;
        GameEvents.OnTimerUpdated += HandleTimerUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnPlayerDetected -= HandlePlayerDetected;
        GameEvents.OnGoalReached -= HandleGoalReached;
        GameEvents.OnTimerUpdated -= HandleTimerUpdated;
    }

    private void HandleGameStateChanged(GameState state)
    {
        Debug.Log($"[EventLogger] OnGameStateChanged: {state}");
    }

    private void HandlePlayerDetected()
    {
        Debug.Log("[EventLogger] OnPlayerDetected");
    }

    private void HandleGoalReached()
    {
        Debug.Log("[EventLogger] OnGoalReached");
    }

    private void HandleTimerUpdated(float elapsed)
    {
        Debug.Log($"[EventLogger] OnTimerUpdated: {elapsed}");
    }
}
