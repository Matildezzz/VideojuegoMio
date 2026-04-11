using UnityEngine;

public sealed class CropBehaviour : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] stageSprites;
    [SerializeField] private Sprite witheredSprite;

    [Header("Growth")]
    [SerializeField] private int[] wateredDaysPerStage;
    [SerializeField] private int maxDryDaysWithoutWater = 2;

    [Header("Harvest")]
    [SerializeField] private ItemData harvestItem;
    [SerializeField] private int minHarvestAmount = 1;
    [SerializeField] private int maxHarvestAmount = 1;

    private int stageIndex;
    private int wateredDaysInCurrentStage;
    private int dryDaysWithoutWater;
    private bool isHarvestable;
    private bool isWithered;

    public bool IsHarvestable => isHarvestable;
    public bool IsWithered => isWithered;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public void OnPlanted()
    {
        stageIndex = 0;
        wateredDaysInCurrentStage = 0;
        dryDaysWithoutWater = 0;
        isHarvestable = false;
        isWithered = false;

        RefreshVisual();
    }

    public void AdvanceDay(bool wasWateredToday)
    {
        if (isHarvestable || isWithered)
        {
            return;
        }

        if (wasWateredToday)
        {
            dryDaysWithoutWater = 0;

            if (stageSprites != null && stageSprites.Length > 1)
            {
                wateredDaysInCurrentStage++;

                int requiredDays = GetRequiredWateredDaysForCurrentStage();

                if (wateredDaysInCurrentStage >= requiredDays)
                {
                    wateredDaysInCurrentStage = 0;
                    stageIndex++;

                    if (stageIndex >= stageSprites.Length - 1)
                    {
                        stageIndex = stageSprites.Length - 1;
                        isHarvestable = true;
                    }

                    RefreshVisual();
                }
            }
        }
        else
        {
            dryDaysWithoutWater++;

            if (dryDaysWithoutWater >= Mathf.Max(1, maxDryDaysWithoutWater))
            {
                isWithered = true;
                RefreshVisual();
            }
        }
    }

    public bool TryHarvest(IItemReceiver receiver)
    {
        if (!isHarvestable)
        {
            return false;
        }

        if (receiver == null || harvestItem == null)
        {
            return false;
        }

        int min = Mathf.Max(1, minHarvestAmount);
        int max = Mathf.Max(min, maxHarvestAmount);
        int amount = Random.Range(min, max + 1);

        return receiver.TryAddItem(harvestItem, amount);
    }

    private int GetRequiredWateredDaysForCurrentStage()
    {
        if (wateredDaysPerStage == null || wateredDaysPerStage.Length == 0)
        {
            return 1;
        }

        int index = Mathf.Clamp(stageIndex, 0, wateredDaysPerStage.Length - 1);
        return Mathf.Max(1, wateredDaysPerStage[index]);
    }

    private void RefreshVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (isWithered && witheredSprite != null)
        {
            spriteRenderer.sprite = witheredSprite;
            return;
        }

        if (stageSprites == null || stageSprites.Length == 0)
        {
            return;
        }

        int index = Mathf.Clamp(stageIndex, 0, stageSprites.Length - 1);
        spriteRenderer.sprite = stageSprites[index];
    }
}