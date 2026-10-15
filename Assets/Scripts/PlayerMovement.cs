using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 11f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 input;
    private Vector2 lastDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if ((DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) || StatusMenu.IsOpen)
        {
            input = Vector2.zero;
            UpdateAnimator();
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f;
        float y = 0f;

        if (keyboard.leftArrowKey.isPressed)  x -= 1f;
        if (keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.downArrowKey.isPressed)  y -= 1f;
        if (keyboard.upArrowKey.isPressed)    y += 1f;

        input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f)
        {
            input = input.normalized;
        }

        // 멈췄을 때 마지막으로 보던 방향의 Idle이 나오도록 입력이 있을 때만 갱신
        if (input != Vector2.zero)
        {
            lastDir = input;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("MoveX", lastDir.x);
        animator.SetFloat("MoveY", lastDir.y);
        animator.SetFloat("Speed", input.magnitude);
    }
}
