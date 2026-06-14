using UnityEngine;

public static class ScoreRules
{
    public readonly struct Result
    {
        public readonly int Score;
        public readonly Rank Rank;

        public Result(int score, Rank rank)
        {
            Score = score;
            Rank = rank;
        }
    }

    public static Result Evaluate(LevelConfig config, float elapsed)
    {
        if (config == null || config.TimeBudget <= 0f)
        {
            return new Result(0, Rank.None);
        }

        float fraction = elapsed / config.TimeBudget;

        float remaining = Mathf.Max(0f, 1f - fraction);
        int score = Mathf.Max(0, Mathf.FloorToInt(config.MaxScore * (remaining * remaining)));

        return new Result(score, GetRank(config, fraction));
    }

    private static Rank GetRank(LevelConfig config, float fraction)
    {
        if (fraction <= config.SRankThreshold)
        {
            return Rank.S;
        }
        if (fraction <= config.ARankThreshold)
        {
            return Rank.A;
        }
        if (fraction <= config.BRankThreshold)
        {
            return Rank.B;
        }
        if (fraction <= config.CRankThreshold)
        {
            return Rank.C;
        }

        return Rank.None;
    }
}