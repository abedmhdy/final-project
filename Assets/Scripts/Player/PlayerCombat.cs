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
    private InputAction attackAction;
    private float lastAttackTime = -999f;
    private float damageMultiplier = 1f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
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
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        DealDamageToEnemiesInRange();
    }

    private void DealDamageToEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
