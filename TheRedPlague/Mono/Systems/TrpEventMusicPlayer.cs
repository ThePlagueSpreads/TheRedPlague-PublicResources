using System;
using UnityEngine;

namespace TheRedPlague.Mono.Systems;

public class TrpEventMusicPlayer : MonoBehaviour
{
    private static TrpEventMusicPlayer _instance;

    private float _timeCanPlayMusicAgain;

    private void Awake()
    {
        _instance = this;
    }

    public static void PlayMusic(FMODAsset asset, float duration)
    {
        try
        {
            if (Time.time < _instance._timeCanPlayMusicAgain) return;
            Utils.PlayFMODAsset(asset, Player.main.transform.position);
            _instance._timeCanPlayMusicAgain = Time.time + duration;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Error while attempting to play " + asset + ": " + e);
        }
    }
}