using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace TheRedPlague.Patches.Visual;

[HarmonyPatch(typeof(Player))]
public static class BrokenPdaPatch
{
    [HarmonyPatch(nameof(Player.Start))]
    [HarmonyPostfix]
    public static void SpawnGlassCracksPatch(Player __instance)
    {
        UWE.CoroutineHost.StartCoroutine(SpawnGlassCracks(__instance.transform.Find(
            "body/player_view/export_skeleton/head_rig/neck/chest/clav_L/clav_L_aim/shoulder_L/elbow_L/hand_L/attachL/PlayerPDA")));
    }

    private static IEnumerator SpawnGlassCracks(Transform pda)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
        yield return task;
        var seamoth = task.GetResult();
        var glassCracks = Object.Instantiate(seamoth.transform
            .Find("SeamothDamageFXSpawn").gameObject.GetComponent<PrefabSpawn>().prefab.transform
            .Find("x_SeamothGlassCracks"), pda, true);
        glassCracks.transform.localPosition = new Vector3(-0.290f, -0.100f, 0.000f);
        glassCracks.transform.localScale = new Vector3(0.01f, 0.2f, 0.1f);
        glassCracks.transform.localEulerAngles = new Vector3(270, 90, 0);
        glassCracks.GetComponent<Renderer>().material.color = new Color(1, 0.351795f, 0.285714f, 0.6f);
    }
}