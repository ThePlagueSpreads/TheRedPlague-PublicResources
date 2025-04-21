using HarmonyLib;
using Nautilus.Utility;
using TheRedPlague.Mono.StoryContent;
using UnityEngine;

namespace TheRedPlague.Patches.ObjectEdits;

[HarmonyPatch]
public class PrecursorGunPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PrecursorGunStoryEvents), nameof(PrecursorGunStoryEvents.Start))]
    public static void StartPostfix(PrecursorGunStoryEvents __instance)
    {
        return;
        
        var grab = __instance.gameObject.AddComponent<GrabGunAnimation>();
        grab.gunOverrideAnimatorController =
            Plugin.AssetBundle.LoadAsset<RuntimeAnimatorController>("PrecursorGunGrabAnimator");
        grab.gunAnimator = __instance.transform.Find("base_anim").gameObject.GetComponent<Animator>();
        
        var gunTentacles = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("GunTentaclePrefab"),
            __instance.transform, true);
        MaterialUtils.ApplySNShaders(gunTentacles, 7f, 2f, 0.5f);
        var applier = gunTentacles.AddComponent<SkyApplier>();
        applier.renderers = gunTentacles.GetComponentsInChildren<Renderer>();
        applier.customSkyPrefab = __instance.GetComponent<SkyApplier>().customSkyPrefab;
        applier.anchorSky = Skies.Custom;
        gunTentacles.transform.localPosition = Vector3.zero;
        gunTentacles.SetActive(false);

        grab.tentacleRoot = gunTentacles;
        grab.tentacle1Animator = gunTentacles.transform.Find("GunTentacle1").gameObject.GetComponent<Animator>();
        grab.tentacle2Animator = gunTentacles.transform.Find("GunTentacle2").gameObject.GetComponent<Animator>();

        var grabTrigger = new GameObject("GunGrabTrigger");
        var collider = grabTrigger.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        grabTrigger.transform.parent = __instance.transform;
        grabTrigger.transform.localPosition = new Vector3(-47, 4, -124);
        grabTrigger.transform.localScale = Vector3.one * 80;
        var trigger = grabTrigger.AddComponent<GrabGunTrigger>();
        trigger.grabGunAnimation = grab;
    }
}