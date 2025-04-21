using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using rail;
using TheRedPlague.Data;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class PhantomLeviathan : CreatureAsset
{
    protected virtual string PrefabFileName => "PhantomLeviathan.prefab";

    public PhantomLeviathan(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 7, null, null);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(
            () => Plugin.CreaturesBundle.LoadAsset<GameObject>(PrefabFileName),
            BehaviourType.Leviathan, EcoTargetType.Leviathan, 10000)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.6f, 3, 0.4f),
            SwimBehaviourData = new SwimBehaviourData(0.6f),
            StayAtLeashData = new StayAtLeashData(0.8f, 10, 60),
            SwimRandomData = new SwimRandomData(0.2f, 20, new Vector3(25, 4, 25), 4, 1, true),
            AvoidTerrainData = new AvoidTerrainData(0.9f, 20, 30, 30, 0.5f, 10),
            AggressiveWhenSeeTargetList = new List<AggressiveWhenSeeTargetData>
            {
                new(EcoTargetType.Shark, 2, 150, 3, false)
            },
            AnimateByVelocityData = new AnimateByVelocityData(4),
            AttackCyclopsData = new AttackCyclopsData(0.91f, 15, 140, 0.4f, 4, 0.01f, 0.6f),
            SurfaceType = VFXSurfaceTypes.vegetation,
            RespawnData = new RespawnData(false),
            FleeOnDamageData = null,
            AcidImmune = true,
            ScannerRoomScannable = true
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.VeryFar, 4000, -0.85f,
            new BehaviourLODData(40, 150, 700), 800);
        template.SetCreatureComponentType<PhantomLeviathanCreature>();
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        var phantomCreature = components.Creature as PhantomLeviathanCreature;
        
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;
        
        var aggressiveOnDamage = prefab.AddComponent<AggressiveOnDamage>();
        aggressiveOnDamage.creature = components.Creature;
        aggressiveOnDamage.minDamageThreshold = 10;
        aggressiveOnDamage.aggressionDamageScalar = 0.02f;
        aggressiveOnDamage.friendlinessDecrement = 1;
        aggressiveOnDamage.tirednessDecrement = 1;
        aggressiveOnDamage.happinessDecrement = 1;

        var spineTransform = prefab.transform.SearchChild("spine");
        var trailManagerBuilder = new TrailManagerBuilder(components, spineTransform, 4f)
        {
            Trails = new []
            {
                spineTransform.Find("spine.001"),
                spineTransform.Find("spine.001/tail"),
                spineTransform.Find("spine.001/tail/tail.001"),
                spineTransform.Find("spine.001/tail/tail.001/tail.002"),
            }
        };
        trailManagerBuilder.AllowDisableOnScreen = false;
        trailManagerBuilder.Apply();

        prefab.AddComponent<VFXSchoolFishRepulsor>();
        
        // Attacks

        var poisonDamageDealer = prefab.transform.Find("PoisonAttackCenter").gameObject.AddComponent<DamageInRange>();
        poisonDamageDealer.damageRadius = 15;
        poisonDamageDealer.canDamagePlayer = true;
        poisonDamageDealer.canDamageVehicles = false;
        poisonDamageDealer.canDamageInfectedCreatures = false;
        poisonDamageDealer.normalDamage = 20;
        poisonDamageDealer.plagueDamage = 50;
        poisonDamageDealer.canDamageCreatures = true;
        poisonDamageDealer.dealerRoot = prefab;

        var poisonAttack = prefab.AddComponent<PhantomPoisonAttack>();
        poisonAttack.phantom = phantomCreature;
        poisonAttack.damage = poisonDamageDealer;
        
        var poisonTrigger = prefab.transform.Find("PoisonAttackTrigger").gameObject
            .AddComponent<PhantomPoisonAttackTrigger>();
        poisonTrigger.poisonAttack = poisonAttack;
        
        var meleeAttack = CreaturePrefabUtils.AddMeleeAttack<PhantomMeleeAttack>(prefab, components,
            prefab.transform.Find("MeleeAttackTrigger").gameObject, true, 85, 4f);
        meleeAttack.canBiteCyclops = true;
        meleeAttack.eatHungerDecrement = 0.2f;
        meleeAttack.biteAggressionThreshold = 0.2f;
        meleeAttack.phantom = phantomCreature;
        var biteSound = prefab.AddComponent<FMOD_StudioEventEmitter>();
        biteSound.path = "LargeMutantBite";
        biteSound.minInterval = 3;
        biteSound.startEventOnAwake = false;
        meleeAttack.attackSound = biteSound;

        phantomCreature.meleeAttack = meleeAttack;
        phantomCreature.poisonAttack = poisonAttack;
        phantomCreature.swimBehaviour = components.SwimBehaviour;

        #region Mist
        
        var gasopodTask = CraftData.GetPrefabForTechTypeAsync(TechType.Gasopod);
        yield return gasopodTask;
        var gasopodGas = gasopodTask.GetResult().GetComponent<GasoPod>().gasFXprefab;
        var mistPrefab = Object.Instantiate(gasopodGas, prefab.transform);
        mistPrefab.SetActive(false);
        mistPrefab.AddComponent<PhantomPoisonInstance>();
        foreach (var renderer in mistPrefab.GetComponentsInChildren<Renderer>(true))
        {
            renderer.material.color = new Color(0.5f, 0.1f, 2, 0.1f);
            renderer.material.SetColor("_ColorStrengthAtNight", Color.gray);
        }

        foreach (var ps in mistPrefab.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.startSizeMultiplier *= 15f;
            main.startLifetimeMultiplier *= 4f;
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new(0, 0.3f), new(1, 1)));
        }

        foreach (var trail in mistPrefab.GetComponentsInChildren<Trail_v2>(true))
        {
            trail.gameObject.SetActive(false);
        }

        var destroyAfterSeconds = mistPrefab.GetComponent<VFXDestroyAfterSeconds>();
        destroyAfterSeconds.lifeTime = 10f;

        mistPrefab.transform.Find("xGasopodSmoke/xSmkMesh").gameObject.SetActive(false);
        
        #endregion
        
        // var poisonEmitters = new List<PhantomPoisonEmitter>();
        foreach (Transform child in prefab.transform.Find("PoisonEmitters"))
        {
            var emitter = child.gameObject.AddComponent<PhantomPoisonEmitter>();
            emitter.rb = components.Rigidbody;
            emitter.mistPrefab = mistPrefab;
            emitter.spawnDelay = 0.4f;
            emitter.spawnRadius = 4f;
        }

        var voice = prefab.AddComponent<CreatureVoice>();
        voice.closeIdleSound = AudioUtils.GetFmodAsset("PhantomLeviathanRoarClose");
        voice.farIdleSound = AudioUtils.GetFmodAsset("PhantomLeviathanRoarFar");
        var voiceEmitter = prefab.AddComponent<FMOD_CustomEmitter>();
        voice.emitter = voiceEmitter;
        voice.farThreshold = 50f;
        voice.playSoundOnStart = false;
        voice.minInterval = 21;
        voice.maxInterval = 37;
        
        var attackLastTarget = prefab.AddComponent<PhantomAttackLastTarget>();
        attackLastTarget.evaluatePriority = 0.9f;
        attackLastTarget.swimVelocity = 25;
        attackLastTarget.aggressionThreshold = 0.6f;
        attackLastTarget.minAttackDuration = 8;
        attackLastTarget.maxAttackDuration = 16;
        attackLastTarget.pauseInterval = 16;
        attackLastTarget.rememberTargetTime = 5;
        attackLastTarget.resetAggressionOnTime = true;
        attackLastTarget.lastTarget = components.LastTarget;
        attackLastTarget.voice = voice;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 7f, 5f, 2f,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent),
            new PhantomMaterialModifier());
    }

    private class PhantomMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (materialType == MaterialUtils.MaterialType.Transparent && renderer is not ParticleSystemRenderer)
            {
                material.SetFloat("_SpecInt", 45);
                material.SetFloat("_Shininess", 8f);
                material.SetFloat("_Fresnel", 0.76f);
                material.SetFloat(ShaderPropertyID._GlowStrength, 0.2f);
                material.SetFloat(ShaderPropertyID._GlowStrengthNight, 0.5f);
            }
        }
        
        public override bool BlockShaderConversion(Material material, Renderer renderer,
            MaterialUtils.MaterialType materialType)
        {
            return renderer is ParticleSystemRenderer;
        }
    }
}