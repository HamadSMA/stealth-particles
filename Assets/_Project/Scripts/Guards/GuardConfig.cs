using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "StealthParticles/Guard Config")]
public class GuardConfig : ScriptableObject
{
    [FormerlySerializedAs("patrolSpeed")]
    public float PatrolSpeed = 2.5f;

    [FormerlySerializedAs("patrolAngularSpeed")]
    public float PatrolAngularSpeed = 360f;

    [FormerlySerializedAs("visionRange")]
    public float VisionRange = 8f;

    [FormerlySerializedAs("visionAngle")]
    public float VisionAngle = 60f;

    [FormerlySerializedAs("holdupRange")]
    public float HoldupRange = 2f;

    [FormerlySerializedAs("holdupAngle")]
    public float HoldupAngle = 120f;

    [FormerlySerializedAs("fadeDuration")]
    public float FadeDuration = 0.5f;
}
