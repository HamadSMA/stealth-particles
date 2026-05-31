using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Powerup : MonoBehaviour
{
    private bool isPlaying;
    private bool isCollected;

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
        isPlaying = state == GameState.Playing;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !isPlaying)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        isCollected = true;
        OnPickup(other.gameObject);
        gameObject.SetActive(false);
    }

    protected abstract void OnPickup(GameObject player);
}
