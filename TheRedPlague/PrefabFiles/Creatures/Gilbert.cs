using System.Collections;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Data;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class Gilbert : CreatureAsset
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("Gilbert")
        .WithIcon(Plugin.CreaturesBundle.LoadAsset<Sprite>("GilbertIcon"));

    public Gilbert() : base(Info)
    {
        CustomPrefab.SetPdaGroupCategory(CustomTechCategories.PlagueBiotechGroup,
            CustomTechCategories.PlagueBiotechCategory);
        CustomPrefab.SetRecipe(new RecipeData(new CraftData.Ingredient(ConsciousNeuralMatter.Info.TechType),
                new CraftData.Ingredient(PlagueIngot.Info.TechType, 2),
                new CraftData.Ingredient(BloodQuartz.Info.TechType)))
            .WithFabricatorType(PlagueAltar.CraftTreeType)
            .WithStepsToFabricatorTab(PlagueAltar.PetsTab)
            .WithCraftingTime(15);
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 3, null, null);
        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("Gilbert.prefab"),
            BehaviourType.MediumFish, EcoTargetType.MediumFish, 2000)
        {
            CanBeInfected = false,
            StayAtLeashData = new StayAtLeashData(0.3f, 4f, 6f),
            SwimRandomData = new SwimRandomData(0.2f, 5, new Vector3(10, 2, 10), 4),
            AvoidObstaclesData = new AvoidObstaclesData(0.41f, 3, false, 5f, 2f),
            AttackLastTargetData = new AttackLastTargetData(1.1f, 5f, 0.5f, 10f, 5f, 4f, 6f),
            FleeOnDamageData = new FleeOnDamageData(1.2f, 5f, 1),
            AnimateByVelocityData = new AnimateByVelocityData(3f),
            PickupableFishData = new PickupableFishData(TechType.Peeper, "WorldModel", "FPModel")
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Global, 4, 0,
            new BehaviourLODData(20, 50, 500), -1);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var meleeAttack = CreaturePrefabUtils.AddMeleeAttack<MeleeAttack>(prefab, components,
            prefab.transform.Find("WorldModel/BiteTrigger").gameObject, true, 33f, 1f);
        meleeAttack.eatHungerDecrement = 0.2f;
        meleeAttack.biteAggressionThreshold = 0.2f;
        meleeAttack.ignoreSameKind = true;
        meleeAttack.canBitePlayer = false;

        var followPlayer = prefab.AddComponent<CreatureFollowPlayer>();
        followPlayer.creature = components.Creature;
        followPlayer.distanceToPlayer = 8;

        var warpWhenFar = prefab.AddComponent<WarpToPlayerWhenFar>();
        warpWhenFar.warpDistance = 50;

        var aggressiveToZombies = prefab.AddComponent<AggressiveWhenSeeZombies>();
        aggressiveToZombies.creature = components.Creature;
        aggressiveToZombies.lastTarget = components.LastTarget;
        aggressiveToZombies.maxRange = 20;
        aggressiveToZombies.aggressionPerSecond = 1.5f;

        var voice = prefab.AddComponent<CreatureVoice>();
        var emitter = prefab.AddComponent<FMOD_CustomEmitter>();
        emitter.followParent = true;
        voice.emitter = emitter;
        voice.closeIdleSound = AudioUtils.GetFmodAsset("GilbertIdle");
        voice.playSoundOnStart = true;
        voice.minInterval = 20;
        voice.maxInterval = 30;
        
        PrefabUtils.AddVFXFabricating(prefab, "WorldModel", -0.15f, 0.18f, Vector3.zero);

        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 5f, 1f, 0f, new GilbertMaterialModifier());
    }

    private class GilbertMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
            MaterialUtils.MaterialType materialType)
        {
            if (renderer.gameObject.name.ToLower().Contains("eye"))
            {
                material.SetFloat("_SpecInt", 4);
                material.SetFloat("_Shininess", 8);
                material.SetFloat("_Fresnel", 0.4f);
            }
        }
    }
}