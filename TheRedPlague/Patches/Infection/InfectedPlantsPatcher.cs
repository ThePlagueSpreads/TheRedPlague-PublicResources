using HarmonyLib;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.Patches.Infection;

[HarmonyPatch]
public static class InfectedPlantsPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Creepvine), nameof(Creepvine.Awake))]
    public static void CreepvineAwakePostfix(Creepvine __instance)
    {
        MakePlantInfectable(__instance.gameObject, 0.25f);
    }

    private static void MakePlantInfectable(GameObject plant, float chanceToInfectWithHiveMind)
    {
        var infect = plant.gameObject.AddComponent<InfectAnything>();
        infect.infectChance = 0;
        infect.infectChanceWhenHiveMindIsReleased = chanceToInfectWithHiveMind;
    }
}