using UnityEngine;
using UnityEngine.UI;

public class MuseumPedestal : MonoBehaviour
{
    [SerializeField] private MuseumItemData requiredItem;

    [Header("Visual en mundo")]
    [SerializeField] private SpriteRenderer worldDisplaySprite;

    [Header("Opcional UI")]
    [SerializeField] private Image displayImage;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        if (MuseumController.Instance != null)
        {
            MuseumController.Instance.OnMuseumChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (MuseumController.Instance != null)
        {
            MuseumController.Instance.OnMuseumChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (requiredItem == null)
        {
            return;
        }

        bool donated = MuseumController.Instance != null && MuseumController.Instance.IsDonated(requiredItem);
        bool hasIcon = requiredItem.Icon != null;

        if (displayImage != null)
        {
            displayImage.sprite = requiredItem.Icon;
            displayImage.enabled = hasIcon;
            displayImage.color = donated ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }

        if (worldDisplaySprite != null)
        {
            worldDisplaySprite.sprite = requiredItem.Icon;
            worldDisplaySprite.enabled = hasIcon;
            worldDisplaySprite.color = donated ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }
    }
}