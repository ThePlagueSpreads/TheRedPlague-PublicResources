using UnityEngine;

namespace TheRedPlague.Mono.InfectionLogic;

public class InfectedVehicle : MonoBehaviour
{
    private void Start()
    {
        GetComponent<LiveMixin>().health = 0;
        var vehicle = GetComponent<Vehicle>();
        vehicle.constructionFallOverride = true;
        vehicle.enabled = false;
        vehicle.useRigidbody.isKinematic = false;
    }
}