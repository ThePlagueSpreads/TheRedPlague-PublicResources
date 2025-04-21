using HarmonyLib;
using TheRedPlague.Mono.VFX;

namespace TheRedPlague.Patches.ObjectEdits;

[HarmonyPatch(typeof(EscapePod))]
public static class EscapePodPatches
{
    [HarmonyPatch(nameof(EscapePod.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(EscapePod __instance)
    {
        __instance.gameObject.EnsureComponent<LifePodScare>();
    }
}