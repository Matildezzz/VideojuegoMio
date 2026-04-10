public interface IItemReceiver
{
    bool TryAddItem(ItemData item, int amount);
}