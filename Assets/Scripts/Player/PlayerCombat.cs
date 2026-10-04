using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputAction attackAction;
    private float lastAttackTime = -999f;
    private float damageMultiplier = 1f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        attackAction = inputActions.FindActionMap("Gameplay").FindAction("Attack");
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

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // Input callbacks still fire while the game is frozen (Paused,
        // Game Over, Victory), so ignore attacks then.
        if (Time.timeScale == 0f) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        DealDamageToEnemiesInRange();
    }

    private void DealDamageToEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(GetAttackCenter(), attackRange, enemyLayer);
        int damage = Mathf.RoundToInt(attackDamage * damageMultiplier);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
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
        StopAllCoroutines();
        StartCoroutine(DamageBoostRoutine(multiplier, duration));
    }

    private IEnumerator DamageBoostRoutine(float multiplier, float duration)
    {
        damageMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        damageMultiplier = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);
    }
}
