using UnityEngine;

public class DebugPlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Debug.Log("POS: " + transform.position + " | VEL: " + rb.linearVelocity);
    }

    private void LateUpdate()
    {
        if (transform.position != lastPosition)
        {
            Debug.Log("La posicion ha cambiado de " + lastPosition + " a " + transform.position);
            lastPosition = transform.position;
        }
    }
}