using System.Collections;
using HarmonyLib;
using Story;
using TheRedPlague.Mono.UI;
using UnityEngine;

namespace TheRedPlague.Patches.UI;

[HarmonyPatch]
public static class SuitChargeDisplayPatcher
{
    public static uGUI_SceneHUD SceneHud { get; private set; }

    [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.UpdateElements))]
    [HarmonyPostfix]
    public static void UpdateElementsPostfix(uGUI_SceneHUD __instance)
    {
        SceneHud = __instance;
        if (StoryGoalManager.main == null || !StoryGoalManager.main.initialized)
        {
            UWE.CoroutineHost.StartCoroutine(UpdateElementsWhenStoryGoalManagerLoads(__instance));
            return;
        }

        UpdateElements(__instance);
    }

    private static IEnumerator UpdateElementsWhenStoryGoalManagerLoads(uGUI_SceneHUD sceneHud)
    {
        yield return new WaitUntil(() => StoryGoalManager.main != null && StoryGoalManager.main.initialized);
        yield return null;
        UpdateElements(sceneHud);
    }

    private static void UpdateElements(uGUI_SceneHUD sceneHud)
    {
        if (!sceneHud) return;
        var active = sceneHud.active;
        if (!StoryGoalManager.main.initialized)
            Plugin.Logger.LogWarning("SuitChargeDisplayPatcher: Story goal manager not initialized!");
        if (!StoryGoalManager.main.IsGoalComplete(StoryUtils.UseBiochemicalProtectionSuitEvent.key))
        {
            PlagueDamageUI.Main.suitChargeBar.gameObject.SetActive(false);
            return;
        }

        sceneHud.backgroundBarsDouble.SetActive(false);
        sceneHud.backgroundBarsQuad.SetActive(false);
        PlagueDamageUI.Main.tripleStatsBackground.SetActive(active && sceneHud._mode == 1);
        PlagueDamageUI.Main.quintupleStatsBackground.SetActive(active && sceneHud._mode == 2);
        PlagueDamageUI.Main.suitChargeBar.gameObject.SetActive(active && sceneHud._mode is 1 or 2);
        PlagueDamageUI.Main.suitChargeBar.transform.localPosition =
            sceneHud._mode == 2 ? new Vector2(243.5f, 87.5f) : new Vector2(87f, 87.5f);
    }
}