using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Mono.StoryContent;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.StoryProps;

public static class NewInfectionDome
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("NewInfectionDome");

    private static GameObject _shieldPrefab;
    private static bool _cyclopsLoaded;
    
    public static void Register()
    {
        var infectionDome = new CustomPrefab(Info);
        infectionDome.SetGameObject(GetPrefab);
        infectionDome.Register();
        infectionDome.RemoveFromCache();
    }
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("NewInfectionDome"));
        obj.SetActive(false);
        MaterialUtils.ApplySNShaders(obj, 7, 1f, 1f, new DomeMaterialModifier());
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType,
            LargeWorldEntity.CellLevel.Global);

        _cyclopsLoaded = false;
        
        yield return new WaitUntil(() => LightmappedPrefabs.main);

        LightmappedPrefabs.main.RequestScenePrefab("Cyclops", OnCyclopsReferenceLoaded);

        yield return new WaitUntil(() => _cyclopsLoaded);

        var shield = Object.Instantiate(_shieldPrefab, obj.transform);
        shield.SetActive(true);
        shield.transform.localPosition = new Vector3(0, -0.07f, 0f);
        shield.transform.localEulerAngles = Vector3.right * 90;
        shield.transform.localScale = new Vector3(0.744f, 0.744f, 0.677f);
        
        var shieldRenderer = shield.GetComponent<Renderer>();
        var shieldMaterial = shieldRenderer.material;
        shieldMaterial.SetFloat(ShaderPropertyID._Intensity, 1);
        shieldMaterial.SetVector("_WobbleParams", new Vector4(2, 0.7f, 8, 0));
        
        var domeController = obj.AddComponent<InfectionDomeController>();
        domeController.domeRenderer = shieldRenderer;
        domeController.baseCollider = obj.transform.Find("Pivot/Alterra Dome15/DomeBaseCollisions/default")
            .GetComponent<Collider>();
        domeController.baseCenterTransform =
            obj.transform.Find("Pivot/Alterra Dome15/rocketship_platform_barrel_01_01 #384020");

        var constructionVfx = obj.AddComponent<DomeConstructionVfx>();
        constructionVfx.domeShieldRenderer = shieldRenderer;
        
        var fabricatorTask = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return fabricatorTask;
        constructionVfx.emissiveTex = fabricatorTask.GetResult().GetComponent<Fabricator>().ghost._EmissiveTex;

        var farPlaneAdjust = obj.AddComponent<AdjustFarPlane>();
        farPlaneAdjust.newFarClipPlane = 4000;
        farPlaneAdjust.transitionDuration = 10;
        farPlaneAdjust.maxDepthToApply = 25;

        obj.EnsureComponent<ConstructionObstacle>();
        
        prefab.Set(obj);
    }

    private static void OnCyclopsReferenceLoaded(GameObject obj)
    {
        _shieldPrefab = obj.transform.Find("FX/x_Cyclops_GlassShield").gameObject;
        _cyclopsLoaded = true;
    }

    private class DomeMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (renderer.gameObject.name == "PowerAugmenter")
            {
                material.SetColor("_GlowColor", new Color(1, 1.3f, 1));
                material.SetFloat(ShaderPropertyID._GlowStrength, 4);
                material.SetFloat(ShaderPropertyID._GlowStrengthNight, 3);
            }
        }
    }
}