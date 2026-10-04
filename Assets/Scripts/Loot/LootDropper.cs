using UnityEngine;

// Sits on an enemy prefab and listens to its EnemyHealth.OnDied event, so
// neither EnemyHealth nor the WaveSpawner need to know about loot.
[RequireComponent(typeof(EnemyHealth))]
public class LootDropper : MonoBehaviour
{
    [SerializeField] private LootTable lootTable;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        enemyHealth.OnDied -= HandleDied;
    }

    private void HandleDied(EnemyHealth enemy)
    {
        if (lootTable == null) return;

        GameObject drop = lootTable.RollDrop();
        if (drop != null)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
        }
    }
}
