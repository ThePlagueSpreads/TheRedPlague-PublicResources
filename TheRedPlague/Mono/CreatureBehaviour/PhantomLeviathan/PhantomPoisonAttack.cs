using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomPoisonAttack : MonoBehaviour
{
    private static readonly int ClawAttack = Animator.StringToHash("claw_attack");

    public PhantomLeviathanCreature phantom;
    public DamageInRange damage;
    public float attackInterval = 13;
    public float damageDelay = 1.9f;

    private float _timeLastAttack;

    public void OnTouchTarget(Collider collider)
    {
        if (Time.time < _timeLastAttack + attackInterval)
            return;
        if (!phantom.CanAttack())
            return;
        if (!damage.IsValidTarget(GenericTrpUtils.GetTargetRoot(collider)))
            return;
        StartAttack();
    }

    private void StartAttack()
    {
        _timeLastAttack = Time.time;
        Invoke(nameof(OnDamage), damageDelay);
        phantom.GetAnimator().SetTrigger(ClawAttack);
        phantom.swimBehaviour.Idle();
    }

    private void OnDamage()
    {
        // emitter.SetAsset(damageSound);
        // emitter.Play();
        damage.DealDamageToTargetsInRange();
    }

    public bool IsAttacking()
    {
        return Time.time < _timeLastAttack + 5;
    }
}