using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Mono.VFX;
using UnityEngine;
using UWE;

namespace TheRedPlague.PrefabFiles.Decorations;

public class SuperFleshDecorationPrefab
{
    public SuperFleshDecorationPrefab(string classId, string modelName, bool infected,
        params MaterialModifier[] materialModifiers)
    {
        ClassId = classId;
        ModelName = modelName;
        Infected = infected;
        MaterialModifiers = materialModifiers;
    }

    public string ClassId { get; }
    public string ModelName { get; }
    public bool Infected { get; }
    public MaterialModifier[] MaterialModifiers { get; }
    public bool HasGlobalVariant { get; set; }

    private PrefabInfo _normalPrefabInfo;
    private PrefabInfo _auroraPrefabInfo;
    private PrefabInfo _farPrefabInfo;
    private PrefabInfo _globalPrefabInfo;

    public void Register()
    {
        _normalPrefabInfo = PrefabInfo.WithTechType(ClassId);
        _auroraPrefabInfo = PrefabInfo.WithTechType(ClassId + "Aurora");
        _farPrefabInfo = PrefabInfo.WithTechType(ClassId + "-FAR");
        RegisterVariant(_normalPrefabInfo, GetNormalPrefab);
        RegisterVariant(_auroraPrefabInfo, GetAuroraPrefab);
        RegisterVariant(_farPrefabInfo, GetFarPrefab);
        if (HasGlobalVariant)
        {
            _globalPrefabInfo = PrefabInfo.WithTechType(ClassId + "-GLOBAL");
            RegisterVariant(_globalPrefabInfo, GetGlobalPrefab);
        }
    }

    private void RegisterVariant(PrefabInfo info, System.Func<IOut<GameObject>, IEnumerator> getPrefab)
    {
        var prefab = new CustomPrefab(info);
        prefab.SetGameObject(getPrefab);
        prefab.Register();
    }

    private GameObject GetPrefabPart1()
    {
        var go = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(ModelName));
        go.SetActive(false);
        MaterialUtils.ApplySNShaders(go, 4f, 1f, 1f, MaterialModifiers);

        if (Infected)
        {
            var infect = go.AddComponent<InfectAnything>();
            infect.infectionAmount = 1;
            infect.infectionHeightStrength = 0f;
        }

        go.AddComponent<ConstructionObstacle>();
        return go;
    }

    private IEnumerator GetNormalPrefab(IOut<GameObject> prefab)
    {
        var obj = GetPrefabPart1();
        PrefabUtils.AddBasicComponents(obj, _normalPrefabInfo.ClassID, _normalPrefabInfo.TechType,
            LargeWorldEntity.CellLevel.Medium);
        prefab.Set(obj);
        yield return null;
    }

    private IEnumerator GetAuroraPrefab(IOut<GameObject> prefab)
    {
        var obj = GetPrefabPart1();
        PrefabUtils.AddBasicComponents(obj, _auroraPrefabInfo.ClassID, _auroraPrefabInfo.TechType,
            LargeWorldEntity.CellLevel.Medium);
        var request = PrefabDatabase.GetPrefabAsync("98ac710d-5390-49fd-a850-dbea7bc07aef");
        yield return request;
        if (request.TryGetPrefab(out var controlRoomPrefab))
        {
            var skyApplier = obj.GetComponent<SkyApplier>();
            skyApplier.customSkyPrefab = controlRoomPrefab.GetComponent<SkyApplier>().customSkyPrefab;
            skyApplier.dynamic = false;
            skyApplier.anchorSky = Skies.Custom;
        }

        prefab.Set(obj);
    }

    private IEnumerator GetFarPrefab(IOut<GameObject> prefab)
    {
        var obj = GetPrefabPart1();
        PrefabUtils.AddBasicComponents(obj, _farPrefabInfo.ClassID, _farPrefabInfo.TechType,
            LargeWorldEntity.CellLevel.VeryFar);
        prefab.Set(obj);
        yield return null;
    }

    private IEnumerator GetGlobalPrefab(IOut<GameObject> prefab)
    {
        var obj = GetPrefabPart1();
        PrefabUtils.AddBasicComponents(obj, _globalPrefabInfo.ClassID, _globalPrefabInfo.TechType,
            LargeWorldEntity.CellLevel.Global);
        prefab.Set(obj);
        yield return null;
    }
}