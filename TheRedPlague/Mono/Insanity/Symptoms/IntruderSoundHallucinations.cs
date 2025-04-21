using System.Collections;
using Nautilus.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class IntruderSoundHallucinations : InsanitySymptom
{
    private float _timeTryFootStepsAgain;
    private float _timeTryHatchAgain;

    private static readonly FMODAsset FootstepsSound = AudioUtils.GetFmodAsset("RandomFootsteps");
    private static readonly FMODAsset HatchSound = AudioUtils.GetFmodAsset("EnterBaseScare");

    // Common
    private const float LowInsanityChance = 0.66f;
    private const float HighInsanityThreshold = 30;
    private const float HighInsanityChance = 1f;
    private const float MinInsanity = 20f;
    private const float MaxInsanity = 60f;

    // Footsteps
    private const float FootstepsMinutesMin = 10;
    private const float FootstepsMinutesMax = 15;
    private const float FootstepsSoundDistance = 10;

    // Hatch sounds
    private const float HatchMinutesMin = 10;
    private const float HatchMinutesMax = 20;
    private const float DefaultHatchSoundDistance = 15;

    private void Start()
    {
        ResetFootstepTimer();
        ResetHatchTimer();
    }

    protected override void PerformSymptoms(float dt)
    {
        var chanceForEither = GetPercentChance();

        if (Time.time >= _timeTryFootStepsAgain)
        {
            ResetFootstepTimer();
            if (Random.value > chanceForEither)
                return;
            if (Player.main.IsSwimming() || Player.main.GetCurrentSub() == null)
                return;
            Utils.PlayFMODAsset(FootstepsSound,
                MainCamera.camera.transform.position + Random.onUnitSphere * FootstepsSoundDistance);
        }

        if (Time.time >= _timeTryHatchAgain)
        {
            ResetHatchTimer();
            if (Random.value > chanceForEither)
                return;
            var sub = Player.main.GetCurrentSub();
            if (sub == null) return;
            if (!InsanityUtils.TryGetRandomHatchPosition(sub, out var hatchPosition, true))
            {
                hatchPosition = Random.onUnitSphere * DefaultHatchSoundDistance;
            }

            Utils.PlayFMODAsset(HatchSound, MainCamera.camera.transform.position + hatchPosition);
        }
    }

    private float GetPercentChance()
    {
        return InsanityPercentage < HighInsanityThreshold ? LowInsanityChance : HighInsanityChance;
    }

    private void ResetFootstepTimer()
    {
        _timeTryFootStepsAgain = Time.time + Random.Range(FootstepsMinutesMin * 60, FootstepsMinutesMax * 60);
    }

    private void ResetHatchTimer()
    {
        _timeTryHatchAgain = Time.time + Random.Range(HatchMinutesMin * 60, HatchMinutesMax * 60);
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
        return InsanityPercentage is >= MinInsanity and < MaxInsanity;
    }
}