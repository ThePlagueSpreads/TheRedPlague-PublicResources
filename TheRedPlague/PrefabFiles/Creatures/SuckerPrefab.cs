using System.Collections;
using ECCLibrary;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.CreatureBehaviour.Sucker;
using TheRedPlague.Mono.VFX;
using UnityEngine;
using UWE;

namespace TheRedPlague.PrefabFiles.Creatures;

public class SuckerPrefab
{
    public PrefabInfo Info { get; }

    private bool Aurora { get; }

    private LiveMixinData _lmData;

    public SuckerPrefab(PrefabInfo info, bool aurora)
    {
        Info = info;
        Aurora = aurora;
    }
    
    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
        _lmData = CreatureDataUtils.CreateLiveMixinData(100000);
        _lmData.broadcastKillOnDeath = true;
    }

    private IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var go = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("Sucker"));
        go.SetActive(false);
        PrefabUtils.AddBasicComponents(go, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Medium);
        MaterialUtils.ApplySNShaders(go);
        // var infect = go.AddComponent<InfectAnything>();
        // infect.infectionHeightStrength = 0.05f;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.mass = 20;
        var wf = go.AddComponent<WorldForces>();
        wf.useRigidbody = rb;
        wf.underwaterDrag = 1.5f;
        wf.underwaterGravity = 4f;

        var blockTrigger = go.transform.Find("BlockTriggers").gameObject;
        
        if (Aurora)
        {
            var request = PrefabDatabase.GetPrefabAsync("98ac710d-5390-49fd-a850-dbea7bc07aef");
            yield return request;
            if (request.TryGetPrefab(out var controlRoomPrefab))
            {
                var skyApplier = go.EnsureComponent<SkyApplier>();
                skyApplier.customSkyPrefab = controlRoomPrefab.GetComponent<SkyApplier>().customSkyPrefab;
                skyApplier.dynamic = false;
                skyApplier.anchorSky = Skies.Custom;
            }

            var fix = go.AddComponent<AuroraSuckerFixPhysics>();
            fix.rigidbody = rb;
            fix.blockTrigger = blockTrigger;
        }
        else
        {
            blockTrigger.SetActive(false);
        }
        
        var look = go.transform.Find("SuckerV2/Sucker2Armature/Root/Eye").gameObject.AddComponent<GenericEyeLook>();
        look.dotLimit = 0;
        look.useLimits = true;

        go.AddComponent<SuckerDamageable>().animator = go.GetComponentInChildren<Animator>();

        var lm = go.AddComponent<LiveMixin>();
        lm.data = _lmData;

        prefab.Set(go);
    }
}