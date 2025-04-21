using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Mono.Buildables.Drone;
using TheRedPlague.Mono.CinematicEvents;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.DomeDrones;

public static class DomeDroneFormationPrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("DomeDroneFormation")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("TadpoleBuildIcon"));

    private const float CraftTime = 20;

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.SetRecipe(new RecipeData(
                new CraftData.Ingredient(TechType.Titanium, 16),
                new CraftData.Ingredient(TechType.CopperWire, 4),
                new CraftData.Ingredient(TechType.Quartz, 6)))
            .WithFabricatorType(CraftTree.Type.Constructor)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.ConstructorVehicles)
            .WithCraftingTime(CraftTime);
        prefab.SetPdaGroupCategory(TechGroup.Constructor, TechCategory.Constructor);
        prefab.Register();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, System.Array.Empty<TechType>(),
            KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("TadpoleUnlockPopup"));
    }

    private static IEnumerator GetGameObject(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("TadpoleFormation"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        var droneTask = CraftData.GetPrefabForTechTypeAsync(DomeDronePrefab.Info.TechType);
        yield return droneTask;
        var dronePrefab = droneTask.GetResult();
        foreach (Transform child in obj.transform)
        {
            var drone = Object.Instantiate(dronePrefab, child);
            Object.DestroyImmediate(drone.GetComponent<PrefabIdentifier>());
            Object.DestroyImmediate(drone.GetComponent<LargeWorldEntity>());
            Object.DestroyImmediate(drone.GetComponent<DomeDroneBehaviour>());
        }
        
        var vfxConstructing = obj.AddComponent<VFXConstructing>();
        var seamothTask = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
        yield return seamothTask;
        var seamoth = seamothTask.GetResult();
        var seamothConstructing = seamoth.GetComponent<VFXConstructing>();
        vfxConstructing.ghostMaterial = seamothConstructing.ghostMaterial;
        vfxConstructing.alphaTexture = seamothConstructing.alphaTexture;
        vfxConstructing.alphaDetailTexture = seamothConstructing.alphaDetailTexture;
        vfxConstructing.transparentShaders = seamothConstructing.transparentShaders;

        var separateOnConstruct = obj.AddComponent<DomeDronesSeparateOnConstruct>();
        separateOnConstruct.spawnTechType = DomeDronePrefab.Info.TechType;
        separateOnConstruct.vfxConstructing = vfxConstructing;

        prefab.Set(obj);
    }
}