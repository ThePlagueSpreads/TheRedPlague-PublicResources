using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.MaterialModifiers;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Plants;

public static class FleshPlant3
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("FleshPlant3");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.Register();
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("FleshPlant3"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Far);
        MaterialUtils.ApplySNShaders(obj, 6, 2f, 2f,
            new WavingEffectModifier(1) { Scale = new Vector4(0.01f, 0.01f, 0.01f, 0.02f) },
            new GlowStrengthNightModifier(0.01f, 0.01f));
        obj.AddComponent<ConstructionObstacle>();
        prefab.Set(obj);
        yield break;
    }
}