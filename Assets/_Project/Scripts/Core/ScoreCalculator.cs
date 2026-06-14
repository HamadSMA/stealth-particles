using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
public class ScoreCalculator : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("levelTimer")]
    private LevelTimer _levelTimer;

    private LevelConfig _levelConfig;

    public int LastScore { get; private set; }

    public Rank LastRank { get; private set; }

    public float LastTime { get; private set; }

    private void Awake()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            _levelConfig = gameManager.LevelConfig;
        }
        else
        {
            Debug.LogWarning("[ScoreCalculator] No GameManager found; level config unavailable.");
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
            case GameState.Success:
                HandleSuccess();
                break;
            case GameState.Fail:
                HandleFail();
                break;
            case GameState.Briefing:
            case GameState.Playing:
                ClearResults();
                break;
        }
    }

    private void HandleSuccess()
    {
        LastTime = _levelTimer != null ? _levelTimer.Elapsed : 0f;

        ScoreRules.Result result = ScoreRules.Evaluate(_levelConfig, LastTime);
        LastScore = result.Score;
        LastRank = result.Rank;

        int level = _levelConfig != null ? _levelConfig.LevelNumber : 0;
        ProgressionManager.RecordSuccess(level, LastScore, LastRank);
    }

    private void HandleFail()
    {
        LastTime = _levelTimer != null ? _levelTimer.Elapsed : 0f;
        LastScore = 0;
        LastRank = Rank.None;
    }

    private void ClearResults()
    {
        LastScore = 0;
        LastRank = Rank.None;
        LastTime = 0f;
    }
}
