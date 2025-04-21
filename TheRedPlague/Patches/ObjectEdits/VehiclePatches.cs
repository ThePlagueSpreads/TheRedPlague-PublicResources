using HarmonyLib;
using TheRedPlague.Mono.CreatureBehaviour.Sucker;

namespace TheRedPlague.Patches.ObjectEdits;

[HarmonyPatch(typeof(Vehicle))]
public static class VehiclePatches
{
    [HarmonyPatch(nameof(Vehicle.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(Vehicle __instance)
    {
        __instance.gameObject.EnsureComponent<SuckerControllerTarget>();
    }
}