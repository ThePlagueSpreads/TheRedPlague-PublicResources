using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using Nautilus.Assets;
using Nautilus.Extensions;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class FleshStalker : CreatureAsset
{
    public FleshStalker(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 5, null, null);
        ZombieManager.RegisterPlagueVariantConversion(TechType.Stalker, TechType, 2f);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("MutantStalker_Prefab"),
            BehaviourType.Shark, EcoTargetType.Shark, 1000)
        {
            // match to stalker
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.6f, 3f, 0.3f),
            StayAtLeashData = new StayAtLeashData(0.4f, 3, 18),
            SwimBehaviourData = new SwimBehaviourData(0.9f),
            SwimRandomData = new SwimRandomData(0.2f, 3, new Vector3(30, 2, 30), 4, 0.5f),
            AvoidObstaclesData = new AvoidObstaclesData(0.45f, 3, false, 5f, 6f),
            AttackLastTargetData = new AttackLastTargetData(0.5f, 11, 0.5f, 8f),
            AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>
            {
                new(EcoTargetType.SmallFish, 1f, 5, 1),
                new(EcoTargetType.Shark, 1f, 30, 1)
            },
            BehaviourLODData = new BehaviourLODData(30, 60, 100),
            FleeOnDamageData = null,
            AnimateByVelocityData = new AnimateByVelocityData(7)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Near, 85, 0,
            new BehaviourLODData(30, 60, 100), 650);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var meleeAttack = CreaturePrefabUtils.AddMeleeAttack<MeleeAttack>(prefab, components,
            prefab.transform.Find("BiteTrigger").gameObject, true, 45f, 2.5f);
        meleeAttack.eatHungerDecrement = 0.5f;
        meleeAttack.biteAggressionThreshold = 0.3f;
        var biteSound = prefab.AddComponent<FMOD_StudioEventEmitter>();
        biteSound.path = "MutantStalkerBite";
        biteSound.minInterval = 2;
        biteSound.startEventOnAwake = false;
        meleeAttack.attackSound = biteSound;

        var trailManagerBuilder = new TrailManagerBuilder(components,
            prefab.SearchChild("body 3").transform,
            2f);
        trailManagerBuilder.SetTrailArrayToAllChildren();
        trailManagerBuilder.Apply();

        var idleSoundEmitter = prefab.AddComponent<FMOD_CustomEmitter>();
        idleSoundEmitter.followParent = true;
        idleSoundEmitter.restartOnPlay = false;

        var voice = prefab.AddComponent<CreatureVoice>();
        voice.emitter = idleSoundEmitter;
        voice.closeIdleSound = AudioUtils.GetFmodAsset("MutantStalkerIdle");
        voice.animator = components.Animator;
        voice.animatorTriggerParam = "roar";
        voice.minInterval = 13;
        voice.maxInterval = 20;
        voice.playSoundOnStart = true;

        var dropItem = prefab.AddComponent<DropItemOnDamage>();
        dropItem.percentage = 0.25f;
        dropItem.spawnRadius = 0.5f;
        dropItem.techType = TechType.StalkerTooth;
        
        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 7f, 2, 1,
            new ColorModifier(new Color(1.5f, 1.5f, 1.5f)));
    }
}