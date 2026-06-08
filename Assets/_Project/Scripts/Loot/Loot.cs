using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class Loot : MonoBehaviour
{
    public event Action<Loot> OnCollected;

    [SerializeField]
    [FormerlySerializedAs("collectPopPrefab")]
    private ParticleSystem _collectPopPrefab;

    private bool _isPlaying;
    private bool _isCollected;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected || !_isPlaying)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isCollected = true;

        if (_collectPopPrefab != null)
        {
            Instantiate(_collectPopPrefab, transform.position, Quaternion.identity);
        }

        OnCollected?.Invoke(this);
        gameObject.SetActive(false);
    }
}
