using HarmonyLib;
using UnityEngine;

namespace TheRedPlague.Patches.ErrorHandling;

[HarmonyPatch]
public static class WarnForFailedLoadPatcher
{
    [HarmonyPatch(typeof(Player), nameof(Player.Start))]
    [HarmonyPrefix]
    public static void PlayerPrefix()
    {
        if (!Plugin.RedPlagueLoaded)
        {
            new GameObject("TRP-Warner").AddComponent<AnnoyingWarningClass>();
        }
    }

    private class AnnoyingWarningClass : MonoBehaviour
    {
        private void Start()
        {
            InvokeRepeating(nameof(Warn), 1, 5);
        }

        private void Warn()
        {
            ErrorMessage.AddMessage("<color=#ff0000>Red Plague failed to load!</color>");
            Plugin.Logger.LogError("Warning user because the Red Plague failed to load!");
        }
    }
}