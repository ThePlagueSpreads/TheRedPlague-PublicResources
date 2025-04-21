using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Utility;
using Story;
using TheRedPlague.Managers.Amalgamation;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Managers;

public static class ZombieManager
{
    private static FMODAsset _biteSoundSmall = AudioUtils.GetFmodAsset("SmallZombieBite");
    private static FMODAsset _biteSoundLarge = AudioUtils.GetFmodAsset("ZombieBite");

    private static readonly Dictionary<TechType, TechType> CreaturePlagueVariantConversions = new();

    private static readonly Dictionary<TechType, float> CreaturePlagueVariantConversionRates = new();

    private const float PlagueVariantConversionChance = 0.12f;
    private const float PlagueVariantConversionChanceForAct1Only = 0.04f;
    
    private const float SmallFishMaxHealth = 80;
    private const float MediumFishMaxHealth = 200;

    public static readonly string[] HeavilyInfectedBiomes =
        { "dunes", "fleshcave_upper", "fleshcave_chamber", "infectedzone" };

    public static bool IsBiomeHeavilyInfected(string biome)
    {
        foreach (var b in HeavilyInfectedBiomes)
        {
            if (b == biome) return true;
        }

        return false;
    }

    public static void RegisterPlagueVariantConversion(TechType original, TechType plagued, float commonalityMultiplier = 1f)
    {
        CreaturePlagueVariantConversions.Add(original, plagued);
        CreaturePlagueVariantConversionRates[original] = commonalityMultiplier;
    }

    public static bool IsZombie(GameObject creature)
    {
        var redPlagueHost = creature.GetComponent<RedPlagueHost>();
        if (redPlagueHost == null) return false;
        return redPlagueHost.IsZombified;
    }

    public static bool TryConversion(GameObject creature)
    {
        var techType = CraftData.GetTechType(creature);

        if (CreaturePlagueVariantConversions.TryGetValue(techType, out var conversion))
        {
            var randomValue = Random.value / GetConversionCommonalityMultiplier(techType);
            if (randomValue <= PlagueVariantConversionChance)
            {
                // If random value is greater than 0.04, and we are still in act 1, skip
                if (randomValue > PlagueVariantConversionChanceForAct1Only &&
                    !StoryUtils.IsAct1Complete())
                {
                    return false;
                }

                UWE.CoroutineHost.StartCoroutine(ConvertToPlagueVariant(creature, conversion));
                return true;
            }
        }

        return false;
    }

    private static float GetConversionCommonalityMultiplier(TechType techType)
    {
        return CreaturePlagueVariantConversionRates.TryGetValue(techType, out var commonality) ? commonality : 1f;
    }

    private static IEnumerator ConvertToPlagueVariant(GameObject creatureToReplace, TechType variant)
    {
        var variantPrefabTask = CraftData.GetPrefabForTechTypeAsync(variant);
        yield return variantPrefabTask;
        if (creatureToReplace == null) yield break;
        Object.Instantiate(variantPrefabTask.GetResult(), creatureToReplace.transform.position,
            creatureToReplace.transform.rotation);
        Object.Destroy(creatureToReplace);
    }

    // Only called upon initial transformation. Theoretically, this is only called once for each creature, ever.
    public static void Zombify(GameObject creature)
    {
        // Initial checks
        var host = creature.EnsureComponent<RedPlagueHost>();

        if (host.IsZombified) return;

        if (TryConversion(creature))
        {
            return;
        }

        if (host.mode == RedPlagueHost.Mode.PlagueCreation) return;

        // Actually zombify it:
        AddZombieBehaviour(host);
        var infectedMixin = creature.GetComponent<InfectedMixin>();
        if (infectedMixin)
        {
            infectedMixin.SetInfectedAmount(4);
        }
    }

    public static void AddZombieBehaviour(RedPlagueHost host)
    {
        host.MarkAsZombified();

        var creatureComponent = host.GetComponent<Creature>();

        if (creatureComponent == null)
        {
            Debug.LogWarning($"No creature component on infected object {host.name}!");
            return;
        }

        bool aggressiveToSharks = false;
        bool aggressiveToSmallFish = false;
        foreach (var aggressiveWhenSeeTarget in host.GetComponents<AggressiveWhenSeeTarget>())
        {
            if (aggressiveWhenSeeTarget.targetType == EcoTargetType.Shark)
                aggressiveToSharks = true;
            if (aggressiveWhenSeeTarget.targetType == EcoTargetType.SmallFish)
                aggressiveToSmallFish = true;
        }

        if (!aggressiveToSharks)
        {
            MakeCreatureAggressiveToEcoTargetType(creatureComponent, EcoTargetType.Shark);
        }

        if (!aggressiveToSmallFish)
        {
            MakeCreatureAggressiveToEcoTargetType(creatureComponent, EcoTargetType.SmallFish);
        }

        if (host.GetComponent<AttackLastTarget>() == null)
        {
            AddAttackLastTarget(creatureComponent);
        }

        if (host.GetComponent<MeleeAttack>() == null)
        {
            AddMeleeAttack(creatureComponent);
        }

        var techTag = host.GetComponent<TechTag>();
        if (techTag && techTag.type == TechType.HoopfishSchool)
        {
            // |   ||
            // ||  | _
            host.gameObject.EnsureComponent<ShoalDamageInRange>();
        }

        var liveMixin = host.GetComponent<LiveMixin>();

        if (liveMixin.maxHealth >= MediumFishMaxHealth)
        {
            host.gameObject.EnsureComponent<DropAmalgamatedBoneOnDeath>();
        }

        creatureComponent.Scared = new CreatureTrait(0, 100000f);
        creatureComponent.Aggression = new CreatureTrait(1, 0.12f);

        creatureComponent.ScanCreatureActions();

        var pickupable = host.GetComponent<Pickupable>();
        if (pickupable)
        {
            pickupable.isPickupable = false;
            host.gameObject.EnsureComponent<AllowPickUpOnDeath>().pickupable = pickupable;
            host.gameObject.EnsureComponent<UsableItem>().SetUniqueUseAction(OnConsumeInfectedFish);
        }

        AmalgamationManager.AmalgamateCreature(host);
    }

    private static void OnConsumeInfectedFish()
    {
        PlagueDamageStat.main.TakeInfectionDamage(Random.Range(18, 25), true);
    }

    private static void AddMeleeAttack(Creature creature)
    {
        if (creature.GetAnimator() == null)
        {
            // Plugin.Logger.LogWarning($"Creature '{creature.gameObject.name}' has no Animator! Skipping MeleeAttack instantiation.");
            return;
        }

        var meleeAttack = creature.gameObject.AddComponent<MeleeAttack>();
        meleeAttack.biteAggressionThreshold = 0.2f;
        meleeAttack.biteInterval = 8;
        meleeAttack.biteDamage = creature.liveMixin.maxHealth >= MediumFishMaxHealth ? 14 : 4;
        meleeAttack.biteAggressionDecrement = 0.5f;
        meleeAttack.lastTarget = creature.GetComponent<LastTarget>();
        meleeAttack.creature = creature;
        meleeAttack.liveMixin = creature.liveMixin;
        meleeAttack.animator = creature.GetAnimator();
        meleeAttack.canBiteVehicle = creature.liveMixin.maxHealth >= MediumFishMaxHealth ? true : false;
        meleeAttack.canBiteCyclops = false;
        var biteEmitter = creature.gameObject.AddComponent<FMOD_StudioEventEmitter>();
        var biteSoundToUse = creature.liveMixin.maxHealth >= MediumFishMaxHealth ? _biteSoundLarge : _biteSoundSmall;
        biteEmitter.asset = biteSoundToUse;
        biteEmitter.path = biteSoundToUse.path;
        meleeAttack.attackSound = biteEmitter;

        var triggerObj = new GameObject("AttackTrigger");
        meleeAttack.mouth = triggerObj;
        var collider = triggerObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        triggerObj.AddComponent<VFXSurface>().surfaceType = VFXSurfaceTypes.organic;
        var onTouch = triggerObj.AddComponent<OnTouch>();
        onTouch.onTouch = new OnTouch.OnTouchEvent();
        onTouch.onTouch.AddListener(meleeAttack.OnTouch);

        triggerObj.transform.parent = creature.transform;
        triggerObj.transform.localPosition = Vector3.forward * 0.5f;
    }

    private static void MakeCreatureAggressiveToEcoTargetType(Creature creature, EcoTargetType type)
    {
        var aggressiveComponent = creature.gameObject.AddComponent<ZombieAggressiveWhenSeeTarget>();
        // What the fuck is this and why does every creature do this
        aggressiveComponent.maxRangeMultiplier =
            new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.5f), new Keyframe(1, 1));
        aggressiveComponent.distanceAggressionMultiplier = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        aggressiveComponent.creature = creature;
        aggressiveComponent.targetType = type;
        var maxHealth = creature.liveMixin.maxHealth;
        aggressiveComponent.maxRangeScalar = maxHealth > MediumFishMaxHealth ? 50 : 18;
        aggressiveComponent.ignoreVehicles = maxHealth < MediumFishMaxHealth;
        aggressiveComponent.maxSearchRings = 2;
        aggressiveComponent.aggressionPerSecond = 0.35f;
        aggressiveComponent.ignoreSameKind = false;
        aggressiveComponent.hungerThreshold = 0;

        var lastTarget = creature.gameObject.EnsureComponent<LastTarget>();
        aggressiveComponent.lastTarget = lastTarget;
    }

    private static void AddAttackLastTarget(Creature creature)
    {
        if (creature.GetAnimator() == null)
        {
            // Plugin.Logger.LogWarning($"Creature '{creature.gameObject.name}' has no Animator! Skipping AttackLastTarget instantiation.");
            return;
        }

        // The creature requires these components for AttackLastTarget
        if (creature.GetComponent<SwimBehaviour>() == null || creature.GetComponent<Locomotion>() == null)
        {
            return;
        }
        
        // Get max health for reference purposes
        var maxHealth = creature.liveMixin.maxHealth;

        var attackDurationMultiplier = maxHealth > SmallFishMaxHealth ? 1f : 0.6f;
        var pauseDurationMultiplier = maxHealth > SmallFishMaxHealth ? 1f : 2f;

        var attackLastTarget = creature.gameObject.AddComponent<AttackLastTarget>();
        var swimRandom = creature.GetComponent<SwimRandom>();
        attackLastTarget.swimVelocity = swimRandom != null ? swimRandom.swimVelocity * 2f : 10f;
        attackLastTarget.aggressionThreshold = 0.8f;
        attackLastTarget.swimInterval = 0.5f;
        attackLastTarget.minAttackDuration = 4.5f * attackDurationMultiplier;
        attackLastTarget.maxAttackDuration = 10f * attackDurationMultiplier;
        attackLastTarget.pauseInterval = 18f * pauseDurationMultiplier;
        attackLastTarget.rememberTargetTime = 10f;
        attackLastTarget.evaluatePriority = 1.1f;
        attackLastTarget.lastTarget = creature.gameObject.GetComponent<LastTarget>();
    }

    public static void InfectSeaEmperor(GameObject seaEmperor)
    {
        var renderers = seaEmperor.GetComponentsInChildren<Renderer>()
            .Where(r => !(r is ParticleSystemRenderer) && !(r is TrailRenderer));
        foreach (var r in renderers)
        {
            var materials = r.materials;
            foreach (var m in materials)
            {
                m.shader = MaterialUtils.Shaders.MarmosetUBER;
                m.color = new Color(3, 3, 3);
                m.EnableKeyword(InfectedMixin.uwe_infection);
                m.SetFloat(ShaderPropertyID._InfectionAmount, 1);
                m.SetFloat("_InfectionHeightStrength", -3.9f);
                m.SetVector("_InfectionScale", new Vector4(2, 2, 2, 0));
                m.SetVector("_InfectionOffset", new Vector4(0.285f, 0, 0.142f, 0));
                m.SetColor("_GlowColor", new Color(3, 0, 0));
                m.SetTexture(ShaderPropertyID._InfectionAlbedomap, Plugin.ZombieInfectionTexture);
            }
        }
    }

    public static float GetInfectionStrengthAtPosition(Vector3 position)
    {
        var depthWeight = Mathf.InverseLerp(0, -1500, position.y);
        var distanceWeight = Mathf.Sqrt(position.x * position.x + position.z * position.z) / 2400;
        return Mathf.Clamp01(depthWeight + distanceWeight);
    }

    public static float GetBiteConversionChance(LiveMixin victim)
    {
        var multiplier = victim.maxHealth switch
        {
            > 2500 => 0.25f,
            > 1000 => 0.33f,
            > 500 => 0.50f,
            > 250 => 0.75f,
            _ => 1.00f
        };
        if (StoryUtils.IsAct1Complete())
        {
            return 0.50f * multiplier;
        }
        return 0.30f * multiplier;
    }
}