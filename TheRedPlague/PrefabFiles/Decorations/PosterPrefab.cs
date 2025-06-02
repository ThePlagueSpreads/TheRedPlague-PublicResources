using System;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Decorations;

public class PosterPrefab
{
    private static readonly int SpecTex = Shader.PropertyToID("_SpecTex");

    private PrefabInfo Info { get; set; }

    private readonly Func<Texture2D> _posterImage;

    public PosterPrefab(PrefabInfo info, Func<Texture2D> posterImage)
    {
        Info = info;
        _posterImage = posterImage;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.SetGameObject(new CloneTemplate(Info, TechType.PosterKitty)
        {
            ModifyPrefab = obj =>
            {
                var renderers = obj.GetComponentsInChildren<Renderer>();
                var posterMaterial = renderers[0].materials[1];

                var posterImage = _posterImage.Invoke();
                posterMaterial.mainTexture = posterImage;
                posterMaterial.SetTexture(SpecTex, posterImage);
            }
        });
        prefab.Register();
    }
}