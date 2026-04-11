using UnityEngine;

public sealed class FarmPlot : MonoBehaviour, IInteractable
{
    [Header("Visual suelo")]
    [SerializeField] private SpriteRenderer soilRenderer;
    [SerializeField] private Sprite untilledSprite;
    [SerializeField] private Sprite tilledDrySprite;
    [SerializeField] private Sprite tilledWetSprite;

    [Header("Cultivo")]
    [SerializeField] private Transform cropAnchor;
    [SerializeField] private PlayerInventory playerInventory;

    private CropBehaviour currentCrop;
    private bool isTilled;
    private bool isWateredToday;

    private void Awake()
    {
        if (cropAnchor == null)
        {
            cropAnchor = transform;
        }

        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged += HandleDayChanged;
        }
    }

    private void Start()
    {
        RefreshSoilVisual();
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged -= HandleDayChanged;
        }
    }

    public bool TryTill()
    {
        if (currentCrop != null)
        {
            if (currentCrop.IsWithered)
            {
                ClearCrop();
                return true;
            }

            return false;
        }

        if (isTilled)
        {
            return false;
        }

        isTilled = true;
        isWateredToday = false;
        RefreshSoilVisual();
        return true;
    }

    public bool TryPlant(SeedItemData seed)
    {
        if (seed == null || seed.CropPrefab == null)
        {
            return false;
        }

        if (!isTilled || currentCrop != null)
        {
            return false;
        }

        GameObject cropObject = Instantiate(seed.CropPrefab, cropAnchor.position, Quaternion.identity, cropAnchor);
        cropObject.transform.localPosition = Vector3.zero;

        CropBehaviour crop = cropObject.GetComponent<CropBehaviour>();
        if (crop == null)
        {
            Debug.LogWarning("FarmPlot: el CropPrefab no tiene CropBehaviour.");
            Destroy(cropObject);
            return false;
        }

        currentCrop = crop;
        currentCrop.OnPlanted();
        isWateredToday = false;
        RefreshSoilVisual();

        return true;
    }

    public bool TryWater()
    {
        if (!isTilled || currentCrop == null)
        {
            return false;
        }

        if (currentCrop.IsWithered || currentCrop.IsHarvestable)
        {
            return false;
        }

        if (isWateredToday)
        {
            return false;
        }

        isWateredToday = true;
        RefreshSoilVisual();
        return true;
    }

    public bool CanInteract()
    {
        return currentCrop != null && (currentCrop.IsHarvestable || currentCrop.IsWithered);
    }

    public void Interact()
    {
        if (currentCrop == null)
        {
            return;
        }

        if (currentCrop.IsWithered)
        {
            ClearCrop();
            return;
        }

        if (!currentCrop.IsHarvestable)
        {
            return;
        }

        if (playerInventory == null)
        {
            return;
        }

        bool harvested = currentCrop.TryHarvest(playerInventory);

        if (harvested)
        {
            ClearCrop();
        }
    }

    private void HandleDayChanged(int newDay)
    {
        if (currentCrop != null)
        {
            currentCrop.AdvanceDay(isWateredToday);
        }

        isWateredToday = false;
        RefreshSoilVisual();
    }

    private void ClearCrop()
    {
        if (currentCrop != null)
        {
            Destroy(currentCrop.gameObject);
            currentCrop = null;
        }

        isWateredToday = false;
        RefreshSoilVisual();
    }

    private void RefreshSoilVisual()
    {
        if (soilRenderer == null)
        {
            return;
        }

        if (!isTilled)
        {
            soilRenderer.sprite = untilledSprite;
            return;
        }

        soilRenderer.sprite = isWateredToday ? tilledWetSprite : tilledDrySprite;
    }
}