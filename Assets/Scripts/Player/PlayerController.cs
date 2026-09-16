using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Reads movement input through the New Input System and moves the player
// with physics. FixedUpdate naturally stops running when the game is
// paused (Time.timeScale = 0), so no extra pause checks are needed here.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Transform visual;

    private Rigidbody2D rb;
    private Animator animator;
    private InputAction moveAction;
    private Vector2 moveInput;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = visual.GetComponent<Animator>();
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

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
            visual.localScale = scale;
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
