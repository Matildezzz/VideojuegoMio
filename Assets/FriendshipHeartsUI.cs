using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FriendshipHeartsUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Imagenes de corazones")]
    [SerializeField] private Image[] heartImages = new Image[5];

    [Header("Sprites")]
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite fullHeartSprite;

    [Header("Opcional")]
    [SerializeField] private TMP_Text heartsValueText;

    private NPCFriendship currentFriendship;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        Hide();
    }

    public void Show(NPCFriendship friendship)
    {
        currentFriendship = friendship;

        if (root != null)
        {
            root.SetActive(true);
        }

        Refresh();
    }

    public void Hide()
    {
        currentFriendship = null;
        ClearVisuals();

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Refresh()
    {
        if (currentFriendship == null)
        {
            ClearVisuals();
            return;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            Image heartImage = heartImages[i];

            if (heartImage == null)
            {
                continue;
            }

            heartImage.enabled = true;
            heartImage.sprite = GetSpriteForHeart(i);
            heartImage.SetNativeSize();
        }

        if (heartsValueText != null)
        {
            heartsValueText.text = currentFriendship.CurrentHearts.ToString("0.0") + " / 5";
        }
    }

    private Sprite GetSpriteForHeart(int heartIndex)
    {
        int startHalfIndex = heartIndex * 2;
        int remainingHalfHearts = currentFriendship.CurrentHalfHearts - startHalfIndex;

        if (remainingHalfHearts >= 2)
        {
            return fullHeartSprite;
        }

        if (remainingHalfHearts == 1)
        {
            return halfHeartSprite;
        }

        return emptyHeartSprite;
    }

    private void ClearVisuals()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                continue;
            }

            heartImages[i].enabled = emptyHeartSprite != null;
            heartImages[i].sprite = emptyHeartSprite;
        }

        if (heartsValueText != null)
        {
            heartsValueText.text = "0.0 / 5";
        }
    }
}
