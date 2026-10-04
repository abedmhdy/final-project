using System;
using UnityEngine;

// Tracks one enemy's health, using the stats from the EnemyData asset
// assigned on its EnemyAI. Raises OnDied so the WaveSpawner (and the
// LootDropper) can react to this enemy's death without EnemyHealth needing
// to know what a wave or a drop is.
[RequireComponent(typeof(EnemyAI))]
public class EnemyHealth : MonoBehaviour
{
    private EnemyAI enemyAI;
    private Animator animator;
    private Collider2D bodyCollider;
    private int currentHealth;
    private bool isDead;

    private static readonly Color DamageNumberColor = new Color(1f, 0.85f, 0.3f);

    public event Action<EnemyHealth> OnDied;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        animator = GetComponentInChildren<Animator>();
        bodyCollider = GetComponent<Collider2D>();
        currentHealth = enemyAI.Data.maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        DamagePopup.Spawn(transform.position + Vector3.up * 0.6f, amount.ToString(), DamageNumberColor);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        isDead = true;
        enemyAI.SetState(EnemyState.Dead);
        animator.SetTrigger("Death");
        bodyCollider.enabled = false;

        OnDied?.Invoke(this);
        Destroy(gameObject, 1f);
    }
}
