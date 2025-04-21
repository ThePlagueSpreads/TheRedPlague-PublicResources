using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomMeleeAttack : MeleeAttack
{
    public PhantomLeviathanCreature phantom;

    public bool IsAttacking()
    {
        return Time.time < timeLastBite + 3;
    }

    public override bool CanBite(GameObject target)
    {
        if (!phantom.CanAttack())
            return false;
        
        return base.CanBite(target);
    }
}