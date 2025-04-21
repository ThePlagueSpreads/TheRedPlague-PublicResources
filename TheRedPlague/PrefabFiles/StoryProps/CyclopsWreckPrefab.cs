using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.StoryProps;

public class CyclopsWreckPrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueCyclopsWreck");

    private static GameObject _cyclopsReference;

    private static bool _loaded;

    private static string[] _infectedChildren = new string[]
    {
        "cyclops_damaged_LOD0/cyclops_control_room (1)",
        "cyclops_damaged_LOD0/cyclops_diving_chamber (1)",
        "cyclops_damaged_LOD0/cyclops_ladder_hallway (1)",
        "cyclops_damaged_LOD0/cyclops_main_room (1)",
        "cyclops_damaged_LOD0/lower_engine_room (1)",
        "cyclops_damaged_LOD0/Submarine_Steering_Console (1)",
        "cyclops_damaged_LOD0/cyclops_launch_bay",
        "cyclops_damaged_LOD0/Engine_room"
    };

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> result)
    {
        if (_cyclopsReference == null)
        {
            _loaded = false;

            yield return new WaitUntil(() => LightmappedPrefabs.main);

            LightmappedPrefabs.main.RequestScenePrefab("Cyclops", OnSubPrefabLoaded);

            yield return new WaitUntil(() => _loaded);
        }

        var damagedCyclops = Object.Instantiate(_cyclopsReference.transform.Find("CyclopsMeshStatic/damaged").gameObject);
        damagedCyclops.SetActive(false);
        PrefabUtils.AddBasicComponents(damagedCyclops, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Far);

        foreach (var infectedChild in _infectedChildren)
        {
            damagedCyclops.transform.Find(infectedChild).gameObject.AddComponent<InfectAnything>().infectionHeightStrength = 0;
        }
        
        damagedCyclops.AddComponent<ConstructionObstacle>();
        
        result.Set(damagedCyclops);
    }

    private static void OnSubPrefabLoaded(GameObject prefab)
    {
        _cyclopsReference = prefab;
        _loaded = true;
    }
}