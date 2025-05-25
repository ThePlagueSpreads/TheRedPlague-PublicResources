using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.Utilities.Gadgets;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public class Meatball : CreatureAsset
{
    public Meatball(PrefabInfo prefabInfo) : base(prefabInfo)
    {
        CustomPrefab.SetRecipe(new RecipeData(new CraftData.Ingredient(RedPlagueSample.Info.TechType, 2)))
            .WithCraftingTime(3.5f)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorMachines);
        CustomPrefab.SetPdaGroupCategory(CustomTechCategories.PlagueBiotechGroup, CustomTechCategories.PlagueBiotechCategory);
        CustomPrefab.SetBackgroundType(CustomBackgroundTypes.PlagueItem);

        KnownTechHandler.SetAnalysisTechEntry(RedPlagueSample.Info.TechType, new[] { prefabInfo.TechType },
            KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("MeatballPopup"));

        // Meatball pack (for plague altar)
        var meatballPack = new CustomPrefab(PrefabInfo.WithTechType("MeatballPack", true)
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("MeatballPackIcon")));
        var meatballPackTemplate = new AssetBundleTemplate(Plugin.AssetBundle, "MeatballPackPrefab", meatballPack.Info);
        meatballPack.SetGameObject(meatballPackTemplate);
        MaterialUtils.ApplySNShaders(meatballPackTemplate.Prefab);
        PrefabUtils.AddBasicComponents(meatballPackTemplate.Prefab, meatballPack.Info.ClassID, meatballPack.Info.TechType, LargeWorldEntity.CellLevel.Near);
        PrefabUtils.AddVFXFabricating(meatballPackTemplate.Prefab,
            "CraftModel", -0.25f, 0.5f, default, 0.5f);
        meatballPack.SetRecipe(new RecipeData(new CraftData.Ingredient(RedPlagueSample.Info.TechType, 2),
                new CraftData.Ingredient(PlagueCatalyst.Info.TechType))
            {
                LinkedItems = new List<TechType> { prefabInfo.TechType, prefabInfo.TechType, prefabInfo.TechType },
                craftAmount = 0
            }).WithCraftingTime(6)
            .WithFabricatorType(PlagueAltar.CraftTreeType)
            .WithStepsToFabricatorTab(PlagueAltar.ConsumableTab);
        meatballPack.Register();
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(Plugin.AssetBundle.LoadAsset<GameObject>("MeatballPrefab"),
            BehaviourType.Shark,
            EcoTargetType.Shark, 1000)
        {
            CellLevel = LargeWorldEntity.CellLevel.Near,
            SwimRandomData = null,
            LocomotionData = new LocomotionData(0, 0, 0),
            Mass = 150f,
            AcidImmune = true,
            RespawnData = new RespawnData(false),
            PickupableFishData = new PickupableFishData(TechType.Peeper, "Meatball", "MeatballFirstPerson"),
            LiveMixinData =
            {
                destroyOnDeath = true
            }
        };
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        components.WorldForces.underwaterGravity = 0;
        components.WorldForces.underwaterDrag = 2.34f;
        prefab.EnsureComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.Immune;
        PrefabUtils.AddVFXFabricating(prefab, "Meatball", -0.15f, 0.24f, Vector3.up * 0.1f, 20f);
        yield break;
    }
}