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
    }

    private async Task FadeTransition(GameObject player)
    {
        isTransitioning = true;
        lastTransitionTime = Time.unscaledTime;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

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

            if (confiner != null && mapBoundry != null)
            {
                confiner.BoundingShape2D = mapBoundry;
                confiner.InvalidateBoundingShapeCache();
            }

            UpdatePlayerPosition(player, rb);
            Physics2D.SyncTransforms();

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
            isTransitioning = false;
        }
    }

    private void UpdatePlayerPosition(GameObject player, Rigidbody2D rb)
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
                    newPos.x -= additivePos;
                    break;

                case Direction.Right:
                    newPos.x += additivePos;
                    break;
            }

            targetPos = newPos;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = targetPos;
        }

        player.transform.position = targetPos;
    }
}