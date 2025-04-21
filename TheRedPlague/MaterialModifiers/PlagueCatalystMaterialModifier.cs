using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace TheRedPlague.MaterialModifiers;

public class PlagueCatalystMaterialModifier : MaterialModifier
{
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        if (materialType == MaterialUtils.MaterialType.Transparent)
        {
            material.SetFloat("_SpecInt", 3);
            material.SetFloat("_Shininess", 8);
            material.SetFloat("_Frensel", 0.1f);
            material.SetFloat(ShaderPropertyID._GlowStrengthNight, 0.3f);
        }
        else
        {
            material.SetFloat(ShaderPropertyID._EmissionLM, 0f);
            material.SetFloat("_EmissionLMNight", 0.1f);
        }
    }
}