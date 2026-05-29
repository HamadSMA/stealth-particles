using UnityEngine;

[CreateAssetMenu(menuName = "StealthParticles/Patrol Pattern")]
public class PatrolPattern : ScriptableObject
{
    public Vector3[] waypoints;
    public PatrolPatternType patternType = PatrolPatternType.Loop;
    public float waypointReachedDistance = 0.5f;
    public float pauseAtWaypoint = 0f;

    public Vector3 GetWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return Vector3.zero;
        }

        int clamped = Mathf.Clamp(index, 0, waypoints.Length - 1);
        return waypoints[clamped];
    }

    public int GetNextIndex(int currentIndex, ref int direction)
    {
        if (waypoints == null || waypoints.Length <= 1)
        {
            return currentIndex;
        }

        int last = waypoints.Length - 1;

        switch (patternType)
        {
            case PatrolPatternType.PingPong:
                if (currentIndex >= last)
                {
                    direction = -1;
                }
                else if (currentIndex <= 0)
                {
                    direction = 1;
                }

                return currentIndex + direction;
            default:
                return currentIndex >= last ? 0 : currentIndex + 1;
        }
    }
}
