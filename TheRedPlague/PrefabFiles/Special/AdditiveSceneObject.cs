using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using TheRedPlague.Mono.Systems;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Special;

public class AdditiveSceneObject
{
    public PrefabInfo Info { get; }
    
    private AdditiveSceneManager.Data _data;

    public AdditiveSceneObject(string classId, AdditiveSceneManager.Data data)
    {
        Info = PrefabInfo.WithTechType(classId);
        _data = data;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.SetSpawns(new SpawnLocation(_data.scenePosition, _data.sceneRotation));
        prefab.Register();
    }

    private GameObject GetGameObject()
    {
        var obj = new GameObject(Info.ClassID);
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        obj.AddComponent<AdditiveSceneManager>().data = _data;
        return obj;
    }
}