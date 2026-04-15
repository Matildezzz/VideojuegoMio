using UnityEngine;
using UnityEngine.UI;

public class MuseumPedestal : MonoBehaviour
{
    [SerializeField] private MuseumItemData requiredItem;
    [SerializeField] private Image displayImage;
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
            displayImage.enabled = donated && requiredItem.Icon != null;
            displayImage.sprite = requiredItem.Icon;
        }
    }
}