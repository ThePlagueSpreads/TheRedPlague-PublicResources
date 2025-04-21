using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Mimics;

public class MimicGasopodAttackTrigger : MonoBehaviour
{
    public Creature creature;
    public DamageInRange rangeDamage;
    public FMOD_CustomEmitter emitter;
    public FMODAsset damageSound;
    public float damageDelay = 1.8f;
    public float attackInterval = 7f;

    private float _timeCanAttackAgain;
    private static readonly int Attack = Animator.StringToHash("attack");

    public void OnTriggerEnter(Collider other)
    {
        if (Time.time < _timeCanAttackAgain)
            return;
        if (!creature.liveMixin.IsAlive())
            return;
        if (!rangeDamage.IsValidTarget(GenericTrpUtils.GetTargetRoot(other)))
            return;
        StartAttack();
    }

    private void StartAttack()
    {
        _timeCanAttackAgain = Time.time + attackInterval;
        Invoke(nameof(OnDamage), damageDelay);
        creature.GetAnimator().SetTrigger(Attack);
    }

    private void OnDamage()
    {
        emitter.SetAsset(damageSound);
        emitter.Play();
        rangeDamage.DealDamageToTargetsInRange();
    }
}