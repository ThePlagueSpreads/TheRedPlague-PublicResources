using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity;

public class InsanityDeterrenceZone : MonoBehaviour
{
    public float radius;
    public bool onlyIndoors;
    
    private static readonly List<InsanityDeterrenceZone> Zones = new();

    private void OnEnable()
    {
        Zones.Add(this);
    }

    private void OnDisable()
    {
        Zones.Remove(this);
    }

    public static bool GetIsInZoneOfDeterrence(Vector3 position, bool indoors)
    {
        foreach (var zone in Zones)
        {
            if (zone.onlyIndoors && !indoors)
            {
                continue;
            }

            if (Vector3.SqrMagnitude(position - zone.transform.position) < zone.radius * zone.radius)
            {
                return true;
            }
        }

        return false;
    }
}