using UnityEngine;
using UnityEngine.Serialization;

public class Laser : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("isActive")]
    private bool _isActive = true;

    [SerializeField]
    [FormerlySerializedAs("beamRenderers")]
    private Renderer[] _beamRenderers;

    [SerializeField]
    [FormerlySerializedAs("activeMaterial")]
    private Material _activeMaterial;

    [SerializeField]
    [FormerlySerializedAs("detectionHalfExtents")]
    private Vector3 _detectionHalfExtents = new Vector3(8f, 0.6f, 0.15f);

    private bool _isPlaying;
    private bool _hasDetected;

    private readonly Collider[] _overlapResults = new Collider[8];

    public bool IsActive => _isActive;

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
        _isActive = value;
        _hasDetected = false;
        ApplyVisualState();
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;

        if (state == GameState.Playing)
        {
            _hasDetected = false;
        }
    }

    private void ApplyVisualState()
    {
        if (_beamRenderers == null)
        {
            return;
        }

        for (int i = 0; i < _beamRenderers.Length; i++)
        {
            if (_beamRenderers[i] == null)
            {
                continue;
            }

            _beamRenderers[i].enabled = _isActive;

            if (_isActive && _activeMaterial != null)
            {
                _beamRenderers[i].sharedMaterial = _activeMaterial;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_isActive || !_isPlaying || _hasDetected)
        {
            return;
        }

        int count = Physics.OverlapBoxNonAlloc(
            transform.position,
            Vector3.Scale(_detectionHalfExtents, transform.lossyScale),
            _overlapResults,
            transform.rotation
        );

        for (int i = 0; i < count; i++)
        {
            if (_overlapResults[i].CompareTag("Player"))
            {
                _hasDetected = true;
                GameEvents.RaisePlayerDetected();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isActive
            ? new Color(1f, 0.1f, 0.6f, 0.35f)
            : new Color(0.4f, 0.4f, 0.4f, 0.25f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, _detectionHalfExtents * 2f);
    }
}
