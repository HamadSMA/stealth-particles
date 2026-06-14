using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SO_Level_New", menuName = "Stealth Particles/Level Config")]
public class LevelConfig : ScriptableObject
{
    [FormerlySerializedAs("levelNumber")]
    public int LevelNumber = 1;

    [FormerlySerializedAs("levelName")]
    public string LevelName;

    [TextArea]
    [FormerlySerializedAs("objectiveText")]
    public string ObjectiveText;

    [FormerlySerializedAs("timeBudget")]
    public float TimeBudget;

    [Tooltip("Loot the player must collect before the goal appears. 0 or less = all placed loot.")]
    [FormerlySerializedAs("lootRequired")]
    public int LootRequired;

    [FormerlySerializedAs("cameraHeight")]
    public float CameraHeight;

    [FormerlySerializedAs("cameraOffset")]
    public Vector3 CameraOffset;

    [FormerlySerializedAs("musicTrack")]
    public AudioClip MusicTrack;

    [FormerlySerializedAs("maxScore")]
    public int MaxScore = 2000;

    [FormerlySerializedAs("sRankThreshold")]
    public float SRankThreshold = 0.20f;

    [FormerlySerializedAs("aRankThreshold")]
    public float ARankThreshold = 0.35f;

    [FormerlySerializedAs("bRankThreshold")]
    public float BRankThreshold = 0.65f;

    [FormerlySerializedAs("cRankThreshold")]
    public float CRankThreshold = 1.0f;
}
