using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public static class BloodQuartz
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("BloodQuartz")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("BloodQuartz"));

    private static PrefabInfo _bloodToNormalQuartz = PrefabInfo.WithTechType("BloodToNormalQuartz")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("BloodToNormalQuartz"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, "8ef17c52-2aa8-46b6-ada3-c3e3c4a78dd6")
        {
            ModifyPrefab = obj =>
            {
                var renderer = obj.GetComponentInChildren<Renderer>();
                var material = renderer.material;
                material.color = new Color(0.83f, 0.285714f, 0.333333f);
                material.SetColor("_SpecColor", new Color(1, 0, 0));
                material.SetColor("_GlowColor", new Color(1, 0.19f, 0.285f));
                material.SetFloat("_SpecInt", 10);
                material.SetFloat("_Fresnel", 0.5f);
            }
        });
        prefab.Register();

        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);

        var bloodToNormalQuartzPrefab = new CustomPrefab(_bloodToNormalQuartz);
        bloodToNormalQuartzPrefab.SetRecipe(new RecipeData(new CraftData.Ingredient(Info.TechType))
            {
                LinkedItems = { TechType.Quartz, TechType.Quartz },
                craftAmount = 0
            })
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithCraftingTime(3)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorsBasicMaterials);
        bloodToNormalQuartzPrefab.SetGameObject(new CloneTemplate(_bloodToNormalQuartz, TechType.Quartz){
            ModifyPrefab = obj =>
            {
                PrefabUtils.AddVFXFabricating(obj, "Quartz_small", 0, 0.5f, default, 1, new Vector3(270, 0, 0));
            }});
        bloodToNormalQuartzPrefab.Register();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, new[]
        {
            _bloodToNormalQuartz.TechType
        });
    }
}
