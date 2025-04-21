using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class SuckerControllerTarget : MonoBehaviour
{
    private static List<SuckerControllerTarget> _all = new();

    private readonly List<SuckerGrabVehicles> _attachedSuckers = new List<SuckerGrabVehicles>();
    
    private void OnEnable()
    {
        _all.Add(this);
    }

    private void OnDisable()
    {
        _all.Remove(this);
    }

    public void AttachSucker(SuckerGrabVehicles sucker)
    {
        _attachedSuckers.Add(sucker);
        RecalculatePrimarySucker();
    }
    
    public void RemoveSucker(SuckerGrabVehicles sucker)
    {
        _attachedSuckers.Remove(sucker);
        RecalculatePrimarySucker();
    }

    public void RecalculatePrimarySucker()
    {
        bool foundPrimary = false;
        foreach (var sucker in _attachedSuckers)
        {
            if (sucker == null) continue;
            if (!foundPrimary)
            {
                sucker.isPrimaryControllerOfTarget = true;
                foundPrimary = true;
                continue;
            }

            sucker.isPrimaryControllerOfTarget = false;
        }
    }

    public static bool TryGetClosest(out SuckerControllerTarget result, Vector3 referencePos, float maxRange)
    {
        result = null;
        
        if (_all.Count == 0) return false;
        
        var closestSqrDistance = float.MaxValue;
        foreach (var target in _all)
        {
            var sqrDist = Vector3.SqrMagnitude(target.transform.position - referencePos);
            if (sqrDist > closestSqrDistance) continue;
            if (sqrDist > maxRange * maxRange) continue;
            closestSqrDistance = sqrDist;
            result = target;
        }

        return result != null;
    }
}