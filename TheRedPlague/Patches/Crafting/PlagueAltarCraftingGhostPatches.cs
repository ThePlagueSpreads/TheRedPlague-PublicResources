using HarmonyLib;
using TheRedPlague.Mono.Buildables.PlagueAltar;
using UnityEngine;

namespace TheRedPlague.Patches.Crafting;

[HarmonyPatch(typeof(CrafterGhostModel))]
public static class PlagueAltarCraftingGhostPatches
{
    // prevents the same model from being modified multiple times
    private static GameObject _lastModifiedGhostModel;

    [HarmonyPatch(nameof(CrafterGhostModel.UpdateProgress))]
    [HarmonyPostfix]
    public static void UpdateProgressPostfix(CrafterGhostModel __instance)
    {
        if (__instance.ghostModel == null || __instance.ghostModel == _lastModifiedGhostModel ||
            __instance.ghostMaterials == null)
            return;

        if (__instance.GetComponent<PlagueAltarCrafter>() == null)
            return;
        
        _lastModifiedGhostModel = __instance.ghostModel;
        
        foreach (var material in __instance.ghostMaterials)
        {
            if (material.shader != null && material.shader.name != "DontRender")
            {
                material.SetColor(ShaderPropertyID._BorderColor, new Color(0.8f, 0.2f, 0.4f));
                // x: scale, y: detail scale, z: frequency, w: speed
                material.SetVector(ShaderPropertyID._BuildParams, new Vector4(4, 0.7f, 9, -0.30f));
            }
        }
    }
}