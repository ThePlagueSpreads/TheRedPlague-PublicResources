using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Special;

public class PropDestroyer
{
    private PrefabInfo Info { get; }
    private TechType[] TechTypesToDestroy { get; }
    private string[] ClassIdsToDestroy { get; }
    private float DestroyRadius { get; }

    public PropDestroyer(PrefabInfo info, TechType[] techTypesToDestroy, string[] classIdsToDestroy, float destroyRadius)
    {
        Info = info;
        TechTypesToDestroy = techTypesToDestroy;
        ClassIdsToDestroy = classIdsToDestroy;
        DestroyRadius = destroyRadius;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private GameObject GetGameObject()
    {
        var obj = new GameObject(Info.ClassID);
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, GetCellLevel(DestroyRadius));
        var destroy = obj.AddComponent<DestroyPropsInRange>();
        destroy.techTypesToDestroy = TechTypesToDestroy;
        destroy.classIdsToDestroy = ClassIdsToDestroy;
        destroy.radius = DestroyRadius;
        return obj;
    }

    private static LargeWorldEntity.CellLevel GetCellLevel(float destroyRadius)
    {
        return destroyRadius switch
        {
            < 30 => LargeWorldEntity.CellLevel.Near,
            < 60 => LargeWorldEntity.CellLevel.Medium,
            _ => LargeWorldEntity.CellLevel.Far
        };
    }
}