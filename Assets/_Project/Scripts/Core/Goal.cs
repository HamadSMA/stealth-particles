using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    public bool requiresAllLoot = true;

    private bool isPlaying;
    private bool allLootCollected;
    private bool hasReached;

    private Renderer goalRenderer;

    private void Awake()
    {
        goalRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnAllLootCollected += HandleAllLootCollected;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnAllLootCollected -= HandleAllLootCollected;
    }

    private void Start()
    {
        allLootCollected = !LootSystemPresent();
        UpdateVisibility();
    }

    private void HandleGameStateChanged(GameState state)
    {
        isPlaying = state == GameState.Playing;
    }

    private void HandleAllLootCollected()
    {
        allLootCollected = true;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (goalRenderer != null)
        {
            goalRenderer.enabled = !requiresAllLoot || allLootCollected;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasReached || !isPlaying)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (requiresAllLoot && !allLootCollected)
        {
            return;
        }

        hasReached = true;
        GameEvents.RaiseGoalReached();
    }

    private bool LootSystemPresent()
    {
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            if (behaviour.GetType().Name.Contains("Loot"))
            {
                return true;
            }
        }

        return false;
    }
}
