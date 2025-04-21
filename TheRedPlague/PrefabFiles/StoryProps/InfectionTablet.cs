using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using TheRedPlague.Data;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.StoryProps;

public static class InfectionTablet
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("InfectionTablet")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("InfectionTabletIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(prefab.Info, TechType.PrecursorKey_Orange)
        {
            ModifyPrefab = obj =>
            {
                var tabletTexture = Plugin.AssetBundle.LoadAsset<Texture2D>("InfectionTabletTexture");
                var mainRenderer =
                    obj.transform.Find("Model/Rig_J/precursor_key_C_02_symbol_02").GetComponent<Renderer>();
                var materials = mainRenderer.materials;
                materials[1].mainTexture = tabletTexture;
                materials[1].SetTexture(ShaderPropertyID._Illum, tabletTexture);
                mainRenderer.materials = materials;
                var viewModelRenderer =
                    obj.transform.Find("ViewModel/Rig_J/precursor_key_C_02_symbol_02").GetComponent<Renderer>();
                var viewModelMaterials = viewModelRenderer.materials;
                viewModelMaterials[1].mainTexture = tabletTexture;
                viewModelMaterials[1].SetTexture(ShaderPropertyID._Illum, tabletTexture);
                viewModelRenderer.materials = viewModelMaterials;
            }
        });
        prefab.SetRecipe(new RecipeData(
                new CraftData.Ingredient(RedPlagueSample.Info.TechType),
                new CraftData.Ingredient(TechType.Diamond)))
            .WithFabricatorType(AdminFabricator.AdminCraftTree);
        prefab.SetPdaGroupCategoryAfter(TechGroup.Personal, TechCategory.Equipment, TechType.PrecursorKey_Orange);
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, System.Array.Empty<TechType>(),
            KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("InfectionTabletPopup"));
        prefab.Register();
        
        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
    }
}