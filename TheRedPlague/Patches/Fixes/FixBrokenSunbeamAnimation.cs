using System;
using HarmonyLib;

namespace TheRedPlague.Patches.Fixes;

[HarmonyPatch]
public static class FixBrokenSunbeamAnimation
{
    private static bool _overrodeGunDisabled;
    private static bool _isGunActuallyDisabled;
    
    // hacky fix to 'disable the gun' when the sunbeam occurs so that it gets canceled
    [HarmonyPatch(typeof(StoryGoalCustomEventHandler), nameof(StoryGoalCustomEventHandler.NotifyGoalComplete))]
    [HarmonyPrefix]
    public static void NotifyGoalCompletePrefix(StoryGoalCustomEventHandler __instance, string key)
    {
        var wasSunbeamEventJustDisabled = false;
        var sunbeamGoals = __instance.sunbeamGoals;
        foreach (var sunbeamGoal in sunbeamGoals)
        {
            if (string.Equals(key, sunbeamGoal.trigger, StringComparison.OrdinalIgnoreCase))
            {
                wasSunbeamEventJustDisabled = true;
            }
        }

        _overrodeGunDisabled = wasSunbeamEventJustDisabled;
        _isGunActuallyDisabled = __instance.gunDisabled;
        // pretend the gun is disabled
        if (_overrodeGunDisabled)
        {
            __instance.gunDisabled = true;
        }
    }

    // clean up our dirty hack
    [HarmonyPatch(typeof(StoryGoalCustomEventHandler), nameof(StoryGoalCustomEventHandler.NotifyGoalComplete))]
    [HarmonyPostfix]
    public static void NotifyGoalCompletePostfix(StoryGoalCustomEventHandler __instance, string key)
    {
        if (_overrodeGunDisabled)
        {
            __instance.gunDisabled = _isGunActuallyDisabled;
            _overrodeGunDisabled = false;
        }
    }
}