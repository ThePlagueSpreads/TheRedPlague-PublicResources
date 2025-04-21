using System;

namespace TheRedPlague.Mono.VFX.Flickering;

public class LightControllerFlicker : FlickerTargetBase
{
    private readonly LightingController _controller;

    private readonly float[] _defaultLightIntensities = Array.Empty<float>();
    private readonly float[] _defaultSkyIntensities = Array.Empty<float>();

    public LightControllerFlicker(LightingController controller)
    {
        _controller = controller;
        var stateIndex = (int)_controller.state;
        if (controller.lights != null)
        {
            _defaultLightIntensities = new float[controller.lights.Length];
            for (var i = 0; i < controller.lights.Length; i++)
            {
                _defaultLightIntensities[i] = controller.lights[i].intensities[stateIndex];
            }
        }

        if (controller.skies != null)
        {
            _defaultSkyIntensities = new float[controller.skies.Length];
            for (var i = 0; i < controller.skies.Length; i++)
            {
                _defaultSkyIntensities[i] = controller.skies[i].masterIntensities[stateIndex];
            }
        }
    }

    public override void SetIntensity(float intensity)
    {
        if (_controller == null) return;
        
        if (_controller.lights != null)
        {
            for (var i = 0; i < _controller.lights.Length; i++)
            {
                _controller.lights[i].light.intensity = _defaultLightIntensities[i] * intensity;
            }
        }

        if (_controller.skies != null)
        {
            for (var i = 0; i < _controller.skies.Length; i++)
            {
                _controller.skies[i].sky.MasterIntensity = _defaultSkyIntensities[i] * intensity;
            }
        }
    }

    public override void ResetIntensity()
    {
        if (_controller == null) return;
        
        if (_controller.lights != null)
        {
            for (var i = 0; i < _controller.lights.Length; i++)
            {
                _controller.lights[i].light.intensity = _defaultLightIntensities[i];
            }
        }

        if (_controller.skies != null)
        {
            for (var i = 0; i < _controller.skies.Length; i++)
            {
                _controller.skies[i].sky.MasterIntensity = _defaultSkyIntensities[i];
            }
        }
    }
}