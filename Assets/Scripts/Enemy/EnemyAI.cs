using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
    Dead
}

// Simple state machine for one enemy: it patrols between points until the
// player enters its detection range, chases the player, and attacks once
// in range. All tuning values come from the EnemyData asset so new enemy
// types don't need new code.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform visual;

    public EnemyData Data => enemyData;

    private EnemyState currentState = EnemyState.Patrol;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator animator;
    private Rigidbody2D rb;

    private int currentPatrolIndex;
    private float lastAttackTime = -999f;

    private void Awake()
    {
        animator = visual.GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol(distanceToPlayer);
                break;
            case EnemyState.Chase:
                UpdateChase(distanceToPlayer);
                break;
            case EnemyState.Attack:
                UpdateAttack(distanceToPlayer);
                break;
        }

        bool isMoving = currentState == EnemyState.Patrol || currentState == EnemyState.Chase;
        animator.SetBool("IsMoving", isMoving);
    }

    private void UpdatePatrol(float distanceToPlayer)
    {
        if (distanceToPlayer <= enemyData.detectionRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform target = patrolPoints[currentPatrolIndex];
        MoveTowards(target.position);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void UpdateChase(float distanceToPlayer)
    {
        if (distanceToPlayer > enemyData.detectionRange * 1.5f)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        if (distanceToPlayer <= enemyData.attackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }

        MoveTowards(player.position);
    }

    private void UpdateAttack(float distanceToPlayer)
    {
        rb.linearVelocity = Vector2.zero;

        if (distanceToPlayer > enemyData.attackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (Time.time >= lastAttackTime + enemyData.attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");
            playerHealth?.TakeDamage(enemyData.damage);
        }
    }

    private void MoveTowards(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * enemyData.moveSpeed;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            visual.localScale = scale;
        }
    }

    public void SetState(EnemyState newState)
    {
        currentState = newState;
        if (newState == EnemyState.Dead)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
