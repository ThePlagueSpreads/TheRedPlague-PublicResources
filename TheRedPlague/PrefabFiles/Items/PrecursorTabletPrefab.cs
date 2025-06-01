using System;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public class PrecursorTabletPrefab
{
    public PrefabInfo Info { get; }
    private Func<Texture2D> TabletTexture { get; }
    private Sprite PopupIcon { get; }
    private RecipeData Recipe { get; }
    private Action<CraftingGadget> SetCraftingGadget { get; }
    private bool UseInfectionBackground { get; }

    public PrecursorTabletPrefab(PrefabInfo info, Func<Texture2D> tabletTexture, Sprite popupIcon, RecipeData recipe,
        Action<CraftingGadget> setCraftingGadget, bool useInfectionBackground)
    {
        Info = info;
        TabletTexture = tabletTexture;
        PopupIcon = popupIcon;
        Recipe = recipe;
        SetCraftingGadget = setCraftingGadget;
        UseInfectionBackground = useInfectionBackground;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(prefab.Info, TechType.PrecursorKey_Orange)
        {
            ModifyPrefab = obj =>
            {
                var tabletTexture = TabletTexture.Invoke();
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
        var craftingGadget = prefab.SetRecipe(Recipe);
        SetCraftingGadget?.Invoke(craftingGadget);
        prefab.SetPdaGroupCategoryAfter(TechGroup.Personal, TechCategory.Equipment, TechType.PrecursorKey_Orange);
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, new[] { Info.TechType },
            KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound,
            PopupIcon);
        prefab.Register();

        if (UseInfectionBackground)
            CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
    }
}