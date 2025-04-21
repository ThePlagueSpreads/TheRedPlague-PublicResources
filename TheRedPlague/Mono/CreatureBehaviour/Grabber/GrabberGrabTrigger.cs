using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Grabber;

public class GrabberGrabTrigger : MonoBehaviour
{
    public GrabberCreature creature;
    
    public GrabMode mode;

    private void OnTriggerEnter(Collider other)
    {
        if (creature.CurrentGrabMode != mode)
            return;
        if (!creature.CanGrab())
            return;
        var root = GenericTrpUtils.GetTargetRoot(other);
        if (!creature.IsTargetValid(root))
            return;
        creature.GrabTarget(root);
    }
}