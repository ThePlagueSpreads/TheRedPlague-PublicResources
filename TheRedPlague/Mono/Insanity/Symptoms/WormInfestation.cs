using System.Collections;
using System.Collections.Generic;
using Nautilus.Utility;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class WormInfestation : InsanitySymptom
{
    private const float MinInsanity = 83f;
    private const float MinPlagueDamage = 60f;
    private const float MinSpawnDelay = 2;
    private const float MaxSpawnDelay = 8;
    private const float MinRemoveDelay = 0.4f;
    private const float MaxRemoveDelay = 1.4f;
    private const float WormScaleMin = 0.02f;
    private const float WormScaleMax = 0.04f;
    private const float ForwardTendency = -0.002f;
    
    private GameObject _meatWormPrefab;

    // Paths to bones are from the neck (Player/body/player_view/export_skeleton/head_rig/neck)
    // Generated on my laptop using C:\Modding\SNExport\ExportedProject\Assets\_LEE\GenerateWormSpawnPoints.cs
    private readonly WormSpawnPoint[] _spawnPoints =
    {
        new("chest/clav_L/clav_L_aim/shoulder_L/elbow_L/hand_L", new Vector3(-0.065f, -0.016f, 0.003f),
            new Vector3(349.679f, 54.34f, 196.043f)),
        new("chest/clav_L/clav_L_aim/shoulder_L/elbow_L/hand_L", new Vector3(0.022f, 0.011f, -0.008f),
            new Vector3(4.262f, 8.761f, 190.641f)),
        new("chest/clav_L/clav_L_aim/shoulder_L/elbow_L", new Vector3(0.068f, 0.005f, -0.025f),
            new Vector3(22.448f, 99.08f, 290.493f)),
        new("chest/clav_L/clav_L_aim/shoulder_L/elbow_L", new Vector3(0.161f, -0.004f, -0.027f),
            new Vector3(31.311f, 223.827f, 41.572f)),
        new("chest/clav_L/clav_L_aim/shoulder_L/elbow_L", new Vector3(0.13f, 0.013f, 0.018f),
            new Vector3(355.335f, 234.494f, 2.772f)),
        new("chest/clav_L/clav_L_aim/shoulder_L/elbow_L", new Vector3(0.023f, 0.021f, 0.008f),
            new Vector3(348.549f, 233.384f, 2.345f)),
        new("chest/clav_L/clav_L_aim/shoulder_L", new Vector3(0.219f, 0.013f, -0.016f),
            new Vector3(354.109f, 98.181f, 311.012f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R/hand_R", new Vector3(0.038f, 0.025f, 0.009f),
            new Vector3(358.683f, 59.811f, 23.265f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R/hand_R", new Vector3(0.006f, -0.016f, 0.007f),
            new Vector3(355.503f, 5.232f, 192.146f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R/hand_R", new Vector3(0.077f, -0.009f, -0.026f),
            new Vector3(345.262f, 143.406f, 177.226f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R/hand_R", new Vector3(0.082f, -0.005f, 0.022f),
            new Vector3(19.534f, 254.24f, 202.223f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R", new Vector3(0.01f, 0.014f, 0.041f),
            new Vector3(292.616f, 217.453f, 329.863f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R", new Vector3(-0.084f, -0.032f, -0.019f),
            new Vector3(26.212f, 352.465f, 176.4f)),
        new("chest/clav_R/clav_R_aim/shoulder_R/elbow_R", new Vector3(-0.163f, 0.012f, -0.007f),
            new Vector3(353.701f, 211.105f, 9.842f)),
        new("chest/clav_R/clav_R_aim/shoulder_R", new Vector3(-0.209f, 0.02f, -0.012f),
            new Vector3(65.58f, 120.662f, 323.734f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_L/calf_L/ankle_L", new Vector3(0.053f, 0.007f, -0.005f),
            new Vector3(3.518f, 104.858f, 359.315f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_L/calf_L", new Vector3(0.347f, -0.024f, 0.021f),
            new Vector3(312.837f, 0.659f, 181.955f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_L", new Vector3(-0.207f, -0.059f, 0.001f),
            new Vector3(342.264f, 262.704f, 143.2f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_L", new Vector3(-0.374f, -0.033f, 0.023f),
            new Vector3(61.372f, 205.987f, 218.47f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_R/calf_R/ankle_R", new Vector3(-0.018f, 0.03f, 0.022f),
            new Vector3(62.063f, 262.556f, 277.673f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_R", new Vector3(-0.215f, 0.078f, 0.02f),
            new Vector3(31.798f, 292.705f, 344.424f)),
        new("chest/spine_3/spine_2/spine_1/hips/thigh_R", new Vector3(-0.494f, 0.014f, -0.029f),
            new Vector3(314.767f, 335.854f, 29.276f)),
        new("chest/spine_3/spine_2/spine_1/hips", new Vector3(0.01f, -0.103f, 0.032f),
            new Vector3(321.983f, 345.214f, 178.68f)),
        new("chest/spine_3/spine_2/spine_1/hips", new Vector3(0.044f, 0.13f, 0.044f),
            new Vector3(354.019f, 108.701f, 23.297f)),
        new("chest/spine_3", new Vector3(-0.077f, 0.075f, 0.112f), new Vector3(348.159f, 96.53f, 54.832f)),
        new("chest/spine_3", new Vector3(0.039f, -0.074f, 0.149f), new Vector3(327.868f, 47.899f, 147.4f)),
        new("chest", new Vector3(0.314f, 0.093f, 0.075f), new Vector3(308.493f, 166.512f, 16.714f)),
        new("chest", new Vector3(0.283f, 0.105f, -0.069f), new Vector3(39.34f, 134.39f, 343.679f)),
    };

    private GameObject[] _worms;
    private Transform _neck;
    private readonly List<int> _tempListForRandomization = new();
    private static readonly int Burrowing = Animator.StringToHash("burrowing");

    protected override IEnumerator OnLoadAssets()
    {
        _meatWormPrefab = UWE.Utils.InstantiateDeactivated(Plugin.AssetBundle.LoadAsset<GameObject>("MeatwormPrefab"));
        var sa = _meatWormPrefab.AddComponent<SkyApplier>();
        sa.renderers = _meatWormPrefab.GetComponentsInChildren<Renderer>();
        sa.dynamic = true;
        MaterialUtils.ApplySNShaders(_meatWormPrefab, 5.76f);
        _worms = new GameObject[_spawnPoints.Length];
        _neck = Player.main.transform.Find("body/player_view/export_skeleton/head_rig/neck");
        yield break;
    }

    protected override void OnActivate()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnWormsCoroutine());
    }

    protected override void OnDeactivate()
    {
        StopAllCoroutines();
        StartCoroutine(RemoveWormsCoroutine());
    }

    private IEnumerator SpawnWormsCoroutine()
    {
        while (GetNumberOfActiveWorms() < _spawnPoints.Length)
        {
            yield return new WaitForSeconds(Random.Range(MinSpawnDelay, MaxSpawnDelay));
            SpawnRandomWorm();
        } 
    }
    
    private IEnumerator RemoveWormsCoroutine()
    {
        while (GetNumberOfActiveWorms() > 0)
        {
            yield return new WaitForSeconds(Random.Range(MinRemoveDelay, MaxRemoveDelay));
            RemoveRandomWorm();
        }
    }

    private int GetNumberOfActiveWorms()
    {
        var count = 0;
        
        foreach (var worm in _worms)
        {
            if (worm != null)
                count++;
        }

        return count;
    }

    private void SpawnRandomWorm()
    {
        SpawnWormAtIndex(GetRandomFreeSlot());
    }
    
    private void RemoveRandomWorm()
    {
        var index = GetRandomWormIndex();
        var worm = _worms[index];
        Destroy(worm, 4);
        _worms[index] = null;
        worm.GetComponentInChildren<Animator>().SetBool(Burrowing, true);
    }

    private int GetRandomFreeSlot()
    {
        for (var i = 0; i < _worms.Length; i++)
        {
            if (_worms[i] == null)
                _tempListForRandomization.Add(i);
        }

        var temp = _tempListForRandomization[Random.Range(0, _tempListForRandomization.Count)];
        _tempListForRandomization.Clear();
        return temp;
    }

    private int GetRandomWormIndex()
    {
        for (var i = 0; i < _worms.Length; i++)
        {
            if (_worms[i] != null)
                _tempListForRandomization.Add(i);
        }

        var temp = _tempListForRandomization[Random.Range(0, _tempListForRandomization.Count)];
        _tempListForRandomization.Clear();
        return temp;
    }
    
    private void SpawnWormAtIndex(int index)
    {
        var spawnPoint = _spawnPoints[index];
        var parent = _neck.Find(spawnPoint.PathToBone);
        var worm = Instantiate(_meatWormPrefab, parent);
        worm.transform.localScale = Vector3.one * Random.Range(WormScaleMin, WormScaleMax);
        worm.transform.localPosition = spawnPoint.LocalPosition;
        worm.transform.localEulerAngles = spawnPoint.LocalEulerAngles;
        worm.transform.position += worm.transform.up * ForwardTendency;
        worm.SetActive(true);
        _worms[index] = worm;
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return InsanityPercentage >= MinInsanity && PlagueDamageStat.main.InfectionPercent >= MinPlagueDamage;
    }

    private struct WormSpawnPoint
    {
        public WormSpawnPoint(string pathToBone, Vector3 localPosition, Vector3 localEulerAngles)
        {
            LocalPosition = localPosition;
            LocalEulerAngles = localEulerAngles;
            PathToBone = pathToBone;
        }

        public Vector3 LocalPosition { get; }
        public Vector3 LocalEulerAngles { get; }
        public string PathToBone { get; }
    }
}