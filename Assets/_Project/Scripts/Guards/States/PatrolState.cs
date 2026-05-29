using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IGuardState
{
    private readonly GuardController guard;

    private int currentIndex;
    private int direction = 1;
    private float pauseTimer;

    public PatrolState(GuardController guard)
    {
        this.guard = guard;
    }

    public void Enter()
    {
        NavMeshAgent agent = guard.Agent;
        PatrolPattern pattern = guard.PatrolPattern;

        if (pattern == null || pattern.waypoints == null || pattern.waypoints.Length == 0)
        {
            Debug.LogWarning("PatrolState: guard has no patrol pattern or waypoints; cannot patrol.");
            return;
        }

        agent.isStopped = false;
        agent.speed = guard.Config.patrolSpeed;
        agent.angularSpeed = guard.Config.patrolAngularSpeed;

        currentIndex = 0;
        direction = 1;
        pauseTimer = 0f;

        guard.SetDestination(guard.SpawnPosition + pattern.GetWaypoint(currentIndex));
    }

    public void Tick()
    {
        NavMeshAgent agent = guard.Agent;
        PatrolPattern pattern = guard.PatrolPattern;

        if (pattern == null || pattern.waypoints == null || pattern.waypoints.Length == 0)
        {
            Debug.LogWarning("PatrolState: guard has no patrol pattern or waypoints; skipping patrol.");
            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        Vector3 target = guard.SpawnPosition + pattern.GetWaypoint(currentIndex);
        float distance = Vector3.Distance(agent.transform.position, target);
        bool hasRemainingPath = agent.hasPath && agent.remainingDistance > pattern.waypointReachedDistance;

        if (distance >= pattern.waypointReachedDistance || hasRemainingPath)
        {
            return;
        }

        if (pauseTimer > 0f)
        {
            agent.isStopped = true;
            pauseTimer -= Time.deltaTime;
            return;
        }

        agent.isStopped = false;
        pauseTimer = pattern.pauseAtWaypoint;
        currentIndex = pattern.GetNextIndex(currentIndex, ref direction);
        guard.SetDestination(guard.SpawnPosition + pattern.GetWaypoint(currentIndex));
    }

    public void Exit()
    {
        NavMeshAgent agent = guard.Agent;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
}
