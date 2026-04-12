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

    public FarmPlotSaveData CaptureSaveData()
    {
        FarmPlotSaveData saveData = new FarmPlotSaveData();
        saveData.plotKey = SaveKey;
        saveData.isTilled = isTilled;
        saveData.isWateredToday = isWateredToday;
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
        plantedSeedItemId = string.Empty;
        RefreshSoilVisual();
    }

    private void ClearCrop()
    {
        if (currentCrop != null)
        {
            Destroy(currentCrop.gameObject);
            currentCrop = null;
        }

        plantedSeedItemId = string.Empty;
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