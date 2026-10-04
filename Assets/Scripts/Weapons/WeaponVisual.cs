using UnityEngine;

// Draws the equipped weapon in the player's hand. It copies the character's
// facing (SpriteRenderer.flipX) every frame, mirroring its position and
// angle, so it never fights the existing facing system. The sprite is drawn
// pointing up with its pivot at the bottom of the handle.
[RequireComponent(typeof(SpriteRenderer))]
public class WeaponVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private float restAngle = -30f;
    [SerializeField] private float swingStartAngle = 70f;
    [SerializeField] private float swingEndAngle = -110f;

    private SpriteRenderer weaponRenderer;
    private Vector3 handPosition;
    private float swingDuration;
    private float swingTimer;

    private void Awake()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();
        handPosition = transform.localPosition;
        weaponRenderer.enabled = false;
    }

    public void Show(WeaponData weapon)
    {
        weaponRenderer.sprite = weapon.icon;
        weaponRenderer.enabled = weapon.icon != null;
    }

    // Heavier weapons (longer cooldown) swing more slowly.
    public void PlaySwing(float attackCooldown)
    {
        swingDuration = Mathf.Clamp(attackCooldown * 0.5f, 0.12f, 0.35f);
        swingTimer = swingDuration;
    }

    private void LateUpdate()
    {
        float angle = restAngle;
        if (swingTimer > 0f)
        {
            swingTimer -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(swingTimer / swingDuration);
            angle = Mathf.Lerp(swingStartAngle, swingEndAngle, progress);
        }

        bool facingLeft = characterRenderer != null && characterRenderer.flipX;
        float side = facingLeft ? -1f : 1f;

        transform.localPosition = new Vector3(handPosition.x * side, handPosition.y, handPosition.z);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle * side);
        weaponRenderer.flipX = facingLeft;
    }
}
