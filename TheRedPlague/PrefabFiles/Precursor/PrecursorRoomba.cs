using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.StoryContent.Precursor;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Precursor;

public static class PrecursorRoombaPrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PrecursorRoomba");
    
    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private static GameObject GetPrefab()
    {
        var prefab = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("RoombaPrefab"));
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(prefab, 5.7f, 2f, 0.7f);
        var worldForces = PrefabUtils.AddWorldForces(prefab, 15, 9.8f, 0.2f);
        worldForces.useRigidbody.maxAngularVelocity = 5;
        var behavior = prefab.AddComponent<RoombaBehavior>();
        behavior.rb = worldForces.useRigidbody;
        return prefab;
    }
}