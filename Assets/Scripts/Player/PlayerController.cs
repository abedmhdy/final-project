using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// FixedUpdate naturally stops running when the game is paused
// (Time.timeScale = 0), so no extra pause checks are needed here.
// Facing uses SpriteRenderer.flipX rather than the Visual's scale, because
// the Animator animates the Visual's scale every frame and would undo it.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Transform visual;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputAction moveAction;
    private Vector2 moveInput;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = visual.GetComponent<Animator>();
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        moveAction = inputActions.FindActionMap("Gameplay").FindAction("Move");
    }

    private void OnEnable() => moveAction.Enable();

    private void OnDisable() => moveAction.Disable();

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }

        // The player sprite is drawn facing right. Don't turn around while
        // the game is frozen (paused / game over / victory).
        if (Time.timeScale > 0f && Mathf.Abs(moveInput.x) > 0.01f)
        {
            spriteRenderer.flipX = moveInput.x < 0f;
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + moveInput.normalized * moveSpeed * speedMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
    }
}
