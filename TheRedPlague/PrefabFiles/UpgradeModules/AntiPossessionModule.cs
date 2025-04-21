using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using TheRedPlague.Mono.UpgradeModules;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.UpgradeModules;

public static class AntiPossessionModule
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("AntiPossessionModule")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("AntiPossessionModule"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, TechType.SeamothSolarCharge)
        {
            ModifyPrefab = obj => obj.GetComponent<Rigidbody>().isKinematic = true
        });
        prefab.SetVehicleUpgradeModule()
            .WithOnModuleAdded(OnAdd)
            .WithOnModuleRemoved(OnRemove);
        prefab.SetUnlock(Info.TechType)
            .WithAnalysisTech(null, KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound,
                KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage);
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(TechType.SeamothElectricalDefense),
            new CraftData.Ingredient(DormantNeuralMatter.Info.TechType)))
            .WithCraftingTime(4.6f)
            .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.VehicleUpgradesCommonModules);
        prefab.SetPdaGroupCategoryAfter(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.VehicleStorageModule);

        prefab.Register();
    }

    private static void OnAdd(Vehicle vehicle, int slot)
    {
        var count = vehicle.modules.GetCount(Info.TechType);
        if (count > 0)
        {
            vehicle.gameObject.EnsureComponent<AntiPossessionModuleBehaviour>();
        }
    }

    private static void OnRemove(Vehicle vehicle, int slot)
    {
        var count = vehicle.modules.GetCount(Info.TechType);
        if (count <= 0)
        {
            Object.Destroy(vehicle.gameObject.GetComponent<AntiPossessionModuleBehaviour>());
        }
    }
}