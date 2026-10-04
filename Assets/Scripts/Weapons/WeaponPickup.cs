using UnityEngine;

// A weapon lying in the room. Walking over it equips it, but only if it is
// stronger (higher tier) than the weapon the player already holds.
public class WeaponPickup : Pickup
{
    [SerializeField] private WeaponData weaponData;

    public WeaponData Weapon => weaponData;

    public override string CollectMessage => $"Equipped {weaponData.weaponName}!";

    public override string RejectMessage => "You already have a stronger weapon";

    private void Awake()
    {
        // The pickup always looks like the weapon it gives.
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && weaponData.icon != null)
        {
            spriteRenderer.sprite = weaponData.icon;
        }
    }

    protected override bool TryApply(GameObject player)
    {
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        return combat != null && combat.TryEquipWeapon(weaponData);
    }
}
