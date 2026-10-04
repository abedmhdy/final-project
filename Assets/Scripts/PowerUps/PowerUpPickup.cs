using UnityEngine;

public class PowerUpPickup : Pickup
{
    [SerializeField] private PowerUpData powerUpData;

    public PowerUpData Data => powerUpData;

    public override string CollectMessage =>
        powerUpData.type == PowerUpType.Health
            ? $"+{Mathf.RoundToInt(powerUpData.value)} Health"
            : $"{powerUpData.powerUpName}!";

    protected override bool TryApply(GameObject player)
    {
        switch (powerUpData.type)
        {
            case PowerUpType.Health:
                player.GetComponent<PlayerHealth>()?.Heal(Mathf.RoundToInt(powerUpData.value));
                break;
            case PowerUpType.SpeedBoost:
                player.GetComponent<PlayerController>()?.ApplySpeedBoost(powerUpData.value, powerUpData.duration);
                break;
            case PowerUpType.DamageBoost:
                player.GetComponent<PlayerCombat>()?.ApplyDamageBoost(powerUpData.value, powerUpData.duration);
                break;
        }
        return true;
    }
}
