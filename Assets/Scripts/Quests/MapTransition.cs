using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundry;
    [SerializeField] private Direction direction;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private float additivePos = 2f;
    [SerializeField] private float transitionCooldown = 0.2f;

    private CinemachineConfiner2D confiner;
    private bool isTransitioning = false;

    private static float lastTransitionTime = -999f;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Teleport
    }

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (isTransitioning)
        {
            return;
        }

        if (Time.unscaledTime - lastTransitionTime < transitionCooldown)
        {
            return;
        }

        _ = FadeTransition(collision.gameObject);
        MapController_Manual.Instance?.HighlightArea(mapBoundry.name);
    }

    private async Task FadeTransition(GameObject player)
    {
        isTransitioning = true;
        lastTransitionTime = Time.unscaledTime;

        PauseController.SetPause(true);

        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        if (confiner != null && mapBoundry != null)
        {
            confiner.BoundingShape2D = mapBoundry;
            confiner.InvalidateBoundingShapeCache();
        }

        UpdatePlayerPosition(player);

        await Task.Yield();
        await Task.Yield();

        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }

        PauseController.SetPause(false);
        isTransitioning = false;
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector2 targetPos = player.transform.position;

        if (direction == Direction.Teleport)
        {
            if (teleportTargetPosition != null)
            {
                targetPos = teleportTargetPosition.position;
            }
        }
        else
        {
            Vector2 newPos = player.transform.position;

            switch (direction)
            {
                case Direction.Up:
                    newPos.y += additivePos;
                    break;

                case Direction.Down:
                    newPos.y -= additivePos;
                    break;

                case Direction.Left:
                    newPos.x += additivePos;
                    break;

                case Direction.Right:
                    newPos.x -= additivePos;
                    break;
            }

            targetPos = newPos;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = targetPos;
        }
        else
        {
            player.transform.position = targetPos;
        }
    }
}