using UnityEngine;

// Configuration for one type of enemy. Designers can create new enemy
// types (or tweak existing ones) as assets in the Project window without
// touching any code.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Dungeon Crawler/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Enemy";
    public int maxHealth = 3;
    public int damage = 1;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
}
