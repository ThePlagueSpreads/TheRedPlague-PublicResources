using HarmonyLib;
using Nautilus.Utility;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Mono.SFX;

namespace TheRedPlague.Patches.ObjectEdits;

[HarmonyPatch(typeof(Creature))]
public static class WarperPatcher
{
    [HarmonyPatch(nameof(Creature.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(Creature __instance)
    {
        if (__instance is not Warper) return;
        
        var sounds = __instance.gameObject.EnsureComponent<PlayRandomSounds>();
        var emitter = __instance.gameObject.AddComponent<FMOD_CustomEmitter>();
        emitter.SetAsset(AudioUtils.GetFmodAsset("InfectedWarperIdle"));
        emitter.followParent = true;
        emitter.playOnAwake = false;
        sounds.minDelay = 10;
        sounds.maxDelay = 25;
        sounds.emitter = emitter;

        __instance.gameObject.EnsureComponent<WarperDropHeartOnDeath>();
        __instance.gameObject.EnsureComponent<InfectOnStart>();
    }
}