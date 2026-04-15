using System;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundry;
    [SerializeField] private Direction direction;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private float additivePos = 2f;
    [SerializeField] private float extraBlockTime = 0.15f;

    private CinemachineConfiner2D confiner;

    private static bool globalTransitionInProgress = false;

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
        if (globalTransitionInProgress)
        {
            return;
        }

        Rigidbody2D hitRb = collision.attachedRigidbody;
        GameObject player = hitRb != null ? hitRb.gameObject : collision.gameObject;

        if (player == null || !player.CompareTag("Player"))
        {
            return;
        }

        _ = FadeTransition(player, hitRb);
    }

    private async Task FadeTransition(GameObject player, Rigidbody2D rb)
    {
        if (globalTransitionInProgress)
        {
            return;
        }

        globalTransitionInProgress = true;

        if (rb == null)
        {
            rb = player.GetComponent<Rigidbody2D>();
        }

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        Vector2 startPos = player.transform.position;
        Vector2 targetPos = GetTargetPosition(startPos);

        try
        {
            PauseController.SetPause(true);

            if (playerInput != null)
            {
                playerInput.DeactivateInput();
            }

            if (playerMovement != null)
            {
                playerMovement.ForceStop();
            }

            if (ScreenFader.Instance != null)
            {
                await ScreenFader.Instance.FadeOut();
            }

            TeleportPlayer(player, rb, targetPos);

            Debug.Log($"{name}: TP REAL de {startPos} a {player.transform.position}");

            if (confiner != null && mapBoundry != null)
            {
                confiner.BoundingShape2D = mapBoundry;
                confiner.InvalidateBoundingShapeCache();
            }

            await Task.Yield();
            await Task.Yield();

            if (ScreenFader.Instance != null)
            {
                await ScreenFader.Instance.FadeIn();
            }

            if (mapBoundry != null)
            {
                MapController_Manual.Instance?.HighlightArea(mapBoundry.name);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error en transición '{name}': {e}");
        }
        finally
        {
            if (playerMovement != null)
            {
                playerMovement.ForceStop();
            }

            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }

            PauseController.SetPause(false);

            if (extraBlockTime > 0f)
            {
                float end = Time.unscaledTime + extraBlockTime;
                while (Time.unscaledTime < end)
                {
                    await Task.Yield();
                }
            }

            globalTransitionInProgress = false;
        }
    }

    private Vector2 GetTargetPosition(Vector2 currentPos)
    {
        if (direction == Direction.Teleport)
        {
            if (teleportTargetPosition != null)
            {
                return teleportTargetPosition.position;
            }

            return currentPos;
        }

        Vector2 newPos = currentPos;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivePos;
                break;

            case Direction.Down:
                newPos.y -= additivePos;
                break;

            case Direction.Left:
                newPos.x -= additivePos;
                break;

            case Direction.Right:
                newPos.x += additivePos;
                break;
        }

        return newPos;
    }

    private void TeleportPlayer(GameObject player, Rigidbody2D rb, Vector2 targetPos)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = targetPos;
        }

        player.transform.position = targetPos;
        Physics2D.SyncTransforms();
    }
}