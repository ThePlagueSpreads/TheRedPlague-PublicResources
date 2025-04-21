using HarmonyLib;
using TheRedPlague.Mono.UI;
using TMPro;
using UnityEngine;

namespace TheRedPlague.Patches.UI.Checklist;

[HarmonyPatch]
public static class PdaChecklistHeadsUpPatcher
{
    [HarmonyPatch(typeof(uGUI_PDA), nameof(uGUI_PDA.Start))]
    [HarmonyPostfix]
    public static void PdaStartPostfix(uGUI_PDA __instance)
    {
        // Create UI
        var contentParent = __instance.transform.Find("Content");

        // Create parent
        var managerObject = new GameObject("ChecklistHeadsUpManager");
        var managerTransform = managerObject.AddComponent<RectTransform>();
        managerTransform.SetParent(contentParent);
        managerTransform.anchorMin = Vector2.zero;
        managerTransform.anchorMax = Vector2.one;
        managerTransform.localScale = Vector3.one;
        managerTransform.localPosition = new Vector3(190, 415);
        
        // Create text object
        var notification = new GameObject("ChecklistNotification");
        notification.SetActive(false);
        var notificationTransform = notification.AddComponent<RectTransform>();
        notificationTransform.SetParent(managerTransform);
        notificationTransform.localScale = Vector3.one;
        notificationTransform.anchorMin = new Vector2(0.5f, 0.5f);
        notificationTransform.anchorMax = new Vector2(0.5f, 0.5f);
        notificationTransform.localPosition = Vector2.zero;
        var notificationText = notification.AddComponent<TextMeshProUGUI>();
        notificationText.overflowMode = TextOverflowModes.Overflow;
        notificationText.text = "\u2193 " + Language.main.Get("ChecklistFirstTimeHeadsUp");
        notificationText.fontSize = 20;
        notificationText.raycastTarget = false;
        
        // Add animations to text
        var bob = notification.AddComponent<UIBobAnimation>();
        bob.distance = 5;
        bob.speed = 3;
        bob.axis = Vector2.up;
        notification.AddComponent<SimpleAnimation>().pulse = true;
        
        // Set references and behaviour
        var manager = managerObject.AddComponent<ChecklistHeadsUpManager>();
        manager.notificationObject = notification;
    }
}