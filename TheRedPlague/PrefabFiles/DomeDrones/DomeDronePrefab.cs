using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Mono.Buildables.Drone;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.DomeDrones;

public static class DomeDronePrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("DomeDrone");

    public static void Register()
    {
        var customPrefab = new CustomPrefab(Info);
        customPrefab.SetGameObject(GetGameObject);
        customPrefab.Register();
    }

    private static IEnumerator GetGameObject(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("TadpolePrefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        MaterialUtils.ApplySNShaders(obj, 6, 1f, 2f,
            new IgnoreParticleSystemsModifier(),
            new DomeDroneMaterialModifier());
        var behaviour = obj.AddComponent<DomeDroneBehaviour>();
        behaviour.animator = obj.GetComponentInChildren<Animator>();
        behaviour.boidBehaviour = obj.AddComponent<BoidBehaviour>();

        var deconstructSound = obj.AddComponent<FMOD_CustomLoopingEmitter>();
        deconstructSound.SetAsset(AudioUtils.GetFmodAsset("event:/tools/builder/loop"));
        deconstructSound.followParent = true;
        deconstructSound.playOnAwake = false;
        behaviour.deconstructSoundEmitter = deconstructSound;
        
        var fabricatorTask = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return fabricatorTask;
        behaviour.deconstructVfxEmissiveTexture = fabricatorTask.GetResult().GetComponent<Fabricator>().ghost._EmissiveTex;

        var builderTask = CraftData.GetPrefabForTechTypeAsync(TechType.Builder);
        yield return builderTask;
        var builderTool = builderTask.GetResult();
        var builderLaser = Object.Instantiate(builderTool.transform.Find(
                "builder/builder_FP/Root/l_nozzle_root/l_nozzle/L_laser").gameObject, obj.transform);
        builderLaser.transform.localScale = Vector3.one * 14;
        behaviour.vfx = builderLaser;
        builderLaser.SetActive(false);

        prefab.Set(obj);
    }

    public class DomeDroneMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (material.name.Contains("Fins"))
            {
                material.SetColor(ShaderPropertyID._GlowColor, new Color(0, 0.7f, 1.5f));
                material.EnableKeyword("MARMO_EMISSION");
            }
        }
    }
}