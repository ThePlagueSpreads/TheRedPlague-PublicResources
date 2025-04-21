using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DestroyWhenFarAway : MonoBehaviour
{
    public float destroyDistance;
    
    private void Start()
    {
        InvokeRepeating(nameof(Check), Random.value, 0.67f);
    }

    private void Check()
    {
        if (Vector3.SqrMagnitude(MainCamera.camera.transform.position - transform.position) >
            destroyDistance * destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}