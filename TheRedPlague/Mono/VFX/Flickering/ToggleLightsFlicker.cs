namespace TheRedPlague.Mono.VFX.Flickering;

public class ToggleLightsFlicker : FlickerTargetBase
{
    private readonly ToggleLights _lights;
    private readonly bool _oldLightState;
    private readonly FMODAsset _onSound;
    private readonly FMODAsset _offSound;
    private readonly FMOD_StudioEventEmitter _onSoundEmitter;
    private readonly FMOD_StudioEventEmitter _offSoundEmitter;
    
    public ToggleLightsFlicker(ToggleLights lights)
    {
        _lights = lights;
        _oldLightState = lights.GetLightsActive();
        _onSound = lights.onSound;
        _offSound = lights.offSound;
        _onSoundEmitter = lights.lightsOnSound;
        _offSoundEmitter = lights.lightsOffSound;
        lights.onSound = null;
        lights.offSound = null;
        lights.lightsOnSound = null;
        lights.lightsOffSound = null;
    }
    
    public override void SetIntensity(float intensity)
    {
        if (_lights)
            _lights.SetLightsActive(intensity > 0.5f);
    }

    public override void ResetIntensity()
    {
        if (!_lights)
            return;
        _lights.SetLightsActive(_oldLightState);
        _lights.onSound = _onSound;
        _lights.offSound = _offSound;
        _lights.lightsOnSound = _onSoundEmitter;
        _lights.lightsOffSound = _offSoundEmitter;
    }
}