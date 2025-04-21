using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Mono.VFX;
using UnityEngine;
using UWE;

namespace TheRedPlague.PrefabFiles.Decorations;

public class FleshDecorationPrefab
{
    public FleshDecorationPrefab(PrefabInfo info, string modelName, bool infected, bool useAuroraSky, params MaterialModifier[] materialModifiers)
    {
        Info = info;
        ModelName = modelName;
        Infected = infected;
        UseAuroraSky = useAuroraSky;
        MaterialModifiers = materialModifiers;
    }

    public PrefabInfo Info { get; }
    public string ModelName { get; }
    public bool Infected { get; }
    public bool UseAuroraSky { get; }
    public MaterialModifier[] MaterialModifiers { get; }
    public LargeWorldEntity.CellLevel CellLevel { get; set; } = LargeWorldEntity.CellLevel.Medium;

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var go = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(ModelName));
        go.SetActive(false);
        PrefabUtils.AddBasicComponents(go, Info.ClassID, Info.TechType, CellLevel);
        MaterialUtils.ApplySNShaders(go, 4f, 1f, 1f, MaterialModifiers);
        
        if (Infected)
        {
            var infect = go.AddComponent<InfectAnything>();
            infect.infectionAmount = 1;
            infect.infectionHeightStrength = 0f;
        }

        if (UseAuroraSky)
        {
            var request = PrefabDatabase.GetPrefabAsync("98ac710d-5390-49fd-a850-dbea7bc07aef");
            yield return request;
            if (request.TryGetPrefab(out var controlRoomPrefab))
            {
                var skyApplier = go.GetComponent<SkyApplier>();
                skyApplier.customSkyPrefab = controlRoomPrefab.GetComponent<SkyApplier>().customSkyPrefab;
                skyApplier.dynamic = false;
                skyApplier.anchorSky = Skies.Custom;
            }
        }

        go.AddComponent<ConstructionObstacle>();
        prefab.Set(go);
    }
}