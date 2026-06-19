using UnityEngine;

public interface IGuardState
{
    void Enter();
    void Tick();
    void Exit();

    bool TryHoldup(Vector3 fromPosition);

    //The difference between kill by TryHoldup and Eliminate is that the first one happen when within distance and at a certain angle, Elminate however is related to the Eliminate powerup which lets you kill one guard from any distance.
    void Eliminate();
}
