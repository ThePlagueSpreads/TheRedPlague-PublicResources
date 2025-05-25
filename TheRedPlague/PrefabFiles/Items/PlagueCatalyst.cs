using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Utilities.Gadgets;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public static class PlagueCatalyst
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueResource")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueCrystalIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.SetBackgroundType(CustomBackgroundTypes.PlagueItem);
        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PlagueCrystal_Prefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 6f, 1f, 0.05f,
            new PlagueCatalystMaterialModifier());
        PrefabUtils.AddWorldForces(obj, 14, isKinematic: true);
        obj.AddComponent<Pickupable>();
        PrefabUtils.AddResourceTracker(obj, Info.TechType);
        return obj;
    }
}