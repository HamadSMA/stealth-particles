using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [FormerlySerializedAs("requiresAllLoot")]
    public bool RequiresAllLoot = true;

    [SerializeField]
    [FormerlySerializedAs("goalBurstPrefab")]
    private ParticleSystem _goalBurstPrefab;

    private bool _isPlaying;
    private bool _allLootCollected;
    private bool _hasReached;

    private Renderer _goalRenderer;

    private void Awake()
    {
        _goalRenderer = GetComponent<Renderer>();
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
        _allLootCollected = !AnyLootPresent();
        UpdateVisibility();
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;
    }

    private void HandleAllLootCollected()
    {
        _allLootCollected = true;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (_goalRenderer != null)
        {
            _goalRenderer.enabled = !RequiresAllLoot || _allLootCollected;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasReached || !_isPlaying)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (RequiresAllLoot && !_allLootCollected)
        {
            return;
        }

        _hasReached = true;

        if (_goalBurstPrefab != null)
        {
            Instantiate(_goalBurstPrefab, transform.position, Quaternion.identity);
        }

        GameEvents.RaiseGoalReached();
    }

    private static bool AnyLootPresent()
    {
        return FindObjectsByType<Loot>(FindObjectsSortMode.None).Length > 0;
    }
}
