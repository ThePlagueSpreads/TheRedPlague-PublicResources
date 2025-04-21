using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class ShoalDamageInRange : MonoBehaviour
{
    public float damageDistance = 3.3f;
    public float damageUpdateInterval = 0.5f;
    public float damage = 1f;

    private float _nextCheckTime;

    private void Update()
    {
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + damageUpdateInterval;
        Check();
    }

    private void Check()
    {
        if (!Player.main.IsSwimming())
        {
            return;
        }

        if (Vector3.SqrMagnitude(Player.main.transform.position - transform.position) > damageDistance * damageDistance)
        {
            return;
        }

        Player.main.liveMixin.TakeDamage(damage, transform.position, DamageType.Normal, gameObject);
    }
}