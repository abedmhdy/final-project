using System;
using UnityEngine;

// Owns the player's health. Raises static events instead of holding
// direct references to the UI or the GameManager - the HUD listens to
// OnHealthChanged to update the health bar, and the GameManager listens
// to OnPlayerDied to switch the game into the GameOver state.
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    private int currentHealth;
    private Animator animator;
    private bool isDead;

    public static event Action<int, int> OnHealthChanged;
    public static event Action OnPlayerDied;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerCombat>().enabled = false;

        OnPlayerDied?.Invoke();
    }
}
