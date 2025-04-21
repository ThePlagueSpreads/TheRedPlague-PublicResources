using System.Collections.Generic;
using Nautilus.Utility;
using Story;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.HoverPet;

public class HoverPetBehavior : MonoBehaviour
{
    private const float NoticeDistance = 13;
    private const float DespawnDistance = 300;
    private const float FindMazeBaseDistance = 100;
    private const float MinimumLifeTime = 80;
    private const float DestroyTimerSeconds = 15;
    private const float InitialVoiceLineDelay = 20;
    private const float RepeatedVoiceLineDelay = 80;
    private const float MaxDistanceForVoiceLines = 100;

    private static readonly FMODAsset[] VoiceLines =
    {
        AudioUtils.GetFmodAsset("HoverPetPDA1"),
        AudioUtils.GetFmodAsset("HoverPetPDA2"),
        AudioUtils.GetFmodAsset("HoverPetPDA3"),
        AudioUtils.GetFmodAsset("HoverPetPDA4")
    };

    public static HoverPetBehavior Main { get; private set; }

    private bool _noticedByPlayer;
    private bool _foundBase;

    private float _startDespawnTime;
    private float _destroyTime;
    private bool _destroying;

    private float _timeNextVoiceLine;
    private int _lastVoiceLineIndex = -1;
    private bool _playedVoiceLine;

    public bool ShouldSwimTowardsMazeBase => _noticedByPlayer && !_foundBase;

    private void Awake()
    {
        Main = this;
    }

    private void Start()
    {
        InvokeRepeating(nameof(LazyUpdate), Random.value, 0.3f);
        _startDespawnTime = Time.time + MinimumLifeTime;
        _foundBase = StoryGoalManager.main.IsGoalComplete(StoryUtils.HoverPetReachedMazeBaseGoal.key);
        _timeNextVoiceLine = Time.time + InitialVoiceLineDelay;
    }

    private void LazyUpdate()
    {
        if (!isActiveAndEnabled)
            return;

        var distanceToPlayer = Vector3.Distance(Player.main.transform.position, transform.position);
        
        if (_noticedByPlayer && !_foundBase && distanceToPlayer > DespawnDistance)
        {
            Destroy(gameObject);
            return;
        }

        if (_foundBase) return;

        if (!_noticedByPlayer)
        {
            if (Time.time > _timeNextVoiceLine && distanceToPlayer < MaxDistanceForVoiceLines)
            {
                PlayRandomVoiceLine();
            }
            
            // Check for being noticed
            var onScreen = GenericTrpUtils.IsPositionOnScreen(transform.position);

            if (onScreen)
            {
                _destroying = false;
            }

            if (onScreen && Vector3.Distance(transform.position, MainCamera.camera.transform.position) < NoticeDistance)
            {
                NoticedByPlayer();
            }

            // [Destroy when off-screen]
            if (Time.time > _startDespawnTime && !onScreen)
            {
                if (!_destroying)
                {
                    _destroyTime = Time.time + DestroyTimerSeconds;
                    _destroying = true;
                }
                else if (Time.time > _destroyTime)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (Vector3.Distance(transform.position, HoverPetSpawner.MazeBasePosition) < FindMazeBaseDistance)
        {
            StoryUtils.HoverPetReachedMazeBaseGoal.Trigger();
            _foundBase = true;
        }
    }

    private void NoticedByPlayer()
    {
        _noticedByPlayer = true;

        if (!_playedVoiceLine)
        {
            PlayRandomVoiceLine();
        }
    }

    private void PlayRandomVoiceLine()
    {
        var line = GetRandomVoiceLine();
        FMODUWE.PlayOneShot(line, transform.position);
        Subtitles.Add(line.path);
        _playedVoiceLine = true;
        _timeNextVoiceLine = Time.time + RepeatedVoiceLineDelay;
    }

    private FMODAsset GetRandomVoiceLine()
    {
        var lines = new List<FMODAsset>(4);
        for (int i = 0; i < VoiceLines.Length; i++)
        {
            if (i != _lastVoiceLineIndex)
                lines.Add(VoiceLines[i]);
        }

        var chosenIndex = Random.Range(0, lines.Count);
        for (int i = 0; i < VoiceLines.Length; i++)
        {
            if (VoiceLines[i] == lines[chosenIndex])
            {
                _lastVoiceLineIndex = i;
                break;
            }
        }
        return lines[chosenIndex];
    }
}