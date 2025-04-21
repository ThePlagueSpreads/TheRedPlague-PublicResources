using System;
using System.Collections;
using System.Collections.Generic;
using TheRedPlague.Mono.Insanity.Symptoms.Bases;
using TheRedPlague.Mono.Util;
using TheRedPlague.Mono.VFX;
using TheRedPlague.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class FakeCreatureHallucination : TimedHallucinationSymptom
{
    protected override IEnumerator OnLoadAssets()
    {
        yield break;
    }

    private const float SpawnDistance = 32;
    private const int SpawnTries = 32;

    protected override float MinInterval => 17;
    protected override float MaxInterval => 27;
    protected override float MinInsanity => 8;
    protected override float MaxInsanity => 90;
    protected override float ChanceAtMinInsanity => 0.10f;
    protected override float ChanceAtMaxInsanity => 0.9f;

    private static readonly TechType[] LeviathanTechTypes =
    {
        TechType.ReaperLeviathan,
        TechType.SeaDragon,
        TechType.GhostLeviathan,
        TechType.GhostLeviathanJuvenile
    };

    private bool _lastSpawnWasLeviathan;

    #region Spawns

    private static readonly TechType[] GlobalCreaturePool =
    {
        TechType.Peeper,
        TechType.Bladderfish,
        TechType.Stalker,
        TechType.Bleeder,
        TechType.Warper,
        TechType.Sandshark,
        TechType.Crash,
        TechType.BoneShark,
        TechType.CrabSquid,
        TechType.Boomerang,
        TechType.Reginald
    };

    private static readonly List<(string, TechType[])> BiomeOverrides = new()
    {
        ("safe", new[]
            {
                TechType.Peeper,
                TechType.Gasopod,
                TechType.Stalker,
                TechType.Boomerang,
                TechType.Bladderfish,
                TechType.Crash,
                TechType.GarryFish
            }
        ),
        ("kelp", new[]
            {
                TechType.Peeper,
                TechType.Gasopod,
                TechType.Stalker,
                TechType.Boomerang,
                TechType.Bladderfish,
                TechType.Sandshark,
                TechType.Bleeder,
                TechType.Mesmer,
                TechType.Hoverfish
            }
        ),
        ("grassy", new[]
            {
                TechType.Reginald,
                TechType.Crabsnake,
                TechType.Boomerang,
                TechType.Hoopfish,
                TechType.Sandshark,
                TechType.Bleeder,
                TechType.Spadefish
            }
        ),
        ("crashZone", new[]
            {
                TechType.ReaperLeviathan,
                TechType.Sandshark,
                TechType.Stalker,
                TechType.Peeper,
                TechType.Eyeye,
                TechType.BoneShark,
                TechType.Reginald,
                TechType.HoleFish
            }
        ),
        ("mountains", new[]
            {
                TechType.ReaperLeviathan,
                TechType.Biter,
                TechType.Hoopfish,
                TechType.Warper,
                TechType.Boomerang,
                TechType.Jumper,
                TechType.Reginald
            }
        ),
        ("jellyshroom", new[]
            {
                TechType.Crabsnake,
                TechType.Oculus,
                TechType.Oculus,
                TechType.Eyeye,
                TechType.Eyeye
            }
        ),
        ("dunes", new[]
            {
                TechType.ReaperLeviathan,
                TechType.Biter,
                TechType.Hoopfish,
                TechType.Sandshark,
                TechType.Stalker,
                TechType.Eyeye,
                TechType.Gasopod
            }
        ),
        ("bloodKelp", new[]
            {
                TechType.Shuttlebug,
                TechType.GhostLeviathan,
                TechType.Shocker,
                TechType.Warper,
                TechType.CrabSquid,
                TechType.Blighter,
                TechType.Spinefish
            }
        ),
        ("lostRiver", new[]
            {
                TechType.SpineEel,
                TechType.GhostLeviathanJuvenile,
                TechType.GhostRayBlue,
                TechType.Warper,
                TechType.CrabSquid,
                TechType.Spinefish,
                TechType.Mesmer
            }
        ),
        ("ilz", new[]
            {
                TechType.LavaLizard,
                TechType.GhostRayRed,
                TechType.SeaDragon,
                TechType.LavaLarva,
                TechType.LavaBoomerang,
                TechType.LavaBoomerang,
                TechType.LavaEyeye,
                TechType.LavaEyeye
            }
        ),
        ("lava", new[]
            {
                TechType.LavaLizard,
                TechType.GhostRayRed,
                TechType.SeaDragon,
                TechType.LavaLarva,
                TechType.LavaBoomerang,
                TechType.LavaBoomerang,
                TechType.LavaEyeye,
                TechType.LavaEyeye
            }
        ),
        ("precursor", new[]
            {
                TechType.PrecursorDroid,
                TechType.PrecursorDroid,
                TechType.Warper,
                TechType.Shuttlebug
            }
        )
    };

    #endregion Spawns

    protected override void OnActivate()
    {
    }

    protected override void OnDeactivate()
    {
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return base.ShouldDisplaySymptoms() && Player.main.IsUnderwaterForSwimming();
    }

    protected override void PerformTimedAction()
    {
        StartCoroutine(SpawnCreatureAsync());
    }

    private IEnumerator SpawnCreatureAsync()
    {
        // -- Determine spawn pos --
        if (!GenericTrpUtils.TryGetSpawnPositionBehindPlayer(out var pos, SpawnDistance, SpawnTries))
            pos = MainCamera.camera.transform.position - MainCamera.camera.transform.forward * SpawnDistance;

        // -- Load assets --
        var creatureTechType = GetRandomCreature();
        if (creatureTechType == TechType.None)
            yield break;
        // Don't spawn leviathans back-to-back
        if (_lastSpawnWasLeviathan && IsTechTypeLeviathan(creatureTechType))
        {
            _lastSpawnWasLeviathan = false;
            yield break;
        }
        _lastSpawnWasLeviathan = IsTechTypeLeviathan(creatureTechType);
        var task = CraftData.GetPrefabForTechTypeAsync(creatureTechType);
        yield return task;

        // -- Spawn the creature --
        var creature = UWE.Utils.InstantiateDeactivated(task.GetResult(), pos, Random.rotation);
        DestroyImmediate(creature.GetComponent<PrefabIdentifier>());
        DestroyImmediate(creature.GetComponent<LargeWorldEntity>());

        // -- Modify the creature --

        // Disable colliders
        foreach (var collider in creature.GetComponentsInChildren<Collider>())
        {
            if (collider.isTrigger)
                collider.enabled = false;
        }

        // Disable pickupability
        var pickupable = creature.GetComponent<Pickupable>();
        if (pickupable != null)
            pickupable.isPickupable = false;

        // Add fade out
        var fadeOut = creature.AddComponent<FadeOutOnApproach>();
        // (We do a little bit of trolling for pickupable fish)
        fadeOut.fadeOutRange = pickupable ? 5f : 16f;
        var isLarge = creature.TryGetComponent<LiveMixin>(out var lm) && lm.maxHealth > 4000;
        fadeOut.fadeDuration = isLarge ? 1.06f : 0.63f;
        
        // Enable the 'creature', release it into the wild!
        creature.SetActive(true);
        
        // Destroy the creature after several minutes
        Destroy(creature, 60 * 8);
        creature.AddComponent<DestroyWhenFarAway>().destroyDistance = 200;
    }

    private TechType GetRandomCreature()
    {
        var biome = Player.main.GetBiomeString();
        foreach (var overrideBiome in BiomeOverrides)
        {
            if (biome.StartsWith(overrideBiome.Item1, StringComparison.OrdinalIgnoreCase))
            {
                return overrideBiome.Item2[Random.Range(0, overrideBiome.Item2.Length)];
            }
        }

        return GlobalCreaturePool[Random.Range(0, GlobalCreaturePool.Length)];
    }

    private static bool IsTechTypeLeviathan(TechType techType)
    {
        foreach (var type in LeviathanTechTypes)
        {
            if (type == techType)
                return true;
        }

        return false;
    }
}