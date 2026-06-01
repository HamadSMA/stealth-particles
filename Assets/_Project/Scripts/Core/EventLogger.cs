using UnityEngine;

public class EventLogger : MonoBehaviour
{
    [SerializeField]
    private bool verbose = false;

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
        Log($"[EventLogger] OnGameStateChanged: {state}");
    }

    private void HandlePlayerDetected()
    {
        Log("[EventLogger] OnPlayerDetected");
    }

    private void HandleGoalReached()
    {
        Log("[EventLogger] OnGoalReached");
    }

    private void HandleTimerUpdated(float elapsed)
    {
        Log($"[EventLogger] OnTimerUpdated: {elapsed}");
    }

    private void Log(string message)
    {
        if (verbose)
        {
            Debug.Log(message);
        }
    }
}
