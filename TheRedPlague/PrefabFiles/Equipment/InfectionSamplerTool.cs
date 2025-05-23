﻿using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using Story;
using TheRedPlague.Mono.Equipment;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheRedPlague.PrefabFiles.Equipment;

public static class InfectionSamplerTool
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("InfectionSampler")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("TransfuserIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.SetRecipe(new RecipeData(
                new CraftData.Ingredient(TechType.Titanium, 2),
                new CraftData.Ingredient(TechType.CopperWire),
                new CraftData.Ingredient(TechType.Silver)))
            .WithCraftingTime(5)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorTools);
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools);
        prefab.Register();
    }

    public static void RegisterLateStoryData()
    {
        KnownTechHandler.SetAnalysisTechEntry(new KnownTech.AnalysisTech
        {
            techType = Info.TechType,
            unlockSound = KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            storyGoals = new List<StoryGoal> { StoryUtils.UnlockTransfuserEvent},
            unlockPopup = Plugin.AssetBundle.LoadAsset<Sprite>("TransfuserPopup"),
            unlockTechTypes = new List<TechType>(),
            unlockMessage = KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage
        });
    }

    public static IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("Transfuser_Prefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 7, 1, 1, new TransfuserMaterialModifier());
        var rb = obj.EnsureComponent<Rigidbody>();
        rb.mass = 8;
        rb.useGravity = false;
        var wf = obj.EnsureComponent<WorldForces>();
        wf.useRigidbody = rb;
        wf.aboveWaterDrag = 0.15f;
        wf.underwaterDrag = 0.3f;
        
        var tool = obj.AddComponent<TransfuserTool>();
        tool.pickupable = obj.EnsureComponent<Pickupable>();
        tool.mainCollider = obj.GetComponent<Collider>();
        tool.hasAnimations = true;
        tool.socket = PlayerTool.Socket.RightHand;
        tool.ikAimLeftArm = false;
        tool.ikAimRightArm = true;

        PrefabUtils.AddVFXFabricating(obj, "Model", -0.15f, 0.2f, new Vector3(-0.2f, 0.08f, 0f), 1,
            new Vector3(90, 0, 0));

        prefab.Set(obj);
        yield break;
    }

    private class TransfuserMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (materialType == MaterialUtils.MaterialType.Transparent)
            {
                material.color = new Color(1, 1, 1, 2);
                material.SetFloat("_SpecInt", 10);
            }
        }
    }
}