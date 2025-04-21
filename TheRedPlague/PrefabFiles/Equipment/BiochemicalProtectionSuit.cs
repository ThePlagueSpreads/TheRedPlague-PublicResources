using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using TheRedPlague.Mono.Equipment;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Equipment;

public static class BiochemicalProtectionSuit
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("BiochemicalProtectionModule")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("BiochemicalProtectionModuleIcon"));
    
    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        var template = new CloneTemplate(Info, TechType.MapRoomUpgradeScanRange);
        template.ModifyPrefab += obj =>
        {
            obj.AddComponent<UsableItem>().SetOnUseAction(Info.ClassID, OnUse);
            obj.GetComponentInChildren<VFXFabricating>().posOffset = default;
        };
        prefab.SetPdaGroupCategoryAfter(TechGroup.Personal, TechCategory.Equipment, TechType.Compass);
        prefab.SetRecipe(new RecipeData(
                new CraftData.Ingredient(TechType.ComputerChip), new CraftData.Ingredient(TechType.Silicone, 2)))
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithCraftingTime(5)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorEquipment);
        prefab.SetGameObject(template);
        prefab.Register();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType,
            System.Array.Empty<TechType>(), KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("BiochemicalProtectionModulePopup"));
    }

    private static void OnUse()
    {
        StoryUtils.UseBiochemicalProtectionSuitEvent.Trigger();
    }
}