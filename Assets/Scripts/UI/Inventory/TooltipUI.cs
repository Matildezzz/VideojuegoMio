using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas parentCanvas;

    [Header("Content")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text extraInfoText;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(18f, -18f);
    [SerializeField] private float screenPadding = 16f;
    [SerializeField] private bool followMouse = true;

    private bool visible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tooltipRoot == null)
        {
            tooltipRoot = gameObject;
        }

        if (tooltipPanel == null && tooltipRoot != null)
        {
            tooltipPanel = tooltipRoot.GetComponent<RectTransform>();
        }

        if (canvasGroup == null && tooltipRoot != null)
        {
            canvasGroup = tooltipRoot.GetComponent<CanvasGroup>();
        }

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.pivot = new Vector2(0f, 1f);
        }

        Hide();
    }

    private void Update()
    {
        if (!visible || !followMouse || Mouse.current == null)
        {
            return;
        }

        UpdatePosition(Mouse.current.position.ReadValue());
    }

    public void Show(ItemData item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        string description = item.Description;

        if (string.IsNullOrWhiteSpace(description))
        {
            description = GetDefaultDescription(item);
        }

        description = AlienNameManager.GetDescription(item, description);

        Show(
            AlienNameManager.GetDisplayName(item),
            description,
            item.Icon,
            BuildExtraInfo(item),
            screenPosition);
    }

    public void Show(string title, string description, Vector2 screenPosition)
    {
        Show(title, description, null, string.Empty, screenPosition);
    }

    public void Show(string title, string description, Sprite icon, string extraInfo, Vector2 screenPosition)
    {
        if (tooltipRoot == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Objeto" : title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(description) ? string.Empty : description;
            descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(description));
        }

        if (extraInfoText != null)
        {
            extraInfoText.text = string.IsNullOrWhiteSpace(extraInfo) ? string.Empty : extraInfo;
            extraInfoText.gameObject.SetActive(!string.IsNullOrWhiteSpace(extraInfo));
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
        }

        tooltipRoot.SetActive(true);
        visible = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        UpdatePosition(screenPosition);
    }

    public void Hide()
    {
        visible = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    private void UpdatePosition(Vector2 screenPosition)
    {
        if (tooltipPanel == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector2 finalPosition = screenPosition + offset;
        float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        float width = tooltipPanel.rect.width * scaleFactor;
        float height = tooltipPanel.rect.height * scaleFactor;

        float minX = screenPadding;
        float maxX = Screen.width - width - screenPadding;
        float minY = height + screenPadding;
        float maxY = Screen.height - screenPadding;

        finalPosition.x = Mathf.Clamp(finalPosition.x, minX, maxX);
        finalPosition.y = Mathf.Clamp(finalPosition.y, minY, maxY);

        tooltipPanel.position = finalPosition;
    }

    private string BuildExtraInfo(ItemData item)
    {
        StringBuilder builder = new StringBuilder();

        string categoryName = GetCategoryName(item.Category);
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            builder.AppendLine("Tipo: " + categoryName);
        }

        int sellPrice = item.GetSellPrice();
        if (sellPrice > 0)
        {
            builder.AppendLine("Precio venta: " + sellPrice);
        }

        ConsumableItemData consumable = item as ConsumableItemData;
        if (consumable != null)
        {
            if (consumable.HealthRestore > 0)
            {
                builder.AppendLine("Restaura vida: +" + consumable.HealthRestore);
            }

            if (consumable.EnergyRestore > 0)
            {
                builder.AppendLine("Restaura energia: +" + consumable.EnergyRestore);
            }
        }

        ToolItemData tool = item as ToolItemData;
        if (tool != null)
        {
            string usage = GetToolUsage(tool.ToolType);
            if (!string.IsNullOrWhiteSpace(usage))
            {
                builder.AppendLine(usage);
            }
        }
        else if (item.Category == ItemCategory.Resource || item.Category == ItemCategory.Material)
        {
            builder.AppendLine("Se puede vender o regalar.");
        }

        return builder.ToString().TrimEnd();
    }

    private string GetDefaultDescription(ItemData item)
    {
        ToolItemData tool = item as ToolItemData;
        if (tool != null)
        {
            return GetDefaultToolDescription(tool.ToolType);
        }

        switch (item.Category)
        {
            case ItemCategory.Consumable:
                return "Objeto consumible que puede recuperar vida o energia.";

            case ItemCategory.Seed:
                return "Semilla que se puede plantar en una parcela preparada.";

            case ItemCategory.Placeable:
                return "Objeto que se puede colocar en el mundo.";

            case ItemCategory.Resource:
                return "Objeto de recurso. Puede servir para vender, regalar o completar misiones.";

            case ItemCategory.Material:
                return "Material util para fabricar, vender o completar misiones.";
        }

        return "Objeto del inventario.";
    }

    private string GetDefaultToolDescription(ToolType toolType)
    {
        switch (toolType)
        {
            case ToolType.Hoe:
                return "Permite preparar la tierra para plantar.";

            case ToolType.WateringCan:
                return "Permite regar parcelas y cultivos.";

            case ToolType.Axe:
                return "Permite cortar arboles u objetos de madera.";

            case ToolType.Pickaxe:
                return "Permite romper rocas y minerales.";

            case ToolType.FishingRod:
                return "Permite pescar en zonas de agua.";

            case ToolType.Weapon:
                return "Permite atacar enemigos.";
        }

        return "Herramienta de trabajo.";
    }

    private string GetToolUsage(ToolType toolType)
    {
        switch (toolType)
        {
            case ToolType.Hoe:
                return "Uso: clic derecho sobre una parcela.";

            case ToolType.WateringCan:
                return "Uso: clic derecho sobre un cultivo o parcela.";

            case ToolType.Axe:
                return "Uso: clic derecho sobre arboles u objetos cortables.";

            case ToolType.Pickaxe:
                return "Uso: clic derecho sobre rocas.";

            case ToolType.FishingRod:
                return "Uso: clic derecho cerca del agua.";

            case ToolType.Weapon:
                return "Uso: clic derecho para atacar.";
        }

        return "Uso: seleccionalo en la hotbar y pulsa clic derecho.";
    }

    private string GetCategoryName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Consumable:
                return "Comida";

            case ItemCategory.Tool:
                return "Herramienta";

            case ItemCategory.Placeable:
                return "Colocable";

            case ItemCategory.Resource:
                return "Recurso";

            case ItemCategory.Seed:
                return "Semilla";

            case ItemCategory.Material:
                return "Material";
        }

        return string.Empty;
    }
}
