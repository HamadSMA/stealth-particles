using UnityEngine;
using UnityEngine.AI;

public class DeadState : IGuardState
{
    private readonly GuardController _guard;

    public DeadState(GuardController guard)
    {
        _guard = guard;
    }

    public void Enter()
    {
        NavMeshAgent agent = _guard.Agent;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (_guard.Vision != null)
        {
            _guard.Vision.enabled = false;
        }
        _guard.PlayHoldupVFX();
        Object.Destroy(_guard.gameObject, _guard.Config.FadeDuration);
    }

    public void Tick() { }

    public void Exit() { }

    public bool TryHoldup(Vector3 fromPosition)
    {
        return false;
    }

    public void Eliminate() { }
}
