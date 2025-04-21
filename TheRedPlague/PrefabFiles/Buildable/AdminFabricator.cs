using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Story;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Buildable;

public static class AdminFabricator
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("AdminFabricator")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("AdminFabricatorIcon"));

    public static CraftTree.Type AdminCraftTree
    {
        get
        {
            if ((int)_adminCraftTree == 0)
            {
                Plugin.Logger.LogError("Attempting to access AdminCraftTree before it is initialized!");
            }
            return _adminCraftTree;
        }
    }
    
    private static CraftTree.Type _adminCraftTree;

    public static void Register()
    {
        var customPrefab = new CustomPrefab(Info);
        var template = new CloneTemplate(Info, TechType.Fabricator)
        {
            ModifyPrefab = ModifyPrefab
        };
        customPrefab.SetGameObject(template);
        customPrefab.CreateFabricator(out var craftTree);
        _adminCraftTree = craftTree;
        customPrefab.SetRecipe(new RecipeData(
            new CraftData.Ingredient(TechType.Titanium, 2),
            new CraftData.Ingredient(TechType.ComputerChip),
            new CraftData.Ingredient(TechType.Lead)));
        customPrefab.SetPdaGroupCategoryAfter(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Fabricator);
        customPrefab.Register();
        KnownTechHandler.SetAnalysisTechEntry(new KnownTech.AnalysisTech
        {
            techType = Info.TechType,
            unlockMessage = KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage,
            unlockPopup = Plugin.AssetBundle.LoadAsset<Sprite>("AdminFabricatorPopup"),
            unlockSound = KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound,
            unlockTechTypes = new List<TechType>(),
            storyGoals = new List<StoryGoal>()
        });
    }

    private static void ModifyPrefab(GameObject prefab)
    {
        ApplyAdminFabricatorMaterials(prefab);
        
        var fabricator = prefab.GetComponent<Fabricator>();
        fabricator.craftTree = AdminCraftTree;

        var modelRoot = prefab.transform.Find("submarine_fabricator_01");
        if (modelRoot) modelRoot.localPosition = new Vector3(0, 0, 0.04f);
    }

    public static void ApplyAdminFabricatorMaterials(GameObject fabricatorModel)
    {
        foreach (var renderer in fabricatorModel.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is ParticleSystemRenderer) continue;
            var material = renderer.material;
            material.mainTexture = Plugin.AssetBundle.LoadAsset<Texture2D>("admin_fabricator_diffuse");
            material.SetTexture("_SpecTex", Plugin.AssetBundle.LoadAsset<Texture2D>("admin_fabricator_spec"));
            material.SetTexture("_Illum", Plugin.AssetBundle.LoadAsset<Texture2D>("adminfabricator_illum"));
        }

    }
}