using UnityEngine;

public static class RankVisuals
{
    public static string LabelFor(Rank rank)
    {
        return rank == Rank.None ? "-" : rank.ToString();
    }

    public static Color ColorFor(Rank rank)
    {
        switch (rank)
        {
            case Rank.S:
                return new Color(1f, 0.2f, 0.6f);
            case Rank.A:
                return new Color(1f, 0.84f, 0.2f);
            case Rank.B:
                return new Color(0.3f, 0.9f, 1f);
            case Rank.C:
                return Color.white;
            default:
                return new Color(0.6f, 0.6f, 0.7f);
        }
    }
}
