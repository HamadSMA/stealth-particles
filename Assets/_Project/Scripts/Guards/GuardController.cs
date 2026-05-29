using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    [SerializeField] private GuardConfig config;
    [SerializeField] private PatrolPattern patrolPattern;
    [SerializeField] private VisionCone visionCone;
    [SerializeField] private Transform playerTransform;

    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private IGuardState currentState;
    private bool isPlaying;
    private bool alreadyDetected;

    public GuardConfig Config => config;
    public PatrolPattern PatrolPattern => patrolPattern;
    public NavMeshAgent Agent => agent;
    public Vector3 SpawnPosition => spawnPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;

        if (visionCone == null)
        {
            visionCone = GetComponent<VisionCone>();
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("GuardController on '" + name + "' has no playerTransform and found no GameObject tagged 'Player'.", this);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.Tick();
        }

        CheckDetection();
    }

    private void CheckDetection()
    {
        if (!isPlaying || alreadyDetected)
        {
            return;
        }

        if (visionCone == null || playerTransform == null)
        {
            return;
        }

        if (visionCone.ContainsPoint(playerTransform.position, out _))
        {
            alreadyDetected = true;
            GameEvents.RaisePlayerDetected();
        }
    }

    public void TransitionTo(IGuardState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("GuardController.TransitionTo called with null state.");
            return;
        }

        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        newState.Enter();
    }

    public void SetDestination(Vector3 worldPos)
    {
        agent.SetDestination(worldPos);
    }

    public bool HasReachedDestination()
    {
        return !agent.pathPending && !agent.hasPath && agent.remainingDistance < 0.05f;
    }

    private void HandleGameStateChanged(GameState state)
    {
        isPlaying = state == GameState.Playing;

        if (state == GameState.Playing && currentState == null)
        {
            TransitionTo(new PatrolState(this));
        }
    }
}
