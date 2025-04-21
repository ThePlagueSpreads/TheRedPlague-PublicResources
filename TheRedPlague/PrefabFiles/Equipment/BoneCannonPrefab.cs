using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Equipment;

public static class BoneCannonPrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("BoneCannon")
        .WithSizeInInventory(new Vector2int(2, 2))
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("DrifterCannonIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefabAsync);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(PlagueIngot.Info.TechType, 3),
                new CraftData.Ingredient(ModPrefabs.AmalgamatedBone.TechType),
                new CraftData.Ingredient(TechType.Benzene)))
            .WithCraftingTime(10)
            .WithFabricatorType(PlagueAltar.CraftTreeType)
            .WithStepsToFabricatorTab(PlagueAltar.EquipmentTab);
        prefab.SetPdaGroupCategory(CustomTechCategories.PlagueBiotechGroup, CustomTechCategories.PlagueBiotechCategory);
        prefab.Register();

        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
    }

    private static IEnumerator GetPrefabAsync(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("DrifterCannonPrefab"));
        obj.SetActive(false);
        var rb = obj.EnsureComponent<Rigidbody>();
        rb.mass = 50;
        rb.useGravity = false;
        var wf = obj.EnsureComponent<WorldForces>();
        wf.useRigidbody = rb;
        wf.aboveWaterDrag = 0.15f;
        wf.underwaterDrag = 0.3f;
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 6f, 2f, 0.5f);
        var fpModel = obj.AddComponent<FPModel>();
        fpModel.propModel = obj.transform.Find("DrifterCannonAnimated").gameObject;
        fpModel.viewModel = obj.transform.Find("DrifterCannonAnimated_ViewModel").gameObject;
        var tool = obj.AddComponent<DrifterCannonTool>();
        tool.pickupable = obj.EnsureComponent<Pickupable>();
        tool.mainCollider = obj.GetComponent<Collider>();
        tool.animator = fpModel.viewModel.GetComponent<Animator>();
        tool.hasAnimations = true;
        tool.socket = PlayerTool.Socket.RightHand;
        tool.ikAimLeftArm = false;
        tool.ikAimRightArm = true;

        var projectile = obj.transform.Find("DrifterProjectilePrefab").gameObject;
        var projectileWf = projectile.AddComponent<WorldForces>();
        projectileWf.useRigidbody = projectile.GetComponent<Rigidbody>();

        tool.projectilePrefab = projectile;
        tool.projectileSpawnPoint = fpModel.viewModel.transform.Find("ProjectileSpawnPoint");
        tool.glowRenderer = fpModel.viewModel.transform.Find("DrifterCannon").gameObject.GetComponent<Renderer>();

        PrefabUtils.AddVFXFabricating(obj, "DrifterCannonAnimated", -0.3f, 0.25f, Vector3.down * 0.2f, 0.5f);

        prefab.Set(obj);
        yield break;
    }
}