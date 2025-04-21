using UnityEngine;

namespace TheRedPlague.Mono.Insanity;

public static class InsanityUtils
{
    public static bool TryGetRandomHatchPosition(SubRoot sub, out Vector3 position, bool tryForCustomSubs)
    {
        if (sub == null)
        {
            position = default;
            return false;
        }
        
        // Check for Cyclops
        if (sub.isCyclops && sub.gameObject.name.Equals("Cyclops-MainPrefab(Clone)"))
        {
            var entrancePoint = sub.transform.Find("CyclopsMeshAnimated/EntranceHatch/EndPoint");
            if (entrancePoint)
            {
                position = entrancePoint.position;
                return true;
            }
        }
        // Check for bases
        else if (sub.isBase && sub.gameObject.GetComponent<BaseRoot>())
        {
            var diveHatches = sub.gameObject.GetComponentsInChildren<UseableDiveHatch>();
            UseableDiveHatch closest = null;
            var closestDistance = float.MaxValue;
            var camPos = MainCamera.camera.transform.position;
            foreach (var hatch in diveHatches)
            {
                var distToCam = (camPos - hatch.transform.position).sqrMagnitude;
                if (distToCam >= closestDistance) continue;
                closest = hatch;
                closestDistance = distToCam;
            }

            if (closest != null && closest.insideSpawn != null)
            {
                position = closest.insideSpawn.transform.position;
                return true;
            }
        }
        // Check for custom subs
        else if (tryForCustomSubs)
        {
            var enterExitHelper = sub.gameObject.GetComponentInChildren<EnterExitHelper>();
            if (enterExitHelper != null)
            {
                position = enterExitHelper.transform.position;
                return true;
            }
        }

        position = default;
        return false;
    }    
}