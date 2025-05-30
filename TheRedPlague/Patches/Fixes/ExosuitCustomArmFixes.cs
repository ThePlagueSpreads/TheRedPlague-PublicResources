using HarmonyLib;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Patches.Fixes;

[HarmonyPatch(typeof(Exosuit))]
public static class ExosuitCustomArmFixes
{
    [HarmonyPatch(nameof(Exosuit.OnUpgradeModuleChange))]
    [HarmonyPostfix]
    public static void OnUpgradeModuleChangePostfix(Exosuit __instance, TechType techType)
    {
        var customArms = CustomExosuitArmUtils.GetCustomExosuitArms();

        foreach (var arm in customArms)
        {
            if (arm.TechType != techType) continue;
            
            __instance.MarkArmsDirty();
            return;
        }
    }
    
    [HarmonyPatch(nameof(Exosuit.SpawnArm))]
    [HarmonyPostfix]
    public static void SpawnArmPostfix(TechType techType, Transform parent)
    {
        // Check if it is a base game arm (or none) first
        if (techType is TechType.None or TechType.ExosuitDrillArmModule or TechType.ExosuitTorpedoArmModule
            or TechType.ExosuitGrapplingArmModule or TechType.ExosuitClawArmModule
            or TechType.ExosuitPropulsionArmModule)
            return;


        // Ensure the arm is custom (not from another mod)
        var customArms = CustomExosuitArmUtils.GetCustomExosuitArms();

        var isCustom = false;

        foreach (var arm in customArms)
        {
            if (arm.TechType == techType)
            {
                isCustom = true;
                break;
            }
        }

        if (!isCustom)
            return;


        if (parent == null)
        {
            Plugin.Logger.LogInfo("Prawn suit arm parent not found");
            return;
        }
        
        // We should expect that the previous arm has NOT been destroyed yet, so instead, check the 2nd child
        var newArm = parent.GetChild(1);
        if (newArm == null)
        {
            newArm = parent.GetChild(0);
        }
        if (newArm == null)
        {
            Plugin.Logger.LogWarning("Failed to find new arm to re-enable!");
            return;
        }
        
        newArm.gameObject.SetActive(true);
    }
}