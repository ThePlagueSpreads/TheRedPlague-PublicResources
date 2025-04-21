using System.Collections;
using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DestroyWhenAwayFromLocation : MonoBehaviour
{
    public Vector3 centerOfLocation;
    public float maxDistance;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        if (Vector3.SqrMagnitude(transform.position - centerOfLocation) > maxDistance * maxDistance)
            Destroy(gameObject);
    }
}