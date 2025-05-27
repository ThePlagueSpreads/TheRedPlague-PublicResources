using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Data;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Mono.CreatureBehaviour.Mutants;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Mono.SFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class Mutant : CreatureAsset
{
    private readonly string _prefabName;
    private readonly Settings _settings;
    private readonly bool _pdaEntry;

    private const float NormalVariantVelocity = 3f;
    private const float HeavilyMutatedVelocity = 14f;

    private static readonly FMODAsset NormalMutantIdle = AudioUtils.GetFmodAsset("NormalMutantIdle");
    private static readonly FMODAsset LargeMutantIdle = AudioUtils.GetFmodAsset("LargeMutantIdle");

    // PrefabName expects a prefab from the normal Red Plague Asset Bundle, NOT the creature Asset Bundle
    public Mutant(PrefabInfo prefabInfo, string prefabName, Settings settings, bool pdaEntry) : base(prefabInfo)
    {
        _prefabName = prefabName;
        _settings = settings;
        _pdaEntry = pdaEntry;
    }

    protected override void PostRegister()
    {
        if (_pdaEntry)
        {
            CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.RedPlagueVictimsPath, null, null, 3,
                null, null);
        }
        
        if (_settings.HasFlag(Settings.HeavilyMutated))
        {
            LootDistributionHandler.AddLootDistributionData(PrefabInfo.ClassID,
                new LootDistributionData.BiomeData
                {
                    biome = BiomeType.ActiveLavaZone_Chamber_Open_CreatureOnly,
                    probability = 0.04f,
                    count = 1
                },
                /*
                new LootDistributionData.BiomeData
                {
                    biome = BiomeType.Dunes_OpenShallow_CreatureOnly,
                    probability = 0.1f,
                    count = 1
                },
                new LootDistributionData.BiomeData
                {
                    biome = BiomeType.Dunes_OpenDeep_CreatureOnly,
                    probability = 0.1f,
                    count = 1
                },
                */
                new LootDistributionData.BiomeData
                {
                    biome = BiomeType.ActiveLavaZone_Falls_Open_CreatureOnly,
                    probability = 0.04f,
                    count = 1
                }
            );
            return;
        }

        LootDistributionHandler.AddLootDistributionData(PrefabInfo.ClassID,
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.KooshZone_OpenDeep_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.KooshZone_OpenShallow_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.UnderwaterIslands_OpenShallow_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.UnderwaterIslands_OpenDeep_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.MushroomForest_Grass,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.CragField_OpenShallow_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.CragField_OpenDeep_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.GrandReef_OpenShallow_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.GrandReef_OpenDeep_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Dunes_OpenShallow_CreatureOnly,
                probability = 0.03f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.Dunes_OpenDeep_CreatureOnly,
                probability = 0.03f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.LostRiverCorridor_Open_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.LostRiverJunction_Open_CreatureOnly,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.BonesField_Open_Creature,
                probability = 0.02f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.InactiveLavaZone_Corridor_Open_CreatureOnly,
                probability = 0.03f,
                count = 1
            },
            new LootDistributionData.BiomeData
            {
                biome = BiomeType.InactiveLavaZone_Chamber_Open_CreatureOnly,
                probability = 0.03f,
                count = 1
            }
        );
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.AssetBundle.LoadAsset<GameObject>(_prefabName),
            BehaviourType.Shark,
            EcoTargetType.Shark, 5000f);
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Medium, 500, -0.5f);
        CreatureTemplateUtils.SetCreatureMotionEssentials(template,
            new SwimRandomData(0.3f, IsHeavilyMutated() ? HeavilyMutatedVelocity : NormalVariantVelocity,
                new Vector3(20f, 6f, 20f), 3f),
            new StayAtLeashData(0.4f, IsHeavilyMutated() ? HeavilyMutatedVelocity : NormalVariantVelocity, 50f));
        template.LocomotionData = new LocomotionData(5f, IsHeavilyMutated() ? 3 : 0.6f);
        template.AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>()
            { new(EcoTargetType.Shark, IsHeavilyMutated() ? 3 : 1, 40, 2) };
        template.AttackLastTargetData =
            new AttackLastTargetData(0.5f, IsHeavilyMutated() ? 20f : NormalVariantVelocity * 2f,
                0.5f, IsHeavilyMutated() ? 30 : 7, IsHeavilyMutated() ? 0.1f : 10);
        template.EyeFOV = IsHeavilyMutated() ? -1f : -0.5f;
        return template;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 4, 2, 1,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Opaque));
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<InfectOnStart>();
        if (IsHeavilyMutated())
        {
            prefab.AddComponent<DisableRigidbodyWhileOnScreen>();
        }

        var attackTrigger = prefab.transform.Find("AttackTrigger").gameObject.AddComponent<MutantAttackTrigger>();
        attackTrigger.prefabFileName = _prefabName;
        attackTrigger.settings = _settings;
        attackTrigger.damage = IsHeavilyMutated() ? 21 : 14;
        attackTrigger.instantKillChance = IsHeavilyMutated() ? 0.33f : 0.1f;

        var emitter = prefab.AddComponent<FMOD_CustomEmitter>();
        emitter.SetAsset(IsHeavilyMutated() ? LargeMutantIdle : NormalMutantIdle);
        emitter.playOnAwake = false;
        emitter.followParent = true;
        var sounds = prefab.AddComponent<PlayRandomSounds>();
        sounds.emitter = emitter;
        sounds.minDelay = 13;
        // lee 23!!
        sounds.maxDelay = 23;
        yield break;
    }

    private bool IsHeavilyMutated() => _settings.HasFlag(Settings.HeavilyMutated);

    [System.Flags]
    public enum Settings
    {
        None = 0,
        Normal = 1,
        HeavilyMutated = 2,
        Large = 4
    }
}