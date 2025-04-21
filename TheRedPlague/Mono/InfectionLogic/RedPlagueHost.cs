using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheRedPlague.Mono.InfectionLogic;

public class RedPlagueHost : MonoBehaviour
{
    private static readonly List<RedPlagueHost> AllTargets = new();

    public Mode mode;
    
    // Zombified creatures are Red Plague infected variants of normal creatures; not plague creations
    public bool IsZombified { get; private set; }
    
    private InfectedMixin _infectedMixin;
    private bool _madeInfectionChanges;

    private void Start()
    {
        _infectedMixin = GetComponent<InfectedMixin>();
        if ((mode == Mode.Normal && _infectedMixin && _infectedMixin.IsInfected()) ||
                                       mode == Mode.PlagueCreation)
        {
            UpdateAsInfected();
        }
    }

    public void UpdateAsInfected()
    {
        if (_madeInfectionChanges) return;
        
        var ecoTarget = gameObject.GetComponent<EcoTarget>();
        if (ecoTarget)
        {
            ecoTarget.SetTargetType(EcoTargetType.None);
        }
        
        _madeInfectionChanges = true;
    }

    public bool IsInfected()
    {
        if (mode == Mode.PlagueCreation)
            return true;
        if (_infectedMixin && _infectedMixin.IsInfected())
            return true;
        return false;
    }
    
    public bool CanBeInfected()
    {
        if (mode == Mode.PlagueCreation || mode == Mode.Immune) return false;
        if (_infectedMixin == null) return false;
        return !_infectedMixin.IsInfected();
    }

    public void Infect()
    {
        if (_infectedMixin)
        {
            _infectedMixin.SetInfectedAmount(4);
        }
    }

    // This method does NOT apply any behavior. It simply changes a boolean.
    public void MarkAsZombified()
    {
        IsZombified = true;
        UpdateAsInfected();
    }

    private void OnEnable()
    {
        AllTargets.Add(this);
    }
    
    private void OnDisable()
    {
        AllTargets.Remove(this);
    }

    public static void InfectInRange(Vector3 center, float range)
    {
        foreach (var target in AllTargets)
        {
            if (!target.CanBeInfected()) continue;
            if (Vector3.SqrMagnitude(center - target.transform.position) > range * range) continue;
            target.Infect();
        }
    }
    
    public static bool TryGetRandomTarget(out RedPlagueHost chosenTarget)
    {
        var validTargets = AllTargets.Where(target => target.CanBeInfected()).ToArray();

        if (validTargets.Length == 0)
        {
            chosenTarget = null;
            return false;
        }

        chosenTarget = validTargets[Random.Range(0, validTargets.Length)];
        return chosenTarget != null;
    }

    public delegate bool IsValidTarget(RedPlagueHost target);

    public static bool TryGetClosest(Vector3 center, IsValidTarget isValidTarget, out RedPlagueHost closestTarget, float maxDistance = float.MaxValue)
    {
        closestTarget = null;
        bool found = false;
        float closestSqrDistance = maxDistance * maxDistance;
        
        foreach (var target in AllTargets)
        {
            var sqrDistance = Vector3.SqrMagnitude(target.transform.position - center);
            if (sqrDistance > closestSqrDistance)
            {
                continue;
            }

            if (!isValidTarget.Invoke(target))
            {
                continue;
            }
            
            closestTarget = target;
            closestSqrDistance = sqrDistance;
            found = true;
        }

        return found;
    }

    public static bool IsGameObjectInfected(GameObject gameObject)
    {
        var host = gameObject.GetComponent<RedPlagueHost>();
        if (host == null || host.mode == Mode.Immune)
            return false;
        return host.IsInfected();
    }

    public enum Mode
    {
        Normal,
        PlagueCreation,
        Immune
    }
}