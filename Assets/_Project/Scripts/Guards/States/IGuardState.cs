using UnityEngine;

public interface IGuardState
{
    void Enter();
    void Tick();
    void Exit();

    bool TryHoldup(Vector3 fromPosition);
    void Eliminate();
}
