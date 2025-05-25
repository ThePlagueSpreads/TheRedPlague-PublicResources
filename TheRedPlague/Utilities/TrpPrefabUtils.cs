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
        var cacheVolumeTask = UWE.PrefabDatabase.GetPrefabAsync("FleshCaveCacheVolume");
        yield return cacheVolumeTask;

        // If we're too late...
        if (gameObject == null)
        {
            yield break;
        }

        upperVolumeTask.TryGetPrefab(out var upperVolume);
        chamberVolumeTask.TryGetPrefab(out var chamberVolume);
        cacheVolumeTask.TryGetPrefab(out var cacheVolume);

        var upperVolumesParent = gameObject.transform.Find("FleshCaveBiomeVolumes");
        var chamberVolumesParent = gameObject.transform.Find("FleshCavernBiomeVolumes");
        var cacheVolumesParent = gameObject.transform.Find("CacheBiomeVolumes");

        SpawnVolumesInPlaceOfChildren(gameObject.transform, upperVolumesParent, upperVolume);
        SpawnVolumesInPlaceOfChildren(gameObject.transform, chamberVolumesParent, chamberVolume);
        SpawnVolumesInPlaceOfChildren(gameObject.transform, cacheVolumesParent, cacheVolume);

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

    public static IEnumerator OnFleshCaveLoadAsync(GameObject root)
    {
        var obstructionRockTask = UWE.PrefabDatabase.GetPrefabAsync("fa986d5a-0cf8-4c63-af9f-8c36acd5bea4");
        yield return obstructionRockTask;
        if (!obstructionRockTask.TryGetPrefab(out var obstructionRock))
        {
            Plugin.Logger.LogError("Failed to find obstruction rock prefab for flesh cave!");
            yield break;
        }

        var material = new Material(obstructionRock.GetComponentInChildren<Renderer>().material);
        material.SetFloat("_CapBorderBlendRange", 0.285f);
        material.SetFloat("_CapBorderBlendOffset", -0.34f);
        material.SetFloat("_CapBorderBlendAngle", 2);
        material.SetFloat("_CapScale", 0.08f);
        material.SetFloat("_SideScale", 0.1f);
        material.SetFloat("_TriplanarBlendRange", 2f);
        material.SetFloat("_InnerBorderBlendRange", 0.76f);
        material.SetFloat("_InnerBorderBlendOffset", 1f);
        material.SetFloat("_Gloss", 0.3f);
        material.SetTexture("_CapSIGMap", Plugin.AssetBundle.LoadAsset<Texture2D>("fleshcache_ground_sig"));
        material.SetTexture("_CapBumpMap", Plugin.AssetBundle.LoadAsset<Texture2D>("fleshcache_ground_normal"));
        material.SetTexture("_CapTexture", Plugin.AssetBundle.LoadAsset<Texture2D>("fleshcache_ground_diffuse"));
        material.SetTexture("_SideSIGMap", Plugin.AssetBundle.LoadAsset<Texture2D>("fleshcache_wall_sig"));
        material.SetTexture("_SideBumpMap", Plugin.AssetBundle.LoadAsset<Texture2D>("fleshcache_wall_normal"));
        material.SetTexture("_SideTexture", Plugin.AssetBundle.LoadAsset<Texture2D>("fleshcache_wall_diffuse"));

        if (root == null)
            yield break;
        
        root.transform.Find("FleshCavePrefab/FleshCaveCache/FleshCaveCache").gameObject.GetComponent<Renderer>().material = material;
    }
}