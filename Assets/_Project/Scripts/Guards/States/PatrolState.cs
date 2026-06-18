using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IGuardState
{
    private readonly GuardController _guard;
    private int _currentIndex;
    private int _direction = 1;
    private float _pauseTimer;

    public PatrolState(GuardController guard)
    {
        _guard = guard;
    }

    public void Enter()
    {
        NavMeshAgent agent = _guard.Agent;
        PatrolPattern pattern = _guard.PatrolPattern;
        if (pattern == null || pattern.Waypoints == null || pattern.Waypoints.Length == 0)
        {
            Debug.LogWarning(
                "PatrolState: guard has no patrol pattern or waypoints; cannot patrol."
            );
            return;
        }
        agent.isStopped = false;
        agent.speed = _guard.Config.PatrolSpeed;
        agent.angularSpeed = _guard.Config.PatrolAngularSpeed;
        _currentIndex = 0;
        _direction = 1;
        _pauseTimer = 0f;
        _guard.SetDestination(_guard.SpawnPosition + pattern.GetWaypoint(_currentIndex));
    }

    public void Tick()
    {
        NavMeshAgent agent = _guard.Agent;
        PatrolPattern pattern = _guard.PatrolPattern;
        if (pattern == null || pattern.Waypoints == null || pattern.Waypoints.Length == 0)
        {
            Debug.LogWarning(
                "PatrolState: guard has no patrol pattern or waypoints; skipping patrol."
            );
            return;
        }
        if (agent.pathPending)
        {
            return;
        }

        Vector3 target = _guard.SpawnPosition + pattern.GetWaypoint(_currentIndex);
        float distance = Vector3.Distance(agent.transform.position, target);
        bool hasRemainingPath =
            agent.hasPath && agent.remainingDistance > pattern.WaypointReachedDistance;
        if (distance >= pattern.WaypointReachedDistance || hasRemainingPath)
        {
            return;
        }

        if (_pauseTimer > 0f)
        {
            agent.isStopped = true;
            _pauseTimer -= Time.deltaTime;
            return;
        }

        agent.isStopped = false;
        _pauseTimer = pattern.PauseAtWaypoint;
        _currentIndex = pattern.GetNextIndex(_currentIndex, ref _direction);
        _guard.SetDestination(_guard.SpawnPosition + pattern.GetWaypoint(_currentIndex));
    }

    public void Exit()
    {
        NavMeshAgent agent = _guard.Agent;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    public bool TryHoldup(Vector3 fromPosition)
    {
        GuardConfig config = _guard.Config;
        Transform t = _guard.transform;

        Vector3 toSource = fromPosition - t.position;
        toSource.y = 0f;
        if (toSource.sqrMagnitude > config.HoldupRange * config.HoldupRange)
        {
            return false;
        }

        Vector3 forward = t.forward;
        forward.y = 0f;
        if (Vector3.Angle(forward, toSource) < 180f - config.HoldupAngle * 0.5f)
        {
            return false;
        }

        Neutralize();
        return true;
    }

    public void Eliminate()
    {
        Neutralize();
    }

    private void Neutralize()
    {
        _guard.TransitionTo(new DeadState(_guard));
        GameEvents.RaiseGuardNeutralized();
    }
}
