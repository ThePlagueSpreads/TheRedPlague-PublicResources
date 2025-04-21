using System.Collections;
using Nautilus.Assets;
using Nautilus.Extensions;
using Nautilus.Utility;
using TheRedPlague.Mono.CreatureBehaviour.Grabber;
using TheRedPlague.PrefabFiles.VFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public static class GrabberPrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("Grabber");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.Register();
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var grabberObj = Object.Instantiate(Plugin.CreaturesBundle.LoadAsset<GameObject>("GrabberPrefab"));
        grabberObj.SetActive(false);
        PrefabUtils.AddBasicComponents(grabberObj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Medium);
        MaterialUtils.ApplySNShaders(grabberObj, 6, 6);

        var creature = grabberObj.AddComponent<GrabberCreature>();
        creature.animator = grabberObj.GetComponentInChildren<Animator>();
        creature.pivotTransform = creature.transform.Find("RotationPivot");
        creature.grabTransform = creature.transform.SearchChild("GrabTransform");
        creature.mainCollider = grabberObj.GetComponent<Collider>();
        creature.renderer = grabberObj.GetComponentInChildren<SkinnedMeshRenderer>();
        
        var grabEmitter = grabberObj.AddComponent<FMOD_CustomLoopingEmitter>();
        grabEmitter.SetAsset(AudioUtils.GetFmodAsset("GrabberAttackLoop"));
        grabEmitter.assetStart = AudioUtils.GetFmodAsset("GrabberAttackStart");
        grabEmitter.restartOnPlay = false;
        grabEmitter.playOnAwake = false;
        creature.grabEmitter = grabEmitter;

        var trigger = grabberObj.transform.Find("SummonTrigger").gameObject.AddComponent<GrabberTrigger>();
        trigger.creature = creature;

        var sideGrabTrigger = grabberObj.transform.Find("RotationPivot/SideGrabTrigger").gameObject
            .AddComponent<GrabberGrabTrigger>();
        sideGrabTrigger.mode = GrabMode.InFront;
        sideGrabTrigger.creature = creature;
        
        var topGrabTrigger = grabberObj.transform.Find("RotationPivot/SideGrabTrigger").gameObject
            .AddComponent<GrabberGrabTrigger>();
        topGrabTrigger.mode = GrabMode.Above;
        topGrabTrigger.creature = creature;
        
        creature.triggers = new[] { topGrabTrigger.gameObject, sideGrabTrigger.gameObject };

        var bloodSplashPrefab = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("GrabberBloodSplash"), grabberObj.transform);
        bloodSplashPrefab.SetActive(false);
        var bloodRenderers = bloodSplashPrefab.GetComponentsInChildren<Renderer>();
        bloodSplashPrefab.AddComponent<SkyApplier>().renderers = bloodRenderers;
        foreach (var renderer in bloodRenderers)
        {
            var material = new Material(renderer.sharedMaterial);
            BloodParticle.ConvertShader(material);
            renderer.material = material;
        }

        creature.bloodPrefab = bloodSplashPrefab;
        
        yield return null;
        prefab.Set(grabberObj);
    }
}