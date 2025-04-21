using UnityEngine;

namespace TheRedPlague.Mono.InfectionLogic;

public class AllowPickUpOnDeath : MonoBehaviour
{
    public Pickupable pickupable;
    
    private void Start()
    {
        var lm = gameObject.GetComponent<LiveMixin>();
        if (lm && !lm.IsAlive() && pickupable != null)
        {
            pickupable.isPickupable = true;
        }
    }
    
    public void OnKill()
    {
        if (pickupable != null)
        {
            pickupable.isPickupable = true;
        }
    }
}