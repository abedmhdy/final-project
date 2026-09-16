using System;
using UnityEngine;

// Tracks one enemy's health, using the stats from the EnemyData asset
// assigned on its EnemyAI. Raises OnDied so the WaveSpawner can notice
// this enemy is gone without EnemyHealth needing to know what a wave is.
[RequireComponent(typeof(EnemyAI))]
public class EnemyHealth : MonoBehaviour
{
    private EnemyAI enemyAI;
    private Animator animator;
    private Collider2D bodyCollider;
    private int currentHealth;
    private bool isDead;

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
