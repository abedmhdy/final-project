using UnityEngine;

public enum PowerUpType
{
    Health,
    SpeedBoost,
    DamageBoost
}

// Configuration for one type of power-up. "value" means different things
// depending on the type (heal amount, speed multiplier, damage multiplier).
// "duration" is only used by the temporary boosts.
[CreateAssetMenu(fileName = "NewPowerUpData", menuName = "Dungeon Crawler/Power-Up Data")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName = "Power-Up";
    public PowerUpType type;
    public float value = 1f;
    public float duration = 5f;
}
