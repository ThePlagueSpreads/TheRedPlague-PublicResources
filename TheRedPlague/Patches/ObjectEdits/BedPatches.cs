using HarmonyLib;
using TheRedPlague.Mono.InfectionLogic;

namespace TheRedPlague.Patches.ObjectEdits;

[HarmonyPatch(typeof(Bed))]
public static class BedPatches
{
    private const float BedInfectionDamageRecovery = 30f;
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Bed.ExitInUseMode))]
    public static void ExitInUseModePrefix(Player player, Bed __instance)
    {
        if (__instance.inUseMode == Bed.InUseMode.Sleeping)
        {
            PlagueDamageStat.main.HealInfectionDamage(BedInfectionDamageRecovery);
        }
    }
}