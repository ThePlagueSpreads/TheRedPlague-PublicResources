using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using Story;
using TheRedPlague.Data;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheRedPlague.PrefabFiles.Equipment;

public static class PlagueGrappler
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueGrappler")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueGrapplerIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.SetPdaGroupCategory(CustomTechCategories.PlagueBiotechGroup, CustomTechCategories.PlagueBiotechCategory);
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(TechType.Titanium, 2),
                new CraftData.Ingredient(TechType.WiringKit),
                new CraftData.Ingredient(PlagueIngot.Info.TechType, 4)))
            .WithCraftingTime(7)
            .WithFabricatorType(PlagueAltar.CraftTreeType)
            .WithStepsToFabricatorTab(PlagueAltar.EquipmentTab);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.Register();
        
        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
    }
    
    public static void RegisterLateStoryData()
    {
        KnownTechHandler.SetAnalysisTechEntry(new KnownTech.AnalysisTech
        {
            techType = Info.TechType,
            unlockSound = KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            storyGoals = new List<StoryGoal> { StoryUtils.UnlockPlagueGrapplerGoal},
            unlockPopup = Plugin.AssetBundle.LoadAsset<Sprite>("PlagueGrapplerPopup"),
            unlockTechTypes = new List<TechType>(),
            unlockMessage = KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage
        });
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PlagueGrappler"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 6);
        var fpModel = obj.AddComponent<FPModel>();
        fpModel.propModel = obj.transform.Find("WorldModel").gameObject;
        var viewModel = obj.transform.Find("FPModel").gameObject;
        fpModel.viewModel = viewModel;
        PrefabUtils.AddVFXFabricating(obj, "WorldModel", -0.2f, 0.25f);
        PrefabUtils.AddWorldForces(obj, 10);
        obj.AddComponent<Pickupable>();

        var stasisRifleTask = CraftData.GetPrefabForTechTypeAsync(TechType.StasisRifle);
        yield return stasisRifleTask;
        var stasisRiflePrefab = stasisRifleTask.GetResult();
        try
        {
            var stasisRifleModel = stasisRiflePrefab.transform.Find("stasis_rifle/stasis_rifle_geo");
            var stasisRifleHandleMaterial = new Material(stasisRifleModel.GetComponent<Renderer>().materials[0]);
            obj.transform.Find("WorldModel/PlagueGrapplerModel/stasis_rifle_geo").GetComponent<Renderer>().material =
                stasisRifleHandleMaterial;
            obj.transform.Find("FPModel/PlagueGrapplerModel/stasis_rifle_geo").GetComponent<Renderer>().material =
                stasisRifleHandleMaterial;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while loading stasis rifle material for plague grappler: " + e);
        }

        // This will be automatically assigned to the PlayerTool class
        var energyMixin = obj.AddComponent<EnergyMixin>();
        var batteryStorageRoot = new GameObject("BatterySlot").AddComponent<ChildObjectIdentifier>();
        batteryStorageRoot.ClassId = "PlagueGrapplerBatterySlot";
        batteryStorageRoot.transform.parent = obj.transform;
        energyMixin.storageRoot = batteryStorageRoot;
        energyMixin.compatibleBatteries = new List<TechType> { TechType.Battery, TechType.PrecursorIonBattery };
        energyMixin.batteryModels = Array.Empty<EnergyMixin.BatteryModels>();

        try
        {
            var tool = obj.AddComponent<PlagueGrapplerTool>();
            tool.mainCollider = obj.GetComponent<Collider>();
            tool.drawSound = AudioUtils.GetFmodAsset("event:/tools/stasis_gun/deploy");
            tool.pickupable = obj.GetComponent<Pickupable>(); // basically useless but idc
            tool.ikAimLeftArm = true;
            tool.ikAimRightArm = true;
            tool.useLeftAimTargetOnPlayer = true;
            tool.hasAnimations = true;
            tool.drawTime = 0;
            tool.holsterTime = 0.1f;
            tool.dropTime = 0;
            tool.bashTime = 0.6f;
            tool.hasBashAnimation = true;

            tool.projectileModel = viewModel.transform.SearchChild("ProjectilePrefab").gameObject;
            tool.lineAttachPoint = viewModel.transform.SearchChild("ToolLineAttachPoint");
            tool.line = viewModel.transform.Find("Trail").GetComponent<LineRenderer>();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while setting up PlagueGrappler: " + e);
        }
        
        // Final step:
        prefab.Set(obj);
    }
}