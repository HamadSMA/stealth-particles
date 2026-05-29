using UnityEngine;

public class PlayerContactDetector : MonoBehaviour
{
    [SerializeField] private LayerMask guardMask;
    [SerializeField] private float contactRadius = 0.6f;

    private bool isPlaying;
    private readonly Collider[] hits = new Collider[4];

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
        if (!isPlaying)
        {
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(transform.position, contactRadius, hits, guardMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].GetComponentInParent<GuardController>() != null)
            {
                GameEvents.RaisePlayerDetected();
                return;
            }
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        isPlaying = state == GameState.Playing;
    }
}
