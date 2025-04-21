using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.StoryProps;

public static class IslandElevatorPlatform
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("IslandElevatorPlatform");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("IslandElevatorPlatform"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        MaterialUtils.ApplySNShaders(obj, 7f, 2f, 2f);
        obj.AddComponent<ConstructionObstacle>();
        return obj;
    }
}