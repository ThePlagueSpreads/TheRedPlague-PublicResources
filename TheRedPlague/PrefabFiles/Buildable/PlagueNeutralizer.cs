using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Mono.Buildables.PlagueNeutralizer;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Buildable;

public static class PlagueNeutralizer
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueNeutralizer")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueNeutralizerIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, "c9bdcc4d-a8c6-43c0-8f7a-f86841cd4493")
        {
            ModifyPrefabAsync = ModifyPrefab
        });
        prefab.SetPdaGroupCategoryBefore(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Radio);
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(TechType.Titanium, 2),
            new CraftData.Ingredient(TechType.WiringKit),
            new CraftData.Ingredient(TechType.Quartz),
            new CraftData.Ingredient(TechType.DisinfectedWater)));
        prefab.Register();

        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, System.Array.Empty<TechType>(),
            KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage,
            Plugin.AssetBundle.LoadAsset<Sprite>("PlagueNeutralizerPopup"));
    }

    private static IEnumerator ModifyPrefab(GameObject prefab)
    {
        Object.DestroyImmediate(prefab.GetComponent<SpecimenAnalyzer>());
        Object.DestroyImmediate(prefab.GetComponentInChildren<SpecimenAnalyzerBase>());

        // LIQUID
        var liquidObj = new GameObject("PlagueNeutralizerLiquid");
        liquidObj.transform.SetParent(prefab.transform, false);
        liquidObj.transform.localScale = Vector3.one * 100;
        liquidObj.transform.localEulerAngles = Vector3.zero;
        liquidObj.transform.localPosition = Vector3.up * 0.817f;
        var liquidRenderer = liquidObj.AddComponent<MeshRenderer>();
        liquidRenderer.material = CreateLiquidMaterial();
        liquidObj.AddComponent<MeshFilter>().mesh = Plugin.AssetBundle.LoadAsset<Mesh>("PlagueNeutralizerLiquid");
        var liquid = liquidObj.AddComponent<PlagueNeutralizerLiquid>();
        liquid.renderer = liquidRenderer;

        // PLAGUE CATALYSTS

        var catalystSpawnPointsRoot = new GameObject("CatalystSpawnPointsRoot").transform;
        catalystSpawnPointsRoot.SetParent(prefab.transform, false);
        catalystSpawnPointsRoot.localPosition = Vector3.zero;
        catalystSpawnPointsRoot.localEulerAngles = Vector3.zero;
        catalystSpawnPointsRoot.localScale = Vector3.one;
        var catalystSpawnPoints = new[]
        {
            CreateCatalystSpawnPoint(catalystSpawnPointsRoot,
                new Vector3(-0.380f, 0.809f, -0.221f), new Vector3(0, 330, 0)),
            CreateCatalystSpawnPoint(catalystSpawnPointsRoot,
                new Vector3(-0.267f, 0.815f, 0.202f), new Vector3(355, 28, 0)),
            CreateCatalystSpawnPoint(catalystSpawnPointsRoot,
                new Vector3(0.199f, 0.8f, 0.393f), new Vector3(355, 30, 0)),
            CreateCatalystSpawnPoint(catalystSpawnPointsRoot,
                new Vector3(0.014f, 0.761f, -0.051f), new Vector3(355, 150, 0)),
            CreateCatalystSpawnPoint(catalystSpawnPointsRoot,
                new Vector3(0.43f, 0.781f, -0.009f), new Vector3(355, 30, 1)),
            CreateCatalystSpawnPoint(catalystSpawnPointsRoot,
                new Vector3(0.051f, 0.736f, -0.403f), new Vector3(4, 254, 2))
        };
        var catalystPrefabTask = CraftData.GetPrefabForTechTypeAsync(PlagueCatalyst.Info.TechType);
        yield return catalystPrefabTask;
        var catalystPrefab = UWE.Utils.InstantiateDeactivated(catalystPrefabTask.GetResult());
        var crystalMaterial = catalystPrefab.transform.Find("PlagueCrystal/Crystal").GetComponent<Renderer>().material;
        Object.DestroyImmediate(catalystPrefab.GetComponent<PrefabIdentifier>());
        Object.DestroyImmediate(catalystPrefab.GetComponent<TechTag>());
        Object.DestroyImmediate(catalystPrefab.GetComponent<ResourceTracker>());
        Object.DestroyImmediate(catalystPrefab.GetComponent<Collider>());
        Object.DestroyImmediate(catalystPrefab.GetComponent<LargeWorldEntity>());
        crystalMaterial.SetColor("_SpecColor", new Color(2, 2, 2));
        var catalystModels = new GameObject[catalystSpawnPoints.Length];
        for (var i = 0; i < catalystModels.Length; i++)
        {
            var catalyst = Object.Instantiate(catalystPrefab, catalystSpawnPoints[i], false);
            catalyst.transform.localScale = Vector3.one * 0.29f;
            catalystModels[i] = catalyst;
        }

        Object.Destroy(catalystPrefab);

        var catalysts = prefab.AddComponent<PlagueNeutralizerCatalysts>();
        catalysts.models = catalystModels;

        // INGOT FABRICATION

        var ingotTask = CraftData.GetPrefabForTechTypeAsync(PlagueIngot.Info.TechType);
        yield return ingotTask;
        var ingot = Object.Instantiate(ingotTask.GetResult().transform.GetChild(0).gameObject, prefab.transform, true);
        ingot.SetActive(false);
        ingot.transform.localPosition = Vector3.up * 1.15f;
        ingot.transform.localRotation = Quaternion.identity;
        ingot.transform.localScale = Vector3.one * 0.5f;

        var fabricatorTask = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return fabricatorTask;
        var baseFabricator = fabricatorTask.GetResult();

        var sparks =
            Object.Instantiate(baseFabricator.GetComponent<Fabricator>().sparksPS, prefab.transform, true);
        sparks.SetActive(true);
        sparks.transform.localPosition = new Vector3(0, 1.15f, 0);
        sparks.transform.localRotation = Quaternion.identity;
        sparks.transform.localScale = Vector3.one;
        sparks.GetComponent<ParticleSystemRenderer>().material.color = new Color(3, 0.7f, 1);
        sparks.transform.Find("xFlash").GetComponent<ParticleSystemRenderer>().material.color =
            new Color(2, 0, 0.3f);
        sparks.transform.Find("xSparkDot").GetComponent<ParticleSystemRenderer>().material.color =
            new Color(3, 1, 2);
        sparks.transform.Find("xSparks").GetComponent<ParticleSystemRenderer>().material.color =
            new Color(3, 0.3f, 0.1f);

        var buildSound = prefab.AddComponent<FMOD_CustomLoopingEmitter>();
        buildSound.SetAsset(AudioUtils.GetFmodAsset("event:/sub_module/fabricator/build_long_loop"));
        buildSound.playOnAwake = false;
        buildSound.restartOnPlay = false;
        
        var buildSound2 = prefab.AddComponent<FMOD_CustomLoopingEmitter>();
        buildSound2.SetAsset(AudioUtils.GetFmodAsset("PlagueNeutralizerFabricating"));
        buildSound2.playOnAwake = false;
        buildSound2.restartOnPlay = false;


        var builderTask = CraftData.GetPrefabForTechTypeAsync(TechType.Builder);
        yield return builderTask;
        var builderBeam = builderTask.GetResult().transform
            .Find("builder/builder_FP/Root/l_nozzle_root/l_nozzle/L_laser/beamLeft")
            .gameObject;
        var slider =
            prefab.transform.Find(
                "model/Species_Analyzer/Root_jnt/Species_Analyzer_slider_01_jnt/Species_Analyzer_slider_02_jnt");
        GameObject[] fabricatorBeams =
        {
            Object.Instantiate(builderBeam, slider),
            Object.Instantiate(builderBeam, slider)
        };
        for (var i = 0; i < fabricatorBeams.Length; i++)
        {
            var beamTransform = fabricatorBeams[i].transform;
            beamTransform.localPosition = new Vector3(i == 0 ? -0.13f : 0.13f, 0.22f, 0.8f);
            beamTransform.localScale = new Vector3(0.4f, 0.4f, 0.8f);
            beamTransform.localEulerAngles = new Vector3(10, 180);
            var material = fabricatorBeams[i].GetComponent<Renderer>().material;
            material.color = new Color(1.5f, 0.1f, 0.2f);
            fabricatorBeams[i].SetActive(false);
        }

        // FOOD INPUT

        Object.DestroyImmediate(prefab.GetComponentInChildren<StorageContainer>());

        var storageContainer = prefab.AddComponent<PlagueNeutralizerContainerInterface>();
        storageContainer.prefabRoot = prefab;
        storageContainer.width = 3;
        storageContainer.height = 6;
        storageContainer.hoverText = "PlagueNeutralizerHoverText";
        storageContainer.storageLabel = "PlagueNeutralizerStorageContainer";
        storageContainer.storageRoot = prefab.transform.Find("StorageRoot").GetComponent<ChildObjectIdentifier>();

        // Putting everything together finally!

        var machine = prefab.AddComponent<PlagueNeutralizerMachine>();
        machine.animator = prefab.GetComponentInChildren<Animator>();
        machine.liquid = liquid;
        machine.catalysts = catalysts;
        machine.container = storageContainer;
        machine.ingotModel = ingot;
        machine.fabricatorEmissiveTexture = baseFabricator.GetComponent<CrafterGhostModel>()._EmissiveTex;
        machine.sparksParticles = sparks.GetComponent<ParticleSystem>();
        machine.buildSounds = new[] { buildSound, buildSound2 };
        machine.beams = fabricatorBeams;

        storageContainer.machine = machine;

        prefab.GetComponent<SkyApplier>().renderers = prefab.GetComponentsInChildren<Renderer>(true);
    }

    private static int _spawnPointIndex;

    private static Transform CreateCatalystSpawnPoint(Transform parent, Vector3 position, Vector3 rotation)
    {
        var spawnPoint = new GameObject("CatalystSpawnPoint" + _spawnPointIndex++).transform;
        spawnPoint.SetParent(parent, false);
        spawnPoint.localScale = Vector3.one;
        spawnPoint.localPosition = position;
        spawnPoint.localEulerAngles = rotation;
        return spawnPoint;
        ;
    }

    private static Material CreateLiquidMaterial()
    {
        var liquidMaterial = new Material(MaterialUtils.Shaders.MarmosetUBER);
        MaterialUtils.ApplyUBERShader(liquidMaterial, 6.4f, 0.5f, 1, MaterialUtils.MaterialType.Transparent);
        liquidMaterial.mainTexture = Plugin.AssetBundle.LoadAsset<Texture2D>("NeutralizerLiquidDiffuse");
        liquidMaterial.EnableKeyword("MARMO_SPECMAP");
        liquidMaterial.EnableKeyword("MARMO_EMISSION");
        liquidMaterial.EnableKeyword("UWE_WAVING");
        liquidMaterial.color = new Color(0.2f, 0.38f, 0.666f, 0.5f);
        liquidMaterial.SetColor("_GlowColor", Color.black);
        liquidMaterial.SetVector("_Scale", new Vector4(0, 0.1f, 0, 0.03f));
        liquidMaterial.SetVector("_Frequency", new Vector4(0.6f, 2, 0.6f, 1.4f));
        liquidMaterial.SetVector("_Speed", new Vector4(0.8f, 0.3f, 0f, 0f));
        liquidMaterial.SetFloat("_Fresnel", 0.7f);
        liquidMaterial.SetFloat("_DstBlend", 3);
        liquidMaterial.SetFloat("_WaveUpMin", 0.1f);
        liquidMaterial.SetTexture("_AnimMask", Plugin.AssetBundle.LoadAsset<Texture2D>("AnalyzerLiquidAnimMask"));
        return liquidMaterial;
    }
}