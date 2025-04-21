using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.CreatureBehaviour.Mimics;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class MimicGasopod : CreatureAsset
{
    public MimicGasopod(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 3, null, null);
        ZombieManager.RegisterPlagueVariantConversion(TechType.Gasopod, TechType, 3f);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("MimicGasopodPrefab"),
            BehaviourType.Shark, EcoTargetType.Shark, 500)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(5, 0.2f),
            StayAtLeashData = new StayAtLeashData(0.4f, 3, 15f),
            SwimRandomData = new SwimRandomData(0.2f, 3, new Vector3(30, 5, 30), 5, 1),
            AvoidObstaclesData = new AvoidObstaclesData(0.7f, 3, false, 5f, 6f),
            AttackLastTargetData = new AttackLastTargetData(0.6f, 3f, 0.5f, 6f),
            AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>
            {
                new(EcoTargetType.Shark, 0.3f, 10, 1)
            },
            AnimateByVelocityData = new AnimateByVelocityData(2f)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Medium, 270);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var attackTrigger = prefab.transform.Find("AttackTrigger").gameObject.AddComponent<MimicGasopodAttackTrigger>();
        attackTrigger.creature = components.Creature;

        var damageInRange = prefab.transform.Find("AttackRangeCenter").gameObject.AddComponent<DamageInRange>();
        damageInRange.normalDamage = 18;
        damageInRange.plagueDamage = 10;
        damageInRange.damageRadius = 1.3f;
        damageInRange.dealerRoot = prefab;
        attackTrigger.rangeDamage = damageInRange;
        
        var emitter = prefab.AddComponent<FMOD_CustomEmitter>();
        emitter.followParent = true;
        emitter.restartOnPlay = true;
        attackTrigger.emitter = emitter;
        attackTrigger.damageSound = AudioUtils.GetFmodAsset("LargeMutantBite");
        
        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 5.7f, 1.5f);
    }
}