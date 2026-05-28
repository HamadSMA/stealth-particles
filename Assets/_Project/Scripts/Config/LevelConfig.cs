using UnityEngine;

[CreateAssetMenu(fileName = "SO_Level_New", menuName = "Stealth Particles/Level Config")]
public class LevelConfig : ScriptableObject
{
    public string levelName;

    [TextArea]
    public string objectiveText;

    public float timeBudget;

    public float cameraHeight;

    public Vector3 cameraOffset;

    public AudioClip musicTrack;

    public ScoringConfig scoringOverride;
}
