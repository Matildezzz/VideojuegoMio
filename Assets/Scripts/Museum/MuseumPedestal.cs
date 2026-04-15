using UnityEngine;
using UnityEngine.UI;

public class MuseumPedestal : MonoBehaviour
{
    [SerializeField] private MuseumItemData requiredItem;

    [Header("Visual en mundo")]
    [SerializeField] private SpriteRenderer worldDisplaySprite;

    [Header("Opcional UI")]
    [SerializeField] private Image displayImage;

    [Header("Estados")]
    [SerializeField] private GameObject donatedVisual;
    [SerializeField] private GameObject missingVisual;

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

        if (donatedVisual != null)
        {
            donatedVisual.SetActive(donated);
        }

        if (missingVisual != null)
        {
            missingVisual.SetActive(!donated);
        }

        if (displayImage != null)
        {
            displayImage.sprite = requiredItem.Icon;
            displayImage.enabled = donated && hasIcon;
        }

        if (worldDisplaySprite != null)
        {
            worldDisplaySprite.sprite = requiredItem.Icon;
            worldDisplaySprite.enabled = donated && hasIcon;
        }
    }
}