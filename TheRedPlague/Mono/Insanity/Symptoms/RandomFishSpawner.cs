using System.Collections;
using TheRedPlague.Managers;
using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class RandomFishSpawner : InsanitySymptom
{
    private const float MinIntervalMinutes = 6;
    private const float MaxIntervalMinutes = 9;
    private const float MinInsanity = 20;

    private float _timeJumpScareAgain;

    private readonly TechType[] _fishTechTypes = {
        TechType.SpineEel,
        TechType.CrabSquid,
        TechType.Warper,
        TechType.Warper,
        TechType.Shocker
    };

    private bool CanJumpscare()
    {
        return Player.main.IsSwimming() && !Player.main.IsInside() && !Player.main.justSpawned && Ocean.GetDepthOf(Player.main.gameObject) > 10;
    }

    protected override void PerformSymptoms(float dt)
    {
        if (Time.time < _timeJumpScareAgain)
        {
            return;
        }
        
        if (CanJumpscare())
        {
            Jumpscare();
        }

        _timeJumpScareAgain = Time.time + Random.Range(MinIntervalMinutes * 60, MaxIntervalMinutes * 60);
    }

    private void Jumpscare()
    {
        var camTransform = MainCamera.camera.transform;
        StartCoroutine(SpawnFishAsync(GetRandomFishTechType(), camTransform.position + camTransform.forward * Random.Range(-2, -13)));
    }

    private TechType GetRandomFishTechType()
    {
        if (Player.main.transform.position.y < -300)
        {
            if (Random.value < 0.2f)
            {
                return Random.value > 0.5f ? ModPrefabs.MutantDiver1.TechType : ModPrefabs.MutantDiver2.TechType;
            }
        }

        return _fishTechTypes[Random.Range(0, _fishTechTypes.Length)];
    }

    private IEnumerator SpawnFishAsync(TechType techType, Vector3 location)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;
        var result = task.GetResult();
        if (!result) yield break;
        var fish = Instantiate(result, location, Quaternion.identity);
        fish.SetActive(true);
        ZombieManager.Zombify(fish);
        var despawn = fish.AddComponent<DespawnWhenOffScreen>();
        despawn.initialDelay = 20f;
        fish.AddComponent<PlaySoundWhenSeen>();
    }

    protected override IEnumerator OnLoadAssets()
    {
        yield break;
    }

    protected override void OnActivate()
    {
        
    }

    protected override void OnDeactivate()
    {
        
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return InsanityPercentage >= MinInsanity;
    }
}