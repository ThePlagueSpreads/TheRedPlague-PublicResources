using System.Collections;
using HarmonyLib;
using Nautilus.Utility;
using Story;
using TheRedPlague.PrefabFiles.Precursor;
using UnityEngine;

namespace TheRedPlague.Patches.ObjectEdits;

[HarmonyPatch(typeof(VFXPrecursorGunElevator))]
public static class PrecursorElevatorPatcher
{
    // private static readonly FMODAsset FirstUseMusic = AudioUtils.GetFmodAsset("IslandElevatorFirst");
    // private static readonly FMODAsset GeneralUseMusic = AudioUtils.GetFmodAsset("IslandElevator");
    private static readonly FMODAsset ElevatorUseMusic = AudioUtils.GetFmodAsset("IslandElevator");

    [HarmonyPatch(typeof(VFXPrecursorGunElevator), nameof(VFXPrecursorGunElevator.OnGunElevatorStart))]
    [HarmonyPostfix]
    public static void UseElevatorPostfix(VFXPrecursorGunElevator __instance)
    {
        if (__instance.transform.parent == null ||
            CraftData.GetTechType(__instance.transform.parent.gameObject) != IslandElevator.Info.TechType) return;
        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.UseElevatorGoal.key))
        {
            // UWE.CoroutineHost.StartCoroutine(PlaySoundDelayed(GeneralUseMusic));
        }
        else
        {
            UWE.CoroutineHost.StartCoroutine(PlaySoundDelayed(ElevatorUseMusic));
            StoryGoalManager.main.OnGoalComplete(StoryUtils.UseElevatorGoal.key);
        }
        UWE.CoroutineHost.StartCoroutine(DisableTopElevatorTriggerForTime(__instance.transform.parent.Find("elevator_top_trigger").gameObject, 28));
    }

    private static IEnumerator PlaySoundDelayed(FMODAsset asset)
    {
        yield return new WaitForSeconds(2.2f);
        Utils.PlayFMODAsset(asset, Player.main.transform.position);
    }

    private static IEnumerator DisableTopElevatorTriggerForTime(GameObject topTrigger, float duration)
    {
        topTrigger.SetActive(false);
        yield return new WaitForSeconds(duration);
        if (topTrigger != null)
        {
            topTrigger.SetActive(true);
        }
    }
}