﻿using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using UnityEngine;
using System.Collections;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.PrefabFiles.Creatures;

namespace TheRedPlague.PrefabFiles.Equipment;

public static class InfectionTracker
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("InfectionTracker");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        var infectionTrackerTemplate = new CloneTemplate(Info, "b98da0ef-29d4-4571-9a82-53a6e6706153");
        infectionTrackerTemplate.ModifyPrefabAsync += ModifyPrefab;
        prefab.SetGameObject(infectionTrackerTemplate);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.Info.WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("InfectionTrackerIcon"));
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType,
            System.Array.Empty<TechType>(), KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("InfectionTrackerPopup"));
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(TechType.PrecursorIonCrystal),
                new CraftData.Ingredient(ConsciousNeuralMatter.Info.TechType)))
            .WithCraftingTime(5)
            .WithFabricatorType(AdminFabricator.AdminCraftTree);
        prefab.SetPdaGroupCategoryAfter(TechGroup.Personal, TechCategory.Equipment, TechType.PrecursorKey_Orange);
        prefab.Register();
    }

    private static IEnumerator ModifyPrefab(GameObject prefab)
    {
        prefab.GetComponentInChildren<Renderer>(true).material
            .SetColor(ShaderPropertyID._GlowColor, new Color(3, 0, 0));
        prefab.GetComponentsInChildren<Collider>(true).ForEach(c => c.enabled = false);
        prefab.GetComponentsInChildren<Animator>(true).ForEach(c => c.enabled = false);
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.7f, 0.07f, 0.7f);
        var tool = prefab.AddComponent<InfectionTrackerTool>();
        tool.mainCollider = collider;
        tool.drawSound = AudioUtils.GetFmodAsset("event:/interface/off_long");
        tool.hasAnimations = false;
        tool.pickupable = prefab.AddComponent<Pickupable>();
        tool.renderers = prefab.GetComponentsInChildren<Renderer>();
        tool.hasAnimations = true;

        var worldModel = prefab.GetComponentInChildren<Animator>().gameObject;

        var viewModel = Object.Instantiate(worldModel, prefab.transform);
        viewModel.SetActive(false);
        viewModel.transform.localScale = Vector3.one * 0.6f;
        viewModel.transform.localPosition = new Vector3(-0.15f, 0.01f, 0.2f);
        viewModel.transform.localEulerAngles = new Vector3(275, 180, 0);

        tool.ikAimRightArm = true;

        var fpModel = prefab.EnsureComponent<FPModel>();
        fpModel.propModel = worldModel;
        fpModel.viewModel = viewModel;

        var diveReelTask = CraftData.GetPrefabForTechTypeAsync(TechType.DiveReel);
        yield return diveReelTask;
        var arrow = Object.Instantiate(
            diveReelTask.GetResult().GetComponent<DiveReel>().nodePrefab.transform.Find("Arrow").gameObject,
            prefab.transform);
        arrow.SetActive(false);
        var arrowCenter = new GameObject("ArrowCenter").transform;
        arrowCenter.parent = viewModel.transform.GetChild(0).GetChild(0);
        arrowCenter.localPosition = Vector3.forward * 0.15f;
        arrow.GetComponentInChildren<Renderer>(true).material.color = Color.red;
        arrow.GetComponentInChildren<Renderer>(true).material
            .SetColor(ShaderPropertyID._ColorStrength, new Color(1.5f, 0, 0));
        arrow.GetComponentInChildren<Renderer>(true).material
            .SetColor(ShaderPropertyID._ColorStrengthAtNight, new Color(1.5f, 0, 0));

        tool.arrowPrefab = arrow;
        tool.arrowRoot = arrowCenter;
        tool.viewModel = viewModel;

        var rb = prefab.EnsureComponent<Rigidbody>();
        rb.mass = 200;
        rb.useGravity = false;
        prefab.EnsureComponent<WorldForces>();

        PrefabUtils.AddVFXFabricating(worldModel, "alien_relic_ctrl/alien_relic_04", -0.03f, 0.1f,
            new Vector3(0, 0, 0), 0.5f, new Vector3(-90, 0, 0));
    }
}