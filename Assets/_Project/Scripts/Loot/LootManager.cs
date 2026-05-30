using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    [SerializeField]
    private LevelConfig levelConfig;

    private readonly List<Loot> tracked = new List<Loot>();
    private int collectedCount;
    private int totalCount;
    private int requiredCount;
    private bool requirementMet;

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
        requirementMet = false;

        requiredCount = totalCount;
        if (levelConfig != null && levelConfig.lootRequired > 0)
        {
            requiredCount = Mathf.Min(levelConfig.lootRequired, totalCount);
        }

        foreach (Loot loot in found)
        {
            loot.Collected += HandleLootCollected;
            tracked.Add(loot);
        }

        if (requiredCount <= 0)
        {
            requirementMet = true;
            GameEvents.RaiseAllLootCollected();
        }
    }

    private void HandleLootCollected(Loot loot)
    {
        collectedCount++;
        GameEvents.RaiseLootCollected(collectedCount, totalCount);

        if (!requirementMet && collectedCount >= requiredCount)
        {
            requirementMet = true;
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
