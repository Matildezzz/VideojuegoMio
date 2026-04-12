using UnityEngine;
using UnityEngine.SceneManagement;

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
    private string plantedSeedItemId = string.Empty;

    [SerializeField] private int wetDurationHours = 6;

    private bool isCurrentlyWet;
    private int wetUntilTotalMinutes = -1;

    public string SaveKey => BuildSaveKey();

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

    private void Update()
    {
        UpdateWetStateOverTime();
    }

    private void UpdateWetStateOverTime()
    {
        if (!isCurrentlyWet)
        {
            return;
        }

        if (TimeManager.Instance == null)
        {
            return;
        }

        int currentTotalMinutes = GetCurrentTotalMinutes();

        if (currentTotalMinutes >= wetUntilTotalMinutes)
        {
            isCurrentlyWet = false;
            RefreshSoilVisual();
        }
    }

    private int GetCurrentTotalMinutes()
    {
        if (TimeManager.Instance == null)
        {
            return 0;
        }

        return ((TimeManager.Instance.Day - 1) * 1440)
            + (TimeManager.Instance.Hour * 60)
            + TimeManager.Instance.Minute;
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged += HandleDayChanged;
        }
        else
        {
            Debug.LogWarning("FarmPlot: no se encontró TimeManager al iniciar.");
        }

        RefreshSoilVisual();
    }

    private void OnDestroy()
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
        isCurrentlyWet = false;
        wetUntilTotalMinutes = -1;
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
        plantedSeedItemId = seed.ItemId;
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

        if (isCurrentlyWet)
        {
            return false;
        }

        if (!isWateredToday)
        {
            isWateredToday = true;
        }

        isCurrentlyWet = true;
        wetUntilTotalMinutes = GetCurrentTotalMinutes() + (Mathf.Max(1, wetDurationHours) * 60);

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
        Debug.Log("FarmPlot -> cambio de día recibido. Día: " + newDay);

        if (currentCrop != null)
        {
            Debug.Log("FarmPlot -> avanzando cultivo. Regado hoy: " + isWateredToday);
            currentCrop.AdvanceDay(isWateredToday);
        }

        isWateredToday = false;
        RefreshSoilVisual();
    }

    public FarmPlotSaveData CaptureSaveData()
    {
        FarmPlotSaveData saveData = new FarmPlotSaveData();
        saveData.plotKey = SaveKey;
        saveData.isTilled = isTilled;
        saveData.isWateredToday = isWateredToday;
        saveData.isCurrentlyWet = isCurrentlyWet;
        saveData.wetUntilTotalMinutes = wetUntilTotalMinutes;
        saveData.hasCrop = currentCrop != null;
        saveData.seedItemId = plantedSeedItemId;

        if (currentCrop != null)
        {
            saveData.crop = currentCrop.CaptureSaveData();
        }

        return saveData;
    }

    public void RestoreFromSaveData(FarmPlotSaveData saveData, ItemDatabase itemDatabase)
    {
        ResetPlotState();

        if (saveData == null)
        {
            return;
        }

        isTilled = saveData.isTilled || saveData.hasCrop;
        isWateredToday = saveData.isWateredToday;
        isCurrentlyWet = saveData.isCurrentlyWet;
        wetUntilTotalMinutes = saveData.wetUntilTotalMinutes;
        plantedSeedItemId = saveData.seedItemId;

        if (saveData.hasCrop && !string.IsNullOrWhiteSpace(saveData.seedItemId))
        {
            ItemData itemData = itemDatabase.GetItemById(saveData.seedItemId);
            SeedItemData seed = itemData as SeedItemData;

            if (seed == null || seed.CropPrefab == null)
            {
                Debug.LogWarning("FarmPlot: no se pudo reconstruir el cultivo para la semilla -> " + saveData.seedItemId);
                RefreshSoilVisual();
                return;
            }

            GameObject cropObject = Instantiate(seed.CropPrefab, cropAnchor.position, Quaternion.identity, cropAnchor);
            cropObject.transform.localPosition = Vector3.zero;

            CropBehaviour crop = cropObject.GetComponent<CropBehaviour>();
            if (crop == null)
            {
                Debug.LogWarning("FarmPlot: el CropPrefab no tiene CropBehaviour al restaurar.");
                Destroy(cropObject);
                RefreshSoilVisual();
                return;
            }

            currentCrop = crop;
            currentCrop.RestoreFromSaveData(saveData.crop);
        }

        if (TimeManager.Instance != null && isCurrentlyWet)
        {
            int currentTotalMinutes = GetCurrentTotalMinutes();

            if (currentTotalMinutes >= wetUntilTotalMinutes)
            {
                isCurrentlyWet = false;
            }
        }

        RefreshSoilVisual();
    }

    public void ResetPlotState()
    {
        if (currentCrop != null)
        {
            if (Application.isPlaying)
            {
                Destroy(currentCrop.gameObject);
            }
            else
            {
                DestroyImmediate(currentCrop.gameObject);
            }

            currentCrop = null;
        }

        isTilled = false;
        isWateredToday = false;
        isCurrentlyWet = false;
        wetUntilTotalMinutes = -1;
        plantedSeedItemId = string.Empty;
        RefreshSoilVisual();
    }

    private void ClearCrop()
    {
        if (currentCrop != null)
        {
            if (Application.isPlaying)
            {
                Destroy(currentCrop.gameObject);
            }
            else
            {
                DestroyImmediate(currentCrop.gameObject);
            }

            currentCrop = null;
        }

        plantedSeedItemId = string.Empty;
        isWateredToday = false;
        isCurrentlyWet = false;
        wetUntilTotalMinutes = -1;
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

        soilRenderer.sprite = isCurrentlyWet ? tilledWetSprite : tilledDrySprite;
    }

    private string BuildSaveKey()
    {
        Vector3 position = transform.position;
        int x = Mathf.RoundToInt(position.x * 100f);
        int y = Mathf.RoundToInt(position.y * 100f);
        int z = Mathf.RoundToInt(position.z * 100f);

        string sceneName = gameObject.scene.IsValid()
            ? gameObject.scene.name
            : SceneManager.GetActiveScene().name;

        return sceneName + "_" + x + "_" + y + "_" + z;
    }
}