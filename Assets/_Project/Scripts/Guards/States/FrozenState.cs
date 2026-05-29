using UnityEngine;
using UnityEngine.AI;

public class FrozenState : IGuardState
{
    private readonly GuardController guard;

    public FrozenState(GuardController guard)
    {
        this.guard = guard;
    }

    public void Enter()
    {
        NavMeshAgent agent = guard.Agent;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    public void Tick()
    {
    }

    public void Exit()
    {
        guard.Agent.isStopped = false;
    }
}
