using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Mono.Systems;
using UnityEngine;

namespace TheRedPlague.MaterialModifiers;

public class LightmapMaterialModifier : MaterialModifier
{
    private static readonly int Lightmap = Shader.PropertyToID("_Lightmap");
    private static readonly int LightmapStrength = Shader.PropertyToID("_LightmapStrength");
    private readonly float _strength;
    
    public LightmapMaterialModifier(float strength)
    {
        _strength = strength;
    }

    public override bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        return renderer.gameObject.name == AdditiveSceneManager.LightmapsMeshObjectName;
    }

    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        if (BlockShaderConversion(material, renderer, materialType)) return;
        if (!renderer.enabled) return;
        var lightmapTextureName = renderer.gameObject.name + "-Lightmap";
        var lightmapTexture = AdditiveSceneManager.GetLightmapTexture(lightmapTextureName);
        if (lightmapTexture == null) return;
        material.EnableKeyword("UWE_LIGHTMAP");
        material.SetTexture(Lightmap, lightmapTexture);
        material.SetFloat(LightmapStrength, material.IsKeywordEnabled("MARMO_EMISSION") ? 1f : _strength);
    }
}