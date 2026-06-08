using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Powerup : MonoBehaviour
{
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
        OnPickup(other.gameObject);
        GameEvents.RaisePowerupCollected();
        gameObject.SetActive(false);
    }

    protected abstract void OnPickup(GameObject player);
}
