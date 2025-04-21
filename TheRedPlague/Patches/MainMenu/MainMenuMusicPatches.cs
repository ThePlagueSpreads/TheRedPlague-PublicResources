using System.Collections;
using FMOD;
using HarmonyLib;
using Nautilus.Extensions;
using Nautilus.Handlers;
using UnityEngine;
using UWE;

namespace TheRedPlague.Patches.MainMenu;

[HarmonyPatch(typeof(MainMenuMusic))]
public static class MainMenuMusicPatches
{
    private static bool _customMusicApplied;
    private static Channel _musicChannel;

    [HarmonyPatch(nameof(MainMenuMusic.Play))]
    [HarmonyPrefix]
    private static void MainMenuMusicPlayPrefix()
    {
        ApplyMusic(MainMenuMusic.main);

        if (!_customMusicApplied)
        {
            return;
        }
        
        MainMenuMusic.main.evt = default;
        CustomSoundHandler.TryPlayCustomSound(MainMenuMusic.main.music.path, out _musicChannel);
    }
    
    [HarmonyPatch(nameof(MainMenuMusic.Stop))]
    [HarmonyPrefix]
    private static void MainMenuMusicStopPrefix()
    {
        if (!_customMusicApplied)
        {
            return;
        }
        
        if (!_musicChannel.hasHandle() || !_musicChannel.isPlaying(out var isPlaying).CheckResult() || !isPlaying)
        {
            return;
        }

        var fadeDuration = 0.81f;
        _musicChannel.AddFadeOut(fadeDuration, out _);
        CoroutineHost.StartCoroutine(StopMusicCoroutine(fadeDuration, _musicChannel));
        _musicChannel.clearHandle();
        _customMusicApplied = false;
    }
    
    private static IEnumerator StopMusicCoroutine(float seconds, Channel channelToStop)
    {
        yield return new WaitForSeconds(seconds);
#pragma warning disable Harmony003
        channelToStop.stop();
#pragma warning restore Harmony003
    }
    
    private static void ApplyMusic(MainMenuMusic instance)
    {
        if (Plugin.Options.DisableRedPlagueMenuMusic)
        {
            return;
        }

        var newMusic = ScriptableObject.CreateInstance<FMODAsset>();
        newMusic.path = "ProjectTRP";
        instance.music = newMusic;
        _customMusicApplied = true;
    }

}