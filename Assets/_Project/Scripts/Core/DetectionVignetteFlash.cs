using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DetectionVignetteFlash : MonoBehaviour
{
    [SerializeField]
    private Volume volume;

    [SerializeField]
    private Color flashColor = new Color(1f, 0.1f, 0.35f, 1f);

    [SerializeField]
    private float flashIntensity = 0.55f;

    [SerializeField]
    private float duration = 0.4f;

    private Vignette _vignette;
    private Color _originalColor;
    private float _originalIntensity;
    private bool _hasVignette;
    private Coroutine _routine;

    private void Awake()
    {
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        if (volume == null)
        {
            volume = FindFirstObjectByType<Volume>();
        }

        if (volume != null && volume.profile != null && volume.profile.TryGet(out _vignette))
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
        float peakIntensity = Mathf.Max(_originalIntensity, flashIntensity);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(1f - elapsed / duration);
            _vignette.color.value = Color.Lerp(_originalColor, flashColor, k);
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
