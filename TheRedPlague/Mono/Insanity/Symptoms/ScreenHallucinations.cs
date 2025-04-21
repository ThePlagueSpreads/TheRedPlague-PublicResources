using System.Collections;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class ScreenHallucinations : InsanitySymptom
{
    private const float GrayScaleTransitionSpeed = 0.1f;
    private Grayscale _grayscale;

    private Preset _activePreset;

    private static readonly Preset DefaultPreset = default;

    private static readonly Preset[] Presets = {
        new(50, 0.1f, 0),
        new(60, 0.25f, 0),
        new(70, 0.4f, 0),
        new(75, 0.5f, 0),
        new(80, 0.5f, 0),
        new(99, 2f, 80)
    };

    protected override IEnumerator OnLoadAssets()
    {
        _grayscale = MainCamera.camera.GetComponent<Grayscale>();
        yield break;
    }

    protected override void PerformSymptoms(float dt)
    {
        _activePreset = GetActivePreset();
    }

    private void Update()
    {
        _grayscale.effectAmount = Mathf.MoveTowards(_grayscale.effectAmount, _activePreset.GrayscaleAmount,
            Time.deltaTime * GrayScaleTransitionSpeed);
        if (_grayscale.effectAmount > 0)
            _grayscale.enabled = true;
    }

    private Preset GetActivePreset()
    {
        if (Plugin.Options.DisableInsanityScreenEffect)
        {
            return DefaultPreset;
        }
        
        var chosenPreset = DefaultPreset;
        var minRequirement = float.MinValue;
        var infectionDamage = PlagueDamageStat.main.InfectionPercent;
        foreach (var preset in Presets)
        {
            if (InsanityPercentage >= preset.RequiredInsanity && preset.RequiredInsanity > minRequirement
                && infectionDamage > preset.MinInfectionDamage)
            {
                chosenPreset = preset;
                minRequirement = preset.RequiredInsanity;
            }
        }

        return chosenPreset;
    }

    protected override void OnActivate()
    {
    }

    protected override void OnDeactivate()
    {
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return true;
    }

    private readonly struct Preset
    {
        public Preset(float requiredInsanity, float grayscaleAmount, float minInfectionDamage)
        {
            RequiredInsanity = requiredInsanity;
            GrayscaleAmount = grayscaleAmount;
            MinInfectionDamage = minInfectionDamage;
        }

        public float RequiredInsanity { get; }
        public float GrayscaleAmount { get; }
        public float MinInfectionDamage { get; }
    }
}