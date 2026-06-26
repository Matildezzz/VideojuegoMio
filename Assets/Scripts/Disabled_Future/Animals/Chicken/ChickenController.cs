using UnityEngine;

public class ChickenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AnimalArea animalArea;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private float distanceToTarget = 0.1f;

    [Header("Sprite")]
    [SerializeField] private bool spriteLooksRight = true;

    private Vector2 targetPosition;
    private float waitTimer;
    private bool isWaiting;
    private bool isSleeping;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        ChooseNewTarget();
    }

    private void FixedUpdate()
    {
        if (isSleeping)
        {
            rb.linearVelocity = Vector2.zero;
            SetWalking(false);
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                ChooseNewTarget();
            }

            SetWalking(false);
            return;
        }

        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector2 currentPosition = rb.position;
        Vector2 direction = targetPosition - currentPosition;

        if (direction.magnitude <= distanceToTarget)
        {
            StartWaiting();
            return;
        }

        direction.Normalize();

        Vector2 newPosition = currentPosition + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        UpdateFlip(direction);
        SetWalking(true);
    }

    private void ChooseNewTarget()
    {
        if (animalArea == null)
        {
            return;
        }

        targetPosition = animalArea.GetRandomPoint();
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    private void UpdateFlip(Vector2 direction)
    {
        if (direction.x == 0)
        {
            return;
        }

        if (spriteLooksRight)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        else
        {
            spriteRenderer.flipX = direction.x > 0;
        }
    }

    private void SetWalking(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", value);
        }
    }

    public void SetSleeping(bool value)
    {
        isSleeping = value;

        if (animator != null)
        {
            animator.SetBool("IsSleeping", value);
        }
    }
}