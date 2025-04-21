using System.Collections;
using Nautilus.Utility;
using TheRedPlague.Mono.Insanity.Symptoms.Bases;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class CustomSoundHallucination : TimedHallucinationSymptom
{
    private readonly FMODAsset[] _sounds =
    {
        AudioUtils.GetFmodAsset("SoundHallucination1"),
        AudioUtils.GetFmodAsset("SoundHallucination2"),
        AudioUtils.GetFmodAsset("SoundHallucination3"),
        AudioUtils.GetFmodAsset("SoundHallucination4"),
        AudioUtils.GetFmodAsset("SoundHallucination5")
    };

    private const float MinDistance = 15;
    private const float MaxDistance = 64;
    
    protected override float MinInterval => 150;
    protected override float MaxInterval => 250;
    protected override float MinInsanity => 30;
    protected override float MaxInsanity => 100;
    protected override float ChanceAtMinInsanity => 0.25f;
    protected override float ChanceAtMaxInsanity => 0.90f;

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

    protected override void PerformTimedAction()
    {
        var sound = _sounds[Random.Range(0, _sounds.Length)];
        var pos = Player.main.transform.position + Random.onUnitSphere * Random.Range(MinDistance, MaxDistance);
        Utils.PlayFMODAsset(sound, pos);
    }
}