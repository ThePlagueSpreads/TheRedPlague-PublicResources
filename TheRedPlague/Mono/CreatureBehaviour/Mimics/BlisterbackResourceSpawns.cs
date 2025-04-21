using System.Collections;
using UnityEngine;
using UWE;

namespace TheRedPlague.Mono.CreatureBehaviour.Mimics;

public class BlisterbackResourceSpawns : MonoBehaviour
{
    private const float ZAxisNoiseOffset = 30;
    private readonly Color _barnacleGlowColor = new(10f, 0, 0.7f);
    private readonly Color _plantGlowColor = new(2.4f, 0.1f, 0.1f);

    public Transform spawnPointsParent;

    public float baseSpawnRate = 0.4f;
    public float minNoiseValueToSpawn = 0.32f;

    private readonly NoiseSettings _availableSpawnSlotsNoiseSettings = new()
    {
        Frequency = 0.33f
    };

    private readonly NoiseSettings _spawnSlotTypeNoiseSettings = new()
    {
        Frequency = 0.09f,
        SeedOffset = 15,
        Min = -0.05f,
        Max = 1.05f
    };

    private readonly string[] _classIds =
    {
        // "fc7c1098-13af-417a-8038-0053b65498e5", // Acid mushrooms
        "MeatShroom",
        "MeatShroom",
        "99cdec62-302b-4999-ba49-f50c73575a4d", // Acid mushrooms
        "31834aae-35ce-49c1-b5ba-ac4227750679", // Acid mushrooms
        "PlagueResource",
        "ae210dd4-68f0-4c77-9025-ef7d116948b3", // Grass (red)
        "4601400c-5e12-4e4a-9e45-4cab5f06a598", // Furled papyrus
        "1a806d20-dc8f-4e6e-9281-f353ed155abf", // Regress shell
        "99bbd145-d50e-4afb-bff0-27b33243642b", // Rouge cradle
        "PlagueResource",
        "84794dd0-2c70-4239-9536-230d56811ad4", // Tiger plant
        "e80b22ff-064d-46ca-b71e-456d6b3426ab", // Veined nettle
        "36fcb5c8-07f6-4d20-b026-f8c41b8e2358", // Violet beau
        "7c6d23d1-4d59-49f8-ac12-b12dfa530beb", // Writhing weed
        "171c6a5b-879b-4785-be7a-6584b2c8c442", // Brain coral
        "061af756-643c-42ad-9645-a522f1338084", // Slanted shell plate
        "f0713f3d-586b-4c71-88a3-18dd6c3dd2a4", // Table coral
        "7f656699-358a-416d-9ecd-f911e3d51bf1", // Veined shell plate
        "31ccc496-c26b-4ed9-8e86-3334582d8d5b", // Barnacles
        "4bc33bd6-cfa1-46a7-bac8-074ba3b76044", // Barnacles
        "Patrick1",
        "PlagueResource",
        "Patrick2",
        "9eb8239b-5b21-4258-9ff7-899cb8df0976", // Pygmy fan
    };

    private GameObject[] _prefabs;

    private IEnumerator Start()
    {
        yield return SpawnCoroutine();
    }

    private IEnumerator SpawnCoroutine()
    {
        _prefabs = new GameObject[_classIds.Length];
        for (var i = 0; i < _classIds.Length; i++)
        {
            var classId = _classIds[i];
            var task = PrefabDatabase.GetPrefabAsync(classId);
            yield return task;
            if (!task.TryGetPrefab(out var prefab))
                Plugin.Logger.LogWarning($"Failed to load prefab by Class ID '{classId}' for infested reefback!");
            _prefabs[i] = prefab;
        }

        foreach (Transform child in spawnPointsParent)
        {
            if (!ShouldSlotSpawn(child))
                continue;
            var prefabIndex =
                GetBestPrefabForNoiseValue(EvaluateNoiseAtPosition(_spawnSlotTypeNoiseSettings, child.position));
            var prefabToUse = _prefabs[prefabIndex];
            if (prefabToUse == null)
                continue;
            var spawned = UWE.Utils.InstantiateDeactivated(prefabToUse, child, Vector3.zero, Quaternion.identity);
            var zUp = WorldEntityDatabase.TryGetInfo(_classIds[prefabIndex], out var worldInfo) && worldInfo.prefabZUp;
            if (zUp)
            {
                spawned.transform.forward = child.up;
                spawned.transform.Rotate(Vector3.forward, Random.value * 360, Space.Self);
            }
            else
            {
                spawned.transform.up = child.up;
                spawned.transform.Rotate(Vector3.up, Random.value * 360, Space.Self);
            }

            if (spawned.TryGetComponent<SkyApplier>(out var sa) && sa.renderers != null)
            {
                foreach (var renderer in sa.renderers)
                {
                    var material = renderer.material;
                    if (material.HasProperty(ShaderPropertyID._GlowColor))
                    {
                        material.SetColor(ShaderPropertyID._GlowColor,
                            material.name.Contains("barnacle") ? _barnacleGlowColor : _plantGlowColor);
                    }
                }
            }

            spawned.AddComponent<ReefbackPlant>();
            spawned.SetActive(true);
        }
    }

    private float EvaluateNoiseAtPosition(NoiseSettings settings, Vector3 position)
    {
        var noise = Mathf.PerlinNoise(
            position.x * settings.Frequency + settings.SeedOffset,
            (position.z + ZAxisNoiseOffset) * settings.Frequency + settings.SeedOffset);
        return Mathf.Lerp(settings.Min, settings.Max, noise);
    }

    private int GetBestPrefabForNoiseValue(float input)
    {
        return Mathf.Clamp(Mathf.RoundToInt((_classIds.Length - 1) * input), 0, _classIds.Length - 1);
    }

    private bool ShouldSlotSpawn(Transform spawnPoint)
    {
        if (Random.value > baseSpawnRate)
            return false;
        return EvaluateNoiseAtPosition(_availableSpawnSlotsNoiseSettings, spawnPoint.position) > minNoiseValueToSpawn;
    }

    private struct NoiseSettings
    {
        public NoiseSettings()
        {
            Frequency = 0;
        }

        public float Frequency { get; init; }
        public float Min { get; init; } = 0f;
        public float Max { get; init; } = 1f;
        public float SeedOffset { get; init; }
    }
}