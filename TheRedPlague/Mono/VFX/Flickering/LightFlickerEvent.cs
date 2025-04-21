using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.VFX.Flickering;

public class LightFlickerEvent : MonoBehaviour
{
    private const float MinDelay = 0.08f;
    private const float MaxDelay = 0.15f;
    private const float MinBrightness = 0;
    private const float MaxBrightness = 1f;

    private float _timeChangeAgain;

    private readonly List<FlickerTargetBase> _targets = new();

    public void SetUp(GameObject technologyRoot, float duration)
    {
        Destroy(this, duration);

        var anyLightsFound = false;
        
        var lightingController = technologyRoot.GetComponent<LightingController>();

        if (lightingController)
        {
            _targets.Add(new LightControllerFlicker(lightingController));
            anyLightsFound = true;
        }

        var toggleLights = technologyRoot.GetComponentInChildren<ToggleLights>();
        if (toggleLights)
        {
            _targets.Add(new ToggleLightsFlicker(toggleLights));
            anyLightsFound = true;
        }

        if (anyLightsFound) return;
        
        var volumetricLights = technologyRoot.GetComponentsInChildren<VFXVolumetricLight>();
        foreach (var volumetric in volumetricLights)
        {
            if (volumetric && volumetric.lightSource)
                _targets.Add(new VolumetricLightFlicker(volumetric));
        }
            
        if (volumetricLights.Length == 0)
        {
            var lights = technologyRoot.GetComponentsInChildren<Light>();
            foreach (var light in lights)
            {
                _targets.Add(new BasicLightFlicker(light));
            }
        }
    }
    
    private void Update()
    {
        if (Time.time < _timeChangeAgain)
        {
            return;
        }

        var newIntensity = Random.Range(MinBrightness, MaxBrightness);
        foreach (var target in _targets)
        {
            target.SetIntensity(newIntensity);
        }

        _timeChangeAgain = Time.time + Random.Range(MinDelay, MaxDelay);
    }

    private void OnDestroy()
    {
        foreach (var target in _targets)
        {
            target.ResetIntensity();
        }
    }
}