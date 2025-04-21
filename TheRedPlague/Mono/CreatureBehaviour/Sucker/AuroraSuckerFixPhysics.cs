using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class AuroraSuckerFixPhysics : MonoBehaviour

{
    public Rigidbody rigidbody;
    public GameObject blockTrigger;

    private void Start()
    {
        InvokeRepeating(nameof(LazyUpdate), Random.value, 0.2f);
    }

    private void LazyUpdate()
    {
        if (!isActiveAndEnabled)
            return;
        if (rigidbody.isKinematic) return;
        blockTrigger.SetActive(false);
        Destroy(this);
    }
}