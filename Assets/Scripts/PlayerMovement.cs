using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f;
        float y = 0f;

        if (keyboard.leftArrowKey.isPressed)  x -= 1f;
        if (keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.downArrowKey.isPressed)  y -= 1f;
        if (keyboard.upArrowKey.isPressed)    y += 1f;

        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f)
        {
            input = input.normalized;
        }

        transform.position += (Vector3)(input * moveSpeed * Time.deltaTime);
    }
}