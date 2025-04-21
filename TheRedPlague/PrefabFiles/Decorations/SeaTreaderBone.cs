using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Decorations;

public class SeaTreaderBone
{
    private readonly PrefabInfo _info;
    private readonly string _modelName;
    
    public SeaTreaderBone(string classId, string modelName)
    {
        _info = PrefabInfo.WithTechType(classId);
        _modelName = modelName;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(_info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private GameObject GetGameObject()
    {
        var prefab = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(_modelName));
        prefab.SetActive(false);
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            renderer.material = Plugin.AssetBundle.LoadAsset<Material>("SeaTreaderSkeleton");
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter)
            {
                var meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.mesh;
                meshCollider.convex = true;
            }
        }
        MaterialUtils.ApplySNShaders(prefab, 6f);
        var infect = prefab.AddComponent<InfectAnything>();
        infect.infectionHeightStrength = 0;
        infect.infectionAmount = 1;
        prefab.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        prefab.AddComponent<SkyApplier>().renderers = prefab.GetComponentsInChildren<Renderer>();
        prefab.AddComponent<PrefabIdentifier>().ClassId = _info.ClassID;
        prefab.AddComponent<ConstructionObstacle>();
        var rb = prefab.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.mass = 500f;
        return prefab;
    }
}