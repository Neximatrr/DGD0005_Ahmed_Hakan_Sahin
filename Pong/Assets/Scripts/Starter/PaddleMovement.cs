


using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleMovement : MonoBehaviour
{
    [SerializeField] private int   side  = 0;
    [SerializeField] private float speed = 8f;

    private void Start()
    {
        transform.position = new Vector3(side == 0 ? -Court.PaddleX : Court.PaddleX, 0.5f, 0);
        Court.Paint(gameObject, side == 0 ? new Color(0.35f, 0.65f, 1f) : new Color(1f, 0.60f, 0.25f));
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        float dir = 0f;
        if (side == 0)
        {
            if (kb.wKey.isPressed) dir += 1f;
            if (kb.sKey.isPressed) dir -= 1f;
        }
        else
        {
            if (kb.upArrowKey.isPressed)   dir += 1f;
            if (kb.downArrowKey.isPressed) dir -= 1f;
        }

        transform.position += Vector3.forward * dir * speed * Time.deltaTime;
        Court.ClampPaddle(transform);
    }
}
