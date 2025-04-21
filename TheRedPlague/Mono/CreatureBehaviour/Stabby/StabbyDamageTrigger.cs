using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Stabby;

public class StabbyDamageTrigger : MonoBehaviour
{
    public StabbyMotion motion;
    
    public float normalDamage = 10;
    public float penetrativeDamage = 6;
    public float damageInterval = 2f;
    public float knockBackForce = 6;
    
    private float _timeLastDamage;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!motion.Stabbing) return;
        if (Time.time < _timeLastDamage + damageInterval) return;
        var lm = other.gameObject.GetComponentInParent<LiveMixin>();
        if (lm == null) return;
        lm.TakeDamage(normalDamage, transform.position, DamageType.Normal, motion.gameObject);
        lm.TakeDamage(penetrativeDamage, transform.position, CustomDamageTypes.PenetrativePlagueDamage, motion.gameObject);
        _timeLastDamage = Time.time;
        if (other.gameObject.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            ApplyKnockBack(rigidbody);
        }
    }

    private void ApplyKnockBack(Rigidbody rb)
    {
        if (rb.isKinematic) return;
        var forceDirection = (rb.centerOfMass - motion.transform.position).normalized;
        rb.AddForce(forceDirection * knockBackForce, ForceMode.VelocityChange);
    }
}