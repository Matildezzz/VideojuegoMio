using UnityEngine;
using UnityEngine.UI;

public class MuseumPedestal : MonoBehaviour, IProximityInfo
{
    [SerializeField] private MuseumItemData requiredItem;

    [Header("Visual en mundo")]
    [SerializeField] private SpriteRenderer worldDisplaySprite;

    [Header("Opcional UI")]
    [SerializeField] private Image displayImage;

    [Header("Texto cuando NO está donado")]
    [SerializeField] private string unknownTitle = "?????";
    [SerializeField] [TextArea] private string unknownDescription = "Todavía no has encontrado este objeto";

    private bool isSubscribed;

    private void Awake()
    {
        if (worldDisplaySprite == null)
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].gameObject != gameObject)
                {
                    worldDisplaySprite = renderers[i];
                    break;
                }
            }
        }
    }

    private void Start()
    {
        SubscribeToMuseum();
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromMuseum();
    }

    private void SubscribeToMuseum()
    {
        if (isSubscribed)
        {
            return;
        }

        if (MuseumController.Instance == null)
        {
            return;
        }

        MuseumController.Instance.OnMuseumChanged += Refresh;
        isSubscribed = true;
    }

    private void UnsubscribeFromMuseum()
    {
        if (!isSubscribed)
        {
            return;
        }

        if (MuseumController.Instance != null)
        {
            MuseumController.Instance.OnMuseumChanged -= Refresh;
        }

        isSubscribed = false;
    }

    public void Refresh()
    {
        SubscribeToMuseum();

        Sprite icon = requiredItem != null ? requiredItem.Icon : null;
        bool donated = MuseumController.Instance != null && MuseumController.Instance.IsDonated(requiredItem);
        bool shouldShowObject = donated && icon != null;

        if (displayImage != null)
        {
            displayImage.sprite = icon;
            displayImage.enabled = shouldShowObject;
            displayImage.color = Color.white;
        }

        if (worldDisplaySprite != null)
        {
            worldDisplaySprite.sprite = icon;
            worldDisplaySprite.enabled = shouldShowObject;
            worldDisplaySprite.color = Color.white;
        }
    }

    public void OnEnterProximity()
    {
        if (requiredItem == null)
        {
            return;
        }

        bool donated = MuseumController.Instance != null && MuseumController.Instance.IsDonated(requiredItem);

        if (donated)
        {
            string title = string.IsNullOrWhiteSpace(requiredItem.MuseumDisplayName)
                ? requiredItem.DisplayName
                : requiredItem.MuseumDisplayName;

            MuseumPedestalOverlay.Show(title, requiredItem.Description, this);
        }
        else
        {
            MuseumPedestalOverlay.Show(unknownTitle, unknownDescription, this);
        }
    }

    public void OnExitProximity()
    {
        MuseumPedestalOverlay.Hide(this);
    }
}