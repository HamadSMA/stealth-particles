using UnityEngine;

[CreateAssetMenu(fileName = "SO_Level_New", menuName = "Stealth Particles/Level Config")]
public class LevelConfig : ScriptableObject
{
    public int levelNumber = 1;

    public string levelName;

    [TextArea]
    public string objectiveText;

    public float timeBudget;

    [Tooltip("Loot the player must collect before the goal appears. 0 or less = all placed loot.")]
    public int lootRequired;

    public float cameraHeight;

    public Vector3 cameraOffset;

    public AudioClip musicTrack;

    public ScoringConfig scoringOverride;
}
