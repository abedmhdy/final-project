using UnityEngine;

// Configuration for one weapon. PlayerCombat reads these numbers for every
// attack, so a new weapon is a new asset, not new code.
// "tier" ranks weapons from weakest to strongest: picking up a weapon only
// works if it is an upgrade, so the player can't swap down by accident.
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Dungeon Crawler/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Weapon";
    public int tier = 1;
    public int damage = 1;
    public float attackRange = 0.8f;
    public float attackCooldown = 0.4f;
    [Tooltip("How hard a hit pushes enemies away.")]
    public float knockback = 2f;
    [Tooltip("Used for the pickup, the weapon in the player's hand and the HUD icon.")]
    public Sprite icon;
}
