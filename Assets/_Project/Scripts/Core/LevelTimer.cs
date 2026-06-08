using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    private LevelConfig _levelConfig;

    private float _elapsed;
    private bool _isRunning;

    public float Elapsed => _elapsed;

    private void Awake()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            _levelConfig = gameManager.LevelConfig;
        }
        else
        {
            Debug.LogWarning("[LevelTimer] No GameManager found; level config unavailable.");
        }
    }

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
                _elapsed = 0f;
                _isRunning = true;
                break;
            case GameState.Briefing:
                _elapsed = 0f;
                _isRunning = false;
                break;
            case GameState.Success:
            case GameState.Fail:
                _isRunning = false;
                break;
        }
    }

    private void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        _elapsed += Time.deltaTime;
        GameEvents.RaiseTimerUpdated(_elapsed);

        if (_levelConfig != null && _elapsed >= _levelConfig.TimeBudget)
        {
            _isRunning = false;
            GameEvents.RaisePlayerDetected();
        }
    }
}
