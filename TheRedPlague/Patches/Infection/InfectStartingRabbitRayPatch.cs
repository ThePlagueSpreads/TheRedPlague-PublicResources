using HarmonyLib;
using UnityEngine;

namespace TheRedPlague.Patches.Infection;

[HarmonyPatch]
public static class InfectStartingRabbitRayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EscapePodFirstUseCinematicsController))]
    [HarmonyPatch(nameof(EscapePodFirstUseCinematicsController.SpawnCinematicCreature))]
    public static void SpawnCinematicCreaturePostfix(GameObject __result, string animationName)
    {
        if (animationName.StartsWith("rabbit_ray"))
        {
            __result.gameObject.GetComponent<InfectedMixin>().SetInfectedAmount(1);
        }
    }
}