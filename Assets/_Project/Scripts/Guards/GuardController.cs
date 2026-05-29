using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    [SerializeField] private GuardConfig config;
    [SerializeField] private PatrolPattern patrolPattern;

    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private IGuardState currentState;

    public GuardConfig Config => config;
    public PatrolPattern PatrolPattern => patrolPattern;
    public NavMeshAgent Agent => agent;
    public Vector3 SpawnPosition => spawnPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;
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
        if (state == GameState.Playing && currentState == null)
        {
            TransitionTo(new PatrolState(this));
        }
    }
}
