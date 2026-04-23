using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatMenuController : MonoBehaviour
{
    public static bool GodModeEnabled { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject cheatPanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private bool pauseGameWhenOpen = true;

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private SaveController saveController;
    [SerializeField] private NPCFriendship debugNpc;

    [Header("Settings")]
    [SerializeField] private bool allowCheatsOutsideDevelopmentBuild = true;
    [SerializeField] private int defaultSeedAmount = 25;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (saveController == null)
        {
            saveController = FindFirstObjectByType<SaveController>();
        }

        if (cheatPanel != null)
        {
            cheatPanel.SetActive(false);
        }
    }

    private void Start()
    {
        WriteStatus("F2 = abrir/cerrar menú de cheats");
    }

    private void Update()
    {
        if (!CanUseCheats())
        {
            return;
        }

        if (Keyboard.current == null || cheatPanel == null)
        {
            return;
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    private bool CanUseCheats()
    {
        if (allowCheatsOutsideDevelopmentBuild)
        {
            return true;
        }

#if UNITY_EDITOR
        return true;
#else
        return Debug.isDebugBuild;
#endif
    }

    public void ToggleMenu()
    {
        if (cheatPanel == null)
        {
            return;
        }

        if (!cheatPanel.activeSelf && PauseController.IsGamePaused)
        {
            return;
        }

        bool open = !cheatPanel.activeSelf;
        cheatPanel.SetActive(open);

        if (pauseGameWhenOpen)
        {
            PauseController.SetPause(open);
        }

        RefreshStatus();
    }

    public void CloseMenu()
    {
        if (cheatPanel == null)
        {
            return;
        }

        cheatPanel.SetActive(false);

        if (pauseGameWhenOpen)
        {
            PauseController.SetPause(false);
        }

        RefreshStatus();
    }

    public void ToggleGodMode()
    {
        GodModeEnabled = !GodModeEnabled;
        RefreshStatus("God Mode: " + (GodModeEnabled ? "ON" : "OFF"));
    }

    public void Add100Gold()
    {
        AddGold(100);
    }

    public void Add1000Gold()
    {
        AddGold(1000);
    }

    public void SetGold99999()
    {
        if (CurrencyController.Instance == null)
        {
            WriteStatus("No se encontró CurrencyController.");
            return;
        }

        CurrencyController.Instance.SetGold(99999);
        RefreshStatus("Gold puesto a 99999");
    }

    private void AddGold(int amount)
    {
        if (CurrencyController.Instance == null)
        {
            WriteStatus("No se encontró CurrencyController.");
            return;
        }

        CurrencyController.Instance.AddGold(amount);
        RefreshStatus("Añadido oro: +" + amount);
    }

    public void FullHealth()
    {
        if (playerHealth == null)
        {
            WriteStatus("No se encontró PlayerHealth.");
            return;
        }

        playerHealth.RestoreFullHealthFromCheat();
        RefreshStatus("Vida restaurada al máximo");
    }

    public void GiveBasicSeeds()
    {
        GiveItemById("carrotSeed", 25);
        GiveItemById("potatoSeed", 25);
        GiveItemById("cornSeed", 25);

        RefreshStatus("Pack básico de semillas añadido");
    }

    public void GiveAllSeeds()
    {
        if (playerInventory == null)
        {
            WriteStatus("No se encontró PlayerInventory.");
            return;
        }

        if (itemDatabase == null)
        {
            WriteStatus("Falta asignar ItemDatabase.");
            return;
        }

        int addedTypes = 0;

        for (int i = 0; i < itemDatabase.Items.Count; i++)
        {
            ItemData item = itemDatabase.Items[i];

            if (item == null)
            {
                continue;
            }

            if (item is SeedItemData)
            {
                int leftover = playerInventory.AddItemAndReturnLeftover(item, defaultSeedAmount);

                if (leftover < defaultSeedAmount)
                {
                    addedTypes++;
                }
            }
        }

        RefreshStatus("Semillas añadidas. Tipos metidos: " + addedTypes);
    }

    public void SetMorning()
    {
        if (TimeManager.Instance == null)
        {
            WriteStatus("No se encontró TimeManager.");
            return;
        }

        TimeManager.Instance.SetTime(TimeManager.Instance.Day, 8, 0);
        RefreshStatus("Hora cambiada a 08:00");
    }

    public void SetNight()
    {
        if (TimeManager.Instance == null)
        {
            WriteStatus("No se encontró TimeManager.");
            return;
        }

        TimeManager.Instance.SetTime(TimeManager.Instance.Day, 22, 0);
        RefreshStatus("Hora cambiada a 22:00");
    }

    public void NextDay()
    {
        if (TimeManager.Instance == null)
        {
            WriteStatus("No se encontró TimeManager.");
            return;
        }

        TimeManager.Instance.SetTime(TimeManager.Instance.Day + 1, 8, 0);
        RefreshStatus("Avanzado al día siguiente");
    }

    public void SaveGameNow()
    {
        if (saveController == null)
        {
            WriteStatus("No se encontró SaveController.");
            return;
        }

        saveController.SaveGame();
        RefreshStatus("Partida guardada");
    }

    public void AddFriendshipOneHeart()
    {
        if (debugNpc == null)
        {
            WriteStatus("No has asignado un NPCFriendship de prueba.");
            return;
        }

        debugNpc.AddHalfHearts(2);
        RefreshStatus("Amistad +1 corazón");
    }

    public void MaxFriendship()
    {
        if (debugNpc == null)
        {
            WriteStatus("No has asignado un NPCFriendship de prueba.");
            return;
        }

        debugNpc.AddHalfHearts(999);
        RefreshStatus("Amistad al máximo");
    }

    private void GiveItemById(string itemId, int amount)
    {
        if (playerInventory == null)
        {
            WriteStatus("No se encontró PlayerInventory.");
            return;
        }

        if (itemDatabase == null)
        {
            WriteStatus("Falta asignar ItemDatabase.");
            return;
        }

        ItemData item = itemDatabase.GetItemById(itemId);

        if (item == null)
        {
            WriteStatus("No existe itemId: " + itemId);
            return;
        }

        int leftover = playerInventory.AddItemAndReturnLeftover(item, amount);

        if (leftover > 0)
        {
            WriteStatus("Inventario lleno. Sobran " + leftover + " de " + item.DisplayName);
            return;
        }

        WriteStatus("Añadido: " + item.DisplayName + " x" + amount);
    }

    private void RefreshStatus(string message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            WriteStatus(message);
            return;
        }

        int gold = CurrencyController.Instance != null ? CurrencyController.Instance.GetGold() : 0;

        WriteStatus(
            "God Mode: " + (GodModeEnabled ? "ON" : "OFF") +
            "\nGold: " + gold
        );
    }

    private void WriteStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log("[CHEATS] " + message);
    }
}