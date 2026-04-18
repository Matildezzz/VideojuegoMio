using System.Collections;
using UnityEngine;

public sealed class PlayerToolAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Action")]
    [SerializeField] private float hoeDuration = 0.25f;
    [SerializeField] private float pickaxeDuration = 0.3f;
    [SerializeField] private float wateringCanDuration = 0.35f;
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private bool lockMovementWhileAnimating = true;

    private Coroutine currentRoutine;

    private static readonly int IsUsingToolHash = Animator.StringToHash("isUsingTool");
    private static readonly int ToolTypeHash = Animator.StringToHash("toolType");

    private const int HoeAnimatorValue = 0;
    private const int PickaxeAnimatorValue = 1;
    private const int WateringCanAnimatorValue = 2;
    private const int DefaultAnimatorValue = 99;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }

    public void PlayToolAnimation(ToolItemData tool)
    {
        if (animator == null || tool == null)
        {
            return;
        }

        int animatorToolType = GetAnimatorToolType(tool);
        float duration = GetDuration(tool);

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(PlayToolAnimationRoutine(animatorToolType, duration));
    }

    private IEnumerator PlayToolAnimationRoutine(int animatorToolType, float duration)
    {
        if (lockMovementWhileAnimating && playerMovement != null)
        {
            playerMovement.SetMovementLocked(true);
        }

        animator.SetInteger(ToolTypeHash, animatorToolType);
        animator.SetBool(IsUsingToolHash, true);

        yield return new WaitForSeconds(duration);

        animator.SetBool(IsUsingToolHash, false);

        if (lockMovementWhileAnimating && playerMovement != null)
        {
            playerMovement.SetMovementLocked(false);
        }

        currentRoutine = null;
    }

    private int GetAnimatorToolType(ToolItemData tool)
    {
        switch (tool.ToolType)
        {
            case ToolType.Hoe:
                return HoeAnimatorValue;

            case ToolType.Pickaxe:
                return PickaxeAnimatorValue;

            case ToolType.WateringCan:
                return WateringCanAnimatorValue;

            default:
                return DefaultAnimatorValue;
        }
    }

    private float GetDuration(ToolItemData tool)
    {
        switch (tool.ToolType)
        {
            case ToolType.Hoe:
                return hoeDuration;

            case ToolType.Pickaxe:
                return pickaxeDuration;

            case ToolType.WateringCan:
                return wateringCanDuration;

            default:
                return defaultDuration;
        }
    }
}