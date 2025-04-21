using System.Collections;
using Nautilus.Utility;
using TheRedPlague.Mono.Insanity.Symptoms.Bases;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class ReusedSoundHallucinations : TimedHallucinationSymptom
{
    private static readonly ReusedSound[] Sounds = {
        new("event:/creature/reaper/idle", 4),
        new("event:/creature/crash/inflate"),
        new("event:/creature/seadragon/idle", 5),
        new("event:/creature/magistrate/roar"),
        new("event:/creature/magistrate/idle"),
        new("event:/creature/magistrate/speak"),
        new("event:/creature/tred/idle"),
        new("event:/player/coughing"),
        new("event:/creature/gasopod/death"),
        new("event:/creature/crabsnake/idle_swim"),
        new("event:/env/background/debris_fall")
    };

    private const float MaxSoundDuration = 10;
    private const float DistanceBehindPlayer = 20;
    private const float UseDistantSoundWhenPossibleChance = 0.7f;

    protected override float MinInterval => 150;
    protected override float MaxInterval => 300;
    protected override float MinInsanity => 25;
    protected override float MaxInsanity => 80;
    protected override float ChanceAtMinInsanity => 0.3f;
    protected override float ChanceAtMaxInsanity => 0.8f;
    
    private FMOD_CustomEmitter _emitter;

    protected override IEnumerator OnLoadAssets()
    {
        _emitter = gameObject.AddComponent<FMOD_CustomEmitter>();
        _emitter.playOnAwake = false;
        _emitter.restartOnPlay = true;
        _emitter.followParent = true;
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
        var chosenSound = Sounds[Random.Range(0, Sounds.Length)];
        var distance = DistanceBehindPlayer;
        if (chosenSound.FarVariantDistanceMultiplier != null && Random.value < UseDistantSoundWhenPossibleChance)
            distance *= chosenSound.FarVariantDistanceMultiplier.Value;
        transform.position =
            Player.main.transform.position + MainCamera.camera.transform.forward * -distance;
        _emitter.SetAsset(chosenSound.SoundAsset);
        _emitter.Play();
        Invoke(nameof(EndSound), MaxSoundDuration);
    }

    private void EndSound()
    {
        _emitter.Stop();
    }

    private struct ReusedSound
    {
        public FMODAsset SoundAsset { get; }
        public float? FarVariantDistanceMultiplier { get; }

        public ReusedSound(string eventName)
        {
            SoundAsset = AudioUtils.GetFmodAsset(eventName);
            FarVariantDistanceMultiplier = null;
        }
        
        public ReusedSound(string eventName, float farVariantDistanceMultiplier)
        {
            SoundAsset = AudioUtils.GetFmodAsset(eventName);
            FarVariantDistanceMultiplier = farVariantDistanceMultiplier;
        }
    }
}