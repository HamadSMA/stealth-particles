using UnityEngine;
using UnityEngine.Serialization;

public class EventLogger : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("verbose")]
    private bool _verbose = false;

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
        if (_verbose)
        {
            Debug.Log(message);
        }
    }
}
