﻿using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Handlers;
using TheRedPlague.PrefabFiles.Fragments;
using UnityEngine;
using UWE;

namespace TheRedPlague.PrefabFiles.StoryProps;

public static class AdministratorDropPod
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("AdministratorDropPod");
    public static PingType PingType { get; private set; }

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
        PingType = EnumHandler.AddEntry<PingType>("AdministratorDropPodPing")
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("AdministratorEscapePodPing"));
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var task = PrefabDatabase.GetPrefabAsync("00037e80-3037-48cf-b769-dc97c761e5f6");
        yield return task;
        task.TryGetPrefab(out var original);
        var pod = Object.Instantiate(original);
        pod.SetActive(false);
        pod.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

        var exteriorRenderer = pod.transform.Find("life_pod_exploded_02_02/exterior/life_pod_damaged")
            .GetComponent<Renderer>();
        var exteriorMaterial = exteriorRenderer.materials[2];
        exteriorMaterial.mainTexture = Plugin.AssetBundle.LoadAsset<Texture2D>("AdministratorEscapePodDecal");

        var light = pod.AddComponent<Light>();
        light.intensity = 0.5f;
        light.color = Color.red;
        light.range = 20;
        
        var oldFabricator = pod.transform.Find("life_pod_exploded_02_02/interior/fabricator_01_base").gameObject;
        oldFabricator.SetActive(false);

        var adminFabTask = CraftData.GetPrefabForTechTypeAsync(AdminFabricatorFragment.Info.TechType);
        yield return adminFabTask;
        var adminFabricator = UWE.Utils.InstantiateDeactivated(adminFabTask.GetResult());
        Object.DestroyImmediate(adminFabricator.GetComponent<LargeWorldEntity>());
        Object.DestroyImmediate(adminFabricator.GetComponent<PrefabIdentifier>());
        Object.DestroyImmediate(adminFabricator.GetComponent<SkyApplier>());
        adminFabricator.transform.parent = oldFabricator.transform.parent;
        adminFabricator.transform.localPosition = oldFabricator.transform.localPosition;
        adminFabricator.transform.localEulerAngles = new Vector3(270, 180, 0);
        adminFabricator.SetActive(true);

        var secondSkyApplier = pod.GetComponents<SkyApplier>()[1];
        var renderers = new List<Renderer>(secondSkyApplier.renderers);
        renderers.AddRange(adminFabricator.GetComponentsInChildren<Renderer>(true));
        secondSkyApplier.renderers = renderers.ToArray();
        
        prefab.Set(pod);
    }
}