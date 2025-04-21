using System.Collections;
using Nautilus.Utility;
using TheRedPlague.Mono.StoryContent;
using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Shuttle;

public class ShuttleExplodeOnCollide : MonoBehaviour
{
    public FakeRigidbody rb;
    public Collider myCollider;

    public float checkRate = 0.2f;
    public float checkRadius = 1.5f;
    public float checkHeight = 3f;

    private float _timeCheckAgain;

    private GameObject _seamothExplodeFx;

    private readonly Collider[] _overlapSphereResults = new Collider[3];
    private static readonly FMODAsset _explodeSound = AudioUtils.GetFmodAsset("ShuttleExplosion");

    private IEnumerator Start()
    {
        var seamothTask = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
        yield return seamothTask;
        var seamothPrefab = seamothTask.GetResult();
        _seamothExplodeFx = seamothPrefab.GetComponent<SeaMoth>().destructionEffect;
    }

    private void Update()
    {
        if (!rb.enabled) return;
        if (Time.time < _timeCheckAgain) return;
        _timeCheckAgain = Time.time + checkRate;
        var overlapping = Physics.OverlapSphereNonAlloc(transform.position + transform.up * checkHeight, checkRadius,
            _overlapSphereResults, -1, QueryTriggerInteraction.Ignore);
        if (ShouldExplode(overlapping))
            Explode();
    }

    private bool ShouldExplode(int overlaps)
    {
        if (overlaps <= 1) return false;
        bool explode = false;
        for (int i = 0; i < overlaps; i++)
        {
            var collider = _overlapSphereResults[i];
            if (collider == myCollider) continue;
            if (collider == null) continue;
            if (collider.isTrigger) continue;
            if (collider.gameObject.layer != 0 && collider.gameObject.layer != LayerID.TerrainCollider) continue;
            
            var creature = collider.GetComponentInParent<Creature>();
            if (creature != null)
            {
                creature.liveMixin.Kill();
                continue;
            }

            var dome = collider.GetComponentInParent<InfectionDomeController>();
            if (dome != null)
            {
                continue;
            }
            
            explode = true;
        }

        return explode;
    }

    private void Explode()
    {
        if (_seamothExplodeFx == null) return;

        Utils.PlayFMODAsset(_explodeSound, transform.position);
        var fx = Instantiate(_seamothExplodeFx);
        fx.transform.position = transform.position;
        fx.SetActive(true);
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.startSizeMultiplier *= 3;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        fx.transform.localScale *= 5;
        fx.transform.GetChild(1).gameObject.SetActive(false);
        Destroy(fx.GetComponent<FMOD_StudioEventEmitter>());
        Destroy(fx, 20);
        Destroy(gameObject);
    }
}