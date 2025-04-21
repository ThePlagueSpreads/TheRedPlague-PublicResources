using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class ScaryMannySpawns : MonoBehaviour
{
    private static List<ScaryMannySpawns> list = new List<ScaryMannySpawns>();

    private void OnEnable()
    {
        list.Add(this);
    }

    private void OnDisable()
    {
        list.Remove(this);
    }

    public static bool TryGetClosestSpawnPoint(Vector3 position, out Vector3 result, out Quaternion rotation)
    {
        result = default;
        rotation = Quaternion.identity;
        if (list.Count == 0)
        {
            return false;
        }

        var maxDistance = float.MaxValue;
        foreach (var spawnPoint in list)
        {
            if (spawnPoint == null) continue;
            var dist = Vector3.Distance(position, spawnPoint.transform.position);
            if (dist > maxDistance) continue;
            maxDistance = dist;
            result = spawnPoint.transform.position;
            rotation = spawnPoint.transform.rotation;
        }

        return maxDistance <= 100000;
    }
}