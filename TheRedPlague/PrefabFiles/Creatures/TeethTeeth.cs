using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class TeethTeeth : CreatureAsset
{
    public TeethTeeth(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 3, null, null);
        ZombieManager.RegisterPlagueVariantConversion(TechType.Eyeye, TechType);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("TeethteethPrefab"),
            BehaviourType.Shark, EcoTargetType.Shark, 200)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(),
            StayAtLeashData = new StayAtLeashData(0.4f, 4, 27),
            SwimRandomData = new SwimRandomData(0.2f, 4, new Vector3(10, 2, 10)),
            AvoidObstaclesData = new AvoidObstaclesData(0.7f, 4, true, 5, 4f),
            AttackLastTargetData = new AttackLastTargetData(0.6f, 5, 0.5f, 6f),
            AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>
            {
                new(EcoTargetType.Shark, 0.3f, 10, 1)
            },
            AnimateByVelocityData = new AnimateByVelocityData(4)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Near, 5f);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var meleeAttack = CreaturePrefabUtils.AddMeleeAttack<MeleeAttack>(prefab, components,
            prefab.transform.Find("BiteTrigger").gameObject, true, 6f, 10f, false);
        var biteSound = prefab.AddComponent<FMOD_StudioEventEmitter>();
        biteSound.path = "SmallZombieBite";
        biteSound.minInterval = 2;
        biteSound.startEventOnAwake = false;
        meleeAttack.attackSound = biteSound;

        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 6, 3f, 0.5f,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent));
    }
}