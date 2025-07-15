using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace TheRedPlague.MaterialModifiers;

public class VectorPropertyModifier : MaterialModifier
{
    private readonly int _id;
    private readonly Vector4 _value;

    public VectorPropertyModifier(string propertyName, Vector4 value) : this(Shader.PropertyToID(propertyName), value)
    {
    }

    public VectorPropertyModifier(int id, Vector4 value)
    {
        _id = id;
        _value = value;
    }

    public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
        MaterialUtils.MaterialType materialType)
    {
        material.SetVector(_id, _value);
    }
}