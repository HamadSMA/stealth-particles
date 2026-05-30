using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private bool isActive = true;

    [SerializeField]
    private Renderer[] beamRenderers;

    [SerializeField]
    private Material activeMaterial;

    [SerializeField]
    private Vector3 detectionHalfExtents = new Vector3(8f, 0.6f, 0.15f);

    private bool isPlaying;
    private bool hasDetected;

    private readonly Collider[] overlapResults = new Collider[8];

    public bool IsActive => isActive;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        ApplyVisualState();
    }

    public void SetActive(bool value)
    {
        isActive = value;
        hasDetected = false;
        ApplyVisualState();
    }

    private void HandleGameStateChanged(GameState state)
    {
        isPlaying = state == GameState.Playing;

        if (state == GameState.Playing)
        {
            hasDetected = false;
        }
    }

    private void ApplyVisualState()
    {
        if (beamRenderers == null)
        {
            return;
        }

        for (int i = 0; i < beamRenderers.Length; i++)
        {
            if (beamRenderers[i] == null)
            {
                continue;
            }

            beamRenderers[i].enabled = isActive;

            if (isActive && activeMaterial != null)
            {
                beamRenderers[i].sharedMaterial = activeMaterial;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isActive || !isPlaying || hasDetected)
        {
            return;
        }

        int count = Physics.OverlapBoxNonAlloc(
            transform.position,
            detectionHalfExtents,
            overlapResults,
            transform.rotation
        );

        for (int i = 0; i < count; i++)
        {
            if (overlapResults[i].CompareTag("Player"))
            {
                hasDetected = true;
                GameEvents.RaisePlayerDetected();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? new Color(1f, 0.1f, 0.6f, 0.35f) : new Color(0.4f, 0.4f, 0.4f, 0.25f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, detectionHalfExtents * 2f);
    }
}
