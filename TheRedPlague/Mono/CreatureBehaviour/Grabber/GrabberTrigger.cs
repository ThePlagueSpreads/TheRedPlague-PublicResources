using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Grabber;

public class GrabberTrigger : MonoBehaviour
{
    public GrabberCreature creature;
    
    private void OnTriggerEnter(Collider other)
    {
        if (creature.IsRevealed())
            return;
        if (Player.main.GetVehicle() == null)
            return;
        var root = GenericTrpUtils.GetTargetRoot(other);
        if (!creature.IsTargetValid(root))
            return;
        var vehicle = root.GetComponent<Vehicle>();
        if (vehicle == null || Player.main.GetVehicle().gameObject != root)
            return;
        creature.Reveal();
        creature.OrientTowardsTarget(root.transform);
    }
}