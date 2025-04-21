using System.Collections;
using Nautilus.Utility;
using TheRedPlague.Mono.Insanity.Symptoms.Bases;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class OxygenWarningHallucination : TimedHallucinationSymptom
{
    private readonly FMODAsset[] _sounds =
    {
        AudioUtils.GetFmodAsset("FakeOxygen10"),
        AudioUtils.GetFmodAsset("FakeOxygen25"),
        AudioUtils.GetFmodAsset("FakeOxygenBeep"),
    };

    private const float MinOxygenForWarnings = 45;
    private const float MinLostOxygenForWarnings = 10;

    protected override float MinInterval => 300;
    protected override float MaxInterval => 500;
    protected override float MinInsanity => 35;
    protected override float MaxInsanity => 100;
    protected override float ChanceAtMinInsanity => 0.44f;
    protected override float ChanceAtMaxInsanity => 0.93f;

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
        Utils.PlayFMODAsset(sound, Player.main.transform.position);
    }

    protected override bool ShouldDisplaySymptoms()
    {
        if (!base.ShouldDisplaySymptoms())
            return false;
        
        var oxygen = Player.main.GetOxygenAvailable();
        return oxygen >= MinOxygenForWarnings && oxygen <= Player.main.GetOxygenCapacity() - MinLostOxygenForWarnings;
    }
}