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
    public ItemData HarvestItem => harvestItem;

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
        Debug.Log("CropBehaviour -> AdvanceDay. wasWateredToday = " + wasWateredToday + ", stageIndex actual = " + stageIndex);

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

                Debug.Log("CropBehaviour -> wateredDaysInCurrentStage = " + wateredDaysInCurrentStage + ", requiredDays = " + requiredDays);

                if (wateredDaysInCurrentStage >= requiredDays)
                {
                    wateredDaysInCurrentStage = 0;
                    stageIndex++;

                    if (stageIndex >= stageSprites.Length - 1)
                    {
                        stageIndex = stageSprites.Length - 1;
                        isHarvestable = true;
                    }

                    Debug.Log("CropBehaviour -> nuevo stageIndex = " + stageIndex);
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
        int harvestedAmount;
        return TryHarvest(receiver, out harvestedAmount);
    }

    public bool TryHarvest(IItemReceiver receiver, out int harvestedAmount)
    {
        harvestedAmount = 0;

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

        bool added = receiver.TryAddItem(harvestItem, amount);

        if (added)
        {
            harvestedAmount = amount;
        }

        return added;
    }

    public CropSaveData CaptureSaveData()
    {
        CropSaveData saveData = new CropSaveData();
        saveData.stageIndex = stageIndex;
        saveData.wateredDaysInCurrentStage = wateredDaysInCurrentStage;
        saveData.dryDaysWithoutWater = dryDaysWithoutWater;
        saveData.isHarvestable = isHarvestable;
        saveData.isWithered = isWithered;
        return saveData;
    }

    public void RestoreFromSaveData(CropSaveData saveData)
    {
        if (saveData == null)
        {
            OnPlanted();
            return;
        }

        int maxStage = 0;
        if (stageSprites != null && stageSprites.Length > 0)
        {
            maxStage = stageSprites.Length - 1;
        }

        stageIndex = Mathf.Clamp(saveData.stageIndex, 0, maxStage);
        wateredDaysInCurrentStage = Mathf.Max(0, saveData.wateredDaysInCurrentStage);
        dryDaysWithoutWater = Mathf.Max(0, saveData.dryDaysWithoutWater);
        isWithered = saveData.isWithered;
        isHarvestable = !isWithered && saveData.isHarvestable;

        RefreshVisual();
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