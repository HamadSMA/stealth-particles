using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressionManager
{
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnPlay()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
#endif

    public static bool IsUnlocked(int level)
    {
        return PlayerPrefs.GetInt(UnlockedKey(level), level == 1 ? 1 : 0) == 1;
    }

    public static Rank GetBestRank(int level)
    {
        return (Rank)PlayerPrefs.GetInt(RankKey(level), (int)Rank.None);
    }

    public static int GetBestScore(int level)
    {
        return PlayerPrefs.GetInt(ScoreKey(level), 0);
    }

    public static void RecordSuccess(int level, int score, Rank rank)
    {
        if (level < 1)
        {
            return;
        }

        if (level + 1 < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPrefs.SetInt(UnlockedKey(level + 1), 1);
        }

        if (IsRankBetter(rank, GetBestRank(level)))
        {
            PlayerPrefs.SetInt(RankKey(level), (int)rank);
        }

        if (score > GetBestScore(level))
        {
            PlayerPrefs.SetInt(ScoreKey(level), score);
        }

        PlayerPrefs.Save();
    }

    private static bool IsRankBetter(Rank candidate, Rank current)
    {
        if (candidate == Rank.None)
        {
            return false;
        }

        if (current == Rank.None)
        {
            return true;
        }

        return (int)candidate < (int)current;
    }

    private static string UnlockedKey(int level)
    {
        return "level_" + level + "_unlocked";
    }

    private static string RankKey(int level)
    {
        return "level_" + level + "_best_rank";
    }

    private static string ScoreKey(int level)
    {
        return "level_" + level + "_best_score";
    }
}
