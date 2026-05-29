using UnityEngine;

[CreateAssetMenu(menuName = "StealthParticles/Guard Config")]
public class GuardConfig : ScriptableObject
{
    public float patrolSpeed = 2.5f;
    public float patrolAngularSpeed = 360f;
    public float visionRange = 8f;
    public float visionAngle = 60f;
    public float holdupRange = 2f;
    public float holdupAngle = 120f;
    public float fadeDuration = 0.5f;
}
