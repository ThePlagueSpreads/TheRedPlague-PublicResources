using System.Collections.Generic;
using TheRedPlague.Data;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class DamageInRange : MonoBehaviour
{
    // Essential
    public GameObject dealerRoot;
    public float normalDamage;
    public float plagueDamage;
    
    // Settings
    public float damageRadius = 1f;
    public bool canDamagePlayer = true;
    public bool canDamageCreatures = true;
    public bool canDamageInfectedCreatures = false;
    public bool canDamageVehicles = true;
    public bool canDamageSubmarines = false;

    private readonly HashSet<LiveMixin> _damagedCreaturesMemory = new();
    
    public void DealDamageToTargetsInRange()
    {
        var damagePos = transform.position;
        var hitColliders = UWE.Utils.OverlapSphereIntoSharedBuffer(damagePos, damageRadius, -1,
            QueryTriggerInteraction.Ignore);
        for (var i = 0; i < hitColliders; i++)
        {
            var target = GenericTrpUtils.GetTargetRoot(UWE.Utils.sharedColliderBuffer[i]);
            if (!IsValidTarget(target))
                continue;
            var lm = target.GetComponent<LiveMixin>();
            if (!_damagedCreaturesMemory.Add(lm))
                continue;
            
            if (normalDamage > Mathf.Epsilon)
                lm.TakeDamage(normalDamage, damagePos, DamageType.Normal, dealerRoot);
            if (plagueDamage > Mathf.Epsilon)
                lm.TakeDamage(plagueDamage, damagePos, CustomDamageTypes.PenetrativePlagueDamage, dealerRoot);
        }
        _damagedCreaturesMemory.Clear();
    }
    
    public bool IsValidTarget(GameObject target)
    {
        if (target.GetComponent<LiveMixin>() == null)
            return false;

        if (canDamagePlayer && target == Player.main.gameObject)
        {
            return Player.main.CanBeAttacked() && Player.main.GetVehicle() == null;
        }
        
        if (canDamageCreatures)
        {
            var creature = target.GetComponent<Creature>();
            if (creature && (canDamageInfectedCreatures || !RedPlagueHost.IsGameObjectInfected(target)))
                return true;
        }

        if (canDamageVehicles)
        {
            if (target.GetComponent<Vehicle>())
                return true;
        }

        if (canDamageSubmarines)
        {
            var sub = target.GetComponent<SubRoot>();
            if (sub && !sub.isBase)
            {
                return true;
            }
        }
        
        return false;
    }

    [System.Serializable]
    public class DamageLayer
    {
        public DamageType type;
        public float damage;

        public DamageLayer(DamageType type, float damage)
        {
            this.type = type;
            this.damage = damage;
        }
    }
}