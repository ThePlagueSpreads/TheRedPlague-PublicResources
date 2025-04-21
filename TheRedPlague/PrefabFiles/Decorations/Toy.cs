using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using TheRedPlague.PrefabFiles.Buildable;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Decorations;

public class Toy
{
    private PrefabInfo Info { get; }
    private string PrefabName { get; }
    
    public Toy(string classId, string prefabName, string iconName)
    {
        Info = PrefabInfo.WithTechType(classId);
        if (!string.IsNullOrEmpty(iconName))
            Info.WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>(iconName));
        PrefabName = prefabName;
    }

    public void Register()
    {
        var customPrefab = new CustomPrefab(Info);
        
        customPrefab.SetGameObject(GetGameObject);
        customPrefab.SetRecipe(new RecipeData(
                new CraftData.Ingredient(TechType.Titanium),
                new CraftData.Ingredient(TechType.FiberMesh)))
            .WithFabricatorType(AdminFabricator.AdminCraftTree)
            .WithCraftingTime(5f);
        customPrefab.SetEquipment(EquipmentType.Hand);
        customPrefab.SetPdaGroupCategory(TechGroup.Machines, TechCategory.Machines);
        customPrefab.Register();
    }

    protected virtual void ApplyMaterials(GameObject prefab) =>
        MaterialUtils.ApplySNShaders(prefab, 6f, 1f, 2f);

    protected virtual void AddVFXFabricating(GameObject prefab) =>
        PrefabUtils.AddVFXFabricating(prefab, PathToWorldModel, -0.01f, 0.3f, default, 0.4f);

    protected virtual string PathToWorldModel => "worldmodel";
    protected virtual string PathToViewModel => "fpmodel";
    
    private GameObject GetGameObject()
    {
        var prefab = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(PrefabName));
        prefab.SetActive(false);
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        ApplyMaterials(prefab);
        var rb = prefab.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        var wf = prefab.AddComponent<WorldForces>();
        wf.useRigidbody = rb;
        wf.underwaterGravity = 0;
        prefab.AddComponent<Pickupable>();
        var placeTool = prefab.AddComponent<PlaceTool>();
        placeTool.allowedOutside = true;
        placeTool.rotationEnabled = true;
        placeTool.hasAnimations = false;
        var fpModel = prefab.AddComponent<FPModel>();
        fpModel.propModel = prefab.transform.Find(PathToWorldModel).gameObject;
        fpModel.viewModel = prefab.transform.Find(PathToViewModel).gameObject;
        var bounds = prefab.AddComponent<ConstructableBounds>();
        bounds.bounds = new OrientedBounds(new Vector3(0, 0.2f, 0), Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f));
        AddVFXFabricating(prefab);
        return prefab;
    }
}