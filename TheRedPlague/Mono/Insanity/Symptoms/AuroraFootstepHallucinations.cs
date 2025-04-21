using System.Collections;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class AuroraFootstepHallucinations : InsanitySymptom
{
    private float _timeTryAgain;

    private static readonly FMODAsset FootstepsSound = AudioUtils.GetFmodAsset("RandomAuroraFootsteps");

    private const float MinutesMin = 5;
    private const float MinutesMax = 8;
    private const float LowInsanityPercent = 0.50f;
    private const float HighInsanityThreshold = 30;
    private const float HighInsanityPercent = 1f;
    private const float SoundSourceDistance = 15;
    private const float MinInsanity = 5;

    private void Start()
    {
        ResetTimer();
    }

    protected override void PerformSymptoms(float dt)
    {
        if (Time.time < _timeTryAgain) return;
        ResetTimer();
        var chance = InsanityPercentage >= HighInsanityThreshold ? HighInsanityPercent : LowInsanityPercent;
        if (!CrashedShipAmbientSound.main.isPlayerInside || Random.value > chance) return;
        var position = MainCamera.camera.transform.position +
                       MainCamera.camera.transform.forward * -SoundSourceDistance;
        Utils.PlayFMODAsset(FootstepsSound, position);
    }

    private void ResetTimer()
    {
        _timeTryAgain = Time.time + Random.Range(MinutesMin * 60, MinutesMax * 60);
    }

    protected override IEnumerator OnLoadAssets()
    {
        yield break;
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