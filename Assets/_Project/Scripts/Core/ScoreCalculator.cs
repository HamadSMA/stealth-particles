using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    [SerializeField]
    private ScoringConfig scoringConfig;

    [SerializeField]
    private LevelConfig levelConfig;

    [SerializeField]
    private LevelTimer levelTimer;

    public int LastScore { get; private set; }

    public Rank LastRank { get; private set; }

    public float LastTime { get; private set; }

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
        float budget = levelConfig != null ? levelConfig.timeBudget : 0f;
        ScoringConfig config = ResolveScoringConfig();

        LastTime = levelTimer != null ? levelTimer.Elapsed : 0f;

        if (config != null)
        {
            LastScore = config.CalculateScore(LastTime, budget);
            LastRank = config.GetRank(LastTime, budget);
        }
        else
        {
            LastScore = 0;
            LastRank = Rank.None;
        }

        int level = levelConfig != null ? levelConfig.levelNumber : 0;
        ProgressionManager.RecordSuccess(level, LastScore, LastRank);

        Debug.Log(
            "[ScoreCalculator] Success - time "
                + LastTime.ToString("F2")
                + "s, score "
                + LastScore
                + ", rank "
                + LastRank
        );
    }

    private void HandleFail()
    {
        LastTime = levelTimer != null ? levelTimer.Elapsed : 0f;
        LastScore = 0;
        LastRank = Rank.None;

        Debug.Log("[ScoreCalculator] Fail - time " + LastTime.ToString("F2") + "s");
    }

    private void ClearResults()
    {
        LastScore = 0;
        LastRank = Rank.None;
        LastTime = 0f;
    }

    private ScoringConfig ResolveScoringConfig()
    {
        if (levelConfig != null && levelConfig.scoringOverride != null)
        {
            return levelConfig.scoringOverride;
        }

        return scoringConfig;
    }
}
