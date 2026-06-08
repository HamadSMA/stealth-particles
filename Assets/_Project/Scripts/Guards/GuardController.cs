using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("config")]
    private GuardConfig _config;

    [SerializeField]
    [FormerlySerializedAs("patrolPattern")]
    private PatrolPattern _patrolPattern;

    [SerializeField]
    [FormerlySerializedAs("visionCone")]
    private VisionCone _visionCone;

    [SerializeField]
    [FormerlySerializedAs("playerTransform")]
    private Transform _playerTransform;

    [SerializeField]
    [FormerlySerializedAs("holdupBurstPrefab")]
    private ParticleSystem _holdupBurstPrefab;

    private NavMeshAgent _agent;
    private PowerupSystem _playerPowerups;
    private Vector3 _spawnPosition;
    private IGuardState _currentState;
    private bool _isPlaying;
    private bool _alreadyDetected;
    private bool _isHeldUp;

    public GuardConfig Config => _config;
    public PatrolPattern PatrolPattern => _patrolPattern;
    public NavMeshAgent Agent => _agent;
    public Vector3 SpawnPosition => _spawnPosition;
    public VisionCone Vision => _visionCone;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _spawnPosition = transform.position;

        if (_visionCone == null)
        {
            _visionCone = GetComponent<VisionCone>();
        }

        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        if (_playerTransform == null)
        {
            Debug.LogWarning(
                "GuardController on '"
                    + name
                    + "' has no player transform and found no GameObject tagged 'Player'.",
                this
            );
        }

        if (_playerTransform != null)
        {
            _playerPowerups = _playerTransform.GetComponentInParent<PowerupSystem>();
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
        if (_currentState != null)
        {
            _currentState.Tick();
        }

        CheckDetection();
    }

    private void CheckDetection()
    {
        if (!_isPlaying || _alreadyDetected)
        {
            return;
        }

        if (_visionCone == null || _playerTransform == null)
        {
            return;
        }

        if (_playerPowerups != null && _playerPowerups.IsCloaked)
        {
            if (_visionCone.ContainsPoint(_playerTransform.position, out _))
            {
                _playerPowerups.ReportCloakedSighting();
            }

            return;
        }

        if (_visionCone.ContainsPoint(_playerTransform.position, out _))
        {
            _alreadyDetected = true;
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

        if (_currentState != null)
        {
            _currentState.Exit();
        }

        _currentState = newState;
        newState.Enter();
    }

    public void SetDestination(Vector3 worldPos)
    {
        _agent.SetDestination(worldPos);
    }

    public void PlayHoldupVFX()
    {
        if (_holdupBurstPrefab != null)
        {
            Instantiate(_holdupBurstPrefab, transform.position, Quaternion.identity);
        }
    }

    public bool TryHoldup(Vector3 fromPosition)
    {
        if (_isHeldUp)
        {
            return false;
        }

        Vector3 toSource = fromPosition - transform.position;
        toSource.y = 0f;

        if (toSource.sqrMagnitude > _config.HoldupRange * _config.HoldupRange)
        {
            return false;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (Vector3.Angle(forward, toSource) < 180f - _config.HoldupAngle * 0.5f)
        {
            return false;
        }

        _isHeldUp = true;
        TransitionTo(new DeadState(this));
        GameEvents.RaiseGuardNeutralized();
        return true;
    }

    public void Eliminate()
    {
        if (_isHeldUp)
        {
            return;
        }

        _isHeldUp = true;
        TransitionTo(new DeadState(this));
        GameEvents.RaiseGuardNeutralized();
    }

    public bool HasReachedDestination()
    {
        return !_agent.pathPending && !_agent.hasPath && _agent.remainingDistance < 0.05f;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;

        if (state == GameState.Playing && _currentState == null)
        {
            TransitionTo(new PatrolState(this));
        }
    }
}
