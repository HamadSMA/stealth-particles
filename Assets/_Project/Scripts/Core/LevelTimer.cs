using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField]
    private LevelConfig levelConfig;

    private float elapsed;
    private bool isRunning;

    public float Elapsed => elapsed;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                elapsed = 0f;
                isRunning = true;
                break;
            case GameState.Briefing:
                elapsed = 0f;
                isRunning = false;
                break;
            case GameState.Success:
            case GameState.Fail:
                isRunning = false;
                break;
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        elapsed += Time.deltaTime;
        GameEvents.RaiseTimerUpdated(elapsed);

        if (levelConfig != null && elapsed >= levelConfig.timeBudget)
        {
            isRunning = false;
            GameEvents.RaisePlayerDetected();
        }
    }
}
