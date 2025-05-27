using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using Story;
using TheRedPlague.Data;
using TheRedPlague.Mono.Buildables.PlagueAltar;
using TheRedPlague.PrefabFiles.Items;
using TheRedPlague.Utilities.Gadgets;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Buildable;

public static class PlagueAltar
{
    private static readonly int TintColor = Shader.PropertyToID("_TintColor");

    private const string CraftTreeName = "PlagueAltar";
    public const string PetsTab = "Pets";
    public const string EquipmentTab = "Equipment";
    public const string ConsumableTab = "Consumable";
    public const string ResourcesTab = "Resources";

    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueAltar")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueAltarIcon"));

    public static CraftTree.Type CraftTreeType { get; private set; }


    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.SetPdaGroupCategoryAfter(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Workbench);
        prefab.SetRecipe(new RecipeData(
            new CraftData.Ingredient(PlagueIngot.Info.TechType, 3),
            new CraftData.Ingredient(RedPlagueSample.Info.TechType),
            new CraftData.Ingredient(ModPrefabs.AmalgamatedBone.TechType)));
        prefab.SetBackgroundType(CustomBackgroundTypes.PlagueItem);
        prefab.Register();
        
        CraftTreeType = EnumHandler.AddEntry<CraftTree.Type>(CraftTreeName).CreateCraftTreeRoot(out var root);
        root.AddTabNode(ConsumableTab, null, Plugin.AssetBundle.LoadAsset<Sprite>("PlagueAltarTab_Consumable"));
        root.AddTabNode(EquipmentTab, null, Plugin.AssetBundle.LoadAsset<Sprite>("PlagueAltarTab_Equipment"));
        root.AddTabNode(PetsTab, null, Plugin.AssetBundle.LoadAsset<Sprite>("PlagueAltarTab_Lifeforms"));
        root.AddTabNode(ResourcesTab, null, Plugin.AssetBundle.LoadAsset<Sprite>("PlagueAltarTab_Resources"));
    }

    public static void RegisterLateStoryData()
    {
        KnownTechHandler.SetAnalysisTechEntry(new KnownTech.AnalysisTech
        {
            techType = Info.TechType,
            unlockSound = KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            storyGoals = new List<StoryGoal> { StoryUtils.ScanPlagueAltarGoal },
            unlockPopup = Plugin.AssetBundle.LoadAsset<Sprite>("PlagueAltarPopup"),
            unlockTechTypes = new List<TechType>(),
            unlockMessage = KnownTechHandler.DefaultUnlockData.BlueprintUnlockMessage
        });
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> result)
    {
        var prefab = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PlagueAltar_Prefab"));
        prefab.SetActive(false);
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        MaterialUtils.ApplySNShaders(prefab, 7, 1, 1, new PlagueAltarMaterialModifier(),
            new IgnoreParticleSystemsModifier());
        var modelParent = prefab.transform.Find("Plague Altar Animated");
        PrefabUtils.AddConstructable(prefab, Info.TechType,
            ConstructableFlags.Base | ConstructableFlags.Ground | ConstructableFlags.Inside |
            ConstructableFlags.Rotatable | ConstructableFlags.Submarine,
            modelParent.gameObject);
        var bounds = prefab.AddComponent<ConstructableBounds>();
        bounds.bounds = new OrientedBounds(new Vector3(0f, 1.77f, 0f), Quaternion.identity,
            new Vector3(2.3f, 1.25f, 2.1f) / 2f);

        var eyes = new[]
        {
            modelParent.Find("Eye.001"),
            modelParent.Find("Eye.002"),
            modelParent.Find("Eye.003"),
            modelParent.Find("Eye.004"),
            modelParent.Find("Eye.005"),
            modelParent.Find("Eye.006"),
        };

        foreach (var eye in eyes)
        {
            var lastNumber = int.Parse(eye.name[^1].ToString());
            eye.gameObject.AddComponent<PlagueAltarEye>().flip = lastNumber >= 4;
        }

        var baseFabricatorTask = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return baseFabricatorTask;
        var baseFabricator = baseFabricatorTask.GetResult();

        var baseFabricatorCrafterGhost = baseFabricator.GetComponent<CrafterGhostModel>();
        var ghostCrafterModel = prefab.AddComponent<CrafterGhostModel>();
        ghostCrafterModel._EmissiveTex = baseFabricatorCrafterGhost._EmissiveTex;
        ghostCrafterModel._NoiseTex = baseFabricatorCrafterGhost._NoiseTex;
        ghostCrafterModel.itemSpawnPoint = prefab.transform.Find("ItemSpawnPoint");

        var craftSoundLoop = prefab.AddComponent<FMOD_CustomLoopingEmitter>();
        craftSoundLoop.SetAsset(AudioUtils.GetFmodAsset("PlagueAltarFabricating"));

        var aliveSoundLoop = prefab.AddComponent<FMOD_CustomLoopingEmitter>();
        aliveSoundLoop.SetAsset(AudioUtils.GetFmodAsset("PlagueAltarIdle"));
        aliveSoundLoop.playOnAwake = true;
        aliveSoundLoop.followParent = true;

        var crafterLogic = prefab.AddComponent<CrafterLogic>();

        var crafter = prefab.AddComponent<PlagueAltarCrafter>();
        crafter.craftTree = CraftTreeType;
        crafter.ghost = ghostCrafterModel;
        crafter.crafterLogic = crafterLogic;
        crafter.closeDistance = 4;
        crafter.animator = prefab.transform.Find("Plague Altar Animated").GetComponent<Animator>();
        crafter.interactSound = AudioUtils.GetFmodAsset("PlagueAltarInteract");
        crafter.craftSoundEmitter = craftSoundLoop;
        crafter.handOverText = "UsePlagueAltar";

        // VFX:

        var beamRootBoneNames = new[]
        {
            "Bone.003",
            "Bone.007",
            "Bone.011",
            "Bone.015"
        };

        var rootBone = prefab.transform.Find("Plague Altar Animated/Armature/Bone");
        var beamEnds = new Transform[beamRootBoneNames.Length];
        for (int i = 0; i < beamRootBoneNames.Length; i++)
        {
            var deepest = rootBone.Find(beamRootBoneNames[i]);
            while (deepest.childCount > 0)
                deepest = deepest.GetChild(0);
            beamEnds[i] = deepest;
        }

        var beamPrefab = baseFabricator.transform.Find("submarine_fabricator_01/printer_left/fabricatorBeam")
            .gameObject;

        var altarParticlePrefab =
            Object.Instantiate(baseFabricator.GetComponent<Fabricator>().sparksPS, prefab.transform, true);
        altarParticlePrefab.SetActive(false);
        altarParticlePrefab.GetComponent<ParticleSystemRenderer>().material.color = new Color(3, 0.7f, 1);
        altarParticlePrefab.transform.Find("xFlash").GetComponent<ParticleSystemRenderer>().material.color =
            new Color(2, 0, 0.3f);
        altarParticlePrefab.transform.Find("xSparkDot").GetComponent<ParticleSystemRenderer>().material.color =
            new Color(3, 1, 2);
        altarParticlePrefab.transform.Find("xSparks").GetComponent<ParticleSystemRenderer>().material.color =
            new Color(3, 0.3f, 0.1f);

        var beamRenderers = new Renderer[beamEnds.Length];
        for (int i = 0; i < beamEnds.Length; i++)
        {
            var beam = Object.Instantiate(beamPrefab, beamEnds[i], true);
            beam.transform.localPosition = Vector3.zero;
            beam.transform.localEulerAngles = Vector3.right * 270;
            beam.transform.localScale *= 0.5f;
            var beamRenderer = beam.GetComponent<Renderer>();
            beamRenderer.material.SetColor(TintColor, new Color(2, 0.1f, 0.3f));
            beamRenderers[i] = beamRenderer;
        }

        crafter.sparksPrefab = altarParticlePrefab;
        crafter.beamEndPoints = beamEnds;
        crafter.beams = beamRenderers;

        result.Set(prefab);
    }

    public class PlagueAltarMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
            MaterialUtils.MaterialType materialType)
        {
            if (renderer.gameObject.name.ToLower().Contains("eye"))
            {
                material.SetColor(ShaderPropertyID._GlowColor, new Color(0.5f, 0.5f, 0.5f));
            }
        }

        public override bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
        {
            return renderer.gameObject.name.StartsWith("fabricatorBeam");
        }
    }
}