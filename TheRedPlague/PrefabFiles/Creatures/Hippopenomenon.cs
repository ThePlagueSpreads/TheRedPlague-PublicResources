using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.CreatureBehaviour.HoverPet;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class Hippopenomenon : CreatureAsset
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("Hippopenomenon");
    
    public Hippopenomenon() : base(Info)
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 5, null, null);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("HoverPetPrefab"),
            BehaviourType.MediumFish, EcoTargetType.None, 10000)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.6f, 3f, 0.3f),
            SwimRandomData = new SwimRandomData(0.2f, 4, new Vector3(10, 2, 10)),
            AvoidObstaclesData = new AvoidObstaclesData(0.8f, 4, false, 10f, 7f, 2f, 1f, 3),
            AnimateByVelocityData = new AnimateByVelocityData(2f)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Global, 80f);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var followPlayer = prefab.AddComponent<CreatureFollowPlayer>();
        followPlayer.creature = components.Creature;
        followPlayer.distanceToPlayer = 14;

        var warpWhenFar = prefab.AddComponent<WarpToPlayerWhenFar>();
        warpWhenFar.warpDistance = 60f;

        prefab.AddComponent<HoverPetBehavior>();
        prefab.AddComponent<HoverPetSwimToMazeBase>();
        
        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 6f);
    }
}