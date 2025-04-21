using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomPoisonAttackTrigger : MonoBehaviour
{
    public PhantomPoisonAttack poisonAttack;
    
    public void OnTriggerEnter(Collider other)
    {
        poisonAttack.OnTouchTarget(other);
    }
}