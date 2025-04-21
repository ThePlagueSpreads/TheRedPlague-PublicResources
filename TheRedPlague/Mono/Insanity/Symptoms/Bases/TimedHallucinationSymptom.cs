using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms.Bases;

public abstract class TimedHallucinationSymptom : InsanitySymptom
{
    protected abstract float MinInterval { get; }
    protected abstract float MaxInterval { get; }
    protected abstract float MinInsanity { get; }
    protected abstract float MaxInsanity { get; }
    protected abstract float ChanceAtMinInsanity { get; }
    protected abstract float ChanceAtMaxInsanity { get; }
    
    private float _timePerformAgain;

    private void Start()
    {
        _timePerformAgain = Time.time + Random.Range(MinInterval, MaxInterval);
    }

    protected override void PerformSymptoms(float dt)
    {
        if (Time.time < _timePerformAgain)
            return;
        _timePerformAgain = Time.time + Random.Range(MinInterval, MaxInterval);
        if (Random.value > GetChance())
            return;
        PerformTimedAction();
    }

    protected abstract void PerformTimedAction();

    private float GetChance()
    {
        var percent = Mathf.InverseLerp(MinInsanity, MaxInsanity, InsanityPercentage);
        return Mathf.Lerp(ChanceAtMinInsanity, ChanceAtMaxInsanity, percent);
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return InsanityPercentage >= MinInsanity;
    }
}