using HarmonyLib;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.Patches.Visual;

[HarmonyPatch(typeof(Incubator))]
public static class IncubatorPatcher
{
    [HarmonyPatch(nameof(Incubator.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(Incubator __instance)
    {
        foreach (Transform eggChild in __instance.transform.parent.Find("Eggs"))
        {
            eggChild.gameObject.AddComponent<InfectAnything>();
        }
        __instance.gameObject.SetActive(false);
    }
}