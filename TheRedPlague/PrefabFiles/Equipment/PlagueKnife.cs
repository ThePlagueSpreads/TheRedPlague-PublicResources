using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.PrefabFiles.Buildable;
using TheRedPlague.Utilities.Gadgets;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Equipment;

public static class PlagueKnife
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueKnife")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueKnifeIcon"));

    public static void Register()
    {
        var plagueKnife = new CustomPrefab(Info);
        var plagueKnifeTemplate = new CloneTemplate(Info, TechType.Knife);
        plagueKnifeTemplate.ModifyPrefab += ModifyPrefab;
        plagueKnife.SetGameObject(plagueKnifeTemplate);
        plagueKnife.SetEquipment(EquipmentType.Hand);
        plagueKnife.SetRecipe(new RecipeData(new CraftData.Ingredient(TechType.Knife),
                new CraftData.Ingredient(ModPrefabs.AmalgamatedBone.TechType, 3)))
            .WithCraftingTime(8)
            .WithFabricatorType(AdminFabricator.AdminCraftTree);
        plagueKnife.SetPdaGroupCategoryAfter(TechGroup.Personal, TechCategory.Tools, TechType.Knife);
        plagueKnife.SetBackgroundType(CustomBackgroundTypes.PlagueItem);
        plagueKnife.Register();
    }

    private static void ModifyPrefab(GameObject prefab)
    {
        var renderer = prefab.GetComponentInChildren<Renderer>();
        Object.DestroyImmediate(renderer.gameObject.GetComponent<VFXFabricating>());
        renderer.enabled = false;

        var newModel = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PlagueKnifeModel"), prefab.transform);
        newModel.transform.localPosition = new Vector3(0.03f, 0.05f, 0.001f);
        newModel.transform.localEulerAngles = new Vector3(85, 180, 180);
        newModel.transform.localScale = Vector3.one * 0.05f;
        MaterialUtils.ApplySNShaders(newModel, 7f, 2f, 10f);
        
        var skyApplier = prefab.GetComponentInChildren<SkyApplier>();
        skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();

        var oldKnifeComponent = prefab.GetComponent<Knife>();

        var newKnifeComponent = prefab.AddComponent<PlagueKnifeTool>();
        newKnifeComponent.attackSound = AudioUtils.GetFmodAsset("event:/creature/warper/swipe");
        newKnifeComponent.underwaterMissSound = AudioUtils.GetFmodAsset("event:/creature/warper/swipe");
        newKnifeComponent.surfaceMissSound = oldKnifeComponent.surfaceMissSound;
        newKnifeComponent.damageType = oldKnifeComponent.damageType;
        newKnifeComponent.damage = 20;
        newKnifeComponent.plagueDamage = 30;
        newKnifeComponent.attackDist = 4;
        newKnifeComponent.vfxEventType = VFXEventTypes.knife;
        newKnifeComponent.mainCollider = oldKnifeComponent.mainCollider;
        newKnifeComponent.drawSound = oldKnifeComponent.drawSound;
        newKnifeComponent.firstUseSound = oldKnifeComponent.firstUseSound;
        newKnifeComponent.hitBleederSound = oldKnifeComponent.hitBleederSound;
        newKnifeComponent.bleederDamage = 50;
        newKnifeComponent.socket = oldKnifeComponent.socket;
        newKnifeComponent.ikAimRightArm = true;
        newKnifeComponent.drawTime = 0;
        newKnifeComponent.holsterTime = 0.1f;
        newKnifeComponent.pickupable = oldKnifeComponent.pickupable;
        newKnifeComponent.hasFirstUseAnimation = true;
        newKnifeComponent.hasBashAnimation = true;
        Object.DestroyImmediate(oldKnifeComponent);
        
        PrefabUtils.AddVFXFabricating(prefab, newModel.gameObject.name, -0.05f, 0.05f, default, 0.03f, Vector3.up * 90);
    }
}