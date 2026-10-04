using System;
using UnityEngine;

// Shared behaviour of everything the player can walk over and collect
// (power-ups and weapons). A subclass only decides what collecting it does.
// OnCollected / OnRejected let the HUD show feedback without the pickups
// knowing the HUD exists.
public abstract class Pickup : MonoBehaviour
{
    public static event Action<Pickup> OnCollected;
    public static event Action<Pickup> OnRejected;

    // Short text the HUD shows when this pickup is collected.
    public abstract string CollectMessage { get; }

    // Short text the HUD shows when the player touches this pickup but
    // can't use it (null = say nothing).
    public virtual string RejectMessage => null;

    // Applies the pickup to the player. Returns false if it can't be used
    // right now, in which case it stays in the room.
    protected abstract bool TryApply(GameObject player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!TryApply(other.gameObject))
        {
            OnRejected?.Invoke(this);
            return;
        }

        OnCollected?.Invoke(this);
        Destroy(gameObject);
    }
}
