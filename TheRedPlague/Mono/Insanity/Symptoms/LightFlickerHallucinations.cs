using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Nautilus.Utility;
using TheRedPlague.Mono.VFX.Flickering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class LightFlickerHallucinations : InsanitySymptom
{
    private const float MinInsanity = 15f;
    private const float MaxInsanity = 100f;
    private const float ChanceAtMinInsanity = 0.2f;
    private const float ChanceAtMaxInsanity = 0.8f;
    private const float MinDelay = 24;
    private const float MaxDelay = 34;
    private const float VoiceGlitchInitialDelay = 60 * 15;
    private const float VoiceGlitchDelay = 60 * 60;
    
    private const float MinDuration = 2f;
    private const float MaxDuration = 3.7f;
    private const float HighInsanityThreshold = 85;
    private const float HighInsanityDurationMultiplier = 1.6f;

    private float _timeTryAgain;

    private readonly List<GameObject> _targets = new();
    private SoundscapeType _currentSoundscapeType;

    private float _timeCanPlayVoiceGlitchAgain;

    private FMOD_CustomEmitter _flickerSoundEmitter;

    private static readonly Dictionary<SoundscapeType, Soundscape> Soundscapes = new()
    {
        {
            SoundscapeType.Cyclops, new Soundscape(AudioUtils.GetFmodAsset("CyclopsBaseLightFlicker"),
                AudioUtils.GetFmodAsset("CyclopsVoiceGlitch"))
        },
        { SoundscapeType.Base, new Soundscape(AudioUtils.GetFmodAsset("CyclopsBaseLightFlicker")) },
        { SoundscapeType.OtherInterior, new Soundscape(AudioUtils.GetFmodAsset("CyclopsBaseLightFlicker")) },
        {
            SoundscapeType.Seamoth, new Soundscape(AudioUtils.GetFmodAsset("SeamothLightFlicker"),
                AudioUtils.GetFmodAsset("SeamothVoiceGlitch"))
        },
        { SoundscapeType.PrawnSuit, new Soundscape(AudioUtils.GetFmodAsset("PrawnsuitLightFlicker")) },
        { SoundscapeType.OtherVehicle, new Soundscape(AudioUtils.GetFmodAsset("SeamothLightFlicker")) },
        { SoundscapeType.Tool, new Soundscape(AudioUtils.GetFmodAsset("ToolsLightFlicker")) }
    };

    private static readonly HashSet<TechType> UnflickerableTools = new()
    {
        TechType.Builder,
        TechType.LaserCutter,
        TechType.DiveReel,
        TechType.Welder,
        TechType.PropulsionCannon,
        TechType.RepulsionCannon,
        TechType.HeatBlade
    };

    private void Start()
    {
        _timeCanPlayVoiceGlitchAgain = Time.time + VoiceGlitchInitialDelay;
    }

    protected override IEnumerator OnLoadAssets()
    {
        _flickerSoundEmitter = gameObject.AddComponent<FMOD_CustomEmitter>();
        _flickerSoundEmitter.playOnAwake = false;
        yield break;
    }

    protected override void OnActivate()
    {
    }

    protected override void OnDeactivate()
    {
    }

    protected override void PerformSymptoms(float dt)
    {
        if (Time.time < _timeTryAgain)
            return;
        _timeTryAgain = Time.time + Random.Range(MinDelay, MaxDelay);
        var insanityPercent = Mathf.InverseLerp(MinInsanity, MaxInsanity, InsanityPercentage);
        var insanityChance = Mathf.Lerp(ChanceAtMinInsanity, ChanceAtMaxInsanity, insanityPercent);
        if (Random.value > insanityChance)
            return;
        FlickerLights();
    }

    private void FlickerLights()
    {
        GetTargets();
        var duration = Random.Range(MinDuration, MaxDuration);
        if (InsanityPercentage >= HighInsanityThreshold)
            duration *= HighInsanityDurationMultiplier;
        foreach (var target in _targets)
        {
            if (!target)
                continue;
            var flickering = target.AddComponent<LightFlickerEvent>();
            flickering.SetUp(target, duration);
        }

        if (_currentSoundscapeType != SoundscapeType.None &&
            Soundscapes.TryGetValue(_currentSoundscapeType, out var soundscape))
        {
            _flickerSoundEmitter.SetAsset(soundscape.FlickerSound);
            _flickerSoundEmitter.Play();
            Invoke(nameof(StopSound), duration - 0.3f);
            if (soundscape.GlitchedVoice == null || Time.time < _timeCanPlayVoiceGlitchAgain)
                return;
            Utils.PlayFMODAsset(soundscape.GlitchedVoice, Player.main.transform.position);
            _timeCanPlayVoiceGlitchAgain = Time.time + VoiceGlitchDelay;
        }
    }

    private void StopSound()
    {
        _flickerSoundEmitter.Stop(STOP_MODE.ALLOWFADEOUT);
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return InsanityPercentage >= MinInsanity && MiscSettings.flashes;
    }

    private void GetTargets()
    {
        _targets.Clear();

        _currentSoundscapeType = SoundscapeType.None;

        var heldTool = Inventory.main.GetHeldTool();
        if (heldTool && heldTool.GetComponentInChildren<Light>(true) != null
            && !UnflickerableTools.Contains(CraftData.GetTechType(heldTool.gameObject)))
        {
            var toggleLights = heldTool.GetComponentInChildren<ToggleLights>();
            if (toggleLights == null || toggleLights.lightsActive)
            {
                _targets.Add(heldTool.gameObject);
                _currentSoundscapeType = SoundscapeType.Tool;
            }
        }

        var sub = Player.main.GetCurrentSub();
        if (sub)
        {
            _targets.Add(Player.main.GetCurrentSub().gameObject);
            if (sub.isCyclops)
                _currentSoundscapeType = SoundscapeType.Cyclops;
            else if (sub.isBase)
                _currentSoundscapeType = SoundscapeType.Base;
            else
                _currentSoundscapeType = SoundscapeType.OtherInterior;
        }

        var vehicle = Player.main.GetVehicle();
        if (vehicle)
        {
            _targets.Add(vehicle.gameObject);
            _currentSoundscapeType = vehicle switch
            {
                SeaMoth => SoundscapeType.Seamoth,
                Exosuit => SoundscapeType.PrawnSuit,
                _ => SoundscapeType.OtherVehicle
            };
        }

        var escapePod = Player.main.currentEscapePod;
        if (escapePod)
        {
            _targets.Add(escapePod.gameObject);
            _currentSoundscapeType = SoundscapeType.OtherInterior;
        }
    }

    private readonly struct Soundscape
    {
        public FMODAsset FlickerSound { get; }
        public FMODAsset GlitchedVoice { get; }

        public Soundscape(FMODAsset flickerSound, FMODAsset glitchedVoice = null)
        {
            FlickerSound = flickerSound;
            GlitchedVoice = glitchedVoice;
        }
    }

    private enum SoundscapeType
    {
        None,

        // Subs (walkable interiors)
        Cyclops = 100,
        Base,
        OtherInterior,

        // Vehicles
        Seamoth = 200,
        PrawnSuit,
        OtherVehicle,

        // Other
        Tool = 300
    }
}