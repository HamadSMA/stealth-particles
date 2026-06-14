using UnityEngine;
using UnityEngine.Serialization;

public class Panel : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("linkedLaser")]
    private Laser _linkedLaser;

    [SerializeField]
    [FormerlySerializedAs("panelRenderer")]
    private Renderer _panelRenderer;

    [SerializeField]
    [FormerlySerializedAs("armedMaterial")]
    private Material _armedMaterial;

    [SerializeField]
    [FormerlySerializedAs("usedMaterial")]
    private Material _usedMaterial;

    [SerializeField]
    [FormerlySerializedAs("activationRange")]
    private float _activationRange = 3.5f;

    [SerializeField]
    [FormerlySerializedAs("disablePuffPrefab")]
    private ParticleSystem _disablePuffPrefab;

    private bool _isPlaying;
    private bool _isUsed;

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
        if (!_isUsed && _panelRenderer != null && _armedMaterial != null)
        {
            _panelRenderer.sharedMaterial = _armedMaterial;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;
    }

    public bool TryDisable(Vector3 playerWorldPos)
    {
        if (!_isPlaying || _isUsed)
        {
            return false;
        }

        if (Vector3.Distance(playerWorldPos, transform.position) > _activationRange)
        {
            return false;
        }

        if (_linkedLaser != null)
        {
            _linkedLaser.SetActive(false);
        }

        _isUsed = true;

        if (_panelRenderer != null && _usedMaterial != null)
        {
            _panelRenderer.sharedMaterial = _usedMaterial;
        }

        if (_disablePuffPrefab != null)
        {
            Instantiate(_disablePuffPrefab, transform.position, Quaternion.identity);
        }

        GameEvents.RaisePanelDisabled();

        return true;
    }
}
