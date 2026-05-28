using UnityEngine;

[CreateAssetMenu(fileName = "SO_Scoring_Config", menuName = "Stealth Particles/Scoring Config")]
public class ScoringConfig : ScriptableObject
{
    public int maxScore = 2000;

    public float sRankThreshold = 0.20f;

    public float aRankThreshold = 0.35f;

    public float bRankThreshold = 0.65f;

    public float cRankThreshold = 1.0f;

    public int CalculateScore(float elapsed, float budget)
    {
        if (budget <= 0f)
        {
            return 0;
        }

        float remaining = Mathf.Max(0f, 1f - (elapsed / budget));
        int score = Mathf.FloorToInt(maxScore * (remaining * remaining));
        return Mathf.Max(0, score);
    }

    public Rank GetRank(float elapsed, float budget)
    {
        if (budget <= 0f)
        {
            return Rank.None;
        }

        float fraction = elapsed / budget;

        if (fraction <= sRankThreshold)
        {
            return Rank.S;
        }
        if (fraction <= aRankThreshold)
        {
            return Rank.A;
        }
        if (fraction <= bRankThreshold)
        {
            return Rank.B;
        }
        if (fraction <= cRankThreshold)
        {
            return Rank.C;
        }

        return Rank.None;
    }
}
