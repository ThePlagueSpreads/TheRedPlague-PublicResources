using System.Collections;
using Nautilus.Utility;
using TheRedPlague.Mono.Util;
using TheRedPlague.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.Insanity.Symptoms;

// har har har har har har har har har har  har har har har har har har har
public class JumpScares : InsanitySymptom
{
    private const float MinDelay = 60 * 5;
    private const float MaxDelay = 60 * 10;
    private const float Radius = 40;
    private const float MinInsanity = 35;
    private float _timeSpawnAgain;

    private static readonly FMODAsset JumpscareSound = AudioUtils.GetFmodAsset("WarperJumpscare");

    private readonly string[] _corpseClassIDs = new[] {"InfectedCorpse", "SkeletonCorpse"};

    public static JumpScares main;

    private void Awake()
    {
        main = this;
    }

    public void JumpScareNow()
    {
        JumpScare();
    }

    private void Start()
    {
        _timeSpawnAgain = Time.time + Random.Range(MinDelay, MaxDelay);
    }

    private GameObject Spawn()
    {
        var model = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("SharkJumpscare"));
        MaterialUtils.ApplySNShaders(model);
        var despawn = model.AddComponent<DespawnWhenOffScreen>();
        despawn.despawnIfTooClose = true;
        despawn.minDistance = 4;
        despawn.waitUntilSeen = true;
        despawn.moveInstead = true;
        despawn.moveRadius = 35f;
        despawn.disappearWhenLookedAtForTooLong = true;
        despawn.jumpscareWhenTooClose = true;
        model.AddComponent<Util.LookAtPlayer>();
        if (Player.main.IsInBase())
        {
            model.transform.localScale *= 0.7f;
        }
        return model;
    }

    private IEnumerator SpawnCorpse(Vector3 pos)
    {
        var classID = _corpseClassIDs[Random.Range(0, _corpseClassIDs.Length)];
        var task = UWE.PrefabDatabase.GetPrefabAsync(classID);
        yield return task;
        task.TryGetPrefab(out var prefab);
        var spawned = Instantiate(prefab, pos, Random.rotation);
        spawned.SetActive(true);
        var despawn = spawned.AddComponent<DespawnWhenOffScreen>();
        despawn.initialDelay = 20;
        despawn.waitUntilSeen = true;
        despawn.despawnIfTooClose = true;
        despawn.minDistance = 3;
        despawn.jumpscareWhenTooClose = true;
        despawn.rareJumpscare = true;
        Destroy(spawned.GetComponent<PrefabIdentifier>());
        var entity = spawned.GetComponent<LargeWorldEntity>();
        LargeWorld.main.streamer.cellManager.UnregisterEntity(entity);
        Destroy(entity);
        Destroy(spawned, 300);
    }

    private void Update()
    {
        if (Time.time > _timeSpawnAgain)
        {
            _timeSpawnAgain = Time.time + Random.Range(MinDelay, MaxDelay);
            JumpScare();
        }
    }

    private void JumpScare()
    {
        if (Plugin.Options.DisableJumpScares) return;
        if (!IsSymptomActive) return;
        var jumpscarePosition = MainCamera.camera.transform.position;
        if (GenericTrpUtils.TryGetSpawnPositionOnGround(out jumpscarePosition, Radius, 50, Radius / 2f))
        {
            var model = Spawn();
            model.transform.position = jumpscarePosition;
            var diff = jumpscarePosition - Player.main.transform.position;
            diff.y = 0;
            model.transform.forward = diff.normalized;
            if (InsanityPercentage >= 45)
            {
                StartCoroutine(SpawnCorpse(jumpscarePosition + Vector3.up * 2));
            }
            
            if (Random.value < 0.4f && !StoryUtils.IsAct1Complete())
            {
                Utils.PlayFMODAsset(JumpscareSound, jumpscarePosition);
            }
        }
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