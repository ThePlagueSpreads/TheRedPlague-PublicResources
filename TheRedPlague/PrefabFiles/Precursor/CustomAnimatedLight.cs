using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Precursor;

public class CustomAnimatedLight
{
    private PrefabInfo Info { get; }
    private readonly float _lightRange;
    private readonly float _lightAngle;
    private readonly float _lightIntensity;
    private readonly float _triggerRadius;
    private readonly Vector3 _volumetricLightScale;
    private readonly Color _overrideColor;

    public CustomAnimatedLight(string classId, float lightRange, Vector3 volumetricLightScale, float lightAngle, float lightIntensity = 2.5f, float triggerRadius = 20f, Color overrideColor = default)
    {
        Info = PrefabInfo.WithTechType(classId);
        _lightRange = lightRange;
        _lightAngle = lightAngle;
        _lightIntensity = lightIntensity;
        _triggerRadius = triggerRadius;
        _volumetricLightScale = volumetricLightScale;
        _overrideColor = overrideColor;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        var template = new CloneTemplate(Info, "ce3c3053-5022-404e-a165-e31abe495f1b");
        template.ModifyPrefab += obj =>
        {
            var volumetricLight = obj.GetComponentInChildren<VFXVolumetricLight>();
            volumetricLight.transform.localScale = _volumetricLightScale;
            var lights = obj.GetComponentsInChildren<Light>();
            var mainLight = lights.FirstOrDefault(l => l.type == LightType.Spot);
            if (mainLight == null) mainLight = lights.First();
            mainLight.range = _lightRange;
            mainLight.spotAngle = _lightAngle;
            if (_overrideColor != default)
            {
                mainLight.color = _overrideColor;
            }
            obj.GetComponent<PrecursorActivatedPillar>().lightIntensity = _lightIntensity;
            obj.GetComponent<SphereCollider>().radius = _triggerRadius;
            foreach (var disableEmissive in obj.GetComponents<DisableEmissiveOnStoryGoal>())
            {
                Object.DestroyImmediate(disableEmissive);
            }
            foreach (var lightIntensity in obj.GetComponents<LightIntensityOnStoryGoal>())
            {
                Object.DestroyImmediate(lightIntensity);
            }
        };
        prefab.SetGameObject(template);
        prefab.Register();
    }
}