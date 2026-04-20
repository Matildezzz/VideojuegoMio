using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MuseumPedestalOverlay : MonoBehaviour
{
    private static MuseumPedestalOverlay instance;

    [Header("Referencias UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private Object currentOwner;

    private void Awake()
    {
        instance = this;
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (root != null)
        {
            root.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        currentOwner = null;
    }

    public static void Show(string itemTitle, string itemDescription, Object owner)
    {
        if (instance == null)
        {
            Debug.LogWarning("MuseumPedestalOverlay: no hay instancia en la escena.");
            return;
        }

        if (instance.root != null)
        {
            instance.root.SetActive(true);
        }

        if (instance.titleText != null)
        {
            instance.titleText.text = itemTitle ?? "";
        }

        if (instance.descriptionText != null)
        {
            instance.descriptionText.text = itemDescription ?? "";
        }

        if (instance.canvasGroup != null)
        {
            instance.canvasGroup.alpha = 1f;
        }

        instance.currentOwner = owner;
    }

    public static void Hide(Object owner)
    {
        if (instance == null)
        {
            return;
        }

        if (instance.currentOwner != null && owner != null && instance.currentOwner != owner)
        {
            return;
        }

        if (instance.canvasGroup != null)
        {
            instance.canvasGroup.alpha = 0f;
        }

        if (instance.root != null)
        {
            instance.root.SetActive(false);
        }

        instance.currentOwner = null;
    }
}