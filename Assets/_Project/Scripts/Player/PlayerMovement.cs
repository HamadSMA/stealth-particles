using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private LayerMask walkableMask;

    [SerializeField]
    private LayerMask guardMask;

    [SerializeField]
    private LayerMask panelMask;

    private NavMeshAgent _agent;
    private Camera _camera;
    private PowerupSystem _powerups;

    private static bool _isPlaying;

    public float Speed
    {
        get => _agent.speed;
        set => _agent.speed = value;
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _camera = Camera.main;
        _powerups = GetComponent<PowerupSystem>();
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
        if (!_isPlaying)
        {
            return;
        }

        if (_camera == null)
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                return;
            }
        }

        if (TryGetPointerPress(out Vector2 pressPosition))
        {
            if (TryHoldupAt(pressPosition))
            {
                return;
            }

            if (TryDisablePanelAt(pressPosition))
            {
                return;
            }

            SteerToward(pressPosition);
        }
    }

    private bool TryHoldupAt(Vector2 pointerPosition)
    {
        Ray ray = _camera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, guardMask.value))
        {
            return false;
        }

        GuardController guard = hit.collider.GetComponent<GuardController>();
        if (guard != null)
        {
            if (_powerups != null && _powerups.Has(PowerupType.Eliminate))
            {
                guard.Eliminate();
                _powerups.TryConsume(PowerupType.Eliminate);
            }
            else
            {
                guard.TryHoldup(transform.position);
            }
        }

        return true;
    }

    private bool TryDisablePanelAt(Vector2 pointerPosition)
    {
        Ray ray = _camera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, panelMask.value))
        {
            return false;
        }

        Panel panel = hit.collider.GetComponent<Panel>();
        if (panel != null)
        {
            panel.TryDisable(transform.position);
        }

        return true;
    }

    private void SteerToward(Vector2 pointerPosition)
    {
        Ray ray = _camera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, walkableMask.value))
        {
            return;
        }

        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
        {
            _agent.SetDestination(navHit.position);
            GameEvents.RaiseTapMove();
        }
    }

    private static bool TryGetPointerPress(out Vector2 position)
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }

        if (
            Touchscreen.current != null
            && Touchscreen.current.primaryTouch.press.wasPressedThisFrame
        )
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        position = default;
        return false;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;
    }
}
