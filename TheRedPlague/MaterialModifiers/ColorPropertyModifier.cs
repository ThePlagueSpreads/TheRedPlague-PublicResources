using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace TheRedPlague.MaterialModifiers;

public class ColorPropertyModifier : MaterialModifier
{
    private readonly int _id;
    private readonly Color _color;

    public ColorPropertyModifier(string propertyName, Color color) : this(Shader.PropertyToID(propertyName), color)
    {
    }

    public ColorPropertyModifier(int id, Color color)
    {
        _id = id;
        _color = color;
    }

    public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
        MaterialUtils.MaterialType materialType)
    {
        material.SetColor(_id, _color);
    }
}