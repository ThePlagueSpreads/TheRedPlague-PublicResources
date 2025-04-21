using System.Collections.Generic;
using HarmonyLib;
using Story;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.Patches.UI;

[HarmonyPatch]
public static class AlterraRadioIconPatcher
{
    private static float _applyTime;

    private static bool IsApplied => Time.time < _applyTime + 10;
    private static Sprite _oldSprite;
    private static Color _oldColor;

    // manually updated list before we get a better system i guess
    private static readonly List<string> _radioMessagesToUseAlterraSymbol = new()
    {
        "AngelinaIntroduction",
        "ResearchTeamARadioLog",
        "Act2MissionInstructions",
        "Act2MissionInstructions",
        "LostRiverCyclopsBroadcast"
    };

    private static Sprite AlterraRadioIcon { get; } =
        Plugin.AssetBundle.LoadAsset<Sprite>("AlterraRadioMessage");
    
    [HarmonyPatch(typeof(StoryGoalManager), nameof(StoryGoalManager.PulsePendingMessages))]
    [HarmonyPrefix]
    public static void PulsePendingMessagesPostfix()
    {
        var pending = StoryGoalManager.main.pendingRadioMessages;
        if (pending == null || pending.Count < 1) return;
        var nextMessage = pending[0];
        if (string.IsNullOrEmpty(nextMessage)) return;
        if (ShouldUseAlterraIcon(nextMessage))
        {
            _applyTime = Time.time;
        }
    }
    
    [HarmonyPatch(typeof(uGUI_RadioMessageIndicator), nameof(uGUI_RadioMessageIndicator.NewRadioMessage))]
    [HarmonyPostfix]
    public static void NewRadioMessagePostfix(uGUI_RadioMessageIndicator __instance, bool newMessages)
    {
        SetIcon(__instance, IsApplied);
    }

    private static bool ShouldUseAlterraIcon(string key)
    {
        return _radioMessagesToUseAlterraSymbol.Contains(key);
    }

    private static void SetIcon(uGUI_RadioMessageIndicator messageIndicator, bool useAlterraIcon)
    {
        if (_oldSprite == null)
        {
            _oldSprite = messageIndicator.sprite.sprite;
            _oldColor = messageIndicator.sprite.color;
        }

        messageIndicator.sprite.sprite = useAlterraIcon ? AlterraRadioIcon : _oldSprite;
        messageIndicator.sprite.color = useAlterraIcon ? Color.red : _oldColor;

        var colorInLateUpdate = messageIndicator.GetComponent<ColorImageInLateUpdate>();
        if (colorInLateUpdate == null)
        {
            colorInLateUpdate = messageIndicator.gameObject.AddComponent<ColorImageInLateUpdate>();
            colorInLateUpdate.newColor = Color.red;
            colorInLateUpdate.image = messageIndicator.sprite;
        }

        colorInLateUpdate.enabled = useAlterraIcon;
    }
}