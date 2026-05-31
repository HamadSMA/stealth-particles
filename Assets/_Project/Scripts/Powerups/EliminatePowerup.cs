using UnityEngine;

public class EliminatePowerup : Powerup
{
    protected override void OnPickup(GameObject player)
    {
        PowerupSystem powerups = player.GetComponentInParent<PowerupSystem>();
        if (powerups != null)
        {
            powerups.Grant(PowerupType.Eliminate);
        }
    }
}
