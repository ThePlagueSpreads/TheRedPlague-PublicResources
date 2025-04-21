using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class AggressiveWhenSeeZombies : MonoBehaviour
{
    public Creature creature;
    public LastTarget lastTarget;

    public AnimationCurve maxRangeMultiplier = new(
        new(0.0f, 1f, 0.0f, 0.0f, 0.333f, 0.333f),
        new(0.5f, 0.5f, 0.0f, 0.0f, 0.333f, 0.333f),
        new(1f, 1f, 0.0f, 0.0f, 0.333f, 0.333f));

    public AnimationCurve distanceAggressionMultiplier =
        new(
            new(0.0f, 1f, 0.0f, 0.0f, 0.333f, 0.333f),
            new(1f, 0.0f, -3f, -3f, 0.333f, 0.333f));

    public float aggressionPerSecond = 1f;

    public float maxRange = 10f;

    public float hungerThreshold;

    private RedPlagueHost.IsValidTarget _isValidTarget;

    private TechType _techType;

    private void Start()
    {
        InvokeRepeating(nameof(ScanForAggressionTarget), Random.Range(0f, 1f), 1f);
        _isValidTarget = IsTargetValid;
        _techType = CraftData.GetTechType(gameObject);
    }

    private void ScanForAggressionTarget()
    {
        if (!gameObject.activeInHierarchy || !enabled ||
            (creature != null && creature.Hunger.Value < hungerThreshold))
        {
            return;
        }

        var aggressionTarget = GetAggressionTarget();
        if (aggressionTarget == null)
        {
            return;
        }

        float distance = Vector3.Distance(aggressionTarget.transform.position, transform.position);
        float currentMaxRange = DayNightUtils.Evaluate(maxRange, maxRangeMultiplier);
        float distancePercent = (currentMaxRange - distance) / currentMaxRange;
        float aggressionMultiplier = distanceAggressionMultiplier.Evaluate(distancePercent);
        float aggression = aggressionPerSecond * aggressionMultiplier;
        if (aggression > 0f)
        {
            Debug.DrawLine(aggressionTarget.transform.position, transform.position, Color.white);
            creature.Aggression.Add(aggression);
            lastTarget.SetTarget(aggressionTarget.gameObject);
        }
    }

    private RedPlagueHost GetAggressionTarget()
    {
        return RedPlagueHost.TryGetClosest(transform.position, _isValidTarget, out var closest, maxRange)
            ? closest
            : null;
    }

    private bool IsTargetValid(RedPlagueHost target)
    {
        if (target == null || target.gameObject == null)
        {
            return false;
        }

        if (target.gameObject == gameObject)
        {
            return false;
        }

        if (!target.IsZombified && target.mode != RedPlagueHost.Mode.PlagueCreation)
        {
            return false;
        }

        if (CraftData.GetTechType(target.gameObject) == _techType)
        {
            return false;
        }

        if (creature.IsFriendlyTo(target.gameObject))
        {
            return false;
        }

        if (target.gameObject == Player.main.gameObject)
        {
            return false;
        }

        if (Vector3.Distance(target.transform.position, transform.position) > maxRange)
        {
            return false;
        }

        return creature.GetCanSeeObject(target.gameObject);
    }
}