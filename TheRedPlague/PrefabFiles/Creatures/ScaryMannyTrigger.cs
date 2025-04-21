using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public static class ScaryMannyTrigger
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("ScaryMannyTrigger");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var obj = new GameObject(Info.ClassID);
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        var collider = obj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        obj.AddComponent<SpawnScaryManny>();
        return obj;
    }
}