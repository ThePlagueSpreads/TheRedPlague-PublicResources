using UnityEngine;

namespace TheRedPlague.Mono.Triggers;

public abstract class PlayerTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (DoesColliderTriggerEvent(other))
        {
            OnTriggerActivated();
        }
    }

    protected abstract void OnTriggerActivated();

    private bool DoesColliderTriggerEvent(Collider other)
    {
        if (other.gameObject.GetComponentInChildren<Player>())
        {
            return true;
        }
        
        var vehicle = other.GetComponentInParent<Vehicle>();
        if (vehicle != null && Player.main.GetVehicle() == vehicle)
        {
            return true;
        }

        var sub = other.GetComponentInParent<SubRoot>();
        if (sub && sub == Player.main.GetCurrentSub())
        {
            return true;
        }

        return false;
    }
}