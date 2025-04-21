﻿using TheRedPlague.Mono.CreatureBehaviour.Mutants;
using TheRedPlague.Mono.VFX;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DespawnWhenOffScreen : MonoBehaviour
{
    public float initialDelay = 2f;
    public float minDotProductForOnScreen = 0.1f;
    public bool despawnIfTooClose;
    public float minDistance;
    public bool waitUntilSeen;
    public bool despawnIfViewNotClear;
    public bool playSoundWhenFound;
    public bool rareJumpscare;

    private bool _wasSeen;
    private float _timeWasSeen;

    public bool moveInstead;
    public float moveRadius = 20f;

    public bool disappearWhenLookedAtForTooLong;
    public float maxLookTime = 10f;

    public bool jumpscareWhenTooClose;

    private bool _despawning;
    private int _terrainMask;

    private void Start()
    {
        InvokeRepeating(nameof(Check), initialDelay, 0.05f);
    }

    private void Check()
    {
        if (waitUntilSeen)
        {
            if (!_wasSeen)
            {
                if (GenericTrpUtils.IsPositionOnScreen(transform.position, minDotProductForOnScreen))
                {
                    _wasSeen = true;
                    _timeWasSeen = Time.time;
                }
                return;
            }
        }

        bool onScreen = GenericTrpUtils.IsPositionOnScreen(transform.position, minDotProductForOnScreen);
        if (!onScreen)
        {
            Despawn(0f);
        }
        else if (disappearWhenLookedAtForTooLong && Time.time > _timeWasSeen + maxLookTime)
        {
            Despawn(0f);
        }
        else
        {
            _terrainMask = LayerMask.GetMask("Terrain");
            if (despawnIfViewNotClear && Physics.Raycast(new Ray(transform.position, (MainCamera.camera.transform.position - transform.position).normalized), (MainCamera.camera.transform.position - transform.position).magnitude - 2f, _terrainMask, QueryTriggerInteraction.Ignore))
            {
                Despawn(0f);
            }
        }
    }

    private void Update()
    {
        if (_despawning)
            return;
        
        var minDistanceToUse = Player.main.IsInBase() ? minDistance / 3 : minDistance;
        if (despawnIfTooClose && Vector3.SqrMagnitude(transform.position - MainCamera.camera.transform.position) < minDistanceToUse * minDistanceToUse)
        {
            if (jumpscareWhenTooClose)
            {
                var canJumpScareThisFrame = !rareJumpscare || Random.value < 0.02f * Time.deltaTime;
                if (!canJumpScareThisFrame) return;
                Despawn(2f);
                if (!Player.main.IsInBase() && !Plugin.Options.DisableJumpScares)
                    DeathScare.PlayDeathScare();
            }
            else
            {
                Despawn(0.15f);
                FadingOverlay.PlayFX(Color.black, 0.1f, 0.1f, 0.1f);
            }
        }
    }

    private void Despawn(float timer)
    {
        _despawning = true;
        if (moveInstead)
        {
            if (GenericTrpUtils.TryGetSpawnPositionOnGround(out var pos, moveRadius, 25, minDistance + 1f))
            {
                transform.position = pos;
                var diff = pos - Player.main.transform.position;
                diff.y = 0;
                transform.forward = diff.normalized;
                return;
            }
        }
        Destroy(gameObject, timer);
    }
}