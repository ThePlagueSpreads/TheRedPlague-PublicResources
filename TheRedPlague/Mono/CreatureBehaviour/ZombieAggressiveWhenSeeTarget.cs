using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class ZombieAggressiveWhenSeeTarget : AggressiveWhenSeeTarget
{
    public bool ignoreVehicles;
    
    public override GameObject GetAggressionTarget()
    {
        return EcoRegionManager.main
            .FindNearestTarget(targetType, transform.position, NewIsTargetValid, maxSearchRings)?.GetGameObject();
    }

    private bool NewIsTargetValid(IEcoTarget target)
    {
        var isValid = IsTargetValid(target);

        if (isValid)
        {
            if (ignoreVehicles && target.GetGameObject().GetComponent<Vehicle>())
                isValid = false;
            else if (ignoreVehicles && Player.main.GetVehicle() != null)
                isValid = false;
        }

        return isValid;
    }
}