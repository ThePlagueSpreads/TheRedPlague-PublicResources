using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Mono.Buildables.PlagueRefinery;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Buildable;

public static class PlagueRefinery
{
    private const string CraftTreeId = "PlagueRefinery";
    
    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");
    private static readonly int Shininess = Shader.PropertyToID("_Shininess");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");

    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueRefinery")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueRefineryIcon"));

    public static CraftTree.Type CraftTreeType { get; private set; }
    private static ModCraftTreeRoot CraftTreeRoot { get; set; }

    public static void Register()
    {
        var customPrefab = new CustomPrefab(Info);
        customPrefab.SetGameObject(CreatePrefab);
        customPrefab.SetRecipe(new RecipeData(new CraftData.Ingredient(TechType.PlasteelIngot),
            new CraftData.Ingredient(TechType.WiringKit),
            new CraftData.Ingredient(TechType.EnameledGlass),
            new CraftData.Ingredient(TechType.Magnetite)));
        customPrefab.SetPdaGroupCategoryBefore(TechGroup.InteriorModules, TechCategory.InteriorModule,
            TechType.Radio);

        customPrefab.Register();

        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, System.Array.Empty<TechType>(),
            KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage,
            Plugin.AssetBundle.LoadAsset<Sprite>("PlagueRefineryPopup"));
        
        // Register craft tree

        CraftTreeType = EnumHandler.AddEntry<CraftTree.Type>(CraftTreeId).CreateCraftTreeRoot(out var root);
        CraftTreeRoot = root;
        
        // Register vanilla nodes
        
        RegisterPlagueRefineryRecipe(TechType.SeaTreaderPoop);
        RegisterPlagueRefineryRecipe(TechType.ScrapMetal);
        RegisterPlagueRefineryRecipe(TechType.BloodOil);
    }

    public static void RegisterPlagueRefineryRecipe(TechType inputItemTechType)
    {
        var recipeInfo = PrefabInfo.WithTechType("Process" + inputItemTechType, true)
            .WithIcon(SpriteManager.Get(inputItemTechType));
        var customPrefab = new CustomPrefab(recipeInfo);
        customPrefab.SetRecipe(new RecipeData(new CraftData.Ingredient(inputItemTechType)))
            .WithFabricatorType(CraftTreeType);
        customPrefab.SetGameObject(() => TrpPrefabUtils.CreateLootCubePrefab(recipeInfo));
        customPrefab.Register();
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PlagueRefineryPrefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 7f, 0.3f, 0.6f,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent),
            new RefineryMaterialModifier());
        var constructable = PrefabUtils.AddConstructable(obj, Info.TechType,
            ConstructableFlags.Base | ConstructableFlags.Ground | ConstructableFlags.Inside |
            ConstructableFlags.Submarine, obj.transform.Find("PlagueRefinery").gameObject);
        constructable.rotationEnabled = true;
        constructable.placeMaxDistance = 5;
        var bounds = obj.AddComponent<ConstructableBounds>();
        bounds.bounds = new OrientedBounds(Vector3.up * 0.54f, Quaternion.identity, new Vector3(1.1f, 0.48f, 0.71f));

        var behavior = obj.AddComponent<PlagueRefineryMachine>();
        behavior.animator = obj.transform.Find("PlagueRefinery").gameObject.GetComponent<Animator>();
        behavior.bloodOverlayRenderer =
            obj.transform.Find("PlagueRefinery/BloodOverlay").gameObject.GetComponent<Renderer>();

        var workingSoundEmitter = obj.AddComponent<FMOD_CustomLoopingEmitter>();
        workingSoundEmitter.SetAsset(AudioUtils.GetFmodAsset("PlagueRefineryWorking"));
        workingSoundEmitter.assetStart = AudioUtils.GetFmodAsset("PlagueRefineryInsert");
        workingSoundEmitter.assetStop = AudioUtils.GetFmodAsset("PlagueRefineryFinish");
        behavior.workingSound = workingSoundEmitter;
        
        behavior.grinderRenderers = new[]
        {
            obj.transform.Find("PlagueRefinery/Grinder").gameObject.GetComponent<Renderer>(),
            obj.transform.Find("PlagueRefinery/Grinder.001").gameObject.GetComponent<Renderer>()
        };
        behavior.grinderBloodDiffuse = Plugin.AssetBundle.LoadAsset<Texture2D>("GrinderBloodyDiffuse");
        behavior.grinderBloodSpecular = Plugin.AssetBundle.LoadAsset<Texture2D>("GrinderBloodySpec");
        
        behavior.inputSpawnPoint = obj.transform.Find("InputSpawnPoint");
        behavior.lootSpawnPoint = obj.transform.Find("LootSpawnPoint");
        behavior.lootStopPoint = obj.transform.Find("LootStopPoint");
        behavior.elevator = obj.transform.Find("PlagueRefinery/Elevator-B");

        var target = obj.AddComponent<PlagueRefineryInterface>();
        target.machine = behavior;
        target.constructable = constructable;
        
        prefab.Set(obj);
        yield break;
    }

    private class RefineryMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
            MaterialUtils.MaterialType materialType)
        {
            if (materialType == MaterialUtils.MaterialType.Transparent)
            {
                if (material.name.Contains("BloodOverlay"))
                {
                    material.SetFloat(SpecInt, 15);
                    material.SetFloat(Shininess, 6.5f);
                }
                else
                {
                    material.SetColor(SpecColor, new Color(1, 3, 3));
                    material.SetFloat(SpecInt, 1);
                    material.SetFloat(Shininess, 9);
                }
            }

            if (material.name.StartsWith("Screen"))
            {
                material.color = Color.white * 0.2f;
                material.SetColor(SpecColor, new Color(1, 0.5f, 0.5f));
                material.SetFloat(SpecInt, 3);
                material.SetFloat(Shininess, 8);
                material.SetFloat("_Fresnel", 0.1f);
            }
        }
    }
}