using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Melee attacks. Damage, range, cooldown and knockback all come from the
// currently equipped WeaponData asset, so every weapon shares this one
// attack routine. OnWeaponChanged lets the HUD show the current weapon.
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private WeaponVisual weaponVisual;

    public static event Action<WeaponData> OnWeaponChanged;

    public WeaponData CurrentWeapon { get; private set; }

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputAction attackAction;
    private float lastAttackTime = -999f;
    private float damageMultiplier = 1f;
    private Coroutine damageBoostRoutine;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        attackAction = inputActions.FindActionMap("Gameplay").FindAction("Attack");
    }

    private void Start()
    {
        EquipWeapon(startingWeapon);
    }

    private void OnEnable()
    {
        attackAction.Enable();
        attackAction.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
        attackAction.Disable();
    }

    // Called by WeaponPickup. Only upgrades are accepted, so walking over a
    // weaker weapon (or the same one) leaves it on the floor.
    public bool TryEquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return false;
        if (CurrentWeapon != null && weapon.tier <= CurrentWeapon.tier) return false;

        EquipWeapon(weapon);
        return true;
    }

    private void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return;

        CurrentWeapon = weapon;
        if (weaponVisual != null) weaponVisual.Show(weapon);
        OnWeaponChanged?.Invoke(weapon);
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // Input callbacks still fire while the game is frozen (Paused,
        // Game Over, Victory), so ignore attacks then.
        if (Time.timeScale == 0f || CurrentWeapon == null) return;
        if (Time.time < lastAttackTime + CurrentWeapon.attackCooldown) return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        if (weaponVisual != null) weaponVisual.PlaySwing(CurrentWeapon.attackCooldown);
        DealDamageToEnemiesInRange();
    }

    private void DealDamageToEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(GetAttackCenter(), CurrentWeapon.attackRange, enemyLayer);
        int damage = Mathf.RoundToInt(CurrentWeapon.damage * damageMultiplier);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth == null) continue;

            enemyHealth.TakeDamage(damage);

            Vector2 pushDirection = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            hit.GetComponent<EnemyAI>()?.ApplyKnockback(pushDirection * CurrentWeapon.knockback);
        }
    }

    // attackPoint sits on the right of the player (the way the sprite is
    // drawn). When the sprite is flipped to face left, mirror it to the left.
    private Vector2 GetAttackCenter()
    {
        Vector2 offset = attackPoint.position - transform.position;
        if (spriteRenderer != null && spriteRenderer.flipX) offset.x = -offset.x;
        return (Vector2)transform.position + offset;
    }

    public void ApplyDamageBoost(float multiplier, float duration)
    {
        // Only restart the boost timer; other coroutines keep running.
        if (damageBoostRoutine != null) StopCoroutine(damageBoostRoutine);
        damageBoostRoutine = StartCoroutine(DamageBoostRoutine(multiplier, duration));
    }

    private IEnumerator DamageBoostRoutine(float multiplier, float duration)
    {
        damageMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        damageMultiplier = 1f;
        damageBoostRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        WeaponData weapon = CurrentWeapon != null ? CurrentWeapon : startingWeapon;
        if (attackPoint == null || weapon == null) return;
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), weapon.attackRange);
    }
}
