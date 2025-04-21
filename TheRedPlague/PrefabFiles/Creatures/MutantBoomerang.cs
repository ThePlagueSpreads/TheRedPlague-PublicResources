using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class MutantBoomerang : CreatureAsset
{
    public MutantBoomerang(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath, null,
            null, 3, null, null);
        ZombieManager.RegisterPlagueVariantConversion(TechType.Boomerang, TechType);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("MutantBoomerangPrefab"),
            BehaviourType.Shark, EcoTargetType.Shark, 500)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.6f, 3f, 0.3f),
            StayAtLeashData = new StayAtLeashData(0.4f, 3, 15f),
            SwimRandomData = new SwimRandomData(0.2f, 4, new Vector3(10, 2, 10)),
            AvoidObstaclesData = new AvoidObstaclesData(0.5f, 4, false, 5f, 6f),
            AttackLastTargetData = new AttackLastTargetData(0.6f, 7f, 0.3f, 8f, 5f),
            AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>
            {
                new(EcoTargetType.Shark, 1.5f, 17f, 1)
            },
            AnimateByVelocityData = new AnimateByVelocityData(5)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Near, 5f);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var meleeAttack = CreaturePrefabUtils.AddMeleeAttack<MeleeAttack>(prefab, components,
            prefab.transform.Find("BiteTrigger").gameObject, true, 13f, 10f, false);
        var biteSound = prefab.AddComponent<FMOD_StudioEventEmitter>();
        biteSound.path = "SmallZombieBite";
        biteSound.minInterval = 2;
        biteSound.startEventOnAwake = false;
        meleeAttack.attackSound = biteSound;

        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 6f, 2f, 1f);
    }
}