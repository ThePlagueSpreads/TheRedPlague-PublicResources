using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.Buildables.AssimilationGenerator;
using TheRedPlague.Mono.VFX;
using TheRedPlague.PrefabFiles.Creatures;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Buildable;

public static class AssimilationGenerator
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("AssimilationGenerator")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("AssimilationGeneratorIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.SetPdaGroupCategoryAfter(TechGroup.ExteriorModules, TechCategory.ExteriorModule, TechType.ThermalPlant);
        prefab.SetRecipe(new RecipeData(
            new CraftData.Ingredient(PlagueIngot.Info.TechType, 3),
            new CraftData.Ingredient(ConsciousNeuralMatter.Info.TechType)));
        prefab.Register();

        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> result)
    {
        var prefab = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("AssimilationGenerator"));
        prefab.SetActive(false);
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        MaterialUtils.ApplySNShaders(prefab, 6);
        var modelParent = prefab.transform.Find("bioreactor aniamted");
        var constructable = PrefabUtils.AddConstructable(prefab, Info.TechType,
            ConstructableFlags.AllowedOnConstructable | ConstructableFlags.Ground | ConstructableFlags.Outside |
            ConstructableFlags.Rotatable,
            modelParent.gameObject);
        constructable.placeDefaultDistance = 7;
        constructable.placeMinDistance = 4;
        constructable.placeMaxDistance = 12f;
        var bounds = prefab.AddComponent<ConstructableBounds>();
        bounds.bounds =
            new OrientedBounds(new Vector3(0f, 3.7f, 0f), Quaternion.identity, new Vector3(2.1f, 2.1f, 2.1f));

        var trigger = prefab.transform.Find("GobbleTrigger").gameObject.AddComponent<AssimilationGeneratorTrigger>();
        var function = prefab.AddComponent<AssimilationGeneratorFunction>();
        function.animator = prefab.GetComponentInChildren<Animator>();
        function.constructable = constructable;
        trigger.function = function;

        var activateEmitter = prefab.AddComponent<FMOD_CustomEmitter>();
        activateEmitter.asset = AudioUtils.GetFmodAsset("AssimilationGeneratorActivate");
        activateEmitter.restartOnPlay = false;
        function.activateEmitter = activateEmitter;

        var loopingEmitter = prefab.AddComponent<FMOD_CustomLoopingEmitter>();
        loopingEmitter.asset = AudioUtils.GetFmodAsset("AssimilationGeneratorLoop");
        loopingEmitter.restartOnPlay = false;
        function.loopEmitter = loopingEmitter;

        var animateEmission = prefab.AddComponent<AnimateMaterialProperty>();
        animateEmission.renderers = prefab.GetComponentsInChildren<Renderer>();
        animateEmission.interpolationSpeed = 0.2f;
        animateEmission.updateInterval = 0.09f;
        animateEmission.propertyIds = new[] { ShaderPropertyID._GlowStrength, ShaderPropertyID._GlowStrengthNight };
        function.glowAnimator = animateEmission;

        result.Set(prefab);
        yield break;
    }
}