using Nautilus.Assets;
using Nautilus.Extensions;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using RootMotion.FinalIK;
using TheRedPlague.Mono.CreatureBehaviour.Stabby;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class StabbyPrefab
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("Stabby");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var obj = Object.Instantiate(Plugin.CreaturesBundle.LoadAsset<GameObject>("StabbyPrefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 3, modifiers: new StabbyMaterialModifier());
        var limbIk = obj.transform.Find("stabbyfix").gameObject.AddComponent<LimbIK>();
        var target = new GameObject("Target").transform;
        target.parent = obj.transform;
        target.localPosition = new Vector3(0, 1.6f, 0);
        target.localRotation = Quaternion.identity;
        limbIk.solver = new IKSolverLimb
        {
            IKPositionWeight = 1,
            root = limbIk.transform,
            target = target,
            IKRotationWeight = 1,
            bendNormal = new Vector3(1, 0, 0),
            bone1 = new IKSolverTrigonometric.TrigonometricBone
            {
                transform = obj.transform.SearchChild("Bone.002"),
                weight = 1
            },
            bone2 = new IKSolverTrigonometric.TrigonometricBone
            {
                transform = obj.transform.SearchChild("Bone.001"),
                weight = 1
            },
            bone3 = new IKSolverTrigonometric.TrigonometricBone
            {
                transform = obj.transform.SearchChild("Bone.008"),
                weight = 1
            }
        };
        
        var stabbyMotion = obj.AddComponent<StabbyMotion>();
        stabbyMotion.ikTarget = target;

        var trigger = obj.transform.Find("StabbyTrigger").gameObject.AddComponent<StabbyTrigger>();
        trigger.motion = stabbyMotion;

        var damageTrigger = obj.transform.SearchChild("StabbyDamageTrigger").gameObject.AddComponent<StabbyDamageTrigger>();
        damageTrigger.motion = stabbyMotion;
        
        var emitter = obj.AddComponent<FMOD_CustomEmitter>();
        emitter.SetAsset(AudioUtils.GetFmodAsset("StabbyDamage"));
        emitter.followParent = true;
        emitter.restartOnPlay = true;
        stabbyMotion.emitter = emitter;
        
        return obj;
    }

    private class StabbyMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (material.name.Contains("Body"))
            {
                material.EnableKeyword("UWE_WAVING");
            }
        }
    }
}