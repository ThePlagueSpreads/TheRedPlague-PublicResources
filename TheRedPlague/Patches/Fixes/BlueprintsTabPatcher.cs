using HarmonyLib;
using TMPro;

namespace TheRedPlague.Patches.Fixes;

[HarmonyPatch(typeof(uGUI_BlueprintsTab))]
public static class BlueprintsTabPatcher
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(uGUI_BlueprintsTab.Awake))]
    public static void AwakePrefix(uGUI_BlueprintsTab __instance)
    {
        if (__instance.prefabEntry == null) return;
        __instance.prefabEntry.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>().richText = true;
    }
}