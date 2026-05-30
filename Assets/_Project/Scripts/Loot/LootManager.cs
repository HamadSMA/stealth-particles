using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    private readonly List<Loot> tracked = new List<Loot>();
    private int collectedCount;
    private int totalCount;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        Unbind();
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        Unbind();

        Loot[] found = FindObjectsByType<Loot>(FindObjectsSortMode.None);
        totalCount = found.Length;
        collectedCount = 0;

        foreach (Loot loot in found)
        {
            loot.Collected += HandleLootCollected;
            tracked.Add(loot);
        }

        if (totalCount == 0)
        {
            GameEvents.RaiseAllLootCollected();
        }
    }

    private void HandleLootCollected(Loot loot)
    {
        collectedCount++;
        GameEvents.RaiseLootCollected(collectedCount, totalCount);

        if (collectedCount >= totalCount)
        {
            GameEvents.RaiseAllLootCollected();
        }
    }

    private void Unbind()
    {
        foreach (Loot loot in tracked)
        {
            if (loot != null)
            {
                loot.Collected -= HandleLootCollected;
            }
        }

        tracked.Clear();
    }
}
