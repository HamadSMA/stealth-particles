using UnityEngine;

public class CloakPowerup : Powerup
{
    protected override void OnPickup(GameObject player)
    {
        PowerupSystem powerups = player.GetComponentInParent<PowerupSystem>();
        if (powerups != null)
        {
            powerups.ActivateCloak();
        }
    }
}
