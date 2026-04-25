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

    [Header("Feedback")]
    [SerializeField] private ParticleSystem waterParticles;

    private bool isCurrentlyWet;
    private int wetUntilTotalMinutes = -1;

    public string SaveKey => BuildSaveKey();

    public string InteractionText
    {
        get
        {
            if (currentCrop != null && currentCrop.IsWithered)
            {
                return "E - Quitar cultivo seco";
            }

            if (currentCrop != null && currentCrop.IsHarvestable)
            {
                string cropName = currentCrop.HarvestItem != null ? currentCrop.HarvestItem.DisplayName : "cultivo";
                return "E - Cosechar " + cropName;
            }

            if (currentCrop != null)
            {
                return "E - Revisar cultivo";
            }

            return "E - Interactuar con parcela";
        }
    }


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
                ShowSuccess("Cultivo seco eliminado", "Hoe");
                return true;
            }

            ShowError("No puedes labrar una parcela con cultivo.");
            return false;
        }

        if (isTilled)
        {
            ShowWarning("Esta parcela ya está preparada.");
            return false;
        }

        isTilled = true;
        isWateredToday = false;
        isCurrentlyWet = false;
        wetUntilTotalMinutes = -1;
        RefreshSoilVisual();

        ShowSuccess("Tierra labrada", "Hoe");

        return true;
    }

    public bool TryPlant(SeedItemData seed)
    {
        if (seed == null || seed.CropPrefab == null)
        {
            ShowError("Esta semilla no tiene cultivo asignado.");
            return false;
        }

        if (!isTilled)
        {
            ShowError("Necesitas preparar la tierra antes de plantar.");
            return false;
        }

        if (currentCrop != null)
        {
            ShowError("Ya hay un cultivo plantado aquí.");
            return false;
        }

        GameObject cropObject = Instantiate(seed.CropPrefab, cropAnchor.position, Quaternion.identity, cropAnchor);
        cropObject.transform.localPosition = Vector3.zero;

        CropBehaviour crop = cropObject.GetComponent<CropBehaviour>();
        if (crop == null)
        {
            Debug.LogWarning("FarmPlot: el CropPrefab no tiene CropBehaviour.");
            Destroy(cropObject);
            ShowError("El cultivo de esta semilla no está configurado.");
            return false;
        }

        currentCrop = crop;
        currentCrop.OnPlanted();
        plantedSeedItemId = seed.ItemId;
        isWateredToday = false;
        RefreshSoilVisual();

        ShowSuccess("Has plantado " + seed.DisplayName, "Plant");

        return true;
    }

    public bool TryWater()
    {
        if (!isTilled)
        {
            ShowError("Necesitas preparar la tierra antes de regar.");
            return false;
        }

        if (currentCrop == null)
        {
            ShowError("No hay ningún cultivo para regar.");
            return false;
        }

        if (currentCrop.IsWithered)
        {
            ShowWarning("Este cultivo está seco. Quítalo con la azada.");
            return false;
        }

        if (currentCrop.IsHarvestable)
        {
            ShowWarning("Este cultivo ya está listo para cosechar.");
            return false;
        }

        if (isCurrentlyWet)
        {
            ShowWarning("Este cultivo ya está regado.");
            return false;
        }

        if (!isWateredToday)
        {
            isWateredToday = true;
        }

        isCurrentlyWet = true;
        wetUntilTotalMinutes = GetCurrentTotalMinutes() + (Mathf.Max(1, wetDurationHours) * 60);

        RefreshSoilVisual();

        if (waterParticles != null)
        {
            waterParticles.Play();
        }

        ShowSuccess("Cultivo regado", "Water");

        return true;
    }

    public bool CanInteract()
    {
        return currentCrop != null;
    }

    public void Interact()
    {
        if (currentCrop == null)
        {
            ShowWarning("No hay cultivo en esta parcela.");
            return;
        }

        if (currentCrop.IsWithered)
        {
            ClearCrop();
            ShowSuccess("Cultivo seco eliminado", "Hoe");
            return;
        }

        if (!currentCrop.IsHarvestable)
        {
            if (!isWateredToday && !isCurrentlyWet)
            {
                ShowWarning("Necesitas regar este cultivo.");
            }
            else
            {
                ShowWarning("Este cultivo aún no está listo.");
            }

            return;
        }

        if (playerInventory == null)
        {
            ShowError("No se encontró el inventario del jugador.");
            return;
        }

        ItemData harvestedItem = currentCrop.HarvestItem;
        int harvestedAmount;
        bool harvested = currentCrop.TryHarvest(playerInventory, out harvestedAmount);

        if (harvested)
        {
            if (harvestedItem != null)
            {
                ShowSuccess("+" + harvestedAmount + " " + harvestedItem.DisplayName, "Harvest");
            }

            ClearCrop();
        }
        else
        {
            ShowError("No hay espacio en el inventario.");
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

    private void ShowSuccess(string message, string soundName)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Success, soundName);
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowWarning(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowError(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Error, "Error");
        }
        else
        {
            Debug.Log(message);
        }
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