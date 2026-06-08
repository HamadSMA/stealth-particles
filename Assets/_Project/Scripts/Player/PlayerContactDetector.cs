using UnityEngine;
using UnityEngine.Serialization;

public class PlayerContactDetector : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("guardMask")]
    private LayerMask _guardMask;

    [SerializeField]
    [FormerlySerializedAs("contactRadius")]
    private float _contactRadius = 0.6f;

    private bool _isPlaying;
    private readonly Collider[] _hits = new Collider[4];

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void FixedUpdate()
    {
        if (!_isPlaying)
        {
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(transform.position, _contactRadius, _hits, _guardMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            if (_hits[i].GetComponentInParent<GuardController>() != null)
            {
                GameEvents.RaisePlayerDetected();
                return;
            }
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;
    }
}
