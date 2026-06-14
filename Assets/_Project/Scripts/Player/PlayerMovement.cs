using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("walkableMask")]
    private LayerMask _walkableMask;

    [SerializeField]
    [FormerlySerializedAs("guardMask")]
    private LayerMask _guardMask;

    [SerializeField]
    [FormerlySerializedAs("panelMask")]
    private LayerMask _panelMask;

    [SerializeField]
    private float _repathThreshold = 0.3f;

    private NavMeshAgent _agent;
    private Camera _camera;
    private PowerupSystem _powerups;

    private bool _actionConsumedThisPress;
    private bool _hasMoveTarget;
    private Vector3 _lastDestination;

    private bool _isPlaying;

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

        bool isHeld = TryGetPointerHold(out Vector2 pointerPosition);

        if (isHeld && WasPointerPressedThisFrame())
        {
            _actionConsumedThisPress = false;

            if (TryHoldupAt(pointerPosition) || TryDisablePanelAt(pointerPosition))
            {
                _actionConsumedThisPress = true;
                return;
            }
        }

        if (isHeld && !_actionConsumedThisPress)
        {
            SteerToward(pointerPosition);
            return;
        }

        if (WasPointerReleasedThisFrame())
        {
            _actionConsumedThisPress = false;
            _hasMoveTarget = false;
            _agent.ResetPath();
        }
    }

    private bool TryHoldupAt(Vector2 pointerPosition)
    {
        Ray ray = _camera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _guardMask.value))
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
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _panelMask.value))
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
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _walkableMask.value))
        {
            return;
        }

        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
        {
            return;
        }

        if (
            _hasMoveTarget
            && (navHit.position - _lastDestination).sqrMagnitude
                < _repathThreshold * _repathThreshold
        )
        {
            return;
        }

        _agent.SetDestination(navHit.position);
        _lastDestination = navHit.position;

        _hasMoveTarget = true;
    }

    private static bool TryGetPointerHold(out Vector2 position)
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        position = default;
        return false;
    }

    private static bool WasPointerPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return Touchscreen.current != null
            && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
    }

    private static bool WasPointerReleasedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }

        return Touchscreen.current != null
            && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _isPlaying = state == GameState.Playing;
    }
}
