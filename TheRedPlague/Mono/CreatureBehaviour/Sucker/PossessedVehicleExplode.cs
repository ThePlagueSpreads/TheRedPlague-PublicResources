using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class PossessedVehicleExplode : MonoBehaviour
{
    public Vehicle vehicle;
    
    public EcoTargetType targetType = EcoTargetType.Shark;
    public float detectionDistance = 8f;
    public float explodeDamage = 70f;
    public float explodeRadius = 12f;
    public float explodeTimer = 4f;

    private EcoRegion.TargetFilter _isTargetValidFilter;

    private static readonly FMODAsset _timerSound = AudioUtils.GetFmodAsset("PossessedVehicleExplodeTimer");
    private static readonly FMODAsset _explodeSound = AudioUtils.GetFmodAsset("PossessedVehicleExplosion");
    
    private void Start()
    {
        _isTargetValidFilter = IsTargetValid;
        InvokeRepeating(nameof(CheckForTargets), Random.value, 0.2f);
    }

    private bool IsTargetValid(IEcoTarget target)
    {
        var targetObject = target.GetGameObject();
        if (targetObject == null) return false;
        if (targetObject == gameObject) return false;
        return true;
    }

    private void CheckForTargets()
    {
        if (!isActiveAndEnabled) return;
        var target = EcoRegionManager.main.FindNearestTarget(targetType, transform.position,
            out var distance, _isTargetValidFilter, 1);
        if (distance < detectionDistance * detectionDistance && target != null && target.GetGameObject() != null)
        {
            CancelInvoke(nameof(CheckForTargets));
            Invoke(nameof(Explode), explodeTimer);
            Utils.PlayFMODAsset(_timerSound, transform.position);
        }
    }

    private void Explode()
    {
        if (vehicle) vehicle.OnKill();
        else Destroy(gameObject);
        Utils.PlayFMODAsset(_explodeSound, transform.position);
        DamageSystem.RadiusDamage(explodeDamage, transform.position, explodeRadius, DamageType.Explosive, gameObject);
    }
}