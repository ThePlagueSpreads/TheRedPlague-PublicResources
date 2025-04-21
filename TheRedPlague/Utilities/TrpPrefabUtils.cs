using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.Utilities;

public static class TrpPrefabUtils
{
    public static GameObject CreateLootCubePrefab(PrefabInfo info)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.SetActive(false);
        PrefabUtils.AddBasicComponents(cube, info.ClassID, info.TechType, LargeWorldEntity.CellLevel.Near);
        PrefabUtils.AddWorldForces(cube, 5);
        cube.AddComponent<Pickupable>();
        return cube;
    }
    
    public static IEnumerator GenerateFleshCaveAtmosphereVolumes(GameObject gameObject)
    {
        var upperVolumeTask = UWE.PrefabDatabase.GetPrefabAsync("FleshCaveUpperVolume");
        yield return upperVolumeTask;
        var chamberVolumeTask = UWE.PrefabDatabase.GetPrefabAsync("FleshCaveChamberVolume");
        yield return chamberVolumeTask;

        // If we're too late...
        if (gameObject == null)
        {
            yield break;
        }
        
        upperVolumeTask.TryGetPrefab(out var upperVolume);
        chamberVolumeTask.TryGetPrefab(out var chamberVolume);
        
        var upperVolumesParent = gameObject.transform.Find("FleshCaveBiomeVolumes");
        var chamberVolumesParent = gameObject.transform.Find("FleshCavernBiomeVolumes");

        SpawnVolumesInPlaceOfChildren(gameObject.transform, upperVolumesParent, upperVolume);
        SpawnVolumesInPlaceOfChildren(gameObject.transform, chamberVolumesParent, chamberVolume);

        yield return null;

        gameObject.EnsureComponent<UpdateBiomeSkyAppliersOnStart>().updateEverySky = true;
    }
    
    public static IEnumerator GenerateShrineBaseAtmosphereVolumes(GameObject gameObject)
    {
        var hallwayTask = UWE.PrefabDatabase.GetPrefabAsync("ShrineBaseHallwayVolume");
        yield return hallwayTask;
        var mainRoomTask = UWE.PrefabDatabase.GetPrefabAsync("ShrineBaseMainRoomVolume");
        yield return mainRoomTask;

        // If we're too late...
        if (gameObject == null)
        {
            yield break;
        }
        
        hallwayTask.TryGetPrefab(out var hallway);
        mainRoomTask.TryGetPrefab(out var mainRoom);
        
        var hallwayParent = gameObject.transform.Find("AtmosphereVolumes-Hallway");
        var mainRoomParent = gameObject.transform.Find("AtmosphereVolumes-MainRoom");

        SpawnVolumesInPlaceOfChildren(gameObject.transform, hallwayParent, hallway);
        SpawnVolumesInPlaceOfChildren(gameObject.transform, mainRoomParent, mainRoom);
        
        yield return null;

        gameObject.EnsureComponent<UpdateBiomeSkyAppliersOnStart>().updateEverySky = true;
    }


    private static void SpawnVolumesInPlaceOfChildren(Transform root, Transform parent, GameObject volumePrefab)
    {
        foreach (Transform child in parent)
        {
            var volume = Object.Instantiate(volumePrefab, root);
            Object.DestroyImmediate(volume.GetComponent<LargeWorldEntity>());
            Object.DestroyImmediate(volume.GetComponent<PrefabIdentifier>());
            volume.transform.position = child.position;
            volume.transform.rotation = child.rotation;
            volume.transform.localScale = child.localScale;
        }
    }
}