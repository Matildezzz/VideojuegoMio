public interface IInteractable
{
    string InteractionText { get; }
    bool CanInteract();
    void Interact();
}
