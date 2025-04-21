using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomPoisonEmitter : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject mistPrefab;

    public float spawnDelay = 0.2f;
    public float spawnRadius = 5f;

    private float _timeSpawnAgain;

    private void Update()
    {
        if (Time.time > _timeSpawnAgain)
        {
            SpawnMist();
            _timeSpawnAgain = Time.time + spawnDelay;
        }
    }

    private void SpawnMist()
    {
        var mist = Instantiate(mistPrefab, transform.position + Random.insideUnitSphere * spawnRadius, Quaternion.identity);
        mist.SetActive(true);
        mist.GetComponent<PhantomPoisonInstance>().SetStartVelocity(rb.velocity);
    }
}