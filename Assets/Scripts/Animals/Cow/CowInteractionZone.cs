using UnityEngine;

public class CowInteractionZone : MonoBehaviour
{
    [SerializeField] private CowMilking cowMilking;

    private void Awake()
    {
        if (cowMilking == null)
        {
            cowMilking = GetComponentInParent<CowMilking>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IItemReceiver receiver = other.GetComponentInParent<IItemReceiver>();

        if (receiver == null)
        {
            return;
        }

        cowMilking.SetPlayerInside(receiver);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IItemReceiver receiver = other.GetComponentInParent<IItemReceiver>();

        if (receiver == null)
        {
            return;
        }

        cowMilking.ClearPlayerInside(receiver);
    }
}