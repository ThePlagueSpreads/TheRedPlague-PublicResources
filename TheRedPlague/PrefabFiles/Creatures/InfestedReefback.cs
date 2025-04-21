using System.Collections;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Mono.CreatureBehaviour.Mimics;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Mono.SFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class InfestedReefback : CreatureAsset
{
    public InfestedReefback(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 10, null, null);
        ZombieManager.RegisterPlagueVariantConversion(TechType.Reefback, TechType, 4f);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("InfestedReefbackPrefab"),
            BehaviourType.Whale, EcoTargetType.Whale, 10000)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.02f, 3f, 0.5f, true),
            StayAtLeashData = new StayAtLeashData(0.4f, 1f, 100f),
            SwimBehaviourData = new SwimBehaviourData(0.3f),
            SwimRandomData = new SwimRandomData(0.2f, 1, new Vector3(100, 2, 100), 5, 1f),
            AvoidObstaclesData = new AvoidObstaclesData(0.45f, 1, true, 50f, 50f),
            ScannerRoomScannable = true,
            FleeOnDamageData = null,
            AnimateByVelocityData = new AnimateByVelocityData(0.5f)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.VeryFar, 10400, 0, new BehaviourLODData(50, 150, 500), 1000);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;
        components.WorldForces.underwaterDrag = 0.1f;
        components.Rigidbody.inertiaTensor = new Vector3(1132427, 1510034, 621844);

        var trailsParent = prefab.transform.Find("InfestedReefback/InfestedReefbackRig");

        Transform[] tendrils =
        {
            trailsParent.Find("BackTendrilsL"),
            trailsParent.Find("BackTendrilsR"),
            trailsParent.Find("FrontTendrilsL"),
            trailsParent.Find("FrontTendrilsR")
        };

        foreach (var tendril in tendrils)
        {
            var trailManagerBuilder = new TrailManagerBuilder(components, tendril, 2f);
            trailManagerBuilder.SetTrailArrayToAllChildren();
            trailManagerBuilder.AllowDisableOnScreen = false;
            trailManagerBuilder.Apply();
        }

        Transform[] tails =
        {
            trailsParent.Find("TailL"),
            trailsParent.Find("TailR")
        };

        foreach (var tail in tails)
        {
            var trailManagerBuilder = new TrailManagerBuilder(components, tail, 0.5f);
            trailManagerBuilder.SetTrailArrayToAllChildren();
            trailManagerBuilder.Apply();
        }

        var emitter = prefab.AddComponent<FMOD_CustomEmitter>();
        emitter.asset = AudioUtils.GetFmodAsset("InfestedReefbackCall");
        emitter.followParent = true;
        emitter.playOnAwake = false;
        var randomSounds = prefab.AddComponent<PlayRandomSounds>();
        randomSounds.minDelay = 15;
        randomSounds.maxDelay = 35;
        randomSounds.emitter = emitter;

        var resourceSpawns = prefab.AddComponent<BlisterbackResourceSpawns>();
        resourceSpawns.spawnPointsParent = prefab.transform.Find("SpawnSlots");
        
        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 6f, 1f, 1f,
            new FresnelModifier(0.6f));
    }
}