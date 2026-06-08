using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    private LevelConfig _levelConfig;

    private readonly List<Loot> _tracked = new List<Loot>();
    private int _collectedCount;
    private int _totalCount;
    private int _requiredCount;
    private bool _requirementMet;

    private void Awake()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            _levelConfig = gameManager.LevelConfig;
        }
        else
        {
            Debug.LogWarning("[LootManager] No GameManager found; level config unavailable.");
        }
    }

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
        _totalCount = found.Length;
        _collectedCount = 0;
        _requirementMet = false;

        _requiredCount = _totalCount;
        if (_levelConfig != null && _levelConfig.LootRequired > 0)
        {
            _requiredCount = Mathf.Min(_levelConfig.LootRequired, _totalCount);
        }

        foreach (Loot loot in found)
        {
            loot.OnCollected += HandleLootCollected;
            _tracked.Add(loot);
        }

        if (_requiredCount <= 0)
        {
            _requirementMet = true;
            GameEvents.RaiseAllLootCollected();
        }
    }

    private void HandleLootCollected(Loot loot)
    {
        _collectedCount++;
        GameEvents.RaiseLootCollected(_collectedCount, _totalCount);

        if (!_requirementMet && _collectedCount >= _requiredCount)
        {
            _requirementMet = true;
            GameEvents.RaiseAllLootCollected();
        }
    }

    private void Unbind()
    {
        foreach (Loot loot in _tracked)
        {
            if (loot != null)
            {
                loot.OnCollected -= HandleLootCollected;
            }
        }

        _tracked.Clear();
    }
}
