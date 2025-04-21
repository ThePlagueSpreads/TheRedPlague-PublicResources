using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomPoisonInstance : MonoBehaviour
{
    public float damagePlayerRange = 6;
    public float fallStartSpeed = 3f;
    public float fallMaxSpeed = 7f;
    public float fallAccel = 1f;

    private float _fallSpeed;

    private float _timeInfectAgain;

    private Vector3 _velocity;

    private static float _timeLastPlayerDamage;

    private const float CheckDelay = 0.5f;

    private void Start()
    {
        _timeInfectAgain = Time.time + Random.value;
    }

    public void SetStartVelocity(Vector3 velocity)
    {
        _velocity = velocity;
    }

    private void Update()
    {
        if (Time.time > _timeInfectAgain)
        {
            DamagePlayer();
            _timeInfectAgain = Time.time + CheckDelay;
        }

        _fallSpeed = Mathf.Clamp(_fallSpeed + fallAccel * Time.deltaTime, fallStartSpeed, fallMaxSpeed);

        transform.position += new Vector3(0, -_fallSpeed * Time.deltaTime, 0) + _velocity * Time.deltaTime;
    }

    private void DamagePlayer()
    {
        if (Time.time < _timeLastPlayerDamage + CheckDelay) return;

        var player = Player.main;
        if (!player.IsUnderwaterForSwimming())
            return;
        var sqrDistToPlayer = Vector3.SqrMagnitude(player.transform.position - transform.position);
        if (sqrDistToPlayer < damagePlayerRange * damagePlayerRange)
        {
            Player.main.liveMixin.TakeDamage(3, transform.position, CustomDamageTypes.PenetrativePlagueDamage);
            _timeLastPlayerDamage = Time.time;
        }
    }
}