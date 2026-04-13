using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool IsOpened { get; private set;}
    public string ChestID { get; private set;}
    public GameObject itemPrefab; // Item that chest drops
    public Sprite openedSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenChest();
    }

    public void OpenChest()
    {
        SetOpened(true);
        SoundEffectManager.Play("Chest");

        // DropItem
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    public void SetOpened(bool opened)
    {
        IsOpened = opened;
        if (IsOpened == opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}
