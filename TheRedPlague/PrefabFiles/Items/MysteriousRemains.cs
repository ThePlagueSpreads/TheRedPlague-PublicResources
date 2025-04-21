using System.Collections;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.Util;
using TheRedPlague.PrefabFiles.Buildable;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public static class MysteriousRemains
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("MysteriousRemains")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("MysteriousRemainsIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
        
        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);

        PlagueRefinery.RegisterPlagueRefineryRecipe(Info.TechType);

        BaseBioReactor.charge[Info.TechType] = 400;
    }

    private static IEnumerator GetGameObject(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("MysteriousRemainsPrefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 6f);
        PrefabUtils.AddWorldForces(obj, 20, isKinematic: true);
        PrefabUtils.AddResourceTracker(obj, Info.TechType);
        obj.AddComponent<Pickupable>();
        var peeperTask = CraftData.GetPrefabForTechTypeAsync(TechType.Peeper);
        yield return peeperTask;
        var peeper = peeperTask.GetResult();
        var peeperMaterial = new Material(peeper.transform.Find("model/peeper/aqua_bird/peeper").gameObject
            .GetComponent<Renderer>().sharedMaterials[1]);
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.materials;
            materials[3] = peeperMaterial;
            renderer.materials = materials;
        }

        obj.AddComponent<DestroyWhenAtOrigin>();
        prefab.Set(obj);
    }
}