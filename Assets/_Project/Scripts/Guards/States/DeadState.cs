using UnityEngine;
using UnityEngine.AI;

public class DeadState : IGuardState
{
    private readonly GuardController guard;

    public DeadState(GuardController guard)
    {
        this.guard = guard;
    }

    public void Enter()
    {
        NavMeshAgent agent = guard.Agent;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (guard.Vision != null)
        {
            guard.Vision.enabled = false;
        }

        guard.PlayHoldupVFX();

        Object.Destroy(guard.gameObject, guard.Config.fadeDuration);
    }

    public void Tick()
    {
    }

    public void Exit()
    {
    }
}
