using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Stealth Particles/Patrol Pattern")]
public class PatrolPattern : ScriptableObject
{
    [FormerlySerializedAs("waypoints")]
    public Vector3[] Waypoints;

    [FormerlySerializedAs("patternType")]
    public PatrolPatternType PatternType = PatrolPatternType.Loop;

    [FormerlySerializedAs("waypointReachedDistance")]
    public float WaypointReachedDistance = 0.5f;

    [FormerlySerializedAs("pauseAtWaypoint")]
    public float PauseAtWaypoint = 0f;

    public Vector3 GetWaypoint(int index)
    {
        if (Waypoints == null || Waypoints.Length == 0)
        {
            return Vector3.zero;
        }

        int clamped = Mathf.Clamp(index, 0, Waypoints.Length - 1);
        return Waypoints[clamped];
    }

    // Note on using ref: a method can only return one value through its return statement, so I'm using ref to flip _direction (in PatrolState.cs) between 1 and -1 while return is given to indexing.
    public int GetNextIndex(int currentIndex, ref int direction)
    {
        if (Waypoints == null || Waypoints.Length <= 1)
        {
            return currentIndex;
        }

        int last = Waypoints.Length - 1;

        switch (PatternType)
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
