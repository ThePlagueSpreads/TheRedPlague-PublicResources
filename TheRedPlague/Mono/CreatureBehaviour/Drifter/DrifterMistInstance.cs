using TheRedPlague.Data;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Drifter;

public class DrifterMistInstance : MonoBehaviour
{
    public float infectionRange = 20f;
    public float damagePlayerRange = 6;
    public float fallStartSpeed = 4f;
    public float fallMaxSpeed = 10f;
    public float fallAccel = 1f;
    public float plagueDamageToPlayerPerSecond = 3;

    private float _fallSpeed;

    private float _timeInfectAgain;

    private Vector3 _velocity;

    private static float _timeLastPlayerDamage;

    private const float InfectDelay = 0.5f;

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
            InfectInRange();
            _timeInfectAgain = Time.time + InfectDelay;
        }

        _fallSpeed = Mathf.Clamp(_fallSpeed + fallAccel * Time.deltaTime, fallStartSpeed, fallMaxSpeed);

        transform.position += new Vector3(0, -_fallSpeed * Time.deltaTime, 0) + _velocity * Time.deltaTime;
    }

    private void InfectInRange()
    {
        RedPlagueHost.InfectInRange(transform.position, infectionRange);

        if (Time.time < _timeLastPlayerDamage + InfectDelay) return;

        var player = Player.main;
        var sqrDistToPlayer = Vector3.SqrMagnitude(player.transform.position - transform.position);
        if (sqrDistToPlayer < damagePlayerRange * damagePlayerRange)
        {
            PlagueDamageStat.main.TakeInfectionDamage(plagueDamageToPlayerPerSecond * InfectDelay);
            _timeLastPlayerDamage = Time.time;
        }
    }
}