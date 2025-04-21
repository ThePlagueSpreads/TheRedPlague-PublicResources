using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class TakeRandomScreenshots : InsanitySymptom
{
    private const float MinutesDelayMin = 30;
    private const float MinutesDelayMax = 60;
    private const float LowInsanityChance = 0.7f;
    private const float HighInsanityThreshold = 75;
    private const float HighInsanityChance = 1f;

    private const float MinInsanity = 55;

    private float _timeTryAgain;

    private void Start()
    {
        ResetTimer();
    }

    protected override IEnumerator OnLoadAssets()
    {
        yield break;
    }

    // Using PerformSymptom would mean that this symptom activates as soon as the symptom begins
    // This isn't ideal for events that are meant to be extremely rare and random
    private void Update()
    {
        if (Time.time < _timeTryAgain)
            return;
        ResetTimer();
        // Welp, try in another hour...
        if (!IsSymptomActive)
            return;
        if (Random.value > GetPercentChance())
            return;
        ScreenshotManager.instance?.TakeScreenshot();
    }

    private void ResetTimer()
    {
        _timeTryAgain = Time.time + Random.Range(MinutesDelayMin * 60, MinutesDelayMax * 60);
    }

    private float GetPercentChance()
    {
        return InsanityPercentage < HighInsanityThreshold ? LowInsanityChance : HighInsanityChance;
    }

    protected override void OnActivate()
    {
    }

    protected override void OnDeactivate()
    {
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return InsanityPercentage >= MinInsanity;
    }
}