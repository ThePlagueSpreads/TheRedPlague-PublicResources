using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DestroyAfterDelay : MonoBehaviour
{
    public float delay = 1f;
    
    private void Start()
    {
        Destroy(gameObject, delay);
    }
}