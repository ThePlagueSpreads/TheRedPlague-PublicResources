using System.Collections;
using Story;
using TheRedPlague.PrefabFiles.Creatures;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.HoverPet;

public class HoverPetSpawner : MonoBehaviour
{
    private const float HoverPetSpawnInterval = 6.5f * 60;
    private const float SpawnDistance = 35;
    private const float MinDepth = 5;
    private const float MaxDepth = 350;
    private const int AttemptSpawnTries = 8;
    
    private bool _hoverPetTimerStarted;
    private float _hoverPetMinSpawnTime;
    
    public static Vector3 MazeBasePosition { get; } = new(-1250, -220, 700);
    
    private void Start()
    {
        InvokeRepeating(nameof(LazyUpdate), Random.value, 1);
    }

    private void LazyUpdate()
    {
        var goalManager = StoryGoalManager.main;
        if (goalManager == null || !goalManager.initialized)
            return;
        var hoverPet = HoverPetBehavior.Main;
        if (hoverPet == null)
        {
            if (ConditionsMet())
                TrySpawnHoverPet();
        }
        else
        {
            _hoverPetMinSpawnTime = DayNightCycle.main.timePassedAsFloat + HoverPetSpawnInterval;
        }
    }

    private void TrySpawnHoverPet()
    {
        // Start the spawn timer if it hasn't been started yet
        if (!_hoverPetTimerStarted)
        {
            _hoverPetTimerStarted = true;
            _hoverPetMinSpawnTime = DayNightCycle.main.timePassedAsFloat + HoverPetSpawnInterval;
            return;
        }

        if (DayNightCycle.main.timePassedAsFloat > _hoverPetMinSpawnTime)
        {
            if (CanHoverPetSpawn())
                SpawnHoverPet();
        }
    }

    private void SpawnHoverPet()
    {
        if (GenericTrpUtils.TryGetSpawnPositionBehindPlayer(out var pos, SpawnDistance, AttemptSpawnTries))
        {
            StartCoroutine(SpawnHoverPetCoroutine(pos));
            _hoverPetMinSpawnTime = DayNightCycle.main.timePassedAsFloat + HoverPetSpawnInterval;
        }
    }

    private IEnumerator SpawnHoverPetCoroutine(Vector3 spawnLocation)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(Hippopenomenon.Info.TechType);
        yield return task;
        var pet = Instantiate(task.GetResult(), spawnLocation, Quaternion.identity);
        LargeWorld.main.streamer.cellManager.RegisterEntity(pet);
    }

    private bool ConditionsMet()
    {
        if (!StoryGoalManager.main.IsGoalComplete(StoryUtils.ArriveToDomeGoal.key))
            return false;
        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.ScanPlagueAltarGoal.key))
            return false;
        return true;
    }

    private bool CanHoverPetSpawn()
    {
        var depth = Ocean.GetDepthOf(Player.main.gameObject);
        if (depth is < MinDepth or > MaxDepth)
            return false;
        if (!Player.main.IsUnderwaterForSwimming())
            return false;
        return true;
    }
}