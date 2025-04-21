using Nautilus.Utility;
using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class SuckerDamageable : MonoBehaviour, IOnTakeDamage
{
    public Animator animator;
    
    private static readonly FMODAsset DeathSound = AudioUtils.GetFmodAsset("SuckerDeath");

    private bool _dead;
    
    private static readonly int DeadParam = Animator.StringToHash("dead");

    public void Kill()
    {
        _dead = true;
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;
        transform.Find("BlockTriggers").gameObject.SetActive(false);
        Utils.PlayFMODAsset(DeathSound, transform.position);
        animator.SetBool(DeadParam, true);
        Destroy(gameObject, 20);
    }

    public void OnTakeDamage(DamageInfo damageInfo)
    {
        if (_dead) return;
        if (damageInfo.type == CustomDamageTypes.PlagueCutting)
        {
            Kill();
        }
    }
}