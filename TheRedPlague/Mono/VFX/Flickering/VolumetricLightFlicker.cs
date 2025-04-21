namespace TheRedPlague.Mono.VFX.Flickering;

public class VolumetricLightFlicker : FlickerTargetBase
{
    private readonly VFXVolumetricLight _light;
    private readonly float _defaultIntensity;
    
    public VolumetricLightFlicker(VFXVolumetricLight light)
    {
        _light = light;
        _defaultIntensity = light.lightSource.intensity;
    }
    
    public override void SetIntensity(float intensity)
    {
        if (!_light || !_light.lightSource)
            return;
        _light.intensity = _defaultIntensity * intensity;
    }

    public override void ResetIntensity()
    {
        if (!_light || !_light.lightSource)
            return;
        _light.intensity = _defaultIntensity;
    }
}