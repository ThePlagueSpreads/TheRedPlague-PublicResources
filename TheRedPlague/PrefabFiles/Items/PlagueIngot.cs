using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public static class PlagueIngot
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueIngot")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueIngotIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.SetPdaGroupCategory(CustomTechCategories.PlagueBiotechGroup, CustomTechCategories.PlagueBiotechCategory);
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(PlagueCatalyst.Info.TechType)));
        prefab.Register();
        
        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
        
        BaseBioReactor.charge[Info.TechType] = 600;
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PlagueIngot"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj);
        obj.AddComponent<Pickupable>();
        PrefabUtils.AddWorldForces(obj, 20);
        prefab.Set(obj);
        yield break;
    }
}