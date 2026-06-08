using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class DetectionVignetteFlash : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("volume")]
    private Volume _volume;

    [SerializeField]
    [FormerlySerializedAs("flashColor")]
    private Color _flashColor = new Color(1f, 0.1f, 0.35f, 1f);

    [SerializeField]
    [FormerlySerializedAs("flashIntensity")]
    private float _flashIntensity = 0.55f;

    [SerializeField]
    [FormerlySerializedAs("duration")]
    private float _duration = 0.4f;

    private Vignette _vignette;
    private Color _originalColor;
    private float _originalIntensity;
    private bool _hasVignette;
    private Coroutine _routine;

    private void Awake()
    {
        if (_volume == null)
        {
            _volume = GetComponent<Volume>();
        }

        if (_volume == null)
        {
            _volume = FindFirstObjectByType<Volume>();
        }

        if (_volume != null && _volume.profile != null && _volume.profile.TryGet(out _vignette))
        {
            _hasVignette = true;
            _originalColor = _vignette.color.value;
            _originalIntensity = _vignette.intensity.value;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDetected += Flash;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDetected -= Flash;
        Restore();
    }

    private void Flash()
    {
        if (!_hasVignette)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float peakIntensity = Mathf.Max(_originalIntensity, _flashIntensity);
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(1f - elapsed / _duration);
            _vignette.color.value = Color.Lerp(_originalColor, _flashColor, k);
            _vignette.intensity.value = Mathf.Lerp(_originalIntensity, peakIntensity, k);
            yield return null;
        }

        Restore();
        _routine = null;
    }

    private void Restore()
    {
        if (!_hasVignette)
        {
            return;
        }

        _vignette.color.value = _originalColor;
        _vignette.intensity.value = _originalIntensity;
    }
}
