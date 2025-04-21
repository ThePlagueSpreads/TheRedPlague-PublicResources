using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DestroyWhenAtOrigin : MonoBehaviour
{
    private void Start()
    {
        if (Vector3.SqrMagnitude(transform.position) < 0.001f)
        {
            Destroy(gameObject);
        }
    }
}