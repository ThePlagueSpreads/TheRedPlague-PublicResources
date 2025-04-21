using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Decorations;

public class FloatingCorpsePrefab
{
    public PrefabInfo Info { get; }
    private readonly string _prefabName;

    public FloatingCorpsePrefab(string classId, string prefabName)
    {
        Info = PrefabInfo.WithTechType(classId);
        _prefabName = prefabName;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private GameObject GetGameObject()
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(_prefabName));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 5);
        PrefabUtils.AddWorldForces(obj, 10, 0);
        obj.AddComponent<EcoTarget>().type = EcoTargetType.Shark;
        ModifyPrefab(obj);
        return obj;
    }

    protected virtual void ModifyPrefab(GameObject prefab)
    {
        
    }
}