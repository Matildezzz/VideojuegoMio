using System;
using System.Collections.Generic;
using UnityEngine;

public class MuseumController : MonoBehaviour
{
    public static MuseumController Instance { get; private set; }

    public event Action OnMuseumChanged;

    [SerializeField] private ItemDatabase itemDatabase;

    [SerializeField] private List<string> donatedItemIds = new List<string>();

    public IReadOnlyList<string> DonatedItemIds => donatedItemIds;

    private HashSet<string> donatedLookup = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RebuildLookup();
    }

    private void RebuildLookup()
    {
        donatedLookup.Clear();

        for (int i = 0; i < donatedItemIds.Count; i++)
        {
            string id = donatedItemIds[i];

            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            donatedLookup.Add(id);
        }
    }

    public bool IsDonated(ItemData item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
        {
            return false;
        }

        return donatedLookup.Contains(item.ItemId);
    }

    public bool CanDonate(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (!(item is MuseumItemData))
        {
            return false;
        }

        return !IsDonated(item);
    }

    public bool DonateItem(PlayerInventory inventory, ItemData item)
    {
        if (inventory == null || item == null)
        {
            return false;
        }

        if (!CanDonate(item))
        {
            return false;
        }

        if (!inventory.HasItem(item, 1))
        {
            return false;
        }

        bool removed = inventory.RemoveItem(item, 1);

        if (!removed)
        {
            return false;
        }

        donatedItemIds.Add(item.ItemId);
        donatedLookup.Add(item.ItemId);

        MuseumItemData museumItem = item as MuseumItemData;
        if (museumItem != null && museumItem.DonationRewardGold > 0 && CurrencyController.Instance != null)
        {
            CurrencyController.Instance.AddGold(museumItem.DonationRewardGold);
        }

        OnMuseumChanged?.Invoke();
        return true;
    }

    public int GetDonatedCount()
    {
        return donatedItemIds.Count;
    }

    public void LoadDonations(List<string> ids)
    {
        donatedItemIds = ids ?? new List<string>();
        RebuildLookup();
        OnMuseumChanged?.Invoke();
    }
}