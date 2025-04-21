using HarmonyLib;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Patches.Infection;

[HarmonyPatch(typeof(Creature))]
public static class CreaturePatcher
{
    [HarmonyPatch(nameof(Creature.OnEnable))]
    [HarmonyPostfix]
    // Attempt to add the Infected Mixin to creatures that need it
    public static void OnEnablePostfix(Creature __instance)
    {
        var redPlagueHost = __instance.gameObject.EnsureComponent<RedPlagueHost>();

        // Return early if the creature can't be infected:
        if (redPlagueHost.mode != RedPlagueHost.Mode.Normal)
        {
            return;
        }
        
        // Return early if the Infected Mixin already exists, and fix it up if needed:
        var infectedMixin = __instance.gameObject.GetComponent<InfectedMixin>();
        if (infectedMixin != null)
        {
            if (infectedMixin.renderers == null || infectedMixin.renderers.Length == 0)
            {
                infectedMixin.renderers = __instance.GetComponentsInChildren<Renderer>(true);
            }
            return;
        }
        
        // Otherwise, finally add the new Infected Mixin
        infectedMixin = __instance.gameObject.AddComponent<InfectedMixin>();
        infectedMixin.renderers = __instance.GetComponentsInChildren<Renderer>(true);
    }
}