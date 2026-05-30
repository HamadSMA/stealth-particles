using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Loot : MonoBehaviour
{
    public event Action<Loot> Collected;

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
        Collected?.Invoke(this);
        gameObject.SetActive(false);
    }
}
