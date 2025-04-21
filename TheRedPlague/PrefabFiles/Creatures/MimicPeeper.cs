using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class MimicPeeper : CreatureAsset
{
    public MimicPeeper(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 3, null, null);
        ZombieManager.RegisterPlagueVariantConversion(TechType.Peeper, TechType);
        LootDistributionHandler.AddLootDistributionData(PrefabInfo.ClassID,
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Precursor_SurfaceVent_Generic,
                count = 1,
                probability = 0.2f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Kelp_DenseVine,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Kelp_Wall,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.GrassyPlateaus_Grass,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.MushroomForest_MushroomTreeTrunk,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.MushroomForest_GiantTreeExterior,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.MushroomForest_GiantTreeExteriorBase,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.MushroomForest_Grass,
                count = 1,
                probability = 0.01f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.KooshZone_RockWall,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.UnderwaterIslands_ValleyWall,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.SparseReef_Coral,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Dunes_SandDune,
                count = 1,
                probability = 0.1f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Dunes_Grass,
                count = 1,
                probability = 0.1f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.GrandReef_PurpleCoral,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Mountains_Grass,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Mountains_Rock,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.SeaTreaderPath_Sand,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Mesas_Top,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.KooshZone_OpenShallow_CreatureOnly,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.KooshZone_OpenDeep_CreatureOnly,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Mountains_OpenShallow_CreatureOnly,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Mountains_OpenDeep_CreatureOnly,
                count = 1,
                probability = 0.02f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Dunes_OpenDeep_CreatureOnly,
                count = 1,
                probability = 0.1f
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Dunes_OpenShallow_CreatureOnly,
                count = 1,
                probability = 0.1f
            });
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("MimicPeeper_Prefab"),
            BehaviourType.Shark, EcoTargetType.Shark, 500)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.6f, 3f, 0.3f),
            StayAtLeashData = new StayAtLeashData(0.4f, 3, 15f),
            SwimRandomData = new SwimRandomData(0.2f, 4, new Vector3(10, 2, 10)),
            AvoidObstaclesData = new AvoidObstaclesData(0.5f, 4, false, 5f, 6f),
            AttackLastTargetData = new AttackLastTargetData(0.6f, 5f, 0.5f, 8f),
            AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>
            {
                new(EcoTargetType.Shark, 1f, 14, 1)
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
            prefab.transform.Find("BiteTrigger").gameObject, true, 8f, 10f, false);
        var biteSound = prefab.AddComponent<FMOD_StudioEventEmitter>();
        biteSound.path = "SmallZombieBite";
        biteSound.minInterval = 2;
        biteSound.startEventOnAwake = false;
        meleeAttack.attackSound = biteSound;

        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 7f, 4f, 0.3f);
    }
}