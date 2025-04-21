using UnityEngine;

namespace TheRedPlague.Mono.VFX.Flickering;

public class BasicLightFlicker : FlickerTargetBase
{
    private readonly Light _light;
    private readonly float _defaultIntensity;
    
    public BasicLightFlicker(Light light)
    {
        _light = light;
        _defaultIntensity = light.intensity;
    }
    
    public override void SetIntensity(float intensity)
    {
        if (_light)
            _light.intensity = _defaultIntensity * intensity;
    }

    public override void ResetIntensity()
    {
        if (_light)
            _light.intensity = _defaultIntensity;
    }
}