using UnityEngine;

// Sits on a pickup prefab in the level. Applies its PowerUpData to
// whichever player walks into it, then removes itself.
public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpData powerUpData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ApplyEffect(other.gameObject);
        Destroy(gameObject);
    }

    private void ApplyEffect(GameObject player)
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
    }
}
