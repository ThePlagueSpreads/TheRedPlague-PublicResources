using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace TheRedPlague.MaterialModifiers;

public class ColorMultiplierModifier : MaterialModifier
{
    private readonly float _colorMultiplier;

    public ColorMultiplierModifier(float colorMultiplier)
    {
        _colorMultiplier = colorMultiplier;
    }

    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        material.color *= _colorMultiplier;
    }
}